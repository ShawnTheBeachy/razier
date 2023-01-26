using FluentAssertions;

namespace Razier.Formatter;

public sealed class FormatterTests
{
    [Theory]
    [InlineData("<div><div></div></div>", "<div>\r\n\t<div></div>\r\n</div>")]
    [InlineData(
        "<div><div><input></div></div>",
        "<div>\r\n\t<div>\r\n\t\t<input>\r\n\t</div>\r\n</div>"
    )]
    public void Format_ShouldNewLineAndIndent_WhenReachesBeginTag(string input, string expected)
    {
        // Arrange.
        var formatter = new Formatter(input, "\t");

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }
}
