namespace Razier.Parsing.Tokens;

public readonly record struct TextToken : IToken
{
    public int Length { get; init; }
    public int Offset { get; init; }

    public ReadOnlySpan<char> Value(ReadOnlySpan<char> source) => source[Offset..(Offset + Length)];
}
