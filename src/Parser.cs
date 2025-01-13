namespace albus.src;

public sealed class Parser(List<Token> tokens) {
    private readonly List<Token> Tokens = tokens;
    private int Current = 0;
    private bool HasError = false;

    private readonly Ast AST = new();

    public Ast ParseTokens() {
        while (!IsLastToken()) {
            var expr = ParseExpression();
            AST.Body.Add(expr);

            if (HasError) {
                return AST;
            }

            Current++;
        }

        return AST;
    }

    private Expression ParseExpression() {
        return Tokens[Current].Type switch {
            TokenType.Let        => ParseVariableDeclaration(),
            TokenType.Identifier => ParseAssignmentStatement(),
            _ => ExprError()
        }; 
    }

    private Expression ParseAssignmentStatement() {
        var identifier = Tokens[Current].Lexeme;
        if (!Expect(TokenType.Identifier, "identifier")) return ExprError();
        if (!Expect(TokenType.SingleEquals, "=")) return ExprError();

        var expr = ParsePrimaryExpr();
        if (HasError) return ExprError();

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

        var identExpr = new IdentifierExpr(identifier);
        return new VariableDeclaration(identExpr, expr, type, true);
    }

    private Expression ParsePrimaryExpr() {
        var currentToken = Tokens[Current];

        Expression token = currentToken.Type switch {
            TokenType.String or TokenType.Integer or TokenType.Float or
            TokenType.Char or TokenType.True or TokenType.False => new LiteralExpr(currentToken.Lexeme),
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
}