﻿using BenchmarkDotNet.Disassemblers;
using CSharpVitamins;
using Iced.Intel;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Channels;
using static System.Collections.Specialized.BitVector32;
using System.Xml.Linq;

namespace SDC.Schema.Tests.Utils
{
	[TestClass()]
	public class CompareTreesTests
	{
		public Setup setup = new Setup();  //run the constructor to read the XML files.  They can now be accessed statically
		CompareTrees<FormDesignType>? _comparer;

		public CompareTreesTests()
		{ }

		//[TestInitialize]
		public void InitV1V2()
		{
			string xNew = Setup.BreastStagingTestV2_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			_comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			//var sGuid_q1 = string.Empty;
			//DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}
		public void InitV1V3()
		{
			string xNew = Setup.BreastStagingTestV3_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			_comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			//var sGuid_q1 = string.Empty;
			//DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}
		public void InitV1V4()
		{
			string xNew = Setup.BreastStagingTestV4_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			_comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			//var sGuid_q1 = string.Empty;
			//DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}
		public void InitV1V5()
		{
			string xNew = Setup.BreastStagingTestV5_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			_comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			//var sGuid_q1 = string.Empty;
			//DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}
		public void InitV1V6()
		{
			string xNew = Setup.BreastStagingTestV6_XML!;
			string xPrev = Setup.BreastStagingTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			_comparer = new CompareTrees<FormDesignType>(tPrev, tNew);

			//var sGuid_q1 = string.Empty;
			//DifNodeIET? q1 = comparer.GetIETattributes(sGuid_q1);
		}
		public void InitCompareTreesV1V2()
		{
			string xNew = Setup.CompareTreesTestV2_XML!;
			string xPrev = Setup.CompareTreesTestV1_XML!;

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tPrev = FormDesignType.DeserializeFromXml(xPrev);
			_comparer = new CompareTrees<FormDesignType>(tPrev, tNew);
		}

		[TestMethod()]
		public void ChangePrevVersionTest()
		{
			InitV1V2();
			string xOld = Setup.BreastStagingTestV1_XML!;
			var tPrev = FormDesignType.DeserializeFromXml(xOld);
			_comparer!.PrevVersion = tPrev;
		}
		[TestMethod()]
		public void ChangeNewVersionTest()
		{
			InitV1V2();
			string xNew = Setup.BreastStagingTestV5_XML!;
			FormDesignType tNew = FormDesignType.DeserializeFromXml(xNew);
			FormDesignType tPrev = _comparer!.PrevVersion;
			_comparer!.NewVersion = tNew;
		}


		[TestMethod]
		public void ChangeSummaryIETnodeTest()
		{
			//SETUP-------------------------------------------------------------------
			InitV1V5(); //init "comparer" (class CompareTrees) with 2 SDC XML doc versions: V1 & V5; There is no lineeage check in this code, so ensure that V5 derives from V1

			//Test IET sGuid:
			Guid GuidIET = ShortGuid.Decode("Ke_ZH_naV0ui-W7MBuNSHQ");
			//Find the sGuid's node:
			IdentifiedExtensionType newNodeIET = (IdentifiedExtensionType)_comparer!.NewVersion.Nodes[GuidIET];


			//-----------------------------------------------------------------------------------------------------

			//Use CompareTree to find icons/decorations for our node:
			DifNodeIET? difNodeIET = ChangeSummaryIETnode(newNodeIET);

			
			if (difNodeIET is not null)
			{
				DifNodeIET dif = difNodeIET.Value; //extract the struct from its nullable wrapper DifNodeIET?

				if (dif.isAttListChanged || dif.isNew || dif.isRemoved || dif.isParChanged || dif.isMoved || dif.hasAddedSubNodes || dif.hasRemovedSubNodes)
				{
					{ //this block is for the TEST  CONSOLE OUTPUT only
						_comparer.PrevVersion.Nodes!.TryGetValue(ShortGuid.Decode(newNodeIET.sGuid), out var prevNodeIET);  //prevNodeIET is allowed to be null
						Console.WriteLine($"\r\nIET: {(newNodeIET.ElementName).PadRight(26, '=')} Name: {(newNodeIET.name).PadRight(20, '=')} sGuid: {newNodeIET.sGuid}===== NamePrev: {prevNodeIET?.name}");
						Console.WriteLine(
							$"\tChanged Atts:   {dif.isAttListChanged.ToString().PadRight(8)} New Node:         {dif.isNew}\r\n" +
							$"\tParent Changed: {dif.isParChanged.ToString().PadRight(8)} Moved:            {dif.isMoved}\r\n" +
							$"\tNew SubNodes:   {dif.hasAddedSubNodes.ToString().PadRight(8)} Removed SubNodes: {dif.hasRemovedSubNodes.ToString().PadRight(8)} Order: {newNodeIET.order}");
					}
				}
				Assert.AreEqual(dif.isAttListChanged, true);
				Assert.AreEqual(dif.isNew, false);
				Assert.AreEqual(dif.isParChanged, false);
				Assert.AreEqual(dif.isMoved, true);
				Assert.AreEqual(dif.hasAddedSubNodes, false);
				Assert.AreEqual(dif.hasRemovedSubNodes, false);
				Assert.AreEqual(newNodeIET.order, 250m);
				//Expected:
				//IET: Section =================== Name: S_49193 ============= sGuid: Ke_ZH_naV0ui - W7MBuNSHQ ===== NamePrev: S_49193
				//		Changed Atts:	True		New Node:			False
				//		Parent Changed: False		Moved:				True
				//		New SubNodes:	False		Removed SubNodes:	False		Order: 250

				return;
			}
			Assert.Fail("");
		}

		public DifNodeIET? ChangeSummaryIETnode(IdentifiedExtensionType newNodeIET)
		{
			//Create dictionary (dDifNodeIET) of all attribute difference checks bt the 2 SDC trees, using the DifNodeIET struct for values
			var dDifNodeIET = _comparer?.GetIETattDiffs;  
			if (dDifNodeIET is null) throw new Exception("comparer.GetIETattDiffs is null"); 

			var dif = dDifNodeIET[newNodeIET.sGuid];
			if (dif.isAttListChanged || dif.isNew || dif.isRemoved || dif.isParChanged || dif.isMoved || dif.hasAddedSubNodes || dif.hasRemovedSubNodes)
				return dif;
			return null;  //We could also return the DifNodeIET struct, but since we found no differences in it, it seems more useful to just return null; 
		}

		[TestMethod()]
		public void ChangeSummaryTest()
		{
			//TODO:
			//Add infor on PreviousIETnodeChanged, PreviousIETNode; For Moved: Current/Previous List positions, 
			//xTODO:Show Element Names on all nodes, incl subnodes: DONE
			//TODO: BUG? - Changed Atts = False: this does not seem to roll up changes from subnodes;
			//	This may be because it only reflects changes from the Previous Version
			//	Should we flag the IET node as Changed if subnodes (new? and/or changed-from-Prev? subnodes) have changed XML attributess?
			//xTODO:Remove "Deleted Node" - will never be populated: DONE

			//LI with _zpr8uljcEGI23Kn55GHyQ was moved in a block from PrevVersion.  In NewVersion, in its new location, it received 3 brand new subnodes.
			//It has the same IET and direct parents.  But because the IET had a previousVersion sGuid match,
			//it is flagged as having "New" subnodes (added since Prev version), and these are displayed.
			//These changes (new subnodes and their serialized attributes) would presumably show up as "changes" in the TE's Metadata panel.

			//In contrast, Section OHCt9rzsMUe2nZI2xCgscg is a brrand new node, with many new subnodes.
			//However, none of its subnodes or thier attributes are displayed, and they would not appear and Metadata panel changes from the PreviousVersion .
			//


			//TODO: Do we need a method to easily roll up all serialized attributes to the IET node ( a single list?)?, and to compare them with the previous version IET's attributes?
			//	Currently, we can get that info from dDifNodeIET, but perhaps it could be simplified? E.g., by expanding AttInfoDiff?
			//	The method could accept an attribute name, and return the new and Prev serialized values (and defaults, if not serialized.
			//	Another method could retain a simple list of all attribute names in the existing subnodes,
			//		a serialized flag, with an option to only return serialized attribute names.
			//	Some functions may belong in CompareTrees, but others might be better placed elsewhere, perhaps in extensions or helper functions in another class.
			//		I.e., some methods do not require any kind of comparison to the previous version.
			//TODO: Need tests for changed attributes on IET nodes and their subnodes.



			InitV1V2();
			string xNew = Setup.BreastStagingTestV5_XML!;
			FormDesignType tNew = FormDesignType.DeserializeFromXml(xNew);
			FormDesignType tPrev = _comparer!.PrevVersion;
			_comparer!.NewVersion = tNew;

			Console.WriteLine("\r\n--------------------------------------------------------------------------------------------------------------\r\n");
			Console.WriteLine("Prev: BreastStagingTestV1_XML");
			Console.WriteLine("New:  BreastStagingTestV5_XML\r\n");

			Console.WriteLine($"NodesRemovedInNew.Count: {_comparer.GetNodesRemovedInNew.Count}");
			Console.WriteLine($"NodesAddedInNew.Count:   {_comparer.GetNodesAddedInNew.Count}\r\n");
			Console.WriteLine($"IETnodesRemovedInNew.Count: {_comparer.GetIETnodesRemovedInNew.Count}");
			Console.WriteLine($"IETnodesAddedInNew.Count:   {_comparer.GetIETnodesAddedInNew.Count}\r\n");
			Console.WriteLine($"IETattDiffs.Count (Nodes with serialized significant attributes): {_comparer.GetIETattDiffs?.Count}");
			Console.WriteLine($"New Nodes.Count:   {tNew.Nodes?.Count}");
			Console.WriteLine($"Prev Nodes.Count:  {tPrev.Nodes?.Count}");

			Console.WriteLine("\r\n-----------------------------------------------Changed Nodes:-----------------------------------------------");

			IOrderedEnumerable<DifNodeIET> dDifNodeIET = _comparer.GetIETattDiffs?.Values
				.OrderBy(
				  difNodeIET =>
				  {
					  var g = ShortGuid.Decode(difNodeIET.sGuidIET);

					  var ord = (tNew.Nodes!.TryGetValue(g, out var n))? n.order : -1; ;
					  //if (ord == -1) Debugger.Break();
					  return ord;
				  }
					)!;

			foreach (DifNodeIET n in dDifNodeIET)  //Process all IET nodes that have some kind of change
			{
				var newNodeIET = tNew.Nodes![ShortGuid.Decode(n.sGuidIET)];
				tPrev.Nodes!.TryGetValue(ShortGuid.Decode(n.sGuidIET), out var prevNodeIET);

				if (n.isAttListChanged || n.isNew || n.isRemoved || n.isParChanged || n.isMoved || n.hasAddedSubNodes || n.hasRemovedSubNodes)
				{
					Console.WriteLine($"\r\nIET: {(newNodeIET.ElementName).PadRight(26, '=')} Name: {(newNodeIET.name).PadRight(20, '=')} sGuid: {newNodeIET.sGuid}===== NamePrev: {prevNodeIET?.name}");

					Console.WriteLine(
						$"\tChanged Atts:   {n.isAttListChanged.ToString().PadRight(8)} New Node:         {n.isNew}\r\n" +
						$"\tParent Changed: {n.isParChanged.ToString().PadRight(8)} Moved:            {n.isMoved}\r\n" +
						$"\tNew SubNodes:   {n.hasAddedSubNodes.ToString().PadRight(8)} Removed SubNodes: {n.hasRemovedSubNodes.ToString().PadRight(8)} Order: {newNodeIET.order}");

					if (n.dlaiDif.Values.Count == 0) Console.WriteLine($"\tNo SubNodes present");

					if (n.addedSubNodes is not null) //"added" means added below this IET since the previous version;  If this is a completely new IET node, it will not have added subnodes.
					{
						if (n.addedSubNodes.Count > 0)
						{
							Console.WriteLine("\r\n\tAdded SubNodes:");
							foreach (var asn in n.addedSubNodes)
							{
								Console.WriteLine($"\t\tSubNode {(asn.ElementName??"").PadRight(30, '=')} Name: {(asn.name??"").PadRight(20, '=')} sGuid: {asn.sGuid}");
							}
							//Console.WriteLine();
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
									Console.WriteLine($"\t\tSubNode {(rsn.ElementName).PadRight(30, '=')} Name: {(rsn.name??"").PadRight(20, '=')} sGuid: {rsn.sGuid}");
								}
								//Console.WriteLine();
							}
						}


						string subNodesGuid = "";
						//foreach (var laiDif in n.dlaiDif.Values)
						foreach (var kvDif in n.dlaiDif)

						{
							var laiDif = kvDif.Value;

							if (laiDif.Count() > 0)
								Console.WriteLine("\r\n\tSubNodes with Attribute Changes:");
							foreach (var aiDif in laiDif)
							{   //retrieve only those subNodes with serialized attributes

								var newSubNode = tNew.Nodes[ShortGuid.Decode(aiDif.sGuidSubnode)];
								tPrev.Nodes.TryGetValue(ShortGuid.Decode(aiDif.sGuidSubnode), out var prevSubNode);
								//we checked for the existance of prevSubNode anywhere in the tree (using tPrev.Nodes), when prevSubNode could have moved from a different IET parent in tPrev,
								//The function FindSerializedXmlAttributesIET only fills an AttributeInfo struct (found in aiDif.aiPrev) for subNodes under the SAME IET parent in both Prev and New trees.
								//Thus, the AI struct (aiDif.aiPrev) for a moved prev subnode (prevSubNode) will be null, even though prevSubNode was actually present in tPrev
								//but under a different IET parent. This result (a null aiDif.aiPrev object) seems like the most correct behavior for evalulating tPrev->tNew changes, but can be confusing.
								//The following debugger breakpoint shows an example of moved subnodes having a null aiDif.aiPrev.
								if (aiDif.sGuidSubnode == "wE2j2CdYYEakySdJvU20IQ") Debugger.Break();

								//new and changed serialized attributes go here
								if (subNodesGuid != newSubNode.sGuid) //Write the next "SubNode Header" line only once per SubNode/sGuid,
																	  //but it may be followed by multiple lines for multiple aiNew and aiPrev attribute changes.
									Console.WriteLine($"\t\tSubNode {(newSubNode.ElementName).PadRight(27, '=')} Name: {(newSubNode.name??"").PadRight(20, '=')} sGuid: {newSubNode.sGuid}\tNamePrev: {prevSubNode?.name}");

								if (aiDif.aiNew is not null) Console.WriteLine($"\t\t\tNewAtt: {(aiDif.aiNew.Value.Name + "").PadRight(20)} Val: {(aiDif.aiNew.Value.Value + "").PadRight(20)}DefVal: {aiDif.aiNew.Value.DefaultValue}\t\tAttOrder: {aiDif.aiNew.Value.Order}");
								if (aiDif.aiPrev is not null) Console.WriteLine($"\t\t\tPrevAtt:{(aiDif.aiPrev.Value.Name + "").PadRight(20)} Val: {(aiDif.aiPrev.Value.Value + "").PadRight(20)}DefVal: {aiDif.aiPrev.Value.DefaultValue}\t\tAttOrder: {aiDif.aiPrev.Value.Order}");
								else Console.WriteLine($"\t\t\tPrevAtt: The subnode was previously located under this IET parent in the previous version:\r\n\t\t\t         IET parName: {prevSubNode.ParentIETnode?.name}\tIET sGuid: {prevSubNode.ParentIETnode?.sGuid}\tIET ID: {prevSubNode.ParentIETnode?.ID}");
								subNodesGuid = newSubNode.sGuid;
							}
						}
					}

				}
			}
		}

		[TestMethod()]
		public void GetSerializedXmlAttributesFromTreeTest()
		{
			InitV1V2();
			_comparer!.FindSerializedXmlAttributesFromTree(_comparer.NewVersion);

		}

		[TestMethod()]
		public void GetIETattributesTest()
		{

			InitV1V2();
			//"XzbI7XtzoUeC84x52v9BTA" < string >
			//"PdQi6PiXV06AIv-Tvlh5Xw"  Property
			//"BlNOOWghDkiN4FHoAjXlbA"  ListItem
			 //< ListItem sGuid = "BlNOOWghDkiN4FHoAjXlbA" ID = "43033_New.100004300" title = "Other (specify) NEW" />
			ShortGuid sg = "BlNOOWghDkiN4FHoAjXlbA";
			DifNodeIET a = _comparer!.GetIETattributes(sg) ?? default;
			DifNodeIET2 b = new();

			Assert.AreEqual(a.isNew, true);

		}
		[TestMethod()]
		public void GetIETnodesRemovedInNewTest()
		{
			InitV1V2();
			var C = _comparer!.GetIETnodesRemovedInNew;

		}
		[TestMethod()]
		public void GetIETnodesAddedInNewTest()
		{
			InitV1V2();
			var ietNodesAdd = _comparer!.GetIETnodesAddedInNew;
			Assert.AreEqual(ietNodesAdd.Count, 1);
			Assert.AreEqual(ietNodesAdd.ElementAt(0).ID, "43033_New.100004300");
		}
		[TestMethod()]
		public void GetNodesRemovedInNewTest()
		{
			InitV1V2();
			var nodesRem = _comparer!.GetNodesRemovedInNew;
			Debug.Print(nodesRem.Count.ToString());
			Assert.AreEqual(nodesRem.Count, 1);
			Debug.Print(nodesRem.ElementAt(0)?.ParentNode?.sGuid); //Since the node is new, its null sGuid is generated uniquely with each test
			Assert.AreEqual(nodesRem.ElementAt(0)?.ParentNode?.sGuid, "p-WbJKmuE0mhBc-_Y_JDNw");
		}
		[TestMethod()]
		public void GetNodesAddedInNewTest()
		{
			InitV1V2();
			var nodesAdd = _comparer!.GetNodesAddedInNew;
			Debug.Print(nodesAdd.Count.ToString());
			Assert.AreEqual(nodesAdd.Count, 3);
			Assert.AreEqual(nodesAdd.ElementAt(0).sGuid, "XzbI7XtzoUeC84x52v9BTA");
		}
		[TestMethod]
		public void CompareTreesTestV1V2()
		{
			InitCompareTreesV1V2();
			DifNodeIET? difs = _comparer!.GetIETattributes("iUnfss9Ppk-frsOz8qTnIw");
			Debug.Print(difs!.Value.isNew.ToString());
			Assert.IsTrue(difs!.Value.isNew);

		}
		[TestMethod]
		public void CompareChangedIETnodeTest()
		{
			InitCompareTreesV1V2();
			var newTree = _comparer!.NewVersion;
			SectionItemType node = (SectionItemType)newTree.Nodes[ShortGuid.Decode("F5GvSNED4kC6C9SDUhNSUA")];
			node.title += "*****";
			node.Property[0].propName += "*****";
			node.Property[1].propName += "*****";
			DifNodeIET nodeDifs = _comparer.CompareIET(node);
			//checked the changed propName in displayName field
			Assert.IsTrue(nodeDifs.dlaiDif.Values.First()[0].displayName == "Section"); //Section title
			Assert.IsTrue(nodeDifs.dlaiDif.Values.First()[0].aiNew?.Name == "title"); //Section title
			Assert.IsTrue(nodeDifs.dlaiDif.Values.First()[0].aiNew?.ValueString == "Nunc non nisi a arcu tempus dapibus*****"); //Section title
			Assert.IsTrue(nodeDifs.dlaiDif.Values.First()[1].displayName == "ReportText*****");  //Property with ReportText
			Assert.IsTrue(nodeDifs.dlaiDif.Values.First()[2].displayName == "altText*****");  //Property with altText
			Assert.IsTrue(nodeDifs.dlaiDif.Values.First()[2].elementName == "Property");


        }
	}
}