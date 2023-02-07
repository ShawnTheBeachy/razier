namespace Razier.Parsing.Tokens;

public sealed record ElementToken : IToken
{
    public List<AttributeToken> Attributes { get; } = new();
    public List<IToken> Children { get; } = new();
    public required int NameLength { get; init; }
    public required int NameOffset { get; init; }

    public ReadOnlySpan<char> Name(ReadOnlySpan<char> source) =>
        source[NameOffset..(NameOffset + NameLength)];
}
