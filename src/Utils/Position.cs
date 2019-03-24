using System;

namespace src.Utils
{
    public class Position
    {
        public readonly Point Start;
        public readonly Point End;

        public Position((int, int) start, (int, int) end)
        {
            Start = new Point(start.Item1, start.Item2);
            End = new Point(end.Item1, end.Item2);
        }

        public Position(int line, int symbol, int length) : this((line, symbol), (line, symbol + length))
        {
        }

        public Position(Point start, Point end)
        {
            this.Start = start;
            this.End = end;
        }

        public override string ToString()
        {
            return $"{Start} -> {End}";
        }
    }

    public class Point : IComparable<Point>
    {
        public readonly int Line;
        public readonly int Symbol;

        public Point(int line, int symbol)
        {
            this.Line = line;
            this.Symbol = symbol;
        }

        public int CompareTo(Point other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            var lineComparison = Line.CompareTo(other.Line);
            return lineComparison != 0 ? lineComparison : Symbol.CompareTo(other.Symbol);
        }

        public override string ToString()
        {
            return $"({Line}, {Symbol})";
        }

        public (int, int) ToTuple()
        {
            return (Line, Symbol);
        }
    }
}