# Tree Operations: Copy, Paste, Graft, Move, Clone-and-Repeat, and Inject

This chapter documents the Structured Data Capture (SDC) Object Model (OM)'s subtree
manipulation operations: moving a node within a tree, copying/pasting or "grafting" a node
between trees, cloning a repeating subtree (e.g. duplicating a `Section`/`Question` block),
and injecting a subtree from elsewhere in the same tree or a different tree entirely
(`InjectForm`-style use cases). It also documents the response-value-stripping rules that
keep repeated/injected copies free of user-entered data by default.

See [glossary.md](../glossary.md) for every initialism used below (Form Design File, or FDF;
Form Design File - Response, or FDF-R; Globally Unique Identifier, or GUID; and so on).

## Why this matters

Every one of these operations must preserve two invariants that the rest of the object model
relies on:

1. **No duplicate `@ID`/`@name` values anywhere in one `TopNode` tree.** SDC forbids duplicate
   `ID`/`name` content within a single document instance. Any operation that introduces a node
   already present in the tree (a repeat, an injected copy) must rename it.
2. **No duplicate `ObjectGUID`/`sGuid` values.** Every node produced by cloning gets a brand-new
   identity, never a copy of the source node's identity.

## Core operations

| Operation | Method | Typical use |
|---|---|---|
| Move within the same tree | `Move()` (`IMoveRemoveExtensions.cs`), `RefreshMode.NoChange` | Drag-and-drop reordering; no identity change. |
| Copy/paste between trees, or into an empty single-node slot | `CopyPaste()`, `Graft()` | Form-authoring: bring an existing node into a target tree/slot. `CopyPaste()` clones the donor and leaves it untouched; `Graft()` moves the original node, removing it from its old parent. |
| Repeat an existing subtree (e.g. duplicate a `Section`) | `Copy()` → `RefreshMode.CloneAndRepeatSubtree` | Repeating nodes — the classic "add another instance of this repeating block" feature. |
| Inject a subtree from anywhere (same tree or a different tree) at an arbitrary position | `InjectSubtree()` | General-purpose cross-tree/same-tree injection, e.g. `InjectForm`-style composition. |
| Inject a subtree while guaranteeing it is response-free | `InjectSubtreeFromTemplate()` | Same as `InjectSubtree()`, but clones from the donor's own source FDF template instead of the live instance, so there is nothing to strip. |

### `Move()` and the null-slot fix

`IMoveRemoveExtensions.Move()` has an inline attach/detach code path used by both `CopyPaste()`
and `Graft()` (the `RefreshMode.UpdateNodeIdentity` branch) and a separate `MoveSingleNode()`
local function used for plain same-tree moves (`RefreshMode.NoChange`). Both independently
compute whether the target slot is a **single-node property slot** (as opposed to a list
slot), including the case where that slot is currently `null` (empty):

```csharp
targetIsSingleNodeSlot = targetPropertyObject is BaseType
    || (targetPropertyObject is null && ... PropertyType is assignable from BaseType);
```

Without the `is null && PropertyType ...` half of that check, attaching into a currently-empty
single-node slot (e.g. a `QuestionItemType.ListField_Item` that has never been populated) would
be mis-classified as "not a single-node slot," causing the attach path to fail. Regression
coverage: `CopyPasteGraftTests.cs` (`CopyPaste_IntoEmptySingleNodeSlot_AttachesSuccessfully`,
`Graft_IntoEmptySingleNodeSlot_AttachesAndRemovesFromOriginalParent`).

`QuestionItemType.ListField_Item` (type `ListFieldType`) is deliberately used as the test target
rather than `FormDesignType.Header`/`.Body`/`.Footer`, because the latter three all share the
identical declared type (`SectionItemType`) with no discriminating metadata, making
`SdcUtil.IsAttachNodeAllowed`'s type-based slot inference genuinely ambiguous among them — a
pre-existing schema characteristic, not something this fix changes.

### `CopyPaste()` deep-clone correctness

`CopyPaste()` must deep-clone an entire subtree (not just its root node) with fresh identity at
every level: new `ObjectGUID`/`sGuid`/`ID`/`name` for the root and every descendant, and the
clone must be fully independent of the donor (mutating the clone must never affect the donor).
Regression coverage: `CopyPasteGraftTests.cs`
(`CopyPaste_SubtreeWithChildren_DeepClonesIndependentlyWithFreshIdentity`), which builds a
`Section → Question → 2 ListItems` subtree, copies it, and asserts fresh identity at every
level plus `Assert.AreNotSame` on the cloned vs. donor `ListItemType` instances.

## Identity regeneration during repeat/inject

For both the `CloneAndRepeatSubtree` and `UpdateNodeIdentity` `RefreshMode` branches,
`SdcUtil.ReflectRefreshSubtreeList`'s internal `NodeWorker()` walk calls
`SdcUtil.AssignGuid_sGuid_BaseName(n, true)` for every node in the subtree — the `true`
argument (`forceNewGuid`) guarantees a brand-new `ObjectGUID` (and therefore a fresh `sGuid`,
which is derived from it) for every cloned node, every time. **No cloned or repeated node ever
keeps its source's `ObjectGUID`/`sGuid`/`ID`/`name`.** This was already correctly implemented
before the work described below; it required no changes.

`@order` is likewise reassigned sequentially for the whole affected subtree, starting from
`(int)newParent.order + 1`, by the same `ReflectRefreshSubtreeList` call — also pre-existing,
unchanged behavior.

## `FormDesignType.RepeatCounter` and suffix assignment

Every `FormDesignType` carries a single, global, monotonically increasing `RepeatCounter`. It
records the last integer used as an `ID`/`name` repeat suffix (`"__N"`) anywhere in that tree. A
value of `0` means no repeats exist yet, so the first repeat/injection in that tree uses suffix
`"__1"`. The suffix is applied uniformly to every node in one cloned subtree per operation (not
incremented per node within the same operation).

- **Plain repeats** (`Copy()`): the new repeat is inserted as the **last sibling among nodes
  sharing the same base `ID`** (`ID` with any trailing `"__N"` stripped) — found by scanning
  forward from the first occurrence of that base `ID` until either the base `ID` changes or the
  siblings run out. The very first repeat of a subtree has no suffix on the original and
  `"__1"` on the new copy.
- **Cross-tree/anywhere injection** (`InjectSubtree()`/`InjectSubtreeFromTemplate()`): the
  target insertion point is found by scanning the target parent's children for the **last**
  node whose base `ID` matches the donor's base `ID`, inserting immediately after it (or at a
  caller-supplied index, or at the end, if no match exists). Unlike a plain repeat, an injected
  root **always** receives a repeat suffix, even on the very first injection of that content —
  there is no "un-suffixed original" already in the target tree to match against, since the
  donor comes from elsewhere. Descendants under the injected root that themselves already
  repeat receive the same `RepeatCounter`-based suffix, uniformly.
- **FDF-authoring-mode subtree copies** (bringing new content into a template while
  editing/versioning it) generally should **not** carry pre-existing `"__N"` suffixes — plain
  `Graft()`/`UpdateNodeIdentity` (full `ID`/`name` reassignment, no suffix scheme) is the
  correct mechanism there, distinct from the repeat/inject suffix scheme. If a template subtree
  *does* happen to already carry a `"__N"` suffix, the implementation is not required to
  produce a fully-correct renumbering of that nested suffix — only to not crash. The current
  implementation appends the new suffix via simple string concatenation (e.g. `"Q1__3"` +
  `"__5"` → `"Q1__3__5"`), producing a valid-but-compound `ID`; this is deliberate, accepted
  scope (see `InjectSubtree_WithDonorIdAlreadyCarryingSuffix_DoesNotThrow`).

## Response-value stripping

**Definition:** a "response value," for stripping purposes, is anything present in an FDF-R
(instance) tree that was not present in the original FDF template before instance data was
added — e.g. `@val`/typed response values, `@selected` (list-item selection state), and
user-added Comment nodes. This is a behavioral definition (diff against the pristine template),
not a fixed property list.

**Where it applies:** repeating nodes and injected nodes, by default. It intentionally does
**not** apply to custom report-style use cases (e.g. copying parts of a filled-in instance to
build a reordered report in a separate instance, which is not a currently implemented use
case) — those need the response data carried forward, not stripped.

**Key design insight — cloning from the template needs no active stripping step.** Rather than
cloning the live instance node and then scrubbing response-shaped content off the clone, the
implementation clones directly from the donor's own **source FDF template**, which by
definition never contained response data. The clone is response-free *by construction*.
`@readOnly`-locked default values (and any other genuine template default, e.g. a pre-selected
`ListItem`) are preserved automatically, because they are part of the template itself, not
instance-only data — no special-casing of `@readOnly` is needed.

### Locating the source FDF template from an instance

Every `TopNode` element (e.g. `FormDesignType`) carries `@ID`, `@fullURI`, and `@filename`
attributes. For an FDF-R instance, `@filename` on the top element is used to locate and load
the source FDF as a **separate, independent OM instance** — its own `TopNode` tree, entirely
distinct from the live FDF-R tree being edited. (`@fullURI` is a logical identity string, not
generally a resolvable path, so it is not used for loading.) The equivalent node in that
separately-loaded template OM is looked up by `ID`, after stripping any trailing `"__N"` repeat
suffix from the instance-side `ID` (since `ID` is stable between an FDF and any FDF-R derived
from it, but instance-only repeats add a suffix the template copy never had).

This is implemented in `SourceFormDesignExtensions.cs`:

- `StripRepeatSuffix()` — removes a trailing `"__N"` suffix from an `ID`/`name`, if present.
- `LoadSourceFormDesign()` — loads the FDF referenced by an instance's `FormDesignType.filename`
  attribute. **Must** use `TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath()`, not the
  lower-level `SdcSerializer<FormDesignType>.LoadFromFile()` — the latter uses raw
  `XmlSerializer` only and does not run the post-deserialize registration step
  (`SdcUtil.ReflectRefreshTree(...)` → `RegisterAll`), leaving the loaded tree's node
  dictionaries empty and node lookup non-functional.
- `FindNodeByTemplateID()` — strips the repeat suffix, then looks up the matching node in the
  loaded template OM.

### `InjectSubtreeFromTemplate()`

```csharp
InjectSubtreeFromTemplate(
    this IdentifiedExtensionType instanceDonorNode,
    ChildItemsType targetParent,
    int newListIndex = -1,
    bool preserveInstanceData = false)
```

- **Default (`preserveInstanceData: false`):** resolves the donor's template counterpart via
  `LoadSourceFormDesign()` + `FindNodeByTemplateID(instanceDonorNode.ID)`, then clones from the
  **template** node (not the live instance node). Throws `InvalidOperationException` (naming
  the offending `ID` and suggesting `preserveInstanceData: true`) if the template can't be
  loaded or has no matching node (e.g. a renamed/orphaned instance node).
- **Opt-in (`preserveInstanceData: true`):** delegates to `InjectSubtree()` directly, cloning
  the live instance node as-is, including any current response values. This performs no
  cross-check against the template, even for `readOnly`-locked fields — it trusts the live
  instance's current state, consistent with the project's existing domain rule that clients
  are expected to follow SDC's own rules.

**Deliberate scope limits (not defects):**
1. Default mode resets to the template's shape — any instance-only structure added to the live
   donor subtree after the instance was created (e.g. nested repeats/injections that exist only
   in the instance) has no template counterpart and is not reproduced.
2. A fallback path ("clone from a cleaned copy of the live instance, stripping `@selected`,
   response `@val`, and internal repeated subtrees, when no source FDF is reachable") is
   accepted as a valid future extension but is **out of scope for the current implementation**
   — deliberately stubbed rather than built, to avoid scope creep before a concrete
   no-source-FDF scenario actually arises.

### `Copy()` and `InjectSubtree()` relationship

`Copy()` (the same-tree repeat entry point) is a thin convenience wrapper over the more general
`InjectSubtree()`, which is the true general-purpose cross-tree/same-tree entry point
introduced to support `InjectForm`-style composition. `Move()`'s `CloneAndRepeatSubtree` branch
was generalized to find the insertion point by scanning the **target's** children directly (a
`^(.*)__\d+$` regex match on base `ID`), rather than walking the donor's own next-siblings —
the old approach only worked when the donor and target were necessarily the same tree.

## Test coverage

- `CopyPasteGraftTests.cs` — null-slot attach fix (`CopyPaste()`/`Graft()`), deep-clone
  correctness for `CopyPaste()`.
- `InjectSubtreeCrossTreeTests.cs` — cross-tree injection with fresh identity, repeated
  cross-tree injection producing sequential suffixes, injection amid unrelated siblings
  (position correctness), donor `ID` already carrying a suffix (must not throw),
  `LoadSourceFormDesign`/`FindNodeByTemplateID` end-to-end via a real temp-file round trip.
- `InjectSubtreeFromTemplateTests.cs` — default mode clones template defaults (not diverging
  instance values, including a `ListItem.selected` case and a `ResponseFieldType`/
  `DataTypes_DEType` value case), `preserveInstanceData: true` clones the live instance's
  diverging values, and a missing-template-counterpart case throws a clear
  `InvalidOperationException`.
- `RepeatingSectionConstructionTests.cs` — pre-existing same-tree `Copy()` coverage, re-verified
  unaffected by the `Move()` refactor.

Full regression result at completion of this work: 713/714 tests passing across the entire
`SDC.Schema.Tests` suite (1 pre-existing, unrelated skip; 0 failures).

## Known, pre-existing, unrelated gap surfaced by this work

While building `InjectSubtreeFromTemplateTests.cs`, reading a `string_DEtype` response value
generically via the shared `IVal.ValXmlString` property threw `InvalidCastException`/
`NotImplementedException`. Investigation found this interface member is unimplemented for most
concrete SDC simple-value types (`*_Stype` classes) in `PartialClasses.cs` — a pre-existing gap
unrelated to tree operations, left unfixed here per project convention and tracked separately:
see [GitHub issue #46](https://github.com/rmoldwin/SDC_ObjectModel/issues/46) and the broader
[GitHub issue #47](https://github.com/rmoldwin/SDC_ObjectModel/issues/47) (full SDC data type
validation/test-coverage audit).
