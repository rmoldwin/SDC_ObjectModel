# SDC Object Model — Threading / Async / Concurrency Changes vs `Net10Main`

**Branch compared:** `Features/NET10/ILandWASM/Main` (HEAD `3739e98`) **vs** `Features/NET10/Net10Main`
**Merge-base:** `d5b88e8`
**Status:** ⚠️ **NOT merged into `Net10Main`.** This document is the single consolidated reference for
every thread-safety / concurrency change made on the WASM line, so the work can be reviewed in one place
before any merge decision.

> **Why this work exists:** The `.NET 10` Blazor WASM test app runs with `WasmEnableThreads=true` /
> `WasmPThreadPoolSize=4`, so SDC trees can be built and read from multiple real OS threads. The SDC
> Object Model was originally written for single-threaded use; its per-tree registries, lazy-init
> getters, ID counters, sort routines, and static reflection caches were all racy under true threading.
> The changes below make the OM safe under concurrent construction/read without changing its
> single-threaded behavior or public API.

---

## 1. Scope at a glance

Production files changed (8), `+331 / −254`:

| File | Area | Nature of change |
|------|------|------------------|
| `Interfaces/ITopNode.cs` | registries + lock contract | `Dictionary`→`ConcurrentDictionary`; new `_ChildNodesMutationLock` member |
| `Interfaces/Interfaces.cs` | ID uniqueness set | `_UniqueIDs` retyped `HashSet`→`ThreadSafeSet` |
| `Partial Classes/PartialClasses.cs` | 5 TopNode impls | concurrent registries, atomic ID counter, race-free lazy init, per-tree mutation lock, `_UniqueIDs` eager init |
| `Utility Classes/ThreadSafeSet.cs` | **new file** | lock-guarded `HashSet` wrapper |
| `Utility Classes/SdcUtil.cs` | sort + caches + IDs | `ConcurrentDictionary` caches w/ `GetOrAdd`; atomic IDs; locked `SortElementKids` (+ orphan fallback) |
| `Utility Classes/Extensions/IMoveRemoveExtensions.cs` | register / unregister | atomic IDs; `GetOrAdd`/`TryAdd`/`TryRemove`; smart document-order insert; locked list remove |
| `Utility Classes/IComparer/CompareTrees.cs` | tree diff | **removed** internal read-locks (lock now owned by callers) |
| `Utility Classes/Extensions/ITopNodeExtensions.cs` | accessor | return type `ConcurrentDictionary` |

There are **no `async`/`await` changes** in the OM itself — the OM is synchronous. "Async" in this line of
work refers only to the multi-threaded WASM *test harness* (`SDC.ScriptEngine.BlazorAsyncTests.Phase2`),
which exercises the OM from `new Thread()` workers synchronized by `Barrier`/`CountdownEvent`. The OM's
job is to be correct under those threads; it does not itself schedule work.

---

## 2. The concurrency model (after these changes)

Each `ITopNode` tree (`FormDesignType`, `DataElementType`, `RetrieveFormPackageType`, `PackageListType`,
`MappingType`) owns its own synchronization primitives — there is **no global lock**:

1. **`TreeRwLock`** — `ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)`. Reader/writer gate
   for tree-wide read vs. mutate operations (`ReadLockScope` / `WriteLockScope`). Recursion-enabled so a
   write scope can re-enter (e.g. construction → registration).
2. **`_ChildNodesMutationLock`** — a plain `object` Monitor lock (added Sprint D). Serializes all
   mutations to the **value lists** inside `_ChildNodes` (`List<BaseType>`), which `ConcurrentDictionary`
   does *not* protect (it only protects the dictionary's own key/bucket structure, not the mutable
   `List<BaseType>` stored as a value). Monitor is reentrant per-thread, so `SortElementKids` →
   `RegisterParentNode` re-entry on the same thread is safe.
3. **Atomic counter** — `_MaxObjectID` is incremented only via `AtomicNextObjectID()` →
   `Interlocked.Increment`.
4. **`ConcurrentDictionary`** registries — `_Nodes`, `_ParentNodes`, `_ChildNodes` are lock-free for
   key operations.
5. **`ThreadSafeSet<string>`** — `_UniqueIDs` self-synchronizes its `Add`/`Remove`/`TryGetValue`.

**Lock-ordering / deadlock note:** `_ChildNodesMutationLock` is intentionally a *single per-tree* lock
rather than one lock per child list. The comparers (`TreeSibComparer`) take **no** locks, and
`SortElementKids` re-enters the same per-tree Monitor, so there is no AB–BA cycle. The earlier Sprint D
attempt used `lock(kids)` (per-list), which created an AB–BA hazard when `TreeSibComparer.Compare`
re-entered `SortElementKids` on a *different* parent's list — that is why Fix2 replaced it with the
per-tree lock.

---

## 3. Change-by-change detail

### 3.1 TS-3 — Concurrent registries (`42a5dd3`, `69f375d`)
- `_Nodes`, `_ParentNodes`, `_ChildNodes` changed from `Dictionary<…>` to
  `ConcurrentDictionary<…>` on the `_ITopNode` interface, all 5 TopNode implementations, and the
  `SdcUtil.Get_*` / `ITopNodeExtensions._Nodes` accessors.
- Registration calls updated to the concurrent API: `Add`→`TryAdd`, `Remove`→`TryRemove(out _)`,
  and the orphan-prone `TryGetValue`+`Add` child-list creation replaced by an atomic
  `GetOrAdd(parentGuid, _ => new List<BaseType>())`.
- **Lazy-init race fixed:** the backing-field getters (`p_Nodes` etc.) previously used
  `if (p_X is null) p_X = new();` (a check-then-act race). Now they use
  `Interlocked.CompareExchange(ref p_X, new ConcurrentDictionary<…>(), null)` so two threads racing
  first-access converge on one instance.
- Removed the cached `p_NodesRO` `ReadOnlyDictionary` field; `_NodesRO` now returns a fresh snapshot
  (`new(((_ITopNode)this)._Nodes)`) each call, eliminating a shared-mutable-cache race.

### 3.2 TS-2 — Atomic `ObjectID` counter (`0fe7d0d`, and Sprint F verification)
- Added `int _ITopNode.AtomicNextObjectID() => Interlocked.Increment(ref _maxObjectID_XX);` to each of
  the 5 TopNode types (each has its own private `int _maxObjectID_XX` backing field).
- Replaced every `ObjectID = ((_ITopNode)…)._MaxObjectID++;` (non-atomic read-modify-write) with
  `AtomicNextObjectID()` — in `BaseType` construction (`PartialClasses.cs`), `RegisterNode` /
  `RegisterNodeAndChildren` (`IMoveRemoveExtensions.cs`), and `ReflectRefreshTree` /
  `UpdateNodeIdentity` (`SdcUtil.cs`). No `_MaxObjectID++` remains.
- **Why a default interface method can't do this:** `Interlocked.Increment` needs a `ref` to a concrete
  field; an interface has no fields, so the atomic op must live in each implementing class.
- **GitHub #19 (TS-6) is CLOSED/COMPLETED.** The original WASM "75 duplicates" was a *test-design* bug
  (one tree per thread → independent per-tree counters); the OM counter was already atomic. Desktop
  repro `ThreadSafetyReproTests.Repro_NonAtomicMaxObjectID_…` passes 5/5 isolated + in the 692 suite.

### 3.3 Thread-safe static reflection caches (`0fe7d0d`, `7ad6da8`)
- The `SdcUtil` static reflection caches (`dListPropInfoElements`, `dListPropInfoAttributes`,
  `dXmlRootAtts`, `dXmlElementAtts`, `dXmlChoiceIdentifierAtts`, `dXmlAttAtts`, `dListAttInfo`) changed
  from `Dictionary` to `ConcurrentDictionary` and their populate sites changed from
  `TryGetValue`+`Add` to atomic `GetOrAdd(type, static t => …)`. These are process-wide caches shared by
  every tree on every thread, so this removes a real cross-tree reflection race.

### 3.4 Cat4 / TS-4 — `CompareTrees` lock relocation (`0fe7d0d`, then `86c5cb1`)
- Sprint C originally wrapped `CompareTrees` constructors in `ReadLockScope`(s) (in GUID order, with a
  same-tree special case) to make tree comparison safe.
- Sprint D **removed** those internal read-locks from `CompareTrees.cs`. Rationale: holding read locks
  inside the comparer conflicted with writer paths and the `_ChildNodesMutationLock` now serializes the
  list mutations that actually matter; the comparer reads sorted snapshots via
  `GetSortedNonIETsubtreeList`. (Remaining edits in this file are cosmetic `var`→explicit-type.)

### 3.5 Sprint D — per-tree `_ChildNodesMutationLock` (`86c5cb1`, `f606d04`)
- Added `object _ITopNode._ChildNodesMutationLock { get; } = new object();` to all 5 TopNode types and
  the interface.
- `SortElementKids` (`SdcUtil.cs`) now sorts under `lock(_ChildNodesMutationLock)` instead of touching
  the list unguarded. **Fix2** replaced the initial `lock(kids)` (per-list) with the per-tree lock to
  remove the AB–BA deadlock hazard described in §2.
- **Orphan fallback (Sprint E Fix C):** when a node has no TopNode yet (e.g. mid-deserialization,
  `_ChildNodesMutationLock` not reachable), `SortElementKids` falls back to `lock(kids)` so two
  deserialization workers can't `Sort()` the same list at once (which threw `Arg_LongerThanDestArray`).

### 3.6 Cat4 Test3 — locked list removal (Sprint E Fix B, in `ccdb190`)
- `UnRegisterParentNode` now removes from a child `List<BaseType>` under `lock(_ChildNodesMutationLock)`
  (with a null-lock fallback when no TopNode exists). Previously a PLINQ `CompareTrees` worker sorting a
  child list raced an `UnRegisterParentNode` removing from it → `Arg_LongerThanDestArray`. The
  `_ChildNodes.TryRemove` of the now-empty entry is inside the same lock.

### 3.7 Cat2 Test5/Test6 — registration insert performance + correctness
This is the most iterated change; three commits, final design shipped:
- **Sprint D** inserted each child via an O(log k) **reflection** binary search on every
  `childNodesSort:false` registration → O(k log k) reflection compares per parent → WASM watchdog
  timeouts when building many siblings.
- **Sprint E Fix A** (`ccdb190`) tried a pure `@order` (`TreeOrderComparer`) insert. **Rejected** —
  `ReflectRefreshTree.DoTree` calls `RegisterAll(childNodesSort:false)` *before* it reassigns `@order`,
  so on refresh/deserialize `@order` is stale/uniform (0); `TreeOrderComparer` is non-antisymmetric for
  equal orders and could **scramble** the list.
- **Sprint F first attempt** (`331d8ab`) used a deferred plain append + `TreeSort_Invalidate`. **Rejected**
  — plain append left `_ChildNodes` in construction order, but `FindPrevIETInDictionaries` walks those
  lists assuming **document order** to find the IET predecessor, breaking explicit position-0 inserts
  (`QuestionItemTypeTests.AddListItemToPosition0`).
- **Sprint F final** (`97c296b`) — `SmartInsertInDocumentOrder` (new static local in
  `RegisterIn_ParentNodes_ChildNodes`):
  - **Fast path:** one reflection compare of the new node vs. the current last sibling; if `>= 0`,
    **O(1) append** (the common case — bulk/construction/deserialize callers add in document order).
  - **Slow path:** reflection binary search only for a genuinely out-of-document-order insert.
  - **Fallback:** `catch (… InvalidOperationException or ArgumentException)` → plain append +
    `SdcUtil.TreeSort_Invalidate(parent)` (covers a node not yet wired into parent properties during
    concurrent construction, or a transiently-null `ParentNode` during `ReflectRefreshTree`).
  - All of this runs inside `lock(_ChildNodesMutationLock)`. The `childNodesSort:true` path
    (Move/AddChild edits) keeps the robust full `kids.Sort(treeSibComparer)`.
  - Net: ~1 reflection compare per node instead of O(log k), preserving exact document order → fixes the
    WASM watchdog timeouts **and** keeps IET indexing correct. Verified by the full 692-test desktop
    suite (`692/692`) which caught both rejected attempts.

### 3.8 Sprint F — thread-safe `_UniqueIDs` (`3739e98`)
- New `Utility Classes/ThreadSafeSet.cs`: `internal sealed class ThreadSafeSet<T> : IEnumerable<T>` —
  serializes `Add`/`Remove`/`TryGetValue`/`Contains`/`Clear`/`Count`/enumeration under one private
  Monitor, preserving exact `HashSet` semantics including null handling. Enumeration returns a snapshot.
- `_IUniqueIDs._UniqueIDs` retyped `HashSet<string>` → `ThreadSafeSet<string>` on the interface and all
  5 implementations, which were converted to **eager auto-properties** (`{ get; } = new();`),
  eliminating the prior lazy `p_UniqueIDs ??= new()` init race.
- **Why a wrapper instead of locking the call sites:** the hottest mutator,
  `IdentifiedExtensionType.set_ID` (`Add` new ID + `Remove` old ID), lives in an `// <auto-generated>`
  file that cannot host its own `lock`. Pushing synchronization into the collection type makes every
  call site (incl. the auto-generated setter and the bulk `Register`/`UnRegister` paths) safe with no
  edits to generated code. This fixed the intermittent
  `BaseTypeThreadSafetyTests.NodeCreation_ConcurrentGuidAssignment` flake (now 3/3, and green in the
  692 suite).

---

## 4. Verification status

- **Desktop regression:** `692 / 692` pass on `ILandWASM/Main` HEAD (`3739e98`), run multiple times.
- **Targeted re-runs:** `Repro_NonAtomicMaxObjectID_…` 5/5; `NodeCreation_ConcurrentGuidAssignment` 3/3.
- **WASM (browser, run manually by user):** Cat2 Test6 `GuidAssignment` PASS (229 ms); Cat2 Test5
  `CollectionModification` addressed via smart-insert + reduced Move-phase volume; projected **17/18**.
- **Build:** OM and Phase2 server build with `0 errors` (pre-existing SonarAnalyzer warnings only).

---

## 5. Open / out-of-scope items

| Item | Issue | Status |
|------|-------|--------|
| `PredActionType` duplicate-key under read/write contention (Cat3 TS4) | #20 | **Deferred / low-priority** — node type slated for removal in a later update; lock-scope fix not worth implementing |
| `_UniqueNames` / `_UniqueBaseNames` still plain `HashSet<string>` | — | Not yet hit by a test; same pattern as `_UniqueIDs`, migrate to `ThreadSafeSet<string>` if a future test exercises them concurrently |
| `TryAttachNewNode` mutates parent property list before `WriteLockScope` | — | Architectural note: true concurrent construction into the **same** parent still relies on the test-side per-thread-parent pattern (Cat5). A coarser "acquire WriteLock before TryAttachNewNode" option was evaluated but not adopted to avoid serializing all independent-subtree construction |

---

## 6. Commit trail (newest → oldest)

```
3739e98  Sprint F: thread-safe _UniqueIDs via ThreadSafeSet wrapper
97c296b  Sprint F Fix2: smart document-order insert in RegisterParentNode
331d8ab  Sprint F: replace Fix A @order insert with deferred sort (superseded by 97c296b)
ccdb190  Sprint E: WASM thread-safety in node registration/sort (Fix B, Fix C)
f606d04  Sprint D Fix2: per-tree _ChildNodesMutationLock replaces per-list lock(kids)
86c5cb1  Sprint D: lock(kids) in RegisterParentNode + SortElementKids; remove WriteLock from CompareTrees
c446e27 / 7ad6da8  Sprint C Fix2: GetOrAdd for SdcUtil caches; WriteLock for GetSortedNonIETsubtreeList
19bd1be / 0fe7d0d  Sprint C: TS-2 ObjectID atomicity + thread-safe static caches + CompareTrees read-lock
69f375d / 42a5dd3  TS-3: _Nodes/_ParentNodes/_ChildNodes → ConcurrentDictionary
```

> **Do not merge into `Net10Main` yet** (per current instruction). When ready, merge
> `Features/NET10/ILandWASM/Main` → `Features/NET10/Net10Main`; the only new file is
> `Utility Classes/ThreadSafeSet.cs`, and no public API signatures changed.
