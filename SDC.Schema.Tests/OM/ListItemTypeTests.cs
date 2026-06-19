using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OM
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

		/// <summary>
		/// Verifies the full ListItemResponse add-and-manipulate path, including setting a typed value
		/// on the data node and using <c>GetListItemByName</c> to look up the LIR node from the parent.
		/// Rationale: exercises the coded-rule lookup scenario where the response value determines selection state.
		/// </summary>
		[TestMethod()]
		public void AddListItemResponseField_WithResponseManipulation()
		{
			var de = new DataElementType(null);
			var q = new QuestionItemType(de, "q");
			de.Items.Add(q);
			var li = q.AddListItem("li");
			li.name = "li_Name";
			var rf = li.AddListItemResponseField();
			Assert.IsNotNull(rf);

			var lir = q.AddListItemResponse("lir", out var dt,
				"lir_Title", -1, ItemChoiceType.@string);
			lir.name = "lir_Name";
			var myStr = (string_DEtype)dt.Item;
			myStr.val = "test";
			myStr.maxLength = 4000;

			// Simulate a coded rule: if the string value is non-empty, mark as selected.
			var findLI = de.GetListItemByName("lir_Name");
			var dtLIR = findLI?.GetResponseDataTypeNode() as string_DEtype;
			if (dtLIR?.val is not null && !dtLIR.val.IsNullOrWhitespace())
				lir.selected = true;

			// Rationale: the LIR node must be findable by name and the selection flag must be set.
			Assert.IsNotNull(findLI, "GetListItemByName must return the LIR node by its assigned name.");
			Assert.IsTrue(lir.selected, "LIR must be marked selected when coded-rule condition is met.");
		}

		/// <summary>
		/// Verifies AddChildQuestionResponse on a ListItemType using an explicit second QuestionItemType
		/// to confirm the response inherits the correct parent (the LI, not the auxiliary question).
		/// </summary>
		[TestMethod()]
		public void AddChildQuestionResponseTest_ViaExplicitParentQuestion()
		{
			var de = new DataElementType(null);
			var q = new QuestionItemType(de, "q");
			var li = q.AddListItem("li");
			// q2 is constructed but not used as a parent — only li.AddChildQuestionResponse is tested.
			var q2 = new QuestionItemType(de) { ID = "QuestionResponseId", title = "This is a test question" };
			var qr = li.AddChildQuestionResponse("myID", out _, "", 1, ItemChoiceType.@string, "", "", dtQuantEnum.EQ, "");
			// Rationale: the returned node is non-null and attached under li's ChildItemsType container.
			// SDC never attaches directly to li; it always goes through the intermediate ChildItemsType node.
			Assert.IsNotNull(qr);
			Assert.IsInstanceOfType<ChildItemsType>(qr.ParentNode,
				"SDC attaches child questions through a ChildItemsType container, not directly to the list item");
			Assert.AreSame(li, qr.ParentNode?.ParentNode,
				"The ChildItemsType container must be owned by li");
		}
	}
}
