using System;
using Erasystemlevel;
using Erasystemlevel.Exception;

namespace src
{
    internal class Program
    {
        private const string CODE_FILE = "code.txt";

        private static void Main()
        {
            var compiler = new Compiler(CODE_FILE);

            string eraAsm;
            try
            {
                eraAsm = compiler.compile();
            }
            catch (TokenizationError e)
            {
                PrintError("Tokenization error: ", e);
                return;
            }
            catch (SyntaxError e)
            {
                PrintError("Syntax error: ", e);
                return;
            }
            catch (SemanticError e)
            {
                PrintError("Semantic error: ", e);
                return;
            }
            catch (GenerationError e)
            {
                PrintError("Generation error: ", e);
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