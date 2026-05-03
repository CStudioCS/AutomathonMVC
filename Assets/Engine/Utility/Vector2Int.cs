using Automathon.Engine.Utility;
using Automathon.Utility;

namespace Automathon
{
    public struct Vector2Int : System.IEquatable<Vector2Int>
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

        public static Vector2Int operator -(Vector2Int a)
            => new Vector2Int(-a.X, -a.Y);

        public static Vector2Int operator *(Vector2Int a, int b)
            => new Vector2Int(a.X * b, a.Y * b);

        public static Vector2Int operator *(int a, Vector2Int b)
            => new Vector2Int(a * b.X, a * b.Y);

        public static Vector2Int operator /(Vector2Int a, int b)
            => new Vector2Int(a.X / b, a.Y / b);

        public static bool operator ==(Vector2Int a, Vector2Int b)
            => a.X == b.X && a.Y == b.Y;

        public static bool operator !=(Vector2Int a, Vector2Int b)
            => !(a == b);

        public bool Equals(Vector2Int other)
            => this == other;

        public override bool Equals(object obj)
            => obj is Vector2Int other && Equals(other);

        public override int GetHashCode()
            => System.HashCode.Combine(X, Y);

        public static Vector2Int Zero => new Vector2Int(0, 0);
        public static Vector2Int Up => new Vector2Int(0, 1);
        public static Vector2Int Right => new Vector2Int(1, 0);

        public int Dot(Vector2Int other)
            => X * other.X + Y * other.Y;

        public long LongDot(Vector2Int other)
            => (long)X * other.X + (long)Y * other.Y;

        public Vector2Int ProjectedOn(Vector2Int other)
        {
            long otherLengthSquared = other.LengthSquared();
            if (otherLengthSquared == 0)
                return Vector2Int.Zero;
            long dot = LongDot(other);

            return new Vector2Int((int)(dot * other.X / otherLengthSquared), (int)(dot * other.Y / otherLengthSquared));
        }

        public int Length()
            => IntMath.Isqrt(X * X + Y * Y);

        public int CalculateAngleMilliRad()
        {
            if (X >= 0)
            {
                return Atan2Int.Atan2(X, Y);
            }
            else
            {
                if (Y >= 0)
                {
                    return (Atan2Int.Atan2(X, Y) + IntMath.PI);
                }
                else
                {
                    return (Atan2Int.Atan2(X, Y) - IntMath.PI);
                }
            }
        }

        public int LengthSquared()
            => X * X + Y * Y;

        public static Vector2Int MilliDirectionFromAngle(int angleMillirad)
            => new Vector2Int(TrigTable.Cos(angleMillirad), TrigTable.Sin(angleMillirad));

        public void NormalizeAtScale(int scale)
        {
            int length = Length();

            if (length == 0) return;

            X = X * scale / length;
            Y = Y * scale / length;
        }
        public Vector2Int OrthogonalClockwise()
            => new Vector2Int(Y, -X);

        public Vector2Int OrthogonalCounterClockwise()
            => new Vector2Int(-Y, X);

        public override string ToString()
            => $"({X}, {Y})";

        public static Vector2Int MilliDirectionFromMilliRad(int milliRadiants)
            => new Vector2Int(TrigTable.Cos(milliRadiants), TrigTable.Sin(milliRadiants));

        public Vector2Int ClipBetween(Vector2Int bound1, Vector2Int bound2)
        {
            int lengthSquared = (bound2 - bound1).LengthSquared();
            if (lengthSquared == 0) return this;

            int scal = (bound2 - bound1).Dot(this - bound1);
            Vector2Int projOnNormal = (this - bound1).ProjectedOn((bound2 - bound1).OrthogonalCounterClockwise());

            if (scal <= 0)
                return projOnNormal + bound1;
            else if (scal >= lengthSquared)
                return projOnNormal + bound2;
            return this;
        }

        public Vector2Int GetClosestOnLine(Vector2Int linePoint1, Vector2Int linePoint2)
        {
            int lengthSquared = (linePoint2 - linePoint1).LengthSquared();
            if (lengthSquared == 0) return linePoint1;

            int scal = (linePoint2 - linePoint1).Dot(this - linePoint1);

            if (scal <= 0)
                return linePoint1;
            else if (scal >= lengthSquared)
                return linePoint2;
            return linePoint1 + scal * (linePoint2 - linePoint1) / lengthSquared;
        }
    }
}
