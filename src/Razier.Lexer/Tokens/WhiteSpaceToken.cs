namespace Razier.Lexer.Tokens;

public record struct WhiteSpaceToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
