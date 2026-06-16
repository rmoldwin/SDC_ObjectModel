using CommandLine.Text;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SDC.Type.Interfaces;
using SDC.Schema;
using SDC.Schema.Extensions;
using SDC.Schema.UtilityClasses.Extensions;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace SDC.Schema.Tests.OMTests
{
    [TestClass]
    public class QuestionItemTypeTest
    {
        [TestMethod]
        public void QuestionItemTypeTest_AddButtonAction()
		{
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);

            var testItem = qi.AddChildButtonAction("test_id", "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddDisplayItem()
		{
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);

			var testItem = qi.AddChildDisplayedItem("test_id","", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddDisplayedTypeToList()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);

            //var rf = qi.AddQuestionResponseField(out _);  // ResponseField_Item = new ListItemResponseFieldType(null);

            var testItem = qi.AddDisplayedTypeToList("test_id", "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddInjectedForm()
        {
			var de = new DataElementType(null);
			//QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);
			var qi = de.AddQuestion(QuestionEnum.QuestionSingle, "qi","qi" );
			var testItem = qi.AddChildInjectedForm("test_id", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddListItem()
        {
			var de = new DataElementType(null);
			//QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);
			var qi = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");
			//var rf = qi.AddQuestionResponseField(out _);  // (qi as QuestionItemType).ResponseField_Item = new ListItemResponseFieldType(null);
			//qi.GetListField().maxSelections = 1; //(qi as QuestionItemType).ListField_Item = new ListFieldType(null) { maxSelections = 1 };
            var testItem = qi.AddListItem("test_id", "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddListItemResponse()
        {
			var de = new DataElementType(null);
			//QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);
			var qi = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");
			var testItem = qi.AddListItemResponse("test_id", out _, "test_id", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddQuestion()
        {
			var de = new DataElementType(null);
			//QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);
			var qi = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");

			var q = qi.AddChildQuestion(QuestionEnum.QuestionSingle, "q1");
            Assert.IsNotNull(q);
            Assert.AreEqual("q1", q.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddQuestionResponse()
        {
			var de = new DataElementType(null);
			//QuestionItemType q1 = new QuestionItemType(de);
			//de.DataElement_Items.Add(q1);
			var q1 = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");
			//var q2 = new QuestionItemType(de)
   //         {
   //             ID = "QuestionResponseId",
   //             title = "This is a test question"
   //         };
			//de.Items.Add(q2);

            var qr = q1.AddChildQuestionResponse("myQ1", out _, "myTitle", 1, ItemChoiceType.@string, "textAfter", "units", dtQuantEnum.EQ, "default");
            Assert.IsNotNull(qr);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddSection()
        {
			var de = new DataElementType(null);
			//QuestionItemType qi = new QuestionItemType(de);
			//de.DataElement_Items.Add(qi);
			var qi = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");
			var s = qi.AddChildSection("test_id", "", 1);
            Assert.IsNotNull(s);
            Assert.AreEqual("test_id", s.ID);
        }
		
        [TestMethod]
		public void QuestionItemTypeTest_SRS521_AddListItemResponsetoQuestion()
		{
			var de = new DataElementType(null, "DE1");
            
			//var q = new QuestionItemType(de, "q1", "q1");
            //de.Items.Add(q);
			var q = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");


			q.AddListItem("li0", "li0"); //first
			q.AddListItem("li10", "li10"); //last
			q.AddListItem("li9", "li9", 1);
			q.AddListItem("li1", "li1", 1);
			q.AddListItem("li2", "li2", 2);
			q.AddListItemResponse("li3", out var dt, "lir3", 3
				, ItemChoiceType.@string, true, "text After"
				, "myUnits", dtQuantEnum.GT, "myDefaultValue");
			q.AddListItem("li4", "li4", 4);
			q.AddListItem("li5", "li5", 5);
			q.AddListItem("li6", "li6", 6);
			q.AddListItem("li7", "li7", 7);
			q.AddListItem("li8", "li8", 8);

			Assert.AreEqual(q.GetListItems()?.Last().title, "li10");
			Assert.AreEqual(q.GetListItems()?.First().title, "li0");
			Assert.AreEqual(q.GetListItems()?[4].title, "li4");
		}
		[TestMethod]
		public void QuestionItemTypeTest_SRS519_Add3LIRsToQuestion()
		{
			var de = new DataElementType(null, "DE1");

			//var q = new QuestionItemType(de, "q1", "q1");
			//de.Items.Add(q);
			var q = de.AddQuestion(QuestionEnum.QuestionSingle, "qi", "qi");

			var li0 = q.AddListItem("li0", "li0"); //first
			var li10 = q.AddListItem("li10", "li10"); //should finish up in last position [10]
			var li9 = q.AddListItem("li9", "li9", 1);
			var li1 = q.AddListItem("li1", "li1", 1);
			var li2 = q.AddListItem("li2", "li2", 2);
			var lir3 = q.AddListItemResponse("lir3", out var dt3string, "lir3", 3
				, ItemChoiceType.@string, true, "text After"
				, "myUnits", dtQuantEnum.GT, "myDefaultValue");
			var lir4 = q.AddListItemResponse("lir4", out var dt4bool, "lir4", 4
				, ItemChoiceType.boolean, true, "text After"
				, "myUnits", dtQuantEnum.GT, "myDefaultValue");
			var lir5 = q.AddListItemResponse("lir5", out var dt5int, "lir5", 5
				, ItemChoiceType.@int, true, "text After"
				, "myUnits", dtQuantEnum.GT, "myDefaultValue");
			var li6 = q.AddListItem("li6", "li6", 6);
			var li7 = q.AddListItem("li7", "li7", 7);
			var li8 = q.AddListItem("li8", "li8", 8);

			Assert.AreEqual(q.GetListItems()?.Last().title, "li10");
			Assert.AreEqual(q.GetListItems()?.First().title, "li0");
			Assert.AreEqual(q.GetListItems()?[4].title, "lir4");

			lir3.title = "lir3 title";
			dt3string.Item.As<string_DEtype>().maxLength = 4000;
			
			lir4.title = "lir4 title";
			dt4bool.Item.As<boolean_DEtype>().val = true;

			string s = lir5.ListItemResponseField.BaseName;





			//Step to repro the error:
			//Add a QM to the template tree
			//Add multiple LIR(I added 3 in this example) to the QM
			//Edit the LI and LIR template tree items
			//After editing first LIR , I went to the second LIR
			//When I clicked on the “Title” text box for the second LIR, the pop - up error message appeared
			/*BUG:
			LIR SetResponseDataType [String]: InvalidOperation_IComparerFailed 
			at System.Collections.Generic.ArraySortHelper'1 [[SDC.Schema.BaseType, SDC.Schema, Version=2.2023.1.26, Culture=neutral, PublicKeyToken=null]].Sort(Span'1 , IComparer‘1 )
			at System.Array.Sort[BaseType](BaseType[] array, Int32 index, Int32 length, IComparer‘1 comparer)
			at System.Collections.Generic.List‘1 [[SDC.Schema.BaseType, SDC.Schema, Version=2.2023.1.26, Culture=neutral, PublicKeyToken=null]].Sort(lnt32 , Int32 , IComparer‘1 ) 
			at System.Collections.Generic.List‘1 [[SDC.Schema.BaseType, SDC.Schema, Version=2.2023.1.26, Culture=neutral, PublicKeyToken=null]].Sort(IComparer'1 )

			at SDC.Schema.Extensions.IMoveRemoveExtensions.<RegisterParent>g__RegisterParentNode|9_0(BaseType btSource, BaseType inParentNode, JTopNode tn, Boolean childNodesSort) 
			at SDC.Schema.Extensions.IMoveRemoveExtensions.RegisterParent(BaseType node, BaseType inParentNode, Boolean childNodesSort) 
			at SDC.Schema.Extensions.IMoveRemoveExtensions.RegisterNodeAndParentfBaseType node, BaseType parentNode, Boolean childNodesSort, Boolean isMoving) 
			at SDC.Schema.BaseType..ctor(BaseType parentNode) 
			at SDC.Schema.ExtensionBaseType..ctor(BaseType parentNode) 
			at SDC.Schema.DataTypes_DEType..ctor(ResponseFieldType parentNode) 
			at SDC.Schema.IDataHelpers.AddDataTypesDEfResponseFieldType rfParent, ItemChoiceType dataTypeEnum, dtQuantEnum quantifierEnum, Object value) 
			at SDC.Schema.Extensions.IResponseFieldExtensions.AddDataType(ResponseFieldType rf, ItemChoiceType dataType, dtQuantEnum dtQuant, Object valDefault) 
			at Template.Editor.Library.Helpers.UpdateHelper.SetResponseDataType(ITreeltem treeltem, ListltemResponseFieldType listltemResponseField)
			*/



		}
		[TestMethod]
		public void QuestionItemTypeTest__AddLItoPosition0()
		{
			var de = new DataElementType(null, "DE1");
			var q2 = de.AddQuestion(QuestionEnum.QuestionSingle, "q2", "q2", 0);//should finish in pos.[2]
			Assert.AreSame(de.IETnodes[1], q2); //[1] is the first q node under de (which is the [0] node]
			var q1 = de.AddQuestion(QuestionEnum.QuestionSingle, "q1", "q1", 0);//should finish in pos.[1]
			Assert.AreSame(de.IETnodes[1], q1);
			Assert.AreSame(de.IETnodes[2], q2);
			var q0 = de.AddQuestion(QuestionEnum.QuestionSingle, "q0", "q0", 0);//should finish in pos.[0]
			Assert.AreSame(de.IETnodes[1], q0);
			Assert.AreSame(de.IETnodes[2], q1);
			Assert.AreSame(de.IETnodes[3], q2);
			var q3 = de.AddQuestion(QuestionEnum.QuestionSingle, "q3", "q3", 3);//should finish in pos.[3]
			Assert.AreSame(de.IETnodes[1], q0);
			Assert.AreSame(de.IETnodes[2], q1);
			Assert.AreSame(de.IETnodes[3], q2);
			Assert.AreSame(de.IETnodes[4], q3);



			var li2 = q0.AddListItem("li2", "li2"); //should finish up in position [2]
			Assert.AreSame(de.IETnodes[2], li2);
			var li1 = q0.AddListItem("li1", "li1", 0); //should finish up in position [1]
			Assert.AreSame(de.IETnodes[2], li1);
			Assert.AreSame(de.IETnodes[3], li2);
			var li0 = q0.AddListItem("li0", "li0", 0);//should finish up in position [0]
			Assert.AreSame(de.IETnodes[2], li0);
			Assert.AreSame(de.IETnodes[3], li1);
			Assert.AreSame(de.IETnodes[4], li2);

			// After inserting list items under q0, q0 itself remains the first question node in the IET list.
			Assert.AreSame(de.IETnodes[1], q0);
			Assert.AreSame(de.IETnodes[5], q1);
			Assert.AreSame(de.IETnodes[6], q2);
			Assert.AreSame(de.IETnodes[7], q3);


			Assert.AreEqual(de.DataElement_Items.Last().As<DisplayedType>().title, "q3");
			Assert.AreEqual(de.DataElement_Items[1].As<DisplayedType>().title, "q1");
			Assert.AreEqual(de.DataElement_Items.First().As<DisplayedType>().title, "q0");
			Assert.AreEqual(de.DataElement_Items[2].As<DisplayedType>().title, "q2");

			Assert.AreEqual(q0.GetListItems()?.Last().title, "li2");
			Assert.AreEqual(q0.GetListItems()?[1].title, "li1");
			Assert.AreEqual(q0.GetListItems()?.First().title, "li0");


		}
		[TestMethod]
		public void QuestionResponseTest_SRS524_ChangeDatatype()
		{
			var de = new DataElementType(null, "DE1");
			//var q1 = new QuestionItemType(de, "q1", "q1");
			//de.Items.Add(q1); //must explicitly add q1 to Items before creating q2 - will throw if this is not done - due to error in sorting.  
			//Need to fix this in "Items" override code
			var q1 = de.AddQuestion(QuestionEnum.QuestionSingle, "q1", "q1");

			//var q2 = new QuestionItemType(de, "q2", "q2");			
			//de.Items.Add(q2);
			var q2 = de.AddQuestion(QuestionEnum.QuestionSingle, "q2", "q2");
			var q3 = de.AddQuestion(QuestionEnum.QuestionRaw, "q3", "q3");
			var q4 = de.AddQuestion(QuestionEnum.QuestionRaw, "q4", "q4");


			var rf3 = q3.AddQuestionResponseField(out var dt1, ItemChoiceType.date);
			var rf4 = q4.AddQuestionResponseField(out var dt2, ItemChoiceType.@int);

			var response3 = rf3.Response;
			var response4 = rf4.Response;
			//response1.Item = new string_DEtype(null); //error - does not run ItemMutator
			//response1.DataTypeDE_Item = new string_DEtype(null); //note the null parent - this node is not completely initialized

			response3.DataTypeDE_Item = new string_DEtype(response3); //
			Assert.AreSame(response3.Item.ParentNode, response3); //Check that ItemMutator detected and fixed the parent node for Item
			Assert.IsTrue(de.GetChildNodes()?[0] == q1); //Show the the _ChildNodes dict has an entry for Item
			Assert.IsTrue(de.GetChildNodes()?[1] == q2); //Show the the _ChildNodes dict has an entry for Item
			Assert.IsTrue(de.GetChildNodes()?[2] == q3); //Show the the _ChildNodes dict has an entry for Item
			Assert.IsTrue(de.GetChildNodes()?[3] == q4); //Show the the _ChildNodes dict has an entry for Item
			response4.DataTypeDE_Item = new boolean_DEtype(response3); //response3 is the wrong parent - this will test if it's detected and fixed by ItemMutator
			Assert.AreSame(response4.Item.ParentNode, response4);




		}

		[TestMethod]
		public void ItemMutator_SameTreeReparent_UpdatesParentAndClearsFormerOwner()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null, "DE_ItemMutator_SameTree");
			var q1 = de.AddQuestion(QuestionEnum.QuestionRaw, "Q1", "Q1");
			var q2 = de.AddQuestion(QuestionEnum.QuestionRaw, "Q2", "Q2");
			var rf1 = q1.AddQuestionResponseField(out _, ItemChoiceType.@string);
			var rf2 = q2.AddQuestionResponseField(out _, ItemChoiceType.@string);

			// Create a datatype node attached under response1, then reassign it to response2.
			// Rationale: this exercises the same-tree ItemMutator branch that must detach from the old owner
			// and fix ParentNode dictionary links without corrupting the top-node dictionaries.
			var moved = new boolean_DEtype(rf1.Response);
			var oldGuid = moved.ObjectGUID;
			rf2.Response.DataTypeDE_Item = moved;

			Assert.AreSame(rf2.Response, moved.ParentNode, "Moved datatype must be re-parented to the new Response node.");
			Assert.IsNull(rf1.Response.Item, "Former owner Response should no longer hold the moved datatype node.");
			Assert.IsTrue(de.Nodes.ContainsKey(oldGuid), "Moved node must remain registered in the top-node dictionary after reassignment.");
		}

		[TestMethod]
		public void ItemMutator_CrossTreeReparent_MovesNodeToTargetTree()
		{
			BaseType.ResetLastTopNode();
			var source = new DataElementType(null, "DE_Source");
			var sourceQuestion = source.AddQuestion(QuestionEnum.QuestionRaw, "QS", "QS");
			var sourceResponseField = sourceQuestion.AddQuestionResponseField(out _, ItemChoiceType.@string);
			var donor = new string_DEtype(sourceResponseField.Response);
			var donorGuidBeforeMove = donor.ObjectGUID;

			var target = new DataElementType(null, "DE_Target");
			var targetQuestion = target.AddQuestion(QuestionEnum.QuestionRaw, "QT", "QT");
			var targetResponseField = targetQuestion.AddQuestionResponseField(out _, ItemChoiceType.@string);

			// Rationale: cross-tree assignment must move the node into the target tree, including dictionary ownership.
			targetResponseField.Response.DataTypeDE_Item = donor;

			Assert.AreSame(targetResponseField.Response, donor.ParentNode, "Cross-tree moved node must attach to target Response.");
			Assert.IsNull(sourceResponseField.Response.Item, "Source Response should no longer reference the moved node.");
			Assert.IsFalse(source.Nodes.ContainsKey(donorGuidBeforeMove), "Source tree must no longer contain moved node dictionary entry.");
			Assert.IsTrue(target.Nodes.ContainsKey(donor.ObjectGUID), "Target tree must contain moved node dictionary entry after reassignment.");
		}

		[TestMethod]
		public void ItemsMutator_ListReplacement_UpdatesNodeRegistration()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null, "DE_ItemsMutator");
			var qOld = de.AddQuestion(QuestionEnum.QuestionSingle, "Q_Old", "Old");
			var qOldGuid = qOld.ObjectGUID;

			var donor = new DataElementType(null, "DE_Donor");
			var incoming = donor.AddQuestion(QuestionEnum.QuestionSingle, "Q_New", "New");
			var incomingGuidBeforeMove = incoming.ObjectGUID;

			// Replace the DataElement question list (ItemsMutator path).
			// Rationale: verifies old nodes are unregistered and incoming nodes are re-registered under new parent.
			de.DataElement_Items = new List<IdentifiedExtensionType> { incoming };

			Assert.AreEqual(1, de.DataElement_Items.Count, "DataElement list replacement should keep only the incoming node.");
			Assert.AreSame(incoming, de.DataElement_Items[0], "Incoming node should become the sole list member after replacement.");
			Assert.AreSame(de, incoming.ParentNode, "Incoming node must be re-parented to the target DataElement.");
			Assert.IsFalse(de.Nodes.ContainsKey(qOldGuid), "Old list member should be removed from the target top-node dictionary.");
			Assert.IsFalse(donor.Nodes.ContainsKey(incomingGuidBeforeMove), "Donor tree should no longer track the moved incoming node.");
			Assert.IsTrue(de.Nodes.ContainsKey(incoming.ObjectGUID), "Target tree should register the incoming node after replacement.");
		}
	}
}
