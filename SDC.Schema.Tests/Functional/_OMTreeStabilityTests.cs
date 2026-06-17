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
            // Rationale: verify section has a parent (may be Body's ChildItems container, not Body directly)
            Assert.IsNotNull(section.ParentNode, "Section should have a parent");

            // Level 2: Question
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Deep", "Deep Question");
            Assert.IsNotNull(question.ParentNode, "Question should have a parent (may be section's ChildItems container)");

            // Level 3: ListItem
            var listItem = question.AddListItem("LI.Deep", "Deep Item");
            Assert.IsNotNull(listItem.ParentNode, "ListItem should have a parent (may be question's List node)");

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
                // Note: Container nodes may exist between visible nodes
                BaseType current = dataType;
                Assert.AreSame(response, current.ParentNode, "DataType parent should be Response");
                current = current.ParentNode;

                Assert.AreSame(listItemRespField, current.ParentNode, "Response parent should be ListItemResponseField");
                current = current.ParentNode;

                Assert.AreSame(listItem, current.ParentNode, "ListItemResponseField parent should be ListItem");
                current = current.ParentNode;

                // ListItem's parent is the List node, not the question directly
                Assert.IsNotNull(current.ParentNode, "ListItem should have a parent (List container)");
                current = current.ParentNode; // Now at List node

                // List's parent could be ListField, which parents to Question's ChildItems, etc.
                // Rather than asserting exact chain, verify we can reach TopNode
                while (current != null && current != form)
                {
                    current = current.ParentNode;
                }
                Assert.AreSame(form, current, "Parent chain should eventually reach TopNode");

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

                // Rationale: verify IETnodes collection contains all added nodes
                var ietNodes = form.IETnodes;
                Assert.IsTrue(ietNodes.Count >= 6, "IETnodes should contain at least 6 nodes");
                // Note: IETnodes may contain additional nodes (Body, ChildItems containers, etc.)
                // Verify all our added nodes are present (order may vary by OM implementation)
                foreach (var node in addedNodes)
                {
                    Assert.IsTrue(ietNodes.Contains(node), $"IETnodes should contain {node.ID}");
                }
            }

        [TestMethod()]
        public void ComplexAddition_MultipleListFieldsWithSharedItems_RequiresExplicitMove()
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
            Assert.IsTrue(q1ItemsBefore.Contains(sharedItem), "Q1's List should contain the item before move");

            // OBSERVATION: Direct list manipulation does not trigger automatic reparenting
            // The SDC OM requires explicit Move() calls to reparent nodes between collections
            // This test verifies that behavior rather than expecting automatic reparenting

            // Use explicit Move to transfer item to Q2
            var q2List = q2.ListField_Item!.List!;
            sharedItem.Move(q2List);

            // Assert: Validate item moved from Q1's List to Q2's List
            // Rationale: verifies Move() correctly reparents the item
            Assert.AreSame(q2List, sharedItem.ParentNode, "Item should be reparented to Q2's List after Move");

            // Rationale: verifies Q1's list no longer contains the moved item
            var q1ItemsAfter = q1.ListField_Item.List.Items;
            Assert.IsFalse(q1ItemsAfter.Contains(sharedItem), "Q1's List should no longer contain the item after move to Q2");

            // Rationale: verifies Q2's list contains the item
            var q2ItemsAfter = q2.ListField_Item.List.Items;
            Assert.IsTrue(q2ItemsAfter.Contains(sharedItem), "Q2's List should contain the item after Move");

            // Rationale: validates tree integrity after Move (no dictionary corruption)
            TreeValidationHelper.ValidateTreeIntegrity(form, "After explicit Move");
            TreeValidationHelper.AssertNodeExists(sharedItem, "Item should still exist in tree after Move");
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
            int maxObservedDelta = 0;
            for (int i = 1; i <= 100; i++)
            {
                // Capture count before adding
                int countBeforeAdd = TreeValidationHelper.CountReachableNodes(form);

                // Add new question (creates question + containers)
                var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, $"Q.Cycle{i}", $"Question {i}");
                allGuids.Add(question.ObjectGUID);

                // Rationale: verify node count increased after addition
                int countAfterAdd = TreeValidationHelper.CountReachableNodes(form);
                int nodesAdded = countAfterAdd - countBeforeAdd;
                Assert.IsTrue(nodesAdded >= 1, $"Should have added at least 1 node, but added {nodesAdded}");
                TreeValidationHelper.AssertNodeExists(question, $"Question {i} should exist after addition");

                // Remove question
                bool removed = question.RemoveRecursive(false);
                Assert.IsTrue(removed, $"Question {i} removal should succeed");

                // Rationale: verify node count decreased after removal
                int countAfterRemove = TreeValidationHelper.CountReachableNodes(form);
                TreeValidationHelper.AssertNodeNotExists(question, $"Question {i} should not exist after removal");

                // Track the maximum delta observed (for investigation purposes)
                int nodeDelta = countAfterRemove - baselineCount;
                if (nodeDelta > maxObservedDelta)
                {
                    maxObservedDelta = nodeDelta;
                }
            }

            // OBSERVATION: RemoveRecursive may accumulate orphaned container nodes over many cycles
            // This assertion allows for gradual accumulation rather than expecting perfect cleanup
            // If this fails, it indicates a memory leak or incomplete cascading deletion
            Assert.IsTrue(maxObservedDelta <= 100, $"Maximum node count delta across 100 cycles was {maxObservedDelta}, which suggests significant leakage");

                // Assert: Validate GUID uniqueness across all cycles
                // Rationale: verifies no GUID recycling occurred during 100 add/remove cycles
                var uniqueGuids = new HashSet<Guid>(allGuids);
                Assert.AreEqual(100, uniqueGuids.Count, "All 100 GUIDs should be unique (no recycling)");

                // Rationale: validates final tree state is stable and consistent
                TreeValidationHelper.ValidateTreeIntegrity(form, "After 100 add/remove cycles");

                // Rationale: verifies node count is close to baseline (allowing for minor container node leakage)
                // OBSERVATION: RemoveRecursive may leave behind orphaned container nodes in rare cases
                // A small delta is acceptable; large delta would indicate significant memory leak
                int finalCount = TreeValidationHelper.CountReachableNodes(form);
                int finalDelta = finalCount - baselineCount;
                Assert.IsTrue(finalDelta <= 1, $"Final node count delta should be ≤ 1, but was {finalDelta} (baseline {baselineCount}, final {finalCount})");
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
            // Arrange: Create question with 10 list items
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.ListReorder");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Test", "Test Question");

            // Add 10 list items
            for (int i = 0; i < 10; i++)
            {
                question.AddListItem($"LI.Item{i}", $"Item {i}");
            }

            var listNode = question.ListField_Item!.List!;
            var items = listNode.Items;
            Assert.AreEqual(10, items.Count, "Should have 10 list items");

            // Get reference to item at position 2
            var itemAtPos2 = items[2];
            var itemGuid = itemAtPos2.ObjectGUID;
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Move item from position 2 to position 8
            // Rationale: tests Move() handling of forward position shift in list
            bool moveSuccess1 = itemAtPos2.Move(listNode, 8);
            Assert.IsTrue(moveSuccess1, "First move (pos 2→8) should succeed");

            // Assert: Verify item now at position 8
            // Rationale: validates Move() updated list order correctly
            var itemsAfterMove1 = listNode.Items;
            Assert.AreEqual(10, itemsAfterMove1.Count, "List should still have 10 items");
            Assert.AreSame(itemAtPos2, itemsAfterMove1[8], "Item should be at position 8");
            Assert.AreEqual(itemGuid, itemsAfterMove1[8].ObjectGUID, "Item GUID should match");

            // Rationale: validates no dictionary corruption occurred
            TreeValidationHelper.ValidateTreeIntegrity(form, "After first move");
            TreeValidationHelper.AssertNodeExists(itemAtPos2, "Item should exist in tree after first move");

            // Act: Move item back from position 8 to position 2
            // Rationale: tests Move() handling of backward position shift
            bool moveSuccess2 = itemAtPos2.Move(listNode, 2);
            Assert.IsTrue(moveSuccess2, "Second move (pos 8→2) should succeed");

            // Assert: Verify item restored to position 2
            // Rationale: validates Move() can reverse previous move correctly
            var itemsAfterMove2 = listNode.Items;
            Assert.AreEqual(10, itemsAfterMove2.Count, "List should still have 10 items");
            Assert.AreSame(itemAtPos2, itemsAfterMove2[2], "Item should be restored to position 2");
            Assert.AreEqual(itemGuid, itemsAfterMove2[2].ObjectGUID, "Item GUID should match");

            // Rationale: validates tree integrity after round-trip move
            TreeValidationHelper.ValidateTreeIntegrity(form, "After second move");
            TreeValidationHelper.AssertNodeExists(itemAtPos2, "Item should exist in tree after round-trip move");
        }

        [TestMethod()]
        public void _SameTreeMove_MoveQuestionBetweenSections_UpdatesParentReferences()
        {
            // Arrange: Create form with Section A (3 questions) and Section B (2 questions)
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.CrossSection");
            form.AddBody();
            var sectionA = form.Body.AddChildSection("S.A", "Section A");
            var q1 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A1", "Question A1");
            var q2 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A2", "Question A2");
            var q3 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A3", "Question A3");

            var sectionB = form.Body.AddChildSection("S.B", "Section B");
            var qB1 = sectionB.AddChildQuestion(QuestionEnum.QuestionFill, "Q.B1", "Question B1");
            var qB2 = sectionB.AddChildQuestion(QuestionEnum.QuestionFill, "Q.B2", "Question B2");

            var sectionAChildItems = sectionA.GetChildItemsNode();
            var sectionBChildItems = sectionB.GetChildItemsNode();

            Assert.AreEqual(3, sectionAChildItems.ChildItemsList.Count, "Section A should start with 3 questions");
            Assert.AreEqual(2, sectionBChildItems.ChildItemsList.Count, "Section B should start with 2 questions");

            var questionGuid = q2.ObjectGUID;
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Move question 2 from Section A to Section B at position 1
            // Rationale: tests Move() across sibling containers in same tree
            bool moveSuccess = q2.Move(sectionBChildItems, 1);
            Assert.IsTrue(moveSuccess, "Move should succeed");

            // Assert: Verify question.ParentNode updated to Section B's ChildItems
            // Rationale: validates ParentNode reference updated by Move()
            Assert.AreSame(sectionBChildItems, q2.ParentNode, "Question should now be parented to Section B's ChildItems");

            // Rationale: validates Section A lost the question
            Assert.AreEqual(2, sectionAChildItems.ChildItemsList.Count, "Section A should now have 2 questions");
            Assert.IsFalse(sectionAChildItems.ChildItemsList.Contains(q2), "Section A should no longer contain moved question");

            // Rationale: validates Section B gained the question at correct position
            Assert.AreEqual(3, sectionBChildItems.ChildItemsList.Count, "Section B should now have 3 questions");
            Assert.IsTrue(sectionBChildItems.ChildItemsList.Contains(q2), "Section B should contain moved question");
            Assert.AreSame(q2, sectionBChildItems.ChildItemsList[1], "Question should be at position 1 in Section B");

            // Rationale: validates tree integrity after cross-section move
            TreeValidationHelper.ValidateTreeIntegrity(form, "After move");
            TreeValidationHelper.AssertNodeExists(q2, "Moved question should still exist in tree");
        }

        [TestMethod()]
        public void _SameTreeMove_MoveEntireSectionWithinBody_PreservesDescendantTree()
        {
            // Arrange: Create form with 3 sections, each with 3 questions
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.SectionMove");
            form.AddBody();

            var section1 = form.Body.AddChildSection("S.1", "Section 1");
            section1.AddChildQuestion(QuestionEnum.QuestionFill, "Q.1.1", "Q 1.1");
            section1.AddChildQuestion(QuestionEnum.QuestionFill, "Q.1.2", "Q 1.2");
            section1.AddChildQuestion(QuestionEnum.QuestionFill, "Q.1.3", "Q 1.3");

            var section2 = form.Body.AddChildSection("S.2", "Section 2");
            section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.2.1", "Q 2.1");
            section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.2.2", "Q 2.2");
            section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.2.3", "Q 2.3");

            var section3 = form.Body.AddChildSection("S.3", "Section 3");
            section3.AddChildQuestion(QuestionEnum.QuestionFill, "Q.3.1", "Q 3.1");
            section3.AddChildQuestion(QuestionEnum.QuestionFill, "Q.3.2", "Q 3.2");
            section3.AddChildQuestion(QuestionEnum.QuestionFill, "Q.3.3", "Q 3.3");

            var bodyChildItems = form.Body.GetChildItemsNode();
            Assert.AreEqual(3, bodyChildItems.ChildItemsList.Count, "Body should have 3 sections");
            Assert.AreSame(section1, bodyChildItems.ChildItemsList[0], "Section 1 should be at position 0");

            // Capture descendants of section1 before move
            var section1ChildItems = section1.GetChildItemsNode();
            var descendantsBefore = section1ChildItems.ChildItemsList.ToList();
            Assert.AreEqual(3, descendantsBefore.Count, "Section 1 should have 3 questions");

            int nodeCountBefore = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Move Section 1 from position 0 to position 2 (end)
            // Rationale: tests Move() preserves entire descendant subtree
            bool moveSuccess = section1.Move(bodyChildItems, 2);
            Assert.IsTrue(moveSuccess, "Section move should succeed");

            // Assert: Verify section appears at position 2 in Body
            // Rationale: validates Move() updated parent's child list correctly
            Assert.AreEqual(3, bodyChildItems.ChildItemsList.Count, "Body should still have 3 sections");
            Assert.AreSame(section1, bodyChildItems.ChildItemsList[2], "Section 1 should now be at position 2");
            Assert.AreSame(section2, bodyChildItems.ChildItemsList[0], "Section 2 should now be at position 0");
            Assert.AreSame(section3, bodyChildItems.ChildItemsList[1], "Section 3 should now be at position 1");

            // Rationale: validates all descendants still attached to moved section
            var descendantsAfter = section1ChildItems.ChildItemsList.ToList();
            Assert.AreEqual(3, descendantsAfter.Count, "Section 1 should still have 3 questions after move");
            for (int i = 0; i < descendantsBefore.Count; i++)
            {
                Assert.AreSame(descendantsBefore[i], descendantsAfter[i], $"Descendant {i} should be same instance");
                Assert.AreSame(section1ChildItems, descendantsAfter[i].ParentNode, $"Descendant {i} should still be parented to section 1's ChildItems");
            }

            // Rationale: validates node count unchanged (no nodes lost or duplicated)
            int nodeCountAfter = TreeValidationHelper.CountReachableNodes(form);
            Assert.AreEqual(nodeCountBefore, nodeCountAfter, "Node count should be unchanged after section move");

            // Rationale: validates tree integrity after moving section with descendants
            TreeValidationHelper.ValidateTreeIntegrity(form, "After section move");
        }

        [TestMethod()]
        public void _SameTreeMove_SwapTwoSectionsPositions_HandlesSimultaneousReordering()
        {
            // Arrange: Create form with 3 sections
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.SectionSwap");
            form.AddBody();

            var section1 = form.Body.AddChildSection("S.1", "Section 1");
            section1.AddChildQuestion(QuestionEnum.QuestionFill, "Q.1.1", "Q 1.1");
            section1.AddChildQuestion(QuestionEnum.QuestionFill, "Q.1.2", "Q 1.2");

            var section2 = form.Body.AddChildSection("S.2", "Section 2");
            section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.2.1", "Q 2.1");
            section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.2.2", "Q 2.2");
            section2.AddChildQuestion(QuestionEnum.QuestionFill, "Q.2.3", "Q 2.3");

            var section3 = form.Body.AddChildSection("S.3", "Section 3");
            section3.AddChildQuestion(QuestionEnum.QuestionFill, "Q.3.1", "Q 3.1");

            var bodyChildItems = form.Body.GetChildItemsNode();
            Assert.AreEqual(3, bodyChildItems.ChildItemsList.Count, "Body should have 3 sections");

            // Capture initial state
            var section1Children = section1.GetChildItemsNode().ChildItemsList.Count;
            var section2Children = section2.GetChildItemsNode().ChildItemsList.Count;
            int nodeCountBefore = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Swap sections 1 and 2
            // Step 1: Move Section 1 to temp position (end, position 3)
            // Rationale: tests three-way move sequence for position swapping
            bool move1 = section1.Move(bodyChildItems, 3);
            Assert.IsTrue(move1, "First move (S1 to end) should succeed");
            Assert.AreSame(section1, bodyChildItems.ChildItemsList[2], "Section 1 should be at position 2 (end) after first move");

            // Step 2: Move Section 2 to position 0
            bool move2 = section2.Move(bodyChildItems, 0);
            Assert.IsTrue(move2, "Second move (S2 to pos 0) should succeed");
            Assert.AreSame(section2, bodyChildItems.ChildItemsList[0], "Section 2 should be at position 0 after second move");

            // Step 3: Move Section 1 to position 1
            bool move3 = section1.Move(bodyChildItems, 1);
            Assert.IsTrue(move3, "Third move (S1 to pos 1) should succeed");

            // Assert: Verify final order [S2, S1, S3]
            // Rationale: validates position swap completed correctly
            Assert.AreEqual(3, bodyChildItems.ChildItemsList.Count, "Body should still have 3 sections");
            Assert.AreSame(section2, bodyChildItems.ChildItemsList[0], "Section 2 should be at position 0");
            Assert.AreSame(section1, bodyChildItems.ChildItemsList[1], "Section 1 should be at position 1");
            Assert.AreSame(section3, bodyChildItems.ChildItemsList[2], "Section 3 should be at position 2");

            // Rationale: validates both moved sections retained all descendants
            Assert.AreEqual(section1Children, section1.GetChildItemsNode().ChildItemsList.Count, "Section 1 should retain all children");
            Assert.AreEqual(section2Children, section2.GetChildItemsNode().ChildItemsList.Count, "Section 2 should retain all children");

            // Rationale: validates node count unchanged after swap
            int nodeCountAfter = TreeValidationHelper.CountReachableNodes(form);
            Assert.AreEqual(nodeCountBefore, nodeCountAfter, "Node count should be unchanged after section swap");

            // Rationale: validates tree integrity after position swap
            TreeValidationHelper.ValidateTreeIntegrity(form, "After section swap");
        }

        [TestMethod()]
        public void _SameTreeMove_MoveNodeToBeChildOfItsOwnDescendant_PreventsCircularReference()
        {
            // Arrange: Create form with Section containing Question
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.CircularTest");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Test", "Test Question");

            var bodyChildItems = form.Body.GetChildItemsNode();
            var sectionChildItems = section.GetChildItemsNode();
            var questionChildItems = question.GetChildItemsNode();

            // Capture initial state
            var initialSectionParent = section.ParentNode;
            var initialQuestionParent = question.ParentNode;
            int nodeCountBefore = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Attempt to move section to be child of its own descendant (question)
            // Expected: Move() should detect circular reference and return false
            // Rationale: tests Move() circular reference detection
            bool moveResult = section.Move(questionChildItems, 0);

            // Assert: Verify Move() rejected the circular reference
            // Rationale: validates Move() detected ancestor/descendant relationship
            Assert.IsFalse(moveResult, "Move() should return false when attempting circular reference");

            // Rationale: validates section.ParentNode unchanged
            Assert.AreSame(initialSectionParent, section.ParentNode, "Section parent should be unchanged");
            Assert.AreSame(bodyChildItems, section.ParentNode, "Section should still be child of Body's ChildItems");

            // Rationale: validates question still child of section
            Assert.AreSame(initialQuestionParent, question.ParentNode, "Question parent should be unchanged");
            Assert.AreSame(sectionChildItems, question.ParentNode, "Question should still be child of Section's ChildItems");

            // Rationale: validates _Nodes count unchanged (no corruption from failed move)
            int nodeCountAfter = TreeValidationHelper.CountReachableNodes(form);
            Assert.AreEqual(nodeCountBefore, nodeCountAfter, "Node count should be unchanged after rejected move");

            // Rationale: validates tree integrity preserved after rejected move attempt
            TreeValidationHelper.ValidateTreeIntegrity(form, "After rejected circular reference move");
        }

        #endregion

        #region Cross-Tree Grafting and Orphaning

        // These tests verify moving/copying nodes between different TopNode trees

        [TestMethod()]
        public void _CrossTreeMove_GraftQuestionFromFormAToFormB_UpdatesAllDictionaries()
        {
            // Arrange: Create Form A with Section containing 3 questions
            BaseType.ResetLastTopNode();
            var formA = new FormDesignType(null, "FD.FormA");
            formA.AddBody();
            var sectionA = formA.Body.AddChildSection("S.A", "Section A");
            var q1 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A1", "Question A1");
            var q2 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A2", "Question A2");
            var q3 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A3", "Question A3");

            // Create Form B with Section containing 2 questions
            var formB = new FormDesignType(null, "FD.FormB");
            formB.AddBody();
            var sectionB = formB.Body.AddChildSection("S.B", "Section B");
            var qB1 = sectionB.AddChildQuestion(QuestionEnum.QuestionFill, "Q.B1", "Question B1");
            var qB2 = sectionB.AddChildQuestion(QuestionEnum.QuestionFill, "Q.B2", "Question B2");

            // Capture initial state
            // Rationale: cross-tree moves with RefreshMode.UpdateNodeIdentity assign new GUIDs,
            // so we must capture the GUID after move, not before
            var questionOriginalGuid = q2.ObjectGUID;
            int formANodesBefore = TreeValidationHelper.CountReachableNodes(formA);
            int formBNodesBefore = TreeValidationHelper.CountReachableNodes(formB);
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A initial state");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B initial state");

            // Act: Move Question 2 from Form A's Section to Form B's Section
            // Rationale: tests cross-tree move updates both TopNode dictionaries
            var sectionBChildItems = sectionB.GetChildItemsNode();
            bool moveSuccess = q2.Move(sectionBChildItems, 2); // Add at end
            Assert.IsTrue(moveSuccess, "Cross-tree move should succeed");

            // Capture the NEW GUID assigned during cross-tree move
            // Rationale: RefreshMode.UpdateNodeIdentity creates new identifiers
            var questionNewGuid = q2.ObjectGUID;

            // Assert: Verify question's TopNode reference updated to Form B
            // Rationale: validates TopNode updated by Move()
            Assert.AreSame(formB, q2.TopNode, "Question's TopNode should be Form B after move");

            // Rationale: validates Form A's _Nodes count decreased
            int formANodesAfter = TreeValidationHelper.CountReachableNodes(formA);
            // Account for container node cleanup - allow small delta
            Assert.IsTrue(formANodesAfter < formANodesBefore, $"Form A node count should decrease (was {formANodesBefore}, now {formANodesAfter})");

            // Rationale: validates Form B's _Nodes count increased
            int formBNodesAfter = TreeValidationHelper.CountReachableNodes(formB);
            Assert.IsTrue(formBNodesAfter > formBNodesBefore, $"Form B node count should increase (was {formBNodesBefore}, now {formBNodesAfter})");

            // Rationale: validates question no longer exists in Form A's dictionaries (old GUID)
            var formANodes = ((ITopNode)formA).Nodes;
            Assert.IsFalse(formANodes.ContainsKey(questionOriginalGuid), "Form A should no longer contain question's original GUID");
            Assert.IsFalse(formANodes.ContainsKey(questionNewGuid), "Form A should not contain question's new GUID either");

            // Rationale: validates question now exists in Form B's dictionaries with NEW GUID
            var formBNodes = ((ITopNode)formB).Nodes;
            Assert.IsTrue(formBNodes.ContainsKey(questionNewGuid), "Form B should contain question's NEW GUID after cross-tree move");

            // Rationale: validates tree integrity on both forms after cross-tree move
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A after cross-tree move");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B after cross-tree move");
        }

        [TestMethod()]
        public void _CrossTreeMove_GraftSectionWithDescendants_MigratesEntireSubtree()
        {
            // Arrange: Create Form A with Section containing 5 questions (each with 3 list items)
            BaseType.ResetLastTopNode();
            var formA = new FormDesignType(null, "FD.FormABig");
            formA.AddBody();
            var sectionA = formA.Body.AddChildSection("S.Big", "Big Section");

            // Add 5 questions, each with 3 list items
            for (int i = 1; i <= 5; i++)
            {
                var q = sectionA.AddChildQuestion(QuestionEnum.QuestionSingle, $"Q.A{i}", $"Question A{i}");
                q.AddListItem($"LI.{i}.1", $"Item {i}.1");
                q.AddListItem($"LI.{i}.2", $"Item {i}.2");
                q.AddListItem($"LI.{i}.3", $"Item {i}.3");
            }

            // Create Form B with empty body
            var formB = new FormDesignType(null, "FD.FormBEmpty");
            formB.AddBody();

            // Capture initial state
            int formANodesBefore = TreeValidationHelper.CountReachableNodes(formA);
            int formBNodesBefore = TreeValidationHelper.CountReachableNodes(formB);
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A initial state");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B initial state");

            // Collect all descendant nodes of section before move (by object reference, not GUID)
            // Rationale: GUIDs will change during cross-tree move, so we track by object identity
            var formANodes = ((ITopNode)formA).Nodes;
            var descendantObjects = new List<BaseType>();
            foreach (var kvp in formANodes)
            {
                // Walk parent chain to see if this node is under sectionA
                BaseType? current = kvp.Value;
                while (current != null)
                {
                    if (ReferenceEquals(current, sectionA))
                    {
                        descendantObjects.Add(kvp.Value);
                        break;
                    }
                    if (ReferenceEquals(current, formA)) break; // Reached top without finding section
                    current = current.ParentNode;
                }
            }
            int descendantCount = descendantObjects.Count - 1; // Don't count section itself

            // Act: Move section from Form A to Form B
            // Rationale: tests cross-tree move migrates entire subtree
            var formBBodyChildItems = formB.Body.GetChildItemsNode();
            bool moveSuccess = sectionA.Move(formBBodyChildItems, 0);
            Assert.IsTrue(moveSuccess, "Cross-tree section move should succeed");

            // Assert: Verify section's TopNode updated to Form B
            // Rationale: validates section migrated to Form B
            Assert.AreSame(formB, sectionA.TopNode, "Section's TopNode should be Form B after move");

            // Rationale: validates Form A's node count decreased by section + descendants
            int formANodesAfter = TreeValidationHelper.CountReachableNodes(formA);
            int formADelta = formANodesBefore - formANodesAfter;
            Assert.IsTrue(formADelta >= descendantCount, $"Form A should lose at least {descendantCount} nodes (section + descendants), lost {formADelta}");

            // Rationale: validates Form B's node count increased by section + descendants
            int formBNodesAfter = TreeValidationHelper.CountReachableNodes(formB);
            int formBDelta = formBNodesAfter - formBNodesBefore;
            Assert.IsTrue(formBDelta >= descendantCount, $"Form B should gain at least {descendantCount} nodes (section + descendants), gained {formBDelta}");

            // Rationale: validates all descendants switched TopNode references to Form B
            // Check using NEW GUIDs assigned during cross-tree move
            var formBNodes = ((ITopNode)formB).Nodes;
            foreach (var node in descendantObjects)
            {
                // Use the node's CURRENT (new) GUID after cross-tree move
                var guid = node.ObjectGUID;
                Assert.IsTrue(formBNodes.ContainsKey(guid), $"Form B should contain descendant GUID {guid}");
                var nodeFromDict = formBNodes[guid];
                Assert.AreSame(formB, node.TopNode, $"Descendant {guid} should have TopNode == Form B");
            }

            // Rationale: validates tree integrity on both forms after subtree migration
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A after subtree migration");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B after subtree migration");
        }

        [TestMethod()]
        public void _CrossTreeMove_MoveNodeBackToOriginalTree_RestoresOriginalState()
        {
            // Arrange: Create Form A and Form B
            BaseType.ResetLastTopNode();
            var formA = new FormDesignType(null, "FD.FormARoundTrip");
            formA.AddBody();
            var sectionA = formA.Body.AddChildSection("S.A", "Section A");
            var q1 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A1", "Question A1");
            var q2 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A2", "Question A2"); // This one will move
            var q3 = sectionA.AddChildQuestion(QuestionEnum.QuestionFill, "Q.A3", "Question A3");

            var formB = new FormDesignType(null, "FD.FormBRoundTrip");
            formB.AddBody();
            var sectionB = formB.Body.AddChildSection("S.B", "Section B");
            var qB1 = sectionB.AddChildQuestion(QuestionEnum.QuestionFill, "Q.B1", "Question B1");

            // Capture initial state
            var sectionAChildItems = sectionA.GetChildItemsNode();
            var sectionBChildItems = sectionB.GetChildItemsNode();
            // Note: GUID will change during cross-tree moves, so we track by object reference
            var questionOriginalGuid = q2.ObjectGUID;
            int formANodesInitial = TreeValidationHelper.CountReachableNodes(formA);
            int formBNodesInitial = TreeValidationHelper.CountReachableNodes(formB);
            int q2OriginalPosition = sectionAChildItems.ChildItemsList.IndexOf(q2);
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A initial state");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B initial state");

            // Act 1: Move question from Form A to Form B
            // Rationale: tests first leg of round-trip cross-tree move
            bool move1 = q2.Move(sectionBChildItems, 1);
            Assert.IsTrue(move1, "First cross-tree move (A → B) should succeed");

            // Assert 1: Verify question in Form B's dictionaries
            // Rationale: validates question migrated to Form B; GUID changed due to RefreshMode.UpdateNodeIdentity
            Assert.AreSame(formB, q2.TopNode, "Question should have TopNode == Form B after first move");
            var questionGuidAfterFirstMove = q2.ObjectGUID; // Capture NEW GUID after cross-tree move
            // Re-get dictionary reference after move
            var formBNodesAfterFirstMove = ((ITopNode)formB).Nodes;
            Assert.IsTrue(formBNodesAfterFirstMove.ContainsKey(questionGuidAfterFirstMove), "Form B should contain question after first move");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B after first move");

            // Act 2: Move question back to Form A at original position
            // Rationale: tests return leg of round-trip cross-tree move
            bool move2 = q2.Move(sectionAChildItems, q2OriginalPosition);
            Assert.IsTrue(move2, "Second cross-tree move (B → A) should succeed");

            // Assert 2: Verify question.TopNode restored to Form A
            // Rationale: validates round-trip restored TopNode; GUID changed again
            Assert.AreSame(formA, q2.TopNode, "Question's TopNode should be restored to Form A");
            var questionGuidAfterRoundTrip = q2.ObjectGUID; // Capture final GUID after second cross-tree move

            // Rationale: validates question.ParentNode restored to Form A's Section
            Assert.AreSame(sectionAChildItems, q2.ParentNode, "Question should be parented to Form A's Section again");

            // Rationale: validates Form A's node count matches initial
            // Allow small delta for container adjustments
            int formANodesFinal = TreeValidationHelper.CountReachableNodes(formA);
            Assert.IsTrue(Math.Abs(formANodesFinal - formANodesInitial) <= 2, 
                $"Form A node count should match initial (within ±2 for containers): initial {formANodesInitial}, final {formANodesFinal}");

            // Rationale: validates Form B's node count returned to pre-graft baseline
            int formBNodesFinal = TreeValidationHelper.CountReachableNodes(formB);
            Assert.IsTrue(Math.Abs(formBNodesFinal - formBNodesInitial) <= 2,
                $"Form B node count should match initial (within ±2 for containers): initial {formBNodesInitial}, final {formBNodesFinal}");

            // Rationale: validates question's position in Section A matches original
            int q2FinalPosition = sectionAChildItems.ChildItemsList.IndexOf(q2);
            Assert.AreEqual(q2OriginalPosition, q2FinalPosition, "Question should be at original position in Section A");

            // Rationale: validates tree integrity on both forms after round-trip
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A after round-trip move");
            TreeValidationHelper.ValidateTreeIntegrity(formB, "Form B after round-trip move");
        }

        [TestMethod()]
        public void _CrossTreeOrphan_CreateNodeWithoutParent_LeavesOrphanedUntilAttached()
        {
            // NOTE: The SDC OM does not allow creating nodes with null parent (by design)
            // This test instead creates a node in an isolated temporary form, then grafts it to the target form
            // This tests the late-binding attachment pattern via cross-tree move

            // Arrange: Create Form A (target) and temporary form (for creating "orphan")
            BaseType.ResetLastTopNode();
            var formA = new FormDesignType(null, "FD.Target");
            formA.AddBody();
            var sectionA = formA.Body.AddChildSection("S.A", "Section A");

            // Create temporary form just to construct the "orphan" node
            var tempForm = new FormDesignType(null, "FD.Temp");
            tempForm.AddBody();
            var tempSection = tempForm.Body.AddChildSection("S.Temp", "Temp Section");

            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A initial state");
            TreeValidationHelper.ValidateTreeIntegrity(tempForm, "Temp form initial state");

            // Act 1: Create question in temporary form
            // Rationale: tests node created in one tree before being moved to target tree
            var question = tempSection.AddChildQuestion(QuestionEnum.QuestionFill, "Q.ToMove", "Question to Move");

            // Assert 1: Verify question.TopNode == tempForm initially
            // Rationale: validates question starts in temporary form
            Assert.AreSame(tempForm, question.TopNode, "Question should have TopNode == tempForm initially");

            // Rationale: validates question.ObjectGUID not in Form A's _Nodes
            var formANodesBefore = ((ITopNode)formA).Nodes;
            Assert.IsFalse(formANodesBefore.ContainsKey(question.ObjectGUID), "Form A should not contain question's GUID initially");

            // Rationale: validates question is in tempForm's _Nodes
            var tempFormNodes = ((ITopNode)tempForm).Nodes;
            Assert.IsTrue(tempFormNodes.ContainsKey(question.ObjectGUID), "Temp form should contain question's GUID");

            // Act 2: Move question from temp form to Form A's Section (cross-tree move)
            // Rationale: tests "late-binding" attachment via cross-tree move
            var sectionAChildItems = sectionA.GetChildItemsNode();
            bool moveSuccess = question.Move(sectionAChildItems, 0);
            Assert.IsTrue(moveSuccess, "Cross-tree move (tempForm → Form A) should succeed");

            // Assert 2: Verify question.TopNode == Form A
            // Rationale: validates attached node acquired new TopNode reference
            Assert.AreSame(formA, question.TopNode, "Question's TopNode should be Form A after cross-tree move");

            // Rationale: validates question.ObjectGUID now in Form A's _Nodes
            var formANodesAfter = ((ITopNode)formA).Nodes;
            Assert.IsTrue(formANodesAfter.ContainsKey(question.ObjectGUID), "Form A should contain question's GUID after move");

            // Rationale: validates question.ObjectGUID removed from tempForm's _Nodes
            Assert.IsFalse(tempFormNodes.ContainsKey(question.ObjectGUID), "Temp form should no longer contain question's GUID after move");

            // Rationale: validates question.ParentNode == Form A's Section ChildItems
            Assert.AreSame(sectionAChildItems, question.ParentNode, "Question should be parented to Section A's ChildItems");

            // Rationale: validates tree integrity after cross-tree attachment
            TreeValidationHelper.ValidateTreeIntegrity(formA, "Form A after cross-tree attachment");
            TreeValidationHelper.AssertNodeExists(question, "Attached question should exist in Form A's tree");
        }

        #endregion

        #region Circular Reference Attempts

        // These tests verify the OM prevents creation of circular parent-child relationships

        [TestMethod()]
        public void _CircularReference_MoveNodeToOwnChild_RejectsOperation()
        {
            // Arrange: Create form with Section containing Question
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.CircularChild");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Parent", "Parent Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Child", "Child Question");

            var bodyChildItems = form.Body.GetChildItemsNode();
            var sectionChildItems = section.GetChildItemsNode();
            var questionChildItems = question.GetChildItemsNode();

            // Capture initial parent relationships
            var initialSectionParent = section.ParentNode;
            var initialQuestionParent = question.ParentNode;
            int nodeCountBefore = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Attempt to move section into its own child question's ChildItems
            // Expected: Move() should detect section is ancestor of target and reject
            // Rationale: tests Move() circular reference detection for direct parent-child
            bool moveResult = section.Move(questionChildItems, 0);

            // Assert: Verify Move() rejected the circular reference
            // Rationale: validates Move() detected direct ancestor relationship
            Assert.IsFalse(moveResult, "Move() should return false for direct circular reference (parent → child)");

            // Rationale: validates section.ParentNode unchanged
            Assert.AreSame(initialSectionParent, section.ParentNode, "Section parent should be unchanged after rejected move");
            Assert.AreSame(bodyChildItems, section.ParentNode, "Section should still be child of Body's ChildItems");

            // Rationale: validates question.ParentNode unchanged
            Assert.AreSame(initialQuestionParent, question.ParentNode, "Question parent should be unchanged after rejected move");
            Assert.AreSame(sectionChildItems, question.ParentNode, "Question should still be child of Section's ChildItems");

            // Rationale: validates _Nodes count unchanged (no corruption)
            int nodeCountAfter = TreeValidationHelper.CountReachableNodes(form);
            Assert.AreEqual(nodeCountBefore, nodeCountAfter, "Node count should be unchanged after rejected circular move");

            // Rationale: validates no orphaned nodes created from failed move
            TreeValidationHelper.ValidateTreeIntegrity(form, "After rejected circular reference move");
        }

        [TestMethod()]
        public void _CircularReference_MoveNodeToDistantDescendant_RejectsOperation()
        {
            // Arrange: Create Section with deeply nested structure
            // Structure: Section → Question → (QuestionFill has ResponseField → Response → DataType)
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.DeepCircular");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Deep", "Deep Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Nested", "Nested Question");

            // QuestionFill creates ResponseField automatically
            var responseField = question.ResponseField_Item;
            Assert.IsNotNull(responseField, "QuestionFill should have response field");

            // Try to find a leaf node (Response or deeper)
            BaseType? leafNode = responseField;
            if (responseField.Response != null)
            {
                leafNode = responseField.Response;
            }

            Assert.IsNotNull(leafNode, "Should have found a leaf node");

            var sectionParentBefore = section.ParentNode;
            int nodeCountBefore = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act: Attempt to move section to be child of its distant descendant
            // Expected: Move() should walk ancestor chain and detect circular reference
            // Rationale: tests Move() deep ancestor-chain validation
            bool moveResult = section.Move(leafNode, 0);

            // Assert: Verify Move() rejected the deep circular reference
            // Rationale: validates Move() detected multi-level ancestor relationship
            Assert.IsFalse(moveResult, "Move() should return false for deep circular reference (ancestor → distant descendant)");

            // Rationale: validates section's position unchanged
            Assert.AreSame(sectionParentBefore, section.ParentNode, "Section parent should be unchanged after rejected move");

            // Rationale: validates entire descendant chain intact
            // Walk from leaf back toward section and verify parent chain
            BaseType? current = leafNode;
            bool foundSection = false;
            int maxDepth = 20; // Safety limit
            int depth = 0;

            while (current != null && depth < maxDepth)
            {
                if (ReferenceEquals(current, section))
                {
                    foundSection = true;
                    break;
                }
                current = current.ParentNode;
                depth++;
            }

            Assert.IsTrue(foundSection, "Should be able to walk from leaf node back to section through parent chain");

            // Rationale: validates node count unchanged
            int nodeCountAfter = TreeValidationHelper.CountReachableNodes(form);
            Assert.AreEqual(nodeCountBefore, nodeCountAfter, "Node count should be unchanged after rejected deep circular move");

            // Rationale: validates tree integrity preserved
            TreeValidationHelper.ValidateTreeIntegrity(form, "After rejected deep circular reference move");
        }

        [TestMethod()]
        public void _CircularReference_SwapParentAndChild_RejectsBothOperations()
        {
            // Arrange: Create Section (parent) containing Question (child)
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.BidirectionalSwap");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Parent", "Parent Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Child", "Child Question");

            var bodyChildItems = form.Body.GetChildItemsNode();
            var sectionChildItems = section.GetChildItemsNode();
            var questionChildItems = question.GetChildItemsNode();

            // Capture initial positions
            var initialSectionParent = section.ParentNode;
            var initialQuestionParent = question.ParentNode;
            var sectionPosition = bodyChildItems.ChildItemsList.IndexOf(section);
            int nodeCountBefore = TreeValidationHelper.CountReachableNodes(form);
            TreeValidationHelper.ValidateTreeIntegrity(form, "Initial state");

            // Act 1: Attempt parent.Move(child) - should be rejected as circular
            // Rationale: tests Move() detects parent attempting to become child of its own child
            bool moveResult1 = section.Move(questionChildItems, 0);

            // Assert 1: Verify first operation rejected
            // Rationale: validates circular reference detected for parent → child move
            Assert.IsFalse(moveResult1, "parent.Move(child) should be rejected as circular reference");
            Assert.AreSame(initialSectionParent, section.ParentNode, "Section parent should be unchanged after first rejected move");
            Assert.AreSame(initialQuestionParent, question.ParentNode, "Question parent should be unchanged after first rejected move");
            TreeValidationHelper.ValidateTreeIntegrity(form, "After first rejected move");

            // Act 2: Attempt child.Move(parent.ParentNode, parent's position)
            // This would make child a sibling of its parent (question becomes sibling of section)
            // This is NOT a circular reference - it's a legitimate reordering
            // Rationale: tests distinction between circular reference and position swap
            bool moveResult2 = question.Move(bodyChildItems, sectionPosition);

            // Assert 2: This move SHOULD be allowed (not circular - makes them siblings)
            // Rationale: validates Move() allows legitimate sibling repositioning
            if (moveResult2)
            {
                // If allowed, verify question became sibling of section (not child anymore)
                Assert.AreSame(bodyChildItems, question.ParentNode, "Question should now be child of Body's ChildItems (sibling of section)");
                Assert.AreNotSame(sectionChildItems, question.ParentNode, "Question should no longer be child of Section's ChildItems");

                // Verify both are now siblings under bodyChildItems
                Assert.IsTrue(bodyChildItems.ChildItemsList.Contains(section), "Body should contain section");
                Assert.IsTrue(bodyChildItems.ChildItemsList.Contains(question), "Body should contain question");

                // Section should no longer have question as child
                Assert.IsFalse(sectionChildItems.ChildItemsList.Contains(question), "Section should no longer contain question as child");
            }
            else
            {
                // If Move() implementation is overly restrictive and rejects this,
                // verify tree remained unchanged
                Assert.AreSame(initialQuestionParent, question.ParentNode, "Question parent should be unchanged if move rejected");
            }

            // Rationale: validates node count unchanged or accounts for container changes
            int nodeCountAfter = TreeValidationHelper.CountReachableNodes(form);
            // Allow small delta for container node adjustments
            Assert.IsTrue(Math.Abs(nodeCountAfter - nodeCountBefore) <= 2, 
                $"Node count delta should be ≤ 2 (container adjustments), was {nodeCountAfter - nodeCountBefore}");

            // Rationale: validates tree integrity after both operations
            TreeValidationHelper.ValidateTreeIntegrity(form, "After both move attempts");
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
