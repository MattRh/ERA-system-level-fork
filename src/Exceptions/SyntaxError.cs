using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using src.Tokenizer;

namespace src.Exceptions
{
    public class SyntaxError : CompilationError
    {
        public SyntaxError(string message) : base(message)
        {
        }

        public SyntaxError(string message, Token token) : base(message, token)
        {
        }

        public static SyntaxError Make(string message, Token token = null)
        {
            var stackTrace = new StackTrace();
            var callee = stackTrace.GetFrame(1).GetMethod();

            return new SyntaxError($"{callee.Name} failed: {message}", token);
        }
    }

    public class SyntaxErrorMessages
    {
        public static string INVALID_TOKEN(Token token)
        {
            return $"Invalid token `{token}` encountered";
        }

        public static string UNEXPECTED_EOS()
        {
            return "Unexpected end of stream";
        }

        public static string UNEXPECTED_TOKEN(string expected, Token received)
        {
            return $"Unexpected token. Expected `{expected}` by got `{received}`";
        }
    }
}