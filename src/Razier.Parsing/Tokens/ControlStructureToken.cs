namespace Razier.Parsing.Tokens;

public sealed record ControlStructureToken : CodeBlockToken
{
    public int ExpressionLength { get; init; }
    public int ExpressionOffset { get; init; }

    public ReadOnlySpan<char> Expression(ReadOnlySpan<char> source) =>
        ExpressionLength < 1
            ? ReadOnlySpan<char>.Empty
            : source[ExpressionOffset..(ExpressionOffset + ExpressionLength)];
}
