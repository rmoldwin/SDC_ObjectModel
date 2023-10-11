using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SDC.Type.Interfaces;
using SDC.Schema;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.OMTests
{
    [TestClass]
    public class ListItemTypeTest
    {
        [TestMethod]
        public void ListItemTypeTest_AddButtonAction()
		{
			var de = new DataElementType(null);
            QuestionItemType q = new(de, "q");
            de.Items.Add(q);
			var li = q.AddListItem("li");

			var btn = li.AddChildButtonAction("test_item", "", 1);
            Assert.AreNotEqual(btn, null);
            Assert.AreEqual("test_item", btn.ID);
        }

        [TestMethod]
        public void ListItemTypeTest_AddDeselectIf()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			//de.Items.Add(q);
			var li = q.AddListItem("li");
			var dsi = li.AddDeselectIf();
            Assert.ReferenceEquals(dsi, q.GetListItems()?[0].As<ListItemType>().DeselectIf);
        }

        [TestMethod]
        public void ListItemTypeTest_AddDisplayItem()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			de.Items.Add(q);
			var li = q.AddListItem("li");
			var dsi = li.AddChildDisplayedItem("test_item", "", 1);
            Assert.AreNotEqual(dsi, null);
            Assert.AreEqual("test_item", dsi.ID);
        }

        [TestMethod]
        public void ListItemTypeTest_AddInjectedForm()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			de.Items.Add(q);
			var li = q.AddListItem("li");
			var injectedForm = li.AddChildInjectedForm("test_item", 1);
            Assert.AreNotEqual(injectedForm, null);
            Assert.AreEqual("test_item", injectedForm.ID);
        }

        [TestMethod]
        public void ListItemTypeTest_AddListItemResponseField()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			de.Items.Add(q);
			var li = q.AddListItem("li");
            li.name = "li_Name";
			var rf = li.AddListItemResponseField();

            //_______sample manipulation of new LIR response value
            var lir = q.AddListItemResponse("lir", out var dt, 
                "lir_Title", -1, ItemChoiceType.@string );
            lir.name = "lir_Name";
            var myStr = (string_DEtype) dt.Item;
            myStr.val = "test";
            myStr.maxLength = 4000;

            //sample SDC coded rule:
            var findLInode = de.GetListItemByName("lir_Name");
            var dtLIR = findLInode?.GetResponseDataTypeNode() as string_DEtype;
            if (dtLIR?.val is not null && !dtLIR.val.IsNullOrWhitespace())
                lir.selected = true;
            //____________________

            Assert.IsNotNull(rf);
        }

        [TestMethod]
        public void ListItemTypeTest_AddOnDeselect()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
            //de.DataElement_Items.Add(q);

			var li = q.AddListItem("li");
			var e = li.AddOnDeselect();
            Assert.IsNotNull(e);
            Assert.AreEqual(1, (li as ListItemType).OnDeselect.Count);
        }

        [TestMethod]
        public void ListItemTypeTest_AddOnSselect()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			//de.DataElement_Items.Add(q);

			var li = q.AddListItem("li");
			var e = li.AddOnSelect();
            //li.selected = true;
            Assert.IsNotNull(e);
            Assert.AreEqual(1, li.OnSelect.Count);
        }

        [TestMethod]
        public void ListItemTypeTest_AddQuestion()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
            //de.DataElement_Items.Add(q);

			var li = q.AddListItem("li");
			var q1 = li.AddChildQuestion(QuestionEnum.QuestionSingle,"q1");
            Assert.IsNotNull(q1);
            Assert.AreEqual("q1", q1.ID);
        }

        [TestMethod]
        public void ListItemTypeTest_AddQuestionResponse()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			//de.DataElement_Items.Add(q);

			var li = q.AddListItem("li");

			var q2 = new QuestionItemType(de)
            {
                ID = "QuestionResponseId",
                title = "This is a test question"
            };
			//de.DataElement_Items.Add(q2);

			var qr = li.AddChildQuestionResponse("myID",out _, "", 1, ItemChoiceType.@string, "", "", dtQuantEnum.EQ, "");
            Assert.IsNotNull(qr);
        }

        [TestMethod]
        public void ListItemTypeTest_AddSection()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			//de.DataElement_Items.Add(q);

			var li = q.AddListItem("li");
			var s = li.AddChildSection("test_id", "", 1);
            Assert.IsNotNull(s);
            Assert.AreEqual("test_id", s.ID);
        }

        [TestMethod]
        public void ListItemTypeTest_AddSelectIf()
		{
			var de = new DataElementType(null);
			QuestionItemType q = new(de, "q");
			//de.DataElement_Items.Add(q);

			var li = q.AddListItem("li");
			var s = li.AddSelectIf();
            Assert.IsNotNull(s);
            
        }
    }
}
