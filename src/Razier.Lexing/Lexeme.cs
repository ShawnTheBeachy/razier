namespace Razier.Lexing;

public readonly record struct Lexeme
{
    public int Length { get; init; }
    public int Offset { get; init; }
    public LexemeType Type { get; init; }

    public ReadOnlySpan<char> Value(ReadOnlySpan<char> source) => source[Offset..(Offset + Length)];
}
