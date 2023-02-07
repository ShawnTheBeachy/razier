using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class ExplicitRazorExpressionTests
{
    [Fact]
    public void Close_ShouldReturnRightParenthesis_WhenCalledWithSource()
    {
        // Arrange.
        var input = "@(code)";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);
        var razorToken = (ExplicitRazorExpressionToken)tokens.First();

        // Assert.
        razorToken.Close(input).ToString().Should().Be(")");
    }

    [Fact]
    public void Code_ShouldReturnTextBetweenParentheses_WhenCalledWithSource()
    {
        // Arrange.
        var input = "@(code)";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);
        var razorToken = (ExplicitRazorExpressionToken)tokens.First();

        // Assert.
        razorToken.Code(input).ToString().Should().Be("code");
    }

    [Fact]
    public void Open_ShouldReturnAtSymbolAndLeftParenthesis_WhenCalledWithSource()
    {
        // Arrange.
        var input = "@(code)";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);
        var razorToken = (ExplicitRazorExpressionToken)tokens.First();

        // Assert.
        razorToken.Open(input).ToString().Should().Be("@(");
    }

    [Fact]
    public void Parse_ShouldNotExitEarly_WhenParenthesesAreNested()
    {
        // Arrange.
        var input = "@(())";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);
        var razorToken = (ExplicitRazorExpressionToken)tokens.First();

        // Assert.
        razorToken.CloseOffset.Should().Be(4);
        razorToken.Code(input).ToString().Should().Be("()");
    }

    [Fact]
    public void Parse_ShouldNotExitEarly_WhenRightParenthesisIsInString()
    {
        // Arrange.
        var input = "@(\"Code)\")";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);
        var razorToken = (ExplicitRazorExpressionToken)tokens.First();

        // Assert.
        razorToken.CloseOffset.Should().NotBe(7);
        razorToken.CloseOffset.Should().Be(9);
    }

    [Fact]
    public void Parse_ShouldNotReturnExplicitRazorExpression_WhenAtSymbolIsEscaped()
    {
        // Arrange.
        var input = "@@()";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);

        // Assert.
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldReturnExplicitRazorExpression_WhenAtSymbolIsFollowedByLeftParenthesis()
    {
        // Arrange.
        var input = "@(code)";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);

        // Assert.
        tokens.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldSetExplicitRazorExpressionOffsets_WhenCalled()
    {
        // Arrange.
        var input = "@(code)";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ExplicitRazorExpressionToken);
        var razorToken = (ExplicitRazorExpressionToken)tokens.First();

        // Assert.
        razorToken.CloseOffset.Should().Be(6);
        razorToken.OpenOffset.Should().Be(0);
    }
}
