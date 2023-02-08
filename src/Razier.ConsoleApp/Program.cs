using Razier.Formatting;
using Razier.Lexing;
using Razier.Parsing;
using Razier.Parsing.Tokens;
using static Crayon.Output;

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (input is null)
        continue;
    else if (input.StartsWith("parse -f "))
        ParseFile(input[9..].Trim('"'));
    else if (input.StartsWith("parse "))
        Parse(input[6..]);
    else if (input.StartsWith("lex "))
        Lex(input[4..]);
    else if (input.StartsWith("format -f "))
        FormatFile(input[10..].Trim('"'));
    else if (input.StartsWith("format "))
        Format(input[7..]);
    else if (input.StartsWith("csharp "))
        CSharp(input[7..].Trim('"'));
}

static void CSharp(string input)
{
    Console.WriteLine(CSharpier.CodeFormatter.Format(input));
}

static void Format(string input)
{
    var formatted = Formatter.Format(input);
    Console.WriteLine(formatted);
}

static void FormatFile(string path)
{
    if (!File.Exists(path))
        return;

    var input = File.ReadAllText(path);
    var formatted = Formatter.Format(input);
    Console.WriteLine(formatted);
}

static void Lex(string input)
{
    var lexemes = Lexer.Lex(input);

    foreach (var lexeme in lexemes)
    {
        Console.WriteLine($"{lexeme.Type}: {lexeme.Value(input)}");
    }
}

static void Output(IToken token, string source)
{
    Action output = token switch
    {
        ElementToken el => () => OutputElement(el, source, ""),
        CommentToken comment => () => OutputComment(comment, source),
        ControlStructureToken control => () => OutputControlStructure(control, source),
        CodeBlockToken code => () => OutputCodeBlock(code, source),
        ImplicitRazorExpressionToken imp => () => OutputImplicitRazorExpression(imp, source),
        LineLevelDirectiveToken directive
            => () =>
                Console.WriteLine(
                    $"{nameof(LineLevelDirectiveToken)}: {Magenta(directive.Directive(source).ToString())} {directive.Line(source).ToString()}"
                ),
        _ => () => Console.WriteLine()
    };
    output();
}

static void OutputCodeBlock(CodeBlockToken code, string source)
{
    Console.WriteLine($"{nameof(CodeBlockToken)}: {Magenta(code.Open(source).ToString())}");
    Console.WriteLine(Yellow("  Contents:"));

    foreach (var child in code.Children)
        if (child is ElementToken element)
            OutputElement(element, source, "");
        else if (child is CSharpToken cSharp)
            Console.WriteLine(Dim(cSharp.Code(source).ToString()));

    Console.WriteLine(Magenta(code.Close(source).ToString()));
}

static void OutputComment(CommentToken comment, string source, string prefix = "")
{
    Console.WriteLine(
        $"{prefix}{nameof(CommentToken)}: {Magenta($"{comment.Open(source)} {comment.Content(source)} {comment.Close(source)}")}"
    );
}

static void OutputControlStructure(ControlStructureToken control, string source, string prefix = "")
{
    Console.WriteLine(
        $"{nameof(ControlStructureToken)}: {Magenta(control.Open(source).ToString())}"
    );
    Console.WriteLine(
        $"{Yellow("  Expression:")} {Magenta(control.Expression(source).ToString())}"
    );
    Console.WriteLine(Yellow("  Contents:"));

    foreach (var child in control.Children)
        if (child is ElementToken element)
            OutputElement(element, source, $"{prefix}  ");
        else if (child is CSharpToken cSharp)
            Console.WriteLine(Dim(cSharp.Code(source).ToString()));
        else if (child is CommentToken comment)
            OutputComment(comment, source);

    Console.WriteLine(Magenta(control.Close(source).ToString()));
}

static void OutputElement(ElementToken token, string source, string prefix)
{
    Console.WriteLine($"{prefix}{nameof(ElementToken)}: {Magenta(token.Name(source).ToString())}");
    Console.WriteLine(Yellow($"{prefix}  Attributes:"));

    foreach (var attribute in token.Attributes)
        Console.WriteLine(
            $"{prefix}    {Dim(attribute.Key(source).ToString())}: {Magenta(attribute.Value(source).ToString())}"
        );

    Console.WriteLine(Yellow($"{prefix}  Children:"));

    foreach (var child in token.Children)
        if (child is ElementToken element)
            OutputElement(element, source, $"{prefix}    ");
        else if (child is ControlStructureToken control)
            OutputControlStructure(control, source, $"{prefix}    ");
        else if (child is CodeBlockToken code)
            OutputCodeBlock(code, source);
        else if (child is ImplicitRazorExpressionToken imp)
            OutputImplicitRazorExpression(imp, source, $"{prefix}    ");
        else if (child is CommentToken comment)
            OutputComment(comment, source, $"{prefix}    ");
        else if (child is TextToken text)
            Console.WriteLine(Dim($"{prefix}    {text.Value(source)}"));
        else
            Console.WriteLine(child);
}

static void OutputImplicitRazorExpression(
    ImplicitRazorExpressionToken token,
    string source,
    string prefix = ""
)
{
    Console.WriteLine(
        $"{prefix}{nameof(ImplicitRazorExpressionToken)}: {Dim(token.Value(source).ToString())}"
    );
}

static void Parse(string input)
{
    var tokens = Parser.Parse(input);

    foreach (var token in tokens)
        Output(token, input);
}

static void ParseFile(string path)
{
    if (!File.Exists(path))
        return;

    var input = File.ReadAllText(path);
    var tokens = Parser.Parse(input);

    foreach (var token in tokens)
        Output(token, input);
}
