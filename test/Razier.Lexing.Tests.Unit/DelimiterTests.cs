using FluentAssertions;

namespace Razier.Lexing.Tests.Unit;

public sealed class DelimiterTests
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
    [InlineData("@", LexemeType.At)]
    [InlineData("<", LexemeType.LeftChevron)]
    [InlineData(">", LexemeType.RightChevron)]
    [InlineData("(", LexemeType.LeftParenthesis)]
    [InlineData(")", LexemeType.RightParenthesis)]
    [InlineData("{", LexemeType.LeftBrace)]
    [InlineData("}", LexemeType.RightBrace)]
    [InlineData("/", LexemeType.ForwardSlash)]
    [InlineData("\\", LexemeType.BackSlash)]
    [InlineData("-", LexemeType.Dash)]
    [InlineData("!", LexemeType.Exclamation)]
    [InlineData("*", LexemeType.Asterisk)]
    [InlineData("'", LexemeType.SingleQuote)]
    [InlineData("\"", LexemeType.DoubleQuote)]
    [InlineData(" ", LexemeType.WhiteSpace)]
    [InlineData("\n", LexemeType.NewLine)]
    [InlineData("\r", LexemeType.CarriageReturn)]
    [InlineData("\t", LexemeType.Tab)]
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
