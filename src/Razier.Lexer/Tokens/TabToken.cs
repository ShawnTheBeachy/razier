namespace Razier.Lexer.Tokens;

public record struct TabToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
