using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Tests.OM;
using System;
using H = SDC.Schema.Tests.OM.NumericResponseTypeTestHelpers;

namespace SDC.Schema.Tests.Functional.Serialization
{
	/// <summary>
	/// Functional round-trip tests for numeric ResponseType datatypes. Each test builds a small
	/// FormDesign tree with one numeric QuestionResponse, sets <c>val</c> (or a constraint facet) to
	/// a boundary value, serializes the whole tree, deserializes it, and asserts the value survived
	/// exactly. XML and JSON get the full min/max matrix; BSON and MsgPack get a focused parity
	/// subset (min/max, constraints, float/double specials). Boundary value selection and the
	/// XSD-vs-.NET divergences are documented in <c>Documentation/NumericRange_XSD_vs_NET.md</c> and
	/// exercised at the object-model level in <see cref="OM.NumericResponseTypeBoundaryTests"/>.
	/// </summary>
	[TestClass]
	public class NumericResponseTypeRoundTripTests
	{
		// Integer family is decimal-backed; the custom MaxDigitsAttribute(29) counts the sign for
		// negatives, so the negative boundary uses 28 digits and the positive boundary 29.
		private const decimal IntegerFamilyLargePos = 7.9e28m;
		private const decimal IntegerFamilyLargeNeg = -7.9e27m;

		[TestInitialize]
		public void Init() => BaseType.ResetLastTopNode();

		private static void XmlJsonRoundTripVal<T>(ItemChoiceType ict, Action<T> setVal, Func<T, bool> matches, string desc)
			where T : BaseType
		{
			H.AssertValRoundTrip(ict, setVal, matches, H.XmlRoundTrip, $"XML round-trip must preserve {desc}.");
			H.AssertValRoundTrip(ict, setVal, matches, H.JsonRoundTrip, $"JSON round-trip must preserve {desc}.");
		}

		private static void XmlRoundTripVal<T>(ItemChoiceType ict, Action<T> setVal, Func<T, bool> matches, string desc)
			where T : BaseType
			=> H.AssertValRoundTrip(ict, setVal, matches, H.XmlRoundTrip, $"XML round-trip must preserve {desc}.");

		/// <summary>
		/// Characterizes a serializer limitation: building + setting <paramref name="setVal"/> succeeds,
		/// but the chosen <paramref name="roundTrip"/> throws. Pins current behavior so a future fix is
		/// a deliberate, test-visible change.
		/// </summary>
		private static void AssertRoundTripThrows<T>(ItemChoiceType ict, Action<T> setVal, Func<FormDesignType, FormDesignType> roundTrip, string because)
			where T : BaseType
		{
			var node = H.DE<T>(ict, out var fd);
			setVal(node);
			try
			{
				roundTrip(fd);
				Assert.Fail(because);
			}
			catch (AssertFailedException) { throw; }
			catch (Exception) { /* expected */ }
		}

		#region XML + JSON val min/max round-trip (full matrix, all 16 types)

		[TestMethod]
		public void Byte_Val_RoundTrip_PreservesMinMax()
		{
			// xs:byte -> .NET sbyte; both signed endpoints must survive lexical (XML) and JSON encoding.
			XmlJsonRoundTripVal<byte_DEtype>(ItemChoiceType.@byte, d => d.val = sbyte.MinValue, d => d.val == sbyte.MinValue, "sbyte.MinValue");
			XmlJsonRoundTripVal<byte_DEtype>(ItemChoiceType.@byte, d => d.val = sbyte.MaxValue, d => d.val == sbyte.MaxValue, "sbyte.MaxValue");
		}

		[TestMethod]
		public void Short_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<short_DEtype>(ItemChoiceType.@short, d => d.val = short.MinValue, d => d.val == short.MinValue, "short.MinValue");
			XmlJsonRoundTripVal<short_DEtype>(ItemChoiceType.@short, d => d.val = short.MaxValue, d => d.val == short.MaxValue, "short.MaxValue");
		}

		[TestMethod]
		public void Int_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<int_DEtype>(ItemChoiceType.@int, d => d.val = int.MinValue, d => d.val == int.MinValue, "int.MinValue");
			XmlJsonRoundTripVal<int_DEtype>(ItemChoiceType.@int, d => d.val = int.MaxValue, d => d.val == int.MaxValue, "int.MaxValue");
		}

		[TestMethod]
		public void Long_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<long_DEtype>(ItemChoiceType.@long, d => d.val = long.MinValue, d => d.val == long.MinValue, "long.MinValue");
			XmlJsonRoundTripVal<long_DEtype>(ItemChoiceType.@long, d => d.val = long.MaxValue, d => d.val == long.MaxValue, "long.MaxValue");
		}

		[TestMethod]
		public void UnsignedByte_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<unsignedByte_DEtype>(ItemChoiceType.unsignedByte, d => d.val = byte.MinValue, d => d.val == byte.MinValue, "byte.MinValue");
			XmlJsonRoundTripVal<unsignedByte_DEtype>(ItemChoiceType.unsignedByte, d => d.val = byte.MaxValue, d => d.val == byte.MaxValue, "byte.MaxValue");
		}

		[TestMethod]
		public void UnsignedShort_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<unsignedShort_DEtype>(ItemChoiceType.unsignedShort, d => d.val = ushort.MinValue, d => d.val == ushort.MinValue, "ushort.MinValue");
			XmlJsonRoundTripVal<unsignedShort_DEtype>(ItemChoiceType.unsignedShort, d => d.val = ushort.MaxValue, d => d.val == ushort.MaxValue, "ushort.MaxValue");
		}

		[TestMethod]
		public void UnsignedInt_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<unsignedInt_DEtype>(ItemChoiceType.unsignedInt, d => d.val = uint.MinValue, d => d.val == uint.MinValue, "uint.MinValue");
			XmlJsonRoundTripVal<unsignedInt_DEtype>(ItemChoiceType.unsignedInt, d => d.val = uint.MaxValue, d => d.val == uint.MaxValue, "uint.MaxValue");
		}

		[TestMethod]
		public void UnsignedLong_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<unsignedLong_DEtype>(ItemChoiceType.unsignedLong, d => d.val = ulong.MinValue, d => d.val == ulong.MinValue, "ulong.MinValue");
			XmlJsonRoundTripVal<unsignedLong_DEtype>(ItemChoiceType.unsignedLong, d => d.val = ulong.MaxValue, d => d.val == ulong.MaxValue, "ulong.MaxValue");
		}

		[TestMethod]
		public void Decimal_Val_XmlRoundTrip_PreservesMinMax()
		{
			// decimal_DEtype.val has no [Range]/MaxDigits, so the full .NET decimal range round-trips
			// through the authoritative SDC XML serializer.
			XmlRoundTripVal<decimal_DEtype>(ItemChoiceType.@decimal, d => d.val = decimal.MinValue, d => d.val == decimal.MinValue, "decimal.MinValue");
			XmlRoundTripVal<decimal_DEtype>(ItemChoiceType.@decimal, d => d.val = decimal.MaxValue, d => d.val == decimal.MaxValue, "decimal.MaxValue");
		}

		[TestMethod]
		public void Decimal_Val_JsonRoundTrip_LargeWholeNumber_ThrowsDocumented()
		{
			// DIVERGENCE: a decimal whose magnitude exceeds ulong range AND has no fractional part is
			// emitted by Newtonsoft as a bare integer literal; on read it is materialized as a
			// System.Numerics.BigInteger, which the OM's value setter cannot convert to decimal
			// (InvalidCastException). XML preserves these values; JSON currently cannot. Pinned here.
			AssertRoundTripThrows<decimal_DEtype>(ItemChoiceType.@decimal, d => d.val = decimal.MaxValue, H.JsonRoundTrip,
				"JSON round-trip of a large whole-number decimal is expected to throw under current behavior.");
		}

		[TestMethod]
		public void Float_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<float_DEtype>(ItemChoiceType.@float, d => d.val = float.MinValue, d => d.val == float.MinValue, "float.MinValue");
			XmlJsonRoundTripVal<float_DEtype>(ItemChoiceType.@float, d => d.val = float.MaxValue, d => d.val == float.MaxValue, "float.MaxValue");
		}

		[TestMethod]
		public void Double_Val_RoundTrip_PreservesMinMax()
		{
			XmlJsonRoundTripVal<double_DEtype>(ItemChoiceType.@double, d => d.val = double.MinValue, d => d.val == double.MinValue, "double.MinValue");
			XmlJsonRoundTripVal<double_DEtype>(ItemChoiceType.@double, d => d.val = double.MaxValue, d => d.val == double.MaxValue, "double.MaxValue");
		}

		[TestMethod]
		public void Integer_Val_RoundTrip_PreservesBoundaries()
		{
			// Large integer-family magnitudes exceed ulong range, so JSON cannot round-trip them
			// (see Integer_Val_JsonRoundTrip_LargeMagnitude_ThrowsDocumented); XML preserves them.
			XmlRoundTripVal<integer_DEtype>(ItemChoiceType.integer, d => d.val = IntegerFamilyLargeNeg, d => d.val == IntegerFamilyLargeNeg, "integer large-negative");
			XmlRoundTripVal<integer_DEtype>(ItemChoiceType.integer, d => d.val = IntegerFamilyLargePos, d => d.val == IntegerFamilyLargePos, "integer large-positive");
			// Small in-range endpoints still survive JSON; assert both serializers there.
			XmlJsonRoundTripVal<integer_DEtype>(ItemChoiceType.integer, d => d.val = 0m, d => d.val == 0m, "integer 0");
		}

		[TestMethod]
		public void Integer_Val_JsonRoundTrip_LargeMagnitude_ThrowsDocumented()
		{
			// DIVERGENCE (same root cause as decimal): a whole-number integer-family value beyond ulong
			// range deserializes from JSON as BigInteger, which cannot be assigned to the decimal-backed
			// val (InvalidCastException). XML preserves it; JSON currently cannot. Pinned here.
			AssertRoundTripThrows<integer_DEtype>(ItemChoiceType.integer, d => d.val = IntegerFamilyLargePos, H.JsonRoundTrip,
				"JSON round-trip of a large integer-family value is expected to throw under current behavior.");
		}

		[TestMethod]
		public void NegativeInteger_Val_RoundTrip_PreservesBoundaries()
		{
			XmlRoundTripVal<negativeInteger_DEtype>(ItemChoiceType.negativeInteger, d => d.val = IntegerFamilyLargeNeg, d => d.val == IntegerFamilyLargeNeg, "negativeInteger large-negative");
			XmlJsonRoundTripVal<negativeInteger_DEtype>(ItemChoiceType.negativeInteger, d => d.val = -1m, d => d.val == -1m, "negativeInteger XSD-exact upper -1");
		}

		[TestMethod]
		public void NonNegativeInteger_Val_RoundTrip_PreservesBoundaries()
		{
			XmlJsonRoundTripVal<nonNegativeInteger_DEtype>(ItemChoiceType.nonNegativeInteger, d => d.val = 1m, d => d.val == 1m, "nonNegativeInteger 1");
			XmlRoundTripVal<nonNegativeInteger_DEtype>(ItemChoiceType.nonNegativeInteger, d => d.val = IntegerFamilyLargePos, d => d.val == IntegerFamilyLargePos, "nonNegativeInteger large-positive");
		}

		[TestMethod]
		public void PositiveInteger_Val_RoundTrip_PreservesBoundaries()
		{
			XmlJsonRoundTripVal<positiveInteger_DEtype>(ItemChoiceType.positiveInteger, d => d.val = 1m, d => d.val == 1m, "positiveInteger XSD-exact lower 1");
			XmlRoundTripVal<positiveInteger_DEtype>(ItemChoiceType.positiveInteger, d => d.val = IntegerFamilyLargePos, d => d.val == IntegerFamilyLargePos, "positiveInteger large-positive");
		}

		[TestMethod]
		public void NonPositiveInteger_Val_RoundTrip_PreservesBoundaries()
		{
			XmlRoundTripVal<nonPositiveInteger_DEtype>(ItemChoiceType.nonPositiveInteger, d => d.val = IntegerFamilyLargeNeg, d => d.val == IntegerFamilyLargeNeg, "nonPositiveInteger large-negative");
			XmlJsonRoundTripVal<nonPositiveInteger_DEtype>(ItemChoiceType.nonPositiveInteger, d => d.val = -1m, d => d.val == -1m, "nonPositiveInteger -1");
		}

		#endregion

		#region Explicit-zero vs default-zero round-trip (ShouldSerialize presence)

		[TestMethod]
		public void Int_ZeroExplicit_XmlRoundTrip_AttributePresent()
		{
			// An explicitly answered 0 must serialize and be reconstituted as an explicit value, so the
			// round-tripped node still reports ShouldSerializeval() == true.
			var node = H.DE<int_DEtype>(ItemChoiceType.@int, out var fd);
			node.val = 0;
			var node2 = H.FindResponseDE<int_DEtype>(H.XmlRoundTrip(fd));
			Assert.AreEqual(0, node2.val, "Explicit 0 must round-trip as 0.");
			Assert.IsTrue(node2.ShouldSerializeval(), "Explicit 0 must remain serializable after an XML round-trip.");
		}

		[TestMethod]
		public void Int_ZeroDefault_XmlRoundTrip_AttributeAbsent()
		{
			// A never-answered value must not be emitted, so the round-tripped node reports
			// ShouldSerializeval() == false (the attribute was absent in the XML).
			var node = H.DE<int_DEtype>(ItemChoiceType.@int, out var fd);
			var node2 = H.FindResponseDE<int_DEtype>(H.XmlRoundTrip(fd));
			Assert.IsFalse(node2.ShouldSerializeval(), "An unset val must remain absent (ShouldSerializeval false) after an XML round-trip.");
		}

		[TestMethod]
		public void Int_ZeroExplicit_JsonRoundTrip_PropertyPresent()
		{
			var node = H.DE<int_DEtype>(ItemChoiceType.@int, out var fd);
			node.val = 0;
			var node2 = H.FindResponseDE<int_DEtype>(H.JsonRoundTrip(fd));
			Assert.AreEqual(0, node2.val, "Explicit 0 must round-trip as 0 through JSON.");
			Assert.IsTrue(node2.ShouldSerializeval(), "Explicit 0 must remain serializable after a JSON round-trip.");
		}

		#endregion

		#region float / double special values (NaN, +/-Infinity) — XML and JSON

		[TestMethod]
		public void Float_Specials_XmlRoundTrip_Preserved()
		{
			// XSD lexical forms are "NaN", "INF", "-INF"; the XML serializer must emit and re-read them.
			H.AssertValRoundTrip<float_DEtype>(ItemChoiceType.@float, d => d.val = float.NaN, d => float.IsNaN(d.val), H.XmlRoundTrip, "XML must preserve float NaN.");
			H.AssertValRoundTrip<float_DEtype>(ItemChoiceType.@float, d => d.val = float.PositiveInfinity, d => float.IsPositiveInfinity(d.val), H.XmlRoundTrip, "XML must preserve float +INF.");
			H.AssertValRoundTrip<float_DEtype>(ItemChoiceType.@float, d => d.val = float.NegativeInfinity, d => float.IsNegativeInfinity(d.val), H.XmlRoundTrip, "XML must preserve float -INF.");
		}

		[TestMethod]
		public void Float_Specials_JsonRoundTrip_Preserved()
		{
			// JSON differs from XML: Newtonsoft (FloatFormatHandling.String) emits "NaN"/"Infinity"/"-Infinity".
			H.AssertValRoundTrip<float_DEtype>(ItemChoiceType.@float, d => d.val = float.NaN, d => float.IsNaN(d.val), H.JsonRoundTrip, "JSON must preserve float NaN.");
			H.AssertValRoundTrip<float_DEtype>(ItemChoiceType.@float, d => d.val = float.PositiveInfinity, d => float.IsPositiveInfinity(d.val), H.JsonRoundTrip, "JSON must preserve float +Infinity.");
			H.AssertValRoundTrip<float_DEtype>(ItemChoiceType.@float, d => d.val = float.NegativeInfinity, d => float.IsNegativeInfinity(d.val), H.JsonRoundTrip, "JSON must preserve float -Infinity.");
		}

		[TestMethod]
		public void Double_Specials_XmlRoundTrip_Preserved()
		{
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NaN, d => double.IsNaN(d.val), H.XmlRoundTrip, "XML must preserve double NaN.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.PositiveInfinity, d => double.IsPositiveInfinity(d.val), H.XmlRoundTrip, "XML must preserve double +INF.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NegativeInfinity, d => double.IsNegativeInfinity(d.val), H.XmlRoundTrip, "XML must preserve double -INF.");
		}

		[TestMethod]
		public void Double_Specials_JsonRoundTrip_Preserved()
		{
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NaN, d => double.IsNaN(d.val), H.JsonRoundTrip, "JSON must preserve double NaN.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.PositiveInfinity, d => double.IsPositiveInfinity(d.val), H.JsonRoundTrip, "JSON must preserve double +Infinity.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NegativeInfinity, d => double.IsNegativeInfinity(d.val), H.JsonRoundTrip, "JSON must preserve double -Infinity.");
		}

		#endregion

		#region Constraint facet round-trip (minInclusive / maxInclusive)

		[TestMethod]
		public void Int_Constraints_RoundTrip_PreservesMinMaxInclusive()
		{
			foreach (var rt in new Func<FormDesignType, FormDesignType>[] { H.XmlRoundTrip, H.JsonRoundTrip })
			{
				var node = H.DE<int_DEtype>(ItemChoiceType.@int, out var fd);
				node.minInclusive = int.MinValue;
				node.maxInclusive = int.MaxValue;
				var node2 = H.FindResponseDE<int_DEtype>(rt(fd));
				Assert.AreEqual(int.MinValue, node2.minInclusive, "minInclusive facet must round-trip exactly.");
				Assert.AreEqual(int.MaxValue, node2.maxInclusive, "maxInclusive facet must round-trip exactly.");
			}
		}

		[TestMethod]
		public void Decimal_Constraints_RoundTrip_PreservesMinMaxInclusive()
		{
			foreach (var rt in new Func<FormDesignType, FormDesignType>[] { H.XmlRoundTrip, H.JsonRoundTrip })
			{
				var node = H.DE<decimal_DEtype>(ItemChoiceType.@decimal, out var fd);
				node.minInclusive = -12345.678m;
				node.maxInclusive = 98765.4321m;
				var node2 = H.FindResponseDE<decimal_DEtype>(rt(fd));
				Assert.AreEqual(-12345.678m, node2.minInclusive, "decimal minInclusive facet must round-trip exactly.");
				Assert.AreEqual(98765.4321m, node2.maxInclusive, "decimal maxInclusive facet must round-trip exactly.");
			}
		}

		#endregion

		#region BSON / MsgPack parity subset (binary serializers)

		[TestMethod]
		public void Int_Val_BsonMsgPack_PreservesMinMax()
		{
			// Binary encoders are the likeliest to mis-handle integer width; assert exact survival.
			H.AssertValRoundTrip<int_DEtype>(ItemChoiceType.@int, d => d.val = int.MinValue, d => d.val == int.MinValue, H.BsonRoundTrip, "BSON must preserve int.MinValue.");
			H.AssertValRoundTrip<int_DEtype>(ItemChoiceType.@int, d => d.val = int.MaxValue, d => d.val == int.MaxValue, H.BsonRoundTrip, "BSON must preserve int.MaxValue.");
			H.AssertValRoundTrip<int_DEtype>(ItemChoiceType.@int, d => d.val = int.MinValue, d => d.val == int.MinValue, H.MsgPackRoundTrip, "MsgPack must preserve int.MinValue.");
			H.AssertValRoundTrip<int_DEtype>(ItemChoiceType.@int, d => d.val = int.MaxValue, d => d.val == int.MaxValue, H.MsgPackRoundTrip, "MsgPack must preserve int.MaxValue.");
		}

		[TestMethod]
		public void Long_Val_BsonMsgPack_PreservesMinMax()
		{
			H.AssertValRoundTrip<long_DEtype>(ItemChoiceType.@long, d => d.val = long.MinValue, d => d.val == long.MinValue, H.BsonRoundTrip, "BSON must preserve long.MinValue.");
			H.AssertValRoundTrip<long_DEtype>(ItemChoiceType.@long, d => d.val = long.MaxValue, d => d.val == long.MaxValue, H.BsonRoundTrip, "BSON must preserve long.MaxValue.");
			H.AssertValRoundTrip<long_DEtype>(ItemChoiceType.@long, d => d.val = long.MinValue, d => d.val == long.MinValue, H.MsgPackRoundTrip, "MsgPack must preserve long.MinValue.");
			H.AssertValRoundTrip<long_DEtype>(ItemChoiceType.@long, d => d.val = long.MaxValue, d => d.val == long.MaxValue, H.MsgPackRoundTrip, "MsgPack must preserve long.MaxValue.");
		}

		[TestMethod]
		public void UnsignedLong_Val_MsgPackRoundTrip_PreservesMax()
		{
			// MsgPack has a native uint64 type, so ulong.MaxValue survives intact.
			H.AssertValRoundTrip<unsignedLong_DEtype>(ItemChoiceType.unsignedLong, d => d.val = ulong.MaxValue, d => d.val == ulong.MaxValue, H.MsgPackRoundTrip, "MsgPack must preserve ulong.MaxValue.");
		}

		[TestMethod]
		public void UnsignedLong_Val_BsonRoundTrip_AtMaxValue_ThrowsDocumented()
		{
			// DIVERGENCE: BSON's integer types are signed only; a ulong above long.MaxValue cannot be
			// written ("Value is too large to fit in a signed 64 bit integer. BSON does not support
			// unsigned values."). XML/JSON/MsgPack preserve ulong.MaxValue; BSON cannot. Pinned here.
			AssertRoundTripThrows<unsignedLong_DEtype>(ItemChoiceType.unsignedLong, d => d.val = ulong.MaxValue, H.BsonRoundTrip,
				"BSON serialization of ulong.MaxValue is expected to throw under current behavior.");
		}

		[TestMethod]
		public void Decimal_Val_MsgPackRoundTrip_PreservesPrecision()
		{
			// MsgPack.Cli serializes decimal as its full string/native form, preserving all digits.
			const decimal v = 12345678901234567890.1234567m;
			H.AssertValRoundTrip<decimal_DEtype>(ItemChoiceType.@decimal, d => d.val = v, d => d.val == v, H.MsgPackRoundTrip, "MsgPack must preserve full decimal precision.");
		}

		[TestMethod]
		public void Decimal_Val_BsonRoundTrip_HighPrecision_LosesPrecisionDocumented()
		{
			// DIVERGENCE: BSON has no native decimal type; Newtonsoft encodes decimal as a 64-bit IEEE
			// double, so a value needing more than ~15-17 significant digits is rounded on read. XML,
			// JSON, and MsgPack preserve full precision; BSON does not. Pinned here.
			const decimal v = 12345678901234567890.1234567m;
			var node = H.DE<decimal_DEtype>(ItemChoiceType.@decimal, out var fd);
			node.val = v;
			var node2 = H.FindResponseDE<decimal_DEtype>(H.BsonRoundTrip(fd));
			Assert.AreNotEqual(v, node2.val, "BSON is expected to lose precision on a high-precision decimal (documented divergence).");
		}

		[TestMethod]
		public void Double_Specials_BsonMsgPack_Preserved()
		{
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NaN, d => double.IsNaN(d.val), H.BsonRoundTrip, "BSON must preserve double NaN.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.PositiveInfinity, d => double.IsPositiveInfinity(d.val), H.BsonRoundTrip, "BSON must preserve double +Infinity.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NaN, d => double.IsNaN(d.val), H.MsgPackRoundTrip, "MsgPack must preserve double NaN.");
			H.AssertValRoundTrip<double_DEtype>(ItemChoiceType.@double, d => d.val = double.NegativeInfinity, d => double.IsNegativeInfinity(d.val), H.MsgPackRoundTrip, "MsgPack must preserve double -Infinity.");
		}

		#endregion

		#region Malformed numeric token deserialization (characterization — current fail-fast contract)

		private static void AssertDeserializeThrows(Action deserialize, string because)
		{
			try
			{
				deserialize();
				Assert.Fail(because);
			}
			catch (AssertFailedException) { throw; }
			catch (Exception) { /* expected: current contract is fail-fast on malformed numeric tokens */ }
		}

		[TestMethod]
		public void Int_MalformedXmlAttribute_Deserialize_ThrowsDocumented()
		{
			// Pins today's behavior: a non-numeric val token in XML fails deserialization (no per-field
			// recovery). If issue #6 introduces graceful handling this test becomes a deliberate change.
			var node = H.DE<int_DEtype>(ItemChoiceType.@int, out var fd);
			node.val = 12345;
			var xml = H.GetXml(fd).Replace("val=\"12345\"", "val=\"abc\"");
			AssertDeserializeThrows(() => TopNodeSerializer<FormDesignType>.DeserializeFromXml(xml),
				"Malformed (non-numeric) xs:int val token must fail XML deserialization under the current fail-fast contract.");
		}

		[TestMethod]
		public void Int_OverflowJsonValue_Deserialize_ThrowsDocumented()
		{
			// Pins today's behavior: an out-of-range val token in JSON fails deserialization.
			var node = H.DE<int_DEtype>(ItemChoiceType.@int, out var fd);
			node.val = 12345;
			var json = H.GetJson(fd).Replace("12345", "99999999999999999999");
			AssertDeserializeThrows(() => TopNodeSerializer<FormDesignType>.DeserializeFromJson(json),
				"Overflowing xs:int val token must fail JSON deserialization under the current fail-fast contract.");
		}

		#endregion
	}
}
