using System.Text;
using Razier.Parser.Tokens;

namespace Razier.Formatter;

// Public methods.
public sealed partial class Formatter
{
    public Formatter(string input, string indentation = "  ")
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
            ConsumeToken();
            Advance();
        }

        return _output.ToString();
    }
}

// Private methods.
public sealed partial class Formatter
{
    private void Advance() => _index++;

    private void ConsumeToken()
    {
        if (Token() is BeginTagToken)
        {
            IncreaseIndentation();
            OutputLine(Token());
        }
        else if (Token() is HardCloseTagToken)
        {
            if (Token().Value != ">")
                DecreaseIndentation();

            if (PreviousToken() is BeginTagToken)
                Output(Token());
            else
                OutputLine(Token());
        }
        else
            Output(Token());
    }

    private void DecreaseIndentation() => _indentationLevel--;

    private bool HasNotReachedEnd() => !HasReachedEnd();

    private bool HasReachedEnd() => _index == _tokens.Length;

    private void IncreaseIndentation() => _indentationLevel++;

    private void Output(IParsedToken token) => _output.Append(token.Value);

    private void OutputLine(IParsedToken token)
    {
        if (_output.Length > 0)
            _output.AppendLine();

        for (var i = 0; i < _indentationLevel; i++)
            _output.Append(_indentation);

        _output.Append(token.Value);
    }

    private IParsedToken PreviousToken() => PreviousToken(_index);

    private IParsedToken PreviousToken(int index) => _tokens[index - 1];

    private IParsedToken Token() => Token(_index);

    private IParsedToken Token(int index) => _tokens[index];
}

// Private fields.
public sealed partial class Formatter
{
    private readonly string _indentation;
    private int _indentationLevel = -1;
    private int _index = 0;
    private readonly StringBuilder _output = new();
    private readonly IParsedToken[] _tokens;
}
