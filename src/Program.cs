using System;
using src.Exceptions;
using src.Utils;

namespace src
{
    internal class Program
    {
        private const string CODE_FILE = "code.txt";

        private static void Main()
        {
            var compiler = new Compiler(CODE_FILE);

            string eraAsm;
            try {
                eraAsm = compiler.Compile();
            }
            catch (CompilationError e) {
                e.Source = compiler.SourceCode;
                Console.WriteLine(e.Verbose());

                return;
            }

            Console.WriteLine(eraAsm);
        }

        private static void PrintError(string description, SystemException error)
        {
            Console.Write(description);
            Console.WriteLine(error);
        }
    }
}