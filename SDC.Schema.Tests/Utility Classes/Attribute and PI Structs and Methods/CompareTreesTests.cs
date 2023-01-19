using CSharpVitamins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace SDC.Schema.Tests.Utils
{
	[TestClass()]
	public class CompareTreesTests
	{
		public Setup setup = new Setup();
		CompareTrees<FormDesignType>? comparer;

		public CompareTreesTests()
		{ }

		[TestInitialize]
		public void Init()
		{
			string xNew = Setup.BreastStagingTestV2_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			var sGuid_q1 = string.Empty;
			DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}

		[TestMethod()]
		public void ChangePrevVersionTest()
		{
			string xOld = Setup.BreastStagingTestV1_XML!;
			var tPrev = FormDesignType.DeserializeFromXml(xOld);
			comparer!.PrevVersion = tPrev;
		}

		[TestMethod()]
		public void ChangeNewVersionTest()
		{
			string xNew = Setup.BreastStagingTestV2_XML!;
			var tNew = FormDesignType.DeserializeFromXml(xNew);
			comparer!.NewVersion = tNew;
		}

		[TestMethod()]
		public void GetSerializedXmlAttributesFromTreeTest()
		{
			comparer!.GetSerializedXmlAttributesFromTree(comparer.NewVersion);

		}

		[TestMethod()]
		public void GetIETattributesTest()
		{

			//"XzbI7XtzoUeC84x52v9BTA" < string >
			//"PdQi6PiXV06AIv-Tvlh5Xw"  Property
			//"BlNOOWghDkiN4FHoAjXlbA"  ListItem
			ShortGuid sg = "BlNOOWghDkiN4FHoAjXlbA";
			DifNodeIET a = comparer!.GetIETattributes(sg)??default;
			DifNodeIET2 b = new();

			Assert.AreEqual(a, default(DifNodeIET));

		}
		[TestMethod()]
		public void GetIETnodesRemovedInNewTest()
		{
			var C = comparer!.GetIETnodesRemovedInNew;

		}
		[TestMethod()]
		public void GetIETnodesAddedInNewTest()
		{
			var ietNodesAdd = comparer!.GetIETnodesAddedInNew;
			Assert.AreEqual(ietNodesAdd.Count, 1);
			Assert.AreEqual(ietNodesAdd.ElementAt(0).ID, "43033_New.100004300");
		}
		[TestMethod()]
		public void GetNodesRemovedInNewTest()
		{
			var nodesRem = comparer!.GetNodesRemovedInNew;
			Debug.Print(nodesRem.Count.ToString());
			Assert.AreEqual(nodesRem.Count, 1);
			Debug.Print(nodesRem.ElementAt(0)?.ParentNode?.sGuid); //Since the node is new, its null sGuid is generated uniquely with each test
			Assert.AreEqual(nodesRem.ElementAt(0)?.ParentNode?.sGuid, "p-WbJKmuE0mhBc-_Y_JDNw");
		}
		[TestMethod()]
		public void GetNodesAddedInNewTest()
		{
			var nodesAdd = comparer!.GetNodesAddedInNew;
			Debug.Print(nodesAdd.Count.ToString());
			Assert.AreEqual(nodesAdd.Count, 3);
			Assert.AreEqual(nodesAdd.ElementAt(0).sGuid, "XzbI7XtzoUeC84x52v9BTA");
		}
	}
}