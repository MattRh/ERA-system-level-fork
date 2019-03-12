using System;
using src.Utils;

namespace src.Tokenizer
{
    public enum TokenType
    {
        Identifier,
        Delimiter,
        Operator,
        Keyword,
        Number,
        Register,
        LineComment,
        NewLine,
    }

    public class Token
    {
        public readonly TokenType Type;
        public readonly string Value;
        public readonly Position Position;

        public Token(TokenType type, string value, (int, int) position)
        {
            this.Type = type;
            this.Value = value;

            var length = value?.Length ?? 0;
            this.Position = new Position(position.Item1, position.Item2, length);
        }

        public bool IsKeyword(string name)
        {
            return Type == TokenType.Keyword && Value == name;
        }

        public bool IsDelimiter(string name)
        {
            return Type == TokenType.Delimiter && Value == name;
        }
        
        public bool IsOperator(string name)
        {
            return Type == TokenType.Operator && Value == name;
        }

        public string ToJsonString()
        {
            return $"{{type: {Type}, pos: {Position.Start}, value: {Value}}}";
        }

        public override string ToString() => Value;

        public bool Equals(Token obj)
        {
            return obj != null && ToJsonString().Equals(obj.ToJsonString());
        }
    }
}