namespace albus.src;

public sealed class Lexer(string source, bool debug) {
    private readonly string Source = source;
    private int Current;
    private int CurrentLine = 1;
    private readonly bool IsDebug = debug;
    public bool HasError = false;

    private readonly List<Token> Tokens = [];

    private static readonly Dictionary<string, TokenType> Keywords = new() {
        ["mut"] = TokenType.Mut,
        ["let"] = TokenType.Let,
        ["while"] = TokenType.While,
        ["if"] = TokenType.If,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["Endif"] = TokenType.Endif,
        ["return"] = TokenType.Return,
        ["void"] = TokenType.Void,
    };

    private static readonly Dictionary<char, TokenType> SingleTokens = new() {
        [';'] = TokenType.SemiColon,
        [':'] = TokenType.Colon,
        ['='] = TokenType.SingleEquals,
        ['('] = TokenType.LeftParen,
        [')'] = TokenType.RightParen,
        [','] = TokenType.Comma,
        ['.'] = TokenType.Dot,
        ['+'] = TokenType.Plus,
        ['-'] = TokenType.Minus,
        ['*'] = TokenType.Star,
        ['/'] = TokenType.Slash,
        ['%'] = TokenType.Modulo,
        ['!'] = TokenType.Exclamation,
        ['>'] = TokenType.GreaterThan,
        ['<'] = TokenType.LessThan,
    };

    private static readonly Dictionary<string, TokenType> DoubleTokens = new() {
        ["=="] = TokenType.DoubleEquals,
        ["!="] = TokenType.NotEquals,
        [">="] = TokenType.GreaterThanEquals,
        ["<="] = TokenType.LessThanEquals, 
    };

    public List<Token> Tokenize() {
        while (!IsEnd()) {
            while (!IsEnd() && (Source[Current] is ' ' or '\n' or '\r')) {
                if (Source[Current] is '\n') CurrentLine++;
                Current++;
            }
            if (IsEnd()) break;

            var token = ParseToken();
            if (HasError) {
                return Tokens;
            }

            Tokens.Add(token);
            Current++;
        }

        Tokens.Add(NewToken(TokenType.Eof, ""));

        if (IsDebug) LexerDebug();
        return Tokens;
    }

    private Token ParseToken() {
        char c = Source[Current];

        return c switch {
            _ when char.IsLetter(c) => ParseIdentifier(),
            _ when char.IsDigit(c)  => ParseNumeric(),
            '\"' => ParseString(),
            '\'' => ParseChar(),
            _ => ParseShortToken()
        };
    }

    private Token ParseShortToken() {
        char c = Source[Current];
        
        foreach (var token in DoubleTokens) {
            if (!token.Key.StartsWith(c)) continue;
            
            Current++;
            if (IsEnd()) break;
            var nextChar = Source[Current];

            var doubleToken = new string([c, nextChar]);
            if (DoubleTokens.TryGetValue(doubleToken, out var doubleType)) {
                Current++;
                return NewToken(doubleType, doubleToken);
            }

            Current--;
        }

        if (SingleTokens.TryGetValue(c, out var type)) {
            return NewToken(type, c.ToString());
        }

        return NewToken(TokenType.BadToken, "");
    }

    private Token ParseIdentifier() {
        int start = Current;

        while (!IsEnd() && char.IsLetter(Source[Current])) 
            Current++;

        var lexeme = Source[start..Current];
        Current--;

        if (Keywords.TryGetValue(lexeme, out var type)) {
            return NewToken(type, lexeme);
        }

        return NewToken(TokenType.Identifier, lexeme);
    }

    private Token ParseNumeric() {
        int start = Current;

        static bool IsNumChar(char c) => char.IsDigit(c) || c == '.';

        bool hasDecimal = false;
        while (!IsEnd() && IsNumChar(Source[Current])) {
            if (Source[Current] == '.') {
                if (hasDecimal) {
                    Current++;
                    return SyntaxError("invalid numeric literal");
                }

                hasDecimal = true;
            }

            Current++;
        }

        var lexeme = Source[start..Current];
        if (lexeme[^1] == '.') {
            return SyntaxError("invalid numeric literal");
        }

        if (hasDecimal) {
            return NewToken(TokenType.Float, lexeme);
        }

        Current--;
        return NewToken(TokenType.Integer, lexeme);
    }

    private Token ParseChar() {
        var start = Current;

        Current++;
        while (!IsEnd() && Source[Current] != '\'')
            Current++;

        var lexeme = Source[start..(Current + (IsEnd() ? 0 : 1))];
        if (lexeme[^1] != '\'') {
            return SyntaxError("Syntax Error: unterminated char literal");
        }

        if (lexeme == "\'\'") {
            return SyntaxError("Syntax Error: empty char literal");
        }

        return NewToken(TokenType.Char, lexeme);
    }

    private Token ParseString() {
        var start = Current;

        Current++;
        while (!IsEnd() && Source[Current] != '\"')
            Current++;

        var lexeme = Source[start..(Current + (IsEnd() ? 0 : 1))];

        if (lexeme[^1] != '\"' || lexeme.Length == 1) {
            return SyntaxError("Syntax Error: unterminated string literal");
        }

        return NewToken(TokenType.String, lexeme);
    }

    private Token SyntaxError(string message) {
        var line = GetCurrentLine();
        var idx = Source.IndexOf(line);

        Console.WriteLine(line);
        Console.WriteLine(new string(' ', Current - ++idx) + "^");

        Console.WriteLine($"{message} on line {CurrentLine}");

        HasError = true;
        return NewToken(TokenType.BadToken, "");
    }

    private string GetCurrentLine() {
        int lineIdx = Current - 1;

        while (lineIdx > 0 && Source[lineIdx] != '\n') {
            lineIdx--;
        }

        var nextNewLineIdx = Source.IndexOf('\n', lineIdx + 1);
        if (nextNewLineIdx == -1)
            return Source[lineIdx..];

        return Source[lineIdx..nextNewLineIdx];
    }

    private void LexerDebug() {
        Console.WriteLine("\nTokens: ");
        foreach (var token in Tokens) {
            Console.WriteLine($"  {token}");
        }
        Console.WriteLine();
    }

    private Token NewToken(TokenType type, string lexeme) {
        return new Token(type, lexeme, CurrentLine);
    }

    private bool IsEnd() {
        return Current >= Source.Length;
    }
}