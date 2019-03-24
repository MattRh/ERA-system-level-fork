using NUnit.Framework;
using src.Utils;

namespace test.TestSuites
{
    public class ParserTest : BaseSuite
    {
        [Test]
        public void AnnotationsTestCase()
        {
            RunTest("parser_1");
        }

        [Test]
        public void DataTestCase()
        {
            RunTest("parser_2");
        }
        
        [Test]
        public void EmptyRoutineTestCase()
        {
            RunTest("parser_3");
        }
        
        [Test]
        public void AsmBlockTestCase()
        {
            RunTest("parser_4");
        }

        public void RunTest(string name)
        {
            LoadSourceFile(name);

            var compiler = new Compiler(Source);
            compiler.Tokenize();
            compiler.Parse();

            AssertResult(compiler.AstTree.ToDebugString());
        }
    }
}