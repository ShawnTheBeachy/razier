using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class CommentTests
{
    [Fact]
    public void Parse_ShouldReturnComment_WhenUsingHtmlDelimiters()
    {
        // Arrange.
        var input = "<!-- Comment -->";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CommentToken);

        // Assert.
        tokens.Should().HaveCount(1);
        var comment = (CommentToken)tokens.First();
        comment.Open(input).ToString().Should().Be("<!--");
        comment.Close(input).ToString().Should().Be("-->");
        comment.Content(input).ToString().Should().Be(" Comment ");
    }

    [Fact]
    public void Parse_ShouldReturnComment_WhenUsingRazorDelimiters()
    {
        // Arrange.
        var input = "@* Comment *@";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CommentToken);

        // Assert.
        tokens.Should().HaveCount(1);
        var comment = (CommentToken)tokens.First();
        comment.Open(input).ToString().Should().Be("@*");
        comment.Close(input).ToString().Should().Be("*@");
        comment.Content(input).ToString().Should().Be(" Comment ");
    }
}
