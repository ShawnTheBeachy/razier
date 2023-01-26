namespace Razier.Lexer.Tokens;

public record struct EndCodeBlockToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
