namespace Razier.Lexing;

public enum LexemeType
{
    Unknown,
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
    DoubleQuote
}
