using BenchmarkDotNet.Attributes;

namespace Razier.Formatting.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class OldVsNewBenchmark
{
    private Parsing.Tokens.IToken[] NewTokens { get; set; } = default!;
    private Parser.Tokens.IParsedToken[] OldTokens { get; set; } = default!;

    [Params(
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Widgets\\Spinner.razor",
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Shared\\LayoutLogin.razor",
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Shared\\CommandPalette.razor"
    )]
    public string FilePath { get; set; } = "";

    public string Source { get; set; } = "";

    [Benchmark]
    public void New()
    {
        Formatter.Format(NewTokens, Source);
    }

    [Benchmark]
    public void Old()
    {
        new Razier.Formatter.Formatter(OldTokens).Format();
    }

    [GlobalSetup]
    public void ParseFile()
    {
        Source = File.ReadAllText(FilePath);
        var oldLexemes = new Lexer.Lexer(Source).Lex();
        OldTokens = new Parser.Parser(oldLexemes.ToArray()).Parse().ToArray();
        NewTokens = Parsing.Parser.Parse(Source).ToArray();
    }
}
