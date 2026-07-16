# Thread Safety — Session Handoff / Resume Context

**Purpose:** Resume the concurrency work in a fresh session without re-deriving prior work.
**Branch:** `Features/Net11Upgrade_ThreadSafety`
**Last updated:** Plan-correction + async/WASM resolution session — TS-# terminology, commit/branch model, **TS-5 corrected (TreeLock LIVE)**, and **§12 async/WASM RESOLVED** (design now fully locked; only the approval gate remains).

> ### ➡️ START AT THE NEW RESTART DOC
> The single restart entry point is now **`ThreadSafety_SessionSummary_AND_Kickstart.md`** (status + answered questions + copy-paste kickstart prompt). Read that first; this handoff is now a supporting reference.

---

## TL;DR — where we are

Diagnosis is complete and the **remediation design is Option C (`ReaderWriterLockSlim` SWMR, no copies)**, now **fully locked** (§12 async/WASM resolved).
The full mechanical implementation spec — TS‑1…TS‑7 edit map, lock-discipline rule, classification + thread-dump playbook, per-TS verification gates, **commit/branch model (§6.1)**, sequencing, effort/risk, and the **async/WASM section (§12, RESOLVED)** — lives in **`ThreadSafety_RemediationPlan_OptionC.md`**. That document is designed so a **lower-cost model can execute the code edits** without re-designing.

**Plan status:** **design fully locked; ready to code pending approval.** Items settled this session: (1) **TS-5 corrected** — `TreeLock` is **LIVE** in `CompareTrees.cs` (18 sites), so **migrate-then-delete**, not delete-first; (2) **§12 async/WASM RESOLVED** — the library is 100% synchronous (no-`await`-in-lock is a free compile-time guardrail), CompareTrees' `ForAll` lazily sorts so it needs a **pre-sort-under-write-lock then read-lock** (folded into TS-2 after TS-7), and the **real WASM test harness is deferred** in favor of a desktop reentrancy proxy. **Approval gate is the only remaining step before coding.**

> **No production code has been changed.** All work so far is diagnosis + design docs + a bounded test harness. The Option C plan stops at the approval gate.

---

## Read these first (in order)

0. **`ThreadSafety_SessionSummary_AND_Kickstart.md` — RESTART ENTRY POINT.** Status, the three answered questions (RC→TS rename, commit model, CompareTrees/WASM), the resolved async/WASM item, and the kickstart prompt.
1. **`ThreadSafety_RemediationPlan_OptionC.md` — the implementation spec.** THE ONE RULE, TS‑1…TS‑7 edit map, §4f lock table (incl. CompareTrees), §5 classification + dump playbook, §6 sequencing, §6.1 commit/branch model, §12 async/WASM (RESOLVED).
2. `ThreadSafety_RootCauseDiagnosis.md` — consolidated root-cause analysis (TS/RC‑1…7) with file:line evidence and the reader/writer map (TS-5 corrected). **Primary technical evidence record.**
3. `ThreadSafety_StrategyDecision.md` — originally-chosen strategy (**`ReaderWriterLockSlim` + mutable references**), finalized as **Option C**.
4. This file — supporting status and commands.

Active supporting analysis: `ThreadSafety_LockingStrategy_Analysis.md` (locking deep-dive — helpful overview for future work).
Supporting (older, **archived** — see `Archived Plans\README.md`) analyses: `Archived Plans\ThreadSafety_ArchitecturalAnalysis.md`, `Archived Plans\ThreadSafety_AuditChecklist.md`, `Archived Plans\ThreadSafety_Phase1_*` , `Archived Plans\ThreadSafetyAnalysis.md`.

---

## Root causes (short form — full evidence in RootCauseDiagnosis.md)

| ID | Root cause | Key location |
|----|-----------|--------------|
| RC‑1 | Process-global `static ITopNode? LastTopNode` (no thread isolation) → cross-tree contamination during concurrent deserialization / multi-tree build | `PartialClasses.cs` (`LastTopNode`, parameterless ctor, `ResetLastTopNode`) |
| RC‑2 | Unsynchronized dictionary **reads** during concurrent writes (the actual hang surface) | `ParentNode` getter `PartialClasses.cs`; `FindRootNode` `BaseTypeExtensions.cs` |
| RC‑3 | Non-atomic `_MaxObjectID++` (no `Interlocked`) → duplicate `ObjectID`s | `PartialClasses.cs` InitBaseType; `IMoveRemoveExtensions.cs` `RegisterAll` |
| RC‑4 | Inconsistent write locking; `ItemsMutator` `lock(new object())` fallback = no mutual exclusion | `PartialClasses.cs` `ItemsMutator` |
| RC‑5 (TS‑5) | `TreeLock` (SemaphoreSlim) is **LIVE** in `CompareTrees.cs` (18 sites guarding a PLINQ read), on a **separate regime** from writers' `_SyncRoot` → the two don't mutually exclude → migrate-then-delete | `CompareTrees.cs:126–303`, `PartialClasses.cs`, `ITopNode.cs` |
| RC‑6 | Cross-tree `Move` dictionary migration not atomic / not lock-ordered | `IMoveRemoveExtensions.cs` `MoveInDictionaries` |
| RC‑7 (candidate) | Serialized **reflection-based** `kids.Sort(treeSibComparer)` on **every** `_ChildNodes` insert under `_SyncRoot` → O(N²·reflection) perf cliff for many children of one parent | `IMoveRemoveExtensions.cs` `RegisterIn_ParentNodes_ChildNodes` |

**Strategy vs reality:** docs chose `ReaderWriterLockSlim`; code actually uses partial `Monitor`/`_SyncRoot` on ~80% of **writers** and **0%** of **readers**. Readers are the crash trigger.

---

## 7-step diagnostic plan — status

| Step | Description | Status |
|------|-------------|--------|
| 1 | Write root-cause diagnosis doc | ✅ done |
| 2 | Inventory unlocked dictionary readers / writers (reader-writer map) | ✅ done |
| 3 | Verify `LastTopNode` lifecycle + contamination window | ✅ done |
| 4 | Create bounded, watchdog-protected repro harness | ✅ done (compiles, runs safely) |
| 5 | Run repro & capture evidence (RC‑2-vs-RC‑7 classification) | ⏸ **PARTIAL** — procedure now fully specified in Option C plan §5; run it FIRST in the coding session |
| 6 | Draft remediation design | ✅ **done** — Option C in `ThreadSafety_RemediationPlan_OptionC.md`; **§12 async/WASM now RESOLVED** (pre-sort-then-read-lock for CompareTrees; WASM harness deferred) |
| 7 | Effort/risk estimate; pause for approval before code changes | ✅ **done** — see Option C plan §8; **the §9 approval gate is the only remaining human action** |

---

## Step 5 — exactly where we paused

**Done:**
- Built the **correct active** test project `SDC.Schema.Tests\SDC.Schema.Tests.csproj` (net11.0). (The orphan `Core Tests.csproj` (net6.0) is **not** in the solution — ignore it; it caused a false `NU1201` earlier.)
- Fixed an invalid repro (was attaching `DisplayedType` to `SectionItemType`); now uses legal `DataElementType` (`de`) parent.
- Upgraded the harness from `Parallel.For` to **exactly `THREADS` dedicated `Thread`s** so `Barrier(THREADS)` is deterministic (removed the thread-pool injection artifact).
- Ran 3 times — see results table in `ThreadSafety_RootCauseDiagnosis.md` §6. **Both tests trip the 6 s watchdog (report "skipped" = `Assert.Inconclusive`), even with the deterministic harness. Exit code 0; runner never crashed.** ⇒ the stall is **real**, not a harness artifact.

**The one open *runtime* question (a §5 classification step run during implementation — NOT a design open item; the design is fully locked):**
Is the 6 s stall a **hard deadlock/livelock (RC‑2)** or the **serialized reflection-sort perf cliff (RC‑7)**?

**Discriminator experiment (next concrete action):**
1. In `ThreadSafetyReproTests.cs`, temporarily set `NODES_PER_THREAD = 10` (currently 250).
2. Rebuild + run only these tests (command below).
3. Interpret:
   - Finishes fast (well under 6 s) ⇒ **RC‑7 perf cliff** dominates → record timings, then revert to 250.
   - Still trips watchdog ⇒ **RC‑2 genuine hang** → capture a dump/stack of the worker threads to localize the spin.
4. Revert `NODES_PER_THREAD` back to **250** when done.

> NOTE: the `NODES_PER_THREAD = 10` change was applied during the session and then **reverted to 250** before pausing, so the file is currently in its canonical state.

---

## Exact commands (PowerShell, from repo root)

Repo root: `C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema`

```powershell
# Build ONLY the active test project (net11.0)
dotnet build "SDC.Schema.Tests\SDC.Schema.Tests.csproj" -c Debug --nologo -v quiet

# Run ONLY the bounded repro tests, no rebuild (safe: watchdog + [Timeout] protect the runner)
dotnet test "SDC.Schema.Tests\SDC.Schema.Tests.csproj" --filter "FullyQualifiedName~ThreadSafetyReproTests" --no-build -v minimal
```

**Safety note:** always run the repro **in a background terminal with a kill-switch** (the harness is bounded, but keep the option to Ctrl-C). Per repo rule, any unit test > 1 s / functional test > 10 s is a failure to be aborted and root-caused — do **not** let anything infinite-loop.

---

## Key files

| File | Role |
|------|------|
| `SDC.Schema.Tests\Documentation\ThreadSafety_RemediationPlan_OptionC.md` | **Implementation spec — START HERE to code** (TS‑1…TS‑7 edit map, lock rule, §4f lock table, gates, §6.1 commit/branch model, sequencing, §12 async/WASM RESOLVED) |
| `SDC.Schema.Tests\Documentation\ThreadSafety_RootCauseDiagnosis.md` | **Primary** technical evidence record (RC‑1…RC‑7, maps, repro results) |
| `SDC.Schema.Tests\Documentation\ThreadSafety_SessionHandoff.md` | **This** resume file |
| `SDC.Schema\Utility Classes\TreeLockScope.cs` | **NEW (to be created in §3 of the plan)** — allocation-free `ReadLockScope`/`WriteLockScope` ref structs |
| `SDC.Schema.Tests\OMTests\ThreadSafetyReproTests.cs` | Bounded watchdog repro harness (dedicated-thread; compiles & runs) |
| `SDC.Schema.Tests\OMTests\BaseTypeThreadSafetyTests.cs` | Pre-existing concurrency tests (mutate pre-built nodes; creation is serial) |
| `SDC.Schema\Partial Classes\PartialClasses.cs` | RC‑1 `LastTopNode`, RC‑3 `_MaxObjectID++`, `ParentNode` getter, `_SyncRoot` (writer regime), `TreeLock` decl (LIVE via CompareTrees), `ItemsMutator` |
| `SDC.Schema\Utility Classes\Extensions\IMoveRemoveExtensions.cs` | `RegisterAll`/`UnRegisterAll` (locked writers), `RegisterIn_ParentNodes_ChildNodes` (RC‑7 sort), `Move`/`MoveInDictionaries` (RC‑6) |
| `SDC.Schema\Utility Classes\Extensions\BaseTypeExtensions.cs` | `FindRootNode` (unlocked reader hot path, RC‑2) |

---

## Remediation direction — Option C (design fully locked)

The design is **locked except for the async/WASM rules (plan §12)**. Full mechanical detail (the authority) is in **`ThreadSafety_RemediationPlan_OptionC.md`**. Summary of the chosen approach:

- **Model:** single-writer / multiple-reader (SWMR) via **one `ReaderWriterLockSlim` per `TopNode`**, constructed with `LockRecursionPolicy.SupportsRecursion`. No copies; readers run concurrently; a rare writer takes the lock exclusively and mutates in place (whole-tree, or both trees for a cross-tree move). This finalizes the documented `ReaderWriterLockSlim` decision and resolves the **reentrancy trap** that previously blocked it (writers lock at the top; **never** a read→write upgrade).
- **RC‑1** static-state isolation: `[ThreadStatic] LastTopNode` (fall back to `AsyncLocal<T>` only if a deserialize path crosses threads).
- **RC‑2/RC‑4** read/write locks: read lock at every public reader boundary (§4f lookup table in the plan), write lock on every mutation path; replace the `lock(new object())` fallback in `ItemsMutator`.
- **RC‑3** atomic counter: likely a no-op once covered by the write lock; `Interlocked.Increment` hardening available (mind the post-increment off-by-one).
- **RC‑5 (TS‑5)** `TreeLock` is **LIVE** in `CompareTrees.cs` (18 sites guarding a PLINQ read) on a **separate regime** from writers' `_SyncRoot`: **migrate** both onto the unified `ReaderWriterLockSlim`, verify, **then delete** (never delete-first).
- **RC‑6** lock-ordered cross-tree `Move` (both trees, ordered by `TopNode.ObjectGUID`).
- **RC‑7** batch the `_ChildNodes` sort (sort once after bulk add via the existing `childNodesSort` flag) instead of per-insert reflection sort.

**Next human action = the approval gate (Option C plan §9).** Then execute the plan's §6 sequence, one root cause per commit, gating with §7 after each.

---

## Conventions reminder (from copilot-instructions)

- Branch naming: PascalCase, no dashes; underscores sparingly. Current branch `Features/Net11Upgrade_ThreadSafety` is correct.
- Test comments must explain the **rationale** of assertions (the repro asserts the thread-safe expectation, so it is **expected to fail/stall pre-fix** — do **not** weaken assertions to make it pass).
- Never modify tests to artificially pass; keep failures visible.
- Abort + root-cause any unit test > 1 s / functional test > 10 s; never allow infinite loops.
