# Session Handoff — Data Integrity & Centralized Validation Plan

**Branch (Phase 1):** `Features/NET10/Net10Main` (commit `6cecee6`)  
**Branch (Phase 2+):** `Features/NET10/TypeValidations` (commit `b30fac0`)  
**Date:** 2026-06  
**Status of prior work:** All 418 tests passing; JSON/BSON/MsgPack serializer repair complete (Phase 1 complete, Phase 2 complete).

---

## Progress to Date (completed)

| Item | Status |
|---|---|
| Test-host stabilization (x64 forced via `vstest.runsettings`) | ✅ Done |
| JSON/BSON round-trip fidelity (decimal converter, `DateTimeZoneHandling.Utc`, `FloatFormatHandling.String`) | ✅ Done |
| MsgPack rewrite: MsgPack-CSharp → `Newtonsoft.Msgpack` | ✅ Done |
| Bulk removal of `using MsgPack.Serialization;` (506 files) | ✅ Done |
| `SdcEntityBase.cs` — dead MsgPack comment block removed | ✅ Done |
| Full test suite 418/418 passing | ✅ Done |
| `global.json` pinning SDK to 10.0.300 | ✅ Done |
| **Phase 1** — Fix B-1/B-6 in `IDataHelpers.AddDataTypesDE` (B-1 uncomment Item assigns, B-2 float_DEtype, B-3 dt.val=s, B-4 anyType, B-5 errors param) | ✅ Done (`6cecee6`) |
| **Phase 2** — `SdcValidationEvents` hub: `SdcValidationEventArgs`, `SdcValidationSeverity`, `SdcValidationEvents` static class; `StoreError` fires hub | ✅ Done (`b30fac0`) |
| **Phase 3** — `SdcUtil.ValidateAndRaise` helper + bulk setter rewrite (34 files, 60 setters); non-throwing assign-and-raise semantics; `IsDeserializing` guard on all | ✅ Done (`6514799`) |

**Open Q answers recorded:**
- Q1: Reject (keep old value) + fire `SdcValidationEvents`.
- Q2: Deferred — xsd2code++ regen timeline TBD; Phase 3 setter wiring held.
- Q3: `AddDataTypesDE` errors → `SdcValidationEvents` (hub); `errors` out-param kept as secondary.

---

## Audit Results — What Exists Now

### Existing Validation Infrastructure

| Component | Location | Description |
|---|---|---|
| `Validator.ValidateProperty(...)` | ~35 generated type files | DataAnnotations-based setter validation; throws `ValidationException` |
| `[RangeAttribute]`, `[FractionDigitsAttribute]`, `[MaxDigitsAttribute]`, `[RegularExpressionAttribute]` | Generated type files | Decorates properties that use setter validation |
| `SdcUtil.IsDeserializing` (`AsyncLocal<bool>`) | `SdcUtil.cs` | Gate used in **some** setters (only `integer_DEtype` confirmed) to skip validation during deserialization |
| `SdcValidate.ValidateSdcObjectTree / ValidateSdcXml` | `Utilities/SdcValidate.cs` | XSD-schema post-hoc XML validator; returns `List<ValidationEventArgs>` |
| `IValidationTests` | `Interfaces.cs` (line 350) | Empty marker interface — placeholder only |
| `ValidationType` | `ValidationType.cs` | SDC Schema type for expressing form-level validation rules (SelectionTests, SelectionSets, ItemAlternatives) — not an imperative validator |
| `ObjectChangeTracker` | `ObjectChangeTracker.cs` | Tracks property changes; not a validation model |

**Key gap:** No centralized imperative validator that aggregates errors without throwing, hooks events, or integrates with UI-side data-entry events.

### `IDataHelpers.AddDataTypesDE` — Bugs Found

This method is the best candidate for a centralized node-creation/validation nucleus but has several blocking bugs:

| # | Bug | Description |
|---|---|---|
| B-1 | **Item never assigned** | All `rfParent.Response.DataTypeDE_Item = dt;` lines are commented out. The created `dt` objects are **orphaned** and never wired to `rfParent.Response.Item`. The correct property name is `Item` (not `DataTypeDE_Item`). |
| B-2 | **Wrong type for `float` case** | `case ItemChoiceType.@float` creates `new double_DEtype(...)` instead of `new float_DEtype(...)`. |
| B-3 | **String escaping bug** | The escape replacements are applied to local variable `s`, but `dt.val = (string)value` then assigns the *original unescaped* `value`. The escaped result in `s` is discarded. Should be `dt.val = s;`. |
| B-4 | **`AddAny_DE` wrong ItemElementName** | Sets `ItemChoiceType2.HTML` instead of `ItemChoiceType2.anyType`. |
| B-5 | **`StoreError` errors silently lost** | `StoreError(...)` populates local `exList` but `exList` is never returned, thrown, or logged. All validation errors in this method are silently discarded. |
| B-6 | **`HTML`/`XML`/`anyType` cases** | These three cases create `dt` but never assign it anywhere (commented-out assignments, never fixed). |
| B-7 | **Most setters lack `IsDeserializing` guard** | Only `integer_DEtype` confirmed to use `SdcUtil.IsDeserializing.Value` guard; most generated setter validators throw unconditionally, which would break deserialization if out-of-range values were ever written. |

### Date/Time Type Mapping — Current State

| SDC XML type | C# property type | `XmlAttribute(DataType=...)` | Concern |
|---|---|---|---|
| `date` | `System.DateTime` | `"date"` | XmlSerializer strips time/tz; correct for XML round-trip. JSON/BSON/MsgPack may corrupt if `DateTimeZoneHandling.Utc` is applied. |
| `time` | `System.DateTime` | `"time"` | XmlSerializer strips date; correct for XML. Same JSON concern. |
| `dateTime` | `System.DateTime` | `"dateTime"` | OK for most uses; no TZ preservation in C# `DateTime`. |
| `dateTimeStamp` | `System.DateTime` | `"dateTime"` | Must be UTC. Applying `DateTimeZoneHandling.Utc` here is *correct*. |
| `gDay/gMonth/gMonthDay/gYear/gYearMonth` | `string` | (via val) | Stored as string; round-trip safe. |
| `duration`/`dayTimeDuration`/`yearMonthDuration` | `string` | (via val) | Stored as string; round-trip safe. |

**Concern:** `DateTimeZoneHandling.Utc` in JSON/BSON/MsgPack serializers is correct for `dateTimeStamp` but will silently shift `date` and `time` values when the host is not in UTC. This needs per-type custom converters (or the setting needs to be removed globally and replaced with per-property handling).

---

## Implementation Plan

### Phase 1 — Fix `IDataHelpers.AddDataTypesDE` bugs (B-1 through B-6)

**Goal:** Make node-creation code functionally correct so it can serve as the foundation for further work.

1. **Fix B-1** — Replace all `//rfParent.Response.DataTypeDE_Item = dt;` commented lines with `rfParent.Response.Item = dt;` (active assignment to the correct `Item` property).
2. **Fix B-2** — Change `new double_DEtype(rfParent.Response)` in the `@float` case to `new float_DEtype(rfParent.Response)`.
3. **Fix B-3** — In the `@string` case, change `dt.val = (string)value;` to `dt.val = s;` so the escaped value is assigned.
4. **Fix B-4** — In `AddAny_DE`, change `ItemChoiceType2.HTML` to `ItemChoiceType2.anyType`.
5. **Fix B-5/B-6** — Surface `StoreError` results: change the signature to return `(DataTypes_DEType response, IReadOnlyList<Exception> errors)` or accept an `IList<Exception>` output parameter, so callers can see validation errors. Update `IResponseFieldExtensions.AddDataType` and `QuestionItemTypeExtensions` callers accordingly.

### Phase 2 — Design `SdcValidationEvent` infrastructure

**Goal:** A non-throwing, event-driven validation model that UI and logger consumers can subscribe to.

**Proposed design:**

```csharp
// Central event args — carries a single validation issue
public class SdcValidationEventArgs : EventArgs
{
	public string NodeID { get; init; }          // BaseType.ID of the node being validated
	public string PropertyName { get; init; }    // Property that failed
	public object? AttemptedValue { get; init; } // Value that was rejected
	public string Message { get; init; }         // Human-readable description
	public SdcValidationSeverity Severity { get; init; } // Error | Warning | Info
	public IReadOnlyList<ValidationResult> Results { get; init; } // DataAnnotations results
}

public enum SdcValidationSeverity { Info, Warning, Error }

// Central static event hub (module-level, simple for now)
public static class SdcValidationEvents
{
	public static event EventHandler<SdcValidationEventArgs>? ValidationOccurred;
	internal static void Raise(SdcValidationEventArgs e) => ValidationOccurred?.Invoke(null, e);
}
```

**Integration points:**
- In setter validation (generated code pattern): instead of `Validator.ValidateProperty(value, ctx)` throwing, collect `ValidationResult`s and call `SdcValidationEvents.Raise(...)`. 
- Gate with `!SdcUtil.IsDeserializing.Value` so deserialization does not fire events.
- `SdcValidate` can subscribe a listener to collect all events into its existing `List<ValidationEventArgs>` or a new `List<SdcValidationEventArgs>`.
- UI can subscribe independently (e.g., in a ViewModel or Blazor component).
- Logger can subscribe and write to `ILogger<T>` or `Trace`.

### Phase 3 — Centralize setter validation wiring

**Goal:** Replace the repeated `Validator.ValidateProperty(...)` + throw pattern in all ~35 generated files with a helper that calls `SdcValidationEvents.Raise(...)` instead of throwing.

**Approach:**
1. Add a static helper in `SdcUtil` (or new `SdcValidationHelper`):
   ```csharp
   public static bool ValidateAndRaise<T>(T value, ValidationContext ctx)
   {
	   var results = new List<ValidationResult>();
	   if (!Validator.TryValidateProperty(value, ctx, results))
	   {
		   SdcValidationEvents.Raise(new SdcValidationEventArgs { ... });
		   return false;
	   }
	   return true;
   }
   ```
2. Setter pattern becomes:
   ```csharp
   if (!SdcUtil.IsDeserializing.Value)
	   SdcUtil.ValidateAndRaise(value, validatorPropContext);
   _field = value; // always assign (caller responsible for not setting invalid values in UI)
   ```
3. Update xsd2code++ template (or post-generation script) to emit the new pattern instead of `Validator.ValidateProperty(...)`.

> **Note:** Whether the assignment still proceeds after a failed validation is a design choice. The recommended approach: always assign (non-throwing), surface the error to UI/logger via event. This keeps round-trip deserialization safe.

### Phase 4 — Audit `IsDeserializing` guard coverage

**Goal:** Ensure all generated type setters that call `Validator.ValidateProperty(...)` are gated.

1. Identify all setters that call `Validator.ValidateProperty(...)` but do NOT check `SdcUtil.IsDeserializing.Value`.
2. Add the guard to each, or centralize via the helper in Phase 3.
3. Add a test that round-trips a document with boundary numeric values and verifies no `ValidationException` is thrown.

### Phase 5 — Date/time serializer settings audit

**Goal:** Prevent `DateTimeZoneHandling.Utc` from silently corrupting `date` and `time` values in JSON/BSON/MsgPack.

**Options (to be evaluated):**
- A) Add per-type `JsonConverter` subclasses for `date_Stype`, `time_Stype` that write/read using `XmlConvert.ToString(dt, XmlDateTimeSerializationMode.Unspecified)` and strip/restore only the relevant part.
- B) Remove `DateTimeZoneHandling.Utc` from all serializer settings and rely on the `[XmlAttribute(DataType="date")]` semantics only in XML; accept that JSON/BSON/MsgPack are UTC-normalized (document as a known limitation).
- C) Wait for xsd2code++ regeneration to produce `DateOnly`/`TimeOnly` types (C# 10+ support), which eliminates the problem entirely.

**Recommendation:** Option C is the cleanest long-term fix. For now, document the limitation clearly and defer until regeneration. If regeneration is imminent (as discussed), do not invest in per-type converters.

### Phase 6 — xsd2code++ Regen Strategy

**Goal:** When the new xsd2code++ version generates fresh code, custom code must be preserved and new features adopted safely.

**Strategy:**
1. Tag all custom/hand-written files with a `// SDC-CUSTOM: do not overwrite` header comment.
2. Define "custom zones" as:
   - All files in `SDC Customized Classes/` folder
   - All files in `Utility Classes/`, `Extensions/`, `Interfaces/`
   - `PartialClasses.cs` custom `partial class` additions
3. After regen:
   - Run a diff tool comparing new generated output to current `SDC Unmodified Classes/`
   - Apply diff to custom classes manually where relevant
   - Run the full test suite (418+ tests) to verify no regressions
4. Update xsd2code++ template to emit the new setter validation pattern (Phase 3) by default.

---

## Open Questions for Next Session

1. **Validation assignment semantics:** Should a setter that receives an invalid value (a) reject it and keep the old value, (b) assign it anyway and emit an event, or (c) assign it only if deserialization is active? This must be decided before Phase 2/3 implementation.

2. **`IDataHelpers` scope:** Should `IDataHelpers` remain an interface with static methods, become an abstract base, or move to a static class? The current interface-with-static-members pattern (C# 8+) works but has IDE limitations.

3. **`StoreError` return pattern:** Should `AddDataTypesDE` return a result tuple `(DataTypes_DEType, IReadOnlyList<SdcValidationEventArgs>)`, or should callers be expected to subscribe to `SdcValidationEvents.ValidationOccurred` before calling?

4. **xsd2code++ upgrade timeline:** If regeneration is imminent, Phase 5 (date/time) and Phase 3 (setter pattern) should be deferred and baked into the new template instead.

5. **`DateTimeZoneHandling.Utc` in current serializers:** Is there any round-trip test that exercises `date_Stype.val` or `time_Stype.val` with non-UTC values? If so, verify those tests still pass, or document a known failure.

---

## Next Immediate Steps (priority order)

1. Fix B-1 through B-6 in `IDataHelpers.AddDataTypesDE` (Phase 1) — unblocks everything else.
2. Design and add `SdcValidationEvents` static hub and `SdcValidationEventArgs` (Phase 2).
3. Add a `SdcUtil.ValidateAndRaise(...)` helper (Phase 3 prep).
4. Answer the open questions above before implementing Phase 3 at scale.
5. Defer Phase 4/5 pending regeneration timeline decision.

---

*Document prepared at end of serializer-repair session. See also: `Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md`*
