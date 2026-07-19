using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on hexBinary_Stype. val's CLR type is byte[] (not string),
	/// so ValXmlString converts to/from the XSD "hexBinary" lexical form (pairs of hex digits) using
	/// Convert.ToHexString/FromHexString, which natively implement that encoding: ToHexString always
	/// produces uppercase digits, and FromHexString accepts either case on parse, matching hexBinary's
	/// case-insensitive spec.
	/// </summary>
	[TestClass]
	public class HexBinaryFamily_ValXmlStringTests
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
		public void HexBinaryStype_ValXmlString_RoundTripsUppercaseInput()
		{
			var sut = CreateDetached<hexBinary_Stype>();

			sut.ValXmlString = "0FB7";

			CollectionAssert.AreEqual(new byte[] { 0x0F, 0xB7 }, sut.val);
			// ToHexString always normalizes output to uppercase.
			Assert.AreEqual("0FB7", sut.ValXmlString);
		}

		[TestMethod]
		public void HexBinaryStype_ValXmlString_AcceptsLowercaseInputButNormalizesOutputToUppercase()
		{
			var sut = CreateDetached<hexBinary_Stype>();

			// hexBinary's lexical form is case-insensitive on input; lowercase "0fb7" must parse
			// identically to "0FB7" above, even though ValXmlString's getter always renders uppercase.
			sut.ValXmlString = "0fb7";

			CollectionAssert.AreEqual(new byte[] { 0x0F, 0xB7 }, sut.val);
			Assert.AreEqual("0FB7", sut.ValXmlString);
		}

		[TestMethod]
		public void HexBinaryStype_ValXmlString_RoundTripsEmptyByteArray()
		{
			var sut = CreateDetached<hexBinary_Stype>();

			// An empty hex string is legal and represents a zero-length byte array.
			sut.ValXmlString = "";

			CollectionAssert.AreEqual(Array.Empty<byte>(), sut.val);
			Assert.AreEqual("", sut.ValXmlString);
		}

		[TestMethod]
		public void HexBinaryStype_ValXmlString_RejectsOddLengthString()
		{
			var sut = CreateDetached<hexBinary_Stype>();
			var originalVal = sut.val;

			// hexBinary digits come in pairs; an odd number of hex digits is illegal.
			sut.ValXmlString = "0FB";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void HexBinaryStype_ValXmlString_RejectsNonHexCharacters()
		{
			var sut = CreateDetached<hexBinary_Stype>();
			var originalVal = sut.val;

			// 'G' and 'Z' are not valid hex digits.
			sut.ValXmlString = "GZ";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void HexBinaryStype_ValXmlString_RejectsNullValue()
		{
			var sut = CreateDetached<hexBinary_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = null!;

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void HexBinaryStype_ValXmlString_ReturnsEmptyStringWhenValIsUnset()
		{
			var sut = CreateDetached<hexBinary_Stype>();

			// val defaults to null (never assigned); ValXmlString's getter must not throw in that case.
			Assert.AreEqual("", sut.ValXmlString);
		}
	}
}
