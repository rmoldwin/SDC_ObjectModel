# SdcUtil

> **Status:** Living document. `SdcUtil` is a large, mixed-responsibility helper class that still
> grows as tree-navigation, validation, and serializer support evolves. This chapter describes the
> implementation currently in this repository baseline and points to
> [thread-safety.md](thread-safety.md) where shared-cache concurrency risks are tracked in more
> detail.

## Summary

`SdcUtil` is the central static helper class behind much of the Structured Data Capture (SDC)
Object Model (OM)'s non-generated behavior. It does five main jobs:

1. manages async-flow validation state and rejected-value storage,
2. refreshes and re-registers tree metadata after construction, cloning, or moves,
3. walks trees in document order using either cached dictionaries or reflection,
4. reflects Extensible Markup Language (XML) element and attribute metadata, including ad hoc
   attributes,
5. provides naming, attachment, formatting, and array/list helper functions used by extension
   methods and serializers.

Most consumer-facing code touches `SdcUtil` indirectly through extension methods, but the class also
exposes a large public helper surface directly.

## Shared state and caches

### Async-flow validation state

`SdcUtil` keeps three async-local switches:

| Member | Purpose |
|---|---|
| `IsDeserializing` | Marks the current async flow as actively rebuilding a tree. |
| `SuppressValidation` | Suppresses validation events and report writes without allowing invalid values to be stored. |
| `ValidationCollector` | Carries the current `SdcValidationReport` when a validating deserialize or full-tree validation sweep is running. |

These are `AsyncLocal<T>` values, so they flow with the current asynchronous execution context
rather than being shared process-wide.

### Rejected-value store

`RecordRejectedValue`, `GetRejectedValues`, `HasRejectedValues`, `ClearRejectedValue`, and
`ClearRejectedValues` implement the "soft reject" contract. Invalid values are not stored on the
real property, but the rejected attempt is preserved out-of-band in a
`ConditionalWeakTable<BaseType, Dictionary<string, SdcRejectedValue>>`.

That store is:

- per node,
- keyed by property name,
- automatically garbage-collected with the node,
- never serialized into XML, JavaScript Object Notation (JSON), or other payloads.

### Reflection and sort caches

`SdcUtil` also keeps several static caches, notably:

- `dListPropInfoElements`
- `dListPropInfoAttributes`
- `dXmlRootAtts`
- `dXmlElementAtts`
- `dXmlChoiceIdentifierAtts`
- `dXmlAttAtts`

These exist to avoid repeated reflection cost. They are plain `Dictionary<,>` instances, not
concurrent collections. The remaining race risk around `dListPropInfoElements` and
`dListPropInfoAttributes` is tracked in [thread-safety.md](thread-safety.md) and
[roadmap.md](../roadmap.md) (issue `#23`).

## Validation and deserialization helpers

### `ValidateAndRaise(object? value, ValidationContext ctx)`

Main setter-facing validation entry point. It:

1. checks any override rules registered in `SdcValidationRuleRegistry`,
2. otherwise falls back to the property's `DataAnnotations`,
3. clears older rejected-value entries on success,
4. records the rejected attempt and optionally raises events on failure.

Important behavior:

- it is **non-throwing**,
- returning `false` means the caller must not assign the incoming value,
- `SuppressValidation = true` suppresses event/report noise only; it does not convert invalid input
  into valid input.

### `ValidateLexicalAndRaise(BaseType node, string memberName, string? lexical, XsdDateKind kind)`

Equivalent helper for raw XML Schema Definition (XSD) lexical strings, used by date/time parsing and
datatype-builder code. It validates the raw text before a parsed value is committed.

### Practical implication

`SdcUtil` is part of the core validation pipeline, not just a convenience library. Even though the
class name sounds generic, these methods are on the correctness path for setters and deserializers.

## Tree refresh, identity refresh, and dictionary rebuilds

### `ReflectRefreshTree(...)`

```csharp
List<BaseType> ReflectRefreshTree(
    ITopNode topNode,
    out string? treeText,
    bool print = false,
    bool refreshTree = true,
    CreateName? createNodeName = null,
    int orderStart = 0,
    int orderGap = 10)
```

This is the heavyweight whole-tree reflection walker. Depending on flags, it can:

- rebuild `_Nodes`, `_ParentNodes`, `_ChildNodes`, and `_IETnodes`,
- assign missing `sGuid`, `ObjectGUID`, `BaseName`, `ObjectID`, and `name` values,
- recalculate `order`,
- emit a printable tree dump in `treeText`.

It also contains special handling for top-node flavors such as `FormDesignType`,
`DemogFormDesignType`, `DataElementType`, `RetrieveFormPackageType`, and `MappingType`.

### `ReflectRefreshSubtreeList(...)`

This is the subtree-scoped variant. It visits a start node and optionally its descendants, applying
reorder, re-registration, rename, or identity-refresh logic. It is the main helper used after
moves, clones, or subtree restoration.

Key parameters with non-obvious behavior:

- `singleNode: true` limits work to the start node.
- `reOrder: true` mutates `order`.
- `reRegisterNodes: true` unregisters and re-registers nodes in the owning top-node dictionaries.
- `targetParentNode` changes which top node and parent context the subtree is refreshed against.
- `nodeWorkerFirst` and `nodeWorkerLast` are hooks that run before and after the built-in work for
  each visited node.

### `RefreshMode`

`ReflectRefreshSubtreeList` uses the `RefreshMode` enum:

- `NoChange` — keep existing identity data.
- `UpdateNodeIdentity` — generate new `sGuid`, `ObjectGUID`, `BaseName`, `ObjectID`, and names.
- `CloneAndRepeatSubtree` — generate new identity and append repeat suffixes such as `__1`.
- `RestoreSubtreeFromOlderVersion` — preserve older-version identity where possible while
  re-registering into the current tree.

### `GetRepeatSuffix(IdentifiedExtensionType rt)`

Parses a trailing `__<number>` repeat suffix from `ID`. It is used during repeat/subtree-clone
maintenance.

## Ordered traversal and navigation helpers

`SdcUtil` provides two parallel families of traversal helpers:

- **dictionary-based** helpers, which use `_ChildNodes` and other top-node dictionaries,
- **reflection-based** helpers, which inspect `[XmlElement]` properties directly.

### Ordered list builders

| Method | Notes |
|---|---|
| `GetSortedTreeList(ITopNode tn)` | Ordered list of the whole tree. |
| `GetSortedSubtreeList(BaseType n, int startReorder = 0, int orderInterval = 1, bool ResetSortFlags = true)` | Ordered list of a subtree; may also rewrite `order`. |
| `GetSortedNonIETsubtreeList(BaseType n, ...)` | Stops descent when another `IdentifiedExtensionType` is encountered. |
| `GetSortedSubtreeIET(BaseType n, bool resortChildNodes = false, bool resetSortFlags = true)` | Ordered list of just `IdentifiedExtensionType` nodes. |

Gotcha: a non-negative `startReorder` is not a read-only traversal. It mutates `order` values.

### Point navigation

Important public helpers include:

- `GetNextElement` / `GetPrevElement`
- `GetFirstChildElement` / `GetLastChildElement`
- `GetFirstSibElement` / `GetNextSibElement` / `GetPrevSibElement` / `GetLastSibElement`
- `GetNextElementIET` / `GetPrevElementIET`
- `GetLastDescendantElement`
- reflection twins such as `ReflectNextElement`, `ReflectPrevElementIET`, and
  `ReflectChildElements`

The dictionary-based helpers are usually the faster and more production-oriented versions. The
reflection versions are useful when validating dictionary integrity or when metadata has not yet
been fully refreshed.

Two historical cycle fixes are worth knowing:

- `ReflectPrevElement` now returns the parent when there is no previous sibling, rather than
  accidentally re-entering the current subtree.
- `ReflectPrevElementIET` and `ReflectNextElementIET` now track visited nodes to avoid infinite
  loops.

### `FindTopNode(BaseType node)`

Walks up `ParentNode` references until it finds an `ITopNode`. If the supplied node is already the
top node, it returns `null` rather than echoing the input back.

## XML metadata and XML-attribute reflection

### `ReflectNodeXmlAttributes(...)`

```csharp
List<AttributeInfo> ReflectNodeXmlAttributes(
    BaseType n,
    bool getAllXmlAttributes = true,
    bool omitDefaultValues = true,
    string[]? attributesToExclude = null,
    string[]? attributesToInclude = null)
```

This is the most important XML-inspection helper in the file. It reflects the attributes that belong
to one node and returns `AttributeInfo` records describing each one.

Notable behavior:

- by default it excludes `name`, `sGuid`, and `order`,
- when `getAllXmlAttributes` is `false`, it tries to mirror real XML serialization rules,
- it checks `ShouldSerialize*` methods and `DefaultValueAttribute` values,
- it includes ad hoc attributes stored through `XmlAnyAttribute`,
- exclude filters win over include filters.

This method is heavily reused by `CompareTrees<T>`.

### `GetAttributeDefaultValue(PropertyInfo pi)`

Returns the `DefaultValueAttribute.Value` associated with an XML attribute property, if present.

### `GetElementPropertyInfoMeta(BaseType item, BaseType? parentNode, bool getNames = true)`

Returns a `PropertyInfoMetadata` value describing how a node is represented inside its parent:

- backing `PropertyInfo`,
- property name,
- list index when the parent property is a collection,
- XML `Order`,
- maximum order in that parent type,
- resolved XML element name.

This is one of the key helpers behind move, attach, and rename logic.

### `GetPropertyObject` and `GetTypeDefaultValue`

Small reflection helpers used throughout tests and refresh logic.

## Attachment and ItemChoice synchronization helpers

These methods are `internal`, but they matter architecturally because higher-level move/add
extensions depend on them.

### `IsAttachNodeAllowed(...)`

Finds whether a node can legally attach to a target parent and, if so, which property and optional
ItemChoice field should be used. It prefers an explicit XML element name. If no element name is
supplied, it falls back to type inference and only succeeds when the match is unambiguous.

### `TryAttachNewNode(...)`

Actually performs the attach. It handles:

- single-object target properties,
- `List<T>` target properties,
- array target properties,
- ItemChoice scalar enums,
- ItemChoice enum lists and arrays.

Non-obvious behavior:

- `overwriteExistingObject` must be `true` before replacing an occupied scalar property,
- `cancelWhenChildNodes` blocks replacement when the existing target subtree already has children,
- when ItemChoice metadata is involved, the helper updates the choice enum and the node collection
  together.

### `TryRemoveItemChoiceEnumValue(...)`

Removes the matching ItemChoice value when a node is removed from a choice-backed property or list.

## Naming and classification helpers

### `GetItemType(IdentifiedExtensionType node)`

Maps runtime node shape to `ItemTypeEnum` (`Section`, `QuestionSingle`, `QuestionMultiple`,
`QuestionResponse`, `ListItem`, and so on). A small gotcha captured in tests: because
`ButtonItemType` derives from `DisplayedType`, the current branch order causes
`ButtonItemType` to classify as `DisplayedItem`.

### `CreateCAPname(...)`

Creates a stable `name` value, usually anchored to the nearest `IdentifiedExtensionType`
identifier. The method's historical naming reflects College of American Pathologists (CAP)-oriented
form conventions. It preserves custom names in `NameChangeEnum.Normal` mode, renames everything in
`NameChangeEnum.RenameAll`, and preserves non-empty names in `NameChangeEnum.PreserveAll`.

### `GetNamePrefix(BaseType node)`

Builds the leading token for names, with special handling for question subtypes and certain property
names such as `reportText` and `altText`.

### `CreateSimpleName(...)`

Builds a simpler name from `ElementPrefix` plus `BaseName`.

### `CreateBaseNameFromsGuid(...)` and `AssignGuid_sGuid_BaseName(...)`

These helpers derive short, identifier-safe names from `sGuid` values and optionally generate new
identity for a node. They are used during tree refresh and clone/move flows.

## Small utility helpers

Less SDC-specific helpers include:

- `FormatXml(string Xml)` — pretty-prints XML,
- `ReorderXml(string Xml, int orderMultiplier = 1)` — writes sequential `order` attributes into raw
  XML,
- array/list helpers such as `IsGenericList`, `GetFirstNullArrayIndex`, `IndexOf`,
  `GetObjectFromIEnumerableIndex`, `RemoveArrayNullsNew`.

## Thread-safety notes

- `SdcUtil` participates in the tree-stability and thread-safety story through child-list sorting,
  dictionary rebuilds, and shared reflection caches.
- `SortElementKids(...)` relies on per-top-node sort flags to avoid repeated expensive sorts; see
  [thread-safety.md](thread-safety.md) for the thread-safety work that changed when and how those
  flags are invalidated.
- The remaining plain static reflection caches (`dListPropInfoElements`,
  `dListPropInfoAttributes`) are a known concurrency gap tracked in issue `#23`; this chapter does
  not duplicate that analysis.

## Examples

### Ordered traversal plus XML-attribute inspection

Adapted from `SdcUtilTests.cs`:

```csharp
FormDesignType form = FormDesignType.DeserializeFromXml(xml);

List<IdentifiedExtensionType> ietNodes = SdcUtil.GetSortedSubtreeIET(form, resortChildNodes: false);
List<AttributeInfo> attributes = SdcUtil.ReflectNodeXmlAttributes(
    form,
    getAllXmlAttributes: true,
    omitDefaultValues: false,
    attributesToExclude: Array.Empty<string>());
```

### Reorder and refresh a subtree

```csharp
List<BaseType>? refreshed = SdcUtil.ReflectRefreshSubtreeList(
    body,
    reOrder: true,
    startReorder: 0,
    orderInterval: 2);
```

## Known limitations and visible TODOs

- The class still mixes several responsibilities; it is a utility hub rather than a narrowly scoped
  service.
- `TreeSort_ClearNodeIds` is public today but marked in a comment as a candidate to become internal.
- `ReflectNodeXmlAttributes` still contains TODO notes about broader default-value coverage.
- Name-generation comments note future work around stronger uniqueness tracking and filtering out
  undesirable generated names.
- The internal attach helpers retain a number of older TODO and retired code paths, especially
  around ambiguous element-name inference and ItemChoice matching.
