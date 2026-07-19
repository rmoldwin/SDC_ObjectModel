# Session Handoff — NET10 Baseline / New Session Kickstart

**Date:** 2026-06-19
**Branch:** `Features/NET10/Net10Main`
**Head commit:** `2a252f0` — docs: add NET10 downgrade session handoff and kickstart
**Workspace:** `C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema\`
**SDK:** 10.0.300 (`global.json` pins `net10.0`, `rollForward: latestPatch`)
**TFM / LangVer:** `net10.0` / `13.0` — all three projects

---

## Branch Lineage

```
master
  └─ Features/Net11Upgrade
	   └─ Features/NET10/DowngradeNET11PreviewToNET10
			└─ Features/NET10/Net10Main  ◄ HEAD
```

`Features/NET10/Net10Main` is the new stable working baseline for all net10.0 development.
`Features/NET10/DowngradeNET11PreviewToNET10` is the completed migration branch (do not develop on it).

---

## What This Session Did

This session was a clean-up and branch-setup session only — no production code was changed.

### Completed

| Action | Commit |
|---|---|
| Downgrade `global.json`: net11 preview SDK pin → `10.0.300`, stable | `17f8f67` |
| All three projects: `net11.0` → `net10.0`, `LangVersion 14.0` → `13.0` | `17f8f67` |
| Remove 15 empty/stub files (`OMTests/BaseTypeTests.cs`, 14 `_*.cs` extension stubs) | `6a87dfe` |
| Remove stale `<Compile Remove>` and `<None Include>` for `_MappingTypeExtensions.cs` | `6a87dfe` |
| Add `Session_Handoff_NET10_Downgrade.md` documentation | `2a252f0` |
| Create branch `Features/NET10/Net10Main` from `DowngradeNET11PreviewToNET10` HEAD | (branch) |

Build is **✅ successful** on `net10.0`.

---

## Current Test Run State (net10.0 baseline)

```
Total: 404   Passed: 362   Failed: 33   Skipped: 9
```

### Failing Tests — Root Cause A: Missing XML Test Fixture Files (pre-existing)

Two fixture files are absent from `SDC.Schema.Tests\Test Files\`. Their absence cascades to 32 test failures:

| Missing File | Tests Affected |
|---|---|
| `ZZZTest_XMLCompTesting.634_1.000.001.REL_sdcFDF.xml` | All 29 `CompareTreesTests` tests (class instantiation fails); `SDC.Schema.Tests.Tests.Test`; `SDC.Schema.Tests.Functional.MiscTests.Test` |
| `WithLinks.xml` | `SDC.Schema.Tests.Functional.FormDesignSerializerUtilTests.SerializeTest` |

These are **pre-existing** failures that existed before the NET10 downgrade. They are not regressions from this session's work. The correct resolution is to supply the missing fixture files, not to modify the tests.

### Failing Test — Root Cause B: Pre-existing Logic Error (pre-existing)

| Test | Class | Error |
|---|---|---|
| `DeserializeFormDesignFromPath` | `SDC.Schema.Tests.Functional.SdcSerializationTests` | `System.InvalidOperationException: A ListItemResponseField object (lirf) already exists. Run the RemoveRecursive() method on that ListItemResponseField object before replacing it` — thrown from `ListItemTypeExtensions.AddListItemResponseField` at line 11 |

This failure is pre-existing. It represents a real logic bug in `SdcSerializationTests.DeserializeFormDesignFromPath` (line 161), which calls `AddListItemResponseField` on a `ListItemType` that already has one. It must remain failing and visible; do not skip or suppress it.

### Skipped Tests (9) — Pre-existing `[Ignore]`

All 9 are in `CompareTreesTests` (`AttInfoDif_*` group):
`AttInfoDif_AttributeChanged_BothExist`, `AttInfoDif_BooleanAttribute_CorrectlyCompared`, `AttInfoDif_DefaultValues_Tracked`, `AttInfoDif_IntegerAttribute_CorrectlyCompared`, `AttInfoDif_MultipleAttributes_SameNode`, `AttInfoDif_MustImplement_AttributeChanged`, `AttInfoDif_PropertyName_Matches`, `AttInfoDif_SguidSubnode_ConsistentAcrossVersions`, `AttInfoDif_Title_AttributeChanged`.

These are pre-existing `[Ignore]`-attributed tests. The appropriate action is to supply the missing fixture file first, then evaluate whether these can be un-ignored.

### Tests from `FormDesignTypeTests.cs` — All Passing ✅

All tests in `SDC.Schema.Tests.TopNode.FormDesignTypeTests` pass on the net10.0 baseline.

---

## Open Items — Carry Forward

### Priority 1 — Restore Missing Test Fixture Files

Provide the two missing files so 32 tests can run:
- `SDC.Schema.Tests\Test Files\ZZZTest_XMLCompTesting.634_1.000.001.REL_sdcFDF.xml`
- `SDC.Schema.Tests\Test Files\WithLinks.xml`

These files may exist outside the repo (OneDrive, a network share, or a colleague's machine). They were likely never committed. Once supplied, add them to the project with `<None Include="..."><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` and commit.

### Priority 2 — Fix `DeserializeFormDesignFromPath` Logic Error

`SdcSerializationTests.cs` line 161 calls `li.AddListItemResponseField()` on a `ListItemType` that already has a `ListItemResponseField` child. The fix is either:
- Call `li.ListItemResponseField?.RemoveRecursive()` before `AddListItemResponseField`, or
- Guard the call with a null check: only add if `li.ListItemResponseField == null`

Do not suppress or skip the test — fix the root cause.

### Priority 3 — Thread Safety Option C (TS-1..TS-7)

The full design is locked in `ThreadSafety_RemediationPlan_OptionC.md`. Implementation has not yet started on the net10.0 baseline. The old WIP safety checkpoint `bbcadca` lives on `Features/Net11Upgrade_ThreadSafety` (a net11 base) and will need rebasing or cherry-picking onto `Features/NET10/Net10Main` before implementation can begin.

See `ThreadSafety_SessionSummary_AND_Kickstart.md` for the complete restart guide. The net10.0 rebase decision (rebase vs. cherry-pick) is the first gate.

### Priority 4 — `SDC.Schema\Utility Classes\Junk\`

Contains 8 Git-tracked legacy source files excluded from active compilation. Candidates for `git rm`. No action taken yet.

### Priority 5 — Serializer Round-Trip Fidelity Tests (Honest Failures)

Three tests are expected to fail honestly until the underlying serializer issues are resolved:

| Test | Root Cause |
|---|---|
| `JsonRoundTripFidelityTest` | `ParentNode` set to null during JSON deserialization of `DataTypes_DEType.DataTypeDE_Item` |
| `BsonRoundTripFidelityTest` | Same `ParentNode` null issue via BSON path |
| `MsgPackRoundTripFidelityTest` | `MsgPack.Cli` cannot serialize `XmlElement`/`XmlAttribute` |

These must remain failing and visible. Do not skip, suppress, or work around them. See `BsonJsonSerializationBugReport.md` for full details.

---

## NuGet Package Inventory (net10.0 baseline)

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

## Test File Inventory

```
SDC.Schema.Tests\
  TopNode\
	FormDesignTypeTests.cs         — FormDesignType construction, header/body/footer, serialization, node lookup
	MappingTypeExtensionTests.cs
	PackageListTypeExtensionTests.cs
	XMLPackageTypeExtensionTests.cs
  OM\
	ActionsTypeTests.cs
	BaseTypeTests.cs
	DisplayedTypeTests.cs
	ListItemTypeTests.cs
	QuestionItemTypeTests.cs
	SectionItemTypeTests.cs
	ThreadSafety\
	  BaseTypeThreadSafetyTests.cs
	  ThreadSafetyReproTests.cs
  Functional\
	FormDesignBuilderTests.cs
	MiscTests.cs
	SdcUtilTests.cs
	ValidationTests.cs
	Serialization\
	  FormDesignSerializerUtilTests.cs
	  SdcSerializationTests.cs
	  SerializationTests.cs
	TreeOperations\
	  ChangeTypeTests.cs
	  MoveTests.cs
	TreeStability\
	  OMTreeStabilityDiagnosticTests.cs
	  OMTreeStabilityTests.cs
  UtilityClasses\
	PropertyInfoOrderedComparerTests.cs
	AnyAttrScenarioTests.cs
	CompareTreesTests.cs
	BaseTypeExtensionsTests.cs
	BaseTypeExtensions_ForManualReview.cs
	INavigateExtensionsTests.cs
	ITopNodeExtensionsTests.cs
  Helpers\
	DefaultValueAttributeOverride.cs
	TreeValidationHelper.cs
  Documentation\   (all .md files)
  Setup.cs
```

---

## Document Index

| Document | Purpose |
|---|---|
| `Documentation\Session_Handoff_NET10_Baseline_NewSession.md` | **THIS** — net10.0 baseline state, test run, open items, kickstart |
| `Documentation\Session_Handoff_NET10_Downgrade.md` | Prior session — downgrade commit details |
| `Documentation\Session_Handoff_TestAudit_SerializerFixes_RepoHygiene.md` | Two sessions prior — test audit, serializer fixes, repo hygiene |
| `Documentation\ThreadSafety_SessionSummary_AND_Kickstart.md` | Thread-safety restart entry point + kickstart prompt (TS-1..TS-7) |
| `Documentation\ThreadSafety_RemediationPlan_OptionC.md` | Thread-safety Option C implementation spec (fully locked) |
| `Documentation\BsonJsonSerializationBugReport.md` | Serializer architecture, root causes, fixes, open fidelity failures |
| `Documentation\GitWorkflow_LockedFolderPrevention.md` | Git locked-folder prevention settings |
| `.github\copilot-instructions.md` | All project rules: naming, serializer architecture, test rules, Git rules |

---

## Kickstart Prompt for Next Session

```
I am resuming work on the SDC.Schema solution.

Branch:    Features/NET10/Net10Main
Head:      2a252f0  (docs: add NET10 downgrade session handoff and kickstart)
Workspace: C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema\
SDK:       10.0.300  (global.json — net10.0, rollForward: latestPatch)
TFM:       net10.0 — all three projects (SDC.Schema, SDC.Schema.Tests, Benchmarks)
LangVer:   13.0

Read these first (in order):
  1. SDC.Schema.Tests\Documentation\Session_Handoff_NET10_Baseline_NewSession.md  ← START HERE
  2. SDC.Schema.Tests\Documentation\ThreadSafety_SessionSummary_AND_Kickstart.md
  3. .github\copilot-instructions.md

Confirmed test baseline (run: dotnet test SDC.Schema.Tests\SDC.Schema.Tests.csproj --no-build -c Debug):
  Total: 404  Passed: 362  Failed: 33  Skipped: 9

All 33 failures and 9 skips are PRE-EXISTING (not regressions from the net10 downgrade).
Root causes:
  A) 32 failures — two missing XML fixture files:
	   SDC.Schema.Tests\Test Files\ZZZTest_XMLCompTesting.634_1.000.001.REL_sdcFDF.xml
	   SDC.Schema.Tests\Test Files\WithLinks.xml
	 These files were never committed. Obtain them and add to the project.
  B) 1 failure — DeserializeFormDesignFromPath (SdcSerializationTests.cs:161):
	   InvalidOperationException: A ListItemResponseField already exists on the target ListItemType.
	   Fix: guard AddListItemResponseField with a null check or call RemoveRecursive() first.
  C) 9 skips — CompareTreesTests AttInfoDif_* group, all [Ignore]-attributed.

Open work (suggested priority):
  1. Restore the two missing fixture files → unblock 32 tests.
  2. Fix DeserializeFormDesignFromPath → eliminate the last pre-existing logic failure.
  3. Thread safety Option C (TS-1..TS-7): design fully locked; needs rebase onto net10 baseline
	 before implementation. See ThreadSafety_SessionSummary_AND_Kickstart.md.
  4. SDC.Schema\Utility Classes\Junk\ — 8 tracked legacy files, candidates for git rm.
  5. Three honest serializer round-trip failures (Json/Bson/MsgPack fidelity tests) —
	 must remain failing; fix requires deeper serializer work per BsonJsonSerializationBugReport.md.

Guiding principles (non-negotiable):
  - Tests must FAIL when serializers or logic are broken. No workarounds, no skipping.
  - CompareTrees<T> is the fidelity oracle for round-trip tests.
  - TS-# = Thread-Safety defect IDs (TS-1..TS-7). Never use RC-# for these.
  - Always switch to an appropriately named new branch at the start of each new dev cycle.
  - Do NOT merge to main/master and do NOT push without explicit user approval.
  - When doing a git amend, always review and adjust the commit message before amending.
  - Branch/folder naming: PascalCase, no dashes, underscores only to prevent run-ons;
	preserve ALL-CAPS abbreviations (IET, SDC, XML, etc.).
```
