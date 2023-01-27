namespace Razier.Parser.Tokens;

public record struct InlineCodeBlockToken : IParsedToken
{
    public string Value { get; init; }
}
