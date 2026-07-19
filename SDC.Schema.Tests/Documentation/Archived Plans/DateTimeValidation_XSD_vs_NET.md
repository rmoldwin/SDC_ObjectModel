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

### 2.4 BSON DateTime read offset — root-caused and fixed (exact 4-format parity)
All date/date-part types — string-backed **and** DateTime-backed — now round-trip with the exact
stored instant across **all four** wire formats (XML, JSON, BSON, MsgPack); the
`DateResponseTypeRoundTripTests.DateTimeBacked_Values_RoundTripThroughAllSerializers` test asserts
this for `date`/`dateTime`/`dateTimeStamp` with no carve-out.

**Root cause (BSON only).** Earlier the BSON path returned DateTime-backed values shifted by the host
UTC offset (e.g. host `UTC−05:00`: `xs:date 2026-06-22T00:00:00` came back as `2026-06-21T19:00:00`).
The write side was correct — `BsonDataWriter` with `DateTimeZoneHandling.Utc` relabels an
`Unspecified`-Kind value to UTC **without shifting** the tick count and stores it as an absolute UTC
instant. The defect was on read: **`BsonDataReader.DateTimeKindHandling` defaults to
`DateTimeKind.Local`**, so the reader re-projected that stored UTC instant into the host's local zone,
subtracting the local offset from the ticks. The serializer-level `DateTimeZoneHandling` does **not**
override the BSON reader's kind handling. (JSON and MsgPack never exhibited this — they decode dates
as UTC, preserving ticks; MsgPack was verified correct empirically.)

**Fix.** `SdcSerializerBson` sets `bsonDataReader.DateTimeKindHandling = DateTimeKind.Utc` on both the
`DeserializeBson` and `DeserializeBsonValidating` read paths, so the stored UTC instant is returned
unchanged. This is a strict correctness improvement for **every** `DateTime` value carried through
BSON, not only date/date-part types. Note `DateTimeKind` itself is not part of the XSD value (the
lexical instant is): XML preserves `Unspecified`, while JSON/BSON/MsgPack normalize to `Utc`; since
`DateTime` equality compares the absolute instant, the round-tripped **value** is identical in all
four formats.

### 2.5 Duration family — full ISO-8601 patterns authored
The generated `dayTimeDuration`/`yearMonthDuration` setters carried a weak/partial regex; the
authoritative `date`/`dateTime`/`time`/`g*` regexes (promoted from `IDataHelpers.AddDataTypesDE`)
were **unanchored**. `XsdDateTimePatterns` defines fully **anchored** (`^…$`) patterns plus complete
ISO-8601 `duration`/`dayTimeDuration`/`yearMonthDuration` forms (rejecting a bare `P`, a bare `PT`,
and parts illegal for the restricted duration subtypes). These anchored patterns are the
authoritative validation; the registry replaces the weak generated regexes with them.

### 2.6 Construction limitation for `gMonth`/`gYearMonth` and `dayTimeDuration` — RESOLVED (issue #10)
Investigated against the live tree-builder. The standard response builder
(`AddChildQuestionResponse` → `AddDataType` → the generic by-CLR-type attach matcher) **used to throw**
`InvalidOperationException` for exactly three types:

| Built type | Original exception detail |
| --- | --- |
| `gMonth_DEtype` | "a unique XmlElementAttribute Type match to newNode's Type could not be determined" (**ambiguous — 2 matches**) |
| `gYearMonth_DEtype` | "a property with a unique type match to newNode's datatype could not be found" (**0 matches**) |
| `dayTimeDuration_DEtype` | "a property with a unique type match to newNode's datatype could not be found" (**0 matches**) |

**Root cause** was the generated `DataTypes_DEType.Item` polymorphic choice mapping, **not**
validation: the matcher resolves a node's element name by scanning the parent's `[XmlElement]`
attributes for the node's runtime CLR type, and the generated mapping made that impossible for these
three:

```
[XmlElement("gMonth",     typeof(gMonth_DEtype), ...)]
[XmlElement("gYearMonth", typeof(gMonth_DEtype), ...)]   // WRONG: SAME CLR type as gMonth → ambiguous
// (no [XmlElement("dayTimeDuration", ...)] existed at all — only "duration" and "yearMonthDuration")
```

So `gMonth_DEtype` matched **two** elements (ambiguous), while `gYearMonth_DEtype` and
`dayTimeDuration_DEtype` matched **zero**. The anchored lexical patterns run only **after** a node is
attached and therefore could not influence this; the prefix-ambiguity of the *old unanchored* regexes
is unrelated (those regexes are value validators inside `AddDataTypesDE`, not the type→element
resolver).

**Resolution (issue #10, approved Option 1).** The generated bindings were hand-corrected:
`gYearMonth` now binds `typeof(gYearMonth_DEtype)`, and a `[XmlElement("dayTimeDuration",
typeof(dayTimeDuration_DEtype), ...)]` binding was added. Because the file is no longer
auto-generated in place, it was moved out of the regen-special `SDC Unmodified Classes/` folder to
`SDC Customized Classes/Hand-Corrected Generated Classes/DataTypes_DEType.cs`, annotated at each edit
site with `ISSUE #10 HAND-CORRECTION` markers, and a README documents the regen-survival procedure
(re-apply the correction and delete any regenerated copy; the `partial class` would otherwise cause a
loud duplicate-member build failure). All twelve date/date-part types now build through the standard
tree API, and the boundary + round-trip suites construct these three the same way as every other
datatype (the deserialization-ctor workaround was removed). All four serializers round-trip them
exactly.

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
