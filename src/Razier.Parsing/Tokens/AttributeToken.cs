namespace Razier.Parsing.Tokens;

public record struct AttributeToken : IToken
{
    public int KeyLength { get; init; }
    public int KeyOffset { get; init; }
    public int ValueLength { get; init; }
    public int ValueOffset { get; init; }

    public ReadOnlySpan<char> Key(ReadOnlySpan<char> source) =>
        source[KeyOffset..(KeyOffset + KeyLength)];

    public ReadOnlySpan<char> Value(ReadOnlySpan<char> source) =>
        ValueOffset == 0
            ? ReadOnlySpan<char>.Empty
            : source[ValueOffset..(ValueOffset + ValueLength)];
}
