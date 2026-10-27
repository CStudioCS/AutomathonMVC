using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Automathon
{
    /// <summary>
    /// Deterministic 64-bit fixed point number in Q32.32 format, stored in a single <see cref="long"/>.
    ///
    /// <para>
    /// This is the "wide" companion of <see cref="Fixed"/>: it has a much bigger range ([-2147483648, 2147483648[)
    /// and precision (2^-32 ~ 0.00000000023). The product of two <see cref="Fixed"/> values always fits exactly
    /// in a <see cref="FixedLong"/> (see <see cref="Multiply(Fixed, Fixed)"/>), which makes it the right type for
    /// accumulations, squared lengths, dot/cross products, inverse masses... anything where Q16.16 would overflow or
    /// lose too much precision.
    /// </para>
    /// <para>
    /// It behaves like a <see cref="double"/>: implicit conversions from int, <see cref="Fixed"/>, float and double,
    /// explicit conversion back to <see cref="Fixed"/> (rounded) and floating point types, and all the usual operators.
    /// 128-bit intermediates are used for * and / so results are always correctly rounded. Overflows saturate.
    /// </para>
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(FixedLongJsonConverter))]
    public readonly struct FixedLong : IEquatable<FixedLong>, IComparable<FixedLong>, IComparable, IFormattable
    {
        public const int FRACTIONAL_BITS = 32;
        public const long ONE_RAW = 1L << FRACTIONAL_BITS;
        private const long HALF_RAW = ONE_RAW >> 1;
        private const int SHIFT_FROM_FIXED = FRACTIONAL_BITS - Fixed.FRACTIONAL_BITS;

        /// <summary>The underlying Q32.32 value, i.e. the represented number multiplied by 2^32.</summary>
        public readonly long Raw;

        private FixedLong(long raw) { Raw = raw; }

        #region Constants

        public static readonly FixedLong Zero = new FixedLong(0);
        public static readonly FixedLong One = new FixedLong(ONE_RAW);
        public static readonly FixedLong Half = new FixedLong(HALF_RAW);
        public static readonly FixedLong MaxValue = new FixedLong(long.MaxValue);
        public static readonly FixedLong MinValue = new FixedLong(long.MinValue);
        public static readonly FixedLong Epsilon = new FixedLong(1);
        public static readonly FixedLong Pi = new FixedLong(13493037705L);

        #endregion

        #region Construction / conversion

        public static FixedLong FromRaw(long raw) => new FixedLong(raw);

        public static FixedLong Ratio(long numerator, long denominator) => (FixedLong)numerator / (FixedLong)denominator;

        /// <summary>Exact product of two Q16.16 numbers (no rounding at all).</summary>
        public static FixedLong Multiply(Fixed a, Fixed b) => new FixedLong((long)a.Raw * b.Raw);

        public static implicit operator FixedLong(int value) => new FixedLong((long)value << FRACTIONAL_BITS);
        public static implicit operator FixedLong(Fixed value) => new FixedLong((long)value.Raw << SHIFT_FROM_FIXED);
        public static implicit operator FixedLong(float value) => FromDouble(value);
        public static implicit operator FixedLong(double value) => FromDouble(value);
        public static explicit operator FixedLong(long value)
            => value > int.MaxValue ? MaxValue : value < int.MinValue ? MinValue : new FixedLong(value << FRACTIONAL_BITS);

        public static FixedLong FromDouble(double value)
        {
            if (double.IsNaN(value)) return Zero;
            double scaled = value * ONE_RAW;
            if (scaled >= long.MaxValue) return MaxValue;
            if (scaled <= long.MinValue) return MinValue;
            return new FixedLong((long)Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        /// <summary>Rounds to the nearest <see cref="Fixed"/> (saturating).</summary>
        public static explicit operator Fixed(FixedLong value)
        {
            long raw = value.Raw;
            long half = 1L << (SHIFT_FROM_FIXED - 1);
            long shifted = raw > long.MaxValue - half ? long.MaxValue >> SHIFT_FROM_FIXED : (raw + half) >> SHIFT_FROM_FIXED;
            return Fixed.FromRawSaturated(shifted);
        }

        public static explicit operator int(FixedLong value) => (int)(value.Raw / ONE_RAW);
        public static explicit operator long(FixedLong value) => value.Raw / ONE_RAW;
        public static explicit operator float(FixedLong value) => (float)((double)value);
        public static explicit operator double(FixedLong value)
        {
            // split to keep the full precision of the integer part
            long integer = value.Raw >> FRACTIONAL_BITS;
            long fraction = value.Raw & (ONE_RAW - 1);
            return integer + fraction / (double)ONE_RAW;
        }

        public Fixed ToFixed() => (Fixed)this;
        public float ToFloat() => (float)this;
        public double ToDouble() => (double)this;

        #endregion

        #region Operators

        public static FixedLong operator +(FixedLong a) => a;
        public static FixedLong operator -(FixedLong a) => a.Raw == long.MinValue ? MaxValue : new FixedLong(-a.Raw);

        public static FixedLong operator +(FixedLong a, FixedLong b)
        {
            long r = unchecked(a.Raw + b.Raw);
            if (((a.Raw ^ r) & (b.Raw ^ r)) < 0) // both operands have the same sign and the result doesn't
                return a.Raw < 0 ? MinValue : MaxValue;
            return new FixedLong(r);
        }

        public static FixedLong operator -(FixedLong a, FixedLong b)
        {
            long r = unchecked(a.Raw - b.Raw);
            if (((a.Raw ^ b.Raw) & (a.Raw ^ r)) < 0)
                return a.Raw < 0 ? MinValue : MaxValue;
            return new FixedLong(r);
        }

        public static FixedLong operator *(FixedLong a, FixedLong b)
        {
            bool negative = (a.Raw < 0) != (b.Raw < 0);
            ulong ua = UInt128Math.Abs(a.Raw);
            ulong ub = UInt128Math.Abs(b.Raw);

            UInt128Math.Multiply(ua, ub, out ulong hi, out ulong lo);
            UInt128Math.Add(ref hi, ref lo, (ulong)HALF_RAW); // round to nearest
            UInt128Math.ShiftRight(ref hi, ref lo, FRACTIONAL_BITS);

            return FromMagnitude(hi, lo, negative);
        }

        public static FixedLong operator /(FixedLong a, FixedLong b)
        {
            if (b.Raw == 0)
                return a.Raw > 0 ? MaxValue : a.Raw < 0 ? MinValue : Zero;

            bool negative = (a.Raw < 0) != (b.Raw < 0);
            ulong ua = UInt128Math.Abs(a.Raw);
            ulong ub = UInt128Math.Abs(b.Raw);

            // (ua << 32) / ub on 128 bits
            ulong hi = ua >> (64 - FRACTIONAL_BITS);
            ulong lo = ua << FRACTIONAL_BITS;
            UInt128Math.Divide(hi, lo, ub, out ulong qHi, out ulong qLo, out ulong rem);

            if (rem >= ub - rem) // rem * 2 >= ub, round half away from zero
                UInt128Math.Add(ref qHi, ref qLo, 1);

            return FromMagnitude(qHi, qLo, negative);
        }

        public static FixedLong operator %(FixedLong a, FixedLong b)
            => b.Raw == 0 || (b.Raw == -1) ? Zero : new FixedLong(a.Raw % b.Raw);

        private static FixedLong FromMagnitude(ulong hi, ulong lo, bool negative)
        {
            if (negative)
            {
                if (hi != 0 || lo > (ulong)long.MaxValue + 1UL) return MinValue;
                return new FixedLong(unchecked(-(long)lo));
            }

            if (hi != 0 || lo > long.MaxValue) return MaxValue;
            return new FixedLong((long)lo);
        }

        public static bool operator ==(FixedLong a, FixedLong b) => a.Raw == b.Raw;
        public static bool operator !=(FixedLong a, FixedLong b) => a.Raw != b.Raw;
        public static bool operator <(FixedLong a, FixedLong b) => a.Raw < b.Raw;
        public static bool operator >(FixedLong a, FixedLong b) => a.Raw > b.Raw;
        public static bool operator <=(FixedLong a, FixedLong b) => a.Raw <= b.Raw;
        public static bool operator >=(FixedLong a, FixedLong b) => a.Raw >= b.Raw;

        #endregion

        #region Helpers

        public FixedLong Abs() => Raw < 0 ? -this : this;
        public int Sign() => Raw > 0 ? 1 : Raw < 0 ? -1 : 0;

        #endregion

        #region Equality / comparison / formatting

        public bool Equals(FixedLong other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is FixedLong other && Equals(other);
        public override int GetHashCode() => Raw.GetHashCode();
        public int CompareTo(FixedLong other) => Raw.CompareTo(other.Raw);
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj is FixedLong other) return CompareTo(other);
            throw new ArgumentException("Object must be of type FixedLong");
        }

        /// <summary>Decimal representation (up to 10 decimals, '.' separator, no exponent).</summary>
        public override string ToString()
        {
            bool negative = Raw < 0;
            ulong abs = UInt128Math.Abs(Raw);
            ulong integerPart = abs >> FRACTIONAL_BITS;
            ulong fraction = abs & ((ulong)ONE_RAW - 1UL);

            // 10 rounded decimals: fraction * 10^10 / 2^32 computed on 128 bits
            const ulong POW10 = 10000000000UL;
            UInt128Math.Multiply(fraction, POW10, out ulong hi, out ulong lo);
            UInt128Math.Add(ref hi, ref lo, 1UL << (FRACTIONAL_BITS - 1));
            UInt128Math.ShiftRight(ref hi, ref lo, FRACTIONAL_BITS);
            ulong decimals = lo;
            if (decimals >= POW10) { integerPart++; decimals -= POW10; }

            string decimalString = decimals.ToString(CultureInfo.InvariantCulture).PadLeft(10, '0').TrimEnd('0');
            string result = integerPart.ToString(CultureInfo.InvariantCulture);
            if (decimalString.Length > 0) result += "." + decimalString;
            return negative && (integerPart != 0 || decimals != 0) ? "-" + result : result;
        }

        public string ToString(string format, IFormatProvider formatProvider)
            => string.IsNullOrEmpty(format) ? ToString() : ((double)this).ToString(format, formatProvider);

        public static FixedLong Parse(string s)
        {
            if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal dec))
            {
                // decimal can't hold value * 2^32 for big values, so do integer and fractional parts separately
                decimal integer = decimal.Truncate(dec);
                if (integer >= int.MaxValue + 1m) return MaxValue;
                if (integer < int.MinValue) return MinValue;
                decimal fraction = (dec - integer) * ONE_RAW;
                return new FixedLong(((long)integer << FRACTIONAL_BITS) + (long)Math.Round(fraction, MidpointRounding.AwayFromZero));
            }
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                return FromDouble(d);
            throw new FormatException($"'{s}' is not a valid FixedLong number");
        }

        #endregion
    }
}
