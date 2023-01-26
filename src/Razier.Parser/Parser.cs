using System.Text;
using Razier.Lexer.Tokens;
using Razier.Parser.Tokens;

namespace Razier.Parser;

// Public methods.
public sealed partial class Parser
{
    public Parser(IToken[] tokens)
    {
        _tokens = tokens;
    }

    public IEnumerable<IParsedToken> Parse()
    {
        _index = 0;

        while (HasNotReachedEnd())
        {
            var token = ConsumeToken();

            if (token is not IgnoreToken)
                yield return token;

            Advance();
        }

        if (_nestLevel > 0)
            throw new Exception("A tag is not closed!");
    }
}

// Private methods.
public sealed partial class Parser
{
    private void Advance() => Advance(1);

    private void Advance(int count) => _index += count;

    private void AdvanceUntil<T>()
        where T : IToken
    {
        do
        {
            Advance();
        } while (Token() is not T);
    }

    private void AdvanceWhile<T>()
        where T : IToken
    {
        while (Token() is T)
        {
            Advance();
        }
    }

    private void AdvanceWhile<T1, T2>()
        where T1 : IToken
        where T2 : IToken
    {
        while (Token() is T1 || Token() is T2)
        {
            Advance();
        }
    }

    private void AdvanceWhile<T1, T2, T3>()
        where T1 : IToken
        where T2 : IToken
        where T3 : IToken
    {
        while (Token() is T1 || Token() is T2 || Token() is T3)
        {
            Advance();
        }
    }

    private AttributeToken ConsumeAttributeToken()
    {
        var value = new StringBuilder();
        value.AddToken(Token());

        if (NextTokenIs<EqualsToken>(true))
        {
            AdvanceUntil<EqualsToken>();
            value.AddToken(Token());
            AdvanceUntil<StringDelimiterToken>();
            value.AddToken(Token());
            var delimiter = Token().Value.ToString();

            do
            {
                Advance();
                value.AddToken(Token());
            } while (
                Token() is not StringDelimiterToken s
                || s.Value.ToString() != delimiter
                || IsEscaped()
            );
        }

        return new() { Value = value.ToString() };
    }

    private HardCloseTagToken ConsumeBeginCloseTagToken()
    {
        var value = new StringBuilder();
        value.AddToken(Token());

        AdvanceUntil<WordToken>();
        value.AddToken(Token());
        AdvanceUntil<EndTagToken>();
        value.AddToken(Token());

        _nestLevel--;
        return new() { Value = value.ToString() };
    }

    private BeginTagToken ConsumeBeginTagToken()
    {
        var value = new StringBuilder();
        value.AddToken(Token());

        AdvanceUntil<WordToken>();
        value.AddToken(Token());
        _tagName = Token().Value.ToString();

        if (NextTokenIs<EndTagToken>(true))
        {
            _isInTag = false;

            if (IsVoidElement(Token()))
                _tagName = null;
            else
                _nestLevel++;

            AdvanceUntil<EndTagToken>();
            value.AddToken(Token());
        }
        else
            _isInTag = true;

        return new() { Value = value.ToString() };
    }

    private CodeBlockToken ConsumeBeginCodeBlockToken()
    {
        var open = Token().Value.ToString();
        Advance();
        var code = Token().Value.ToString();
        Advance();
        var close = Token().Value.ToString();
        return new()
        {
            Close = close,
            Open = open,
            Value = code
        };
    }

    private CommentToken ConsumeCommentToken()
    {
        var value = new StringBuilder();
        value.AddToken(Token());

        while (Token() is not EndCommentToken)
        {
            Advance();
            value.AddToken(Token());
        }

        return new() { Value = value.ToString() };
    }

    private ContentToken ConsumeContentToken()
    {
        var value = new StringBuilder();
        value.AddToken(Token());

        while (NextTokenIsNot<BeginOpenTagToken>() && NextTokenIsNot<BeginCloseTagToken>())
        {
            Advance();
            value.AddToken(Token());
        }

        return new() { Value = value.ToString() };
    }

    private IParsedToken ConsumeEndTagToken()
    {
        _isInTag = false;

        if (IsVoidElement(_tagName!))
        {
            _tagName = null;
            return new HardCloseTagToken { Value = Token().Value.ToString() };
        }
        else
        {
            _tagName = null;
            _nestLevel++;
            var token = new SoftCloseTagToken { Value = Token().Value.ToString() };
            return token;
        }
    }

    private IParsedToken ConsumeToken() =>
        Token() switch
        {
            BeginCommentToken => ConsumeCommentToken(),
            BeginCloseTagToken => ConsumeBeginCloseTagToken(),
            BeginCodeBlockToken => ConsumeBeginCodeBlockToken(),
            BeginOpenTagToken => ConsumeBeginTagToken(),
            EndTagToken => ConsumeEndTagToken(),
            WordToken => _isInTag ? ConsumeAttributeToken() : ConsumeContentToken(),
            _ => new IgnoreToken()
        };

    private T ConsumeToken<T>()
        where T : IParsedToken, new()
    {
        Advance();
        var token = new T { Value = Token().Value.ToString() };
        return token;
    }

    private bool HasNotReachedEnd() => !HasReachedEnd();

    private bool HasReachedEnd() => _index == _tokens.Length;

    private bool IsEscaped() => IsEscaped(_index);

    private bool IsEscaped(int index) =>
        PreviousTokenIs<EscapeToken>(index) && (index > 0 ? !IsPreviousTokenEscaped(index) : false);

    private bool IsPreviousTokenEscaped(int index) => index > 0 && IsEscaped(index - 1);

    private bool IsVoidElement(IToken token) => IsVoidElement(token.Value.ToString());

    private bool IsVoidElement(string type) => _voidTypes.Contains(type);

    private T Last<T>()
        where T : IToken
    {
        var i = _index;

        do
        {
            i--;
        } while (TokenIsNot<T>());

        return (T)Token(i);
    }

    private IToken NextToken() => NextToken(_index);

    private IToken NextToken(int index) => _tokens[index + 1];

    private bool NextTokenIs<T>(bool ignoreWhiteSpace = false)
    {
        var i = _index + 1;

        if (i == _tokens.Length)
            return false;

        if (ignoreWhiteSpace)
        {
            while (
                HasNotReachedEnd() && (Token(i) is WhiteSpaceToken)
                || Token(i) is NewLineToken
                || Token(i) is TabToken
                || Token(i) is CarriageReturnToken
            )
            {
                i++;
            }
        }

        return Token(i) is T;
    }

    private bool NextTokenIsNot<T>(bool ignoreWhiteSpace = false) =>
        !NextTokenIs<T>(ignoreWhiteSpace);

    private IToken? PreviousToken(bool ignoreWhiteSpace = false) =>
        PreviousToken(_index, ignoreWhiteSpace);

    private IToken? PreviousToken(int index, bool ignoreWhiteSpace = false)
    {
        index--;

        if (ignoreWhiteSpace)
            while (
                TokenIs<WhiteSpaceToken>(index)
                || TokenIs<NewLineToken>(index)
                || TokenIs<CarriageReturnToken>(index)
                || TokenIs<TabToken>(index)
            )
            {
                index--;
            }

        return index < 1 ? null : Token(index);
    }

    private bool PreviousTokenIs<T>() => PreviousTokenIs<T>(_index);

    private bool PreviousTokenIs<T>(int index) => PreviousToken() is T;

    private bool PreviousTokenIsNot<T>() => PreviousTokenIsNot<T>(_index);

    private bool PreviousTokenIsNot<T>(int index) => PreviousToken() is not T;

    private IToken Token() => Token(_index);

    private IToken Token(int index) => _tokens[index];

    private bool TokenIs<T>() => TokenIs<T>(_index);

    private bool TokenIsNot<T>() => TokenIsNot<T>(_index);

    private bool TokenIs<T>(int index) => _tokens[index] is T;

    private bool TokenIsNot<T>(int index) => _tokens[index] is not T;
}

// Private fields.
public sealed partial class Parser
{
    private int _index;
    private bool _isInTag;
    private int _nestLevel;
    private string? _tagName;
    private readonly IToken[] _tokens;
    private readonly HashSet<string> _voidTypes =
        new()
        {
            "area",
            "base",
            "br",
            "col",
            "command",
            "embed",
            "hr",
            "img",
            "input",
            "keygen",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr"
        };
}
