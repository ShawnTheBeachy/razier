using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class TextTests
{
    [Fact]
    public void Parse_ShouldNotThrow_WhenTextExtendsToEndOfFile()
    {
        // Arrange.
        var input = "Text extends to end of file";

        // Act.
        var parse = () => Parser.Parse(input);

        // Assert.
        parse.Should().NotThrow();
    }

    [Theory]
    [InlineData("<div>Content @value here</div>", 2)]
    [InlineData("<div>Content @value goes @value here</div>", 3)]
    public void Parse_ShouldReturnAllTextTokens_WhenInterruptedByImplicitRazorExpression(
        string input,
        int expected
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input);
        var element = (ElementToken)tokens.First();
        var textTokens = element.Children.Where(x => x is TextToken);

        // Assert.
        textTokens.Should().HaveCount(expected);
    }

    [Theory]
    [InlineData("<div>Content here</div>", "Content here")]
    [InlineData("<div>Content goes here</div>", "Content goes here")]
    public void Parse_ShouldReturnOneTextToken_WhenMultipleWordsArePresent(
        string input,
        string expected
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input);
        var element = (ElementToken)tokens.First();
        var textTokens = element.Children.Where(x => x is TextToken);

        // Assert.
        textTokens.Should().HaveCount(1);
        ((TextToken)textTokens.First()).Value(input).ToString().Should().Be(expected);
    }

    [Fact]
    public void Parse_ShouldTerminateTextToken_WhenNonWordTokenIsEncountered()
    {
        // Arrange.
        var input = "<div>Content here @value</div>";

        // Act.
        var tokens = Parser.Parse(input);
        var element = (ElementToken)tokens.First();
        var textTokens = element.Children.Where(x => x is TextToken);

        // Assert.
        textTokens.Should().HaveCount(1);
        ((TextToken)textTokens.First()).Value(input).ToString().Should().Be("Content here ");
    }
}
