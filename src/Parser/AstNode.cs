using System;
using System.Collections.Generic;
using System.Linq;
using src.Interfaces;
using src.Tokenizer;
using src.Utils;

namespace src.Parser
{
    /*public class AstNode
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
            if (Type != NodeType.Literal) {
                throw new ArgumentException("Can't get numeric value from non-literal node");
            }

            if (_numeric == null) {
                _numeric = int.Parse(Value);
            }

            return (int) _numeric;
        }

        public override string ToString()
        {
            var currentName = Type.ToString();
            if (!string.IsNullOrEmpty(Value) && !currentName.Equals(Value)) {
                currentName += "(" + Value + ")";
            }

            var childrenJson = "";
            foreach (var i in Children) {
                var childrenList = i.ToString().Split('\n');
                var formattedChildren = childrenList.Aggregate("", (current, j) => $"{current} {j}\n");

                childrenJson = childrenJson + formattedChildren;
            }

            var currentJson = currentName;
            if (childrenJson.Length > 0) {
                currentJson += ": {\n" + childrenJson + "}";
            }

            return currentJson;
        }
    }*/

    public abstract class AstNode: IDebuggable
    {
        public readonly string Value;
        public Position Position { get; private set; }

        public readonly List<AstNode> Children = new List<AstNode>();

        public AstNode(string value = null)
        {
            this.Value = value;
        }

        public AstNode(Token token)
        {
            this.Value = token.Value;
            this.Position = token.Position;
        }

        public void AddChild(AstNode node)
        {
            if (node == null) {
                throw new ArgumentNullException();
            }
            
            Children.Add(node);

            if (node.Position != null) {
                PropagatePosition(node.Position);
            }
        }

        public void AddChildren(IEnumerable<AstNode> nodes)
        {
            foreach (var node in nodes) {
                AddChild(node);
            }
        }

        public virtual void ParseChild(AstNode node)
        {
        }

        public void ParseChildren()
        {
            foreach (var child in Children) {
                ParseChild(child);
            }
        }

        public void PropagatePosition(Position p)
        {
            if (Position == null) {
                Position = p;
                return;
            }

            Position = new Position(Position.Start, p.End);
        }

        public void PropagatePosition(Token t)
        {
            PropagatePosition(t.Position);
        }

        public string ToDebugString()
        {
            var currentName = GetType().Name;
            if (!string.IsNullOrEmpty(Value) && !currentName.Equals(Value)) {
                currentName += "(" + Value + ")";
            }

            var childrenJson = "";
            foreach (var i in Children) {
                var childrenList = i.ToDebugString().Split('\n');
                var formattedChildren = childrenList.Aggregate("", (current, j) => $"{current}  {j}\n");

                childrenJson = childrenJson + formattedChildren;
            }

            var currentJson = currentName;
            if (childrenJson.Length > 0) {
                currentJson += ": {\n" + childrenJson + "}";
            }

            return currentJson;
        }
    }
}