namespace src.Exceptions
{
    public class GenerationError: CompilationError
    {
        public GenerationError(string message) : base(message)
        {
        }
    }
}