using System;
using System.Collections.Generic;
using System.Reflection;

namespace src.Tokenizer
{
    public class Delimiter
    {
        public const string
            Colon = ":",
            Semicolon = ";",
            Comma = ",",
            Dot = ".",
            ParenthesisOpen = "(",
            ParenthesisClose = ")",
            BraceOpen = "[",
            BraceClose = "]";
    }

    public class Whitespace
    {
        public const string
            Space = " ",
            Tab = "\t",
            NewLine = "\n";
    }

    public class Operator
    {
        public const string
            Plus = "+",
            Minus = "-",
            Asterisk = "*", // multiply || ref
            Ampersand = "&", // and || deref
            Line = "|", // or
            Hat = "^", // xor
            Question = "?", // todo: what is that?
            //
            Equal = "=",
            Less = "<",
            Greater = ">",
            NotEqual = "/=",
            //
            Assign = ":=",
            AssignPlus = "+=",
            AssignMinus = "-=",
            AssignShiftRight = ">>=",
            AssignShiftLeft = "<<=",
            AssignOr = "|=",
            AssignAnd = "&=",
            AssignXor = "^=",
            AssignLess = "<=",
            AssignGreater = ">=",
            AssignCond = "?=", // todo: what is that?
            AssignSwap = "<=>";
    }

    public class Keyword
    {
        public const string
            Pragma = "pragma",
            Module = "module",
            Data = "data",
            Code = "code",
            //
            Routine = "routine",
            Start = "start",
            Entry = "entry",
            //
            If = "if",
            Else = "else",
            ElseIf = "elif",
            //
            Do = "do",
            End = "end",
            //
            Const = "const",
            Int = "int",
            Short = "short",
            Byte = "byte",
            //
            Format = "format",
            Skip = "skip",
            Stop = "stop",
            //
            For = "for",
            From = "from",
            To = "to",
            Step = "step",
            While = "while",
            Loop = "loop",
            Break = "break",
            //
            Goto = "goto";
    }

    public class OtherTerminals
    {
        public const string
            Empty = "",
            LineComment = "//";
    }

    public class TerminalsHelper
    {
        public static IEnumerable<string> TerminalsList(Type t)
        {
            var terminals = new List<string>();

            var fieldInfos = t.GetFields(BindingFlags.Public | BindingFlags.Static);

            // Go through the field list and only pick out the constants
            foreach (var fi in fieldInfos) {
                if (fi.IsLiteral && !fi.IsInitOnly) {
                    terminals.Add((string) fi.GetValue(null));
                }
            }

            return terminals;
        }
    }
}