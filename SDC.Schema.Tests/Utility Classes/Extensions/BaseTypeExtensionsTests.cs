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
			[TestMethod]
		public void GetXmlAttributesFilledTree()
		{
			//SdcUtil.ReflectRefreshTree(Setup.FD, out _);
			Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var FD = Setup.FD;

			//var lst = FD.TopNode.GetItemByName("S_57219")
			//	.GetXmlAttributesSerialized();			

			//foreach (var n in lst) Debug.Print($"{n.Name}");
			//foreach (var n in lst) Debug.Print($"{n.Name}: \tval:{n.AttributeValue?.ToString()}, " +
			//	$"\tsGuid: {n.ParentNodesGuid}, " +
			//	$"\torder: {n.Order}, " +
			//	$"\ttype: {n.AttributePropInfo.PropertyType}");

			//Dictionary<iet_sGuid, Dictionary<parent_sGuid, child_List<AttributeInfo>>>
			SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();
			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------
			
			foreach (IdentifiedExtensionType iet in Setup.FD.IETnodes)
			{
				var en = iet.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");
				
				//Dictionary<parent_sGuid, child_List<AttributeInfo>>
				Dictionary<string, List<AttributeInfo>> dlai = new();

				//Process iet node's attributes
				var lai = iet.GetXmlAttributesSerialized();
				//Log(iet, lai);
				//dlai.Add(iet.sGuid, lai);

				//process iet's child nodes and thier attributes
				var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1,0);
				if (sublist is not null)
				{
					foreach (var subNode in sublist)
					{
						//if (subNode is ListType) Debugger.Break();
						//if (subNode is IdentifiedExtensionType) break; //skip these child nodes; instead, we look for the child nodes of the next node (if present)
						lai = subNode.GetXmlAttributesSerialized();
						Log(subNode, lai);
						dlai.Add(subNode.sGuid, lai);
					}
					dictAttr.Add(iet.sGuid, dlai);
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
        public void GetXmlAttributesFilledCompareVersions()
        {
			Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var FD = Setup.FD;
			//Dictionary<iet_sGuid, Dictionary<parent_sGuid, child_List<AttributeInfo>>>
			SortedList<string, Dictionary<string, List<AttributeInfo>>> slAttNew = new();
			SortedList<string, Dictionary<string, List<AttributeInfo>>> slAttOld = new();
			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------

			foreach (IdentifiedExtensionType iet in Setup.FD.IETnodes)
			{
				var en = iet.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				//Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");
				//Dictionary<parent_sGuid, child_List<AttributeInfo>>
				Dictionary<string, List<AttributeInfo>> dlai = new();
				//Process iet node's attributes
				var lai = iet.GetXmlAttributesSerialized();
				//process iet's child nodes and thier attributes
				var sublist = SdcUtil.GetSortedNonIETsubtreeList(iet, -1, 0);
				if (sublist is not null)
				{
					foreach (var subNode in sublist)
					{
						lai = subNode.GetXmlAttributesSerialized();
						Log(subNode, lai);
						dlai.Add(subNode.sGuid, lai);
					}
					slAttNew.Add(iet.sGuid, dlai);
				}
			}

			FormDesignType? fdNew = null;
			FormDesignType? fdOld = null;
			BaseType? btOld = null;
			Dictionary<string, List<AttributeInfo>>? dlaiNew = null;
			Dictionary<string, List<AttributeInfo>>? dlaiOld = null;


			//do a quick Nodes compare to determine New nodes
			//No need to find deleted nodes for this task, so we can start with the New tree, and find matches (hits, misses) in the old tree
			//Can compare ParentNode and index order in sib list using ParentNodes and ChildNodes dictionaries.

			//Then iterate matching nodes and look for changed attributes of interest
			//This can be done without creating new dictionaries, but it may be easier (but slower) to process each tree separately, an dumping results into dictionaries.
			foreach (var ietNew in fdNew.IETnodes)
			{
				//find matching iet node, in new fdOld tree
				if(fdOld.Nodes.TryGetValue(ietNew.ObjectGUID, out btOld ))
				{
					var ietOld = (IdentifiedExtensionType)btOld;

					if (slAttOld.TryGetValue(ietNew.sGuid, out dlaiOld)) //dlaiOld holds all the attributes collected under ietOld/btOld
					{ //
						if (slAttNew.TryGetValue(ietNew.sGuid, out dlaiNew))//dlaiNew holds all the attributes collected under ietNew
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
											{ }//save the old and new values in a data structure array, with one array per iet node.
											   //Each array element is a struct like ~ {subnodeElementName, AttrName, AttrVal} for both the old and new values, but only if they differ
											   //iet sGuid and a node ref, sub-node type, sub-node elementName, sub-node's parentNode sGuid, subnode index in IEnumerable sib list
										}
										//also want to document attributes that have been removed from the new version, added, or changed
										//skip name, order, sGuid comparisons
										//include parentNode sGuid, index in IEnumerable sib list
									}
								}else
								{ } //attribute is missing in the old tree fdOld
							}
						}
					}
					else//ietOld does not exist
					{ } //we have a new node -  nothing to compare

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
    }
}