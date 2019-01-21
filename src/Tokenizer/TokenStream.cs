using System.Collections.Generic;

namespace src.Tokenizer
{
    public class TokenStream
    {
        private readonly HashSet<TokenType> _skipList = new HashSet<TokenType>()
        {
            TokenType.Comment, TokenType.NewLine
        };

        private readonly Tokenizer _tokenizer;
        private int _position = 0;

        public TokenStream(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public void Reset()
        {
            _position = 0;
        }

        public Token Next(bool movePointer = true)
        {
            var pos = _position;

            Token res;
            do
            {
                res = GetToken(pos++);
            } while (res != null && _skipList.Contains(res.Type));

            if (movePointer)
            {
                _position = pos;
            }

            return res;
        }

        public Token Previous(bool movePointer = true)
        {
            var pos = _position;

            Token res;
            do
            {
                res = GetToken(--pos);
            } while (res != null && _skipList.Contains(res.Type));

            if (movePointer)
            {
                _position = pos;
            }

            return res;
        }

        private Token GetToken(int pos)
        {
            if (pos >= _tokenizer.Tokens.Count || pos < 0)
            {
                return null;
            }

            return _tokenizer.Tokens[pos];
        }
    }
}