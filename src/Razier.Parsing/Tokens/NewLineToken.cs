namespace Razier.Parsing.Tokens;

public readonly record struct NewLineToken : IToken
{
    public int Offset { get; init; }

    public ReadOnlySpan<char> Value(ReadOnlySpan<char> source) => source[Offset..(Offset + 1)];
}
