using System;
using System.Collections.Generic;
using src.Exceptions;
using src.Parser.Nodes;
using src.Tokenizer;

namespace src.Parser
{
    public class Parser : BaseParser
    {
        public Parser(TokenStream stream) : base(stream)
        {
        }

        public ProgramRaw ParseProgram()
        {
            var program = new ProgramRaw();

            while (_stream.HasTokens()) {
                var nextNode = TryExtractVariants(new Func<AstNode>[] {
                    ParseAnnotation,
                    ParseData,
                    ParseModule,
                    ParseRoutine,
                    ParseCode,
                });

                if (nextNode != null) {
                    program.AddChild(nextNode);
                }
                else {
                    throw SyntaxError.Make(SyntaxErrorMessages.INVALID_TOKEN, NextToken());
                }
            }

            return program;
        }

        private Annotation ParseAnnotation()
        {
            var annotation = (Annotation) TryReadNode(Keyword.Pragma, typeof(Annotation));
            if (annotation == null) {
                return null;
            }

            do {
                var id = ParseIdentifier();
                if (id == null) {
                    throw SyntaxError.Make(SyntaxErrorMessages.IDENTIFIER_EXPECTED, NextToken());
                }
                // todo
            } while (NextToken().IsDelimiter(Delimiter.Comma));

            var t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            annotation.PropagatePosition(t);

            return annotation;
        }

        private AstNode ParsePragmaDeclaration()
        {
            return null;
        }

        private AstNode ParseCode()
        {
            var code = (Code) TryReadNode(Keyword.Code, typeof(Code));
            if (code == null) {
                return null;
            }

            var children = ExtractAllChildren(new Func<AstNode>[] {
                ParseVarDeclaration,
                ParseStatment,
            });
            code.AddChildren(children);

            var t = NextToken();
            AssertKeyword(Keyword.End, t);

            code.PropagatePosition(t);

            return code;
        }

        private AstNode ParseData()
        {
            return null;
        }

        private AstNode ParseModule()
        {
            return null;
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

        private Identifier ParseIdentifier()
        {
            return null;
        }
    }

    public class BaseParser
    {
        protected readonly TokenStream _stream;

        public BaseParser(TokenStream stream)
        {
            _stream = stream;
        }

        public Token NextToken(bool fixate = true)
        {
            var t = _stream.Next();
            AssertTokenExist(t);

            if (fixate) {
                _stream.Fixate();
            }

            return t;
        }

        protected AstNode TryExtractVariants(IEnumerable<Func<AstNode>> variants)
        {
            foreach (var parse in variants) {
                var res = parse();
                if (res == null) {
                    _stream.Rollback();
                }
                else {
                    _stream.Fixate();
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
            var nextToken = _stream.Next();
            if (nextToken == null || !nextToken.IsKeyword(expectedKey)) {
                return null;
            }

            var node = (AstNode) Activator.CreateInstance(nodeType, new object[] {nextToken});
            _stream.Fixate();

            return node;
        }

        protected static void AssertTokenExist(Token token)
        {
            if (token == null) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_EOS);
            }
        }

        protected static void AssertKeyword(string expected, Token received)
        {
            if (!received.IsKeyword(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN, expected, received);
            }
        }

        protected static void AssertDelimiter(string expected, Token received)
        {
            if (!received.IsDelimiter(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN, expected, received);
            }
        }

        protected static void AssertOperator(string expected, Token received)
        {
            if (!received.IsOperator(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN, expected, received);
            }
        }
    }
}