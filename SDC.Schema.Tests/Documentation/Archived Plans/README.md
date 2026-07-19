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

Thread-safety content now lives at **[../../../docs/architecture/thread-safety.md](../../../docs/architecture/thread-safety.md)**,
which synthesizes the final implemented state (not a concatenation of the drafts below).

### Archived in this folder (thread-safety, superseded)

| File | Why archived |
|------|--------------|
| `ThreadSafetyAnalysis.md` | Early broad analysis; superseded by the consolidated chapter. |
| `ThreadSafety_ArchitecturalAnalysis.md` | Early architectural pass; folded into the consolidated chapter. |
| `ThreadSafety_AuditChecklist.md` | Early API audit checklist; superseded by the consolidated chapter. |
| `ThreadSafety_Phase1_ActionPlan.md` | Early phased plan; superseded by the consolidated chapter. |
| `ThreadSafety_Phase1_BLOCKED_STATUS.md` | Point-in-time "blocked by file lock" status; the `TreeLock` "public" blocker is resolved. |
| `ThreadSafety_Phase1_Task1.1_Summary.md` | Early task summary; superseded by the consolidated chapter. |
| `ThreadSafety_RootCauseDiagnosis.md` | Root-cause evidence record (TS-1…TS-7); content merged into the consolidated chapter. |
| `ThreadSafety_RemediationPlan_OptionC.md` | Locked Option C implementation spec; content merged into the consolidated chapter. |
| `ThreadSafety_StrategyDecision.md` | Origin of the Option C decision; content merged into the consolidated chapter. |
| `ThreadSafety_LockingStrategy_Analysis.md` | **Recommended `SemaphoreSlim`, not the strategy actually implemented** (the project shipped `ReaderWriterLockSlim`, "Option C"). Previously mislabeled "kept active" in this README — that was wrong; this document's core recommendation was never adopted. Archived as a rejected alternative, not as guidance. |

Session handoff/kickstart documents for thread-safety work (`ThreadSafety_SessionHandoff.md`,
`ThreadSafety_SessionSummary_AND_Kickstart.md`, `ThreadSafety_TS6_Complete_Handoff.md`) were **not**
archived — they were moved to the top-level [`sessions/`](../../../sessions/) folder instead, since
they describe *how and when* the work was done rather than durable architecture.

> **Note on `RC-#` labels:** The archived documents use `RC-1`…`RC-7` (intended as "Root Cause").
> In this project **RC means "Release Candidate"**, so that label was incorrect. The canonical IDs
> are `TS-1`…`TS-7` ("Thread-Safety"). The archived docs have not been edited; treat any `RC-#`
> found inside them as the equivalent `TS-#`.

---

## Section 2 — IDataHelpers Refactor Plans (superseded by SdcDataTypeBuilder)

These documents describe the **pre-refactor architecture** where `IDataHelpers.AddDataTypesDE`
held the parsing and validation logic directly. That logic has since been extracted to the
`SdcDataTypeBuilder` internal static class. `IDataHelpers` is now a thin `[Obsolete]` shim that
delegates to `SdcDataTypeBuilder`. This content, and the completed 10-phase validation-pipeline
unification it planned, is now consolidated in
**[../../../docs/architecture/validation.md](../../../docs/architecture/validation.md)**
(see "Validation pipeline unification").

### Archived in this folder (IDataHelpers, superseded)

| File | Why archived |
|------|--------------|
| `DateTimeValidation_Plan.md` | Phase 1–3 plan to fix `IDataHelpers.AddDataTypesDE`; all bugs fixed, logic moved to `SdcDataTypeBuilder`. |
| `Kickstart_DateTimeValidation.md` | Old session kickstart for date/time validation work; work completed and merged. |
| `Session_Handoff_ValidationPlan.md` | Phase 1 handoff describing `IDataHelpers` bugs and fix plan; all phases now complete. |
| `ValidationUnificationPlan.md` | The full 10-phase unification plan; all phases complete — summarized in `validation.md`. |
| `ValidationScenarios.md` | End-to-end validation entry-point guide; content merged into `validation.md`. |

---

## Section 3 — Type Fidelity & Tree Stability (superseded by consolidated architecture chapters)

| File | Why archived | Canonical replacement |
|------|--------------|------------------------|
| `AnyURI_XSD_vs_NET.md` | `anyURI` vs. `Uri` divergence notes; merged. | [`docs/architecture/xsd-dotnet-type-mapping.md`](../../../docs/architecture/xsd-dotnet-type-mapping.md) |
| `NumericRange_XSD_vs_NET.md` | Numeric range divergence notes; merged. | same |
| `DateTimeValidation_XSD_vs_NET.md` | Date/time-part divergence notes; merged. | same |
| `BsonJsonSerializationBugReport.md` | BSON/JSON/MsgPack bug history + Xsd2Code++ serializer architecture notes; merged. | [`docs/architecture/serialization.md`](../../../docs/architecture/serialization.md) |
| `OM_TreeStability_CurrentState.md` | Superseded snapshot (27/27 tests); final state is 38/38. | [`docs/architecture/tree-stability.md`](../../../docs/architecture/tree-stability.md) |
| `OM_TreeStability_Implementation_Progress.md` | In-progress snapshot; superseded by the final consolidated chapter. | same |

Related session-completion documents (`Session_OMTreeStability_Implementation_Complete.md`,
`Session_MoveReparent_BugFix_Complete.md`) were moved to top-level [`sessions/`](../../../sessions/)
rather than archived here, since they document *when/how* the work was completed, not the
resulting architecture.
