using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace SDC.Schema.Tests.Functional.Serialization
{
	/// <summary>
	/// Tests for <see cref="SdcSerializerJsonXml{T}"/> — the XML-isomorphic JSON serializer.
	/// Covers round-tripping, document-order preservation, attribute conventions, and file I/O
	/// across a range of SDC documents from trivially simple to full clinical forms.
	/// </summary>
	[TestClass]
	public class SdcSerializerJsonXmlTests
	{
		private static readonly string TestFilesDir =
			Path.Combine("..", "..", "..", "Test Files");

		private static string TestFile(string name) => Path.Combine(TestFilesDir, name);

		private static readonly string AdrenalPartialPath = TestFile("Adrenal_partial.xml");

		// ── Shared helpers ────────────────────────────────────────────────────────

		/// <summary>
		/// Recursively walks every <c>"childNodes"</c> array in the JSON tree and asserts
		/// that the <c>@order</c> attribute values within each array are strictly monotonically
		/// increasing. Any failure indicates that document order was not preserved during
		/// serialization — the original ordering bug this serializer was designed to fix.
		/// </summary>
		private static void AssertAllChildNodesOrdered(JToken token, string path = "root")
		{
			if (token is JObject obj)
			{
				if (obj["childNodes"] is JArray childNodes)
				{
					// Collect (index, @order) pairs for entries that carry @order
					var orderEntries = childNodes
						.Select((item, idx) =>
						{
							string orderStr = (item as JObject)
								?.Properties().FirstOrDefault()
								?.Value?["@order"]?.ToString();
							return (idx, orderStr);
						})
						.Where(e => e.orderStr != null && int.TryParse(e.orderStr, out _))
						.Select(e => (e.idx, order: int.Parse(e.orderStr)))
						.ToList();

					// Rationale: @order must strictly increase across all entries in the array;
					// any non-increasing pair means a node was placed out of document order.
					for (int i = 1; i < orderEntries.Count; i++)
					{
						Assert.IsTrue(
							orderEntries[i].order > orderEntries[i - 1].order,
							$"childNodes @order not increasing at {path}: " +
							$"item[{orderEntries[i - 1].idx}]={orderEntries[i - 1].order} " +
							$"then item[{orderEntries[i].idx}]={orderEntries[i].order}");
					}

					// Recurse into each child wrapper
					foreach (var (item, idx) in childNodes.Select((v, i) => (v, i)))
						AssertAllChildNodesOrdered(item, $"{path}/childNodes[{idx}]");
				}

				// Recurse into all other properties
				foreach (var prop in obj.Properties().Where(p => p.Name != "childNodes"))
					AssertAllChildNodesOrdered(prop.Value, $"{path}/{prop.Name}");
			}
			else if (token is JArray arr)
			{
				foreach (var (item, idx) in arr.Select((v, i) => (v, i)))
					AssertAllChildNodesOrdered(item, $"{path}[{idx}]");
			}
		}

		/// <summary>
		/// Parses a JSON string with unlimited nesting depth (MaxDepth = null).
		/// SDC documents can be deeply nested; the Newtonsoft default of 64 is insufficient
		/// for complex clinical forms like TestFlow and ColoRectal.
		/// </summary>
		private static JObject ParseJsonUnlimited(string json)
		{
			using var sr = new StringReader(json);
			using var jr = new Newtonsoft.Json.JsonTextReader(sr) { MaxDepth = null };
			return JObject.Load(jr);
		}

		/// <summary>
		/// Counts XML elements in a serialized SDC object, using a fresh XML document parse.
		/// </summary>
		private static int CountElements(FormDesignType fd)
		{
			var doc = new XmlDocument();
			doc.LoadXml(TopNodeSerializer<FormDesignType>.GetXml(fd, refreshSdc: false));
			return doc.SelectNodes("//*")!.Count;
		}

		/// <summary>
		/// Full round-trip helper: load XML → serialize to JsonXml → deserialize back.
		/// Returns (original, roundTripped, jsonXml).
		/// </summary>
		private static (FormDesignType original, FormDesignType roundTripped, string jsonXml)
			RoundTripFormDesign(string filePath)
		{
			BaseType.ResetLastTopNode();
			var original = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(filePath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(original);
			BaseType.ResetLastTopNode();
			var roundTripped = SdcSerializerJsonXml<FormDesignType>.DeserializeJsonXml(jsonXml);
			return (original, roundTripped, jsonXml);
		}

		// ── Basic round-trip (existing tests, kept) ───────────────────────────────

		[TestMethod]
		public void RoundTrip_JsonXml_ProducesIdenticalXml()
		{
			// Rationale: serializing to XML-isomorphic JSON and deserializing back must
			// reconstruct an object graph whose re-serialized XML has the same element count.
			var (fd, fd2, _) = RoundTripFormDesign(AdrenalPartialPath);
			Assert.AreEqual(CountElements(fd), CountElements(fd2),
				"Round-tripped XML element count must equal original.");
		}

		[TestMethod]
		public void RoundTrip_ViaExtensionMethod_Succeeds()
		{
			// Rationale: the ITopNodeSerializeExtensions.GetJsonXml() must produce the
			// same output as calling the static serializer directly.
			BaseType.ResetLastTopNode();
			var fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string viaStatic    = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);
			string viaExtension = fd.GetJsonXml(refreshSdc: false);
			Assert.AreEqual(viaStatic, viaExtension,
				"Extension method GetJsonXml() must produce identical output to the static serializer.");
		}

		// ── Data-driven round-trips across document sizes ─────────────────────────

		[DataTestMethod]
		[DataRow("Adrenal_partial.xml",                          DisplayName = "AdrenalPartial")]
		[DataRow("Adrenal.Bx.Res.129_3.004.001.REL_sdcFDF.xml", DisplayName = "AdrenalFull")]
		[DataRow("Test.Flow.237_4.001.001.UNK_sdcFDF.xml",       DisplayName = "TestFlow")]
		[DataRow("ColoRectal.Res.126_3.004.001.REL_sdcFDF.xml",  DisplayName = "ColoRectal")]
		public void RoundTrip_ElementCountPreserved_FormDesign(string fileName)
		{
			// Rationale: after a JsonXml round-trip the re-serialized XML must contain
			// exactly the same number of elements as the original — no nodes lost or duplicated.
			var (fd, fd2, _) = RoundTripFormDesign(TestFile(fileName));
			Assert.AreEqual(CountElements(fd), CountElements(fd2),
				$"Element count mismatch after JsonXml round-trip for {fileName}.");
		}

		[TestMethod]
		public void RoundTrip_DemogFormDesign_ElementCountPreserved()
		{
			// Rationale: DemogFormDesignType (different root type/serializer) must also
			// round-trip without losing or duplicating nodes.
			BaseType.ResetLastTopNode();
			var fd = TopNodeSerializer<DemogFormDesignType>.DeserializeFromXmlPath(
				TestFile("Demog CCO Lung Surgery.xml"));
			string jsonXml = SdcSerializerJsonXml<DemogFormDesignType>.SerializeJsonXml(fd);
			BaseType.ResetLastTopNode();
			var fd2 = SdcSerializerJsonXml<DemogFormDesignType>.DeserializeJsonXml(jsonXml);

			var doc1 = new XmlDocument(); doc1.LoadXml(TopNodeSerializer<DemogFormDesignType>.GetXml(fd,  refreshSdc: false));
			var doc2 = new XmlDocument(); doc2.LoadXml(TopNodeSerializer<DemogFormDesignType>.GetXml(fd2, refreshSdc: false));
			Assert.AreEqual(doc1.SelectNodes("//*")!.Count, doc2.SelectNodes("//*")!.Count,
				"DemogFormDesignType round-trip must preserve element count.");
		}

		// ── Document-order preservation ───────────────────────────────────────────

		[DataTestMethod]
		[DataRow("Adrenal_partial.xml",                          DisplayName = "AdrenalPartial")]
		[DataRow("Adrenal.Bx.Res.129_3.004.001.REL_sdcFDF.xml", DisplayName = "AdrenalFull")]
		[DataRow("Test.Flow.237_4.001.001.UNK_sdcFDF.xml",       DisplayName = "TestFlow")]
		[DataRow("ColoRectal.Res.126_3.004.001.REL_sdcFDF.xml",  DisplayName = "ColoRectal")]
		public void OrderInvariant_AllChildNodesMonotonic_FormDesign(string fileName)
		{
			// Rationale: every "childNodes" array in the entire JSON tree must have
			// strictly increasing @order values — this is the definitive proof that
			// document order is preserved and no same-typed siblings were grouped
			// (which was the bug in the Newtonsoft SerializeXmlNode approach).
			BaseType.ResetLastTopNode();
			var fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(TestFile(fileName));
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);
			var root = ParseJsonUnlimited(jsonXml);
			AssertAllChildNodesOrdered(root);
		}

		[TestMethod]
		public void OrderInvariant_MixedTypeSiblings_AppearInterleaved_AdrenalPartial()
		{
			// Rationale: in Adrenal_partial.xml the Body > ChildItems element contains
			// these children in document order (by @order value):
			//   order=170  DisplayedItem  DI_39617
			//   order=190  Section        S_4257
			//   order=270  Section        S_17537
			//   order=380  Section        S_17875
			//   order=690  Question       Q_2168
			//   order=740  DisplayedItem  DI_39617_1   ← was incorrectly grouped to top in old format
			//   order=760  Question       Q_49275
			// This test verifies that DI_39617_1 appears BETWEEN Q_2168 and Q_49275,
			// not grouped with DI_39617 at the top — the key correctness invariant.
			BaseType.ResetLastTopNode();
			var fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);
			var root = ParseJsonUnlimited(jsonXml);

			// Navigate to Body > ChildItems > childNodes
			var bodyChildNodes = root
				.SelectToken("FormDesign.childNodes[?(@.Body)].Body.childNodes[?(@.ChildItems)].ChildItems.childNodes")
				as JArray;
			Assert.IsNotNull(bodyChildNodes, "Body > ChildItems > childNodes must be present.");

			// Extract (elementName, @order) for each wrapper entry
			var entries = bodyChildNodes
				.Select(item =>
				{
					var prop = (item as JObject)?.Properties().FirstOrDefault();
					if (prop == null) return (name: (string)null, order: -1);
					string orderStr = prop.Value?["@order"]?.ToString();
					return (name: prop.Name, order: int.TryParse(orderStr, out int o) ? o : -1);
				})
				.Where(e => e.name != null && e.order >= 0)
				.ToList();

			Assert.IsTrue(entries.Count >= 7,
				$"Body > ChildItems should have at least 7 children; found {entries.Count}.");

			// Find the two DisplayedItems
			var di1 = entries.FirstOrDefault(e => e.order == 170);
			var di2 = entries.FirstOrDefault(e => e.order == 740);
			var q1  = entries.FirstOrDefault(e => e.order == 690);
			var q2  = entries.FirstOrDefault(e => e.order == 760);

			Assert.AreEqual("DisplayedItem", di1.name, "order=170 must be a DisplayedItem.");
			Assert.AreEqual("DisplayedItem", di2.name, "order=740 must be a DisplayedItem.");
			Assert.AreEqual("Question",      q1.name,  "order=690 must be a Question.");
			Assert.AreEqual("Question",      q2.name,  "order=760 must be a Question.");

			int idxDi1 = entries.IndexOf(di1);
			int idxDi2 = entries.IndexOf(di2);
			int idxQ1  = entries.IndexOf(q1);
			int idxQ2  = entries.IndexOf(q2);

			// DI_39617_1 (order=740) must appear AFTER Q_2168 (order=690) and BEFORE Q_49275 (order=760)
			Assert.IsTrue(idxDi2 > idxQ1,
				$"DisplayedItem order=740 (index {idxDi2}) must appear after Question order=690 (index {idxQ1}).");
			Assert.IsTrue(idxDi2 < idxQ2,
				$"DisplayedItem order=740 (index {idxDi2}) must appear before Question order=760 (index {idxQ2}).");

			// The two DisplayedItems must NOT be adjacent (they are not contiguous in document order)
			Assert.IsTrue(Math.Abs(idxDi1 - idxDi2) > 1,
				$"The two DisplayedItems (indices {idxDi1} and {idxDi2}) must not be adjacent " +
				$"— they are separated by Sections and Questions in document order.");
		}

		[TestMethod]
		public void OrderInvariant_RoundTrip_PreservesNodePositions_AdrenalPartial()
		{
			// Rationale: after a round-trip the SDC node dictionary must contain nodes
			// in the same @order sequence as the original. We compare the ordered list of
			// (name, ID) pairs from both graphs to confirm no positions were swapped.
			var (fd, fd2, _) = RoundTripFormDesign(AdrenalPartialPath);

			// Collect (name, order) from each graph via XPath on re-serialized XML
			XmlNamespaceManager MakeNs(XmlDocument doc)
			{
				var ns = new XmlNamespaceManager(doc.NameTable);
				ns.AddNamespace("s", "urn:ihe:qrph:sdc:2016");
				return ns;
			}

			var doc1 = new XmlDocument();
			doc1.LoadXml(TopNodeSerializer<FormDesignType>.GetXml(fd,  refreshSdc: false));
			var doc2 = new XmlDocument();
			doc2.LoadXml(TopNodeSerializer<FormDesignType>.GetXml(fd2, refreshSdc: false));

			// Extract all elements with their @order as integers, in document order
			List<(string tag, int order)> GetOrderedNodes(XmlDocument doc)
			{
				return doc.SelectNodes("//*[@order]", MakeNs(doc))!
					.Cast<XmlElement>()
					.Select(e => (e.LocalName, int.TryParse(e.GetAttribute("order"), out int o) ? o : -1))
					.Where(t => t.Item2 >= 0)
					.ToList();
			}

			var nodes1 = GetOrderedNodes(doc1);
			var nodes2 = GetOrderedNodes(doc2);

			Assert.AreEqual(nodes1.Count, nodes2.Count,
				"Node count must be identical after round-trip.");

			// Rationale: for each position in the ordered node list, the tag name and @order
			// must match — this confirms not only that counts match but that no nodes swapped positions.
			for (int i = 0; i < nodes1.Count; i++)
			{
				Assert.AreEqual(nodes1[i].tag,   nodes2[i].tag,
					$"Node[{i}] element name mismatch: original='{nodes1[i].tag}' roundTripped='{nodes2[i].tag}'.");
				Assert.AreEqual(nodes1[i].order, nodes2[i].order,
					$"Node[{i}] @order mismatch: original={nodes1[i].order} roundTripped={nodes2[i].order}.");
			}
		}

		// ── Structure: element names and childNodes ───────────────────────────────

		[TestMethod]
		public void Structure_XmlElementNamesAreJsonKeys()
		{
			// Rationale: XML-isomorphic JSON must use XML element names (FormDesign, Body,
			// ChildItems, Section, Question, etc.) as wrapper keys inside "childNodes" arrays.
			// CLR property names ("Item", "Items") and "$type" discriminators must never appear.
			BaseType.ResetLastTopNode();
			FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);

			// Root element name must be the outermost JSON key
			Assert.IsTrue(jsonXml.Contains("\"FormDesign\""),
				"Root JSON key must be 'FormDesign' (the XML root element name).");

			// Core structural element names must appear as childNodes wrapper keys
			Assert.IsTrue(jsonXml.Contains("\"Body\""),        "JSON must contain key 'Body'.");
			Assert.IsTrue(jsonXml.Contains("\"ChildItems\""),  "JSON must contain key 'ChildItems'.");
			Assert.IsTrue(jsonXml.Contains("\"Section\""),     "JSON must contain key 'Section'.");
			Assert.IsTrue(jsonXml.Contains("\"Question\""),    "JSON must contain key 'Question'.");
			Assert.IsTrue(jsonXml.Contains("\"childNodes\""),  "JSON must contain 'childNodes' arrays.");

			// CLR property names and type discriminators must NOT appear
			Assert.IsFalse(jsonXml.Contains("\"Items\""),
				"JSON must NOT contain key 'Items' (CLR property name).");
			Assert.IsFalse(jsonXml.Contains("\"$type\""),
				"JSON must NOT contain '$type' discriminators.");
		}

		[TestMethod]
		public void Structure_RepeatedSiblingsAreInChildNodesOrder()
		{
			// Rationale: repeated siblings of different types (Section, DisplayedItem, Question)
			// must appear interleaved in document order inside "childNodes", not grouped by type.
			// The @order attribute values in the "childNodes" array must be monotonically
			// increasing to confirm document order is preserved.
			BaseType.ResetLastTopNode();
			FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);

			var root = ParseJsonUnlimited(jsonXml);

			// Navigate to Body/ChildItems/childNodes — the mixed-type level
			var bodyChildNodes = root
				.SelectToken("FormDesign.childNodes[?(@.Body)].Body.childNodes[?(@.ChildItems)].ChildItems.childNodes")
				as JArray;

			Assert.IsNotNull(bodyChildNodes,
				"Body > ChildItems > childNodes array must be present.");
			Assert.IsTrue(bodyChildNodes.Count > 1,
				"Body > ChildItems > childNodes must contain multiple child entries.");

			// Extract @order values from each wrapped element and verify monotonic increase
			var orders = bodyChildNodes
				.Select(item => item is JObject w
					? w.Properties().FirstOrDefault()?.Value?["@order"]?.ToString()
					: null)
				.Where(o => o != null)
				.Select(o => int.TryParse(o, out int n) ? n : -1)
				.Where(n => n >= 0)
				.ToList();

			Assert.IsTrue(orders.Count > 1, "childNodes must contain entries with @order attributes.");
			for (int i = 1; i < orders.Count; i++)
				Assert.IsTrue(orders[i] > orders[i - 1],
					$"childNodes @order values must be strictly increasing: {orders[i - 1]} then {orders[i]}.");
		}

		// ── Attributes: @prefix ───────────────────────────────────────────────────

		[TestMethod]
		public void Attributes_XmlAttributesHaveAtPrefix()
		{
			// Rationale: XML attributes must appear in the JSON as "@attributeName" keys
			// (the standard Newtonsoft XML-to-JSON convention), distinguishing them clearly
			// from child elements.
			BaseType.ResetLastTopNode();
			FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);

			Assert.IsTrue(jsonXml.Contains("\"@ID\""),    "XML @ID attribute must appear as JSON key '@ID'.");
			Assert.IsTrue(jsonXml.Contains("\"@name\""),  "XML @name attribute must appear as JSON key '@name'.");
			Assert.IsTrue(jsonXml.Contains("\"@order\""), "XML @order attribute must appear as JSON key '@order'.");
			Assert.IsTrue(jsonXml.Contains("\"@xmlns\""), "XML namespace declaration must appear as JSON key '@xmlns'.");
		}

		[TestMethod]
		public void Attributes_NoXmlDeclarationInOutput()
		{
			// Rationale: the XML processing instruction (<?xml version='1.0'...?>) must NOT
			// appear in the JSON output — we serialize only the root element.
			BaseType.ResetLastTopNode();
			FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);

			Assert.IsFalse(jsonXml.Contains("\"?xml\""),
				"XML declaration must not appear as a '?xml' key in the JSON output.");
		}

		// ── Try-pattern overloads ─────────────────────────────────────────────────

		[TestMethod]
		public void TryDeserialize_ReturnsTrue_OnValidInput()
		{
			// Rationale: the bool-returning overload must return true and populate obj
			// when given valid XML-isomorphic JSON.
			BaseType.ResetLastTopNode();
			FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);
			string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);

			bool ok = SdcSerializerJsonXml<FormDesignType>.DeserializeJsonXml(jsonXml, out var result, out var ex);
			Assert.IsTrue(ok,         "DeserializeJsonXml must return true for valid input.");
			Assert.IsNotNull(result,  "out obj must be non-null on success.");
			Assert.IsNull(ex,         "out exception must be null on success.");
		}

		[TestMethod]
		public void TryDeserialize_ReturnsFalse_OnInvalidInput()
		{
			// Rationale: the bool-returning overload must return false and set exception
			// when given malformed input, without throwing.
			bool ok = SdcSerializerJsonXml<FormDesignType>.DeserializeJsonXml(
				"{ not valid json xml }", out var result, out var ex);

			Assert.IsFalse(ok,       "DeserializeJsonXml must return false for malformed input.");
			Assert.IsNull(result,    "out obj must be default/null on failure.");
			Assert.IsNotNull(ex,     "out exception must be non-null on failure.");
		}

		// ── File I/O ──────────────────────────────────────────────────────────────

		[TestMethod]
		public void FileIO_SaveAndLoad_RoundTrips()
		{
			// Rationale: SaveToFileJsonXml followed by LoadFromFileJsonXml must produce
			// an object graph with the same node count as the original.
			BaseType.ResetLastTopNode();
			FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);

			string tempFile = Path.GetTempFileName() + ".jsonxml";
			try
			{
				SdcSerializerJsonXml<FormDesignType>.SaveToFileJsonXml(fd, tempFile);
				Assert.IsTrue(File.Exists(tempFile),      "SaveToFileJsonXml must create the output file.");
				Assert.IsTrue(new FileInfo(tempFile).Length > 0, "Output file must be non-empty.");

				BaseType.ResetLastTopNode();
				FormDesignType fd2 = SdcSerializerJsonXml<FormDesignType>.LoadFromFileJsonXml(tempFile);
				Assert.IsNotNull(fd2, "LoadFromFileJsonXml must return a non-null object.");

				var doc1 = new XmlDocument(); doc1.LoadXml(TopNodeSerializer<FormDesignType>.GetXml(fd,  refreshSdc: false));
				var doc2 = new XmlDocument(); doc2.LoadXml(TopNodeSerializer<FormDesignType>.GetXml(fd2, refreshSdc: false));
				Assert.AreEqual(doc1.SelectNodes("//*")!.Count, doc2.SelectNodes("//*")!.Count,
					"File round-trip must preserve the XML element count.");
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}
	}
}
