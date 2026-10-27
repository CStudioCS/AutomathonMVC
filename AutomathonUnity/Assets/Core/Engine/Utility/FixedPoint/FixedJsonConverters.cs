using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Automathon
{
    /// <summary>
    /// Serializes <see cref="Fixed"/> as a plain JSON number (ex: 1.5, -0.1, 12). <see cref="Fixed.ToString()"/> writes the
    /// shortest decimal that converts back to the exact same value, so the round trip through JSON (and through Python
    /// floats) is lossless. Reading accepts JSON integers, floats and numeric strings.
    /// </summary>
    public class FixedJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Fixed) || objectType == typeof(Fixed?);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) { writer.WriteNull(); return; }
            writer.WriteRawValue(((Fixed)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                case JsonToken.Undefined:
                    return objectType == typeof(Fixed?) ? (object)null : Fixed.Zero;
                case JsonToken.Integer:
                    if (reader.Value is System.Numerics.BigInteger big)
                        return big.Sign > 0 ? Fixed.MaxValue : Fixed.MinValue;
                    return (Fixed)Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
                case JsonToken.Float:
                    if (reader.Value is decimal dec) return (Fixed)dec;
                    return Fixed.FromDouble(Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture));
                case JsonToken.String:
                    return Fixed.Parse((string)reader.Value);
                default:
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading a Fixed number.");
            }
        }
    }

    /// <summary>Serializes <see cref="FixedLong"/> as a plain JSON number (up to 10 decimals).</summary>
    public class FixedLongJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(FixedLong) || objectType == typeof(FixedLong?);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null) { writer.WriteNull(); return; }
            writer.WriteRawValue(((FixedLong)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                case JsonToken.Undefined:
                    return objectType == typeof(FixedLong?) ? (object)null : FixedLong.Zero;
                case JsonToken.Integer:
                    if (reader.Value is System.Numerics.BigInteger big)
                        return big.Sign > 0 ? FixedLong.MaxValue : FixedLong.MinValue;
                    return (FixedLong)Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
                case JsonToken.Float:
                    return FixedLong.FromDouble(Convert.ToDouble(reader.Value, CultureInfo.InvariantCulture));
                case JsonToken.String:
                    return FixedLong.Parse((string)reader.Value);
                default:
                    throw new JsonSerializationException($"Unexpected token {reader.TokenType} when reading a FixedLong number.");
            }
        }
    }
}
