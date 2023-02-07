namespace Razier.Lexing;

[Flags]
public enum LexemeType
{
    At,
    LeftChevron,
    RightChevron,
    LeftParenthesis,
    RightParenthesis,
    LeftBrace,
    RightBrace,
    BackSlash,
    ForwardSlash,
    Dash,
    Exclamation,
    Asterisk,
    WhiteSpace,
    NewLine,
    CarriageReturn,
    Tab,
    Text,
    EndOfFile,
    RazorKeyword,
    CSharpRazorKeyword,
    SingleQuote,
    DoubleQuote,
    Semicolon,
    Equals
}
