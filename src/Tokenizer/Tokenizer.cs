using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Erasystemlevel;
using Erasystemlevel.Exception;

namespace src.Tokenizer
{
    public class Tokenizer
    {
        private readonly Regex _numeric = new Regex("^((\\+|-)?\\d+)\\z");
        private readonly Regex _identifier = new Regex("^([A-Za-z_][A-Za-z0-9_]*)\\z");
        private readonly Regex _register = new Regex("^(R([0-9]|[12][0-9]|3[01]))\\z");

        private readonly Regex _idSymbol = new Regex("[A-Za-z0-9_]");

        private readonly HashSet<string> _delimiters = new HashSet<string>()
        {
            ";", ",", ".", "(", ")", "[", "]", ":>"
        };

        private readonly HashSet<string> _operators = new HashSet<string>()
        {
            "+", "-", "*", "&", "|", "^", "?", "=", "<", ">", "/=", ":=",
            "+=", "-=", ">>=", "<<=", "|=", "&=", "^=", "<=", ">=", "?=",
            "<=>"
        };

        private readonly HashSet<string> _keywords = new HashSet<string>()
        {
            "if", "else", "int", "short", "byte", "const", "routine", "do", "end",
            "start", "entry", "skip", "stop", "goto", "format", "for", "from", "to",
            "step", "while", "loop", "break", "then", "by", "trace", "data", "module", "code",
            "this"
        };

        private readonly HashSet<string> _whitespace = new HashSet<string>()
        {
            "", " ", "\n", "\r", "\t"
        };

        public readonly SourceCode Source;

        private int _prevLineLength = 0;
        public int CurrentLine { get; private set; } = 0;
        public int CurrentSymbol { get; private set; } = 0;
        public (int, int) CurrentPosition => (CurrentLine, CurrentSymbol);

        public List<Token> Tokens = new List<Token>();

        public Tokenizer(SourceCode source)
        {
            this.Source = source;
        }

        public void Process()
        {
            Token next;
            while ((next = NextToken()) != null)
            {
                Tokens.Add(next);
            }
        }

        private Token NextToken()
        {
            var readSequence = string.Empty;

            while (!Source.EndOfFile())
            {
                var next = ReadSymbol();

                if (next.Equals("\n"))
                {
                    if (readSequence.Length > 0)
                    {
                        throw new TokenizationError($"Got new line, while reading token: `{readSequence}`");
                    }

                    return MakeToken(TokenType.NewLine, null);
                }

                if (_whitespace.Contains(next))
                {
                    continue;
                }

                readSequence += next;

                if (readSequence.Equals("//"))
                {
                    var text = ReadLine();
                    return MakeToken(TokenType.Comment, text);
                }

                if (_delimiters.Contains(readSequence))
                {
                    return MakeToken(TokenType.Delimiter, readSequence);
                }

                if (_register.IsMatch(readSequence))
                {
                    var lookahead = Source.PeekChar().ToString();
                    if (!_idSymbol.IsMatch(lookahead))
                    {
                        return MakeToken(TokenType.Register, readSequence);
                    }
                }

                if (_operators.Contains(readSequence))
                {
                    while (_operators.Contains(readSequence + Source.PeekChar()))
                    {
                        readSequence += ReadSymbol();
                    }

                    return MakeToken(TokenType.Operator, readSequence);
                }

                if (_keywords.Contains(readSequence))
                {
                    var lookahead = Source.PeekChar().ToString();
                    if (!_idSymbol.IsMatch(lookahead) || _whitespace.Contains(lookahead))
                    {
                        return MakeToken(TokenType.Keyword, readSequence);
                    }
                }

                if (_identifier.IsMatch(readSequence))
                {
                    while (_idSymbol.IsMatch(Source.PeekChar().ToString()))
                    {
                        readSequence += ReadSymbol();
                    }

                    return MakeToken(TokenType.Identifier, readSequence);
                }

                if (_numeric.IsMatch(readSequence))
                {
                    while (_numeric.IsMatch(readSequence + Source.PeekChar()))
                    {
                        readSequence += ReadSymbol();
                    }

                    return MakeToken(TokenType.Number, readSequence);
                }
            }

            if (readSequence.Length > 0)
            {
                throw new TokenizationError($"Failed to tokenize string: `{readSequence}`");
            }

            return null;
        }

        public void Restart()
        {
            CurrentLine = 0;
            CurrentSymbol = 0;

            Tokens.Clear();
            Source.Reset();
        }

        private Token MakeToken(TokenType type, string value)
        {
            var pos = CurrentPosition;
            
            // Fix newline token
            if (type == TokenType.NewLine)
            {
                pos.Item1--; // Decrease line
                pos.Item2 = _prevLineLength + 1; // Set old line length and one character longer
            }
            
            return new Token(type, value, pos);
        } 

        private string ReadSymbol()
        {
            var next = Source.PopChar().ToString();
            if (next.Equals("\n"))
            {
                _prevLineLength = CurrentSymbol;
                
                CurrentLine += 1;
                CurrentSymbol = 0;
            }
            else if (!next.Equals(""))
            {
                CurrentSymbol += 1;
            }

            return next;
        }

        private string ReadLine()
        {
            var text = string.Empty;
            while (!Source.EndOfFile() && !Source.PeekChar().Equals('\n'))
            {
                text += ReadSymbol();
            }

            return text;
        }
    }
}