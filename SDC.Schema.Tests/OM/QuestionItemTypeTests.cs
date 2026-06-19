using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using SDC.Schema.UtilityClasses.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SDC.Schema.Tests.OM
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

		/// <summary>
		/// SRS-521 regression: verifies ordering is preserved when mixing ListItems and
		/// a ListItemResponse in a single-select question list.
		/// </summary>
		[TestMethod()]
		public void SRS521_AddListItemResponseToQuestion()
		{
			var de = new DataElementType(null, "DE1");
			var q = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");

			q.AddListItem("li0", "li0");     // first
			q.AddListItem("li10", "li10");   // last before insertions
			q.AddListItem("li9", "li9", 1);
			q.AddListItem("li1", "li1", 1);
			q.AddListItem("li2", "li2", 2);
			q.AddListItemResponse("li3", out var dt, "lir3", 3,
				ItemChoiceType.@string, true, "text After", "myUnits", dtQuantEnum.GT, "myDefaultValue");
			q.AddListItem("li4", "li4", 4);
			q.AddListItem("li5", "li5", 5);
			q.AddListItem("li6", "li6", 6);
			q.AddListItem("li7", "li7", 7);
			q.AddListItem("li8", "li8", 8);

			// Rationale: last item must remain "li10" regardless of insertions at earlier positions.
			Assert.AreEqual("li10", q.GetListItems()?.Last().title);
			// Rationale: first item must remain "li0".
			Assert.AreEqual("li0", q.GetListItems()?.First().title);
			// Rationale: item at index 4 must be "li4" after ordered insertions.
			Assert.AreEqual("li4", q.GetListItems()?[4].title);
		}

		/// <summary>
		/// SRS-519 regression: verifies ordering is preserved with three consecutive
		/// ListItemResponses of different data types interspersed with plain ListItems.
		/// </summary>
		[TestMethod()]
		public void SRS519_Add3LIRsToQuestion()
		{
			var de = new DataElementType(null, "DE1");
			var q = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");

			q.AddListItem("li0", "li0");
			q.AddListItem("li10", "li10");
			q.AddListItem("li9", "li9", 1);
			q.AddListItem("li1", "li1", 1);
			q.AddListItem("li2", "li2", 2);
			var lir3 = q.AddListItemResponse("lir3", out var dt3string, "lir3", 3,
				ItemChoiceType.@string, true, "text After", "myUnits", dtQuantEnum.GT, "myDefaultValue");
			var lir4 = q.AddListItemResponse("lir4", out var dt4bool, "lir4", 4,
				ItemChoiceType.boolean, true, "text After", "myUnits", dtQuantEnum.GT, "myDefaultValue");
			var lir5 = q.AddListItemResponse("lir5", out var dt5int, "lir5", 5,
				ItemChoiceType.@int, true, "text After", "myUnits", dtQuantEnum.GT, "myDefaultValue");
			q.AddListItem("li6", "li6", 6);
			q.AddListItem("li7", "li7", 7);
			q.AddListItem("li8", "li8", 8);

			// Rationale: position-ordering invariants after mixed-type insertions.
			Assert.AreEqual("li10", q.GetListItems()?.Last().title);
			Assert.AreEqual("li0", q.GetListItems()?.First().title);
			Assert.AreEqual("lir4", q.GetListItems()?[4].title);

			lir3.title = "lir3 title";
			dt3string.Item.As<string_DEtype>().maxLength = 4000;
			lir4.title = "lir4 title";
			dt4bool.Item.As<boolean_DEtype>().val = true;
			// Rationale: BaseName must be accessible on the response field without exception.
			_ = lir5.ListItemResponseField.BaseName;
		}

		/// <summary>
		/// Verifies that AddListItem correctly inserts items at position 0 and shifts existing items right,
		/// and that IETnodes indices update consistently with DataElement_Items ordering.
		/// </summary>
		[TestMethod()]
		public void AddListItemToPosition0()
		{
			var de = new DataElementType(null, "DE1");
			var q2 = de.AddQuestion(QuestionEnum.QuestionSingle, "q2", "q2", 0);
			// Rationale: q2 is the first child question node.
			Assert.AreSame(de.IETnodes[1], q2);
			var q1 = de.AddQuestion(QuestionEnum.QuestionSingle, "q1", "q1", 0);
			Assert.AreSame(de.IETnodes[1], q1);
			Assert.AreSame(de.IETnodes[2], q2);
			var q0 = de.AddQuestion(QuestionEnum.QuestionSingle, "q0", "q0", 0);
			Assert.AreSame(de.IETnodes[1], q0);
			Assert.AreSame(de.IETnodes[2], q1);
			Assert.AreSame(de.IETnodes[3], q2);
			var q3 = de.AddQuestion(QuestionEnum.QuestionSingle, "q3", "q3", 3);
			Assert.AreSame(de.IETnodes[1], q0);
			Assert.AreSame(de.IETnodes[2], q1);
			Assert.AreSame(de.IETnodes[3], q2);
			Assert.AreSame(de.IETnodes[4], q3);

			var li2 = q0.AddListItem("li2", "li2");
			Assert.AreSame(de.IETnodes[2], li2);
			var li1 = q0.AddListItem("li1", "li1", 0);
			Assert.AreSame(de.IETnodes[2], li1);
			Assert.AreSame(de.IETnodes[3], li2);
			var li0 = q0.AddListItem("li0", "li0", 0);
			Assert.AreSame(de.IETnodes[2], li0);
			Assert.AreSame(de.IETnodes[3], li1);
			Assert.AreSame(de.IETnodes[4], li2);

			// Rationale: q0 itself must still be at IETnodes[1] after list-item insertions beneath it.
			Assert.AreSame(de.IETnodes[1], q0);
			Assert.AreSame(de.IETnodes[5], q1);
			Assert.AreSame(de.IETnodes[6], q2);
			Assert.AreSame(de.IETnodes[7], q3);

			Assert.AreEqual("q3", de.DataElement_Items.Last().As<DisplayedType>().title);
			Assert.AreEqual("q1", de.DataElement_Items[1].As<DisplayedType>().title);
			Assert.AreEqual("q0", de.DataElement_Items.First().As<DisplayedType>().title);
			Assert.AreEqual("q2", de.DataElement_Items[2].As<DisplayedType>().title);

			Assert.AreEqual("li2", q0.GetListItems()?.Last().title);
			Assert.AreEqual("li1", q0.GetListItems()?[1].title);
			Assert.AreEqual("li0", q0.GetListItems()?.First().title);
		}

		/// <summary>
		/// SRS-524 regression: verifies that reassigning <c>DataTypeDE_Item</c> via ItemMutator
		/// updates ParentNode correctly and does not corrupt child-node ordering.
		/// </summary>
		[TestMethod()]
		public void SRS524_ChangeDatatype()
		{
			var de = new DataElementType(null, "DE1");
			var q1 = de.AddQuestion(QuestionEnum.QuestionSingle, "q1", "q1");
			var q2 = de.AddQuestion(QuestionEnum.QuestionSingle, "q2", "q2");
			var q3 = de.AddQuestion(QuestionEnum.QuestionRaw, "q3", "q3");
			var q4 = de.AddQuestion(QuestionEnum.QuestionRaw, "q4", "q4");

			var rf3 = q3.AddQuestionResponseField(out var dt1, ItemChoiceType.date);
			var rf4 = q4.AddQuestionResponseField(out var dt2, ItemChoiceType.@int);

			var response3 = rf3.Response;
			var response4 = rf4.Response;

			response3.DataTypeDE_Item = new string_DEtype(response3);
			// Rationale: ItemMutator must fix ParentNode for the newly assigned item.
			Assert.AreSame(response3.Item.ParentNode, response3);
			// Rationale: child-node ordering must be intact in the DataElement after the assignment.
			Assert.IsTrue(de.GetChildNodes()?[0] == q1);
			Assert.IsTrue(de.GetChildNodes()?[1] == q2);
			Assert.IsTrue(de.GetChildNodes()?[2] == q3);
			Assert.IsTrue(de.GetChildNodes()?[3] == q4);
			// Rationale: intentionally passing wrong parent (response3) to test ItemMutator's parent-fix logic.
			response4.DataTypeDE_Item = new boolean_DEtype(response3);
			Assert.AreSame(response4.Item.ParentNode, response4);
		}

		/// <summary>
		/// Verifies same-tree ItemMutator reassignment updates ParentNode and clears the former owner's slot.
		/// </summary>
		[TestMethod()]
		public void ItemMutator_SameTreeReparent_UpdatesParentAndClearsFormerOwner()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null, "DE_ItemMutator_SameTree");
			var q1 = de.AddQuestion(QuestionEnum.QuestionRaw, "Q1", "Q1");
			var q2 = de.AddQuestion(QuestionEnum.QuestionRaw, "Q2", "Q2");
			var rf1 = q1.AddQuestionResponseField(out _, ItemChoiceType.@string);
			var rf2 = q2.AddQuestionResponseField(out _, ItemChoiceType.@string);

			var moved = new boolean_DEtype(rf1.Response);
			var oldGuid = moved.ObjectGUID;
			rf2.Response.DataTypeDE_Item = moved;

			// Rationale: moved datatype must be re-parented to the new Response node.
			Assert.AreSame(rf2.Response, moved.ParentNode);
			// Rationale: former owner Response should no longer hold the moved node.
			Assert.IsNull(rf1.Response.Item);
			// Rationale: node must remain registered in the top-node dictionary after reassignment.
			Assert.IsTrue(de.Nodes.ContainsKey(oldGuid));
		}

		/// <summary>
		/// Verifies cross-tree ItemMutator reassignment moves dictionary ownership to the target tree.
		/// </summary>
		[TestMethod()]
		public void ItemMutator_CrossTreeReparent_MovesNodeToTargetTree()
		{
			BaseType.ResetLastTopNode();
			var source = new DataElementType(null, "DE_Source");
			var sourceQ = source.AddQuestion(QuestionEnum.QuestionRaw, "QS", "QS");
			var sourceRF = sourceQ.AddQuestionResponseField(out _, ItemChoiceType.@string);
			var donor = new string_DEtype(sourceRF.Response);
			var donorGuid = donor.ObjectGUID;

			var target = new DataElementType(null, "DE_Target");
			var targetQ = target.AddQuestion(QuestionEnum.QuestionRaw, "QT", "QT");
			var targetRF = targetQ.AddQuestionResponseField(out _, ItemChoiceType.@string);

			// Rationale: cross-tree assignment must move the node into the target tree.
			targetRF.Response.DataTypeDE_Item = donor;

			Assert.AreSame(targetRF.Response, donor.ParentNode);
			Assert.IsNull(sourceRF.Response.Item);
			Assert.IsFalse(source.Nodes.ContainsKey(donorGuid));
			Assert.IsTrue(target.Nodes.ContainsKey(donor.ObjectGUID));
		}

		/// <summary>
		/// Verifies ItemsMutator list replacement unregisters old nodes and re-registers incoming nodes.
		/// </summary>
		[TestMethod()]
		public void ItemsMutator_ListReplacement_UpdatesNodeRegistration()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null, "DE_ItemsMutator");
			var qOld = de.AddQuestion(QuestionEnum.QuestionSingle, "Q_Old", "Old");
			var qOldGuid = qOld.ObjectGUID;

			var donor = new DataElementType(null, "DE_Donor");
			var incoming = donor.AddQuestion(QuestionEnum.QuestionSingle, "Q_New", "New");
			var incomingGuid = incoming.ObjectGUID;

			// Rationale: list-level replacement must unregister old nodes and reparent incoming nodes.
			de.DataElement_Items = new System.Collections.Generic.List<IdentifiedExtensionType> { incoming };

			Assert.AreEqual(1, de.DataElement_Items.Count);
			Assert.AreSame(incoming, de.DataElement_Items[0]);
			Assert.AreSame(de, incoming.ParentNode);
			Assert.IsFalse(de.Nodes.ContainsKey(qOldGuid));
			Assert.IsFalse(donor.Nodes.ContainsKey(incomingGuid));
			Assert.IsTrue(de.Nodes.ContainsKey(incoming.ObjectGUID));
		}
	}

}
