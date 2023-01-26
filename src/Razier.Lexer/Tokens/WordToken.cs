namespace Razier.Lexer.Tokens;

public record struct WordToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
