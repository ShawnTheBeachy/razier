using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class ElementTests
{
    [Fact]
    public void Format_ShouldCloseWithRightChevron_WhenElementIsVoid()
    {
        // Arrange.
        var input = "<input />";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("<input>\r\n");
    }

    [Theory]
    [InlineData(
        "<button class='btn' disabled></button>",
        "<button\r\n\tclass=\"btn\"\r\n\tdisabled>\r\n</button>\r\n"
    )]
    [InlineData(
        "<div class='container' disabled><p></p></div>",
        "<div\r\n\tclass=\"container\"\r\n\tdisabled>\r\n\t<p></p>\r\n</div>\r\n"
    )]
    public void Format_ShouldIndentAttributesOnNewLines_WhenMultipleAttributes(
        string input,
        string expected
    )
    {
        // Arrange.

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData("<p><p></p></p>", "<p>\r\n\t<p></p>\r\n</p>\r\n")]
    [InlineData("<p><p></p><p></p></p>", "<p>\r\n\t<p></p>\r\n\t<p></p>\r\n</p>\r\n")]
    [InlineData("<p><p><p></p></p></p>", "<p>\r\n\t<p>\r\n\t\t<p></p>\r\n\t</p>\r\n</p>\r\n")]
    public void Format_ShouldIndentChildren_WhenInsideParent(string input, string expected)
    {
        // Arrange.

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be(expected);
    }

    [Fact]
    public void Format_ShouldIndentElement_WhenInsideCodeBlock()
    {
        // Arrange.
        var input = "@foreach (value) { <div></div> }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("@foreach (value)\r\n{\r\n    <div></div>\r\n}\r\n");
    }

    [Fact]
    public void Format_ShouldInsertNewLine_WhenInsideCodeBlockAndAfterCSharp()
    {
        // Arrange.
        var input = "@foreach (value) { var x = 1;<div></div> }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("@foreach (value)\r\n{\r\n    var x = 1;\r\n\r\n    <div></div>\r\n}\r\n");
    }

    [Theory]
    [InlineData("<button class='btn'></button>", "<button class=\"btn\"></button>\r\n")]
    [InlineData(
        "<div class='container'><p></p></div>",
        "<div class=\"container\">\r\n\t<p></p>\r\n</div>\r\n"
    )]
    public void Format_ShouldKeepAttributeOnSameLine_WhenOnlyOneAttribute(
        string input,
        string expected
    )
    {
        // Arrange.

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be(expected);
    }

    [Fact]
    public void Format_ShouldNotDuplicateNewLine_WhenInsideCodeBlockAndAfterCSharpWithNewLine()
    {
        // Arrange.
        var input = "@foreach (value) { var x = 1;\r\n<div></div> }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("@foreach (value)\r\n{\r\n    var x = 1;\r\n\r\n    <div></div>\r\n}\r\n");
    }
}
