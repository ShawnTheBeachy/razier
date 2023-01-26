namespace Razier.Lexer.Tokens;

public record struct BeginCommentToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
