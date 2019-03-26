using System;
using System.Collections.Generic;
using src.Interfaces;

namespace src.Tokenizer
{
    public class TokenStream : IDebuggable
    {
        private readonly HashSet<TokenType> _skipList = new HashSet<TokenType>() {
            TokenType.LineComment, TokenType.NewLine
        };

        private readonly Tokenizer _tokenizer;

        private int _position;
        private LinkedList<int> _checkpoints;

        public TokenStream(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;

            Reset();
        }

        public void Reset()
        {
            _position = 0;
            _checkpoints = new LinkedList<int>();
        }

        public Token Previous(bool movePointer = true)
        {
            var pos = _position;

            Token res;
            do {
                res = GetToken(--pos);
            } while (res != null && InSkipList(res));

            if (movePointer) {
                _position = pos;
            }

            return res;
        }

        public Token Current()
        {
            return GetToken(_position);
        }

        public Token Next(bool movePointer = true)
        {
            var pos = _position;

            Token res;
            do {
                res = GetToken(pos++);
            } while (res != null && InSkipList(res));

            if (movePointer) {
                _position = pos;
            }

            return res;
        }

        public Token Last()
        {
            var pos = _tokenizer.Tokens.Count;

            Token res;
            do {
                res = GetToken(pos--);
            } while (res == null || InSkipList(res));

            return res;
        }

        public bool HasTokens()
        {
            return Next(false) != null;
        }

        public void Rollback(LinkedListNode<int> node = null)
        {
            if (node == null) {
                node = _checkpoints.Last;
            }

            _position = node.Value;

            _checkpoints.Remove(node);
        }

        public LinkedListNode<int> Fixate()
        {
            return _checkpoints.AddLast(_position);
        }

        private Token GetToken(int pos)
        {
            if (pos >= _tokenizer.Tokens.Count || pos < 0) {
                return null;
            }

            return _tokenizer.Tokens[pos];
        }

        private bool InSkipList(Token token)
        {
            return _skipList.Contains(token.Type);
        }

        public void DisableNewLineIgnore()
        {
            _skipList.Remove(TokenType.NewLine);
        }

        public void EnableNewLineIgnore()
        {
            _skipList.Add(TokenType.NewLine);
        }

        public string ToDebugString()
        {
            var currentIndex = _position;

            var res = string.Empty;
            Token next;
            do {
                next = Next();
                if (next != null) {
                    res += next.ToJsonString() + "\n";
                }
            } while (next != null);

            _position = currentIndex;

            return res.Trim();
        }
    }
}