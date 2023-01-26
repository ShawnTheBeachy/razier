namespace Razier.Lexer.Tokens;

public record struct StringDelimiterToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
