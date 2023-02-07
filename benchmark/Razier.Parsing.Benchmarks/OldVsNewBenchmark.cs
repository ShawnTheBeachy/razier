using BenchmarkDotNet.Attributes;
using Razier.Lexing;

namespace Razier.Parsing.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class OldVsNewBenchmark
{
    private Lexeme[] NewLexemes { get; set; } = Array.Empty<Lexeme>();
    private Lexer.Tokens.IToken[] OldLexemes { get; set; } = default!;

    [Params(
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Widgets\\Spinner.razor",
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Shared\\LayoutLogin.razor",
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Shared\\CommandPalette.razor"
    )]
    public string FilePath { get; set; } = "";

    private string Source { get; set; } = "";

    [Benchmark]
    public void New()
    {
        Parser.Parse(NewLexemes, Source).ToArray();
    }

    [Benchmark]
    public void Old()
    {
        var parser = new Razier.Parser.Parser(OldLexemes);
        parser.Parse().ToArray();
    }

    [GlobalSetup]
    public void ReadFile()
    {
        Source = File.ReadAllText(FilePath);
        NewLexemes = Lexing.Lexer.Lex(Source).ToArray();
        OldLexemes = new Lexer.Lexer(Source).Lex().ToArray();
    }
}
