namespace src.Parser
{
    public class TokenStream
    {
        private Tokenizer.Tokenizer _tokenizer;
        private int _position = 0;

        public TokenStream(Tokenizer.Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        // todo: add option to skip useless tokens
    }
}