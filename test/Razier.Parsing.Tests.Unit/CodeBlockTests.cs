using FluentAssertions;
using Razier.Parsing.Tokens;

namespace Razier.Parsing.Tests.Unit;

public sealed class CodeBlockTests
{
    [Theory]
    [InlineData("@code", false)]
    [InlineData("@code { }", true)]
    [InlineData("@{ }", true)]
    [InlineData("@functions", false)]
    [InlineData("@functions { }", true)]
    public void Parse_ShouldDetectCodeBlock_WhenAnyIdentifierIsMatched(
        string input,
        bool isCodeBlock
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);

        // Assert.
        if (isCodeBlock)
            tokens.Should().HaveCount(1);
        else
            tokens.Should().BeEmpty();
    }

    [Theory]
    [InlineData("@{ }", 2)]
    [InlineData("@code { }", 7)]
    [InlineData("@functions { }", 12)]
    public void Parse_ShouldDetectCodeBlockOpenLength_WhenAnyIdentifierIsUsed(
        string input,
        int expectedLength
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);
        var codeToken = (CodeBlockToken)tokens.First();

        // Assert.
        codeToken.OpenLength.Should().Be(expectedLength);
    }

    [Theory]
    [InlineData("@\r\n{ }", false)]
    [InlineData("@code\r\n{ }", true)]
    [InlineData("@functions\r\n{ }", true)]
    public void Parse_ShouldDetectCodeBlock_WhenKeywordIsUsedAndLeftBraceIsOnNewLine(
        string input,
        bool shouldMatch
    )
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);

        // Assert.
        if (shouldMatch)
            tokens.Should().HaveCount(1);
        else
            tokens.Should().BeEmpty();
    }

    [Theory]
    [InlineData("@{<div></div>}")]
    [InlineData("@code { var x = 1;\n<div></div>}")]
    public void Parse_ShouldSwitchToHtml_WhenOpenTagIsEncountered(string input)
    {
        // Arrange.

        // Act.
        var tokens = Parser.Parse(input).Where(x => x is CodeBlockToken);
        var code = (CodeBlockToken)tokens.First();

        // Assert.
        code.Children.Where(x => x is ElementToken).Should().HaveCount(1);
    }
}
