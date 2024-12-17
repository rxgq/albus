namespace albus.src.Parser;

public sealed class Token {
    public readonly TokenType Type;
    public readonly string Lexeme;

    public Token(TokenType type, string lexeme) {
        Type = type;
        Lexeme = lexeme;
    }

    public Token(TokenType type, char lexeme) {
        Type = type;
        Lexeme = lexeme.ToString();
    }

    public override string ToString() {
        return $"^{Lexeme}^  {Type}";
    }
}
