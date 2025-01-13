namespace albus.src;

public enum TokenType {
    SingleEquals,
    SemiColon,
    LeftParen,
    RightParen,
    Comma,
    Dot,
    Plus,
    Minus,
    Star,
    Slash,
    Modulo,
    Exclamation,
    DoubleEquals,
    NotEquals,
    GreaterThan,
    LessThan,
    GreaterThanEquals,
    LessThanEquals,
    Colon,

    Integer,
    Float,
    Char,
    String,
    Void,
    True,
    False,

    Identifier,
    Let,
    Mut,
    While,
    If,
    Endif,
    Return,

    BadToken,
    Eof
}