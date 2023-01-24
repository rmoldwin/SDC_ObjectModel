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

		//[TestInitialize]
		public void InitV1V2()
		{
			string xNew = Setup.BreastStagingTestV2_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			var sGuid_q1 = string.Empty;
			DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}
		public void InitV1V3()
		{
			string xNew = Setup.BreastStagingTestV3_XML!;
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
			InitV1V2();
			string xOld = Setup.BreastStagingTestV1_XML!;
			var tPrev = FormDesignType.DeserializeFromXml(xOld);
			comparer!.PrevVersion = tPrev;
		}

		[TestMethod()]
		public void ChangeNewVersionTest()
		{
			InitV1V2();
			string xNew = Setup.BreastStagingTestV3_XML!;
			FormDesignType tNew = FormDesignType.DeserializeFromXml(xNew);
			FormDesignType tPrev = comparer!.PrevVersion;
			comparer!.NewVersion = tNew;

			Console.WriteLine($"NodesRemovedInNew.Count: {comparer.GetNodesRemovedInNew.Count}");
			Console.WriteLine($"NodesAddedInNew.Count: {comparer.GetNodesAddedInNew.Count}");
			Console.WriteLine($"IETnodesRemovedInNew.Count: {comparer.GetIETnodesRemovedInNew.Count}");
			Console.WriteLine($"IETnodesAddedInNew.Count: {comparer.GetIETnodesAddedInNew.Count}");
			Console.WriteLine($"IETattDiffs.Count: {comparer.GetIETattDiffs?.Count}");

			Console.WriteLine("------------------------------------Changed Nodes:------------------------------------");

			IOrderedEnumerable<DifNodeIET> dDifNodeIET = comparer.GetIETattDiffs?.Values.OrderBy(difNodeIET => tNew.Nodes[ShortGuid.Decode(difNodeIET.sGuidIET)].order)!;
			
			foreach (DifNodeIET n in dDifNodeIET)  //Process all IET nodes that have some kind of change
			{
				var newNodeIET = tNew.Nodes[ShortGuid.Decode(n.sGuidIET)];
				tPrev.Nodes.TryGetValue(ShortGuid.Decode(n.sGuidIET), out var prevSubNodeIET);

				//Console.WriteLine($"name: {newNodeIET.name},\torder: {newNodeIET.order}");

				if (n.isAttListChanged || n.isNew || n.isRemoved || n.isParChanged || n.isMoved || n.hasAddedSubNodes || n.hasRemovedSubNodes)
				{
					Console.WriteLine($"IET_Node NameNew: {newNodeIET.name}\t\tNamePrev: {prevSubNodeIET?.name}\t\tsGuid: {newNodeIET.sGuid} ");

					Console.WriteLine(
						$"\tChanged Atts:   {n.isAttListChanged}\tNew Node:         {n.isNew}\t\tDeleted Node: {n.isRemoved}\r\n" +
						$"\tParent Changed: {n.isParChanged}\tMoved:            {n.isMoved}\t\tOrder:        {newNodeIET.order}\r\n" +
						$"\tNew SubNodes:   {n.hasAddedSubNodes} \tRemoved SubNodes: {n.hasRemovedSubNodes}");
					if (n.dlaiDif.Values.Count == 0) Console.WriteLine($"\tNo SubNodes present");

					foreach (var laiDif in n.dlaiDif.Values)
					{
						//?Do we need to add Empty structs to DifNodeIET for added/removed/changed sub-nodes?  
						//?Or create a DifNodeIET struct for subnodes too (a DifNodeSub struct)?

						if (laiDif.Count() == 0) Console.WriteLine();
						int i = 0;
						foreach (var aiDif in laiDif) 
						{	//retrieve only those subNodes with serialized attributes

							if (i++ == 0) Console.WriteLine();
							var newSubNode = tNew.Nodes[ShortGuid.Decode(aiDif.sGuidSubnode)];
							tPrev.Nodes.TryGetValue(ShortGuid.Decode(aiDif.sGuidSubnode), out var prevSubNode);
							//new and changed serialized attributes go here

							Console.WriteLine($"\t\tSubNode NameNew_: {newSubNode.name} \tNamePrev: {prevSubNode?.name}\tsGuid: {newSubNode.sGuid}");

							if (aiDif.aiNew is not null) Console.WriteLine($"\t\tNew: {aiDif.aiNew}");
							if (aiDif.aiPrev is not null) Console.WriteLine($"\t\tPrev: {aiDif.aiPrev}");

							Console.WriteLine();
						}						
					}
				}
			}


		}

		[TestMethod()]
		public void GetSerializedXmlAttributesFromTreeTest()
		{
			InitV1V2();
			comparer!.GetSerializedXmlAttributesFromTree(comparer.NewVersion);

		}

		[TestMethod()]
		public void GetIETattributesTest()
		{

			InitV1V2();
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
			InitV1V2();
			var C = comparer!.GetIETnodesRemovedInNew;

		}
		[TestMethod()]
		public void GetIETnodesAddedInNewTest()
		{
			InitV1V2();
			var ietNodesAdd = comparer!.GetIETnodesAddedInNew;
			Assert.AreEqual(ietNodesAdd.Count, 1);
			Assert.AreEqual(ietNodesAdd.ElementAt(0).ID, "43033_New.100004300");
		}
		[TestMethod()]
		public void GetNodesRemovedInNewTest()
		{
			InitV1V2();
			var nodesRem = comparer!.GetNodesRemovedInNew;
			Debug.Print(nodesRem.Count.ToString());
			Assert.AreEqual(nodesRem.Count, 1);
			Debug.Print(nodesRem.ElementAt(0)?.ParentNode?.sGuid); //Since the node is new, its null sGuid is generated uniquely with each test
			Assert.AreEqual(nodesRem.ElementAt(0)?.ParentNode?.sGuid, "p-WbJKmuE0mhBc-_Y_JDNw");
		}
		[TestMethod()]
		public void GetNodesAddedInNewTest()
		{
			InitV1V2();
			var nodesAdd = comparer!.GetNodesAddedInNew;
			Debug.Print(nodesAdd.Count.ToString());
			Assert.AreEqual(nodesAdd.Count, 3);
			Assert.AreEqual(nodesAdd.ElementAt(0).sGuid, "XzbI7XtzoUeC84x52v9BTA");
		}
	}
}