using System.Collections.Generic;

namespace src.Tokenizer
{
    public class TokenStream
    {
        private readonly HashSet<TokenType> _skipList = new HashSet<TokenType>() {
            TokenType.LineComment, TokenType.NewLine
        };

        private readonly Tokenizer _tokenizer;
        private int _position = 0;
        private int _checkpoint = 0;

        public TokenStream(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public void Reset()
        {
            _position = 0;
            _checkpoint = 0;
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

        public bool HasTokens()
        {
            return Next(false) != null;
        }

        public void Rollback()
        {
            _position = _checkpoint;
        }

        public void Fixate()
        {
            _checkpoint = _position;
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
    }
}