using System.Text;
using Razier.Parsing;
using Razier.Parsing.Tokens;

namespace Razier.Formatting;

// Public methods.
public static partial class Formatter
{
    public static string Format(string source, string tab = "  ")
    {
        var output = new StringBuilder();
        var tokens = Parser.Parse(source);
        var newLineCount = 0;

        foreach (var token in tokens)
        {
            var isNewLine = token is NewLineToken;

            if (!isNewLine)
            {
                for (var i = 0; i < Math.Min(1, newLineCount); i++)
                    output.AppendLine();

                FormatToken(token, source, output, 0, tab);
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
        int indentLevel,
        string tab
    )
    {
        for (var i = 0; i < indentLevel; i++)
            output.Append(tab);

        output.Append(value);
    }

    private static void AppendIndented(
        this StringBuilder output,
        char value,
        int indentLevel,
        string tab
    )
    {
        for (var i = 0; i < indentLevel; i++)
            output.Append(tab);

        output.Append(value);
    }

    private static void FormatAttribute(
        AttributeToken attribute,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        var key = attribute.Key(source);

        if (indentLevel > 0)
        {
            output.AppendLine();
            output.AppendIndented($"{prefix}{key}", indentLevel, tab);
        }
        else
            output.Append(key);

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
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        if (output.Length > 0)
            output.AppendLine();

        var open = codeBlock.Open(source).Trim();

        if (MemoryExtensions.Equals("@{", open[..2], StringComparison.Ordinal))
            output.AppendIndented($"{prefix}@{{", indentLevel, tab);
        else
        {
            output.AppendIndented($"{prefix}{open[..^1].Trim()}", indentLevel, tab);
            output.AppendLine();
            output.AppendIndented($"{prefix}{{", indentLevel, tab);
        }

        output.AppendLine();
        FormatCodeBlockContent(codeBlock, source, output, indentLevel, tab, prefix);
        output.AppendLine();
        output.AppendIndented($"{prefix}{codeBlock.Close(source)}", indentLevel, tab);
    }

    private static void FormatCodeBlockContent(
        CodeBlockToken codeBlock,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
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
                code.AppendLine($"//{nameof(ElementToken)}-{elements.Count}");
                elements[elements.Count.ToString()] = element;
            }
        }

        var formatted = CSharpier.CodeFormatter.Format(
            code.ToString(),
            new CSharpier.CodeFormatterOptions { Width = 80 - (4 * indentLevel) }
        )[12..].Trim('\r', '\n');

        var lines = formatted.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lastLineWasCSharp = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Contains($"//{nameof(ElementToken)}-"))
            {
                if (lastLineWasCSharp)
                    output.AppendLine();

                FormatElement(
                    elements[line.Split('-')[1]],
                    source,
                    output,
                    indentLevel,
                    tab,
                    !lastLineWasCSharp,
                    $"{prefix}    "
                );
                lastLineWasCSharp = false;
            }
            else
            {
                if (i > 0)
                    output.AppendLine();

                output.AppendIndented($"{prefix}    {line}", indentLevel, tab);
                lastLineWasCSharp = true;
            }
        }
    }

    private static void FormatComment(
        CommentToken comment,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        var open = comment.Open(source);
        output.AppendLine();
        output.AppendIndented($"{prefix}{open}", indentLevel, tab);

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
                        output.AppendIndented($"{prefix}{content[i]}", indentLevel + 1, tab);
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
            output.AppendIndented($"{prefix}{comment.Close(source)}", indentLevel, tab);
        }
    }

    private static void FormatControlStructureToken(
        ControlStructureToken control,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        if (output.Length > 0)
        {
            if (indentLevel == 0)
                output.AppendLine();

            output.AppendLine();
        }

        var open = control.Open(source).Trim();
        output.AppendIndented($"{prefix}{open}", indentLevel, tab);
        var expression = control.Expression(source).Trim();
        var isDoWhile = MemoryExtensions.Equals("@do", open, StringComparison.Ordinal);

        if (!isDoWhile)
        {
            output.Append(' ');
            output.Append(expression);
        }

        output.AppendLine();
        output.AppendIndented($"{prefix}{{", indentLevel, tab);
        output.AppendLine();
        FormatCodeBlockContent(control, source, output, indentLevel, tab, prefix);
        output.AppendLine();
        output.AppendIndented($"{prefix}}}", indentLevel, tab);

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
        int indentLevel,
        string tab,
        bool sameLine = false,
        string prefix = ""
    )
    {
        if (output.Length > 0 && !sameLine)
            output.AppendLine();

        output.AppendIndented($"{prefix}<", indentLevel, tab);
        var tokenName = token.Name(source);
        output.Append(tokenName);

        if (token.Attributes.Count == 1)
        {
            output.Append(' ');
            FormatAttribute(token.Attributes.First(), source, output, 0, tab);
        }
        else
            foreach (var attribute in token.Attributes)
                FormatAttribute(attribute, source, output, indentLevel + 1, tab, prefix);

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
            FormatToken(children[0], source, output, 0, tab);
        else
            foreach (var child in children)
            {
                var isText =
                    child is TextToken
                    || child is ImplicitRazorExpressionToken
                    || child is ExplicitRazorExpressionToken;

                if (isText && lastChildWasText)
                {
                    output.Append(' ');
                    FormatToken(child, source, output, 0, tab);
                }
                else
                    FormatToken(child, source, output, indentLevel + 1, tab, prefix);

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
            output.AppendIndented($"{prefix}</", indentLevel, tab);
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
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        var value = $"{prefix}{razor.Open(source)}{razor.Code(source).Trim()}{razor.Close(source)}";

        if (indentLevel > 0)
        {
            output.AppendLine();
            output.AppendIndented(value, indentLevel, tab);
        }
        else
            output.Append(value);
    }

    private static void FormatImplicitRazorExpression(
        ImplicitRazorExpressionToken razor,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        var value = $"{prefix}{razor.Value(source).Trim()}";

        if (indentLevel > 0)
        {
            output.AppendLine();
            output.AppendIndented(value, indentLevel, tab);
        }
        else
            output.Append(value);
    }

    private static void FormatText(
        TextToken text,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        var value = text.Value(source).Trim();

        if (indentLevel > 0)
        {
            output.AppendLine();
            output.AppendIndented($"{prefix}{value}", indentLevel, tab);
        }
        else
            output.Append(value);
    }

    private static void FormatToken(
        IToken token,
        ReadOnlySpan<char> source,
        StringBuilder output,
        int indentLevel,
        string tab,
        string prefix = ""
    )
    {
        if (token is ElementToken element)
            FormatElement(element, source, output, indentLevel, tab, false, prefix);
        else if (token is CommentToken comment)
            FormatComment(comment, source, output, indentLevel, tab, prefix);
        else if (token is ExplicitRazorExpressionToken explicitRazor)
            FormatExplicitRazorExpression(explicitRazor, source, output, indentLevel, tab, prefix);
        else if (token is ImplicitRazorExpressionToken implicitRazor)
            FormatImplicitRazorExpression(implicitRazor, source, output, indentLevel, tab, prefix);
        else if (token is TextToken text)
            FormatText(text, source, output, indentLevel, tab, prefix);
        else if (token is ControlStructureToken control)
            FormatControlStructureToken(control, source, output, indentLevel, tab, prefix);
        else if (token is CodeBlockToken codeBlock)
            FormatCodeBlock(codeBlock, source, output, indentLevel, tab, prefix);
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
