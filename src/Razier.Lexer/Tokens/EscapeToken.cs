namespace Razier.Lexer.Tokens;

public record struct EscapeToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
