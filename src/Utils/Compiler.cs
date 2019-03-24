using System;
using src.Interfaces;
using src.Parser;
using src.Tokenizer;

namespace src.Utils
{
    public class Compiler
    {
        private const bool Debug = true;
        private const bool Optimize = true;

        public SourceCode SourceCode { get; private set; }

        public Tokenizer.Tokenizer Tokenizer { get; private set; }
        public TokenStream TokenStream { get; private set; }

        public Parser.Parser Parser { get; private set; }
        public AstNode AstTree { get; private set; }

        public Compiler(string filepath)
        {
            Read(filepath);
        }

        public Compiler(SourceCode source)
        {
            SourceCode = source;
        }

        public string Compile()
        {
            Tokenize();
            Parse();
//            Analyze();
//            Generate();

            return "";
        }

        private void Read(string path)
        {
            SourceCode = new SourceCode(path);
        }

        public void Tokenize()
        {
            Contract.Requires(SourceCode != null);

            Tokenizer = new Tokenizer.Tokenizer(SourceCode);
            Tokenizer.Process();
            //PrintDebug(Tokenizer);

            TokenStream = new TokenStream(Tokenizer);
            //PrintDebug(TokenStream);
        }

        public void Parse()
        {
            Contract.Requires(Tokenizer != null);
            Contract.Requires(TokenStream != null);

            Parser = new Parser.Parser(TokenStream);

            AstTree = Parser.ParseProgram();
            PrintDebug(AstTree, "Parse tree");
        }

        /*public void Analyze()
        {
            var semantic = new SemanticAnalyzer(astTree);
            semantic.analyze();

            var aTree = semantic.annotatedTree;
            printDebug("Semantic tree:\n" + aTree + "\n");

            //var semantic = new SemanticAnalyzer(tree);
            //semantic.generateTables();
            //semantic.analyze();
            //printDebug("Semantic tree:\n" + tree + "\n");
        }*/

        /*public void Generate()
        {
            var codeGen = new CodeGenerator(aTree, semantic.moduleTable, semantic.dataTable);
            codeGen.generate();

            var asmCode = codeGen.assembly.ToString();
            printDebug("Generated assembly:\n" + asmCode);
        }*/

        private static void PrintDebug(IDebuggable o, string description = null)
        {
            if (description == null) {
                description = o.GetType().ToString();
            }

            PrintDebug(description + ":\n" + o.ToDebugString());
            PrintDebug("");
        }

        private static void PrintDebug(string line)
        {
            if (Debug) {
                Console.WriteLine(line);
            }
        }
    }
}