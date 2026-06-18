# Thread Safety — Session Summary & Kickstart (Restart Entry Point)

**Branch:** `Features/Net11Upgrade_ThreadSafety`
**Created:** end of the design-review session, immediately before a multi-hour user break.
**Why this file exists:** disconnect insurance. If the chat session is lost, **start here.** It restates everything decided (including the now-resolved async/WASM design item), the defect found mid-session, and a copy-paste kickstart prompt.

> **Design is fully locked; we are at the §9 approval gate.** The doc/design sessions changed **no** production code — but ⚠️ **the git checkpoint is NOT a clean baseline.** The current pushed commit `bbcadca` already contains **partial production changes vs. baseline `a98660a`** (`ITopNode.cs`, `PartialClasses.cs`, `CompareTrees.cs`, `IMoveRemoveExtensions.cs` — the **`SemaphoreSlim TreeLock` infra**, CompareTrees GUID-ordered locking, and a `prevXml`/`newXml` deserialization bug fix). This is a **WIP SAFETY checkpoint**, not the Option C implementation. The next model's **first decision** (before any edits) is **migrate-on-top vs. revert-to-baseline** — see §5 step 3.

---

## 1. One-screen status

- **Goal:** make the SDC object model (per-`TopNode` dictionaries `_Nodes`/`_ParentNodes`/`_ChildNodes`/`_IETnodes`) thread/async-safe under a **read-heavy, write-rare** profile, then hand the *mechanical implementation* to a **lower-cost model** to save cost.
- **Strategy LOCKED = Option C:** one **`ReaderWriterLockSlim` per `TopNode`**, `LockRecursionPolicy.SupportsRecursion`, **single-writer / multiple-reader (SWMR)**. Readers take a read lock; rare writers take the whole-tree write lock and mutate in place (no copies). Cross-tree ops lock **both** trees in `ObjectGUID` order. THE ONE RULE: lock once at the public boundary; writers lock at the top; **never** a read→write upgrade.
- **Primary design doc:** `ThreadSafety_RemediationPlan_OptionC.md` (the mechanical TS-1…TS-7 edit map + gates + sequencing). **Read it second, after this file.**
- **Evidence doc:** `ThreadSafety_RootCauseDiagnosis.md` (RC/TS-1…7 with file:line proof, reader/writer map, repro results).
- **Status:** design is **fully locked and ready to code** pending the user's approval. The async/WASM item (§3) is **RESOLVED**; the only remaining gate is the **§9 approval gate**.
- **Git checkpoint:** committed **and pushed** as `bbcadca` on `Features/Net11Upgrade_ThreadSafety` (local = `origin`, in sync, tree clean). **Baseline = `a98660a`** ("Merge OM Tree Stability Suite + Move/Reparent Bug Fixes"). ⚠️ `bbcadca` already carries the partial `SemaphoreSlim TreeLock` production work (see the top banner) — it is a **WIP safety checkpoint**, not the finished Option C.

---

## 2. Answers to the three questions asked this session (recorded so they survive a restart)

### 2.1 What does "RC" mean? → **"Root Cause," not "Release Candidate."** Renaming to **TS-#**.
The seven findings were numbered `RC-1…RC-7` ("Root Cause") in the diagnosis doc — this collides with the universal "Release Candidate" meaning. **Decision: adopt `TS-#` (Thread-Safety) as the canonical IDs**, with `RC-#` kept as a legacy alias during the transition. Mapping is 1:1: `TS-1 ≡ RC-1`, …, `TS-7 ≡ RC-7`.

| ID | Root cause (short) |
|----|--------------------|
| TS-1 | Process-global `static LastTopNode` → cross-tree contamination on concurrent build/deserialize |
| TS-2 | Unsynchronized dictionary **reads** during writes (the hang surface) |
| TS-3 | Non-atomic `_MaxObjectID++` → duplicate ObjectIDs |
| TS-4 | Ineffective `ItemsMutator` `lock(new object())` (zero mutual exclusion) |
| TS-5 | `TreeLock`/`_SyncRoot` lock-regime split — **CORRECTED this session, see §3** |
| TS-6 | Cross-tree `Move` migration not atomic / not lock-ordered |
| TS-7 | Serialized reflection `kids.Sort()` on every `_ChildNodes` insert (perf cliff) |

### 2.2 Commit frequency & branching → **one TS-item per commit; implementation sub-branch; no merge without approval.**
- **Commit model (now explicit in the plan §6.1):** ~**9 commits** — infra → classify → one per TS fix. Each commit must pass its build+test **gate** before the next starts. Each commit is independently revertable, so a bad fix rolls back one item, not the whole effort. Commit-message convention: `TS-#: <change> — <gate result>` (e.g., `TS-7: batch _ChildNodes sort — repro 250 nodes 0.4s (was >6s watchdog)`).
- **Branching:** stay on `Features/Net11Upgrade_ThreadSafety` for design; create implementation sub-branch **`Features/Net11Upgrade_ThreadSafety_OptionCImpl`** for code edits (PascalCase, no dashes, underscore run-in — per repo convention). **Do not merge to `master` until the full test run is green AND the user approves** (repo rule: always prompt before merging back).

### 2.3 CompareTrees parallel reads, the corruption window, and Blazor WASM → **real window; strongest argument for Option C; async/WASM now RESOLVED (§3).**
Verified in code this session:
- `CompareTrees.cs` runs a **parallel read**: `_slAttNew.AsParallel().ForAll(...)` (≈ line 347) whose body reads `_prevVersion.Nodes`, `_newVersion.Nodes[…]`, `ParentNode`, `GetNodePreviousSib()` — the shared dictionaries.
- It guards that with **`TreeLock` (SemaphoreSlim)** at **18 sites** (lines 126–303), locking one tree, or **both trees in `ObjectGUID` order** for cross-tree compares; a local `lock(locker)` (a `new object()`) serializes only the non-thread-safe helper calls **among PLINQ workers**.
- **The corruption window is REAL:** writers (`RegisterAll`/`UnRegisterAll`) lock **`_SyncRoot` (Monitor)** while CompareTrees locks **`TreeLock` (SemaphoreSlim)** — **two different objects, so they do not mutually exclude.** A mutation on another thread during the `ForAll` ⇒ concurrent `Dictionary` read+write ⇒ torn buckets / `InvalidOperationException` / 100% CPU hang. CompareTrees is protected from other CompareTrees calls, but **not** from writers.
- **This is the strongest argument FOR Option C:** unifying both regimes onto one `ReaderWriterLockSlim` (CompareTrees → **read** lock; writers → **write** lock) makes them finally exclude each other.

**Async/await + Blazor WASM — RESOLVED this session (plan §12 now LOCKED):**
- **The library is 100% synchronous** — a workspace scan found **zero `async`/`await`/`Task`/`ValueTask`** in `SDC.Schema` (only an XML-comment mention in `ITopNode.cs:104`). So "no lock across `await`" is **vacuously true today** and becomes a **free compile-time guardrail**: the `ref struct` `using` scopes make the compiler *reject* `await` inside a lock.
- **CompareTrees' `ForAll` body is NOT pure reads — it lazily MUTATES.** `CompareTrees.cs:411` `GetNodePreviousSib()` → `SdcUtil.GetPrevSibElement:1393` → `SortElementKids:3290`, which does `kids.Sort(...)` (in-place on the live `_ChildNodes` list, `:3308`) + `TreeSort_Add` (`:3309`). A naive read lock would be **wrong**. **Resolution:** run a **single-threaded pre-sort pass under the WRITE lock** before the `ForAll` (reusing TS-7's batched sort), after which `SortElementKids` is a no-op and the `ForAll` is genuinely read-only → wrap in one `ReadLockScope`, drop the local `lock(locker)`. **Order TS-7 before the CompareTrees part of TS-2.**
- **WASM test harness DEFERRED; desktop proxy chosen.** No Blazor/WASM project exists (all `net11.0`, MSTest); a real `browser-wasm` run needs the `wasm-tools` workload, a new test project, a virtual FS (browser has no filesystem, but tests load XML from disk), and WASM's single thread makes the parallel stress tests degrade to sequential — **desktop multithreading already covers a superset** of WASM's parallel hazards. The one WASM-specific hazard (same-thread cooperative reentrancy) is covered by a **cheap desktop proxy test authored during TS-2**.

---

## 3. What changed in the plan (TS-5 correction + async/WASM, both now RESOLVED)

1. **TS-5 was factually wrong and is corrected.** The old text said `TreeLock` is "dead infrastructure, zero `Wait/Release` call sites, DELETE it." **False** — `CompareTrees.cs` uses it at 18 sites. A cheap model following the old TS-5 verbatim would **delete `TreeLock` and break the CompareTrees build.** Corrected TS-5 = **MIGRATE then delete**: repoint CompareTrees' `TreeLock` reads and writers' `_SyncRoot` onto the unified `ReaderWriterLockSlim`, verify, *then* remove both. The diagnosis doc's "referenced nowhere / count=0" claim is likewise corrected.
2. **Async/await + WASM (plan §12) is now RESOLVED/LOCKED.** Two investigations closed it: (a) the library is **100% synchronous** (no `await` to violate the rule — it becomes a free `ref struct` compile-time guardrail); (b) CompareTrees' `ForAll` body is **NOT pure reads** — `GetNodePreviousSib` lazily sorts `_ChildNodes` in place — so the fix is a **pre-sort pass under the write lock** (reusing TS-7) **then** a read lock, not a naive read lock. The **WASM test harness is deferred**; a cheap desktop reentrancy proxy (authored in TS-2) replaces it.

> Net: the plan is now **fully locked and ready to code** once the user approves. The only code consequence beyond the original plan is the CompareTrees **pre-sort-before-read-lock** step, folded into TS-2 (after TS-7).

---

## 4. Immediate next actions (for the resumed session, in order)

1. **Verify the checkpoint builds** (`dotnet build "SDC.Schema.Tests\SDC.Schema.Tests.csproj" -c Debug --nologo -v quiet`) — `bbcadca` already carries the partial `SemaphoreSlim TreeLock` infra, so confirm it compiles before deciding anything.
2. **Resolve the one open decision with the user:** build Option C **on top of** the existing `SemaphoreSlim TreeLock` infra (migrate-then-delete per TS-5), **or revert** `bbcadca`'s production changes to baseline `a98660a` and implement Option C clean. *(Recommended: migrate-then-delete — `TreeLock` is live in CompareTrees and the plan already specifies it.)*
3. **Get user approval** of the now-fully-locked plan (§9 approval gate). No design work remains — §12 is resolved.
4. **Execute the locked sequence** (plan §6) on a NEW sub-branch `Features/Net11Upgrade_ThreadSafety_OptionCImpl`: infra (§3) → TS classification (§5) → TS-1 → **TS-7** → TS-2 (incl. CompareTrees **pre-sort-before-read-lock**, which depends on TS-7) → TS-4 → TS-3 → TS-6 → TS-5 (migrate+delete), one commit per item, gating with §7.
5. During **TS-2**, author the **desktop cooperative-reentrancy proxy test** (the WASM-hazard stand-in; the real WASM harness is deferred per §12.6).
6. Keep the **approval gate**: no production edits until the user signs off.

---

## 5. KICKSTART PROMPT (copy-paste to begin the next session)

```
Resume the SDC.Schema thread-safety effort.

REPO: C:\Users\RMold\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema
BRANCH: Features/Net11Upgrade_ThreadSafety  (HEAD = bbcadca, pushed & in sync with origin)
BASELINE: a98660a "Merge OM Tree Stability Suite + Move/Reparent Bug Fixes"

READ THESE DOCS FIRST, IN ORDER (all under SDC.Schema.Tests\Documentation\):
1. ThreadSafety_SessionSummary_AND_Kickstart.md   (START HERE — status, decisions, kickstart)
2. ThreadSafety_RemediationPlan_OptionC.md          (locked Option C spec + TS-1..TS-7 edit map, §6 sequencing, §6.1 commit model, §12 async/WASM RESOLVED)
3. ThreadSafety_RootCauseDiagnosis.md               (evidence, TS/RC-1..7, reader/writer map)
   Supporting: ThreadSafety_StrategyDecision.md, ThreadSafety_LockingStrategy_Analysis.md
   (Superseded weaker-model docs live in Archived Plans\ — do NOT use them; see its README.md.)

STATE YOU MUST HONOR:
- The current safety commit bbcadca ALREADY contains partial production changes vs baseline a98660a:
  ITopNode.cs, PartialClasses.cs, CompareTrees.cs, IMoveRemoveExtensions.cs (SemaphoreSlim TreeLock infra + CompareTrees GUID-ordered locking + a prevXml/newXml deserialization bug fix).
  This is a WIP SAFETY checkpoint, not a finished design implementation.
- Strategy is LOCKED = Option C: one ReaderWriterLockSlim per TopNode, LockRecursionPolicy.SupportsRecursion, single-writer/multiple-reader. THE ONE RULE: lock once at the public boundary; writers lock at the top; NEVER a read->write upgrade.
- IDs are TS-1..TS-7 (alias of legacy RC-1..RC-7).

RESOLVED FACTS — DO NOT RE-INVESTIGATE:
- The library is 100% SYNCHRONOUS (no async/await/Task in SDC.Schema). "No lock across await" is a free compile-time guardrail; keep the lock-scope types as ref struct.
- CompareTrees.cs guards a parallel read (AsParallel().ForAll, ~line 347) with TreeLock (SemaphoreSlim) at 18 sites (lines 126-303); writers use _SyncRoot (Monitor) -> the two locks do NOT mutually exclude -> a real corruption window. Unify BOTH onto the per-TopNode ReaderWriterLockSlim.
- CompareTrees' ForAll body is NOT pure reads: GetNodePreviousSib -> SdcUtil.GetPrevSibElement (:1393) -> SortElementKids (:3290) does kids.Sort(...) in place + TreeSort_Add. FIX = pre-sort the involved parents under the WRITE lock (reuse TS-7's batched sort) BEFORE the ForAll, then wrap the ForAll in one ReadLockScope and remove the local lock(locker). Order TS-7 before the CompareTrees part of TS-2.
- TS-5 is CORRECTED: TreeLock is LIVE (not dead). migrate-then-delete, never delete-first.
- The real WASM test harness is DEFERRED (no Blazor/WASM project; browser has no filesystem; WASM is single-threaded so desktop multithreading is a superset). Cover the one WASM hazard with a desktop cooperative-reentrancy proxy test during TS-2.

FIRST ACTIONS (in order):
1. Confirm you have read docs 1-3. Summarize the locked decisions back to the user.
2. Verify the checkpoint builds: dotnet build "SDC.Schema.Tests\SDC.Schema.Tests.csproj" -c Debug --nologo -v quiet
3. Resolve the ONE open decision with the user BEFORE editing code:
   Build Option C on top of the existing SemaphoreSlim TreeLock infra (migrate-then-delete per TS-5),
   OR revert the production changes in bbcadca back to baseline a98660a and implement Option C clean.
   (Recommended: migrate-then-delete, because TreeLock is live in CompareTrees and the plan already specifies it.)
4. Get explicit approval at the §9 approval gate.
5. Create implementation sub-branch Features/Net11Upgrade_ThreadSafety_OptionCImpl, then execute plan §6:
   infra (§3) -> TS-1 -> TS-7 -> TS-2 (incl. CompareTrees pre-sort-before-read-lock) -> TS-4 -> TS-3 -> TS-6 -> TS-5 (migrate+delete).
   ONE TS-item per commit, message convention "TS-#: <change> — <gate result>", gate (build+test) after each.
6. During TS-2, author the desktop cooperative-reentrancy proxy test (WASM-hazard stand-in).

REPO RULES (hard constraints):
- Never weaken the repro assertions; the repro is EXPECTED to fail/stall pre-fix — keep failures visible.
- Abort + root-cause any unit test > 1s or functional test > 10s; never allow an infinite loop.
- Run the bounded repro (ThreadSafetyReproTests) ONLY in a background terminal with a kill switch.
- Branch/folder naming: PascalCase, no dashes, underscores sparingly; preserve ALL-CAPS abbreviations (IET).
- Do NOT merge to master and do NOT push without explicit user approval.
- When amending, always review/adjust the commit message first. Keep all .md (incl. Archived Plans\) tracked.

BEGIN by confirming the three docs are read, then run the build check, then ask the user the step-3 decision.
```

---

## 6. Document index (all under `SDC.Schema.Tests\Documentation\`)

| File | Role |
|------|------|
| `ThreadSafety_SessionSummary_AND_Kickstart.md` | **THIS** — restart entry point + kickstart prompt |
| `ThreadSafety_RemediationPlan_OptionC.md` | Option C spec (**fully locked**; §12 async/WASM RESOLVED); TS-1…TS-7 mechanical edit map, §4f lock table, gates, sequencing, §6.1 commit/branch model, §12 async/WASM (pre-sort-then-read-lock; WASM harness deferred) |
| `ThreadSafety_RootCauseDiagnosis.md` | Evidence record (TS/RC-1…7, reader/writer map, repro results) |
| `ThreadSafety_SessionHandoff.md` | Prior resume file (now points here) |
| `ThreadSafety_StrategyDecision.md` | Original `ReaderWriterLockSlim` decision (finalized as Option C) |
| `ThreadSafety_LockingStrategy_Analysis.md` | Locking deep-dive — kept active as a future-work overview |
| `Archived Plans\` | Superseded weaker-model plans (do **not** use); see `Archived Plans\README.md` for provenance + canonical map |

**Key code anchors** (verified): `CompareTrees.cs` TreeLock 18 sites lines 126–303, parallel read `AsParallel().ForAll` ~347; `PartialClasses.cs` `LastTopNode` ~2348, `_MaxObjectID++` 1746, `ParentNode` getter 2143, infra block 155–163, `ItemsMutator` 2265; `IMoveRemoveExtensions.cs` `RegisterAll` 803–847 (sort 905–913, increment 818), `UnRegisterAll` 1045+, `MoveInDictionaries` 34–58; `BaseTypeExtensions.cs` `FindRootNode` 418–429; `ITopNode.cs` `_ITopNode` declares `TreeLock` + `_SyncRoot`.
