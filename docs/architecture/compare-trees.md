# CompareTrees

> **Status:** Living document. This chapter describes the `CompareTrees<T>` implementation currently
> present on the `origin/Features/NET10/Net10Main` baseline used by this worktree. A separate,
> not-yet-merged branch, `origin/Features/CompareTrees`, is ahead with additional work, so this
> chapter intentionally documents only the behavior visible in the current baseline.

## Summary

`CompareTrees<T>` is a version-comparison helper for two Structured Data Capture (SDC) Object Model
(OM) trees of the same top-node type (`FormDesignType`, `RetrieveFormPackageType`, and so on). Its
main use case is comparing two versions of the same form instance or form definition and answering
questions such as:

- which `IdentifiedExtensionType` nodes were added or removed,
- which nodes moved or changed parent,
- which serialized Extensible Markup Language (XML) attributes changed,
- which non-`IdentifiedExtensionType` child nodes were added to or removed from an existing
  `IdentifiedExtensionType` subtree.

That makes it useful for diff-style user interfaces, change summaries, lineage review, and
audit-oriented tooling built on top of the in-memory tree.

## Public Application Programming Interface (API)

### Construction and version selection

| Member | Purpose |
|---|---|
| `CompareTrees(T prevVersion, T newVersion)` | Compares two already-built top-node trees. |
| `CompareTrees(string prevXml, string newXml)` | Deserializes both XML documents to `T`, then compares them. |
| `T PrevVersion { get; set; }` | Gets or replaces the older tree. Setting it recomputes the cached comparison state. |
| `T NewVersion { get; set; }` | Gets or replaces the newer tree. Setting it recomputes the cached comparison state. |

The XML constructor adds extra error context if either deserialize step fails. The newer document is
deserialized from `newXml`, and the older document is deserialized from `prevXml`.

### Whole-tree results

| Member | Returns |
|---|---|
| `ReadOnlyCollection<IdentifiedExtensionType> GetIETnodesRemovedInNew` | `IdentifiedExtensionType` nodes present in `PrevVersion` but absent from `NewVersion`. |
| `ReadOnlyCollection<IdentifiedExtensionType> GetIETnodesAddedInNew` | `IdentifiedExtensionType` nodes present in `NewVersion` but absent from `PrevVersion`. |
| `ReadOnlyCollection<BaseType> GetNodesRemovedInNew` | Any node type present in `PrevVersion` but absent from `NewVersion`. |
| `ReadOnlyCollection<BaseType> GetNodesAddedInNew` | Any node type present in `NewVersion` but absent from `PrevVersion`. |
| `ReadOnlyDictionary<string, DifNodeIET>? GetIETattDiffs` | One `DifNodeIET` summary per newer-tree `IdentifiedExtensionType`, keyed by that node's `sGuid`. |

### Per-node inspection

| Member | Purpose |
|---|---|
| `DifNodeIET CompareIET(IdentifiedExtensionType ietNew)` | Recomputes the diff summary for one newer-tree `IdentifiedExtensionType` node and updates the cached dictionary entry. |
| `DifNodeIET? GetIETattributes(IdentifiedExtensionType IETnode)` | Looks up the already-computed summary for that `IdentifiedExtensionType`. |
| `DifNodeIET? GetIETattributes(ShortGuid sGuidIET)` | Same lookup by `sGuid`. Throws `ArgumentException` if the `sGuid` belongs to a non-`IdentifiedExtensionType` node. |
| `List<BaseType>? FindAddedIETsubNodes(ShortGuid sGuidIETnew)` | Finds newer-tree descendants under the specified `IdentifiedExtensionType` that were not under the matching older-tree subtree. |
| `List<BaseType>? FindRemovedIETsubNodes(ShortGuid sGuidIETnew)` | Finds older-tree descendants under the matching older-tree subtree that are no longer under that subtree in the newer tree. |

### Attribute extraction helpers

| Member | Purpose |
|---|---|
| `SortedList<string, Dictionary<string, List<AttributeInfo>>> FindSerializedXmlAttributesFromTree(ITopNode topNode)` | Builds a per-`IdentifiedExtensionType` map of attributes that would actually serialize to XML. |
| `Dictionary<string, List<AttributeInfo>> FindSerializedXmlAttributesIET(IdentifiedExtensionType iet)` | Builds the same map for one `IdentifiedExtensionType` subtree. |

### Two lightly wrapped dictionary probes

`IsNewNodeAdded(BaseType nodeNew, out BaseType? NodePrev)` and
`IsPrevNodeRemoved(BaseType prevNode, out BaseType? newNode)` are very small wrappers over
`TryGetValue` against the opposite tree's `Nodes` dictionary. Despite their names, `true` means a
matching node **was found** in the opposite tree.

## Core data structures

`CompareTrees<T>` precomputes and keeps several caches:

- `_slAttPrev` and `_slAttNew` — `SortedList<string, Dictionary<string, List<AttributeInfo>>>`
  keyed by `IdentifiedExtensionType.sGuid`. Each value maps every node in that
  `IdentifiedExtensionType` subtree to the list of XML attributes that would serialize for that
  node.
- `_IETnodesRemovedInNew` / `_IETnodesAddedInNew` — set differences at the
  `IdentifiedExtensionType` level.
- `_nodesRemovedInNew` / `_nodesAddedInNew` — set differences across all `BaseType` nodes.
- `_dDifNodeIET` — `ConcurrentDictionary<string, DifNodeIET>` keyed by newer-tree
  `IdentifiedExtensionType.sGuid`. Each `DifNodeIET` record carries booleans such as `isMoved`,
  `isParChanged`, `isAttListChanged`, `hasAddedSubNodes`, `hasRemovedSubNodes`, plus the detailed
  `AttInfoDif` payloads.

`DifNodeIET` and `AttInfoDif` are defined in `AttributeInfo.cs`.

## How matching works

### Node identity

The comparer treats the short Globally Unique Identifier (`sGuid`) / `ObjectGUID` pair as the
stable identity for matching nodes between versions:

- top-level node and subtree membership checks use the `TopNode.Nodes` dictionaries,
- `IdentifiedExtensionType`-level matching uses the newer node's `sGuid`,
- subnode attribute matching uses the subnode `sGuid` within the current
  `IdentifiedExtensionType` subtree.

There is no lineage or ancestry validation. The tests explicitly note that callers must ensure the
newer tree really is a later version of the older one.

### Parent and position changes

For an `IdentifiedExtensionType` node that exists in both trees:

- `isParChanged` is set when `ParentNode?.sGuid` differs.
- `isMoved` is set when the previous sibling returned by `GetNodePreviousSib()` differs.

So "moved" here means "its sibling position changed within the serialized tree order," not just
"its subtree contents changed."

### Added and removed descendants

`FindAddedIETsubNodes` and `FindRemovedIETsubNodes` walk the non-`IdentifiedExtensionType`
descendants of the matching subtree. A node counts as added or removed when:

- it does not exist at all in the opposite tree, or
- it exists but now sits under a different `ParentIETnode`, or
- it exists but now sits under a different direct `ParentNode`.

That means a reparented child is intentionally reported as added at the new location and removed
from the old one.

## Comparison algorithm

### 1. Lock both trees for a read-only comparison

`CtorCompareTrees` acquires `ReadLockScope` over one or two `_ITopNode.TreeRwLock` instances. When
two distinct trees are involved, it locks them in Globally Unique Identifier (GUID) order to avoid
deadlock. This is the
same single-writer/multiple-reader regime described in [thread-safety.md](thread-safety.md).

### 2. Snapshot the would-serialize XML attributes

`FindSerializedXmlAttributesFromTree` iterates every `IdentifiedExtensionType` node in the tree and
calls `FindSerializedXmlAttributesIET`. That helper:

1. gets the ordered subtree via `SdcUtil.GetSortedNonIETsubtreeList`,
2. reflects each node with `SdcUtil.ReflectNodeXmlAttributes(subNode, false)`,
3. stores only attributes that would actually serialize, including ad hoc attributes.

This design makes the diff XML-focused: it ignores internal fields and compares what would matter in
serialized output.

### 3. Compute added and removed node sets

`ComputeAddedRemovedNodes` uses `Except(...)` with `SDCsGuidEqualityComparer<T>` to produce both the
`IdentifiedExtensionType`-only and all-node added/removed lists.

### 4. Build `DifNodeIET` summaries

`CompareVersionAttributes()` walks `_slAttNew` with `AsParallel().ForAll(...)`. For each newer-tree
`IdentifiedExtensionType`, it:

1. locates the matching older-tree node by `sGuid`,
2. checks parent change and previous-sibling change,
3. calls `FindAddedIETsubNodes` and `FindRemovedIETsubNodes`,
4. compares each newer attribute list to the matching older list,
5. records differences as `AttInfoDif` entries,
6. inserts the result into `_dDifNodeIET`.

Attribute comparison uses two rules:

- same subnode + same attribute name + different `ValueString` => changed attribute,
- attribute present in one version but not the other => added or removed attribute.

The helper `SdcSerializedAttComparer` compares attributes by `(sGuid, attribute name)` so set
difference operations can find attributes removed from the newer version.

### 5. On-demand recomputation for one node

`CompareIET(IdentifiedExtensionType ietNew)` reruns the same comparison idea for a single newer-tree
`IdentifiedExtensionType` and overwrites that entry in `_dDifNodeIET`. This is useful when a caller
mutates one node after the original constructor pass and wants a targeted refresh.

## Thread-safety notes

- Whole-tree construction is guarded by read locks on the participating trees.
- `_dDifNodeIET` is a `ConcurrentDictionary` because the main attribute-comparison pass runs in
  parallel.
- The class still uses a local `lock` around `FindAddedIETsubNodes`, `FindRemovedIETsubNodes`, and
  `GetNodePreviousSib()` calls because those helpers may trigger child-list sorting or other shared
  tree reads that are not fully lock-free.
- The object is not a live view. If either compared tree changes later, callers must set
  `PrevVersion`/`NewVersion` again or call `CompareIET(...)` to refresh the cached results.

## Example

Adapted from `CompareTreesTests.cs`:

```csharp
FormDesignType previous = FormDesignType.DeserializeFromXml(previousXml);
FormDesignType current = FormDesignType.DeserializeFromXml(currentXml);

var comparer = new CompareTrees<FormDesignType>(previous, current);

ReadOnlyCollection<IdentifiedExtensionType> addedIetNodes = comparer.GetIETnodesAddedInNew;
DifNodeIET? dif = comparer.GetIETattributes("iUnfss9Ppk-frsOz8qTnIw");

if (dif is not null && dif.Value.isNew)
{
    Console.WriteLine($"New node count: {addedIetNodes.Count}");
}
```

## Known limitations and visible TODOs

- There is **no built-in lineage check** between the two trees.
- `GetIETattDiffs` is keyed by newer-tree `IdentifiedExtensionType` nodes only. Removed
  `IdentifiedExtensionType` nodes are exposed separately through `GetIETnodesRemovedInNew`; a code
  comment asks whether dedicated `DifNodeIET` entries for removed-only nodes should also be added.
- Move detection still depends on previous-sibling lookup. The code comments note a desire to carry
  previous-sibling data directly in the diff payload or move to a safer non-extension helper.
- Attribute matching currently does a linear `FirstOrDefault(...)` lookup by name inside each older
  attribute list; the file contains a TODO about optimizing that path.
- Several "should never happen" branches still contain `Debugger.Break()` placeholders rather than a
  production error path.
- When a subnode moved from a different `IdentifiedExtensionType` parent, the diff can legitimately
  contain `AttInfoDif.aiPrev == null` for that parent-local comparison even though the subnode still
  exists elsewhere in the older tree.
