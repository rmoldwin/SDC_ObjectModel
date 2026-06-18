# Archived Plans — Superseded Concurrency/Async Documents

These thread-safety planning documents were produced by **earlier, weaker model passes** and have been **superseded** by the current, canonical document set in the parent folder (`SDC.Schema.Tests\Documentation\`).

They are retained for historical/provenance reasons only. **Do not use them to drive implementation** — they contain claims that were later corrected (most notably the early "`TreeLock` is dead infrastructure, delete it" assertion, which is **false**: `TreeLock` is LIVE in `CompareTrees.cs`).

## Canonical (current) documents — use these instead

| File (parent folder) | Role |
|------|------|
| `ThreadSafety_SessionSummary_AND_Kickstart.md` | Restart entry point + kickstart prompt (START HERE) |
| `ThreadSafety_RemediationPlan_OptionC.md` | Locked Option C implementation spec (TS-1…TS-7 edit map) |
| `ThreadSafety_RootCauseDiagnosis.md` | Evidence record (TS-1…TS-7, reader/writer map, repro results) |
| `ThreadSafety_SessionHandoff.md` | Supporting resume/handoff reference |
| `ThreadSafety_StrategyDecision.md` | Origin of the Option C (`ReaderWriterLockSlim`) decision — still cited as accurate |
| `ThreadSafety_LockingStrategy_Analysis.md` | Locking deep-dive — kept active as a helpful overview for future work |

## Archived in this folder (superseded)

| File | Why archived |
|------|--------------|
| `ThreadSafetyAnalysis.md` | Early broad analysis; superseded by `ThreadSafety_RootCauseDiagnosis.md`. |
| `ThreadSafety_ArchitecturalAnalysis.md` | Early architectural pass; folded into the diagnosis + Option C plan. |
| `ThreadSafety_AuditChecklist.md` | Early API audit checklist; superseded by the §4f lock table in the Option C plan. |
| `ThreadSafety_Phase1_ActionPlan.md` | Early phased plan; superseded by the Option C plan §6 sequencing. |
| `ThreadSafety_Phase1_BLOCKED_STATUS.md` | Point-in-time "blocked by file lock" status; the `TreeLock` "public" blocker is resolved. |
| `ThreadSafety_Phase1_Task1.1_Summary.md` | Early task summary; superseded by the locked plan. |

> If you need the rationale or wording of any decision, prefer the canonical documents above. These archived files may contain **outdated or corrected** statements.

> **Note on `RC-#` labels:** The archived documents use `RC-1`…`RC-7` (intended as "Root Cause"). In this project **RC means "Release Candidate"**, so that label was incorrect. The canonical IDs are `TS-1`…`TS-7` ("Thread-Safety"). The archived docs have not been edited; treat any `RC-#` found inside them as the equivalent `TS-#`.
