namespace Razier.Parsing;

public static class Keywords
{
    public static bool IsReservedKeyword(this string value) => Reserved.Contains(value);

    public static readonly HashSet<string> Reserved =
        new()
        {
            "page",
            "namespace",
            "functions",
            "inherits",
            "model",
            "section",
            "case",
            "do",
            "default",
            "for",
            "foreach",
            "if",
            "else",
            "lock",
            "switch",
            "try",
            "catch",
            "finally",
            "using",
            "while"
        };
}
