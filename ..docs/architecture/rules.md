# Rules Engine

> **Status:** Living document, migrated from the public GitHub gist "SDC OM Rules: built-in
> SDAC/SDS, ad hoc custom rules, future FDF-embedded rule roadmap". See also
> [validation.md](validation.md) (the companion document) and the design-history drafts in
> [../changes/](../changes/).

## Summary

`SDC.Schema` (the SDC Object Model / "SDC-OM" library) currently has **no
mechanism** for detecting or reacting to certain business-level tree
"coherence" rules defined by the SDC Technical Reference Guide (TRG) §8 — most
notably **SDAC** (Selection Disables Children) and **SDS** (Selection
Deselects Siblings). These rules describe how selecting a `ListItem` should
affect the activation/selection state of other nodes in the tree, and — more
importantly for data safety — when a selection is about to make previously
captured user data unreachable/inconsistent.

This document describes a small, deliberately minimal extensibility seam —
`ISdcCoherenceRule` plus a static `SdcCoherenceRuleRegistry` — that:

1. Ships **two built-in rules now** (SDAC and SDS) that detect when a
   selection puts existing user-entered data "at risk" (not destroyed —
   just flagged, so a client UI can offer the user an undo before anything
   is lost).
2. Is **already extensible today** for client-app-supplied custom rules
   (e.g., an organization's own "cancer staging correctness" checker) —
   register your own `ISdcCoherenceRule` implementation, no `SDC.Schema`
   code changes needed.
3. Is **forward-compatible** with a future capability to load rules that
   travel embedded inside the FDF/document itself (as C# source or
   Base64-encoded IL) — without committing to building that capability now.

This is explicitly **not** a general rules engine, and it is explicitly
**not** an XML/declarative predicate-expression language. SDC-OM already has
schema-level constructs that *could* have supported a declarative rules
language (`RulesType`, `PredGuardType`, etc.) — see "Relationship to the
existing (unused) SDC-OM Rules schema types" below — but the decision made in
this project is to support only **C# source and Base64-encoded IL** as
embeddable rule payloads, consistent with a WASM-hosted precedent already used
elsewhere in the solution, and to keep the core registry mechanism itself
free of any particular rule-authoring-language assumption.

## User's Guide

### Running the built-in SDAC/SDS checks

Both built-in rules run automatically whenever you call `ValidateTree()` on a
tree's top node (the same entry point used for ordinary DataAnnotations and
coherence validation):

```csharp
ITopNode topNode = /* your deserialized or in-memory SDC tree */;
SdcValidationReport report = topNode.ValidateTree();

foreach (var issue in report.Issues.Where(i => i.RuleCode is "SDAC" or "SDS"))
{
    Console.WriteLine($"[{issue.Severity}] {issue.RuleCode}: {issue.Message}");
}
```

`ValidateTree()` is called automatically during deserialization sweeps in the
same way existing coherence checks are, and can also be called on-demand at
any point (e.g. before saving/submitting a form).

### What SDAC/SDS actually detect

Both rules are about **protecting user data from silent loss**, not about
"illegal states" — an SDC form is allowed to have an SDAC/SDS-selected LI; the
rule only fires when there is also **pre-existing user data downstream that
would become orphaned or discarded** as a result of that selection.

- **SDAC (Selection Disables Children)** — when a `ListItem` with
  `selectionDisablesChildren = true` becomes `selected`, **all of that LI's
  descendants** become inactive. The rule walks those descendants looking for
  existing user data (`@val`, any populated response value, or a nested LI's
  own `selected == true`). If found, it raises a `Warning`-severity finding
  naming the LI and how many descendants hold at-risk data.
- **SDS (Selection Deselects Siblings)** — when a `ListItem` with
  `selectionDeselectsSiblings = true` becomes `selected`, all **sibling**
  LIs under the same parent question get auto-deselected. The rule walks
  *those siblings'* descendants (not the SDS LI's own descendants — a
  different node set than SDAC) for the same kind of at-risk user data, and
  raises a `Warning`-severity finding per affected sibling.
- **Both are `Warning`, not `Error`, on purpose.** The user may have selected
  the LI by mistake; a client UI needs the opportunity to let them undo the
  selection before anything is actually discarded. This mechanism only
  *flags* the risk — it never deletes, blocks, or auto-corrects anything.
- **No finding is raised if there's no at-risk data.** Selecting an
  SDAC/SDS-flagged LI with empty descendants/siblings is completely
  unremarkable and produces no output.

### Registering your own custom rule

Any consumer of `SDC.Schema` — including a client application, not just
`SDC.Schema` itself — can add a fully custom tree-coherence rule with no
library changes:

```csharp
public sealed class CancerStagingRule : ISdcCoherenceRule
{
    public string RuleId => "ORG-CancerStaging-001";

    public IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode)
    {
        // Walk topNode, apply your organization-specific logic, and yield
        // SdcNodeValidationIssue instances for anything worth flagging.
        // This is ordinary C# — no special rule DSL, no registration file,
        // no schema changes.
    }
}

// Once, at application startup:
SdcCoherenceRuleRegistry.Register(new CancerStagingRule());

// From then on, every ValidateTree() call (and, if wired, the real-time
// event stream) picks up your rule automatically alongside SDAC/SDS.
```

### On-demand vs. real-time vs. (de)serialization-time

| Surface | When it runs | Status |
|---|---|---|
| `ValidateTree()` | On-demand call, or automatically during (de)serialization sweeps | **Implemented** — covers SDAC/SDS + any custom registered rule |
| `SdcValidationEvents.ValidationOccurred` (real-time, e.g. the instant a `selected` setter changes) | Immediately on mutation | **Deferred** — would require an explicit, approved one-line hook in the auto-generated `ListItemBaseType.selected` setter. Not wired yet; `ValidateTree()` sweeps are the only mechanism today. |
| `SDC.Schema.QA`'s `QaEngine`/`IQaRule` (`BP-VAL-002`) | On-demand, alongside all other QA rules | **Bridged** — wraps `SdcCoherenceRuleRegistry`'s output as `QaFinding`s so the same rule shows up in both the core validation report and the QA Markdown/HTML report. See the companion [Validation](validation.md) chapter for details. |

## Technical Explanation

### `ISdcCoherenceRule` and `SdcCoherenceRuleRegistry`

```csharp
public interface ISdcCoherenceRule
{
    string RuleId { get; }
    IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode);
}

public static class SdcCoherenceRuleRegistry
{
    public static void Register(ISdcCoherenceRule rule);
    public static void Unregister(string ruleId);
    internal static IReadOnlyList<ISdcCoherenceRule> ActiveRules { get; }
}
```

`ValidateTree()` calls `Evaluate(topNode)` on every registered rule as part of
its existing tree-wide sweep, alongside the pre-existing DataAnnotations and
constraint-coherence checks, and funnels each resulting
`SdcNodeValidationIssue` through the same `SdcValidationEvents`/
`SdcValidationReport` plumbing already used for every other kind of
validation finding in `SDC.Schema` — so a coherence rule is a first-class
citizen of the existing reporting pipeline, not a bolted-on side channel.

`SdacRule`/`SdsRule` are registered by default as the first two built-in
rules. This generalizes the existing `SdcValidationRuleRegistry` precedent
(which is scoped only to per-property `ValidationAttribute` overrides) to
full tree-context procedural rules.

### Why this design, and not an off-the-shelf C# rules engine

Evaluated NRules (Rete-algorithm, rules compiled into the host app),
RulesEngine.NET (Microsoft OSS, JSON-expression rules), and Nected (paid SaaS,
remote rule evaluation). None of them solve SDC-OM's differentiating
requirement: a rule that travels *embedded inside the serialized document
itself*, across XML/JSON/BSON/MsgPack, executed by whatever host later loads
that document — all three assume the executing application owns the rule set
(compiled-in, local config, or a remote call), not the document instance.
Given SDC-OM's current scale (2 built-in rules + a handful of QA rules), the
performance case for something like NRules' Rete algorithm (which pays off
with hundreds of interdependent forward-chaining rules) doesn't apply either.
The lightweight in-house registry is the right fit; an off-the-shelf engine
could still be added later as *just another registered rule source* if scale
ever demands it, without reshaping anything built now.

### Future roadmap: FDF-embedded / externally-hosted rules (not yet built)

The registry design is deliberately agnostic about *how* a given
`ISdcCoherenceRule` implementation was produced or loaded — it only cares
about the `RuleId` + `Evaluate(ITopNode)` shape. That means two future
adapters (not implemented this session) can plug into the *same* registry
without any core-plumbing changes:

- **`ScriptedCSharpRuleAdapter`** — compiles C# source embedded in the FDF
  (via Roslyn scripting) into a delegate matching `Evaluate`'s signature, and
  registers it. Trust boundary: this executes FDF-supplied source at runtime,
  so it should only run when the host explicitly opts in — never silently on
  every deserialize.
- **`ScriptedIlRuleAdapter`** — loads a Base64-encoded, precompiled assembly
  embedded in the FDF and does the same. If hosted inside a WASM sandbox
  (per the existing solution precedent), this is the safer of the two from a
  trust-boundary standpoint.

A rule's need to call an external service (e.g. a REST API) is **not** a
separate execution mechanism — it's simply something a C#-source or IL rule's
`Evaluate(...)` body does internally (an ordinary HTTP call), the same as any
other I/O a rule might perform. There is no separate "external rule" adapter
concept in this design.

### Relationship to the existing (unused) SDC-OM Rules schema types

SDC-OM's XSD-derived object model already contains an entire, currently
**unexecuted** family of "Rules" schema types (`RulesType.Items` — holding
`AutoActivation`/`AutoSelection`/`ConditionalActions`/`ExternalRule`/
`ScriptedRule`/`SelectMatchingListItems`/`Validation` elements — plus the
recursive `PredGuardType` predicate-guard mechanism used elsewhere). None of
this is deserialized-and-run by `SDC.Schema` today; it's purely data-carrying,
auto-generated schema classes. An analysis of these types (three "buckets" —
simple name/data lookups, a fixed-vocabulary predicate set, and the actual
recursive general-purpose predicate-expression engine) is tracked in
[GitHub issue #29](https://github.com/rmoldwin/SDC_ObjectModel/issues/29),
which is the designated home for a future "major surgery" pass deciding what
(if anything) of that schema-level machinery is worth keeping alongside the
`ISdcCoherenceRule` mechanism described in this document. No implementation
work on that front has started.

