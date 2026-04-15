using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Automathon.Engine.Utility
{
    public static class IntMath
    {
        // Fast Log2 via unrolled right-shifts (no BitOperations needed)
        private static int Log2(uint n)
        {
            int r = 0;
            if (n >= 0x10000) { r += 16; n >>= 16; }
            if (n >= 0x100) { r += 8; n >>= 8; }
            if (n >= 0x10) { r += 4; n >>= 4; }
            if (n >= 0x4) { r += 2; n >>= 2; }
            if (n >= 0x2) { r += 1; n >>= 1; }
            return r;
        }

        private static int Log2(ulong n)
        {
            int r = 0;
            if (n >= 0x100000000UL) { r += 32; n >>= 32; }
            if (n >= 0x10000) { r += 16; n >>= 16; }
            if (n >= 0x100) { r += 8; n >>= 8; }
            if (n >= 0x10) { r += 4; n >>= 4; }
            if (n >= 0x4) { r += 2; n >>= 2; }
            if (n >= 0x2) { r += 1; }
            return r;
        }

        public static int Isqrt(int n)
            => (int)Isqrt((uint)n);

        public static long Isqrt(long n)
            => (long)Isqrt((ulong)n);

        public static uint Isqrt(uint n)
        {
            if (n < 2) return n;

            uint x = 1u << ((Log2(n) / 2) + 1);

            uint y = (x + n / x) >> 1;
            while (y < x)
            {
                x = y;
                y = (x + n / x) >> 1;
            }

            return x;
        }

        // 64-bit
        public static ulong Isqrt(ulong n)
        {
            if (n < 2) return n;

            ulong x = 1ul << ((Log2(n) / 2) + 1);

            ulong y = (x + n / x) >> 1;
            while (y < x)
            {
                x = y;
                y = (x + n / x) >> 1;
            }

            return x;
        }
    }
}