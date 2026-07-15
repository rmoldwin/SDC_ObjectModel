// Shared Json.NET converter for decimal values used by SdcSerializerJson and SdcSerializerBson.
#pragma warning disable
namespace SDC.Schema
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;

    /// <summary>
    /// Custom <see cref="JsonConverter{T}"/> for <see cref="decimal"/> that normalises trailing-zero
    /// scale so that the round-trip value matches what was originally loaded from XML.
    /// <para>
    /// Problem: Json.NET serialises <c>decimal 2M</c> as the JSON token <c>2.0</c> (floating-point
    /// notation), and <c>decimal.Parse("2.0")</c> returns <c>2.0M</c> (scale=1).  When
    /// <c>CompareTrees{T}</c> calls <c>.ToString()</c> on the attribute value it sees <c>"2"</c> vs
    /// <c>"2.0"</c> and reports <c>isAttListChanged=true</c> — a false positive round-trip failure.
    /// </para>
    /// <para>
    /// Fix: Serialise using the <c>G29</c> format specifier, which strips trailing fractional zeros
    /// (<c>2.0M</c> → <c>"2"</c>, <c>2.5M</c> → <c>"2.5"</c>).  Deserialise by parsing the raw
    /// token and then normalising the scale via a <c>G29</c> round-trip
    /// (<c>"2.0"</c> → <c>2M</c>, scale=0).
    /// </para>
    /// </summary>
    internal sealed class SdcJsonDecimalConverter : JsonConverter<decimal>
    {
        // Singleton for zero-allocation reuse in serializer settings.
        public static readonly SdcJsonDecimalConverter Instance = new();

        // Write the decimal without trailing fractional zeros so "2M" serialises as the JSON
        // integer token 2 rather than the float token 2.0. This preserves round-trip fidelity
        // with values originally read from XML integer/decimal attributes.
        // For binary writers (e.g. BsonDataWriter) WriteRawValue is not supported, so fall back
        // to WriteValue with a normalised decimal (trailing zeros stripped via G29 round-trip).
        public override void WriteJson(JsonWriter writer, decimal value, JsonSerializer serializer)
        {
            // Normalise: strip trailing fractional zeros (2.0M → 2M, 2.50M → 2.5M)
            string g29 = value.ToString("G29", CultureInfo.InvariantCulture);
            decimal normalised = decimal.Parse(g29, CultureInfo.InvariantCulture);

            if (writer is Newtonsoft.Json.Bson.BsonDataWriter || writer is Newtonsoft.Msgpack.MessagePackWriter)
                // Binary writers (BSON, MsgPack) do not support WriteRawValue; write as native decimal.
                writer.WriteValue(normalised);
            else
                // JSON text writers: write without quotes and without trailing .0
                writer.WriteRawValue(g29);
        }

        // Normalise scale on read-back: "2.0" → 2M (scale 0), "2.5" → 2.5M (scale 1).
        public override decimal ReadJson(JsonReader reader, Type objectType, decimal existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return existingValue;
            decimal d = Convert.ToDecimal(reader.Value, CultureInfo.InvariantCulture);
            // Strip trailing fractional zeros by round-tripping through G29 format.
            return decimal.Parse(d.ToString("G29", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }
    }
}
#pragma warning restore
