using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using SDC.Schema.Tests;
using SDC.Schema.Extensions;
using CSharpVitamins;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System.Collections.Specialized;

namespace SDC.Schema.Tests.Utils.Extensions
{ 
    
    [TestClass()]
    public class BaseTypeExtensionsTests
    {

        [TestMethod()]
        public void GetChildrenTest()
        {
            
        }
		[TestMethod]
		public void GetXmlAttributesAllOneNode()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			var FD = Setup.FD;
			var lst = FD.TopNode?.GetNodeByName("S_57219")?
				.GetXmlAttributesAll();			
			foreach (var n in lst) Debug.Print($"{n.Name}");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void GetXmlAttributesFilledOneNode()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			var FD = Setup.FD;
			var lst = FD.TopNode?.GetNodeByName("S_57219")?
				.GetXmlAttributesSerialized();
			foreach (var n in lst) Debug.Print($"{n.Name}");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		public SortedList<string, Dictionary<string, List<AttributeInfo>>> GetXmlAttributesFilledTree(ITopNode topNode)
		{

			Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			topNode = Setup.FD;

			//Dictionary<iet_sGuid, Dictionary<parent_sGuid, child_List<AttributeInfo>>>
			SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();
			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------
			
			foreach (IdentifiedExtensionType iet in topNode.IETnodes)
			{
				var en = iet.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");
				
				//Dictionary<parent_sGuid, child_List<AttributeInfo>>
				Dictionary<string, List<AttributeInfo>> dlai = new(); 

				//process iet's child nodes and their attributes
				var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1,0);
				if (sublist is not null)
				{
					foreach (var subNode in sublist)
					{
						var lai = subNode.GetXmlAttributesSerialized();
						Log(subNode, lai);
						dlai.Add(subNode.sGuid, lai);
					}
					dictAttr.Add(iet.sGuid, dlai);
				}				
			}
			return dictAttr;
			//  ------------------------------------------------------------------------------------
			void Log(BaseType subNode, List<AttributeInfo> lai)
			{
				var en = subNode.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {"".PadRight(pad, gt)}");
				Debug.Print("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>");
				foreach (AttributeInfo ai in lai)
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? "").PadRight(13)}| {ai.AttributeValue?.ToString()}");
			}
			//  ------------------------------------------------------------------------------------
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Debug.Print(topNode.GetXml());


		}

        [TestMethod()]
        public void GetXmlAttributesFilledCompareVersions()
        {
			Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var FD = Setup.FD;
			//Dictionary<iet_sGuid, Dictionary<parent_sGuid, child_List<AttributeInfo>>>


			var pathOrig = Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xml");
			var pathV2 = Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xmlv2");
			var pathV1 = Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xmlv1");

			//prepare 2 versions of the same file; need to manually edit them to introduce changes in v2
			//var origFile = File.ReadAllText(pathOrig);
			//File.WriteAllText(pathV2, origFile); //write v2 file with @order & sGuid
			//File.WriteAllText(pathV1, origFile); //write v1 file (original)
			

			var fNew = File.OpenWrite(pathV2);
			FormDesignType? fdV2 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV2));
			FormDesignType? fdV1 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV1));
			//System.IO.File.AppendAllText(pathOld, fdV2.GetXml());

			//fdV2.SaveXmlToFile(pathV2);
			//fdV1.SaveXmlToFile(pathV1);

			var slAttV2= GetXmlAttributesFilledTree(fdV2);
			SortedList<string, Dictionary<string, List<AttributeInfo>>>? slAttV1 = GetXmlAttributesFilledTree(fdV1);
			Dictionary<string, List<AttributeInfo>>? daiV1;	
			Dictionary<string, List<AttributeInfo>>? daiV2;

			slAttV2.AsParallel().ForAll(kv2 =>
			{
				//Look for match in slAttV1
				if (slAttV1.TryGetValue(kv2.Key, out daiV1))  //for each V2 IET node
				{
					//Get the V2 IET attribute dictionary.  The first entry contains the V2 IET node data
					string ietsGuid = kv2.Key;
					var ietlAIv2 = kv2.Value[ietsGuid];  //could simply use kv2.Value.Values[0] instead
					var ietAIv2 = ietlAIv2[0];
					string ietNameV2 = ietAIv2.Name;

					var ietlAIv1 = daiV1[ietsGuid]; //could simply use daiV1[0] instead
					var ietAIv1 = ietlAIv1[0];


					var ietGuid = ShortGuid.Decode(ietsGuid);
					var ietV1 = fdV1.Nodes[ietGuid] as IdentifiedExtensionType;
					var ietV2 = (IdentifiedExtensionType)fdV2.Nodes[ietGuid] as IdentifiedExtensionType;

					HashSet<(string sGuidIET, string sGuid, string attName)> aiHashV1;
					HashSet<(string sGuidIET, string sGuid, string attName)> aiHashV2;


					//If V2 IET parent node is not the same as V1 parent node, mark as PARENT CHANGED
					if (ietAIv2.ParentNodesGuid != ietAIv1.ParentNodesGuid)
					{ }//mark as PARENT CHANGED

					//If V2 IET prev sib node is not the same as V1 prev sib, mark as POSITION CHANGED
					//TODO: see if we can ad prev sib to the ai struct, to perhaps avoid this lookup
					if (ietV2.GetNodePreviousSib != ietV2.GetNodePreviousSib)
					{ }//mark as POSITION CHANGED

					//loop through attributes of each V2 node under the current IET, test for a mismatched value
					daiV2 = kv2.Value; //dict contains List<AttributeInfo> for iet and all non IET descendant nodes.
					foreach (var sguidV2 in daiV2.Keys)
					{
						aiHashV2 = new();
						aiHashV1 = new();

						foreach (var aiV2 in daiV2[sguidV2])
						{
							
							aiHashV2.Add((ietsGuid, sguidV2, aiV2.Name));

							//look for attribute match in daiV1
							if (daiV1.TryGetValue(sguidV2, out var laiV1))
							{
								var aiV1 = laiV1.Find(aiV1 => aiV1.Name == aiV2.Name);
								aiHashV1.Add((ietsGuid, sguidV2, aiV1.Name));


								if (aiV1 == default)
								{ } //?write "missing" aiV1 value to attsRemovedInV2 struct output, or just use **attsRemovedInV2** hashSet result output below
								else if (aiV1.AttributeValue != aiV2.AttributeValue)
								{ }//write aiV1 to the struct output 

							}
							else
							{ }//the node is missing, or there are no attributes on the node
						}
						var attsRemovedInV2 = aiHashV1.AsParallel().Except(aiHashV2);
						var attsAddedInV2 = aiHashV1.AsParallel().Except(aiHashV1);
						//We need to feed these tuples into some sruct, keyed by an sGuid
					}
				}
				else { }//mark entire V2 node as NEW (along with all its attributes)

			});

			void CompareNodes ()
			{



			}

			//char gt = ">"[0];
			char gt = '>';
			//  ------------------------------------------------------------------------------------


			
			BaseType? btOld = null;
			Dictionary<string, List<AttributeInfo>>? dlaiNew = null;
			Dictionary<string, List<AttributeInfo>>? dlaiOld = null;
			ConditionalWeakTable<BaseType, List<AttributeInfo>>? cwtNewNodesAi = null;
			ConditionalWeakTable<BaseType, List<AttributeInfo>>? cwtOldNodesAi = null;


			//do a quick Nodes compare to determine New nodes
			//No need to find deleted nodes for this task, so we can start with the New tree, and find matches (hits, misses) in the old tree
			//Can compare ParentNode and index order in sib list using ParentNodes and ChildNodes dictionaries.

			//Then iterate matching nodes and look for changed attributes of interest
			//This can be done without creating new dictionaries, but it may be easier (but slower) to process each tree separately, an dumping results into dictionaries.
			foreach (var ietNew in fdV2?.IETnodes)
			{
				//find matching iet node, in new fdV1 tree
				if(fdV1.Nodes.TryGetValue(ietNew.ObjectGUID, out btOld ))
				{
					var ietOld = (IdentifiedExtensionType)btOld;

					if (slAttV1.TryGetValue(ietNew.sGuid, out dlaiOld)) //dlaiOld holds all the attributes collected under ietOld/btOld
					{ //
						if (slAttV2.TryGetValue(ietNew.sGuid, out dlaiNew))//dlaiNew holds all the attributes collected under ietNew
						{
							//compare attribute lists node by node
							foreach (var kvLaiNew in dlaiNew) //iterate all the sub-nodes of ietNew, and return each sub-node's AttributeInfo list
							{
								List<AttributeInfo>? laiOld = new();
								if (dlaiOld.TryGetValue(kvLaiNew.Key, out laiOld)) //fins matching sub-node in dlaiOld, which holds each sub-node's AttributeInfo list
								{
									foreach (var attNew in kvLaiNew.Value)
									{
										//find the matching node in dlaiOld, and compare each attribute, adding any non-matches into a List<iet_sGuid, mismatched_AttributeInfo>
										var attOld = laiOld.FirstOrDefault(p => p.Name == attNew.Name);
										if(attOld.Name is not null)
										{ 
											if(attOld.AttributeValue != attNew.AttributeValue)
											{ }//save the old and new values in separate data structure array (HashSet of SortedList), with one array per iet node.
												//We can then compute an non-matching set operation to return the differences.
												//Differences must also account for added and removed nodes on the new side, and possibly on th old sidee
											   //Each array element is a struct like ~ {subnodeElementName, AttrName, AttrVal} for both the old and new values, but only if they differ
											   //iet sGuid and a node ref, sub-node type, sub-node elementName, sub-node's parentNode sGuid, subnode index in IEnumerable sib list
										}
										//also want to document attributes that have been removed from the new version, added, or changed
										//skip name, order, sGuid comparisons
										//include parentNode sGuid, index in IEnumerable sib list
									}
								}else
								{ } //attribute is missing in the old tree fdV1
							}
						}
					}
					else//ietOld does not exist
					{ } //we have a new node -  nothing to compare but Mark it as new for fdV2

				}
			}


		//  ------------------------------------------------------------------------------------
		void Log(BaseType subNode, List<AttributeInfo> lai)
			{
				var en = subNode.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {"".PadRight(pad, gt)}");
				Debug.Print("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>");
				foreach (AttributeInfo ai in lai)
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? "").PadRight(13)}| {ai.AttributeValue?.ToString()}");
			}
			//  ------------------------------------------------------------------------------------
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Debug.Print(FD.GetXml());


		}

		[TestMethod()]
        public void GetPropertyInfoListTest()
        {

		}
		[TestMethod()]
		public void GetDoteLevelIET()
		{
			int i = 0;
			foreach (var n in Setup.FD.IETnodes)
			{
				i++;
				Console.WriteLine($"#: {i}, DotLevel: {n.DotLevelIET??"{error}"}, name: {n.name??"{none}"}, ID: {n.ID}, title: {((n as DisplayedType)?.title)??"{none}"}\r\n");
				if (i == 100) break;
			}
		}

		[TestMethod()]
        public void GetPropertyInfoMetaDataTest()
        {

        }

        [TestMethod()]
        public void GetSubtreeTest()
        {

        }

        [TestMethod()]
        public void GetSibsTest()
        {

        }

        [TestMethod()]
        public void IsItemChangeAllowedTest()
        {

        }



		//!ToDo: include ParentNode (if changed) and Previous sib (if changed) in record
		public readonly record struct Attribute(BaseType node, string sGuid, string attName, string? attVal, bool isDefault = true);
		public readonly record struct AttributeDiff(BaseType oldNode, BaseType newNode, string sGuidOld, string sGuidNew, string attName, string? attValOld, string? attValNew);
		public readonly record struct SDCattribute(string AttName, string? AttVal, bool IsSerialized, bool IsDefault);
		public readonly record struct NodeInfo(string DotNotation, ShortGuid ParentNodesGuid, ShortGuid IETparentNodesGuid, int SibIndex, List<AttributeInfo> cwtNewNodesAi);
		public readonly record struct NodeInfoIET(string IetDotNotation, ShortGuid ParentNodesGuid, ShortGuid? IETparentNodesGuid, int SibIndex, ConditionalWeakTable<BaseType, List<AttributeInfo>> cwtNewNodesAi);
		public bool AddedNode(BaseType nodeNew, Dictionary<Guid, BaseType> dictOld, out BaseType? oldNode)
		=> dictOld.TryGetValue(nodeNew.ObjectGUID, out oldNode);
		public bool RemovedNode(BaseType nodeOld, Dictionary<Guid, BaseType> dictNew, out BaseType? newNode)
		=> dictNew.TryGetValue(nodeOld.ObjectGUID, out newNode);





	}


}


public readonly record struct RemovedNode(BaseType node, BaseType newNode, string sGuidOld, string sGuidNew);