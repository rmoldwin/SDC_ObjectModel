# BlazorAsyncTests Phase 2 — Browser Run Results

**Branch:** `Features/NET10/ILandWASM/BlazorAsyncTests/Phase2`  
**Base:** `Features/NET10/ILandWASM/Main` (tip: 69f375d, Sprint A TS-3 ConcurrentDictionary fix)  
**Config:** `WasmEnableThreads=true`, `WasmPThreadPoolSize=4`, interpreter mode (not AOT)

## How to Run

```
dotnet run --project SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server
```

Then open `http://localhost:5000` in a browser. Do NOT use the Blazor DevServer — it cannot
add the COOP/COEP headers required for multi-threaded WASM.

## Expected Pre-flight Checks (Cat 1 Dashboard)

| Check | Expected |
|-------|----------|
| OperatingSystem.IsBrowser() | true |
| Environment.ProcessorCount | > 1 (e.g. 8 or 16) |
| crossOriginIsolated (JS) | true |
| WasmPThreadPoolSize | 4 (configured; reduced from 16 — Mono pthread attach assertion limit) |

## Category 2: Barrier Tests (7 tests) — Expected: All PASS

| Test | Status | Duration (ms) |
|------|--------|--------------|
| ItemMutator_ConcurrentReassignments_NoRace | TBD | TBD |
| ItemsMutator_ConcurrentListReplacements_NoRace | TBD | TBD |
| TopNodeDictionaries_ConcurrentReadWrite_NoCorruption | TBD | TBD |
| SameReference_ConcurrentReassignment_Stable | TBD | TBD |
| CollectionModification_UnderConcurrentLoad_NoException | TBD | TBD |
| GuidAssignment_ConcurrentThreads_AllUnique | TBD | TBD |
| LastTopNode_ThreadStatic_IsolatedPerThread | TBD | TBD |

## Category 3: ThreadSafetyRepro Tests (4 tests) — Expected: All PASS

| Test | Status | Duration (ms) |
|------|--------|--------------|
| TS1_LastTopNode_ThreadStatic_IsolatedPerThread | TBD | TBD |
| TS2_AtomicObjectId_ConcurrentGeneration_AllUnique | TBD | TBD |
| TS3_NodeDictionary_ConcurrentAddRemove_NoCorruption | TBD | TBD |
| TS4_ReadLockScope_WriteLockScope_UnderContention | TBD | TBD |

## Category 4: CompareTrees Parallel (3 tests) — Informational

| Test | Status | Duration (s) | Notes |
|------|--------|-------------|-------|
| CompareTrees_V1vsV5_ParallelTiming_Informational | TBD | TBD | Serial baseline; compare to Phase 1 ~8–12s |
| CompareTrees_ConcurrentInstances_8Threads_SameResults | TBD | TBD | 4 independent instances (THREAD_COUNT=4) |
| CompareTrees_LockContention_NoDeadlock | TBD | TBD | V1 vs V2 under contention |

### Baseline Assertions (V1 vs V5)
- IETattDiffs.Count == 907
- IETnodesAddedInNew.Count == 3
- IETnodesRemovedInNew.Count == 0

## Category 5: Shared TopNode Tests (3 tests) — Expected: All PASS (Sprint A fix applied)

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication | TBD | TBD | 4 threads × 50 nodes |
| ConcurrentNodeConstruction_SingleTopNode_LazyInitRace | TBD | TBD | Interlocked.CompareExchange lazy-init |
| ConcurrentReadWrite_SingleTopNode_StressReaders | TBD | TBD | 7 readers + 1 writer × 3s |

## Known Limitations / Notes

- `lock(locker)` at CompareTrees.cs:352 serialises `GetNodePreviousSib()` — Cat 4 concurrent timing
  will NOT show linear speedup even with 4 threads.
- The result dict (`_dDifNodeIET`) IS `ConcurrentDictionary` already — partial parallelism is present.
- Cat 4 Test 2 and 3 have 5-minute watchdogs due to 4 simultaneous heavy CompareTrees calls.
- Cat 3 TS4 has a 60-second watchdog (4 readers on the same heavy comparison).
- `WasmPThreadPoolSize` reduced from 16 to 4: Mono WASM raises a pthread-attach assertion failure
  (`mono-threads-wasm.c:201`) when too many workers attach simultaneously at boot.

## Run Date / Environment

| Field | Value |
|-------|-------|
| Run Date | TBD |
| Browser | TBD |
| OS | TBD |
| .NET Runtime | TBD |
