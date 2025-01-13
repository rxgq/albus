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
        return ParseVariableDeclaration();
    }

    private Expression ParseVariableDeclaration() {
        if (!Expect(TokenType.Let, "let")) return ExprError();
        
        var identifier = Tokens[Current].Lexeme;

        if (!Expect(TokenType.Identifier, "identifier")) return ExprError();
        if (!Expect(TokenType.SingleEquals, "=")) return ExprError();

        var expr = ParsePrimaryExpr();
        if (HasError) return ExprError();

        if (!Expect(TokenType.SemiColon, ";")) return ExprError();

        var identExpr = new IdentifierExpr(identifier);
        return new VariableDeclaration(identExpr, expr, true);
    }

    private Expression ParsePrimaryExpr() {
        var currentToken = Tokens[Current];

        Expression token = currentToken.Type switch {
            TokenType.String => new LiteralExpr(currentToken.Lexeme),
            _ => ExprError($"invalid expression") 
        }; 

        Current++;
        return token;
    }

    private bool Expect(TokenType type, string value) {
        if (Tokens[Current].Type == type) {
            Current++;
            return true;
        }

        return Error($"expected '{value}'");
    }
    
    private IdentifierExpr ExprError(string? message = null) {
        if (message is not null) {
            Error(message);
        }

        return new IdentifierExpr("");
    }

    private bool Error(string message) {
        HasError = true;

        var currentToken = Tokens[Current];
        Console.WriteLine($"parsing error: {message} on line {currentToken.Line}");

        return false;
    }

    private bool IsLastToken() {
        return Current >= Tokens.Count;
    }
}