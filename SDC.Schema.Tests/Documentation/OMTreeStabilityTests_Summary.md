# OM Tree Stability Tests - Summary and Implementation Guide

## Overview

Created comprehensive functional test suite `_OMTreeStabilityTests.cs` in `SDC.Schema.Tests/Functional/` to detect object-model tree corruption during complex mutation scenarios. These tests complement existing unit tests by exercising realistic multi-level SDC trees through sequences of additions, deletions, moves, and cross-tree operations.

**Status:** Stub file created with 26 test method stubs + 4 helper methods  
**Naming Convention:** Leading underscore `_` in filename and test names indicates stub status per repository guidelines  
**Remove Underscore:** Once tests are implemented, rename file and methods to remove `_` prefix

---

## File Structure

### Location
- **File:** `SDC.Schema.Tests/Functional/_OMTreeStabilityTests.cs`
- **Namespace:** `SDC.Schema.Tests.Functional`
- **Test Framework:** MSTest (`[TestClass]`, `[TestMethod]`)

### Helper Methods (4)

1. **`CreateComplexFormTree(string formId)`** ✅ *Implemented*
   - Builds realistic multi-level SDC structure
   - Structure: FormDesign → Body → 3 Sections → 10 Questions → ListItems/ResponseFields
   - ~40+ nodes for comprehensive stress testing
   - Uses: `QuestionEnum.QuestionSingle`, `QuestionMultiple`, `QuestionRaw`
   - Includes nested sections and mixed question types

2. **`ValidateTreeIntegrity(BaseType topNode, string contextMessage)`** ⚠️ *Stub*
   - Comprehensive dictionary validation
   - Checks: _Nodes, _ParentNodes, _ChildNodes, _IETnodes consistency
   - Detects orphaned nodes and dangling references
   - Validates GUID uniqueness
   - **Implementation guidance:** 7-step validation algorithm documented in stub comments

3. **`ValidateParentChildSymmetry(BaseType node)`** ⚠️ *Stub*
   - Bidirectional parent-child link validation
   - Ensures node appears in parent's child collection and vice versa
   - **Implementation guidance:** 3-step verification process documented

4. **`CountReachableNodes(BaseType topNode)`** ⚠️ *Stub*
   - Tree traversal node counter
   - Used to detect orphaned nodes when compared to dictionary counts
   - **Implementation guidance:** Depth-first/breadth-first traversal algorithm documented

---

## Test Coverage Matrix

### Category 1: Complex Addition Sequences (5 tests)

| Test | Purpose | Key Validations | Thread-Safety Note |
|------|---------|-----------------|-------------------|
| `_ComplexAddition_BulkSiblingInsertion_MaintainsDictionaryConsistency` | Add 50 list items | _ChildNodes list order, sibling navigation | Concurrent additions would corrupt List<> |
| `_ComplexAddition_DeeplyNestedChildSequence_MaintainsParentChain` | 6+ level nesting | Parent chain integrity, _ParentNodes consistency | Nested concurrent additions risk dictionary corruption |
| `_ComplexAddition_InterleavedSectionAndQuestionAddition_PreservesTreeOrder` | Interleaved additions | _IETnodes insertion order, section/question ordering | Global _IETnodes not thread-safe |
| `_ComplexAddition_MultipleListFieldsWithSharedItems_DetectsIllegalSharing` | Shared node instances | Node cannot have two parents, Move() logic | Race on parent assignment |
| `_ComplexAddition_RapidAddRemoveAddCycles_DetectsGUIDRecycling` | 100 add/remove cycles | GUID uniqueness, no dictionary leaks | GUID generation is thread-safe, but dictionary updates aren't |

### Category 2: Bulk Deletion and Cascading Removes (4 tests)

| Test | Purpose | Key Validations |
|------|---------|-----------------|
| `_BulkDeletion_RemoveSectionWithChildren_CascadesAllDescendants` | Remove section with ~50+ descendants | All descendants removed from all dictionaries |
| `_BulkDeletion_RemoveMiddleSiblings_PreservesFirstAndLastSiblingLinks` | Remove items 3-7 from 10-item list | Sibling navigation skips removed nodes |
| `_BulkDeletion_RemoveAllChildrenIteratively_LeavesParentNodeClean` | Remove all children one-by-one | _ChildNodes entry cleaned up, no orphans |
| `_BulkDeletion_RemoveNodeThenAttemptAccessFromSibling_HandlesGracefully` | Navigate after removal | Navigation methods handle removed nodes gracefully |

### Category 3: Multi-Hop Same-Tree Moves (5 tests)

| Test | Purpose | Key Validations | Thread-Safety Note |
|------|---------|-----------------|-------------------|
| `_SameTreeMove_ReorderListItemsBackAndForth_MaintainsListIntegrity` | Move 2→8→2 | List order, sibling links, _ChildNodes consistency | Position updates not atomic |
| `_SameTreeMove_MoveQuestionBetweenSections_UpdatesParentReferences` | Cross-section move | _ParentNodes updated, correct list membership | Parent dictionary race |
| `_SameTreeMove_MoveEntireSectionWithinBody_PreservesDescendantTree` | Move section with descendants | All descendants remain attached, parent chains intact | Subtree migration not atomic |
| `_SameTreeMove_SwapTwoSectionsPositions_HandlesSimultaneousReordering` | Position swap | Both sections retain descendants, dictionaries valid | Swap sequence not atomic |
| `_SameTreeMove_MoveNodeToBeChildOfItsOwnDescendant_PreventsCircularReference` | Circular reference attempt | Move() rejects, no corruption | Race on ancestor validation check |

### Category 4: Cross-Tree Grafting and Orphaning (4 tests)

| Test | Purpose | Key Validations | Thread-Safety Note |
|------|---------|-----------------|-------------------|
| `_CrossTreeMove_GraftQuestionFromFormAToFormB_UpdatesAllDictionaries` | Cross-tree single-node move | Both TopNode dictionaries updated correctly | Touches two dictionary sets |
| `_CrossTreeMove_GraftSectionWithDescendants_MigratesEntireSubtree` | Cross-tree subtree move | All ~25+ descendants switch TopNode references | Large subtree migration not atomic |
| `_CrossTreeMove_MoveNodeBackToOriginalTree_RestoresOriginalState` | Round-trip A→B→A | Final state matches initial, no dictionary leaks | Bidirectional cross-tree race |
| `_CrossTreeOrphan_CreateNodeWithoutParent_LeavesOrphanedUntilAttached` | Late-binding attachment | Orphaned node not in dictionaries until attached | - |

### Category 5: Circular Reference Attempts (3 tests)

| Test | Purpose | Key Validations |
|------|---------|-----------------|
| `_CircularReference_MoveNodeToOwnChild_RejectsOperation` | Direct child circular ref | Move() returns false, no corruption |
| `_CircularReference_MoveNodeToDistantDescendant_RejectsOperation` | Deep ancestor chain validation | Move() detects circular ref through 4+ levels |
| `_CircularReference_SwapParentAndChild_RejectsBothOperations` | Bidirectional swap attempt | Both operations fail or handled correctly |

### Category 6: Mixed Mutation Sequences (5 tests)

| Test | Purpose | Key Validations | Thread-Safety Note |
|------|---------|-----------------|-------------------|
| `_MixedMutation_AddMoveMoveDeleteSequence_MaintainsConsistency` | Add→move→move→delete | Tree returns to baseline, no leaks | Multi-step sequence not atomic |
| `_MixedMutation_BulkAddThenSelectiveDelete_MaintainsRemainingNodes` | Add 20, delete even-numbered | Odd-numbered nodes maintain order and identity | - |
| `_MixedMutation_MoveMultipleNodesSequentiallyToSameTarget_PreservesOrder` | Sequential moves to same target | Nodes appear in move order [1,2,3] | Sequential Move() calls not atomic |
| `_MixedMutation_DeleteParentDuringChildEnumeration_HandlesGracefully` | Delete parent during iteration | No collection-modified exceptions | Tests single-threaded snapshot protection |
| `_MixedMutation_ReplaceListPropertyMultipleTimes_ClearsOldReferences` | Replace list 3 times | Old items detached, no orphans accumulate | ItemsMutator snapshot logic |

### Category 7: Dictionary Consistency Stress Tests (1 test)

| Test | Purpose | Key Validations | Performance Target |
|------|---------|-----------------|-------------------|
| `_StressTest_RapidMutationsCycled100Times_MaintainsDictionaryIntegrity` | 100 cycles: add 5→move 3→delete 2→move 1 | No gradual corruption or memory leaks, node count formula validation | < 10 seconds |

---

## Implementation Priority

### Phase 1: Core Validation Infrastructure
1. Implement `ValidateTreeIntegrity()` - required by most tests
2. Implement `CountReachableNodes()` - required by orphan detection
3. Implement `ValidateParentChildSymmetry()` - optional but useful

### Phase 2: High-Value Tests (Start Here)
1. `_BulkDeletion_RemoveSectionWithChildren_CascadesAllDescendants` - tests cascading delete
2. `_SameTreeMove_MoveQuestionBetweenSections_UpdatesParentReferences` - tests cross-section move
3. `_CrossTreeMove_GraftQuestionFromFormAToFormB_UpdatesAllDictionaries` - tests cross-tree basics
4. `_MixedMutation_ReplaceListPropertyMultipleTimes_ClearsOldReferences` - exercises ItemsMutator snapshot fix

### Phase 3: Edge Cases and Circular References
5. All `_CircularReference_*` tests - validate Move() safety checks
6. `_ComplexAddition_MultipleListFieldsWithSharedItems_DetectsIllegalSharing` - tests parent uniqueness
7. `_MixedMutation_DeleteParentDuringChildEnumeration_HandlesGracefully` - tests snapshot behavior

### Phase 4: Stress and Advanced Scenarios
8. `_StressTest_RapidMutationsCycled100Times_MaintainsDictionaryIntegrity` - gradual corruption detection
9. `_ComplexAddition_BulkSiblingInsertion_MaintainsDictionaryConsistency` - large list stress
10. All remaining tests

---

## Thread-Safety Implications

### High-Risk Areas Identified by Test Stubs

1. **TopNode Dictionary Operations** (all tests)
   - `_Nodes`, `_ParentNodes`, `_ChildNodes` are not thread-safe
   - Concurrent modifications would corrupt dictionaries
   - **Mitigation needed if concurrent mutations become a requirement**

2. **List Modifications** (Category 1, 2, 6)
   - `ChildItemsList` and other `List<>` properties not thread-safe
   - ItemsMutator snapshot logic prevents single-threaded enumeration errors
   - Does NOT prevent concurrent modification corruption

3. **Multi-Step Operations** (Category 3, 4)
   - Move, delete, add sequences not atomic
   - Concurrent operations can interleave and corrupt state
   - **Example:** Thread A starts move, Thread B deletes same node → corruption

4. **Global Collections** (Category 1)
   - `_IETnodes` ObservableCollection not thread-safe
   - Concurrent additions corrupt insertion order and fire events incorrectly

### Tests Useful for Thread-Safety Analysis

- Tests marked with "Thread-safety note" in comments identify specific concurrency vulnerabilities
- These tests can be adapted to create targeted concurrency tests similar to `BaseTypeThreadSafetyTests.cs`
- Current stubs focus on single-threaded correctness; concurrent stress tests would require:
  - `Parallel.For` wrappers
  - `Barrier` for synchronized starts
  - `ConcurrentBag<Exception>` for failure collection
  - `Assert.Inconclusive` when race conditions detected

---

## Relationship to Existing Tests

### Complements `BaseTypeTests.cs`
- `BaseTypeTests.cs`: Isolated unit tests for single methods (ItemMutator, ItemsMutator)
- `_OMTreeStabilityTests.cs`: Functional/integration tests for complex mutation sequences

### Complements `MoveTests.cs`
- `MoveTests.cs`: Move operation correctness and specific scenarios
- `_OMTreeStabilityTests.cs`: Move within broader mutation sequences, cross-tree scenarios

### Complements `BaseTypeThreadSafetyTests.cs`
- `BaseTypeThreadSafetyTests.cs`: Concurrent stress tests detecting race conditions
- `_OMTreeStabilityTests.cs`: Single-threaded correctness identifying vulnerable code regions

---

## Running Tests (Once Implemented)

### Run All Stability Tests
```powershell
dotnet test --filter "FullyQualifiedName~OMTreeStabilityTests"
```

### Run Specific Category
```powershell
# Complex additions
dotnet test --filter "FullyQualifiedName~ComplexAddition"

# Bulk deletions
dotnet test --filter "FullyQualifiedName~BulkDeletion"

# Same-tree moves
dotnet test --filter "FullyQualifiedName~SameTreeMove"

# Cross-tree operations
dotnet test --filter "FullyQualifiedName~CrossTreeMove"

# Circular references
dotnet test --filter "FullyQualifiedName~CircularReference"

# Mixed mutations
dotnet test --filter "FullyQualifiedName~MixedMutation"

# Stress tests
dotnet test --filter "FullyQualifiedName~StressTest"
```

### Run Single Test
```powershell
dotnet test --filter "FullyQualifiedName~BulkDeletion_RemoveSectionWithChildren_CascadesAllDescendants"
```

---

## Expected Outcomes

### When All Tests Pass
- ✅ Dictionary consistency maintained across all mutation scenarios
- ✅ No orphaned nodes or dangling references
- ✅ Parent-child relationships remain symmetric
- ✅ Tree navigation (sibling, parent, child) works correctly after mutations
- ✅ Cross-tree moves properly update both TopNode dictionary sets
- ✅ Circular reference attempts rejected safely
- ✅ No memory leaks from accumulated dictionary entries

### When Tests Fail
- 🔍 Identifies specific mutation patterns that corrupt tree state
- 🔍 Pinpoints dictionary inconsistencies (which dict, which operation)
- 🔍 Reveals gaps in ItemMutator/ItemsMutator logic
- 🔍 Exposes Move() validation weaknesses
- 🔍 Highlights need for additional RemoveRecursive() cleanup

---

## Next Steps

1. **Implement validation helpers** (`ValidateTreeIntegrity`, `CountReachableNodes`)
2. **Start with Phase 2 high-value tests** (cascading delete, cross-section move)
3. **Run tests frequently** during implementation to catch regressions early
4. **Use test failures** to identify and fix OM corruption bugs
5. **Remove leading underscores** from file and test names once implementation complete
6. **Consider creating additional targeted tests** if failures reveal new edge cases
7. **Document any discovered OM behavior** that differs from expected in test comments

---

## Maintenance Notes

- **File Naming:** Leading `_` indicates stub status; remove when implemented
- **Test Naming:** Test names use descriptive verbs and expected outcomes
- **Comment Quality:** Each stub has 6-12 step implementation guidance
- **Performance Guidelines:** Functional tests should run < 10s per repository guidelines
- **Validation Consistency:** All tests should call `ValidateTreeIntegrity()` at end

---

**Created:** During Features/Net11Upgrade branch - OM tree stability analysis  
**Related Files:**
- `SDC.Schema.Tests/Functional/_OMTreeStabilityTests.cs` (this test file)
- `SDC.Schema.Tests/Functional/MoveTests.cs` (reference)
- `SDC.Schema.Tests/OMTests/BaseTypeTests.cs` (unit test reference)
- `SDC.Schema.Tests/OMTests/BaseTypeThreadSafetyTests.cs` (concurrency reference)
- `SDC.Schema.Tests/Documentation/ThreadSafetyAnalysis.md` (thread-safety assessment)
- `SDC.Schema/Partial Classes/PartialClasses.cs` (ItemMutator, ItemsMutator implementation)
