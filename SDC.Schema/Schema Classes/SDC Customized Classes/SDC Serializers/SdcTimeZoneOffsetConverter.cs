// Shared Json.NET converter for timezone offsets used by the date/dateTime/time OM types.
#pragma warning disable
namespace SDC.Schema
{
    using System;
    using Newtonsoft.Json;

    internal sealed class SdcTimeZoneOffsetConverter : JsonConverter<TimeSpan?>
    {
        public override void WriteJson(JsonWriter writer, TimeSpan? value, JsonSerializer serializer)
        {
            if (!value.HasValue)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteValue(XsdDateTimePatterns.FormatOffset(value.Value));
        }

        public override TimeSpan? ReadJson(JsonReader reader, Type objectType, TimeSpan? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.Value is string s)
            {
                if (!XsdDateTimePatterns.TryParseOffset(s, out var offset) || offset is null)
                    throw new JsonSerializationException($"'{s}' is not a valid timezone offset.");
                return offset;
            }

            throw new JsonSerializationException("Timezone offset values must be serialized as strings.");
        }
    }
}
#pragma warning restore
