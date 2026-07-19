using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on the fixed-width numeric family: byte_Stype (sbyte),
	/// short_Stype (short), int_Stype (int), long_Stype (long), unsignedByte_Stype (byte),
	/// unsignedShort_Stype (ushort), unsignedInt_Stype (uint), unsignedLong_Stype (ulong),
	/// float_Stype (float), double_Stype (double), and decimal_Stype (decimal).
	/// All of these delegate to System.Xml.XmlConvert's Toxxx/ToString methods (see NumericXmlHelper in
	/// PartialClasses.cs), which already implement the canonical XSD lexical representations for these
	/// primitive types -- including the special floating-point tokens "INF"/"-INF"/"NaN" -- so the tests
	/// here focus on confirming that wiring is correct and that min/max boundary values and illegal input
	/// are all handled without loss of precision or unhandled exceptions escaping the setter.
	/// </summary>
	[TestClass]
	public class FixedWidthNumericFamily_ValXmlStringTests
	{
		/// <summary>
		/// Constructs a detached (un-attached-to-any-parent) _Stype instance via its protected
		/// parameterless constructor, using reflection -- see the identical helper and rationale in
		/// DateTimeFamily_ValXmlStringTests.cs. None of these numeric _Stype classes have a unique
		/// real-world parent property available either, so the same approach is used here.
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
		public void ByteStype_ValXmlString_RoundTripsSByteBoundaries()
		{
			var sut = CreateDetached<byte_Stype>();

			// byte_Stype's val is CLR sbyte (XSD "byte" is signed 8-bit) -- exercise both extremes to
			// confirm no silent truncation/overflow occurs during the round trip.
			sut.ValXmlString = "-128";
			Assert.AreEqual((sbyte)-128, sut.val);
			Assert.AreEqual("-128", sut.ValXmlString);

			sut.ValXmlString = "127";
			Assert.AreEqual((sbyte)127, sut.val);
			Assert.AreEqual("127", sut.ValXmlString);
		}

		[TestMethod]
		public void ByteStype_ValXmlString_RejectsOutOfRangeValue()
		{
			var sut = CreateDetached<byte_Stype>();
			var originalVal = sut.val;

			// 128 overflows signed 8-bit range; must be handled via StoreError, not an unhandled
			// OverflowException, and must not corrupt the existing val.
			sut.ValXmlString = "128";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void ShortStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<short_Stype>();

			sut.ValXmlString = short.MinValue.ToString();
			Assert.AreEqual(short.MinValue, sut.val);
			Assert.AreEqual(short.MinValue.ToString(), sut.ValXmlString);

			sut.ValXmlString = short.MaxValue.ToString();
			Assert.AreEqual(short.MaxValue, sut.val);
			Assert.AreEqual(short.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void ShortStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<short_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = "not-a-number";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void IntStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<int_Stype>();

			sut.ValXmlString = int.MinValue.ToString();
			Assert.AreEqual(int.MinValue, sut.val);
			Assert.AreEqual(int.MinValue.ToString(), sut.ValXmlString);

			sut.ValXmlString = int.MaxValue.ToString();
			Assert.AreEqual(int.MaxValue, sut.val);
			Assert.AreEqual(int.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void LongStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<long_Stype>();

			sut.ValXmlString = long.MinValue.ToString();
			Assert.AreEqual(long.MinValue, sut.val);
			Assert.AreEqual(long.MinValue.ToString(), sut.ValXmlString);

			sut.ValXmlString = long.MaxValue.ToString();
			Assert.AreEqual(long.MaxValue, sut.val);
			Assert.AreEqual(long.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void UnsignedByteStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<unsignedByte_Stype>();

			sut.ValXmlString = "0";
			Assert.AreEqual((byte)0, sut.val);
			Assert.AreEqual("0", sut.ValXmlString);

			sut.ValXmlString = byte.MaxValue.ToString();
			Assert.AreEqual(byte.MaxValue, sut.val);
			Assert.AreEqual(byte.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void UnsignedByteStype_ValXmlString_RejectsNegativeValue()
		{
			var sut = CreateDetached<unsignedByte_Stype>();
			var originalVal = sut.val;

			// Negative input is illegal for an unsigned type; must not throw unhandled, must not corrupt val.
			sut.ValXmlString = "-1";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void UnsignedShortStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<unsignedShort_Stype>();

			sut.ValXmlString = ushort.MaxValue.ToString();
			Assert.AreEqual(ushort.MaxValue, sut.val);
			Assert.AreEqual(ushort.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void UnsignedIntStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<unsignedInt_Stype>();

			sut.ValXmlString = uint.MaxValue.ToString();
			Assert.AreEqual(uint.MaxValue, sut.val);
			Assert.AreEqual(uint.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void UnsignedLongStype_ValXmlString_RoundTripsBoundaries()
		{
			var sut = CreateDetached<unsignedLong_Stype>();

			sut.ValXmlString = ulong.MaxValue.ToString();
			Assert.AreEqual(ulong.MaxValue, sut.val);
			Assert.AreEqual(ulong.MaxValue.ToString(), sut.ValXmlString);
		}

		[TestMethod]
		public void FloatStype_ValXmlString_RoundTripsOrdinaryValue()
		{
			var sut = CreateDetached<float_Stype>();

			sut.ValXmlString = "3.5";

			Assert.AreEqual(3.5f, sut.val);
			Assert.AreEqual("3.5", sut.ValXmlString);
		}

		[TestMethod]
		public void FloatStype_ValXmlString_RoundTripsPositiveInfinityUsingXsdToken()
		{
			var sut = CreateDetached<float_Stype>();

			// XSD's lexical space for float/double uses the literal tokens "INF"/"-INF"/"NaN" for these
			// special values -- not .NET's default "Infinity"/"-Infinity"/"NaN" ToString() output. This
			// must round-trip through the XSD-compliant token, which is why NumericXmlHelper delegates to
			// XmlConvert rather than float.Parse/ToString directly.
			sut.ValXmlString = "INF";

			Assert.AreEqual(float.PositiveInfinity, sut.val);
			Assert.AreEqual("INF", sut.ValXmlString);
		}

		[TestMethod]
		public void FloatStype_ValXmlString_RoundTripsNegativeInfinityUsingXsdToken()
		{
			var sut = CreateDetached<float_Stype>();

			sut.ValXmlString = "-INF";

			Assert.AreEqual(float.NegativeInfinity, sut.val);
			Assert.AreEqual("-INF", sut.ValXmlString);
		}

		[TestMethod]
		public void FloatStype_ValXmlString_RoundTripsNaN()
		{
			var sut = CreateDetached<float_Stype>();

			sut.ValXmlString = "NaN";

			Assert.IsTrue(float.IsNaN(sut.val));
			Assert.AreEqual("NaN", sut.ValXmlString);
		}

		[TestMethod]
		public void FloatStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<float_Stype>();
			var originalVal = sut.val;

			// Not a legal float lexical form (XSD or otherwise) -- must be handled via StoreError, not an
			// unhandled FormatException, and must not corrupt the existing val.
			sut.ValXmlString = "not-a-float";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void DoubleStype_ValXmlString_RoundTripsOrdinaryValue()
		{
			var sut = CreateDetached<double_Stype>();

			sut.ValXmlString = "123.456";

			Assert.AreEqual(123.456, sut.val);
			Assert.AreEqual("123.456", sut.ValXmlString);
		}

		[TestMethod]
		public void DoubleStype_ValXmlString_RoundTripsNegativeInfinityUsingXsdToken()
		{
			var sut = CreateDetached<double_Stype>();

			sut.ValXmlString = "-INF";

			Assert.AreEqual(double.NegativeInfinity, sut.val);
			Assert.AreEqual("-INF", sut.ValXmlString);
		}

		[TestMethod]
		public void DoubleStype_ValXmlString_RoundTripsNaN()
		{
			var sut = CreateDetached<double_Stype>();

			sut.ValXmlString = "NaN";

			Assert.IsTrue(double.IsNaN(sut.val));
			Assert.AreEqual("NaN", sut.ValXmlString);
		}

		[TestMethod]
		public void DecimalStype_ValXmlString_RoundTripsOrdinaryValue()
		{
			var sut = CreateDetached<decimal_Stype>();

			sut.ValXmlString = "1234.5678";

			// Round trip must preserve precision exactly -- decimal has no XSD-special tokens (INF/NaN
			// don't apply), so a straightforward value comparison is sufficient here.
			Assert.AreEqual(1234.5678m, sut.val);
			Assert.AreEqual("1234.5678", sut.ValXmlString);
		}

		[TestMethod]
		public void DecimalStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<decimal_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = "not-a-decimal";

			Assert.AreEqual(originalVal, sut.val);
		}
	}
}
