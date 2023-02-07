using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class CSharpTests
{
    [Theory]
    [InlineData("@{ foreach() { } }", " foreach() { } ")]
    [InlineData("@{ while() { foreach() { } } }", " while() { foreach() { } } ")]
    public void Parse_ShouldNotTerminateCSharp_WhenBracesAreNested(string input, string expected)
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);
        var code = (CodeBlockToken)tokens.First();

        // Arrange.
        code.Children.Should().HaveCount(1);
        code.Children.First().Should().BeOfType<CSharpToken>();
        ((CSharpToken)code.Children.First()).Code(input).ToString().Should().Be(expected);
    }

    [Fact]
    public void Parse_ShouldNotTerminateCSharp_WhenSemicolonIsInString()
    {
        // Arrange.
        var input = "@{ var x = \";\"; }";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);
        var code = (CodeBlockToken)tokens.First();

        // Arrange.
        code.Children.Should().HaveCount(1);
        code.Children.First().Should().BeOfType<CSharpToken>();
        ((CSharpToken)code.Children.First()).Code(input).ToString().Should().Be(" var x = \";\"; ");
    }

    [Fact]
    public void Parse_ShouldReturnCSharp_WhenInCodeBlock()
    {
        // Arrange.
        var input = "@{ var x = 1; }";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);
        var code = (CodeBlockToken)tokens.First();

        // Arrange.
        code.Children.Should().HaveCount(1);
        code.Children.First().Should().BeOfType<CSharpToken>();
        ((CSharpToken)code.Children.First()).Code(input).ToString().Should().Be(" var x = 1; ");
    }

    [Fact]
    public void Parse_ShouldTerminateCSharp_WhenBracesAreInString()
    {
        // Arrange.
        var input = "@{ var x = \"{\"; }";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);
        var code = (CodeBlockToken)tokens.First();

        // Arrange.
        code.Children.Should().HaveCount(1);
        code.Children.First().Should().BeOfType<CSharpToken>();
        ((CSharpToken)code.Children.First()).Code(input).ToString().Should().Be(" var x = \"{\"; ");
    }
}
