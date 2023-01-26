namespace Razier.Lexer.Tokens;

public record struct EndOfFileToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
