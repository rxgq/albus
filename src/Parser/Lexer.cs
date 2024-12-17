namespace albus.src.Parser;

public sealed class Lexer(string source) {
    private readonly string Source = source;
    private int Current;
    private int CurrentLine = 1;

    private readonly List<Token> Tokens = [];

    private readonly Dictionary<string, TokenType> Keywords = new() {
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
            while (!IsEnd() && (Source[Current] == ' ' || Source[Current] == '\n')) {
                if (Source[Current] == '\n') CurrentLine++;
                Current++;
            }
            if (IsEnd()) break;

            var result = ParseToken();
            if (result.Error is not null) {
                return Error(result.Error);
            }

            Tokens.Add(result.Value!);
            Current++;
        }

        Tokens.Add(new Token(TokenType.Eof, ""));
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

            var nextChar = Source[Current + 1];

            var doubleToken = new string([c, nextChar]);
            if (DoubleTokens.TryGetValue(doubleToken, out var doubleType)) {
                Current++;
                return Result<Token>.Ok(new Token(doubleType, doubleToken));
            }
        }

        if (SingleTokens.TryGetValue(c, out var type)) {
            return Result<Token>.Ok(new Token(type, c));
        }

        return Result<Token>.Ok(new Token(TokenType.BadToken, ""));
    }

    private Result<Token> ParseIdentifier() {
        int start = Current;

        while (!IsEnd() && char.IsLetter(Source[Current])) 
            Current++;

        var lexeme = Source[start..Current];

        if (Keywords.TryGetValue(lexeme, out var type)) {
            return Result<Token>.Ok(new Token(type, lexeme));
        }

        return Result<Token>.Ok(new Token(TokenType.Identifier, lexeme));
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
            return Result<Token>.Ok(new Token(TokenType.Float, lexeme));
        }

        return Result<Token>.Ok(new Token(TokenType.Integer, lexeme));
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

        return Result<Token>.Ok(new Token(TokenType.Char, lexeme));
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

        return Result<Token>.Ok(new Token(TokenType.String, lexeme));
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

    private bool IsEnd() {
        return Current >= Source.Length;
    }
}