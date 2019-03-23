using NUnit.Framework;
using src.Utils;

namespace test.TestSuites
{
    public class ParserTest : BaseSuite
    {
        [Test]
        public void AnnotationsTestCase()
        {
            LoadSourceFile("parser_1");

            var compiler = new Compiler(Source);
            compiler.Tokenize();
            compiler.Parse();

            AssertResult(compiler.AstTree.ToDebugString());
        }
    }
}