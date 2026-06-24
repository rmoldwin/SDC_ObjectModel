// SDC-CUSTOM: do not overwrite. Hand-authored anyURI boundary tests for issue #12.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Tests.OM;
using System;

namespace SDC.Schema.Tests.OM
{
	/// <summary>
	/// Boundary tests for the <c>xs:anyURI</c> case in
	/// <see cref="IDataHelpers.AddDataTypesDE"/>  (issue #12).
	///
	/// Background:  XSD <c>anyURI</c> is an IRI-reference type (RFC 3986 / RFC 3987).  It is
	/// intentionally very broad: absolute URIs, relative paths, fragment-only references,
	/// bare strings, Unicode IRI characters, and the empty string are all valid values.
	/// A scheme is NOT required.  The type does not impose scheme-specific validation —
	/// only the generic URI-reference grammar applies.
	///
	/// The original code built a <c>Regex</c> from the XSD character-class literals
	/// <c>[#x1-#xD7FF]</c> / <c>[#xE000-#xFFFD]</c> / <c>[#x10000-#x10FFFF]</c> — the
	/// XML 1.0 Char production, NOT the anyURI grammar.  In .NET regex the literal "#x1-#"
	/// is an inverted range (0x31 &gt; 0x23) → <see cref="ArgumentException"/> the first
	/// time a non-null string reaches that branch.  Issue #12 replaced the pattern with a
	/// four-step helper that mirrors <c>XmlConvert.TryToUri</c> (the .NET-internal method
	/// used by the .NET XSD schema validator for xs:anyURI):
	///   1. Trim XML whitespace (whiteSpace=collapse facet).
	///   2. Reject all-whitespace input (empty after trim, non-empty before).
	///   3. Reject strings containing "##" (only one '#' fragment delimiter per RFC 3986).
	///   4. <c>Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out _)</c> as structural gate.
	///
	/// Test structure: the <c>anyURI_Stype.val</c> setter is a plain generated setter with
	/// no validation logic — validation only runs in <c>AddDataTypesDE</c>.  Tests therefore
	/// fall into two groups:
	///   • <b>Storage tests</b> — assign directly to <c>val</c> to verify the setter stores
	///     values without interference (no spurious errors or silent drops).
	///   • <b>Factory-path tests</b> — use <c>IDataHelpers.AddDataTypesDE</c> to exercise
	///     the actual validation gate (the code path that had the bug).
	///
	/// Note on raw spaces: XSD anyURI "highly discourages" but does NOT forbid raw internal
	/// spaces (XSD 1.0 §3.2.17). In .NET 10, <c>Uri.TryCreate(s, RelativeOrAbsolute)</c>
	/// accepts strings with raw spaces (relative-URI path interpretation), consistent with
	/// the XSD spec's permissive stance. "hello world" is therefore <b>accepted</b> here.
	/// .NET Framework 4.x rejected raw spaces; that was version-specific, not spec-mandated.
	/// See <c>IDataHelpers.cs</c> (anyURI case) and <c>Documentation/AnyURI_XSD_vs_NET.md</c>.
	/// </summary>
	[TestClass]
	public class AnyURIResponseTypeBoundaryTests
	{
		[TestInitialize]
		public void Init() => BaseType.ResetLastTopNode();

		/// <summary>
		/// Calls <see cref="IDataHelpers.AddDataTypesDE"/> with the given <paramref name="value"/>
		/// and returns the created <c>anyURI_DEtype</c> node.  Exercises the real validation gate.
		/// </summary>
		private static anyURI_DEtype CallFactory(object? value)
		{
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "Q.URI");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.anyURI);
			var rf = q.ResponseField_Item!;

			IDataHelpers.AddDataTypesDE(rf, ItemChoiceType.anyURI, value: value);
			return (anyURI_DEtype)rf.Response.DataTypeDE_Item!;
		}

		// ═══════════════════════════════════════════════════════════════════════════
		// Group 1 — Regression: the ArgumentException must not fire
		// ═══════════════════════════════════════════════════════════════════════════

		[TestMethod]
		public void AnyURI_NonNullString_DoesNotThrowArgumentException()
		{
			// Rationale: the original code threw ArgumentException (invalid .NET regex) the moment
			// any non-null string reached the anyURI branch in AddDataTypesDE. This regression
			// test confirms the fix is effective for a typical absolute URI string.
			Exception? caught = null;
			try { CallFactory("https://example.com/path"); }
			catch (Exception ex) { caught = ex; }

			Assert.IsNull(caught,
				"Passing a URI string through AddDataTypesDE must not throw — the ArgumentException " +
				"from the broken [#x1-#xD7FF] regex (issue #12) must be gone.");
		}

		// ═══════════════════════════════════════════════════════════════════════════
		// Group 2 — Factory-path: valid anyURI values → val stored verbatim
		// All use IDataHelpers.AddDataTypesDE (the validation gate).
		// ═══════════════════════════════════════════════════════════════════════════

		[TestMethod]
		public void AnyURI_Factory_AbsoluteUri_StoredExactly()
		{
			// Rationale: an absolute URI with scheme, host, path, query, and fragment is the
			// most canonical anyURI form; AddDataTypesDE must accept it and store it verbatim.
			var node = CallFactory("https://example.com/path?q=1#section");
			Assert.AreEqual("https://example.com/path?q=1#section", node.val,
				"A fully-qualified absolute URI must be passed through to val.");
		}

		[TestMethod]
		public void AnyURI_Factory_RelativePath_StoredExactly()
		{
			// Rationale: xs:anyURI is an IRI-reference, not an absolute URI. Relative paths such
			// as "../foo" are explicitly valid per RFC 3986 §4.2 (relative-ref). A scheme-less
			// string must NOT be rejected — anyURI imposes no scheme requirement.
			var node = CallFactory("../relative/path");
			Assert.AreEqual("../relative/path", node.val,
				"A relative path reference is valid xs:anyURI and must be stored.");
		}

		[TestMethod]
		public void AnyURI_Factory_FragmentOnly_StoredExactly()
		{
			// Rationale: "#section" is a same-document fragment reference — a valid relative-ref
			// per RFC 3986 §4.2 / §3.5. The XSD spec value space is URI-reference, which
			// includes fragment-only forms, so these must be accepted.
			var node = CallFactory("#section");
			Assert.AreEqual("#section", node.val,
				"A fragment-only reference '#section' is valid xs:anyURI and must be stored.");
		}

		[TestMethod]
		public void AnyURI_Factory_BareString_StoredExactly()
		{
			// Rationale: a bare identifier with no scheme or slashes is a path-noscheme
			// relative-ref per RFC 3986 §4.2. The XSD spec explicitly does not impose
			// scheme-specific validation, so "myIdentifier" is legal xs:anyURI.
			var node = CallFactory("myIdentifier");
			Assert.AreEqual("myIdentifier", node.val,
				"A bare identifier string is valid xs:anyURI (path-noscheme relative-ref) and must be stored.");
		}

		[TestMethod]
		public void AnyURI_Factory_UnicodeIriPath_StoredExactly()
		{
			// Rationale: XSD 1.1 updated the anyURI value space to RFC 3987 IRIs, which allow
			// Unicode characters (ucschar: U+00A0–U+D7FF, U+F900–U+FFEF, etc.) to appear
			// unencoded in IRI paths. .NET 5+ handles IRI parsing correctly. Rejecting Unicode
			// paths would be incorrect. URIs with Chinese/Japanese path segments are common in
			// multilingual applications that use SDC.
			var node = CallFactory("https://example.com/路径/値");
			Assert.AreEqual("https://example.com/路径/値", node.val,
				"A URI with Unicode IRI path characters is valid xs:anyURI and must be stored verbatim.");
		}

		[TestMethod]
		public void AnyURI_Factory_URN_StoredExactly()
		{
			// Rationale: URNs are absolute URIs with scheme "urn" — a common anyURI value in
			// XML vocabularies. SDC itself uses OID URNs (e.g. "urn:oid:2.16.840.1.113883.6.96")
			// in semantic annotations, so URN acceptance is important for practical SDC usage.
			var node = CallFactory("urn:oid:2.16.840.1.113883.6.96");
			Assert.AreEqual("urn:oid:2.16.840.1.113883.6.96", node.val,
				"A URN is a valid absolute URI and must be accepted as xs:anyURI.");
		}

		[TestMethod]
		public void AnyURI_Factory_EmptyString_StoredAsEmpty()
		{
			// Rationale: the empty string is a valid URI-reference (same-document reference per
			// RFC 3986 §4.4). The original code used IsNullOrWhiteSpace to guard the assignment,
			// which silently dropped a legitimate empty-string val. The fix uses (tmp != null)
			// so the empty string IS stored. Note: ShouldSerializeval() returns false for empty
			// strings (it uses !IsNullOrEmpty), so "" will not appear in XML output — that is
			// pre-existing generated-class behavior and is separate from storage correctness.
			var node = CallFactory("");
			Assert.AreEqual("", node.val,
				"An empty string is a valid xs:anyURI (same-document reference) and must be stored, not silently dropped.");
		}

		[TestMethod]
		public void AnyURI_Factory_NullValue_NodeCreatedWithNullVal()
		{
			// Rationale: passing null means "create the datatype node with no default value".
			// The factory must still create the anyURI_DEtype node (so the caller has a usable
			// stub), but val must remain null — not an empty string or any other sentinel.
			var node = CallFactory(null);
			Assert.IsNotNull(node, "AddDataTypesDE must create an anyURI_DEtype node even when value is null.");
			Assert.IsNull(node.val,
				"When no value is supplied (null), the node's val must remain null.");
		}

		// ═══════════════════════════════════════════════════════════════════════════
		// Group 3 — Factory-path: invalid anyURI values → val stays null
		// ═══════════════════════════════════════════════════════════════════════════

		[TestMethod]
		public void AnyURI_Factory_DoubleHash_Rejected()
		{
			// Rationale: RFC 3986 allows exactly one '#' fragment delimiter per URI-reference.
			// A second '#' is syntactically invalid and causes fragment-identifier ambiguity.
			// .NET's XmlConvert.TryToUri explicitly rejects "##" strings; our helper mirrors that.
			// The node must be created (non-null) but val must remain null.
			var node = CallFactory("https://example.com/path##badFragment");
			Assert.IsNotNull(node, "AddDataTypesDE must still create the node even when the value is rejected.");
			Assert.IsNull(node.val,
				"A string containing '##' is invalid per RFC 3986; val must remain null after rejection.");
		}

		[TestMethod]
		public void AnyURI_Factory_AllWhitespace_Rejected()
		{
			// Rationale: after XSD whiteSpace=collapse (trim), an all-whitespace string becomes
			// empty having started non-empty — .NET's XmlConvert.TryToUri (and our helper) treat
			// this as invalid. A truly empty string IS valid (see above); "   " is not.
			var node = CallFactory("   ");
			Assert.IsNotNull(node, "AddDataTypesDE must still create the node even when the value is rejected.");
			Assert.IsNull(node.val,
				"An all-whitespace string is not valid xs:anyURI; val must remain null after rejection.");
		}

		[TestMethod]
		public void AnyURI_Factory_RawInternalSpace_Accepted_PerXsdSpec()
		{
			// Rationale (documented behavior — not a defect): XSD anyURI "highly discourages"
			// but does NOT forbid raw internal spaces (XSD 1.0 §3.2.17 lexical space note).
			// In .NET 10, Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out _) accepts "hello world"
			// as a relative-URI path, which aligns with the XSD spec's permissive stance.
			// Note: .NET Framework 4.x and some earlier .NET Core versions rejected raw spaces in
			// Uri, so this behavior is .NET-version-specific. We document it rather than add a
			// special-case rejection that contradicts the XSD spec.
			var node = CallFactory("hello world");
			Assert.IsNotNull(node, "AddDataTypesDE must create the node.");
			Assert.AreEqual("hello world", node.val,
				"A string with a raw space is accepted by Uri.TryCreate(RelativeOrAbsolute) in .NET 10, " +
				"consistent with XSD anyURI's permissive lexical space (spaces are discouraged, not forbidden).");
		}

		// ═══════════════════════════════════════════════════════════════════════════
		// Group 4 — val setter storage (no validation — plain setter tests)
		// These verify the generated setter stores values without interference.
		// ═══════════════════════════════════════════════════════════════════════════

		[TestMethod]
		public void AnyURI_ValSetter_StoresArbitraryString()
		{
			// Rationale: anyURI_Stype.val is a generated setter with no validation logic —
			// validation only runs in AddDataTypesDE. Directly assigning to val must always
			// store the value (the setter must not silently drop or modify it).
			var node = NumericResponseTypeTestHelpers.DE<anyURI_DEtype>(ItemChoiceType.anyURI, out _);
			node.val = "https://example.com/direct";
			Assert.AreEqual("https://example.com/direct", node.val,
				"The val setter must store the assigned string without modification.");
		}

		[TestMethod]
		public void AnyURI_ValSetter_StoresEmptyString()
		{
			// Rationale: the generated setter must not guard against empty string (no
			// IsNullOrEmpty check). This confirms the setter itself does not interfere with
			// the empty-string case that AddDataTypesDE now correctly forwards.
			var node = NumericResponseTypeTestHelpers.DE<anyURI_DEtype>(ItemChoiceType.anyURI, out _);
			node.val = "";
			Assert.AreEqual("", node.val,
				"The val setter must store an empty string; it must not apply IsNullOrEmpty or similar guards.");
		}
	}
}
