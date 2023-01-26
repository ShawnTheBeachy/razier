namespace Razier.Lexer.Tokens;

public record struct BeginCodeBlockToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
