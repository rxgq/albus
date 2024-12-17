namespace albus.src.Parser;

public enum TokenType {
    SingleEquals,
    SemiColon,
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    Comma,
    Dot,
    Plus,
    Minus,
    Star,
    Slash,
    Modulo,
    DoubleEquals,

    Identifier,
    Integer,
    Float,
    Char,
    String,
    True,
    False,
    Let,
    Mut,
    While,
    If,
    BadToken,
    Eof
}