using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Identifier: AstNode
    {
        public Identifier(Token token) : base(token)
        {
        }
    }
}