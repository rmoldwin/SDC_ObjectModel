# Worktree & Branch Cleanup Audit

Generated during the docs-restructuring work, as a snapshot of every local worktree and every
branch (local and remote) in this repository, checked against `origin/Features/NET10/Net10Main`
(the integration branch most work merges into). "Ahead" = commits unique to the branch, not yet
in `Net10Main`. "Behind" = commits in `Net10Main` not yet in the branch.

Two branches are being merged into `Net10Main` as a direct follow-up to this audit —
`Features/CompareTrees` and `Features/NET10/ILandWASM/Main` — see the "Not yet safe to delete"
section below for their status.

## Safe to remove now

These branches have **zero commits** not already in `Net10Main` (any real work they contain has
already been merged). Their local worktree (if any) can be removed with
`git worktree remove <path>`, then the branch itself with `git branch -D <name>` (and
`git push origin --delete <name>` for the remote copy).

| Branch | Local worktree (if any) |
|---|---|
| `Docs/DocsRestructure` | `rmoldwin-cautious-barnacle` |
| `Features/NET10/CoherenceValidator` | `rmoldwin-special-guide` |
| `Features/NET10/IDataHelpers` | — |
| `Features/NET10/IDataHelpers_Docs` | `rmoldwin-scaling-tribble` |
| `Features/NET10/IDataHelpers_Refactor` | `rmoldwin-crispy-system` |
| `Features/NET10/ILandWASM/BlazorTest` | `rmoldwin-probable-telegram` |
| `Features/NET10/ILandWASM/CoreEngine` (local-only, never pushed) | `rmoldwin-literate-disco` |
| `Features/NET10/ILandWASM/DiagnosticLineFix` | `rmoldwin-legendary-winner` |
| `Features/NET10/ILandWASM/InBrowserRoslyn` | — |
| `Features/NET10/ILandWASM/StressTests` | `rmoldwin-vigilant-garbanzo` |
| `Features/NET10/ILandWASM/Tests` | `rmoldwin-curly-garbanzo` |
| `Features/NET10/ILandWASM/WasmSpike` (local-only, never pushed) | `rmoldwin-friendly-pancake` |
| `Features/NET10/ILandWASM/WpfTest` | `rmoldwin-musical-umbrella` |
| `Features/NET10/IntegerDEtypeCoherence` | — |
| `Features/NET10/SetterCompletion` | `rmoldwin-literate-giggle` |
| `Features/NET10/TimezoneOffsetPlan` (local-only, never pushed) | `rmoldwin-expert-eureka` |
| `Features/NET10/TodoStubs` | `rmoldwin-effective-succotash` |
| `Features/NET10/TryAssignValue` | `rmoldwin-miniature-garbanzo` |
| `Features/NET10/ValidationContractPlan` (local-only, never pushed) | `rmoldwin-symmetrical-goggles` |
| `Features/NET10/ValidationDocs` | — |
| `Features/NET10/ValidationPatternNormalize` | — |
| `Features/Net11Upgrade_ThreadSafety` | — |
| `test-stub-file-rename-cleanup` | — |
| `Fred_NugetTest` | — |
| `Fred_SchemaTests` | — |
| `rmoldwin-jubilant-waddle` (auto-named, local-only) | `rmoldwin-jubilant-waddle` |
| `temp/docs-merge` (local-only) | `rmoldwin-reimagined-invention` |
| `rmoldwin-refactored-parakeet` (auto-named, local-only) | `rmoldwin-refactored-parakeet` |

Notes on a few of these:
- `Fred_NugetTest` and `Fred_SchemaTests` contain only "Merge pull request from master" commits —
  no unique content of their own.
- `Features/NET10/ILandWASM/CoreEngine`, `WasmSpike`, `TimezoneOffsetPlan`, and
  `ValidationContractPlan` were never pushed to `origin` — they only exist as local branches with
  a worktree.
- `rmoldwin-refactored-parakeet` has the **exact same commit history** as
  `Features/NET10/ILandWASM/Main` (identical tip commit `110f726`) — it looks like a duplicate,
  differently-named local branch for the same work. Safe to delete once `ILandWASM/Main` is
  confirmed merged (this audit's merge work covers that).
- `rmoldwin-jubilant-waddle` is 19 commits behind `Net10Main` with 0 ahead — an old, stale,
  never-advanced branch.

## Not yet safe to delete — active or unmerged work

| Branch | Local worktree | What needs to happen |
|---|---|---|
| `Features/CompareTrees` | (none currently) | **12 unmerged commits** of real `CompareTrees` enhancements (default-value display, parent/child logic fixes, incremental `_dDifNodeIET` cache updates, a generic wrapped-column table formatter). Being merged into `Net10Main` as part of this cleanup pass — see the new merge branch/PR. Once merged and confirmed, this branch can be deleted. |
| `Features/NET10/ILandWASM/Main` | `rmoldwin-stunning-guide` | **16 unmerged commits** of WebAssembly (WASM) thread-safety hardening (Sprint C through Sprint F: `ConcurrentDictionary` migration, per-tree child-node mutation locking, deferred child-node sorting, thread-safe unique-ID handling). Being merged into `Net10Main` as part of this cleanup pass. Once merged and confirmed, this branch can be deleted. |
| `Features/NET10/ILandWASM/SprintC` | — | An earlier checkpoint of the same work now on `ILandWASM/Main` (7 of its 16 commits). No separate action needed — delete once `ILandWASM/Main` is merged. |
| `Features/NET10/ILandWASM/TS3Fix` | `rmoldwin-vigilant-train` | An earlier checkpoint of the same work now on `ILandWASM/Main` (4 of its 16 commits). No separate action needed — delete once `ILandWASM/Main` is merged. |
| `Features/NET10/ILandWASM/BlazorAsyncTests/Phase1` | `rmoldwin-cautious-spork` | An earlier checkpoint of the same work now on `ILandWASM/Main` (2 of its 16 commits). No separate action needed — delete once `ILandWASM/Main` is merged. |
| `Features/NET10/ILandWASM/BlazorAsyncTests/Phase2` | `rmoldwin-curly-carnival` | **28 unmerged commits** — the still-open, separate WASM multi-threading investigation (GitHub issues #17, #20–#25, #28; see the scope-caveat banner in `..docs/architecture/thread-safety.md`). Found new, unresolved bugs under real multi-threaded WASM (duplicate-key exceptions, array-copy exceptions, `Barrier` deadlocks, a non-thread-safe reflection cache, `[ThreadStatic]` leakage across MSTest worker threads). Needs further debugging/fixes before it can be merged — not part of this cleanup pass. |
| `rmoldwin-soft-reject-validation` | `rmoldwin-miniature-barnacle` | 3 unmerged commits — planning/decision notes for the "soft-reject" date/date-part validation work (issue #8) and a memory note on branch-management rules. Needs a decision: merge the planning notes into `..docs/`/`sessions/` (they may already be superseded by later roadmap/convention updates), or archive and delete. |
| `rmoldwin-sdc-om-event-dispatch-phase2` | `rmoldwin-issue-14-architecture-om-defined-events-attach-s-b55843` | 0 ahead of `Net10Main`, but 199 behind — very stale. Confirm this exploratory branch (for issue #14's OM event-model design) is abandoned/superseded before deleting; if still relevant, it needs to be rebased and picked back up, not silently dropped. |
| `rmoldwin-sprint-f-wasm-threading` | `rmoldwin-glowing-enigma` | 0 ahead, 199 behind — very stale. Appears to be an earlier/alternate checkpoint of the Sprint F work that landed on `ILandWASM/Main`. Confirm it's fully superseded before deleting. |

## Active use — do not delete

| Branch | Local worktree | Status |
|---|---|---|
| `master` | (main checkout, elsewhere) | The repository's default branch. |
| `Features/NET10/Net10Main` | main checkout at `SDC Git Repo/SDC.Schema` | The integration branch nearly everything above targets. |
| `Features/AnyAtt_Support` | `rmoldwin-redesigned-waffle` | Active ad-hoc-attribute support feature work; this session's original branch. Currently 0 ahead of `Net10Main` (fully caught up) after the Net10Main merge that landed via PR #39. |
| `Features/AnyAtt_Support_MergeNet10Main` | `rmoldwin-solid-adventure` (this session) | This session's own worktree/branch, used to merge `Net10Main` into `Features/AnyAtt_Support` (already done via PR #39). Still the active session branch — do not delete while this session is in use. |
| `Docs/ProjectReadmes` | `docs-readmes-worktree` | This cleanup pass's own docs branch (PR #42, open, awaiting review). |

---
*Generated 2026-07-16. Ahead/behind counts and content were verified against `origin` at that
time — re-verify before deleting anything if significant time has passed.*
