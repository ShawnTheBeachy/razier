namespace Razier.Lexer.Tokens;

public record struct NewLineToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
