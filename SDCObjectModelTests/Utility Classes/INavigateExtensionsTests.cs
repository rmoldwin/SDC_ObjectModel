using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SDC.Schema.Tests
{
    [TestClass()]
    public class INavigateExtensionsTests
    {
        [TestInitialize()]
        public void Setup()
        {

        }
        [TestMethod()]
        public void GetNodeFirstSibTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeLastSibTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [DataRow("1", new string[] { "1", "1" }, "val 1")]
        [DataRow("1", new string[] { "2", "2" }, "val 2")]
        [DataRow("1", new string[] { "3", "3" }, "val 3")]
        public void GetNodePreviousSibTest(BaseType v1, string[] v2)
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeNextSibTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodePreviousTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeNextTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeFirstChildTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeLastChildTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetNodeLastDescendantTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void HasChildrenTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetChildListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSubtreeListTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetSubtreeDictionaryTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetPropertyInfoTest()
        {
            Assert.Fail();
        }
    }
}