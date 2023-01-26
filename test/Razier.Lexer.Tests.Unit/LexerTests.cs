using FluentAssertions;
using Razier.Lexer.Tokens;

namespace Razier.Lexer.Tests.Unit;

public sealed class LexerTests
{
    [Theory]
    [InlineData("<div class=\\'")]
    [InlineData("<div class=\\\"")]
    public void Lex_ShouldNotReturnStringDelimiterToken_WhenDelimiterIsEscaped(string input)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is StringDelimiterToken);

        // Assert.
        tokens.Should().BeEmpty();
    }

    [Fact]
    public void Lex_ShouldNotThrow_WhenTokensAreWhiteSpace()
    {
        // Arrange.
        var input = "    ";
        var lexer = new Lexer(input);

        // Act.
        var tokens = () => lexer.Lex();

        // Assert.
        tokens.Should().NotThrow();
    }

    [Fact]
    public void Lex_ShouldReturnBeginCloseTagToken_WhenSlashFollowsOpenBracket()
    {
        // Arrange.
        var input = "</";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex();

        // Assert.
        tokens.First().Should().BeOfType<BeginCloseTagToken>();
        tokens.First().Value.ToString().Should().Be("</");
    }

    [Theory]
    [InlineData("@code {")]
    [InlineData("@code   {")]
    [InlineData("@code{")]
    [InlineData("@code  {")]
    [InlineData("@code \n{")]
    public void Lex_ShouldReturnBeginCodeBlockToken_WhenAtSymbolIsFollowedByCodeOpenBrace(
        string input
    )
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex();

        // Assert.
        tokens.First().Should().BeOfType<BeginCodeBlockToken>();
        tokens.First().Value.ToString().Should().Be(input);
    }

    [Fact]
    public void Lex_ShouldReturnBeginCodeBlockToken_WhenAtSymbolIsFollowedByOpenBrace()
    {
        // Arrange.
        var input = "@{";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex();

        // Assert.
        tokens.First().Should().BeOfType<BeginCodeBlockToken>();
        tokens.First().Value.ToString().Should().Be(input);
    }

    [Fact]
    public void Lex_ShouldReturnBeginCommentToken_WhenExclamationDashDashFollowsOpenBracket()
    {
        // Arrange.
        var input = "<!--";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex();

        // Assert.
        tokens.First().Should().BeOfType<BeginCommentToken>();
        tokens.First().Value.ToString().Should().Be("<!--");
    }

    [Fact]
    public void Lex_ShouldReturnBeginOpenTagToken_WhenSlashDoesNotFollowOpenBracket()
    {
        // Arrange.
        var input = "<";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex();

        // Assert.
        tokens.First().Should().BeOfType<BeginOpenTagToken>();
        tokens.First().Value.ToString().Should().Be("<");
    }

    [Fact]
    public void Lex_ShouldReturnCarriageReturnToken_WhenPresent()
    {
        // Arrange.
        var input = "\r";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is CarriageReturnToken);

        // Assert.
        tokens.Should().HaveCount(1);
        tokens.First().Value.ToString().Should().Be(input);
    }

    [Theory]
    [InlineData("@code { foreach { } }", " foreach { } ")]
    [InlineData("@code { foreach { while { } } }", " foreach { while { } } ")]
    public void Lex_ShouldReturnCodeBlockContentToken_WhenCodeIsNested(
        string input,
        string expected
    )
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is CodeBlockContentToken);

        // Assert.
        tokens.Should().HaveCount(1);
        tokens.First().Value.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("@code { foreach { } }")]
    [InlineData("@code { }")]
    [InlineData("@{ var x = 1; \n}")]
    public void Lex_ShouldReturnEndCodeBlockToken_WhenEndOfCodeBlockIsReached(string input)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is EndCodeBlockToken);

        // Assert.
        tokens.Should().HaveCount(1);
        tokens.First().Value.ToString().Should().Be("}");
    }

    [Theory]
    [InlineData("-->")]
    [InlineData("comment-->")]
    public void Lex_ShouldReturnEndCommentToken_WhenDashCloseBracketFollowsDash(string input)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is EndCommentToken);

        // Assert.
        tokens.Should().HaveCount(1);
        tokens.First().Value.ToString().Should().Be("-->");
    }

    [Theory]
    [InlineData("<div>", 1)]
    [InlineData("<div></div>", 2)]
    public void Lex_ShouldReturnEndTagToken_WhenGreaterThanCharacterExists(
        string input,
        int expected
    )
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is EndTagToken);

        // Assert.
        tokens.Should().HaveCount(expected);
        tokens.Select(x => x.Value.ToString()).Should().AllBe(">");
    }

    [Theory]
    [InlineData("  ")]
    [InlineData("")]
    public void Lex_ShouldReturnEofToken_WhenEndIsReached(string input)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex();

        // Assert.
        tokens.Last().Should().BeOfType<EndOfFileToken>();
    }

    [Theory]
    [InlineData("<div class='btn'>", 1)]
    [InlineData("<div>1 + 1 = 2</div>", 1)]
    public void Lex_ShouldReturnEqualsToken_WhenInStringOrNotInString(string input, int expected)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is EqualsToken);

        // Assert.
        tokens.Should().HaveCount(expected);
        tokens.Select(x => x.Value.ToString()).Should().AllBe("=");
    }

    [Theory]
    [InlineData("\\", 1)]
    [InlineData("\\\\", 2)]
    [InlineData("\\\\\\", 3)]
    public void Lex_ShouldReturnEscapeToken_WhenPresent(string input, int expected)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is EscapeToken);

        // Assert.
        tokens.Should().HaveCount(expected);
        tokens.Select(x => x.Value.ToString()).Should().AllBe("\\");
    }

    [Fact]
    public void Lex_ShouldReturnNewLineToken_WhenPresent()
    {
        // Arrange.
        var input = "\n";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is NewLineToken);

        // Assert.
        tokens.Should().HaveCount(1);
        tokens.First().Value.ToString().Should().Be(input);
    }

    [Theory]
    [InlineData("<div class='btn'>", 2, "'")]
    [InlineData("<div class=\"btn\">", 2, "\"")]
    public void Lex_ShouldReturnStringDelimiterToken_WhenSingleOrDoubleQuoteIsUsed(
        string input,
        int expected,
        string delimiter
    )
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is StringDelimiterToken);

        // Assert.
        tokens.Should().HaveCount(expected);
        tokens.Select(x => x.Value.ToString()).Should().AllBe(delimiter);
    }

    [Theory]
    [InlineData("<div class=\\\\'", 1, "'")]
    [InlineData("<div class=\\\\\"", 1, "\"")]
    public void Lex_ShouldReturnStringDelimiterToken_WhenEscapeCharacterIsEscaped(
        string input,
        int expected,
        string delimiter
    )
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is StringDelimiterToken);

        // Assert.
        tokens.Should().HaveCount(expected);
        tokens.Select(x => x.Value.ToString()).Should().AllBe(delimiter);
    }

    [Fact]
    public void Lex_ShouldReturnTabToken_WhenPresent()
    {
        // Arrange.
        var input = "\t";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is TabToken);

        // Assert.
        tokens.Should().HaveCount(1);
        tokens.First().Value.ToString().Should().Be(input);
    }

    [Theory]
    [InlineData(" ", 1)]
    [InlineData(" h  ", 3)]
    public void Lex_ShouldReturnWhiteSpaceTokens_WhenCalled(string input, int expected)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is WhiteSpaceToken);

        // Assert.
        tokens.Should().HaveCount(expected);
        tokens.First().Value.ToString().Should().Be(" ");
    }

    [Fact]
    public void Lex_ShouldReturnWordToken_WhenDocType()
    {
        // Arrange.
        var input = "<!DOCTYPE html>";
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is WordToken);

        // Assert.
        tokens.Should().HaveCount(2);
        tokens.First().Value.ToString().Should().Be("!DOCTYPE");
    }

    [Theory]
    [InlineData("<div class='container'>'", 3)]
    [InlineData("<div>Hello my name is Bob!</div>", 7)]
    public void Lex_ShouldReturnWordTokens_WhenNotSpecialCharacters(string input, int expected)
    {
        // Arrange.
        var lexer = new Lexer(input);

        // Act.
        var tokens = lexer.Lex().Where(x => x is WordToken);

        // Assert.
        tokens.Should().HaveCount(expected);
    }
}
