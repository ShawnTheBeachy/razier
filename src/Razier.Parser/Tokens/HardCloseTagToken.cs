namespace Razier.Parser.Tokens;

public record struct HardCloseTagToken : IParsedToken
{
    public string Value { get; init; }
}
