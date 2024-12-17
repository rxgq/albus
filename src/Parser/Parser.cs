namespace albus.src.Parser;

public class Parser(List<Token> tokens, bool isDebug)
{
    private readonly List<Token> Tokens = tokens;
    private int Current = 0;
    private readonly bool IsDebug = isDebug;

    private readonly List<Expression> Expressions = []; 

    public Result<List<Expression>> ParseAst() {
        while (!IsEnd() && Tokens[Current].Type != TokenType.Eof) {
            var result = ParseStatement();
            
            if (!result.IsSuccess) {
                return Result<List<Expression>>.Err();
            }

            Expressions.Add(result.Value!);
            Current++;
        }

        if (IsDebug) ParserDebug();
        return Result<List<Expression>>.Ok(Expressions);
    }

    private Result<Expression> ParseStatement() {
        return Tokens[Current].Type switch {
            TokenType.Let => ParseVariableDeclaration(),
            _ => ParseExpression()
        }; 
    }

    private Result<Expression> ParseExpression() {
        return ParseEquality();
    }

    private Result<Expression> ParseEquality() {
        var left = ParseComparison();

        while (Check(TokenType.DoubleEquals) || Check(TokenType.NotEquals)) {
            var op = Tokens[Current - 1];
            var right = ParseComparison();
            left = Result<Expression>.Ok(new BinaryExpression(left.Value!, op, right.Value!, op.Line));
        }

        return left;
    }

    private Result<Expression> ParseComparison() {
        var left = ParseTerm();

        while (Check(TokenType.GreaterThan) || Check(TokenType.LessThan) || 
                Check(TokenType.GreaterThanEquals) || Check(TokenType.LessThanEquals)) {
            var op = Tokens[Current - 1];
            var right = ParseTerm();
            left = Result<Expression>.Ok(new BinaryExpression(left.Value!, op, right.Value!, op.Line));
        }

        return left;
    }

    private Result<Expression> ParseTerm() {
        var left = ParseFactor();

        while (Check(TokenType.Plus) || Check(TokenType.Minus)) {
            var op = Tokens[Current - 1];
            var right = ParseFactor();
            left = Result<Expression>.Ok(new BinaryExpression(left.Value!, op, right.Value!, op.Line));
        }

        return left;
    }

    private Result<Expression> ParseFactor() {
        var left = ParseUnary();

        while (Check(TokenType.Star) || Check(TokenType.Slash) || Check(TokenType.Modulo)) {
            var op = Tokens[Current - 1];
            var right = ParseUnary();

            left = Result<Expression>.Ok(new BinaryExpression(left.Value!, op, right.Value!, op.Line));
            
            if (ContainsDivisionByZero(left.Value!)) {
                return SyntaxError("cannot divide by 0");
            }
        }

        return left;
    }

    private Result<Expression> ParseUnary() {
        if (Check(TokenType.Exclamation) ) {
            var op = Tokens[Current - 1];
            var right = ParseUnary();
            return Result<Expression>.Ok(new UnaryExpression(op, right.Value!, op.Line));
        }

        return ParsePrimary();
    }

    private Result<Expression> ParseVariableDeclaration() {
        Expect(TokenType.Let);

        bool mutable = Tokens[Current].Type == TokenType.Mut;
        if (mutable) Current++;

        var identifier = Match(TokenType.Identifier);
        if (identifier is null) return SyntaxError("expected identifier");

        var hasEquals = Expect(TokenType.SingleEquals);
        if (!hasEquals) return SyntaxError("expected =");

        var primaryResult = ParsePrimary();
        if (!primaryResult.IsSuccess) return SyntaxError("expected expression");

        var hasSemiColon = Expect(TokenType.SemiColon);
        if (!hasSemiColon) return SyntaxError("expected ;");

        Current--;
        return Result<Expression>.Ok(new VariableDeclaration(identifier.Lexeme, mutable, primaryResult.Value!, identifier.Line));
    }

    private Result<Expression> ParsePrimary() {
        var token = Tokens[Current];

        var expr = token.Type switch {
            TokenType.String  => new LiteralExpression(Tokens[Current].Lexeme, token.Line),
            TokenType.Integer => new LiteralExpression(int.Parse(Tokens[Current].Lexeme), token.Line),
            TokenType.Float   => new LiteralExpression(double.Parse(Tokens[Current].Lexeme), token.Line),
            TokenType.True    => new LiteralExpression(bool.Parse(Tokens[Current].Lexeme), token.Line),
            TokenType.False   => new LiteralExpression(bool.Parse(Tokens[Current].Lexeme), token.Line),
            _ => null
        };

        Current++;

        if (expr is null) return SyntaxError("unknown token");
        return Result<Expression>.Ok(expr);
    }

    private bool Expect(TokenType type) {
        if (IsEnd()) return false;

        if (Tokens[Current].Type == type) {
            Current++;
            return true;
        }

        return false;
    }

    private Token? Match(TokenType type) {
        if (IsEnd()) return null;

        if (Tokens[Current].Type == type) {
            Current++;
            return Tokens[Current - 1];
        }

        return null;
    }

    private bool Check(TokenType type) {
        if (Tokens[Current].Type == type) {
            Current++;
            return true;
        }

        return false;
    }

    private Result<Expression> SyntaxError(string expected) {
        var line = Tokens[Current - 1].Line;

        var tokensOnLine = Tokens.Where(x => x.Line == line).ToList();
        foreach (var token in tokensOnLine) {
            Console.Write($"{token.Lexeme} ");
        }

        var errorToken = Tokens.FirstOrDefault(x => x == Tokens[Current - 1]);
        
        int len = 0;
        int offset = -1;
        foreach (var token in tokensOnLine) {
            len += token.Lexeme.Length;
            offset++;
            if (token == errorToken) break;
        }
        
        Console.Write("\n" + new string(' ', len + offset) + "^");
        Console.WriteLine($"\n\nSyntax Error: \'{expected}\' on line {line}");

        return Result<Expression>.Err();
    }

    private bool ContainsDivisionByZero(Expression expr) {
        if (expr is BinaryExpression binExpr) {
            if (binExpr.Op.Type == TokenType.Slash && binExpr.Right is LiteralExpression lit && lit.Literal == 0) {
                return true;
            }
        }

        return false;
    }

    private void ParserDebug() {
        foreach (var expr in Expressions ?? []) {
            Console.WriteLine(expr.ToString());
        }
    }

    private bool IsEnd() {
        return Current >= Tokens.Count;
    }
}