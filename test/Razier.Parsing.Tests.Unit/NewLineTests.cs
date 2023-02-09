using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class NewLineTests
{
    [Fact]
    public void Parse_ShouldReturnNewLine_WhenCarriageReturnIsNotFollowedByNewLine()
    {
        // Arrange.
        var input = "\r";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is NewLineToken);

        // Assert.
        tokens.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShouldReturnOneNewLine_WhenCarriageReturnIsFollowedByNewLine()
    {
        // Arrange.
        var input = "\r\n";

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is NewLineToken);

        // Assert.
        tokens.Should().HaveCount(1);
    }
}
