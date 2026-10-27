namespace Automathon
{
    /// <summary>
    /// Minimal unsigned 128-bit helpers used by <see cref="FixedLong"/> and <see cref="FixedMath"/>.
    /// System.Int128 / Math.BigMul(long, long) are not available in Unity (.NET Standard 2.1), so this is done by hand.
    /// Every function is pure integer arithmetic, hence deterministic.
    /// </summary>
    internal static class UInt128Math
    {
        /// <summary>Full 64x64 -> 128 bit unsigned product.</summary>
        public static void Multiply(ulong a, ulong b, out ulong hi, out ulong lo)
        {
            ulong aLo = a & 0xFFFFFFFFUL, aHi = a >> 32;
            ulong bLo = b & 0xFFFFFFFFUL, bHi = b >> 32;

            ulong loLo = aLo * bLo;
            ulong hiLo = aHi * bLo;
            ulong loHi = aLo * bHi;
            ulong hiHi = aHi * bHi;

            ulong cross = (loLo >> 32) + (hiLo & 0xFFFFFFFFUL) + loHi; // cannot overflow
            lo = (cross << 32) | (loLo & 0xFFFFFFFFUL);
            hi = hiHi + (hiLo >> 32) + (cross >> 32);
        }

        /// <summary>(hi:lo) + value</summary>
        public static void Add(ref ulong hi, ref ulong lo, ulong value)
        {
            ulong newLo = lo + value;
            if (newLo < lo) hi++;
            lo = newLo;
        }

        /// <summary>(hi:lo) >> shift, 0 &lt;= shift &lt; 64</summary>
        public static void ShiftRight(ref ulong hi, ref ulong lo, int shift)
        {
            if (shift == 0) return;
            lo = (lo >> shift) | (hi << (64 - shift));
            hi >>= shift;
        }

        /// <summary>
        /// 128 by 64 bit unsigned division: (hi:lo) / divisor. Quotient is 128 bits (qHi:qLo). divisor must not be 0.
        /// </summary>
        public static void Divide(ulong hi, ulong lo, ulong divisor, out ulong qHi, out ulong qLo, out ulong remainder)
        {
            if (hi == 0)
            {
                qHi = 0;
                qLo = lo / divisor;
                remainder = lo % divisor;
                return;
            }

            // Schoolbook binary long division. rem < divisor <= 2^64 - 1 at every step, carry handles the 65th bit.
            ulong rem = 0;
            qHi = 0;
            qLo = 0;
            for (int i = 127; i >= 0; i--)
            {
                ulong bit = i >= 64 ? (hi >> (i - 64)) & 1UL : (lo >> i) & 1UL;
                bool carry = (rem >> 63) != 0;
                rem = (rem << 1) | bit;

                if (carry || rem >= divisor)
                {
                    rem -= divisor;
                    if (i >= 64) qHi |= 1UL << (i - 64);
                    else qLo |= 1UL << i;
                }
            }

            remainder = rem;
        }

        /// <summary>floor(sqrt(hi:lo)), the result always fits in 64 bits.</summary>
        public static ulong Sqrt(ulong hi, ulong lo)
        {
            if (hi == 0)
                return Sqrt(lo);

            // Newton iteration on 128 bits, starting from an over-estimate so the sequence decreases monotonically.
            int bits = 128 - LeadingZeroCount(hi);
            int shift = (bits + 1) / 2;
            ulong x = shift >= 64 ? ulong.MaxValue : (1UL << shift);

            while (true)
            {
                Divide(hi, lo, x, out ulong qHi, out ulong qLo, out _);
                // y = (x + q) / 2 computed on 128 bits
                ulong sumLo = qLo + x;
                ulong sumHi = qHi + (sumLo < qLo ? 1UL : 0UL);
                ulong y = (sumLo >> 1) | (sumHi << 63);
                if ((sumHi >> 1) != 0) y = ulong.MaxValue;
                if (y >= x) return x;
                x = y;
            }
        }

        /// <summary>floor(sqrt(n)) for 64-bit integers.</summary>
        public static ulong Sqrt(ulong n)
        {
            if (n < 2) return n;

            int shift = (64 - LeadingZeroCount(n) + 1) / 2;
            ulong x = shift >= 64 ? ulong.MaxValue : 1UL << shift; // >= sqrt(n)
            while (true)
            {
                ulong y = (x + n / x) >> 1;
                if (x + n / x < x) y = (x >> 1) + ((n / x) >> 1) + 1; // overflow guard (only for huge n)
                if (y >= x) return x;
                x = y;
            }
        }

        public static int LeadingZeroCount(ulong n)
        {
            if (n == 0) return 64;
            int count = 0;
            if ((n & 0xFFFFFFFF00000000UL) == 0) { count += 32; n <<= 32; }
            if ((n & 0xFFFF000000000000UL) == 0) { count += 16; n <<= 16; }
            if ((n & 0xFF00000000000000UL) == 0) { count += 8; n <<= 8; }
            if ((n & 0xF000000000000000UL) == 0) { count += 4; n <<= 4; }
            if ((n & 0xC000000000000000UL) == 0) { count += 2; n <<= 2; }
            if ((n & 0x8000000000000000UL) == 0) { count += 1; }
            return count;
        }

        /// <summary>|value| as an unsigned number (works for long.MinValue).</summary>
        public static ulong Abs(long value) => value < 0 ? (ulong)(-(value + 1)) + 1UL : (ulong)value;
    }
}
