using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class CommentTests
{
    [Fact]
    public void Format_ShouldCollapseMultipleNewLines_WhenEncountered()
    {
        // Arrange.
        var input = "<div><!-- Comment\r\nComment --></div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("<div>\r\n\t<!--\r\n\t\tComment\r\n\t\tComment\r\n\t-->\r\n</div>\r\n");
    }

    [Fact]
    public void Format_ShouldIndentCommentOnNewLine_WhenEncountered()
    {
        // Arrange.
        var input = "<div><!-- Comment --></div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("<div>\r\n\t<!-- Comment -->\r\n</div>\r\n");
    }

    [Theory]
    [InlineData(
        "<div>@* Comment\rComment*@</div>",
        "<div>\r\n\t@*\r\n\t\tComment\r\n\t\tComment\r\n\t*@\r\n</div>\r\n"
    )]
    [InlineData(
        "<div>@* Comment\rComment\nComment*@</div>",
        "<div>\r\n\t@*\r\n\t\tComment\r\n\t\tComment\r\n\t\tComment\r\n\t*@\r\n</div>\r\n"
    )]
    public void Format_ShouldIndentCommentOnMultipleNewLines_WhenCommentContainsNewLines(
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
    [InlineData("<div>@*   Comment   *@</div>", "<div>\r\n\t@* Comment *@\r\n</div>\r\n")]
    [InlineData("<div>@*\tComment\t*@</div>", "<div>\r\n\t@* Comment *@\r\n</div>\r\n")]
    [InlineData("<div>@*\r\nComment\n*@</div>", "<div>\r\n\t@* Comment *@\r\n</div>\r\n")]
    public void Format_ShouldIgnoreLeadingAndTrailingWhiteSpaceCharacters_WhenPresent(
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
}
