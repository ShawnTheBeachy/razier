using FluentAssertions;
using Razier.Lexer.Tokens;
using Razier.Parser.Tokens;

namespace Razier.Parser.Tests.Unit;

// Tests.
public sealed partial class ParserTests
{
    [Theory]
    [InlineData("<div class='container'", 1)]
    [InlineData("<div class='container' disabled>", 2)]
    [InlineData("<div active disabled>", 2)]
    public void Parse_ShouldReturnAllAttributeTokens_WhenPresent(string input, int expected)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is AttributeToken);

        // Assert.
        parsed.Should().HaveCount(expected);
    }

    [Theory]
    [InlineData("<div class='container'")]
    [InlineData("< div class='container'>")]
    [InlineData("<  div class = 'container'  \n>")]
    public void Parse_ShouldReturnAttributeToken_WhenHasValue(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is AttributeToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be("class='container'");
    }

    [Theory]
    [InlineData("<div title='Say \"Hello\"!'", "'Say \"Hello\"!'")]
    [InlineData("<div title='Say \\'Hello\\'!'", "'Say \\'Hello\\'!'")]
    [InlineData("<div title=\"Say 'Hello'!\"", "\"Say 'Hello'!\"")]
    [InlineData("<div title=\"Say \\\"Hello\\\"!\"", "\"Say \\\"Hello\\\"!\"")]
    public void Parse_ShouldReturnAttributeToken_WhenHasValueWithEscapedCharacters(
        string input,
        string expected
    )
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is AttributeToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be($"title={expected}");
    }

    [Theory]
    [InlineData("<div class")]
    [InlineData("< div class>")]
    [InlineData("<  div class  \n>")]
    public void Parse_ShouldReturnAttributeToken_WhenNoValue(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is AttributeToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be("class");
    }

    [Theory]
    [InlineData("<div class")]
    [InlineData("< div class>")]
    [InlineData("<  div class>")]
    public void Parse_ShouldReturnBeginTagToken_WhenHasAttributes(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is BeginTagToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be("<div");
    }

    [Theory]
    [InlineData("<div>")]
    [InlineData("< div >")]
    [InlineData("<  div >")]
    public void Parse_ShouldReturnBeginTagToken_WhenNoAttributes(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is BeginTagToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be("<div>");
    }

    [Theory]
    [InlineData("@{ var x = 1; }")]
    [InlineData("@{var x = 1;}")]
    [InlineData("@{\n\tvar x = 1; \n\t}")]
    [InlineData("@{foreach(){}}")]
    public void Parse_ShouldReturnCodeBlockToken_WhenAtSymbolIsFollowedByBrace(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is CodeBlockToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be(input[2..^1]);
        ((CodeBlockToken)parsed.First()).Open.Should().Be("@{");
        ((CodeBlockToken)parsed.First()).Close.Should().Be("}");
    }

    [Theory]
    [InlineData("@code { var x = 1; }")]
    [InlineData("@code {var x = 1;}")]
    [InlineData("@code {\n\tvar x = 1; \n\t}")]
    [InlineData("@code {foreach(){}}")]
    public void Parse_ShouldReturnCodeBlockToken_WhenAtSymbolIsFollowedByCode(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is CodeBlockToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be(input[7..^1]);
        ((CodeBlockToken)parsed.First()).Open.Should().Be("@code {");
        ((CodeBlockToken)parsed.First()).Close.Should().Be("}");
    }

    [Theory]
    [InlineData("<!-- Hey! -->")]
    [InlineData("<!--Hey-->")]
    [InlineData("<!---->")]
    [InlineData("<!--\nHey\n-->")]
    public void Parse_ShouldReturnCommentToken_WhenPresent(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is CommentToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be(input);
    }

    [Theory]
    [InlineData("<div disabled>Content!</div>", 1)]
    [InlineData("<div>Content!<div></div>More!</div>", 2)]
    [InlineData("<div>Content!\r\nMore!</div>", 1)]
    public void Parse_ShouldReturnContentTokens_WhenTextIsNotInTag(string input, int expected)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is ContentToken);

        // Assert.
        parsed.Should().HaveCount(expected);
    }

    [Theory]
    [InlineData("<div></div>", 1)]
    [InlineData("<div>Content</div>", 1)]
    [InlineData("<div><div></div></div>", 2)]
    public void Parse_ShouldReturnHardCloseTagTokens_WhenPresent(string input, int expected)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is HardCloseTagToken);

        // Assert.
        parsed.Should().HaveCount(expected);
        parsed.Select(x => x.Value).Should().AllBe("</div>");
    }

    [Theory]
    [InlineData("<input active>")]
    [InlineData("<area active>")]
    [InlineData("<base active>")]
    [InlineData("<br active>")]
    [InlineData("<col active>")]
    [InlineData("<command active>")]
    [InlineData("<embed active>")]
    [InlineData("<hr active>")]
    [InlineData("<img active>")]
    [InlineData("<input active>")]
    [InlineData("<keygen active>")]
    [InlineData("<link active>")]
    [InlineData("<meta active>")]
    [InlineData("<param active>")]
    [InlineData("<source active>")]
    [InlineData("<track active>")]
    [InlineData("<wbr active>")]
    public void Parse_ShouldReturnHardCloseTagToken_WhenVoidElement(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is HardCloseTagToken);

        // Assert.
        parsed.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("<div active>", false)]
    [InlineData("<div class='container'>", false)]
    [InlineData("<input active>", true)]
    [InlineData("<area active>", true)]
    [InlineData("<base active>", true)]
    [InlineData("<br active>", true)]
    [InlineData("<col active>", true)]
    [InlineData("<command active>", true)]
    [InlineData("<embed active>", true)]
    [InlineData("<hr active>", true)]
    [InlineData("<img active>", true)]
    [InlineData("<input active>", true)]
    [InlineData("<keygen active>", true)]
    [InlineData("<link active>", true)]
    [InlineData("<meta active>", true)]
    [InlineData("<param active>", true)]
    [InlineData("<source active>", true)]
    [InlineData("<track active>", true)]
    [InlineData("<wbr active>", true)]
    public void Parse_ShouldReturnSoftCloseTagToken_WhenElementIsNotVoidElement(
        string input,
        bool isVoid
    )
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is SoftCloseTagToken);

        // Assert.

        if (isVoid)
            parsed.Should().BeEmpty();
        else
        {
            parsed.Should().HaveCount(1);
            parsed.First().Value.Should().Be(">");
        }
    }
}

// Private methods.
public sealed partial class ParserTests
{
    private static T LexToken<T>(string value)
        where T : IToken, new() => new() { Value = value.AsMemory() };
}
