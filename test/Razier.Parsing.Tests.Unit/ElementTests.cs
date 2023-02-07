using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class ElementTests
{
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
