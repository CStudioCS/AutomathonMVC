namespace Automathon
{
    public struct Vector2Int
    {
        public int X;
        public int Y;

        public Vector2Int(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
            => new Vector2Int(a.X + b.X, a.Y + b.Y);

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
            => new Vector2Int(a.X - b.X, a.Y - b.Y);

        public static Vector2Int operator *(Vector2Int a, int b)
            => new Vector2Int(a.X * b, a.Y * b);

        public static Vector2Int operator *(int a, Vector2Int b)
            => new Vector2Int(a * b.X, a * b.Y);

        public static Vector2Int up => new Vector2Int(0, 1);
        public static Vector2Int right => new Vector2Int(1, 0);

        public override string ToString()
            => $"({X}, {Y})";
    }
}
