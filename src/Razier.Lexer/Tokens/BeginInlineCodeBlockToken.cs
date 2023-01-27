namespace Razier.Lexer.Tokens;

public record struct BeginInlineCodeBlockToken : IToken
{
    public ReadOnlyMemory<char> Value { get; init; }
}
