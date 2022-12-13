// See https://aka.ms/new-console-template for more information
using SDC.Schema;
using System.Diagnostics;
using SDC.Schema.Extensions;
using SDC.Schema.Tests;
using BenchmarkDotNet.Attributes;
using System.Text;
using BenchmarkDotNet.Running;
using CSharpVitamins;
using System.Collections.Concurrent;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

Console.WriteLine("Hello, World!");

//var summary = BenchmarkRunner.Run<MemoryBenchmarkerDemo>();
var summary = BenchmarkRunner.Run<SdcTests>();


//var s = new SdcTests();
//s.CompareVersions();

public class AntiVirusFriendlyConfig : ManualConfig
{
	public AntiVirusFriendlyConfig()
	{
		AddJob(Job.MediumRun
			.WithToolchain(InProcessNoEmitToolchain.Instance));
	}
}


[Config(typeof(AntiVirusFriendlyConfig))]
[MemoryDiagnoser]
public class SdcTests
{
	ITopNode topNode = Setup.FD;
	public SdcTests()
	{
		var topNode = Setup.FD;
	}
	private class Config : ManualConfig
	{
		public Config()
		{
			AddJob(Job.MediumRun
				.WithLaunchCount(1)
				.WithId("OutOfProc"));

			AddJob(Job.MediumRun
				.WithLaunchCount(1)
				.WithToolchain(InProcessEmitToolchain.Instance)
				.WithId("InProcess"));
		}
	}

	//[Benchmark]
	public void TestGetXmlAttributesFilledTree()
	{
		GetXmlAttributesFilledTree(topNode);
	}
	//[Benchmark]
	public void TestGetXmlAttributesFilledTreeFast()
	{
		GetXmlAttributesFilledTree(topNode);
	}

	[Benchmark]
	public void ReflectNodes()
	{ //Now with PropInfo caching.
		SdcUtil.ReflectRefreshTree(Setup.FD, out var text); 
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
					var lai = SdcUtil.ReflectChildXmlAttributes(subNode);
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
	[Benchmark]
	public void CompareVersions()
	{
		//Setup.Reset();
		Setup.TimerStart($"==>{Setup.CallerName()} Compare Setup Started");

		//var pathOrig = Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xml");
		var pathV2 = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest2v2.xml");
		var pathV1 = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest2v1.xml");

		//var fNew = File.OpenWrite(pathV2);
		FormDesignType? fdV2 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV2));
		//BaseType.ResetRootNode(); //This line is probably no longer needed, as it's called by the deserializer methods
		FormDesignType? fdV1 = FormDesignType.DeserializeFromXml(File.ReadAllText(pathV1));

		SortedList<string, Dictionary<string, List<AttributeInfo>>>? slAttV2 = GetXmlAttributesFilledTree(fdV2);//keys are IET sGuid, subNode sGuid; holds serializable attribute List for individual subNodes
		SortedList<string, Dictionary<string, List<AttributeInfo>>>? slAttV1 = GetXmlAttributesFilledTree(fdV1);

		var nodesRemovedInV2IET = fdV1.IETnodes.Except(fdV2.IETnodes); //V1 nodes no longer found in V2
		var nodesAddedInV2IET = fdV2.IETnodes.Except(fdV1.IETnodes); //V2 nodes that were not present in V1

		//ConcurrentBag<(string, _DifNodeIET)> cbDifNodeIET;
		ConcurrentDictionary<string, DifNodeIET> dDifNodeIET = new(); //the key is the IET node sGuid. Holds attribute changes in all IET and subNodes
																	  //foreach(var kv2 in slAttV2)
		Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Compare Setup Complete");
		Setup.TimerStart($"==>{Setup.CallerName()} Compare Started");
		var eqAttCompare = new SdcSerializedAttComparer(); //should be thread-safe

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
			

			List<AttInfoDif> laiDif = new(); //For each IET node, there is one laiDif per subnode (including the IET node)
			Dictionary<string, List<AttInfoDif>> dlaiDif = new();  //the key is the IET sGuid; dlaiDif will be added later to difNodeIET, which will then be added to **d**_DifNodeIET
			dlaiDif.Add(sGuidIET, laiDif); //add the laiDif to its dictionary; later we will stuff this laiDiff List object with attribute change data for the IET node and all of its subNodes.

			//we now have to populate laiDif with with AttInfoDif structs for each changed attribute
			//We also have to set all the above bool settings for difNodeIET
			//Then finally, we need to add one new dDifNodeIET struct entry (difNodeIET) for each V2 IET.
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

				//If V2 IET prev sib node is not the same as V1 prev sib, mark as POSITION CHANGED (isMovedIET = true;)


				//TODO: see if we can add prev sib to the ai struct, to perhaps avoid this lookup
				//TODO: use a non-static thread-safe version of GetNodePreviousSib to avoid locking;  Thus it could not be an extension method
				//!- Tried to create thread-safe version unsuccessfully, so we still need a lock when looking up previous sib nodes,
				//! and potentially incurring the need for sorting of ChildNodes entries
				//var util = new SdcUtilParallel();
				//lock(locker) 	if (util.GetPrevSibElement(ietV1)?.sGuid != util.GetPrevSibElement(ietV2)?.sGuid) //thread safe instance (?) method hierarchy with (hopefully) no shared state

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
									
									//bool isValueType = false;
									//ValueType? v1 = default;
									//ValueType? v2 = default; ;
									//if (aiV2.Value is ValueType) //A ValueType is never null
									//{
									//	isValueType = true;
									//	v2 = aiV2.Value! as ValueType;
									//}
									//if (aiV1.Value is ValueType) //A ValueType is never null
									//{
									//	isValueType = true;
									//	v1 = aiV1.Value! as ValueType;
									//}

									//bool isEqualAtts = false;
									//if (isValueType && v1 == v2) isEqualAtts = true;
									//else if (aiV1.ValueString == aiV2.ValueString) isEqualAtts = true;

									//if (!isEqualAtts)
									if (aiV1.ValueString != aiV2.ValueString) //See if the attribute values match;
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

			//We could also use a ConcurrentBag<(string, _DifNodeIET)>, and add nodes to a dictionary after this method completes 
			//We could also try a regular dictionary with a lock, but that might be slower if there are many Add contentions on the lock - needs testing 

			//TODO: Should we add isRemoved _DifNodeIET entries, for IETs in V1 but not in V2?  This is not strictly necessary 
			//!We could fill a hash table with all V1 matching nodes in this loop; the V2 nodes (or sGuids) not in the V1-match hashtable were removed in V2

			//return true;
		}//END of each V2 IET node loop processing in lambda
			);
		//Add V1 nodes that are not in V2


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

}

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

[MemoryDiagnoser]
public class MemoryBenchmarkerDemo
{
	int NumberOfItems = 100000;
	[Benchmark]
	public string ConcatStringsUsingStringBuilder()
	{
		var sb = new StringBuilder();
		for (int i = 0; i < NumberOfItems; i++)
		{
			sb.Append("Hello World!" + i);
		}
		return sb.ToString();
	}
	[Benchmark]
	public string ConcatStringsUsingGenericList()
	{
		var list = new List<string>(NumberOfItems);
		for (int i = 0; i < NumberOfItems; i++)
		{
			list.Add("Hello World!" + i);
		}
		return list.ToString();
	}
}



