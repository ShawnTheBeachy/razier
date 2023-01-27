using Razier.Formatter;
using Razier.Lexer;
using Razier.Parser;

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();

    if (input is null)
        continue;

    if (input.StartsWith("parse "))
    {
        try
        {
            Parse(input[6..]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    else if (input.StartsWith("lex "))
    {
        try
        {
            Lex(input[4..]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    else if (input.StartsWith("format "))
    {
        try
        {
            Format(input[7..]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

static void Format(string input)
{
    var formatter = new Formatter(input);
    var formatted = formatter.Format();
    Console.WriteLine(formatted);
}

static void Lex(string input)
{
    var lexer = new Lexer(input!);
    var tokens = lexer.Lex();

    foreach (var token in tokens)
    {
        Console.WriteLine($"{token.GetType().Name}: {token.Value}");
    }
}

static void Parse(string input)
{
    var lexer = new Lexer(input!);
    var parser = new Parser(lexer.Lex().ToArray());
    var tokens = parser.Parse();

    foreach (var token in tokens)
    {
        Console.WriteLine($"{token.GetType().Name}: {token.Value}");
    }
}
