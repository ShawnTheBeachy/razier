using Razier.Lexer.Tokens;

namespace Razier.Lexer;

// Public methods.
public sealed partial class Lexer
{
    public Lexer(string input)
    {
        _input = input.AsMemory();
    }

    public IEnumerable<IToken> Lex()
    {
        _index = 0;
        StartToken();

        while (HasNotReachedEnd())
        {
            var tokens = ConsumeTokens();

            foreach (var token in tokens)
                if (token is not null)
                    yield return token;
        }

        yield return new EndOfFileToken();
    }
}

// Private methods.
public sealed partial class Lexer
{
    private void Advance() => Advance(1);

    private void Advance(int count) => _index += count;

    private void AdvanceUntil(char c)
    {
        do
        {
            _index++;
        } while (CharIsNot(c));
    }

    private char Char() => Char(_index);

    private char Char(int i) => _input.Span[i];

    private bool CharIs(char c) => _input.Span[_index] == c;

    private bool CharIs(int index, char c) => _input.Span[index] == c;

    private bool CharIsNot(char c) => !CharIs(c);

    private bool CharIsNotWhiteSpace() => !CharIsWhiteSpace();

    private bool CharIsWhiteSpace() => IsWhiteSpace(Char());

    private IToken ConsumeBeginTagToken() =>
        NextStringIs(Constants.BeginCloseTag)
            ? ConsumeFixedLengthToken<BeginCloseTagToken>(Constants.BeginCloseTag.Length)
            : NextStringIs(Constants.BeginComment)
                ? ConsumeFixedLengthToken<BeginCommentToken>(Constants.BeginComment.Length)
                : ConsumeFixedLengthToken<BeginOpenTagToken>(1);

    private IToken ConsumeCodeBlockContentToken()
    {
        var nestLevel = 1;
        var isInString = false;

        do
        {
            Advance();

            if (CharIs('"') && !IsEscaped(_index))
                isInString = !isInString;
            else if (CharIs('{') && !isInString)
                nestLevel++;
            else if (CharIs('}') && !isInString)
                nestLevel--;
        } while (CharIsNot('}') || nestLevel > 0);

        return ReadToken<CodeBlockContentToken>();
    }

    private IEnumerable<IToken> ConsumeCodeBlockTokenOrWordToken()
    {
        if (NextCharIs('{'))
        {
            yield return ConsumeFixedLengthToken<BeginCodeBlockToken>(2);
            yield return ConsumeCodeBlockContentToken();
            yield return ConsumeFixedLengthToken<EndCodeBlockToken>(1);
        }
        else if (NextStringIs("@code") && NextCharIs(_index + 4, '{', true))
        {
            AdvanceUntil('{');
            Advance();
            yield return ReadToken<BeginCodeBlockToken>();
            yield return ConsumeCodeBlockContentToken();
            yield return ConsumeFixedLengthToken<EndCodeBlockToken>(1);
        }
        else
            yield return ConsumeWordToken();
    }

    private IToken ConsumeEndCommentTokenOrWordToken() =>
        NextStringIs(Constants.CloseComment)
            ? ConsumeFixedLengthToken<EndCommentToken>(Constants.CloseComment.Length)
            : ConsumeWordToken();

    private T ConsumeFixedLengthToken<T>(int length)
        where T : IToken, new()
    {
        Advance(length);
        return ReadToken<T>();
    }

    private IToken ConsumeStringDelimiterTokenOrWordToken()
    {
        var delimiter = Char();

        if (!IsEscaped(_index))
        {
            Advance();
            return ReadToken<StringDelimiterToken>();
        }

        return ConsumeWordToken();
    }

    private IEnumerable<IToken?> ConsumeTokens()
    {
        if (CharIs('@'))
        {
            var tokens = ConsumeCodeBlockTokenOrWordToken();

            foreach (var token in tokens)
                yield return token;
        }
        else
            yield return Char() switch
            {
                Constants.BeginOpenTag => ConsumeBeginTagToken(),
                Constants.EndTag => ConsumeFixedLengthToken<EndTagToken>(1),
                Constants.DoubleQuoteString
                or Constants.SingleQuoteString
                    => ConsumeStringDelimiterTokenOrWordToken(),
                ' ' => ConsumeFixedLengthToken<WhiteSpaceToken>(1),
                '-' => ConsumeEndCommentTokenOrWordToken(),
                '=' => ConsumeFixedLengthToken<EqualsToken>(1),
                '\n' => ConsumeFixedLengthToken<NewLineToken>(1),
                '\r' => ConsumeFixedLengthToken<CarriageReturnToken>(1),
                '\t' => ConsumeFixedLengthToken<TabToken>(1),
                '\\' => ConsumeFixedLengthToken<EscapeToken>(1),
                _ => ConsumeWordToken()
            };
    }

    private WordToken ConsumeWordToken()
    {
        do
        {
            Advance();
        } while (
            HasNotReachedEnd()
            && CharIsNotWhiteSpace()
            && CharIsNot('=')
            && CharIsNot(Constants.BeginOpenTag)
            && CharIsNot(Constants.EndTag)
            && CharIsNot(Constants.DoubleQuoteString)
            && CharIsNot(Constants.SingleQuoteString)
            && CharIsNot('@')
            && CharIsNot('\\')
            && (CharIsNot('-') || NextStringIsNot("-->"))
        );

        return ReadToken<WordToken>();
    }

    private bool HasNotReachedEnd() => !HasReachedEnd();

    private bool HasReachedEnd() => HasReachedEnd(_index);

    private bool HasReachedEnd(int i) => i == _input.Length;

    private IToken? IgnoreToken()
    {
        Advance();
        return null;
    }

    private bool IsEscaped(int index) =>
        PreviousCharIs(index, '\\') && (index > 0 ? !IsPreviousCharEscaped(index) : false);

    private bool IsFirstChar() => _index == 0;

    private bool IsNotFirstChar() => !IsFirstChar();

    private bool IsPreviousCharEscaped(int index) => index > 0 && IsEscaped(index - 1);

    private bool IsWhiteSpace(char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

    private bool NextCharIs(char c, bool ignoreWhiteSpace = false) =>
        NextCharIs(_index, c, ignoreWhiteSpace);

    private bool NextCharIs(int index, char c, bool ignoreWhiteSpace = false)
    {
        var i = index + 1;

        if (HasReachedEnd(i))
            return false;

        if (ignoreWhiteSpace)
        {
            while (HasNotReachedEnd() && IsWhiteSpace(Char(i)))
                i++;
        }

        return HasNotReachedEnd() && Char(i) == c;
    }

    private bool NextCharIsNot(char c) => !NextCharIs(c);

    private bool NextStringIs(string s) => NextStringIs(_index, s);

    private bool NextStringIsNot(string s) => NextStringIsNot(_index, s);

    private bool NextStringIs(int index, string s)
    {
        for (var i = 0; i < s.Length; i++)
        {
            if (index + i == _input.Length || _input.Span[index + i] != s[i])
                return false;
        }

        return true;
    }

    private bool NextStringIsNot(int index, string s) => !NextStringIs(index, s);

    private bool PreviousCharIs(char c) => PreviousCharIs(_index, c);

    private bool PreviousCharIs(int index, char c) => index > 0 && _input.Span[index - 1] == c;

    private bool PreviousCharIsNot(char c) => !PreviousCharIs(c);

    private bool PreviousCharIsNot(int index, char c) => !PreviousCharIs(index, c);

    private T ReadToken<T>()
        where T : IToken, new()
    {
        var token = new T { Value = SliceToken() };
        StartToken();
        return token;
    }

    private ReadOnlyMemory<char> SliceToken() =>
        _input.Slice(_tokenStartIndex, _index - _tokenStartIndex);

    private void StartToken() => _tokenStartIndex = _index;
}

// Private fields.
public sealed partial class Lexer
{
    private int _index;
    private readonly ReadOnlyMemory<char> _input;
    private int _tokenStartIndex;
}
