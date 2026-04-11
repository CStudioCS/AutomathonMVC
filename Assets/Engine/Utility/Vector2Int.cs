namespace Automathon
{
    public struct Vector2Int
    {
        public int X;
        public int Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
            => new Vector2Int(a.X + b.X, a.Y + b.Y);

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
            => new Vector2Int(a.X - b.X, a.Y - b.Y);

        public static Vector2Int operator *(Vector2Int a, int b)
            => new Vector2Int(a.X * b, a.Y * b);

        public static Vector2Int operator *(int a, Vector2Int b)
            => new Vector2Int(a * b.X, a * b.Y);

        public static Vector2Int operator /(Vector2Int a, int b)
            => new Vector2Int(a.X / b, a.Y / b);

        public static Vector2Int Zero => new Vector2Int(0, 0);
        public static Vector2Int Up => new Vector2Int(0, 1);
        public static Vector2Int Right => new Vector2Int(1, 0);

        public int Dot(Vector2Int other)
            => X * other.X + Y * other.Y;

        public Vector2Int ProjectedOn(Vector2Int other)
            => Dot(other) * other / other.LengthSquared();

        public int LengthSquared()
            => X * X + Y * Y;

        public void NormalizeAtScale(int scale)
        {
            int l = LengthSquared();
            X = X * scale / l;
            Y = Y * scale / l;
        }

        public Vector2Int Normal()
            => new Vector2Int(-Y, X);

        public Vector2Int Normal2()
            => new Vector2Int(Y, -X);

        public override string ToString()
            => $"({X}, {Y})";
    }
}
