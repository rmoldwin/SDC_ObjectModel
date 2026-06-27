# BlazorAsyncTests Phase 2 — Browser Run Results

**Branch:** `Features/NET10/ILandWASM/BlazorAsyncTests/Phase2`  
**Base:** `Features/NET10/ILandWASM/Main` (tip: 69f375d, Sprint A TS-3 ConcurrentDictionary fix)  
**Config:** `WasmEnableThreads=true`, `WasmPThreadPoolSize=4`, interpreter mode (not AOT)

## How to Run

> **IMPORTANT**: `dotnet run` does NOT work for this project.
> In dev mode, the Mono native WASM binary does not have pthread support compiled in.
> `WasmEnableThreads=true` only takes effect in the published output, which uses
> the `wasm-tools` workload's thread-enabled native binaries (`dotnet.native.worker.*.mjs`).

```
cd <repo-root>
dotnet publish SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server -c Release -o publish_out
cd publish_out
dotnet SDC.ScriptEngine.BlazorAsyncTests.Phase2.Server.dll
```

Then open `http://localhost:5000` in Chrome. Do NOT use the Blazor DevServer — it cannot
add the COOP/COEP headers required for multi-threaded WASM.

**Required workload:** `wasm-tools` (10.0.108 or later). Verify with `dotnet workload list`.
If missing: `dotnet workload install wasm-tools` or update VS to include it.

## Expected Pre-flight Checks (Cat 1 Dashboard) — CONFIRMED ✅

| Check | Expected | Actual |
|-------|----------|--------|
| OperatingSystem.IsBrowser() | true | true ✅ |
| Environment.ProcessorCount | > 1 | 20 ✅ |
| crossOriginIsolated (JS) | true | true ✅ |
| WasmPThreadPoolSize | 4 (configured) | 4 ✅ |
| Phase 1 serial baseline (CompareTrees V1→V5) | ~8–12 s, IETattDiffs=907, added=3, removed=0 | ✅ confirmed |

## Category 2: Barrier Tests (7 tests) — 6/7 PASS

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| ItemMutator_ConcurrentReassignments_NoRace | ✅ PASS | TBD | |
| ItemsMutator_ConcurrentListReplacements_NoRace | ✅ PASS | TBD | |
| TopNodeDictionaries_ConcurrentReadWrite_NoCorruption | ✅ PASS | TBD | |
| SameReference_ConcurrentReassignment_Stable | ✅ PASS | TBD | |
| CollectionModification_UnderConcurrentLoad_NoException | ✅ PASS | TBD | |
| GuidAssignment_ConcurrentThreads_AllUnique | ❌ FAIL | ~6010 | DEADLOCK — watchdog expired |
| LastTopNode_ThreadStatic_IsolatedPerThread | ✅ PASS | TBD | |

### Cat 2 Failure: GuidAssignment_ConcurrentThreads_AllUnique — DEADLOCK
**Error:** Watchdog expired at 6010 ms.  
**Root cause:** `OrchestrateBarrierTest` uses `new Thread()` for THREAD_COUNT=4 dedicated threads, but this test creates 4 threads × 100 `DisplayedType` nodes (400 total) on a single shared `DataElementType` TopNode — each constructor call registers into `_Nodes` via `ConcurrentDictionary.TryAdd` and also calls into the parent-binding code path. Under `WasmEnableThreads=true` with pool size 4, all 4 pthread workers are consumed by the orchestrated threads; the internal Barrier `SignalAndWait` has no spare thread to execute its post-phase action, causing a deadlock. The GUID uniqueness assertion itself is sound — `Guid.NewGuid()` is threadsafe. The deadlock is in the Barrier/thread pool interaction.  
**GitHub Issue:** TS-5 (see issues below)

## Category 3: ThreadSafetyRepro Tests (4 tests) — 2/4 PASS

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| TS1_LastTopNode_ThreadStatic_IsolatedPerThread | ✅ PASS | TBD | |
| TS2_AtomicObjectId_ConcurrentGeneration_AllUnique | ❌ FAIL | TBD | 75 duplicate ObjectIDs in 4×25=100 nodes |
| TS3_NodeDictionary_ConcurrentAddRemove_NoCorruption | ✅ PASS | TBD | Sprint A fix confirmed green |
| TS4_ReadLockScope_WriteLockScope_UnderContention | ❌ FAIL | TBD | Duplicate key on PredActionType under lock |

### Cat 3 Failure: TS2_AtomicObjectId_ConcurrentGeneration_AllUnique
**Error:** 75 duplicate `ObjectID` values in 100 concurrently-created nodes (4 threads × 25 nodes each).  
**Root cause:** `_MaxObjectID` per-tree counter is incremented with non-atomic `++` (i.e., `_MaxObjectID++` without `Interlocked.Increment`). Under real WASM threading contention, multiple threads read the same counter value before any writes back, producing duplicate integer IDs. Sprint A addressed `_Nodes` dictionary via `ConcurrentDictionary` but left `_MaxObjectID` unprotected.  
**Fix needed:** Replace `_MaxObjectID++` assignment in node constructors with `Interlocked.Increment(ref _MaxObjectID)`.  
**GitHub Issue:** TS-6

### Cat 3 Failure: TS4_ReadLockScope_WriteLockScope_UnderContention
**Error:** Duplicate key exception on `PredActionType` even when all readers hold `ReadLockScope` and writer holds `WriteLockScope`.  
**Root cause:** `PredActionType` node construction or registration into a shared collection is not fully protected by the same lock object that `ReadLockScope`/`WriteLockScope` use, allowing a reader to observe a partially-constructed state or two writers to insert the same key. The lock mechanism may not cover the full construction + registration sequence.  
**GitHub Issue:** TS-7

## Category 4: CompareTrees Parallel (3 tests) — 2/3 PASS

| Test | Status | Duration (s) | Notes |
|------|--------|-------------|-------|
| CompareTrees_V1vsV5_ParallelTiming_Informational | ✅ PASS | TBD | IETattDiffs=907, added=3, removed=0 confirmed |
| CompareTrees_ConcurrentInstances_8Threads_SameResults | ✅ PASS | TBD | 4 independent instances all return 907 |
| CompareTrees_LockContention_NoDeadlock | ❌ FAIL | TBD | Arg_LongerThanDestArray array race |

### Cat 4 Failure: CompareTrees_LockContention_NoDeadlock
**Error:** `Arg_LongerThanDestArray` — destination array is shorter than source during an array copy operation inside `CompareTrees`.  
**Root cause:** An internal array (likely a `List<T>` backing store or array used for intermediate results) is being resized by one thread while another thread is iterating or copying it. Despite the `lock(locker)` at CompareTrees.cs:352 protecting `GetNodePreviousSib()`, there is at least one other shared mutable array inside `CompareTrees` that is not protected under the same lock. The V1 vs V2 XML files (smaller than V1 vs V5) produce faster iterations, increasing contention frequency.  
**GitHub Issue:** TS-8

## Category 5: Shared TopNode Tests (3 tests) — 2/3 PASS

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication | ❌ FAIL | TBD | Parent-binding cache race |
| ConcurrentNodeConstruction_SingleTopNode_LazyInitRace | ✅ PASS | TBD | Interlocked.CompareExchange lazy-init confirmed |
| ConcurrentReadWrite_SingleTopNode_StressReaders | ✅ PASS | TBD | 7 readers + 1 writer × 3s, no exception |

### Cat 5 Failure: ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication
**Error:** `"SectionItemType cannot be attached to FormDesignType, Could not find a property in parentTarget to bind to"`  
**Root cause:** `SdcUtil.cs` maintains two static `Dictionary<Type, IEnumerable<PropertyInfo>?>` caches (`dListPropInfoElements`, `dListPropInfoAttributes`) used to speed up reflection-based parent-child property binding. These are plain `Dictionary<Type,…>` (not `ConcurrentDictionary`), populated lazily on first access per type. When multiple threads simultaneously attempt first-access for the same type, concurrent writes into a non-thread-safe `Dictionary` can corrupt the internal state, causing a key not to be found on subsequent reads — making the binding appear to fail. Fix: convert both caches to `ConcurrentDictionary<Type, IEnumerable<PropertyInfo>?>`.  
**GitHub Issue:** TS-9

## Summary: 13/18 Pass — 5 Concurrency Bugs Found

| Category | Pass | Fail | Total |
|----------|------|------|-------|
| Cat 1: Threading Verification | 1 | 0 | 1 |
| Cat 2: Barrier Tests | 6 | 1 | 7 |
| Cat 3: TS Repro Tests | 2 | 2 | 4 |
| Cat 4: CompareTrees Parallel | 2 | 1 | 3 |
| Cat 5: Shared TopNode | 2 | 1 | 3 |
| **Total** | **13** | **5** | **18** |

### Bugs Filed as GitHub Issues

| Issue | ID | GitHub | Location | Fix |
|-------|----|--------|----------|-----|
| Cat2/Test6 Deadlock under Barrier+4 threads | TS-5 | #22 | BarrierTests.razor: `OrchestrateBarrierTest` with large node count | Reduce nodes per thread or use ThreadPool instead of fixed threads |
| TS2: `_MaxObjectID` non-atomic increment | TS-6 | #19 | `PartialClasses.cs` node constructors | `Interlocked.Increment(ref _MaxObjectID)` |
| TS4: `PredActionType` duplicate key under lock | TS-7 | #20 | `CompareTrees.cs` or lock target mismatch | Investigate lock scope vs. constructor sequence |
| Cat4/Test3: Array race in CompareTrees V1 vs V2 | TS-8 | #21 | `CompareTrees.cs` shared array not under lock | Identify and protect the unguarded array |
| Cat5/Test1: `dListPropInfoElements` race (binding cache) | TS-9 | #23 | `SdcUtil.cs` lines 394/398 | Convert to `ConcurrentDictionary<Type, IEnumerable<PropertyInfo>?>` |

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
| Run Date | 2026-06-26 |
| Browser | Chrome |
| OS | Windows (user machine) |
| .NET Runtime | .NET 10 WASM (WasmEnableInterpreter=true, RunAOTCompilation=false) |
| wasm-tools workload | 10.0.108 |
| ProcessorCount | 20 |
| crossOriginIsolated | true |
| WasmPThreadPoolSize | 4 |
| THREAD_COUNT | 4 |
| Publish command | `dotnet publish Phase2.Server -c Release -o publish_out` → `cd publish_out && dotnet *.dll` |
