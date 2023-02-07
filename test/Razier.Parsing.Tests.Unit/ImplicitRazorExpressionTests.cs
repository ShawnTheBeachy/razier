using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class ImplicitRazorExpressionTests
{
    [Fact]
    public void Parse_ShouldReturnImplicitRazorExpression_WhenNextWordIsNotReserved()
    {
        // Arrange.
        var input = "@myValue";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ImplicitRazorExpressionToken);

        // Assert.
        tokens.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldTerminateImplicitRazorExpression_WhenLeftChevronIsReached()
    {
        // Arrange.
        var input = "<div>@myValue</div>";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Children.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldTerminateImplicitRazorExpression_WhenSpaceIsReached()
    {
        // Arrange.
        var input = "@myValue @mySecondValue";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ImplicitRazorExpressionToken);

        // Assert.
        tokens.Should().HaveCount(2);
    }

    [Fact]
    public void Value_ShouldReturnEntireValue_WhenCalled()
    {
        // Arrange.
        var input = "@myValue";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ImplicitRazorExpressionToken);
        var imp = (ImplicitRazorExpressionToken)tokens.First();

        // Assert.
        imp.Value(input).ToString().Should().Be(input);
    }
}
