using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class FormatterTests
{
    [Theory]
    [InlineData(
        "using System;\r\n\r\n\r\nusing System.Text;",
        "using System;\r\n\r\nusing System.Text;\r\n"
    )]
    [InlineData(
        "using System;\r\n\r\n\r\n\rusing System.Text;",
        "using System;\r\n\r\nusing System.Text;\r\n"
    )]
    [InlineData("using System;\r\n\r\n\r\n<div></div>", "using System;\r\n\r\n<div></div>\r\n")]
    public void Format_ShouldCollapseMoreThanTwoNewLines_WhenNotIndented(
        string input,
        string expected
    )
    {
        // Arrange.

        // Act.
        var formatted = Formatter.Format(input);

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData("<div></div>\r\n")]
    [InlineData("<div></div>\r\n\n")]
    [InlineData("<div></div>\r\n\t")]
    public void Format_ShouldEndOutputWithSingleNewLine_WhenCalled(string input)
    {
        // Arrange.

        // Act.
        var formatted = Formatter.Format(input);

        // Assert.
        formatted.Should().Be("<div></div>\r\n");
    }
}
