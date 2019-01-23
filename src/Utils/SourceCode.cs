using System;
using System.IO;
using System.Security;
using System.Text;

namespace Erasystemlevel
{
    public class SourceCode
    {
        private readonly FileStream _fileStream;

        private string _fullText;
        public int Symbol { get; private set; } = 0;

        public SourceCode(string filePath)
        {
            _fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            Read();
        }

        private void Read()
        {
            var reader = new StreamReader(_fileStream, Encoding.UTF8);

            _fullText = reader.ReadToEnd();

            // Convert CRLF and CR to LF
            _fullText = _fullText.Replace("\r\n", "\n")
                .Replace("\n\r", "\n")
                .Replace("\r", "\n");

            // Ensure that we always have new line a the end
            if (!_fullText.EndsWith("\n"))
            {
                _fullText += "\n";
            }
        }

        public void Reset()
        {
            Symbol = 0;
        }

        public char PopChar()
        {
            var next = PeekChar();
            Symbol++;

            return next;
        }

        public char PeekChar()
        {
            if (EndOfFile())
            {
                throw new SecurityException("Failed to peek symbol. End of file is reached");
            }

            return _fullText[Symbol];
        }

        public void GoBack(int symbols = 1)
        {
            Symbol = Math.Max(0, Symbol - symbols);
        }

        public bool EndOfFile()
        {
            return Symbol >= _fullText.Length;
        }

        public string FullText()
        {
            return _fullText;
        }

        public string Line(int number)
        {
            var lines = _fullText.Split('\n');

            return lines.Length <= number ? "" : lines[number];
        }

        public string Highlight(int line, int? symbol, int? length)
        {
            var res = Line(line);
            if (symbol != null)
            {
                res += "\n" + MakeHighlight((int) symbol, length);
            }

            return res;
        }

        private static string MakeHighlight(int offset, int? length)
        {
            if (length == null || length <= 0)
            {
                length = 1;
            }

            var dashesLen = Math.Max(0, offset);
            var dashes = new string('-', dashesLen);

            var pointer = new string('^', (int) length);

            return dashes + pointer;
        }
    }
}