using System;
using src.Tokenizer;

namespace src.Parser.Nodes
{
    public class AssemblyBlock : AstNode
    {
        public AssemblyBlock(Token t) : base(t.Position)
        {
        }
    }

    public class AssemblyStatement : AstNode
    {
        public AssemblyStatement(Token t) : base(t)
        {
        }

        protected AssemblyStatement() : base(string.Empty)
        {
        }
    }

    public class AssemblyMetaOperation : AssemblyStatement
    {
        public AssemblyMetaOperation(Token t) : base(t)
        {
        }
    }

    public class AssemblyOperation : AssemblyStatement
    {
        public AssemblyOperation(Token t) : base(t)
        {
        }
    }

    public class AssemblyOperationFormat : AssemblyStatement
    {
        public AssemblyOperationFormat(Token t) : base(t)
        {
        }
    }

    public class AssemblyCondition : AssemblyStatement
    {
        public AssemblyCondition(Token t) : base(t)
        {
        }
    }

    public class Register : AstNode
    {
        public Register(Token t) : base(t)
        {
        }
    }
}