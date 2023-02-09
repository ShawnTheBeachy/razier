using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class ControlStructureTests
{
    [Fact]
    public void Format_ShouldFormatControlStructureExpression_WhenPresent()
    {
        // Arrange.
        var input = "@foreach( var x in y ) { }";

        // Act.
        var formatted = Formatter.Format(input, "    ");

        // Assert.
        formatted.Should().Be("@foreach (var x in y)\r\n{\r\n\r\n}\r\n");
    }

    [Fact]
    public void Format_ShouldFormatElseWithCSharpier_WhenPresent()
    {
        // Arrange.
        var input = "@if (true) { } else { }";

        // Act.
        var formatted = Formatter.Format(input, "    ");

        // Assert.
        formatted.Should().Be("@if (true) { }\r\nelse { }\r\n");
    }

    [Fact]
    public void Format_ShouldIndentControlStructures_WhenNested()
    {
        // Arrange.
        var input = "<div>@foreach (value) { <div>@foreach (value) { <p></p> }</div> }</div>";

        // Act.
        var formatted = Formatter.Format(input, "    ");

        // Assert.
        formatted
            .Should()
            .Be(
                @"<div>
    @foreach (value)
    {
        <div>
            @foreach (value)
            {
                <p></p>
            }
        </div>
    }
</div>
"
            );
    }

    [Fact]
    public void Format_ShouldIndentHtml_WhenInControlStructureLoop()
    {
        // Arrange.
        var input = "<div>@foreach (value) { <div></div> }</div>";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("<div>\r\n\t@foreach (value)\r\n\t{\r\n\t    <div></div>\r\n\t}\r\n</div>\r\n");
    }

    [Fact]
    public void Format_ShouldPlaceWhileAfterDo_WhenControlStructureIsDoWhile()
    {
        // Arrange.
        var input = "@do { <div></div> } while (true);";

        // Act.
        var formatted = Formatter.Format(input, "    ");

        // Assert.
        formatted
            .Should()
            .Be(
                @"@do
{
    <div></div>
} while (true);
"
            );
    }
}
