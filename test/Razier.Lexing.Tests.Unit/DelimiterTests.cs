using FluentAssertions;

namespace Razier.Lexing.Tests.Unit;

// Tests.
public sealed partial class DelimiterTests
{
    [Theory]
    [InlineData("  ", 3)]
    [InlineData("@*()", 5)]
    public void Lex_ShouldNotReturnExtraLexeme_WhenDelimiterIsReachedAndAllLexemesAreConsumed(
        string input,
        int expected
    )
    {
        // Arrange.

        // Act.
        var lexemes = Lexer.Lex(input);

        // Assert.
        lexemes.Should().HaveCount(expected);
    }

    [Theory]
    [MemberData(nameof(Delimiters))]
    public void Lex_ShouldReturnCorrectLexemeType_WhenLexemeIsDelimiter(
        string input,
        LexemeType type
    )
    {
        // Arrange.

        // Act.
        var lexemes = Lexer.Lex(input);

        // Assert.
        lexemes.First().Type.Should().Be(type);
        lexemes.First().Offset.Should().Be(0);
        lexemes.First().Length.Should().Be(1);
    }

    [Theory]
    [InlineData("Content ", 3)]
    [InlineData("@*Comment*@", 6)]
    public void Lex_ShouldReturnUnconsumedLexeme_WhenDelimiterIsReached(string input, int expected)
    {
        // Arrange.

        // Act.
        var lexemes = Lexer.Lex(input);

        // Assert.
        lexemes.Should().HaveCount(expected);
    }
}

// Private fields.
public sealed partial class DelimiterTests
{
    public static IEnumerable<object[]> Delimiters =>
        Enum.GetValues<LexemeType>()
            .Select(
                x =>
                    new object[]
                    {
                        x switch
                        {
                            LexemeType.At => "@",
                            LexemeType.LeftChevron => "<",
                            LexemeType.RightChevron => ">",
                            LexemeType.LeftParenthesis => "(",
                            LexemeType.RightParenthesis => ")",
                            LexemeType.LeftBrace => "{",
                            LexemeType.RightBrace => "}",
                            LexemeType.ForwardSlash => "/",
                            LexemeType.BackSlash => "\\",
                            LexemeType.Dash => "-",
                            LexemeType.Exclamation => "!",
                            LexemeType.Asterisk => "*",
                            LexemeType.SingleQuote => "'",
                            LexemeType.DoubleQuote => "\"",
                            LexemeType.WhiteSpace => " ",
                            LexemeType.NewLine => "\n",
                            LexemeType.CarriageReturn => "\r",
                            LexemeType.Tab => "\t",
                            LexemeType.Semicolon => ";",
                            LexemeType.Equals => "=",
                            _ => ""
                        },
                        x
                    }
            )
            .Where(x => x[0] is string s && s != "");
}
