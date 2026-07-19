// ------------------------------------------------------------------------------
//  Hand-written customization (NOT xsd2code++ generated).
//  Added to fix GitHub issue #27: JSON/BSON deserialization throws
//  Newtonsoft.Json.JsonSerializationException ("XmlNodeConverter only supports
//  deserializing XmlDocument, XmlElement or XmlNode") whenever a tree contains
//  a populated ExtensionType.AnyAttr (List<XmlAttribute>).
// ------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SDC.Schema
{
    /// <summary>
    /// Custom <see cref="JsonConverter"/> for <see cref="List{XmlAttribute}"/> (used by
    /// <see cref="ExtensionType.AnyAttr"/> to hold ad-hoc/custom attributes added via
    /// <c>AddOrUpdateAdHocAttribute()</c>).
    /// <br/><br/>
    /// Newtonsoft's built-in <see cref="Newtonsoft.Json.Converters.XmlNodeConverter"/> activates
    /// automatically for any <see cref="XmlNode"/>-derived type (including <see cref="XmlAttribute"/>)
    /// even without being registered, because <see cref="XmlAttribute"/> is a subtype of
    /// <see cref="XmlNode"/>. Its <c>WriteJson</c> handles <see cref="XmlAttribute"/> fine, but its
    /// <c>ReadJson</c> explicitly only supports <see cref="XmlDocument"/>, <see cref="XmlElement"/>,
    /// or plain <see cref="XmlNode"/> — so deserialization of a populated <c>AnyAttr</c> list always
    /// throws. This converter fully replaces that path for <see cref="List{XmlAttribute}"/>: it
    /// serializes each attribute as a plain <c>{Prefix, LocalName, NamespaceURI, Value}</c> JSON
    /// object and reconstructs <see cref="XmlAttribute"/> instances on read via
    /// <see cref="XmlDocument.CreateAttribute(string, string, string)"/>, so mixed namespaces and
    /// escaped/illegal XML content round-trip correctly through both JSON and BSON.
    /// <br/><br/>
    /// Note: <see cref="ExtensionType.Any"/> (<see cref="List{XmlElement}"/>) is NOT affected by this
    /// bug — Newtonsoft's stock <c>XmlNodeConverter.ReadJson</c> already supports <see cref="XmlElement"/>
    /// natively, so only <see cref="List{XmlAttribute}"/> needs this custom converter.
    /// </summary>
    public sealed class XmlAttributeListJsonConverter : JsonConverter
    {
        // A single shared XmlDocument is sufficient: XmlDocument.CreateAttribute() only uses the
        // document as an attribute factory/owner; the resulting XmlAttribute nodes are detached
        // (not appended to any element) and are freely reassignable to any other document/tree,
        // exactly as AddOrUpdateAdHocAttribute() already does when building these lists.
        private static readonly XmlDocument s_factoryDoc = new XmlDocument();

        public override bool CanConvert(Type objectType) => objectType == typeof(List<XmlAttribute>);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            var list = (List<XmlAttribute>)value;
            writer.WriteStartArray();
            foreach (var attr in list)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Prefix");
                writer.WriteValue(attr.Prefix);
                writer.WritePropertyName("LocalName");
                writer.WriteValue(attr.LocalName);
                writer.WritePropertyName("NamespaceURI");
                writer.WriteValue(attr.NamespaceURI);
                writer.WritePropertyName("Value");
                writer.WriteValue(attr.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var array = JArray.Load(reader);
            var result = new List<XmlAttribute>(array.Count);
            foreach (var item in array)
            {
                string? prefix = (string?)item["Prefix"];
                string? localName = (string?)item["LocalName"];
                string? ns = (string?)item["NamespaceURI"];
                string? val = (string?)item["Value"];

                var attr = s_factoryDoc.CreateAttribute(prefix, localName, ns);
                attr.Value = val;
                result.Add(attr);
            }
            return result;
        }
    }
}
