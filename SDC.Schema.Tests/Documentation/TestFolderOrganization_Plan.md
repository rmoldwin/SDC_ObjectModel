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

- [x] **Step 1** — Create branch `Refactor/TestFolderOrganization` *(done — ce36f6e / d685066)*
- [x] **Step 2** — Create this plan document *(done — d685066)*
- [x] **Step 3** — Phase 1: top-level folder renames via `git mv` *(done — cb93e47)*
- [x] **Step 4** — Phase 2: create new subfolders and move files via `git mv` *(done — b40fd43)*
- [x] **Step 5** — Phase 3: file fixups (DefaultValueAttributeOverride, XML typo, HexConversions deleted) *(done — c3568e9)*
- [x] **Step 6** — Phase 4: update all namespace declarations in moved files *(done — 4e24d69)*
- [x] **Step 7** — Phase 5: clean up csproj `<Compile Remove>` entries *(done — 449d4e1)*
- [x] **Step 8** — Build errors resolved: AttributeInfo→AttrMetadata rename, stale usings fixed *(done — b1d6c9e)*
- [x] **Step 9** — All 411 tests pass; stubs implemented; naming rules enforced *(done — b7837a6)*

---

## Stub Tests Remaining

**None.** All stubs have been implemented and all `_` prefixes removed from both methods and filenames.

| Previously stubbed | Final status |
|---|---|
| `_MoveListDIinList` through `_MoveSectionToNewChildItems` (6 methods) | Implemented — `_` prefix removed |
| `_RefreshSdcSubtreeOMTest` | Implemented via `Move()` + `UpdateNodeIdentity` pattern |
| `_CloneSdcSubtreeBsonTest` | Implemented — documents known broken BSON round-trip; asserts expected exception type |
| `_CloneSdcSubtreeMpackTest` | Implemented — full MsgPack round-trip with node count + identity assertions |
| `MoveTests.cs` file prefix | Removed — file renamed from `_MoveTests.cs` to `MoveTests.cs` |

---

## Risks

- `HexConversionsTests.cs` was excluded from build for an unknown reason — inspect before re-including.
- `DefaultValueAttributeOverride.cs` is a helper class (no `[TestMethod]`), but any test file that uses it must still compile after the namespace change.
- CI filter strings that reference old namespaces (e.g., `SDC.Schema.Tests.OMTests`) would need updating — no CI config found, so low risk.
