namespace Razier.Lexing;

// Public methods.
public static partial class Lexer
{
    public static IList<Lexeme> Lex(string input)
    {
        var lexemeStart = 0;
        var lexemes = new List<Lexeme>();
        var source = input.AsSpan();

        for (var i = 0; i < source.Length; i++)
        {
            if (!_delimiters.Contains(source[i]))
                continue;

            if (lexemeStart < i)
            {
                lexemes.Add(
                    Lexeme(lexemeStart, i - lexemeStart, Type(source[lexemeStart..(i + 1)]))
                );
                lexemeStart = i;
            }

            lexemes.Add(
                Lexeme(lexemeStart, i - lexemeStart + 1, Type(source[lexemeStart..(i + 1)]))
            );
            lexemeStart = i + 1;
        }

        if (lexemeStart < source.Length - 1)
            lexemes.Add(
                Lexeme(
                    lexemeStart,
                    source.Length - lexemeStart,
                    Type(source[lexemeStart..source.Length])
                )
            );

        lexemes.Add(
            new()
            {
                Length = 0,
                Offset = source.Length,
                Type = LexemeType.EndOfFile
            }
        );

        return lexemes;
    }
}

// Private methods.
public static partial class Lexer
{
    private static Lexeme Lexeme(int offset, int length, LexemeType type) =>
        new()
        {
            Length = length,
            Offset = offset,
            Type = type
        };

    private static LexemeType Type(ReadOnlySpan<char> span) =>
        span switch
        {
            "@" => LexemeType.At,
            "<" => LexemeType.LeftChevron,
            ">" => LexemeType.RightChevron,
            "(" => LexemeType.LeftParenthesis,
            ")" => LexemeType.RightParenthesis,
            "{" => LexemeType.LeftBrace,
            "}" => LexemeType.RightBrace,
            "/" => LexemeType.ForwardSlash,
            "\\" => LexemeType.BackSlash,
            "-" => LexemeType.Dash,
            "!" => LexemeType.Exclamation,
            "*" => LexemeType.Asterisk,
            "\"" => LexemeType.DoubleQuote,
            "'" => LexemeType.SingleQuote,
            " " => LexemeType.WhiteSpace,
            "\n" => LexemeType.NewLine,
            "\r" => LexemeType.CarriageReturn,
            "\t" => LexemeType.Tab,
            _ => LexemeType.Unknown
        };
}

// Private fields.
public static partial class Lexer
{
    private static readonly HashSet<char> _delimiters =
        new()
        {
            '@',
            '<',
            '>',
            '(',
            ')',
            '{',
            '}',
            '/',
            '\\',
            '-',
            '!',
            '*',
            ' ',
            '\n',
            '\r',
            '\t',
            '\'',
            '"'
        };
}
