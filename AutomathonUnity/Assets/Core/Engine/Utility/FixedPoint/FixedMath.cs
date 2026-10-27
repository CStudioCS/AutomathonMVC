namespace Automathon
{
    /// <summary>
    /// Deterministic math functions for <see cref="Fixed"/> and <see cref="FixedLong"/> (the fixed point equivalent of
    /// System.Math / UnityEngine.Mathf). Everything is computed with integer arithmetic only, so results are bit-for-bit
    /// identical on every platform. Angles are in radians.
    /// </summary>
    public static class FixedMath
    {
        public static readonly Fixed Pi = Fixed.Pi;
        public static readonly Fixed HalfPi = Fixed.HalfPi;
        public static readonly Fixed TwoPi = Fixed.TwoPi;

        #region Basic helpers

        public static Fixed Abs(Fixed value) => value.Abs();
        public static FixedLong Abs(FixedLong value) => value.Abs();
        public static int Sign(Fixed value) => value.Sign();
        public static int Sign(FixedLong value) => value.Sign();

        public static Fixed Min(Fixed a, Fixed b) => a < b ? a : b;
        public static Fixed Max(Fixed a, Fixed b) => a > b ? a : b;
        public static FixedLong Min(FixedLong a, FixedLong b) => a < b ? a : b;
        public static FixedLong Max(FixedLong a, FixedLong b) => a > b ? a : b;

        public static Fixed Clamp(Fixed value, Fixed min, Fixed max) => value < min ? min : value > max ? max : value;
        public static FixedLong Clamp(FixedLong value, FixedLong min, FixedLong max) => value < min ? min : value > max ? max : value;
        public static Fixed Clamp01(Fixed value) => Clamp(value, Fixed.Zero, Fixed.One);

        public static Fixed Floor(Fixed value) => Fixed.FromRaw(value.Raw & ~(Fixed.ONE_RAW - 1));
        public static Fixed Ceiling(Fixed value) => -Floor(-value);
        /// <summary>Rounds to the nearest integer, halfway values are rounded away from zero.</summary>
        public static Fixed Round(Fixed value) => Fixed.FromInt(value.RoundToInt());
        public static Fixed Truncate(Fixed value) => Fixed.FromInt((int)value);

        public static Fixed Lerp(Fixed a, Fixed b, Fixed t) => a + (b - a) * Clamp01(t);
        public static Fixed LerpUnclamped(Fixed a, Fixed b, Fixed t) => a + (b - a) * t;

        public static Fixed MoveTowards(Fixed current, Fixed target, Fixed maxDelta)
            => Abs(target - current) <= maxDelta ? target : current + maxDelta * Sign(target - current);

        #endregion

        #region Square root

        /// <summary>Square root rounded to the nearest representable value. Negative inputs return 0.</summary>
        public static Fixed Sqrt(Fixed value)
        {
            if (value.Raw <= 0) return Fixed.Zero;
            // sqrt(raw / 2^16) * 2^16 == sqrt(raw * 2^16)
            return Fixed.FromRawSaturated((long)SqrtRounded((ulong)value.Raw << Fixed.FRACTIONAL_BITS));
        }

        /// <summary>Square root rounded to the nearest representable value. Negative inputs return 0.</summary>
        public static FixedLong Sqrt(FixedLong value)
        {
            if (value.Raw <= 0) return FixedLong.Zero;
            // sqrt(raw / 2^32) * 2^32 == sqrt(raw * 2^32), computed on 128 bits
            ulong raw = (ulong)value.Raw;
            ulong hi = raw >> 32, lo = raw << 32;
            ulong root = UInt128Math.Sqrt(hi, lo);

            // round to nearest: compare n with root^2 + root (= (root + 0.5)^2 - 0.25)
            UInt128Math.Multiply(root, root, out ulong sqHi, out ulong sqLo);
            UInt128Math.Add(ref sqHi, ref sqLo, root);
            if (hi > sqHi || (hi == sqHi && lo > sqLo))
                root++;

            return FixedLong.FromRaw((long)root);
        }

        /// <summary>Square root of an exact Q32.32 value (ex: a squared length) returned as a Q16.16 <see cref="Fixed"/>.</summary>
        public static Fixed SqrtToFixed(FixedLong value)
        {
            if (value.Raw <= 0) return Fixed.Zero;
            // sqrt(raw / 2^32) * 2^16 == sqrt(raw)
            return Fixed.FromRawSaturated((long)SqrtRounded((ulong)value.Raw));
        }

        /// <summary>round(sqrt(n)) for 64-bit integers.</summary>
        internal static ulong SqrtRounded(ulong n)
        {
            ulong root = UInt128Math.Sqrt(n);
            // n > root^2 + root  <=>  sqrt(n) > root + 0.5 (n integer)
            if (n - root * root > root) root++;
            return root;
        }

        #endregion

        #region Trigonometry (CORDIC, Q30 internal precision)

        private const int Q30_BITS = 30;
        private const int Q30_TO_Q16 = Q30_BITS - Fixed.FRACTIONAL_BITS;
        private const long PI_Q30 = 3373259426L;
        private const long HALF_PI_Q30 = 1686629713L;
        private const long TWO_PI_Q30 = 6746518852L;
        private const long CORDIC_GAIN_INV_Q30 = 652032874L; // prod(1 / sqrt(1 + 2^-2i)) * 2^30
        private const int CORDIC_ITERATIONS = 30;

        // atan(2^-i) * 2^30
        private static readonly long[] AtanTable = new long[]
        {
            843314857L, 497837829L, 263043837L, 133525159L, 67021687L, 33543516L, 16775851L, 8388437L,
            4194283L, 2097149L, 1048576L, 524288L, 262144L, 131072L, 65536L, 32768L,
            16384L, 8192L, 4096L, 2048L, 1024L, 512L, 256L, 128L,
            64L, 32L, 16L, 8L, 4L, 2L, 1L
        };

        private static int Q30ToFixedRaw(long q30) => (int)((q30 + (1L << (Q30_TO_Q16 - 1))) >> Q30_TO_Q16);

        /// <summary>Computes both sine and cosine of an angle (radians) at once.</summary>
        public static void SinCos(Fixed angle, out Fixed sin, out Fixed cos)
        {
            // Reduce to [-pi, pi] in Q30
            long z = ((long)angle.Raw << Q30_TO_Q16) % TWO_PI_Q30;
            if (z > PI_Q30) z -= TWO_PI_Q30;
            else if (z < -PI_Q30) z += TWO_PI_Q30;

            // Reduce to [-pi/2, pi/2], CORDIC only converges there. cos/sin(z +- pi) = -cos/sin(z)
            bool negate = false;
            if (z > HALF_PI_Q30) { z -= PI_Q30; negate = true; }
            else if (z < -HALF_PI_Q30) { z += PI_Q30; negate = true; }

            // CORDIC in rotation mode
            long x = CORDIC_GAIN_INV_Q30;
            long y = 0;
            for (int i = 0; i < CORDIC_ITERATIONS; i++)
            {
                long xShift = x >> i;
                long yShift = y >> i;
                if (z >= 0)
                {
                    x -= yShift;
                    y += xShift;
                    z -= AtanTable[i];
                }
                else
                {
                    x += yShift;
                    y -= xShift;
                    z += AtanTable[i];
                }
            }

            if (negate) { x = -x; y = -y; }

            // Clamp the tiny CORDIC overshoots so |sin|, |cos| <= 1
            const long ONE_Q30 = 1L << Q30_BITS;
            if (x > ONE_Q30) x = ONE_Q30; else if (x < -ONE_Q30) x = -ONE_Q30;
            if (y > ONE_Q30) y = ONE_Q30; else if (y < -ONE_Q30) y = -ONE_Q30;

            cos = Fixed.FromRaw(Q30ToFixedRaw(x));
            sin = Fixed.FromRaw(Q30ToFixedRaw(y));
        }

        public static Fixed Sin(Fixed angle)
        {
            SinCos(angle, out Fixed sin, out _);
            return sin;
        }

        public static Fixed Cos(Fixed angle)
        {
            SinCos(angle, out _, out Fixed cos);
            return cos;
        }

        /// <summary>Tangent. Saturates near +-pi/2.</summary>
        public static Fixed Tan(Fixed angle)
        {
            SinCos(angle, out Fixed sin, out Fixed cos);
            return sin / cos;
        }

        /// <summary>
        /// Angle (radians, in ]-pi, pi]) of the vector (x, y), same convention as Math.Atan2(y, x). Atan2(0, 0) == 0.
        /// </summary>
        public static Fixed Atan2(Fixed y, Fixed x) => Atan2Raw(y.Raw, x.Raw);

        /// <summary>Same as <see cref="Atan2(Fixed, Fixed)"/> but for <see cref="FixedLong"/> coordinates.</summary>
        public static Fixed Atan2(FixedLong y, FixedLong x) => Atan2Raw(y.Raw, x.Raw);

        private static Fixed Atan2Raw(long yRaw, long xRaw)
        {
            if (xRaw == 0)
            {
                if (yRaw > 0) return Fixed.HalfPi;
                if (yRaw < 0) return -Fixed.HalfPi;
                return Fixed.Zero;
            }
            if (yRaw == 0)
                return xRaw > 0 ? Fixed.Zero : Fixed.Pi;

            // Work in the right half plane: atan2(y, x) = atan2(-y, -x) +- pi when x < 0.
            // Huge values are pre-shifted down to stay away from overflow (CORDIC gain is ~1.65).
            long x = xRaw, y = yRaw;
            const long LIMIT = 1L << 60;
            if (x >= LIMIT || x <= -LIMIT || y >= LIMIT || y <= -LIMIT)
            {
                x >>= 2;
                y >>= 2;
            }
            long correction = 0;
            if (x < 0)
            {
                x = -x;
                y = -y;
                correction = yRaw >= 0 ? PI_Q30 : -PI_Q30;
            }

            // Scale up to use the full precision (results only depend on the ratio y/x)
            long absY = y < 0 ? -y : y;
            long maxAbs = x > absY ? x : absY;
            while (maxAbs < (1L << 58))
            {
                x <<= 1;
                y <<= 1;
                maxAbs <<= 1;
            }

            // CORDIC in vectoring mode: rotate (x, y) towards the x axis and accumulate the angle
            long angle = 0;
            for (int i = 0; i < CORDIC_ITERATIONS; i++)
            {
                long xShift = x >> i;
                long yShift = y >> i;
                if (y >= 0)
                {
                    x += yShift;
                    y -= xShift;
                    angle += AtanTable[i];
                }
                else
                {
                    x -= yShift;
                    y += xShift;
                    angle -= AtanTable[i];
                }
            }

            angle += correction;
            if (angle > PI_Q30) angle -= TWO_PI_Q30;
            if (angle <= -PI_Q30) angle += TWO_PI_Q30;

            return Fixed.FromRaw(Q30ToFixedRaw(angle));
        }

        public static Fixed Atan(Fixed value) => Atan2(value, Fixed.One);

        /// <summary>Arc sine, input is clamped to [-1, 1].</summary>
        public static Fixed Asin(Fixed value)
        {
            value = Clamp(value, Fixed.NegativeOne, Fixed.One);
            FixedLong cos = Sqrt(FixedLong.One - FixedLong.Multiply(value, value));
            return Atan2((FixedLong)value, cos);
        }

        /// <summary>Arc cosine, input is clamped to [-1, 1].</summary>
        public static Fixed Acos(Fixed value)
        {
            value = Clamp(value, Fixed.NegativeOne, Fixed.One);
            FixedLong sin = Sqrt(FixedLong.One - FixedLong.Multiply(value, value));
            return Atan2(sin, (FixedLong)value);
        }

        /// <summary>Wraps an angle to ]-pi, pi].</summary>
        public static Fixed WrapAngle(Fixed angle)
        {
            Fixed wrapped = angle % Fixed.TwoPi;
            if (wrapped > Fixed.Pi) wrapped -= Fixed.TwoPi;
            else if (wrapped <= -Fixed.Pi) wrapped += Fixed.TwoPi;
            return wrapped;
        }

        /// <summary>Shortest signed difference between two angles, in ]-pi, pi].</summary>
        public static Fixed DeltaAngle(Fixed from, Fixed to) => WrapAngle(to - from);

        #endregion
    }
}
