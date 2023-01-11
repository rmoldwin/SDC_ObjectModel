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
using System.Collections.Concurrent;
using System.Data.SqlTypes;

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

			//Setup.Reset();
			//Setup.TimerStart($"==>{Setup.CallerName()} Started");
			//topNode = Setup.FD;

			//Dictionary<iet_sGuid, Dictionary<parent_sGuid, child_List<AttributeInfo>>>
			SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();
			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------

			foreach (IdentifiedExtensionType iet in topNode.IETnodes)
			{
				var en = iet.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				//Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");

				//Dictionary<parent_sGuid, child_List<AttributeInfo>>
				Dictionary<string, List<AttributeInfo>> dlai = new();

				//process iet's child nodes and their attributes
				var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1, 0, false);
				if (sublist is not null)
				{
					foreach (var subNode in sublist)
					{
						var lai = subNode.GetXmlAttributesSerialized();
						//Log(subNode, lai);
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
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? "").PadRight(13)}| {ai.Value?.ToString()}");
			}
			//  ------------------------------------------------------------------------------------
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Debug.Print(topNode.GetXml());


		}

		[TestMethod()]
		public void CompareVersions()
		{
			//Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Compare Setup Started");

			//var pathOrig = Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xml");
			var pathV2 = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest2v2.xml");
			var pathV1 = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest2v1.xml");

			//var fNew = File.OpenWrite(pathV2);
			FormDesignType? fdV2 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV2));
			FormDesignType? fdV1 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV1));

			SortedList<string, Dictionary<string, List<AttributeInfo>>>? slAttV2 = GetXmlAttributesFilledTree(fdV2);//keys are IET sGuid, subNode sGuid; holds serializable attribute List for individual subNodes
			SortedList<string, Dictionary<string, List<AttributeInfo>>>? slAttV1 = GetXmlAttributesFilledTree(fdV1);

			ConcurrentBag<(string, DifNodeIET)> cbDifNodeIET;
			ConcurrentDictionary<string, DifNodeIET> dDifNodeIET = new(); //the key is the IET node sGuid. Holds attribute changes in all IET and subNodes
																		  //foreach(var kv2 in slAttV2)
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Compare Setup Complete");
			Setup.TimerStart($"==>{Setup.CallerName()} Compare Started");

			var locker = new object();
			slAttV2.AsParallel().ForAll(kv2 =>
			//slAttV2.All(kv2 =>
			{
				//Setup IET node data;
				string sGuidIET = kv2.Key;
				Guid GuidIET = ShortGuid.Decode(sGuidIET); //may need locking here - check source code
				bool isParChangedIET = false;
				bool isMovedIET = false;
				bool isNewIET = false;
				bool isRemovedIET = false;
				bool isAttListChanged = false;
				var eqAttCompare = new SdcSerializedAttComparer();

				List<AttInfoDif> laiDif = new(); //For each IET node, there is one laiDif per subnode (including the IET node)
				Dictionary<string, List<AttInfoDif>> dlaiDif = new();  //the key is the IET sGuid; **d**laiDif will be added later to difNodeIET, which will then be added to **d**DifNodeIET
				dlaiDif.Add(sGuidIET, laiDif); //add the laiDif to its dictionary; later we will stuff this laiDiff List object with attribute change data for the IET node and all of its subNodes.

				//we now have to populate laiDif with with AttInfoDif structs for each changed attribute
				//We also have to set all the above bool settings for difNodeIET
				//Then finally, we need to add one new **d**DifNodeIET struct entry (difNodeIET) for each V2 IET.
				////We can also add difNodeIET structs for V1 IET nodes V1 that were not present in V2

				//holds the List<AttributeInfo> where the attributes differ from V1 to V2; part of dDiffNodeIET; the key of the IET node sGuid.
				//laiDif will become the value part of dlaiDifIET

				IdentifiedExtensionType? ietV1;
				if (fdV1.Nodes.TryGetValue(GuidIET, out BaseType? value))
					ietV1 = value as IdentifiedExtensionType;
				else ietV1 = null;


				if (ietV1 is not null)
				{
					var ietV2 = fdV2.Nodes[GuidIET] as IdentifiedExtensionType;

					//If V2 IET parent node is not the same as V1 parent node, mark as PARENT CHANGED
					if (ietV1.ParentNode?.sGuid != ietV2?.ParentNode?.sGuid)
					{ isParChangedIET = true; }

					//If V2 IET prev sib node is not the same as V1 prev sib, mark as POSITION CHANGED
					//TODO: see if we can add prev sib to the ai struct, to perhaps avoid this lookup
					lock (locker) if (ietV1.GetNodePreviousSib()?.sGuid != ietV2!.GetNodePreviousSib()?.sGuid)  //static extension method needs locking
						{ isMovedIET = true; }

					//Look for match in slAttV1
					if (slAttV1.TryGetValue(kv2.Key, out var dlaiV1))  //retrieve attribute dictionary for each V2 IET node
					{
						//Get the V1 IET attribute dictionary.  The first entry contains the V2 IET node data					
						List<AttributeInfo>? laiV1IET = dlaiV1[sGuidIET]; //could simply use dlaiV1[0] instead
						AttributeInfo aiV1IET = laiV1IET[0];

						//loop through attributes of each V2 node under the current IET, test for a mismatched value
						var dlaiV2 = kv2.Value; //dict contains List<AttributeInfo> for iet node and all non IET descendant nodes.

						foreach (var sGuidV2 in dlaiV2.Keys) //loop through IET subNodes
						{

							var aiHashV1IET = new HashSet<SdcSerializedAtt>(eqAttCompare);
							var aiHashV2IET = new HashSet<SdcSerializedAtt>(eqAttCompare);
							//HashSet<(string sGuidIET, string sGuid, AttributeInfo ai)> aiHashV1IET = new();

							dlaiV1.TryGetValue(sGuidV2, out var laiV1); //Find matching subNode in V1 (using sGuidV2), and retrieve its serializable attributes (laiV1)

							foreach (var aiV2 in dlaiV2[sGuidV2]) //Loop through V2 **attributes** in the currrent subNode (with subNode key: sGuidV2)
							{
								aiHashV2IET.Add(new(sGuidV2, aiV2)); //document that the serializable attribute exists in V2

								if (laiV1 is not null)
								{   //look for V1 subNode attribute match in laiV1
									var aiV1 = laiV1.FirstOrDefault(aiV1 => aiV1.Name == aiV2.Name);  //TODO: can be optimized to remove Linq

									if (aiV1 != default) //matching serialized attributes were found on the V1 subNode
									{
										aiHashV1IET.Add(new(sGuidV2, aiV1)); //document that the serializable attribute exists in V1
										if (aiV1.Value?.ToString() != aiV2.Value?.ToString()) //See if the attribute values match;
																							  //TODO: could perhaps make this more efficient by doing direct compare of value types, instead of using ToString()
										{
											laiDif.Add(new AttInfoDif(sGuidV2, aiV1, aiV2));
											isAttListChanged = true;
										}
									}
									else //if (aiV1 == default) //a matching serialized attribute was NOT found on the V1 subNode
									{
										//The V1 subNode does exist here.
										//aiV1 has default value -  so, the aiV1 attribute on the V1 subNode is not serialized (i.e., it's missing or at default tvalue) 
										laiDif.Add(new AttInfoDif(sGuidV2, default, aiV2));
										isAttListChanged = true;
									}
								}
								else //sGuidV2 does not match a list of filled V1 attributes (laiV1) on a matching V1 subNode (if it exists);
									 //we probably have a new V2 **sub**Node here, without a matching V1 **sub**Node
									 // (i.e., a V2-matching V1 subNode does not exist),
									 // or maybe? it's a subNode with all default attributes (this should not happen here, as laiV1 should still exist, but with no values)
								{
									laiDif.Add(new AttInfoDif(sGuidV2, default, aiV2));
									isAttListChanged = true;
									//at present, there is no way to document that the V1 subNode did not exist, but that subNode info is not currently needed.
									//However, the (non-)existance of the V1 node can be determined easily from V1 Nodes dictionary.
								}
							}

							var attsRemovedInV2 = aiHashV1IET.Except(aiHashV2IET, eqAttCompare); //Add IEqualityComparer to only look at sGuid and Name; ai.Value is an object, which requires special handling (convert to string before comparing)

							//Document the V2 removed attributes in the laiDif List:
							//The missing attribute name/value can be found by querying on AttInfoDif.sGuidSubnode, and looking in AttInfoDif.aiV1.Name and AttInfoDif.aiV1.Value
							foreach (var rem in attsRemovedInV2)
							{
								laiDif.Add(new AttInfoDif(rem.sGuid, aiV1: rem.ai, default));  //note that aiV2 is **default**; this indicates that **all** V1 serialized attribute were removed in V2
								isAttListChanged = true;
							}

						}//looping through IET subNodes ends here


					}//retrieve a V1 attribute dictionary for each IET node ends here
					else //could not retrieve a V1 attribute dictionary (dlaiV1) matching a V2 IET node, even though the V1 IET node exists;
						 //It should be present even if it has no Key/Value entries.
						 //If the V1 subNode was null, we would not be here.  See label **V1SubNodeIsNull**
					{
						Debugger.Break();
						//throw error here?
					}
				}//Find matching V1 IET node ends here			
				else //matching ietV1 is not found in V1 Nodes
				{
				V1SubNodeIsNull: isNewIET = true;
				}
				//finished looking for subNodes with attribute differences, as well as missing subnodes
				//Construct difNodeIET and add to dDifNodeIET for each IET node 

				DifNodeIET difNodeIET = new(sGuidIET, isParChangedIET, isMovedIET, isNewIET, isRemovedIET, isAttListChanged, dlaiDif);
				dDifNodeIET.AddOrUpdate(sGuidIET, difNodeIET, (sGuidIET, difNodeIET) => difNodeIET);

				//We could also use a ConcurrentBag<(string, DifNodeIET)>, and add nodes to a dictionary after this method completes 
				//We could also try a regular dictionary with a lock, but that might be slower if there are many Add contentions on the lock - needs testing 

				//TODO: Should we add isRemoved DifNodeIET entries, for IETs in V1 but not in V2?  This is not strictly necessary 

				//return true;
			}//END of each V2 IET node loop processing in lambda
				);

			void CompareNodes()
			{



			}



			//  ------------------------------------------------------------------------------------
			void Log(BaseType subNode, List<AttributeInfo> lai)
			{
				//char gt = ">"[0];
				const char gt = '>';
				var en = subNode.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {"".PadRight(pad, gt)}");
				Debug.Print("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>");
				foreach (AttributeInfo ai in lai)
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? "").PadRight(13)}| {ai.Value?.ToString()}");
			}

			//  ------------------------------------------------------------------------------------
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

		}

		[TestMethod()]
		public void GetPropertyInfoListTest()
		{

		}
		[TestMethod()]
		public void GetDotLevelIET()
		{
			int i = 0;
			foreach (var n in Setup.FD.IETnodes)
			{
				i++;
				Console.WriteLine($"#: {i}, DotLevel: {n.DotLevelIET ?? "{error}"}, name: {n.name ?? "{none}"}, ID: {n.ID}, title: {((n as DisplayedType)?.title) ?? "{none}"}\r\n");
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

		public readonly record struct Attribute(BaseType node, string sGuid, string attName, string? attVal, bool isDefault = true);
		public readonly record struct AttributeDiff(BaseType oldNode, BaseType newNode, string sGuidOld, string sGuidNew, string attName, string? attValOld, string? attValNew);
		public readonly record struct SDCattributeName(string AttName, string? AttVal, bool IsSerialized, bool IsDefault);

		public readonly record struct NodeInfo(string DotNotation, ShortGuid ParentNodesGuid, ShortGuid IETparentNodesGuid, int SibIndex, List<AttributeInfo> cwtNewNodesAi);
		public readonly record struct AttInfoDif(string sGuidSubnode, AttributeInfo aiV1, AttributeInfo aiV2);
		public readonly record struct DifNodeIET(
			string sGuidIET,
			bool isParChanged, //parent node has changed
			bool isMoved, //prev sibling node has changed
			bool isNew, //Node present in V2 only
			bool isRemoved, //Node present in V1 only
			bool isAttListChanged,
			Dictionary<string, List<AttInfoDif>> dlaiDif //in case we need to look up attribute Diffs by subnode sGuid
			);

		public bool AddedNode(BaseType nodeNew, Dictionary<Guid, BaseType> dictOld, out BaseType? oldNode)
		=> dictOld.TryGetValue(nodeNew.ObjectGUID, out oldNode);
		public bool RemovedNode(BaseType nodeOld, Dictionary<Guid, BaseType> dictNew, out BaseType? newNode)
		=> dictNew.TryGetValue(nodeOld.ObjectGUID, out newNode);


	}

	public readonly record struct TestStruct (int i)
	{ };
	public record class TestClass(int i)
	{ 

	};
	public readonly record struct  myEntity(int i, string s)
	{
		public readonly int I = i;
		public void Test(int j)
		{
			//I=123;  //read only
			var n = new myEntity(i,s);
			n.Test(12);
			//n.i = 123; //read only
			//n.I = 123; //read only
			//n.I = 111; //read only
		}
	};


}


public readonly record struct RemovedNode(BaseType node, BaseType newNode, string sGuidOld, string sGuidNew);