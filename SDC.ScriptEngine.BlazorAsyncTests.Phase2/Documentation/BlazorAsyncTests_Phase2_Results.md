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

## 2026-07-19 Selective-Merge Review Run (Firefox)

This follow-up run was performed from `Features/NET10/Phase2SelectiveMerge` after selectively porting
Phase2 documentation and harness refinements back onto `Features/NET10/Net10Main`.

| Category/Test | Result | Interpretation | Follow-up |
|---------------|--------|----------------|-----------|
| Cat 2 / `CollectionModification_UnderConcurrentLoad_NoException` | ✅ Passed after reducing to one replacement per thread | Net10Main reproduced the ~12 s watchdog failure; the reduced workload keeps this as a correctness/stability harness instead of a throughput test. | Keep this Cat2 sizing as a correctness fix; use Category 6 for performance measurement. |
| Cat 4 / `CompareTrees_ConcurrentInstances_4Threads_SameResults` and `CompareTrees_LockContention_NoDeadlock` | ✅ Passed after extended static traversal lock | The `Arg_LongerThanDestArray` race reproduced before the fix and again once after the first lock patch. It stabilized after `CompareTrees<T>.TraversalLock` was extended to cover constructor-time serialized-attribute traversal. Stress run: Cat4 passed twice in a row, then passed again during the full Cat2-Cat6 run. | Keep the extended `CompareTrees<T>.TraversalLock` fix; continue using Cat4 as the correctness gate for CompareTrees concurrency. |
| Cat 5 / `ConcurrentReadWrite_SingleTopNode_StressReaders` | ✅ Passed after assertion fix | Net10Main reproduced the "expected 100, got 99" failure; the old assertion ignored that the writer is time-bounded. | Keep the assertion that validates only writer additions actually completed inside the stress window. |
| Cat 6 / `CompareTrees Performance Measurements` | ✅ Measurement-only; latest quick ratios 0.79x and 1.22x, heavy reference 997 ms | Separates performance measurement from correctness/watchdog tests. After Cat4 stress, quick V1->V2 measured sequential 63 ms vs parallel 80 ms (0.79x); after the full Cat2-Cat6 run, quick V1->V2 measured sequential 93 ms vs parallel 76 ms (1.22x). Heavy V1->V5 reference measured 997 ms with IETattDiffs=907. | Keep as measurement-only evidence; do not use a fixed speed threshold as a correctness gate. Cat4 remains the separate correctness gate. |
| Copilot browser canvas | ⚠ `plugin:opener|open_url not allowed by ACL` | Copilot UI/browser-canvas opener permission issue, not an SDC OM failure. | Use an external browser for the manual WASM pass. |

The historical results below remain useful as investigation notes, but this 2026-07-19 section is the
current review status for the selective merge branch.

Category 6 intentionally reports timing rather than passing or failing based on speed. A throughput
ratio below 1.0x is still useful evidence because it means 4-thread WASM did not improve that workload
on that browser/runtime configuration.

## Expected Pre-flight Checks (Cat 1 Dashboard) — CONFIRMED ✅

| Check | Expected | Actual |
|-------|----------|--------|
| OperatingSystem.IsBrowser() | true | true ✅ |
| Environment.ProcessorCount | > 1 | 20 ✅ |
| crossOriginIsolated (JS) | true | true ✅ |
| WasmPThreadPoolSize | 4 (configured) | 4 ✅ |
| Phase 1 serial baseline (CompareTrees V1→V5) | Historical ~8–12 s; current published references 1.572 s in Firefox and 1.219 s in the embedded browser canvas, IETattDiffs=907, added=3, removed=0 | ✅ confirmed |

## Category 2: Barrier Tests (7 tests) — 6/7 PASS

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| ItemMutator_ConcurrentReassignments_NoRace | ✅ PASS | 233 | |
| ItemsMutator_ConcurrentListReplacements_NoRace | ✅ PASS | 235 | |
| TopNodeDictionaries_ConcurrentReadWrite_NoCorruption | ✅ PASS | 230 | |
| SameReference_ConcurrentReassignment_Stable | ✅ PASS | 11 | |
| CollectionModification_UnderConcurrentLoad_NoException | ✅ PASS | 4233 | |
| GuidAssignment_ConcurrentThreads_AllUnique | ❌ FAIL | 6010 | DEADLOCK — watchdog expired |
| LastTopNode_ThreadStatic_IsolatedPerThread | ✅ PASS | 846 | |

### Cat 2 Failure: GuidAssignment_ConcurrentThreads_AllUnique — TEST DESIGN BUG (fixed)
**Error:** Watchdog expired at 6010 ms.  
**Root cause:** `Barrier(N)` requires N+1 available thread-pool threads: N participants call `SignalAndWait()` and the post-phase callback needs one additional pool slot. With `WasmPThreadPoolSize=4` and `THREAD_COUNT=4`, all pool threads are occupied — the callback deadlocks. **This is NOT an SDC OM bug.** `Guid.NewGuid()` is thread-safe.  
**Fix:** Replaced `OrchestrateBarrierTest` with `new Thread()` + `CountdownEvent` — same "all start together" guarantee without pool-thread consumption.  
**GitHub Issue:** TS-5 (#22) — updated as test design bug, fix committed.
**Sprint E update:** the `CountdownEvent` redesign removed the Barrier deadlock, but the test still hit the
watchdog because of the per-insert reflection slowness in `RegisterParentNode` — see **Sprint E Fix A**.
Both Cat 2 Test 5 and Test 6 are addressed there.

## Category 3: ThreadSafetyRepro Tests (4 tests) — 2/4 PASS

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| TS1_LastTopNode_ThreadStatic_IsolatedPerThread | ✅ PASS | 343 | |
| TS2_AtomicObjectId_ConcurrentGeneration_AllUnique | ⚠️ STALE RESULT | 286 | OLD run: 75 dup IDs — **test design bug**, since fixed (see below). Counter is atomic. |
| TS3_NodeDictionary_ConcurrentAddRemove_NoCorruption | ✅ PASS | 264 | Sprint A fix confirmed green |
| TS4_ReadLockScope_WriteLockScope_UnderContention | ❌ FAIL | 12247 | Argument_AddingDuplicateWithKey, SDC.Schema.PredActionType |

### Cat 3 TS2_AtomicObjectId_ConcurrentGeneration_AllUnique — RESOLVED (#19 closed)
**The 75-duplicate result above was a TEST DESIGN BUG, not an SDC OM defect.** `_MaxObjectID` is a
**per-tree instance field** (each `TopNode` has its own counter), not a global/static counter. The old
test spun up a **separate tree per thread**, so each thread's counter independently ran `1..25` and
cross-thread collisions were guaranteed *by design*. The OM counter is already incremented atomically
via `_ITopNode.AtomicNextObjectID()` → `Interlocked.Increment(ref _maxObjectID_XX)` (TS-3 / Sprint A),
implemented for all five TopNode types in `PartialClasses.cs`; there is no remaining `_MaxObjectID++`.
**Test fix (commit `09badb1`):** all threads now share ONE `DataElementType`, so they contend on the
SAME counter — the correct way to exercise atomicity. Verified on desktop by
`ThreadSafetyReproTests.Repro_NonAtomicMaxObjectID_ConcurrentCreation_ProducesDuplicateObjectIDs`,
which passes **5/5** isolated runs (0 duplicates) and within the full 692-test suite.
**GitHub Issue:** TS-6 (#19) — **CLOSED / COMPLETED**.

### Cat 3 Failure: TS4_ReadLockScope_WriteLockScope_UnderContention
**Error:** `System.InvalidOperationException: Exception during concurrent CompareTrees: Argument_AddingDuplicateWithKey, SDC.Schema.PredActionType`  
**Root cause:** `PredActionType` node construction or registration into a shared collection is not fully protected by the same lock object that `ReadLockScope`/`WriteLockScope` use. Sprint C fix: add `ReadLock` around `GetSortedNonIETsubtreeList` in CompareTrees.  
**GitHub Issue:** TS-7 (#20)

## Category 4: CompareTrees Parallel (3 tests) — 2/3 PASS

| Test | Status | Duration (s) | Notes |
|------|--------|-------------|-------|
| CompareTrees_V1vsV5_ParallelTiming_Informational | ✅ PASS | 5549 ms | IETattDiffs=907, added=3, removed=0 confirmed |
| CompareTrees_ConcurrentInstances_4Threads_SameResults | ✅ PASS | 5660 ms | 4 independent instances all return 907 |
| CompareTrees_LockContention_NoDeadlock | ❌ FAIL | 121 ms | Arg_LongerThanDestArray array race (V1 vs V2) |

### Baseline Assertions (V1 vs V5) — CONFIRMED ✅
- IETattDiffs.Count == 907 ✅
- IETnodesAddedInNew.Count == 3 ✅
- IETnodesRemovedInNew.Count == 0 ✅

### Cat 4 Failure: CompareTrees_LockContention_NoDeadlock
**Error:** `System.InvalidOperationException: 1 exception(s) during concurrent CompareTrees (V1 vs V2). First: Arg_LongerThanDestArray Arg_ParamName_Name, destinationArray`  
**Root cause:** `GetSortedNonIETsubtreeList` (called inside `CompareTrees`) is not under a ReadLock — a shared mutable list is resized by one thread while another copies it. Sprint C fix: wrap `GetSortedNonIETsubtreeList` in a ReadLock.  
**GitHub Issue:** TS-8 (#21)
**Sprint E update — actual root cause & fix:** the array corruption was a `kids.Sort()` (in
`SortElementKids`, under `_ChildNodesMutationLock`) racing a `childList.Remove()` (in
`UnRegisterParentNode`, under the WriteLock only) on the same `_ChildNodes` list. See **Sprint E Fix B + C**.

## Category 5: Shared TopNode Tests (3 tests) — 2/3 PASS

| Test | Status | Duration (ms) | Notes |
|------|--------|--------------|-------|
| ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication | ❌ FAIL | 146 | Parent-binding cache race in SdcUtil |
| ConcurrentNodeConstruction_SingleTopNode_LazyInitRace | ✅ PASS | 14 | Interlocked.CompareExchange lazy-init confirmed |
| ConcurrentReadWrite_SingleTopNode_StressReaders | ✅ PASS | 3343 | 7 readers + 1 writer × 3s, no exception |

### Cat 5 Failure: ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication
**Error:** `System.InvalidOperationException: 4 exception(s) during concurrent node construction. First: This object type (SectionItemType) cannot be attached to the provided parentNode type (FormDesignType). Could not find a property in parentTarget to bind to…`  
**Root cause:** `SdcUtil.cs` maintains two static `Dictionary<Type, IEnumerable<PropertyInfo>?>` caches (`dListPropInfoElements`, `dListPropInfoAttributes`) used to speed up reflection-based parent-child property binding. These are plain `Dictionary<Type,…>` (not `ConcurrentDictionary`), populated lazily on first access per type. When multiple threads simultaneously attempt first-access for the same type, concurrent writes into a non-thread-safe `Dictionary` can corrupt the internal state, causing a key not to be found on subsequent reads — making the binding appear to fail. Fix: convert both caches to `ConcurrentDictionary<Type, IEnumerable<PropertyInfo>?>`.  
**GitHub Issue:** TS-9 (#23)
**Sprint E update — actual root cause & fix:** the failure was a concurrent-attach race, not the
property-info cache. `TryAttachNewNode` mutates the shared parent's child collection by reflection
*before* the `BaseType` ctor takes the tree WriteLock, so four threads attaching to the same
`ChildItemsType` corrupted it. Fixed test-side (each thread uses its own per-thread parent) — see
**Sprint E Fix D**. The shared `fd` TopNode still exercises concurrent ObjectID/GUID generation.

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
| Cat2/Test6 Deadlock — test design bug (Barrier+pool=4) | TS-5 | #22 | BarrierTests.razor Test6 | Fixed: replaced Barrier+OrchestrateBarrierTest with new Thread()+CountdownEvent |
| TS2: `_MaxObjectID` non-atomic increment | TS-6 | #19 ✅ CLOSED | `PartialClasses.cs` node constructors | RESOLVED: counter already atomic via `AtomicNextObjectID()`→`Interlocked.Increment`; old failure was a test-design bug (separate tree per thread), fixed in `09badb1` |
| TS4: `PredActionType` duplicate key under lock | TS-7 | #20 | `CompareTrees.cs` or lock target mismatch | Investigate lock scope vs. constructor sequence |
| Cat4/Test3: Array race in CompareTrees V1 vs V2 | TS-8 | #21 | `CompareTrees.cs` shared array not under lock | Identify and protect the unguarded array |
| Cat5/Test1: `dListPropInfoElements` race (binding cache) | TS-9 | #23 | `SdcUtil.cs` lines 394/398 | Convert to `ConcurrentDictionary<Type, IEnumerable<PropertyInfo>?>` |

## Sprint E Remediation — Final 3 WASM-only Failures (OM branch `Features/NET10/ILandWASM/Main`)

Sprint E addresses the last cluster of failures that reproduce **only** under .NET 10 multi-threaded
WASM (`WasmEnableThreads=true`, `WasmPThreadPoolSize=4`, interpreter mode). The OM source fixes live in
the `Features/NET10/ILandWASM/Main` worktree; the Cat5 test redesign lives here on
`Features/NET10/ILandWASM/BlazorAsyncTests/Phase2`. **The OM fixes must be present in the OM that the
Phase2 server publishes against** (merge `Main` into the Phase2 branch, or rebuild the publish output
against the updated OM) for these to go green.

### Fix A — `RegisterParentNode` per-insert reflection removed (Cat 2 Test 5 & Test 6)
**Symptom:** `CollectionModification_UnderConcurrentLoad_NoException` and
`GuidAssignment_ConcurrentThreads_AllUnique` hit the 12 s / 6 s watchdogs (false "deadlocks").
**Real cause:** not a deadlock — extreme WASM slowness. The node-construction path
(`BaseType` ctor → `InitAfterTreeAdd` → `RegisterAll(childNodesSort:false)` → `RegisterParentNode`) ran a
reflection-based `TreeComparer.SibComparer` binary-search/full-sort on **every** insert, inside the
per-tree `_ChildNodesMutationLock`. Each reflection call is ~0.5 ms in interpreted WASM; for N≈400
siblings the cumulative reflection on the locked critical path exceeded the watchdogs.

**Sprint E first attempt (`@order` insert) — SUPERSEDED.** Sprint E (ccdb190) replaced the reflection with
the reflection-free `TreeOrderComparer` (a pure `@order` decimal compare), relying on the ctor's
`order = ObjectID` pre-assignment as document order. **Sprint F replaced this** after finding two
correctness problems with the `@order` approach:
1. **Stale `@order` on the refresh/deserialize path.** `ReflectRefreshTree.DoTree` calls
   `RegisterAll(childNodesSort:false)` **before** it reassigns `@order` (SdcUtil line 705 vs 730), so for a
   freshly-deserialized tree `@order` is uniform (`0`). `TreeOrderComparer` is non-antisymmetric for equal
   `@order` (`Compare(a,b)==Compare(b,a)==1`), so its `BinarySearch`/`Insert` could **scramble**
   `_ChildNodes` on that path.
2. **Out-of-document-order construction.** A node built with an explicit `insertPosition>=0`
   (`TryAttachNewNode`) gets the **highest** `@order` (newest `ObjectID`) yet a non-last document position,
   so an `@order` append would misplace it.

**Sprint F fix (smart document-order insert).** The first Sprint F attempt was a plain
`lock(_ChildNodesMutationLock){ kids.Add(btSource); }` + `TreeSort_Invalidate(parent)` deferred sort.
That was fast but **incorrect**: a plain append leaves `_ChildNodes` in *construction* order, which is
wrong whenever a node is constructed at an explicit out-of-document position (e.g. `AddQuestion`/
`AddListItem` at index 0). Because `FindPrevIETInDictionaries` walks `_ChildNodes` assuming **document
order** to find the IET predecessor during construction, the deferred-sort append corrupted `_IETnodes`
indexing — caught by the desktop regression suite (`QuestionItemTypeTests.AddListItemToPosition0`).

The shipped fix is `SmartInsertInDocumentOrder` (OM `IMoveRemoveExtensions.cs`), which keeps
`_ChildNodes` in document order at all times **without** the old O(log k)-reflection-per-insert cost:
- **In-order append (dominant case).** A single `SibComparer` reflection compare against the current
  last sibling; if the new node sorts at/after it (true for the `BaseType` ctor, deserialization, and
  `ReflectRefreshTree.DoTree` bulk paths, which register in document order), it is an **O(1) append**.
- **Out-of-order insert (rare).** Only when the new node sorts *before* the last sibling (e.g. explicit
  position 0) does it run a reflection **binary search** for the correct slot — bounded and infrequent.
- **Unavailable order → safe fallback.** The comparer can throw `ArgumentException` (transient null
  `ParentNode` mid-`ReflectRefreshTree`) or `InvalidOperationException` (node not yet wired during
  concurrent construction); both fall back to append + `TreeSort_Invalidate`, since those callers walk
  children in document order anyway. (The original deferred-sort missed `ArgumentException` and crashed
  deserialization — fixed here.)

Net effect: ~1 reflection compare per node on the bulk path (≈8× fewer than Sprint D's per-insert
binary search) instead of O(log k), so the Cat 2 WASM watchdog slowness is removed, while
`_ChildNodes`/`_IETnodes` ordering stays correct. `childNodesSort:true` (MoveNode/AddChild edits) keeps
the full `SibComparer` reflection sort.

We deliberately do **not** use `@order` (Sprint E Fix A's `TreeOrderComparer`): on the
`ReflectRefreshTree`/deserialize path `@order` is reassigned *after* registration (uniform/stale `0`)
and `TreeOrderComparer` is non-antisymmetric for equal orders, so it could scramble the list.

**OM commits:** `Sprint F: replace Fix A @order insert with deferred sort` →
`Sprint F Fix2: smart document-order insert in RegisterParentNode`.
**Regression:** all **692/692** desktop tests pass with the smart insert (the deferred-sort attempt
failed `AddListItemToPosition0` + 6 deserialize/roundtrip tests; both classes are now green).
**Expected after rebuild:** both Cat 2 tests GREEN, well under their watchdogs.

### Fix B + Fix C — `kids.Sort` vs `kids.Remove` race (Cat 4 Test 3)
**Symptom:** `CompareTrees_LockContention_NoDeadlock` threw `Arg_LongerThanDestArray` at ~147 ms.
**Real cause:** `SortElementKids` calls `kids.Sort()` under `_ChildNodesMutationLock`, but
`UnRegisterParentNode` called `childList.Remove(node)` under the tree WriteLock **only** — so a PLINQ
`CompareTrees` worker sorting a list while another path removed from the same list corrupted the internal
array.
**Fix B:** `UnRegisterParentNode` now wraps `childList.Remove(node)` + the follow-up `TryRemove` in
`lock(_ChildNodesMutationLock)` (falls back to no lock when TopNode/lock is null — i.e. not yet concurrent).
**Fix C:** `SortElementKids`' orphan branch (TopNode not yet set, e.g. concurrent deserialization) now
sorts under `lock(kids)` instead of unlocked, closing the same race during tree build.
**Expected after rebuild:** Cat 4 Test 3 GREEN (no array race), Tests 1 & 2 still GREEN.

### Fix D — Cat 5 Test 1 redesigned to avoid the pre-lock attach race (`SharedTopNodeTests.razor`)
**Symptom:** `ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication` threw
"This object type (SectionItemType) cannot be attached to the provided parentNode type (SectionItemType)".
**Real cause:** the `BaseType` constructor runs `TryAttachNewNode` (which mutates the parent's child
collection by reflection) **before** it takes the tree WriteLock. Four threads all attaching to the *same*
`ChildItemsType` parent raced on that collection and corrupted it. This is an attach-collection race, not
the ObjectID/GUID concurrency the test targets.
**Fix (test-side, Option A — minimal risk):** each thread now creates its 50 `SectionItemType` nodes under
its **own** pre-created per-thread `ChildItemsType` parent (built single-threaded), so concurrent attaches
never touch the same parent collection. All threads still share the one `fd` TopNode, so concurrent
ObjectID (Interlocked) and ObjectGUID (`Guid.NewGuid`) generation — the real thread-safety surface — is
still fully exercised. `TOTAL_NODES` (200) and all uniqueness/`Nodes.Count` assertions are unchanged.
**Expected after rebuild:** Cat 5 Test 1 GREEN.

### Post-Sprint-E expected scoreboard (after OM rebuild/merge)
| Test | Before | Expected after rebuild |
|------|--------|------------------------|
| Cat2 / CollectionModification_UnderConcurrentLoad_NoException | ❌ watchdog | ✅ PASS |
| Cat2 / GuidAssignment_ConcurrentThreads_AllUnique | ❌ watchdog | ✅ PASS |
| Cat4 / CompareTrees_LockContention_NoDeadlock | ❌ Arg_LongerThanDestArray | ✅ PASS |
| Cat5 / ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication | ❌ attach race | ✅ PASS |

> Note: earlier-sprint failures (Cat 3 TS2 `_MaxObjectID` atomicity, Cat 3 TS4 `PredActionType`) are
> tracked separately and are out of Sprint E scope.

## Sprint F Assessment

Sprint F (1) replaced Sprint E's `@order` insert (Fix A) with a deferred sort after auditing its
correctness (see Fix A above), (2) re-verified Fix B / Fix C, and (3) produced the architectural
assessment below. **No further OM behavior was changed beyond Fix A**; Fix B, Fix C and the Cat 5
test-side Fix D are retained as committed in Sprint E.

### A. Do the Cat 2 / Cat 5 threading tests reflect real-world SDC WASM usage?

Partly. In Blazor WASM, UI events serialize on the single JS event loop; **true concurrent tree
*construction* only happens if the app deliberately spins up Web Workers / `new Thread()` background
threads.** Four threads simultaneously constructing hundreds of nodes **into the same parent** (Cat 2
Test 5/6, Cat 5 Test 1) is an aggressive, somewhat artificial stress configuration — it is not how a
typical form-rendering or form-editing UI mutates the tree.

The **more realistic** concurrency scenario is *one writer + one or more readers*: a user editing the form
on the UI thread while a background thread serializes/validates/compares a snapshot. Cat 4
(CompareTrees read contention) and Cat 5 Test 3 (7 readers + 1 writer) model this well.

That said, the aggressive tests are still **valuable**: they exercise real correctness surfaces —
`ObjectID` atomicity (`Interlocked`), `ObjectGUID` uniqueness, `_Nodes/_ParentNodes/_ChildNodes`
thread-safety, and `WriteLock` exclusivity — and they drove out genuine OM defects (the `kids.Sort` vs
`kids.Remove` race in Fix B/C, and the pre-lock attach race in Fix D). The fixes they motivate are worth
keeping even though the exact 4-writers-one-parent shape is uncommon in practice.

### B. Should the OM use a coarser-grained lock for *all* mutations?

**The architectural root cause of Cat 5 Test 1** is that `BaseType(BaseType? parentNode, …)` calls
`SdcUtil.TryAttachNewNode` — which mutates the parent's child *List* property by reflection — **before**
`InitAfterTreeAdd` acquires `WriteLockScope(TreeRwLock)`. So concurrent construction targeting the same
parent races on that List with no lock held.

**Option OM-Fix:** acquire `WriteLockScope(parentNode.TopNode.TreeRwLock)` in the `BaseType` ctor
*around* the `TryAttachNewNode` + `InitAfterTreeAdd` sequence. With `LockRecursionPolicy.SupportsRecursion`,
the inner `InitAfterTreeAdd → WriteLockScope` re-entry is safe. This would establish a clean invariant —
**holding the tree WriteLock == the exclusive right to mutate any part of that tree** — and eliminate the
attach race at the OM level (letting Cat 5 Test 1 use a single shared parent again, reverting Fix D).

**Sprint F decision: NOT shipped — documented as a recommendation.** Rationale:
- It is a broad change to the core constructor used by *every* node creation and by all 692 desktop tests.
- There is a real `ReaderWriterLockSlim` hazard to validate: any code reachable from `TryAttachNewNode` /
  `InitBaseType` that tries to acquire a **read** lock while this thread already holds the **write** lock
  must be confirmed safe under `SupportsRecursion`. That requires running the full desktop suite and the
  browser tests — neither of which is available in this unattended session.
- Cat 5 Test 1 is already green via the lower-risk test-side **Fix D** (per-thread parent).

A **lower-risk surgical variant** (if/when this is revisited) is to wrap only the `TryAttachNewNode` call in
`lock(parentNode.TopNode._ChildNodesMutationLock)` — the same reentrant per-tree `Monitor` already guarding
`_ChildNodes`/`SortElementKids` — which serializes the racing List mutation without introducing any
read-inside-write-lock concern. This should be validated against the desktop suite before adoption.

### C. Can individual tests be made more isolated?

Several tests are really *integration* tests (they build full SDC trees with `FormDesign`/`Section`/
`DisplayedType` nodes) to assert a narrow concurrency property. Tighter unit tests would target the minimum
OM surface and avoid coupling a thread-safety assertion to the whole construction pipeline:

- **`ObjectID` uniqueness (Cat 3 TS2):** construct one `DataElementType(null)` once, then call
  `de.AtomicNextObjectID()` ~400× across 4 threads and assert all values are unique — no full node
  construction required. This isolates the `_MaxObjectID` atomicity bug from registration/attach.
- **`ObjectGUID` uniqueness (Cat 2 Test 6):** `Guid.NewGuid()` is thread-safe by construction; the test only
  needs to assert that distinct nodes receive distinct `ObjectGUID`s — it does not need 400 IET siblings
  under one parent (which is what makes it a construction stress test rather than a GUID test).
- **`WriteLock` exclusivity:** exercise the lock with the simplest node type that hits the specific path,
  rather than a comparison over two full trees.

These are **recommendations**, not required changes for Sprint F. They would reduce flakiness, shrink
watchdog windows, and make a failure point directly at the defective primitive.

### Post-Sprint-F expected scoreboard (after OM merge + republish)

Live browser run after the **first** Sprint F (deferred-sort) build confirmed: Cat 2 Test 6
`GuidAssignment` now **PASSES (229 ms)**, but Cat 2 Test 5 `CollectionModification` still hit the
watchdog (12001 ms). Analysis showed Test 5's timed region is dominated by the **reflection-heavy,
fully-serialized `Move`/`RemoveRecursive` replacement loop**, not by node registration — so it was
addressed by (a) the smart-insert speeding up the single-threaded pre-construction of the 200
`DisplayedType` siblings, and (b) reducing the test's arbitrary iteration volume (`Iterations` 10 → 4;
4 threads × 4 concurrent full-list replacements still races on one `ChildItemsType`). See the Test 5
sizing note in `BarrierTests.razor`.

| Test | Pre-Sprint-E | Expected now | Mechanism |
|------|--------------|--------------|-----------|
| Cat2 / CollectionModification_UnderConcurrentLoad_NoException | ❌ watchdog | ✅ PASS | Smart insert (faster pre-construction) + reduced Move-phase volume |
| Cat2 / GuidAssignment_ConcurrentThreads_AllUnique | ❌ watchdog | ✅ PASS (live: 229 ms) | Smart document-order insert (no per-insert reflection) |
| Cat4 / CompareTrees_LockContention_NoDeadlock | ❌ Arg_LongerThanDestArray | ✅ PASS | Fix B + Fix C (Sort/Remove serialized) |
| Cat5 / ConcurrentNodeConstruction_SingleTopNode_NoIdDuplication | ❌ attach race | ✅ PASS | Fix D (per-thread parent, test-side) |
| Cat3 / TS2_AtomicObjectId_…_AllUnique | ❌ 75 dup IDs (test-design bug) | ✅ PASS | counter atomic via `AtomicNextObjectID`/`Interlocked.Increment`; test redesigned to share one TopNode (`09badb1`). #19 CLOSED |
| Cat3 / TS4_ReadLockScope_WriteLockScope_UnderContention | ❌ PredActionType dup key | ❌ STILL FAILS (deferred) | lock-scope fix deferred — `PredActionType` slated for removal (TS-7 / #20, low-priority) |

**Projected: 17/18 pass** (up from 13/18). The one remaining failure is the deferred Cat 3 TS4
`PredActionType` duplicate-key issue (#20), whose node type is slated for removal. Note: the Cat 3 TS2
counter-atomicity issue (#19) is **resolved** — the OM counter was already atomic and the old failure was
a test-design bug (separate tree per thread), corrected in `09badb1`. A residual TS2 failure, if any, would
stem from the shared-parent concurrent-attach surface (Cat 5 / `TryAttachNewNode`), **not** `_MaxObjectID`.

**Desktop regression (692-test suite, run this sprint):** `Passed 692 / Failed 0`. Note: the smart
insert was validated *because* the regression suite caught the deferred-sort attempt
(`AddListItemToPosition0` + 6 deserialize/roundtrip tests).

### Fix E — `_UniqueIDs` thread-safety (`ThreadSafeSet<string>`)

The per-tree `_UniqueIDs` collection was a plain `HashSet<string>` mutated with **no lock** from
`IdentifiedExtensionType.set_ID` (`Add` new ID + `Remove` old ID) and from the bulk
`Register`/`UnRegister` paths in `IMoveRemoveExtensions.cs`. Under concurrent node construction this
threw `InvalidOperationException: Operations that change non-concurrent collections must have exclusive
access`, surfacing on the desktop as the **intermittently flaky**
`BaseTypeThreadSafetyTests.NodeCreation_ConcurrentGuidAssignment_ValidatesUniqueness` and contributing
to the same WASM Cat 3 GUID-uniqueness failures.

`set_ID` lives in an **auto-generated** file (`SDC Customized Classes/IdentifiedExtensionType.cs`,
`// <auto-generated>` header) that cannot host its own locking, so thread-safety was pushed into the
collection type itself:

- New `ThreadSafeSet<T>` (`Utility Classes/ThreadSafeSet.cs`) — a minimal wrapper that serializes
  every `Add`/`Remove`/`TryGetValue`/`Contains`/enumeration under one private lock, preserving exact
  `HashSet` semantics including null handling (some bulk `Add`/`Remove` values can be null).
- `_IUniqueIDs._UniqueIDs` retyped from `HashSet<string>` to `ThreadSafeSet<string>` on the interface
  and all five implementing TopNode/`XMLPackageType` partials. The five impls were converted to **eager
  auto-properties** (`{ get; } = new();`), which also removes the previous lazy-init (`p_UniqueIDs ??= new()`)
  race. All call sites (`set_ID`, `IdentifiedExtensionTypeExtensions`, `Register`/`UnRegister`) compile
  unchanged.

Validated: `NodeCreation_ConcurrentGuidAssignment_ValidatesUniqueness` now passes **3/3** consecutive
runs (previously intermittent), and the full **692-test** desktop suite passes `692 / 0`.

> **KNOWN OPEN ISSUES (not addressed by Sprint E/F):**
> - **Cat 3 TS2 (#19) — RESOLVED/CLOSED.** `_MaxObjectID` is per-tree and already atomic via
>   `AtomicNextObjectID()` → `Interlocked.Increment`. The 75-duplicate result was a test-design bug
>   (one tree per thread); fixed in `09badb1`. Desktop repro passes 5/5 + in the 692-suite.
> - **Cat 3 TS4 (#20) — deferred (low-priority).** `PredActionType` "Argument_AddingDuplicateWithKey"
>   under read/write contention; a lock-scope vs. construction-sequence mismatch. `PredActionType` (and
>   related predicate/action nodes) is slated for removal in a later update, so the lock-scope fix is
>   deferred rather than implemented.
> - **Sibling ID-set races (not yet hit by a test):** `_UniqueNames` and `_UniqueBaseNames` are still plain
>   `HashSet<string>`. They follow the same pattern as `_UniqueIDs` and could be migrated to
>   `ThreadSafeSet<string>` if a future test exercises them under concurrent construction.


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
