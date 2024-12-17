using System.Threading.Tasks.Dataflow;

namespace albus.src.Parser;

public class Parser(List<Token> tokens)
{
    private readonly List<Token> Tokens = tokens;
    private int Current = 0;

    private readonly List<Expression> Expressions = []; 

    public Result<List<Expression>> ParseAst() {
        while (!IsEnd() && Tokens[Current].Type != TokenType.Eof) {
            var result = ParseExpression();
            
            if (!result.IsSuccess) {
                return Result<List<Expression>>.Err(result.Error!);
            }

            Expressions.Add(result.Value!);

            Current++;
        }

        return Result<List<Expression>>.Ok(Expressions);
    }

    private Result<Expression> ParseExpression() {
        return Tokens[Current].Type switch {
            TokenType.Let => ParseVariableDeclaration(),
            _ => Result<Expression>.Err("Syntax Error: Unknown token")
        }; 
    }

    private Result<Expression> ParseVariableDeclaration() {
        Expect(TokenType.Let);

        bool mutable = Tokens[Current].Type == TokenType.Mut;
        if (mutable) Current++;

        var identifier = Match(TokenType.Identifier);
        if (identifier is null) return SyntaxError("identifier");

        var hasEquals = Expect(TokenType.SingleEquals);
        if (!hasEquals) return SyntaxError("=");

        var result = ParsePrimary();
        if (!result.IsSuccess) return result;

        var hasSemiColon = Expect(TokenType.SemiColon);
        if (!hasSemiColon) return SyntaxError(";");

        return Result<Expression>.Ok(new VariableDeclaration(identifier.Lexeme, mutable, result.Value!));
    }

    private Result<Expression> ParsePrimary() {
        Expression? expr = Tokens[Current].Type switch {
            TokenType.String => new LiteralExpression(Tokens[Current].Lexeme),
            TokenType.Integer => new LiteralExpression(Tokens[Current].Lexeme),
            TokenType.Float => new LiteralExpression(Tokens[Current].Lexeme),
            TokenType.True => new LiteralExpression(Tokens[Current].Lexeme),
            TokenType.False => new LiteralExpression(Tokens[Current].Lexeme),
            _ => null
        };

        Current++;

        if (expr is null) return Result<Expression>.Err("Syntax Error: Unknown token");
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
            return Tokens[Current];
        }

        return null;
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

        return Result<Expression>.Err($"\n\nSyntax Error: expected \'{expected}\' on line {line}");
    }

    private bool IsEnd() {
        return Current >= Tokens.Count;
    }
}