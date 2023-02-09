using FluentAssertions;

namespace Razier.Formatting.Tests.Unit;

public sealed class CodeBlockTests
{
    [Theory]
    [InlineData("<div>@{ //Comment\r\n\r\n\r\npublic void A() { } }</div>")]
    [InlineData("<div>@{ //Comment\r\n\r\r\n\r\npublic void A() { } }</div>")]
    public void Format_ShouldCollapseNewLines_WhenMoreThanTwoConsecutive(string input)
    {
        // Arrange.

        // Act.
        var formatted = Formatter.Format(input, "  ");

        // Assert.
        formatted
            .Should()
            .Be(
                @"<div>
  @{
      //Comment

      public void A() { }
  }
</div>
"
            );
    }

    [Fact]
    public void Format_ShouldFormatHtmlElement_WhenInCodeBlock()
    {
        // Arrange.
        var input = "@{ <div class='container'>Content</div> }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("@{\r\n    <div class=\"container\">\r\n    \tContent\r\n    </div>\r\n}\r\n");
    }

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

    [Fact]
    public void Format_ShouldPreserveCSharpierNewLines_WhenPresent()
    {
        // Arrange.
        var input = "@code { private void A() { } private void B() { } }";

        // Act.
        var formatted = Formatter.Format(input, "\t");

        // Assert.
        formatted
            .Should()
            .Be("@code\r\n{\r\n    private void A() { }\r\n\r\n    private void B() { }\r\n}\r\n");
    }
}
