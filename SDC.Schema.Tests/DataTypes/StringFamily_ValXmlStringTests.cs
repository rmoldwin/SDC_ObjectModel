using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Reflection;

namespace SDC.Schema.Tests.DataTypes
{
	/// <summary>
	/// Round-trip tests for IVal.ValXmlString on string_Stype. val's CLR type is already string, and the
	/// XSD "string" type imposes no lexical-form restriction, so ValXmlString is a direct pass-through
	/// with no parsing/formatting logic -- any string content is legal, including whitespace-only and
	/// content requiring XML escaping (the escaping itself is XmlSerializer's job at write-time, not
	/// ValXmlString's; these tests only confirm ValXmlString preserves the literal string value exactly).
	/// Note: string_Stype was found to not implement IVal in its generated declaration (unlike every other
	/// _Stype class in this project); `: IVal` was added to its customization-layer partial class
	/// declaration here for consistency with the rest of the family.
	/// </summary>
	[TestClass]
	public class StringFamily_ValXmlStringTests
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
		public void StringStype_ValXmlString_RoundTripsOrdinaryText()
		{
			var sut = CreateDetached<string_Stype>();

			sut.ValXmlString = "Hello, world!";

			Assert.AreEqual("Hello, world!", sut.val);
			Assert.AreEqual("Hello, world!", sut.ValXmlString);
		}

		[TestMethod]
		public void StringStype_ValXmlString_RoundTripsEmptyString()
		{
			var sut = CreateDetached<string_Stype>();

			// An empty string is a legal XSD string value, distinct from null.
			sut.ValXmlString = "";

			Assert.AreEqual("", sut.val);
			Assert.AreEqual("", sut.ValXmlString);
		}

		[TestMethod]
		public void StringStype_ValXmlString_RoundTripsContentRequiringXmlEscaping()
		{
			var sut = CreateDetached<string_Stype>();

			// Characters like '<', '&', and '"' must survive round-trip through ValXmlString untouched --
			// XML escaping/unescaping is XmlSerializer's responsibility at actual document write/read time,
			// not ValXmlString's, so the literal string (unescaped form) is what val should hold.
			const string raw = "<tag> & \"quoted\" 'value'";
			sut.ValXmlString = raw;

			Assert.AreEqual(raw, sut.val);
			Assert.AreEqual(raw, sut.ValXmlString);
		}

		[TestMethod]
		public void StringStype_ValXmlString_RoundTripsWhitespaceOnlyContent()
		{
			var sut = CreateDetached<string_Stype>();

			// Unlike XSD's normalizedString/token restrictions, plain "string" preserves whitespace verbatim.
			sut.ValXmlString = "  \t\n  ";

			Assert.AreEqual("  \t\n  ", sut.val);
			Assert.AreEqual("  \t\n  ", sut.ValXmlString);
		}

		[TestMethod]
		public void StringStype_ValXmlString_RejectsNullValue()
		{
			var sut = CreateDetached<string_Stype>();
			var originalVal = sut.val;

			// Consistent with the rest of the IVal family, a null assignment is reported via StoreError
			// rather than silently clearing val or throwing.
			sut.ValXmlString = null!;

			Assert.AreEqual(originalVal, sut.val);
		}
	}
}
