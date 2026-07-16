# Validation

> **Status:** Living document, migrated from the public GitHub gist "SDC OM Validation: ad hoc +
> (de)serialization-time validation, QaEngine/SdcValidationReport bridge". See also
> [rules.md](rules.md) (the companion document) and the design-history drafts in
> [../changes/](../changes/).

## Summary

`SDC.Schema` validation happens on **two historically separate tracks** that
this project is progressively unifying:

1. **Core-library validation** (`SdcValidate.cs`, `SdcValidationEvents`,
   `SdcValidationReport`) — hand-authored, runs DataAnnotations validation
   and constraint-coherence checks over an entire tree, feeds a static
   real-time event stream, and (as of this project) also runs the
   `ISdcCoherenceRule` registry (SDAC/SDS + any custom-registered rules).
   Used both automatically during (de)serialization sweeps and on-demand.
2. **`SDC.Schema.QA`** — a separate project, deliberately restricted to only
   the public `SDC.Schema.dll` API surface (no internals access), built
   around a pluggable `IQaRule`/`QaEngine`/`QaReport` model. Originally
   designed for on-demand "best practices" checks (construction conventions,
   ad-hoc attribute usage, serialization round-trip safety), not wired to
   the core library's event/report system.

Historically there was **no bridge** between these two tracks — a
`SdcValidationReport` and a `QaReport` were two independent, non-overlapping
outputs. This project closes that gap with a thin adapter rule
(`BP-VAL-002`) so that any rule registered once — including SDAC/SDS and any
future custom coherence rule — shows up in **both** reports, without a rule
author ever needing to implement the same check twice.

## User's Guide

### Running on-demand validation

**Core-library sweep** (DataAnnotations + coherence + SDAC/SDS + any custom
`ISdcCoherenceRule`):

```csharp
ITopNode topNode = /* your tree */;
SdcValidationReport report = topNode.ValidateTree();

foreach (var issue in report.Issues)
    Console.WriteLine($"[{issue.Severity}] {issue.RuleCode}: {issue.Message}");
```

**`SDC.Schema.QA` sweep** (best-practices rules, including the `BP-VAL-002`
bridge to the above):

```csharp
var engine = new QaEngine(new IQaRule[]
{
    new DuplicateIdRule(),                 // BP-MUT-001
    new UnresolvedRejectedValuesRule(),    // BP-VAL-001
    new SdacSdsCoherenceRule(),            // BP-VAL-002 (bridges the registry above)
    // ...plus any of your own IQaRule implementations
});

QaReport qaReport = engine.Evaluate(topNode);
foreach (var finding in qaReport.Findings)
    Console.WriteLine($"[{finding.RuleId}] {finding.Message}");
```

`QaEngine`'s constructor takes any `IEnumerable<IQaRule>` — it is already a
working plug-in point today. A client app that only needs **on-demand**
(not real-time/(de)serialization-time) custom checks can register an
`IQaRule` directly here with zero core-library changes.

### (De)serialization-time validation

`ValidateTree()` (and therefore SDAC/SDS + any registered
`ISdcCoherenceRule`) already runs automatically as part of the existing
deserialization validation sweep — no extra step is required to get
SDAC/SDS coverage on every deserialize. This is separate from, and in
addition to, the pre-existing per-property `RejectedValues`
(`SdcRejectedValue`) mechanism, which records values a setter refused to
store (a different, narrower concept than a coherence-rule finding — nothing
is "rejected" in an SDAC/SDS finding; the selection *is* applied, it's just
flagged as data-at-risk).

### Severity conventions

| Severity | Meaning | Example |
|---|---|---|
| `Error` | The value/state is rejected; the setter refuses to store it | Standard DataAnnotations validation failures |
| `Warning` | The state is accepted, but flagged for attention — typically because a client-side undo opportunity should be offered before anything is lost | SDAC/SDS coherence findings (deliberately `Warning`, not `Error` — see the SDC OM Rules gist for the full rationale) |
| `Info` | Informational only | (reserved for future use) |

## Technical Explanation

### Core-library validation internals

- **`SdcValidate.ValidateTree()`** (`SDC.Schema/Utility Classes/SdcValidate.cs`)
  — extension method on `ITopNode`. Creates an `SdcValidationReport`, sets it
  as the ambient `SdcUtil.ValidationCollector`, temporarily forces
  `SdcUtil.SuppressValidation = false`, and walks the whole tree running (a)
  DataAnnotations validation (`Validator.TryValidateObject`) per node, (b)
  constraint-coherence checks (`CheckValAgainstConstraints`), and (c) — as of
  this project — every registered `ISdcCoherenceRule.Evaluate(topNode)` call.
- **`SdcValidationEvents`** (`SDC.Schema/Utility Classes/SdcValidationEvents.cs`)
  — a **static** event hub (`ValidationOccurred`). `Raise(...)` is `internal`,
  so only `SDC.Schema` itself (and, via `InternalsVisibleTo`,
  `SDC.Schema.Tests`) can raise it — `SDC.Schema.QA` cannot, by design (it's
  public-API-only). This is why the `ISdcCoherenceRule` registry lives in
  `SDC.Schema` core rather than in the QA project: it needs access to this
  internal event-raising plumbing.
- **`SdcValidate.Coherence.cs`'s `RaiseCoherenceViolation`** — the established
  "one function does it all" pattern: records an `SdcRejectedValue`,
  conditionally adds an `SdcNodeValidationIssue` to the ambient
  `ValidationCollector`, and raises `SdcValidationEvents`. SDAC/SDS follow the
  same three-part pattern (adapted — since nothing is "rejected", only the
  report + event parts apply, not the rejected-value recording).
- **`SdcCoherenceRuleRegistry`** (new, this project) — a static registry of
  `ISdcCoherenceRule` implementations, consulted by `ValidateTree()`. See the
  companion [Rules Engine](rules.md) chapter for the full
  interface/registry design and the built-in SDAC/SDS rules.
- **Additive field:** `SdcValidationEventArgs`/`SdcNodeValidationIssue` gain a
  `RuleCode`/`Category` field (e.g. `"SDAC"`, `"SDS"`) so consumers can filter
  or branch on rule identity instead of message-string matching. This is a
  non-breaking, additive change (init-only property).

### `SDC.Schema.QA` internals

- **`IQaRule`** — `{ RuleId, Description, Convention, Evaluate(ITopNode) ->
  IEnumerable<QaFinding>, TryFix(ITopNode, QaFinding) -> bool }`.
- **`QaEngine`** — constructed with an explicit `IEnumerable<IQaRule>`; this
  is already inherently pluggable, since any consumer can pass in custom
  `IQaRule` implementations with zero core-library changes.
- **`BP-VAL-002` (`SdacSdsCoherenceRule` or similarly named)** — the bridge
  rule. Internally, it either (a) calls `topNode.ValidateTree()` and filters
  for `RuleCode is "SDAC" or "SDS"`-tagged issues, or (b) calls
  `SdcCoherenceRuleRegistry`'s rules directly and maps
  `SdcNodeValidationIssue` → `QaFinding`. Either approach means a rule author
  only ever writes **one** `ISdcCoherenceRule` implementation to have it show
  up in both the real-time event stream/`SdcValidationReport` **and** the QA
  Markdown/HTML report — avoiding duplicate implementations of the same
  check in two different shapes. This closes a previously-documented known
  gap (the missing `SdcValidationReport` ↔ `QaReport` bridge).

### Test coverage strategy

Per project policy, every new rule needs a passing MSTest proof test in
`SDC.Schema.QA.Tests`, using only the public `SDC.Schema.dll` API surface.
For this feature specifically:

1. **SDAC test(s)** — an SDAC LI selected while a descendant LI/Response
   still carries a captured value → expect a `Warning`-severity finding
   referencing both nodes and stating that data is at risk.
2. **SDS test(s)** — an SDS LI selected while a sibling LI (with descendant
   data) is also selected → expect a `Warning`-severity finding referencing
   the deselected sibling and its at-risk descendant data.
3. **Ad-hoc/custom-rule extensibility proof test** — a throwaway,
   test-only `ISdcCoherenceRule` implementation registered via
   `SdcCoherenceRuleRegistry.Register(...)`, proven to fire through
   `ValidateTree()`/the event stream/the QA bridge exactly like the built-in
   rules. This is the test that actually proves the "future extensibility"
   claim — not just SDAC/SDS behavior — and is treated as first-class
   required coverage, not an afterthought.

All three run through both surfaces (`ValidateTree()` and, once the
`BP-VAL-002` bridge lands, the `QaEngine` report) to confirm the two tracks
stay in sync.

