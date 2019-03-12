using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Annotation : AstNode
    {
        public Annotation(Token token) : base(token)
        {
        }
    }

    public class PragmaDeclaration : AstNode
    {
    }
}