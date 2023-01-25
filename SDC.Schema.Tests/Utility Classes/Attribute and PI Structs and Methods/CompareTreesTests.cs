using BenchmarkDotNet.Disassemblers;
using CSharpVitamins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;

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
			//TODO:
			//Show Element Names on all nodes, incl subnodes: DONE
			//Changed Atts: False: this does not seem to roll up changes from subnodes; change to true if subnodes have changed Atts?
			//Remove "Deleted Node" - will never be populated



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

			Console.WriteLine("-----------------------------------------------Changed Nodes:-----------------------------------------------");

			IOrderedEnumerable<DifNodeIET> dDifNodeIET = comparer.GetIETattDiffs?.Values.OrderBy(difNodeIET => tNew.Nodes[ShortGuid.Decode(difNodeIET.sGuidIET)].order)!;

			foreach (DifNodeIET n in dDifNodeIET)  //Process all IET nodes that have some kind of change
			{
				var newNodeIET = tNew.Nodes[ShortGuid.Decode(n.sGuidIET)];
				tPrev.Nodes.TryGetValue(ShortGuid.Decode(n.sGuidIET), out var prevSubNodeIET);

				//Console.WriteLine($"name: {newNodeIET.name},\torder: {newNodeIET.order}");

				if (n.isAttListChanged || n.isNew || n.isRemoved || n.isParChanged || n.isMoved || n.hasAddedSubNodes || n.hasRemovedSubNodes)
				{
					Console.WriteLine($"IET: {(newNodeIET.ElementName).PadRight(30, '=')} Name: {(newNodeIET.name).PadRight(20, '=')} sGuid: {newNodeIET.sGuid}===== NamePrev: {prevSubNodeIET?.name}");

					Console.WriteLine(
						$"\tChanged Atts:   {n.isAttListChanged}\tNew Node:         {n.isNew}\r\n" +
						$"\tParent Changed: {n.isParChanged}\tMoved:            {n.isMoved}\r\n" +
						$"\tNew SubNodes:   {n.hasAddedSubNodes} \tRemoved SubNodes: {n.hasRemovedSubNodes}\t\tOrder: {newNodeIET.order}");

					if (n.dlaiDif.Values.Count == 0) Console.WriteLine($"\tNo SubNodes present");

					if (n.addedSubNodes is not null) //"added" means added below this IET since the previous version;  If this is a completely new IET node, it will not have added subnodes.
					{
						if (n.addedSubNodes.Count > 0)
						{
							Console.WriteLine("\r\n\tAdded SubNodes:");
							foreach (var asn in n.addedSubNodes)
							{
								Console.WriteLine($"\t\tSubNode {(asn.ElementName).PadRight(30)}Name: {(asn.name).PadRight(20)}sGuid: {asn.sGuid}");
							}
							Console.WriteLine();
						}
					}

					if (n.removedSubNodes is not null)//"removed" means removed below this IET since the previous version;  If this is a completely new IET node, it will not have removed subnodes.
					{
						{
							if (n.removedSubNodes.Count > 0)
							{
								Console.WriteLine("\r\n\tRemoved SubNodes:");
								foreach (var rsn in n.removedSubNodes)
								{
									Console.WriteLine($"\t\tSubNode {(rsn.ElementName).PadRight(30)}Name: {(rsn.name).PadRight(20)}sGuid: {rsn.sGuid}");
								}
								Console.WriteLine();
							}
						}


						string subNodesGuid = "";
						//foreach (var laiDif in n.dlaiDif.Values)
						foreach (var kvDif in n.dlaiDif)

						{
							var laiDif = kvDif.Value;

							if (laiDif.Count() > 0)
								Console.Write("\tSubNodes with Attribute Changes:");
							foreach (var aiDif in laiDif)
							{   //retrieve only those subNodes with serialized attributes

								//if (i++ == 0) 
								//Console.WriteLine();
								var newSubNode = tNew.Nodes[ShortGuid.Decode(aiDif.sGuidSubnode)];
								tPrev.Nodes.TryGetValue(ShortGuid.Decode(aiDif.sGuidSubnode), out var prevSubNode);
								//new and changed serialized attributes go here
								if (subNodesGuid != newSubNode.sGuid)
									Console.WriteLine($"\r\n\t\tSubNode {(newSubNode.ElementName).PadRight(30)}Name: {(newSubNode.name).PadRight(20)}sGuid: {newSubNode.sGuid}\tNamePrev: {prevSubNode?.name}");

								if (aiDif.aiNew is not null) Console.WriteLine($"\t\t\tNewAtt: {(aiDif.aiNew.Value.Name + "").PadRight(20)} Val: {(aiDif.aiNew.Value.Value + "").PadRight(20)}DefVal: {aiDif.aiNew.Value.DefaultValue}\t\tAttOrder: {aiDif.aiNew.Value.Order}");
								if (aiDif.aiPrev is not null) Console.WriteLine($"\t\t\tPrevAtt: {(aiDif.aiPrev.Value.Name + "").PadRight(20)} Val: {(aiDif.aiPrev.Value.Value + "").PadRight(20)}DefVal: {aiDif.aiPrev.Value.DefaultValue}\t\tAttOrder: {aiDif.aiPrev.Value.Order}");

								subNodesGuid = newSubNode.sGuid;
							}
						}
						//Console.WriteLine();
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
			DifNodeIET a = comparer!.GetIETattributes(sg) ?? default;
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