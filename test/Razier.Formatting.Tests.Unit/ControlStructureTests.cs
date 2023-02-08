using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class ControlStructureTests
{
    [Fact]
    public void Format_ShouldNotAddNewLine_WhenControlStructureIsFirstToken()
    {
        // Arrange.
        var input = "@foreach (value) { var x = 1; }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted.Should().Be("@foreach (value)\r\n{\r\n    var x = 1;\r\n}\r\n");
    }

    [Fact]
    public void Format_ShouldNotAddNewLineBeforeControlStructure_WhenIndentationLevelIsGreaterThanZero()
    {
        // Arrange.
        var input = "<div>@foreach (value){ var x = 1; }</div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("<div>\r\n\t@foreach (value)\r\n\t{\r\n\t    var x = 1;\r\n\t}\r\n</div>\r\n");
    }
}
