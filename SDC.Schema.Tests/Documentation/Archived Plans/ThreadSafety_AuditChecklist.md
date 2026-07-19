# Thread Safety API Audit Checklist

## Instructions
- Mark each item with ✅ (audited), ⏳ (in progress), or ⬜ (not started)
- Fill in findings as you audit each file/method
- Priority: P1 (Phase 1), P2 (Phase 2), P3 (Phase 3)

---

## Category A: Mutable Reference Returns Audit

### File: IMoveRemoveExtensions.cs
| Status | Method | Return Type | Risk | Fix Strategy | Priority |
|--------|--------|-------------|------|--------------|----------|
| ⏳ | RegisterAll | BaseType | Returns `this` (mutable) | Return void or immutable handle | P1 |
| ⬜ | [Add more methods] | | | | |

### File: BaseTypeExtensions.cs
| Status | Method | Return Type | Risk | Fix Strategy | Priority |
|--------|--------|-------------|------|--------------|----------|
| ⬜ | TryGetChildNodes | bool + out ReadOnlyCollection<BaseType>? | ROC can be cast to mutable | Return IEnumerable or true immutable | P1 |
| ⬜ | GetChildNodes | List<BaseType>? | Direct mutable reference | Return ReadOnlyCollection | P1 |
| ⬜ | GetSubtreeIETList | List<IdentifiedExtensionType> | Direct mutable reference | Return IEnumerable with yield | P1 |
| ⬜ | [Add more methods] | | | | |

### File: PartialClasses.cs
| Status | Property/Method | Return Type | Risk | Fix Strategy | Priority |
|--------|-----------------|-------------|------|--------------|----------|
| ⬜ | _ITopNode._Nodes | Dictionary<Guid, BaseType> | Public mutable dictionary | Make private, add accessor methods | P1 |
| ⬜ | _ITopNode._ParentNodes | Dictionary<Guid, BaseType> | Public mutable dictionary | Make private, add accessor methods | P1 |
| ⬜ | _ITopNode._ChildNodes | Dictionary<Guid, List<>> | Nested mutable | Make private, return immutable views | P1 |
| ⬜ | _ITopNode._IETnodes | ObservableCollection<IET> | Observable + mutable | Make private, control access | P1 |
| ⬜ | ChildItemsType.ChildItemsList | List<IET> | Direct list property | Return ReadOnlyCollection | P1 |
| ⬜ | [Add more] | | | | |

### File: CompareTrees.cs ⚠️ **USER REQUESTED - PRIORITY 1**

#### Public Properties (Mutable References to Live Trees)
| Status | Property | Return Type | Risk | Fix Strategy | Priority |
|--------|----------|-------------|------|--------------|----------|
| ⏳ | NewVersion | T (ITopNode) | ✅ Mutable ref (design preference) | Add read lock in getter | P1 |
| ⏳ | PrevVersion | T (ITopNode) | ✅ Mutable ref (design preference) | Add read lock in getter | P1 |

**Analysis:** Properties return live tree references (✅ correct per user design preference)

#### Public Methods Returning ReadOnlyCollection (Snapshot at Ctor Time)
| Status | Method | Return Type | Risk | Fix Strategy | Priority |
|--------|--------|-------------|------|--------------|----------|
| ✅ | GetIETnodesRemovedInNew | ReadOnlyCollection&lt;IET> | ⚠️ Point-in-time, but nodes are mutable refs | Document staleness behavior | P1 |
| ✅ | GetIETnodesAddedInNew | ReadOnlyCollection&lt;IET> | ⚠️ Point-in-time, but nodes are mutable refs | Document staleness behavior | P1 |
| ✅ | GetNodesRemovedInNew | ReadOnlyCollection&lt;BaseType> | ⚠️ Point-in-time, but nodes are mutable refs | Document staleness behavior | P1 |
| ✅ | GetNodesAddedInNew | ReadOnlyCollection&lt;BaseType> | ⚠️ Point-in-time, but nodes are mutable refs | Document staleness behavior | P1 |

**Analysis:** Collections computed in ctor (lines 171-182), frozen at comparison time. ✅ **Correct design:**
- Collection membership is point-in-time snapshot (which nodes added/removed)
- Node references themselves are mutable (properties can change ✅ by design)
- Client detects post-comparison mutations via ObservableCollection
- **No fix needed** - behavior matches user requirements

#### Public Methods Returning Live Dictionary View
| Status | Method | Return Type | Risk | Fix Strategy | Priority |
|--------|--------|-------------|------|--------------|----------|
| ⏳ | GetIETattDiffs | ReadOnlyDictionary&lt;string, DifNodeIET> | ⚠️ Wraps ConcurrentDictionary (live) | Document that it's a live view | P1 |

**Analysis:** Returns `ReadOnlyDictionary` wrapping `_dDifNodeIET` (ConcurrentDictionary).
- Dictionary computed during ctor via `CompareVersionAttributes()` (line 119)
- **Not** a snapshot - it's a read-only **view** of a mutable concurrent dictionary
- If trees mutate after comparison, this dictionary does NOT reflect new changes
- **Fix:** Document that it's point-in-time comparison result, but underlying nodes remain mutable

#### Public Methods Returning Nullable Structs/References
| Status | Method | Return Type | Risk | Fix Strategy | Priority |
|--------|--------|-------------|------|--------------|----------|
| ✅ | GetIETattributes(IET) | DifNodeIET? (struct) | ✅ Safe (value type) | None needed | P1 |
| ✅ | GetIETattributes(ShortGuid) | DifNodeIET? (struct) | ✅ Safe (value type) | None needed | P1 |
| ⏳ | FindAddedIETsubNodes | List&lt;BaseType>? | ⚠️ Returns new mutable list with mutable refs | Document behavior | P1 |
| ⏳ | FindRemovedIETsubNodes | List&lt;BaseType>? | ⚠️ Returns new mutable list with mutable refs | Document behavior | P1 |
| ✅ | IsNewNodeAdded | bool + out BaseType? | ✅ Mutable ref (design preference) | None needed | P1 |
| ✅ | IsPrevNodeRemoved | bool + out BaseType? | ✅ Mutable ref (design preference) | None needed | P1 |

**Analysis:** 
- `DifNodeIET` is a struct (line 380-381), so returns are value copies (✅ safe)
- `FindAddedIETsubNodes` / `FindRemovedIETsubNodes` create NEW lists (lines 684, 722) ✅ safe
- Lists contain mutable node references ✅ by design
- `out` params return mutable references ✅ correct per user preference

#### Internal Comparison Entry Points (Need Lock Acquisition)
| Status | Method | Internal? | Lock Needed? | Fix Strategy | Priority |
|--------|--------|-----------|--------------|--------------|----------|
| ⬜ | CtorCompareTrees | private | ✅ YES | Acquire read locks on both trees | P1 |
| ⬜ | ChangePrevVersion | private | ✅ YES | Acquire read lock on prevVersion tree | P1 |
| ⬜ | ChangeNewVersion | private | ✅ YES | Acquire read lock on newVersion tree | P1 |
| ⬜ | CompareIET | public | ✅ YES | Acquire read lock on both trees | P1 |
| ⬜ | CompareVersionAttributes | private | ❌ NO (called from locked ctor) | Already protected by ctor lock | P1 |

**Analysis (CRITICAL):**
- Per user: "The trees should be locked when the comparison starts"
- Per user: "Comparison already has some locks for parallelized node visiting" (lines 192, 249, 251, 270)
- **Required:** Acquire ReaderWriterLockSlim **read locks** on BOTH trees at comparison start:
  - In `CtorCompareTrees()` before calling `FindSerializedXmlAttributesFromTree()` (lines 115-116)
  - In `ChangePrevVersion()` and `ChangeNewVersion()` before recomputing
  - In `CompareIET()` before comparing single IET
- Existing internal `lock(locker)` (lines 192, 249, 251, 270) protects parallel node visiting ✅
- **Keep existing locks**, add tree-level read locks around entire comparison operation

#### Blazor/WASM Compatibility ⚠️
| Issue | Current State | Required Action | Priority |
|-------|---------------|-----------------|----------|
| Parallel.ForAll (line 206) | Uses TPL parallelism | Test in WASM (single-threaded event loop) | P1 |
| ReaderWriterLockSlim | Standard lock | May need SemaphoreSlim for async/await in WASM | P1 |
| ConcurrentDictionary | Thread-safe | Should work in WASM | P1 |

**User Note:** "has some challenges when running in Blazor/WASM due to different 'threading' model"
- WASM uses single-threaded event loop (no true threads)
- `Parallel.ForAll` may degrade to sequential execution (still safe)
- `ReaderWriterLockSlim` works synchronously (safe in WASM)
- If using `async/await` comparison in WASM, may need `SemaphoreSlim` instead
- **Action:** Phase 2 testing must include WASM verification

**Notes on CompareTrees.cs:**
- ✅ User confirmed: "I prefer to return mutable reference objects, as they reflect up-to-date properties for each node"
- ✅ Comparison returns point-in-time membership (which nodes added/removed), but nodes themselves remain live
- ✅ "Trees should be locked when comparison starts" - need read locks on both trees
- ✅ "Comparison already has some locks for parallelized node visiting" - internal locks at lines 192, 249, 251, 270
- ✅ "After comparison completes, client app responsibility to detect new mutations via observable collection"
- ⚠️ WASM compatibility needs testing in Phase 2

---

## Category C: Complex Multi-Step Operations Requiring Locks

### File: IMoveRemoveExtensions.cs
| Status | Method | Steps | Lock Strategy | Priority |
|--------|--------|-------|---------------|----------|
| ⬜ | Move | 6 steps (validate, clear source, refresh, attach, register, order) | Tree lock (entire operation) | P2 |
| ⬜ | RemoveRecursive | 5 steps (check, recurse, enum, remove obj, unregister) | Tree lock (entire operation) | P2 |
| ⬜ | DropMove | Delegates to Move | Inherit Move's tree lock | P2 |
| ⬜ | Graft | Delegates to Move | Inherit Move's tree lock | P2 |
| ⬜ | Copy | Delegates to Move | Inherit Move's tree lock | P2 |
| ⬜ | Restore | Delegates to Move | Inherit Move's tree lock | P2 |
| ⬜ | RegisterAll | 5 dictionary updates | Tree lock | P2 |
| ⬜ | UnRegisterAll | 5 dictionary removals | Tree lock | P2 |
| ⬜ | MoveInDictionaries | Calls UnRegisterAll + RegisterAll | Tree lock (inherited) | P2 |

### File: SdcUtil.cs
| Status | Method | Steps | Lock Strategy | Priority |
|--------|--------|-------|---------------|----------|
| ⬜ | ReflectRefreshTree | Rebuilds all dictionaries | Tree lock (entire tree) | P2 |
| ⬜ | ReflectRefreshSubtreeList | Updates subtree identity + dicts | Tree lock | P2 |
| ⬜ | [Add more reflection methods] | | | |

---

## Category D: IEnumerable/Collection Enumeration Sites

### File: IMoveRemoveExtensions.cs
| Status | Method/Location | Collection Being Enumerated | Current Protection | Fix Needed | Priority |
|--------|-----------------|----------------------------|-------------------|------------|----------|
| ✅ | ItemsMutator (PartialClasses.cs) | List<T> | `.ToArray()` snapshot | Race condition check→ToArray | P2 |
| ⬜ | RegisterSubtreeIn_IETnodes | GetSubtreeIETList() result | None | Add snapshot before foreach | P2 |
| ⬜ | UnRegisterAll.UnRegister | GetSubtreeIETList() result | None | Add snapshot before foreach | P2 |
| ⬜ | RemoveNodesRecursively | kids List<BaseType> | None (while loop) | Lock or snapshot | P2 |
| ⬜ | [Scan for foreach/while over collections] | | | | |

### Enumeration Pattern Search
**Search patterns to check:**
```csharp
// Dangerous patterns (enumerate without snapshot):
foreach (var item in [dictionary/list/collection])
while ([collection].Count > 0)
for (int i = 0; i < [collection].Count; i++)  // Count can change during loop

// Safe patterns (snapshot first):
foreach (var item in [collection].ToArray())
foreach (var item in [collection].ToList())
var snapshot = [collection].ToArray(); foreach (var item in snapshot)
```

**Files to scan:**
- [ ] IMoveRemoveExtensions.cs
- [ ] BaseTypeExtensions.cs
- [ ] PartialClasses.cs
- [ ] SdcUtil.cs
- [ ] [Add other utility files]

---

## Category H: Static Shared State Audit

### Search Results for `static` Fields/Properties
| Status | File | Member | Type | Mutable? | Fix Needed | Priority |
|--------|------|--------|------|----------|------------|----------|
| ✅ | IMoveRemoveExtensions.cs | treeSibComparer | TreeSibComparer | Appears stateless | Verify immutability | P1 |
| ⬜ | BaseType.cs | [ResetLastTopNode related] | ? | ? | Investigate | P1 |
| ⬜ | [Search all .cs files for "private static", "public static"] | | | | | |

**Search Commands:**
```powershell
# Find all static fields (run in workspace root)
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "^\s*(private|public|internal|protected)\s+static\s+" | Select-Object Path, LineNumber, Line

# Find all static properties
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "static.*\{.*get" | Select-Object Path, LineNumber, Line
```

---

## Category F: Event-Driven Side Effects Audit

### ObservableCollection Usages
| Status | Location | Collection | Events Subscribed? | Re-entrancy Risk | Fix Strategy | Priority |
|--------|----------|------------|-------------------|------------------|--------------|----------|
| ⬜ | _ITopNode._IETnodes | ObservableCollection<IET> | Unknown | High | Audit event handlers | P2 |
| ⬜ | [Search for CollectionChanged subscriptions] | | | | | |

**Search Commands:**
```powershell
# Find CollectionChanged subscriptions
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "CollectionChanged\s*\+=" | Select-Object Path, LineNumber, Line

# Find ObservableCollection declarations
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "ObservableCollection<" | Select-Object Path, LineNumber, Line
```

---

## Category G: Lazy Initialization Patterns

### Search for Double-Init Risks
| Status | Location | Pattern | Thread-Safe? | Fix Needed | Priority |
|--------|----------|---------|--------------|------------|----------|
| ⬜ | [Search for `??=`] | Null-coalescing assignment | Maybe | Check each case | P3 |
| ⬜ | [Search for `if (x == null) x = `] | Null-check init | No | Add lock or Lazy<T> | P3 |
| ⬜ | [Search for `Lazy<`] | Lazy<T> usage | Yes (by default) | Verify LazyThreadSafetyMode | P3 |

**Search Commands:**
```powershell
# Null-coalescing assignment
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "\?\?=" | Select-Object Path, LineNumber, Line

# Lazy<T> usage
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "Lazy<" | Select-Object Path, LineNumber, Line

# Null-check initialization pattern
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "if\s*\(.*==\s*null\s*\).*=\s*new" | Select-Object Path, LineNumber, Line
```

---

## Quick Reference: Search Commands Summary

Run these in workspace root to populate audit tables:

```powershell
# 1. Find mutable collection returns
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "(List|Dictionary|Collection)<.*>\s+\w+\s*\(" | Select-Object Path, Line

# 2. Find public/internal properties returning collections
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "(public|internal).*(\w+Collection|List|Dictionary)<.*>\s+\w+\s*\{" | Select-Object Path, Line

# 3. Find all foreach loops
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "foreach\s*\(" | Select-Object Path, LineNumber, Line

# 4. Find while loops accessing Count
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "while\s*\(.*\.Count" | Select-Object Path, LineNumber, Line

# 5. Find static members
Get-ChildItem -Recurse -Include *.cs | Select-String -Pattern "static\s+(readonly\s+)?[^\s]+\s+\w+\s*[=;]" | Select-Object Path, Line
```

---

## Audit Progress Summary

### Overall Progress
- [ ] Phase 1 Audit: Category A + H (Foundation)
- [ ] Phase 1 Fixes: Immutable returns, static state
- [ ] Phase 2 Audit: Category C + D + F (Synchronization)
- [ ] Phase 2 Implementation: Add locks, fix enumerations
- [ ] Phase 3 Audit: Category G + E (Optimization)
- [ ] Phase 3 Implementation: Optimize, document

### Quick Stats
| Category | Total Items | Audited | Fixed | Remaining |
|----------|------------|---------|-------|-----------|
| A: Mutable Returns | ? | 0 | 0 | ? |
| C: Complex Ops | 9 | 0 | 0 | 9 |
| D: Enumerations | 4+ | 1 | 1 | 3+ |
| F: Events | ? | 0 | 0 | ? |
| G: Lazy Init | ? | 0 | 0 | ? |
| H: Static State | 1+ | 1 | 0 | 1+ |
| **TOTAL** | **?** | **2** | **1** | **?** |

---

## Notes & Decisions Log

### [Date] - Initial Audit Setup
- Created audit checklist
- User requested focus on CompareTrees.cs for mutable references
- Identified 9 Category C methods requiring tree-level locks
- treeSibComparer identified as potential static shared state (needs verification)

### 2024-Current - User Input on Open Questions ✅

**Agent interaction model:**
- ✅ Multiple agents mutating the SAME shared TopNode tree (high contention scenario)
- ✅ Read:Write ratio = 100:1 (read-heavy workload)
- **Implication:** Reader-Writer locks are ideal for this scenario

**Consistency requirements:**
- ✅ Strong consistency required (reads must see latest writes)
- **Implication:** Cannot use eventual consistency or stale snapshots for reads

**Performance priorities:**
- ✅ Mutation latency: 1 second acceptable
- ✅ Read throughput: 1000 ops/sec
- ✅ Write throughput: 1 large tree branch/sec
- ✅ Concurrency: Up to CPU count parallel processes
- **Implication:** Write locks can be heavyweight; read locks must be fast

**CompareTrees.cs specifics:**
- ✅ Returns point-in-time comparison with some mutable references attached
- ✅ Primary use: Annotate visible SDC tree for manual editing
- ✅ Trees should be locked when comparison starts
- ✅ Comparison already has internal locks for parallel node visiting
- ✅ Post-comparison mutations detected via ObservableCollection in client app
- **Implication:** CompareTrees needs read lock acquisition, client handles staleness

**Design Preference (CRITICAL):**
- ✅ **PREFER returning mutable reference objects** (reflects up-to-date properties)
- ✅ Mutable refs allow immediate propagation to client apps (desirable)
- ✅ Node movement detection via ObservableCollection (mitigation strategy)
- ✅ **Main concern: Dictionary/List corruption from concurrent moves/adds/deletes**
- **Implication:** Keep mutable returns; protect collections, not references

---

*Audit maintained by: Development Team*  
*Branch: Features/Net11Upgrade_ThreadSafety*  
*Last Updated: [Current Date]*
