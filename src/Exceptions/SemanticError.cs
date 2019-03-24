namespace src.Exceptions
{
    public class SemanticError : CompilationError
    {
        public SemanticError(string message) : base(message)
        {
        }
    }
}