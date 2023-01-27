namespace Razier.Lexing;

public record struct Lexeme
{
    public int Length { get; init; }
    public int Offset { get; init; }
    public LexemeType Type { get; init; }
}
