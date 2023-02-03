using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SDC.Type.Interfaces;
using SDC.Schema;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OMTests
{
    [TestClass]
    public class SectionItemTypeTest
    {
        [TestMethod]
        public void SectionItemTypeTest_AddButtonAction()
        {
			var de = new DataElementType(null);
			SectionItemType si = new (de);
			de.DataElement_Items.Add(si);

            var btn = si.AddChildButtonAction("test_item", "myTitle", 1);
            Assert.AreNotEqual(btn, null);
            Assert.AreEqual("test_item", btn.ID);
        }

        [TestMethod]
        public void SectionItemTypeTest_AddDisplayedItem()
        {
			var de = new DataElementType(null);
			SectionItemType si = new(de);
			de.DataElement_Items.Add(si);

			var di = si.AddChildDisplayedItem("test_item","dtTitle", 1);
            Assert.AreNotEqual(di, null);
            Assert.AreEqual("test_item", di.ID);
            Assert.AreEqual("dtTitle", di.title);
        }

        [TestMethod]
        public void SectionItemTypeTest_AddInjectedForm()
        {
			var de = new DataElementType(null);
			SectionItemType si = new(de, "sitID");
			de.DataElement_Items.Add(si);

            var injectedForm = si.AddChildInjectedForm("test_item", 1);
            Assert.AreNotEqual(injectedForm, null);
            Assert.AreEqual("test_item", injectedForm.ID);
        }

        [TestMethod]
        public void SectionItemTypeTest_AddQuestionItem()
        {
			var de = new DataElementType(null);
			SectionItemType si = new(de, "sitID");
			de.DataElement_Items.Add(si);

			var q = si.AddChildQuestion(QuestionEnum.QuestionSingle, "test_item","qTitle", 1);
            Assert.AreNotEqual(q, null);
            Assert.AreEqual("test_item", q.ID);
        }

        [TestMethod]
        public void SectionItemTypeTest_AddQuestionResponseItem()
        {
			var de = new DataElementType(null);
			SectionItemType si = new(de, "sitID");
			de.DataElement_Items.Add(si);

			//var q = new QuestionItemType(de)
   //         {
   //             ID = "QuestionResponseId",
   //             title = "This is a test question"
   //         };

            var qr = si.AddChildQuestionResponse("QR_id1", out _, "QR_id1_Title", 2, ItemChoiceType.@string, "textAfterResp", "units",  dtQuantEnum.EQ, "test");
            Assert.AreNotEqual(qr, null);
			//Assert.AreEqual(q.ID, qr.ID);
			Assert.AreEqual("QR_id1", qr.ID);
		}

        [TestMethod]
        public void SectionItemTypeTest_AddSection()
        {
			var de = new DataElementType(null);
			SectionItemType si = new(de, "sitID");
			de.DataElement_Items.Add(si);

			var s1 = si.AddChildSection("s1", "s1_Title", 1);
            Assert.AreNotEqual(s1, null);
            Assert.AreEqual("s1", s1.ID);
        }
    }
}
