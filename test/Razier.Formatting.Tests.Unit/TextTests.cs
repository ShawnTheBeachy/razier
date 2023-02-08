using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class TextTests
{
    [Fact]
    public void Format_ShouldIndentTextOnNewLine_WhenMultipleChildrenArePresent()
    {
        // Arrange.
        var input = "<div>Content<input></div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("<div>\r\n\tContent\r\n\t<input>\r\n</div>\r\n");
    }

    [Fact]
    public void Format_ShouldKeepTextOnSameLine_WhenNoAttributes()
    {
        // Arrange.
        var input = "<div>Content</div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("<div>Content</div>\r\n");
    }

    [Fact]
    public void Format_ShouldTrimText_WhenCalled()
    {
        // Arrange.
        var input = "<div>\n\t  Content\r\n</div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("<div>Content</div>\r\n");
    }
}
