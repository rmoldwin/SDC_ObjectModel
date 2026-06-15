using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OMTests
{
	[TestClass()]
	public class QuestionItemTypeTests
	{
		private static QuestionItemType CreateQuestion(QuestionEnum subtype = QuestionEnum.QuestionSingle)
		{
			var de = new DataElementType(null);
			var q = new QuestionItemType(de, "q");
			if (subtype == QuestionEnum.QuestionFill)
				q.AddQuestionResponseField(out _, ItemChoiceType.@string);
			else if (subtype == QuestionEnum.QuestionSingle || subtype == QuestionEnum.QuestionMultiple)
				q.GetListField();
			return q;
		}

		[TestMethod()]
		public void QuestionItemTypeTest()
		{
			var q = CreateQuestion();
			Assert.IsNotNull(q);
		}

		[TestMethod()]
		public void AddChildSectionTest()
		{
			var q = CreateQuestion();
			var s = q.AddChildSection("S1", "Section", 0);
			Assert.IsNotNull(s);
		}

		[TestMethod()]
		public void AddChildQuestionTest()
		{
			var q = CreateQuestion();
			var child = q.AddChildQuestion(QuestionEnum.QuestionSingle, "Q1");
			Assert.IsNotNull(child);
		}

		[TestMethod()]
		public void AddChildInjectedFormTest()
		{
			var q = CreateQuestion();
			var injected = q.AddChildInjectedForm("I1", 0);
			Assert.IsNotNull(injected);
		}

		[TestMethod()]
		public void AddChildButtonActionTest()
		{
			var q = CreateQuestion();
			var btn = q.AddChildButtonAction("B1", "Button", 0);
			Assert.IsNotNull(btn);
		}

		[TestMethod()]
		public void AddChildDisplayedItemTest()
		{
			var q = CreateQuestion();
			var di = q.AddChildDisplayedItem("D1", "Display", 0);
			Assert.IsNotNull(di);
		}

		[TestMethod()]
		public void HasChildItemsTest()
		{
			var q = CreateQuestion();
			Assert.IsNull(q.GetChildItemsList());
			q.AddChildSection("S1", "Section", 0);
			Assert.IsTrue((q.GetChildItemsList()?.Count ?? 0) > 0);
		}

		[TestMethod()]
		public void GetQuestionSubtypeTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			Assert.AreEqual(QuestionEnum.QuestionSingle, q.GetQuestionSubtype());
		}

		[TestMethod()]
		public void AddListItemTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			var li = q.AddListItem("LI1", "Item", 0);
			Assert.IsNotNull(li);
			Assert.AreEqual("LI1", li.ID);
		}

		[TestMethod()]
		public void AddListItemResponseTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			var li = q.AddListItemResponse("LIR1", out var dt, "LIR", 0);
			Assert.IsNotNull(li);
			Assert.IsNotNull(dt);
		}

		[TestMethod()]
		public void AddDisplayedTypeToListTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			var di = q.AddDisplayedTypeToList("DI1", "Display", 0);
			Assert.IsNotNull(di);
		}

		[TestMethod()]
		public void AddQuestionResponseFieldTest()
		{
			var q = new QuestionItemType(new DataElementType(null), "qr");
			var rf = q.AddQuestionResponseField(out var dt, ItemChoiceType.@string);
			Assert.IsNotNull(rf);
			Assert.IsNotNull(dt);
		}

		[TestMethod()]
		public void GetListFieldTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			var lf = q.GetListField();
			Assert.IsNotNull(lf);
		}

		[TestMethod()]
		public void GetListItemsTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			q.AddListItem("LI1", "Item", 0);
			Assert.IsTrue((q.GetListItems()?.Count ?? 0) > 0);
		}

		[TestMethod()]
		public void GetResponseDataTypeNodeTest()
		{
			var q = new QuestionItemType(new DataElementType(null), "qr");
			q.AddQuestionResponseField(out _, ItemChoiceType.@string);
			Assert.IsNotNull(q.GetResponseDataTypeNode());
		}

		[TestMethod()]
		public void GetListAndChildItemsListTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			q.AddListItem("LI1", "Item", 0);
			var nodes = q.GetListAndChildItemsList();
			Assert.IsNotNull(nodes);
			Assert.IsTrue(nodes!.Count > 0);
		}

		[TestMethod()]
		public void TryGetListAndChildIETNodesTest()
		{
			var q = CreateQuestion(QuestionEnum.QuestionSingle);
			q.AddListItem("LI1", "Item", 0);
			var ok = q.TryGetListAndChildIETNodes(out var nodes);
			Assert.IsTrue(ok);
			Assert.IsNotNull(nodes);
		}
	}

}
