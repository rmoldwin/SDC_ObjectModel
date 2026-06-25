using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema.Tests.OM
{
	/// <summary>
	/// Object-model boundary tests for every numeric ResponseType datatype. These verify the
	/// in-memory <c>val</c> and constraint-property behavior (storage, ShouldSerialize gating,
	/// [Range] enforcement, float/double special values, and graceful malformed-input parsing)
	/// without serialization. Round-trip preservation is covered in
	/// <see cref="SDC.Schema.Tests.Functional.Serialization.NumericResponseTypeRoundTripTests"/>.
	///
	/// Boundary values follow the XML-Schema value space of each datatype, except where the
	/// backing .NET type cannot represent the full XSD range (the integer family and decimal),
	/// in which case the .NET/[Range] limit is the binding constraint. See
	/// <c>Documentation/NumericRange_XSD_vs_NET.md</c> for the full divergence table.
	/// </summary>
	[TestClass]
	public class NumericResponseTypeBoundaryTests
	{
		[TestInitialize]
		public void Init() => BaseType.ResetLastTopNode();

		private static T DE<T>(ItemChoiceType ict) where T : BaseType
			=> NumericResponseTypeTestHelpers.DE<T>(ict, out _);

		/// <summary>
		/// Verifies the soft-reject contract (issue #8) for a numeric setter: the assignment must
		/// NOT throw, the stored value must be unchanged from <paramref name="prior"/>, and the
		/// offending value must be recorded on the node (<see cref="BaseType.RejectedValues"/>
		/// keyed by <paramref name="prop"/>) with a message that includes the offending value so a
		/// UI can surface it for correction.
		/// </summary>
		private static void AssertValSoftRejected(
			BaseType node, string prop, decimal prior,
			Func<decimal> read, Action assign, object offending, string because)
		{
			assign(); // soft reject: must not throw
			// Rationale: an invalid value must never enter the OM — the prior/unset value is retained.
			Assert.AreEqual(prior, read(), $"{because} — invalid value must not be stored (prior retained).");
			// Rationale: the offending value is relocated to the out-of-band store, never silently dropped.
			Assert.IsTrue(node.RejectedValues.TryGetValue(prop, out var rv),
				$"{because} — a rejected value for '{prop}' must be recorded.");
			Assert.AreEqual(offending, rv!.AttemptedValue,
				$"{because} — the recorded rejected value must be the offending value.");
			// Rationale: the message must name the offending value so the user knows what to fix.
			StringAssert.Contains(rv.Message, offending!.ToString(),
				$"{because} — the rejection message must include the offending value.");
		}

		#region Axis A — val min / max

		[TestMethod]
		public void Byte_Val_MinMax_StoredExactly()
		{
			// xs:byte maps to .NET sbyte (signed, -128..127), NOT byte. The full XSD range is
			// representable, so both endpoints must store without overflow.
			var min = DE<byte_DEtype>(ItemChoiceType.@byte); min.val = sbyte.MinValue;
			var max = DE<byte_DEtype>(ItemChoiceType.@byte); max.val = sbyte.MaxValue;
			Assert.AreEqual(sbyte.MinValue, min.val, "xs:byte min (-128) must round-trip through the sbyte-backed val.");
			Assert.AreEqual(sbyte.MaxValue, max.val, "xs:byte max (127) must store exactly.");
		}

		[TestMethod]
		public void Short_Val_MinMax_StoredExactly()
		{
			// xs:short = .NET short; full range representable.
			var min = DE<short_DEtype>(ItemChoiceType.@short); min.val = short.MinValue;
			var max = DE<short_DEtype>(ItemChoiceType.@short); max.val = short.MaxValue;
			Assert.AreEqual(short.MinValue, min.val, "xs:short min must store exactly.");
			Assert.AreEqual(short.MaxValue, max.val, "xs:short max must store exactly.");
		}

		[TestMethod]
		public void Int_Val_MinMax_StoredExactly()
		{
			// xs:int = .NET int; full range representable.
			var min = DE<int_DEtype>(ItemChoiceType.@int); min.val = int.MinValue;
			var max = DE<int_DEtype>(ItemChoiceType.@int); max.val = int.MaxValue;
			Assert.AreEqual(int.MinValue, min.val, "xs:int min must store exactly.");
			Assert.AreEqual(int.MaxValue, max.val, "xs:int max must store exactly.");
		}

		[TestMethod]
		public void Long_Val_MinMax_StoredExactly()
		{
			// xs:long = .NET long; full range representable.
			var min = DE<long_DEtype>(ItemChoiceType.@long); min.val = long.MinValue;
			var max = DE<long_DEtype>(ItemChoiceType.@long); max.val = long.MaxValue;
			Assert.AreEqual(long.MinValue, min.val, "xs:long min must store exactly.");
			Assert.AreEqual(long.MaxValue, max.val, "xs:long max must store exactly.");
		}

		[TestMethod]
		public void UnsignedByte_Val_MinMax_StoredExactly()
		{
			// xs:unsignedByte = .NET byte (0..255); full range representable.
			var min = DE<unsignedByte_DEtype>(ItemChoiceType.unsignedByte); min.val = byte.MinValue;
			var max = DE<unsignedByte_DEtype>(ItemChoiceType.unsignedByte); max.val = byte.MaxValue;
			Assert.AreEqual(byte.MinValue, min.val, "xs:unsignedByte min (0) must store exactly.");
			Assert.AreEqual(byte.MaxValue, max.val, "xs:unsignedByte max (255) must store exactly.");
		}

		[TestMethod]
		public void UnsignedShort_Val_MinMax_StoredExactly()
		{
			var min = DE<unsignedShort_DEtype>(ItemChoiceType.unsignedShort); min.val = ushort.MinValue;
			var max = DE<unsignedShort_DEtype>(ItemChoiceType.unsignedShort); max.val = ushort.MaxValue;
			Assert.AreEqual(ushort.MinValue, min.val, "xs:unsignedShort min (0) must store exactly.");
			Assert.AreEqual(ushort.MaxValue, max.val, "xs:unsignedShort max (65535) must store exactly.");
		}

		[TestMethod]
		public void UnsignedInt_Val_MinMax_StoredExactly()
		{
			var min = DE<unsignedInt_DEtype>(ItemChoiceType.unsignedInt); min.val = uint.MinValue;
			var max = DE<unsignedInt_DEtype>(ItemChoiceType.unsignedInt); max.val = uint.MaxValue;
			Assert.AreEqual(uint.MinValue, min.val, "xs:unsignedInt min (0) must store exactly.");
			Assert.AreEqual(uint.MaxValue, max.val, "xs:unsignedInt max (4294967295) must store exactly.");
		}

		[TestMethod]
		public void UnsignedLong_Val_MinMax_StoredExactly()
		{
			var min = DE<unsignedLong_DEtype>(ItemChoiceType.unsignedLong); min.val = ulong.MinValue;
			var max = DE<unsignedLong_DEtype>(ItemChoiceType.unsignedLong); max.val = ulong.MaxValue;
			Assert.AreEqual(ulong.MinValue, min.val, "xs:unsignedLong min (0) must store exactly.");
			Assert.AreEqual(ulong.MaxValue, max.val, "xs:unsignedLong max (18446744073709551615) must store exactly.");
		}

		[TestMethod]
		public void Decimal_Val_MinMax_StoredExactly()
		{
			// xs:decimal is arbitrary-precision/unbounded; .NET decimal caps it at ~+/-7.92e28.
			// The .NET limit is the binding boundary (documented XSD divergence). decimal val has
			// no [Range] attribute, so decimal.MinValue/MaxValue are accepted.
			var min = DE<decimal_DEtype>(ItemChoiceType.@decimal); min.val = decimal.MinValue;
			var max = DE<decimal_DEtype>(ItemChoiceType.@decimal); max.val = decimal.MaxValue;
			Assert.AreEqual(decimal.MinValue, min.val, "decimal.MinValue is the binding .NET lower bound for xs:decimal.");
			Assert.AreEqual(decimal.MaxValue, max.val, "decimal.MaxValue is the binding .NET upper bound for xs:decimal.");
		}

		// Integer-family large-magnitude boundary. The OM caps xs:integer (unbounded in XSD) with a
		// decimal-backed val. Besides the [Range], val also carries a custom MaxDigitsAttribute(29)
		// that validates value.ToString().Length <= 29. Because ToString() includes the '-' sign for
		// negatives, the effective limit is 29 significant digits for positives but only 28 for
		// negatives (see Integer_Val_DecimalExtremes_RangeEdgeBehavior). The positive boundary uses a
		// 29-digit value; the negative boundary uses a 28-digit value so both stay inside the limit.
		private const decimal IntegerFamilyLargePos = 7.9e28m;   // 29 digits
		private const decimal IntegerFamilyLargeNeg = -7.9e27m;  // 28 digits (+ sign = 29 chars)

		[TestMethod]
		public void Integer_Val_MinMax_StoredExactly()
		{
			var min = DE<integer_DEtype>(ItemChoiceType.integer); min.val = IntegerFamilyLargeNeg;
			var max = DE<integer_DEtype>(ItemChoiceType.integer); max.val = IntegerFamilyLargePos;
			Assert.AreEqual(IntegerFamilyLargeNeg, min.val, "xs:integer large-negative boundary must store exactly.");
			Assert.AreEqual(IntegerFamilyLargePos, max.val, "xs:integer large-positive boundary must store exactly.");
		}

		[TestMethod]
		public void Integer_Val_DecimalExtremes_RangeEdgeBehavior()
		{
			// Characterizes a documented divergence caused by the custom MaxDigitsAttribute(29) on val,
			// which validates value.ToString().Length <= 29. decimal.MaxValue is 29 digits ("7922...0335")
			// and is accepted, but decimal.MinValue renders as "-7922...0335" (30 chars including the
			// sign) and is rejected by MaxDigits(29). Under the soft-reject contract the rejected value
			// is not stored (val keeps its prior default) and is recorded on the node — no exception.
			var accept = DE<integer_DEtype>(ItemChoiceType.integer);
			accept.val = decimal.MaxValue;
			Assert.AreEqual(decimal.MaxValue, accept.val, "decimal.MaxValue is 29 chars and is accepted by MaxDigits(29).");

			var reject = DE<integer_DEtype>(ItemChoiceType.integer);
			AssertValSoftRejected(reject, "val", reject.val, () => reject.val,
				() => reject.val = decimal.MinValue, decimal.MinValue,
				"decimal.MinValue is 30 chars (sign + 29 digits) and is rejected by MaxDigits(29) (documented divergence)");
		}

		[TestMethod]
		public void NegativeInteger_Val_BoundaryValues_StoredExactly()
		{
			// xs:negativeInteger value space is (-inf, -1]. XSD-exact upper endpoint -1 is
			// representable; lower endpoint uses a value safely inside the .NET [Range].
			var min = DE<negativeInteger_DEtype>(ItemChoiceType.negativeInteger); min.val = IntegerFamilyLargeNeg;
			var max = DE<negativeInteger_DEtype>(ItemChoiceType.negativeInteger); max.val = -1m;
			Assert.AreEqual(IntegerFamilyLargeNeg, min.val, "negativeInteger large-negative boundary must store exactly.");
			Assert.AreEqual(-1m, max.val, "negativeInteger XSD-exact upper boundary -1 must be accepted.");
		}

		[TestMethod]
		public void NonNegativeInteger_Val_BoundaryValues_StoredExactly()
		{
			// xs:nonNegativeInteger value space is [0, +inf). XSD-exact lower endpoint 0; upper inside .NET [Range].
			var min = DE<nonNegativeInteger_DEtype>(ItemChoiceType.nonNegativeInteger); min.val = 0m;
			var max = DE<nonNegativeInteger_DEtype>(ItemChoiceType.nonNegativeInteger); max.val = IntegerFamilyLargePos;
			Assert.AreEqual(0m, min.val, "nonNegativeInteger XSD-exact lower boundary 0 must be accepted.");
			Assert.AreEqual(IntegerFamilyLargePos, max.val, "nonNegativeInteger large-positive boundary must store exactly.");
		}

		[TestMethod]
		public void PositiveInteger_Val_BoundaryValues_StoredExactly()
		{
			// xs:positiveInteger value space is [1, +inf). XSD-exact lower endpoint 1; upper inside .NET [Range].
			var min = DE<positiveInteger_DEtype>(ItemChoiceType.positiveInteger); min.val = 1m;
			var max = DE<positiveInteger_DEtype>(ItemChoiceType.positiveInteger); max.val = IntegerFamilyLargePos;
			Assert.AreEqual(1m, min.val, "positiveInteger XSD-exact lower boundary 1 must be accepted.");
			Assert.AreEqual(IntegerFamilyLargePos, max.val, "positiveInteger large-positive boundary must store exactly.");
		}

		[TestMethod]
		public void NonPositiveInteger_Val_BoundaryValues_StoredExactly()
		{
			// xs:nonPositiveInteger value space is (-inf, 0]. XSD-exact upper endpoint 0; lower inside .NET [Range].
			var min = DE<nonPositiveInteger_DEtype>(ItemChoiceType.nonPositiveInteger); min.val = IntegerFamilyLargeNeg;
			var max = DE<nonPositiveInteger_DEtype>(ItemChoiceType.nonPositiveInteger); max.val = 0m;
			Assert.AreEqual(IntegerFamilyLargeNeg, min.val, "nonPositiveInteger large-negative boundary must store exactly.");
			Assert.AreEqual(0m, max.val, "nonPositiveInteger XSD-exact upper boundary 0 must be accepted.");
		}

		[TestMethod]
		public void Float_Val_MinMax_StoredExactly()
		{
			var min = DE<float_DEtype>(ItemChoiceType.@float); min.val = float.MinValue;
			var max = DE<float_DEtype>(ItemChoiceType.@float); max.val = float.MaxValue;
			Assert.AreEqual(float.MinValue, min.val, "xs:float min (-3.4e38) must store exactly.");
			Assert.AreEqual(float.MaxValue, max.val, "xs:float max (3.4e38) must store exactly.");
		}

		[TestMethod]
		public void Double_Val_MinMax_StoredExactly()
		{
			var min = DE<double_DEtype>(ItemChoiceType.@double); min.val = double.MinValue;
			var max = DE<double_DEtype>(ItemChoiceType.@double); max.val = double.MaxValue;
			Assert.AreEqual(double.MinValue, min.val, "xs:double min must store exactly.");
			Assert.AreEqual(double.MaxValue, max.val, "xs:double max must store exactly.");
		}

		#endregion

		#region Axis A — explicit zero vs default zero (ShouldSerialize contract)

		[TestMethod]
		public void Int_Val_ZeroExplicit_ShouldSerializeTrue()
		{
			// Setting val = 0 via the property setter sets _shouldSerializeval = true, so an
			// explicit zero is emitted. This distinguishes "answered 0" from "unanswered".
			var de = DE<int_DEtype>(ItemChoiceType.@int);
			de.val = 0;
			Assert.IsTrue(de.ShouldSerializeval(), "Explicitly assigning val = 0 must cause ShouldSerializeval() to return true.");
		}

		[TestMethod]
		public void Int_Val_ZeroByDefault_ShouldSerializeFalse()
		{
			// A node whose val was never assigned must suppress the attribute (no false "0" answer).
			var de = DE<int_DEtype>(ItemChoiceType.@int);
			Assert.IsFalse(de.ShouldSerializeval(), "An untouched val (default 0) must have ShouldSerializeval() return false.");
		}

		[TestMethod]
		public void Double_Val_ZeroExplicit_ShouldSerializeTrue()
		{
			var de = DE<double_DEtype>(ItemChoiceType.@double);
			de.val = 0.0;
			Assert.IsTrue(de.ShouldSerializeval(), "Explicit 0.0 must be serialized for floating-point response values.");
		}

		[TestMethod]
		public void UnsignedLong_Val_ZeroByDefault_ShouldSerializeFalse()
		{
			// For unsigned types 0 is both the XSD minimum and the default; an untouched val must
			// still be suppressed so absence is distinguishable from an explicit 0 answer.
			var de = DE<unsignedLong_DEtype>(ItemChoiceType.unsignedLong);
			Assert.IsFalse(de.ShouldSerializeval(), "Untouched unsigned val must not serialize even though 0 is its minimum.");
		}

		#endregion

		#region Axis B — constraint properties store + ShouldSerialize

		[TestMethod]
		public void Int_Constraints_MinMaxInclusive_StoredAndSerialized()
		{
			// minInclusive/maxInclusive are independent facet properties; setting them must store
			// the exact value and flag the facet for serialization.
			var de = DE<int_DEtype>(ItemChoiceType.@int);
			de.minInclusive = int.MinValue;
			de.maxInclusive = int.MaxValue;
			Assert.AreEqual(int.MinValue, de.minInclusive, "minInclusive must store the assigned boundary.");
			Assert.AreEqual(int.MaxValue, de.maxInclusive, "maxInclusive must store the assigned boundary.");
			Assert.IsTrue(de.ShouldSerializeminInclusive(), "Set minInclusive must serialize.");
			Assert.IsTrue(de.ShouldSerializemaxInclusive(), "Set maxInclusive must serialize.");
		}

		[TestMethod]
		public void Decimal_Constraints_Default_ShouldSerializeFalse()
		{
			// Untouched constraint facets must not appear in output.
			var de = DE<decimal_DEtype>(ItemChoiceType.@decimal);
			Assert.IsFalse(de.ShouldSerializeminInclusive(), "Untouched minInclusive must not serialize.");
			Assert.IsFalse(de.ShouldSerializemaxInclusive(), "Untouched maxInclusive must not serialize.");
		}

		#endregion

		#region Axis B — [Range] enforcement (negative tests)

		[TestMethod]
		public void Int_MaxExclusive_SetToIntMinValue_SoftRejected()
		{
			// int_DEtype.maxExclusive carries [Range(-2147483647, 2147483647)], which EXCLUDES
			// int.MinValue (-2147483648). Under soft-reject the assignment must not throw, the value
			// must stay at its prior default, and the offending value must be recorded — a documented
			// .NET narrowing of the xs:int value space at the exclusive-bound facet.
			var de = DE<int_DEtype>(ItemChoiceType.@int);
			AssertValSoftRejected(de, "maxExclusive", de.maxExclusive, () => de.maxExclusive,
				() => de.maxExclusive = int.MinValue, int.MinValue,
				"maxExclusive = int.MinValue is below the [Range] lower bound -2147483647");
		}

		[TestMethod]
		public void Long_MinExclusive_AtLongMaxValue_NotEnforcedDueToDoublePrecision()
		{
			// Divergence characterization: long_DEtype.minExclusive carries a [Range] whose bounds are
			// long literals, but RangeAttribute has no (long,long) overload so the compiler binds the
			// (double,double) one. long.MaxValue and the generated upper bound round to the SAME double,
			// so the exclusive-bound facet cannot be enforced at the type extreme: the assignment is
			// accepted instead of throwing. Pinned here so a future precise validator is a deliberate change.
			var de = DE<long_DEtype>(ItemChoiceType.@long);
			de.minExclusive = long.MaxValue;
			Assert.AreEqual(long.MaxValue, de.minExclusive,
				"long.MaxValue is accepted for minExclusive because the double-based [Range] cannot distinguish it from MaxValue-1.");
		}

		[TestMethod]
		public void Long_MaxExclusive_AtLongMinValue_NotEnforcedDueToDoublePrecision()
		{
			// Same double-precision [Range] divergence as above, at the lower extreme.
			var de = DE<long_DEtype>(ItemChoiceType.@long);
			de.maxExclusive = long.MinValue;
			Assert.AreEqual(long.MinValue, de.maxExclusive,
				"long.MinValue is accepted for maxExclusive because the double-based [Range] cannot distinguish it from MinValue+1.");
		}

		[TestMethod]
		public void PositiveInteger_Val_SetToNegativeOne_SoftRejected()
		{
			// positiveInteger.val [Range] is [1, ~7.92e28]; -1 is below the lower bound. (0 is the
			// decimal default and is skipped by the setter's change-guard, so -1 is used to trigger.)
			// Soft-reject: no throw, value unchanged, offending value recorded.
			var de = DE<positiveInteger_DEtype>(ItemChoiceType.positiveInteger);
			AssertValSoftRejected(de, "val", de.val, () => de.val, () => de.val = -1m, -1m,
				"positiveInteger val = -1 is below the value space, which starts at 1");
		}

		[TestMethod]
		public void NegativeInteger_Val_SetToOne_SoftRejected()
		{
			// negativeInteger.val [Range] is [~-7.92e28, -1]; 1 is above the upper bound.
			var de = DE<negativeInteger_DEtype>(ItemChoiceType.negativeInteger);
			AssertValSoftRejected(de, "val", de.val, () => de.val, () => de.val = 1m, 1m,
				"negativeInteger val = 1 is above the value space, which ends at -1");
		}

		[TestMethod]
		public void NonNegativeInteger_Val_SetToNegativeOne_SoftRejected()
		{
			// nonNegativeInteger.val [Range] is [0, ~7.92e28]; -1 is below the lower bound.
			var de = DE<nonNegativeInteger_DEtype>(ItemChoiceType.nonNegativeInteger);
			AssertValSoftRejected(de, "val", de.val, () => de.val, () => de.val = -1m, -1m,
				"nonNegativeInteger val = -1 is below the value space, which starts at 0");
		}

		[TestMethod]
		public void NonPositiveInteger_Val_SetToOne_SoftRejected()
		{
			// nonPositiveInteger.val [Range] is [~-7.92e28, 0]; 1 is above the upper bound.
			var de = DE<nonPositiveInteger_DEtype>(ItemChoiceType.nonPositiveInteger);
			AssertValSoftRejected(de, "val", de.val, () => de.val, () => de.val = 1m, 1m,
				"nonPositiveInteger val = 1 is above the value space, which ends at 0");
		}

		#endregion

		#region Axis C — float / double special values (NaN, +/-Inf, -0, Epsilon)

		[TestMethod]
		public void Float_Val_SpecialValues_PreservedExactly()
		{
			// xs:float's value space legally includes NaN, +/-INF and -0. The OM must preserve them
			// bit-for-bit. == treats -0.0 == 0.0, so a bit comparison is required for the sign of zero.
			var nan = DE<float_DEtype>(ItemChoiceType.@float); nan.val = float.NaN;
			var pInf = DE<float_DEtype>(ItemChoiceType.@float); pInf.val = float.PositiveInfinity;
			var nInf = DE<float_DEtype>(ItemChoiceType.@float); nInf.val = float.NegativeInfinity;
			var nZero = DE<float_DEtype>(ItemChoiceType.@float); nZero.val = -0.0f;
			var eps = DE<float_DEtype>(ItemChoiceType.@float); eps.val = float.Epsilon;

			Assert.IsTrue(float.IsNaN(nan.val), "float.NaN must be retained.");
			Assert.IsTrue(float.IsPositiveInfinity(pInf.val), "float.PositiveInfinity must be retained.");
			Assert.IsTrue(float.IsNegativeInfinity(nInf.val), "float.NegativeInfinity must be retained.");
			// Divergence: the generated val setter skips assignment when the incoming value .Equals the
			// current one. For a fresh node the backing field is +0.0f and (+0.0f).Equals(-0.0f) is true,
			// so assigning -0 collapses to +0. The OM does NOT preserve the sign of zero.
			Assert.AreEqual(0.0f, nZero.val, "Negative zero is numerically zero; the OM does not preserve its sign bit (documented divergence).");
			Assert.AreEqual(float.Epsilon, eps.val, "float.Epsilon (smallest denormal) must be retained.");
		}

		[TestMethod]
		public void Double_Val_SpecialValues_PreservedExactly()
		{
			var nan = DE<double_DEtype>(ItemChoiceType.@double); nan.val = double.NaN;
			var pInf = DE<double_DEtype>(ItemChoiceType.@double); pInf.val = double.PositiveInfinity;
			var nInf = DE<double_DEtype>(ItemChoiceType.@double); nInf.val = double.NegativeInfinity;
			var nZero = DE<double_DEtype>(ItemChoiceType.@double); nZero.val = -0.0d;
			var eps = DE<double_DEtype>(ItemChoiceType.@double); eps.val = double.Epsilon;

			Assert.IsTrue(double.IsNaN(nan.val), "double.NaN must be retained.");
			Assert.IsTrue(double.IsPositiveInfinity(pInf.val), "double.PositiveInfinity must be retained.");
			Assert.IsTrue(double.IsNegativeInfinity(nInf.val), "double.NegativeInfinity must be retained.");
			// Divergence: see Float_Val_SpecialValues_PreservedExactly. Assigning -0.0 to a fresh node
			// collapses to +0.0 because the setter's .Equals change-guard treats them as equal.
			Assert.AreEqual(0.0d, nZero.val, "Negative zero is numerically zero; the OM does not preserve its sign bit (documented divergence).");
			Assert.AreEqual(double.Epsilon, eps.val, "double.Epsilon (smallest denormal) must be retained.");
		}

		#endregion

		#region Axis D — totalDigits / fractionDigits facets

		[TestMethod]
		public void Int_TotalDigits_ExplicitVsDefault_ShouldSerializeContract()
		{
			// totalDigits is an XSD constraining facet (byte-typed); it must serialize only when set.
			var def = DE<int_DEtype>(ItemChoiceType.@int);
			Assert.IsFalse(def.ShouldSerializetotalDigits(), "Untouched totalDigits must not serialize.");

			var set = DE<int_DEtype>(ItemChoiceType.@int);
			set.totalDigits = 5;
			Assert.AreEqual((byte)5, set.totalDigits, "totalDigits must store the assigned value.");
			Assert.IsTrue(set.ShouldSerializetotalDigits(), "Explicitly set totalDigits must serialize.");
		}

		[TestMethod]
		public void Decimal_FractionDigits_ExplicitVsDefault_ShouldSerializeContract()
		{
			// fractionDigits gates lexical precision for decimal/float/double; serialize only when set.
			var def = DE<decimal_DEtype>(ItemChoiceType.@decimal);
			Assert.IsFalse(def.ShouldSerializefractionDigits(), "Untouched fractionDigits must not serialize.");

			var set = DE<decimal_DEtype>(ItemChoiceType.@decimal);
			set.fractionDigits = 2;
			Assert.AreEqual((byte)2, set.fractionDigits, "fractionDigits must store the assigned value.");
			Assert.IsTrue(set.ShouldSerializefractionDigits(), "Explicitly set fractionDigits must serialize.");
		}

		#endregion

		#region Malformed / out-of-range programmatic input (graceful builder path)

		private static IList<Exception> AddMalformed(ItemChoiceType ict, string badValue)
		{
			var fd = NumericResponseTypeTestHelpers.NewForm(ict, out var q, out _);
			var rf = q.ResponseField_Item!;
			var errors = new List<Exception>();
			// AddDataTypesDE uses TryParse internally and must NOT throw on malformed numeric input;
			// it records a structured error in the supplied list instead.
			SdcDataTypeBuilder.AddDataTypesDE(rf, ict, dtQuantEnum.EQ, badValue, errors);
			return errors;
		}

		[TestMethod]
		public void Int_NonNumericString_ParsedGracefully_ErrorRecorded()
		{
			var errors = AddMalformed(ItemChoiceType.@int, "abc");
			// The builder must swallow the parse failure (no exception) and surface it via the list.
			Assert.IsTrue(errors.Count > 0, "Non-numeric input for xs:int must record a parse error, not throw.");
		}

		[TestMethod]
		public void Int_OverflowString_ParsedGracefully_ErrorRecorded()
		{
			var errors = AddMalformed(ItemChoiceType.@int, "999999999999999999999");
			Assert.IsTrue(errors.Count > 0, "Overflow input for xs:int must record a parse error, not throw.");
		}

		[TestMethod]
		public void Byte_OutOfRangeString_ParsedGracefully_ErrorRecorded()
		{
			// "200" overflows signed sbyte (max 127); must be reported gracefully.
			var errors = AddMalformed(ItemChoiceType.@byte, "200");
			Assert.IsTrue(errors.Count > 0, "Out-of-range input for xs:byte (sbyte) must record a parse error, not throw.");
		}

		[TestMethod]
		public void UnsignedByte_NegativeString_ParsedGracefully_ErrorRecorded()
		{
			// "-1" is invalid for unsigned byte; must be reported gracefully.
			var errors = AddMalformed(ItemChoiceType.unsignedByte, "-1");
			Assert.IsTrue(errors.Count > 0, "Negative input for xs:unsignedByte must record a parse error, not throw.");
		}

		[TestMethod]
		public void Double_NonNumericString_ParsedGracefully_ErrorRecorded()
		{
			var errors = AddMalformed(ItemChoiceType.@double, "not-a-number");
			Assert.IsTrue(errors.Count > 0, "Non-numeric input for xs:double must record a parse error, not throw.");
		}

		[TestMethod]
		public void Decimal_NonNumericString_ParsedGracefully_NoExceptionThrown()
		{
			// Primary contract: the builder never throws on malformed decimal input.
			var fd = NumericResponseTypeTestHelpers.NewForm(ItemChoiceType.@decimal, out var q, out _);
			var rf = q.ResponseField_Item!;
			var errors = new List<Exception>();
			SdcDataTypeBuilder.AddDataTypesDE(rf, ItemChoiceType.@decimal, dtQuantEnum.EQ, "12.3.4", errors);
			Assert.IsTrue(errors.Count > 0, "Malformed decimal input must be recorded as an error without throwing.");
		}

		#endregion
	}
}
