namespace Razier.Parsing.Tokens;

public record struct CSharpToken : IToken
{
    public int Length { get; init; }
    public int Offset { get; init; }

    public ReadOnlySpan<char> Code(ReadOnlySpan<char> source) => source[Offset..(Offset + Length)];
}
