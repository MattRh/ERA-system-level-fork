using System;
using System.IO;
using NUnit.Framework;
using src.Utils;

namespace test.TestSuites
{
    public abstract class Suite
    {
        protected string Filename;
        protected SourceCode Source;

        protected void AssertResult(string result)
        {
            var desiredResult = ReadFile(ResultFilePath(Filename));

            Assert.AreEqual(desiredResult, result);
        }

        protected void LoadSourceFile(string name)
        {
            Filename = name;
            Source = new SourceCode(SourceFilePath(name));
        }

        protected static string ReadFile(string path)
        {
            var contents = File.ReadAllText(path);
            contents = contents.Replace("\r\n", "\n").Replace("\r", "\n");
            
            return contents;
        }

        protected static string SourceFilePath(string filename)
        {
            return "_sources/" + filename + ".txt";
        }

        protected static string ResultFilePath(string filename)
        {
            return "_results/" + filename + ".txt";
        }
    }
}