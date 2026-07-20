# Sessions

This top-level folder holds session continuity documents for AI-assisted work on the
SDC.Schema solution: handoffs, kickstart prompts, and work summaries meant to let a new session
resume prior work. It is intentionally kept outside `..docs/`, since these documents
describe *how* and *when* work was done rather than the solution's architecture.

## Contents

| File | Topic |
|---|---|
| `Kickstart_OpenIssuesTriage.md` | Kickstart prompt for triaging open GitHub issues |
| `Session_Handoff_NET10_Baseline_NewSession.md` | .NET 10 baseline handoff for a fresh session |
| `Session_Handoff_NET10_Downgrade.md` | Handoff covering a .NET 10 downgrade investigation |
| `Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md` | Handoff covering a test audit, serializer fixes, and repo hygiene pass |
| `Session_MoveReparent_BugFix_Complete.md` | Completion summary for the move/reparent bug fixes — see [../..docs/architecture/tree-stability.md](../..docs/architecture/tree-stability.md) for the resulting architecture |
| `Session_OMTreeStability_Implementation_Complete.md` | Completion summary for the OM tree-stability test suite — see [../..docs/architecture/tree-stability.md](../..docs/architecture/tree-stability.md) |
| `Session_Summary_Complete_ItemMutator_To_Stability.md` | Summary spanning `ItemMutator` work through tree-stability completion |
| `Session_Summary_DateTimeValidation_Complete.md` | Completion summary for date/time validation work — see [../..docs/architecture/xsd-dotnet-type-mapping.md](../..docs/architecture/xsd-dotnet-type-mapping.md) |
| `Session_Summary_OMTreeStability_Setup.md` | Early setup summary for the OM tree-stability test suite |
| `ThreadSafety_SessionHandoff.md` | Thread-safety investigation handoff — see [../..docs/architecture/thread-safety.md](../..docs/architecture/thread-safety.md) for the resulting architecture |
| `ThreadSafety_SessionSummary_AND_Kickstart.md` | Thread-safety session summary + kickstart prompt |
| `ThreadSafety_TS6_Complete_Handoff.md` | Handoff after fixing thread-safety defect TS-6 |
| `XmlAnnotationPlan.md` | Active plan/checklist for adding XML `<summary>` documentation comments across public SDC.Schema members |

These files were moved here (with git history preserved) from
`SDC.Schema.Tests/Documentation/` — see
[`SDC.Schema.Tests/Documentation/Archived Plans/README.md`](../SDC.Schema.Tests/Documentation/Archived%20Plans/README.md)
for the architecture content that was extracted from related documents into `..docs/architecture/`.
