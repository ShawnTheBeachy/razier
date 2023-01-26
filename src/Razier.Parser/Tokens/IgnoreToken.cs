namespace Razier.Parser.Tokens;

public record struct IgnoreToken : IParsedToken
{
    public string Value { get; init; }
}
