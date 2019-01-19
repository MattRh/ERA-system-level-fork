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
        private readonly Regex _identifier = new Regex("\\b([A-Za-z][A-Za-z0-9_]*)\\b");
        private readonly Regex _register = new Regex("\\bR([0-9]|[12][0-9]|3[01])\\b");

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
            "", "\n", "\r", " "
        };

        public readonly SourceCode Source;

        public int CurrentLine { get; private set; } = 0;
        public int CurrentSymbol { get; private set; } = 0;

        public (int, int) CurrentPosition => (CurrentLine, CurrentSymbol);

        public Tokenizer(SourceCode source)
        {
            this.Source = source;
        }

        public Token NextToken()
        {
            var readSequence = string.Empty;

            while (!Source.EndOfFile())
            {
                // todo
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

            Source.Reset();
        }

        public Token Tokenize()
        {
            var currentToken = string.Empty;

            while (!reader.EndOfStream)
            {
                var next = Convert.ToChar(reader.Read());
                if (next.ToString().Equals("\n"))
                {
                    symbolNumber = 0;
                    lineNumber += 1;
                }

                if (!next.ToString().Equals("") && !next.ToString().Equals("\n"))
                {
                    symbolNumber += 1;
                }

                if (_whitespace.Contains(next.ToString()))
                {
                    continue;
                }

                currentToken += next;


                if (currentToken.Equals("//"))
                {
                    currentToken = string.Empty;

                    while (!reader.EndOfStream)
                    {
                        next = Convert.ToChar(reader.Read());
                        if (next.Equals('\n'))
                        {
                            break;
                        }
                    }

                    continue;
                }

                if (_delimeters.Contains(currentToken))
                {
                    return new Token(Token.TokenType.Delimiter, currentToken);
                }

                if (_register.IsMatch(currentToken))
                {
                    var nextChar = Convert.ToChar(reader.Peek());

                    if (!char.IsDigit(nextChar) && !char.IsLetter(nextChar) && nextChar.ToString() != "_")
                    {
                        return new Token(Token.TokenType.Register, currentToken);
                    }
                }

                if (_operators.Contains(currentToken))
                {
                    while (_operators.Contains(currentToken + Convert.ToChar(reader.Peek())))
                    {
                        currentToken = currentToken + Convert.ToChar(reader.Read());
                    }

                    return new Token(Token.TokenType.Operator, currentToken);
                }

                if (_keywords.Contains(currentToken))
                {
                    if (reader.EndOfStream || _whitespace.Contains(Convert.ToString(reader.Peek())))
                    {
                        return new Token(Token.TokenType.Keyword, currentToken);
                    }

                    var nextChar = Convert.ToChar(reader.Peek());
                    if (!char.IsDigit(nextChar) && !char.IsLetter(nextChar) && nextChar.ToString() != "_")
                    {
                        return new Token(Token.TokenType.Keyword, currentToken);
                    }
                }

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
        }
    }
}