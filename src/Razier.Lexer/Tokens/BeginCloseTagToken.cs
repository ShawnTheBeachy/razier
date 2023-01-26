namespace Razier.Lexer.Tokens;

public record struct BeginCloseTagToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
