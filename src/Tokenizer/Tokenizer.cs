using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Erasystemlevel;
using Erasystemlevel.Exception;

namespace src.Tokenizer
{
    public class Tokenizer
    {
        private readonly Regex _numeric = new Regex("^(\\+|-)?\\d+$");
        private readonly Regex _identifier = new Regex("\\b([A-Za-z_][A-Za-z0-9_]*)\\b");
        private readonly Regex _register = new Regex("\\bR([0-9]|[12][0-9]|3[01])\\b");

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

        public int CurrentLine { get; private set; } = 0;
        public int CurrentSymbol { get; private set; } = 0;

        public (int, int) CurrentPosition => (CurrentLine, CurrentSymbol);
        private string _readSequence;

        public Tokenizer(SourceCode source)
        {
            this.Source = source;
        }

        public void Process()
        {
            // todo
        }

        public Token NextToken()
        {
            _readSequence = string.Empty;

            while (!Source.EndOfFile())
            {
                var next = ReadSymbol();

                if (next.Equals("\n"))
                {
                    if (_readSequence.Length > 0)
                    {
                        throw new TokenizationError($"Got new line, while reading token: `{_readSequence}`");
                    }

                    return new Token(TokenType.NewLine, null);
                }

                if (_whitespace.Contains(next))
                {
                    continue;
                }

                _readSequence += next;

                if (_readSequence.Equals("//"))
                {
                    var text = ReadLine();
                    return new Token(TokenType.Comment, text);
                }

                if (_delimiters.Contains(_readSequence))
                {
                    return new Token(TokenType.Delimiter, _readSequence);
                }

                if (_register.IsMatch(_readSequence))
                {
                    var lookahead = Source.PeekChar().ToString();
                    if (!_idSymbol.IsMatch(lookahead))
                    {
                        return new Token(TokenType.Register, _readSequence);
                    }
                }

                if (_operators.Contains(_readSequence))
                {
                    while (_operators.Contains(_readSequence + Source.PeekChar()))
                    {
                        _readSequence += Source.PopChar();
                    }

                    return new Token(TokenType.Operator, _readSequence);
                }

                if (_keywords.Contains(_readSequence))
                {
                    var lookahead = Source.PeekChar().ToString();
                    if (!_idSymbol.IsMatch(lookahead) || _whitespace.Contains(lookahead))
                    {
                        return new Token(TokenType.Keyword, _readSequence);
                    }
                }

                // todo
                // identifier
                // numbers
            }

            if (_readSequence.Length > 0)
            {
                throw new TokenizationError($"Failed to tokenize string: `{_readSequence}`");
            }

            return null;
        }

        public void Restart()
        {
            CurrentLine = 0;
            CurrentSymbol = 0;

            Source.Reset();
        }

        private string ReadSymbol()
        {
            var next = Source.PopChar().ToString();

            if (next.Equals("\n"))
            {
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
            while (!Source.EndOfFile() || !Source.PeekChar().Equals('\n'))
            {
                text += Source.PopChar();
            }

            return text;
        }

        /*public Token Tokenize()
        {
            var currentToken = string.Empty;

            while (!reader.EndOfStream)
            {
                if (identifierRegex.IsMatch(currentToken))
                {
                    if (reader.EndOfStream || reader.Peek() == ' ' || Convert.ToString(reader.Peek()).Equals("\n"))
                    {
                        return new Token(Token.TokenType.Identifier, currentToken);
                    }

                    var nextChar = Convert.ToChar(reader.Peek());
                    if ((char.IsDigit(nextChar) || char.IsLetter(nextChar)) && nextChar != ' ')
                    {
                        continue;
                    }

                    return new Token(Token.TokenType.Identifier, currentToken);
                }

                if (!numericRegex.IsMatch(currentToken)) continue;
                if (reader.EndOfStream)
                {
                    return new Token(Token.TokenType.Number, currentToken);
                }

                while (numericRegex.IsMatch(currentToken + Convert.ToChar(reader.Peek())) && !reader.EndOfStream)
                {
                    currentToken = currentToken + Convert.ToChar(reader.Read());
                }

                return new Token(Token.TokenType.Number, currentToken);
            }

            reader.Close();
            return null;
        }*/
    }
}