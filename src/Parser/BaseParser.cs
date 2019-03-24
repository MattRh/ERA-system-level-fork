using System;
using System.Collections.Generic;
using src.Exceptions;
using src.Tokenizer;
using src.Utils;

namespace src.Parser
{
    public class BaseParser
    {
        protected readonly TokenStream _stream;

        public BaseParser(TokenStream stream)
        {
            _stream = stream;
        }

        public Token NextToken(bool movePointer = true, bool fixate = false)
        {
            var t = _stream.Next(movePointer);
            AssertTokenExist(t);

            //if (fixate) {
            //    _stream.Fixate();
            //}

            return t;
        }

        protected AstNode TryExtractVariants(IEnumerable<Func<AstNode>> variants)
        {
            foreach (var parse in variants) {
                var res = parse();
                if (res == null) {
                    //_stream.Rollback();
                }
                else {
                    //_stream.Fixate();
                    return res;
                }
            }

            return null;
        }

        protected IEnumerable<AstNode> ExtractAllChildren(IEnumerable<Func<AstNode>> extractors)
        {
            var found = new List<AstNode>();

            AstNode nextNode;
            do {
                nextNode = TryExtractVariants(extractors);
                if (nextNode != null) {
                    found.Add(nextNode);
                }
            } while (nextNode != null);

            return found;
        }

        protected AstNode TryReadNode(string expectedKey, System.Type nodeType)
        {
            var nextToken = _stream.Next(false);
            if (nextToken == null || !nextToken.IsKeyword(expectedKey)) {
                return null;
            }
            _stream.Next(); // instead of using fixate
            //_stream.Fixate();

            var node = (AstNode) Activator.CreateInstance(nodeType, new object[] {nextToken});

            return node;
        }

        protected void ValidateBlockEnd(AstNode node)
        {
            var t = NextToken();
            AssertKeyword(Keyword.End, t);

            node.PropagatePosition(t);
        }

        protected AstNode ParseBasicNode(System.Type nodeType, Action<Token> assertion, bool assert = true)
        {
            var t = NextToken();

            try {
                assertion.Invoke(t);
            }
            catch (SyntaxError) {
                if (assert) {
                    throw;
                }

                _stream.Previous();
                return null;
            }

            var node = (AstNode) Activator.CreateInstance(nodeType, new object[] {t});

            return node;
        }

        protected void AssertTokenExist(Token token)
        {
            if (token == null) {
                var last = _stream.Last();

                // Next char position
                var start = last.Position.Start.ToTuple();
                var end = last.Position.End.ToTuple();
                start.Item2 += 1;
                end.Item2 += 1;
                var prettyPos = new Position(start, end);

                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_EOS(), prettyPos);
            }
        }

        protected void AssertKeyword(string expected, Token received)
        {
            if (!received.IsKeyword(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN, expected, received);
            }
        }

        protected void AssertDelimiter(string expected, Token received)
        {
            if (!received.IsDelimiter(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN, expected, received);
            }
        }

        protected void AssertOperator(string expected, Token received)
        {
            if (!received.IsOperator(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN, expected, received);
            }
        }

        protected void AssertIdentifier(Token received)
        {
            if (received.Type != TokenType.Identifier) {
                throw SyntaxError.Make(SyntaxErrorMessages.IDENTIFIER_EXPECTED, received);
            }
        }

        protected void AssertLiteral(Token received)
        {
            if (received.Type != TokenType.Number) {
                throw SyntaxError.Make(SyntaxErrorMessages.LITERAL_EXPECTED, received);
            }
        }

        protected void AssertRegister(Token received)
        {
            if (received.Type != TokenType.Register) {
                throw SyntaxError.Make(SyntaxErrorMessages.REGISTER_EXPECTED, received);
            }
        }
    }
}