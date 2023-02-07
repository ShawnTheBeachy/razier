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
    private static AttributeToken ConsumeAttribute(Lexeme[] lexemes, ref int index, string source)
    {
        var token = new AttributeToken { KeyOffset = lexemes[index].Offset };

        do
        {
            index++;
        } while (
            lexemes[index].Type != LexemeType.Equals && !lexemes[index].Type.IsAnyWhiteSpaceType()
        );

        token = token with { KeyLength = lexemes[index].Offset - token.KeyOffset };

        while (lexemes[index].Type.IsAnyWhiteSpaceType())
        {
            index++;
        }

        if (lexemes[index].Type != LexemeType.Equals)
            return token;

        do
        {
            index++;
        } while (
            lexemes[index].Type != LexemeType.DoubleQuote
            && lexemes[index].Type != LexemeType.SingleQuote
        );

        token = token with { ValueOffset = lexemes[index + 1].Offset };
        var delimiter = lexemes[index++].Type;

        while (lexemes[index].Type != delimiter || IsEscaped(lexemes, index))
        {
            index++;
        }

        return token with
        {
            ValueLength = lexemes[index++].Offset - token.ValueOffset
        };
    }

    private static CodeBlockToken ConsumeCodeBlock(Lexeme[] lexemes, ref int index, string source)
    {
        var token = new CodeBlockToken { OpenOffset = lexemes[index].Offset };

        while (lexemes[index].Type != LexemeType.LeftBrace)
            index++;

        token = token with { OpenLength = lexemes[++index].Offset - token.OpenOffset };
        token = token with { Children = ConsumeCodeBlockContent(lexemes, ref index, source) };
        token = token with { CloseOffset = index };
        index++;
        return token;
    }

    private static List<IToken> ConsumeCodeBlockContent(
        Lexeme[] lexemes,
        ref int index,
        string source
    )
    {
        var tokens = new List<IToken>();

        while (index < lexemes.Length && lexemes[index].Type != LexemeType.RightBrace)
        {
            tokens.Add(
                lexemes[index].Type switch
                {
                    LexemeType.LeftChevron => ConsumeElement(lexemes, ref index, source),
                    _ => ConsumeCSharp(lexemes, ref index, source)
                }
            );
        }

        return tokens;
    }

    private static ControlStructureToken ConsumeControlStructure(
        Lexeme[] lexemes,
        ref int index,
        string source
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

    private static CSharpToken ConsumeCSharp(Lexeme[] lexemes, ref int index, string source)
    {
        var isInString = false;
        var isInStatement = true;
        var nestLevel = 0;
        var token = new CSharpToken { Offset = lexemes[index].Offset };

        while (
            index < lexemes.Length
            && (lexemes[index].Type != LexemeType.RightBrace || nestLevel > 0)
            && (isInStatement || isInString || lexemes[index].Type != LexemeType.LeftChevron)
        )
        {
            if (lexemes[index].Type == LexemeType.LeftBrace && !isInString)
            {
                isInStatement = false;
                nestLevel++;
            }
            else if (lexemes[index].Type == LexemeType.RightBrace && !isInString)
            {
                isInStatement = false;
                nestLevel--;
            }
            else if (lexemes[index].Type == LexemeType.Semicolon && !isInString)
                isInStatement = false;
            else if (lexemes[index].Type == LexemeType.DoubleQuote && !IsEscaped(lexemes, index))
                isInString = !isInString;

            index++;
        }

        return token with
        {
            Length = lexemes[index].Offset - token.Offset
        };
    }

    private static ElementToken ConsumeElement(Lexeme[] lexemes, ref int index, string source)
    {
        var token = new ElementToken
        {
            NameLength = lexemes[++index].Length,
            NameOffset = lexemes[index++].Offset
        };

        while (index < lexemes.Length)
        {
            if (lexemes[index].Type == LexemeType.RightChevron)
            {
                break;
            }

            if (
                lexemes[index].Type == LexemeType.ForwardSlash
                && lexemes[index + 1].Type == LexemeType.RightChevron
            )
                return token;

            if (lexemes[index].Type == LexemeType.Text)
                token.Attributes.Add(ConsumeAttribute(lexemes, ref index, source));
            else
                index++;
        }

        if (IsVoidElement(token.Name(source).ToString()))
            return token;

        index++;

        for (; index < lexemes.Length; index++)
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

    private static ImplicitRazorExpressionToken ConsumeImplicitRazorExpression(
        Lexeme[] lexemes,
        ref int index,
        string source
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

        return token = token with { Length = lexemes[index].Offset - token.Offset };
    }

    private static LineLevelDirectiveToken ConsumeLineLevelDirective(
        Lexeme[] lexemes,
        ref int index,
        string source
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

        return token = token with { LineLength = lexemes[index].Offset - token.LineOffset };
    }

    private static IToken ConsumeToken(Lexeme[] lexemes, ref int index, string source)
    {
        Lexeme? l;

        IToken token = lexemes[index].Type switch
        {
            LexemeType.Text
                when _followingControlStructureKeywords.Contains(
                    lexemes[index].Value(source).ToString()
                )
                => ConsumeControlStructure(lexemes, ref index, source),
            LexemeType.At
                when lexemes[index + 1].Type == LexemeType.LeftParenthesis
                    && (index == 0 || lexemes[index - 1].Type != LexemeType.At)
                => ConsumeExplicitRazorExpression(lexemes, ref index),
            LexemeType.At
                when lexemes[index + 1].Type == LexemeType.LeftBrace
                    || (
                        (l = lexemes.Next(LexemeType.Text, index)).HasValue
                        && IsCodeBlockDirective(l.Value, source)
                        && (l = lexemes.Next(LexemeType.LeftBrace, index + 2)).HasValue
                    )
                => ConsumeCodeBlock(lexemes, ref index, source),
            LexemeType.At
                when lexemes[index + 1].Type == LexemeType.Text
                    && IsControlStructureKeyword(lexemes, index + 1, source)
                => ConsumeControlStructure(lexemes, ref index, source),
            LexemeType.At
                when IsLineLevelDirective(lexemes[index + 1], source) && IsLineStart(lexemes, index)
                => ConsumeLineLevelDirective(lexemes, ref index, source),
            LexemeType.At => ConsumeImplicitRazorExpression(lexemes, ref index, source),
            LexemeType.LeftChevron => ConsumeElement(lexemes, ref index, source),
            _ => new IgnoreToken()
        };
        return token;
    }

    private static bool IsCodeBlockDirective(Lexeme lexeme, string source)
    {
        var value = lexeme.Value(source);
        return MemoryExtensions.Equals("code", value, StringComparison.Ordinal)
            || MemoryExtensions.Equals("functions", value, StringComparison.Ordinal);
    }

    private static bool IsControlStructureKeyword(Lexeme[] lexemes, int index, string source)
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

    private static bool IsLineLevelDirective(Lexeme lexeme, string source) =>
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
