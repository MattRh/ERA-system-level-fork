using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using src.Exceptions;
using src.Interfaces;
using src.Parser;
using src.Parser.Nodes;
using src.Utils;

namespace src.Tokenizer
{
    public class Tokenizer : IDebuggable
    {
        private readonly Regex _numeric = new Regex("^((\\+|-)?\\d+)\\z");
        private readonly Regex _identifier = new Regex("^([A-Za-z_][A-Za-z0-9_]*)\\z");
        private readonly Regex _register = new Regex("^(R([0-9]|[12][0-9]|3[01]))\\z");

        private readonly Regex _idSymbol = new Regex("[A-Za-z0-9_]");

        private readonly HashSet<string> _delimiters;
        private readonly HashSet<string> _operators;
        private readonly HashSet<string> _keywords;
        private readonly HashSet<string> _whitespace;

        private readonly HashSet<string> _terminals = new HashSet<string>();

        public readonly SourceCode Source;

        private int _prevLineLength = 0;
        public int CurrentLine { get; private set; } = 0;
        public int CurrentSymbol { get; private set; } = 0;
        public (int, int) CurrentPosition => (CurrentLine, CurrentSymbol);

        private string _readSequence;

        public List<Token> Tokens = new List<Token>();

        public Tokenizer(SourceCode source)
        {
            this.Source = source;

            _delimiters = new HashSet<string>(TerminalsHelper.TerminalsList(typeof(Delimiter)));
            _operators = new HashSet<string>(TerminalsHelper.TerminalsList(typeof(Operator)));
            _keywords = new HashSet<string>(TerminalsHelper.TerminalsList(typeof(Keyword)));
            _whitespace = new HashSet<string>(TerminalsHelper.TerminalsList(typeof(Whitespace)));

            _terminals.UnionWith(_delimiters);
            _terminals.UnionWith(_operators);
            _terminals.UnionWith(_keywords);

            _terminals.Add(OtherTerminals.LineComment);
        }

        public void Process()
        {
            Token next;
            while ((next = NextToken()) != null) {
                Tokens.Add(next);
            }
        }

        private Token NextToken()
        {
            _readSequence = string.Empty;

            while (!Source.EndOfFile()) {
                var next = ReadSymbol();

                if (next.Equals(Whitespace.NewLine)) {
                    if (_readSequence.Length > 0) {
                        throw TokenizationError.UnexpectedEol(_readSequence, GetPosition());
                    }

                    return MakeToken(TokenType.NewLine, null);
                }

                if (_whitespace.Contains(next) || next.Equals(OtherTerminals.Empty)) {
                    continue;
                }

                _readSequence += next;

                var lookahead = Source.PeekChar();
                var sequenceLookahead = _readSequence + lookahead;

                if (_identifier.IsMatch(sequenceLookahead) || _numeric.IsMatch(sequenceLookahead)) {
                    continue;
                }

                // Here starts actual checks for token type
                // If passes this test then it is not identifier or numeric
                if (SimilarTerminalExists(_readSequence)) {
                    while (SimilarTerminalExists(_readSequence + Source.PeekChar())) {
                        _readSequence += ReadSymbol();
                    }

                    if (!_terminals.Contains(_readSequence)) {
                        throw TokenizationError.UnknownSymbol(_readSequence, GetPosition());
                    }

                    if (_readSequence.Equals(OtherTerminals.LineComment)) {
                        var text = ReadRemainingLine();
                        return MakeToken(TokenType.LineComment, text);
                    }

                    if (_delimiters.Contains(_readSequence)) {
                        return MakeToken(TokenType.Delimiter, _readSequence);
                    }
                    if (_operators.Contains(_readSequence)) {
                        return MakeToken(TokenType.Operator, _readSequence);
                    }
                    if (_keywords.Contains(_readSequence)) {
                        // there should be check for non id symbol in lookahead
                        // but it is not needed doe to greedy reading
                        return MakeToken(TokenType.Keyword, _readSequence);
                    }
                }

                if (_register.IsMatch(_readSequence)) {
                    // there should be check for non id symbol in lookahead
                    // but it is not needed doe to greedy reading
                    return MakeToken(TokenType.Register, _readSequence);
                }
                if (_identifier.IsMatch(_readSequence)) {
                    return MakeToken(TokenType.Identifier, _readSequence);
                }
                if (_numeric.IsMatch(_readSequence)) {
                    return MakeToken(TokenType.Number, _readSequence);
                }

                throw TokenizationError.UnknownSymbol(_readSequence, GetPosition());
            }

            if (_readSequence.Length > 0) {
                throw TokenizationError.TokenizationFail(_readSequence, GetPosition());
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

        private bool SimilarTerminalExists(string value)
        {
            foreach (var terminal in _terminals) {
                if (terminal.StartsWith(value)) {
                    return true;
                }
            }

            return false;
        }

        private Position GetPosition()
        {
            var line = CurrentLine;
            var symbol = CurrentSymbol - 1;
            var length = Math.Max(1, _readSequence.Length);

            return new Position(line, symbol, length);
        }

        private Token MakeToken(TokenType type, string value)
        {
            var pos = CurrentPosition;

            // Fix newline token
            if (type == TokenType.NewLine) {
                pos.Item1--; // Decrease line
                pos.Item2 = _prevLineLength + 1; // Set old line length and one character longer
            }
            else {
                pos.Item2 -= value.Length;
            }

            return new Token(type, value, pos);
        }

        private string ReadSymbol()
        {
            var next = Source.PopChar();
            if (next.Equals(Whitespace.NewLine)) {
                _prevLineLength = CurrentSymbol;

                CurrentLine += 1;
                CurrentSymbol = 0;
            }
            else if (!next.Equals(OtherTerminals.Empty)) {
                CurrentSymbol += 1;
            }

            return next;
        }

        private string ReadRemainingLine()
        {
            var text = string.Empty;
            while (!Source.EndOfFile() && !Source.PeekChar().Equals(Whitespace.NewLine)) {
                text += ReadSymbol();
            }

            return text;
        }

        public string ToDebugString()
        {
            var res = string.Empty;
            foreach (var token in Tokens) {
                res += token.ToJsonString() + "\n";
            }

            return res.Trim();
        }
    }
}