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

        public AstNode ParseProgram()
        {
            var program = new ProgramRaw();

            while (_stream.HasTokens()) {
                var nextNode = TryExtractVariants(new Func<AstNode>[] {
                    ParseData,
                    ParseModule,
                    ParseRoutine,
                    ParseCode,
                });

                if (nextNode != null) {
                    program.AddChild(nextNode);
                }
                else {
                    var nextToken = _stream.Next();
                    AssertTokenExist(nextToken);

                    throw SyntaxError.Make(SyntaxErrorMessages.INVALID_TOKEN(nextToken), nextToken);
                }
            }

            return program;
        }

        private AstNode ParseCode()
        {
            var nextToken = _stream.Next();
            if (!nextToken.IsKeyword(Keyword.Code)) return null;

            var code = new Code();
            _stream.Fixate();

            var children = ExtractAllChildren(new Func<AstNode>[] {
                ParseVariable,
                ParseConstant,
                ParseStatment,
            });
            code.AddChildren(children);

            nextToken = _stream.Next();

            AssertTokenExist(nextToken);
            AssertKeyword(Keyword.End, nextToken);

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

        private AstNode ParseTMP()
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

        protected static void AssertTokenExist(Token token)
        {
            if (token == null) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_EOS());
            }
        }

        protected static void AssertKeyword(string expected, Token received)
        {
            if (!received.IsKeyword(expected)) {
                throw SyntaxError.Make(SyntaxErrorMessages.UNEXPECTED_TOKEN(expected, received), received);
            }
        }
    }
}