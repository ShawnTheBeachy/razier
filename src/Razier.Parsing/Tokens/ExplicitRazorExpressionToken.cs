namespace Razier.Parsing.Tokens;

public record struct ExplicitRazorExpressionToken : IToken
{
    public int CloseOffset { get; init; }
    public int OpenOffset { get; init; }

    public ReadOnlySpan<char> Close(ReadOnlySpan<char> source) =>
        source[CloseOffset..(CloseOffset + 1)];

    public ReadOnlySpan<char> Open(ReadOnlySpan<char> source) =>
        source[CloseOffset..(CloseOffset + 2)];
}
