namespace SDC.Schema
{
    using MessagePack;
    using MessagePack.Formatters;
    using System.Xml;

    // Formatter to serialize System.Xml.XmlElement as its OuterXml string and
    // reconstruct an XmlElement on deserialization.
    internal class XmlElementFormatter : IMessagePackFormatter<XmlElement>
    {
        public void Serialize(ref MessagePackWriter writer, XmlElement value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }
            writer.Write(value.OuterXml);
        }

        public XmlElement Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) return null;
            var xml = reader.ReadString();
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc.DocumentElement;
        }
    }
}
