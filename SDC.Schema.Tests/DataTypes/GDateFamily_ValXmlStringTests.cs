using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on the gDate family: gYear_Stype, gYearMonth_Stype,
	/// gMonth_Stype, gDay_Stype, and gMonthDay_Stype. Like the date/time family, each type already has a
	/// generated, separate `timeZone` string property holding the optional XSD timezone suffix ('Z' or
	/// '&#177;hh:mm') verbatim -- no offset arithmetic is performed on it. Unlike the date/time family,
	/// however, `val` remains a raw string here (not DateTime), because gMonth/gDay/gMonthDay represent
	/// partial calendar dates with no year (or no month/day) component, which System.DateTime cannot
	/// represent. GDateXmlHelper validates the numeric-designator portion of each lexical string against
	/// a per-type regex before splitting it from the optional timezone suffix.
	/// </summary>
	[TestClass]
	public class GDateFamily_ValXmlStringTests
	{
		/// <summary>
		/// Constructs a detached (un-attached-to-any-parent) _Stype instance via its protected
		/// parameterless constructor, using reflection -- see the identical helper and rationale in
		/// DateTimeFamily_ValXmlStringTests.cs.
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
		public void GYearStype_ValXmlString_RoundTripsPlainYear()
		{
			var sut = CreateDetached<gYear_Stype>();

			sut.ValXmlString = "2024";

			Assert.AreEqual("2024", sut.val);
			Assert.IsNull(sut.timeZone);
			Assert.AreEqual("2024", sut.ValXmlString);
		}

		[TestMethod]
		public void GYearStype_ValXmlString_RoundTripsNegativeYearWithZuluOffset()
		{
			var sut = CreateDetached<gYear_Stype>();

			// A leading '-' (BCE year) and a 'Z' timezone suffix are both legal for gYear; the suffix must
			// be preserved verbatim in the separate timeZone property, not folded into val.
			sut.ValXmlString = "-0099Z";

			Assert.AreEqual("-0099", sut.val);
			Assert.AreEqual("Z", sut.timeZone);
			Assert.AreEqual("-0099Z", sut.ValXmlString);
		}

		[TestMethod]
		public void GYearStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<gYear_Stype>();
			var originalVal = sut.val;

			// A 3-digit year is illegal -- gYear requires at least 4 digits.
			sut.ValXmlString = "202";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void GYearMonthStype_ValXmlString_RoundTripsWithOffset()
		{
			var sut = CreateDetached<gYearMonth_Stype>();

			sut.ValXmlString = "2024-05+02:00";

			Assert.AreEqual("2024-05", sut.val);
			Assert.AreEqual("+02:00", sut.timeZone);
			Assert.AreEqual("2024-05+02:00", sut.ValXmlString);
		}

		[TestMethod]
		public void GYearMonthStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<gYearMonth_Stype>();
			var originalVal = sut.val;

			// Month must be zero-padded to 2 digits; "2024-5" omits the leading zero and is illegal.
			sut.ValXmlString = "2024-5";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void GMonthStype_ValXmlString_RoundTripsPlainMonth()
		{
			var sut = CreateDetached<gMonth_Stype>();

			// gMonth's lexical form has a distinctive "--MM" prefix (no year, no day).
			sut.ValXmlString = "--05";

			Assert.AreEqual("--05", sut.val);
			Assert.IsNull(sut.timeZone);
			Assert.AreEqual("--05", sut.ValXmlString);
		}

		[TestMethod]
		public void GMonthStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<gMonth_Stype>();
			var originalVal = sut.val;

			// Missing the required "--" prefix is illegal.
			sut.ValXmlString = "05";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void GDayStype_ValXmlString_RoundTripsWithZuluOffset()
		{
			var sut = CreateDetached<gDay_Stype>();

			// gDay's lexical form has a distinctive "---DD" prefix (no year, no month).
			sut.ValXmlString = "---15Z";

			Assert.AreEqual("---15", sut.val);
			Assert.AreEqual("Z", sut.timeZone);
			Assert.AreEqual("---15Z", sut.ValXmlString);
		}

		[TestMethod]
		public void GDayStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<gDay_Stype>();
			var originalVal = sut.val;

			// Only two leading hyphens instead of the required three is illegal.
			sut.ValXmlString = "--15";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void GMonthDayStype_ValXmlString_RoundTripsWithOffset()
		{
			var sut = CreateDetached<gMonthDay_Stype>();

			sut.ValXmlString = "--05-15-05:00";

			Assert.AreEqual("--05-15", sut.val);
			Assert.AreEqual("-05:00", sut.timeZone);
			Assert.AreEqual("--05-15-05:00", sut.ValXmlString);
		}

		[TestMethod]
		public void GMonthDayStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<gMonthDay_Stype>();
			var originalVal = sut.val;

			// Day must be zero-padded to 2 digits; "--05-5" omits the leading zero and is illegal.
			sut.ValXmlString = "--05-5";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void GDayStype_ValXmlString_RejectsNullValue()
		{
			var sut = CreateDetached<gDay_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = null!;

			Assert.AreEqual(originalVal, sut.val);
		}
	}
}
