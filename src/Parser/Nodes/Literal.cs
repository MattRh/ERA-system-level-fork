using System;
using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Literal : AstNode
    {
        public int Numeric { get; private set; }

        public Literal(Token t) : base(t)
        {
        }

        public override void ParseChildren()
        {
            Numeric = int.Parse(Value);
        }
    }
}