using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class ControlStructureTests
{
    [Theory]
    [InlineData("@if (value) { }")]
    [InlineData("else if (value) { }")]
    [InlineData("@else if (value) { }")]
    [InlineData("else { }")]
    [InlineData("@else { }")]
    [InlineData("@switch (value) { }")]
    [InlineData("@for (value) { }")]
    [InlineData("@foreach (value) { }")]
    [InlineData("@while (value) { }")]
    [InlineData("@do { } while (value);")]
    [InlineData("@using (value) { }")]
    [InlineData("@try { }")]
    [InlineData("catch (ex) { }")]
    [InlineData("@catch (ex) { }")]
    [InlineData("finally { }")]
    [InlineData("@finally { }")]
    [InlineData("@lock (value) { }")]
    public void Parse_ShouldReturnControlStructure_WhenAnyControlStructureIdentifierIsReached(
        string input
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ControlStructureToken);

        // Assert.
        tokens.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("@if (value) { }", "(value)")]
    [InlineData("else if (value) { }", "(value)")]
    [InlineData("else { }")]
    [InlineData("@switch (value) { }", "(value)")]
    [InlineData("@for (value) { }", "(value)")]
    [InlineData("@foreach (value) { }", "(value)")]
    [InlineData("@while (value) { }", "(value)")]
    [InlineData("@do { } while (value);", "while (value);")]
    [InlineData("@using (value) { }", "(value)")]
    [InlineData("@try { }")]
    [InlineData("catch (ex) { }", "(ex)")]
    [InlineData("finally { }")]
    [InlineData("@lock (value) { }", "(value)")]
    public void Parse_ShouldSetControlStructureExpression_WhenExpressionIsPresent(
        string input,
        string? expectedExpression = null
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ControlStructureToken);
        var control = (ControlStructureToken)tokens.First();
        var actualExpression = control.Expression(input).ToString();

        // Assert.
        if (expectedExpression is null)
            actualExpression.Should().Be("");
        else
            actualExpression.Should().Be(expectedExpression);
    }

    [Theory]
    [InlineData("@while (value) {<div></div>}")]
    [InlineData("@foreach (value) { var x = 1;\n<div></div>}")]
    public void Parse_ShouldSwitchToHtml_WhenOpenTagIsEncountered(string input)
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ControlStructureToken);
        var control = (ControlStructureToken)tokens.First();

        // Assert.
        control.Children.Where(x => x is ElementToken).Should().HaveCount(1);
    }
}
