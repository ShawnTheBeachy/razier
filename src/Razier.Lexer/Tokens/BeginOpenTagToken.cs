namespace Razier.Lexer.Tokens;

public record struct BeginOpenTagToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
