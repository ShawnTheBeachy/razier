namespace Razier.Parsing.Tokens;

public readonly record struct CommentToken : IToken
{
    public int CloseLength { get; init; }
    public int CloseOffset { get; init; }
    public int ContentLength { get; init; }
    public int ContentOffset { get; init; }
    public int OpenLength { get; init; }
    public int OpenOffset { get; init; }

    public ReadOnlySpan<char> Close(ReadOnlySpan<char> source) =>
        source[CloseOffset..(CloseOffset + CloseLength)];

    public ReadOnlySpan<char> Content(ReadOnlySpan<char> source) =>
        source[ContentOffset..(ContentOffset + ContentLength)];

    public ReadOnlySpan<char> Open(ReadOnlySpan<char> source) =>
        source[OpenOffset..(OpenOffset + OpenLength)];
}
