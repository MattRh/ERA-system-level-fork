using System;
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
            var callee = GetCallee();

            return new SyntaxError($"{callee.Name} failed: {message}", token);
        }

        public static SyntaxError Make(Func<string> message)
        {
            return Make(message.Invoke());
        }

        public static SyntaxError Make(Func<Token, string> message, Token token)
        {
            return Make(message.Invoke(token), token);
        }

        public static SyntaxError Make(Func<string, Token, string> message, string val, Token token)
        {
            return Make(message.Invoke(val, token), token);
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
            return $"Unexpected token. Expected `{expected}` but got `{received}`";
        }

        public static string IDENTIFIER_EXPECTED(Token received)
        {
            return $"Unexpected token. Expected identifier but got `{received}`";
        }
    }
}