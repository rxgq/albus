namespace albus.src;

public sealed class Parser(List<Token> tokens, bool isDebug) {
    private readonly List<Token> Tokens = tokens;
    private int Current = 0;
    private readonly bool IsDebug = isDebug;
    private readonly Ast AST = new();

    public bool HasError = false;

    public Ast ParseTokens() {
        while (!IsLastToken()) {
            var expr = ParseStatement();
            AST.Body.Add(expr);

            if (HasError) {
                return AST;
            }

            Current++;
        }

        if (IsDebug) AstPrint();
        return AST;
    }

    private Expression ParseStatement() {
        return Tokens[Current].Type switch {
            TokenType.Let        => ParseVariableDeclaration(),
            TokenType.Identifier => ParseAssignmentStatement(),
            _                    => ParseExpression()
        };
    }

    private Expression ParseExpression() {
        return Tokens[Current].Type switch {
            _ => ParsePrimaryExpr()
        };
    }

    private Expression ParseAssignmentStatement() {
        var identifier = Tokens[Current].Lexeme;
        if (!Expect(TokenType.Identifier, "identifier")) return ExprError();
        if (!Expect(TokenType.SingleEquals, "=")) return ExprError();

        var expr = ParsePrimaryExpr();
        if (HasError) return ExprError();

        if (!Expect(TokenType.SemiColon, ";")) return ExprError();

        var identExpr = new IdentifierExpr(identifier);
        return new AssignmentStatement(identExpr, expr);
    }

    private Expression ParseVariableDeclaration() {
        if (!Expect(TokenType.Let, "let")) return ExprError();
        
        var identifier = Tokens[Current].Lexeme;
        if (!Expect(TokenType.Identifier, "identifier")) return ExprError();

        string? type = null;
        if (Match(TokenType.Colon)) {
            Current++;

            if (!Expect(TokenType.Identifier, "type")) return ExprError();
            type = Tokens[Current].Lexeme;
        }

        if (!Expect(TokenType.SingleEquals, "=")) return ExprError();

        var expr = ParsePrimaryExpr();
        if (HasError) return ExprError();

        if (!Expect(TokenType.SemiColon, ";")) return ExprError();

        Current--;

        var identExpr = new IdentifierExpr(identifier);
        return new VariableDeclaration(identExpr, expr, type, true);
    }

    private Expression ParsePrimaryExpr() {
        var currentToken = Tokens[Current];

        Expression token = currentToken.Type switch {
            TokenType.String or TokenType.Integer or TokenType.Float or
            TokenType.Char or TokenType.True or TokenType.False => new LiteralExpr(currentToken.Lexeme),
            TokenType.Identifier => new IdentifierExpr(currentToken.Lexeme),
            _ => ExprError($"invalid expression")
        }; 

        Current++;
        return token;
    }

    private bool Expect(TokenType type, string value) {
        if (IsLastToken() || Tokens[Current].Type != type) {
            return Error($"expected '{value}'");
        }

        Current++;
        return true;
    }

    private bool Match(TokenType type) {
        if (IsLastToken()) {
            return false;
        }

        return Tokens[Current].Type == type;
    }
    
    private IdentifierExpr ExprError(string? message = null) {
        if (message is not null) {
            Error(message);
        }

        return new IdentifierExpr("");
    }

    private bool Error(string message) {
        HasError = true;

        var currentToken = Tokens[Current - 1];
        Console.WriteLine($"parsing error: {message} on line {currentToken.Line}");

        return false;
    }

    private bool IsLastToken() {
        return Current >= Tokens.Count;
    }

    private void AstPrint() {
        foreach (var expr in AST.Body) {
            PrintExpression(expr, 0);
        }
    }

    private static void PrintExpression(Expression expr, int indentLevel) {
        var indent = new string(' ', indentLevel * 2);

        switch (expr) {
            case VariableDeclaration varDec:
                Console.WriteLine($"{indent}VariableDeclaration:");
                Console.WriteLine($"{indent}  Identifier: {varDec.Id.Name}");
                if (varDec.Type is not null) Console.WriteLine($"{indent}  Type: {varDec.Type}");
                Console.WriteLine($"{indent}  Value:");
                PrintExpression(varDec.Expr, indentLevel + 1);
                break;

            case AssignmentStatement assignStmt:
                Console.WriteLine($"{indent}AssignmentStatement:");
                Console.WriteLine($"{indent}  Identifier: {assignStmt.Id.Name}");
                Console.WriteLine($"{indent}  Value:");
                PrintExpression(assignStmt.Expr, indentLevel + 1);
                break;

            case LiteralExpr literal:
                Console.WriteLine($"{indent}LiteralExpr: {literal.Value}");
                break;

            case IdentifierExpr identifier:
                Console.WriteLine($"{indent}IdentifierExpr: {identifier.Name}");
                break;

            default:
                Console.WriteLine($"{indent}Unknown Expression Type");
                break;
        }
    }
}