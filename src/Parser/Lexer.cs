namespace albus.src.Parser;

public sealed class Lexer(string source, bool debug) {
    private readonly string Source = source;
    private int Current;
    private int CurrentLine = 1;
    private readonly bool IsDebug = debug;

    private readonly List<Token> Tokens = [];

    private static readonly Dictionary<string, TokenType> Keywords = new() {
        ["mut"] = TokenType.Mut,
        ["let"] = TokenType.Let,
        ["while"] = TokenType.While,
        ["if"] = TokenType.If,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
    };

    private static readonly Dictionary<char, TokenType> SingleTokens = new() {
        [';'] = TokenType.SemiColon,
        ['='] = TokenType.SingleEquals,
        ['('] = TokenType.LeftParen,
        [')'] = TokenType.RightParen,
        ['{'] = TokenType.LeftBrace,
        ['}'] = TokenType.RightParen,
        [','] = TokenType.Comma,
        ['.'] = TokenType.Dot,
        ['+'] = TokenType.Plus,
        ['-'] = TokenType.Minus,
        ['*'] = TokenType.Star,
        ['/'] = TokenType.Slash,
        ['%'] = TokenType.Modulo,
    };

    private static readonly Dictionary<string, TokenType> DoubleTokens = new() {
        ["=="] = TokenType.DoubleEquals

    };

    public Result<List<Token>> Tokenize() {
        while (!IsEnd()) {
            while (!IsEnd() && (Source[Current] == ' ' || Source[Current] == '\n' || Source[Current] == '\r')) {
                if (Source[Current] == '\n') CurrentLine++;
                Current++;
            }
            if (IsEnd()) break;

            var result = ParseToken();
            if (!result.IsSuccess) {
                return Error(result.Error!);
            }

            Tokens.Add(result.Value!);
            Current++;
        }

        if (IsDebug) LexerDebug();

        Tokens.Add(NewToken(TokenType.Eof, ""));
        return Result<List<Token>>.Ok(Tokens);
    }

    private Result<Token> ParseToken() {
        char c = Source[Current];

        return c switch {
            _ when char.IsLetter(c) => ParseIdentifier(),
            _ when char.IsDigit(c)  => ParseNumeric(),
            '\"'                    => ParseString(),
            '\''                    => ParseCharLiteral(),
            _                       => ParseShortToken()
        };
    }

    private Result<Token> ParseShortToken() {
        char c = Source[Current];
        
        foreach (var token in DoubleTokens) {
            if (!token.Key.StartsWith(c)) continue;
            
            Current++;
            if (IsEnd()) break;
            var nextChar = Source[Current];

            var doubleToken = new string([c, nextChar]);
            if (DoubleTokens.TryGetValue(doubleToken, out var doubleType)) {
                Current++;
                return Result<Token>.Ok(NewToken(doubleType, doubleToken));
            }

            Current--;
        }

        if (SingleTokens.TryGetValue(c, out var type)) {
            return Result<Token>.Ok(NewToken(type, c.ToString()));
        }

        return Result<Token>.Ok(NewToken(TokenType.BadToken, ""));
    }

    private Result<Token> ParseIdentifier() {
        int start = Current;

        while (!IsEnd() && char.IsLetter(Source[Current])) 
            Current++;

        var lexeme = Source[start..Current];

        if (Keywords.TryGetValue(lexeme, out var type)) {
            return Result<Token>.Ok(NewToken(type, lexeme));
        }

        return Result<Token>.Ok(NewToken(TokenType.Identifier, lexeme));
    }

    private Result<Token> ParseNumeric() {
        int start = Current;

        static bool IsNumChar(char c)
            => char.IsDigit(c) || c == '.';

        bool hasDecimal = false;
        while (!IsEnd() && IsNumChar(Source[Current])) {
            if (Source[Current] == '.') {
                if (hasDecimal) {
                    // Current is incremented here as it places the error arrow (^) in the correct position
                    Current++;
                    return Result<Token>.Err("Syntax Error: invalid numeric literal");
                }

                hasDecimal = true;
            }

            Current++;
        }

        var lexeme = Source[start..Current];
        if (lexeme[^1] == '.') {
            return Result<Token>.Err("Syntax Error: invalid numeric literal");
        }

        if (hasDecimal) {
            return Result<Token>.Ok(NewToken(TokenType.Float, lexeme));
        }

        Current--;
        return Result<Token>.Ok(NewToken(TokenType.Integer, lexeme));
    }

    private Result<Token> ParseCharLiteral() {
        var start = Current;

        Current++;
        while (!IsEnd() && Source[Current] != '\'')
            Current++;

        var lexeme = Source[start..(Current + (IsEnd() ? 0 : 1))];
        if (lexeme[^1] != '\'') {
            return Result<Token>.Err("Syntax Error: unterminated char literal");
        }

        if (lexeme == "\'\'") {
            return Result<Token>.Err("Syntax Error: empty char literal");
        }

        return Result<Token>.Ok(NewToken(TokenType.Char, lexeme));
    }

    private Result<Token> ParseString() {
        var start = Current;

        Current++;
        while (!IsEnd() && Source[Current] != '\"')
            Current++;

        var lexeme = Source[start..(Current + (IsEnd() ? 0 : 1))];

        if (lexeme[^1] != '\"') {
            return Result<Token>.Err("Syntax Error: unterminated string literal");
        }

        return Result<Token>.Ok(NewToken(TokenType.String, lexeme));
    }

    private Result<List<Token>> Error(string message) {
        var line = GetCurrentLine();
        var idx = Source.IndexOf(line);

        Console.WriteLine(line);
        Console.WriteLine(new string(' ', Current - ++idx) + "^");

        Console.WriteLine($"{message} on line {CurrentLine}");

        return Result<List<Token>>.Err(message);
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