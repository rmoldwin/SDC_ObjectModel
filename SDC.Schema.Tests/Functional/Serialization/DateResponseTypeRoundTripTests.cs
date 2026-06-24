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
	/// Coverage note: every date/date-part <c>*_DEtype</c> is now built through the standard response
	/// API. <c>gMonth</c>/<c>gYearMonth</c> and <c>dayTimeDuration</c> previously could not be
	/// constructed because the generated <c>DataTypes_DEType</c> choice bound <c>gYearMonth</c> to the
	/// wrong CLR type and had no <c>dayTimeDuration</c> element; that generated-binding bug was
	/// hand-corrected under issue #10, so all twelve types are exercised through the real builder here.
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

				// gMonth/gYearMonth/dayTimeDuration are now buildable through the standard API after the
				// issue #10 binding hand-correction, so they get the same exact-round-trip coverage as the
				// other string-backed g-types and durations.
				NumericResponseTypeTestHelpers.AssertValRoundTrip<gMonth_DEtype>(
					ItemChoiceType.gMonth, n => n.val = "--06", n => n.val == "--06", rt,
					$"xs:gMonth '--06' must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<gYearMonth_DEtype>(
					ItemChoiceType.gYearMonth, n => n.val = "2026-06", n => n.val == "2026-06", rt,
					$"xs:gYearMonth '2026-06' must survive a {sz} round-trip.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<dayTimeDuration_DEtype>(
					ItemChoiceType.dayTimeDuration, n => n.val = "P4DT12H30M5S", n => n.val == "P4DT12H30M5S", rt,
					$"xs:dayTimeDuration must survive a {sz} round-trip.");
			}
		}

		// ─── DateTime-backed: instant survives every serializer ────────────────────

		[TestMethod]
		public void DateTimeBacked_Values_RoundTripThroughAllSerializers()
		{
			// Rationale: the DateTime instant (set via the lexical boundary) must survive ALL four wire
			// formats exactly. DateTime equality compares the absolute instant (ticks), so even though
			// XML preserves DateTimeKind.Unspecified while JSON/BSON/MsgPack normalise to UTC, the value
			// (== the XSD lexical instant) must be identical after every round-trip. BSON previously came
			// back shifted by the host UTC offset because BsonDataReader.DateTimeKindHandling defaults to
			// Local (re-projecting the stored UTC instant into local time); SdcSerializerBson now reads as
			// Utc, restoring exact parity. See DateTimeValidation_XSD_vs_NET.md §2.4.
			var date = new DateTime(2026, 6, 22);
			var dtm = new DateTime(2026, 6, 22, 10, 30, 0);
			var utc = new DateTime(2026, 6, 22, 10, 0, 0, DateTimeKind.Utc);

			for (int i = 0; i < AllSerializers.Length; i++)
			{
				var rt = AllSerializers[i];
				string sz = SerializerName(i);

				NumericResponseTypeTestHelpers.AssertValRoundTrip<date_DEtype>(
					ItemChoiceType.date, n => n.SetLexicalValue("2026-06-22"), n => n.val == date, rt,
					$"xs:date must survive a {sz} round-trip with the exact instant.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<dateTime_DEtype>(
					ItemChoiceType.dateTime, n => n.SetLexicalValue("2026-06-22T10:30:00"), n => n.val == dtm, rt,
					$"xs:dateTime must survive a {sz} round-trip with the exact instant.");

				NumericResponseTypeTestHelpers.AssertValRoundTrip<dateTimeStamp_DEtype>(
					ItemChoiceType.dateTimeStamp, n => n.SetLexicalValue("2026-06-22T10:00:00Z"), n => n.val == utc, rt,
					$"xs:dateTimeStamp (UTC) must survive a {sz} round-trip with the exact instant.");
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
