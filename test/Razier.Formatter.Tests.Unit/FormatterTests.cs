using FluentAssertions;

namespace Razier.Formatter;

public sealed class FormatterTests
{
    [Theory]
    [InlineData(
        "<div><div>@{while(true)var x = 1;}</div></div>",
        "<div>\r\n    <div>\r\n        @{\r\n            while (true)\r\n                var x = 1;\r\n        }\r\n    </div>\r\n</div>"
    )]
    public void Format_ShouldIndentCode_WhenIndentLevelIsMoreThanZero(string input, string expected)
    {
        // Arrange.
        var formatter = new Formatter(input);

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData("<div disabled></div>", "<div disabled></div>")]
    [InlineData(
        "<div disabled class='container'></div>",
        "<div\r\n\tdisabled\r\n\tclass='container'>\r\n</div>"
    )]
    [InlineData(
        "<div disabled active>Hello!</div>",
        "<div\r\n\tdisabled\r\n\tactive>\r\n\tHello!\r\n</div>"
    )]
    public void Format_ShouldKeepSoftCloseTagOnSameLine_WheneverPresent(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input, "\t");

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData("<div disabled></div>", "<div disabled></div>")]
    [InlineData("<input disabled>", "<input disabled>")]
    [InlineData("<input disabled />", "<input disabled />")]
    public void Format_ShouldKeepAttributeOnSameLine_WhenOnlyOneAttribute(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input);

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData("<div>Hello</div>", "<div>Hello</div>")]
    [InlineData("<div active>Hello</div>", "<div active>Hello</div>")]
    [InlineData(
        "<div active>1238456789012345678901234567890123456789012345678901234567890123456789012345678901</div>",
        "<div active>\r\n\t1238456789012345678901234567890123456789012345678901234567890123456789012345678901\r\n</div>"
    )]
    public void Format_ShouldKeepContentOnSameLine_WhenInTagWithOneOrZeroAttributesAndLessThan80Characters(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input, "\t");

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

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

    [Theory]
    [InlineData(
        "<div disabled class='container'></div>",
        "<div\r\n\tdisabled\r\n\tclass='container'>\r\n</div>"
    )]
    [InlineData(
        "<div active=\"true\" class='container'></div>",
        "<div\r\n\tactive=\"true\"\r\n\tclass='container'>\r\n</div>"
    )]
    public void Format_ShouldPutAttributesOnNewLine_WhenMoreThanOneAttribute(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input, "\t");

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData(
        "<div disabled class='container' />",
        "<div\r\n\tdisabled\r\n\tclass='container'\r\n/>"
    )]
    [InlineData(
        "<input active='true' class='username'>",
        "<input\r\n\tactive='true'\r\n\tclass='username'>"
    )]
    public void Format_ShouldPutCloseTagOnNewLineWithNoIndent_WhenMoreThanOneAttributeAndNotVoidElement(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input, "\t");

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    //[InlineData("<div>Hello<input></div>", "<div>\r\n\tHello\r\n\t<input>\r\n</div>")]
    [InlineData("<div>Hello\r\nHi<input></div>", "<div>\r\n\tHello\r\n\tHi\r\n\t<input>\r\n</div>")]
    public void Format_ShouldPutContentOnNewLineWithIndent_WhenFollowedByBeginTag(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input, "\t");

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }

    [Theory]
    [InlineData("<div></div>Hello", "<div></div>\r\nHello")]
    [InlineData("<input>Hello", "<input>\r\nHello")]
    public void Format_ShouldPutContentOnNewLineWithNoIndent_WhenAfterTag(
        string input,
        string expected
    )
    {
        // Arrange.
        var formatter = new Formatter(input);

        // Act.
        var formatted = formatter.Format();

        // Assert.
        formatted.Should().Be(expected);
    }
}
