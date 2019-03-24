using System;
using System.Collections.Generic;
using System.Text;
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

            return node;
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

            AstNode inner = ParseAssemblyBlock();
            if (inner == null) {
                inner = ParseExtensionStatement();
            }
            if (label == null && inner == null) {
                return null;
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

        private VarDeclaration ParseVarDeclaration()
        {
            AstNode child = ParseConstant();
            if (child == null) {
                child = ParseVariable();
            }
            if (child == null) {
                return null;
            }

            var node = new VarDeclaration();
            node.AddChild(child);

            return node;
        }

        private Variable ParseVariable()
        {
            var type = ParseType(false);
            if (type == null) {
                return null;
            }

            var node = new Variable();
            node.AddChild(type);

            do {
                var definition = ParseVarDefinition();

                node.AddChild(definition);
            } while (NextToken().IsDelimiter(Delimiter.Comma));
            _stream.Previous(); // Not comma encountered

            var t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            return node;
        }

        private VarType ParseType(bool assert = true)
        {
            var t = NextToken();

            var allowedTypes = new HashSet<string>() {Keyword.Int, Keyword.Short, Keyword.Byte};

            if (t.Type != TokenType.Keyword || !allowedTypes.Contains(t.Value)) {
                _stream.Previous();
                
                if (assert) {
                    throw SyntaxError.Make(SyntaxErrorMessages.TYPE_EXPECTED, t);
                }

                return null;
            }

            var node = new VarType(t);

            return node;
        }

        private VarDefinition ParseVarDefinition()
        {
            var id = ParseIdentifier();

            var node = new VarDefinition();
            node.AddChild(id);

            var t = NextToken();
            if (t.IsOperator(Operator.Assign)) {
                var expr = ParseExpression();

                node.AddChild(expr);
            } else if (t.IsDelimiter(Delimiter.BraceOpen)) {
                node.IsArray = true;

                var expr = ParseExpression();
                node.AddChild(expr);
                
                t = NextToken();
                AssertDelimiter(Delimiter.BraceClose, t);

                node.PropagatePosition(t);
            }
            else {
                _stream.Previous();
            }

            return node;
        }

        private Constant ParseConstant()
        {
            var node = (Constant) TryReadNode(Keyword.Const, typeof(Constant));
            if (node == null) {
                return null;
            }
            
            do {
                var definition = ParseConstDefinition();

                node.AddChild(definition);
            } while (NextToken().IsDelimiter(Delimiter.Comma));
            _stream.Previous(); // Not comma encountered

            var t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);
            
            return node;
        }

        private ConstDefinition ParseConstDefinition()
        {
            var id = ParseIdentifier();

            var node = new ConstDefinition();
            node.AddChild(id);

            var t = NextToken();
            AssertOperator(Operator.Equal, t);

            var expr = ParseExpression();
            node.AddChild(expr);
            
            return node;
        }

        private AssemblyBlock ParseAssemblyBlock()
        {
            var node = (AssemblyBlock) TryReadNode(Keyword.Asm, typeof(AssemblyBlock));
            if (node == null) {
                return null;
            }

            var statement = ParseAssemblyStatement();
            node.AddChild(statement);

            if (NextToken(false).IsDelimiter(Delimiter.Semicolon)) {
                _stream.Next();

                return node; // End of inline block
            }

            while (NextToken().IsDelimiter(Delimiter.Comma)) {
                statement = ParseAssemblyStatement();

                node.AddChild(statement);
            }
            _stream.Previous(); // Not comma encountered

            ValidateBlockEnd(node);

            return node;
        }

        private AssemblyStatement ParseAssemblyStatement()
        {
            AssemblyStatement node;

            node = ParseAssemblyMetaOperation();
            if (node != null) {
                return node;
            }

            node = ParseAssemblyCondition();
            if (node != null) {
                return node;
            }

            var format = ParseAssemblyOperationFormat();

            node = ParseAssemblyOperation();
            if (format != null) {
                node.AddChild(format);
            }

            return node;
        }

        private AssemblyMetaOperation ParseAssemblyMetaOperation()
        {
            var node = (AssemblyMetaOperation) TryReadNode(Keyword.Skip, typeof(AssemblyMetaOperation));
            if (node != null) {
                node = (AssemblyMetaOperation) TryReadNode(Keyword.Stop, typeof(AssemblyMetaOperation));
            }

            return node;
        }

        private AssemblyCondition ParseAssemblyCondition()
        {
            var node = (AssemblyCondition) TryReadNode(Keyword.If, typeof(AssemblyCondition));
            if (node == null) {
                return null;
            }

            var reg = ParseRegister();
            node.AddChild(reg);

            var t = NextToken();
            AssertKeyword(Keyword.Goto, t);

            node.PropagatePosition(t);

            var expr = ParseExpression();
            if (expr != null) {
                node.AddChild(expr);
            }
            else {
                reg = ParseRegister();
                node.AddChild(reg);
            }

            return node;
        }

        private AssemblyOperationFormat ParseAssemblyOperationFormat()
        {
            var kw = NextToken();
            if (!kw.IsKeyword(Keyword.Format)) {
                _stream.Previous();

                return null;
            }

            var allowedVals = new HashSet<string>() {"8", "16", "32"};

            var t = NextToken();
            AssertLiteral(t);
            if (!allowedVals.Contains(t.Value)) {
                throw SyntaxError.Make(SyntaxErrorMessages.INVALID_TOKEN, t);
            }

            var node = new AssemblyOperationFormat(t);
            node.PropagatePosition(kw.Position);

            return node;
        }

        private AssemblyOperation ParseAssemblyOperation()
        {
            var asterisk = NextToken(false);
            if (asterisk.IsOperator(Operator.Asterisk)) {
                _stream.Next();
            }
            else {
                asterisk = null;
            }

            var reg = ParseRegister();

            var allowedOps = new HashSet<string>() {
                Operator.Assign,
                Operator.AssignPlus,
                Operator.AssignMinus,
                Operator.AssignShiftRight,
                Operator.AssignShiftLeft,
                Operator.AssignOr,
                Operator.AssignAnd,
                Operator.AssignXor,
                Operator.AssignLess,
                Operator.AssignGreater,
                Operator.AssignCond
            };
            var op = NextToken();
            if (op.Type != TokenType.Operator || !allowedOps.Contains(op.Value)) {
                throw SyntaxError.Make(SyntaxErrorMessages.INVALID_TOKEN, op);
            }

            var node = new AssemblyOperation(op);

            if (asterisk != null) {
                var deref = new Dereference(asterisk);
                deref.AddChild(reg);

                node.AddChild(deref);
            }
            else {
                node.AddChild(reg);

                asterisk = NextToken(false);
                if (asterisk.IsOperator(Operator.Asterisk)) {
                    _stream.Next();
                }
                else {
                    asterisk = null;
                }
            }

            // If we have two dereferences in the statement ParseRegister will fail
            reg = ParseRegister();

            if (asterisk != null) {
                var deref = new Dereference(asterisk);
                deref.AddChild(reg);

                node.AddChild(deref);
            }
            else {
                node.AddChild(reg);
            }

            if (!op.IsOperator(Operator.Assign) && asterisk != null) {
                throw SyntaxError.Make(SyntaxErrorMessages.INVALID_ASM_DEREFERENCE_USE(), node.Position);
            }

            return node;
        }

        private AstNode ParseExtensionStatement()
        {
            return null;
        }

        private AstNode ParseExpression()
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
}