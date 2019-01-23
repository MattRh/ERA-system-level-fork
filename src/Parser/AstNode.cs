using System;
using System.Collections.Generic;
using System.Linq;
using src.Tokenizer;

namespace src.Parser
{
    public class AstNode
    {
        public readonly NodeType Type;
        public readonly string Value;
        public readonly Token Token;

        public readonly List<AstNode> Children = new List<AstNode>();

        private int? _numeric;

        public AstNode(NodeType type, string value = null)
        {
            this.Type = type;
            this.Value = value;
        }

        public AstNode(NodeType type, Token token) : this(type, token.Value)
        {
            this.Token = token;
        }

        public int NumericValue()
        {
            if (Type != NodeType.Literal)
            {
                throw new ArgumentException("Can't get numeric value from non-literal node");
            }

            if (_numeric == null)
            {
                _numeric = int.Parse(Value);
            }

            return (int) _numeric;
        }

        public override string ToString()
        {
            var currentName = Type.ToString();
            if (!string.IsNullOrEmpty(Value) && !currentName.Equals(Value))
            {
                currentName += "(" + Value + ")";
            }

            var childrenJson = "";
            foreach (var i in Children)
            {
                var childrenList = i.ToString().Split('\n');
                var formattedChildren = childrenList.Aggregate("", (current, j) => $"{current} {j}\n");

                childrenJson = childrenJson + formattedChildren;
            }

            var currentJson = currentName;
            if (childrenJson.Length > 0)
            {
                currentJson += ": {\n" + childrenJson + "}";
            }

            return currentJson;
        }
    }
}