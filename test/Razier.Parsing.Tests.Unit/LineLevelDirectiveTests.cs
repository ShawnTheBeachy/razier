using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class LineLevelDirectiveTests
{
    [Theory]
    [InlineData("@implements Value")]
    [InlineData("@inherits Value")]
    [InlineData("@using Value")]
    [InlineData("@model Value")]
    [InlineData("@inject Value")]
    [InlineData("@layout Value")]
    [InlineData("@namespace Value")]
    [InlineData("@page Value")]
    [InlineData("@preservewhitespace Value")]
    [InlineData("@section Value")]
    [InlineData("@typeparam Value")]
    public void Parse_ShouldReturnLineLevelDirective_WhenAnyKeywordIsReached(string input)
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is LineLevelDirectiveToken);

        // Assert.
        tokens.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldSetLineLevelDirectiveKeyword_WhenPresent()
    {
        // Arrange.
        var input = "@implements Value";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is LineLevelDirectiveToken);
        var directive = (LineLevelDirectiveToken)tokens.First();

        // Assert.
        directive.Directive(input).ToString().Should().Be("@implements");
    }

    [Fact]
    public void Parse_ShouldSetLineLevelDirectiveLine_WhenPresent()
    {
        // Arrange.
        var input = "@implements Value<Type>;\n";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is LineLevelDirectiveToken);
        var directive = (LineLevelDirectiveToken)tokens.First();

        // Assert.
        directive.Line(input).ToString().Should().Be("Value<Type>;");
    }
}
