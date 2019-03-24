using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Module : AstNode
    {
        public Module(Token t) : base(t.Position)
        {
        }
    }
}