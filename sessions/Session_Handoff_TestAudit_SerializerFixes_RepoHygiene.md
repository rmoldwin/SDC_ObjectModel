# Session Handoff — Test Audit, Serializer Fixes, Repo Hygiene

**Date:** 2026-06-16  
**Branch:** `Features/Net11Upgrade`  
**Head commit:** `eb452a8`  
**Parent history:** `Refactor/TestFolderOrganization` → `Features/Net11Upgrade_ThreadSafety_OptionCImpl` → `Features/Net11Upgrade`

---

## Session Overview

This session covered three distinct phases:

1. **Test suite audit and implementation** — exhaustive discovery and remediation of all incomplete/stub test methods in `SDC.Schema.Tests`, including a no-workaround policy for serializer tests.
2. **Serializer fixes and fidelity testing** — corrected BSON/JSON `JsonSerializer` settings; reverted MsgPack to true `Pack`/`Unpack`; added `CompareTrees`-based round-trip fidelity tests; rewrote the BSON/JSON bug report with official xsd2code++ architecture details.
3. **Branch merges and repo hygiene** — merged `Refactor/TestFolderOrganization` → `Features/Net11Upgrade_ThreadSafety_OptionCImpl` → `Features/Net11Upgrade`; audited and removed dead empty directories left by locked-folder merge prompts; applied and documented permanent Git locked-folder prevention settings.

---

## Completed Work

### Phase 1 — Test Suite Audit

| Deliverable | Status |
|---|---|
| Full audit of all stub/empty/trivial tests | ✅ Complete |
| `IncompleteTests_ImplementationPlan.md` created | ✅ |
| Category A (timer-shell stubs) — implemented | ✅ |
| Category B (no assertions) — assertions added | ✅ |
| Category C (trivially-passing) — real assertions added | ✅ |
| Empty stub files deleted (`_MoveTests.cs`, old `Functional\MoveTests.cs`) | ✅ |
| `DeserializeFormDesignFromPath` assertions moved before `FD.ResetRootNode()` | ✅ |

**Testing policy enforced:** No workarounds. If a serializer cannot round-trip, the test must fail and remain failing. No XML tunneling, no skipping, no masking.

### Phase 2 — Serializer Fixes

| Item | Fix Applied |
|---|---|
| `SdcSerializerJson.cs` | Added `TypeNameHandling.All` + `ConstructorHandling.AllowNonPublicDefaultConstructor` |
| `SdcSerializerBson.cs` | Same two settings added to `JsonSerializer` singleton; BSON uses `BsonDataWriter`/`BsonDataReader` (correct — not a bug) |
| `SdcSerializerMsgPack.cs` | Reverted XML-tunnel workaround; restored true `MessagePackSerializer<T>.Pack`/`Unpack` |
| `SdcSerializationTests.cs` | Added `AssertRoundTripFidelity<T>()` (CompareTrees-based) and four format round-trip tests: `XmlRoundTripFidelityTest`, `JsonRoundTripFidelityTest`, `BsonRoundTripFidelityTest`, `MsgPackRoundTripFidelityTest` |
| `MoveTests.cs` comments | Updated RC-# → TS-# terminology; removed stale "currently fails" wording |
| `BsonJsonSerializationBugReport.md` | Fully rewritten: marks JSON/BSON as fixed, MsgPack as honestly failing, adds xsd2code++ architecture notes |
| `.github/copilot-instructions.md` | Added full SDC OM Serializer Architecture section with settings, Base64 BSON, advanced JSON options |

**Known open issues:**
- `BsonRoundTripFidelityTest` — deeper `DataTypes_DEType.DataTypeDE_Item` sets `ParentNode` null during BSON deserialization; test currently fails honestly.
- `MsgPackRoundTripFidelityTest` — `MsgPack.Cli` cannot serialize `XmlElement`/`XmlAttribute` natively; test currently fails honestly.
- `JsonRoundTripFidelityTest` — similar `ParentNode` null issue during JSON deserialization; test currently fails honestly.

### Phase 3 — Branch Merges and Repo Hygiene

| Action | Result |
|---|---|
| `Refactor/TestFolderOrganization` → `Features/Net11Upgrade_ThreadSafety_OptionCImpl` | ✅ Merged (commit `67badd4`) |
| `Features/Net11Upgrade_ThreadSafety_OptionCImpl` → `Features/Net11Upgrade` | ✅ Merged (commit `9965a40`) |
| Dead folder audit after locked-folder merge prompts | ✅ Completed |
| Deleted empty ghost dirs: `SDC.Schema.Tests\Utility Classes\`, `UtilityClasses\AttributeInfo\`, `.github\upgrades\scenarios\newtonsoft-json-migration\`, `Docs\` | ✅ No tracked files lost |
| `GIT_TERMINAL_PROMPT=0` added to PS profile | ✅ Applied (permanent) |
| `core.fscache=true`, `checkout.workers=1` set in repo `.git/config` | ✅ Applied |
| `GitWorkflow_LockedFolderPrevention.md` created | ✅ Committed `eb452a8` |
| `.github/copilot-instructions.md` Git Workflow section updated | ✅ Committed `eb452a8` |

---

## Current Branch State

```
Branch:  Features/Net11Upgrade
HEAD:    eb452a8  docs: document Git locked-folder prevention and post-merge cleanup
Status:  Clean working tree (git status --short = empty)
```

Branches that have been merged into `Features/Net11Upgrade` and can be considered inactive:
- `Refactor/TestFolderOrganization` (at `61b4988`)
- `Features/Net11Upgrade_ThreadSafety_OptionCImpl` (at `67badd4`)

---

## Open / Next Items

### Serializer Round-Trip Failures (must not be hidden)

The following three tests are expected to **fail** until the underlying serializer incompatibilities are resolved. They must remain failing — do not skip, workaround, or suppress.

| Test | Root Cause | Required Resolution |
|---|---|---|
| `JsonRoundTripFidelityTest` | `ParentNode` set to null during JSON deserialization of `DataTypes_DEType.DataTypeDE_Item` | Investigate JSON ctor path for `DataTypes_DEType`; may need `[JsonConstructor]` or a custom converter |
| `BsonRoundTripFidelityTest` | Same `ParentNode` null issue via BSON path | Same fix as JSON |
| `MsgPackRoundTripFidelityTest` | `MsgPack.Cli` cannot serialize `XmlElement`/`XmlAttribute` | Evaluate alternative: custom resolver, skip those fields in MsgPack, or accept permanent failure if MsgPack is incompatible with SDC OM |

### `IncompleteTests_ImplementationPlan.md`

This document still lists the original branch (`Refactor/TestFolderOrganization`) in its header. Its content remains accurate as a historical audit record, but the branch reference should be updated if re-opened.

### `SDC.Schema\Utility Classes\Junk\`

This folder contains 8 Git-tracked source files (copies, old project files, experimental code). It is not included in any active project. If these files are no longer needed, they should be removed via `git rm` and committed. No action was taken this session.

---

## Key Documents

| Document | Purpose |
|---|---|
| `Documentation\IncompleteTests_ImplementationPlan.md` | Full audit of stub/empty tests; plan and completion status |
| `Documentation\BsonJsonSerializationBugReport.md` | Serializer architecture, root causes, fixes, and open failures |
| `Documentation\GitWorkflow_LockedFolderPrevention.md` | Git locked-folder issue: root cause, applied fixes, cleanup script |
| `Documentation\TestFolderOrganization_Plan.md` | Test folder refactor plan and completion status |
| `.github\copilot-instructions.md` | All project rules: serializer architecture, test rules, Git rules, naming conventions |

---

## Kickstart Prompt for Next Session

```
I am resuming work on the SDC.Schema solution.

Branch: Features/Net11Upgrade  
Head:   eb452a8  
Workspace: C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema\

Context documents to read first:
  - SDC.Schema.Tests\Documentation\Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md
  - SDC.Schema.Tests\Documentation\BsonJsonSerializationBugReport.md
  - .github\copilot-instructions.md

Current open issues:
1. Three serializer round-trip tests are currently failing HONESTLY and must remain
   failing until the underlying issues are fixed:
   - JsonRoundTripFidelityTest  (ParentNode null on DataTypes_DEType deserialization)
   - BsonRoundTripFidelityTest  (same root cause)
   - MsgPackRoundTripFidelityTest  (MsgPack.Cli cannot serialize XmlElement/XmlAttribute)

2. SDC.Schema\Utility Classes\Junk\ contains 8 Git-tracked legacy files that may
   be candidates for removal.

Guiding principle: Tests must FAIL when serializers are broken. No workarounds,
no XML tunneling, no skipping. CompareTrees<T> is the fidelity oracle.
```
