namespace albus.src;

public sealed class Token {
    public readonly TokenType Type;
    public readonly string Lexeme;
    public readonly int Line;

    public Token(TokenType type, string lexeme, int line) {
        Type = type;
        Lexeme = lexeme;
        Line = line;
    }

    public Token(TokenType type, char lexeme, int line) {
        Type = type;
        Lexeme = lexeme.ToString();
        Line = line;
    }

    public override string ToString() {
        return $"Line {Line}: ^{Lexeme}^  {Type}";
    }
}