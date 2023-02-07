namespace Razier.Parsing.Tokens;

public record CodeBlockToken : IToken
{
    public List<IToken> Children { get; set; } = new();
    public int CloseOffset { get; init; }
    public int OpenLength { get; init; }
    public int OpenOffset { get; init; }

    public ReadOnlySpan<char> Close(ReadOnlySpan<char> source) =>
        source[CloseOffset..(CloseOffset + 1)];

    public ReadOnlySpan<char> Open(ReadOnlySpan<char> source) =>
        source[OpenOffset..(OpenOffset + OpenLength)];
}
