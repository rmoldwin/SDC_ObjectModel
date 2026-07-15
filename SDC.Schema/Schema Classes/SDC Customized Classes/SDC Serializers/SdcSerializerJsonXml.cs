#pragma warning disable
namespace SDC.Schema
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	/// <summary>
	/// XML-isomorphic JSON (de)serializer for SDC object trees.
	/// <para>
	/// Produces JSON whose structure mirrors the SDC XML output exactly, preserving
	/// document order. Each JSON object represents an XML element; its XML attributes
	/// appear as <c>"@attributeName"</c> flat keys, and its child elements appear in
	/// document order inside a <c>"childNodes"</c> array. Each entry in that array is
	/// a single-key wrapper object <c>{"ElementName": { … }}</c>, so the element type
	/// is always identifiable and sibling elements of different types can be interleaved
	/// without key-collision.
	/// </para>
	/// <para>
	/// This format is ideal for side-by-side XML/JSON comparison, teaching the SDC
	/// object model, and consumption by raw JSON tree viewers.
	/// Round-trips through <see cref="SdcSerializer{T}"/>, so fidelity is identical
	/// to XML round-tripping.
	/// </para>
	/// <para>
	/// Unlike <see cref="SdcSerializerJson{T}"/>, this serializer does not use
	/// <c>TypeNameHandling.All</c> or <c>"$type"</c> discriminators. The XML element
	/// name itself serves as the type discriminator, as in the XML format.
	/// </para>
	/// </summary>
	public static partial class SdcSerializerJsonXml<T> where T : ITopNode
	{
		#region Serialize

		/// <summary>
		/// Serializes an SDC object tree to an XML-isomorphic JSON string.<br/>
		/// XML element names appear as single-key wrapper objects inside each
		/// <c>"childNodes"</c> array, preserving document order exactly.
		/// XML attributes are emitted as <c>"@attributeName"</c> flat keys before
		/// <c>"childNodes"</c>.
		/// </summary>
		/// <param name="obj">The SDC top-node object to serialize.</param>
		/// <returns>Indented XML-isomorphic JSON string.</returns>
		public static string SerializeJsonXml(T obj)
		{
			string xmlString = SdcSerializer<T>.Serialize(obj);
			var doc = new XmlDocument();
			doc.LoadXml(xmlString);
			var root = new JObject { [doc.DocumentElement.LocalName] = ElementToJObject(doc.DocumentElement) };
			return root.ToString(Newtonsoft.Json.Formatting.Indented);
		}

		/// <summary>
		/// Recursively converts an <see cref="XmlElement"/> to a <see cref="JObject"/>.<br/>
		/// Attributes → <c>"@attrName"</c> flat keys (namespace declarations use
		/// <c>"@xmlns"</c> / <c>"@xmlns:prefix"</c>).<br/>
		/// Child elements → <c>"childNodes"</c> array of <c>{"TagName":{…}}</c> wrappers
		/// in document order.<br/>
		/// Text content → <c>"#text"</c> key (leaf elements only).
		/// </summary>
		private static JObject ElementToJObject(XmlElement el)
		{
			var jObj = new JObject();

			// Attributes — @-prefixed, namespace declarations as @xmlns / @xmlns:prefix
			foreach (XmlAttribute attr in el.Attributes)
			{
				string key = attr.Prefix == "xmlns" ? "@xmlns:" + attr.LocalName
				           : attr.Name   == "xmlns" ? "@xmlns"
				           : attr.Prefix != ""      ? "@" + attr.Name
				           :                          "@" + attr.LocalName;
				jObj[key] = attr.Value;
			}

			// Text content on leaf elements
			string text = string.Concat(el.ChildNodes.Cast<XmlNode>()
			                              .OfType<XmlText>().Select(t => t.Value));
			if (!string.IsNullOrEmpty(text))
				jObj["#text"] = text;

			// Child elements → "childNodes" array, each item {"TagName": {…}}
			var childEls = el.ChildNodes.Cast<XmlNode>().OfType<XmlElement>().ToList();
			if (childEls.Count > 0)
			{
				var arr = new JArray();
				foreach (var child in childEls)
					arr.Add(new JObject { [child.LocalName] = ElementToJObject(child) });
				jObj["childNodes"] = arr;
			}

			return jObj;
		}

		#endregion

		#region Deserialize

		/// <summary>
		/// Deserializes an XML-isomorphic JSON string produced by <see cref="SerializeJsonXml"/>
		/// back into a typed SDC object tree.<br/>
		/// The JSON must have a single root key (e.g. <c>"FormDesign"</c>) whose value is
		/// an element object. Child elements are read from <c>"childNodes"</c> arrays in
		/// order, restoring the original document structure before passing to
		/// <see cref="SdcSerializer{T}.Deserialize"/>.
		/// </summary>
		/// <param name="jsonXmlInput">XML-isomorphic JSON string to deserialize.</param>
		/// <returns>Hydrated SDC object tree.</returns>
		/// <exception cref="JsonException">Thrown when the JSON is empty or cannot be parsed.</exception>
		public static T DeserializeJsonXml(string jsonXmlInput)
		{
			// Use MaxDepth=null (unlimited) — deeply nested SDC documents (e.g. TestFlow,
			// ColoRectal) exceed Newtonsoft's default MaxDepth of 64.
			JObject root;
			using (var sr = new System.IO.StringReader(jsonXmlInput))
			using (var jr = new JsonTextReader(sr) { MaxDepth = null })
				root = JObject.Load(jr);
			var rootProp = root.Properties().FirstOrDefault()
				?? throw new JsonException(
					"XML-isomorphic JSON must have a single root element key (e.g. \"FormDesign\").");
			if (rootProp.Value is not JObject rootObj)
				throw new JsonException(
					$"Root key \"{rootProp.Name}\" must be a JSON object.");

			var sw = new StringWriter();
			using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
				WriteElement(writer, rootProp.Name, rootObj, inheritedNs: "");

			return SdcSerializer<T>.Deserialize(sw.ToString());
		}

		/// <summary>
		/// Recursively writes an XML element from its <see cref="JObject"/> representation.<br/>
		/// <c>"@attrName"</c> keys → XML attributes; <c>"childNodes"</c> array →
		/// child elements in order; <c>"#text"</c> → text node content.
		/// Namespace inheritance is tracked via <paramref name="inheritedNs"/>.
		/// </summary>
		private static void WriteElement(XmlWriter writer, string name, JObject obj, string inheritedNs)
		{
			string ns = obj["@xmlns"]?.ToString() ?? inheritedNs;
			writer.WriteStartElement(name, ns);

			foreach (var prop in obj.Properties())
			{
				if (!prop.Name.StartsWith("@")) continue;
				string attrKey = prop.Name.Substring(1);
				string attrVal = prop.Value.ToString();

				if (attrKey == "xmlns")
					continue;  // already handled in WriteStartElement above
				else if (attrKey.StartsWith("xmlns:"))
					writer.WriteAttributeString("xmlns", attrKey.Substring(6),
					                            "http://www.w3.org/2000/xmlns/", attrVal);
				else if (attrKey.Contains(':'))
				{
					int colon = attrKey.IndexOf(':');
					writer.WriteAttributeString(attrKey.Substring(0, colon),
					                            attrKey.Substring(colon + 1), null, attrVal);
				}
				else
					writer.WriteAttributeString(attrKey, attrVal);
			}

			if (obj["#text"] is JToken textToken)
				writer.WriteString(textToken.ToString());

			if (obj["childNodes"] is JArray childNodes)
			{
				foreach (var item in childNodes)
				{
					if (item is JObject wrapper)
					{
						var childProp = wrapper.Properties().FirstOrDefault();
						if (childProp?.Value is JObject childObj)
							WriteElement(writer, childProp.Name, childObj, ns);
					}
				}
			}

			writer.WriteEndElement();
		}

		/// <summary>
		/// Tries to deserialize an XML-isomorphic JSON string into an SDC object tree.
		/// </summary>
		/// <param name="jsonXmlInput">XML-isomorphic JSON string to deserialize.</param>
		/// <param name="obj">The deserialized SDC object tree on success; default on failure.</param>
		/// <param name="exception">The exception on failure; null on success.</param>
		/// <returns><see langword="true"/> on success; <see langword="false"/> on failure.</returns>
		public static bool DeserializeJsonXml(string jsonXmlInput, out T obj, out Exception exception)
		{
			exception = null;
			obj = default;
			try
			{
				obj = DeserializeJsonXml(jsonXmlInput);
				return true;
			}
			catch (Exception ex)
			{
				exception = ex;
				return false;
			}
		}

		/// <summary>
		/// Tries to deserialize an XML-isomorphic JSON string into an SDC object tree.
		/// </summary>
		public static bool DeserializeJsonXml(string jsonXmlInput, out T obj)
		{
			Exception exception = null;
			return DeserializeJsonXml(jsonXmlInput, out obj, out exception);
		}

		#endregion

		#region File I/O

		/// <summary>
		/// Serializes an SDC object tree to an XML-isomorphic JSON file (UTF-8 encoding).
		/// </summary>
		/// <param name="obj">The SDC top-node object to serialize.</param>
		/// <param name="fileName">Full path of the output file.</param>
		public static void SaveToFileJsonXml(T obj, string fileName)
		{
			StreamWriter streamWriter = null;
			try
			{
				string jsonString = SerializeJsonXml(obj);
				streamWriter = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);
				streamWriter.Write(jsonString);
				streamWriter.Close();
			}
			finally
			{
				streamWriter?.Dispose();
			}
		}

		/// <summary>
		/// Reads an XML-isomorphic JSON file and deserializes it into an SDC object tree.
		/// </summary>
		/// <param name="fileName">Full path of the input file.</param>
		/// <returns>Hydrated SDC object tree.</returns>
		public static T LoadFromFileJsonXml(string fileName)
		{
			FileStream file = null;
			StreamReader sr = null;
			try
			{
				file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				sr = new StreamReader(file, System.Text.Encoding.UTF8);
				string jsonString = sr.ReadToEnd();
				sr.Close();
				file.Close();
				return DeserializeJsonXml(jsonString);
			}
			finally
			{
				file?.Dispose();
				sr?.Dispose();
			}
		}

		#endregion
	}
}
#pragma warning restore
