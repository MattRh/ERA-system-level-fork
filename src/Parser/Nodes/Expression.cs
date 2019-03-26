using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Expression : AstNode
    {
    }

    public class EmptyExpression : AstNode
    {
    }

    public class ExpressionOperator : AstNode
    {
        public ExpressionOperator(Token t) : base(t)
        {
        }
    }

    public class CompOperator : ExpressionOperator
    {
        public CompOperator(Token t) : base(t)
        {
        }
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
        public Reference(Token t) : base(t.Position)
        {
        }
    }

    public class Dereference : AstNode
    {
        public Dereference(Token t) : base(t.Position)
        {
        }
    }

    public class ExplicitAddress : AstNode
    {
        public ExplicitAddress(Token t) : base(t.Position)
        {
        }
    }
}