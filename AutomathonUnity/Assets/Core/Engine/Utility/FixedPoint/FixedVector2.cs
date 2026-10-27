using System;

namespace Automathon
{
    /// <summary>
    /// 2D vector made of two <see cref="Fixed"/> components. Works like UnityEngine.Vector2 / System.Numerics.Vector2,
    /// but every operation is deterministic. World space is expressed in Unity units (1 = 1 Unity unit).
    ///
    /// <para>
    /// Products that would lose precision in Q16.16 (dot, cross, squared lengths) are computed exactly on 64 bits
    /// internally; the *Long variants give the exact <see cref="FixedLong"/> result.
    /// </para>
    /// Only the X and Y fields are serialized (everything else is a method), so its JSON form is {"X": 1.5, "Y": -2}.
    /// </summary>
    [Serializable]
    public struct FixedVector2 : IEquatable<FixedVector2>
    {
        public Fixed X;
        public Fixed Y;

        public FixedVector2(Fixed x, Fixed y)
        {
            X = x;
            Y = y;
        }

        #region Constants

        public static FixedVector2 Zero => new FixedVector2(Fixed.Zero, Fixed.Zero);
        public static FixedVector2 One => new FixedVector2(Fixed.One, Fixed.One);
        public static FixedVector2 Up => new FixedVector2(Fixed.Zero, Fixed.One);
        public static FixedVector2 Down => new FixedVector2(Fixed.Zero, Fixed.NegativeOne);
        public static FixedVector2 Right => new FixedVector2(Fixed.One, Fixed.Zero);
        public static FixedVector2 Left => new FixedVector2(Fixed.NegativeOne, Fixed.Zero);

        #endregion

        #region Operators

        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X + b.X, a.Y + b.Y);
        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X - b.X, a.Y - b.Y);
        public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.X, -a.Y);
        public static FixedVector2 operator *(FixedVector2 a, Fixed b) => new FixedVector2(a.X * b, a.Y * b);
        public static FixedVector2 operator *(Fixed a, FixedVector2 b) => new FixedVector2(a * b.X, a * b.Y);
        public static FixedVector2 operator /(FixedVector2 a, Fixed b) => new FixedVector2(a.X / b, a.Y / b);

        /// <summary>Scales by a wide scalar; the multiplication is done in Q32.32 and rounded once.</summary>
        public static FixedVector2 operator *(FixedVector2 a, FixedLong b) => new FixedVector2((Fixed)(a.X * b), (Fixed)(a.Y * b));
        public static FixedVector2 operator *(FixedLong a, FixedVector2 b) => b * a;
        public static FixedVector2 operator /(FixedVector2 a, FixedLong b) => new FixedVector2((Fixed)(a.X / b), (Fixed)(a.Y / b));

        /// <summary>Component-wise product.</summary>
        public static FixedVector2 Scale(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X * b.X, a.Y * b.Y);

        public static bool operator ==(FixedVector2 a, FixedVector2 b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(FixedVector2 a, FixedVector2 b) => !(a == b);

        #endregion

        #region Products / lengths

        /// <summary>Exact dot product in Q32.32.</summary>
        public FixedLong DotLong(FixedVector2 other)
            => FixedLong.Multiply(X, other.X) + FixedLong.Multiply(Y, other.Y);

        /// <summary>Dot product (computed exactly, rounded once).</summary>
        public Fixed Dot(FixedVector2 other) => (Fixed)DotLong(other);

        public static Fixed Dot(FixedVector2 a, FixedVector2 b) => a.Dot(b);

        /// <summary>Exact 2D cross product (z component of the 3D cross product) in Q32.32.</summary>
        public FixedLong CrossLong(FixedVector2 other)
            => FixedLong.Multiply(X, other.Y) - FixedLong.Multiply(Y, other.X);

        /// <summary>2D cross product X * other.Y - Y * other.X (computed exactly, rounded once).</summary>
        public Fixed Cross(FixedVector2 other) => (Fixed)CrossLong(other);

        public static Fixed Cross(FixedVector2 a, FixedVector2 b) => a.Cross(b);

        public FixedLong LengthSquaredLong() => DotLong(this);
        public Fixed LengthSquared() => (Fixed)LengthSquaredLong();

        public Fixed Length()
        {
            // raw sum of squares is exactly length^2 in Q32.32 and fits in an ulong, its integer sqrt is length in Q16.16
            ulong sum = (ulong)((long)X.Raw * X.Raw) + (ulong)((long)Y.Raw * Y.Raw);
            return Fixed.FromRawSaturated((long)FixedMath.SqrtRounded(sum));
        }

        public static Fixed Distance(FixedVector2 a, FixedVector2 b) => (a - b).Length();
        public static Fixed DistanceSquared(FixedVector2 a, FixedVector2 b) => (a - b).LengthSquared();

        #endregion

        #region Normalization

        /// <summary>Unit vector with the same direction. The zero vector stays zero.</summary>
        public FixedVector2 Normalized()
        {
            long x = X.Raw, y = Y.Raw;
            if (x == 0 && y == 0) return Zero;

            // Bring the biggest component around 2^30 so that the computation keeps full precision even for tiny
            // vectors (and doesn't overflow for huge ones). The direction only depends on the x/y ratio.
            long maxAbs = Math.Max(Math.Abs(x), Math.Abs(y));
            while (maxAbs < (1L << 29)) { x <<= 1; y <<= 1; maxAbs <<= 1; }

            long length = (long)FixedMath.SqrtRounded((ulong)(x * x) + (ulong)(y * y)); // < 2^31.5
            return new FixedVector2(
                Fixed.FromRawSaturated(Fixed.RoundedDivide(x << Fixed.FRACTIONAL_BITS, length)),
                Fixed.FromRawSaturated(Fixed.RoundedDivide(y << Fixed.FRACTIONAL_BITS, length)));
        }

        /// <summary>Normalizes this vector in place (like UnityEngine.Vector2.Normalize).</summary>
        public void Normalize() => this = Normalized();

        /// <summary>Returns a copy whose length is at most maxLength.</summary>
        public FixedVector2 ClampMagnitude(Fixed maxLength)
        {
            if (LengthSquaredLong() <= FixedLong.Multiply(maxLength, maxLength))
                return this;
            return Normalized() * maxLength;
        }

        #endregion

        #region Geometry

        public FixedVector2 OrthogonalClockwise() => new FixedVector2(Y, -X);
        public FixedVector2 OrthogonalCounterClockwise() => new FixedVector2(-Y, X);

        /// <summary>Unit vector pointing in the direction of the given angle (radians): (cos, sin).</summary>
        public static FixedVector2 FromAngle(Fixed radians)
        {
            FixedMath.SinCos(radians, out Fixed sin, out Fixed cos);
            return new FixedVector2(cos, sin);
        }

        /// <summary>Angle of this vector in radians, in ]-pi, pi] (0 for the zero vector).</summary>
        public Fixed Angle() => FixedMath.Atan2(Y, X);

        public FixedVector2 Rotated(Fixed radians)
        {
            FixedMath.SinCos(radians, out Fixed sin, out Fixed cos);
            return new FixedVector2(
                (Fixed)(FixedLong.Multiply(X, cos) - FixedLong.Multiply(Y, sin)),
                (Fixed)(FixedLong.Multiply(X, sin) + FixedLong.Multiply(Y, cos)));
        }

        public FixedVector2 ProjectedOn(FixedVector2 other)
        {
            FixedLong otherLengthSquared = other.LengthSquaredLong();
            if (otherLengthSquared == FixedLong.Zero)
                return Zero;

            return other * (DotLong(other) / otherLengthSquared);
        }

        public static FixedVector2 Lerp(FixedVector2 a, FixedVector2 b, Fixed t)
        {
            t = FixedMath.Clamp01(t);
            return a + (b - a) * t;
        }

        /// <summary>
        /// If the projection of this point on the (bound1, bound2) line falls outside of the segment, moves it back
        /// on the segment's end (along the segment's normal). Otherwise returns the point unchanged.
        /// </summary>
        public FixedVector2 ClipBetween(FixedVector2 bound1, FixedVector2 bound2)
        {
            FixedVector2 segment = bound2 - bound1;
            FixedLong lengthSquared = segment.LengthSquaredLong();
            if (lengthSquared == FixedLong.Zero) return this;

            FixedLong scal = segment.DotLong(this - bound1);
            FixedVector2 projOnNormal = (this - bound1).ProjectedOn(segment.OrthogonalCounterClockwise());

            if (scal <= FixedLong.Zero)
                return projOnNormal + bound1;
            else if (scal >= lengthSquared)
                return projOnNormal + bound2;
            return this;
        }

        /// <summary>Closest point to this one on the segment [linePoint1, linePoint2].</summary>
        public FixedVector2 GetClosestOnSegment(FixedVector2 linePoint1, FixedVector2 linePoint2)
        {
            FixedVector2 segment = linePoint2 - linePoint1;
            FixedLong lengthSquared = segment.LengthSquaredLong();
            if (lengthSquared == FixedLong.Zero) return linePoint1;

            FixedLong scal = segment.DotLong(this - linePoint1);

            if (scal <= FixedLong.Zero)
                return linePoint1;
            else if (scal >= lengthSquared)
                return linePoint2;
            return linePoint1 + segment * (scal / lengthSquared);
        }

        #endregion

        #region Conversions / equality

        public static FixedVector2 FromDouble(double x, double y) => new FixedVector2(Fixed.FromDouble(x), Fixed.FromDouble(y));

        public bool Equals(FixedVector2 other) => this == other;
        public override bool Equals(object obj) => obj is FixedVector2 other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X.Raw, Y.Raw);
        public override string ToString() => $"({X}, {Y})";

        public void Deconstruct(out Fixed x, out Fixed y)
        {
            x = X;
            y = Y;
        }

        #endregion
    }
}
