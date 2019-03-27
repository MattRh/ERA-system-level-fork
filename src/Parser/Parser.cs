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

        #region CoreStructures

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

        #endregion

        #region Pragma

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

        #endregion

        #region Routine

        private Routine ParseRoutine()
        {
            var attr = ParseAttribute();

            var node = (Routine) TryReadNode(Keyword.Routine, typeof(Routine));
            if (node == null) {
                return null;
            }

            if (attr == null) {
                attr = new RoutineAttribute(Keyword.Entry);
            }
            node.AddChild(attr);

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

        #endregion

        #region Variable

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
            }
            else if (t.IsDelimiter(Delimiter.BraceOpen)) {
                node = new ArrayDefinition();
                node.AddChild(id);

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

        #endregion

        private Statement ParseStatement()
        {
            var label = ParseLabel();

            var inner = TryExtractVariants(new Func<AstNode>[] {
                ParseAssemblyBlock,
                ParseExtensionStatement,
            });
            if (label == null && inner == null) {
                return null;
            }
            if (label != null && inner == null) {
                var t = NextToken();

                throw SyntaxError.Make(SyntaxErrorMessages.STATEMENT_EXPECTED, t);
            }

            var node = new Statement();

            if (label != null) {
                node.AddChild(node);
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

        #region Assembly

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

            var reg0 = ParseRegister();
            node.AddChild(reg0);

            var t = NextToken();
            AssertKeyword(Keyword.Goto, t);

            var reg1 = ParseRegister();
            node.AddChild(reg1);

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

        #endregion

        private ExtensionStatement ParseExtensionStatement()
        {
            var node = TryExtractVariants(new Func<ExtensionStatement>[] {
                ParseIf,
                ParseLoop,
                ParseBreak,
                ParseGoto,
                ParseCall, // Should be before swap and assignment
                ParseSwap,
                ParseAssignment,
            });

            return (ExtensionStatement) node;
        }

        #region Loop

        private Loop ParseLoop()
        {
            var node = TryExtractVariants(new Func<AstNode>[] {
                ParseForLoop,
                ParseWhileLoop,
                ParseInfiniteLoop,
            });

            return (Loop) node;
        }

        private ForLoop ParseForLoop()
        {
            var node = TryReadNode(Keyword.For, typeof(ForLoop));
            if (node == null) {
                return null;
            }

            var id = ParseIdentifier();
            node.AddChild(id);

            AstNode ParsePossibleExpression(string keyword)
            {
                var t = NextToken();
                if (!t.IsKeyword(keyword)) {
                    _stream.Previous();

                    return new EmptyExpression();
                }

                return ParseExpression();
            }

            node.AddChild(ParsePossibleExpression(Keyword.From));
            node.AddChild(ParsePossibleExpression(Keyword.To));
            node.AddChild(ParsePossibleExpression(Keyword.Step));

            node.AddChild(ParseLoopBody());

            return (ForLoop) node;
        }

        private WhileLoop ParseWhileLoop()
        {
            var node = TryReadNode(Keyword.For, typeof(ForLoop));
            if (node == null) {
                return null;
            }

            node.AddChild(ParseExpression());
            node.AddChild(ParseLoopBody());

            return (WhileLoop) node;
        }

        private InfiniteLoop ParseInfiniteLoop()
        {
            var body = ParseLoopBody(false);
            if (body == null) {
                return null;
            }

            var node = new InfiniteLoop();
            node.AddChild(body);

            return node;
        }

        private LoopBody ParseLoopBody(bool assert = true)
        {
            var node = TryReadNode(Keyword.Loop, typeof(LoopBody));
            if (node == null) {
                if (assert) {
                    // todo: throw
                }

                return null;
            }

            var block = ParseBlockBody();
            node.AddChildren(block.Children);

            ValidateBlockEnd(node);

            return (LoopBody) node;
        }

        #endregion

        private BlockBody ParseBlockBody()
        {
            var node = new BlockBody();

            var statements = ExtractAllChildren(new Func<Statement>[] {
                ParseStatement
            });

            node.AddChildren(statements);

            return node;
        }

        private BreakStatement ParseBreak()
        {
            var node = TryReadNode(Keyword.Break, typeof(BreakStatement));
            if (node == null) {
                return null;
            }

            var t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            return (BreakStatement) node;
        }

        private GoToStatement ParseGoto()
        {
            var node = TryReadNode(Keyword.Break, typeof(GoToStatement));
            if (node == null) {
                return null;
            }

            var id = ParseIdentifier();
            node.AddChild(id);

            var t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            return (GoToStatement) node;
        }

        private Assignment ParseAssignment()
        {
            var tx = _stream.Fixate();

            var arg0 = ParsePrimary();
            if (arg0 == null) {
                return null;
            }

            var t = NextToken();
            if (!t.IsOperator(Operator.Assign)) {
                _stream.Rollback(tx); // Undo operator and 'primary' read

                return null;
            }

            var node = new Assignment();
            node.AddChild(arg0);

            var arg1 = ParseExpression();
            node.AddChild(arg1);

            t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            return node;
        }

        private Swap ParseSwap()
        {
            var tx = _stream.Fixate();

            var arg0 = ParsePrimary();
            if (arg0 == null) {
                return null;
            }

            var t = NextToken();
            if (!t.IsOperator(Operator.AssignSwap)) {
                _stream.Rollback(tx); // Undo operator and 'primary' read

                return null;
            }

            var node = new Swap();
            node.AddChild(arg0);

            var arg1 = ParsePrimary();
            if (arg1 == null) {
                throw SyntaxError.Make(SyntaxErrorMessages.PRIMARY_EXPECTED, t);
            }
            node.AddChild(arg1);

            t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            return node;
        }

        private IfStatement ParseIf()
        {
            var node = TryReadNode(Keyword.If, typeof(IfStatement));
            if (node == null) {
                return null;
            }

            var expr = ParseExpression();
            node.AddChild(expr);

            var t = NextToken();
            AssertKeyword(Keyword.Do, t);

            var body = ParseBlockBody();
            node.AddChild(body);

            t = NextToken(false);
            if (t.IsKeyword(Keyword.Else)) {
                _stream.Next();

                body = ParseBlockBody();
                node.AddChild(body);
            }

            ValidateBlockEnd(node);

            return (IfStatement) node;
        }

        private RoutineCall ParseCall()
        {
            var id = ParseIdentifier(false);
            if (id == null) {
                return null;
            }

            var node = new RoutineCall();
            node.AddChild(id);

            var t = NextToken(false);
            if (t.IsDelimiter(Delimiter.Dot)) {
                _stream.Next();
                
                id = ParseIdentifier();
                node.AddChild(id);
            }
            else if (!t.IsDelimiter(Delimiter.ParenthesisOpen)) {
                _stream.Previous(); // Undo identifier read

                return null;
            }

            var args = ParseCallArgs();
            node.AddChild(args);

            t = NextToken();
            AssertDelimiter(Delimiter.Semicolon, t);

            return node;
        }

        private CallArgs ParseCallArgs()
        {
            var t = NextToken();
            AssertDelimiter(Delimiter.ParenthesisOpen, t);

            var node = new CallArgs();

            t = NextToken(false);
            if (t.IsDelimiter(Delimiter.ParenthesisClose)) {
                _stream.Next();

                return node;
            }

            do {
                var expr = ParseExpression();

                node.AddChild(expr);
            } while (NextToken().IsDelimiter(Delimiter.Comma));
            _stream.Previous(); // Not comma encountered

            t = NextToken();
            AssertDelimiter(Delimiter.ParenthesisClose, t);

            return node;
        }

        private Expression ParseExpression()
        {
            var node = new Expression();

            var operand0 = ParseOperand();
            node.AddChild(operand0);

            var op = ParseOperator();
            if (op != null) {
                node.AddChild(op);

                var operand1 = ParseOperand();
                node.AddChild(operand1);
            }

            return node;
        }

        private ExpressionOperator ParseOperator()
        {
            var compOp = ParseCompOperator();
            if (compOp != null) {
                return compOp;
            }

            var allowedOps = new HashSet<string>() {
                Operator.Plus,
                Operator.Minus,
                Operator.Asterisk,
                Operator.Ampersand,
                Operator.Line,
                Operator.Hat,
                //Operator.Question,
            };

            var t = NextToken();
            if (!allowedOps.Contains(t.Value)) {
                _stream.Previous();

                return null;
            }

            var node = new ExpressionOperator(t);

            return node;
        }

        private CompOperator ParseCompOperator()
        {
            var allowedOps = new HashSet<string>() {
                Operator.Equal,
                Operator.NotEqual,
                Operator.Less,
                Operator.Greater,
            };

            var t = NextToken();
            if (!allowedOps.Contains(t.Value)) {
                _stream.Previous();

                return null;
            }

            var node = new CompOperator(t);

            return node;
        }

        private Operand ParseOperand()
        {
            var operand = TryExtractVariants(new Func<AstNode>[] {
                ParseReceiver,
                ParseReference,
                () => ParseLiteral(false),
            });
            if (operand == null) {
                var t = NextToken();

                throw SyntaxError.Make(SyntaxErrorMessages.OPERAND_EXPECTED, t);
            }

            var node = new Operand();
            node.AddChild(operand);

            return node;
        }

        private Primary ParsePrimary()
        {
            var primary = TryExtractVariants(new Func<AstNode>[] {
                ParseReceiver,
                ParseExplicitAddress, // should be before ParseDereference
                ParseDereference,
            });
            if (primary == null) {
                return null;
            }

            var node = new Primary();
            node.AddChild(primary);

            return node;
        }

        private AstNode ParseReceiver()
        {
            var node = TryExtractVariants(new Func<AstNode>[] {
                ParseArrayAccess, // should be before ParseIdentifier
                () => ParseIdentifier(false),
                () => ParseRegister(false),
            });

            return node;
        }

        private ArrayAccess ParseArrayAccess()
        {
            var tx = _stream.Fixate();

            var id = ParseIdentifier(false);
            if (id == null) {
                return null;
            }

            var t = NextToken();
            if (!t.IsDelimiter(Delimiter.BraceOpen)) {
                _stream.Rollback(tx); // Undo brace and identifier read

                return null;
            }

            var node = new ArrayAccess();
            node.AddChild(id);

            var expr = ParseExpression();
            node.AddChild(expr);

            t = NextToken();
            AssertDelimiter(Delimiter.BraceClose, t);

            node.PropagatePosition(t);

            return node;
        }

        private Reference ParseReference()
        {
            var t = NextToken();
            if (!t.IsOperator(Operator.Ampersand)) {
                _stream.Previous();

                return null;
            }

            var node = new Reference(t);

            var id = ParseIdentifier();
            node.AddChild(id);

            return node;
        }

        private Dereference ParseDereference()
        {
            var t = NextToken();
            if (!t.IsOperator(Operator.Asterisk)) {
                _stream.Previous();

                return null;
            }

            var node = new Dereference(t);

            var reg = ParseRegister(false);
            if (reg != null) {
                node.AddChild(reg);
            }
            else {
                var id = ParseIdentifier();
                node.AddChild(id);
            }

            return node;
        }

        private ExplicitAddress ParseExplicitAddress()
        {
            var t = NextToken();
            if (!t.IsOperator(Operator.Asterisk)) {
                _stream.Previous();

                return null;
            }

            var node = new ExplicitAddress(t);

            var num = ParseLiteral(false);
            if (num == null) {
                _stream.Previous(); // Undo asterisk read

                return null;
            }

            node.AddChild(num);

            return node;
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