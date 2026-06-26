# BlazorAsyncTests Phase 1 — Results

## Environment
- Branch: Features/NET10/ILandWASM/BlazorAsyncTests/Phase1
- .NET version: 10.0
- WASM threading mode: Single-threaded (WasmEnableThreads not set)
- OperatingSystem.IsBrowser(): [TBD — populate at runtime from Index.razor dashboard]
- Environment.ProcessorCount: [TBD]

## Notes on Test Scope

Phase 1 validates sequential fallback correctness only. `Parallel.For`, `AsParallel().ForAll()`,
and `Task.WhenAll` all serialize onto the single JS event loop. No race conditions are exercised.

`[ThreadStatic] LastTopNode` collapses to a single slot in single-threaded WASM. All tree builds
in these tests are synchronous (no await mid-build) to avoid false `LastTopNode` clobbering.
See Issue #17 for the `AsyncLocal<T>` migration that would fix this properly.

Two `Debugger.Break()` calls exist in production code at CompareTrees.cs lines ~451 and ~641.
In WASM without DevTools attached they are no-ops. Category 3 tests detect if these paths are
reached by asserting complete diff results. Recommend replacing with logged exceptions (TODO).

## Phase 2 Gap: Single Shared TopNode Race Surface

The actual TS-2/TS-3 race bugs live in shared `_Nodes`/`_ParentNodes`/`_ChildNodes` on a
single TopNode plus `AtomicNextObjectID()`. Phase 1's isolated multi-TopNode tests never
exercise this surface. Phase 2 (`WasmEnableThreads=true`) must add:
- Concurrent construction on a single TopNode
- Concurrent read/write of `_Nodes` dict from multiple WebWorker threads
- `Barrier`-based simultaneous-start tests (ported directly from `BaseTypeThreadSafetyTests.cs`)

## Serial Baseline Constants (Category 3)

Established by running `ChangeSummaryTest` in `SDC.Schema.Tests/UtilityClasses/AttrMetadata/CompareTreesTests.cs`
on branch `Features/NET10/ILandWASM/Main` with `dotnet test --filter "FullyQualifiedName~ChangeSummaryTest"`:

| Metric | V1 vs V5 |
|---|---|
| `IETattDiffs.Count` | 907 |
| `IETnodesAddedInNew.Count` | 3 |
| `IETnodesRemovedInNew.Count` | 0 |
| `Nodes.Count` (new tree) | 2465 |
| `Nodes.Count` (prev tree) | 2464 |

## Test Results

[Populated after browser run — open the WASM app and click "Run All" on each test page]

| Test | Status | Duration | Notes |
|---|---|---|---|
| (run in browser) | | | |

## Bugs Found

[GitHub issues created with labels `wasm-async-bug` and `area:concurrency`]
