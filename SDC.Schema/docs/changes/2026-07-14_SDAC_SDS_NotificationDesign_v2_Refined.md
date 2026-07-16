# SDAC/SDS Notification Mechanism — Design Options (refined draft, 2026-07-14)

> **Status:** Superseded planning document. This is the refined design-exploration pass (see
> the earlier draft in this same folder for the original framing) that preceded the final
> design captured in [../architecture/rules.md](../architecture/rules.md) and
> [../architecture/validation.md](../architecture/validation.md). Kept for change history only —
> do not treat as current guidance.
# SDAC/SDS Notification Mechanism — Design Options

## Problem statement (refined per user clarification, 2026-07-14)

Neither TRG §8 business rule is enforced or even detected anywhere in `SDC.Schema` today.
**Refined semantics** (superseding the original "violation to detect" framing below — this
is now about flagging *at-risk user data*, not "illegal simultaneous states"):

- **SDAC (Selection Disables Children)** — `ListItemBaseType.selectionDisablesChildren`
  (bool, auto-generated attribute): when the LI is `selected`, **ALL** descendants of that
  LI become inactive — this overrides every other rule; SDAC always wins. The check to run:
  when an SDAC LI transitions to `selected`, walk *all* its descendant
  `IdentifiedExtensionType` (LI/Response) nodes and look for existing user-entered data
  (`@val`, or any response value of any data type, or a descendant LI's own `selected == true`).
  **Why this is a `Warning`, not an `Error`, and why we don't just discard the data:** the
  user may have selected the SDAC LI by mistake, and the client UI needs the opportunity to
  let them undo the selection before any descendant data is destroyed. This session's scope
  is detection/notification only — nothing here deletes or blocks anything.
- **SDS (Selection Deselects Siblings)** — `ListItemBaseType.selectionDeselectsSiblings`
  (bool, auto-generated attribute): when the LI is `selected`, all sibling LIs under the same
  parent QM get auto-deselected by the DEF. The check to run: when an SDS LI transitions to
  `selected`, for each sibling LI that would be auto-deselected as a result, walk *that
  sibling's* descendants and look for existing user-entered data (same `@val`/response/`selected`
  check as SDAC). Same reasoning as SDAC: this is a `Warning` because the user may want to
  undo the SDS selection before sibling data is discarted, and undo should still be possible
  in the client UI.
- **Key structural difference between the two** (do not conflate them): SDAC operates on
  **all descendants of the SDAC LI itself**; SDS operates on **the sibling LIs that get
  auto-deselected, and their descendants** — a different node set entirely. Each rule's
  `Evaluate(...)` must walk a different part of the tree.
- **Message content:** the finding is only interesting when descendant data actually exists.
  If an SDAC/SDS-triggering selection occurs but no descendant/sibling data is present, no
  finding should be raised (nothing is at risk). When data *is* found, the message text should
  explicitly say so — e.g. "SDAC selection of LI '{name}' will deactivate N descendant(s)
  that currently hold user-entered data; consider allowing undo before this data is
  discarded" (SDAC) vs. "SDS selection of LI '{name}' will auto-deselect sibling LI '{name2}',
  which currently holds user-entered data in M descendant(s); consider allowing undo before
  this data is discarded" (SDS) — so the two rules' findings are visibly distinguishable in
  a report even though both are `Warning` severity.
- **Out of scope, deferred:** actually preventing edits to already-inactive descendants
  (the "user tries to add data to a child while an ancestor SDAC LI is selected" prospective
  case) is NOT implemented this session — it would require hooking every descendant IET
  setter, which is exactly the kind of real-time enforcement this session explicitly defers.
  `ValidateTree()`-based detection (the retrospective/sweep case) is the only mechanism built
  now.

This session's scope remains **detection/notification only** — flagging the at-risk-data
condition to a client and/or a validation report. Actually *correcting* the tree
(auto-deselecting siblings, disabling children, discarding data) is explicitly deferred to a
later design conversation.

The question to resolve now: **what mechanism notifies "this needs attention" — and how
does that mechanism also feed a consolidated validation report** (both at
(de)serialization time and on-demand), so future rules beyond SDAC/SDS can reuse the
same pipeline instead of every new rule inventing its own reporting shape.

## Existing infrastructure this must fit into (read-only research, no changes)

`SDC.Schema` already has **two parallel, currently-disconnected** reporting systems:

1. **Core-library, hand-authored, "coherence" plumbing** (all in `SDC.Schema`,
   non-auto-generated files):
   - `SdcValidationEvents.ValidationOccurred` — a **static**
     `EventHandler<SdcValidationEventArgs>`. `SdcValidationEventArgs` carries
     `NodeID`, `PropertyName`, `AttemptedValue`, `Message`,
     `SdcValidationSeverity` (Info/Warning/Error), `Results`. The `Raise(...)`
     methods are `internal` — only `SDC.Schema` (and `SDC.Schema.Tests`, via
     `InternalsVisibleTo`) can raise it. `SDC.Schema.QA` (public-API-only, by
     design) **cannot** raise this event.
   - `SdcValidationReport` / `SdcNodeValidationIssue` — an aggregated report
     (Issues, ErrorCount/WarningCount/InfoCount, IsValid, Summary). Built and
     returned by `ValidateTree()` (`SdcValidate.cs`), which walks the whole tree.
   - `BaseType.RejectedValues` (`SdcRejectedValue`: PropertyName, AttemptedValue,
     Message, RejectedAt, Results) — an out-of-band, never-serialized,
     per-node bag for values a setter refused to actually store. **Semantically
     wrong fit for SDAC/SDS**: nothing is "rejected" here — the selection *is*
     applied, we just want to flag that it's now in conflict with another node.
   - The existing pattern that ties all of this together is
     `SdcValidate.Coherence.cs`'s private `RaiseCoherenceViolation(...)`: one
     function that (a) records a rejected value, (b) adds an issue to the
     ambient `SdcUtil.ValidationCollector` (feeding whatever `SdcValidationReport`
     is currently active, e.g. from `ValidateTree()`), and (c) raises
     `SdcValidationEvents.ValidationOccurred` — all three, from one call site,
     gated by `SdcUtil.SuppressValidation` (so it's automatically silent during
     raw deserialization and only active for real mutations / explicit
     `ValidateTree()` sweeps).
   - This machinery is invoked today from property **setters** (mostly
     auto-generated, calling into hand-authored coherence-check helpers) and from
     `ValidateTree()`'s tree-wide sweep.

2. **`SDC.Schema.QA` rules engine** (separate project, public-`SDC.Schema.dll`-API
   only, by hard architectural constraint): `IQaRule.Evaluate(ITopNode)` yields
   `QaFinding` records (RuleId, Convention, Severity, NodeId, NodeObjectGuid,
   Message, Recommendation, IsAutoFixable) into a `QaReport` with
   `ToMarkdown()`/`ToHtml()`. Rules run **on demand** (the client calls
   `QaEngine.Run(topNode)` whenever they choose — e.g. after a mutation batch, or
   after deserializing). There is **no automatic invocation** and **no bridge**
   today between `SdcValidationReport` and `QaReport` — they are two unrelated
   result shapes (this is already known-gap #1/#17 in `07-known-gaps-and-future-work.md`).

Navigation available for the actual SDAC/SDS check (all public API,
non-auto-generated helpers): `GetChildNodes()`/`GetSubtreeList()` (descendants),
`ParentNode` + sibling iteration (siblings), all in `INavigateExtensions`/
`IChildItemsParentExtensions`.

## Options for the notification mechanism

### Option 1 — Reuse `SdcValidationEvents` as-is, no new event type
Add a new coherence-check function (SDAC/SDS-specific) inside `SDC.Schema`,
following the *exact* `RaiseCoherenceViolation` pattern: raise
`SdcValidationEvents.ValidationOccurred` with `Severity = Warning` (not Error —
the action isn't blocked, just flagged) and route the same issue into
`SdcUtil.ValidationCollector` so it automatically shows up in any active
`SdcValidationReport` (including from `ValidateTree()` and from
(de)serialization if we choose to run the check during `ReflectRefreshTree`/post-load).
- **Pros:** Reuses 100% existing, already-proven, already-documented plumbing.
  Zero new concepts for existing consumers of `SdcValidationEvents`/`ValidateTree()`
  to learn. Automatically gives you "real-time event" + "on-demand via
  `ValidateTree()`" + "(de)serialization-time" for free from ONE function, exactly
  matching what you described wanting.
- **Cons:** `SdcValidationEventArgs` has no "rule kind" field today — a client
  wanting to special-case "this is an SDAC violation, not just any old coherence
  issue" has to pattern-match on `Message`/`PropertyName` text, which is fragile.
  (Small additive fix: add an optional `RuleCode`/`Category` string to
  `SdcValidationEventArgs`/`SdcNodeValidationIssue` — non-breaking, since these
  are classes with `init` properties, not positional records.) Also: because
  `Raise()` is `internal`, this check must live inside `SDC.Schema` core, not in
  `SDC.Schema.QA` — meaning **real-time** notification (the instant `selected` is
  set) requires a one-line call added to the auto-generated `selected` setter in
  `ListItemBaseType.cs`, which needs your **explicit permission** per the
  auto-generated-file policy.

### Option 2 — New dedicated event/report type just for tree-context "business rule" violations (parallel to, not reusing, `SdcValidationEvents`)
E.g. `SdcCoherenceEvents.SelectionRuleViolated`, with a richer purpose-built
payload: `ViolatingNode`, `ConflictingNode` (the sibling/ancestor actually in
conflict — not just a string message), `RuleKind` enum `{Sdac, Sds, ...}` for
easy future extension as more TRG business rules get added later.
- **Pros:** Cleaner separation of concerns (per-property DataAnnotations
  validation vs. cross-node business-rule coherence are genuinely different
  categories). Richer payload is more directly useful to a client UI (e.g.
  highlight both nodes in conflict, not just the one that changed). Scales
  better if more TRG business rules get added over time — a dedicated small
  framework grows more gracefully than overloading one generic string-message
  event forever.
- **Cons:** A second static event hub — more surface area, more for consumers
  and docs to track ("which event do I subscribe to?"). More new code/tests
  than Option 1. Still requires the same auto-generated-setter permission for
  real-time triggering.

### Option 3 — `SDC.Schema.QA` rule only, no runtime hook in `SDC.Schema` at all
A new `BP-VAL-002` rule (`SdacSdsCoherenceRule`), same shape as the existing
`BP-MUT-001`/`BP-VAL-001` rules: detection-only, `TryFix` returns false
(auto-fixing requires a business decision about which selection "wins" — same
reasoning already documented for `BP-MUT-001`). Client calls
`QaEngine.Run(topNode)` whenever they want a check (after a mutation batch,
after deserializing, on a schedule, etc.).
- **Pros:** Zero risk to `SDC.Schema` itself. **No auto-generated files touched,
  no permission needed** — ships fastest. Matches the QA project's existing,
  already-proven "detection-only, public-API-only" convention exactly.
- **Cons:** No true real-time push — the client must remember to re-run the QA
  engine after mutations (same opt-in-discipline caveat the guide already
  documents for `ValidateTree()`/`WouldBeValid()`). Findings land only in
  `QaReport`, not in `SdcValidationReport` — "consolidated" only within the QA
  world, not unified with core-library (de)serialization-time validation, unless
  we also build the (currently-nonexistent) bridge between the two report types.

### Option 4 — Hybrid: one shared checker, exposed through every existing surface (recommended)
Write the actual SDAC/SDS detection logic **once**, as a new hand-authored
(non-auto-generated) function in `SDC.Schema` (e.g.
`SdcValidate.CheckSdacSdsCoherence(BaseType node)` or tree-wide
`CheckSdacSdsCoherence(ITopNode)`), following the existing
`RaiseCoherenceViolation` pattern (Option 1's plumbing). Then expose it through
**three** surfaces that already exist, instead of inventing a fourth new concept:
1. **`ValidateTree()`** calls it automatically as part of its tree-wide sweep —
   covers (de)serialization-time and any explicit on-demand call. **No
   auto-generated files touched, no permission needed** for this part alone.
2. **`SdcValidationEvents`** (Option 1's mechanism) fires in real time —
   *only* if you authorize a one-line call added to the auto-generated
   `selected` setter in `ListItemBaseType.cs` (the only realistic trigger point
   for "the instant a selection changes"). This part is optional/deferrable.
3. **A thin `SDC.Schema.QA` rule (`BP-VAL-002`)** that simply calls
   `ValidateTree()` (or a small public wrapper around the same checker) and maps
   `SdcNodeValidationIssue` → `QaFinding` — which incidentally **closes
   known-gap #1/#17** (the missing `SdcValidationReport` ↔ `QaReport` bridge) as
   a side effect, something you'll likely want eventually regardless of this
   feature.
- **Pros:** Single source of truth for the detection logic — no risk of the
  "event version" and "QA rule version" silently diverging over time. Directly
  matches your stated goal ("(de)serialization validation process, as well as
  an on-demand validation process... consolidated way"). Naturally extensible:
  the *next* TRG business rule (beyond SDAC/SDS) reuses the same three-surface
  wiring for free.
- **Cons:** Largest scope of the four options (touches 3 layers, though each
  individually is small). The real-time piece (item 2) still needs your
  explicit sign-off to touch the auto-generated `selected` setter — but it's a
  **one-line, additive call site** (`if (value) SdcValidate.CheckSdacSdsCoherence(this);`),
  not a rewrite of setter logic, in the same spirit as the small, already-approved
  `XmlAttributeListJsonConverter` registrations from issue #27.

### Option 4b — Same as Option 4, but defer the auto-generated-setter edit entirely
Ship items 1 and 3 now (checker + `ValidateTree()` integration + `BP-VAL-002` QA
rule) with **zero auto-generated files touched**. Real-time notification is
achieved instead by the client explicitly calling a new public method (e.g.
`li.CheckSdacSdsCoherence()`) right after setting `selected = true` — the same
"opt-in, call it yourself" pattern the guide already documents for
`WouldBeValid()`. Revisit the auto-generated setter hook later, once the
detection logic itself has some real-world mileage.
- **Pros:** Everything ships with zero permission-gated work, fastest safe
  path; the design is provably correct/tested before we touch any generated
  code.
- **Cons:** No truly automatic real-time push notification until/unless the
  setter hook is added later — client must remember to call the checker
  explicitly after each selection change (weaker guarantee than Option 4 full).

## Extending toward future template-embedded / client-app-supplied custom rules

The user's follow-up question: don't just solve SDAC/SDS — make sure whichever
option we pick can grow, in later iterations, into supporting (a) rules
embedded IN the FDF/template itself (potentially as C#/IL-ish code), and
(b) rules maintained and supplied by a client app (e.g. an org-specific
"cancer staging correctness" checker), **without** committing to building a
full rules engine right now.

### Key discovery: the SDC standard already anticipates all three rule "shapes"

Investigated `RulesType` and its schema-defined member types
(`SDC Unmodified Classes/RulesType.cs` + siblings). `RulesType.Items` is a
`List<ExtensionBaseType>` that can hold, per node, any mix of:

| Schema element | Backing type | Shape | Maps to |
|---|---|---|---|
| `ScriptedRule` | `ScriptCodeAnyType` : `ScriptCodeBaseType` | plain **`language`** (string) + **`code`** (string) attributes, plus `returnVal`/`returnList`/parameters | **"code embedded in the FDF"** — kept per your feedback, narrowed to C# source and Base64-encoded IL as the two `language`/encoding values actually used |
| `ExternalRule` | `CallFuncType` : `CallFuncBaseType` | a `Item` (`anyURI_Stype` — a URI/endpoint reference), `ItemElementName` discriminator, `Security`, and an `Items` parameter list | **"rule maintained/hosted by a client app"** — kept; the endpoint is expected to be a C#-authored function/service, not a rule-DSL host |
| `Validation` | `ValidationType` → `SelectionTest`/`SelectionSets`/`ItemAlternatives` | fully **declarative** (no code/URI at all — just data) | **deprioritized per your feedback** — you doubt anyone wants an XML-based rule language |
| `AutoActivation`/`AutoSelection`/`ConditionalActions` | `RuleAutoActivateType`/`RuleAutoSelectType`/`PredActionType` | declarative if/then/assertion-style rule shapes | **deprioritized per your feedback**, same reasoning |

**None of these are executed anywhere in `SDC.Schema` today.** They are purely
data-carrying (Xsd2Code++-generated) schema elements — nothing deserializes a
`RulesType.Items` entry and *runs* it. This is the other side of known-gap #6
(`AddRule_()` throws `NotImplementedException`): that gap is about the
**builder** side; there is currently no **executor** side either. So this
isn't a small missing piece — it's an entire not-yet-started subsystem, and
we're free to design the executor-facing shape from scratch.

**Note on scope of "removing Rules support":** I'm reading your feedback as
"deprioritize building any execution/adapter support for the declarative
element types (`Validation`, `AutoActivation`, `AutoSelection`,
`ConditionalActions`, `SelectMatchingListItems`)" — i.e. we simply never write
an interpreter for them — **not** "delete the auto-generated schema classes
themselves." Those classes are part of the standard XSD-derived object model
(auto-generated, `<auto-generated>` header) and removing them would be a much
bigger, separate, schema-conformance-affecting decision requiring its own
explicit discussion. Flagging this distinction explicitly as an open question
below in case you meant something more literal.

### Compatibility check against your narrowed direction (C# source + Base64 IL, AI-assisted authoring, both embedded and external)

**Yes, still fully compatible — and it actually simplifies the future adapter
roadmap.** The `ISdcCoherenceRule` seam I'm proposing was deliberately designed
to be agnostic about *how* a rule got compiled/loaded — it only cares about the
end result being something with a `RuleId` and an
`Evaluate(ITopNode) -> IEnumerable<SdcNodeValidationIssue>` method. That
end-state shape is *exactly* the natural signature for a C# rule function
anyway (AI-assisted or hand-written) — "given the tree, return a list of
issues" is precisely what a `CancerStagingRule.Evaluate(...)` or a
`SdacRule.Evaluate(...)` method body already looks like. Concretely, this
narrows the *future* adapter work (still explicitly out of scope for this
session) from "4+ different declarative-element interpreters" down to just
two, mirroring the WASM-hosted precedent you mentioned from the other solution
projects:

- **`ScriptedCSharpRuleAdapter`** — compiles `ScriptCodeAnyType.code`
  (`language == "C#"` or similar) via Roslyn scripting
  (`Microsoft.CodeAnalysis.CSharp.Scripting`) into a delegate matching
  `ISdcCoherenceRule.Evaluate`'s signature, wraps it, registers it. Trust
  boundary: this executes FDF-supplied source at runtime — should only run
  when the host explicitly opts in per template/source (never silently on
  every deserialize).
- **`ScriptedIlRuleAdapter`** — loads a Base64-encoded, precompiled assembly
  (`ScriptCodeAnyType.code` holding the Base64 payload, `language`/`objectFormat`
  flagging it as IL) via `Assembly.Load`/`AssemblyLoadContext`, locates the
  expected entry point, wraps it the same way. If your WASM precedent means
  the IL actually runs inside a WASM sandbox (e.g. via a WASM runtime hosted
  in-process) rather than a plain in-process `AssemblyLoadContext`, that's
  even better from a trust-boundary standpoint — same adapter shape either way,
  just a different loader/host underneath.
- **`ExternalRuleAdapter`** (for `CallFuncType`) — resolves the referenced
  URI/service (itself presumably a C# function/service, per your note) and
  invokes it, mapping its response into the same `Evaluate(...)` result shape.
  Naturally sandboxed already since it's just an out-of-process call the host
  controls.

**Update per your item 7 feedback (2026-07-14):** you pointed out that invoking
an external service is really "just another kind of C# code" and questioned
whether it needs its own separate SDC node/handling. Agreed — folding this in
simplifies the picture further: `ExternalRuleAdapter` isn't a *third*
execution mechanism, it's simply what the C# code inside a
`ScriptedCSharpRuleAdapter`-compiled rule *does internally* (an ordinary
`HttpClient` call, or any other I/O) — no different in kind from a rule that
reads a local file or queries a database from within its `Evaluate(...)` body.
So there are really only **two** execution mechanisms worth planning for, not
three: **C# source** (`ScriptedCSharpRuleAdapter`) and **Base64-encoded IL**
(`ScriptedIlRuleAdapter`) — and whether a given rule's logic happens to call
out to an external service is just an implementation detail *inside* one of
those two, not a distinct adapter/schema concept. `CallFuncType`/`ExternalRule`
itself doesn't need a dedicated adapter at all under this reading — it can be
treated as schema-level data that a `ScriptedCSharpRuleAdapter`-hosted rule
*chooses* to read (e.g. "the URI to call") rather than something SDC-OM
executes on the rule author's behalf. **Flagged as open question #7 below** in
case you meant something more specific by "external rule" that this
simplification doesn't capture.

All adapters, whenever they're eventually built, register into the same
`SdcCoherenceRuleRegistry` and flow through the same `ValidateTree()`/
`SdcValidationEvents`/`SdcValidationReport`/QA-bridge pipeline as SDAC/SDS —
no core-plumbing change needed when that later work happens. **This session's
actual deliverable (SDAC/SDS as the first two built-in rules) is unaffected by
any of this** — it's proof that the registry seam pays for itself immediately,
independent of which future adapters get built on top of it.

## Which existing SDC-OM declarative Rules constructs are simple enough to keep? (per your item 8)

Per your request, evaluated every schema-level "Rules"-related type
(`RulesType.Items`, `PredGuardType.Items`, and their nested member types) to
identify which are **simple enough to fire directly from an event/guard
without any predicate-expression evaluation of any kind** — i.e. candidates
worth retaining even after a future "major surgery" pass removes the general
predicate-expression machinery. None of this is being implemented or removed
this session — this is purely the requested inventory/analysis, captured here
and destined for a known-gap doc entry + a future GitHub issue (see below).

**Bucket A — pure data, no evaluator needed beyond direct name/property
lookups (strong keep candidates):**

| Type | Shape | Why it's simple |
|---|---|---|
| `RuleAutoActivateType` (`AutoActivation`) | `selectedItemSet` (space-delimited named-item watch list, `-`-prefix = "watch for deselected") + `targetNameActivationList` (named items to activate/deactivate, `-`-prefix = deactivate) + `setVisibility`/`setEnabled`/`setExpanded` toggles + `onlyIf` | Just "are all these named items in the expected selected/deselected state? If so, toggle these other named items' activation flags." No boolean-tree composition, no code. **This is structurally almost identical to what we're about to hand-build for SDAC** — worth remembering as a possible future *generalization* of the SDAC/SDS rules once they've proven out. |
| `RuleAutoSelectType` (`AutoSelection`) | Same shape as above but for `targetNameSelectList` (select/deselect instead of activate/deactivate) | Same reasoning. **This is structurally almost identical to what we're about to hand-build for SDS** — same note as above. |
| `RuleSelectMatchingListItemsType` (`SelectMatchingListItems`) | `MatchSource` (an item+attribute reference) + `ListItemMatchTargets` (`MatchListItems`/`MatchQuestions` name lists + `attributeToMatch` enum) | "Copy/mirror a selection based on an attribute-value match" — bounded name/attribute-equality matching, no expression syntax. |
| `PredAlternativesType` (`ItemAlternatives`) | `itemNames` (named-item list) + `minAnswered`/`maxAnswered` counts + `not` | Pure counting logic ("at least N of these M named items must be validly answered") — no expression language. |
| `PredSingleSelectionSetsType` (`SelectionSets`) / `PredSelectionTestType` (`SelectionTest`, adds `testItemName`) | `listItemNames` + `maxSelections` count, or `testItemName` + `listItemNames` presence check | Pure counting/presence logic over named items — same bucket as above. This is exactly the shape of a generic "mutual exclusion" or "SDS-like" check. |

**Bucket B — a fixed, bounded vocabulary of named predicates (still no
free-form expression syntax, moderate but tractable complexity):**

| Type | Shape | Why it's still tractable |
|---|---|---|
| `PredEvalAttribValuesType` (`AttributeEval`) | `itemNames` + a **fixed enumerated set** of ~15 named boolean/count checks (`isSelected`, `isActive`, `hasSelectionsGTE/LTE/Exact`, `hasResponse`, `isVisible`, `isEnabled`, `isRequired`, `isReadOnly`, `hasType`, `hasStyleClass`, `hasValue`, `supportDatesAndIntervals`, `hasPattern`) + `boolOp` (AND/OR across the checks that are set) + `not` | Looks like a "predicate language" at first glance, but it isn't one — there's no parser, no arbitrary syntax, just a closed set of named flags that map 1:1 onto simple property reads on the referenced node(s) (e.g. `isSelected` → `node.selected`, `hasResponse` → "does the response property differ from its default"). A single `switch`/dispatch function implements the whole thing. Worth keeping standalone (i.e. as a single, non-nested predicate), even if the surrounding recursive composition (Bucket C) is removed. |

**Bucket C — the part that actually IS a general expression/rules engine,
and is the right target for future "major surgery" / deprioritization:**

| Type | Shape | Why it's the real complexity |
|---|---|---|
| `PredGuardType` (the `Group`/`boolOp`/`not` recursive composition mechanism, used by `ConditionalActions`/`PredActionType`'s `If` guard and nested guards) | `Items` can itself contain another `PredGuardType` (`Group`), recursively, combined with `boolOp` (AND/OR) and `not`, mixing in any of the Bucket A/B leaf types **plus** `CallBoolFunc` (`CallFuncBoolType`) and `ScriptBoolFunc` (`ScriptCodeBoolType`) as boolean-returning leaves | This is the actual general-purpose recursive boolean-expression-tree evaluator — arbitrary-depth AND/OR/NOT nesting over a mix of leaf types. This is the part that would require building a real interpreter/tree-walker, and is the most natural target to deprioritize/remove in a future cleanup. Note: `CallBoolFunc`/`ScriptBoolFunc` **don't need separate removal** — under the item-7 simplification above, they're just the same C#-source/Base64-IL execution mechanism reused as a boolean-returning leaf, so they fold into whatever `ScriptedCSharpRuleAdapter`/`ScriptedIlRuleAdapter` ends up supporting anyway. The thing worth deprioritizing is specifically the *recursive Group/boolOp tree-building around arbitrary leaves* — not any single leaf type. |

**Bottom line recommendation for the future "major surgery":** the individual
leaf types in Buckets A and B are simple, useful, and worth keeping exactly as
they are (no code changes needed to keep them — they already exist and do no
harm sitting unused). What's worth removing/deprioritizing is narrowly the
generic recursive predicate-tree composition in `PredGuardType` (the
`Group`/nested-`boolOp` machinery) — everything else in the "Rules" family is
either already simple enough to implement directly as an `ISdcCoherenceRule`
someday, or (per item 7) folds into the same C#/IL execution mechanism you've
already decided to keep.

### Proposed architectural seam: generalize "the shared checker" into a tiny, registrable rule abstraction

Instead of writing SDAC/SDS detection as one bespoke hardcoded function
(as Option 4/4b originally described), define one small new interface and a
registry for it, in `SDC.Schema` core (new hand-authored files, no
auto-generated files touched):

```csharp
public interface ISdcCoherenceRule
{
    string RuleId { get; }              // e.g. "SDAC", "SDS", "ORG-CancerStaging-001"
    IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode);
}

public static class SdcCoherenceRuleRegistry
{
    public static void Register(ISdcCoherenceRule rule) { ... }
    public static void Unregister(string ruleId) { ... }
    internal static IReadOnlyList<ISdcCoherenceRule> ActiveRules => ...;
}
```

`ValidateTree()` runs every registered rule's `Evaluate(...)` as part of its
sweep (in addition to the existing DataAnnotations + coherence checks), and
funnels each resulting `SdcNodeValidationIssue` through the *same*
`SdcValidationEvents`/`SdcValidationReport` plumbing already used everywhere
else — so this is purely additive to Option 4/4b, not a competing design.

This one seam directly answers both parts of your question, without building
a rules engine now:

1. **SDAC and SDS become the first two built-in rules**, registered by
   default — e.g. `SdcCoherenceRuleRegistry.Register(new SdacRule())` /
   `new SdsRule()` in a static initializer. This is this session's actual
   deliverable; nothing about it is deferred or speculative.
2. **Client-app-supplied custom rules** (the "cancer staging correctness"
   example) are trivially "already extensible" the moment
   `ISdcCoherenceRule` exists: the host app writes an ordinary C# class,
   calls `SdcCoherenceRuleRegistry.Register(new CancerStagingRule())` once at
   startup, and from then on `ValidateTree()`, the real-time event stream,
   and (de)serialization-time checks (if wired) all pick it up automatically
   — **no `SDC.Schema` code changes required per new custom rule.** This is
   the generalization of the existing `SdcValidationRuleRegistry` precedent
   (today scoped to per-property `ValidationAttribute`s) to full tree-context
   procedural rules.
3. **FDF-embedded or externally-hosted rules** (per your narrowed direction:
   C# source via `ScriptedRule`, Base64-encoded IL via `ScriptedRule`, or a
   C#-authored external service via `ExternalRule`) become, later, simply
   *adapters* that implement `ISdcCoherenceRule` (see the three adapter
   sketches above: `ScriptedCSharpRuleAdapter`, `ScriptedIlRuleAdapter`,
   `ExternalRuleAdapter`). **Building any of these three adapters is
   explicitly out of scope for now** — flagged only to confirm the registry
   design doesn't need to change shape when we eventually build them. That's
   the "forward-compatible without committing to it now" answer you asked for,
   now narrowed to match your AI-assisted-C#-authoring direction instead of a
   4-way declarative-element interpreter set.
4. **`SDC.Schema.QA`'s `QaEngine`/`IQaRule` is already a working plug-in
   point today**, independent of any of the above — its constructor takes
   `IEnumerable<IQaRule>`, so a client app that only needs **on-demand**
   (not real-time/(de)serialization-time) custom checks can already write an
   `IQaRule` and pass it in, no core changes at all. What it *doesn't* give
   you (until the registry above exists) is automatic invocation during
   `ValidateTree()`/deserialization, or a rule whose *definition* travels
   inside the FDF itself. A thin bridge (`BP-VAL-002` wrapping
   `SdcCoherenceRuleRegistry`'s output as `QaFinding`s, as already planned in
   Option 4 item 3) means a rule author only ever writes **one**
   `ISdcCoherenceRule` implementation to show up in *both* the real-time
   event stream/`SdcValidationReport` *and* the QA Markdown/HTML report —
   avoiding duplicate implementations of the same logic in two different
   shapes.

Net effect: this costs a small, fixed amount of extra design/code now (one
interface + one static registry, on top of the SDAC/SDS check itself), and in
exchange the SDC-OM design is explicitly ready to grow into template-embedded
and client-app-supplied rules later, entirely by *adding* adapters/registrations
— never by reshaping the core plumbing again.

## Recommendation

**Option 4b, generalized through the `ISdcCoherenceRule`/`SdcCoherenceRuleRegistry`
seam above, rather than one hardcoded SDAC/SDS function.** This still ships
with zero auto-generated-file risk (SDAC/SDS wired only through `ValidateTree()`
+ the registry + a `BP-VAL-002` QA-rule wrapper), defers the one
policy-gated decision (editing `ListItemBaseType.cs`'s `selected` setter for
true real-time push) to an explicit follow-up ask once the detection logic has
some mileage, AND buys the future-extensibility story the user asked about —
for the cost of one small interface and one small static registry, both
hand-authored, both trivial to unit-test. Also recommend a small additive field
(`RuleCode`/`Category`, e.g. `"SDAC"`/`"SDS"`) on `SdcValidationEventArgs`/
`SdcNodeValidationIssue` so clients can filter/branch on rule identity instead
of message-string matching — low-risk, non-breaking (init-only properties).
The `ScriptedCSharpRuleAdapter`/`ScriptedIlRuleAdapter`/`ExternalRuleAdapter`
adapters described above are explicitly **not** part of this session's work —
they're the natural "next iteration" once `ISdcCoherenceRule` exists and
SDAC/SDS have proven it out, and are now scoped to your narrowed
C#-source/Base64-IL/external-C#-service direction rather than a broader
declarative-rule interpreter set.

## Should we adopt an off-the-shelf C# rules engine instead of building this into SDC-OM?

Researched (per your request) https://www.nected.ai/blog/c-sharp-business-rule-engine,
covering **NRules** (Rete-algorithm, C#-fluent-DSL, rules compiled into the app),
**RulesEngine.NET** (Microsoft OSS, rules as JSON strings evaluated at runtime),
and **Nected** (paid SaaS, visual rule builder, evaluated via remote HTTP call).

**Conclusion: build the lightweight functionality into SDC-OM
(`ISdcCoherenceRule`/`SdcCoherenceRuleRegistry`), do not take a dependency on
any of the three.** None of them solve SDC-OM's actual differentiating
requirement — a rule (C# source or compiled IL) that travels *embedded inside
the serialized document/FDF itself*, across XML/JSON/BSON/MsgPack, and is
executed by whatever host later loads that document. All three assume the
rule set is owned by the *executing application* instead: compiled into the
app (NRules), in a JSON file/config the app loads (RulesEngine.NET), or
hosted on a remote server the app calls (Nected). That's a different
architecture than "the document instance itself carries its own rule
payload."

| | NRules | RulesEngine.NET | Nected | Build into SDC-OM |
|---|---|---|---|---|
| Rule authoring | C# classes (fluent DSL), compiled into host app | JSON expression strings | Visual UI, hosted remotely | C# source / Base64 IL, embeddable in the FDF itself |
| Rule travels with the document? | No — owned by host app | No — owned by host app | No — owned by vendor platform | **Yes** — the whole point |
| New dependency/footprint | NRules NuGet + Rete engine | RulesEngine NuGet + JSON parser | Vendor SaaS, network calls, cost | None — hand-authored, ~2 small files |
| Rule language matches your stated direction (AI-assisted C#, no DSL)? | Partial — DSL is C#, but rules aren't portable data | No — it *is* a rule-string DSL, same tradeoff class as XML | No | **Yes** |
| Worth it at SDC-OM's current scale (2 built-in rules + a handful of QA rules)? | No — Rete's advantage is for hundreds of interdependent forward-chaining rules | No | No | Yes — matches actual scale |

**When to revisit:** only if rule count/interdependency later grows to a scale
where Rete-style incremental re-evaluation demonstrably matters (unlikely for
tree-shaped coherence checks like SDAC/SDS). Even then, an engine like NRules
could be added later as *just another adapter* registered behind
`ISdcCoherenceRule` — it wouldn't require reshaping anything built now.

## Testing strategy (regardless of which option is chosen)

Per your feedback, test coverage must prove out the registry/engine itself,
not just the two built-in rules:
1. **SDAC test(s)** — an SDAC LI selected while a descendant LI/Response still
   carries a captured value → expect a `Warning`-severity finding referencing
   both nodes.
2. **SDS test(s)** — an SDS LI selected while a sibling LI is also selected →
   expect a `Warning`-severity finding referencing both LIs.
3. **Ad-hoc/custom rule test** — a throwaway test-only `ISdcCoherenceRule`
   implementation (not shipped as a product feature) registered via
   `SdcCoherenceRuleRegistry.Register(...)` and proven to fire through
   `ValidateTree()`/the event stream/the QA bridge exactly like the built-in
   rules — this is the test that actually proves the "future extensibility"
   claim, not just the SDAC/SDS behavior.

All three run through both surfaces: `ValidateTree()` (on-demand +
(de)serialization-time) and, if Option 4 full is later approved, the
real-time `SdcValidationEvents` stream.

## Documentation deliverable: GitHub Gists (rules + validation)

Per your request, produce GitHub Gists (via `gh gist create`) covering both
**rules** (built-in and ad hoc) and **validation** (ad hoc, and during
(de)serialization). Each Gist should contain, at minimum:
- A **summary** section (what problem this solves, one paragraph).
- A **user's guide** (how to register a custom rule, how to run
  `ValidateTree()`/`QaEngine`, how to read the resulting report, what
  severities mean, how built-in SDAC/SDS rules behave).
- A **technical explanation** of how rules/validation work internally
  (`ISdcCoherenceRule`, `SdcCoherenceRuleRegistry`, `SdcValidationEvents`,
  `SdcValidationReport`, the `QaReport` bridge, and how the two
  (de)serialization-time and on-demand code paths converge on one pipeline).

These Gists are written **after the design is finalized** (so they document
the agreed architecture, not a moving target) but **before implementation
begins** — per your item 4, they become part of the context system that
implementation sub-agents read, instead of each agent needing the full design
conversation re-explained.

## Execution strategy once this plan is approved

Per your instruction, once approved, execute using **separate background
sub-agents per major work item**, so each keeps a clean, focused context:
1. First, a sub-agent (or done directly in this session, since it requires
   the full design context already built up here) authors the GitHub Gists
   above — these become shared context for every subsequent agent.
2. A sub-agent implements `ISdcCoherenceRule`/`SdcCoherenceRuleRegistry` +
   the `SdacRule`/`SdsRule` built-ins + `ValidateTree()` wiring (pointed at
   the Gists for context instead of a re-explained design brief).
3. A sub-agent implements the `BP-VAL-002` QA-rule bridge in
   `SDC.Schema.QA`.
4. A sub-agent writes the SDAC/SDS/ad-hoc-rule tests described above.
5. A sub-agent updates the local guide corpus
   (`guide/05-validation-coherence.md`, `07-known-gaps-and-future-work.md`,
   `INDEX.md`) to reflect the shipped design and link to the Gists.
Each sub-agent's prompt will point back at the Gists + this plan for full
context, per your request to keep things coherent across separate contexts.

## Decisions (resolved 2026-07-14 — plan approved, proceeding to execution)

1. **Option 4b** — confirmed. Zero auto-generated-file risk; real-time push deferred.
2. **`ISdcCoherenceRule`/`SdcCoherenceRuleRegistry`** — approved, build as designed.
3. **`RuleCode`/`Category` additive field** on `SdcValidationEventArgs`/
   `SdcNodeValidationIssue` — approved.
4. **Real-time `selected`-setter hook (Option 4 full)** — deferred; using 4b for now.
5. **Severity = `Warning`** — confirmed for both SDAC and SDS findings.
6. **SDAC/SDS detection scope — refined and finalized** (see "Problem statement"
   above for full detail): SDAC walks *all descendants of the SDAC LI itself*;
   SDS walks *the auto-deselected sibling LIs and their descendants*. Both only
   raise a finding when descendant/sibling user data (`@val`/response
   value/nested `selected`) actually exists, and the message text must state
   that data is at risk. Both are `Warning` severity specifically because the
   user must be able to undo the selection in the client UI before data is
   discarded — this is now the documented rationale, not just a default choice.
7. **`ExternalRuleAdapter` folded into the C#/IL execution mechanism** — no
   separate adapter/schema handling needed; calling an external service is just
   something a C#-source or IL rule's `Evaluate(...)` body does internally (an
   ordinary HTTP call), not a distinct rule *kind*. Only two future adapters
   remain on the roadmap: `ScriptedCSharpRuleAdapter`, `ScriptedIlRuleAdapter`.
8. **Declarative-rules "major surgery" scope** — confirmed as (a): never write
   interpreters for the recursive `PredGuardType` predicate-tree machinery;
   leave the auto-generated schema classes themselves untouched/in place (they
   cause no harm sitting unused). See the "Which existing SDC-OM declarative
   Rules constructs are simple enough to keep?" section above for the full
   three-bucket analysis (A: pure name/data lookups — keep; B: fixed-vocabulary
   predicate set — keep; C: recursive Group/boolOp tree composition — the
   actual future-removal target). **Action items**: file a GitHub issue
   capturing this as a future "major surgery" backlog item (not started this
      session), and add a corresponding SQL todo. **Done**: filed as
      [GitHub issue #29](https://github.com/rmoldwin/SDC_ObjectModel/issues/29).
9. **GitHub Gists** — two separate **public** Gists (one for rules, one for
   validation), each with a summary/user's guide/technical-internals structure
   as described above. Kept updated as the design evolves; more system-level
   Gists may be added later as additional context-control artifacts.

## GitHub Gists (published 2026-07-14)

- **SDC OM Rules**: https://gist.github.com/rmoldwin/4d13c24c25ffd472ddf4f77d91b62d5c
- **SDC OM Validation**: https://gist.github.com/rmoldwin/d9fe0c69ff2fad2f703754619b0346c8

Both are public, cross-linked, and will be kept updated as implementation
lands (per the user's explicit request to treat these as living
context-control artifacts for future sub-agents/sessions).

## Todos (tracked in SQL)


