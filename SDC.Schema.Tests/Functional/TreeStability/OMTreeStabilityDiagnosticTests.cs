using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.Tests.Helpers;

namespace SDC.Schema.Tests.Functional.TreeStability
{
    /// <summary>
    /// Diagnostic tests to isolate the cause of test execution crashes.
    /// These are minimal tests to identify validation helper issues.
    /// </summary>
    [TestClass()]
    public class OMTreeStabilityDiagnosticTests
    {
        [TestMethod()]
        public void Diagnostic_SimpleFormCreation_NoValidation()
        {
            // Test: Can we create a simple form without any validation?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic1");
            form.AddBody();

            Assert.IsNotNull(form);
            Assert.IsNotNull(form.Body);
        }

        [TestMethod()]
        public void Diagnostic_SimpleFormCreation_WithNodeCount()
        {
            // Test: Can we count nodes without crashing?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic2");
            form.AddBody();

            int count = TreeValidationHelper.CountReachableNodes(form);
            Assert.IsTrue(count > 0, $"Node count should be > 0, was {count}");
        }

        [TestMethod()]
        public void Diagnostic_SimpleFormCreation_WithBasicValidation()
        {
            // Test: Does basic validation crash?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic3");
            form.AddBody();

            // Try just the basic checks without reflection traversal
            var topNode = form as ITopNode;
            Assert.IsNotNull(topNode);
            Assert.IsNotNull(topNode.Nodes);
            Assert.IsTrue(topNode.Nodes.Count > 0);
        }

        [TestMethod()]
        public void Diagnostic_AddSingleNode_NoValidation()
        {
            // Test: Can we add a single node without validation?
            // NOTE: SDC uses intermediate container nodes, so section.ParentNode is the ChildItemsType container, not Body
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic4");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");

            Assert.IsNotNull(section);
            Assert.IsNotNull(section.ParentNode, "Section should have a parent");
            // Don't assert direct parent is Body - SDC uses intermediate containers
        }

        [TestMethod()]
        public void Diagnostic_AddSingleNode_WithCountValidation()
        {
            // Test: Does validation work after single node addition?
            // NOTE: Adding a section also creates intermediate container nodes, so count increases by more than 1
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic5");
            form.AddBody();

            int beforeCount = TreeValidationHelper.CountReachableNodes(form);

            var section = form.Body.AddChildSection("S.Test", "Test Section");

            int afterCount = TreeValidationHelper.CountReachableNodes(form);
            Assert.IsTrue(afterCount > beforeCount, "Node count should increase after adding section");
            // Don't assert exact count - SDC adds intermediate container nodes
        }

        [TestMethod()]
        public void Diagnostic_Test6_MinimalTree()
        {
            // Test: Renamed to avoid potential test-name collision
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic6");
            form.AddBody();

            // Absolute minimal - just check we created it
            Assert.IsNotNull(form);
            Assert.IsNotNull(form.Body);
        }

        [TestMethod()]
        public void Diagnostic_Test7_WithOneSection()
        {
            // Test: Does validation work after adding one section?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic7");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");

            TreeValidationHelper.ValidateTreeIntegrity(form, "Tree with one section");
        }

        [TestMethod()]
        public void Diagnostic_Test8_WithQuestion()
        {
            // Test: Does validation work with question added?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic8");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Test", "Test Question");

            TreeValidationHelper.ValidateTreeIntegrity(form, "Tree with section and question");
        }

        [TestMethod()]
        public void Diagnostic_Test9_WithListItem()
        {
            // Test: Does validation work with list item?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic9");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Test", "Test Question");
            var item = question.AddListItem("LI.Test", "Test Item");

            TreeValidationHelper.ValidateTreeIntegrity(form, "Tree with list item");
        }

        [TestMethod()]
        public void Diagnostic_BulkAddition_10Items_NoValidation()
        {
            // Test: Can we add 10 items without validation?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic10");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Test", "Test Question");

            for (int i = 1; i <= 10; i++)
            {
                question.AddListItem($"LI.Item{i}", $"Item {i}");
            }

            Assert.IsTrue(true, "10 items added successfully");
        }

        [TestMethod()]
        public void Diagnostic_BulkAddition_10Items_WithValidation()
        {
            // Test: Does validation crash with 10 items?
            BaseType.ResetLastTopNode();
            var form = new FormDesignType(null, "FD.Diagnostic11");
            form.AddBody();
            var section = form.Body.AddChildSection("S.Test", "Test Section");
            var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Test", "Test Question");

            for (int i = 1; i <= 10; i++)
            {
                question.AddListItem($"LI.Item{i}", $"Item {i}");
            }

            TreeValidationHelper.ValidateTreeIntegrity(form, "Tree with 10 list items");
        }
    }
}
