using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Code : AstNode
    {
        public Code(Token t) : base(t.Position)
        {
        }
    }
}