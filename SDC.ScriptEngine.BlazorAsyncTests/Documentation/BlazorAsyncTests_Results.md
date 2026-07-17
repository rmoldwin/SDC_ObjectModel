# BlazorAsyncTests Phase 1 — Results

## Environment
- Branch: Features/NET10/ILandWASM/BlazorAsyncTests/Phase1
- .NET version: 10.0
- WASM threading mode: Single-threaded (WasmEnableThreads not set)
- OperatingSystem.IsBrowser(): true
- Environment.ProcessorCount: 1 (confirmed single-threaded mode)
- Run date: 2026-06-26
- Browser: Chrome (via Copilot in-app browser panel)

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

**Overall: 24 Passed, 1 Skipped (Phase 2 placeholder), 0 Failed**

### Category 2: Async Fallback Correctness

| Test | Status | Duration (ms) | Notes |
|---|---|---|---|
| EmbeddedResources_AllResolveNonNull | ✅ Passed | 1.4 | |
| ItemMutator_SequentialReassignment_Correct | ✅ Passed | 89.0 | |
| ChildItemsList_SequentialReplacement_Correct | ✅ Passed | 335.3 | |
| TopNodeDictionary_ReadAndRemove_Consistent | ✅ Passed | 234.1 | |
| GuidAssignment_MultipleNodes_AllUnique | ✅ Passed | 1858.2 | 200 nodes; slow but correct |
| OMStateConsistency_AfterSequentialMutations | ✅ Passed | 1302.8 | XML round-trip verified |
| ConcurrentMutation_SingleSharedTopNode | ⏭ Skipped | 0.0 | Phase 2 (WasmEnableThreads=true) |

### Category 3: CompareTrees Correctness

| Test | Status | Duration (ms) | Notes |
|---|---|---|---|
| CompareTrees_EmbeddedXml_V1vsVMax_DiffCount_MatchesSerialBaseline | ✅ Passed | 7,915.7 | Matches baseline: 907/3/0 |
| CompareTrees_V1vsVMax_Deterministic_5Runs | ✅ Passed | 34,073.8 | 5 runs identical — fully deterministic |
| CompareTrees_AsParallelForAll_CompletesWithoutException | ✅ Passed | 10,206.8 | AsParallel serialized, no crash |
| CompareTrees_ErrorPaths_NotReached | ✅ Passed | 11,805.9 | Neither Debugger.Break path hit |
| CompareTrees_SequentialInstances_SameResults | ✅ Passed | 33,486.2 | 3 instances, identical results |
| CompareTrees_V1EqualsV1_NoDiffs | ✅ Passed | 10,525.2 | 0 isNew, 0 isRemoved, 0 isChanged |

### Category 4+5: Complex Mutations & WASM Risk Points

| Test | Status | Duration (ms) | Notes |
|---|---|---|---|
| AddSection_CompareDetectsNewSection | ✅ Passed | 1,340.9 | |
| RemoveQuestion_CompareDetectsRemoval | ✅ Passed | 74.9 | |
| ChangeAttribute_CompareDetectsAttChange | ✅ Passed | 47.9 | |
| BulkMutation_AllDiffsDetected | ✅ Passed | 98.6 | |
| MultipleTopNodes_SequentialComparers_IndependentResults | ✅ Passed | 139.6 | |
| CompareTrees_IdenticalTrees_NoDiffs | ✅ Passed | 62.1 | |
| AsyncMutation_ThenCompare_Consistent | ✅ Passed | 60.4 | |
| LargeTree_100Nodes_CompareCorrect | ✅ Passed | 208.9 | |
| SortedList_SequentialReads_Stable | ✅ Passed | 60.3 | |
| CancellationToken_PropagatesCorrectly | ✅ Passed | 1.2 | |
| GarbageCollection_LargeTreesReleased | ✅ Passed | 220.1 | |

## Performance Notes

`CompareTrees` on V1 vs V5 (2464–2465 nodes) is significantly slower in WASM than desktop:
- Single run: ~8–12 seconds
- Five sequential runs: ~34 seconds total (~7s average)

Root cause: `AsParallel().ForAll()` inside `CompareVersionAttributes()` serializes entirely onto
the single JS event loop thread in single-threaded WASM. There is no thread-pool available for
parallel work. Each attribute comparison runs sequentially. On desktop with a thread pool the
same operation completes in milliseconds.

This is expected behavior, not a bug. Phase 2 (`WasmEnableThreads=true`) will restore parallelism.

## Startup Note

`index.html` required a fix: the agent-generated script reference used the fingerprinting
placeholder syntax (`blazor.webassembly#[.{fingerprint}].js`) which browsers treat as a URL
fragment, causing a 404. Fixed to `blazor.webassembly.js` — the standard dev-mode path.

## Bugs Found

[GitHub issues created with labels `wasm-async-bug` and `area:concurrency`]
