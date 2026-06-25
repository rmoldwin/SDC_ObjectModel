# Archived Plans — Superseded Documents

This folder holds planning and design documents from prior sessions that have been superseded
by refactoring work or by later, more accurate documents. They are retained for historical/
provenance reasons only. **Do not use them to drive implementation.**

---

## Section 1 — Thread-Safety Plans (superseded by canonical docs in parent folder)

These thread-safety planning documents were produced by **earlier, weaker model passes** and
have been superseded by the current, canonical document set in the parent folder. They contain
claims that were later corrected (most notably the early "`TreeLock` is dead infrastructure,
delete it" assertion, which is **false**: `TreeLock` is LIVE in `CompareTrees.cs`).

### Canonical thread-safety documents — use these instead

| File (parent folder) | Role |
|------|------|
| `ThreadSafety_SessionSummary_AND_Kickstart.md` | Restart entry point + kickstart prompt (START HERE) |
| `ThreadSafety_RemediationPlan_OptionC.md` | Locked Option C implementation spec (TS-1…TS-7 edit map) |
| `ThreadSafety_RootCauseDiagnosis.md` | Evidence record (TS-1…TS-7, reader/writer map, repro results) |
| `ThreadSafety_SessionHandoff.md` | Supporting resume/handoff reference |
| `ThreadSafety_StrategyDecision.md` | Origin of the Option C (`ReaderWriterLockSlim`) decision — still cited as accurate |
| `ThreadSafety_LockingStrategy_Analysis.md` | Locking deep-dive — kept active as a helpful overview for future work |

### Archived in this folder (thread-safety, superseded)

| File | Why archived |
|------|--------------|
| `ThreadSafetyAnalysis.md` | Early broad analysis; superseded by `ThreadSafety_RootCauseDiagnosis.md`. |
| `ThreadSafety_ArchitecturalAnalysis.md` | Early architectural pass; folded into the diagnosis + Option C plan. |
| `ThreadSafety_AuditChecklist.md` | Early API audit checklist; superseded by the §4f lock table in the Option C plan. |
| `ThreadSafety_Phase1_ActionPlan.md` | Early phased plan; superseded by the Option C plan §6 sequencing. |
| `ThreadSafety_Phase1_BLOCKED_STATUS.md` | Point-in-time "blocked by file lock" status; the `TreeLock` "public" blocker is resolved. |
| `ThreadSafety_Phase1_Task1.1_Summary.md` | Early task summary; superseded by the locked plan. |

> **Note on `RC-#` labels:** The archived documents use `RC-1`…`RC-7` (intended as "Root Cause").
> In this project **RC means "Release Candidate"**, so that label was incorrect. The canonical IDs
> are `TS-1`…`TS-7` ("Thread-Safety"). The archived docs have not been edited; treat any `RC-#`
> found inside them as the equivalent `TS-#`.

---

## Section 2 — IDataHelpers Refactor Plans (superseded by SdcDataTypeBuilder)

These documents describe the **pre-refactor architecture** where `IDataHelpers.AddDataTypesDE`
held the parsing and validation logic directly. That logic has since been extracted to the
`SdcDataTypeBuilder` internal static class (Phases 1–5). `IDataHelpers` is now a thin
`[Obsolete]` shim that delegates to `SdcDataTypeBuilder`.

### Canonical IDataHelpers/SdcDataTypeBuilder documents — use these instead

| File (parent folder) | Role |
|------|------|
| `Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md` | Current overall session state and next steps |
| `NumericRange_XSD_vs_NET.md` | XSD vs .NET numeric range reference (still current) |
| `DateTimeValidation_XSD_vs_NET.md` | XSD vs .NET date/time reference (still current) |

### Archived in this folder (IDataHelpers, superseded)

| File | Why archived |
|------|--------------|
| `DateTimeValidation_Plan.md` | Phase 1–3 plan to fix `IDataHelpers.AddDataTypesDE`; all bugs fixed, logic moved to `SdcDataTypeBuilder`. |
| `Kickstart_DateTimeValidation.md` | Old session kickstart for date/time validation work; work completed and merged. |
| `Session_Handoff_ValidationPlan.md` | Phase 1 handoff describing `IDataHelpers` bugs and fix plan; all phases now complete. |
