using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on the arbitrary-precision integer restriction family:
	/// integer_Stype, negativeInteger_Stype, nonNegativeInteger_Stype, nonPositiveInteger_Stype, and
	/// positiveInteger_Stype. All five use a CLR `decimal` for val (an approximation of XSD's unbounded
	/// integer, bounded here by decimal's own range), constrained by generated
	/// [FractionDigitsAttribute(0)]/[MaxDigitsAttribute(29)]/[RangeAttribute] facets that the generated
	/// `val` setter enforces via Validator.ValidateProperty. Because that validator throws
	/// ValidationException on violation (not just FormatException from parsing), these tests specifically
	/// cover both parse failures (illegal lexical form) and semantic-range failures (legal decimal, but
	/// outside the type's permitted range/fractional-digits), confirming both are funneled through
	/// StoreError rather than escaping as unhandled exceptions.
	/// </summary>
	[TestClass]
	public class IntegerRestrictionFamily_ValXmlStringTests
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
		public void IntegerStype_ValXmlString_RoundTripsPositiveAndNegativeValues()
		{
			var sut = CreateDetached<integer_Stype>();

			// integer_Stype permits any whole number in its supported range (positive, negative, or zero).
			sut.ValXmlString = "-123456789";
			Assert.AreEqual(-123456789m, sut.val);
			Assert.AreEqual("-123456789", sut.ValXmlString);

			sut.ValXmlString = "987654321";
			Assert.AreEqual(987654321m, sut.val);
			Assert.AreEqual("987654321", sut.ValXmlString);
		}

		[TestMethod]
		public void IntegerStype_ValXmlString_RejectsNonIntegralValue()
		{
			var sut = CreateDetached<integer_Stype>();
			var originalVal = sut.val;

			// "integer" requires FractionDigits = 0; a fractional value like "1.5" is legal decimal syntax
			// but violates the type's facet, and must be handled via StoreError (from the generated val
			// setter's ValidationException), not thrown, and must not corrupt val.
			sut.ValXmlString = "1.5";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void IntegerStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<integer_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = "not-a-number";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void NegativeIntegerStype_ValXmlString_RoundTripsLegalValue()
		{
			var sut = CreateDetached<negativeInteger_Stype>();

			// negativeInteger's range is [min, -1] -- -1 is the boundary "least negative" legal value.
			sut.ValXmlString = "-1";

			Assert.AreEqual(-1m, sut.val);
			Assert.AreEqual("-1", sut.ValXmlString);
		}

		[TestMethod]
		public void NegativeIntegerStype_ValXmlString_RejectsZero()
		{
			var sut = CreateDetached<negativeInteger_Stype>();
			var originalVal = sut.val;

			// 0 is outside negativeInteger's permitted range (which excludes 0); this is a legal decimal
			// value but fails the type's RangeAttribute, so it must be routed through StoreError.
			sut.ValXmlString = "0";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void NonNegativeIntegerStype_ValXmlString_RoundTripsZeroAndPositiveValue()
		{
			var sut = CreateDetached<nonNegativeInteger_Stype>();

			// nonNegativeInteger's range is [0, max] -- 0 is the boundary legal value.
			sut.ValXmlString = "0";
			Assert.AreEqual(0m, sut.val);
			Assert.AreEqual("0", sut.ValXmlString);

			sut.ValXmlString = "42";
			Assert.AreEqual(42m, sut.val);
			Assert.AreEqual("42", sut.ValXmlString);
		}

		[TestMethod]
		public void NonNegativeIntegerStype_ValXmlString_RejectsNegativeValue()
		{
			var sut = CreateDetached<nonNegativeInteger_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = "-1";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void NonPositiveIntegerStype_ValXmlString_RoundTripsZeroAndNegativeValue()
		{
			var sut = CreateDetached<nonPositiveInteger_Stype>();

			// nonPositiveInteger's range is [min, 0] -- 0 is the boundary legal value.
			sut.ValXmlString = "0";
			Assert.AreEqual(0m, sut.val);
			Assert.AreEqual("0", sut.ValXmlString);

			sut.ValXmlString = "-42";
			Assert.AreEqual(-42m, sut.val);
			Assert.AreEqual("-42", sut.ValXmlString);
		}

		[TestMethod]
		public void NonPositiveIntegerStype_ValXmlString_RejectsPositiveValue()
		{
			var sut = CreateDetached<nonPositiveInteger_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = "1";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void PositiveIntegerStype_ValXmlString_RoundTripsLegalValue()
		{
			var sut = CreateDetached<positiveInteger_Stype>();

			// positiveInteger's range is [1, max] -- 1 is the boundary "least positive" legal value.
			sut.ValXmlString = "1";

			Assert.AreEqual(1m, sut.val);
			Assert.AreEqual("1", sut.ValXmlString);
		}

		[TestMethod]
		public void PositiveIntegerStype_ValXmlString_RejectsZero()
		{
			var sut = CreateDetached<positiveInteger_Stype>();
			var originalVal = sut.val;

			// 0 is outside positiveInteger's permitted range (which excludes 0).
			sut.ValXmlString = "0";

			Assert.AreEqual(originalVal, sut.val);
		}
	}
}
