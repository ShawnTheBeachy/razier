using System.Text;
using Razier.Parsing;
using Razier.Parsing.Tokens;

namespace Razier.Formatting;

// Public methods.
public static partial class Formatter
{
    public static string Format(string source, string tab = "  ")
    {
        var tokens = Parser.Parse(source);
        return Format(tokens, source, tab);
    }

    public static string Format(IList<IToken> tokens, string source, string tab = "  ")
    {
        var output = new StringBuilder();
        var newLineCount = 0;

        foreach (var token in tokens)
        {
            var isNewLine = token is NewLineToken;

            if (!isNewLine)
            {
                for (var i = 0; i < Math.Min(2, newLineCount); i++)
                    output.AppendLine();

                FormatToken(token, source, output, "", tab);
                newLineCount = 0;
            }
            else
                newLineCount++;
        }

        output.AppendLine();
        return output.ToString();
    }
}

// Private methods.
public static partial class Formatter
{
    private static void AppendIndented(
        this StringBuilder output,
        ReadOnlySpan<char> value,
        ReadOnlySpan<char> indent
    )
    {
        output.Append(indent);
        output.Append(value);
    }

    private static void AppendIndented(
        this StringBuilder output,
        char value,
        ReadOnlySpan<char> indent
    )
    {
        output.Append(indent);
        output.Append(value);
    }

    private static void FormatAttribute(
        AttributeToken attribute,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent
    )
    {
        var key = attribute.Key(source);
        output.AppendIndented(key, indent);

        if (attribute.ValueLength > 0)
        {
            output.Append("=\"");
            output.Append(attribute.Value(source));
            output.Append('"');
        }
    }

    private static void FormatCodeBlock(
        CodeBlockToken codeBlock,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent,
        string tab
    )
    {
        var open = codeBlock.Open(source).Trim();

        if (MemoryExtensions.Equals("@{", open[..2], StringComparison.Ordinal))
            output.AppendIndented("@{", indent);
        else
        {
            output.AppendIndented(open[..^1].Trim(), indent);
            output.AppendLine();
            output.AppendIndented("{", indent);
        }

        output.AppendLine();
        FormatCodeBlockContent(codeBlock, source, output, indent, tab);
        output.AppendLine();
        output.AppendIndented(codeBlock.Close(source), indent);
    }

    private static void FormatCodeBlockContent(
        CodeBlockToken codeBlock,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent,
        string tab
    )
    {
        const string htmlMarker = $"//{nameof(ElementToken)}-";

        var code = new StringBuilder();
        code.Append("namespace A;");
        code.AppendLine();
        var elements = new Dictionary<string, ElementToken>();

        foreach (var child in codeBlock.Children)
        {
            if (child is CSharpToken cSharp)
                code.Append(cSharp.Code(source));
            else if (child is ElementToken element)
            {
                code.AppendLine();
                code.AppendLine($"{htmlMarker}{elements.Count}");
                elements[elements.Count.ToString()] = element;
            }
        }

        var formatted = CSharpier.CodeFormatter.Format(
            code.ToString(),
            new CSharpier.CodeFormatterOptions { Width = 80 - indent.Length }
        )[12..]
            .AsSpan()
            .Trim('\r', '\n');
        bool? wasLastLineHtml = null;
        char? lastChar = null;
        var consecutiveNewLines = 0;

        for (var i = 0; i < formatted.Length; i++)
        {
            if (
                formatted[i] == '/'
                && formatted.Length - i >= htmlMarker.Length
                && MemoryExtensions.Equals(
                    htmlMarker,
                    formatted[i..(i + htmlMarker.Length)],
                    StringComparison.Ordinal
                )
            )
            {
                i += htmlMarker.Length;
                var markerStart = i;

                while (i < formatted.Length && char.IsNumber(formatted[i]))
                    i++;

                var markerKey = formatted[markerStart..i].ToString();
                i--;
                var element = elements[markerKey];

                if (wasLastLineHtml.HasValue && !wasLastLineHtml.Value && consecutiveNewLines < 2)
                    output.AppendLine();

                FormatElement(element, source, output, indent + "    ", tab);
                wasLastLineHtml = true;
            }
            else if (
                i == 0
                || (
                    formatted[i] != '\r'
                    && formatted[i] != '\n'
                    && (lastChar == '\r' || lastChar == '\n')
                )
            )
            {
                output.AppendIndented(ReadOnlySpan<char>.Empty, indent);
                output.Append("    ");
                output.Append(formatted[i]);
            }
            else if (formatted[i] != '\r' && formatted[i] != '\n')
            {
                output.Append(formatted[i]);
                wasLastLineHtml = false;
            }
            else if (consecutiveNewLines < 2)
                output.Append(formatted[i]);

            consecutiveNewLines = formatted[i] switch
            {
                '\n' => consecutiveNewLines + 1,
                '\r' when formatted[i + 1] != '\n' => 1,
                '\r' => consecutiveNewLines,
                _ => 0
            };

            lastChar = formatted[i];
        }
    }

    private static void FormatComment(
        CommentToken comment,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent,
        string tab
    )
    {
        output.AppendIndented(comment.Open(source), indent);

        var content = comment.Content(source).ToString().Trim(' ', '\t', '\r', '\n');
        var hasMultipleLines = content.Contains('\n') || content.Contains('\r');

        if (!hasMultipleLines)
        {
            output.Append(' ');
            output.Append(content);
            output.Append(' ');
            output.Append(comment.Close(source));
        }
        else
        {
            output.AppendLine();
            var lastCharWasNewLine = true;

            for (var i = 0; i < content.Length; i++)
            {
                if (content[i] != '\n' && content[i] != '\r')
                {
                    if (lastCharWasNewLine)
                        output.AppendIndented(content[i], indent + tab);
                    else
                        output.Append(content[i]);

                    lastCharWasNewLine = false;
                }
                else if (lastCharWasNewLine)
                    continue;
                else
                {
                    output.AppendLine();
                    lastCharWasNewLine = true;
                }
            }

            output.AppendLine();
            output.AppendIndented(comment.Close(source), indent);
        }
    }

    private static void FormatControlStructure(
        ControlStructureToken control,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent,
        string tab
    )
    {
        var open = control.Open(source).Trim();
        output.AppendIndented(open, indent);

        if (open[0] == '@')
            open = open[1..];

        var expression = control.Expression(source).Trim();

        var formattable = "namespace A;void A(){";
        var (formattableOpen, isFollowing) = open.Trim() switch
        {
            "else" or "@else" => ("if (true){} else", true),
            "else if" or "@else if" => ("if (true){} else if", true),
            "catch" or "@catch" => ("try {} catch", true),
            "finally" or "@finally" => ("try {} finally", true),
            _ => (open.ToString(), false)
        };
        formattable += $"{formattableOpen}{expression}{{}}}}";
        var formatted = CSharpier.CodeFormatter.Format(formattable);
        var offset = formatted.IndexOf(open.ToString());

        if (isFollowing)
            for (var i = offset; formatted[i] != '}'; i--)
                if (formatted[i] == '\n')
                    output.AppendLine();

        expression = formatted[
            (offset + open.Length)..^(formatted.Length - formatted.LastIndexOf('{') + 1)
        ].Trim();

        var isDoWhile = MemoryExtensions.Equals("@do", open, StringComparison.Ordinal);

        if (!isDoWhile)
            output.AppendIndented(expression, " ");

        if (control.Children.Count > 0)
        {
            output.AppendLine();
            output.AppendIndented('{', indent);
            output.AppendLine();
            FormatCodeBlockContent(control, source, output, indent, tab);
            output.AppendLine();
            output.AppendIndented('}', indent);
        }
        else
            output.Append(" { }");

        if (isDoWhile)
        {
            output.Append(' ');
            output.Append(expression);
        }
    }

    private static void FormatElement(
        ElementToken token,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent,
        string tab
    )
    {
        output.AppendIndented('<', indent);
        var tokenName = token.Name(source);
        output.Append(tokenName);

        if (token.Attributes.Count == 1)
            FormatAttribute(token.Attributes.First(), source, output, " ");
        else
            foreach (var attribute in token.Attributes)
            {
                output.AppendLine();
                FormatAttribute(attribute, source, output, indent + tab);
            }

        output.Append('>');

        if (IsVoidElement(tokenName))
            return;

        var children = token.Children.Where(x => x is not NewLineToken).ToArray();
        var lastChildWasText = false;
        int? childLength = null;

        if (children.Length == 1 && token.Attributes.Count == 0)
            childLength = children[0] switch
            {
                TextToken text => text.Value(source).Length,
                ImplicitRazorExpressionToken imp => imp.Value(source).Length,
                ExplicitRazorExpressionToken exp => exp.Code(source).Length,
                _ => null
            };

        if (childLength is not null && childLength < 10)
            FormatToken(children[0], source, output, "", "");
        else
            foreach (var child in children)
            {
                var isText =
                    child is TextToken
                    || child is ImplicitRazorExpressionToken
                    || child is ExplicitRazorExpressionToken;

                if (isText && lastChildWasText)
                    FormatToken(child, source, output, " ", "");
                else
                {
                    output.AppendLine();
                    FormatToken(child, source, output, indent + tab, tab);
                }

                lastChildWasText = isText;
            }

        if (
            token.Attributes.Count > 1
            || children.Length > 1
            || (
                children.Length > 0
                && (childLength is null || childLength.Value >= 10 || token.Attributes.Count > 0)
            )
        )
        {
            output.AppendLine();
            output.AppendIndented("</", indent);
        }
        else
            output.Append("</");

        output.Append(tokenName);
        output.Append('>');
    }

    private static void FormatExplicitRazorExpression(
        ExplicitRazorExpressionToken razor,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent
    )
    {
        output.AppendIndented(razor.Open(source), indent);
        output.Append(razor.Code(source).Trim());
        output.Append(razor.Close(source));
    }

    private static void FormatImplicitRazorExpression(
        ImplicitRazorExpressionToken razor,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent
    )
    {
        output.AppendIndented(razor.Value(source).Trim(), indent);
    }

    private static void FormatLineLevelDirective(
        LineLevelDirectiveToken directive,
        ReadOnlySpan<char> source,
        StringBuilder output
    )
    {
        output.Append(directive.Directive(source));
        output.Append(' ');
        var line = directive.Line(source).Trim();
        output.Append(line);

        if (line[^1] != ';')
            output.Append(';');
    }

    private static void FormatText(
        TextToken text,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent
    )
    {
        output.AppendIndented(text.Value(source).Trim(), indent);
    }

    private static void FormatToken(
        IToken token,
        ReadOnlySpan<char> source,
        StringBuilder output,
        string indent,
        string tab
    )
    {
        if (token is ElementToken element)
            FormatElement(element, source, output, indent, tab);
        else if (token is CommentToken comment)
            FormatComment(comment, source, output, indent, tab);
        else if (token is ExplicitRazorExpressionToken explicitRazor)
            FormatExplicitRazorExpression(explicitRazor, source, output, indent);
        else if (token is ImplicitRazorExpressionToken implicitRazor)
            FormatImplicitRazorExpression(implicitRazor, source, output, indent);
        else if (token is TextToken text)
            FormatText(text, source, output, indent);
        else if (token is ControlStructureToken control)
            FormatControlStructure(control, source, output, indent, tab);
        else if (token is CodeBlockToken codeBlock)
            FormatCodeBlock(codeBlock, source, output, indent, tab);
        else if (token is LineLevelDirectiveToken directive)
            FormatLineLevelDirective(directive, source, output);
        else if (token is NewLineToken)
            output.AppendLine();
    }

    private static bool IsVoidElement(ReadOnlySpan<char> name) =>
        _voidTypes.Contains(name.ToString());

    private static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span) =>
        span.Trim(' ', '\n', '\r', '\t');

    private static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> span, params char[] chars)
    {
        var startIndex = 0;
        var endIndex = 1;

        for (; startIndex < span.Length; startIndex++)
            if (!chars.Contains(span[startIndex]))
                break;

        for (; endIndex <= span.Length; endIndex++)
            if (!chars.Contains(span[^endIndex]))
                break;

        if (startIndex == span.Length)
            return ReadOnlySpan<char>.Empty;

        return span[startIndex..(span.Length - endIndex + 1)];
    }
}

// Private fields.
public static partial class Formatter
{
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
