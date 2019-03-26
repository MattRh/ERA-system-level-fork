using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class Statement : AstNode
    {
    }

    public class Label : AstNode
    {
    }

    public class ExtensionStatement : AstNode
    {
        public ExtensionStatement() : base()
        {
        }

        public ExtensionStatement(Token t) : base(t)
        {
        }
    }
}