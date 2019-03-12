using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using src.Tokenizer;
using src.Utils;

namespace src.Exceptions
{
    public class CompilationError : SystemException
    {
        public SourceCode Source;

        private readonly Position _position;

        public CompilationError(string message) : base(message)
        {
        }

        public CompilationError(string message, Position position) : base(message)
        {
            _position = position;
        }

        public CompilationError(string message, Token token) : this(message, token.Position)
        {
        }

        protected static MethodBase GetCallee(string prefix = null)
        {
            var stackTrace = new StackTrace();

            if (prefix == null) {
                return stackTrace.GetFrame(2).GetMethod();
            }

            foreach (var frame in stackTrace.GetFrames()) {
                var method = frame.GetMethod();
                var name = method.Name.ToLower();

                if (name.StartsWith(prefix)) {
                    return method;
                }
            }

            return null;
        }

        public string Verbose()
        {
            var result = GetType().Name;
            if (_position != null) {
                var start = _position.Start;
                var end = _position.End;

                var line = end.Line;
                var symbol = Math.Min(start.Symbol, end.Symbol);
                var length = Math.Abs(end.Symbol - start.Symbol);

                result += $" in {line}:{symbol}:";

                if (Source != null) {
                    var codePart = Source.Highlight(line, symbol - length, length);
                    result += "\n\n" + codePart;
                }
            }

            return result + "\n\n" + ToString();
        }
    }
}