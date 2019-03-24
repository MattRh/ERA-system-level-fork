using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Identifier : AstNode
    {
        public Identifier(Token t) : base(t)
        {
        }
    }
}