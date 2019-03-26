using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Loop : ExtensionStatement
    {
        public Loop() : base()
        {
        }

        public Loop(Token t) : base(t)
        {
        }
    }

    public class ForLoop : Loop
    {
        public ForLoop(Token t) : base(t)
        {
        }
    }

    public class WhileLoop : Loop
    {
        public WhileLoop(Token t) : base(t)
        {
        }
    }

    public class InfiniteLoop : Loop
    {
        public InfiniteLoop() : base()
        {
        }
    }

    public class LoopBody : AstNode
    {
    }
}