namespace Razier.Lexer.Tokens;

public record struct CarriageReturnToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
