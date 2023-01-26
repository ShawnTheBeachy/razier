namespace Razier.Parser.Tokens;

public record struct AttributeToken : IParsedToken
{
    public string Value { get; init; }
}
