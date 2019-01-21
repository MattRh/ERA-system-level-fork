using System;

namespace Erasystemlevel.Exception
{
    public class SemanticError : CompilationError
    {
        public SemanticError(string message) : base(message)
        {
        }
    }
}