using FluentAssertions;

namespace Razier.Lexing.Tests.Unit;

public sealed class ValueTests
{
    [Fact]
    public void Value_ShouldReturnTokenValue_WhenCalledWithSource()
    {
        // Arrange.
        var input = "@()text";

        // Act.
        var tokens = Lexer.Lex(input).Where(x => x.Type == LexemeType.Text);

        // Assert.
        tokens.First().Value(input).ToString().Should().Be("text");
    }
}
