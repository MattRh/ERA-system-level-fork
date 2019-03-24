using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class AssemblyBlock : AstNode
    {
    }

    public class AssemblyStatement : AstNode
    {
    }

    public class Register : AstNode
    {
        public Register(Token t) : base(t)
        {
        }
    }
}