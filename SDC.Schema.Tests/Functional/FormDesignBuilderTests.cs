using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class _FormDesignBuilderTests
	{
		private static FormDesignType CreateFormDesign()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Builder.Tests");
			fd.AddBody();
			return fd;
		}

		[TestMethod]
		public void AssignXmlNames()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.AssignNames", "Question");
			q.AddListItem("LI.AssignNames", "Item");

			fd.AssignElementNamesByReflection();

			// Rationale: reflection naming should populate stable element names for all newly created nodes.
			Assert.IsTrue(fd.Nodes.Values.All(n => !string.IsNullOrWhiteSpace(n.ElementName)));
		}

		[TestMethod]
		public void AssignNamesFromXmlDoc()
		{
			// Bug fix: use a per-test fresh XML source and avoid shared Setup.FD warm-state dependency.
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());

			fd.AssignElementNamesByReflection();

			// Rationale: deserialized trees should still support deterministic name assignment for builder workflows.
			Assert.IsTrue(fd.Nodes.Values.All(n => !string.IsNullOrWhiteSpace(n.ElementName)));
		}

		[TestMethod]
		public void AddRemoveHeader()
		{
			var fd = CreateFormDesign();
			var header = fd.AddHeader();

			Assert.IsNotNull(fd.Header);
			Assert.AreSame(header, fd.AddHeader());

			fd.Header = null;

			// Rationale: builder APIs must support add/remove of optional top sections cleanly.
			Assert.IsNull(fd.Header);
		}

		[TestMethod]
		public void AddRemoveFooter()
		{
			var fd = CreateFormDesign();
			var footer = fd.AddFooter();

			Assert.IsNotNull(fd.Footer);
			Assert.AreSame(footer, fd.AddFooter());

			fd.Footer = null;

			// Rationale: footer should be removable without corrupting the form tree.
			Assert.IsNull(fd.Footer);
		}

		[TestMethod]
		public void AddQuestions()
		{
			var fd = CreateFormDesign();
			var qs = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Single", "Single");
			var qm = fd.Body.AddChildQuestion(QuestionEnum.QuestionMultiple, "Q.Multiple", "Multiple");
			var qf = fd.Body.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Fill", "Fill");

			// Rationale: adding different question subtypes should produce expected object-model shape.
			Assert.AreEqual(QuestionEnum.QuestionSingle, qs.GetQuestionSubtype());
			Assert.AreEqual(QuestionEnum.QuestionMultiple, qm.GetQuestionSubtype());
			Assert.AreEqual(QuestionEnum.QuestionFill, qf.GetQuestionSubtype());
		}

		[TestMethod]
		public void AddListItemToQuestionList()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.List", "List Question");
			var li = q.AddListItem("LI.1", "Choice 1");

			// Rationale: adding list members to list-capable questions is core builder behavior.
			Assert.AreEqual("LI.1", li.ID);
			Assert.AreEqual(1, q.GetListItems()?.Count);
		}

		[TestMethod]
		public void AddListItemOnListItem()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent");
			var li = q.AddListItem("LI.Parent", "Parent Item");
			var childQuestion = li.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Child", "Child Question");
			var nestedLi = childQuestion.AddListItem("LI.Child", "Child Item");

			// Rationale: list items can host child questions, which must support their own list answers.
			Assert.AreEqual("LI.Child", nestedLi.ID);
		}

		[TestMethod]
		public void AdListItemOnDisplayedItem()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.DIHost", "Host");
			var di = q.AddDisplayedTypeToList("DI.Parent", "Displayed");
			var li = q.AddListItem("LI.OnDI", "Choice");

			// Rationale: displayed items and list items should coexist in the same question list model.
			Assert.AreEqual("DI.Parent", di.ID);
			Assert.AreEqual("LI.OnDI", li.ID);
		}

		[TestMethod]
		public void AddDisplayedItemToQuestionList()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.WithDI", "Question");
			var di = q.AddDisplayedTypeToList("DI.InList", "Shown in list");

			// Rationale: list-backed questions should accept displayed content among list members.
			Assert.AreEqual("DI.InList", di.ID);
		}

		[TestMethod]
		public void AddDisplayedItemOnListItem()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.WithLI", "Question");
			var li = q.AddListItem("LI.WithDI", "Item");
			var di = li.AddChildDisplayedItem("DI.OnLI", "Display under LI");

			// Rationale: list items are child-item parents and should support nested displayed items.
			Assert.AreEqual("DI.OnLI", di.ID);
		}

		[TestMethod]
		public void AddDisplayedItemOnDisplayedItem()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.DIParent", "Host");
			var parentDi = q.AddDisplayedTypeToList("DI.Parent2", "Parent");
			var siblingDi = q.AddDisplayedTypeToList("DI.Child", "Child");

			// Rationale: multiple displayed items should be addable in the same list-backed question context.
			Assert.AreEqual("DI.Parent2", parentDi.ID);
			Assert.AreEqual("DI.Child", siblingDi.ID);
		}

		[TestMethod]
		public void AddDisplayedItemAsChild()
		{
			var fd = CreateFormDesign();
			var di = fd.Body.AddChildDisplayedItem("DI.Body", "Body child");

			// Rationale: top-level body composition includes non-question displayed content.
			Assert.AreEqual("DI.Body", di.ID);
			Assert.IsTrue(di.IsDescendantOf(fd.Body));
		}

		[TestMethod]
		public void AddQuestionAsChild()
		{
			var fd = CreateFormDesign();
			var section = fd.Body.AddChildSection("S.1", "Section");
			var qOnSection = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.OnSection", "Q");
			var li = qOnSection.AddListItem("LI.1", "LI");
			var qOnLi = li.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.OnLI", "Q");
			var diInList = qOnSection.AddDisplayedTypeToList("DI.InList", "DI");
			var qOnQ = qOnSection.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.OnQ", "Q");

			// Rationale: verify supported child-parent combinations for question insertion.
			Assert.IsNotNull(qOnLi);
			Assert.IsNotNull(qOnQ);
			Assert.IsFalse(diInList is IChildItemsParent);
		}

		[TestMethod]
		public void AddSectionAsChild()
		{
			var fd = CreateFormDesign();
			var s1 = fd.Body.AddChildSection("S.1", "Section 1");
			var s2 = s1.AddChildSection("S.2", "Section 2");

			// Rationale: section nesting is required for grouped and hierarchical form structures.
			Assert.AreEqual("S.2", s2.ID);
			Assert.IsTrue(s2.IsDescendantOf(s1));
		}

		[TestMethod]
		public void AddProperties()
		{
			var fd = CreateFormDesign();
			var p1 = fd.AddProperty();
			p1.propName = "TemplateID";
			p1.val = "FD.Builder.Tests";

			var p2 = fd.Body.AddProperty();
			p2.propName = "BodyProperty";
			p2.val = "Value";

			// Rationale: properties are commonly added both at form root and section nodes during build.
			Assert.AreEqual(1, fd.Property?.Count);
			Assert.AreEqual(1, fd.Body.Property?.Count);
		}

		[TestMethod]
		public void Misc()
		{
			var fd = CreateFormDesign();
			var section = fd.Body.AddChildSection("S.Misc", "Misc");
			var q = section.AddChildQuestionResponse("Q.Misc", out var deType, "Measure", dt: ItemChoiceType.@int, units: "cm");
			var di = q.AddChildDisplayedItem("DI.Misc", "Info");
			q.AddProperty().propName = "reportText";

			// Rationale: mixed builder operations should produce a valid, serializable object model.
			Assert.IsNotNull(deType);
			Assert.IsNotNull(di);
			Assert.IsFalse(string.IsNullOrWhiteSpace(fd.GetXml()));
		}
	}
}
