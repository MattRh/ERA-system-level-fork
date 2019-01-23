using System;
using src.Tokenizer;

namespace Erasystemlevel.Exception
{
    public class CompilationError : SystemException
    {
        public SourceCode Source;

        private readonly Token _token;

        public CompilationError(string message) : base(message)
        {
        }

        public CompilationError(string message, Token token) : base(message)
        {
            _token = token;
        }

        public string Verbose()
        {
            var result = GetType().Name;
            if (_token != null)
            {
                var line = _token.Position.Item1;
                var symbol = _token.Position.Item2;
                var length = _token.Value.Length;

                result += $" in {line}:{symbol}:";

                if (Source != null)
                {
                    var codePart = Source.Highlight(line, symbol - length, length);
                    result += "\n\n" + codePart;
                }
            }

            return result + "\n\n" + ToString();
        }
    }
}