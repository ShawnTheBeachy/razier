namespace Razier.Lexer.Tokens;

public interface IToken
{
    ReadOnlyMemory<char> Value { get; init; }
}
