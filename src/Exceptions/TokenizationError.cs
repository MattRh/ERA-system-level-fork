using src.Parser.Nodes;
using src.Utils;

namespace src.Exceptions
{
    public class TokenizationError : CompilationError
    {
        public TokenizationError(string message) : base(message)
        {
        }

        public TokenizationError(string message, Position pos) : base(message, pos)
        {
        }

        public static TokenizationError UnknownSymbol(string symbol, Position pos)
        {
            return new TokenizationError($"Unknown symbol `{symbol}` encountered", pos);
        }
        
        public static TokenizationError TokenizationFail(string symbol, Position pos)
        {
            return new TokenizationError($"Failed to tokenize string: `{symbol}`", pos);
        }
        
        public static TokenizationError UnexpectedEol(string symbol, Position pos)
        {
            return new TokenizationError($"Got new line, while reading token: `{symbol}`", pos);
        }
    }
}