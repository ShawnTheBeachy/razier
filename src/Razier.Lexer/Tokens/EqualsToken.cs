namespace Razier.Lexer.Tokens;

public record struct EqualsToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
