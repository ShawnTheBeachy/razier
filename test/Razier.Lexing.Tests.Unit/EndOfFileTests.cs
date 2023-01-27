using FluentAssertions;

namespace Razier.Lexing.Tests.Unit;

public sealed class EndOfFileTests
{
    [Theory]
    [InlineData("@*", 3)]
    [InlineData("Content ", 3)]
    [InlineData(" ", 2)]
    public void Lex_ShouldNotReturnExtraLexeme_WhenEndOfFileIsReachedAndAllLexemesAreConsumed(
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
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Content")]
    public void Lex_ShouldReturnEndOfFileToken_WhenEndOfFileIsReached(string input)
    {
        // Arrange.

        // Act.
        var lexemes = Lexer.Lex(input);

        // Assert.
        lexemes.Last().Type.Should().Be(LexemeType.EndOfFile);
    }

    [Fact]
    public void Lex_ShouldReturnUnconsumedTokenWhen_EndOfFileIsReached()
    {
        // Arrange.
        var input = "Content";

        // Act.
        var lexemes = Lexer.Lex(input);

        // Assert.
        lexemes.Should().HaveCount(2);
        lexemes.First().Type.Should().Be(LexemeType.Text);
        lexemes.First().Offset.Should().Be(0);
        lexemes.First().Length.Should().Be(input.Length);
    }
}
