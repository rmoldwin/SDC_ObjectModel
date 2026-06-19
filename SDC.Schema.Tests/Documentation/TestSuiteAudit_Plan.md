# Test Suite Prefix Audit and Consolidation Plan

**Branch:** `Features/Net11Upgrade_ThreadSafety_OptionCImpl`  
**Started:** 2025 (current session)  
**Status:** IN PROGRESS

---

## Rules Being Enforced

1. **`_` prefix on stub methods** — any `[TestMethod]` with no body, or only a trivial lone `Assert.IsNotNull(sut)` where `sut` is just the constructed object with no behavioral check, is a stub and must be prefixed `_`.
2. **`_` prefix on stub files** — any `.cs` test file containing *any* stub method must be prefixed `_`.
3. **Remove `_` when complete** — once a method has real behavioral assertions, remove its `_`. Once all methods in a file are complete, remove the file `_`.
4. **Helper files** → `*_Helper.cs` or `*Helpers.cs` (shared across multiple test files).
5. **Diagnostic / manual-review methods** (output-heavy, intended for human inspection, with or without formal assertions) → file ending `_ForManualReview.cs`. Must still `Assert.Fail` / re-throw on unexpected exceptions.
6. **Old `*Test.cs` files** (without the trailing `s`) alongside newer `*Tests.cs` files: consolidate unique tests into the `*Tests.cs` file, delete the old file.
7. **Empty stub files (0 bytes)** → delete.
8. **Empty stub directories** → delete.
9. **Duplicate test methods** → merge; prefer `INavigateExtensionsTests.cs` over `BaseTypeTests.cs` for navigation coverage.
10. **No test methods may be deleted** — only consolidated into another file.

---

## Files In Scope

| File | Action | Status |
|------|--------|--------|
| `ITopNodeExtensionsTests.cs` | Rename `U_AssignElementNamesFromXmlDocTest`; remove `GetItemByIDTest` + `GetItemByNameTest` duplicates | ✅ Done |
| `BaseTypeExtensionsTests.cs` | Add assertions to 3 stub methods; extract `CompareVersions` + helper to `_ForManualReview.cs`; clean scaffolding types | ✅ Done |
| `BaseTypeTests.cs` | Remove navigation duplicates (keep in `INavigateExtensionsTests.cs`) | ✅ Done |
| `SectionItemTypeTest.cs` | Consolidate unique tests → `SectionItemTypeTests.cs`; delete old file | ✅ Done |
| `ListItemTypeTest.cs` | Consolidate unique tests → `ListItemTypeTests.cs`; delete old file | ✅ Done |
| `QuestionItemTypeTest.cs` | Consolidate unique tests → `QuestionItemTypeTests.cs`; delete old file | ✅ Done |
| `_OMTreeStabilityTests.cs` | git mv → `OMTreeStabilityTests.cs` (remove `_` from filename) | ✅ Done |
| `_FormDesignBuilderTests.cs` | Delete (empty) | ✅ Done |
| `_NavigationTests.cs` | Delete (empty) | ✅ Done |
| `TopNode/_DataElementExtensionTests/` | Delete empty directory | ✅ Done |
| `FormDesignBuilderStubTests.cs` | Rename / consolidate into `FormDesignBuilderTests.cs` | ✅ Done |
| **Build + Test** | Full build; 0 failed, 0 skipped | ✅ Done |
| **Commit** | Single descriptive commit | ✅ Done |

---

## Duplicate / Similar Test Inventory

### Confirmed exact duplicates (one removed):
| Removed | Kept | File |
|---------|------|------|
| `GetItemByIDTest` | `GetIetNodeByIDTest` | `ITopNodeExtensionsTests.cs` |
| `GetItemByNameTest` | `GetNodeByNameTest` | `ITopNodeExtensionsTests.cs` |

### Navigation methods — kept in INavigateExtensionsTests.cs, removed from BaseTypeTests.cs:
`GetNodeFirstSibTest`, `GetNodeLastSibTest`, `GetNodePreviousSibTest`, `GetNodeNextSibTest`,
`GetNodePreviousTest`, `GetNodeNextTest`, `GetNodeFirstChildTest`, `GetNodeLastChildTest`,
`GetNodeLastDescendantTest`, `GetPropertyInfoTest`

### Near-duplicates to watch (NOT removed — different behavioral angle):
| Method A | Method B | Distinction |
|----------|----------|-------------|
| `AssignElementNamesByReflectionTest` | `AssignElementNamesFromXmlDocTest` | former tests programmatically built tree; latter tests deserialized XML tree |
| `SectionItemTypeTest_AddButtonAction` (old) | `AddChildButtonActionTest` (new) | old uses `de.DataElement_Items.Add(si)` manually — kept as unique variation |

---

## Kickstart Prompt

If resuming in a fresh session, paste this:

```
We are auditing and consolidating the SDC.Schema.Tests test suite for correct `_`-prefix stub conventions.
Plan is at: SDC.Schema.Tests/Documentation/TestSuiteAudit_Plan.md
Branch: Features/Net11Upgrade_ThreadSafety_OptionCImpl
Rules:
- Stub method (_-prefix) = [TestMethod] with no body or trivial lone Assert.IsNotNull(sut) only
- Stub file (_-prefix) = any file containing at least one stub method
- Old *Test.cs files consolidate unique tests into *Tests.cs then are deleted
- Empty files and empty directories are deleted
- Diagnostic/output-heavy methods go in *_ForManualReview.cs files
- Shared helpers go in *_Helper.cs files
- Duplicate navigation methods: INavigateExtensionsTests.cs takes precedence over BaseTypeTests.cs
- No test methods may be deleted, only consolidated

Current status: see the Status column in the Files In Scope table above.
Pick up from the first non-✅ row.
```
