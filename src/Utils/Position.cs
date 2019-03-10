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

        public override string ToString()
        {
            return $"{Start} -> {End}";
        }
    }

    public class Point
    {
        public readonly int Line;
        public readonly int Symbol;

        public Point(int line, int symbol)
        {
            this.Line = line;
            this.Symbol = symbol;
        }

        public override string ToString()
        {
            return $"({Line}, {Symbol})";
        }
    }
}