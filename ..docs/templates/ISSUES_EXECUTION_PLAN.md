# Open Issues & Execution Plan

Snapshot taken 2026-07-20 against the live GitHub issue tracker (28 open issues). This is the
dependency-ordered execution plan referenced from [`../roadmap.md`](../roadmap.md). Work tiers are
ordered so that foundational/blocking issues are resolved before issues that depend on them or
would otherwise need to be re-tested afterward. Within a tier, issues are independent of each
other unless a "Depends on" note says otherwise.

## Tier 1 — Foundational bug fixes (unblock everything downstream)

| # | Title | Labels | Depends on | Notes |
|---|---|---|---|---|
| [#50](https://github.com/rmoldwin/SDC_ObjectModel/issues/50) | `dateTimeStamp_Stype.val` setter always throws (broken regex) | bug | — | Small, isolated fix; do first. |
| [#23](https://github.com/rmoldwin/SDC_ObjectModel/issues/23) | `SdcUtil` reflection binding cache not thread-safe | bug, thread-safety | — | Swap plain `Dictionary` for `ConcurrentDictionary`; likely root cause feeding #24/#25. |
| [#17](https://github.com/rmoldwin/SDC_ObjectModel/issues/17) | Migrate `[ThreadStatic] LastTopNode` to `AsyncLocal<T>` | (add thread-safety, area:concurrency) | — | Ambient-state fix that several WASM async bugs likely depend on. |

## Tier 2 — Validation core

| # | Title | Labels | Depends on | Notes |
|---|---|---|---|---|
| [#46](https://github.com/rmoldwin/SDC_ObjectModel/issues/46) | `IVal.ValXmlString` throws for ~32/35 types | bug, validation | — | Implement generic accessor across data types. |
| [#49](https://github.com/rmoldwin/SDC_ObjectModel/issues/49) | `IVal.ValXmlString` for `XML_Stype`/`HTML_Stype` | enhancement | #46 | Same mechanism, deferred subset. |
| [#8](https://github.com/rmoldwin/SDC_ObjectModel/issues/8) | Soft-reject contract for setters + deserialization | bug, enhancement | — | Architectural decision; #6/#7 should follow its resolution. |
| [#6](https://github.com/rmoldwin/SDC_ObjectModel/issues/6) | Malformed numeric input: logging/error-collection | enhancement, validation, tech-debt | #8 | Coordinate wording/contract with #8. |
| [#7](https://github.com/rmoldwin/SDC_ObjectModel/issues/7) | Numeric `ResponseType` round-trip/validation divergences | bug, enhancement | #8 | Reconcile "throws" wording with soft-reject contract. |
| [#47](https://github.com/rmoldwin/SDC_ObjectModel/issues/47) | Audit validation + test coverage for all SDC data types | enhancement, validation, tech-debt | #46, #49 | Broad audit; run after the two `ValXmlString` fixes land so the inventory reflects current state. |
| [#13](https://github.com/rmoldwin/SDC_ObjectModel/issues/13) | Revisit timezone offset modeling | enhancement | — | Design discussion; feeds #9. |
| [#9](https://github.com/rmoldwin/SDC_ObjectModel/issues/9) | Migrate `timeZone` to `TimeSpan`/offset type | enhancement | #13 | Implementation once #13's design question is settled. |

## Tier 3 — Thread safety / WASM concurrency

| # | Title | Labels | Depends on | Notes |
|---|---|---|---|---|
| [#25](https://github.com/rmoldwin/SDC_ObjectModel/issues/25) | TS-4 `PredActionType` duplicate key under `CompareTrees` | (add bug, thread-safety) | #23 | Re-test after the reflection-cache fix. |
| [#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20) | TS-7 `PredActionType` duplicate key under lock contention | bug, thread-safety, low-priority | #23 | Likely same root cause as #25. |
| [#21](https://github.com/rmoldwin/SDC_ObjectModel/issues/21) | TS-8 array-copy race in `CompareTrees` | bug, thread-safety | #23 | Re-test after #23. |
| [#24](https://github.com/rmoldwin/SDC_ObjectModel/issues/24) | `GuidAssignment` deadlock under 4-thread `Barrier` | bug, thread-safety | #23 | Root cause suspected to be test-design (`Barrier` vs. `CountdownEvent`, see #22). |
| [#22](https://github.com/rmoldwin/SDC_ObjectModel/issues/22) | TS-5 `GuidAssignment` deadlock on WASM | bug, thread-safety, wasm | #24 | Roadmap notes this is already fixed by switching to `CountdownEvent` — verify and close if confirmed. |
| [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28) | `[ThreadStatic]`/ambient flags leak across MSTest workers | bug, thread-safety | #17 | May be resolved once ambient state migrates to `AsyncLocal<T>`. |
| [#18](https://github.com/rmoldwin/SDC_ObjectModel/issues/18) | Trim-safe XML serialization for production WASM | area:wasm, low-priority | — | Independent; can run anytime. |
| [#52](https://github.com/rmoldwin/SDC_ObjectModel/issues/52) | Adaptive Blazor WASM multithreading optimizer | enhancement, area:wasm | #17, #23, #24, #25 | Design work should start once the underlying races are fixed and stable. |

## Tier 4 — Serialization

| # | Title | Labels | Depends on | Notes |
|---|---|---|---|---|
| [#35](https://github.com/rmoldwin/SDC_ObjectModel/issues/35) | Fix MessagePack (MsgPack) polymorphic round-trip | bug, tech-debt | — | Independent of other tiers. |
| [#26](https://github.com/rmoldwin/SDC_ObjectModel/issues/26) | Async `LoadAsync`/`SaveAsync` overloads | enhancement, low-priority | — | Convenience-only; defer freely. |

## Tier 5 — Rules engine / Phase 2 scripting (deferred by design)

| # | Title | Labels | Depends on | Notes |
|---|---|---|---|---|
| [#14](https://github.com/rmoldwin/SDC_ObjectModel/issues/14) | OM-defined events for IET script triggers | enhancement, architecture | Phase 2 | Deferred; needs design discussion first. |
| [#15](https://github.com/rmoldwin/SDC_ObjectModel/issues/15) | Script hash verification against template registry | enhancement, security | Phase 2, #14 | Deferred. |
| [#16](https://github.com/rmoldwin/SDC_ObjectModel/issues/16) | In-browser Roslyn compilation via `WasmReferenceProvider` | enhancement | Phase 2 | Deferred. |
| [#29](https://github.com/rmoldwin/SDC_ObjectModel/issues/29) | Deprioritize/remove recursive `PredGuardType` machinery | tech-debt, low-priority | — | Needs a design discussion before implementation ("major surgery"). |

## Tier 6 — Documentation & process

| # | Title | Labels | Depends on | Notes |
|---|---|---|---|---|
| [#38](https://github.com/rmoldwin/SDC_ObjectModel/issues/38) | AI skill to keep docs/plans/wiki/archives in sync | documentation | — | Substantially delivered by this session's work (`skills/DocsIssueHygiene.md`, this plan, the doc/issue templates); leave open until the skill has run through at least one real before-PR cycle to confirm no false positives, then close. |
| [#37](https://github.com/rmoldwin/SDC_ObjectModel/issues/37) | Finish XML doc-comment coverage for public members | documentation | — | See `sessions/XmlAnnotationPlan.md`. |
| [#36](https://github.com/rmoldwin/SDC_ObjectModel/issues/36) | Populate project wiki with settled architecture content | documentation | Stable content in `..docs/architecture/` | Pull from already-settled chapters first. |

## How to execute this plan autonomously

1. Work tier-by-tier, top to bottom; within a tier, issues are parallelizable.
2. Before starting an issue, re-read it on GitHub (state may have changed) and confirm its
   "Depends on" issues are closed or otherwise resolved.
3. Follow [`ISSUE_TEMPLATE.md`](ISSUE_TEMPLATE.md) formatting when updating the issue with
   findings; follow [`DOC_TEMPLATE.md`](DOC_TEMPLATE.md) for any new/updated `..docs/` content.
4. Add/adjust tests per `.github/copilot-instructions.md` (all affected tests must be run and
   pass), then open a PR referencing the issue (`Fixes #N`).
5. Before opening the PR, run the before-PR doc/issue refresh described in
   [`DocsIssueHygiene.md`](../skills/DocsIssueHygiene.md).
6. After the PR merges, update this table's row (or remove it once the issue is closed) and
   `..docs/roadmap.md`'s status column.
