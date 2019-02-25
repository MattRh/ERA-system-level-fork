using System;
using System.Collections.Generic;
using Erasystemlevel.Exception;
using src.Tokenizer;

namespace src.Parser
{
    public class Parser
    {
        private readonly TokenStream _stream;

        public Parser(TokenStream stream)
        {
            _stream = stream;
        }

        public AstNode ParseUnit()
        {
            var node = new AstNode(NodeType.Unit);

            while (_stream.HasTokens()) {
                var nextNode = TryVariants(new Func<AstNode>[] {
                    ParseData,
                    ParseModule,
                    ParseRoutine,
                    ParseCode,
                });

                if (nextNode != null) {
                    node.Children.Add(nextNode);
                }
                else {
                    var nextToken = _stream.Next();
                    throw new SyntaxError($"Failed to parse unit. Invalid token `{nextToken}` encountered", nextToken);
                }
            }

            return node;
        }

        private AstNode ParseCode()
        {
            var nextToken = _stream.Next();
            if (!nextToken.IsKeyword("code")) return null;

            var node = new AstNode(NodeType.Code);
            _stream.Fixate();

            var children = AllChildren(new Func<AstNode>[] {
                ParseVariable,
                ParseConstant,
                ParseStatment,
            });
            node.Children.AddRange(children);

            nextToken = _stream.Next();
            if (nextToken == null) {
                throw new SyntaxError("Unexpected end of stream");
            }

            if (!nextToken.IsKeyword("end")) {
                throw new SyntaxError($"Unexpected token. Expected `end` by got `{nextToken}`", nextToken);
            }

            return node;
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

        private AstNode TryVariants(IEnumerable<Func<AstNode>> variants)
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

        private IEnumerable<AstNode> AllChildren(IEnumerable<Func<AstNode>> extractors)
        {
            var found = new List<AstNode>();

            AstNode nextNode;
            do {
                nextNode = TryVariants(extractors);
                if (nextNode != null) {
                    found.Add(nextNode);
                }
            } while (nextNode != null);

            return found;
        }
    }
}