using Razier.Lexing;
using Razier.Parsing.Tokens;

namespace Razier.Parsing;

// Public methods.
public static partial class Parser
{
    public static IList<IToken> Parse(string source)
    {
        var lexemes = Lexer.Lex(source);
        return Parse(lexemes.ToArray(), source);
    }

    public static IList<IToken> Parse(Lexeme[] lexemes, string source)
    {
        var tokens = new List<IToken>();
        var index = 0;

        while (index < lexemes.Length)
        {
            var token = ConsumeToken(lexemes, ref index, source);

            if (token is not IgnoreToken)
                tokens.Add(token);
            else
                index++;
        }

        return tokens;
    }
}

// Private methods.
public static partial class Parser
{
    private static AttributeToken ConsumeAttribute(Lexeme[] lexemes, ref int index)
    {
        var token = new AttributeToken { KeyOffset = lexemes[index].Offset };

        do
        {
            index++;
        } while (
            lexemes[index].Type != LexemeType.Equals
            && !lexemes[index].Type.IsAnyWhiteSpaceType()
            && lexemes[index].Type != LexemeType.ForwardSlash
            && lexemes[index].Type != LexemeType.RightChevron
        );

        token = token with { KeyLength = lexemes[index].Offset - token.KeyOffset };

        while (lexemes[index].Type.IsAnyWhiteSpaceType())
            index++;

        if (lexemes[index].Type != LexemeType.Equals)
            return token;

        do
        {
            index++;
        } while (IsAnyWhiteSpaceType(lexemes[index].Type));

        LexemeType? delimiter = null;

        if (
            lexemes[index].Type == LexemeType.DoubleQuote
            || lexemes[index].Type == LexemeType.SingleQuote
        )
        {
            delimiter = lexemes[index].Type;
            token = token with { ValueOffset = lexemes[++index].Offset };
        }
        else
            token = token with { ValueOffset = lexemes[index].Offset };

        var razorExpressionNestLevel = 0;
        var isInRazorExpressionString = false;

        while (
            razorExpressionNestLevel > 0
            || (
                delimiter is null
                && lexemes[index].Type != LexemeType.ForwardSlash
                && lexemes[index].Type != LexemeType.RightChevron
                && !IsAnyWhiteSpaceType(lexemes[index].Type)
            )
            || (
                delimiter is not null
                && (lexemes[index].Type != delimiter || IsEscaped(lexemes, index))
            )
        )
        {
            if (
                !isInRazorExpressionString
                && razorExpressionNestLevel > 0
                && lexemes[index].Type == LexemeType.LeftParenthesis
            )
                razorExpressionNestLevel++;
            else if (
                lexemes[index].Type == LexemeType.At
                && lexemes[index + 1].Type == LexemeType.LeftParenthesis
            )
            {
                razorExpressionNestLevel++;
                index++;
            }
            else if (
                !isInRazorExpressionString && lexemes[index].Type == LexemeType.RightParenthesis
            )
                razorExpressionNestLevel--;
            else if (
                razorExpressionNestLevel > 0
                && lexemes[index].Type == LexemeType.DoubleQuote
                && !IsEscaped(lexemes, index)
            )
                isInRazorExpressionString = !isInRazorExpressionString;

            index++;
        }

        token = token with { ValueLength = lexemes[index].Offset - token.ValueOffset };

        if (delimiter is not null)
            index++;

        return token;
    }

    private static CodeBlockToken ConsumeCodeBlock(
        Lexeme[] lexemes,
        ref int index,
        ReadOnlySpan<char> source
    )
    {
        var token = new CodeBlockToken { OpenOffset = lexemes[index].Offset };

        while (lexemes[index].Type != LexemeType.LeftBrace)
            index++;

        token = token with { OpenLength = lexemes[++index].Offset - token.OpenOffset };
        token = token with { Children = ConsumeCodeBlockContent(lexemes, ref index, source) };
        token = token with { CloseOffset = lexemes[index].Offset };
        index++;
        return token;
    }

    private static List<IToken> ConsumeCodeBlockContent(
        Lexeme[] lexemes,
        ref int index,
        ReadOnlySpan<char> source
    )
    {
        var tokens = new List<IToken>();

        while (index < lexemes.Length && lexemes[index].Type != LexemeType.RightBrace)
        {
            if (IsAnyWhiteSpaceType(lexemes[index].Type))
            {
                index++;
                continue;
            }

            tokens.Add(
                lexemes[index].Type switch
                {
                    LexemeType.LeftChevron => ConsumeElement(lexemes, ref index, source),
                    _ => ConsumeCSharp(lexemes, ref index)
                }
            );
        }

        return tokens;
    }

    private static ControlStructureToken ConsumeControlStructure(
        Lexeme[] lexemes,
        ref int index,
        ReadOnlySpan<char> source
    )
    {
        var token = new ControlStructureToken { OpenOffset = lexemes[index].Offset };

        while (
            lexemes[index].Type != LexemeType.LeftParenthesis
            && lexemes[index].Type != LexemeType.LeftBrace
        )
            index++;

        token = token with { OpenLength = lexemes[index].Offset - token.OpenOffset };

        if (lexemes[index].Type == LexemeType.LeftParenthesis)
        {
            token = token with { ExpressionOffset = lexemes[index].Offset };
            var nestLevel = 0;
            var isInString = false;

            do
            {
                index++;

                if (lexemes[index].Type == LexemeType.DoubleQuote && !IsEscaped(lexemes, index))
                    isInString = !isInString;
                else if (!isInString && lexemes[index].Type == LexemeType.LeftParenthesis)
                    nestLevel++;
                else if (!isInString && lexemes[index].Type == LexemeType.RightParenthesis)
                    nestLevel--;
            } while (
                !isInString && (nestLevel > 0 || lexemes[index].Type != LexemeType.RightParenthesis)
            );

            token = token with
            {
                ExpressionLength = lexemes[++index].Offset - token.ExpressionOffset
            };
        }

        while (lexemes[index].Type != LexemeType.LeftBrace)
            index++;

        index++;
        token = token with { Children = ConsumeCodeBlockContent(lexemes, ref index, source) };
        index++;

        if (MemoryExtensions.Equals(token.Open(source)[..3], "@do", StringComparison.Ordinal))
        {
            while (lexemes[index].Type != LexemeType.Text)
                index++;

            token = token with { ExpressionOffset = lexemes[index].Offset };
            var isInString = false;

            while (isInString || lexemes[index].Type != LexemeType.Semicolon)
            {
                if (lexemes[index].Type == LexemeType.DoubleQuote && !IsEscaped(lexemes, index))
                    isInString = !isInString;

                index++;
            }

            token = token with
            {
                ExpressionLength = lexemes[++index].Offset - token.ExpressionOffset
            };
        }

        return token;
    }

    private static CSharpToken ConsumeCSharp(Lexeme[] lexemes, ref int index)
    {
        var isInString = false;
        var isInStatement = true;
        var expressionNestLevel = 0;
        var nestLevel = 0;
        var token = new CSharpToken { Offset = lexemes[index].Offset };

        while (
            index < lexemes.Length
            && (lexemes[index].Type != LexemeType.RightBrace || nestLevel > 0)
            && (
                isInString
                || isInStatement
                || expressionNestLevel > 0
                || lexemes[index].Type != LexemeType.LeftChevron
            )
        )
        {
            if (lexemes[index].Type == LexemeType.DoubleQuote && !IsEscaped(lexemes, index))
                isInString = !isInString;
            else if (isInString || IsAnyWhiteSpaceType(lexemes[index].Type))
            {
                index++;
                continue;
            }

            if (lexemes[index].Type == LexemeType.LeftParenthesis)
                expressionNestLevel++;
            else if (lexemes[index].Type == LexemeType.RightParenthesis)
                expressionNestLevel--;
            else if (lexemes[index].Type == LexemeType.LeftBrace)
                nestLevel++;
            else if (lexemes[index].Type == LexemeType.RightBrace)
            {
                isInStatement = false;
                nestLevel--;
            }
            else if (lexemes[index].Type == LexemeType.Semicolon)
                isInStatement = false;
            else
                isInStatement = true;

            index++;
        }

        return token with
        {
            Length = lexemes[index].Offset - token.Offset
        };
    }

    private static ElementToken ConsumeElement(
        Lexeme[] lexemes,
        ref int index,
        ReadOnlySpan<char> source
    )
    {
        var token = new ElementToken
        {
            NameLength = lexemes[++index].Length,
            NameOffset = lexemes[index++].Offset
        };

        while (index < lexemes.Length && lexemes[index].Type != LexemeType.RightChevron)
        {
            if (
                lexemes[index].Type == LexemeType.ForwardSlash
                && lexemes[index + 1].Type == LexemeType.RightChevron
            )
                return token;

            if (IsAnyWhiteSpaceType(lexemes[index].Type))
                index++;
            else
                token.Attributes.Add(ConsumeAttribute(lexemes, ref index));
        }

        if (IsVoidElement(token.Name(source).ToString()))
            return token;

        index++;

        while (index < lexemes.Length)
        {
            if (
                lexemes[index].Type == LexemeType.LeftChevron
                && lexemes[index + 1].Type == LexemeType.ForwardSlash
            )
            {
                do
                {
                    index++;
                } while (lexemes[index].Type != LexemeType.RightChevron);
                index++;
                return token;
            }

            var child = ConsumeToken(lexemes, ref index, source);

            if (child is not IgnoreToken)
                token.Children.Add(child);
            else
                index++;
        }

        return token;
    }

    private static ExplicitRazorExpressionToken ConsumeExplicitRazorExpression(
        Lexeme[] lexemes,
        ref int index
    )
    {
        var isInString = false;
        var nestLevel = 0;
        var token = new ExplicitRazorExpressionToken { OpenOffset = lexemes[index].Offset };
        index += 2;

        for (; index < lexemes.Length; index++)
        {
            if (lexemes[index].Type == LexemeType.DoubleQuote && !IsEscaped(lexemes, index))
                isInString = !isInString;
            else if (lexemes[index].Type == LexemeType.LeftParenthesis && !isInString)
                nestLevel++;
            else if (lexemes[index].Type == LexemeType.RightParenthesis && !isInString)
            {
                nestLevel--;

                if (nestLevel < 0)
                    break;
            }
        }

        return token with
        {
            CloseOffset = lexemes[index++].Offset,
        };
    }

    private static CommentToken ConsumeHtmlComment(Lexeme[] lexemes, ref int index)
    {
        var token = new CommentToken { OpenLength = 4, OpenOffset = lexemes[index].Offset };
        index += 4;
        token = token with { ContentOffset = lexemes[index].Offset };

        while (
            lexemes[index].Type != LexemeType.Dash
            || lexemes[index + 1].Type != LexemeType.Dash
            || lexemes[index + 2].Type != LexemeType.RightChevron
        )
        {
            index++;
        }

        token = token with
        {
            CloseLength = 3,
            CloseOffset = lexemes[index].Offset,
            ContentLength = lexemes[index].Offset - token.ContentOffset
        };
        index += 3;
        return token;
    }

    private static ImplicitRazorExpressionToken ConsumeImplicitRazorExpression(
        Lexeme[] lexemes,
        ref int index,
        ReadOnlySpan<char> source
    )
    {
        var token = new ImplicitRazorExpressionToken { Offset = lexemes[index++].Offset };

        if (
            MemoryExtensions.Equals("await", lexemes[index].Value(source), StringComparison.Ordinal)
        )
        {
            do
            {
                index++;
            } while (
                lexemes[index].Type == LexemeType.WhiteSpace
                || lexemes[index].Type == LexemeType.Tab
            );
        }

        while (
            !IsAnyWhiteSpaceType(lexemes[index].Type)
            && lexemes[index].Type != LexemeType.LeftChevron
            && lexemes[index].Type != LexemeType.EndOfFile
        )
            index++;

        return token with
        {
            Length = lexemes[index].Offset - token.Offset
        };
    }

    private static LineLevelDirectiveToken ConsumeLineLevelDirective(
        Lexeme[] lexemes,
        ref int index
    )
    {
        var token = new LineLevelDirectiveToken { DirectiveOffset = lexemes[index].Offset };
        index += 2;
        token = token with { DirectiveLength = lexemes[index].Offset - token.DirectiveOffset };

        while (
            lexemes[index].Type == LexemeType.WhiteSpace || lexemes[index].Type == LexemeType.Tab
        )
            index++;

        token = token with { LineOffset = lexemes[index].Offset };

        while (
            lexemes[index].Type != LexemeType.NewLine
            && lexemes[index].Type != LexemeType.CarriageReturn
            && lexemes[index].Type != LexemeType.EndOfFile
        )
            index++;

        return token with
        {
            LineLength = lexemes[index].Offset - token.LineOffset
        };
    }

    private static NewLineToken ConsumeNewLine(Lexeme[] lexemes, ref int index) =>
        new() { Offset = lexemes[index++].Offset };

    private static CommentToken ConsumeRazorComment(Lexeme[] lexemes, ref int index)
    {
        var token = new CommentToken { OpenLength = 2, OpenOffset = lexemes[index++].Offset };
        token = token with { ContentOffset = lexemes[++index].Offset };

        while (
            lexemes[index].Type != LexemeType.Asterisk || lexemes[index + 1].Type != LexemeType.At
        )
        {
            index++;
        }

        token = token with
        {
            CloseLength = 2,
            CloseOffset = lexemes[index].Offset,
            ContentLength = lexemes[index].Offset - token.ContentOffset
        };
        index += 2;
        return token;
    }

    private static TextToken ConsumeText(Lexeme[] lexemes, ref int index, ReadOnlySpan<char> source)
    {
        var token = new TextToken { Offset = lexemes[index].Offset };
        TokenType tokenType;

        do
        {
            index++;
        } while (
            lexemes[index].Type != LexemeType.EndOfFile
            && (
                (tokenType = GetTokenType(lexemes, index, source)) == TokenType.Text
                || tokenType == TokenType.Ignore
            )
        );

        return token with
        {
            Length = lexemes[index].Offset - token.Offset
        };
    }

    private static IToken ConsumeToken(
        Lexeme[] lexemes,
        ref int index,
        ReadOnlySpan<char> source
    ) =>
        GetTokenType(lexemes, index, source) switch
        {
            TokenType.ControlStructure => ConsumeControlStructure(lexemes, ref index, source),
            TokenType.ExplicitRazorExpression => ConsumeExplicitRazorExpression(lexemes, ref index),
            TokenType.CodeBlock => ConsumeCodeBlock(lexemes, ref index, source),
            TokenType.LineLevelDirective => ConsumeLineLevelDirective(lexemes, ref index),
            TokenType.ImplicitRazorExpression
                => ConsumeImplicitRazorExpression(lexemes, ref index, source),
            TokenType.Element => ConsumeElement(lexemes, ref index, source),
            TokenType.HtmlComment => ConsumeHtmlComment(lexemes, ref index),
            TokenType.RazorComment => ConsumeRazorComment(lexemes, ref index),
            TokenType.Text => ConsumeText(lexemes, ref index, source),
            TokenType.NewLine => ConsumeNewLine(lexemes, ref index),
            TokenType.CarriageReturn
                when index == lexemes.Length - 1
                    || GetTokenType(lexemes, index + 1, source) != TokenType.NewLine
                => ConsumeNewLine(lexemes, ref index),
            _ => new IgnoreToken()
        };

    private static TokenType GetTokenType(Lexeme[] lexemes, int index, ReadOnlySpan<char> source)
    {
        Lexeme? l;

        return lexemes[index].Type switch
        {
            LexemeType.CarriageReturn => TokenType.CarriageReturn,
            LexemeType.NewLine => TokenType.NewLine,
            LexemeType.Text
                when _followingControlStructureKeywords.Contains(
                    lexemes[index].Value(source).ToString()
                )
                => TokenType.ControlStructure,
            LexemeType.Text => TokenType.Text,
            LexemeType.At when lexemes[index + 1].Type == LexemeType.Asterisk
                => TokenType.RazorComment,
            LexemeType.At
                when lexemes[index + 1].Type == LexemeType.LeftParenthesis
                    && (index == 0 || lexemes[index - 1].Type != LexemeType.At)
                => TokenType.ExplicitRazorExpression,
            LexemeType.At
                when lexemes[index + 1].Type == LexemeType.LeftBrace
                    || (
                        (l = lexemes.Next(LexemeType.Text, index)).HasValue
                        && IsCodeBlockDirective(l.Value, source)
                        && (lexemes.Next(LexemeType.LeftBrace, index + 2)).HasValue
                    )
                => TokenType.CodeBlock,
            LexemeType.At
                when lexemes[index + 1].Type == LexemeType.Text
                    && IsControlStructureKeyword(lexemes, index + 1, source)
                => TokenType.ControlStructure,
            LexemeType.At
                when IsLineLevelDirective(lexemes[index + 1], source) && IsLineStart(lexemes, index)
                => TokenType.LineLevelDirective,
            LexemeType.At => TokenType.ImplicitRazorExpression,
            LexemeType.LeftChevron
                when lexemes[index + 1].Type == LexemeType.Exclamation
                    && lexemes[index + 2].Type == LexemeType.Dash
                    && lexemes[index + 3].Type == LexemeType.Dash
                => TokenType.HtmlComment,
            LexemeType.LeftChevron => TokenType.Element,
            _ => TokenType.Ignore
        };
    }

    private static bool IsCodeBlockDirective(Lexeme lexeme, ReadOnlySpan<char> source)
    {
        var value = lexeme.Value(source);
        return MemoryExtensions.Equals("code", value, StringComparison.Ordinal)
            || MemoryExtensions.Equals("functions", value, StringComparison.Ordinal);
    }

    private static bool IsControlStructureKeyword(
        Lexeme[] lexemes,
        int index,
        ReadOnlySpan<char> source
    )
    {
        var value = lexemes[index].Value(source).ToString();

        if (!_controlStructureKeywords.Contains(value))
            return false;

        if (value == "using")
        {
            do
            {
                index++;
            } while (IsAnyWhiteSpaceType(lexemes[index].Type));

            return lexemes[index].Type == LexemeType.LeftParenthesis;
        }

        return true;
    }

    private static bool IsEscaped(Lexeme[] lexemes, int index)
    {
        if (index < 1)
            return false;

        if (lexemes[index - 1].Type != LexemeType.BackSlash)
            return false;

        return !IsEscaped(lexemes, index - 1);
    }

    private static bool IsLineLevelDirective(Lexeme lexeme, ReadOnlySpan<char> source) =>
        _lineLevelDirectives.Contains(lexeme.Value(source).ToString());

    private static bool IsLineStart(Lexeme[] lexemes, int index)
    {
        while (index > 0)
        {
            index--;

            if (
                lexemes[index].Type == LexemeType.NewLine
                || lexemes[index].Type == LexemeType.CarriageReturn
            )
                return true;

            if (
                lexemes[index].Type == LexemeType.Tab
                || lexemes[index].Type == LexemeType.WhiteSpace
            )
                continue;

            return false;
        }

        return true;
    }

    private static bool IsAnyWhiteSpaceType(this LexemeType type) =>
        type switch
        {
            LexemeType.WhiteSpace
            or LexemeType.Tab
            or LexemeType.NewLine
            or LexemeType.CarriageReturn
                => true,
            _ => false
        };

    private static bool IsVoidElement(string name) => _voidTypes.Contains(name);

    private static Lexeme? Next(
        this Lexeme[] lexemes,
        LexemeType type,
        int index,
        int skip = 0,
        bool ignoreWhiteSpace = true
    )
    {
        var skipped = 0;

        for (var i = index; i < lexemes.Length; i++)
        {
            var skipWhiteSpace =
                ignoreWhiteSpace
                && lexemes[i].Type switch
                {
                    LexemeType.WhiteSpace
                    or LexemeType.Tab
                    or LexemeType.CarriageReturn
                    or LexemeType.NewLine
                        => true,
                    _ => false
                };

            if (skipWhiteSpace)
                continue;

            if (lexemes[i].Type == type)
            {
                if (skipped == skip)
                    return lexemes[i];
                else
                    skipped++;
            }
        }

        return null;
    }
}

// Private fields.
public static partial class Parser
{
    private static readonly HashSet<string> _controlStructureKeywords =
        new()
        {
            "if",
            "else",
            "switch",
            "for",
            "foreach",
            "while",
            "do",
            "using",
            "try",
            "catch",
            "finally",
            "lock"
        };

    private static readonly HashSet<string> _followingControlStructureKeywords =
        new() { "else", "catch", "finally" };

    private static readonly HashSet<string> _lineLevelDirectives =
        new()
        {
            "implements",
            "inherits",
            "using",
            "model",
            "inject",
            "layout",
            "namespace",
            "page",
            "preservewhitespace",
            "section",
            "typeparam"
        };

    private static readonly HashSet<string> _voidTypes =
        new()
        {
            "area",
            "base",
            "br",
            "col",
            "command",
            "embed",
            "hr",
            "img",
            "input",
            "keygen",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr",
            "!DOCTYPE"
        };
}
