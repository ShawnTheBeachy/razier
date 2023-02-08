using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class FormatterTests
{
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
