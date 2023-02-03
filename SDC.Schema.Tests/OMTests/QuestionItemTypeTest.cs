using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SDC.Type.Interfaces;
using SDC.Schema;
using SDC.Schema.Extensions;
using System.Linq;

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
			de.DataElement_Items.Add(qi);

            var testItem = qi.AddChildButtonAction("test_id", "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddDisplayItem()
		{
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var testItem = qi.AddChildDisplayedItem("test_id","", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddDisplayedTypeToList()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

            var rf = qi.AddQuestionResponseField(out _);  // ResponseField_Item = new ListItemResponseFieldType(null);
            qi.GetListField().maxSelections = 1;
            //qi.ListField_Item = new ListFieldType(null) { maxSelections = 1 };
            var testItem = qi.AddDisplayedTypeToList("test_id", "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddInjectedForm()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var testItem = qi.AddChildInjectedForm("test_id", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddListItem()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var rf = qi.AddQuestionResponseField(out _);  // (qi as QuestionItemType).ResponseField_Item = new ListItemResponseFieldType(null);
			qi.GetListField().maxSelections = 1; //(qi as QuestionItemType).ListField_Item = new ListFieldType(null) { maxSelections = 1 };
            var testItem = qi.AddListItem("test_id", "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddListItemResponse()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var testItem = qi.AddListItemResponse("test_id", out _,  "", 1);
            Assert.IsNotNull(testItem);
            Assert.AreEqual("test_id", testItem.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddQuestion()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var q = qi.AddChildQuestion(QuestionEnum.QuestionSingle, "q1");
            Assert.IsNotNull(q);
            Assert.AreEqual("q1", q.ID);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddQuestionResponse()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var question = new QuestionItemType(null)
            {
                ID = "QuestionResponseId",
                title = "This is a test question"
            };
            var qr = qi.AddChildQuestionResponse("myQ1", out _, "myTitle", 1, ItemChoiceType.@string, "textAfter", "units", dtQuantEnum.EQ, "default");
            Assert.IsNotNull(qr);
        }

        [TestMethod]
        public void QuestionItemTypeTest_AddSection()
        {
			var de = new DataElementType(null);
			QuestionItemType qi = new QuestionItemType(de);
			de.DataElement_Items.Add(qi);

			var s = qi.AddChildSection("test_id", "", 1);
            Assert.IsNotNull(s);
            Assert.AreEqual("test_id", s.ID);
        }
		
        [TestMethod]
		public void AddListItemResponsetoQuestionTest()
		{
			var de = new DataElementType(null, "DE1");
			var q = new QuestionItemType(de, "q1", "q1");
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
	}
}
