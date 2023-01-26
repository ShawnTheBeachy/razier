namespace Razier.Parser.Tokens;

public record struct CodeBlockToken : IParsedToken
{
    public string Close { get; init; }
    public string Open { get; init; }
    public string Value { get; init; }
}
