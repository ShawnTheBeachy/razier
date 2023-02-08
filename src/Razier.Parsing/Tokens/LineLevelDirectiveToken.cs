namespace Razier.Parsing.Tokens;

public readonly record struct LineLevelDirectiveToken : IToken
{
    public int DirectiveLength { get; init; }
    public int DirectiveOffset { get; init; }
    public int LineLength { get; init; }
    public int LineOffset { get; init; }

    public ReadOnlySpan<char> Directive(ReadOnlySpan<char> source) =>
        source[DirectiveOffset..(DirectiveOffset + DirectiveLength)];

    public ReadOnlySpan<char> Line(ReadOnlySpan<char> source) =>
        source[LineOffset..(LineOffset + LineLength)];
}
