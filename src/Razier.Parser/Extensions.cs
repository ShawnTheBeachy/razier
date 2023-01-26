using System.Text;
using Razier.Lexer.Tokens;

namespace Razier.Parser;

public static class Extensions
{
    public static void AddToken(this StringBuilder sb, IToken token) => sb.Append(token.Value);
}
