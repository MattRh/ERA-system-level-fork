using System;

namespace Erasystemlevel.Exception
{
    public class TokenizationError : SystemException
    {
        public TokenizationError(string message) : base(message)
        {
        }
    }
}