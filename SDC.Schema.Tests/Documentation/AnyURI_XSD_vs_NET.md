# XSD `anyURI` vs. .NET `Uri` — Divergence and Implementation Notes

> **Issue:** #12 — `IDataHelpers.AddDataTypesDE` anyURI case used an invalid .NET regex → `ArgumentException`  
> **Status:** RESOLVED (see commit on `Features/NET10/Net10Main`)  
> **File fixed:** `SDC.Schema/Interfaces/IDataHelpers.cs`, `anyURI` case

---

## 1. What XSD `anyURI` Actually Is

XSD `anyURI` is an **IRI-reference type** — intentionally very broad.

| Spec version | Normative reference |
|---|---|
| XSD 1.0 §3.2.17 | RFC 2396 (URI) as amended by RFC 2732 |
| XSD 1.1 §3.3.17 | RFC 3986 (URI) + RFC 3987 (IRI) — explicitly updated |

The value space is every **URI-reference** (XSD 1.0) / **IRI-reference** (XSD 1.1).  
A URI-reference includes:

| Form | Example | Valid? |
|---|---|---|
| Absolute URI | `https://example.com/path?q=1#frag` | ✅ |
| Relative path | `../foo`, `./bar` | ✅ |
| Fragment-only | `#section` | ✅ (same-document reference, RFC 3986 §4.4) |
| Bare identifier | `myIdentifier` | ✅ (path-noscheme relative-ref, RFC 3986 §4.2) |
| URN | `urn:oid:2.16.840.1.113883.6.96` | ✅ |
| Unicode IRI path | `https://example.com/路径/値` | ✅ (ucschar per RFC 3987 §2.2) |
| Empty string | `""` | ✅ (same-document reference, RFC 3986 §4.4) |
| String with internal space | `"hello world"` | ✅ (discouraged, not forbidden, per XSD 1.0 §3.2.17) |

**The XSD spec explicitly does NOT require a scheme** and does **not** impose scheme-specific validation.  
Any value that satisfies the generic RFC 3986/3987 grammar is a legal `anyURI`, regardless of scheme.

RFC 3987 §1.2 confirms: *"XML schema has an explicit type 'anyURI' that includes IRIs and IRI references."*

---

## 2. The Original Bug (Issue #12)

The `anyURI` case in `IDataHelpers.AddDataTypesDE` used:

```csharp
Regex.Match(s, @"([#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF])+")
```

**Two problems:**

1. **Wrong grammar.** This is the **XML 1.0 `Char` production** (W3C XML 1.0 §2.2) — the set of
   characters valid in an XML document. It is *not* the `anyURI` grammar. Using it as a URI validator
   was a category error: `Char` defines what can appear in XML attribute values; `anyURI` additionally
   constrains those characters to form a valid URI-reference structure.

2. **Invalid .NET regex.** In .NET regex, `[#x1-#xD7FF]` is a character-class range from `0x23` (`#`)
   to `0x23` via `x`, `1`, `-`, `#` — but `1` (0x31) > `#` (0x23) makes it an inverted range →
   `ArgumentException` is thrown by `new Regex(...)` / `Regex.Match(...)` the instant a non-null
   string reaches this branch. The bug was latent because no test exercised the `anyURI` branch.

---

## 3. The Fix

Replace the broken regex with a four-step helper that mirrors `.NET`'s own internal
`XmlConvert.TryToUri` — the method the .NET XSD schema validator calls for `xs:anyURI`:

```csharp
static bool IsValidAnyUri(string s)
{
    // A truly empty string is a valid same-document URI-reference (RFC 3986 §4.4).
    if (s.Length == 0) return true;

    // Apply XSD whiteSpace=collapse: trim XML whitespace chars.
    string trimmed = s.Trim(' ', '\t', '\r', '\n');

    // All-whitespace input (non-empty before trim, empty after) is not a valid anyURI.
    if (trimmed.Length == 0) return false;

    // "##" is syntactically invalid per RFC 3986 (only one fragment delimiter '#' is allowed).
    if (trimmed.Contains("##")) return false;

    return Uri.TryCreate(trimmed, UriKind.RelativeOrAbsolute, out _);
}
```

**Why `UriKind.RelativeOrAbsolute`?**  
`UriKind.Absolute` would wrongly reject relative URIs, fragment-only refs, bare paths, and the empty
string — all of which are valid `anyURI` values. `RelativeOrAbsolute` is the correct gate.

**The `##` check:** `XmlConvert.TryToUri` (dotnet/runtime `XmlConvert.cs`) explicitly rejects strings
containing `##` because only one `#` fragment delimiter is syntactically valid per RFC 3986.
`Uri.TryCreate` may accept `##` on some .NET versions; the explicit check makes the behavior consistent.

---

## 4. .NET 10 Behavior Notes

### Empty string
`Uri.TryCreate("", UriKind.RelativeOrAbsolute, out _)` returns `true`. The empty string represents a
"same-document" reference (RFC 3986 §4.4). The old code guarded `dt.val` with `IsNullOrWhiteSpace`,
which silently dropped valid empty-string values. The fix uses `(tmp != null)` instead.

Note: `anyURI_Stype.ShouldSerializeval()` uses `!IsNullOrEmpty(val)`, so an empty string will
not appear in serialized XML output — but it is stored correctly in the in-memory OM. This is
pre-existing generated-class behavior and is separate from storage correctness.

### Raw internal spaces (`"hello world"`)
XSD anyURI "highly discourages" but does NOT forbid raw internal spaces (XSD 1.0 §3.2.17 note).
In .NET 10, `Uri.TryCreate("hello world", UriKind.RelativeOrAbsolute, out _)` returns `true` — the
string is parsed as a relative-URI path, consistent with the XSD spec's permissive stance.
Earlier .NET (Framework 4.x) rejected raw spaces in `Uri`; that was version-specific behavior,
not mandated by the XSD spec. In .NET 10 these values are therefore **accepted**.

### Unicode IRI paths
.NET 5+ enables IRI and IDN processing globally. Unicode characters in IRI paths (`路径/値`) are
handled correctly by `Uri.TryCreate` in .NET 10.

---

## 5. What `anyURI` Is NOT

The original XML 1.0 `Char` production that the broken code used:

```
Char ::= #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
```

This defines the outer envelope of characters valid in any XML document.
It is **not** a structural URI grammar.

Contrasting `Char` vs `anyURI`:

| Check | Char | anyURI |
|---|---|---|
| Control chars `#x1`–`#x1F` (minus TAB/LF/CR) | ✅ | ❌ (not valid URI chars) |
| Raw space `#x20` | ✅ | ⚠️ discouraged but not forbidden |
| `{`, `}`, `|`, `^`, `` ` `` | ✅ | ❌ (not in RFC 3986 unreserved/reserved sets) |
| Valid URI structure (scheme/hier-part grammar) | ❌ (no structural check) | ✅ |
| Fragment syntax (`##` invalid) | ❌ (no structural check) | ✅ |

---

## 6. Tests

`SDC.Schema.Tests/OM/AnyURIResponseTypeBoundaryTests.cs` — 14 tests:

| Group | Tests |
|---|---|
| Regression: ArgumentException must not fire | `AnyURI_NonNullString_DoesNotThrowArgumentException` |
| Factory-path valid values stored | Absolute URI, relative path, fragment-only, bare string, Unicode IRI, URN, empty string, null |
| Factory-path invalid values rejected | `##`, all-whitespace |
| Factory-path space behavior documented | `AnyURI_Factory_RawInternalSpace_Accepted_PerXsdSpec` |
| val setter storage (no validation) | stores arbitrary string, stores empty string |

---

## 7. References

| Claim | Source |
|---|---|
| XSD 1.0 anyURI definition | https://www.w3.org/TR/xmlschema-2/#anyURI |
| XSD 1.1 updates refs to RFC 3986/3987 | https://www.w3.org/TR/xmlschema11-2/ Appendix I.4 |
| RFC 3986 URI grammar (URI-reference = URI / relative-ref) | https://datatracker.ietf.org/doc/html/rfc3986#appendix-A |
| RFC 3987 anyURI includes IRIs (§1.2) | https://datatracker.ietf.org/doc/html/rfc3987#section-1.2 |
| RFC 3987 ucschar ranges (IRI Unicode chars) | https://datatracker.ietf.org/doc/html/rfc3987#section-2.2 |
| XML 1.0 Char production (the wrong pattern) | https://www.w3.org/TR/xml/#charsets |
| .NET `XmlConvert.TryToUri` (mirrors our helper) | dotnet/runtime `XmlConvert.cs` |
| .NET `Datatype_anyURI` calls `XmlConvert.TryToUri` | dotnet/runtime `DataTypeImplementation.cs` |
