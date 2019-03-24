using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Expression : AstNode
    {
    }

    public class ExpressionOperator : AstNode
    {
    }

    public class CompOperator : ExpressionOperator
    {
    }

    public class Operand : AstNode
    {
    }

    public class Primary : AstNode
    {
    }

    public class Receiver : AstNode
    {
    }

    public class ArrayAccess : AstNode
    {
    }

    public class Reference : AstNode
    {
    }

    public class Dereference : AstNode
    {
        public Dereference(Token t) : base(t.Position)
        {
        }
    }

    public class ExplicitAddress : AstNode
    {
    }
}