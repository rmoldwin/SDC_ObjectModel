# Thread Safety TS-6 — Completion Handoff

**Branch:** `Features/NET10/ThreadSafety_TSAudit`
**Date completed:** 2025
**Status:** ✅ ALL TS-1 through TS-7 fixes applied and verified. 12/12 ThreadSafety tests pass.

---

## What was done this session

### Defect: TS-6 — Unprotected `IList<T>` mutations in `Move()`

`IMoveRemoveExtensions.Move()` called `FindRootNode()` and `IsAttachNodeAllowed()` (both
read the non-thread-safe `Dictionary<Guid,BaseType> _ParentNodes` / `_ChildNodes`) before
acquiring any lock, then mutated source and target `IList<T>` parent lists with no outer
synchronization. Concurrent callers raced on the same backing arrays and dictionary entries,
producing `ArgumentException` / `InvalidOperationException` and silently lost or duplicated
list entries.

### Root cause (precise)

`_ParentNodes` and `_ChildNodes` are plain `Dictionary<Guid,BaseType>` instances — not
`ConcurrentDictionary`. Any concurrent read **and** write, or two concurrent writes, can
corrupt them. The `Move()` preamble contained two such unprotected reads before the body of
the method even began.

### Fix applied — `IMoveRemoveExtensions.cs`

| Change | Detail |
|--------|--------|
| `DualWriteLock` acquired at **top of `Move()`** | After the 3 null guards, before `FindRootNode()` / `IsAttachNodeAllowed()` / any IList mutation. Covers every code path in the method. |
| `AcquireMoveLocks()` uses `TopNode.ObjectGUID` | Replaced `FindRootNode()` (dict read) with `TopNode.ObjectGUID` (plain field) for GUID-order cross-tree lock ordering. Now safe to call before any lock is held. |
| Removed redundant inner `AcquireMoveLocks` calls | Deleted from the `UpdateNodeIdentity` branch and the `MoveSingleNode()` IList branch (both now covered by the outer lock). |

### New regression tests — `ThreadSafetyReproTests.cs`

| Test | Status |
|------|--------|
| `Repro_ConcurrentMoves_UnprotectedListMutations_CorruptListIntegrity` | ✅ PASSES (was: 31 exceptions, `sourceListCount=2`, `targetListCount=197`) |
| `Repro_ConcurrentCrossTreeMoves_UpdateNodeIdentityPath_NoCorruption` | ✅ PASSES (new cross-tree `UpdateNodeIdentity` gate) |
| `Repro_ConcurrentCrossTreeMoves_DoNotDeadlock` | ✅ PASSES (deadlock probe, unchanged) |

### Commits on `Features/NET10/ThreadSafety_TSAudit`

```
d573240  fix(TS-6): acquire DualWriteLock at top of Move() — protects dict reads and IList mutations
8d79976  test(TS-6): add Repro_ConcurrentMoves_UnprotectedListMutations_CorruptListI
```

---

## Full thread-safety status (all TS items)

| ID | Description | Status | Location |
|----|-------------|--------|----------|
| TS-1 | `static LastTopNode` cross-tree contamination | ✅ Fixed | `[ThreadStatic]` in `PartialClasses.cs` |
| TS-2 | Unsynchronized `Dictionary<>` read-during-write (hang surface) | ✅ Fixed | `WriteLockScope` in `RegisterAll`, `ItemsMutator`, `BaseName` setter |
| TS-3 | Non-atomic `_MaxObjectID++` → duplicate `ObjectID`s | ✅ Fixed | `Interlocked.Increment` in `PartialClasses.cs` `AtomicNextObjectID()` |
| TS-4 | `ItemsMutator` `lock(new object())` fallback = no mutual exclusion | ✅ Fixed | `WriteLockScope` on tree's `TreeRwLock` in `ItemsMutator<T>` |
| TS-5 | `TreeLock` (SemaphoreSlim) in `CompareTrees.cs` on a separate regime from writers | ✅ Fixed / Migrated | `ReadLockScope`/`WriteLockScope` in `CompareTrees.cs`; `TreeLock` SemaphoreSlim removed |
| TS-6 | `Move()` IList mutations and dict reads with no outer lock | ✅ Fixed (this session) | `DualWriteLock` at top of `Move()` in `IMoveRemoveExtensions.cs` |
| TS-7 | `AssignOrder()` and sort under `_ChildNodes` insert: reflection perf cliff | ✅ Fixed | `WriteLockScope` guards; `childNodesSort=false` on critical paths |

### Test suite state

```
ThreadSafety tests (FullyQualifiedName~ThreadSafety): 12/12 PASS
MoveTests:   14 pass / 3 fail (CloneSdcSubtreeBsonTest, CloneSdcSubtreeJsonTest,
			 CloneSdcSubtreeMpackTest — pre-existing serializer bugs, NOT regressions)
Full suite:  406/417 pass — 11 failures, all pre-existing serializer/MsgPack issues
```

---

## Pre-existing failures (NOT caused by thread-safety work)

The 3 `CloneSdcSubtree*` failures and 8 additional serializer/MsgPack failures were
present before the TS-6 work began (confirmed with `git stash` round-trip). They are:

| Test | Failure | Root cause |
|------|---------|------------|
| `CloneSdcSubtreeBsonTest` | `InvalidOperationException: ParentNode cannot be null` | Missing `TypeNameHandling.All` + `ConstructorHandling` on BSON serializer |
| `CloneSdcSubtreeJsonTest` | Same | Missing `TypeNameHandling.All` on JSON serializer |
| `CloneSdcSubtreeMpackTest` | `SerializationException: Cannot serialize type XmlElement` | `MsgPack.Cli` does not understand `[XmlInclude]` / `[JsonProperty]` |
| `JsonRoundTripFidelityTest` | `JsonSerializationException: Could not create abstract type` | Missing `TypeNameHandling.All` in `SdcSerializerJson<T>` |
| `BsonRoundTripFidelityTest` | Same + constructor null | Missing both settings in `SdcSerializerBson<T>` |
| `MsgPackRoundTripFidelityTest` | `SerializationException: XmlElement` | MsgPack.Cli limitation |
| `Get/Deserialize/SaveMsgPackTest` (×4) | Same | MsgPack.Cli limitation |

These are the target of the **next session** (branch `Features/NET10/FixBSON_JSON_MsgPack_RoundTrips`).
Full diagnosis is already in `BsonJsonSerializationBugReport.md`.

---

## Next session: Fix JSON, BSON, and MsgPack round-trip failures

### Branch to create

```powershell
git checkout Features/NET10/ThreadSafety_TSAudit
git checkout -b Features/NET10/FixBSON_JSON_MsgPack_RoundTrips
```

### Files to fix

| File | Fix needed |
|------|-----------|
| `SDC.Schema\SDC Customized Classes\SDC Serializers\SdcSerializerJson.cs` | Add `TypeNameHandling = TypeNameHandling.All` to both `SerializeJson` and `DeserializeJson` settings |
| `SDC.Schema\SDC Customized Classes\SDC Serializers\SdcSerializerBson.cs` | Set `TypeNameHandling = TypeNameHandling.All` and `ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor` on the cached `JsonSerializer` instance |
| `SDC.Schema\SDC Customized Classes\SDC Serializers\SdcSerializerMsgPack.cs` | Evaluate whether `MsgPack.Cli` can be made to work; if not, replace with a library that understands `[XmlInclude]` / reflection attributes (see `BsonJsonSerializationBugReport.md §5`) |

### Tests that should pass after fixes

- `JsonRoundTripFidelityTest` ✅ (after JSON fix)
- `BsonRoundTripFidelityTest` ✅ (after BSON fix)
- `CloneSdcSubtreeJsonTest` ✅
- `CloneSdcSubtreeBsonTest` ✅
- `MsgPackRoundTripFidelityTest` ❓ (MsgPack.Cli may be fundamentally incompatible)
- `CloneSdcSubtreeMpackTest` ❓

### Reference documents

- `BsonJsonSerializationBugReport.md` — full diagnosis, exact code locations, code before/after
- `SDC.Schema.Tests\Functional\Serialization\SdcSerializationTests.cs` — round-trip fidelity tests
- `SDC.Schema.Tests\Functional\TreeOperations\MoveTests.cs` — clone/restore subtree tests

---

## Kickstart prompt for next session

Paste the following into a fresh GitHub Copilot session to resume:

---

```
I'm resuming work on the SDC_ObjectModel repository (.NET 10, MSTest).
Branch to create from Features/NET10/ThreadSafety_TSAudit:
  Features/NET10/FixBSON_JSON_MsgPack_RoundTrips

The thread-safety phase (TS-1 through TS-7) is COMPLETE and committed on
Features/NET10/ThreadSafety_TSAudit. All 12 ThreadSafety tests pass.

NEXT TASK: Fix JSON, BSON, and MsgPack serializer round-trip failures.

Pre-existing failing tests (confirmed NOT regressions from thread-safety work):
  - CloneSdcSubtreeBsonTest    → InvalidOperationException: ParentNode cannot be null
  - CloneSdcSubtreeJsonTest    → same
  - CloneSdcSubtreeMpackTest   → SerializationException: Cannot serialize XmlElement
  - JsonRoundTripFidelityTest  → JsonSerializationException: cannot instantiate abstract type
  - BsonRoundTripFidelityTest  → same + constructor null
  - MsgPackRoundTripFidelityTest → SerializationException: XmlElement
  - GetMsgPackTest, DeserializeFromMsgPackTest, etc. (×4)

KNOWN ROOT CAUSES (see BsonJsonSerializationBugReport.md for full diagnosis):
  Bug-1 (BSON): SdcSerializerBson<T>.SerializerBson property creates JsonSerializer()
	with no settings. Needs:
	  TypeNameHandling    = TypeNameHandling.All
	  ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
  Bug-2 (JSON + BSON): JsonConvert calls in SdcSerializerJson<T> use empty
	JsonSerializerSettings. SerializeJson needs TypeNameHandling.All;
	DeserializeJson already has ConstructorHandling but is missing TypeNameHandling.All.
  Bug-3 (MsgPack): MsgPack.Cli cannot serialize XmlElement and doesn't understand
	[XmlInclude]/[JsonProperty] for polymorphic SDC types. May need replacement library.

FILES TO EDIT:
  SDC.Schema\SDC.Schema\SDC Customized Classes\SDC Serializers\SdcSerializerJson.cs
  SDC.Schema\SDC.Schema\SDC Customized Classes\SDC Serializers\SdcSerializerBson.cs
  SDC.Schema\SDC.Schema\SDC Customized Classes\SDC Serializers\SdcSerializerMsgPack.cs

TESTS TO TARGET:
  dotnet test SDC.Schema.Tests\SDC.Schema.Tests.csproj --filter "FullyQualifiedName~Serializ"
  dotnet test SDC.Schema.Tests\SDC.Schema.Tests.csproj --filter "FullyQualifiedName~MoveTests"

Please:
1. Create branch Features/NET10/FixBSON_JSON_MsgPack_RoundTrips from current branch.
2. Read the serializer source files before editing.
3. Apply the Bug-1 and Bug-2 fixes first (JSON and BSON), verify tests, then tackle MsgPack.
4. Document findings and commit when tests pass.
```

---
