# XSD vs. .NET Type Fidelity

> **Status:** Living document, consolidated from `AnyURI_XSD_vs_NET.md`,
> `NumericRange_XSD_vs_NET.md`, and the type-mapping portions of `DateTimeValidation_XSD_vs_NET.md`
> (originally in `SDC.Schema.Tests/Documentation`, now archived — see
> [../changes/](../changes/)). Covers every place the .NET backing type used by a
> generated `*_Stype`/`*_DEtype` property diverges from, or narrows, the XML Schema Definition
> (XSD) value space it is meant to represent. See [validation.md](validation.md) for the
> soft-reject contract these divergences are enforced through (GitHub issue #8).

## 1. `anyURI` vs. .NET `Uri`

XSD `anyURI` is an Internationalized Resource Identifier (IRI)-reference type — intentionally
very broad (XSD 1.0 references RFC 2396/2732; XSD 1.1 explicitly references RFC 3986 for URIs and
RFC 3987 for IRIs). It does **not** require a scheme, and accepts absolute URIs, relative paths,
fragment-only references, bare identifiers, Uniform Resource Names (URNs), Unicode IRI paths, and
even the empty string (a same-document reference per RFC 3986 §4.4).

**Historical bug (GitHub issue #12, resolved):** `IDataHelpers.AddDataTypesDE`'s `anyURI` case
used `Regex.Match(s, @"([#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF])+")` — the XML 1.0
`Char` production (which characters may appear in an XML document), not a URI grammar — and the
character-class range was also invalid .NET regex syntax, throwing `ArgumentException` on any
non-null string reaching that branch.

**Fix:** a helper mirroring .NET's own `XmlConvert.TryToUri` (the method the .NET XSD validator
uses for `xs:anyURI`):

```csharp
static bool IsValidAnyUri(string s)
{
    if (s.Length == 0) return true; // empty string = valid same-document reference (RFC 3986 §4.4)
    string trimmed = s.Trim(' ', '\t', '\r', '\n');
    if (trimmed.Length == 0) return false;
    if (trimmed.Contains("##")) return false; // only one '#' fragment delimiter is legal
    return Uri.TryCreate(trimmed, UriKind.RelativeOrAbsolute, out _);
}
```

`UriKind.RelativeOrAbsolute` is required — `UriKind.Absolute` would wrongly reject relative URIs,
fragment-only references, bare paths, and the empty string, all of which are valid `anyURI`
values. In .NET 10, raw internal spaces (discouraged but not forbidden by XSD 1.0 §3.2.17) and
Unicode IRI paths are both accepted by `Uri.TryCreate`.

Tests: `SDC.Schema.Tests/OM/AnyURIResponseTypeBoundaryTests.cs` (14 tests).

## 2. Numeric `ResponseType` ranges

Scope: every numeric datatype exposed for `ResponseType` fields — the `*_Stype` response value
(`val`) and the `*_DEtype` constraint facets (`minInclusive`, `maxInclusive`, `minExclusive`,
`maxExclusive`). Range policy: boundaries follow the XSD value space, except where the .NET
backing type cannot represent the full XSD range — in that case the .NET type is the binding
(more restrictive) constraint, documented below as a divergence. Divergence text is mirrored as
`<remarks>` XML comments in regeneration-safe partial-class companion files
(`SDC Customized Classes/Validation Divergences/*.Divergences.cs`) and in `SdcValidate.cs`.

| `ItemChoiceType` | `val` .NET type | XSD value space | .NET narrows XSD? |
|---|---|---|---|
| `@byte` | `sbyte` | −128…127 | No (exact) |
| `@short` | `short` | −32768…32767 | No (exact) |
| `@int` | `int` | int range | No (exact) |
| `@long` | `long` | long range | No (exact) |
| `@decimal` | `decimal` | arbitrary-precision decimal | **Yes** — XSD decimal is unbounded |
| `@float` | `float` | IEEE single + NaN/±INF/−0 | No (exact) |
| `@double` | `double` | IEEE double + NaN/±INF/−0 | No (exact) |
| `unsignedByte`/`unsignedShort`/`unsignedInt`/`unsignedLong` | `byte`/`ushort`/`uint`/`ulong` | 0…type max | No (exact) |
| `integer` and subtypes (`negativeInteger`, `nonNegativeInteger`, `positiveInteger`, `nonPositiveInteger`) | `decimal` | unbounded integer | **Yes** — capped at ≈±7.92e28 via `[Range]` |

Note: XSD `xs:byte` is **signed** (−128…127); the object model (OM) correctly backs it with
`sbyte`, not `byte`.

### Divergences (each pinned by a characterization test)

- **A. Integer-family `MaxDigitsAttribute(29)` counts the sign.** Negative integer-family values
  are limited to 28 significant digits (positive values get 29), because the custom
  `MaxDigitsAttribute` counts the leading `-` as one of the 29 characters. `decimal.MinValue` (30
  characters including sign) is therefore soft-rejected while `decimal.MaxValue` is accepted.
- **B. `long_DEtype` exclusive facets aren't enforceable at the type extreme**, because
  `[RangeAttribute(double,double)]` rounds `long.MaxValue` and `long.MaxValue − 1` to the same
  `double`.
- **C. Sign of zero (`−0`) is not preserved when assigned to a still-default node**, because the
  generated setter's `_val.Equals(value)` guard treats `+0.0`/`−0.0` as equal.
- **D. JSON cannot round-trip large whole-number decimals/integers.** Newtonsoft emits a bare
  integer literal for a whole-number `decimal`; on read, a literal beyond `ulong` range becomes a
  `BigInteger`, which the `decimal`-backed setter cannot convert — JSON throws where XML preserves
  the value.
- **E. BSON does not support unsigned values above `long.MaxValue`** (BSON's integer types are
  signed-only) — `unsignedLong_Stype.val = ulong.MaxValue` throws on BSON write; XML/JSON/MsgPack
  preserve it.
- **F. BSON loses precision on high-precision decimals** (no native BSON decimal type; Newtonsoft
  encodes `decimal` as a 64-bit IEEE `double`, rounding beyond ~15–17 significant digits).

Special floating-point values (`NaN`, `+INF`, `−INF`) round-trip correctly through get/set, XML,
and JSON (`FloatFormatHandling.String` emits `"NaN"`/`"Infinity"`/`"-Infinity"`).

Malformed numeric input parsed via `IDataHelpers.AddDataTypesDE` never throws; parse failures are
appended to a caller-supplied `IList<Exception> errors`. Public overloads omitting that argument
silently drop the error — tracked as GitHub issue #6 (deferred).

Tests: `SDC.Schema.Tests/OM/NumericResponseTypeBoundaryTests.cs`,
`SDC.Schema.Tests/Functional/Serialization/NumericResponseTypeRoundTripTests.cs`.

## 3. Date / date-part `ResponseType`s

Scope: the twelve XSD date/date-part types the OM exposes — `date`, `dateTime`, `time`,
`dateTimeStamp`, `gYear`, `gYearMonth`, `gMonth`, `gMonthDay`, `gDay`, `duration`,
`dayTimeDuration`, `yearMonthDuration`. This reuses the identical soft-reject contract (issue #8)
described in [validation.md](validation.md).

**Backing-type split:**
- **String-backed** (`gYear`, `gYearMonth`, `gMonth`, `gMonthDay`, `gDay`, `duration`,
  `dayTimeDuration`, `yearMonthDuration`): `val` **is** the XSD lexical string; validated by a
  registered `XsdDateLexicalAttribute` rule on the generated soft-reject setter.
- **DateTime-backed** (`date`, `dateTime`, `time`, `dateTimeStamp`): `val` is a `System.DateTime`
  (plus a separate `string timeZone` on `date`/`dateTime`/`time`), because `DateTime` cannot
  encode the lexical rules a raw string can (timezone presence, stray date/time components, the
  end-of-day `24:00:00`, a mandatory timezone). Lexical validation therefore happens at the
  **string boundary**, `SetLexicalValue(string)`, before `val`/`timeZone` are assigned.

### Divergences

- **A `DateTime` cannot carry a timezone**, so `timeZone` is kept as a separate, range-validated
  (`-14:00`..`+14:00`) string. Migrating `timeZone` to a `TimeSpan`/offset type is tracked as a
  GitHub issue (deferred; the validator already also accepts the .NET `TimeSpan` textual form for
  backward compatibility).
- **`dateTimeStamp` requires a mandatory timezone**, enforced at the string boundary rather than
  via a `[RegularExpression]` on the `DateTime`-typed property (which could never match
  `DateTime.ToString()` — this was a real bug, since fixed; a `dateTimeStamp` is normalized to UTC
  on assignment).
- **End-of-day `24:00:00`** is XSD-legal but unrepresentable in `DateTime`; `SetLexicalValue`
  accepts it and normalizes to `00:00:00` of the next day.
- **BSON DateTime read offset (root-caused and fixed):** all date/date-part types now round-trip
  the exact stored instant across XML, JSON, BSON, and MessagePack (MsgPack); the earlier bug
  shifted DateTime-backed BSON values by the host's UTC offset on read (the write side, using
  `DateTimeZoneHandling.Utc`, was already correct).

Tests: `SDC.Schema.Tests/OM/DateResponseTypeBoundaryTests.cs`,
`SDC.Schema.Tests/Functional/Serialization/DateResponseTypeRoundTripTests.cs`.
