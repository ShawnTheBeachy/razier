namespace Razier.Lexer.Tokens;

public record struct EndTagToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
