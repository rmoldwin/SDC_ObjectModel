using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OMTests
{
	[TestClass()]
	public class ListItemTypeTests
	{
		private static ListItemType CreateListItem()
		{
			var de = new DataElementType(null);
			var q = new QuestionItemType(de, "q");
			return q.AddListItem("li");
		}

		[TestMethod()]
		public void ListItemTypeTest()
		{
			var li = CreateListItem();
			Assert.IsNotNull(li);
		}

		[TestMethod()]
		public void AddListItemResponseFieldTest()
		{
			var li = CreateListItem();
			var rf = li.AddListItemResponseField();
			Assert.IsNotNull(rf);
		}

		[TestMethod()]
		public void AddOnDeselectTest()
		{
			var li = CreateListItem();
			var ev = li.AddOnDeselect();
			Assert.IsNotNull(ev);
			Assert.AreEqual(1, li.OnDeselect.Count);
		}

		[TestMethod()]
		public void AddOnSelectTest()
		{
			var li = CreateListItem();
			var ev = li.AddOnSelect();
			Assert.IsNotNull(ev);
			Assert.AreEqual(1, li.OnSelect.Count);
		}

		[TestMethod()]
		public void AddSelectIfTest()
		{
			var li = CreateListItem();
			var guard = li.AddSelectIf();
			Assert.IsNotNull(guard);
		}

		[TestMethod()]
		public void AddDeselectIfTest()
		{
			var li = CreateListItem();
			var guard = li.AddDeselectIf();
			Assert.IsNotNull(guard);
		}

		[TestMethod()]
		public void GetResponseDataTypeNodeTest()
		{
			var li = CreateListItem();
			Assert.IsNull(li.GetResponseDataTypeNode());
		}

		[TestMethod()]
		public void AddChildSectionTest()
		{
			var li = CreateListItem();
			var section = li.AddChildSection("S1", "Section", 0);
			Assert.IsNotNull(section);
			Assert.AreEqual("S1", section.ID);
		}

		[TestMethod()]
		public void AddChildQuestionTest()
		{
			var li = CreateListItem();
			var q = li.AddChildQuestion(QuestionEnum.QuestionSingle, "Q1");
			Assert.IsNotNull(q);
			Assert.AreEqual("Q1", q.ID);
		}

		[TestMethod()]
		public void AddChildInjectedFormTest()
		{
			var li = CreateListItem();
			var injected = li.AddChildInjectedForm("I1", 0);
			Assert.IsNotNull(injected);
			Assert.AreEqual("I1", injected.ID);
		}

		[TestMethod()]
		public void AddChildButtonActionTest()
		{
			var li = CreateListItem();
			var btn = li.AddChildButtonAction("B1", "Button", 0);
			Assert.IsNotNull(btn);
			Assert.AreEqual("B1", btn.ID);
		}

		[TestMethod()]
		public void AddChildDisplayedItemTest()
		{
			var li = CreateListItem();
			var di = li.AddChildDisplayedItem("D1", "Display", 0);
			Assert.IsNotNull(di);
			Assert.AreEqual("D1", di.ID);
		}

		[TestMethod()]
		public void HasChildItemsTest()
		{
			var li = CreateListItem();
			Assert.IsNull(li.GetChildItemsList());
			li.AddChildSection("S1", "Section", 0);
			Assert.IsTrue((li.GetChildItemsList()?.Count ?? 0) > 0);
		}
	}
}
