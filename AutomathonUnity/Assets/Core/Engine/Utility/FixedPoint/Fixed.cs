using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Automathon
{
    /// <summary>
    /// Deterministic 32-bit fixed point number in Q16.16 format (16 integer bits including the sign, 16 fractional bits),
    /// stored in a single <see cref="int"/>. Intermediate results are computed with <see cref="long"/>s, so no precision
    /// is lost before the final rounding.
    ///
    /// <para>It is meant to be used just like a <see cref="float"/>:</para>
    /// <code>
    /// Fixed speed = 7.5;                // implicit conversion from double / float / int
    /// Fixed d = speed * 2 / 3 + 0.25;   // the usual operators
    /// Fixed root = FixedMath.Sqrt(d);   // math functions live in FixedMath
    /// float f = (float)d;               // explicit conversion back to floating point
    /// </code>
    ///
    /// <para>
    /// Range is [-32768, 32767.99998] with a resolution of 1/65536 (~0.0000153).
    /// Every operation is purely integer based and therefore bit-for-bit reproducible on every platform.
    /// Out of range results saturate to <see cref="MinValue"/>/<see cref="MaxValue"/> (much like floats that go to
    /// infinity) instead of silently wrapping around; dividing by zero also saturates.
    /// </para>
    /// <para>
    /// Note: converting a <see cref="double"/>/<see cref="float"/> to a <see cref="Fixed"/> is deterministic (it only depends on the
    /// value of the double), but computing that double at runtime usually is not, so prefer doing the gameplay math with Fixed.
    /// </para>
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(FixedJsonConverter))]
    public readonly struct Fixed : IEquatable<Fixed>, IComparable<Fixed>, IComparable, IFormattable
    {
        public const int FRACTIONAL_BITS = 16;
        public const int ONE_RAW = 1 << FRACTIONAL_BITS;
        private const int HALF_RAW = ONE_RAW >> 1;
        private const int FRACTION_MASK = ONE_RAW - 1;

        /// <summary>The underlying Q16.16 value, i.e. the represented number multiplied by 65536.</summary>
        public readonly int Raw;

        private Fixed(int raw) { Raw = raw; }

        #region Constants

        public static readonly Fixed Zero = new Fixed(0);
        public static readonly Fixed One = new Fixed(ONE_RAW);
        public static readonly Fixed NegativeOne = new Fixed(-ONE_RAW);
        public static readonly Fixed Half = new Fixed(HALF_RAW);
        public static readonly Fixed MaxValue = new Fixed(int.MaxValue);
        public static readonly Fixed MinValue = new Fixed(int.MinValue);
        /// <summary>Smallest positive value (1/65536).</summary>
        public static readonly Fixed Epsilon = new Fixed(1);

        public static readonly Fixed Pi = new Fixed(205887);
        public static readonly Fixed HalfPi = new Fixed(102944);
        public static readonly Fixed TwoPi = new Fixed(2 * 205887); // exactly 2 * Pi so that angle wrapping is consistent
        public static readonly Fixed Deg2Rad = new Fixed(1144);   // pi / 180
        public static readonly Fixed Rad2Deg = new Fixed(3754936); // 180 / pi

        #endregion

        #region Construction / conversion

        public static Fixed FromRaw(int raw) => new Fixed(raw);

        /// <summary>Exact numerator / denominator, rounded to the nearest representable value. Ex: Fixed.Ratio(3, 10) == 0.3</summary>
        public static Fixed Ratio(long numerator, long denominator)
            => FromRawSaturated(RoundedDivide(numerator << FRACTIONAL_BITS, denominator));

        /// <summary>Converts a value expressed in thousandths (the old integer convention) to a Fixed: FromMilli(1500) == 1.5</summary>
        public static Fixed FromMilli(long milli) => Ratio(milli, 1000);

        internal static Fixed FromRawSaturated(long raw)
        {
            if (raw > int.MaxValue) return MaxValue;
            if (raw < int.MinValue) return MinValue;
            return new Fixed((int)raw);
        }

        public static Fixed FromInt(int value) => FromRawSaturated((long)value << FRACTIONAL_BITS);

        public static Fixed FromDouble(double value)
        {
            if (double.IsNaN(value)) return Zero;
            double scaled = value * ONE_RAW;
            if (scaled >= int.MaxValue) return MaxValue;
            if (scaled <= int.MinValue) return MinValue;
            return new Fixed((int)Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        public static implicit operator Fixed(int value) => FromInt(value);
        public static implicit operator Fixed(float value) => FromDouble(value);
        public static implicit operator Fixed(double value) => FromDouble(value);
        public static explicit operator Fixed(long value)
            => value > short.MaxValue ? MaxValue : value < short.MinValue ? MinValue : new Fixed((int)value << FRACTIONAL_BITS);
        public static explicit operator Fixed(decimal value) => FromDouble((double)value);

        /// <summary>Truncates towards zero, like casting a float to an int.</summary>
        public static explicit operator int(Fixed value) => value.Raw / ONE_RAW;
        public static explicit operator long(Fixed value) => value.Raw / ONE_RAW;
        public static explicit operator float(Fixed value) => (float)value.Raw / ONE_RAW;
        public static explicit operator double(Fixed value) => (double)value.Raw / ONE_RAW; // exact
        public static explicit operator decimal(Fixed value) => (decimal)value.Raw / ONE_RAW; // exact

        public float ToFloat() => (float)this;
        public double ToDouble() => (double)this;

        /// <summary>Rounds to the nearest integer (halfway values are rounded away from zero).</summary>
        public int RoundToInt() => (int)(RoundedDivide(Raw, ONE_RAW));
        public int FloorToInt() => Raw >> FRACTIONAL_BITS;
        public int CeilToInt() => (int)(((long)Raw + FRACTION_MASK) >> FRACTIONAL_BITS);

        #endregion

        #region Operators

        public static Fixed operator +(Fixed a) => a;
        public static Fixed operator -(Fixed a) => a.Raw == int.MinValue ? MaxValue : new Fixed(-a.Raw);

        public static Fixed operator +(Fixed a, Fixed b) => FromRawSaturated((long)a.Raw + b.Raw);
        public static Fixed operator -(Fixed a, Fixed b) => FromRawSaturated((long)a.Raw - b.Raw);
        public static Fixed operator ++(Fixed a) => a + One;
        public static Fixed operator --(Fixed a) => a - One;

        /// <summary>Exact product computed on 64 bits, then rounded to the nearest representable value.</summary>
        public static Fixed operator *(Fixed a, Fixed b)
            => FromRawSaturated(((long)a.Raw * b.Raw + HALF_RAW) >> FRACTIONAL_BITS);

        /// <summary>Division rounded to the nearest representable value. Division by zero saturates (like float infinities).</summary>
        public static Fixed operator /(Fixed a, Fixed b)
        {
            if (b.Raw == 0)
                return a.Raw > 0 ? MaxValue : a.Raw < 0 ? MinValue : Zero;

            return FromRawSaturated(RoundedDivide((long)a.Raw << FRACTIONAL_BITS, b.Raw));
        }

        /// <summary>Remainder with the sign of the dividend (same as float/int % in C#). x % 0 == 0.</summary>
        public static Fixed operator %(Fixed a, Fixed b)
            => b.Raw == 0 ? Zero : new Fixed((int)((long)a.Raw % b.Raw));

        public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;
        public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;
        public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;
        public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;
        public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;
        public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;

        /// <summary>Integer division rounding halfway values away from zero.</summary>
        internal static long RoundedDivide(long numerator, long denominator)
        {
            if (denominator == 0)
                return numerator > 0 ? long.MaxValue : numerator < 0 ? long.MinValue : 0;

            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            long half = denominator / 2;
            return numerator >= 0 ? (numerator + half) / denominator : (numerator - half) / denominator;
        }

        #endregion

        #region Helpers

        public Fixed Abs() => Raw < 0 ? -this : this;
        public int Sign() => Raw > 0 ? 1 : Raw < 0 ? -1 : 0;
        public bool IsZero() => Raw == 0;

        #endregion

        #region Equality / comparison / formatting

        public bool Equals(Fixed other) => Raw == other.Raw;
        public override bool Equals(object obj) => obj is Fixed other && Equals(other);
        public override int GetHashCode() => Raw;
        public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj is Fixed other) return CompareTo(other);
            throw new ArgumentException("Object must be of type Fixed");
        }

        /// <summary>
        /// Shortest decimal representation that converts back to exactly this value (ex: 0.1 is printed "0.1" even
        /// though it's stored as 6554/65536). Always uses '.' as decimal separator, never uses exponents
        /// (so it's also a valid JSON number).
        /// </summary>
        public override string ToString()
        {
            long raw = Raw;
            bool negative = raw < 0;
            long abs = negative ? -raw : raw;
            long integerPart = abs >> FRACTIONAL_BITS;
            long fraction = abs & FRACTION_MASK;

            string sign = negative ? "-" : "";

            if (fraction == 0)
                return sign + integerPart.ToString(CultureInfo.InvariantCulture);

            // Find the least number of decimals that round trips. 5 decimals always suffice since 10^-5 < 2^-16.
            long pow10 = 1;
            for (int digits = 1; digits <= 6; digits++)
            {
                pow10 *= 10;
                long candidate = RoundedDivide(fraction * pow10, ONE_RAW);
                if (RoundedDivide(candidate * ONE_RAW, pow10) == fraction || digits == 6)
                {
                    long intPart = integerPart;
                    if (candidate == pow10) { intPart++; candidate = 0; }
                    string decimals = candidate.ToString(CultureInfo.InvariantCulture).PadLeft(digits, '0').TrimEnd('0');
                    return decimals.Length == 0
                        ? sign + intPart.ToString(CultureInfo.InvariantCulture)
                        : sign + intPart.ToString(CultureInfo.InvariantCulture) + "." + decimals;
                }
            }

            return ((double)this).ToString("R", CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
            => string.IsNullOrEmpty(format) ? ToString() : ((double)this).ToString(format, formatProvider);

        public string ToString(string format) => ToString(format, CultureInfo.InvariantCulture);

        /// <summary>Parses a number written with '.' as decimal separator (ex: "-12.375", "3", "1e-3").</summary>
        public static Fixed Parse(string s)
        {
            if (TryParse(s, out Fixed result))
                return result;
            throw new FormatException($"'{s}' is not a valid Fixed number");
        }

        public static bool TryParse(string s, out Fixed result)
        {
            // decimal parsing is exact (no binary rounding before our own rounding), double is the fallback for exponents
            if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal dec))
            {
                decimal scaled = dec * ONE_RAW;
                if (scaled >= int.MaxValue) result = MaxValue;
                else if (scaled <= int.MinValue) result = MinValue;
                else result = new Fixed((int)Math.Round(scaled, MidpointRounding.AwayFromZero));
                return true;
            }
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
            {
                result = FromDouble(d);
                return true;
            }

            result = Zero;
            return false;
        }

        #endregion
    }
}
