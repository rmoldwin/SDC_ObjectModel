using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on base64Binary_Stype and boolean_Stype. Both classes'
	/// ValXmlString implementations were found, during a final coverage sweep of this feature, to already
	/// exist (pre-dating this datatype-by-datatype implementation effort) but to contain functional bugs:
	/// base64Binary_Stype's getter called `val.ToString()` on a byte[] (producing the literal string
	/// "System.Byte[]" instead of a base64 encoding) and its setter passed a permanently zero-length
	/// Span&lt;byte&gt; to Convert.TryFromBase64String (guaranteeing failure for any non-empty input);
	/// boolean_Stype's getter called `val.ToString()` on a bool, producing .NET's capitalized "True"/"False"
	/// instead of XSD boolean's required lowercase "true"/"false" lexical form. Both were fixed to use
	/// Convert.ToBase64String/FromBase64String and XmlConvert.ToString/ToBoolean respectively, which
	/// correctly implement their XSD lexical forms.
	/// </summary>
	[TestClass]
	public class Base64BinaryAndBooleanFamily_ValXmlStringTests
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
		public void Base64BinaryStype_ValXmlString_RoundTripsNonEmptyByteArray()
		{
			var sut = CreateDetached<base64Binary_Stype>();

			// "SGVsbG8=" is the base64 encoding of the ASCII bytes for "Hello"; this exercises the
			// previously-broken setter path (which could never successfully decode any non-empty input).
			sut.ValXmlString = "SGVsbG8=";

			CollectionAssert.AreEqual(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, sut.val);
			// This exercises the previously-broken getter path (which returned "System.Byte[]").
			Assert.AreEqual("SGVsbG8=", sut.ValXmlString);
		}

		[TestMethod]
		public void Base64BinaryStype_ValXmlString_RoundTripsEmptyByteArray()
		{
			var sut = CreateDetached<base64Binary_Stype>();

			sut.ValXmlString = "";

			CollectionAssert.AreEqual(Array.Empty<byte>(), sut.val);
			Assert.AreEqual("", sut.ValXmlString);
		}

		[TestMethod]
		public void Base64BinaryStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<base64Binary_Stype>();
			var originalVal = sut.val;

			// "not valid base64!!" contains characters and padding that are illegal in base64.
			sut.ValXmlString = "not valid base64!!";

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void Base64BinaryStype_ValXmlString_RejectsNullValue()
		{
			var sut = CreateDetached<base64Binary_Stype>();
			var originalVal = sut.val;

			sut.ValXmlString = null!;

			Assert.AreEqual(originalVal, sut.val);
		}

		[TestMethod]
		public void BooleanStype_ValXmlString_RoundTripsTrueAsLowercaseXsdLexicalForm()
		{
			var sut = CreateDetached<boolean_Stype>();

			sut.ValXmlString = "true";

			Assert.IsTrue(sut.val);
			// This is the key regression check: the getter must return XSD's lowercase "true", not .NET's
			// capitalized "True" (the pre-fix behavior, which was non-conformant and non-round-tripping).
			Assert.AreEqual("true", sut.ValXmlString);
		}

		[TestMethod]
		public void BooleanStype_ValXmlString_RoundTripsFalseAsLowercaseXsdLexicalForm()
		{
			var sut = CreateDetached<boolean_Stype>();

			sut.ValXmlString = "false";

			Assert.IsFalse(sut.val);
			Assert.AreEqual("false", sut.ValXmlString);
		}

		[TestMethod]
		public void BooleanStype_ValXmlString_RoundTripsNumericXsdLexicalForm()
		{
			var sut = CreateDetached<boolean_Stype>();

			// XSD boolean also permits "1"/"0" as numeric lexical forms, both of which XmlConvert.ToBoolean
			// accepts, but the getter must always normalize back to the canonical "true"/"false" spelling.
			sut.ValXmlString = "1";

			Assert.IsTrue(sut.val);
			Assert.AreEqual("true", sut.ValXmlString);
		}

		[TestMethod]
		public void BooleanStype_ValXmlString_RejectsIllegalFormat()
		{
			var sut = CreateDetached<boolean_Stype>();
			var originalVal = sut.val;

			// XSD boolean's lexical space is exactly {"true","false","1","0"} (case-sensitive lowercase);
			// "True" (capitalized, .NET's own ToString() spelling) is not a legal XSD boolean lexical form.
			sut.ValXmlString = "True";

			Assert.AreEqual(originalVal, sut.val);
		}
	}
}
