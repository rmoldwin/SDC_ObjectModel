# Session Handoff — .NET 10 Downgrade (NET11 Preview → NET10)

**Date:** 2026-06-19
**Branch:** `Features/NET10/DowngradeNET11PreviewToNET10`
**Head commit:** `6a87dfe`
**Parent history:** `Features/Net11Upgrade` → `Features/NET10/DowngradeNET11PreviewToNET10`
**Next branch:** `Features/NET10/Net10Main`

---

## Session Overview

This session downgraded the entire solution from the .NET 11 preview SDK/TFM to the stable .NET 10 SDK/TFM. It also performed a repo hygiene pass removing all empty and stub placeholder files that had accumulated across previous sessions.

---

## Completed Work

### Phase 1 — .NET 10 Downgrade

| Item | Change |
|---|---|
| `global.json` | Replaced `.NET 11` preview pin (`11.0.100-preview.5.26302.115`, `allowPrerelease: true`, `rollForward: latestFeature`, custom `paths`) with stable .NET 10 pin (`version: 10.0.300`, `rollForward: latestPatch`) |
| `SDC.Schema/SDC.Schema.csproj` | `TargetFramework`: `net11.0` → `net10.0`; `LangVersion`: `14.0` → `13.0` |
| `SDC.Schema.Tests/SDC.Schema.Tests.csproj` | Same TFM and LangVersion changes |
| `Benchmarks/Benchmarks.csproj` | Same TFM and LangVersion changes |
| NuGet packages | No version changes required — all packages already at latest stable, fully compatible with `net10.0` |
| C# 14 language features | None found in source; no code remediation needed |

**Verified:** `dotnet --list-sdks` shows `10.0.300` at `C:\Program Files\dotnet\sdk\10.0.300`. Build successful.

**Commit:** `17f8f67  build: downgrade from net11.0 preview to net10.0`

### Phase 2 — Repo Hygiene: Remove Empty and Stub Files

| File | Reason for removal |
|---|---|
| `SDC.Schema.Tests/OMTests/BaseTypeTests.cs` | Empty placeholder added in the downgrade commit; never populated |
| `SDC.Schema.Tests/OMTests/` (directory) | Deleted from disk after file removal |
| `SDC.Schema/Utility Classes/Extensions/_ActActionTypeExtensions.cs` | Empty stub class (no members, no implementation) |
| `SDC.Schema/Utility Classes/Extensions/_ActSendMessageTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_ActSendReportTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_BlobTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_ButtonItemTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_CallFuncBaseTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_ChildItemsTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_MappingTypeExtensions.cs` | Empty stub class (was also explicitly listed in `SDC.Schema.csproj` as both `<Compile Remove>` and `<None Include>` — both entries removed) |
| `SDC.Schema/Utility Classes/Extensions/_PackageListTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_PredicateGuardTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_RegistrySummaryTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_RetrieveFormPackageTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_RulesTypeExtensions.cs` | Empty stub class |
| `SDC.Schema/Utility Classes/Extensions/_XMLPackageTypeExtensions.cs` | Empty stub class |

**Verified:** Build successful after all removals.

**Commit:** `6a87dfe  chore: remove all empty and stub files`

---

## Current Branch State

```
Branch:  Features/NET10/DowngradeNET11PreviewToNET10
HEAD:    6a87dfe  chore: remove all empty and stub files
Status:  Clean working tree
SDK:     10.0.300 (C:\Program Files\dotnet\sdk\10.0.300)
TFM:     net10.0 (all three projects)
LangVer: 13.0 (all three projects)
```

---

## NuGet Package Inventory (net10.0 baseline)

All packages confirmed latest stable, net10.0-compatible:

| Package | Version | Project(s) |
|---|---|---|
| `BenchmarkDotNet` | 0.15.8 | Benchmarks, SDC.Schema.Tests |
| `Microsoft.NET.Test.Sdk` | 18.6.0 | SDC.Schema.Tests |
| `MSTest.TestAdapter` | 4.2.3 | SDC.Schema.Tests |
| `MSTest.TestFramework` | 4.2.3 | SDC.Schema.Tests |
| `coverlet.collector` | 10.0.1 | SDC.Schema.Tests |
| `SonarAnalyzer.CSharp` | 10.27.0.140913 | SDC.Schema, SDC.Schema.Tests |
| `CommunityToolkit.Diagnostics` | 8.4.2 | SDC.Schema |
| `CSharpVitamins.ShortGuid` | 2.0.0 | SDC.Schema |
| `Newtonsoft.Json` | 13.0.4 | SDC.Schema |
| `Newtonsoft.Json.Bson` | 1.0.3 | SDC.Schema |
| `Newtonsoft.Json.Schema` | 4.0.1 | SDC.Schema |
| `Newtonsoft.Msgpack` | 0.1.11 | SDC.Schema |

---

## Open / Carry-Forward Items

These are pre-existing open issues from earlier sessions, unchanged by this session:

### Serializer Round-Trip Failures (must remain failing — no workarounds)

| Test | Root Cause | Required Resolution |
|---|---|---|
| `JsonRoundTripFidelityTest` | `ParentNode` set to null during JSON deserialization of `DataTypes_DEType.DataTypeDE_Item` | Investigate JSON ctor path for `DataTypes_DEType`; may need `[JsonConstructor]` or custom converter |
| `BsonRoundTripFidelityTest` | Same `ParentNode` null issue via BSON path | Same fix as JSON |
| `MsgPackRoundTripFidelityTest` | `MsgPack.Cli` cannot serialize `XmlElement`/`XmlAttribute` | Evaluate alternative: custom resolver, skip those fields, or accept permanent failure |

### Thread Safety (Option C — pending approval to implement)

Full design is locked in `ThreadSafety_RemediationPlan_OptionC.md`. The WIP safety checkpoint `bbcadca` on `Features/Net11Upgrade_ThreadSafety` carries partial `SemaphoreSlim TreeLock` production infra. The .NET 10 work is independent — these branches have diverged; the thread-safety branch will need rebasing or merging onto the net10 baseline when implementation begins.

See `ThreadSafety_SessionSummary_AND_Kickstart.md` for the complete restart guide and kickstart prompt.

### `SDC.Schema/Utility Classes/Junk/`

Contains 8 Git-tracked legacy source files (copies, old project files, experimental code). Excluded from active compilation. Candidates for removal via `git rm`. No action taken this session.

---

## Key Documents

| Document | Purpose |
|---|---|
| `Documentation\Session_Handoff_NET10_Downgrade.md` | **THIS** — NET10 downgrade session record |
| `Documentation\Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md` | Prior session — test audit, serializer fixes, repo hygiene (branch: `Features/Net11Upgrade`) |
| `Documentation\ThreadSafety_SessionSummary_AND_Kickstart.md` | Thread-safety restart entry point + kickstart prompt |
| `Documentation\ThreadSafety_RemediationPlan_OptionC.md` | Thread-safety Option C implementation spec (TS-1…TS-7) |
| `Documentation\BsonJsonSerializationBugReport.md` | Serializer architecture, root causes, fixes, open failures |
| `Documentation\GitWorkflow_LockedFolderPrevention.md` | Git locked-folder prevention settings |
| `.github\copilot-instructions.md` | All project rules: naming, serializer architecture, test rules, Git rules |

---

## Kickstart Prompt for Next Session

```
I am resuming work on the SDC.Schema solution on the NET10 baseline.

Branch:    Features/NET10/Net10Main
Head:      6a87dfe  (chore: remove all empty and stub files)
Workspace: C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema\
SDK:       10.0.300  (global.json pins net10.0, rollForward: latestPatch)
TFM:       net10.0 — all three projects (SDC.Schema, SDC.Schema.Tests, Benchmarks)
LangVer:   13.0

Read these first:
  1. SDC.Schema.Tests\Documentation\Session_Handoff_NET10_Downgrade.md  (this session — NET10 baseline state)
  2. SDC.Schema.Tests\Documentation\Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md  (prior session)
  3. SDC.Schema.Tests\Documentation\ThreadSafety_SessionSummary_AND_Kickstart.md  (thread-safety restart guide)
  4. .github\copilot-instructions.md  (project rules)

Open issues to address (in rough priority order):
1. Three serializer round-trip tests fail honestly and must remain failing until fixed:
	 - JsonRoundTripFidelityTest   (ParentNode null on DataTypes_DEType deserialization)
	 - BsonRoundTripFidelityTest   (same root cause)
	 - MsgPackRoundTripFidelityTest (MsgPack.Cli cannot serialize XmlElement/XmlAttribute)
2. Thread-safety Option C (TS-1..TS-7): design is fully locked in
   ThreadSafety_RemediationPlan_OptionC.md; implementation not yet started on the net10 baseline.
   The old WIP checkpoint bbcadca is on Features/Net11Upgrade_ThreadSafety (net11 base) and will
   need rebasing onto net10 when implementation begins.
3. SDC.Schema\Utility Classes\Junk\ — 8 tracked legacy files, candidates for git rm.

Guiding principles:
- Tests must FAIL when serializers are broken. No workarounds, no XML tunneling, no skipping.
- CompareTrees<T> is the fidelity oracle.
- TS-# prefix = Thread-Safety defect IDs (TS-1..TS-7). Never use RC-# for these.
- Do NOT merge to main/master and do NOT push without explicit user approval.
- Always switch to an appropriately named new branch at the start of each new dev cycle.
```
