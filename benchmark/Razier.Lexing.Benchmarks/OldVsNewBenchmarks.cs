using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Razier.Lexing.Benchmarks;

[SimpleJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class OldVsNewBenchmarks
{
    private string Content { get; set; } = "";

    [Params(
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Widgets\\Spinner.razor",
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Shared\\LayoutLogin.razor",
        "C:\\dev\\sdsa\\sdsa-receipt-checklist\\SDSA.ReceiptChecklist\\src\\SDSA.ReceiptChecklist.Presentation.BlazorApp\\Shared\\CommandPalette.razor"
    )]
    public string FilePath { get; set; } = "";

    [Benchmark]
    public void New()
    {
        Lexer.Lex(Content).ToArray();
    }

    [Benchmark]
    public void Old()
    {
        var lexer = new Razier.Lexer.Lexer(Content);
        lexer.Lex().ToArray();
    }

    [GlobalSetup]
    public void ReadFile()
    {
        Content = File.ReadAllText(FilePath);
    }
}
