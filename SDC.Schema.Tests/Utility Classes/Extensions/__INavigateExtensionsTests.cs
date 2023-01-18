using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SDC.Schema.Tests.Utils.Extensions
{
	[TestClass()]
	public class __INavigateExtensionsTests
	{
		[TestInitialize()]
		public void Setup()
		{

		}
		[TestMethod()]
		public void GetNodeFirstSibTest()
		{

		}

		[TestMethod()]
		public void GetNodeLastSibTest()
		{

		}

		[TestMethod()]
		[DataRow(1, new string[] { "1", "1" }, "val 1")]
		[DataRow(1, new string[] { "2", "2" }, "val 2")]
		[DataRow(1, new string[] { "3", "3" }, "val 3")]
		public void GetNodePreviousSibTest(int v1, string[] v2, string v3)
		{
			Assert.IsTrue(true);
		}

		[TestMethod()]
		public void GetNodeNextSibTest()
		{

		}

		[TestMethod()]
		public void GetNodePreviousTest()
		{

		}

		[TestMethod()]
		public void GetNodeNextTest()
		{

		}

		[TestMethod()]
		public void GetNodeFirstChildTest()
		{

		}

		[TestMethod()]
		public void GetNodeLastChildTest()
		{

		}

		[TestMethod()]
		public void GetNodeLastDescendantTest()
		{

		}

		[TestMethod()]
		public void HasChildrenTest()
		{

		}

		[TestMethod()]
		public void GetChildListTest()
		{

		}

		[TestMethod()]
		public void GetSubtreeListTest()
		{

		}

		[TestMethod()]
		public void GetSubtreeDictionaryTest()
		{

		}

		[TestMethod()]
		public void GetPropertyInfoTest()
		{

		}
	}
}