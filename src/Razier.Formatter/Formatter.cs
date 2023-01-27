using System.Text;
using Razier.Parser.Tokens;

namespace Razier.Formatter;

// Public methods.
public sealed partial class Formatter
{
    public Formatter(string input, string indentation = "    ")
    {
        _indentation = indentation;
        var lexer = new Lexer.Lexer(input);
        var parser = new Parser.Parser(lexer.Lex().ToArray(), true);
        _tokens = parser.Parse().ToArray();
    }

    public string Format()
    {
        _output.Clear();
        _index = 0;

        while (HasNotReachedEnd())
        {
            ConsumeToken(-1, false);
            Advance();
        }

        return _output.ToString();
    }
}

// Private methods.
public sealed partial class Formatter
{
    private void Advance() => _index++;

    private void ConsumeCode(int indentLevel)
    {
        var ct = (CodeBlockToken)(Token());
        var code = CSharpier.CodeFormatter.Format(
            Token().Value,
            new CSharpier.CodeFormatterOptions
            {
                Width = MAX_LINE_LENGTH - (indentLevel * _indentation.Length)
            }
        );

        if (code.IndexOf('\n') < 0)
            OutputLine(new ContentToken { Value = $"{ct.Open} {code} {ct.Close}" }, ++indentLevel);
        else
        {
            OutputLine(new ContentToken { Value = ct.Open }, ++indentLevel);
            OutputLine(new ContentToken { Value = code }, ++indentLevel);
            OutputLine(new ContentToken { Value = ct.Close }, --indentLevel);
        }
    }

    private void ConsumeContent(int indentLevel, bool forceNewLine)
    {
        if (
            !forceNewLine
            && !Token().Value.Contains('\r')
            && !Token().Value.Contains('\n')
            && PreviousTokenIs<SoftCloseTagToken>()
            && NextTokenIs<HardCloseTagToken>()
            && LineLength(Token(), indentLevel) < MAX_LINE_LENGTH
        )
            Output(Token());
        else
        {
            indentLevel++;
            OutputLine(Token(), indentLevel);
        }
    }

    private void ConsumeElement(int indentLevel)
    {
        OutputLine(Token(), ++indentLevel);
        var hasMultipleAttributes =
            NextTokenIs<AttributeToken>() && NextTokenIs<AttributeToken>(_index + 1);

        while (NextTokenIs<AttributeToken>())
        {
            Advance();

            if (hasMultipleAttributes)
                OutputLine(Token(), indentLevel + 1);
            else
                Output(Token(), true);
        }

        Advance();

        if (TokenIs<HardCloseTagToken>())
        {
            if (Token().Value == ">")
                Output(Token());
            else if (hasMultipleAttributes)
                OutputLine(Token(), indentLevel);
            else
                Output(Token(), true);

            return;
        }
        else if (TokenIs<SoftCloseTagToken>())
            Output(Token());

        Advance();
        var children = 0;

        while (TokenIsNot<HardCloseTagToken>())
        {
            children++;
            ConsumeToken(indentLevel, hasMultipleAttributes);
            Advance();
        }

        if (
            !hasMultipleAttributes
            && (
                children == 0
                || (
                    children == 1
                    && PreviousTokenIs<ContentToken>()
                    && LineLength(PreviousToken(), indentLevel) < MAX_LINE_LENGTH
                )
            )
        )
            Output(Token());
        else
            OutputLine(Token(), indentLevel);
    }

    private void ConsumeToken(int indentLevel, bool forceNewLine)
    {
        Action consume = Token() switch
        {
            BeginTagToken => () => ConsumeElement(indentLevel),
            CodeBlockToken => () => ConsumeCode(indentLevel),
            _ => () => ConsumeContent(indentLevel, forceNewLine)
        };
        consume();
    }

    private void DecreaseIndentation() => _indentationLevel--;

    private bool HasNotReachedEnd() => !HasReachedEnd();

    private bool HasReachedEnd() => _index == _tokens.Length;

    private void IncreaseIndentation() => _indentationLevel++;

    private int LineLength(IParsedToken token, int indentLevel) =>
        token.Value.Length + (indentLevel * _indentation.Length);

    private bool NextTokenIs<T>()
        where T : IParsedToken => NextTokenIs<T>(_index);

    private bool NextTokenIs<T>(int index)
        where T : IParsedToken =>
        HasNotReachedEnd() && index < _tokens.Length - 1 && _tokens[index + 1] is T;

    private bool NextTokenIsNot<T>()
        where T : IParsedToken => !NextTokenIs<T>();

    private void Output(IParsedToken token, bool withSpace = false) =>
        _output.Append($"{(withSpace ? " " : "")}{token.Value}");

    private void OutputLine(IParsedToken token) => OutputLine(token, _indentationLevel);

    private void OutputLine(IParsedToken token, int indentLevel)
    {
        var lines = token.Value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < lines.Length; i++)
        {
            if (_output.Length > 0)
                _output.AppendLine();

            for (var j = 0; j < indentLevel; j++)
                _output.Append(_indentation);

            _output.Append(lines[i]);
        }
    }

    private IParsedToken PreviousToken() => PreviousToken(_index);

    private IParsedToken PreviousToken(int index) => _tokens[index - 1];

    private bool PreviousTokenIs<T>()
        where T : IParsedToken => PreviousTokenIs<T>(_index);

    private bool PreviousTokenIsNot<T>()
        where T : IParsedToken => !PreviousTokenIs<T>(_index);

    private bool PreviousTokenIs<T>(int index)
        where T : IParsedToken => index > 0 && _tokens[index - 1] is T;

    private bool PreviousTokenIsNot<T>(int index)
        where T : IParsedToken => !PreviousTokenIs<T>(index);

    private IParsedToken Token() => Token(_index);

    private IParsedToken Token(int index) => _tokens[index];

    private bool TokenIs<T>()
        where T : IParsedToken => TokenIs<T>(_index);

    private bool TokenIsNot<T>()
        where T : IParsedToken => TokenIsNot<T>(_index);

    private bool TokenIs<T>(int index)
        where T : IParsedToken => _tokens[index] is T;

    private bool TokenIsNot<T>(int index)
        where T : IParsedToken => !TokenIs<T>(index);
}

// Private fields.
public sealed partial class Formatter
{
    private readonly string _indentation;
    private int _indentationLevel = -1;
    private int _index = 0;
    private readonly StringBuilder _output = new();
    private readonly IParsedToken[] _tokens;
    private const int MAX_LINE_LENGTH = 80;
}
