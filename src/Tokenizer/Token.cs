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
        Comment,
    }

    public class Token
    {
        public readonly TokenType Type;
        public readonly string Value;

        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public string ToJsonString() => $"{{{Type}: {Value}}}";

        public override string ToString() => Value;

        public bool Equals(Token obj)
        {
            return obj != null && ToJsonString().Equals(obj.ToJsonString());
        }
    }
}