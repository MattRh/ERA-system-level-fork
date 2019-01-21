using System;

namespace Erasystemlevel.Exception
{
    public class CompilationError: SystemException
    {
        public CompilationError(string message) : base(message)
        {
        }
    }
}