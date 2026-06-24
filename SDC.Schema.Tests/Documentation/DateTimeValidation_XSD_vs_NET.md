# Date / Date-Part ResponseTypes: XSD vs .NET, and Serializer Divergences

**Scope:** Every XML Schema date and date-part datatype the SDC Object Model exposes for
ResponseType fields — the `*_Stype` response value (`val`), the associated `timeZone`
property (where present), and the `*_DEtype` constraint facets. The twelve types covered:

`date`, `dateTime`, `time`, `dateTimeStamp`, `gYear`, `gYearMonth`, `gMonth`, `gMonthDay`,
`gDay`, `duration`, `dayTimeDuration`, `yearMonthDuration`.

This is the date/time companion to `NumericRange_XSD_vs_NET.md`; it reuses the **identical**
issue #8 soft-reject contract and pipeline. Read that document first for the contract details.

**Companion code / tests**

| Layer | File |
|---|---|
| Lexical patterns + messages (single source of truth) | `SDC.Schema/SDC.Schema/SDC Customized Classes/Date Validation/XsdDateTimePatterns.cs` |
| Validation attributes | `SDC.Schema/SDC.Schema/SDC Customized Classes/Date Validation/XsdDateValidationAttributes.cs` |
| Rule registry (regen-safe wiring) | `SDC.Schema/Utility Classes/SdcValidationRuleRegistry.cs` + `…/Date Validation/SdcDateValidationRules.cs` |
| Lexical set+validate entry points | `SDC.Schema/SDC.Schema/SDC Customized Classes/Date Validation/SdcDateLexical.Partials.cs` |
| Object-model (unit) | `SDC.Schema.Tests/OM/DateResponseTypeBoundaryTests.cs` |
| Serialization (functional round-trip) | `SDC.Schema.Tests/Functional/Serialization/DateResponseTypeRoundTripTests.cs` |

---

## 0. Validation contract: soft-reject (issue #8, reused verbatim)

A date/date-part setter — or any deserialization path (XML/JSON/BSON/MsgPack) — that receives a
value violating the XSD lexical rules **soft-rejects** it: never stores the invalid value, never
throws, always records the offending value in `BaseType.RejectedValues[propertyName]` (an
`SdcRejectedValue` whose message quotes the value, names the `xs:` type, gives the canonical form +
an example, and pinpoints the violation), and raises events/reports unless validation is suppressed.
Nothing is silently dropped or silently truncated.

The messages are produced by `XsdDateTimePatterns.BuildLexicalErrorMessage` /
`BuildTimezoneErrorMessage` and are pinned by characterization tests so their teaching quality
cannot silently degrade.

---

## 1. Backing-type matrix

| Family | Types | `val` backing | Lexical validation point |
|---|---|---|---|
| **String-backed** | `gYear, gYearMonth, gMonth, gMonthDay, gDay, duration, dayTimeDuration, yearMonthDuration` | `string` (the val **is** the XSD lexical string) | the generated soft-reject setter, via a registered `XsdDateLexicalAttribute` rule |
| **DateTime-backed** | `date, dateTime, time, dateTimeStamp` | `System.DateTime` (+ a separate `string timeZone` on date/dateTime/time) | the **string boundary** — `SetLexicalValue(string)` |

For string-backed types the lexical string rides the issue #8 setter pipeline directly. For
DateTime-backed types a `System.DateTime` cannot encode the lexical rules (timezone presence, a
stray time/date component, the end-of-day `24:00:00`, a required timezone), so lexical validation is
enforced where the string still exists: `SetLexicalValue(string)` validates first and only assigns
`val`/`timeZone` on success.

---

## 2. Divergences (XSD value space vs the .NET implementation)

### 2.1 A `DateTime` cannot carry a timezone, so `timeZone` is a separate string
`date`/`dateTime`/`time` store the instant in a `System.DateTime val` and the offset in a separate
`string timeZone`. The timezone is validated (`XsdTimezoneOffsetAttribute`, range `-14:00..+14:00`)
but kept as text. **Future work:** migrate `timeZone` to a `TimeSpan`/offset type — see the filed
GitHub issue. The validator deliberately also accepts the .NET `TimeSpan` textual form
(`±h:mm:ss`) that the existing `IDataHelpers` parse path emits, so the change does not break
already-stored documents.

### 2.2 `dateTimeStamp` requires a timezone — enforced at the string boundary (issue I-1 fix)
`xs:dateTimeStamp` is an `xs:dateTime` whose timezone is **mandatory**. The generated
`dateTimeStamp_Stype.val` (and the four `dateTimeStamp_DEtype` facets) are `System.DateTime` yet
carry `[RegularExpression(".*(Z|±dd:dd)")]`. Under issue #8 soft-reject that regex runs against
`DateTime.ToString()` and can **never** match, so **every** `dateTimeStamp` value was being dropped.

**Fix (regen-safe, no generated file edited):** the rule registry registers an **empty** rule set
for those members, neutralizing the impossible regex; the timezone-required rule is instead enforced
on the lexical string via `SetLexicalValue` (`XsdDateKind.DateTimeStamp`). A `dateTimeStamp` is
normalized to **UTC** on assignment (matching the existing `IDataHelpers` behavior). Regression:
`DateResponseTypeBoundaryTests.DateTimeStamp_ValidDateTime_IsStored_I1Regression`.

### 2.3 End-of-day `24:00:00`
XSD permits the end-of-day time `24:00:00`; .NET cannot store hour 24. `SetLexicalValue` accepts it
lexically and normalizes it to `00:00:00` of the next day.

### 2.4 BSON / MsgPack shift DateTime by the host offset
The string-backed types survive **all four** wire formats byte-for-byte. The DateTime-backed types'
exact stored instant is round-trip-asserted only for **XML and JSON**: BSON and MsgPack encode a
`DateTime` as a UTC tick count and apply the serializer's `DateTimeZoneHandling`, which shifts an
`Unspecified`-Kind local `DateTime` by the host offset. This is a property of those wire formats'
DateTime handling, not of the validation work.

### 2.5 Duration family — full ISO-8601 patterns authored
The generated `dayTimeDuration`/`yearMonthDuration` setters carried a weak/partial regex; the
authoritative `date`/`dateTime`/`time`/`g*` regexes (promoted from `IDataHelpers.AddDataTypesDE`)
were **unanchored**. `XsdDateTimePatterns` defines fully **anchored** (`^…$`) patterns plus complete
ISO-8601 `duration`/`dayTimeDuration`/`yearMonthDuration` forms (rejecting a bare `P`, a bare `PT`,
and parts illegal for the restricted duration subtypes). These anchored patterns are the
authoritative validation; the registry replaces the weak generated regexes with them.

### 2.6 Construction limitation for `gMonth`/`gYearMonth` and `dayTimeDuration`
`gMonth_DEtype` is intentionally reused for two element names (`gMonth` **and** `gYearMonth`) under
`DataTypes_DEType`, and `dayTimeDuration` is not an element of that parent. The standard response
builder therefore cannot construct these without an explicit element name (a **pre-existing**
limitation, unrelated to validation). Their lexical validation is fully covered by the OM boundary
tests (which construct the node via its deserialization constructor); the round-trip suite covers the
nine types the builder can construct.

---

## 3. Regen-safe mechanism (no `<auto-generated>` file edited by hand)

* **Rule registry:** `SdcValidationRuleRegistry` maps `(Type, memberName) → ValidationAttribute[]`,
  consulted inside `SdcUtil.ValidateAndRaise`. A registered rule set **overrides** the property's
  declared attributes (an empty set neutralizes them). Registrations are applied at load via a
  `[ModuleInitializer]` in `SdcDateValidationRules`.
* **Plain string setters** (`gYear/gYearMonth/gMonth/gMonthDay/gDay/duration`) that xsd2code++ emits
  with no validation call are wired into the soft-reject pipeline by a narrowly-scoped, idempotent
  transform in `SDC Schema Files/Apply_SoftReject_Setters.py` (re-run after every regeneration).
* **`SetLexicalValue`** lives in partial-class files, never in generated files.
