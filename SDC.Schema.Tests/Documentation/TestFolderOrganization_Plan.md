# SDC.Schema.Tests — Folder Organization Refactor Plan

**Branch:** `Refactor/TestFolderOrganization`  
**Status:** In Progress

---

## Goals

1. All folder names: PascalCase, no spaces, no redundant suffixes.
2. Tests grouped by semantic domain, not by historical accident.
3. Namespaces match folder paths exactly.
4. All stale `<Compile Remove>` entries in the csproj removed.
5. All remaining stub test methods (`_`-prefixed) receive real implementations; `_` prefixes removed when complete.

---

## Proposed Final Structure

```
SDC.Schema.Tests/
├── Setup.cs                                       (namespace: SDC.Schema.Tests — unchanged)
├── Helpers/
│   ├── TreeValidationHelper.cs                    (unchanged)
│   └── DefaultValueAttributeOverride.cs           (MOVED from UtilityClasses\; namespace → SDC.Schema.Tests.Helpers)
├── Documentation/                                 (unchanged)
├── Test Files/                                    (XML fixtures — unchanged)
│
├── OM/                                            (RENAMED from OMTests\)
│   ├── ActionsTypeTests.cs
│   ├── BaseTypeTests.cs
│   ├── DisplayedTypeTests.cs
│   ├── ListItemTypeTests.cs
│   ├── QuestionItemTypeTests.cs
│   ├── SectionItemTypeTests.cs
│   └── ThreadSafety/                              (NEW subfolder)
│       ├── BaseTypeThreadSafetyTests.cs
│       └── ThreadSafetyReproTests.cs
│
├── TopNode/                                       (unchanged)
│   ├── FormDesignTypeTests.cs
│   ├── MappingTypeExtensionTests.cs
│   ├── PackageListTypeExtensionTests.cs
│   └── XMLPackageTypeExtensionTests.cs            (TYPO FIX: Packaget → Package)
│
├── Functional/
│   ├── FormDesignBuilderTests.cs                  (unchanged)
│   ├── MiscTests.cs                               (unchanged)
│   ├── SdcUtilTests.cs                            (unchanged)
│   ├── ValidationTests.cs                         (unchanged)
│   ├── Serialization/                             (NEW subfolder)
│   │   ├── FormDesignSerializerUtilTests.cs
│   │   ├── SdcSerializationTests.cs
│   │   └── SerializationTests.cs
│   ├── TreeOperations/                            (NEW subfolder)
│   │   ├── ChangeTypeTests.cs
│   │   └── MoveTests.cs
│   └── TreeStability/                             (NEW subfolder)
│       ├── OMTreeStabilityDiagnosticTests.cs
│       └── OMTreeStabilityTests.cs
│
└── UtilityClasses/                                (RENAMED from "Utility Classes\" — removes spaces)
	├── HexConversionsTests.cs                     (FIX namespace + re-include in build)
	├── PropertyInfoOrderedComparerTests.cs
	├── AnyAttr/
	│   └── AnyAttrScenarioTests.cs
	├── AttributeInfo/                             (RENAMED from "Attribute and PI Structs and Methods\")
	│   └── CompareTreesTests.cs
	└── Extensions/
		├── BaseTypeExtensions_ForManualReview.cs
		├── BaseTypeExtensionsTests.cs
		├── INavigateExtensionsTests.cs
		└── ITopNodeExtensionsTests.cs
```

---

## Namespace Mapping

| Old namespace | New namespace |
|---|---|
| `SDC.Schema.Tests.OMTests` | `SDC.Schema.Tests.OM` |
| `SDC.Schema.Tests.OMTests` (ThreadSafety files) | `SDC.Schema.Tests.OM.ThreadSafety` |
| `SDC.Schema.Tests.Functional` (Serialization files) | `SDC.Schema.Tests.Functional.Serialization` |
| `SDC.Schema.Tests.Functional` (TreeOperations files) | `SDC.Schema.Tests.Functional.TreeOperations` |
| `SDC.Schema.Tests.Functional` (TreeStability files) | `SDC.Schema.Tests.Functional.TreeStability` |
| `SDCObjectModelTests.UtilityClasses` | `SDC.Schema.Tests.UtilityClasses` |
| `SDC.Schema.Tests.Utils` | `SDC.Schema.Tests.UtilityClasses` |
| `SDC.Schema.Tests.Utils.Extensions` | `SDC.Schema.Tests.UtilityClasses.Extensions` |
| `SDC.Schema.Tests.UtilityClasses.AnyAttr` | `SDC.Schema.Tests.UtilityClasses.AnyAttr` *(no change)* |
| `SDC.Schema.Tests.Utils` (CompareTreesTests) | `SDC.Schema.Tests.UtilityClasses.AttributeInfo` |
| `SDC.Schema.Tests.Utils` (DefaultValueAttributeOverride) | `SDC.Schema.Tests.Helpers` |

---

## Phases / Checklist

- [ ] **Step 1** — Create branch `Refactor/TestFolderOrganization` *(done)*
- [ ] **Step 2** — Create this plan document *(in progress)*
- [ ] **Step 3** — Phase 1: top-level folder renames via `git mv`
- [ ] **Step 4** — Phase 2: create new subfolders and move files via `git mv`
- [ ] **Step 5** — Phase 3: file fixups (DefaultValueAttributeOverride, XML typo, HexConversions namespace)
- [ ] **Step 6** — Phase 4: update all namespace declarations in moved files
- [ ] **Step 7** — Phase 5: clean up csproj `<Compile Remove>` entries
- [ ] **Step 8** — Build; verify zero errors
- [ ] **Step 9** — Run all tests; verify no regressions; implement remaining stub tests; commit

---

## Stub Tests Remaining

After the folder reorganization is complete, all `_`-prefixed test methods must receive real implementations.  
Files confirmed to contain stubs (from prior audit):

| File | Stub methods |
|---|---|
| `Functional\MoveTests.cs` | `_MoveListDIinList`, `_MoveListDItoOtherList`, `_MoveListDIQuestionChild`, `_MoveQuestionInChildItems`, `_MoveQuestionToNewChildItems`, `_MoveSectionToNewChildItems`, `_RefreshSdcSubtreeOMTest` |
| *(others to be confirmed during execution)* | |

---

## Risks

- `HexConversionsTests.cs` was excluded from build for an unknown reason — inspect before re-including.
- `DefaultValueAttributeOverride.cs` is a helper class (no `[TestMethod]`), but any test file that uses it must still compile after the namespace change.
- CI filter strings that reference old namespaces (e.g., `SDC.Schema.Tests.OMTests`) would need updating — no CI config found, so low risk.
