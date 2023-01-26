namespace Razier.Lexer.Tokens;

public record struct CodeBlockContentToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
