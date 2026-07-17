# Thread Safety

> **⚠️ Important scope caveat:** This chapter documents the **desktop, single-process** thread-safety
> investigation only (TS-1 through TS-7 below, all fixed). A **separate, still-open** investigation
> into thread safety under **real WebAssembly (WASM) multi-threading** (`WasmEnableThreads`,
> `WasmPThreadPoolSize`, exercised by the `SDC.ScriptEngine.BlazorAsyncTests.Phase2` test project) has found
> additional, unresolved bugs — duplicate-key exceptions and deadlocks — and, confusingly, **reuses
> some of the same `TS-#` numbers** (TS-4, TS-5, TS-7, TS-8, TS-9) for different defects than the
> ones described here. Do **not** read "TS-7 fixed" below as meaning the WASM TS-7 is also fixed —
> it is a different bug with a colliding label. See open GitHub issues
> [#20](https://github.com/rmoldwin/SDC_ObjectModel/issues/20),
> [#21](https://github.com/rmoldwin/SDC_ObjectModel/issues/21),
> [#22](https://github.com/rmoldwin/SDC_ObjectModel/issues/22),
> [#23](https://github.com/rmoldwin/SDC_ObjectModel/issues/23),
> [#24](https://github.com/rmoldwin/SDC_ObjectModel/issues/24),
> [#25](https://github.com/rmoldwin/SDC_ObjectModel/issues/25), and
> [#28](https://github.com/rmoldwin/SDC_ObjectModel/issues/28) for the current, unresolved WASM
> thread-safety backlog, tracked separately in [../roadmap.md](../roadmap.md).

> **Status:** Living document — describes the **final, implemented** state, with a short history
> of the investigation that led there. Consolidated from `ThreadSafety_RootCauseDiagnosis.md`,
> `ThreadSafety_RemediationPlan_OptionC.md`, and `ThreadSafety_TS6_Complete_Handoff.md`
> (originally in `SDC.Schema.Tests/Documentation`, now archived — see [../changes/](../changes/)).
> Two earlier exploratory documents — `ThreadSafety_StrategyDecision.md` (first draft, before the
> root-cause investigation) and `ThreadSafety_LockingStrategy_Analysis.md` (an alternate analysis
> that recommended `SemaphoreSlim` instead) — were superseded by the design below and are archived
> alongside the others; they are not reflected here except where noted.

## Current status

All seven identified thread-safety defects (numbered **TS-1** through **TS-7**, using "TS" as
the canonical prefix — not "RC", which means Release Candidate elsewhere in this project) are
fixed and verified **in the desktop/single-process investigation described in this chapter**:
12/12 dedicated thread-safety tests pass, and the full test suite passes except for pre-existing
serializer/MessagePack (MsgPack) issues unrelated to threading (see [serialization.md](serialization.md)).
This does **not** cover the separate, still-open WebAssembly (WASM) multi-threading investigation
described in the caveat above.

| ID | Description | Fix |
|----|---|---|
| TS-1 | `static LastTopNode` field caused cross-tree contamination between concurrently-constructed trees | Converted to `[ThreadStatic]` in `PartialClasses.cs` |
| TS-2 | Unsynchronized `Dictionary<>` reads during concurrent writes (the root cause of the original crash-loop hang) | `WriteLockScope`/`ReadLockScope` (see below) added at every reader/writer boundary |
| TS-3 | Non-atomic `_MaxObjectID++` produced duplicate `ObjectID`s under concurrency | `Interlocked.Increment` via a new `AtomicNextObjectID()` helper |
| TS-4 | `ItemsMutator`'s `lock(new object())` fallback provided zero mutual exclusion | Replaced with `WriteLockScope` on the tree's real lock |
| TS-5 | `TreeLock` (a `SemaphoreSlim`) was live in `CompareTrees.cs` (18 call sites guarding a parallel read) on a *separate* lock regime from writers' `_SyncRoot` — so readers and writers never excluded each other | Migrated `CompareTrees.cs` onto the unified `ReaderWriterLockSlim`, verified, then deleted the old `TreeLock`/`_SyncRoot` (migrate-then-delete, never delete-first) |
| TS-6 | `Move()` read `_ParentNodes`/`_ChildNodes` and mutated `IList<T>` parent/child lists with no lock held at all | A `DualWriteLock` is acquired at the very top of `Move()`, before any dictionary read |
| TS-7 | `RegisterAll`'s child-list sort (`kids.Sort(treeSibComparer)`, driven by reflection) ran on *every* insert, serialized under the write lock — an O(N²·reflection) performance cliff that could exceed test watchdog timeouts under bulk insert | Batch/defer the sort (`childNodesSort` flag) so it runs once after bulk inserts, not per-insert |

## Why this happened: root-cause diagnosis

The project's chosen strategy (a `ReaderWriterLockSlim` per `TopNode`, single-writer/multiple-reader)
was decided early, but was never actually implemented — the code instead grew **two disconnected
lock regimes**: ad hoc `lock(_SyncRoot)` (a plain `Monitor`) on some write paths, and a separate
`TreeLock` (`SemaphoreSlim`) used only inside `CompareTrees.cs` to guard its own parallel read.
Because readers and writers locked *different objects*, they never mutually excluded each other —
plain `Dictionary<TKey,TValue>` is explicitly documented as unsafe for concurrent read+write, so a
writer mutating a dictionary while a reader iterated it could spin forever following a corrupted
bucket chain (100% CPU hang) or throw. This matched a real crash-loop (10 sequential `testhost.exe`
crash dumps) that ended a prior session, in violation of the project rule that tests must never be
allowed to enter an infinite loop.

Reproduction work (`SDC.Schema.Tests/OMTests/ThreadSafetyReproTests.cs`, watchdog-protected)
confirmed the stall was real (not a test-harness artifact) and that, at high node counts, the TS-7
performance cliff could **dominate and mask** the underlying TS-2 hang — informing the fix
ordering (batch the sort first, then re-verify the read/write hang surface is also fixed).

A related, subtler finding: several APIs that look like pure reads (`GetPrevSibElement`, sibling
navigation helpers) actually **lazily mutate** shared state on first access (an in-place sort of
`_ChildNodes`, `TreeSort_Add`). A plain read lock is insufficient for these — the fix runs a
single-threaded pre-sort pass under the write lock before any parallel read (e.g. in
`CompareTrees`), so the lazy sort becomes a true no-op by the time the parallel read runs.

## Locking strategy: Option C (single-writer / multiple-reader)

The SDC object model (OM) is **read-heavy and write-rare** (roughly 100:1 in typical use), which is
the textbook profile for a **single-writer/multiple-reader (SWMR)** lock: one
`ReaderWriterLockSlim` per `TopNode`, constructed with `LockRecursionPolicy.SupportsRecursion`.
Many readers run concurrently with zero copying; a rare writer takes the lock exclusively and
mutates the tree **in place** (both trees, ordered by `TopNode.ObjectGUID`, for a cross-tree move).

**Rejected alternatives:**
- **Copy-on-write snapshots** — zero reader-blocking, but adds a copy-per-write-transaction and
  publish machinery that isn't needed given how rare writes are.
- **`ConcurrentDictionary`** — fixes single-operation tearing, but not the multi-dictionary
  invariant (a reader could observe `_ParentNodes` updated but `_ChildNodes` not yet updated).
- **`SemaphoreSlim`** for writers — breaks the write-within-write reentrancy the code relies on
  (`RegisterAll` is called from inside `InitAfterTreeAdd`). An earlier analysis explored
  `SemaphoreSlim` as the primary mechanism (for async/await and WebAssembly, or WASM,
  compatibility), but this was superseded once it became clear the library is 100% synchronous —
  see "Async/await and WASM" below.

### THE ONE RULE

1. Acquire the lock at the **public boundary** of an operation, once.
2. **Readers** take the **read** lock; **writers** take the **write** lock.
3. A writer takes the **write** lock at the very top of the operation — **never** take a read
   lock and later try to upgrade to a write lock (the classic `ReaderWriterLockSlim` deadlock).
4. Internal helper methods invoked while a lock is already held must **not** take the lock again
   at a different level; `SupportsRecursion` covers genuine same-kind nesting (read-in-read,
   write-in-write) only.

This resolves the OM's inherent reentrancy patterns: `FindRootNode` reads-within-reads via the
`ParentNode` getter; `ItemsMutator` reads then writes; `RegisterAll` is called from inside
`InitAfterTreeAdd` (write-in-write).

### Allocation-free lock scopes

`SDC.Schema/Utility Classes/TreeLockScope.cs` defines two `ref struct`s,
`ReadLockScope`/`WriteLockScope`, giving `using`-based, exception-safe lock/unlock with zero heap
allocation on the hot read path.

### Async/await and WASM

An early design question was whether the lock needed to be async-compatible for Blazor/WASM
hosting. This is **resolved**: the library is 100% synchronous, so the "never `await` inside a
lock" rule is a free compile-time guardrail rather than an active risk, and a full async test
harness for WASM was deferred in favor of a cheaper desktop reentrancy proxy. See the planned
"Running in WASM / Blazor" chapter (`architecture/wasm-blazor.md`, tracked in
[../roadmap.md](../roadmap.md)) for WASM hosting considerations more generally.

## Test coverage

`SDC.Schema.Tests/OMTests/ThreadSafetyReproTests.cs` and the dedicated `ThreadSafety`-filtered test
suite (12/12 passing) cover TS-1 through TS-7, including dedicated repro tests for the TS-6
`Move()` fix (`Repro_ConcurrentMoves_UnprotectedListMutations_CorruptListIntegrity`,
`Repro_ConcurrentCrossTreeMoves_UpdateNodeIdentityPath_NoCorruption`,
`Repro_ConcurrentCrossTreeMoves_DoNotDeadlock`).
