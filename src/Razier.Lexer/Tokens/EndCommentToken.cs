namespace Razier.Lexer.Tokens;

public record struct EndCommentToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
