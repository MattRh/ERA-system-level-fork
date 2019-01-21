using System;

namespace Erasystemlevel.Exception
{
    public class TokenizationError : CompilationError
    {
        public TokenizationError(string message) : base(message)
        {
        }
    }
}