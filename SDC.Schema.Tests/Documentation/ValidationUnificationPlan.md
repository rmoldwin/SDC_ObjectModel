# SDC Validation Unification Plan
**Session:** `730e6451-32ce-4ec4-bbed-5820dac35bba`  
**Branch:** `Features/NET10/IDataHelpers`  
**Created:** 2026-06-24  
**Last Updated:** 2026-06-25  
**Status:** Phases 1–9 ✅ COMPLETE. Pending merge of Phase 9 to Net10Main.
**Test count: 656 passed, 0 failed (post-Phase 9)**

---

## Goal

Create a single, DRY validation pipeline where **every** entry point that can
write a user-supplied value into the SDC OM — direct setter mutation, extension
method construction, IDataHelpers/SdcDataTypeBuilder, SetLexicalValue, and all
four (de)serializers — routes through the same validators and fires the same
`SdcValidationEvents.ValidationOccurred` event and populates the same
`SdcUtil.ValidationCollector`.

---

## Existing Infrastructure (already correct — do not change)

| Component | File | Role |
|---|---|---|
| `SdcUtil.ValidateAndRaise()` | `SdcUtil.cs:157` | Central soft-reject; DataAnnotations + RuleRegistry; fires event + collector |
| `SdcUtil.ValidateLexicalAndRaise()` | `SdcUtil.cs:195` | Same contract for lexical date/duration strings |
| `SdcUtil.SuppressValidation` | `SdcUtil.cs:65` | AsyncLocal: suppresses events/collector noise during normal deserialization |
| `SdcUtil.IsDeserializing` | `SdcUtil.cs:52` | AsyncLocal: suppresses tree-mutation side-effects; decoupled from SuppressValidation |
| `SdcUtil.ValidationCollector` | `SdcUtil.cs:75` | AsyncLocal: aggregates issues during validating-deserialize and explicit ValidateTree sweeps |
| `SdcUtil.RecordRejectedValue` / `GetRejectedValues` | `SdcUtil.cs:88–126` | ConditionalWeakTable soft-reject store per node |
| `SdcValidationEvents.ValidationOccurred` | `SdcValidationEvents.cs:72` | Central static event hub |
| `SdcValidationRuleRegistry` | `SdcValidationRuleRegistry.cs` | Override per-type/member DataAnnotations attributes without touching generated files |
| `SdcValidationReport` | `SdcValidationReport.cs` | Structured report from collector |
| All four serializers | `SdcSerializer*.cs` | All bracket deserialization with `IsDeserializing.Value = true/false` in try/finally |
| `InternalsVisibleTo("SDC.Schema.Tests")` | `SdcValidationEvents.cs:5` | Tests already see internal members |

---

## Known Gaps (to be fixed)

### GAP 1 — IDataHelpers.StoreError() silo
- `IDataHelpers.AddDataTypesDE` uses a private `StoreError()` that calls `SdcValidationEvents.Raise()` directly
- Does NOT call `SdcUtil.RecordRejectedValue`, does NOT append to `ValidationCollector`
- 36 direct `dt.val = parsed_value` assignments without constraint cross-check

### GAP 2 — Inconsistent val setter coverage
- `integer` family in `SDC Constructor Removed`: ✅ ValidateAndRaise already wired
- `decimal_Stype`, `string_Stype`, `float_Stype`, `double_Stype` etc.: ❌ raw assignment, no validation
- Some in `SDC Unmodified Classes` (auto-generated): need AddedValidationLogic copies

### GAP 3 — No cross-property coherence validation
- Nothing prevents `val=50, minInclusive=100` (incoherent but silently accepted)
- 9 rules not enforced: val vs min/maxInclusive, min/maxExclusive, pattern, minLength, maxLength, fractionDigits, totalDigits
- Also: minInclusive vs maxInclusive coherence not checked

### GAP 4 — HTML/XML/anyType TODO stubs
- Three ItemChoiceType cases have stub bodies only

### GAP 5 — Duration/gDate TODO items
- dayTimeDuration: year-to-hours conversion missing
- yearMonthDuration: truncation after hh:mm:ss missing
- gDay/gMonth/gMonthDay: leading-dash handling missing

---

## Policy: Edited Generated Files

User permission granted (2026-06-24) to edit generated SDC OM class files for
validation purposes. Files must be:
1. Copied/moved into a new `AddedValidationLogic` subfolder under their current parent
2. Original logic commented out where replaced
3. New logic placed alongside original (commented out) logic

Auto-generated file folders eligible for AddedValidationLogic treatment:
- `SDC.Schema/SDC.Schema/SDC Unmodified Classes/AddedValidationLogic/`
- `SDC.Schema/SDC.Schema/SDC Constructor Removed/Constructor Commented Out Only/AddedValidationLogic/`
- `SDC.Schema/SDC.Schema/SDC Constructor Removed/DateTimeOffset Datatypes and Constructor/AddedValidationLogic/`
- `SDC.Schema/SDC.Schema/SDC Constructor Removed/SDC TimeSpan string Datatypes/AddedValidationLogic/`

---

## Full Implementation Plan

### PHASE 0 — BASELINE & AUDIT (no code changes)
- [x] P0.1  Run full test suite → baseline: 568 tests passing
- [x] P0.2  Enumerated every *_Stype.cs: val setter coverage, folder, auto-gen status
- [x] P0.3  Enumerated every *_DEtype.cs: constraint setter coverage
- [x] P0.4  Confirmed files excluded from build in .csproj
- [x] P0.5  Identified 4 AddedValidationLogic subfolders needed
**Status: ✅ COMPLETE**

### PHASE 1 — CONVERT IDataHelpers TO internal static class
**Branch:** `Features/NET10/IDataHelpers_Refactor`
**Status:** ✅ COMPLETE (committed `fbb82d2`) — 554 tests pass

- [x] P1.1  Created `SDC.Schema/Utility Classes/SdcDataTypeBuilder.cs` as `internal static class SdcDataTypeBuilder`
- [x] P1.2  Moved all static method bodies from IDataHelpers: AddDataTypesDE, AddHTML_DE, AddXML_DE, AddAny_DE, AssignQuantifier
- [x] P1.3  Deleted 5 orphaned non-static interface signatures
- [x] P1.4  Updated IResponseFieldExtensions.cs → SdcDataTypeBuilder
- [x] P1.5  Updated QuestionItemTypeExtensions.cs → SdcDataTypeBuilder; deleted commented duplicate; deleted X_AddListItemResponse
- [x] P1.6  Updated test call sites (NumericResponseTypeBoundaryTests ×2, SdcValidationEventsTests ×1)
- [x] P1.7  Obsoleted IDataHelpers interface with [Obsolete] shim delegating to SdcDataTypeBuilder
- [x] P1.8  All tests passing at baseline

### PHASE 2 — REPLACE StoreError() WITH ValidateAndRaise()
**Branch:** `Features/NET10/IDataHelpers_Refactor`
**Status:** ✅ COMPLETE (committed `d213055`) — 554 tests pass

- [x] P2.1  Removed StoreError() private method from SdcDataTypeBuilder
- [x] P2.2  Replaced all ~26 StoreError() call sites with SdcUtil.ValidateAndRaise() / ValidateLexicalAndRaise(); caller IList<Exception> routed via local ValidationOccurred subscription
- [x] P2.3  All 26 switch cases have updated parse-error path
- [x] P2.4  All tests passing

### PHASE 3 — CROSS-PROPERTY COHERENCE CHECKER
**Branch:** `Features/NET10/CoherenceValidator`
**Status:** ✅ COMPLETE — 22 new tests passing, pushed

- [x] P3.1  Added `SdcValidate.CheckValAgainstConstraints(BaseType node, string memberName, object? newValue) → bool` (9 rules; no-op when SuppressValidation=true)
- [x] P3.2  Added `SdcValidate.CheckConstraintCoherence(BaseType node, string constraintName, object? newValue) → bool` (Error on constraint-vs-constraint; Warning on constraint-vs-val)
- [x] P3.3  Both methods wired from val setters (P4) and constraint setters (P4)
- [x] P3.4  SdcCoherenceValidationTests.cs: 22 tests — all passing

### PHASE 4 — COMPLETE val SETTER & CONSTRAINT SETTER COVERAGE
**Branch:** `Features/NET10/SetterCompletion`
**Status:** ✅ COMPLETE — 17 new tests, 593 total passing

User permission granted to edit generated files (2026-06-24).
Files go into `AddedValidationLogic` subfolders; original logic commented out.

- [x] P4.1  Created AddedValidationLogic subfolders (4 locations)
- [x] P4.2  Group A: val setters fixed — numeric, string, binary, date/time *_Stype (20 files)
- [x] P4.3  Group B: *_DEtype constraint setters fixed — byte, decimal, double, float, short, unsigned family, date/time, duration (12 files)
- [x] P4.4  CheckValAgainstConstraints call in val setters
- [x] P4.5  CheckConstraintCoherence call in constraint setters
- [x] P4.6  .csproj updated: originals excluded, AddedValidationLogic copies included
- [x] P4.7  SetterValidationTests.cs: 17 tests — 593 total passing

### PHASE 5 — FILL TODO STUBS
**Branch:** `Features/NET10/TodoStubs`
**Status:** ✅ COMPLETE — 27 new tests, 620 total (on merged branch)

- [x] P5.1  HTML_DEtype: string→XML fragment parse; ill-formed → ValidateAndRaise
- [x] P5.2  XML_DEtype: string→XmlDocument; ill-formed → ValidateAndRaise; design note added
- [x] P5.3  anyType_DEtype: same as XML; AddAny_DE retained
- [x] P5.4  dayTimeDuration: anchored regex `^-?P(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$`; Y/M components → rejected
- [x] P5.5  yearMonthDuration: anchored regex `^-?P(\d+Y)?(\d+M)?$`; D/H/S components → rejected
- [x] P5.6  gDay/gMonth/gMonthDay: auto-normalize bare digits to XSD canonical form (e.g. "05" → "---05")
- [x] anyURI: upgraded from simple Uri.TryCreate to 4-step IsValidAnyUri() — see merge note below
- [x] SdcDataTypeBuilderTodoTests.cs: 27 tests

### PHASE 6 — DOCUMENTATION & CLEANUP
**Branch:** `Features/NET10/IDataHelpers_Docs`
**Status:** ✅ COMPLETE — 620 tests passing, pushed; merged to Net10Main (commit `788aa7f`)

- [x] P6.1  Full XML docs on SdcDataTypeBuilder: class-level summary+remarks, AddDataTypesDE, AddHTML_DE/AddXML_DE/AddAny_DE/AssignQuantifier, private helpers, #region markers in switch
- [x] P6.2  XML docs on SdcValidate.CheckValAgainstConstraints, CheckConstraintCoherence; Error vs Warning severity distinction explicit
- [x] P6.3  Region groupings in switch (Numeric, Date/Time, Binary, String/URI, Markup)
- [x] P6.4  SdcValidationEvents.cs remarks updated: replaced outdated IsDeserializing gate with SuppressValidation unified flow; cross-references to SdcDataTypeBuilder, ValidateAndRaise, coherence methods
- [x] P6.5  IDataHelpers shim: <inheritdoc cref="SdcDataTypeBuilder.XXX"/> on all 6 shim methods; IResponseFieldExtensions class+method docs added
- [x] P6.6  Archived 3 superseded design docs (DateTimeValidation_Plan.md, Kickstart_DateTimeValidation.md, Session_Handoff_ValidationPlan.md) via git mv; Archived Plans/README.md updated

### PHASE 7 — INTEGER-FAMILY DEtype COHERENCE GAP
**Branch:** `Features/NET10/IntegerDEtypeCoherence`
**Status:** ✅ COMPLETE — merged to Net10Main (commit `d9b07a1`), 639 tests passing

Root cause: Phase 4 treated the integer-family DEtypes as "reference implementations" already complete. They were not audited for completeness. All 7 were missing `CheckConstraintCoherence`; `int_DEtype` also had 4 raw setters with no `ValidateAndRaise` at all.

**AddedValidationLogic copies created (all in `Constructor Commented Out Only/AddedValidationLogic/`):**

| File | Fix applied |
|---|---|
| `integer_DEtype.cs` | Added CheckConstraintCoherence to all constraint setters |
| `long_DEtype.cs` | Same |
| `int_DEtype.cs` | Added ValidateAndRaise to minInclusive, maxInclusive, minExclusive, totalDigits; added CheckConstraintCoherence to all |
| `negativeInteger_DEtype.cs` | Added CheckConstraintCoherence to all constraint setters |
| `nonNegativeInteger_DEtype.cs` | Same |
| `nonPositiveInteger_DEtype.cs` | Same |
| `positiveInteger_DEtype.cs` | Same |

`.csproj` updated: all 7 originals excluded, all 7 AddedValidationLogic copies included.

**Prevention note:** After any phase that fixes a set of files, run an exhaustive per-setter grep audit before accepting the phase as complete.

### PHASE 8 — VALIDATION LAYER CLEANUP
**Status:** Agent A ✅ merged `c7307b3` | Agent B 🔄 running

#### Agent A — AddedValidationLogic pattern normalization
**Branch:** `Features/NET10/ValidationPatternNormalize` → merged to Net10Main

Correctness bugs found and fixed across all 39 AddedValidationLogic files:
- **15 DEtype files**: `Validator.ValidateProperty` (throws on failure) replaced with `SdcUtil.ValidateAndRaise` (soft-reject, returns bool) in constraint setters — `byte_DEtype`, `decimal_DEtype`, `double_DEtype`, `float_DEtype`, `short_DEtype`, `unsignedByte_DEtype`, `unsignedInt_DEtype`, `unsignedLong_DEtype`, `unsignedShort_DEtype`, `dateTime_DEtype`, `date_DEtype`, `time_DEtype`, `duration_DEtype`, `anyURI_DEtype`, `string_DEtype`
- **19 Stype files**: `CheckValAgainstConstraints` was missing `!SdcUtil.IsDeserializing.Value` guard in val setters — added to prevent false violations during XML deserialization when constraint properties arrive in document order before val
- **15 DEtype files**: `CheckConstraintCoherence` was incorrectly gated behind SuppressValidation/deserialization guards — normalized to unconditional soft-reject flow
- **34 files**: stale comments referencing `StoreError`, `IDataHelpers`, `TODO` cleaned

#### Agent B — Core infrastructure XML docs + comment cleanup
**Branch:** `Features/NET10/ValidationDocs` — ✅ MERGED (with regression fix)
**Merged commit:** `e70e0d4` on Net10Main
**Status:** ✅ COMPLETE — 639 tests passing

**Agent B regression (Phase 8-B):**
- Agent B's `ValidationDocs` branch accidentally reverted the correct `IsValidAnyUri()` 4-step helper in `SdcDataTypeBuilder.cs` back to the broken `[#x1-#xD7FF]` regex (a regression), causing 30 test failures.
- Resolution: merged Agent B's 4 clean files (`IDataHelpers.cs`, `IResponseFieldExtensions.cs`, `SdcUtil.cs`, `SdcValidationEvents.cs`) and restored `SdcDataTypeBuilder.cs` from Net10Main (commit `c7307b3`).
- Final merge commit: `e70e0d4` — 639 tests passing.

**Agent B changes accepted:**
- `IDataHelpers.cs`: improved `[Obsolete]` text, better `<remarks>` block with full migration guidance
- `IResponseFieldExtensions.cs`: doc improvements
- `SdcUtil.cs`: improved XML docs on `ValidateAndRaise`, `ValidateLexicalAndRaise`, `SuppressValidation`, `IsDeserializing`
- `SdcValidationEvents.cs`: CS1591 fix — added `<summary>` to all `SdcValidationSeverity` enum values

**anyURI conflict resolution (merge `788aa7f`):**
- Net10Main had a hand-crafted 4-step `IsValidAnyUri()` in IDataHelpers.cs
- Resolved: took Ph6 [Obsolete] shim; ported `IsValidAnyUri()` into `SdcDataTypeBuilder.cs`
- Fixed empty-string bug: `IsNullOrWhiteSpace(tmp)` → `tmp != null` (empty string is valid anyURI per RFC 3986 §4.4)

**CS1574 fixes (`8c7b75f`):**
- `SdcValidationEvents.cs` lines 28, 86: `BaseType.ID` → `IdentifiedExtensionType.ID` (ID lives on IET subtype)
- `BaseTypeExtensions.cs` line 432: `CreateSimpleName(BaseType)` → full 4-param signature to match actual overload
- `IMoveRemoveExtensions.cs` line 1076: `<see cref="treeOrderComparer"/>` → `<c>treeOrderComparer</c>` (private identifier, not a resolvable cref target)
- `SdcUtil.cs` line 3410: `<see cref="UniqueBaseNames"/>` → `<see cref="_ITopNode._UniqueBaseNames"/>` (local var replaced by correct interface member)

**CS0618 fixes (`8c7b75f`):**
- `NumericResponseTypeBoundaryTests.cs`: `IDataHelpers.AddDataTypesDE` → `SdcDataTypeBuilder.AddDataTypesDE`; added `using SDC.Schema`
- `SdcValidationPhase4Tests.cs`: same
- `AnyURIResponseTypeBoundaryTests.cs`: same

---

## Teardown Checklist (things removed/retired)

| Item | Action | Phase |
|---|---|---|
| `IDataHelpers.StoreError()` private method | Deleted | P2 |
| 5 orphaned non-static signatures on IDataHelpers | Deleted | P1 |
| `IDataHelpers` interface | Obsoleted with [Obsolete] shim | P1 |
| `QuestionItemTypeExtensions.cs` line 143 commented duplicate | Deleted | P1 |
| `X_AddListItemResponse` private method | Deleted (confirmed unused) | P1 |
| Original *_Stype.cs files replaced by AddedValidationLogic copies | Excluded from .csproj | P4 |

---

## Call Chain (Reference)

```
══════════════════════════════════════════════════════════════════
  CONSTRUCTION PATH
══════════════════════════════════════════════════════════════════
rf.AddDataType(ItemChoiceType.integer, dtQuant.EQ, 42)
    → SdcDataTypeBuilder.AddDataTypesDE(rf, integer, EQ, 42)
        → long.TryParse("42") → ok
        → [P2] SdcUtil.ValidateAndRaise(42, ctx)
        → [P3] SdcValidate.CheckValAgainstConstraints(dt, "val", 42)
        → dt.val = 42  → val setter →  SdcUtil.ValidateAndRaise ✅

══════════════════════════════════════════════════════════════════
  DIRECT MUTATION PATH
══════════════════════════════════════════════════════════════════
dt.val = 99
    → integer_Stype.val setter
        → SdcUtil.ValidateAndRaise(99, ctx)   ← [already ✅ for integer family]
        → [P4] SdcValidate.CheckValAgainstConstraints(this, "val", 99)
        → if ok: _val = 99

dt.minInclusive = 150
    → integer_DEtype.minInclusive setter
        → Validator.ValidateProperty → [P4] → SdcUtil.ValidateAndRaise
        → [P4] SdcValidate.CheckConstraintCoherence(this, "minInclusive", 150)
        → if ok: _minInclusive = 150

══════════════════════════════════════════════════════════════════
  DESERIALIZATION PATH
══════════════════════════════════════════════════════════════════
SdcSerializer.Deserialize(xml)
    SdcUtil.IsDeserializing.Value = true
    SdcUtil.SuppressValidation.Value = true  (normal) / false (validating overload)
    XmlSerializer → val setter → SdcUtil.ValidateAndRaise
        SuppressValidation=true:  no events/collector, but invalid → rejected (not stored)
        SuppressValidation=false: full events + collector
    SdcUtil.IsDeserializing.Value = false
    ReflectRefreshTree()

══════════════════════════════════════════════════════════════════
  ALL PATHS CONVERGE
══════════════════════════════════════════════════════════════════
SdcUtil.ValidateAndRaise(value, ctx)
    → SdcValidationRuleRegistry (override attributes without touching generated files)
    → Validator.TryValidateProperty / TryValidateValue
    on failure:
        → RecordRejectedValue()           ← always
        → ValidationOccurred event        ← if !SuppressValidation
        → ValidationCollector?.Add()      ← if !SuppressValidation
    → returns bool
```

---

### PHASE 9 — TryAssignValue / WouldBeValid EXTENSION METHODS
**Branch:** `Features/NET10/TryAssignValue`
**Commit:** `cf8bd0c`
**Status:** ✅ COMPLETE — 656 tests passing (639 + 17 new)
**Baseline:** 639 tests passing

#### Motivation

The existing validation pipeline requires callers to either:
1. Assign directly (`dt.val = x`) — no return value; check event subscription or `GetRejectedValues` to know if it succeeded
2. Wire up `SdcValidationEvents.ValidationOccurred` — asynchronous, global, verbose

Phase 9 adds two **caller-friendly helpers** in a new `SdcValidationExtensions.cs` that make the call site clean and self-contained:

```csharp
// Pure predicate — runs DataAnnotations validation only; no side effects whatsoever
bool ok = dt.WouldBeValid(d => d.minInclusive, -1m);
bool ok = dt.WouldBeValid(d => d.minInclusive, -1m, out string? message);

// Attempt the assignment through the real setter — all validation fires normally
bool ok = dt.TryAssignValue(d => d.minInclusive, -1m);
bool ok = dt.TryAssignValue(d => d.minInclusive, -1m, out SdcRejectedValue? rejection);
```

#### Design decisions

| Decision | Choice | Rationale |
|---|---|---|
| Extension method vs SdcUtil static | Extension on `BaseType` | More ergonomic (dt.TryAssign…); property tied to node via generic constraint |
| Property selector | `Expression<Func<TNode, TVal>>` lambda | Only form that guarantees node instance and property name are the same object at compile time; `nameof`, `CallerArgumentExpression`, `ref` all have exploitable misuse holes |
| Value type | Generic `TVal` inferred from lambda | Compile-time type safety — wrong value type is a build error, not a runtime failure |
| `WouldBeValid` side effects | None | Pure predicate; no event fired, rejection store untouched; uses `Validator.TryValidateProperty/Value` directly (same logic as `ValidateAndRaise` but without recording) |
| `TryAssignValue` mechanism | Reflection `SetValue` through the real setter | All real validation (ValidateAndRaise, CheckValAgainstConstraints, CheckConstraintCoherence) fires; no duplication of logic |
| Prior rejection clearing | Yes — clear before attempt | Prevents a stale prior failure from being returned as if it were the result of this call |
| `ValidationOccurred` event during TryAssign | Fires normally | WYSIWYG — same behavior as direct assignment; callers who want silence should set `SuppressValidation = true` themselves |
| Reflection type mismatch | Soft-reject via synthetic `SdcRejectedValue` | Never throw `ArgumentException` at callers for a type mismatch on a validation helper |
| Invalid expression (chained/method call) | `ArgumentException` at call time | Programming error, not a data validation error; fail fast |

#### Critical design finding: TryAssignValue success detection

**Problem**: The original plan said to detect success by checking whether a rejection entry was recorded for the property after `SetValue`. This is incorrect.

**Root cause**: `CheckConstraintCoherence` records a rejection (`SdcRejectedValue`) on the node *even when the constraint value was successfully stored* — for example, setting `minInclusive = 200` when `maxInclusive = 100` stores the value but records a coherence Warning. Checking the rejection store would incorrectly return `false` for a successfully-stored-but-incoherent constraint.

**Resolution**: `TryAssignValue` uses a **value-was-stored check** instead:
```csharp
var storedAfter = pi.GetValue(node);
bool stored = Equals(storedAfter, value);
```
This correctly distinguishes:
- DataAnnotations rejection → value NOT stored → `stored = false` → return false
- Coherence violation → value IS stored, advisory rejection recorded → `stored = true` → return true

The `out SdcRejectedValue? rejection` overload still surfaces the coherence entry when `stored = true` but a rejection was recorded — so callers can inspect both the success and any warnings.

#### Files created

| File | Action |
|---|---|
| `SDC.Schema/Utility Classes/Extensions/SdcValidationExtensions.cs` | ✅ Created — `WouldBeValid` (2 overloads) + `TryAssignValue` (2 overloads) |
| `SDC.Schema.Tests/Validation/SdcValidationExtensionTests.cs` | ✅ Created — 17 tests, all pass |
| `SDC.Schema.Tests/Documentation/ValidationScenarios.md` | ✅ Created — 10-section end-to-end validation guide |

#### API signatures

```csharp
// File: SdcValidationExtensions.cs
// Namespace: SDC.Schema.Extensions  (or SDC.Schema — TBD; must be in scope for BaseType callers)

public static class SdcValidationExtensions
{
    // WouldBeValid — bool only
    public static bool WouldBeValid<TNode, TVal>(
        this TNode node,
        Expression<Func<TNode, TVal>> property,
        TVal value)
        where TNode : BaseType;

    // WouldBeValid — bool + human-readable failure message
    public static bool WouldBeValid<TNode, TVal>(
        this TNode node,
        Expression<Func<TNode, TVal>> property,
        TVal value,
        out string? message)
        where TNode : BaseType;

    // TryAssignValue — bool only
    public static bool TryAssignValue<TNode, TVal>(
        this TNode node,
        Expression<Func<TNode, TVal>> property,
        TVal value)
        where TNode : BaseType;

    // TryAssignValue — bool + full rejection details
    public static bool TryAssignValue<TNode, TVal>(
        this TNode node,
        Expression<Func<TNode, TVal>> property,
        TVal value,
        out SdcRejectedValue? rejection)
        where TNode : BaseType;
}
```

#### Internal helpers needed

```csharp
// Extract "memberName" from Expression<Func<TNode, TVal>>
// Throws ArgumentException for chained or non-member expressions
private static string GetMemberName<TNode, TVal>(Expression<Func<TNode, TVal>> expr);

// Core of WouldBeValid — no side effects
// Returns (isValid, message)
private static (bool, string?) RunDryValidation<TNode>(
    TNode node, string memberName, object? value)
    where TNode : BaseType;
```

#### Test cases required

| # | Scenario | Expected |
|---|---|---|
| 1 | `WouldBeValid` — valid value | `true`, `message == null` |
| 2 | `WouldBeValid` — invalid value (violates Range) | `false`, `message` contains constraint text |
| 3 | `WouldBeValid` — rejection store unchanged after call | `GetRejectedValues` empty before and after |
| 4 | `WouldBeValid` — no event fired | Subscribe; call; verify event never raised |
| 5 | `TryAssignValue` — valid value | `true`, property updated, `rejection == null` |
| 6 | `TryAssignValue` — invalid value | `false`, property unchanged, `rejection` populated |
| 7 | `TryAssignValue` — clears stale rejection before attempt | Prior failed value not returned as new rejection |
| 8 | `TryAssignValue` — valid after prior invalid clears rejection store | `GetRejectedValues` empty after success |
| 9 | `TryAssignValue` — event fires on failure | Subscribe; call with bad value; verify event raised |
| 10 | `TryAssignValue` — event not fired on success | Subscribe; call with good value; verify event not raised |
| 11 | Chained expression `d => d.Child.val` | `ArgumentException` |
| 12 | Method call expression `d => d.GetVal()` | `ArgumentException` |
| 13 | Type mismatch via reflection | Soft-reject `SdcRejectedValue`, no throw |
| 14 | `WouldBeValid` with constraint override in `SdcValidationRuleRegistry` | Registry rule is honored |
| 15 | `TryAssignValue` — coherence check fires for constraint setter | `CheckConstraintCoherence` runs; incoherent constraint rejected |
| 16 | `WouldBeValid` on constraint property (not val) | Works identically — not restricted to val |
| 17 | `SuppressValidation = true` during `TryAssignValue` | Assignment rejected silently (no event); `rejection` still populated |

#### Documentation artifact

A `.md` file (`SDC.Schema.Tests/Documentation/ValidationScenarios.md`) covering all validation scenarios end-to-end:
- Direct assignment (`dt.val = x`)
- `TryAssignValue` / `WouldBeValid`
- `ValidationOccurred` event subscription
- `GetRejectedValues` query
- `ValidationCollector` batch sweep
- Serializer paths (normal + validating-deserialize)
- `SuppressValidation` and `IsDeserializing` scoping

---

### PHASE 10 — COMPLETE val/CONSTRAINT SWEEP COVERAGE
**Branch:** `Features/NET10/ValConstraintSweep` (to be created from Net10Main after Ph9 merge)
**Status:** 📋 PLANNED
**Baseline:** 656 tests passing (post-Phase 9)

#### Three gaps to fix

**Gap 1 — Integer-family Stype val setters missing CheckValAgainstConstraints**

`integer`, `negativeInteger`, `nonNegativeInteger`, `nonPositiveInteger`, `positiveInteger` have DEtype constraint properties (minInclusive, maxInclusive, minExclusive, maxExclusive) but their Stype val setters do NOT call `CheckValAgainstConstraints`. `ValidateAndRaise` still rejects structurally invalid values (e.g. -1 for positiveInteger via `[Range]`), but cross-property checks (val vs. runtime minInclusive/maxInclusive settings) are missing.

Files needed (all in `Constructor Commented Out Only/AddedValidationLogic/`):
- `integer_Stype.cs`, `negativeInteger_Stype.cs`, `nonNegativeInteger_Stype.cs`, `nonPositiveInteger_Stype.cs`, `positiveInteger_Stype.cs`

Pattern: copy the existing `int_Stype.cs` AddedValidationLogic file as a template — it has the correct `!IsDeserializing.Value` guard.

**Gap 2 — Duration/TimeSpan Stype val setters missing CheckValAgainstConstraints**

`duration_Stype`, `dayTimeDuration_Stype`, `yearMonthDuration_Stype` similarly lack CheckValAgainstConstraints. Their DEtypes have minInclusive/maxInclusive/minExclusive/maxExclusive properties (using TimeSpan or string values).

Files needed (in `SDC TimeSpan string Datatypes/AddedValidationLogic/`):
- `duration_Stype.cs`, `dayTimeDuration_Stype.cs`, `yearMonthDuration_Stype.cs`

Note: verify that `SdcValidate.CheckValAgainstConstraints` handles TimeSpan/string constraint types correctly before relying on it for these types. May need extension of the coherence checker.

**Gap 3 — `ValidateTree()` does not call `CheckValAgainstConstraints`**

`SdcValidate.ValidateTree()` currently only runs `Validator.TryValidateObject()` (DataAnnotations attributes). Cross-property coherence (val vs. min/maxInclusive) is never checked in a post-deserialization sweep.

Fix: extend `ValidateTree()` to call `CheckValAgainstConstraints` on each node that has a `val` property.  
Key requirement: **post-sweep mode must be purely reportorial** — values are already stored (deserialization set them); violations are recorded in `ValidationCollector` / `SdcValidationReport` but values are NOT discarded. This is different from the live setter behavior where a failed check blocks storage.

#### Why constraint checks are suppressed during deserialization (architecture note)

XML properties arrive in document order (arbitrary). If `val` is deserialized before `minInclusive`, the check runs against `minInclusive=0` (default) — a false pass. If `minInclusive` arrives first, `val` may be falsely rejected. Either way, inline constraint-vs-constraint checking during deserialization is order-dependent and unreliable.

**Correct model:**
1. During deserialization: suppress `CheckValAgainstConstraints` (as now) — all values stored as-is
2. Post-deserialization: caller invokes `ValidateTree()` — sweep calls `CheckValAgainstConstraints` in report-only mode; violations recorded, no values discarded

**Three-outcome model for live editing (already implemented):**
- `val` stored, no rejection → clean success
- `val` stored, coherence rejection recorded (Warning) → stored but flagged
- `val` not stored, rejection recorded (Error) → blocked by DataAnnotations

**Post-sweep mode adds a fourth:** value already stored, coherence issue found → recorded as issue, value left in place.

#### Files to create/modify

| File | Action |
|---|---|
| `Constructor Commented Out Only/AddedValidationLogic/integer_Stype.cs` | New — val setter + CheckValAgainstConstraints |
| `Constructor Commented Out Only/AddedValidationLogic/negativeInteger_Stype.cs` | New |
| `Constructor Commented Out Only/AddedValidationLogic/nonNegativeInteger_Stype.cs` | New |
| `Constructor Commented Out Only/AddedValidationLogic/nonPositiveInteger_Stype.cs` | New |
| `Constructor Commented Out Only/AddedValidationLogic/positiveInteger_Stype.cs` | New |
| `SDC TimeSpan string Datatypes/AddedValidationLogic/duration_Stype.cs` | New |
| `SDC TimeSpan string Datatypes/AddedValidationLogic/dayTimeDuration_Stype.cs` | New |
| `SDC TimeSpan string Datatypes/AddedValidationLogic/yearMonthDuration_Stype.cs` | New |
| `SDC.Schema/SDC.Schema/SDC.Schema.csproj` | Update — exclude originals, include AddedValidationLogic copies |
| `SDC.Schema/Utility Classes/SdcValidate.cs` (or SdcValidate.Coherence.cs) | Extend ValidateTree() to call CheckValAgainstConstraints in report-only mode |
| `SDC.Schema.Tests/Validation/SdcPhase10Tests.cs` | New — tests for all gaps |

#### Test cases required

1. `integer`/`negativeInteger`/`nonNeg`/`nonPos`/`positive` val setter calls CheckValAgainstConstraints when minInclusive/maxInclusive is set
2. Setting val outside runtime constraints is soft-rejected with correct message
3. Duration val setter calls CheckValAgainstConstraints (all 3 duration types)
4. `ValidateTree()` post-deserialization sweep reports val/constraint violations without modifying stored values
5. `ValidateTree()` sweep populates `ValidationCollector` correctly
6. `ValidateTree()` sweep fires `ValidationOccurred` events for each violation (when SuppressValidation=false)
7. `ValidateTree()` sweep respects `SuppressValidation=true` (no events, issues still recorded in collector)
8. Deserialized tree with invalid val/constraint combo: values preserved, `ValidateTree()` reports issues
9. Duration type: verify `CheckValAgainstConstraints` handles TimeSpan constraints correctly

---

## Branches

| Branch | Phases | Status | Notes |
|---|---|---|---|
| `Features/NET10/IDataHelpers_Refactor` | 1+2 | ✅ merged | Mechanical wiring |
| `Features/NET10/CoherenceValidator` | 3 | ✅ merged | Design judgment |
| `Features/NET10/SetterCompletion` | 4 | ✅ merged | Repetitive; policy-sensitive |
| `Features/NET10/TodoStubs` | 5 | ✅ merged | Independent |
| `Features/NET10/IDataHelpers_Docs` | 6 | ✅ merged | Docs only |
| `Features/NET10/IntegerDEtypeCoherence` | 7 | ✅ merged | Gap fix |
| `Features/NET10/ValidationPatternNormalize` | 8-A | ✅ merged | Correctness bugs |
| `Features/NET10/ValidationDocs` | 8-B | ✅ merged (regression fixed) | Docs |
| `Features/NET10/TryAssignValue` | 9 | ✅ complete, pending merge | New API |
| `Features/NET10/ValConstraintSweep` | 10 | 📋 planned | Gaps + ValidateTree |

---

## Kickstart Prompt (for new sessions resuming this work)

```
This session continues SDC validation unification work on SDC.Schema
(rmoldwin/SDC_ObjectModel). Read the full plan before doing anything:
  SDC.Schema.Tests/Documentation/ValidationUnificationPlan.md  (in the repo)

CURRENT STATE (as of session end 2026-06-25):
- Net10Main = commit e70e0d4, 656 tests passing
- Phase 9 branch Features/NET10/TryAssignValue (commit cf8bd0c) is MERGED to Net10Main
- Phase 10 is next — branch Features/NET10/ValConstraintSweep (not yet created)

PHASE 10 GOALS (3 gaps):
1. Integer-family Stype AddedValidationLogic (5 files): integer, negativeInteger,
   nonNegativeInteger, nonPositiveInteger, positiveInteger — add CheckValAgainstConstraints
   to val setters. Template: existing int_Stype.cs in AddedValidationLogic.
2. Duration/TimeSpan Stype AddedValidationLogic (3 files): duration, dayTimeDuration,
   yearMonthDuration — same treatment. Verify CheckValAgainstConstraints handles
   TimeSpan/string constraint types before relying on it.
3. Extend ValidateTree() to call CheckValAgainstConstraints in POST-SWEEP (report-only)
   mode — violations recorded in ValidationCollector/SdcValidationReport, values NOT
   discarded (they are already stored). This enables post-deserialization auditing.

KEY ARCHITECTURE:
- CheckValAgainstConstraints is SUPPRESSED during deserialization (!IsDeserializing guard)
  because XML properties arrive in document order — val may arrive before constraints,
  making inline checking order-dependent and unreliable. Post-sweep is the right model.
- TryAssignValue success detection uses value-was-stored comparison (pi.GetValue), NOT
  rejection store — because CheckConstraintCoherence records rejections even on successfully
  stored values (coherence Warning ≠ DataAnnotations Error).
- AddedValidationLogic pattern: copy original to subfolder, comment out original logic,
  add new logic; exclude originals in .csproj, include copies.
- All 4 AddedValidationLogic subfolders:
    SDC Unmodified Classes/AddedValidationLogic/
    SDC Constructor Removed/Constructor Commented Out Only/AddedValidationLogic/
    SDC Constructor Removed/DateTimeOffset Datatypes and Constructor/AddedValidationLogic/
    SDC Constructor Removed/SDC TimeSpan string Datatypes/AddedValidationLogic/

VALIDATION CALL CHAIN (reference):
  dt.val = x  →  val setter  →  SdcUtil.ValidateAndRaise()  →  RaiseAndRecord()
                              →  [if !IsDeserializing] SdcValidate.CheckValAgainstConstraints()
  dt.minInclusive = x  →  constraint setter  →  SdcUtil.ValidateAndRaise()
                                              →  SdcValidate.CheckConstraintCoherence()

IMPORTANT AGENT INSTRUCTIONS:
- When proposing to fix a set of files/gaps, LIST ALL of them explicitly. Do not silently
  drop items from a proposal. If you identify N gaps, all N must appear in the plan.
- After any phase claiming to fix a set of files, run an exhaustive per-setter grep audit
  before marking the phase complete.
- No test may run >1 second (unit) or >10 seconds (integration). Abort and report on timeout.
- Branch naming: Features/NET10/PascalCase — NEVER use rename_branch tool (forces kebab).
  Use: git checkout -b Features/NET10/BranchName origin/Features/NET10/Net10Main
- Always push to origin/Features/NET10/Net10Main via:
  git push origin HEAD:Features/NET10/Net10Main

REPO LAYOUT (key files):
- SDC.Schema/Utility Classes/SdcDataTypeBuilder.cs — factory, all type dispatch
- SDC.Schema/Utility Classes/SdcValidate.Coherence.cs — CheckValAgainstConstraints, CheckConstraintCoherence
- SDC.Schema/Utility Classes/SdcUtil.cs — ValidateAndRaise, ValidateLexicalAndRaise, RaiseAndRecord,
    SuppressValidation, IsDeserializing, ValidationCollector, RecordRejectedValue, GetRejectedValues
- SDC.Schema/Utility Classes/SdcValidationEvents.cs — ValidationOccurred event hub
- SDC.Schema/Utility Classes/Extensions/SdcValidationExtensions.cs — WouldBeValid, TryAssignValue
- SDC.Schema/Interfaces/IDataHelpers.cs — [Obsolete] shim only; use SdcDataTypeBuilder directly
- SDC.Schema/SDC.Schema/SDC.Schema.csproj — excludes originals, includes AddedValidationLogic copies
```
