# XML Comment Annotation Plan

**Status:** Active  
**Branch:** Refactor/TestFolderOrganization  
**Purpose:** Systematic addition of XML `<summary>` (and `<remarks>` / `<param>` / `<returns>` /
`<cref>` where useful) to all un-annotated public members across the SDC.Schema source tree,
module by module.

---

## Annotation Style (from copilot-instructions.md)

- Use explicit, precise `<summary>` tags for all public members.
- Add `<remarks>` only when the behavior needs further explanation beyond the summary.
- Add `<param>` and `<returns>` for all non-trivial public methods and properties.
- Add `<cref>` links whenever they improve navigation without cluttering the comment.
- For public struct fields and change-summary types: use explicit, precise XML comments.
- Do **not** pad or repeat what the member name already says.
- Match the conciseness level already present in well-annotated files (e.g., `TopNodeSerializer.cs`,
  `SdcUtil.cs`, `CompareTrees.cs`).
- Add a comment near every bug fix explaining the change (existing project rule).

---

## Scope Assessment (files with unannotated public members)

| Priority | Folder | File | Public members | Existing summaries | Gap |
|----------|--------|------|---------------|-------------------|-----|
| 1 | Extensions | `Unimplemented Extensions.cs` | 72 | 0 | 72 |
| 2 | Interfaces | `ITreeBuilderRemote.cs` | 30 | 1 | 29 |
| 3 | Extensions | `ITopNodeExtensions.cs` | 40 | 13 | 27 |
| 4 | Extensions | `ActionsTypeExtensions.cs` | 17 | 0 | 17 |
| 5 | Extensions | `INavigateExtensions.cs` | 17 | 2 | 15 |
| 6 | SDC Serializers | `SdcSerializer.cs` | 14 | 4 | 10 |
| 7 | Extensions | `DisplayedTypeExtensions.cs` | 10 | 0 | 10 |
| 8 | Interfaces | `ITopNodeDeserialize.cs` | 8 | 0 | 8 |
| 9 | Extensions | `ITopNodeSerializeExtensions.cs` | 8 | 1 | 7 |
| 10 | Extensions | `ListItemTypeExtensions.cs` | 6 | 1 | 5 |
| 11 | Extensions | `DataElementTypeExtensions.cs` | 6 | 1 | 5 |
| 12 | Extensions | `IChildItemsParentExtensions.cs` | 9 | 4 | 5 |
| 13 | Extensions | `IAddOrganizationExtension.cs` | 4 | 0 | 4 |
| 14 | IComparer | `TreeComparer.cs` | 5 | 1 | 4 |
| 15 | SDC Serializers | `SdcSerializerBson.cs` | 6 | 2 | 4 |
| 16 | Extensions | `ExtensionBaseTypeExtensions.cs` | 4 | 0 | 4 |
| 17 | SDC Serializers | `SdcSerializerJson.cs` | 6 | 2 | 4 |
| 18 | Extensions | `FormDesignTypeExtensions.cs` | 4 | 1 | 3 |
| 19 | Helpers | `Hex Conversions.cs` | 3 | 0 | 3 |
| 20 | SDC Serializers | `SdcSerializerMsgPack.cs` | 6 | 3 | 3 |
| 21 | IComparer | `SDCsGuidEqualityComparer.cs` | 3 | 1 | 2 |
| 22 | Various | Small gap files (gap=1) | — | — | 1 each |

**Additional improvement targets** (files where existing comments may be outdated or low-quality):

| File | Notes |
|------|-------|
| `CompareTrees.cs` | copilot-instructions.md explicitly asks for a review of older comments |
| `SdcSerializerBson.cs` | Need to annotate the new bug-fix notes and root-cause analysis |
| `SdcSerializerJson.cs` | Same — comments should reflect TypeNameHandling discussion |
| `SdcSerializerMsgPack.cs` | Internal XML-tunnel fix deserves explanation in `<remarks>` |
| `IMoveRemoveExtensions.cs` | Negative gap indicates more summaries than detected public members; review for accuracy |

---

## Phases

Phases are sized to be done in a single focused session (≤15 methods per session).

### Phase 1 — SDC Serializers (highest leverage, directly tied to Bug-1/Bug-2 analysis)
**Files:** `SdcSerializer.cs`, `SdcSerializerBson.cs`, `SdcSerializerJson.cs`, `SdcSerializerMsgPack.cs`  
**Goal:** Every public method has a `<summary>`. Serializer-specific limitations documented in
`<remarks>` (referencing `BsonJsonSerializationBugReport.md`). Bug-fix comments added near
`ConstructorHandling` and future `TypeNameHandling` fix points.  
**Status:** ☐ Not started

### Phase 2 — Interfaces
**Files:** `ITreeBuilderRemote.cs`, `ITopNodeDeserialize.cs`, `IBaseType.cs`, `ITopNode.cs`, `Interfaces.cs`  
**Goal:** All interface members documented. `<cref>` links to implementing classes where
these exist and improve navigation.  
**Status:** ☐ Not started

### Phase 3 — Core Extensions (navigation + move/remove)
**Files:** `ITopNodeExtensions.cs`, `INavigateExtensions.cs`, `IMoveRemoveExtensions.cs`,
`IChildItemsParentExtensions.cs`  
**Goal:** Every public extension method has `<summary>`, `<param>` for non-obvious args,
`<returns>` for non-void. Note thread-safety concerns where relevant.  
**Status:** ☐ Not started

### Phase 4 — Type-specific Extensions
**Files:** `DisplayedTypeExtensions.cs`, `ActionsTypeExtensions.cs`, `ListItemTypeExtensions.cs`,
`DataElementTypeExtensions.cs`, `FormDesignTypeExtensions.cs`, `ExtensionBaseTypeExtensions.cs`  
**Goal:** Full coverage of public members. Cross-reference related extension classes via
`<seealso cref="..."/>` where appropriate.  
**Status:** ☐ Not started

### Phase 5 — Unimplemented Extensions
**File:** `Unimplemented Extensions.cs`  
**Goal:** Every stub/placeholder gets a `<summary>` that states it is not yet implemented and
describes the intended behavior. Where design notes exist in comments, elevate them to XML doc.  
**Status:** ☐ Not started

### Phase 6 — IComparer, Helpers, and Miscellaneous
**Files:** `TreeComparer.cs`, `SDCsGuidEqualityComparer.cs`, `Hex Conversions.cs`,
`SortableObservableCollection.cs`, `SdcValidate.cs`, `SdcSerializedAttComparer.cs`,
`MaxDigitsAttribute.cs`, `IAddOrganizationExtension.cs`, `IAddContactExtensions.cs`,
`IHasActionsNodeExtensions.cs`  
**Goal:** Full gap closure for small-gap files.  
**Status:** ☐ Not started

### Phase 7 — CompareTrees.cs comment review (explicit instruction from copilot-instructions.md)
**File:** `CompareTrees.cs`  
**Goal:** Review existing comments for accuracy, update stale/incorrect docs, apply annotation
style guide. Exclude `DifNodeIET2` (may be deleted soon).  
**Status:** ☐ Not started

---

## Improvement Suggestions for Existing Comments

The following patterns were observed in existing annotations that should be improved during
each phase pass:

1. **Missing `<returns>` on non-void methods** — many extension methods have `<summary>` but
   no `<returns>` tag. Add a concise `<returns>` that describes what is returned and under
   what conditions it may be `null`.

2. **Missing `<param>` on key parameters** — methods like `Move(BaseType target, int index,
   bool deleteEmptyParent, SdcUtil.RefreshMode mode)` document the method but not the params.
   Add `<param name="...">` at least for parameters whose purpose is non-obvious from the name.

3. **`<inheritdoc cref="..."/>` overuse** — some methods chain `<inheritdoc>` from a base
   method that itself is not well documented. Where the inherited doc is vague or absent,
   replace with a concrete `<summary>`.

4. **"TODO" comments in XML doc** — several files contain `<!-- TODO: ... -->` inside doc
   comments. During each phase, convert actionable TODOs into GitHub issues or code comments
   (outside the `///` block) and replace the XML doc entry with accurate current behavior.

5. **Single-sentence summaries that duplicate the method name** — e.g.,
   `/// <summary>Gets the section.</summary>` for `GetSection(...)`. Improve to describe
   what the method searches, what it returns when found/not found, and any preconditions.

---

## Progress Tracking

| Phase | Status | Commit |
|-------|--------|--------|
| 1 — Serializers | ☐ | — |
| 2 — Interfaces | ☐ | — |
| 3 — Core Extensions | ☐ | — |
| 4 — Type Extensions | ☐ | — |
| 5 — Unimplemented Extensions | ☐ | — |
| 6 — IComparer/Helpers/Misc | ☐ | — |
| 7 — CompareTrees review | ☐ | — |

---

*Document created automatically from annotation gap analysis on branch
`Refactor/TestFolderOrganization`.*
