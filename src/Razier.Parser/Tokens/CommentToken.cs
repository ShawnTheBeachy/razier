namespace Razier.Parser.Tokens;

public record struct CommentToken : IParsedToken
{
    public string Value { get; init; }
}
