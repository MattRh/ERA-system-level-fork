using System;
using System.Collections.Generic;
using System.Linq;
using src.Interfaces;
using src.Tokenizer;
using src.Utils;

namespace src.Parser
{
    public abstract class AstNode : IDebuggable
    {
        public readonly string Value;
        public Position Position { get; private set; }
        public AstNode Parent { get; private set; }

        public readonly List<AstNode> Children = new List<AstNode>();

        public AstNode(string value = null)
        {
            this.Value = value;
        }

        public AstNode(Position position)
        {
            this.Position = position;
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
            node.Parent = this;

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

        public virtual void ParseChildren()
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

            var start = Position.Start.CompareTo(p.Start) > 0 ? p.Start : Position.Start;
            var end = Position.End.CompareTo(p.End) < 0 ? p.End : Position.End;

            Position = new Position(start, end);
            //Position = new Position(Position.Start, p.End);
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