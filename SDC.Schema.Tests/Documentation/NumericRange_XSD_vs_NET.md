# Numeric ResponseType Ranges: XSD vs .NET, and Serializer Divergences

**Scope:** Every numeric datatype the SDC Object Model exposes for ResponseType
fields — the `*_Stype` response value (`val`) and the `*_DEtype` constraint facets
(`minInclusive`, `maxInclusive`, `minExclusive`, `maxExclusive`).

**Companion tests**

| Layer | File |
|---|---|
| Object-model (unit) | `SDC.Schema.Tests/OM/NumericResponseTypeBoundaryTests.cs` |
| Serialization (functional round-trip) | `SDC.Schema.Tests/Functional/Serialization/NumericResponseTypeRoundTripTests.cs` |
| Shared construction/round-trip helper | `SDC.Schema.Tests/OM/NumericResponseTypeTestHelpers.cs` |

**Range policy:** Boundaries are the **XML Schema** value space, *except* where the
implemented .NET backing type cannot represent the full XSD range. In that case the
.NET type is the binding (more-restrictive) constraint and is documented as a
divergence below. The same divergence text is mirrored, as `<remarks>` XML comments,
in regeneration-safe partial-class companion files
(`SDC.Schema/SDC.Schema/SDC Customized Classes/Validation Divergences/*.Divergences.cs`)
and in the hand-written validator (`SDC.Schema/Utility Classes/SdcValidate.cs`).

---

## 0. Validation contract: soft-reject (issue #8)

When a numeric (or any) setter — or any deserialization path (XML/JSON/BSON/MsgPack) — receives a
value that fails its DataAnnotations facets (`[Range]`, `[MaxDigits]`, `[FractionDigits]`, …), the OM
**soft-rejects** it:

1. **Never stores the invalid value.** The typed backing field keeps its prior value (or stays unset).
   The OM is never left holding data that violates its own schema.
2. **Does not throw.** Assignment and deserialization continue (soft reject), so an invalid source
   document surfaces *all* of its problems instead of aborting on the first one.
3. **Always records the offending value** on the node, out-of-band, in
   `BaseType.RejectedValues[propertyName]` (an `SdcRejectedValue` carrying the attempted value, a
   message that includes that value, the timestamp, and the `ValidationResult`s). Nothing is silently
   dropped — a UI can read `RejectedValues` to show the user what to correct.
4. **Surfaces events/reports only when validation is not suppressed.** `SdcValidationEvents.ValidationOccurred`
   fires and `ValidationCollector` is populated on programmatic mutation and on the
   `Deserialize*Validating` overloads; the plain (non-validating) `Deserialize` stays quiet but **still**
   rejects and records (recording is unconditional, event/report noise is gated by `SuppressValidation`).

The mechanism is `SdcUtil.ValidateAndRaise`, which now returns `bool` (true = valid → the generated
setter assigns; false = invalid → the setter skips the assignment). A subsequent **valid** set for the
same property clears its recorded rejected value.

**Deliberate exception — `BaseType.sGuid`:** the structural node-identity property is a hard-reject
invariant (its value is decode-validated before assignment and node dictionaries are re-registered
against it), so it is *not* soft-rejected.

---


| `ItemChoiceType` | `*_Stype` | `val` .NET type | XSD value space | Effective min | Effective max | .NET narrows XSD? |
|---|---|---|---|---|---|---|
| `@byte` | `byte_Stype` | `sbyte` | −128 … 127 | −128 | 127 | No (exact) |
| `@short` | `short_Stype` | `short` | −32768 … 32767 | −32768 | 32767 | No (exact) |
| `@int` | `int_Stype` | `int` | −2147483648 … 2147483647 | int.MinValue | int.MaxValue | No (exact) |
| `@long` | `long_Stype` | `long` | −9223372036854775808 … 9223372036854775807 | long.MinValue | long.MaxValue | No (exact) |
| `@decimal` | `decimal_Stype` | `decimal` | arbitrary-precision decimal | decimal.MinValue ≈ −7.92e28 | decimal.MaxValue ≈ 7.92e28 | **Yes** (XSD decimal is unbounded precision/range) |
| `@float` | `float_Stype` | `float` | IEEE single + NaN/±INF/−0 | float.MinValue ≈ −3.4e38 | float.MaxValue ≈ 3.4e38 | No (exact) |
| `@double` | `double_Stype` | `double` | IEEE double + NaN/±INF/−0 | double.MinValue ≈ −1.8e308 | double.MaxValue ≈ 1.8e308 | No (exact) |
| `unsignedByte` | `unsignedByte_Stype` | `byte` | 0 … 255 | 0 | 255 | No (exact) |
| `unsignedShort` | `unsignedShort_Stype` | `ushort` | 0 … 65535 | 0 | 65535 | No (exact) |
| `unsignedInt` | `unsignedInt_Stype` | `uint` | 0 … 4294967295 | 0 | 4294967295 | No (exact) |
| `unsignedLong` | `unsignedLong_Stype` | `ulong` | 0 … 18446744073709551615 | 0 | ulong.MaxValue | No (exact) |
| `integer` | `integer_Stype` | `decimal` | unbounded integer | ≈ −7.92e28 (29 digits) | ≈ 7.92e28 (29 digits) | **Yes** (XSD integer is unbounded) |
| `negativeInteger` | `negativeInteger_Stype` | `decimal` | … −1 | ≈ −7.92e27 (28 digits) | −1 | **Yes** |
| `nonNegativeInteger` | `nonNegativeInteger_Stype` | `decimal` | 0 … | 0 | ≈ 7.92e28 (29 digits) | **Yes** |
| `positiveInteger` | `positiveInteger_Stype` | `decimal` | 1 … | 1 | ≈ 7.92e28 (29 digits) | **Yes** |
| `nonPositiveInteger` | `nonPositiveInteger_Stype` | `decimal` | … 0 | ≈ −7.92e27 (28 digits) | 0 | **Yes** |

Notes:
- XSD `xs:byte` is signed (−128…127); the OM correctly backs it with `sbyte`, **not** `byte`.
- XSD `xs:integer` and its subtypes are theoretically unbounded. The OM caps them at the
  `decimal` range via `[Range(±7.9228e28)]`. This .NET narrowing is intentional and is the
  binding constraint (see Divergence A).

---

## 2. Divergences

Each divergence is exercised by a dedicated **characterization test** that pins the
*current* behavior, so any future change is deliberate and test-visible.

### A. Integer-family `MaxDigitsAttribute(29)` counts the sign for negatives
The custom `MaxDigitsAttribute(29)` (in
`SDC.Schema/SDC.Schema/SDC Customized Classes/MaxDigitsAttribute.cs`) validates
`value.ToString().Length <= 29`. For negative values the leading `-` consumes one of the
29 characters, so **negative** integer-family values are limited to **28 significant
digits** while **positive** values get **29**. Consequently `decimal.MaxValue`
(29 digits) is accepted, but `decimal.MinValue` (30 chars including the sign) is
**soft-rejected** (issue #8): the setter keeps the prior/unset value, does **not** throw, and
records the offending value on the node (`BaseType.RejectedValues`). Test constants therefore
use `7.9e28` (positive, 29 digits) and
`-7.9e27` (negative, 28 digits).
*Tests:* `NumericResponseTypeBoundaryTests.Integer_Val_DecimalExtremes_RangeEdgeBehavior`.

### B. `long_DEtype` exclusive facets cannot be enforced at the type extreme
`[RangeAttribute(-9223372036854775807, 9223372036854775807)]` on
`long_DEtype.minExclusive` / `maxExclusive` binds the `RangeAttribute(double, double)`
overload. `long.MaxValue` and `long.MaxValue − 1` both round to the same `double`, so the
facet boundary is not enforceable at the extreme (assigning `long.MaxValue` does **not**
throw). Inner long values validate normally.
*Tests:* `NumericResponseTypeBoundaryTests.Long_*Exclusive_AtBoundary_NotEnforcedDueToDoublePrecision`.

### C. Sign of zero (`−0`) is not preserved on assignment from default
The generated `val` setter guards with `if (_val.Equals(value) != true)`. A fresh node's
`_val` is `+0.0`, and `(+0.0).Equals(−0.0)` is `true`, so assigning `−0` to an untouched
node is skipped and the stored value stays `+0`. The OM cannot store negative zero starting
from the default. (Numeric equality is preserved; only the sign bit of zero is lost.)
*Tests:* `NumericResponseTypeBoundaryTests.Float_Val_NegativeZero_*`, `Double_Val_NegativeZero_*`.

### D. JSON cannot round-trip large whole-number decimals / integer-family values
`SdcSerializerJson` (Newtonsoft) emits a whole-number `decimal` with no fractional part as
a bare integer literal. On read, a literal whose magnitude exceeds `ulong` range
(> 1.8e19) is materialized as `System.Numerics.BigInteger`, which the OM's `decimal`-backed
`val` setter cannot convert (`InvalidCastException`). XML preserves these values; **JSON
currently throws**. Values within `ulong` range, and any value with a fractional part,
round-trip through JSON normally.
*Tests:* `NumericResponseTypeRoundTripTests.Decimal_Val_JsonRoundTrip_LargeWholeNumber_ThrowsDocumented`,
`Integer_Val_JsonRoundTrip_LargeMagnitude_ThrowsDocumented`.

### E. BSON does not support unsigned values above `long.MaxValue`
BSON integer types are signed-only. `unsignedLong_Stype.val = ulong.MaxValue` cannot be
written (`JsonWriterException`: "Value is too large to fit in a signed 64 bit integer. BSON
does not support unsigned values."). XML, JSON, and MsgPack preserve `ulong.MaxValue`; BSON
throws.
*Tests:* `NumericResponseTypeRoundTripTests.UnsignedLong_Val_BsonRoundTrip_AtMaxValue_ThrowsDocumented`.

### F. BSON loses precision on high-precision decimals
BSON has no native decimal type; Newtonsoft encodes `decimal` as a 64-bit IEEE `double`, so
values needing more than ~15–17 significant digits are rounded on read. XML, JSON, and
MsgPack preserve full decimal precision; BSON does not.
*Tests:* `NumericResponseTypeRoundTripTests.Decimal_Val_BsonRoundTrip_HighPrecision_LosesPrecisionDocumented`.

---

## 3. Special floating-point values

XSD `xs:float`/`xs:double` value spaces include `NaN`, `+INF`, `−INF`, and `−0`. The OM
preserves `NaN`/`±INF` through get/set and through XML and JSON round-trips. XML lexical
forms are `NaN` / `INF` / `-INF`; Newtonsoft JSON emits `"NaN"` / `"Infinity"` /
`"-Infinity"` (via `FloatFormatHandling.String`). `−0` is subject to Divergence C above.

---

## 4. `ShouldSerialize*` zero semantics

Setting `val = 0` (or a facet `= 0`) through the property setter always sets the backing
`_shouldSerialize* = true`, so an explicit zero **is** serialized. A value never assigned
leaves `_shouldSerialize* = false`, so default zero is **omitted** — preserving the
"empty vs. explicit zero" distinction. Verified in both the unit and round-trip suites via
`ShouldSerializeval()` rather than string-scraping the output.

---

## 5. Malformed numeric input

The builder path `IDataHelpers.AddDataTypesDE(...)` parses incoming string values with
`TryParse` and never throws on malformed numerics; parse failures are appended to the
caller-supplied `IList<Exception> errors`. Public overloads that omit the `errors` argument
silently drop the error — tracked separately for graceful logging in GitHub issue
**rmoldwin/SDC_ObjectModel #6** (deferred; not part of this test work).
*Tests:* `NumericResponseTypeBoundaryTests` malformed-input region.
