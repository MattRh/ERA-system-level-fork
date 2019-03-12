using System.Collections.Generic;
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

    public class PragmaText : AstNode
    {
        public PragmaText(string value = null) : base(value)
        {
        }
    }
}