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
            var unit = new AstNode(NodeType.Unit);

            while (_stream.HasTokens())
            {
                var next = TryVariants(new Func<AstNode>[]
                {
                    ParseData,
                    ParseModule,
                    ParseRoutine,
                    ParseCode,
                });

                if (next != null)
                {
                    unit.Children.Add(next);
                }
                else
                {
                    throw new SyntaxError("Failed to parse unit");
                }
            }

            return unit;
        }

        internal AstNode ParseCode()
        {
            return null;
        }

        internal AstNode ParseData()
        {
            return null;
        }

        internal AstNode ParseModule()
        {
            return null;
        }

        internal AstNode ParseRoutine()
        {
            return null;
        }

        internal AstNode ParseTMP()
        {
            return null;
        }

        private AstNode TryVariants(IEnumerable<Func<AstNode>> variants)
        {
            foreach (var parse in variants)
            {
                var res = parse();
                if (res != null)
                {
                    _stream.Fixate();
                    return res;
                }
            }

            return null;
        }
    }
}