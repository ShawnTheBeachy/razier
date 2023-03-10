namespace Razier.Parsing.Tokens;

public readonly record struct ImplicitRazorExpressionToken : IToken
{
    public int Length { get; init; }
    public int Offset { get; init; }

    public ReadOnlySpan<char> Value(ReadOnlySpan<char> source) => source[Offset..(Offset + Length)];
}
