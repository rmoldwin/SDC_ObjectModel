# Thread Safety — Remediation Plan (Option C: ReaderWriterLockSlim SWMR)

**Branch:** `Features/Net11Upgrade_ThreadSafety`
**Status:** DESIGN LOCKED — ready for a lower-cost model to execute. **No production code changed by this document.**
**Supersedes the "Step 6 / Step 7" placeholders in** `ThreadSafety_RootCauseDiagnosis.md` **§5.**
**This is the single entry point for the implementation session.** Read this top-to-bottom; you should not need frontier-level reasoning to execute it.

> ### ⚠️ Terminology & status banner (read first)
> - **`TS-#` = "Thread-Safety item #" and is the canonical ID.** `RC` means "Release Candidate" in this project and must NOT be used for thread-safety defect IDs. All TS-1…TS-7 labels are authoritative; any legacy "RC-#" references in archived documents are historical artefacts only.
> - **Design is now fully locked.** The async/await + Blazor WASM item is **RESOLVED** (see **§12**): library is 100% synchronous so the no-`await`-in-lock rule is a free compile-time guardrail; CompareTrees needs a **pre-sort-before-read-lock** step (folded into TS-2, piggybacking TS-7); the real WASM test harness is **deferred** in favor of a cheap desktop reentrancy proxy. Only the standard **approval gate (§9)** remains before coding.
> - **TS-5 was corrected** this session: `TreeLock` is **NOT** dead — `CompareTrees.cs` uses it at 18 sites. The fix is **migrate-then-delete**, never delete-first. See TS-5 and §12.
> - Restart/onboarding entry point is `ThreadSafety_SessionSummary_AND_Kickstart.md`.

---

## 0. Why Option C, in one paragraph

The SDC object model is **read-heavy and write-rare**. Several processes read the per-`TopNode` dictionaries (`_Nodes`, `_ParentNodes`, `_ChildNodes`, `_IETnodes`) constantly; tree mutations are infrequent and it is **acceptable to lock the whole tree** (or both trees for a cross-tree move) for the brief duration of a write. That is the textbook **single-writer / multiple-reader (SWMR)** profile, so we use **one `ReaderWriterLockSlim` per `TopNode`**: many readers run concurrently with zero copying; a writer takes the lock exclusively and mutates **in place**. This is exactly the strategy already chosen in `ThreadSafety_StrategyDecision.md` ("ReaderWriterLockSlim + mutable references"). It was never implemented because of the **reentrancy trap** (§2). This plan resolves that trap explicitly and enumerates every edit.

> **Rejected alternatives (do not implement):**
> - *Copy-on-write snapshots (Option B):* zero reader-blocking, but copies a dictionary per write transaction and adds publish machinery — unneeded because brief reader-blocking during a rare write is acceptable.
> - *`ConcurrentDictionary`:* fixes single-operation tearing but **not** the multi-dictionary invariant (a reader could see `_ParentNodes` updated but `_ChildNodes` not). SWMR gives whole-transaction consistency.
> - *`SemaphoreSlim` for writers:* breaks the **write-within-write** reentrancy the code relies on (`RegisterAll` is called from inside `InitAfterTreeAdd`). Do not use it for synchronous mutation.

---

## 1. THE ONE RULE (read this twice)

> **A.** Acquire the lock at the **public boundary** of an operation, **once**.
> **B.** **Readers** take the **read** lock; **writers** take the **write** lock.
> **C.** A writer must take the **write** lock **at the very top** of the operation. **NEVER** take a read lock and later try to "upgrade" to a write lock — that is the classic `ReaderWriterLockSlim` deadlock. If an operation might write, it takes the **write** lock from the start.
> **D.** Internal helper methods (the ones called *while a lock is already held*) must **NOT** take the lock again at a different level — rely on `LockRecursionPolicy.SupportsRecursion` only for genuinely nested same-kind reentry (read-in-read, write-in-write), never read→write.

Every edit in §4 is an application of this rule. If you are ever unsure whether a method should lock, answer two questions:
1. *Is it a public/entry-level operation, or an internal helper invoked under an already-held lock?* → Only entry-level locks.
2. *Does it ever mutate a shared dictionary, now or transitively?* → If yes, **write** lock; if no, **read** lock.

A lookup table for every touched member is in **§4f**. When in doubt, use that table verbatim.

---

## 2. The reentrancy trap (why naïve locking fails here)

The OM constantly reads-within-reads and reads-within-writes:

| Pattern | Evidence | Consequence if locked naïvely |
|---|---|---|
| read-in-read | `FindRootNode` loops calling the `ParentNode` getter | `EnterReadLock` inside `EnterReadLock` → OK only with `SupportsRecursion` |
| read-then-write | `ItemsMutator` reads via `FindRootNode`, then writes via `Register/UnRegisterAll` | read→write **upgrade deadlock** unless writer locks at the top |
| write-in-write | `RegisterAll` is called from inside `InitAfterTreeAdd`; both currently `lock(_SyncRoot)` | `EnterWriteLock` inside `EnterWriteLock` → OK only with `SupportsRecursion` |

**Resolution:**
- Construct the lock with **`LockRecursionPolicy.SupportsRecursion`** (handles read-in-read and write-in-write).
- Enforce **THE ONE RULE C** (writers lock at the top) so a read→write upgrade can never occur.
- Internal `Register*/UnRegister*` helpers **stop locking individually**; the public entry (`RegisterAll`/`UnRegisterAll` and the mutators that call them) owns the write lock. (Because the helpers are only ever reached under the entry-level write lock, removing their inner locks is safe and removes upgrade risk.)

---

## 3. Infrastructure to add (do this FIRST, before any RC edit)

### 3a. Replace the lock primitive on every `TopNode`

**File:** `SDC.Schema\Partial Classes\PartialClasses.cs` — the `#region Thread Safety Infrastructure` block (currently around **lines 155–163**):

Current:
```csharp
#region Thread Safety Infrastructure
private readonly SemaphoreSlim _treeLock = new(1, 1);
public SemaphoreSlim TreeLock => _treeLock;
private readonly object _syncRoot = new();
public object _SyncRoot => _syncRoot;
#endregion
```

Target shape (names are guidance; keep `_SyncRoot` available during migration to avoid a big-bang change):
```csharp
#region Thread Safety Infrastructure
// Option C: single-writer / multiple-reader per TopNode tree.
// SupportsRecursion is REQUIRED: the OM does read-in-read (FindRootNode -> ParentNode)
// and write-in-write (InitAfterTreeAdd -> RegisterAll). NEVER perform a read->write upgrade
// (see ThreadSafety_RemediationPlan_OptionC.md §1 Rule C).
private readonly ReaderWriterLockSlim _treeRwLock = new(LockRecursionPolicy.SupportsRecursion);
public ReaderWriterLockSlim TreeRwLock => _treeRwLock;

// Kept temporarily so existing lock(_SyncRoot) sites compile during staged migration.
// DELETE after all write paths move to TreeRwLock (TS-5 cleanup).
private readonly object _syncRoot = new();
public object _SyncRoot => _syncRoot;
#endregion
```

> Update the interface `ITopNode.cs` (and `_ITopNode` if the lock is exposed there) to surface `ReaderWriterLockSlim TreeRwLock { get; }`. The dead `SemaphoreSlim TreeLock` declaration (interface `ITopNode.cs:~107`) is removed in **TS-5**.

### 3b. Add allocation-free lock scopes (one new file)

**New file:** `SDC.Schema\Utility Classes\TreeLockScope.cs`. Two `ref struct`s give `using`-based, exception-safe lock/unlock with **zero heap allocation** on the hot read path:

```csharp
using System;
using System.Threading;

namespace SDC.Schema
{
	/// <summary>Acquires a READ lock for the duration of a using-scope. Allocation-free (ref struct).</summary>
	internal readonly ref struct ReadLockScope
	{
		private readonly ReaderWriterLockSlim _lock;
		public ReadLockScope(ReaderWriterLockSlim rwLock)
		{
			_lock = rwLock;
			_lock.EnterReadLock();
		}
		public void Dispose() => _lock.ExitReadLock();
	}

	/// <summary>Acquires a WRITE lock for the duration of a using-scope. Allocation-free (ref struct).</summary>
	internal readonly ref struct WriteLockScope
	{
		private readonly ReaderWriterLockSlim _lock;
		public WriteLockScope(ReaderWriterLockSlim rwLock)
		{
			_lock = rwLock;
			_lock.EnterWriteLock();
		}
		public void Dispose() => _lock.ExitWriteLock();
	}
}
```

Usage pattern at a **reader** boundary:
```csharp
using var _ = new ReadLockScope(((_ITopNode)TopNode).TreeRwLock);
return topNode._ParentNodes.TryGetValue(this.ObjectGUID, out var p) ? p : null;
```

Usage pattern at a **writer** boundary:
```csharp
using var _ = new WriteLockScope(_topNode.TreeRwLock);
// ... all dictionary mutations for this operation ...
```

> **Null/segregation guard:** a node may legitimately have no `TopNode` yet (created with a null parent). When `TopNode` is null there are no shared dictionaries to protect, so **skip locking** (there is nothing another thread can reach). Add a tiny helper rather than repeating the null check:
> ```csharp
> internal static ReaderWriterLockSlim? RwLockOrNull(this BaseType n)
>     => n.TopNode is _ITopNode itn ? itn.TreeRwLock : null;
> ```
> Reader/writer sites then do: `var rw = node.RwLockOrNull(); if (rw is null) { /* unsynchronized local node */ } else { using var _ = new ReadLockScope(rw); ... }`. This replaces the broken `lock(new object())` fallback in `ItemsMutator` (TS-4).

### 3c. Build gate

After 3a + 3b, **build the production project only** and fix compile errors before touching any RC:
```powershell
dotnet build "SDC.Schema\SDC.Schema.csproj" -c Debug --nologo -v quiet
```

---

## 4. Root-cause edit map (mechanical)

Apply in the **sequence in §6**. Each RC lists the **anchor (verified)**, the **change**, and the **gate**. Line numbers are anchors captured during diagnosis; if they drift, search for the quoted code.

### TS-1 — Process-global `static LastTopNode` (latent corruption on concurrent deserialization / multi-tree build)
- **Anchors:** `PartialClasses.cs` — field `private static ITopNode? LastTopNode;` (~`:2348` region); read/written in the parameterless ctor **lines 1730–1746**; reset in `ResetLastTopNode()` **lines 2198–2202**.
- **Problem:** one `static` shared by all threads/trees; concurrent builds overwrite each other → cross-tree contamination.
- **Change (minimal, lowest-risk):** make the static isolated per execution flow:
  ```csharp
  [ThreadStatic] private static ITopNode? LastTopNode;
  ```
  `[ThreadStatic]` is correct for the **synchronous** construction/deserialization paths (each builds on one thread). If any deserialization path is genuinely `async` and crosses threads mid-build, use `private static readonly AsyncLocal<ITopNode?> _lastTopNode = new();` with a property wrapper instead. **Default to `[ThreadStatic]`**; only switch to `AsyncLocal` if §5's tests show a build crossing threads.
- **Do NOT** try to remove `LastTopNode` entirely (threading the TopNode through every ctor) in this pass — that is a larger refactor. Isolation is sufficient and safe.
- **Gate:** production build green; `BaseTypeThreadSafetyTests` still pass; no new failures in the full test run.

### TS-2 — Unsynchronized dictionary READS during writes (the actual hang surface)
- **Anchors:** `ParentNode` getter `PartialClasses.cs:2135–2147` (the `_ParentNodes.TryGetValue` at **:2143**); `FindRootNode` `BaseTypeExtensions.cs:418–429` (loops on `ParentNode`). Full unlocked-reader inventory: `RootCauseDiagnosis.md` §4b (25 sites across 6 files).
- **Change:** wrap **public read entry points** in `ReadLockScope` per **§4f**. Critically:
  - Lock the **`ParentNode` getter** (it is the most-called reader and the root of `FindRootNode`). Because the lock is recursive, `FindRootNode` calling `ParentNode` in a loop is fine; optionally take **one** read lock in `FindRootNode` around the whole walk to avoid repeated enter/exit.
  - Lock the other public readers in §4f (`Nodes`/`IETnodes` `.Values` enumerations, sibling navigation in `SdcUtil`, `TreeComparer` lookups).
  - **Do NOT** add locks to internal helpers that already run under a writer's lock (e.g., `RegisterIn_*`, `UnRegisterIn_*`).
- **Gate:** `ThreadSafetyReproTests.Repro_ConcurrentChildrenSameParent_*` no longer trips the watchdog **and** asserts the correct child count (see §5 — confirm this is TS-2 and not only TS-7 first).

### TS-3 — Non-atomic `_MaxObjectID++`
- **Anchors:** parameterless ctor `PartialClasses.cs:1746` (`ObjectID = ((_ITopNode)TopNode)._MaxObjectID++;`); `RegisterAll` `IMoveRemoveExtensions.cs:818` (`node.ObjectID = ((_ITopNode)node.TopNode)._MaxObjectID++;`). Declaration is an **interface auto-property**: `int _ITopNode._MaxObjectID { get; set; }` at `PartialClasses.cs:168`.
- **Two valid fixes — pick based on whether the increment happens under the write lock:**
  - **(Preferred, simplest) Covered-by-lock:** TS-3's `RegisterAll` increment at `:818` runs **inside** the `RegisterAll` write lock once TS-2/§4 is in place, so it is already serialized — **no change needed there**. The **ctor** increment at `:1746` runs during construction *before* the node is shared; with TS-1 isolation it is single-threaded per tree build, so it is also safe. **Verify** with the TS-3 repro; if green, TS-3 needs **no code change** beyond TS-1+TS-2.
  - **(Hardened, if the repro still shows duplicates) Interlocked:** convert `_MaxObjectID` to a real backing field and use `Interlocked.Increment`. Because `Interlocked.Increment` returns the **post**-increment value, preserve the current **post-increment** semantics (`++` returns the value *before* adding):
	```csharp
	node.ObjectID = ((_ITopNode)node.TopNode).NextObjectID(); // returns value, then increments
	// where, on the TopNode:
	private int _maxObjectId = 0;
	int _ITopNode._MaxObjectID { get => _maxObjectId; set => _maxObjectId = value; }
	internal int NextObjectID() => Interlocked.Increment(ref _maxObjectId) - 1; // mimic post-increment
	```
	⚠️ **Off-by-one warning:** `x++` yields the old value; `Interlocked.Increment` yields the new. The `- 1` above preserves the original numbering. Keep the seed (`_MaxObjectID = 1` for TopNodes) unchanged.
- **Gate:** `ThreadSafetyReproTests.Repro_NonAtomicMaxObjectID_*` reports `duplicates == 0`.

### TS-4 — Ineffective `ItemsMutator` lock (`lock(new object())`)
- **Anchor:** `PartialClasses.cs:2265` — `object lockObj = TopNode is _ITopNode itn ? itn._SyncRoot : new object();` then `lock (lockObj) { ... RemoveRecursive / Move ... }`.
- **Problem:** the `new object()` fallback gives zero mutual exclusion; also this whole block **mutates** (via `RemoveRecursive`/`Move`) so it must hold the **write** lock.
- **Change:** replace the `lock(lockObj)` with the write-lock scope using the §3b helper (skip when no TopNode):
  ```csharp
  var rw = this.RwLockOrNull();
  if (rw is null) { /* no shared tree yet — run body unsynchronized */ }
  else { using var _ = new WriteLockScope(rw); /* body */ }
  ```
  Keep the existing snapshot-array bug fixes (`itemsListOld.ToArray()`, `valueListNew.ToArray()`) intact.
- **Gate:** `BaseTypeThreadSafetyTests.ItemMutator_*` pass; production build green.

### TS-5 (≡ TS-5) — `TreeLock` + `_SyncRoot` are LIVE → migrate-then-delete (CORRECTED)
> ⚠️ **Correction (this session).** The earlier text claimed `TreeLock` was dead with "**zero** `Wait/Release` call sites" and said to **DELETE** it outright. **That is false.** `CompareTrees.cs` uses `TreeLock.Wait()`/`.Release()` at **18 sites (lines 126–303)** to guard a **parallel read** (`AsParallel().ForAll`, ~line 347) over the shared dictionaries. A delete-first edit would **break the CompareTrees build** and remove the only guard CompareTrees has. Writers separately use `_SyncRoot` (Monitor). The two locks are **different objects → they do NOT mutually exclude → real read/write corruption window** (see §12).
- **Anchors:** `TreeLock` declaration `PartialClasses.cs:157–158`; interface `_ITopNode.TreeLock` + `_ITopNode._SyncRoot` in `ITopNode.cs`; **live readers** `CompareTrees.cs:126–303` (18 `TreeLock.Wait/Release` sites); **live writers** `lock(_SyncRoot)` in `RegisterAll`/`UnRegisterAll`/`BaseName`/`ItemsMutator`.
- **Decision: MIGRATE, then DELETE (never delete-first).**
  1. Repoint **CompareTrees' `TreeLock.Wait()/.Release()`** onto a `ReadLockScope` over the same tree's unified `TreeRwLock` (preserve the existing **two-tree `ObjectGUID` order** for cross-tree compares; see `ChangePrevVersion`/`ChangeNewVersion`).
  2. Repoint **writers' `lock(_SyncRoot)`** onto `WriteLockScope` (done incrementally in TS-2/TS-4).
  3. **Verify** full build + full test run green (CompareTrees + mutators now share one lock → they finally exclude each other).
  4. **Only then DELETE** `_treeLock`/`TreeLock`, `_syncRoot`/`_SyncRoot`, and their interface members.
- **Sequencing:** this is the **last** code step (plan §6 step 8) — every reader and writer must already be on `TreeRwLock` before deletion.
- **Gate:** full production build + **full** test run green after removal (no dangling references; CompareTrees still compiles and passes).

### TS-6 — Cross-tree `Move` migration not atomic / not lock-ordered
- **Anchors:** `MoveInDictionaries` `IMoveRemoveExtensions.cs:34–58` (throws on `TopNode` mismatch; calls `UnRegisterAll` then `RegisterAll`); cross-tree entry is `Move` (caller). Reader `FindRootNode` participates via `ItemsMutator`.
- **Change:** for a **cross-tree** move, acquire **both** trees' write locks in a **deterministic global order** to prevent AB/BA deadlock, then perform unregister-from-source + register-into-target as one critical section:
  ```csharp
  // deterministic order by TopNode identity (stable, total order)
  var a = (_ITopNode)sourceTopNode; var b = (_ITopNode)targetTopNode;
  bool aFirst = a.TopNode!.ObjectGUID.CompareTo(b.TopNode!.ObjectGUID) <= 0;
  var first = aFirst ? a : b;  var second = aFirst ? b : a;
  using var _1 = new WriteLockScope(first.TreeRwLock);
  using var _2 = new WriteLockScope(second.TreeRwLock);
  // ... existing migration (ReflectRefreshSubtreeList already-done check, UnRegisterAll, RegisterAll) ...
  ```
  Same-tree moves (the `ReferenceEquals(currentTopNode, targetTopNode)` branch) take just that one tree's write lock.
- **Gate:** existing Move/cross-tree tests pass; no deadlock under the repro (add a small 2-tree move stress only if time permits — not required for sign-off).

### TS-7 — Serialized reflection sort on every `_ChildNodes` insert (perf cliff)
- **Anchor:** `RegisterIn_ParentNodes_ChildNodes` → local `RegisterParentNode`, `IMoveRemoveExtensions.cs:904–913`: `kids.Add(btSource); if (kids.Count > 1 && childNodesSort) kids.Sort(treeSibComparer);` — `TreeSibComparer` orders by **reflecting** the object tree → O(N²·reflection) when N children share one parent, all under the write lock.
- **Change (batch the sort):** do **not** sort on every insert. Two acceptable approaches; prefer the first:
  1. **Defer-and-sort-once:** during a bulk add, pass `childNodesSort: false` on each insert (the flag already exists and is plumbed through `RegisterAll`), then sort each affected parent's `kids` list **once** after the bulk operation completes (e.g., at the end of the enclosing build/move, or via the existing `AssignOrder`/`ReflectRefreshTree` path that already re-orders). The `catch (InvalidOperationException)` skip already present at `:908–912` shows the code tolerates deferred ordering.
  2. **Insert-at-position:** compute the reflected index once and `kids.Insert(index, btSource)` instead of `Add`+full `Sort`. Higher risk (must derive the index correctly) — only if approach 1 is infeasible.
- **Gate (this is also the §5 discriminator):** with the sort batched, `ThreadSafetyReproTests` (250 nodes/thread) should finish **well under** the 6 s watchdog. If it does, TS-7 was the dominant stall.

### §4f — Lock lookup table (authoritative — use verbatim)

| Member | File:anchor | Lock |
|---|---|---|
| `ParentNode` getter | `PartialClasses.cs:2135–2147` | **READ** |
| `FindRootNode` | `BaseTypeExtensions.cs:418–429` | **READ** (one scope around the walk) |
| `Nodes` getter `.Values`/LINQ readers | `ITopNodeExtensions.cs:159,162,166,173` | **READ** |
| `IETnodes` getter / `.IndexOf` / `.Count` reads | `IMoveRemoveExtensions.cs:927,939` | **READ** |
| Sibling navigation `GetNodeNext/Previous` etc. | `SdcUtil.cs:1477,1535,1960,1974` | **READ** |
| `TreeComparer` `_Nodes.TryGetValue` ×2 | `TreeComparer.cs:233,234` | **READ** |
| `CompareTrees` parallel read body (`Nodes`, `ParentNode`, `GetNodePreviousSib`) | `CompareTrees.cs:~347` (`AsParallel().ForAll`) | **READ — but ONLY after a pre-sort pass under WRITE lock** (the body lazily sorts via `GetNodePreviousSib`→`SortElementKids`; see §12.0 #2 / §12.5). Pre-sort parents, then wrap the `ForAll` in one `ReadLockScope`; remove the local `lock(locker)`. |
| `CompareTrees` single-tree guards (migrate from `TreeLock.Wait/Release`) | `CompareTrees.cs:126–303` (18 sites) | **READ** (TS-5 migration target) |
| `CompareTrees` cross-tree compare (`ChangePrevVersion`/`ChangeNewVersion`) | `CompareTrees.cs` | **READ** on **both** trees, locked in `ObjectGUID` order (preserve existing ordering) |
| `_ChildNodes.TryGetValue` in `RemoveRecursive` | `IMoveRemoveExtensions.cs:128` | **READ** (it only reads; the actual remove is under the writer) |
| `RegisterAll` (entry) | `IMoveRemoveExtensions.cs:803–847` | **WRITE** (replace inner `lock(_SyncRoot)` at `:827`) |
| `UnRegisterAll` (entry) | `IMoveRemoveExtensions.cs:1045–1075+` | **WRITE** (replace inner `lock(_SyncRoot)` at `:1062`) |
| `RegisterIn_*` / `UnRegisterIn_*` helpers | `IMoveRemoveExtensions.cs:848–916, 1059+` | **NONE** (run under entry write lock) |
| `InitAfterTreeAdd` | `PartialClasses.cs:1874` region | **WRITE** (entry; inner `RegisterAll` reuses via recursion) |
| `ItemsMutator` | `PartialClasses.cs:2265` | **WRITE** (TS-4) |
| `BaseName` setter | `PartialClasses.cs:2300–2324` | **WRITE** (replace inner `lock(_SyncRoot)` at `:2314`) |
| `_MaxObjectID` increment | `PartialClasses.cs:1746`, `IMoveRemoveExtensions.cs:818` | covered by ctor/RegisterAll scope (see TS-3) |

> If you find a shared-dictionary access **not** in this table, classify it with **THE ONE RULE** (§1) and add it. Reads that occur **only** inside an already-held write scope take **NONE**.

---

## 5. Step-5 classification FIRST (do this before TS-2/TS-7 code edits)

The 6 s watchdog trip in `ThreadSafetyReproTests` is confirmed **real** (deterministic dedicated-thread harness, exit code 0, runner never crashed) but **not yet classified** as TS-2 (hard hang) vs TS-7 (perf cliff). Classify before fixing, so you know which fix to validate against.

### 5a. Discriminator experiment
1. Open `SDC.Schema.Tests\OMTests\ThreadSafetyReproTests.cs`.
2. Temporarily change `private const int NODES_PER_THREAD = 250;` → `= 10;`.
3. Rebuild the **test** project and run only these tests:
   ```powershell
   dotnet build "SDC.Schema.Tests\SDC.Schema.Tests.csproj" -c Debug --nologo -v quiet
   dotnet test  "SDC.Schema.Tests\SDC.Schema.Tests.csproj" --filter "FullyQualifiedName~ThreadSafetyReproTests" --no-build -v minimal
   ```
4. **Interpret:**
   - **Finishes fast** (well under 6 s, may now *fail assertions* instead of going Inconclusive) ⇒ **TS-7 perf cliff** dominates. Record the timing, then **prioritize TS-7**.
   - **Still trips the watchdog at ~6 s** even with only 10 nodes/thread ⇒ **TS-2 genuine hang/livelock**. Capture a thread dump (§5b) to localize the spin, then **prioritize TS-2**.
5. **Revert `NODES_PER_THREAD` back to `250`** when done (canonical state). This revert is mandatory.

> Likely outcome (from diagnosis): TS-7 dominates at 250 nodes on one shared parent. But **measure, don't assume** — the dump procedure exists for the TS-2 case.

### 5b. Thread-dump playbook (DIAGNOSTIC ONLY — never ships, never runs per-mutation)

> This is a **test/diagnosis** tool you run **only when a hang is observed**. It is **not** wired into any production mutation path and adds **zero** runtime cost to the product. (Production hang-safety comes from the design: a single recursive writer cannot intra-deadlock, and cross-tree moves lock in a deterministic order — TS-6.)

If §5a still trips the watchdog (TS-2 path), capture where threads are stuck:

**Option 1 — `dotnet-stack` (preferred, no rebuild):**
```powershell
# one-time tool install
dotnet tool install --global dotnet-stack

# Start the test in the BACKGROUND so it is killable, then snapshot its stacks while it is stalled:
$env:REPRO="1"
Start-Process -FilePath "dotnet" -ArgumentList 'test','SDC.Schema.Tests\SDC.Schema.Tests.csproj','--filter','FullyQualifiedName~ThreadSafetyReproTests','--no-build' -PassThru |
  Tee-Object -Variable proc
# Find the testhost PID (child of the runner) and dump its managed stacks during the ~6s stall:
dotnet-stack report --name testhost
```
Look for multiple `ReproWorker_*` threads parked in `ReaderWriterLockSlim`/`Monitor` (→ true lock contention/deadlock = TS-2) vs. parked in `TreeSibComparer`/reflection/`List.Sort` (→ perf cliff = TS-7).

**Option 2 — `dotnet-dump` (full dump for offline analysis):**
```powershell
dotnet tool install --global dotnet-dump
# capture while stalled:
dotnet-dump collect --name testhost
# then analyze:
dotnet-dump analyze <dumpfile>
#   > clrstack -all        (managed stacks for every thread)
#   > syncblk               (which threads own/await which locks)
#   > threads               (find the spinning workers)
```

**Safety:** keep the run in a background terminal with a kill switch (Ctrl-C / `Stop-Process`). Per repo rule, any unit test > 1 s / functional test > 10 s is a failure to be aborted and root-caused — do **not** let anything infinite-loop. The harness `[Timeout(10000)]` + `WATCHDOG_MS=6000` are the backstops; the dump must be taken **within** that window.

---

## 6. Recommended execution sequence (one TS item at a time, gate between each)

> Fix **one** TS item, run its gate + a full build, commit, then proceed. Never batch multiple items into one unverified change.

0. **Infra (§3):** add `ReaderWriterLockSlim` + `TreeLockScope.cs`; build production green. **Commit.**
0.5. **Async/WASM rules (§12) — RESOLVED:** rules locked; the only code consequence is the CompareTrees **pre-sort-before-read-lock** step, folded into TS-2 (and dependent on TS-7 landing first). No separate step needed.
1. **Classify (§5):** run the discriminator; record TS-2-vs-TS-7 result + (if needed) a thread dump in `RootCauseDiagnosis.md §6`. Revert `NODES_PER_THREAD=250`. **Commit docs.**
2. **TS-1:** `[ThreadStatic] LastTopNode`. Gate: build + `BaseTypeThreadSafetyTests`. **Commit.**
3. **TS-7:** batch the `_ChildNodes` sort. Gate: repro finishes < 6 s at 250 nodes. **Commit.**
4. **TS-2:** add read-lock scopes per §4f (**including CompareTrees' parallel read**); convert `RegisterAll`/`UnRegisterAll`/`InitAfterTreeAdd`/`BaseName` write paths from `lock(_SyncRoot)` to `WriteLockScope`. Gate: `Repro_ConcurrentChildrenSameParent_*` passes assertions. **Commit.**
5. **TS-4:** fix `ItemsMutator` to `WriteLockScope` (+ null-tree guard). Gate: `ItemMutator_*` tests. **Commit.**
6. **TS-3:** verify repro shows 0 duplicates; apply `Interlocked` hardening only if needed (mind the off-by-one). Gate: `Repro_NonAtomicMaxObjectID_*`. **Commit.**
7. **TS-6:** lock-ordered cross-tree `Move`. Gate: Move tests, no deadlock. **Commit.**
8. **TS-5 (migrate-then-delete):** repoint CompareTrees' `TreeLock` reads + writers' `_SyncRoot` onto the unified `ReaderWriterLockSlim`, verify, **then** delete `TreeLock` and `_SyncRoot`. Gate: full build + **full** test run green. **Commit.**

> Ordering rationale: infra first; async/WASM rules before any lock code; TS-7 before TS-2 because batching the sort removes the perf cliff that otherwise masks whether the read locks fixed the *hang*; TS-5 last because it removes `TreeLock`/`_SyncRoot` only **after** every reader (CompareTrees) and writer has migrated onto the unified lock.

### 6.1 Commit & Branch Model (explicit)

**Branch:**
- Design/docs stay on **`Features/Net11Upgrade_ThreadSafety`** (current).
- All production-code edits go on a NEW implementation sub-branch **`Features/Net11Upgrade_ThreadSafety_OptionCImpl`** (PascalCase, no dashes, underscore run-in — per repo convention). Create it at the start of step 0.
- **Do NOT merge to `master` (or any parent) until** plan step 8 is green **AND** the user has explicitly approved. Repo rule: always prompt before merging back.

**Commit granularity — one TS item (or infra/classify step) per commit, ≈ 9–11 commits total:**
| # | Commit | Gate that must pass before committing |
|---|--------|----------------------------------------|
| 0 | infra (RWLockSlim + `TreeLockScope.cs`) | production build green |
| 0.5 | async/WASM rules (§12) doc + approval | doc only |
| 1 | classification result + (opt) dump | repro re-timed; `NODES_PER_THREAD` reverted to 250 |
| 2 | TS-1 `[ThreadStatic]` | build + `BaseTypeThreadSafetyTests` |
| 3 | TS-7 batch sort | repro < 6 s at 250 |
| 4 | TS-2 read/write locks (+ CompareTrees) | `Repro_ConcurrentChildrenSameParent_*` asserts |
| 5 | TS-4 `ItemsMutator` | `ItemMutator_*` |
| 6 | TS-3 verify/Interlocked | `Repro_NonAtomicMaxObjectID_*` 0 dups |
| 7 | TS-6 cross-tree Move | Move tests, no deadlock |
| 8 | TS-5 migrate-then-delete | full build + full test run green |

**Commit-message convention:** `TS-#: <change> — <gate result>`
- e.g. `TS-7: batch _ChildNodes sort (childNodesSort=false + sort-once) — repro 250 nodes 0.4s (was >6s watchdog)`
- e.g. `TS-2: read/write ReaderWriterLockSlim on reader/writer boundaries incl. CompareTrees — Repro_ConcurrentChildrenSameParent passes`

**Why this granularity:** each commit is independently revertable, so a regression rolls back exactly one TS item rather than the whole effort; gates keep every commit green; the cheap model never proceeds past a red gate (escalate instead).

---

## 7. Verification commands (canonical)

```powershell
# Repo root:
# C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema

# Build production only (after §3 infra and each production edit)
dotnet build "SDC.Schema\SDC.Schema.csproj" -c Debug --nologo -v quiet

# Build the ACTIVE test project (net11.0). IGNORE the orphan "Core Tests.csproj" (net6.0) — it is NOT in the solution.
dotnet build "SDC.Schema.Tests\SDC.Schema.Tests.csproj" -c Debug --nologo -v quiet

# Run ONLY the bounded repro tests (watchdog + [Timeout] protect the runner)
dotnet test "SDC.Schema.Tests\SDC.Schema.Tests.csproj" --filter "FullyQualifiedName~ThreadSafetyReproTests" --no-build -v minimal

# Run the pre-existing concurrency tests (mutate pre-built nodes)
dotnet test "SDC.Schema.Tests\SDC.Schema.Tests.csproj" --filter "FullyQualifiedName~BaseTypeThreadSafetyTests" --no-build -v minimal

# Full test run (final gate before sign-off)
dotnet test "SDC.Schema.Tests\SDC.Schema.Tests.csproj" --no-build -v minimal
```

**Test rules (from copilot-instructions):**
- Do **not** weaken or modify the repro assertions to make them pass; they are the regression gate and are *expected to fail/stall pre-fix*.
- Abort + root-cause any unit test > 1 s / functional test > 10 s; never allow an infinite loop.
- Keep failures visible; fix one root cause at a time.

---

## 8. Effort / risk estimate (for the approval gate)

| Item | Effort | Risk | Notes |
|---|---|---|---|
| §3 Infra (RWLockSlim + scopes) | S | Low | Mechanical; recursive policy is the key setting |
| TS-1 `[ThreadStatic]` | S | Low | One attribute; `AsyncLocal` only if §5 shows thread-crossing build |
| TS-2 read locks (§4f) | M | **Med** | Cross-cutting (~25 sites); main risk is mis-bucketing a helper as an entry point — use §4f verbatim |
| TS-3 Interlocked | S | Low–Med | Likely no-op after TS-1/TS-2; if applied, **off-by-one** is the trap |
| TS-4 ItemsMutator | S | Low | Replace `new object()` fallback with write scope + null guard |
| TS-5 migrate-then-delete TreeLock/_SyncRoot | M | **Med** | NOT a pure deletion — must first migrate CompareTrees' 18 `TreeLock` sites + writers' `_SyncRoot` onto the unified lock, verify, then delete. Delete-first breaks the CompareTrees build (§12). |
| TS-6 cross-tree Move | M | Med | Deterministic two-lock ordering; add small stress test if time permits |
| TS-7 batch sort | M | Med | Behavior must stay identical (final order); reuse `childNodesSort=false` + sort-once |

**Legend:** S ≈ <½ day, M ≈ ½–1 day, for a competent mid-tier model with these anchors.

**Net:** medium overall. The only genuinely cross-cutting step is **TS-2** (read locks at all reader boundaries); everything else is local. TS-2's risk is fully mitigated by the **§4f lookup table** + **THE ONE RULE**.

---

## 9. APPROVAL GATE

**Stop here for user approval before any production-code edit.** This document is design + spec only. **All design items are now resolved** (§12 async/WASM is LOCKED: rules firm, CompareTrees pre-sort decided, WASM harness deferred). When approved, execute **§6** in order, gating with **§7** after each TS item, and keep this file as the running checklist.

---

## 10. Cheap-model operating instructions (read before you start coding)

1. **Do not redesign.** The mechanism is fixed (Option C / `ReaderWriterLockSlim`, recursive, writer-at-top). If something seems to require a different mechanism, **stop and escalate** — do not improvise a lock strategy.
2. **One root cause per commit.** Run the RC's gate (§7) before moving on. If a gate fails, fix or escalate — never weaken a test.
3. **Use §4f verbatim** for whether a member takes READ / WRITE / NONE. When a member is missing, apply **THE ONE RULE** (§1).
4. **Never** add a read lock then a write lock in the same operation (no upgrade). Writers lock at the top.
5. **Respect the time limits.** Any unit test > 1 s or functional test > 10 s = abort + root-cause; never loop forever. Use the §5b dump playbook to localize a hang.
6. **Keep `NODES_PER_THREAD = 250`** in the repro except during the §5 discriminator, which you must revert.
7. If stuck after two attempts on the same gate, **escalate** (record the failing build/test output and hand back) rather than guessing.

---

## 11. File anchor index (verified this session)

| Anchor | File:line | Used by |
|---|---|---|
| `LastTopNode` field | `PartialClasses.cs` ~`:2348` | TS-1 |
| parameterless ctor (reads/writes `LastTopNode`, `_MaxObjectID++`) | `PartialClasses.cs:1722–1753` | TS-1, TS-3 |
| `InitBaseType` (parameterized path; derives TopNode from parent) | `PartialClasses.cs:1795+` | context (why stress tests hit TS-2/3/7, not TS-1) |
| `ResetLastTopNode` | `PartialClasses.cs:2198–2202` | TS-1 |
| `ParentNode` getter (`_ParentNodes.TryGetValue`) | `PartialClasses.cs:2143` | TS-2 |
| `_SyncRoot` / `TreeLock` infra block | `PartialClasses.cs:155–163` | §3, TS-5 |
| `TreeLock.Wait()/.Release()` (18 live sites) | `CompareTrees.cs:126–303` | TS-5 migrate (reader) |
| `AsParallel().ForAll(...)` parallel read | `CompareTrees.cs:~347` | TS-2 read lock / §12 |
| `_MaxObjectID` interface auto-property | `PartialClasses.cs:168` | TS-3 |
| `ItemsMutator` (`lock(new object())` fallback) | `PartialClasses.cs:2265` | TS-4 |
| `BaseName` setter (`lock(_SyncRoot)`) | `PartialClasses.cs:2314` | TS-2 (writer) |
| `FindRootNode` | `BaseTypeExtensions.cs:418–429` | TS-2 |
| `RegisterAll` (`lock(_SyncRoot)` at :827; `_MaxObjectID++` at :818) | `IMoveRemoveExtensions.cs:803–847` | TS-2, TS-3 |
| `RegisterIn_ParentNodes_ChildNodes` / `RegisterParentNode` (`kids.Sort`) | `IMoveRemoveExtensions.cs:867–916` (sort at :905–913) | TS-7 |
| `UnRegisterAll` (`lock(_SyncRoot)` at :1062) | `IMoveRemoveExtensions.cs:1045–1075+` | TS-2 (writer) |
| `MoveInDictionaries` (cross-tree) | `IMoveRemoveExtensions.cs:34–58` | TS-6 |
| Repro harness (watchdog, dedicated threads) | `ThreadSafetyReproTests.cs` | §5 |

---

## 12. Async/await & Blazor WASM constraints — **RESOLVED (rules locked; WASM harness deferred)**

> This was the one remaining design item. It is now **resolved**: the async rules are firm, the CompareTrees acquisition is settled (it needs a **pre-sort pass**, not a naive read lock — see 12.5), and the WASM *test harness* is **explicitly deferred** with a cheaper desktop proxy chosen instead. Two investigations this session closed all open checkboxes.

### 12.0 Two findings that closed this section (this session)
1. **The production library is 100% synchronous.** A workspace scan found **zero `async`/`await`/`Task`/`ValueTask`** in `SDC.Schema` (the only `async`/`WaitAsync` token is inside an XML doc-comment in `ITopNode.cs:104`). **Consequence:** the "no lock across `await`" rule is currently **vacuously satisfied — there is no `await` in the library to violate it.** It therefore becomes a **preventive guardrail** (enforced for free by the `ref struct` scopes, 12.4), not a bug to fix today.
2. **The CompareTrees parallel-read body is NOT pure reads — it lazily MUTATES shared state.** `CompareTrees.cs:411` `GetNodePreviousSib()` → `SdcUtil.GetPrevSibElement` (`:1385`) → `SortElementKids(par, sibs)` (`:1393`), where `sibs` **is the live `_ChildNodes` list**. On first access `SortElementKids` (`:3290`) does **`kids.Sort(new TreeSibComparer())`** (in-place list mutation, `:3308`) **and `TreeSort_Add(parentItem)`** (mutates the `_TreeSort_NodeIds` HashSet, `:3309`). The existing `lock(locker)` (a local `new object()`) only serializes PLINQ workers among themselves — it does **not** exclude external writers, and a plain **read** lock would still permit two PLINQ threads to lazily sort/flag the same parent concurrently. **A naive `ReadLockScope` around the `ForAll` is therefore WRONG.**

### 12.1 Why this section exists (corruption window)
- `CompareTrees.cs` runs a **parallel read** (`AsParallel().ForAll`, ~line 347) over the shared dictionaries, guarded today by **`TreeLock` (SemaphoreSlim)** at 18 sites. Writers (`RegisterAll`/`UnRegisterAll`) guard with **`_SyncRoot` (Monitor)**. **Two different lock objects ⇒ they do NOT mutually exclude ⇒ a writer on another thread can mutate a `Dictionary` while the `ForAll` reads it** ⇒ torn buckets / `InvalidOperationException` / 100% CPU hang. Unifying both onto the per-`TopNode` `ReaderWriterLockSlim` is what finally makes them exclude each other.
- The user's **original** code on `master` used simple `lock()` around all List/Dictionary access. It "worked fine and was fast enough" but **may have caused unexplained Blazor WASM glitches**. Hardening async-safety is an **explicit goal**, satisfied here by guardrails + the pre-sort fix below.

### 12.2 Hard rules (firm)
1. **NEVER hold a lock across an `await`.** `ReaderWriterLockSlim` is **thread-affine**: a continuation can resume on a different thread, so `ExitReadLock`/`ExitWriteLock` would then throw `SynchronizationLockException` or **leak the lock**. Critical sections must be **fully synchronous**; gather inputs, lock, mutate/read, release, **then** `await`. (Today: vacuously true — no `await` in the library. Keep it that way, or revisit this section if an `async` mutator is ever added.)
2. **No read→write upgrade** (THE ONE RULE, §1). With `SupportsRecursion`, an interleaved mutation that calls `EnterWriteLock` while a read lock is still held **on the same thread** throws `LockRecursionException`. Same cure as rule 1.
3. **CompareTrees acquisition is NOT a plain read lock — see 12.5.** Because the `ForAll` body lazily sorts (12.0 #2), it must first be made a genuine read via a **pre-sort pass under the write lock**, after which a `ReadLockScope` over the whole `ForAll` is correct.

### 12.3 WASM-specific reasoning
- **WASM is effectively single-threaded** → `AsParallel().ForAll` degrades to sequential; there is **no true parallel torn-bucket hang on WASM today**. (Likely why the desktop repro hangs but WASM "works".)
- **The WASM risk is cooperative re-entrancy, not parallelism:** an `await` yields to the WASM message loop, which can run a **mutation continuation (UI event / HTTP callback) on the same thread mid-operation**. A resumed `_Nodes.Values` / `ForAll` enumeration then throws **"collection was modified"** — same corruption *class*, different trigger. **Rule 1 is the cure for both desktop and WASM.** (Since the library has no `await` today, this is preventive.)

### 12.4 Guardrail (firm)
- `ReadLockScope`/`WriteLockScope` (§3) are **`ref struct` + `using`**, so the C# compiler **refuses to compile** an `await` inside their `using` block — turning rule 1 from a convention into a **compile-time guarantee**. **Keep them `ref struct`.**

### 12.5 RESOLVED design decisions (was the OPEN checklist)
- [x] **Is the `ForAll` body pure reads? NO** (12.0 #2). **Resolution — pre-sort, then read-lock:** immediately before the `AsParallel().ForAll`, run a **single-threaded pre-sort pass** over the parents involved (call `SortElementKids`/force the lazy sort) **under the tree's `WriteLockScope`**, so every parent is flagged in `_TreeSort_NodeIds`. After that pass, `TreeSort_IsSorted` is true everywhere, `SortElementKids` becomes a **genuine no-op**, and the `ForAll` body is now **truly read-only** → wrap it in **one `ReadLockScope`** over `TreeRwLock` (two-tree `ObjectGUID` order for cross-tree compares). The local `lock(locker)` can then be **removed** (the read lock covers it). **This directly reuses TS-7's batched-sort work** — order TS-7 before the CompareTrees migration in TS-2.
- [x] **WASM stance:** rely on single-thread + the no-lock-across-`await` guardrail. **No experimental WASM threads** (out of scope unless the user asks). Zero new infra.
- [x] **Async critical-section boundary for async mutators:** none required — the library has no async mutators (12.0 #1). If one is ever added, it must gather inputs, take `WriteLockScope`, mutate, release, then `await`.
- [x] **Repro strategy — DESKTOP PROXY chosen; real WASM harness DEFERRED.** See 12.6.

### 12.6 WASM test harness — DEFERRED (decision + rationale)
**Decision:** do **not** build an in-browser/`browser-wasm` test run now. Cover the one WASM-specific hazard with a **synchronous desktop proxy test** instead, authored during TS-2.
**Why defer the full WASM harness (large, low value):**
- No Blazor/WASM project exists in the solution (all three projects are `net11.0`, MSTest); standing one up needs the `wasm-tools` workload, a `browser-wasm` test project, and porting off the MSTest desktop host.
- The browser sandbox has **no filesystem**, but many existing tests **load SDC XML from disk** → each would need a virtual FS / embedded resources to run at all.
- **WASM is single-threaded**, so the parallel stress tests degrade to sequential and would exercise *less* concurrency than the existing desktop multithreaded `ThreadSafetyReproTests`. **Desktop multithreading is a strict superset of WASM's parallel hazards.**
**What the desktop proxy must cover (cheap, bounded, in the existing net11 MSTest project):** a single-thread **cooperative-reentrancy** test — begin enumerating `Nodes`/run a compare, and from a reentrant callback on the **same thread** mutate the tree; assert the unified lock + no-lock-across-`await` discipline prevents a "collection was modified" throw. Author it during **TS-2** (it needs the unified lock to exist).

> **Net:** §12 is now LOCKED. The only code consequence beyond the original plan is the **pre-sort-before-read-lock** step for CompareTrees (12.5), which piggybacks on TS-7 and is folded into TS-2's CompareTrees migration. No production code changed yet; approval gate (§9) still stands.
