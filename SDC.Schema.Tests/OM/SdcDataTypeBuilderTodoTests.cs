// SDC-CUSTOM: do not overwrite. Phase 5 tests for SdcDataTypeBuilder TODO stubs.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using SDC.Schema.Tests.OM;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SDC.Schema.Tests.OM
{
	/// <summary>
	/// Tests for the Phase 5 SdcDataTypeBuilder TODO stub completions:
	/// HTML, XML, anyType (P5.1–P5.3), dayTimeDuration (P5.4), yearMonthDuration (P5.5),
	/// gDay/gMonth/gMonthDay dash normalization (P5.6), and the anyURI Uri.TryCreate fix.
	///
	/// All tests exercise the AddDataTypesDE builder path via the ResponseField_Item API,
	/// matching the pattern used in NumericResponseTypeBoundaryTests. No test here retests
	/// the val-setter soft-reject logic already covered in DateResponseTypeBoundaryTests.
	/// </summary>
	[TestClass]
	public class SdcDataTypeBuilderTodoTests
	{
		[TestInitialize]
		public void Init() => BaseType.ResetLastTopNode();

		// ─── Shared builder helper ─────────────────────────────────────────────────

		/// <summary>
		/// Calls AddDataTypesDE on a freshly created ResponseField for the given <paramref name="ict"/>
		/// with <paramref name="value"/> and returns both the errors list and the concrete DEtype node.
		/// </summary>
		private static (T dt, List<Exception> errors) Build<T>(ItemChoiceType ict, object? value)
			where T : BaseType
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.TodoTest");
			fd.AddBody();
			var q = fd.Body.AddChildQuestionResponse("Q.TodoTest", out _, "test question");
			var rf = q.ResponseField_Item!;
			var errors = new List<Exception>();
			SdcDataTypeBuilder.AddDataTypesDE(rf, ict, dtQuantEnum.EQ, value, errors);
			return ((T)rf.Response!.DataTypeDE_Item!, errors);
		}

		// ─── P5.1 — HTML ──────────────────────────────────────────────────────────

		[TestMethod]
		public void HTML_StringValue_ParsedAsXmlFragments_AssignedToAny()
		{
			// Rationale: a well-formed XML string passed as value for HTML_DEtype must be parsed
			// and its child elements assigned to dt.Any without error.
			var (dt, errors) = Build<HTML_DEtype>(ItemChoiceType.HTML, "<p>Hello</p><br/>");
			Assert.AreEqual(0, errors.Count, "A valid XML fragment string must produce no errors.");
			Assert.IsNotNull(dt.Any, "dt.Any must not be null after a successful parse.");
			Assert.AreEqual(2, dt.Any.Count, "Both <p> and <br/> sibling elements must be extracted.");
			Assert.AreEqual("p", dt.Any[0].LocalName, "First element must be <p>.");
			Assert.AreEqual("br", dt.Any[1].LocalName, "Second element must be <br/>.");
		}

		[TestMethod]
		public void HTML_InvalidXmlString_ErrorRecorded_AnyEmpty()
		{
			// Rationale: an ill-formed XML string must never be stored; the error must be
			// captured in the errors list and dt.Any must be set to an empty list.
			var (dt, errors) = Build<HTML_DEtype>(ItemChoiceType.HTML, "<p>unclosed");
			Assert.IsTrue(errors.Count > 0, "An ill-formed XML string for HTML_DEtype must be recorded as an error.");
			Assert.IsNotNull(dt.Any, "dt.Any must still be initialized (empty) after a parse failure.");
			Assert.AreEqual(0, dt.Any.Count, "dt.Any must be empty when the string cannot be parsed.");
		}

		[TestMethod]
		public void HTML_UnsupportedValueType_ErrorRecorded()
		{
			// Rationale: passing a non-string, non-List<XmlElement> value must produce a soft-reject;
			// no exception should propagate to the caller.
			var (dt, errors) = Build<HTML_DEtype>(ItemChoiceType.HTML, 42);
			Assert.IsTrue(errors.Count > 0, "An unsupported value type for HTML_DEtype must produce a recorded error.");
			Assert.AreEqual(0, dt.Any.Count, "dt.Any must be empty when value type is unsupported.");
		}

		[TestMethod]
		public void HTML_ListXmlElementValue_AssignedDirectly()
		{
			// Rationale: a pre-built List<XmlElement> must be assigned without any additional parsing.
			var xdoc = new XmlDocument();
			xdoc.LoadXml("<root><span>hi</span></root>");
			var list = new List<XmlElement> { xdoc.DocumentElement! };
			var (dt, errors) = Build<HTML_DEtype>(ItemChoiceType.HTML, list);
			Assert.AreEqual(0, errors.Count, "A List<XmlElement> must be assigned without error.");
			Assert.AreSame(list, dt.Any, "The supplied list must be assigned by reference, not copied.");
		}

		// ─── P5.2 — XML ───────────────────────────────────────────────────────────

		[TestMethod]
		public void XML_ValidXmlString_DocumentElementInAny()
		{
			// Rationale: a well-formed XML string must be parsed and its document element assigned
			// as the sole entry in dt.Any.
			var (dt, errors) = Build<XML_DEtype>(ItemChoiceType.XML, "<data><item>42</item></data>");
			Assert.AreEqual(0, errors.Count, "A valid XML string for XML_DEtype must produce no errors.");
			Assert.AreEqual(1, dt.Any.Count, "dt.Any must contain exactly one element (the document element).");
			Assert.AreEqual("data", dt.Any[0].LocalName, "The document element name must match.");
		}

		[TestMethod]
		public void XML_InvalidXmlString_ErrorRecorded_AnyEmpty()
		{
			// Rationale: an ill-formed XML string must never be stored; the error is soft-recorded
			// and dt.Any stays empty.
			var (dt, errors) = Build<XML_DEtype>(ItemChoiceType.XML, "<data>missing close");
			Assert.IsTrue(errors.Count > 0, "An ill-formed XML string for XML_DEtype must produce an error.");
			Assert.AreEqual(0, dt.Any.Count, "dt.Any must be empty when parsing fails.");
		}

		[TestMethod]
		public void XML_UnsupportedValueType_ErrorRecorded()
		{
			// Rationale: unsupported value types for XML_DEtype must be soft-rejected.
			var (dt, errors) = Build<XML_DEtype>(ItemChoiceType.XML, new object());
			Assert.IsTrue(errors.Count > 0, "An unsupported value type for XML_DEtype must be recorded as an error.");
		}

		// ─── P5.3 — anyType ───────────────────────────────────────────────────────

		[TestMethod]
		public void AnyType_ValidXmlString_DocumentElementInAny()
		{
			// Rationale: well-formed XML string for anyType must produce a single document element in dt.Any.
			var (dt, errors) = Build<anyType_DEtype>(ItemChoiceType.anyType, "<observation><value>7</value></observation>");
			Assert.AreEqual(0, errors.Count, "A valid XML string for anyType_DEtype must produce no errors.");
			Assert.AreEqual(1, dt.Any.Count, "dt.Any must contain exactly the document element.");
			Assert.AreEqual("observation", dt.Any[0].LocalName, "The document element name must match.");
		}

		[TestMethod]
		public void AnyType_InvalidXmlString_ErrorRecorded()
		{
			// Rationale: ill-formed XML string for anyType must be soft-rejected.
			var (dt, errors) = Build<anyType_DEtype>(ItemChoiceType.anyType, "<broken");
			Assert.IsTrue(errors.Count > 0, "An ill-formed XML string for anyType_DEtype must produce an error.");
			Assert.AreEqual(0, dt.Any.Count, "dt.Any must be empty after a failed parse.");
		}

		[TestMethod]
		public void AnyType_UnsupportedValueType_ErrorRecorded()
		{
			// Rationale: unsupported value types for anyType_DEtype must be soft-rejected without throwing.
			var (dt, errors) = Build<anyType_DEtype>(ItemChoiceType.anyType, 3.14m);
			Assert.IsTrue(errors.Count > 0, "An unsupported value type for anyType_DEtype must produce an error.");
		}

		// ─── P5.4 — dayTimeDuration ───────────────────────────────────────────────

		[TestMethod]
		public void DayTimeDuration_ValidPnDTnHnMnS_AcceptedByBuilder()
		{
			// Rationale: "P5DT12H30M15S" is a canonical dayTimeDuration string. The builder's
			// updated anchored regex must accept it and assign it to dt.val via the switch path.
			var (dt, errors) = Build<dayTimeDuration_DEtype>(ItemChoiceType.dayTimeDuration, "P5DT12H30M15S");
			Assert.AreEqual(0, errors.Count, "A canonical dayTimeDuration string must produce no errors.");
			Assert.AreEqual("P5DT12H30M15S", dt.val, "The valid dayTimeDuration string must be stored verbatim.");
		}

		[TestMethod]
		public void DayTimeDuration_StringWithYearPart_RejectedByBuilder()
		{
			// Rationale: dayTimeDuration must not contain year (Y) or month (M) components.
			// The builder must reject "P1Y2DT3H" at the switch-case level, before assigning to val.
			var (dt, errors) = Build<dayTimeDuration_DEtype>(ItemChoiceType.dayTimeDuration, "P1Y2DT3H");
			Assert.IsTrue(errors.Count > 0, "A dayTimeDuration string with a year part must produce an error in the builder.");
			Assert.IsNull(dt.val, "val must not be set when the year part makes the string invalid for dayTimeDuration.");
		}

		[TestMethod]
		public void DayTimeDuration_TimeSpanValue_ConvertedToValidDayTimeDuration()
		{
			// Rationale: TimeSpan has no year/month concept, so XmlConvert.ToString(TimeSpan) always
			// produces a valid dayTimeDuration string. No error should be recorded.
			var ts = new TimeSpan(days: 3, hours: 4, minutes: 5, seconds: 6);
			var (dt, errors) = Build<dayTimeDuration_DEtype>(ItemChoiceType.dayTimeDuration, ts);
			Assert.AreEqual(0, errors.Count, "A TimeSpan value for dayTimeDuration must convert without error.");
			Assert.IsNotNull(dt.val, "val must be set from the TimeSpan conversion.");
		}

		// ─── P5.5 — yearMonthDuration ─────────────────────────────────────────────

		[TestMethod]
		public void YearMonthDuration_ValidPnYnM_AcceptedByBuilder()
		{
			// Rationale: "P3Y6M" is a canonical yearMonthDuration. The updated regex must accept it.
			var (dt, errors) = Build<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration, "P3Y6M");
			Assert.AreEqual(0, errors.Count, "A canonical yearMonthDuration string must produce no errors.");
			Assert.AreEqual("P3Y6M", dt.val, "The valid yearMonthDuration string must be stored verbatim.");
		}

		[TestMethod]
		public void YearMonthDuration_StringWithDayPart_RejectedByBuilder()
		{
			// Rationale: yearMonthDuration must not contain D (day), T, H, or S components.
			// "P5D" is a dayTimeDuration, not a yearMonthDuration, and must be rejected.
			var (dt, errors) = Build<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration, "P5D");
			Assert.IsTrue(errors.Count > 0, "A string with a day part must be rejected for yearMonthDuration.");
			Assert.IsNull(dt.val, "val must not be set when the day part makes the string invalid.");
		}

		[TestMethod]
		public void YearMonthDuration_StringWithTimePart_RejectedByBuilder()
		{
			// Rationale: "PT10H" contains a time component (T, H) which is illegal in yearMonthDuration.
			var (dt, errors) = Build<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration, "PT10H");
			Assert.IsTrue(errors.Count > 0, "A string with a time part must be rejected for yearMonthDuration.");
			Assert.IsNull(dt.val, "val must not be set when the time part makes the string invalid.");
		}

		[TestMethod]
		public void YearMonthDuration_TimeSpanValue_RejectedByBuilder()
		{
			// Rationale: TimeSpan represents days/hours/minutes/seconds and cannot express
			// years or months (which have variable day counts). A TimeSpan must be soft-rejected
			// with an explanatory message directing the caller to use a lexical string.
			var ts = new TimeSpan(days: 365, hours: 0, minutes: 0, seconds: 0);
			var (dt, errors) = Build<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration, ts);
			Assert.IsTrue(errors.Count > 0, "A TimeSpan value for yearMonthDuration must produce a recorded error.");
			Assert.IsNull(dt.val, "val must not be set from an invalid TimeSpan for yearMonthDuration.");
		}

		// ─── P5.6 — gDay/gMonth/gMonthDay normalization ───────────────────────────

		[TestMethod]
		public void GDay_BareDayDigits_NormalizedToCanonicalForm()
		{
			// Rationale: UI forms often supply bare two-digit day numbers without leading dashes.
			// The builder must prepend "---" automatically and store the canonical form "---05".
			var (dt, errors) = Build<gDay_DEtype>(ItemChoiceType.gDay, "05");
			Assert.AreEqual(0, errors.Count, "A bare day number '05' must be normalized to '---05' without error.");
			Assert.AreEqual("---05", dt.val, "The stored value must be the canonical '---05' form.");
		}

		[TestMethod]
		public void GDay_CanonicalForm_AcceptedUnchanged()
		{
			// Rationale: a string that already has the '---DD' prefix must be accepted as-is.
			var (dt, errors) = Build<gDay_DEtype>(ItemChoiceType.gDay, "---15");
			Assert.AreEqual(0, errors.Count, "A canonical '---DD' string must be accepted without error.");
			Assert.AreEqual("---15", dt.val, "The canonical form must be stored verbatim.");
		}

		[TestMethod]
		public void GDay_InvalidDayNumber_RejectedByBuilder()
		{
			// Rationale: day 32 does not exist; even after normalization, "---32" must fail the regex.
			var (dt, errors) = Build<gDay_DEtype>(ItemChoiceType.gDay, "32");
			Assert.IsTrue(errors.Count > 0, "An invalid day number (32) must produce a recorded error.");
		}

		[TestMethod]
		public void GMonth_BareMonthDigits_NormalizedToCanonicalForm()
		{
			// Rationale: UI forms may supply "07" instead of "--07". The builder must prepend "--".
			var (dt, errors) = Build<gMonth_DEtype>(ItemChoiceType.gMonth, "07");
			Assert.AreEqual(0, errors.Count, "A bare month number '07' must be normalized to '--07' without error.");
			Assert.AreEqual("--07", dt.val, "The stored value must be the canonical '--07' form.");
		}

		[TestMethod]
		public void GMonth_CanonicalForm_AcceptedUnchanged()
		{
			// Rationale: a string already in '--MM' form must not be double-prefixed.
			var (dt, errors) = Build<gMonth_DEtype>(ItemChoiceType.gMonth, "--06");
			Assert.AreEqual(0, errors.Count, "A canonical '--MM' string must be accepted without error.");
			Assert.AreEqual("--06", dt.val, "The canonical form must be stored verbatim.");
		}

		[TestMethod]
		public void GMonth_InvalidMonthNumber_RejectedByBuilder()
		{
			// Rationale: month 13 does not exist; "13" normalized to "--13" must fail the regex.
			var (dt, errors) = Build<gMonth_DEtype>(ItemChoiceType.gMonth, "13");
			Assert.IsTrue(errors.Count > 0, "An invalid month number (13) must produce a recorded error.");
		}

		[TestMethod]
		public void GMonthDay_BareMonthDayString_NormalizedToCanonicalForm()
		{
			// Rationale: UI forms may supply "07-04" (July 4) without the leading "--".
			// The builder must prepend "--" to produce the canonical "--07-04".
			var (dt, errors) = Build<gMonthDay_DEtype>(ItemChoiceType.gMonthDay, "07-04");
			Assert.AreEqual(0, errors.Count, "A bare 'MM-DD' string must be normalized to '--MM-DD' without error.");
			Assert.AreEqual("--07-04", dt.val, "The stored value must be the canonical '--07-04' form.");
		}

		[TestMethod]
		public void GMonthDay_CanonicalForm_AcceptedUnchanged()
		{
			// Rationale: a string already in '--MM-DD' form must be accepted as-is.
			var (dt, errors) = Build<gMonthDay_DEtype>(ItemChoiceType.gMonthDay, "--12-25");
			Assert.AreEqual(0, errors.Count, "A canonical '--MM-DD' string must be accepted without error.");
			Assert.AreEqual("--12-25", dt.val, "The canonical form must be stored verbatim.");
		}

		// ─── anyURI Uri.TryCreate fix ─────────────────────────────────────────────

		[TestMethod]
		public void AnyURI_ValidAbsoluteUri_Accepted()
		{
			// Rationale: a well-formed absolute URI must be accepted and stored verbatim.
			var (dt, errors) = Build<anyURI_DEtype>(ItemChoiceType.anyURI, "https://example.com/sdcform?id=42");
			Assert.AreEqual(0, errors.Count, "A valid absolute URI must produce no errors.");
			Assert.AreEqual("https://example.com/sdcform?id=42", dt.val, "The valid URI must be stored verbatim.");
		}

		[TestMethod]
		public void AnyURI_ValidRelativeUri_Accepted()
		{
			// Rationale: xs:anyURI includes relative URIs; a relative path must be accepted
			// (Uri.TryCreate with UriKind.RelativeOrAbsolute handles this correctly).
			var (dt, errors) = Build<anyURI_DEtype>(ItemChoiceType.anyURI, "/forms/sdc/v2/template.xml");
			Assert.AreEqual(0, errors.Count, "A valid relative URI must produce no errors.");
			Assert.AreEqual("/forms/sdc/v2/template.xml", dt.val, "The relative URI must be stored verbatim.");
		}
	}
}
