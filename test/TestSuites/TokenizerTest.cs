using System;
using NUnit.Framework;
using src.Tokenizer;

namespace test.TestSuites
{
    public class TokenizerTest: BaseSuite
    {
        [Test]
        public void TokenizationTestCase() {
            LoadSourceFile("tokenizer_1");
            
            var tokenizer = new Tokenizer(Source);
            tokenizer.Process();
            
            AssertResult(tokenizer.ToDebugString());
        }
        
        [Test]
        public void TokenStreamTestCase()
        {
            LoadSourceFile("tokenizer_2");
            
            var tokenizer = new Tokenizer(Source);
            var stream = new TokenStream(tokenizer);
            
            tokenizer.Process();
            
            AssertResult(stream.ToDebugString());
        }
        
    }
}