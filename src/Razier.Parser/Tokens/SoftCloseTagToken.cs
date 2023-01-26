namespace Razier.Parser.Tokens;

public record struct SoftCloseTagToken : IParsedToken
{
    public string Value { get; init; }
}
