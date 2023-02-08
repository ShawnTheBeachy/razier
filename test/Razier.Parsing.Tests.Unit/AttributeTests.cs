using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class AttributeTests
{
    [Fact]
    public void Parse_ShouldNotTerminateAttributeName_WhenDashIsPresent()
    {
        // Arrange.
        var input = "<div aria-labelledby=''></div>";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldNotTerminateAttributeName_WhenSemicolonIsPresent()
    {
        // Arrange.
        var input = "<div @bind:event='oninput'></div>";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
        element.Attributes.First().Key(input).ToString().Should().Be("@bind:event");
        element.Attributes.First().Value(input).ToString().Should().Be("oninput");
    }

    [Fact]
    public void Parse_ShouldNotTerminateAttributeValue_WhenInExplicitRazorExpression()
    {
        // Arrange.
        var input = "<div class=\"@(true ? \"\" : \"\")\"></div>";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldReturnAllAttributes_WhenMultipleArePresent()
    {
        // Arrange;
        var input = "<button disabled class='btn' />";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(2);
        element.Attributes.First().Key(input).ToString().Should().Be("disabled");
        element.Attributes.First().Value(input).ToString().Should().Be("");
        element.Attributes.Last().Key(input).ToString().Should().Be("class");
        element.Attributes.Last().Value(input).ToString().Should().Be("btn");
    }

    [Theory]
    [InlineData("<button class=@myClass></button>")]
    [InlineData("<button class=@myClass\n/>")]
    [InlineData("<button class=@myClass/>")]
    public void Parse_ShouldReturnAttribute_WhenNoQuotesArePresent(string input)
    {
        // Arrange;

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
        element.Attributes.First().Key(input).ToString().Should().Be("class");
        element.Attributes.First().Value(input).ToString().Should().Be("@myClass");
    }

    [Fact]
    public void Parse_ShouldReturnAttribute_WhenNoValueIsPresent()
    {
        // Arrange;
        var input = "<button disabled/>";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
        element.Attributes.First().Key(input).ToString().Should().Be("disabled");
    }

    [Theory]
    [InlineData("<div class =\n'container'></div>")]
    [InlineData("<div class = 'container'></div>")]
    [InlineData("<div class\t='container'></div>")]
    [InlineData("<div class\r\n=\t'container'></div>")]
    public void Parse_ShouldReturnAttribute_WhenSpacesOrNewLinesArePresent(string input)
    {
        // Arrange;

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
        element.Attributes.First().Key(input).ToString().Should().Be("class");
        element.Attributes.First().Value(input).ToString().Should().Be("container");
    }

    [Fact]
    public void Parse_ShouldReturnAttribute_WhenValueIsPresent()
    {
        // Arrange;
        var input = "<button title=\"Hey\" />";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
        element.Attributes.First().Key(input).ToString().Should().Be("title");
    }

    [Theory]
    [InlineData("<button title=\"Hey\" />")]
    [InlineData("<button title='Hey' />")]
    public void Parse_ShouldReturnAttributeValue_WhenUsingDoubleOrSingleQuotes(string input)
    {
        // Arrange;

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.Should().HaveCount(1);
        element.Attributes.First().Value(input).ToString().Should().Be("Hey");
    }

    [Fact]
    public void Value_ShouldReturnEmpty_WhenNoValueIsPresent()
    {
        // Arrange;
        var input = "<button disabled />";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is ElementToken);
        var element = (ElementToken)tokens.First();

        // Assert.
        element.Attributes.First().Value(input).ToString().Should().Be("");
    }
}
