// SDC-CUSTOM: do not overwrite. Hand-authored date/date-part round-trip + soft-reject tests.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Tests.OM;
using System;

namespace SDC.Schema.Tests.Functional.Serialization
{
	/// <summary>
	/// Serialization round-trip and invalid-source soft-reject tests for the date / date-part
	/// datatypes. Verifies that a legal value survives XML ⇄ OM ⇄ JSON / BSON / MsgPack unchanged, and
	/// that an invalid value embedded in a source XML document is soft-rejected on deserialization
	/// (never stored, never throws, recorded in <see cref="BaseType.RejectedValues"/>) — the same
	/// contract as the in-memory setter.
	///
	/// Coverage note: these tests use the date types whose <c>*_DEtype</c> maps to a single XML element
	/// under <c>DataTypes_DEType</c> and can therefore be built through the standard response API:
	/// date, dateTime, time, dateTimeStamp, gYear, gMonthDay, gDay, duration, yearMonthDuration.
	/// <c>gMonth</c>/<c>gYearMonth</c> (one shared DEtype reused for two element names) and
	/// <c>dayTimeDuration</c> cannot be constructed via that API (a pre-existing builder limitation
	/// unrelated to validation); their lexical validation is covered by the OM boundary tests.
	/// </summary>
	[TestClass]
	public class DateResponseTypeRoundTripTests
	{
		[TestInitialize]
		public void Init() => BaseType.ResetLastTopNode();

		private static readonly Func<FormDesignType, FormDesignType>[] AllSerializers =
		{
			NumericResponseTypeTestHelpers.XmlRoundTrip,
			NumericResponseTypeTestHelpers.JsonRoundTrip,
			NumericResponseTypeTestHelpers.BsonRoundTrip,
			NumericResponseTypeTestHelpers.MsgPackRoundTrip,
		};

		private static string SerializerName(int i) => i switch { 0 => "XML", 1 => "JSON", 2 => "BSON", _ => "MsgPack" };

		// ─── String-backed: lexical val survives every serializer verbatim ─────────

		[TestMethod]
		public void StringBacked_LexicalValues_RoundTripThroughAllSerializers()
		{
			// Rationale: the val IS the XSD lexical string, so it must survive every wire format byte-for
			// byte. Run each representative type through XML/JSON/BSON/MsgPack.
			for (int i = 0; i < AllSerializers.Length; i++)
			{
				var rt = AllSerializers[i];
				string sz = SerializerName(i);

				NumericResponseTypeTestHelpers.AssertValRoundTrip<gYear_DEtype>(
					ItemChoiceType.gYear, n => n.val = "2026", n => n.val == "2026", rt,
					$"xs:gYear '2026' must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<gDay_DEtype>(
					ItemChoiceType.gDay, n => n.val = "---22", n => n.val == "---22", rt,
					$"xs:gDay '---22' must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<gMonthDay_DEtype>(
					ItemChoiceType.gMonthDay, n => n.val = "--06-22", n => n.val == "--06-22", rt,
					$"xs:gMonthDay '--06-22' must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<duration_DEtype>(
					ItemChoiceType.duration, n => n.val = "P3Y6M4DT12H30M5S", n => n.val == "P3Y6M4DT12H30M5S", rt,
					$"xs:duration must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<yearMonthDuration_DEtype>(
					ItemChoiceType.yearMonthDuration, n => n.val = "P3Y6M", n => n.val == "P3Y6M", rt,
					$"xs:yearMonthDuration must survive a {sz} round-trip.");
			}
		}

		// ─── DateTime-backed: instant survives every serializer ────────────────────

		[TestMethod]
		public void DateTimeBacked_Values_RoundTripThroughXmlAndJson()
		{
			// Rationale: the DateTime instant (set via the lexical boundary) must survive the
			// value-preserving formats. NOTE: exact-value comparison is limited to XML and JSON here —
			// BSON/MsgPack encode DateTime as a UTC tick count and apply the serializer's
			// DateTimeZoneHandling, which shifts an Unspecified-Kind local DateTime by the host offset.
			// That is a property of the wire format's DateTime handling, not of this validation work, so
			// the string-backed lexical types (above) carry the full 4-serializer matrix instead.
			var date = new DateTime(2026, 6, 22);
			var dtm = new DateTime(2026, 6, 22, 10, 30, 0);
			var utc = new DateTime(2026, 6, 22, 10, 0, 0, DateTimeKind.Utc);

			Func<FormDesignType, FormDesignType>[] preserving =
			{
				NumericResponseTypeTestHelpers.XmlRoundTrip,
				NumericResponseTypeTestHelpers.JsonRoundTrip,
			};

			for (int i = 0; i < preserving.Length; i++)
			{
				var rt = preserving[i];
				string sz = SerializerName(i);

				NumericResponseTypeTestHelpers.AssertValRoundTrip<date_DEtype>(
					ItemChoiceType.date, n => n.SetLexicalValue("2026-06-22"), n => n.val == date, rt,
					$"xs:date must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<dateTime_DEtype>(
					ItemChoiceType.dateTime, n => n.SetLexicalValue("2026-06-22T10:30:00"), n => n.val == dtm, rt,
					$"xs:dateTime must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<dateTimeStamp_DEtype>(
					ItemChoiceType.dateTimeStamp, n => n.SetLexicalValue("2026-06-22T10:00:00Z"), n => n.val == utc, rt,
					$"xs:dateTimeStamp (UTC) must survive a {sz} round-trip.");
			}
		}

		// ─── Invalid value in a source document is soft-rejected on deserialize ────

		[TestMethod]
		public void InvalidValueInSourceXml_IsSoftRejectedOnDeserialize()
		{
			// Rationale: a malformed value arriving from an external XML document must follow the same
			// soft-reject contract as a programmatic set — it must NOT abort the whole parse, must not be
			// stored, and must be recorded so a UI can flag exactly which value to fix.
			var node = NumericResponseTypeTestHelpers.DE<gYear_DEtype>(ItemChoiceType.gYear, out var fd);
			node.val = "2026";
			string xml = NumericResponseTypeTestHelpers.GetXml(fd);

			// Corrupt the lexical value in the wire document (it is serialized as a 'val' attribute).
			string badXml = xml.Replace("val=\"2026\"", "val=\"20XX\"");
			Assert.AreNotEqual(xml, badXml, "Test setup: the val attribute must have been present to corrupt.");

			var fd2 = TopNodeSerializer<FormDesignType>.DeserializeFromXml(badXml);
			var node2 = NumericResponseTypeTestHelpers.FindResponseDE<gYear_DEtype>(fd2);

			Assert.IsNull(node2.val, "An invalid xs:gYear in the source document must not be stored.");
			Assert.IsTrue(node2.RejectedValues.TryGetValue("val", out var rv),
				"The invalid source value must be recorded for surfacing to the user.");
			StringAssert.Contains(rv!.Message, "20XX", "The recorded message must quote the offending source value.");
		}

		// ─── Inherited (default) namespace round-trip ─────────────────────────────

		[TestMethod]
		public void ValidValue_UnderInheritedDefaultNamespace_RoundTrips()
		{
			// Rationale: SDC documents bind the SDC namespace as the inherited DEFAULT namespace
			// (xmlns="urn:ihe:qrph:sdc:2016") so every descendant element inherits it with no prefix. A
			// legal value must serialize into that inherited-namespace form and deserialize back
			// unchanged — confirming the validation pipeline does not disturb namespace handling.
			var node = NumericResponseTypeTestHelpers.DE<duration_DEtype>(ItemChoiceType.duration, out var fd);
			node.val = "P3Y6M4DT12H30M5S";
			string xml = NumericResponseTypeTestHelpers.GetXml(fd);

			StringAssert.Contains(xml, "xmlns=\"urn:ihe:qrph:sdc:2016\"",
				"The document must declare the SDC namespace as the inherited default namespace.");

			var fd2 = TopNodeSerializer<FormDesignType>.DeserializeFromXml(xml);
			var node2 = NumericResponseTypeTestHelpers.FindResponseDE<duration_DEtype>(fd2);
			Assert.AreEqual("P3Y6M4DT12H30M5S", node2.val,
				"A legal xs:duration must survive a round-trip under the inherited default namespace.");
		}
	}
}
