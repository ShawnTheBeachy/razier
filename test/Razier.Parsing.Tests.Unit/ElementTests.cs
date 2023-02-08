using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class ElementTests
{
    [Theory]
    [InlineData("<div><div></div></div>", 1)]
    [InlineData("<div><div></div><span></span></div>", 2)]
    [InlineData("<div><p></p><p></p><p></p></div>", 3)]
    public void Parse_ShouldAddChildToElement_WhenChildIsElement(string input, int expected)
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Children.Should().HaveCount(expected);
    }

    [Fact]
    public void Parse_ShouldAddChildToElement_WhenChildIsPlainText()
    {
        // Arrange.
        var input = "<div>Content</div>";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Children.Should().HaveCount(1);
        element.Children.First().Should().BeOfType<TextToken>();
    }

    [Fact]
    public void Parse_ShouldConsumeElementClosingTag_WhenHasContent()
    {
        // Arrange.
        var input = "<div><input></div>";

        // Act.
        var tokens = Parser.Parse(input);

        // Assert.
        tokens.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("<div />")]
    [InlineData("<div></div>")]
    [InlineData("<div><input></div>")]
    public void Parse_ShouldReturnElement_WhenClosedWithOrWithoutContent(string input)
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);

        // Assert.
        tokens.Should().HaveCount(1);
        ((ElementToken)tokens.First()).Name(input).ToString().Should().Be("div");
    }
}
