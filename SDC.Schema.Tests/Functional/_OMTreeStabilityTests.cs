using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDC.Schema.Tests.Functional
{
    /// <summary>
    /// Functional tests for SDC object model tree stability under complex mutation scenarios.
    /// These tests exercise realistic multi-level SDC trees to detect:
    /// - Dictionary corruption during node additions, deletions, and moves
    /// - Inconsistencies in TopNode dictionaries (_Nodes, _ParentNodes, _ChildNodes, _IETnodes)
    /// - Violations of parent-child relationship invariants
    /// - Vulnerable code regions for thread-safety concerns
    /// 
    /// Unlike unit tests that isolate single methods, these functional tests perform
    /// sequences of mutations to expose hidden invariant violations.
    /// </summary>
    [TestClass()]
    public class OMTreeStabilityTests
    {
        // Test stubs follow repository convention: leading '_' indicates stub file
        // Remove '_' from filename and test names once implementation is complete

        #region Helper Methods

        /// <summary>
        /// Creates a realistic complex SDC form tree for testing.
        /// Structure: FormDesign → Body → Header/Footer → Multiple Sections → Questions with responses
        /// Returns a tree with:
        /// - 1 FormDesignType (root)
        /// - 1 Body with Header (2 display items) and Footer (1 display item)
        /// - 3 Sections in Body
        ///   - Section 1: 5 questions (3 single-choice, 2 multi-choice) with list items
        ///   - Section 2: 3 questions (1 text, 1 numeric, 1 datetime) with response fields
        ///   - Section 3: nested section with 2 questions
        /// - Total ~40+ nodes for realistic stress testing
        /// </summary>
        private static FormDesignType CreateComplexFormTree(string formId)
        {
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, formId);
            form.AddBody();

            // Section 1: Single/multi-choice questions with list items
            var section1 = form.Body.AddChildSection("S.Demographics", "Demographics");
            var q1 = section1.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Gender", "Gender");
            q1.AddListItem("LI.Male", "Male");
            q1.AddListItem("LI.Female", "Female");
            q1.AddListItem("LI.Other", "Other");

            var q2 = section1.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.AgeRange", "Age Range");
            q2.AddListItem("LI.Age1", "Under 18");
            q2.AddListItem("LI.Age2", "18-30");
            q2.AddListItem("LI.Age3", "31-50");
            q2.AddListItem("LI.Age4", "51-65");
            q2.AddListItem("LI.Age5", "Over 65");

            var q3 = section1.AddChildQuestion(QuestionEnum.QuestionMultiple, "Q.Symptoms", "Symptoms");
            q3.AddListItem("LI.Fever", "Fever");
            q3.AddListItem("LI.Cough", "Cough");
            q3.AddListItem("LI.Fatigue", "Fatigue");
            q3.AddListItem("LI.Headache", "Headache");

            var q4 = section1.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Severity", "Severity");
            q4.AddListItem("LI.Mild", "Mild");
            q4.AddListItem("LI.Moderate", "Moderate");
            q4.AddListItem("LI.Severe", "Severe");

            var q5 = section1.AddChildQuestion(QuestionEnum.QuestionMultiple, "Q.Medications", "Current Medications");
            q5.AddListItem("LI.Med1", "Aspirin");
            q5.AddListItem("LI.Med2", "Ibuprofen");
            q5.AddListItem("LI.Med3", "Other");

            // Section 2: Text/numeric/datetime response fields
            var section2 = form.Body.AddChildSection("S.Details", "Additional Details");

            var q6 = section2.AddChildQuestion(QuestionEnum.QuestionRaw, "Q.Name", "Full Name");
            q6.AddQuestionResponseField(out var deType6);

            var q7 = section2.AddChildQuestion(QuestionEnum.QuestionRaw, "Q.Weight", "Weight (kg)");
            var rf7 = q7.AddQuestionResponseField(out var deType7, ItemChoiceType.integer);

            var q8 = section2.AddChildQuestion(QuestionEnum.QuestionRaw, "Q.LastVisit", "Last Visit Date");
            var rf8 = q8.AddQuestionResponseField(out var deType8, ItemChoiceType.date);

            // Section 3: Nested section
            var section3 = form.Body.AddChildSection("S.Notes", "Additional Notes");
            var nestedSection = section3.AddChildSection("S.Nested", "Clinical Observations");

            var q9 = nestedSection.AddChildQuestion(QuestionEnum.QuestionRaw, "Q.Notes", "Observation Notes");
            q9.AddQuestionResponseField(out var deType9);

            var q10 = nestedSection.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.FollowUp", "Follow-up Required");
            q10.AddListItem("LI.Yes", "Yes");
            q10.AddListItem("LI.No", "No");

            return form;
        }

        /// <summary>
        /// Validates that all TopNode dictionaries are consistent and contain no corruption.
        /// Checks:
        /// - All nodes in tree are registered in _Nodes
        /// - Every node's ParentNode matches _ParentNodes dictionary
        /// - Child nodes in _ChildNodes match actual object tree structure
        /// - No orphaned nodes (nodes in dictionaries but not reachable from TopNode)
        /// - No dangling references (references to nodes not in _Nodes)
        /// - GUID uniqueness across all nodes
        /// </summary>
        private static void ValidateTreeIntegrity(BaseType topNode, string contextMessage = "")
        {
            // Stub: Comprehensive validation implementation
            // When implemented, should:
            // 1. Get ITopNode interface and access all dictionaries
            // 2. Traverse tree from topNode and collect all reachable nodes
            // 3. Compare reachable count vs dictionary counts
            // 4. For each reachable node:
            //    - Assert node.ObjectGUID exists in _Nodes
            //    - Assert _Nodes[guid] == node (same reference)
            //    - Assert node.ParentNode matches _ParentNodes[guid]
            //    - If node has children, assert they exist in _ChildNodes[guid]
            // 5. Check for orphaned entries (in dictionaries but not reachable)
            // 6. Validate GUID uniqueness (no duplicates)
            // 7. For IET nodes, validate _IETnodes collection consistency
            throw new NotImplementedException($"ValidateTreeIntegrity stub: {contextMessage}");
        }

        /// <summary>
        /// Validates parent-child relationship symmetry for a specific node.
        /// Ensures the node appears in its parent's child collection and vice versa.
        /// </summary>
        private static void ValidateParentChildSymmetry(BaseType node)
        {
            // Stub: Bidirectional link validation
            // When implemented, should:
            // 1. If node.ParentNode != null:
            //    - Get parent's _ChildNodes entry
            //    - Assert node appears in parent's child list
            // 2. Get node's _ChildNodes entry (if it has children)
            // 3. For each child, assert child.ParentNode == node
            throw new NotImplementedException("ValidateParentChildSymmetry stub");
        }

        /// <summary>
        /// Counts all nodes reachable by traversing the tree from topNode.
        /// Used to detect orphaned nodes when compared to dictionary counts.
        /// </summary>
        private static int CountReachableNodes(BaseType topNode)
        {
            // Stub: Tree traversal counter
            // When implemented, should:
            // 1. Use depth-first or breadth-first traversal
            // 2. Track visited nodes to avoid cycles
            // 3. Count each unique node once
            // 4. Return total count
            throw new NotImplementedException("CountReachableNodes stub");
        }

        #endregion

        #region Complex Addition Sequences

        // These tests verify dictionary integrity when adding nodes in various patterns

        [TestMethod()]
        public void _ComplexAddition_BulkSiblingInsertion_MaintainsDictionaryConsistency()
        {
            // Test: Add 50 list items to a single question
            // Validates: _ChildNodes list maintains correct order and count
            // Validates: All 50 items registered in _Nodes and _IETnodes
            // Validates: Sibling navigation (GetNodeNextSib) works across all 50 items
            // Thread-safety note: Concurrent bulk additions would corrupt _ChildNodes List<>
            // Implementation should:
            // 1. Create form with question
            // 2. Add 50 list items in a loop
            // 3. After each addition, verify node count increases by 1
            // 4. After all additions, call ValidateTreeIntegrity()
            // 5. Verify list order matches insertion order
            // 6. Test sibling navigation from first to last item
        }

        [TestMethod()]
        public void _ComplexAddition_DeeplyNestedChildSequence_MaintainsParentChain()
        {
            // Test: Create deeply nested structure (6+ levels)
            // Structure: Section → Question → ListField → List → ListItem → Response → DataType
            // Validates: Each level's ParentNode correctly points to parent
            // Validates: _ParentNodes dictionary matches actual parent chain
            // Implementation should:
            // 1. Build nested structure level by level
            // 2. At each level, verify node.ParentNode == expected parent
            // 3. Verify _ParentNodes[node.ObjectGUID] == expected parent
            // 4. Walk back up parent chain to TopNode
            // 5. Call ValidateTreeIntegrity() at end
        }

        [TestMethod()]
        public void _ComplexAddition_InterleavedSectionAndQuestionAddition_PreservesTreeOrder()
        {
            // Test: Add sections and questions in interleaved pattern
            // Pattern: Add S1, add Q1 to S1, add S2, add Q2 to S1, add Q3 to S2, add S3
            // Validates: Section order maintained in Body.ChildItems
            // Validates: Question order maintained within each section
            // Validates: _IETnodes maintains global insertion order
            // Implementation should:
            // 1. Create form with body
            // 2. Follow interleaved addition pattern
            // 3. After each add, capture _IETnodes order
            // 4. Verify _IETnodes reflects true insertion sequence
            // 5. Verify section order in Body matches expected
            // 6. Verify question order within each section
        }

        [TestMethod()]
        public void _ComplexAddition_MultipleListFieldsWithSharedItems_DetectsIllegalSharing()
        {
            // Test: Attempt to add same ListItemType instance to two different questions
            // Expected: Second assignment should trigger ItemMutator/Move logic
            // Validates: Node cannot have two parents simultaneously
            // Validates: First parent's list loses the node when reassigned
            // Implementation should:
            // 1. Create form with two questions (Q1, Q2)
            // 2. Create ListItemType and add to Q1
            // 3. Attempt to add same instance to Q2
            // 4. Verify item moves from Q1 to Q2 (or is rejected)
            // 5. Verify _ParentNodes shows only one parent (Q2)
            // 6. Verify Q1's list no longer contains the item
        }

        [TestMethod()]
        public void _ComplexAddition_RapidAddRemoveAddCycles_DetectsGUIDRecycling()
        {
            // Test: Add node, remove, add new node with same ID pattern, repeat 100 times
            // Validates: GUID uniqueness maintained across add/remove cycles
            // Validates: No dictionary entry leaks after remove
            // Validates: _Nodes count returns to baseline after each remove
            // Implementation should:
            // 1. Capture initial node count
            // 2. Loop 100 times:
            //    - Add new question
            //    - Verify node count increased by 1
            //    - Remove question
            //    - Verify node count returned to baseline
            // 3. Check for GUID collisions (all GUIDs should be unique)
            // 4. Verify no memory leaks (_Nodes size stable)
        }

        #endregion

        #region Bulk Deletion and Cascading Removes

        // These tests verify cascading deletion correctly updates all dictionaries

        [TestMethod()]
        public void _BulkDeletion_RemoveSectionWithChildren_CascadesAllDescendants()
        {
            // Test: Remove section containing 10 questions with responses and list items
            // Validates: All descendants removed from _Nodes, _ParentNodes, _ChildNodes, _IETnodes
            // Expected: ~50+ nodes removed in single RemoveRecursive call
            // Implementation should:
            // 1. Create complex form (use CreateComplexFormTree)
            // 2. Capture initial _Nodes count
            // 3. Get reference to Section 1 (has 5 questions with list items)
            // 4. Count descendants of Section 1
            // 5. Call section.RemoveRecursive(false)
            // 6. Verify _Nodes count decreased by exact descendant count + 1 (section itself)
            // 7. Verify section and all descendants no longer in _Nodes
            // 8. Verify _ParentNodes cleaned up for all removed nodes
            // 9. Call ValidateTreeIntegrity() on remaining tree
        }

        [TestMethod()]
        public void _BulkDeletion_RemoveMiddleSiblings_PreservesFirstAndLastSiblingLinks()
        {
            // Test: Remove items 3-7 from 10-item list
            // Validates: Sibling navigation skips removed nodes correctly
            // Validates: Items 1-2 and 8-10 remain with correct sibling links
            // Implementation should:
            // 1. Create question with 10 list items
            // 2. Get references to items 1, 2, 3, 7, 8, 9, 10
            // 3. Remove items 3-7
            // 4. Verify item2.GetNodeNextSib() == item8
            // 5. Verify item8.GetNodePreviousSib() == item2
            // 6. Verify items 3-7 no longer in _Nodes
            // 7. Verify parent's _ChildNodes list contains exactly 5 items
            // 8. Verify list order: [1,2,8,9,10]
        }

        [TestMethod()]
        public void _BulkDeletion_RemoveAllChildrenIteratively_LeavesParentNodeClean()
        {
            // Test: Remove all children one-by-one from a section
            // Validates: Parent's _ChildNodes entry is cleaned up
            // Validates: No orphaned child references remain
            // Implementation should:
            // 1. Create section with 5 questions
            // 2. Capture child GUIDs
            // 3. Remove each child iteratively (forward iteration)
            // 4. After each removal:
            //    - Verify child no longer in _Nodes
            //    - Verify remaining children count correct
            // 5. After all removals:
            //    - Verify parent's _ChildNodes entry is null or empty
            //    - Verify parent's GetChildItemsNode().ChildItemsList is empty
            //    - Verify all child GUIDs removed from _ParentNodes
            // 6. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _BulkDeletion_RemoveNodeThenAttemptAccessFromSibling_HandlesGracefully()
        {
            // Test: Remove middle sibling, then navigate from previous sibling
            // Validates: Navigation methods skip removed nodes gracefully
            // Validates: No exceptions thrown when accessing removed node's former position
            // Implementation should:
            // 1. Create question with 5 list items (A, B, C, D, E)
            // 2. Get references to B, C, D
            // 3. Remove C
            // 4. Call B.GetNodeNextSib() → should return D (skipping removed C)
            // 5. Call D.GetNodePreviousSib() → should return B
            // 6. Verify C.ParentNode == null
            // 7. Attempt to call C.GetNodeNextSib() → should handle gracefully (return null or throw expected exception)
            // 8. Verify list order in parent: [A, B, D, E]
        }

        #endregion

        #region Multi-Hop Same-Tree Moves

        // These tests verify complex move operations within a single TopNode tree

        [TestMethod()]
        public void _SameTreeMove_ReorderListItemsBackAndForth_MaintainsListIntegrity()
        {
            // Test: Move list item from position 2 to 8, then back to 2
            // Validates: List order updated correctly after each move
            // Validates: Sibling links remain consistent
            // Validates: _ChildNodes list reflects actual positions
            // Implementation should:
            // 1. Create question with 10 list items
            // 2. Get reference to item at position 2
            // 3. Capture item's GUID and identity
            // 4. Move item to position 8
            // 5. Verify item appears at position 8 in parent's ChildItemsList
            // 6. Verify sibling links: item[7].GetNodeNextSib() == movedItem
            // 7. Move item back to position 2
            // 8. Verify item restored to position 2
            // 9. Verify no dictionary corruption
            // 10. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _SameTreeMove_MoveQuestionBetweenSections_UpdatesParentReferences()
        {
            // Test: Move QuestionItemType from Section A to Section B
            // Validates: _ParentNodes dictionary updated to new parent
            // Validates: Question appears in Section B's ChildItemsList
            // Validates: Question removed from Section A's ChildItemsList
            // Implementation should:
            // 1. Create form with Section A (3 questions) and Section B (2 questions)
            // 2. Get reference to Question 2 from Section A
            // 3. Capture question's GUID
            // 4. Move question to Section B at position 1
            // 5. Verify question.ParentNode == Section B's ChildItems
            // 6. Verify _ParentNodes[questionGUID] == Section B's ChildItems
            // 7. Verify Section A's ChildItemsList count == 2
            // 8. Verify Section B's ChildItemsList count == 3
            // 9. Verify question's position in Section B == 1
            // 10. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _SameTreeMove_MoveEntireSectionWithinBody_PreservesDescendantTree()
        {
            // Test: Move section from position 0 to end of body
            // Validates: All descendant questions/responses remain attached
            // Validates: Descendant parent chains still point correctly through moved section
            // Implementation should:
            // 1. Create form with 3 sections, each with 3 questions
            // 2. Get reference to Section 0 and count its descendants
            // 3. Capture descendant GUIDs
            // 4. Move Section 0 to position 2 (end)
            // 5. Verify section appears at position 2 in Body's ChildItems
            // 6. Verify all descendants still have correct ParentNode chain to moved section
            // 7. Walk descendant tree and verify all nodes still reachable
            // 8. Verify _Nodes count unchanged (no nodes lost or duplicated)
            // 9. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _SameTreeMove_SwapTwoSectionsPositions_HandlesSimultaneousReordering()
        {
            // Test: Swap positions of Section 1 (pos 0) and Section 2 (pos 1)
            // Validates: Both sections maintain their descendants
            // Validates: Both sections' dictionary entries remain valid
            // Validates: Position swapping doesn't corrupt parent's child list
            // Implementation should:
            // 1. Create form with 3 sections
            // 2. Get references to Section 1 (pos 0) and Section 2 (pos 1)
            // 3. Capture descendant counts for both
            // 4. Move Section 1 to temp position (end)
            // 5. Move Section 2 to position 0
            // 6. Move Section 1 to position 1
            // 7. Verify final order: [original S2, original S1, original S3]
            // 8. Verify both moved sections retain all descendants
            // 9. Verify _Nodes count unchanged
            // 10. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _SameTreeMove_MoveNodeToBeChildOfItsOwnDescendant_PreventsCircularReference()
        {
            // Test: Attempt to move Section to be child of one of its own Questions
            // Expected: Move() should detect circular reference and return false
            // Validates: Tree structure unchanged after rejected move
            // Validates: No dictionary corruption from failed move attempt
            // Implementation should:
            // 1. Create form with Section containing Question
            // 2. Capture initial tree state (node count, parent relationships)
            // 3. Attempt: section.Move(question.ChildItems, 0)
            // 4. Verify Move() returns false
            // 5. Verify section.ParentNode unchanged (still Body's ChildItems)
            // 6. Verify question still child of section
            // 7. Verify _Nodes count unchanged
            // 8. Call ValidateTreeIntegrity()
            // Thread-safety note: Concurrent move attempts could race on parent validation check
        }

        #endregion

        #region Cross-Tree Grafting and Orphaning

        // These tests verify moving/copying nodes between different TopNode trees

        [TestMethod()]
        public void _CrossTreeMove_GraftQuestionFromFormAToFormB_UpdatesAllDictionaries()
        {
            // Test: Move question from Form A's Section 1 to Form B's Section 1
            // Validates: Question removed from Form A's dictionaries (_Nodes, _ParentNodes, etc.)
            // Validates: Question added to Form B's dictionaries
            // Validates: Question's TopNode reference updated to Form B
            // Implementation should:
            // 1. Create Form A with Section containing 3 questions
            // 2. Create Form B with Section containing 2 questions
            // 3. Get reference to Question 2 from Form A
            // 4. Capture Form A's initial _Nodes count
            // 5. Capture Form B's initial _Nodes count
            // 6. Move Question 2 to Form B's Section
            // 7. Verify Form A's _Nodes count decreased by 1
            // 8. Verify Form B's _Nodes count increased by 1
            // 9. Verify question.TopNode == Form B
            // 10. Verify Form A's dictionaries no longer contain question's GUID
            // 11. Verify Form B's dictionaries contain question's GUID
            // 12. Call ValidateTreeIntegrity() on both forms
            // Thread-safety note: Cross-tree moves touch two TopNode dictionary sets
        }

        [TestMethod()]
        public void _CrossTreeMove_GraftSectionWithDescendants_MigratesEntireSubtree()
        {
            // Test: Move section with 5 questions and 15 list items from Form A to Form B
            // Validates: All ~25+ descendants switch TopNode references
            // Validates: All descendants removed from Form A's dictionaries
            // Validates: All descendants added to Form B's dictionaries
            // Implementation should:
            // 1. Create Form A with Section containing 5 questions (each with 3 list items)
            // 2. Create Form B with empty body
            // 3. Count descendants of section to be moved (~25 nodes)
            // 4. Capture Form A's _Nodes count
            // 5. Capture Form B's _Nodes count
            // 6. Move section from Form A to Form B
            // 7. Verify Form A's _Nodes count decreased by descendant count + 1
            // 8. Verify Form B's _Nodes count increased by descendant count + 1
            // 9. Walk moved subtree and verify all nodes have TopNode == Form B
            // 10. Call ValidateTreeIntegrity() on both forms
        }

        [TestMethod()]
        public void _CrossTreeMove_MoveNodeBackToOriginalTree_RestoresOriginalState()
        {
            // Test: Move question A→B→A, verify final state matches initial
            // Validates: Bidirectional cross-tree move consistency
            // Validates: No dictionary entry leaks after round-trip move
            // Implementation should:
            // 1. Create Form A and Form B
            // 2. Add question to Form A's Section 1 at position 1
            // 3. Capture Form A's _Nodes count and question's GUID
            // 4. Move question to Form B
            // 5. Verify question in Form B's dictionaries
            // 6. Move question back to Form A at original position
            // 7. Verify question.ParentNode restored to Form A's Section 1
            // 8. Verify Form A's _Nodes count matches initial count
            // 9. Verify Form B's _Nodes count returned to pre-graft baseline
            // 10. Verify question's position in Section 1 == original position
            // 11. Call ValidateTreeIntegrity() on both forms
        }

        [TestMethod()]
        public void _CrossTreeOrphan_CreateNodeWithoutParent_LeavesOrphanedUntilAttached()
        {
            // Test: Create QuestionItemType(null), verify orphan state, then attach
            // Validates: Orphaned node not in any TopNode dictionaries
            // Validates: Upon attachment, node enters parent's TopNode dictionaries
            // Implementation should:
            // 1. Create Form A
            // 2. Create QuestionItemType with parent=null (orphaned)
            // 3. Verify question.TopNode == null
            // 4. Verify question.ObjectGUID not in any form's _Nodes
            // 5. Move/attach orphaned question to Form A's Section
            // 6. Verify question.TopNode == Form A
            // 7. Verify question.ObjectGUID now in Form A's _Nodes
            // 8. Verify question.ParentNode == Form A's Section ChildItems
            // 9. Call ValidateTreeIntegrity() on Form A
            // Note: Tests late-binding attachment pattern
        }

        #endregion

        #region Circular Reference Attempts

        // These tests verify the OM prevents creation of circular parent-child relationships

        [TestMethod()]
        public void _CircularReference_MoveNodeToOwnChild_RejectsOperation()
        {
            // Test: Section attempts to move into its own child question's ChildItems
            // Expected: Move() detects circular reference and returns false
            // Validates: No tree corruption occurs from rejected move
            // Validates: Both section and question remain in original positions
            // Implementation should:
            // 1. Create form with Section containing Question
            // 2. Capture initial parent relationships
            // 3. Attempt: section.Move(question.ChildItems, 0)
            // 4. Verify Move() returns false
            // 5. Verify section.ParentNode unchanged (still Body's ChildItems)
            // 6. Verify question.ParentNode unchanged (still Section's ChildItems)
            // 7. Verify _Nodes count unchanged
            // 8. Verify no orphaned nodes created
            // 9. Call ValidateTreeIntegrity()
            // Note: Tests ancestor-chain validation in Move()
        }

        [TestMethod()]
        public void _CircularReference_MoveNodeToDistantDescendant_RejectsOperation()
        {
            // Test: Section attempts to move into deeply nested descendant (4 levels down)
            // Structure: Section → Question → ResponseField → Response → DataType
            // Expected: Move() walks ancestor chain and detects circular reference
            // Implementation should:
            // 1. Create Section with nested structure (4+ levels)
            // 2. Get reference to leaf node (DataType)
            // 3. Attempt: section.Move(leafNode, 0)
            // 4. Verify Move() returns false
            // 5. Verify section's position unchanged
            // 6. Verify entire descendant chain intact
            // 7. Walk from leaf back to section and verify parent chain
            // 8. Call ValidateTreeIntegrity()
            // Note: Tests deep ancestor-chain validation
        }

        [TestMethod()]
        public void _CircularReference_SwapParentAndChild_RejectsBothOperations()
        {
            // Test: Attempt to make parent the child of its own child (bidirectional)
            // Try both: parent.Move(child) AND child.Move(parent.Parent) with position swap
            // Expected: Both operations should fail safely
            // Implementation should:
            // 1. Create Section (parent) containing Question (child)
            // 2. Capture initial positions
            // 3. Attempt: parent.Move(child, 0)
            // 4. Verify operation rejected (returns false)
            // 5. Verify tree unchanged
            // 6. Attempt: child.Move(parent.ParentNode, parent's position)
            //    (This would effectively put child "above" parent)
            // 7. If allowed, verify parent becomes child's sibling (not a circular reference)
            // 8. Call ValidateTreeIntegrity()
            // Note: Tests position-swap vs true circular reference distinction
        }

        #endregion

        #region Mixed Mutation Sequences

        // These tests verify complex sequences of adds, moves, and deletes

        [TestMethod()]
        public void _MixedMutation_AddMoveMoveDeleteSequence_MaintainsConsistency()
        {
            // Test: Add section, move to position 2, move to position 0, delete
            // Validates: Tree state returns to pre-add baseline after delete
            // Validates: No dictionary entry leaks from intermediate moves
            // Implementation should:
            // 1. Create form with 3 existing sections
            // 2. Capture initial _Nodes count
            // 3. Add new Section 4 (initially at end, position 3)
            // 4. Verify _Nodes count increased by 1
            // 5. Move Section 4 to position 2
            // 6. Verify section appears at position 2
            // 7. Move Section 4 to position 0
            // 8. Verify section appears at position 0
            // 9. Delete Section 4
            // 10. Verify _Nodes count returned to initial count
            // 11. Verify original 3 sections remain in correct order
            // 12. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _MixedMutation_BulkAddThenSelectiveDelete_MaintainsRemainingNodes()
        {
            // Test: Add 20 questions to section, delete even-numbered ones
            // Validates: Odd-numbered questions maintain correct order and references
            // Validates: _Nodes count matches expected (section + 10 remaining questions)
            // Implementation should:
            // 1. Create form with empty section
            // 2. Add 20 questions (Q0 through Q19)
            // 3. Capture references to all questions
            // 4. Delete even-numbered questions (Q0, Q2, Q4, ... Q18) - 10 deletions
            // 5. Verify section's ChildItemsList contains exactly 10 questions
            // 6. Verify remaining questions are Q1, Q3, Q5, ... Q19
            // 7. Verify remaining questions maintain their identity (same GUID)
            // 8. Verify sibling navigation works correctly across remaining questions
            // 9. Verify _Nodes count reflects 10 remaining questions + section
            // 10. Call ValidateTreeIntegrity()
        }

        [TestMethod()]
        public void _MixedMutation_MoveMultipleNodesSequentiallyToSameTarget_PreservesOrder()
        {
            // Test: Move questions 1,2,3 from Section A to Section B in sequence
            // Validates: Questions appear in Section B in order [1,2,3]
            // Validates: Section A retains remaining questions in correct order
            // Implementation should:
            // 1. Create Section A with 5 questions (Q1-Q5)
            // 2. Create Section B with 2 questions (QB1, QB2)
            // 3. Move Q1 from Section A to Section B at end
            // 4. Move Q2 from Section A to Section B at end
            // 5. Move Q3 from Section A to Section B at end
            // 6. Verify Section B's ChildItemsList order: [QB1, QB2, Q1, Q2, Q3]
            // 7. Verify Section A's ChildItemsList order: [Q4, Q5]
            // 8. Verify all questions have correct ParentNode references
            // 9. Call ValidateTreeIntegrity()
            // Note: Tests sequential Move() calls to same target
        }

        [TestMethod()]
        public void _MixedMutation_DeleteParentDuringChildEnumeration_HandlesGracefully()
        {
            // Test: While iterating section's children, delete parent section
            // Expected: Should handle without collection-modified exceptions
            // Validates: ItemsMutator snapshot logic prevents enumeration errors
            // Implementation should:
            // 1. Create section with 5 questions
            // 2. Get reference to section and its ChildItemsList
            // 3. Start enumerating ChildItemsList
            // 4. During enumeration, call section.RemoveRecursive(false)
            // 5. Verify enumeration completes without exception
            //    (Tests whether snapshot was used vs live collection)
            // 6. After enumeration, verify section and children removed from _Nodes
            // 7. Call ValidateTreeIntegrity() on remaining tree
            // Thread-safety note: This tests single-threaded snapshot protection;
            // concurrent enumeration would still fail
        }

        [TestMethod()]
        public void _MixedMutation_ReplaceListPropertyMultipleTimes_ClearsOldReferences()
        {
            // Test: Replace section's ChildItemsList three times with different lists
            // Validates: Old list items are detached after each replacement
            // Validates: New list items are attached correctly
            // Validates: No orphaned nodes accumulate in dictionaries
            // Implementation should:
            // 1. Create section with 3 initial questions (List1)
            // 2. Capture initial _Nodes count
            // 3. Create List2 with 4 new questions
            // 4. Replace: section.ChildItems.ChildItemsList = List2
            // 5. Verify List1 items detached (ParentNode == null)
            // 6. Verify List2 items attached (ParentNode == section.ChildItems)
            // 7. Create List3 with 2 new questions
            // 8. Replace: section.ChildItems.ChildItemsList = List3
            // 9. Verify List2 items detached
            // 10. Verify List3 items attached
            // 11. Verify _Nodes contains only: section + List3 items (no leaks from List1/List2)
            // 12. Call ValidateTreeIntegrity()
            // Thread-safety note: This exercises ItemsMutator snapshot logic
        }

        #endregion

        #region Dictionary Consistency Stress Tests

        [TestMethod()]
        public void _StressTest_RapidMutationsCycled100Times_MaintainsDictionaryIntegrity()
        {
            // Test: Perform 100 cycles of complex mutation sequences
            // Each cycle: add 5 nodes → move 3 → delete 2 → move 1 back
            // Validates: Tree integrity maintained across all cycles
            // Validates: No gradual dictionary corruption or memory leaks
            // Implementation should:
            // 1. Create form with initial structure (3 sections, 5 questions)
            // 2. Capture baseline _Nodes count
            // 3. For each of 100 cycles:
            //    a. Add 5 new list items to Question 1
            //    b. Move 3 of them to Question 2
            //    c. Delete 2 of the moved items
            //    d. Move 1 remaining item back to Question 1
            //    e. Verify _Nodes count = baseline + (cycle * 2) [net +2 per cycle]
            // 4. After all cycles:
            //    - Verify final _Nodes count = baseline + 200
            //    - Call ValidateTreeIntegrity()
            //    - Verify no orphaned nodes in dictionaries
            //    - Verify all reachable nodes have valid parent chains
            // 5. Measure and log execution time (should complete < 10 seconds per test guidelines)
            // Performance note: If test exceeds 10s, reduce cycle count or node counts
            // Thread-safety note: Identifies gradual corruption patterns that concurrent access would amplify
        }

        #endregion
    }
}
