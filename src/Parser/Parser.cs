using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using src.Exceptions;
using src.Parser.Nodes;
using src.Tokenizer;
using src.Utils;

namespace src.Parser
{
    public class Parser : BaseParser
    {
        public Parser(TokenStream stream) : base(stream)
        {
        }

        public ProgramRaw ParseProgram()
        {
            var node = new ProgramRaw();

            while (_stream.HasTokens()) {
                var nextNode = TryExtractVariants(new Func<AstNode>[] {
                    ParseAnnotation,
                    ParseData,
                    ParseModule,
                    ParseRoutine,
                    ParseCode,
                });

                if (nextNode != null) {
                    node.AddChild(nextNode);
                }
                else {
                    throw SyntaxError.Make(SyntaxErrorMessages.INVALID_TOKEN, NextToken());
                }
            }

            return node;
        }

        private Annotation ParseAnnotation()
        {
            var node = (Annotation) TryReadNode(Keyword.Pragma, typeof(Annotation));
            if (node == null) {
                return null;
            }

            do {
                var pragma = ParsePragmaDeclaration();

                node.AddChild(pragma);
            } while (NextToken().IsDelimiter(Delimiter.Comma));
            _stream.Previous(); // Not comma encountered

            ValidateBlockEnd(node);

            return node;
        }

        private AstNode ParsePragmaDeclaration()
        {
            var node = new PragmaDeclaration();

            var id = ParseIdentifier();
            node.AddChild(id);

            AssertDelimiter(Delimiter.ParenthesisOpen, NextToken());

            if (!NextToken(false).IsDelimiter(Delimiter.ParenthesisClose)) {
                var text = ParsePragmaText();

                node.AddChild(text);
            }

            ValidateBlockEnd(node);

            return node;
        }

        private AstNode ParsePragmaText()
        {
            var tmp = new PragmaText();
            var text = string.Empty;

            var next = NextToken();
            while (!next.IsDelimiter(Delimiter.ParenthesisClose)) {
                text += next.Value;
                tmp.PropagatePosition(next);

                next = NextToken();
            }
            _stream.Previous(); // Parenthesis encountered

            var node = new PragmaText(text);
            node.PropagatePosition(tmp.Position);

            return node;
        }

        private AstNode ParseCode()
        {
            var node = (Code) TryReadNode(Keyword.Code, typeof(Code));
            if (node == null) {
                return null;
            }

            var children = ExtractAllChildren(new Func<AstNode>[] {
                ParseVarDeclaration,
                ParseStatment,
            });
            node.AddChildren(children);

            ValidateBlockEnd(node);

            return node;
        }

        private AstNode ParseData()
        {
            var node = (Data) TryReadNode(Keyword.Data, typeof(Data));
            if (node == null) {
                return null;
            }

            var id = ParseIdentifier();
            node.AddChild(id);

            var next = ParseLiteral(false);
            if (next != null) {
                node.AddChild(next);
            }

            while (NextToken().IsDelimiter(Delimiter.Comma)) {
                next = ParseLiteral();
                node.AddChild(next);
            }
            _stream.Previous(); // Not comma encountered

            ValidateBlockEnd(node);

            return node;
        }

        private AstNode ParseModule()
        {
            var node = (Module) TryReadNode(Keyword.Module, typeof(Module));
            if (node == null) {
                return null;
            }

            var id = ParseIdentifier();
            node.AddChild(id);

            var children = ExtractAllChildren(new Func<AstNode>[] {
                ParseVarDeclaration,
                ParseRoutine,
            });
            node.AddChildren(children);

            ValidateBlockEnd(node);

            return node;
        }

        private AstNode ParseRoutine()
        {
            return null;
        }

        private AstNode ParseVarDeclaration()
        {
            return null;
        }

        private AstNode ParseVariable()
        {
            return null;
        }

        private AstNode ParseConstant()
        {
            return null;
        }

        private AstNode ParseStatment()
        {
            return null;
        }

        private Literal ParseLiteral(bool assert = true)
        {
            var t = NextToken();

            try {
                AssertLiteral(t);
            }
            catch (SyntaxError) {
                if (assert) {
                    throw;
                }

                _stream.Previous();
                return null;
            }

            var node = new Literal(t);

            return node;
        }

        private Identifier ParseIdentifier(bool assert = true)
        {
            var t = NextToken();

            try {
                AssetIdentifier(t);
            }
            catch (SyntaxError) {
                if (assert) {
                    throw;
                }

                _stream.Previous();
                return null;
            }

            var node = new Identifier(t);

            return node;
        }
    }

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

        protected AstNode TryReadNode(string expectedKey, Type nodeType)
        {
            var nextToken = _stream.Next(false);
            if (nextToken == null || !nextToken.IsKeyword(expectedKey)) {
                return null;
            }
            _stream.Next(); // instead of using fixate

            var node = (AstNode) Activator.CreateInstance(nodeType, new object[] {nextToken});
            //_stream.Fixate();

            return node;
        }

        protected void ValidateBlockEnd(AstNode node)
        {
            var t = NextToken();
            AssertKeyword(Keyword.End, t);

            node.PropagatePosition(t);
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

        protected void AssetIdentifier(Token received)
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
    }
}