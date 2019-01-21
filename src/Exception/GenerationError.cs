using System;

namespace Erasystemlevel.Exception
{
    public class GenerationError: CompilationError
    {
        public GenerationError(string message) : base(message)
        {
        }
    }
}