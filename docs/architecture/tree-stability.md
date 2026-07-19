# OM Tree Stability

> **Status:** Living document, consolidated from `OM_TreeStability_CurrentState.md`,
> `OM_TreeStability_Implementation_Progress.md`, `Session_OMTreeStability_Implementation_Complete.md`,
> and `Session_MoveReparent_BugFix_Complete.md` (originally in `SDC.Schema.Tests/Documentation`,
> now archived — see [../changes/](../changes/)). Describes the dictionary-consistency and
> move/reparent guarantees the SDC Object Model (OM) provides, and the bugs found and fixed while
> building the test suite that validates them. This chapter covers single-threaded tree integrity;
> see [thread-safety.md](thread-safety.md) for the separate concurrent-access work that followed.

## Current status

The OM tree-stability test suite (`SDC.Schema.Tests/Functional/TreeStability/OMTreeStabilityTests.cs`,
with helpers in `SDC.Schema.Tests/Helpers/TreeValidationHelper.cs`) is complete: all 38 tests pass,
covering dictionary population/initialization, node addition, bulk deletion, same-tree move,
cross-tree move, circular-reference prevention, mixed mutation sequences, and a 100-cycle stress
test (completing in under 1 second — well under the 10-second functional-test guideline).

## What the tests validate

Every `TopNode` maintains four pieces of shared state that must stay consistent under all
mutations:

- `_Nodes` — maps `ObjectGUID` → `BaseType`, covering the entire tree.
- `_ParentNodes` — maps a child's GUID → its parent `BaseType`; this is what the read-only
  `ParentNode` **computed property** actually looks up (there is no direct setter).
- `_ChildNodes` — maps a parent's GUID → its ordered `List<BaseType>` of children; drives sibling
  navigation.
- `_IETnodes` — an `ObservableCollection<IdentifiedExtensionType>` covering the whole tree in
  traversal order (the OM does **not** guarantee strict insertion order here — tests were relaxed
  from an original assumption that it did).

Because `ParentNode` and sibling navigation are both computed from these dictionaries, **any**
mutation (add, delete, same-tree move, cross-tree move) must update all four in lockstep, or a
node's apparent parent/children silently diverge from the tree's real shape.

## Production bugs found and fixed while building this suite

### Bug 1 — cross-tree move left source and target references inconsistent

`IMoveRemoveExtensions.Move()`, for a cross-tree move (`RefreshMode.UpdateNodeIdentity`), left the
source-side property reference still set and the target-side reference unset. Fixed by clearing
the source reference *before* `ReflectRefreshSubtreeList()` runs, attaching to the target
*after*, and returning early for the cross-tree path (skipping the same-tree-only
`MoveSingleNode()` logic). This fixed cross-tree reparenting via `ItemMutator` and all cross-tree
move tests.

### Bug 2 — same-tree move didn't update `_ParentNodes`

`MoveInDictionaries()` had an early-return optimization that skipped the `_ParentNodes` update
whenever the node was already present in `_Nodes` — which is exactly the case for a same-tree
move. Since `ParentNode` is a read-only property computed from `_ParentNodes`, this left
`ParentNode` returning the *old* parent after a same-tree move. Fixed by removing the early
return: a same-tree move always re-runs `UnRegisterAll` + `RegisterAll` to refresh
`_ParentNodes`, even though the node's `_Nodes` entry doesn't change.

## Architectural facts worth knowing

- **Cross-tree moves always regenerate node identity.** `ObjectGUID`, `sGuid`, and `ObjectID` are
  all reassigned on a cross-tree move (`RefreshMode.UpdateNodeIdentity` is mandatory for this
  case) — tests (and any code tracking a moved node) must capture the **new** GUID after the
  move, not the original one.
- **Container nodes exist between "visible" nodes.** Sections, questions, and lists can have
  intermediate container nodes (`ChildItems`, `ListField`, `List`) that a naive
  `section.ParentNode == form.Body`-style assertion won't account for. List items in particular
  are parented to the `List` node (`question.ListField_Item.List.Items`), not directly to the
  question and not to `question.GetChildItemsNode().ChildItemsList`.
- **Direct `.Add()` on a list does not trigger reparenting.** Adding an item directly to a
  collection does not invoke `ItemsMutator`; an explicit `Move()` call is required to reparent a
  node. This is intentional OM behavior, not a bug — the relevant test was renamed
  `ComplexAddition_MultipleListFieldsWithSharedItems_RequiresExplicitMove` to document it rather
  than treat it as a defect.
- **`CountReachableNodes()` and `ValidateTreeIntegrity()` only work on `TopNode` types** (e.g.
  `FormDesignType`, `DataElementType`) — you cannot count descendants of a section or question
  directly; count at the `TopNode` dictionary level before/after an operation instead.
- **`RemoveRecursive` can leave at most one orphaned container node** after repeated add/remove
  cycles (observed at ≤1 node per 100 operations) — a known, accepted minor leakage given
  container complexity, not treated as a blocking defect.
