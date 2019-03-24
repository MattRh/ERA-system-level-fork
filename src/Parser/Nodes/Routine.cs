using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Routine : AstNode
    {
        public Routine(Token t) : base(t.Position)
        {
        }
    }

    public class RoutineAttribute : AstNode
    {
        public RoutineAttribute(Token t) : base(t)
        {
        }
    }

    public class Parameters : AstNode
    {
    }

    public class Parameter : AstNode
    {
    }

    public class Results : AstNode
    {
    }

    public class RoutineBody : AstNode
    {
    }
}