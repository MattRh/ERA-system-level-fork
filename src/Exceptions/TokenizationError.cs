namespace src.Exceptions
{
    public class TokenizationError : CompilationError
    {
        public TokenizationError(string message) : base(message)
        {
        }
    }
}