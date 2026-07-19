using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on the date/time family (date_Stype, dateTime_Stype,
	/// dateTimeStamp_Stype, time_Stype), per the design in
	/// SDC.Schema/Documentation/IVal_DateTime_ValXmlString_Design.md.
	/// The core requirement under test is that the timezone suffix ('Z', an explicit offset, or absent)
	/// is preserved exactly as written -- never converted through DateTimeOffset or the local machine's
	/// timezone -- so that set-then-get reproduces the original lexical string byte-for-byte.
	/// </summary>
	[TestClass]
	public class DateTimeFamily_ValXmlStringTests
	{
		/// <summary>
		/// Constructs a detached (un-attached-to-any-parent) _Stype instance via its protected
		/// parameterless constructor, using reflection. These date/time _Stype classes only expose a
		/// public constructor that requires a parent object with a *uniquely* type-matched property to
		/// auto-attach to (via SdcUtil.TryAttachNewNode) -- and no such unique real-world parent property
		/// currently exists in the schema for time_Stype/dateTimeStamp_Stype, and only ambiguous
		/// (multi-property) matches exist for date_Stype/dateTime_Stype. Since ValXmlString's logic only
		/// touches val/timeZone and (on error) StoreError -- none of which require a populated parent/tree
		/// -- constructing these instances detached is sufficient and appropriate for testing this member
		/// in isolation.
		/// </summary>
		private static T CreateDetached<T>() where T : BaseType
		{
			var ctor = typeof(T).GetConstructor(
				BindingFlags.NonPublic | BindingFlags.Instance,
				binder: null,
				types: Type.EmptyTypes,
				modifiers: null)
				?? throw new InvalidOperationException($"{typeof(T).Name} has no protected/internal parameterless constructor.");
			return (T)ctor.Invoke(null);
		}

		[TestMethod]
		public void DateStype_ValXmlString_RoundTripsWithNoTimezone()
		{
			var sut = CreateDetached<date_Stype>();

			sut.ValXmlString = "2024-07-07";

			// val should hold the literal date with no time-of-day component, and timeZone should be null
			// since no timezone suffix was present -- absence must not be reinterpreted as UTC or local time.
			Assert.AreEqual(new DateTime(2024, 7, 7), sut.val);
			Assert.IsNull(sut.timeZone);
			// Round trip must reproduce the exact original string.
			Assert.AreEqual("2024-07-07", sut.ValXmlString);
		}

		[TestMethod]
		public void DateStype_ValXmlString_RoundTripsWithZuluTimezone()
		{
			var sut = CreateDetached<date_Stype>();

			sut.ValXmlString = "2024-07-07Z";

			// 'Z' must be preserved verbatim in timeZone, not converted into an offset computation.
			Assert.AreEqual(new DateTime(2024, 7, 7), sut.val);
			Assert.AreEqual("Z", sut.timeZone);
			Assert.AreEqual("2024-07-07Z", sut.ValXmlString);
		}

		[TestMethod]
		public void DateStype_ValXmlString_RoundTripsWithExplicitOffset()
		{
			var sut = CreateDetached<date_Stype>();

			sut.ValXmlString = "2024-07-07-06:00";

			// The offset text must be stored exactly as supplied -- not recalculated relative to the
			// local machine's timezone, which would make the round trip depend on where the test runs.
			Assert.AreEqual(new DateTime(2024, 7, 7), sut.val);
			Assert.AreEqual("-06:00", sut.timeZone);
			Assert.AreEqual("2024-07-07-06:00", sut.ValXmlString);
		}

		[TestMethod]
		public void DateStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<date_Stype>();
			var originalVal = sut.val;

			// An illegal value must be handled gracefully via the internal StoreError path (no unhandled
			// exception escaping the setter), and must not corrupt the existing val.
			sut.ValXmlString = "not-a-date";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void DateTimeStype_ValXmlString_RoundTripsWithNoTimezone()
		{
			var sut = CreateDetached<dateTime_Stype>();

			sut.ValXmlString = "2024-07-07T10:30:15";

			Assert.AreEqual(new DateTime(2024, 7, 7, 10, 30, 15), sut.val);
			Assert.IsNull(sut.timeZone);
			Assert.AreEqual("2024-07-07T10:30:15", sut.ValXmlString);
		}

		[TestMethod]
		public void DateTimeStype_ValXmlString_RoundTripsWithZuluTimezone()
		{
			var sut = CreateDetached<dateTime_Stype>();

			sut.ValXmlString = "2024-07-07T10:30:15Z";

			Assert.AreEqual(new DateTime(2024, 7, 7, 10, 30, 15), sut.val);
			Assert.AreEqual("Z", sut.timeZone);
			Assert.AreEqual("2024-07-07T10:30:15Z", sut.ValXmlString);
		}

		[TestMethod]
		public void DateTimeStype_ValXmlString_RoundTripsWithExplicitOffset()
		{
			var sut = CreateDetached<dateTime_Stype>();

			sut.ValXmlString = "2024-07-07T10:30:15+05:30";

			// The wall-clock digits (10:30:15) must be stored exactly as supplied, not shifted by the
			// offset -- val is never combined with timeZone via arithmetic.
			Assert.AreEqual(new DateTime(2024, 7, 7, 10, 30, 15), sut.val);
			Assert.AreEqual("+05:30", sut.timeZone);
			Assert.AreEqual("2024-07-07T10:30:15+05:30", sut.ValXmlString);
		}

		[TestMethod]
		public void DateTimeStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<dateTime_Stype>();
			var originalVal = sut.val;

			// An illegal value must be handled gracefully via the internal StoreError path (no unhandled
			// exception escaping the setter), and must not corrupt the existing val.
			sut.ValXmlString = "07/07/2024 garbage";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void TimeStype_ValXmlString_RoundTripsWithNoTimezone()
		{
			var sut = CreateDetached<time_Stype>();

			sut.ValXmlString = "14:22:05";

			// time_Stype's val holds only the time-of-day; the date component defaults to 0001-01-01
			// (DateTimeStyles.NoCurrentDateDefault), never today's date.
			Assert.AreEqual(new DateTime(1, 1, 1, 14, 22, 5), sut.val);
			Assert.IsNull(sut.timeZone);
			Assert.AreEqual("14:22:05", sut.ValXmlString);
		}

		[TestMethod]
		public void TimeStype_ValXmlString_RoundTripsWithExplicitOffset()
		{
			var sut = CreateDetached<time_Stype>();

			sut.ValXmlString = "14:22:05-08:00";

			Assert.AreEqual(new DateTime(1, 1, 1, 14, 22, 5), sut.val);
			Assert.AreEqual("-08:00", sut.timeZone);
			Assert.AreEqual("14:22:05-08:00", sut.ValXmlString);
		}

		[TestMethod]
		public void DateTimeStampStype_ValXmlString_RoundTripsWithZuluTimezone()
		{
			var sut = CreateDetached<dateTimeStamp_Stype>();

			sut.ValXmlString = "2024-07-07T10:30:15Z";

			Assert.AreEqual(new DateTime(2024, 7, 7, 10, 30, 15), sut.val);
			Assert.AreEqual("Z", sut.timeZone);
			Assert.AreEqual("2024-07-07T10:30:15Z", sut.ValXmlString);
		}

		[TestMethod]
		public void DateTimeStampStype_ValXmlString_RoundTripsWithExplicitOffset()
		{
			var sut = CreateDetached<dateTimeStamp_Stype>();

			sut.ValXmlString = "2024-07-07T10:30:15-06:00";

			Assert.AreEqual(new DateTime(2024, 7, 7, 10, 30, 15), sut.val);
			Assert.AreEqual("-06:00", sut.timeZone);
			Assert.AreEqual("2024-07-07T10:30:15-06:00", sut.ValXmlString);
		}

		[TestMethod]
		public void DateTimeStampStype_ValXmlString_RejectsMissingTimezone()
		{
			var sut = CreateDetached<dateTimeStamp_Stype>();
			var originalVal = sut.val;

			// dateTimeStamp requires an explicit timezone per the XSD spec; omitting it is illegal
			// for this specific type (unlike dateTime_Stype, where omission is legal). It must be
			// handled gracefully (via StoreError), not thrown, and must not corrupt val/timeZone.
			sut.ValXmlString = "2024-07-07T10:30:15";

			Assert.AreEqual(originalVal, sut.val);
			Assert.IsNull(sut.timeZone);
		}
	}
}
