namespace Razier.Parsing.Tokens;

public readonly record struct ExplicitRazorExpressionToken : IToken
{
    public int CloseOffset { get; init; }
    public int OpenOffset { get; init; }

    public ReadOnlySpan<char> Close(ReadOnlySpan<char> source) =>
        source[CloseOffset..(CloseOffset + 1)];

    public ReadOnlySpan<char> Code(ReadOnlySpan<char> source) =>
        source[(OpenOffset + 2)..CloseOffset];

    public ReadOnlySpan<char> Open(ReadOnlySpan<char> source) =>
        source[OpenOffset..(OpenOffset + 2)];
}
