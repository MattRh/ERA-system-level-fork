using System;
using src.Parser;
using src.Tokenizer;

namespace Erasystemlevel
{
    public class Compiler
    {
        private const bool Debug = true;
        private const bool Optimize = true;

        public SourceCode SourceCode { get; private set; }
        public Tokenizer Tokenizer { get; private set; }
        public TokenStream TokenStream { get; private set; }

        /*public Tokenizer tokenizer;
        public TokenReader tokenReader;
        public AstNode astTree;*/

        public Compiler(string filepath)
        {
            Read(filepath);
        }

        public string compile()
        {
            Tokenize();
//            Parse();
//            Analyze();
//            Generate();

            return "";
        }

        private void Read(string path)
        {
            SourceCode = new SourceCode(path);
        }

        private void Tokenize()
        {
            Tokenizer = new Tokenizer(SourceCode);
            Tokenizer.Process();

            TokenStream = new TokenStream(Tokenizer);
        }

        /*private void Parse()
        {
            Parser.Parser._debug = false;
            astTree = Parser.Parser.ParseUnit(tokenReader);
            printDebug("Parse tree:\n" + astTree + "\n");
        }*/

        /*private void Analyze()
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

        /*private void Generate()
        {
            var codeGen = new CodeGenerator(aTree, semantic.moduleTable, semantic.dataTable);
            codeGen.generate();

            var asmCode = codeGen.assembly.ToString();
            printDebug("Generated assembly:\n" + asmCode);
        }*/

        private void PrintDebug(string line)
        {
            if (Debug)
            {
                Console.WriteLine(line);
            }
        }
    }
}