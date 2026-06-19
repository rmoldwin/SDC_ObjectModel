using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SDC.Schema.Tests.Functional.Serialization
{
	[TestClass]
	public class _SerializationTests
	{
		private static string TestFilePath(string fileName)
			=> Path.Combine("..", "..", "..", "Test Files", fileName);

		private static string ReadTestXml(string fileName)
			=> File.ReadAllText(TestFilePath(fileName), Encoding.UTF8);

		private static string NormalizeXml(string xml)
		{
			var doc = XDocument.Parse(xml);
			return doc.ToString(SaveOptions.DisableFormatting);
		}

		private static void AssertStrictXmlRoundtrip<T>(
			string fileName,
			Func<string, T> deserialize,
			Func<T, string> serialize)
			where T : class
		{
			var originalXml = ReadTestXml(fileName);
			var model1 = deserialize(originalXml);
			Assert.IsNotNull(model1, $"Deserialization returned null for {fileName}.");

			var roundtripXml1 = serialize(model1);
			var model2 = deserialize(roundtripXml1);
			Assert.IsNotNull(model2, $"Second deserialization returned null for {fileName}.");

			var roundtripXml2 = serialize(model2);

			// Rationale: strict normalized equality is enforced between consecutive serializer-normalized outputs to catch regressions deterministically.
			Assert.AreEqual(
				NormalizeXml(roundtripXml1),
				NormalizeXml(roundtripXml2),
				$"Roundtrip stability mismatch detected for {fileName}.");
		}

		private static string AddLegalAdHocAttributes(string xml, string rootElementName)
		{
			var token = $"<{rootElementName} ";
			var attrs = "xmlns:ext='urn:test:ext' xmlns:alt='urn:test:alt' ext:legal='A&amp;B' alt:other='&lt;ok&gt;' ";
			return xml.Replace(token, $"<{rootElementName} {attrs}");
		}

		private static void AssertStableRoundtrip<T>(string xml, Func<string, T> deserialize, Func<T, string> serialize, string scenario)
			where T : class
		{
			var model1 = deserialize(xml);
			Assert.IsNotNull(model1, $"Deserialization returned null for {scenario}.");

			var xml1 = serialize(model1);
			var model2 = deserialize(xml1);
			Assert.IsNotNull(model2, $"Second deserialization returned null for {scenario}.");

			var xml2 = serialize(model2);

			// Rationale: serializer-normalized XML must stabilize over repeated deserialize/serialize cycles.
			Assert.AreEqual(NormalizeXml(xml1), NormalizeXml(xml2), $"Roundtrip stability mismatch for {scenario}.");
		}

		private static void AssertInvalidXmlRejected(Func<string, object> deserialize, string invalidXml)
		{
			try
			{
				deserialize(invalidXml);
				Assert.Fail("Expected invalid XML to be rejected.");
			}
			catch (Exception ex)
			{
				// Rationale: malformed ad-hoc attribute content should fail predictably and not silently deserialize.
				Assert.IsTrue(
					ex is InvalidOperationException || ex is XmlException || ex.InnerException is XmlException,
					$"Unexpected exception type for invalid XML: {ex.GetType().FullName}");
			}
		}

		[TestMethod]
		public void RoundtripFormDesign()
		{
			AssertStrictXmlRoundtrip(
				"BreastStagingTest.xml",
				xml => FormDesignType.DeserializeFromXml(xml),
				model => TopNodeSerializer<FormDesignType>.GetXml(model));
		}

		[TestMethod]
		public void RoundtripDemogFormDesign()
		{
			AssertStrictXmlRoundtrip(
				"Demog CCO Lung Surgery.xml",
				xml => DemogFormDesignType.DeserializeFromXml(xml),
				model => TopNodeSerializer<DemogFormDesignType>.GetXml(model));
		}

		[TestMethod]
		public void RoundtripDataElement()
		{
			AssertStrictXmlRoundtrip(
				"DE sample.xml",
				xml => DataElementType.DeserializeFromXml(xml),
				model => TopNodeSerializer<DataElementType>.GetXml(model));
		}

		[TestMethod]
		public void RoundtripPackage()
		{
			AssertStrictXmlRoundtrip(
				"..Sample SDCPackage.xml",
				xml => RetrieveFormPackageType.DeserializeFromXml(xml),
				model => TopNodeSerializer<RetrieveFormPackageType>.GetXml(model));
		}

		[TestMethod]
		public void RoundtripComplexPackage()
		{
			AssertStrictXmlRoundtrip(
				"Complex SDCPackage.xml",
				xml => RetrieveFormPackageType.DeserializeFromXml(xml),
				model => TopNodeSerializer<RetrieveFormPackageType>.GetXml(model));
		}

		[TestMethod]
		public void RoundtripIntegratedDiseaseReport()
		{
			var idrType = Type.GetType("SDC.Schema.IntegratedDiseaseReportType, SDC.Schema");

			// Rationale: this guards current behavior until a concrete IDR top-node type is added to the object model.
			Assert.IsNull(idrType, "IntegratedDiseaseReportType is now available; replace this unsupported-scenario test with a true roundtrip test.");

			var xml = ReadTestXml("IntegratedDiseaseReport sample.xml");
			AssertInvalidXmlRejected(
				x => FormDesignType.DeserializeFromXml(x),
				xml);
		}

		[TestMethod]
		public void RoundtripMap()
		{
			var mapXml = ReadTestXml("Map sample.xml");
			var mapWithLegalAdHocAttrs = AddLegalAdHocAttributes(mapXml, "Map");

			AssertStableRoundtrip(
				mapWithLegalAdHocAttrs,
				xml => MappingType.DeserializeFromXml(xml),
				model => TopNodeSerializer<MappingType>.GetXml(model),
				"Map legal mixed-namespace ad-hoc attributes");

			var illegalMapXml = mapWithLegalAdHocAttrs.Replace("templateID=\"urn:example:template:source\"", "templateID=\"urn:example:template:<source\"");
			AssertInvalidXmlRejected(
				x => MappingType.DeserializeFromXml(x),
				illegalMapXml);
		}
	}
}
