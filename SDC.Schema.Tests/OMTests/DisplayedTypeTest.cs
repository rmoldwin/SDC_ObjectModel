using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SDC.Type.Interfaces;
using SDC.Schema;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OMTests
{
    [TestClass]
    public class DisplayedTypeTest
    {
        [TestMethod]
        public void DisplayedTypeTest_AddLink()
        {
            var de = new DataElementType(null);
            DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var link = di.AddLink(1);
            Assert.AreNotEqual(link, null);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddBlob()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var blob = di.AddBlob(1);
            Assert.AreNotEqual(blob, null);
            Assert.AreEqual(1, (di as DisplayedType).BlobContent.Count);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddContact()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var contact = di.AddContact(1);
            Assert.AreNotEqual(contact, null);
            Assert.AreEqual(1, (di as DisplayedType).Contact.Count);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddCodedValue()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var codedValue = di.AddCodedValue(1);
            Assert.AreNotEqual(codedValue, null);
            Assert.AreEqual(1, (di as DisplayedType).CodedValue.Count);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddActiveIf()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var ai = di.AddActivateIf();
            Assert.AreNotEqual(ai, null);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddDeActiveIf()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var dai = di.AddDeActivateIf();
            Assert.AreNotEqual(dai, null);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddOnEnter()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var ee = di.AddOnEnter();
			

			Assert.AreNotEqual(ee, null);
            Assert.AreEqual(1, (di as DisplayedType).OnEnter.Count);
        }

        [TestMethod]
        public void DisplayedTypeTest_AddOnExit()
        {
			var de = new DataElementType(null);
			DisplayedType di = new DisplayedType(de);
			de.DataElement_Items.Add(di);

			var ee = di.AddOnExit();

            Assert.AreNotEqual(ee, null);
            Assert.AreEqual(0, di.OnEnter?.Count??0);
            Assert.AreEqual(1, di.OnExit?.Count??0);
        }
    }
}
