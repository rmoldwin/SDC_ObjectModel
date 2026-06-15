using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class FormDesignBuilderStubTests
	{
		private static FormDesignType CreateFormDesign()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Builder.StubTests");
			fd.AddBody();
			return fd;
		}

		[TestMethod]
		public void AssignXmlNamesTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.AssignNames", "Question");
			q.AddListItem("LI.AssignNames", "Item");
			fd.AssignElementNamesByReflection();
			Assert.IsTrue(fd.Nodes.Values.All(n => !string.IsNullOrWhiteSpace(n.ElementName)));
		}

		[TestMethod]
		public void AssignNamesFromXmlDocTest()
		{
			// Bug fix: use a per-test fresh XML source and avoid shared Setup.FD warm-state dependency.
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			fd.AssignElementNamesByReflection();
			Assert.IsTrue(fd.Nodes.Values.All(n => !string.IsNullOrWhiteSpace(n.ElementName)));
		}

		[TestMethod]
		public void AddRemoveHeaderTest()
		{
			var fd = CreateFormDesign();
			var header = fd.AddHeader();
			Assert.IsNotNull(fd.Header);
			Assert.AreSame(header, fd.AddHeader());
			fd.Header = null;
			Assert.IsNull(fd.Header);
		}

		[TestMethod]
		public void AddRemoveFooterTest()
		{
			var fd = CreateFormDesign();
			var footer = fd.AddFooter();
			Assert.IsNotNull(fd.Footer);
			Assert.AreSame(footer, fd.AddFooter());
			fd.Footer = null;
			Assert.IsNull(fd.Footer);
		}

		[TestMethod]
		public void AddQuestionsTest()
		{
			var fd = CreateFormDesign();
			var qs = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Single", "Single");
			var qm = fd.Body.AddChildQuestion(QuestionEnum.QuestionMultiple, "Q.Multiple", "Multiple");
			var qf = fd.Body.AddChildQuestion(QuestionEnum.QuestionFill, "Q.Fill", "Fill");
			Assert.AreEqual(QuestionEnum.QuestionSingle, qs.GetQuestionSubtype());
			Assert.AreEqual(QuestionEnum.QuestionMultiple, qm.GetQuestionSubtype());
			Assert.AreEqual(QuestionEnum.QuestionFill, qf.GetQuestionSubtype());
		}

		[TestMethod]
		public void AddListItemToQuestionListTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.List", "List Question");
			var li = q.AddListItem("LI.1", "Choice 1");
			Assert.AreEqual("LI.1", li.ID);
			Assert.AreEqual(1, q.GetListItems()?.Count);
		}

		[TestMethod]
		public void AddListItemOnListItemTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent");
			var li = q.AddListItem("LI.Parent", "Parent Item");
			var childQuestion = li.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Child", "Child Question");
			var nestedLi = childQuestion.AddListItem("LI.Child", "Child Item");
			Assert.AreEqual("LI.Child", nestedLi.ID);
		}

		[TestMethod]
		public void AdListItemOnDisplayedItemTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.DIHost", "Host");
			var di = q.AddDisplayedTypeToList("DI.Parent", "Displayed");
			var li = q.AddListItem("LI.OnDI", "Choice");
			Assert.AreEqual("DI.Parent", di.ID);
			Assert.AreEqual("LI.OnDI", li.ID);
		}

		[TestMethod]
		public void AddDisplayedItemToQuestionListTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.WithDI", "Question");
			var di = q.AddDisplayedTypeToList("DI.InList", "Shown in list");
			Assert.AreEqual("DI.InList", di.ID);
		}

		[TestMethod]
		public void AddDisplayedItemOnListItemTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.WithLI", "Question");
			var li = q.AddListItem("LI.WithDI", "Item");
			var di = li.AddChildDisplayedItem("DI.OnLI", "Display under LI");
			Assert.AreEqual("DI.OnLI", di.ID);
		}

		[TestMethod]
		public void AddDisplayedItemOnDisplayedItemTest()
		{
			var fd = CreateFormDesign();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.DIParent", "Host");
			var parentDi = q.AddDisplayedTypeToList("DI.Parent2", "Parent");
			var siblingDi = q.AddDisplayedTypeToList("DI.Child", "Child");
			Assert.AreEqual("DI.Parent2", parentDi.ID);
			Assert.AreEqual("DI.Child", siblingDi.ID);
		}

		[TestMethod]
		public void AddDisplayedItemAsChildTest()
		{
			var fd = CreateFormDesign();
			var di = fd.Body.AddChildDisplayedItem("DI.Body", "Body child");
			Assert.AreEqual("DI.Body", di.ID);
			Assert.IsTrue(di.IsDescendantOf(fd.Body));
		}

		[TestMethod]
		public void AddQuestionAsChildTest()
		{
			var fd = CreateFormDesign();
			var section = fd.Body.AddChildSection("S.1", "Section");
			var qOnSection = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.OnSection", "Q");
			var li = qOnSection.AddListItem("LI.1", "LI");
			var qOnLi = li.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.OnLI", "Q");
			var diInList = qOnSection.AddDisplayedTypeToList("DI.InList", "DI");
			var qOnQ = qOnSection.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.OnQ", "Q");
			Assert.IsNotNull(qOnLi);
			Assert.IsNotNull(qOnQ);
			Assert.IsFalse(diInList is IChildItemsParent);
		}

		[TestMethod]
		public void AddSectionAsChildTest()
		{
			var fd = CreateFormDesign();
			var s1 = fd.Body.AddChildSection("S.1", "Section 1");
			var s2 = s1.AddChildSection("S.2", "Section 2");
			Assert.AreEqual("S.2", s2.ID);
			Assert.IsTrue(s2.IsDescendantOf(s1));
		}

		[TestMethod]
		public void AddPropertiesTest()
		{
			var fd = CreateFormDesign();
			var p1 = fd.AddProperty();
			p1.propName = "TemplateID";
			p1.val = "FD.Builder.StubTests";
			var p2 = fd.Body.AddProperty();
			p2.propName = "BodyProperty";
			p2.val = "Value";
			Assert.AreEqual(1, fd.Property?.Count);
			Assert.AreEqual(1, fd.Body.Property?.Count);
		}

		[TestMethod]
		public void MiscTest()
		{
			var fd = CreateFormDesign();
			var section = fd.Body.AddChildSection("S.Misc", "Misc");
			var q = section.AddChildQuestionResponse("Q.Misc", out var deType, "Measure", dt: ItemChoiceType.@int, units: "cm");
			var di = q.AddChildDisplayedItem("DI.Misc", "Info");
			q.AddProperty().propName = "reportText";
			Assert.IsNotNull(deType);
			Assert.IsNotNull(di);
			Assert.IsFalse(string.IsNullOrWhiteSpace(fd.GetXml()));
		}
	}
}
