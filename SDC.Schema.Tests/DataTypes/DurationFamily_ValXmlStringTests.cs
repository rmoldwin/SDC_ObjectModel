using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on the duration family: duration_Stype (unrestricted),
	/// dayTimeDuration_Stype (no Y/M designators permitted, must contain a D or T section), and
	/// yearMonthDuration_Stype (no D or T section permitted). All three use a CLR `string` for val
	/// (the raw XSD "duration" lexical text is stored verbatim, so ValXmlString is effectively a thin,
	/// validating pass-through rather than a type conversion). duration_Stype has no generated format
	/// validator at all, so ValXmlString performs its own XSD duration lexical-form check
	/// (DurationXmlHelper.IsValidDuration); the two restricted subtypes rely on the generated
	/// RegularExpressionAttribute facet (enforced via Validator.ValidateProperty in the generated val
	/// setter), whose ValidationException is caught and routed through StoreError.
	/// </summary>
	[TestClass]
	public class DurationFamily_ValXmlStringTests
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
		public void DurationStype_ValXmlString_RoundTripsFullDesignatorForm()
		{
			var sut = CreateDetached<duration_Stype>();

			// "P5Y2M10DT15H30M10S" exercises every designator (years, months, days, hours, minutes,
			// seconds) in one legal XSD duration string.
			sut.ValXmlString = "P5Y2M10DT15H30M10S";

			Assert.AreEqual("P5Y2M10DT15H30M10S", sut.val);
			Assert.AreEqual("P5Y2M10DT15H30M10S", sut.ValXmlString);
		}

		[TestMethod]
		public void DurationStype_ValXmlString_RoundTripsNegativeAndFractionalSeconds()
		{
			var sut = CreateDetached<duration_Stype>();

			// A leading '-' (negative duration) and a fractional-seconds component are both legal per
			// the XSD duration lexical grammar.
			sut.ValXmlString = "-P1DT2H30M5.5S";

			Assert.AreEqual("-P1DT2H30M5.5S", sut.val);
			Assert.AreEqual("-P1DT2H30M5.5S", sut.ValXmlString);
		}

		[TestMethod]
		public void DurationStype_ValXmlString_RejectsBareP()
		{
			var sut = CreateDetached<duration_Stype>();
			var originalVal = sut.val;

			// "P" alone has no designators at all, which is illegal -- the generated setter has no
			// validator for this class, so ValXmlString's own DurationXmlHelper check must catch it.
			sut.ValXmlString = "P";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void DurationStype_ValXmlString_RejectsBarePT()
		{
			var sut = CreateDetached<duration_Stype>();
			var originalVal = sut.val;

			// "PT" declares a time section but supplies no H/M/S designator within it, which is illegal.
			sut.ValXmlString = "PT";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void DurationStype_ValXmlString_RejectsMissingLeadingP()
		{
			var sut = CreateDetached<duration_Stype>();
			var originalVal = sut.val;

			// Every legal duration must begin with 'P'; omitting it is illegal.
			sut.ValXmlString = "5Y2M10D";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void DayTimeDurationStype_ValXmlString_RoundTripsDayAndTimeComponents()
		{
			var sut = CreateDetached<dayTimeDuration_Stype>();

			// dayTimeDuration permits only D/T-section designators (no Y or M-for-months); "P3DT4H" is
			// a legal example combining a day component with a time-section hour component.
			sut.ValXmlString = "P3DT4H";

			Assert.AreEqual("P3DT4H", sut.val);
			Assert.AreEqual("P3DT4H", sut.ValXmlString);
		}

		[TestMethod]
		public void DayTimeDurationStype_ValXmlString_RejectsYearOrMonthDesignator()
		{
			var sut = CreateDetached<dayTimeDuration_Stype>();
			var originalVal = sut.val;

			// The generated facet "[^YM]*[DT].*" forbids any Y or M designator anywhere in the string;
			// "P1Y" violates it and must surface via StoreError (from the caught ValidationException),
			// not an unhandled exception.
			sut.ValXmlString = "P1Y";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void DayTimeDurationStype_ValXmlString_RejectsNullValue()
		{
			var sut = CreateDetached<dayTimeDuration_Stype>();
			var originalVal = sut.val;

			// A null ValXmlString assignment must be reported via StoreError, not silently ignored --
			// the generated setter's own early-exit-on-equality-with-null-and-null would otherwise no-op.
			sut.ValXmlString = null!;

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void YearMonthDurationStype_ValXmlString_RoundTripsYearAndMonthComponents()
		{
			var sut = CreateDetached<yearMonthDuration_Stype>();

			// yearMonthDuration permits only Y/M(-for-months) designators, no D or T section at all.
			sut.ValXmlString = "P5Y2M";

			Assert.AreEqual("P5Y2M", sut.val);
			Assert.AreEqual("P5Y2M", sut.ValXmlString);
		}

		[TestMethod]
		public void YearMonthDurationStype_ValXmlString_RejectsDayOrTimeSection()
		{
			var sut = CreateDetached<yearMonthDuration_Stype>();
			var originalVal = sut.val;

			// The generated facet "[^DT]*" forbids any D or T character in the string; "P1D" violates it.
			sut.ValXmlString = "P1D";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void YearMonthDurationStype_ValXmlString_RejectsNullValue()
		{
			var sut = CreateDetached<yearMonthDuration_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = null!;

			Assert.AreEqual(originalVal, sut.val);
		}
	}
}
