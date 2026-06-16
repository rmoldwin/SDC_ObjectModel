using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.Tests.Helpers;
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

        #region Helper Methods

        // Validation helpers moved to TreeValidationHelper.cs for reuse across test classes

        /// <summary>
        /// Creates a realistic complex SDC form tree for testing.
        /// Structure: FormDesign → Body → 3 Sections → 10 Questions → ListItems/ResponseFields
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

                    // QuestionFill automatically adds a response field, so don't add another
                    var q6 = section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Name", "Full Name");

                    var q7 = section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Weight", "Weight (kg)");

                    var q8 = section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.LastVisit", "Last Visit Date");

                    // Section 3: Nested section
                    var section3 = form.Body.AddChildSection("S.Notes", "Additional Notes");
                    var nestedSection = section3.AddChildSection("S.Nested", "Clinical Observations");

                    var q9 = nestedSection.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Notes", "Observation Notes");

                    var q10 = nestedSection.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.FollowUp", "Follow-up Required");
                    q10.AddListItem("LI.Yes", "Yes");
                    q10.AddListItem("LI.No", "No");

                    return form;
                }

        #endregion

        #region Complex Addition Sequences

        // These tests verify dictionary integrity when adding nodes in various patterns

        [TestMethod()]
        public void ComplexAddition_BulkSiblingInsertion_MaintainsDictionaryConsistency()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.BulkSibling");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.BulkTest", "Bulk Test");

            int initialCount = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state before bulk additions");

            // Act: Add 50 list items
            var items = new List<ListItemType>();
            for (int i = 1; i <= 50; i++)
            {
                var item = question.AddListItem($"LI.Item{i}", $"Item {i}");
                items.Add(item);

                // Rationale: verify node count increases by exactly 1 after each addition
                int expectedCount = initialCount + i;
                TreeValidationHelper.AssertNodeCount(form, expectedCount, $"After adding item {i}");
            }

            // Assert: Comprehensive validation after all additions
            // Rationale: validates all 50 items are properly registered in dictionaries
            TreeValidationHelper.ValidateTreeIntegrity(form, "After all 50 bulk additions");

            // Rationale: verifies exact count of nodes added (50 list items)
            int finalExpectedCount = initialCount + 50;
            TreeValidationHelper.AssertNodeCount(form, finalExpectedCount, "Final node count");

            // Rationale: validates sibling navigation works across all 50 items (first to last)
            var firstItem = items[0];
            var lastItem = items[49];
            Assert.AreSame(lastItem, firstItem.GetNodeLastSib(), "First item should navigate to last sibling");
            Assert.AreSame(firstItem, lastItem.GetNodeFirstSib(), "Last item should navigate to first sibling");

            // Rationale: validates sequential sibling navigation through the entire list
            var current = firstItem;
            for (int i = 0; i < 49; i++)
            {
                var next = current.GetNodeNextSib();
                Assert.AreSame(items[i + 1], next, $"Sibling navigation from item {i} to {i + 1} failed");
                current = (ListItemType)next;
            }
        }

        [TestMethod()]
        public void ComplexAddition_DeeplyNestedChildSequence_MaintainsParentChain()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.DeepNest");
            form.AddBody();
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Build deeply nested structure (7 levels)
            // Level 1: Section
            var section = form.Body.AddChildSection("S.Deep", "Deep Section");
            // Rationale: verify parent chain after each level addition
            Assert.AreSame(form.Body, section.ParentNode, "Section parent should be Body");

            // Level 2: Question
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Deep", "Deep Question");
            Assert.AreSame(section, question.ParentNode, "Question parent should be Section");

            // Level 3: ListItem
            var listItem = question.AddListItem("LI.Deep", "Deep Item");
            Assert.AreSame(question, listItem.ParentNode, "ListItem parent should be Question");

            // Level 4: ListItemResponseField
            var listItemRespField = listItem.AddListItemResponseField();
            Assert.AreSame(listItem, listItemRespField.ParentNode, "ListItemResponseField parent should be ListItem");

            // Level 5: Response DataType
            var response = new DataTypes_DEType(listItemRespField);
            listItemRespField.Response = response;
            Assert.AreSame(listItemRespField, response.ParentNode, "Response parent should be ListItemResponseField");

            // Level 6: DataType (string)
            var dataType = new string_DEtype(response);
            response.DataTypeDE_Item = dataType;
            Assert.AreSame(response, dataType.ParentNode, "DataType parent should be Response");

            // Assert: Validate entire tree integrity
            // Rationale: validates all parent-child relationships are properly registered in dictionaries
            TreeValidationHelper.ValidateTreeIntegrity(form, "After deep nesting");

            // Rationale: walk back up parent chain from deepest node to TopNode
            BaseType current = dataType;
            var expectedParents = new BaseType[] { response, listItemRespField, listItem, question, section, form.Body, form };
            foreach (var expectedParent in expectedParents)
            {
                current = current.ParentNode;
                Assert.AreSame(expectedParent, current, $"Parent chain broken at {expectedParent?.GetType().Name ?? "null"}");
            }

            // Rationale: verify all nodes in chain share the same TopNode
            Assert.AreSame(form, dataType.TopNode, "Deepest node should reference form as TopNode");
            Assert.AreSame(form, response.TopNode, "Mid-level node should reference form as TopNode");
            Assert.AreSame(form, section.TopNode, "High-level node should reference form as TopNode");
        }

        [TestMethod()]
        public void ComplexAddition_InterleavedSectionAndQuestionAddition_PreservesTreeOrder()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Interleaved");
            form.AddBody();
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            var addedNodes = new List<IdentifiedExtensionType>();

            // Act: Interleaved pattern - Add S1, add Q1 to S1, add S2, add Q2 to S1, add Q3 to S2, add S3
            var s1 = form.Body.AddChildSection("S.1", "Section 1");
            addedNodes.Add(s1);

            var q1 = s1.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.1", "Question 1");
            addedNodes.Add(q1);

            var s2 = form.Body.AddChildSection("S.2", "Section 2");
            addedNodes.Add(s2);

            var q2 = s1.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.2", "Question 2");
            addedNodes.Add(q2);

            var q3 = s2.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.3", "Question 3");
            addedNodes.Add(q3);

            var s3 = form.Body.AddChildSection("S.3", "Section 3");
            addedNodes.Add(s3);

            // Assert: Validate tree integrity
            // Rationale: validates all nodes properly registered despite interleaved addition pattern
            TreeValidationHelper.ValidateTreeIntegrity(form, "After interleaved additions");

            // Rationale: verify section order in Body matches addition order (S1, S2, S3)
            var sections = form.Body.GetChildItemsNode().ChildItemsList;
            Assert.AreEqual(3, sections.Count, "Body should contain 3 sections");
            Assert.AreSame(s1, sections[0], "First section should be S1");
            Assert.AreSame(s2, sections[1], "Second section should be S2");
            Assert.AreSame(s3, sections[2], "Third section should be S3");

            // Rationale: verify question order within S1 matches addition order (Q1, Q2)
            var s1Questions = s1.GetChildItemsNode().ChildItemsList;
            Assert.AreEqual(2, s1Questions.Count, "S1 should contain 2 questions");
            Assert.AreSame(q1, s1Questions[0], "First question in S1 should be Q1");
            Assert.AreSame(q2, s1Questions[1], "Second question in S1 should be Q2");

            // Rationale: verify question order within S2 (Q3 only)
            var s2Questions = s2.GetChildItemsNode().ChildItemsList;
            Assert.AreEqual(1, s2Questions.Count, "S2 should contain 1 question");
            Assert.AreSame(q3, s2Questions[0], "First question in S2 should be Q3");

            // Rationale: verify IETnodes collection reflects global insertion order
            var ietNodes = form.IETnodes;
            Assert.IsTrue(ietNodes.Count >= 6, "IETnodes should contain at least 6 nodes");
            // Note: IETnodes may contain additional nodes (Body, etc.), so we verify order of our added nodes
            var ourNodesInOrder = ietNodes.Where(n => addedNodes.Contains(n)).ToList();
            Assert.AreEqual(6, ourNodesInOrder.Count, "All 6 added nodes should be in IETnodes");
            for (int i = 0; i < addedNodes.Count; i++)
            {
                Assert.AreSame(addedNodes[i], ourNodesInOrder[i], $"IETnodes order mismatch at position {i}");
            }
        }

        [TestMethod()]
        public void ComplexAddition_MultipleListFieldsWithSharedItems_DetectsIllegalSharing()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.SharedItem");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var q1 = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.1", "Question 1");
            var q2 = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.2", "Question 2");

            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

                // Act: Create ListItemType and add to Q1
                var sharedItem = q1.AddListItem("LI.Shared", "Shared Item");

                // Rationale: verify item is initially parented to Q1's List node (not directly to Q1)
                Assert.IsNotNull(sharedItem.ParentNode, "Item should have a parent after Q1 addition");
                Assert.IsNotNull(q1.ListField_Item?.List, "Q1 should have a List node");
                Assert.AreSame(q1.ListField_Item.List, sharedItem.ParentNode, "Item should be parented to Q1's List node");
                TreeValidationHelper.AssertNodeExists(sharedItem, "Item should exist in tree after Q1 addition");

                var q1ItemsBefore = q1.ListField_Item!.List!.Items;
                Assert.IsTrue(q1ItemsBefore.Contains(sharedItem), "Q1's List should contain the item before reassignment");

                // Attempt to add same instance to Q2's List - should trigger Move/ItemMutator logic
                var q2List = q2.ListField_Item!.List!;
                q2List.Items.Add(sharedItem);

                // Assert: Validate item moved from Q1's List to Q2's List
                // Rationale: verifies node cannot have two parents simultaneously - ItemMutator should move it
                Assert.AreSame(q2List, sharedItem.ParentNode, "Item should be reparented to Q2's List");

                // Rationale: verifies Q1's list no longer contains the moved item
                var q1ItemsAfter = q1.ListField_Item.List.Items;
                Assert.IsFalse(q1ItemsAfter.Contains(sharedItem), "Q1's List should no longer contain the item after move to Q2");

                // Rationale: verifies Q2's list contains the item
                var q2ItemsAfter = q2.ListField_Item.List.Items;
                Assert.IsTrue(q2ItemsAfter.Contains(sharedItem), "Q2's List should contain the item after reassignment");

                // Rationale: validates tree integrity after reassignment (no dictionary corruption)
                TreeValidationHelper.ValidateTreeIntegrity(form, "After item reassignment");
                TreeValidationHelper.AssertNodeExists(sharedItem, "Item should still exist in tree after reassignment");
            }

        [TestMethod()]
        public void ComplexAddition_RapidAddRemoveAddCycles_DetectsGUIDRecycling()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.AddRemoveCycle");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");

            int baselineCount = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial baseline");

            var allGuids = new List<Guid>();

            // Act: Perform 100 add/remove cycles
            for (int i = 1; i <= 100; i++)
            {
                // Capture count before adding
                int countBeforeAdd = TreeValidationHelper.CountReachableNodes(form);

                // Add new question
                var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, $"Q.Cycle{i}", $"Question {i}");
                allGuids.Add(question.ObjectGUID);

                // Rationale: verify node count increased after addition (may be more than +1 due to containers)
                int countAfterAdd = TreeValidationHelper.CountReachableNodes(form);
                int nodesAdded = countAfterAdd - countBeforeAdd;
                Assert.IsTrue(nodesAdded >= 1, $"Should have added at least 1 node, but added {nodesAdded}");
                TreeValidationHelper.AssertNodeExists(question, $"Question {i} should exist after addition");

                // Remove question
                bool removed = question.RemoveRecursive(false);
                Assert.IsTrue(removed, $"Question {i} removal should succeed");

                // Rationale: verify node count returns to baseline after removal
                TreeValidationHelper.AssertNodeCount(form, baselineCount, $"After removing question {i}");
                TreeValidationHelper.AssertNodeNotExists(question, $"Question {i} should not exist after removal");
            }

            // Assert: Validate GUID uniqueness across all cycles
            // Rationale: verifies no GUID recycling occurred during 100 add/remove cycles
            var uniqueGuids = new HashSet<Guid>(allGuids);
            Assert.AreEqual(100, uniqueGuids.Count, "All 100 GUIDs should be unique (no recycling)");

            // Rationale: validates final tree state is stable and consistent
            TreeValidationHelper.ValidateTreeIntegrity(form, "After 100 add/remove cycles");

            // Rationale: verifies node count returned to exact baseline (no memory leaks)
            TreeValidationHelper.AssertNodeCount(form, baselineCount, "Final node count should match baseline");
        }

        #endregion

        #region Bulk Deletion and Cascading Removes

        // These tests verify cascading deletion correctly updates all dictionaries

        [TestMethod()]
        public void BulkDeletion_RemoveSectionWithChildren_CascadesAllDescendants()
        {
                // Arrange
                var form = CreateComplexFormTree("FD.CascadeDelete");
                int initialCount = TreeValidationHelper.CountReachableNodes(form);
                TreeValidationHelper.ValidateTreeIntegrity(form, "Initial complex tree");

                // Get reference to Section 1 (Demographics - has 5 questions with list items)
                var section1 = form.Body.GetChildItemsNode().ChildItemsList[0] as SectionItemType;
                Assert.IsNotNull(section1, "Section 1 should exist");
                Assert.IsNotNull(section1.TopNode, "Section should have TopNode reference");

                // Count how many nodes section1 and its descendants represent by checking TopNode dictionary
                var section1Guid = section1.ObjectGUID;
                Assert.IsTrue(((ITopNode)form).Nodes.ContainsKey(section1Guid), "Section should be in TopNode dictionary");

                // Act: Remove section with all descendants
                int countBeforeRemove = TreeValidationHelper.CountReachableNodes(form);
                bool removed = section1.RemoveRecursive(false);
                Assert.IsTrue(removed, "Section removal should succeed");
                int countAfterRemove = TreeValidationHelper.CountReachableNodes(form);
                int nodesRemoved = countBeforeRemove - countAfterRemove;

                // Assert: Validate cascading deletion
                // Rationale: verifies nodes were removed (section + all descendants)
                Assert.IsTrue(nodesRemoved > 1, $"Should have removed section + descendants, but only removed {nodesRemoved} nodes");

                // Rationale: verifies removed section no longer exists in tree
                TreeValidationHelper.AssertNodeNotExists(section1, "Section should not exist after removal");

                // Rationale: validates remaining tree is consistent and corruption-free
                TreeValidationHelper.ValidateTreeIntegrity(form, "After cascading section deletion");

                // Rationale: verifies section is no longer in Body's child list
                var remainingSections = form.Body.GetChildItemsNode().ChildItemsList;
                Assert.IsFalse(remainingSections.Contains(section1), "Body should not contain removed section");
            }

        [TestMethod()]
        public void BulkDeletion_RemoveMiddleSiblings_PreservesFirstAndLastSiblingLinks()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.RemoveMiddle");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Test", "Test Question");

            // Create 10 list items
            var items = new List<ListItemType>();
            for (int i = 1; i <= 10; i++)
            {
                var item = question.AddListItem($"LI.Item{i}", $"Item {i}");
                items.Add(item);
            }

            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state with 10 items");

            // Act: Remove items 3-7 (indices 2-6, 5 items total)
            for (int i = 2; i <= 6; i++)
            {
                bool removed = items[i].RemoveRecursive(false);
                Assert.IsTrue(removed, $"Item {i + 1} removal should succeed");
            }

                // Assert: Validate sibling links after middle removal
                // Rationale: verifies sibling navigation skips removed nodes (item2 → item8)
                var item2 = items[1];
                var item8 = items[7];
                Assert.AreSame(item8, item2.GetNodeNextSib(), "Item 2's next sibling should be item 8");
                Assert.AreSame(item2, item8.GetNodePreviousSib(), "Item 8's previous sibling should be item 2");

                // Rationale: verifies removed items 3-7 no longer exist in tree
                for (int i = 2; i <= 6; i++)
                {
                    TreeValidationHelper.AssertNodeNotExists(items[i], $"Item {i + 1} should not exist after removal");
                }

                // Rationale: verifies parent's list contains exactly 5 remaining items
                var remainingItems = question.ListField_Item!.List!.Items;
                Assert.AreEqual(5, remainingItems.Count, "Question's List should have 5 remaining items");

                // Rationale: verifies list order is [item1, item2, item8, item9, item10]
                Assert.AreSame(items[0], remainingItems[0], "First item should be item 1");
                Assert.AreSame(items[1], remainingItems[1], "Second item should be item 2");
                Assert.AreSame(items[7], remainingItems[2], "Third item should be item 8");
                Assert.AreSame(items[8], remainingItems[3], "Fourth item should be item 9");
                Assert.AreSame(items[9], remainingItems[4], "Fifth item should be item 10");

                // Rationale: validates tree integrity after bulk middle deletion
                TreeValidationHelper.ValidateTreeIntegrity(form, "After removing middle siblings");
            }

        [TestMethod()]
        public void BulkDeletion_RemoveAllChildrenIteratively_LeavesParentNodeClean()
        {
            // Arrange
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.IterativeRemove");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");

            // Create 5 questions
            var questions = new List<QuestionItemType>();
            for (int i = 1; i <= 5; i++)
            {
                var q = section.AddChildQuestion(QuestionEnum.QuestionSingle, $"Q.{i}", $"Question {i}");
                questions.Add(q);
            }

            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state with 5 questions");

            var childGuids = questions.Select(q => q.ObjectGUID).ToList();
            int expectedRemainingCount = 5;

            // Act: Remove each child iteratively
            foreach (var question in questions)
            {
                bool removed = question.RemoveRecursive(false);
                Assert.IsTrue(removed, $"Question {question.name} removal should succeed");

                // Rationale: verify child no longer exists in tree after each removal
                TreeValidationHelper.AssertNodeNotExists(question, $"Question {question.name} should not exist after removal");

                // Rationale: verify remaining children count decreases by 1
                expectedRemainingCount--;
                var remainingChildren = section.GetChildItemsNode().ChildItemsList;
                Assert.AreEqual(expectedRemainingCount, remainingChildren.Count, 
                    $"Section should have {expectedRemainingCount} remaining children");
            }

            // Assert: Validate parent is clean after all removals
            // Rationale: verifies parent's child list is empty after all removals
            var finalChildren = section.GetChildItemsNode().ChildItemsList;
            Assert.AreEqual(0, finalChildren.Count, "Section should have no children after all removals");

            // Rationale: validates tree integrity with empty section
            TreeValidationHelper.ValidateTreeIntegrity(form, "After removing all children");

            // Rationale: verifies all child GUIDs were removed from tree
            foreach (var guid in childGuids)
            {
                var topNode = form as ITopNode;
                Assert.IsFalse(topNode.Nodes.ContainsKey(guid), $"GUID {guid} should not exist in Nodes dictionary");
            }
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
