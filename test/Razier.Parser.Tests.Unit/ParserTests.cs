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
    [InlineData("<div>")]
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
        parsed.First().Value.Should().Be("<div");
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
    [InlineData("<input />", 1)]
    public void Parse_ShouldReturnHardCloseTagTokens_WhenPresent(string input, int expected)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is HardCloseTagToken);

        // Assert.
        parsed.Should().HaveCount(expected);
    }

    [Theory]
    [InlineData("<input>")]
    [InlineData("<area>")]
    [InlineData("<base>")]
    [InlineData("<br>")]
    [InlineData("<col>")]
    [InlineData("<command>")]
    [InlineData("<embed>")]
    [InlineData("<hr>")]
    [InlineData("<img>")]
    [InlineData("<keygen>")]
    [InlineData("<link>")]
    [InlineData("<meta>")]
    [InlineData("<param>")]
    [InlineData("<source>")]
    [InlineData("<track>")]
    [InlineData("<wbr>")]
    [InlineData("<!DOCTYPE html>")]
    [InlineData("<input/>")]
    [InlineData("<area/>")]
    [InlineData("<base/>")]
    [InlineData("<br/>")]
    [InlineData("<col/>")]
    [InlineData("<command/>")]
    [InlineData("<embed/>")]
    [InlineData("<hr/>")]
    [InlineData("<img/>")]
    [InlineData("<keygen/>")]
    [InlineData("<link/>")]
    [InlineData("<meta/>")]
    [InlineData("<param/>")]
    [InlineData("<source/>")]
    [InlineData("<track/>")]
    [InlineData("<wbr/>")]
    [InlineData("<!DOCTYPE html/>")]
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
    [InlineData("@using Humanizer;")]
    [InlineData("@foreach (var i in items)")]
    public void Parse_ShouldReturnInlineCodeBlockToken_WhenBeginInlineCodeBlockEncountered(
        string input
    )
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray());

        // Act.
        var parsed = parser.Parse().Where(x => x is InlineCodeBlockToken);

        // Assert.
        parsed.Should().HaveCount(1);
        parsed.First().Value.Should().Be(input);
    }

    [Theory]
    [InlineData("<div active>", false)]
    [InlineData("<div>", false)]
    [InlineData("<div class='container'>", false)]
    [InlineData("<input>", true)]
    [InlineData("<area>", true)]
    [InlineData("<base>", true)]
    [InlineData("<br>", true)]
    [InlineData("<col>", true)]
    [InlineData("<command>", true)]
    [InlineData("<embed>", true)]
    [InlineData("<hr>", true)]
    [InlineData("<img>", true)]
    [InlineData("<keygen>", true)]
    [InlineData("<link>", true)]
    [InlineData("<meta>", true)]
    [InlineData("<param>", true)]
    [InlineData("<source>", true)]
    [InlineData("<track>", true)]
    [InlineData("<wbr>", true)]
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

    [Fact]
    public void Parse_ShouldTrimNewLinesTabsAndSpaces_FromContent()
    {
        // Arrange.
        var input = "<div>\r\nContent\r\n\t     </div>";
        var lexer = new Lexer.Lexer(input);
        var parser = new Parser(lexer.Lex().ToArray(), false);

        // Act.
        var parsed = parser.Parse().Where(x => x is ContentToken);

        // Assert.
        parsed.First().Value.Should().NotEndWith("\r\n\t     ");
    }

    [Theory]
    [InlineData("<div></div>")]
    [InlineData("<div><div></div></div>")]
    [InlineData("<div><input></div>")]
    [InlineData("<div disabled class='container' />")]
    public void Validation_ShouldNotThrow_WhenAllTagsAreClosed(string input)
    {
        // Arrange.
        var tokens = new Lexer.Lexer(input).Lex();
        var parser = new Parser(tokens.ToArray(), true);

        // Act.
        var parse = () => parser.Parse();

        // Assert.
        parse.Should().NotThrow();
    }
}

// Private methods.
public sealed partial class ParserTests
{
    private static T LexToken<T>(string value)
        where T : IToken, new() => new() { Value = value.AsMemory() };
}
