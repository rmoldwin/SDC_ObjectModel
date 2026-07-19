// SDC-CUSTOM: do not overwrite. Hand-authored date/date-part soft-reject tests.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Tests.OM;
using System;

namespace SDC.Schema.Tests.OM
{
	/// <summary>
	/// Object-model soft-reject tests for the XML Schema date / date-part datatypes, extending the
	/// issue #8 numeric soft-reject contract to: date, dateTime, time, dateTimeStamp, gYear,
	/// gYearMonth, gMonth, gMonthDay, gDay, duration, dayTimeDuration, yearMonthDuration.
	///
	/// The contract under test (identical to issue #8): an invalid value is NEVER stored, NEVER
	/// throws, and is recorded out-of-band in <see cref="BaseType.RejectedValues"/> keyed by the
	/// member name, with a message that quotes the offending value and names the xs: type so a UI can
	/// teach the user the correct form. Legal values must store unchanged.
	///
	/// Notes on backing types:
	///  * g* and the duration family are STRING-backed (val IS the XSD lexical string) and are wired
	///    into the pipeline via the generated soft-reject setter + the regen-safe rule registry.
	///  * date/dateTime/time/dateTimeStamp are DateTime-backed; their lexical rules are enforced at
	///    the string boundary via SetLexicalValue. The dateTimeStamp DateTime val itself carries an
	///    impossible generated [RegularExpression] that issue I-1 had caused to drop every value — the
	///    registry now neutralizes it (regression covered below).
	/// </summary>
	[TestClass]
	public class DateResponseTypeBoundaryTests
	{
		[TestInitialize]
		public void Init() => BaseType.ResetLastTopNode();

		// All twelve date/date-part DEtypes are now built through the standard response-tree API
		// (AddChildQuestionResponse → AddDataType). gMonth/gYearMonth/dayTimeDuration previously could
		// not be attached because the generated DataTypes_DEType choice bound gYearMonth to the wrong
		// CLR type (gMonth_DEtype) and had no dayTimeDuration element at all; that generated-binding bug
		// was hand-corrected under issue #10, so the deserialization-ctor workaround is no longer needed
		// and these tests exercise the real builder path the same as every other datatype.
		private static T DE<T>(ItemChoiceType ict) where T : BaseType
			=> NumericResponseTypeTestHelpers.DE<T>(ict, out _);

		// ─── String-backed: legal values stored exactly ───────────────────────────

		[TestMethod]
		public void StringBacked_LegalLexicalValues_StoredExactly()
		{
			// Rationale: a canonical lexical form for each string-backed type must pass validation and
			// be stored verbatim (the val IS the lexical string — no normalization is applied).
			var gy = DE<gYear_DEtype>(ItemChoiceType.gYear); gy.val = "2026";
			Assert.AreEqual("2026", gy.val, "A canonical xs:gYear must be stored verbatim.");

			var gym = DE<gYearMonth_DEtype>(ItemChoiceType.gYearMonth); gym.val = "2026-06";
			Assert.AreEqual("2026-06", gym.val, "A canonical xs:gYearMonth must be stored verbatim.");

			var gm = DE<gMonth_DEtype>(ItemChoiceType.gMonth); gm.val = "--06";
			Assert.AreEqual("--06", gm.val, "A canonical xs:gMonth must be stored verbatim.");

			var gmd = DE<gMonthDay_DEtype>(ItemChoiceType.gMonthDay); gmd.val = "--06-22";
			Assert.AreEqual("--06-22", gmd.val, "A canonical xs:gMonthDay must be stored verbatim.");

			var gd = DE<gDay_DEtype>(ItemChoiceType.gDay); gd.val = "---22";
			Assert.AreEqual("---22", gd.val, "A canonical xs:gDay must be stored verbatim.");

			var du = DE<duration_DEtype>(ItemChoiceType.duration); du.val = "P3Y6M4DT12H30M5S";
			Assert.AreEqual("P3Y6M4DT12H30M5S", du.val, "A full xs:duration must be stored verbatim.");

			var dtd = DE<dayTimeDuration_DEtype>(ItemChoiceType.dayTimeDuration); dtd.val = "P5DT12H";
			Assert.AreEqual("P5DT12H", dtd.val, "A canonical xs:dayTimeDuration must be stored verbatim.");

			var ymd = DE<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration); ymd.val = "P3Y6M";
			Assert.AreEqual("P3Y6M", ymd.val, "A canonical xs:yearMonthDuration must be stored verbatim.");
		}

		[TestMethod]
		public void StringBacked_TimezoneVariants_AreLegal()
		{
			// Rationale: the timezone suffix (Z or ±hh:mm within -14:00..+14:00) is part of the lexical
			// space of the g* types and must be accepted, not rejected.
			var z = DE<gYear_DEtype>(ItemChoiceType.gYear); z.val = "2026Z";
			Assert.AreEqual("2026Z", z.val, "xs:gYear with a 'Z' timezone is legal.");

			var off = DE<gYear_DEtype>(ItemChoiceType.gYear); off.val = "2026-05:00";
			Assert.AreEqual("2026-05:00", off.val, "xs:gYear with a -05:00 offset is legal.");

			var max = DE<gMonth_DEtype>(ItemChoiceType.gMonth); max.val = "--06+14:00";
			Assert.AreEqual("--06+14:00", max.val, "The maximum legal offset +14:00 is accepted.");
		}

		// ─── String-backed: illegal values soft-rejected ──────────────────────────

		[TestMethod]
		public void GMonth_OutOfRangeMonth_SoftRejected()
		{
			// Rationale: '--13' is the classic month-out-of-range mistake. It must not be stored, must
			// not throw, and must be recorded with a message that names the type and the offending value.
			var gm = DE<gMonth_DEtype>(ItemChoiceType.gMonth);
			gm.val = "--13"; // soft reject — must not throw

			Assert.IsNull(gm.val, "An out-of-range xs:gMonth must not be stored (unset retained).");
			Assert.IsTrue(gm.RejectedValues.TryGetValue("val", out var rv),
				"The offending xs:gMonth must be recorded in RejectedValues['val'].");
			Assert.AreEqual("--13", rv!.AttemptedValue, "The recorded attempted value must be the offending string.");
			StringAssert.Contains(rv.Message, "--13", "The message must quote the offending value.");
			StringAssert.Contains(rv.Message, "xs:gMonth", "The message must name the xs: type being violated.");
		}

		[TestMethod]
		public void GDay_OutOfRangeDay_SoftRejected()
		{
			// Rationale: '---55' exceeds the 01-31 day range; soft-reject and teach the day range.
			var gd = DE<gDay_DEtype>(ItemChoiceType.gDay);
			gd.val = "---55";
			Assert.IsNull(gd.val, "An out-of-range xs:gDay must not be stored.");
			Assert.IsTrue(gd.RejectedValues.TryGetValue("val", out var rv), "The offending xs:gDay must be recorded.");
			StringAssert.Contains(rv!.Message, "01-31", "The message must teach the legal day range.");
		}

		[TestMethod]
		public void Duration_IllegalForms_SoftRejected()
		{
			// Rationale: a bare 'P' has no components and 'PT' has no time component — both are illegal
			// per ISO-8601 / XSD and must be soft-rejected (not coerced).
			var d1 = DE<duration_DEtype>(ItemChoiceType.duration);
			d1.val = "P";
			Assert.IsNull(d1.val, "A bare 'P' duration must not be stored.");
			Assert.IsTrue(d1.RejectedValues.ContainsKey("val"), "A bare 'P' must be recorded as rejected.");

			var d2 = DE<duration_DEtype>(ItemChoiceType.duration);
			d2.val = "PT"; // 'T' with no following time component
			Assert.IsNull(d2.val, "A 'PT' with no time component must not be stored.");
		}

		[TestMethod]
		public void DayTimeDuration_YearPart_SoftRejected()
		{
			// Rationale: a dayTimeDuration permits only day+time parts; a 'Y' (here 'PT1Y') is illegal
			// regardless of position and must be soft-rejected with a part-restriction message.
			var dtd = DE<dayTimeDuration_DEtype>(ItemChoiceType.dayTimeDuration);
			dtd.val = "PT1Y";
			Assert.IsNull(dtd.val, "A dayTimeDuration with a year part must not be stored.");
			Assert.IsTrue(dtd.RejectedValues.TryGetValue("val", out var rv), "The offending value must be recorded.");
			StringAssert.Contains(rv!.Message, "day and time", "The message must explain the day/time-only restriction.");
		}

		[TestMethod]
		public void YearMonthDuration_DayPart_SoftRejected()
		{
			// Rationale: a yearMonthDuration permits only year+month parts; a day part 'P5D' is illegal.
			var ymd = DE<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration);
			ymd.val = "P5D";
			Assert.IsNull(ymd.val, "A yearMonthDuration with a day part must not be stored.");
			Assert.IsTrue(ymd.RejectedValues.ContainsKey("val"), "The offending value must be recorded.");
		}

		[TestMethod]
		public void StringBacked_PriorValidValue_RetainedAfterReject()
		{
			// Rationale: soft-reject must preserve the last good value — a subsequent invalid assignment
			// cannot corrupt or clear an already-valid stored value.
			var gm = DE<gMonth_DEtype>(ItemChoiceType.gMonth);
			gm.val = "--06";       // valid
			gm.val = "--13";       // invalid — must be ignored
			Assert.AreEqual("--06", gm.val, "The prior valid value must be retained when a later value is rejected.");
			Assert.IsTrue(gm.RejectedValues.ContainsKey("val"), "The rejected value is still recorded for surfacing to the user.");
		}

		// ─── dateTimeStamp I-1 regression ──────────────────────────────────────────

		[TestMethod]
		public void DateTimeStamp_ValidDateTime_IsStored_I1Regression()
		{
			// Regression for Open-Question I-1: dateTimeStamp's DateTime 'val' carries an impossible
			// generated [RegularExpression(".*(Z|±dd:dd)")] that runs against DateTime.ToString() and can
			// NEVER match, so under issue #8 soft-reject EVERY dateTimeStamp value was being dropped.
			// The registry now neutralizes that broken rule. This proves a valid DateTime is stored and
			// nothing is recorded as rejected.
			var dts = DE<dateTimeStamp_DEtype>(ItemChoiceType.dateTimeStamp);
			var when = new DateTime(2026, 6, 22, 10, 0, 0, DateTimeKind.Utc);
			dts.val = when;
			Assert.AreEqual(when, dts.val, "A valid dateTimeStamp DateTime must now be stored (I-1 regression).");
			Assert.IsFalse(dts.RejectedValues.ContainsKey("val"),
				"A valid dateTimeStamp must NOT be recorded as rejected (the broken regex is neutralized).");
		}

		// ─── DateTime-backed lexical entry point (SetLexicalValue) ─────────────────

		[TestMethod]
		public void Date_SetLexicalValue_LegalAndIllegal()
		{
			// Rationale: SetLexicalValue is the string boundary for the DateTime-backed types. A legal
			// xs:date sets val (date-only); a value carrying a stray time component is a HARD reject (no
			// silent truncation) with a message that explains date must not carry a time.
			var d = DE<date_DEtype>(ItemChoiceType.date);
			Assert.IsTrue(d.SetLexicalValue("2026-06-22"), "A canonical xs:date must be accepted.");
			Assert.AreEqual(new DateTime(2026, 6, 22), d.val, "The parsed date must be stored.");

			var bad = DE<date_DEtype>(ItemChoiceType.date);
			Assert.IsFalse(bad.SetLexicalValue("2026-06-22T10:00:00"), "A date with a time component must be rejected.");
			Assert.IsTrue(bad.RejectedValues.TryGetValue("val", out var rv), "The stray-time date must be recorded.");
			StringAssert.Contains(rv!.Message, "time component", "The message must explain that a date carries no time.");
		}

		[TestMethod]
		public void DateTimeStamp_SetLexicalValue_RequiresTimezone()
		{
			// Rationale: a dateTimeStamp REQUIRES a timezone. A timezone-bearing value is accepted and
			// stored (UTC-normalized); the same value without a timezone is soft-rejected with the
			// timezone-required explanation — the lexical rule the DateTime field cannot itself encode.
			var ok = DE<dateTimeStamp_DEtype>(ItemChoiceType.dateTimeStamp);
			Assert.IsTrue(ok.SetLexicalValue("2026-06-22T10:00:00Z"), "A dateTimeStamp WITH a timezone must be accepted.");
			Assert.AreEqual(new DateTime(2026, 6, 22, 10, 0, 0, DateTimeKind.Utc), ok.val, "The instant must be stored as UTC.");

			var bad = DE<dateTimeStamp_DEtype>(ItemChoiceType.dateTimeStamp);
			Assert.IsFalse(bad.SetLexicalValue("2026-06-22T10:00:00"), "A dateTimeStamp WITHOUT a timezone must be rejected.");
			Assert.IsTrue(bad.RejectedValues.TryGetValue("val", out var rv), "The missing-timezone value must be recorded.");
			StringAssert.Contains(rv!.Message, "timezone", "The message must explain the timezone is required.");
		}

		[TestMethod]
		public void DateTimeBacked_TimeZoneOffset_Property_CanonicalizesAndUpdatesBackingValue()
		{
			// Rationale: the typed offset property should expose the same logical timezone information as the
			// existing string property, but canonicalize it to the OM's +/−hh:mm form and keep the backing
			// lexical string in sync for the existing XML contract.
			var dateNode = DE<date_DEtype>(ItemChoiceType.date);
			Assert.IsTrue(dateNode.SetLexicalValue("2026-06-22Z"), "A date with a Z timezone should be accepted.");
			Assert.AreEqual("+00:00", dateNode.timeZone, "Z should be normalized to the canonical +00:00 offset.");
			Assert.AreEqual(TimeSpan.Zero, dateNode.TimeZoneOffset, "The typed getter should expose a zero offset for +00:00.");
			dateNode.TimeZoneOffset = TimeSpan.FromHours(-5);
			Assert.AreEqual("-05:00", dateNode.timeZone, "The typed setter should canonicalize the backing timezone string.");
			dateNode.TimeZoneOffset = null;
			Assert.IsNull(dateNode.timeZone, "Clearing the typed offset should clear the backing timezone string.");

			var dtNode = DE<dateTime_DEtype>(ItemChoiceType.dateTime);
			dtNode.TimeZoneOffset = TimeSpan.FromMinutes(330);
			Assert.AreEqual("+05:30", dtNode.timeZone, "A 5h30m offset should format as +05:30.");
			Assert.AreEqual(TimeSpan.FromMinutes(330), dtNode.TimeZoneOffset, "The typed property should preserve the offset value.");

			var timeNode = DE<time_DEtype>(ItemChoiceType.time);
			Assert.IsTrue(timeNode.SetLexicalValue("10:30:00-05:00"), "An xs:time with a -05:00 offset should be accepted.");
			Assert.AreEqual("-05:00", timeNode.timeZone, "The lexical parse path should canonicalize the timezone token.");
			Assert.AreEqual(TimeSpan.FromHours(-5), timeNode.TimeZoneOffset, "The typed getter should parse the canonical offset.");
		}

		[TestMethod]
		public void Time_SetLexicalValue_EndOfDayAndStrayDate()
		{
			// Rationale: xs:time allows the end-of-day '24:00:00' (which .NET cannot store as hour 24 —
			// it is normalized), and must reject a value that carries a date component.
			var eod = DE<time_DEtype>(ItemChoiceType.time);
			Assert.IsTrue(eod.SetLexicalValue("24:00:00"), "xs:time end-of-day 24:00:00 must be accepted (normalized).");
			
			var bad = DE<time_DEtype>(ItemChoiceType.time);
			Assert.IsFalse(bad.SetLexicalValue("2026-06-22T10:00:00"), "An xs:time carrying a date must be rejected.");
		}

		// ─── Pinned worked-example messages (characterization) ─────────────────────

		[TestMethod]
		public void WorkedExampleMessages_AreHelpfulAndStable()
		{
			// Rationale: these six messages are the user-facing teaching surface; pin them so a future
			// edit cannot silently degrade their helpfulness (quote value, name type, pinpoint cause).
			var date = XsdDateTimePatterns.BuildLexicalErrorMessage(XsdDateKind.Date, "2026-13-05");
			StringAssert.Contains(date, "xs:date", "date message names the type.");
			StringAssert.Contains(date, "month must be 01-12", "date message pinpoints the month range.");
			StringAssert.Contains(date, "'13'", "date message quotes the offending month.");

			var dts = XsdDateTimePatterns.BuildLexicalErrorMessage(XsdDateKind.DateTimeStamp, "2026-06-22T10:00:00");
			StringAssert.Contains(dts, "REQUIRES a timezone", "dateTimeStamp message explains the timezone requirement.");

			var gm = XsdDateTimePatterns.BuildLexicalErrorMessage(XsdDateKind.GMonth, "--13");
			StringAssert.Contains(gm, "month must be 01-12", "gMonth message pinpoints the month range.");

			var dur = XsdDateTimePatterns.BuildLexicalErrorMessage(XsdDateKind.Duration, "P");
			StringAssert.Contains(dur, "empty duration", "duration message explains an empty 'P' is illegal.");

			var dtd = XsdDateTimePatterns.BuildLexicalErrorMessage(XsdDateKind.DayTimeDuration, "PT1Y");
			StringAssert.Contains(dtd, "day and time", "dayTimeDuration message explains the part restriction.");

			var tz = XsdDateTimePatterns.BuildTimezoneErrorMessage("+15:00");
			StringAssert.Contains(tz, "+15:00", "timezone message quotes the offending offset.");
			StringAssert.Contains(tz, "-14:00..+14:00", "timezone message states the legal range.");
		}
	}
}
