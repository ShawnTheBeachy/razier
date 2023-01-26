namespace Razier.Parser.Tokens;

public record struct ContentToken : IParsedToken
{
    public string Value { get; init; }
}
