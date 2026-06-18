# Thread Safety — Root Cause Diagnosis (Post-Crash Investigation)

**Branch:** Features/Net11Upgrade_ThreadSafety
**Context:** Reconstructed after a `testhost.exe` crash-loop ended the prior dev session (10 sequential crash dumps, 6/16, 2:14–3:03 PM).
**Scope:** Diagnosis + remediation design only. No production-code changes are made by this document.

> This document **builds on**, and does not replace, the existing analysis:
> - `ThreadSafety_StrategyDecision.md` (chosen strategy: **ReaderWriterLockSlim + mutable references**)
> - `ThreadSafety_LockingStrategy_Analysis.md` (locking deep-dive; **active** — helpful overview for future work)
> - `Archived Plans\ThreadSafety_Phase1_BLOCKED_STATUS.md` (Task 1.1 infra; the `TreeLock` "public" blocker is now **resolved** in code)
> - `Archived Plans\ThreadSafety_ArchitecturalAnalysis.md`, `Archived Plans\ThreadSafety_AuditChecklist.md` (superseded — see `Archived Plans\README.md`)

---

## 0. Key Finding — Strategy vs. Implementation Mismatch

| Item | Documented Strategy | Current Code Reality |
|------|--------------------|----------------------|
| Primary lock | `ReaderWriterLockSlim` per TopNode | `object _SyncRoot` (Monitor) per TopNode |
| Reader protection | Read lock on every dictionary read | **None** — reads are unlocked |
| Writer protection | Write lock on all mutations | `_SyncRoot` on **some** write paths only |
| `TreeLock` (SemaphoreSlim) | (not the chosen mechanism) | Declared on every TopNode; **LIVE in `CompareTrees.cs` (18 sites) guarding a PLINQ read** — a *separate* lock regime from writers' `_SyncRoot`, so reader and writer do **not** mutually exclude |

The chosen `ReaderWriterLockSlim` design was **never implemented**. The code instead grew **two disconnected lock regimes**: ad-hoc `lock(_SyncRoot)` blocks on *write* paths, and `TreeLock` (SemaphoreSlim) on CompareTrees' *parallel read* path — while **all other read paths remain unlocked**. Because readers and writers lock **different objects**, they don't exclude each other. This half-locked, split-regime state is the direct cause of the crash-loop.

---

## 1. Crash Mechanism (why the testhost looped)

`Dictionary<TKey,TValue>` is explicitly documented as **not** safe for concurrent read + write. When one thread mutates a dictionary (triggering a bucket resize) while another thread reads it, the reader can:
- spin forever following a corrupted bucket chain → **100% CPU hang**, or
- throw `InvalidOperationException` / `IndexOutOfRangeException`.

A hung testhost is killed by the runner's timeout → forced crash dump → restart → re-run → hang again. This matches the **10 sequential `testhost.exe.*.dmp` files** and violates the repo rule *"tests must never be allowed to enter an infinite loop."*

---

## 2. Confirmed Root Causes (with evidence)

### RC‑1 — Process-global static `LastTopNode` (PRIMARY contamination driver)
- **Evidence:** `PartialClasses.cs:2348` `private static ITopNode? LastTopNode;`
  Read/written in parameterless ctor (`PartialClasses.cs:1728-1745`), `InitBaseType`, reset via `ResetLastTopNode()` (`PartialClasses.cs:2198`).
- **Problem:** A single `static` field is shared by **all threads and all trees** in the process. During concurrent construction, thread A's `new FormDesignType()` overwrites `LastTopNode` while thread B is mid-build → B's descendants register into **A's** dictionaries.
- **Impact:** Cross-tree contamination, wrong `TopNode`, corrupted `_Nodes`/`_ParentNodes`/`_ChildNodes`. Also makes parallel MSTest classes contaminate each other even without explicit `Parallel.For`.

#### RC‑1 lifecycle (verified Step 3)
- **Isolation:** NONE. No `[ThreadStatic]`, `AsyncLocal<T>`, or `ThreadLocal<T>` exists anywhere in the production code.
- **Writes:** parameterless ctor `PartialClasses.cs:1732,1739`; reset to null at `:2200`.
- **Reads:** parameterless ctor `PartialClasses.cs:1730,1738,1743,1745,1748`.
- **Resets (17 sites):** every TopNode parameterless ctor (`:140,438,660,950,1170`) + all serializers (`SdcSerializer*.cs`).
- **Contamination window (concurrent deserialization / multi-tree build):**
  1. Thread A `DeserializeFromXml` → `ResetLastTopNode()` (null) → builds TopNode (`LastTopNode = A`) → starts creating child nodes (each reads `LastTopNode == A`).
  2. Thread B concurrently `DeserializeFromXml` → `ResetLastTopNode()` **wipes A's context mid-stream** → builds its TopNode (`LastTopNode = B`).
  3. Thread A's *next* child node reads `LastTopNode == B` → registers into **B's** dictionaries. Both trees corrupt.
- **Scope nuance (which path each failing test uses):**
  - `LastTopNode` is consumed by the **parameterless** ctor (deserialization) and by construction of *separate* top-level trees.
  - The crashing `_OMTreeStabilityTests` / `BaseTypeThreadSafetyTests` mostly use the **parameterized** ctor (`new FormDesignType(null, id)`, `new QuestionItemType(de, …)`), whose `InitBaseType` derives `TopNode` from `parentNode` and does **not** read `LastTopNode`.
  - ⇒ For those specific stress tests the **immediate** crash trigger is **RC‑2** (unlocked reads during writes), while **RC‑1** is the latent corruption bug for concurrent deserialization and concurrent multi-tree creation. Both must be fixed.

### RC‑2 — Unsynchronized dictionary READS during writes (the hang itself)
- **Evidence:**
  - `ParentNode` getter `PartialClasses.cs:2143` → `_ParentNodes.TryGetValue(...)` with **no lock**.
  - `FindRootNode` `BaseTypeExtensions.cs:418-429` loops calling `ParentNode` (unlocked) — used by `Move` to compute `sameRoot`.
- **Problem:** Writers (`RegisterAll`/`UnRegisterAll`) mutate these dictionaries under `_SyncRoot`; readers take **no** lock → classic concurrent read/write on `Dictionary<>`.
- **Impact:** The hang/crash mechanism in §1.

### RC‑3 — Non-atomic `_MaxObjectID++`
- **Evidence:** `PartialClasses.cs:1745` and `RegisterAll` (`IMoveRemoveExtensions.cs:819`) do `ObjectID = ((_ITopNode)TopNode)._MaxObjectID++;`
- **Problem:** Read-modify-write on a shared `int` with no `Interlocked.Increment`.
- **Impact:** Duplicate `ObjectID`s under concurrency → later dictionary keying/ordering errors.

### RC‑4 — Inconsistent / ineffective write locking
- **Evidence:** Write paths lock `_SyncRoot` (`RegisterAll` `IMoveRemoveExtensions.cs:826`, `UnRegisterAll` `:1061`, `InitAfterTreeAdd` `PartialClasses.cs:1874`, `BaseName` setter `:2312`). But `ItemsMutator` (`PartialClasses.cs:2265`) falls back to `lock (new object())` when `TopNode` is not `_ITopNode`.
- **Problem:** Locking a freshly-allocated object provides **zero** mutual exclusion; and reads are never coordinated with these write locks anyway.
- **Impact:** Race windows persist even where a lock "appears" present.

### RC‑5 (≡ TS‑5) — `TreeLock` (SemaphoreSlim) is LIVE in CompareTrees → migrate-then-delete
> ⚠️ **CORRECTION (supersedes the earlier "dead infrastructure / call-site count = 0" claim, which was wrong).** `TreeLock` **is** acquired: `CompareTrees.cs` calls `TreeLock.Wait()`/`.Release()` at **18 sites (lines 126–303)** to guard a **parallel read** (`AsParallel().ForAll`, ~line 347) over the shared dictionaries. Writers separately use `lock(_SyncRoot)` (Monitor). The earlier grep that returned 0 missed the `CompareTrees.cs` usage.
- **Evidence (corrected):** Declared on every TopNode partial (`PartialClasses.cs:157,385,608,898,1097`) + interface (`ITopNode.cs`); **acquired at 18 sites** in `CompareTrees.cs:126–303` guarding the PLINQ read at `:~347`. Writers use a **different** lock (`_SyncRoot`).
- **Problem:** `TreeLock` (reader regime) and `_SyncRoot` (writer regime) are **different objects → they do NOT mutually exclude.** A writer on another thread can mutate a `Dictionary` while CompareTrees' `ForAll` reads it ⇒ torn buckets / `InvalidOperationException` / 100% CPU hang. CompareTrees is protected from *other CompareTrees calls* but **not from writers.**
- **Impact:** Real concurrent read/write corruption window (see remediation §12). **Fix = migrate, then delete:** repoint CompareTrees' `TreeLock` reads + writers' `_SyncRoot` onto the unified `ReaderWriterLockSlim` (reader→read lock, writer→write lock), verify, **then** delete both. **Never delete-first** — that breaks the CompareTrees build.

### RC‑6 — Cross-tree `Move` dictionary-migration gap
- **Evidence:** `MoveInDictionaries` (`IMoveRemoveExtensions.cs:35-60`) throws on `TopNode` mismatch and depends on `ReflectRefreshSubtreeList` having already migrated dictionary entries; migration reads/writes `_Nodes`/`_ParentNodes`/`_ChildNodes` unlocked.
- **Problem:** Multi-step migration is neither atomic nor lock-ordered across the two trees.
- **Impact:** Matches last session's noted "cross-tree Move() dictionary migration limitation"; partial failure leaves both trees inconsistent.

---

## 3. Shared State Inventory (the data that needs protection)

Per TopNode (`_ITopNode`):
- `Dictionary<Guid,BaseType> _Nodes`
- `Dictionary<Guid,BaseType> _ParentNodes`
- `Dictionary<Guid,List<BaseType>> _ChildNodes`
- `ObservableCollection<IdentifiedExtensionType> _IETnodes`
- `HashSet<string> _UniqueBaseNames`, `_UniqueNames`; `HashSet<int> _TreeSort_NodeIds`; `_UniqueIDs`
- `int _MaxObjectID`

Process-global statics:
- `BaseType.LastTopNode` (RC‑1) — the worst offender.

---

## 4. Reader vs. Writer Map (completed in Step 2)

### 4a. Summary
- **25 uncommented read-style accesses** (`TryGetValue`/`ContainsKey`/`Values`/`Count`/`IndexOf`/indexer) to the four shared dictionaries across 6 files — **none take a read lock**.
- All **write** paths funnel through `RegisterAll`/`UnRegisterAll` (locked on `_SyncRoot`), but readers do not coordinate with that lock.
- Net: every read in the table below can race a concurrent write. This is the §1 hang surface.

### 4b. Unlocked reader hot spots (representative)
| File:Line | Access | Dictionary |
|-----------|--------|------------|
| `PartialClasses.cs:2143` | `ParentNode` getter `TryGetValue` | `_ParentNodes` |
| `BaseTypeExtensions.cs:418` | `FindRootNode` loop (via `ParentNode`) | `_ParentNodes` |
| `ITopNodeExtensions.cs:159,162,166,173` | `_Nodes.Values` / `TryGetValue` / LINQ `Where` | `_Nodes` |
| `TreeComparer.cs:233,234` | `_Nodes.TryGetValue` x2 | `_Nodes` |
| `SdcUtil.cs:1477,1535,1960,1974` | `_ChildNodes.TryGetValue` | `_ChildNodes` |
| `IMoveRemoveExtensions.cs:128` | `_ChildNodes.TryGetValue` (RemoveRecursive) | `_ChildNodes` |
| `IMoveRemoveExtensions.cs:927,939` | `_IETnodes.IndexOf` / `.Count` | `_IETnodes` |
| `IMoveRemoveExtensions.cs:1013,1017,1019` | `_ParentNodes.ContainsKey`, `_ChildNodes[...]` | `_ParentNodes`,`_ChildNodes` |

### 4c. Lock status by member
| Member | Reads | Writes | Locked? |
|--------|-------|--------|---------|
| `ParentNode` getter | `_ParentNodes` | — | ❌ no |
| `FindRootNode` | `_ParentNodes` (via ParentNode) | — | ❌ no |
| `Nodes` / `IETnodes` getters + `.Values` LINQ | `_Nodes` / `_IETnodes` | — | ❌ no |
| `GetNodeNext`/`GetNodePrevious`/sib nav (SdcUtil) | `_ChildNodes` | — | ❌ no |
| `CompareTrees` PLINQ read (`AsParallel().ForAll` ~:347) | `_Nodes`,`_ParentNodes` (+ sib nav) | — | ⚠️ `TreeLock` (SemaphoreSlim, 18 sites :126–303) — **separate regime from writers' `_SyncRoot`, so excludes other CompareTrees calls but NOT writers** |
| `RegisterAll` | — | `_Nodes`,`_ParentNodes`,`_ChildNodes`,`_IETnodes` | ✅ `_SyncRoot` |
| `UnRegisterAll` | — | same | ✅ `_SyncRoot` |
| `ItemsMutator` | `_ParentNodes` (via FindRootNode) | via Register/UnRegister | ⚠️ partial / `new object()` fallback |
| `_MaxObjectID++` | `_MaxObjectID` | `_MaxObjectID` | ❌ no |

**Conclusion:** writers are ~80% protected (on `_SyncRoot`); readers are ~0% protected **except** CompareTrees' PLINQ read, which is on a **separate** `TreeLock` regime that does **not** exclude writers. Under the documented 100:1 read:write profile, the unprotected readers are both the most frequent operation and the crash trigger — confirming the `ReaderWriterLockSlim` strategy (one unified lock, read lock on every read, write lock on every write) is the correct unimplemented fix.

---

## 5. Remediation Design — LOCKED (Option C)

**Status: COMPLETE.** The Step 6 design is finished and the authoritative implementation spec is **`ThreadSafety_RemediationPlan_OptionC.md`** (same folder). This section is now just the bridge from evidence → design; do not duplicate the spec here.

**Chosen model — Option C (single-writer / multiple-reader):** one **`ReaderWriterLockSlim` per `TopNode`**, constructed with `LockRecursionPolicy.SupportsRecursion`. Readers take a shared read lock at every public reader boundary (§4b/§4c inventory); a rare writer takes the lock exclusively and mutates **in place** (no copies) — whole-tree, or **both** trees (ordered by `TopNode.ObjectGUID`) for a cross-tree move. This finalizes the `ReaderWriterLockSlim` strategy in `ThreadSafety_StrategyDecision.md`; the **reentrancy trap** that blocked it (read-in-read, write-in-write, and the read→write `ItemsMutator` upgrade) is resolved by `SupportsRecursion` + a "writers lock at the top, never upgrade" rule. (Copy-on-write/Option B and `ConcurrentDictionary` were considered and rejected — see the plan §0.)

**Per-RC remediation (full mechanical detail in the Option C plan §4):**
1. **RC‑1** static-state isolation: `[ThreadStatic] LastTopNode` (or `AsyncLocal<T>` only if a deserialize path crosses threads).
2. **RC‑2/RC‑4** unified read/write lock: read lock at every reader boundary (plan §4f lookup table), write lock on every mutation; replace the `ItemsMutator` `lock(new object())` fallback.
3. **RC‑3** `_MaxObjectID`: covered by the write lock once §4 lands; `Interlocked.Increment` hardening available (post-increment off-by-one noted).
4. **RC‑5 (≡ TS‑5)** migrate CompareTrees' `TreeLock` reads + writers' `_SyncRoot` onto the unified `ReaderWriterLockSlim`, verify, **then** delete `TreeLock`/`_SyncRoot` (never delete-first — it breaks the CompareTrees build).
5. **RC‑6** lock-ordered, atomic cross-tree `Move` (both trees, deterministic order).
6. **RC‑7** batch the `_ChildNodes` sort (sort once after bulk add via the existing `childNodesSort` flag) — see §5a.

### 5a. NEW finding from Step 5 repro — serialized sort under the write lock (RC‑7 candidate)
- **Evidence:** `RegisterAll` holds `lock (_topNode._SyncRoot)` for the whole registration, and `RegisterIn_ParentNodes_ChildNodes` calls `kids.Sort(treeSibComparer)` **on every insert** when `childNodesSort` is true (`IMoveRemoveExtensions.cs:905-913`). `TreeSibComparer` orders by **reflecting** the object tree.
- **Problem:** When many nodes share **one** parent, that parent's `_ChildNodes` list grows to N and is re-sorted on every single insert → **O(N² · reflection)** work, fully **serialized** under `_SyncRoot`. Concurrency degenerates to a single-threaded sort storm.
- **Impact:** Under the bounded repro (4–N threads × 250 nodes on one shared parent) this exceeds the 6 s watchdog. This is a **performance cliff**, distinct from (but stacked on top of) the RC‑2 read/write hang.
- **Remediation implication:** the read/write lock redesign must **also** address bulk-insert sort cost (e.g., defer/batch sort once after bulk add, or insert at the reflected position instead of full re-sort). Captured as **RC‑7** and specified in `ThreadSafety_RemediationPlan_OptionC.md` §4 (RC‑7) and §6 (sequenced before RC‑2 so the perf cliff does not mask the read/write hang).

### 5b. NEW finding — "reads" lazily MUTATE shared state (couples RC‑7 → RC‑2/CompareTrees)
- **Evidence:** `CompareTrees.cs:411` calls `GetNodePreviousSib()` → `SdcUtil.GetPrevSibElement` (`SdcUtil.cs:1385`), which at `:1393` calls `SortElementKids(par, sibs)` where `sibs` **is the live `_ChildNodes` list**. `SortElementKids` (`SdcUtil.cs:3290`) on first access does **`kids.Sort(new TreeSibComparer())`** (in-place mutation, `:3308`) **and `TreeSort_Add(parentItem)`** (mutates `_TreeSort_NodeIds`, `:3309`). Many sibling-navigation readers (`GetPrevSibElement`, `GetFirstChildElement`, `GetLastChildElement`, …) share this lazy-sort.
- **Problem:** These nominally-read APIs are **not pure reads** — first touch mutates per-TopNode shared state. So a plain **read** lock is **insufficient** for CompareTrees' `AsParallel().ForAll` (two PLINQ threads could lazily sort/flag the same parent concurrently; an external writer is not excluded either). The original code only masked this with a local `lock(new object())` that serializes PLINQ workers among themselves.
- **Remediation implication:** before the parallel read, run a **single-threaded pre-sort pass under the WRITE lock** (reuse the RC‑7 batched sort) so every involved parent is already flagged in `_TreeSort_NodeIds`; thereafter `SortElementKids` is a genuine **no-op** and the `ForAll` is **truly read-only** under one `ReadLockScope`. **This couples RC‑7 before the CompareTrees portion of RC‑2.** Full design in `ThreadSafety_RemediationPlan_OptionC.md` §12.5.

---

## 6. Reproduction Evidence (Step 5 — partial, paused mid-classification)

**Harness:** `SDC.Schema.Tests/OMTests/ThreadSafetyReproTests.cs`
- Bounded, watchdog-protected (`WATCHDOG_MS = 6000`, `[Timeout(10000)]`). Safe by design: all dictionary **reads** happen post-join, so it cannot recreate the §1 crash-loop.
- Two tests: RC‑3 (duplicate `ObjectID`), RC‑2/RC‑4 (same-parent child corruption). Both attach `DisplayedType` to a shared `DataElementType` TopNode (legal per `BaseTypeThreadSafetyTests`).
- Harness threading was **upgraded** from `Parallel.For` to **exactly `THREADS` dedicated `Thread` objects** so the `Barrier(THREADS)` participant count matches real concurrency (eliminating the thread-pool injection artifact).

**Runs this session (`dotnet test --filter FullyQualifiedName~ThreadSafetyReproTests --no-build`):**

| Run | Harness | NODES_PER_THREAD | Result | Interpretation |
|-----|---------|------------------|--------|----------------|
| 1 | `Parallel.For` + invalid `SectionItemType` parent | 250 | 1 failed / 1 passed | Invalid: 5000 structural attach exceptions (wrong parent type) |
| 2 | `Parallel.For` + legal `DataElementType` parent | 250 | **2 skipped** (both watchdog @6s) | Possible artifact: `Parallel.For` doesn't guarantee `THREADS` concurrent workers → Barrier stalls |
| 3 | **Dedicated threads** + legal parent | 250 | **2 skipped** (both watchdog @6s) | Artifact ruled out — real production-side stall confirmed |

**Confirmed:** the stall is **not** a test artifact (dedicated-thread harness still trips at exactly 6 s on both tests; exit code 0; watchdog protected the runner — no testhost crash).

**CLASSIFIED (branch `Features/Net11Upgrade_ThreadSafety_OptionCImpl`, §5 discriminator run):**
- **NODES_PER_THREAD = 10:** both tests **passed** (2/2 passed, 0 skipped, total wall-clock ≈ 6.2 s for the full `dotnet test` run including harness startup; actual parallel work < 3 s per test — well under the 6 s watchdog). `NODES_PER_THREAD` reverted to 250 immediately after.
- **Verdict: RC-7 perf cliff DOMINATES.** At 10 nodes the O(N²·reflection) sort is negligible → tests complete without hitting the watchdog. At 250 nodes the sort storm under `lock(_SyncRoot)` fills the entire 6 s budget. The genuine RC-2 read/write hang *may also be present*, but it is masked by the perf cliff.
- **Implication:** fix TS-7 (batch sort) FIRST, then re-run at 250 nodes to see if the underlying RC-2 hang surface surfaces. This matches the plan §6 ordering (TS-7 before TS-2).

**Files left in a clean, compiling state:** `ThreadSafetyReproTests.cs` builds (0 errors); only pre-existing warnings remain in unrelated test files.
