namespace Razier.Parser.Tokens;

public record struct BeginTagToken : IParsedToken
{
    public string Value { get; init; }
}
