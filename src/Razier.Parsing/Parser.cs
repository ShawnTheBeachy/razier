using Razier.Lexing;
using Razier.Parsing.Tokens;

namespace Razier.Parsing;

// Public methods.
public static partial class Parser
{
    public static IList<IToken> Parse(string source)
    {
        var lexemes = Lexer.Lex(source);
        var tokens = new List<IToken>();

        for (var i = 0; i < lexemes.Count; i++)
        {
            Func<IToken> consume = lexemes[i].Type switch
            {
                LexemeType.At
                    when lexemes[i + 1].Type == LexemeType.LeftParenthesis
                        && (i == 0 || lexemes[i - 1].Type != LexemeType.At)
                    => () => ConsumeExplicitRazorExpression(lexemes, ref i),
                _ => throw new NotImplementedException()
            };
            tokens.Add(consume());
        }

        return tokens;
    }
}

// Private methods.
public static partial class Parser
{
    private static ExplicitRazorExpressionToken ConsumeExplicitRazorExpression(
        IList<Lexeme> lexemes,
        ref int index
    )
    {
        var isInString = false;
        var nestLevel = 0;
        var token = new ExplicitRazorExpressionToken { OpenOffset = index };

        for (; index < lexemes.Count; index++)
        {
            if (lexemes[index].Type == LexemeType.DoubleQuote && !IsEscaped(lexemes, index))
                isInString = !isInString;
            else if (lexemes[index].Type == LexemeType.LeftParenthesis && !isInString)
                nestLevel++;
            else if (lexemes[index].Type == LexemeType.RightParenthesis && !isInString)
                nestLevel--;
            else if (lexemes[index].Type == LexemeType.RightParenthesis)
                break;
        }

        return token with
        {
            CloseOffset = index++
        };
    }

    private static bool IsEscaped(IList<Lexeme> lexemes, int index)
    {
        if (index < 1)
            return false;

        if (lexemes[index - 1].Type != LexemeType.BackSlash)
            return false;

        return !IsEscaped(lexemes, index - 1);
    }
}
