using System;

namespace Erasystemlevel.Exception
{
    public class SyntaxError : CompilationError
    {
        public SyntaxError(string message) : base(message)
        {
        }
    }
}