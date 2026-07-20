# Modeling Best Practices

This chapter is guidance for anyone authoring or generating a Structured Data Capture (SDC) form
(a Form Design File, or FDF) intended to be loaded into this Object Model (OM), or writing code
that builds/mutates a tree in memory. It complements — and should stay consistent with — the
machine-checked Best Practice (`BP-*`) rules cataloged in
[qa-best-practices.md](qa-best-practices.md); where a rule below is enforced automatically by the
Quality Assurance (QA) engine, its rule ID is given so violations surface as QA findings, not just
as documentation advice.

> **Status:** Starter chapter. This is an initial pass, not yet a complete authoring guide — expand
> it as new modeling pitfalls are discovered (e.g., in a bug fix, a QA rule addition, or a rules
> engine change) rather than only recording them in a change log. See
> [object-model-overview.md](object-model-overview.md) for the tree/type background this guidance
> assumes.

## Identity: don't confuse `ObjectID` with an IET's durable ID

Every node gets a transient `ObjectID`, freshly assigned on each build/deserialization pass — do
not persist it, compare it across two different load cycles, or use it as a stable cross-reference
inside form content. If a node needs a durable, form-author-controlled identity (for cross-
references, rules targeting, or change tracking across versions), it must derive from
`IdentifiedExtensionType` (IET) and be given a unique ID — enforced by `BP-MUT-001` ("every IET
node's ID must be unique within its tree"). See
[object-model-overview.md](object-model-overview.md#identifiedextensiontype-iet) for the
distinction.

## Tree integrity

Every non-root node must have a `ParentNode` that belongs to the same tree — not a dangling
reference into another tree, and not `null` except for the tree's own top node. This is enforced by
`BP-GEN-001`. Violations typically come from manually re-parenting or copying nodes between two
trees without using the object model's move/attach helpers (`IMoveRemove`/`INavigate`, see
[object-model-overview.md](object-model-overview.md)) — always use those helpers rather than
assigning node references directly.

## Ad hoc ("any") attributes and elements

An `<Extension>` element should never be left with zero ad hoc attributes and zero ad hoc child
elements — if there is nothing form-specific to attach, omit the `<Extension>` element entirely
rather than emitting an empty one (`BP-ADH-001`). When ad hoc content does span multiple XML
namespaces (including a form's default/inherited namespace), see
[serialization.md](serialization.md) for the current round-trip support matrix across XML, JSON,
BSON (Binary JSON), and MessagePack (MsgPack) — as of this writing, mixed-namespace ad hoc content
is confirmed for XML only (see [qa-best-practices.md](qa-best-practices.md)'s numbered example
`03-adhoc-attributes-mixed-namespaces.xml`); do not assume the other three wire formats already
round-trip this correctly without checking the current state of
[roadmap.md](../roadmap.md).

## Validation: design for the "soft-reject" contract, don't fight it

The object model's built-in validation is designed so that an invalid value assigned to a
Data Element (DE) is **not stored** — the prior/unset value is retained instead, and the rejected
value plus the reason are recorded so the caller (or a rules/QA check) can react. Model authors and
code that constructs/mutates trees should expect this: reading a property back immediately after
assigning an invalid value will not return what you just assigned, and a node must not be left with
unresolved rejected-value entries once processing completes (`BP-VAL-001`). See
[validation.md](validation.md) for the full contract, including where it is not yet uniformly
applied (see the open items linked from [roadmap.md](../roadmap.md)'s Validation section).

## Rules engine: coherence findings must be visible

Any coherence-rule finding produced by `ValidateTree()` should also surface in the QA report, not
only in a separate/lower-visibility channel (`BP-VAL-002`) — if you add a new rule to the rules
engine (see [rules.md](rules.md)), make sure its findings are wired into the same QA reporting path
so a form author actually sees them.

## Serialization hygiene

Internal implementation state must never leak into serialized output — for example, the internal
`TreeRwLock` (a `ReaderWriterLockSlim` used for thread safety, see
[thread-safety.md](thread-safety.md)) must never appear in serialized JSON (`BP-SER-001`). More
generally: if you add a new field to a customized class (see
[object-model-overview.md](object-model-overview.md) for where customizations live), consider
whether it needs `[XmlIgnore]`/`[JsonIgnore]` before it ends up in every serialized form instance.

## Concurrency

Building a tree from multiple threads, or mutating a tree that another thread is reading, is only
safe within the boundaries documented in [thread-safety.md](thread-safety.md) and
[tree-stability.md](tree-stability.md) — read the scope-caveat banner at the top of
[thread-safety.md](thread-safety.md) first: the desktop investigation's "fixed" status does not
extend to the separate, still-open WebAssembly (WASM) multi-threading investigation (see
[wasm-blazor.md](wasm-blazor.md)).

## Related chapters

- [object-model-overview.md](object-model-overview.md) — tree/type background
- [qa-best-practices.md](qa-best-practices.md) — the machine-checked rule catalog referenced above
- [validation.md](validation.md) — the soft-reject validation contract in full
- [rules.md](rules.md) — the rules engine
- [serialization.md](serialization.md) — wire-format round-trip details
