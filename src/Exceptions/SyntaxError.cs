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
    }
}