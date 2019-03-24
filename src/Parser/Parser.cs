using System;
using System.Collections.Generic;
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

            var t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            node.PropagatePosition(t);

            return node;
        }

        private PragmaDeclaration ParsePragmaDeclaration()
        {
            var node = new PragmaDeclaration();

            var id = ParseIdentifier();
            node.AddChild(id);

            AssertDelimiter(Delimiter.ParenthesisOpen, NextToken());

            if (!NextToken(false).IsDelimiter(Delimiter.ParenthesisClose)) {
                var text = ParsePragmaText();

                node.AddChild(text);
            }

            var t = NextToken();
            AssertDelimiter(Delimiter.ParenthesisClose, t);

            node.PropagatePosition(t);

            return node;
        }

        private PragmaText ParsePragmaText()
        {
            var tmp = new PragmaText(); // contains only position
            var text = string.Empty;

            var next = NextToken();
            while (!next.IsDelimiter(Delimiter.ParenthesisClose)) {
                text += next.Value;
                tmp.PropagatePosition(next);

                next = NextToken();
            }
            _stream.Previous(); // Parenthesis encountered

            if (text == string.Empty) {
                return null;
            }

            var node = new PragmaText(text);
            node.PropagatePosition(tmp.Position);

            return node;
        }

        private Code ParseCode()
        {
            var node = (Code) TryReadNode(Keyword.Code, typeof(Code));
            if (node == null) {
                return null;
            }

            var children = ExtractAllChildren(new Func<AstNode>[] {
                ParseVarDeclaration,
                ParseStatement,
            });
            node.AddChildren(children);

            ValidateBlockEnd(node);

            return node;
        }

        private Data ParseData()
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

        private Module ParseModule()
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

        private Routine ParseRoutine()
        {
            var attr = ParseAttribute();

            var node = (Routine) TryReadNode(Keyword.Routine, typeof(Routine));
            if (node == null) {
                return null;
            }

            if (attr != null) {
                node.AddChild(attr);
            }

            var id = ParseIdentifier();
            node.AddChild(id);

            var parameters = ParseParameters();
            if (parameters != null) {
                node.AddChild(parameters);
            }

            var results = ParseResults();
            if (results != null) {
                node.AddChild(results);
            }

            var next = NextToken();
            if (next.IsDelimiter(Delimiter.Semicolon)) {
                node.PropagatePosition(next);

                return node;
            }
            _stream.Previous(); // not semicolon encountered

            var body = ParseRoutineBody();
            node.AddChild(body);

            return node;
        }

        private RoutineAttribute ParseAttribute()
        {
            var node = (RoutineAttribute) TryReadNode(Keyword.Start, typeof(RoutineAttribute));
            if (node == null) {
                node = (RoutineAttribute) TryReadNode(Keyword.Entry, typeof(RoutineAttribute));
            }

            return node;
        }

        private Parameters ParseParameters()
        {
            var t = NextToken();
            if (!t.IsDelimiter(Delimiter.ParenthesisOpen)) {
                _stream.Previous(); // Not open parenthesis encountered

                return null;
            }

            var node = new Parameters();
            do {
                var parameter = ParseParameter();

                node.AddChild(parameter);
            } while (NextToken().IsDelimiter(Delimiter.Comma));
            _stream.Previous(); // Not comma encountered

            t = NextToken();
            AssertDelimiter(Delimiter.ParenthesisClose, t);

            return null;
        }

        private Parameter ParseParameter()
        {
            var node = new Parameter();

            var register = ParseRegister(false);
            if (register != null) {
                node.AddChild(register);
            }
            else {
                var type = ParseType();
                var id = ParseIdentifier();

                node.AddChild(type);
                node.AddChild(id);
            }

            return node;
        }

        private Results ParseResults()
        {
            var t = NextToken();
            if (!t.IsDelimiter(Delimiter.Colon)) {
                _stream.Previous(); // Not colon encountered

                return null;
            }

            var node = new Results();
            do {
                var register = ParseRegister();

                node.AddChild(register);
            } while (NextToken().IsDelimiter(Delimiter.Comma));
            _stream.Previous(); // Not comma encountered

            return node;
        }

        private RoutineBody ParseRoutineBody()
        {
            var t = NextToken();
            AssertKeyword(Keyword.Do, t);

            var node = new RoutineBody();

            var children = ExtractAllChildren(new Func<AstNode>[] {
                ParseVarDeclaration,
                ParseStatement,
            });
            node.AddChildren(children);

            ValidateBlockEnd(node);

            return node;
        }

        private Statement ParseStatement()
        {
            var label = ParseLabel();

            var node = new Statement();

            if (label != null) {
                node.AddChild(node);
            }

            var inner = ParseAssemblyBlock();
            if (inner == null) {
                inner = ParseExtensionStament();
            }

            node.AddChild(inner);

            return node;
        }

        private Label ParseLabel()
        {
            var t = NextToken();
            if (!t.IsOperator(Operator.Less)) {
                _stream.Previous();

                return null;
            }

            var node = new Label();

            var id = ParseIdentifier();
            node.AddChild(id);

            t = NextToken();
            AssertOperator(Operator.Greater, t);

            return node;
        }

        private AstNode ParseVarDeclaration()
        {
            return null;
        }

        private AstNode ParseVariable()
        {
            return null;
        }

        private AstNode ParseType()
        {
            return null;
        }

        private AstNode ParseVarDefinition()
        {
            return null;
        }

        private AstNode ParseConstant()
        {
            return null;
        }

        private AstNode ParseConstDefinition()
        {
            return null;
        }

        private AstNode ParseAssemblyBlock()
        {
            return null;
        }

        private AstNode ParseAssemblyStatement()
        {
            return null;
        }

        private AstNode ParseExtensionStament()
        {
            return null;
        }

        private Register ParseRegister(bool assert = true)
        {
            var node = ParseBasicNode(typeof(Register), AssertRegister, assert);

            return (Register) node;
        }

        private Literal ParseLiteral(bool assert = true)
        {
            var node = ParseBasicNode(typeof(Literal), AssertLiteral, assert);

            return (Literal) node;
        }

        private Identifier ParseIdentifier(bool assert = true)
        {
            var node = ParseBasicNode(typeof(Identifier), AssertIdentifier, assert);

            return (Identifier) node;
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