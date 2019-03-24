using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class VarDeclaration : AstNode
    {
    }

    public class Variable : AstNode
    {
    }

    public class VarType : AstNode
    {
        public VarType(Token t) : base(t)
        {
        }
    }

    public class VarDefinition : AstNode
    {
        public bool IsArray;
    }

    public class Constant : AstNode
    {
        public Constant(Token t) : base(t.Position)
        {
        }
    }

    public class ConstDefinition : AstNode
    {
    }
}