using System.IO;
using System.Text;

namespace Erasystemlevel
{
    public class SourceCode
    {
        private FileStream _fileStream;

        private SourceCode(string filePath)
        {
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var reader = new StreamReader(_fileStream, Encoding.UTF8);
        }

        public void Reset()
        {
            // todo
        }

        public void Close()
        {
            // todo
        }

        public char NextChar()
        {
            // todo
            return '';
        }

        public void GoBack(int symbols = 1)
        {
            // todo
        }

        public bool EndOfFile()
        {
            // todo
            return false;
        }

        public string FullText()
        {
            RequireReset();

            // todo
            return "";
        }

        public string Line(int number)
        {
            RequireReset();

            // todo
            return "";
        }

        public string Highlight(int line, int? symbol, int? length)
        {
            // todo
            return "";
        }

        private void RequireReset()
        {
            // todo
        }
    }
}