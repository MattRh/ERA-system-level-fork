using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class GoToStatement : ExtensionStatement
    {
    }

    public class BreakStatement : ExtensionStatement
    {
    }

    public class IfStatement : ExtensionStatement
    {
        public IfStatement(Token t) : base(t.Position)
        {
        }
    }

    public class BlockBody : AstNode
    {
    }
}