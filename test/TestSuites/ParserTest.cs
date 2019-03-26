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
        
        [Test]
        public void BasicAssignmentTestCase()
        {
            RunTest("parser_5");
        }
        
        [Test]
        public void ArrayElAssignmentTestCase()
        {
            RunTest("parser_6");
        }
        
        [Test]
        public void ComplexAssignmentTestCase()
        {
            RunTest("parser_7");
        }
        
        [Test]
        public void ComplexRoutineTestCase()
        {
            RunTest("parser_8");
        }
        
        [Test]
        public void ArrayDeclarationTestCase()
        {
            RunTest("parser_9");
        }
        
        [Test]
        public void VariableDeclarationTestCase()
        {
            RunTest("parser_10");
        }
        
        [Test]
        public void ModuleRoutineTestCase()
        {
            RunTest("parser_11");
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