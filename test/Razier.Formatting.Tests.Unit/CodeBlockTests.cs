using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class CodeBlockTests
{
    [Fact]
    public void Format_ShouldKeepOpenBraceOnSameLine_WhenNoWordIsPresent()
    {
        // Arrange.
        var input = "@{ var x = 1; }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("@{\r\n    var x = 1;\r\n}\r\n");
    }

    [Fact]
    public void Format_ShouldNotAddNewLineBeforeCodeBlock_WhenIndentationLevelIsGreaterThanZero()
    {
        // Arrange.
        var input = "<div>@{ var x = 1; }</div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("<div>\r\n\t@{\r\n\t    var x = 1;\r\n\t}\r\n</div>\r\n");
    }

    [Fact]
    public void Format_ShouldNotAddNewLine_WhenCodeBlockIsFirstToken()
    {
        // Arrange.
        var input = "@{ var x = 1; }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("@{\r\n    var x = 1;\r\n}\r\n");
    }
}
