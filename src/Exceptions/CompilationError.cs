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
        public new SourceCode Source;

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

                result += $" at {line + 1}:{symbol + 1}:{length}:";

                if (Source != null) {
                    var prevLine = Source.FetchLine(line - 1, null);
                    var errorPart = Source.Highlight(line, symbol, length);
                    var nextLine = Source.FetchLine(line + 1, null);

                    var codePart = "*----------------------------*\n";
                    if (prevLine != null) {
                        codePart += prevLine + "\n";
                    }
                    codePart += errorPart;
                    if (nextLine != null) {
                        codePart += "\n" + nextLine;
                    }
                    codePart += "\n*----------------------------*";
                    
                    result += "\n\n" + codePart;
                }
            }

            return result + "\n\n" + ToString();
        }
    }
}