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
				Log(iet, lai);
				dlai.Add(iet.sGuid, lai);

				//process iet's child nodes and thier attributes
				var sublist = iet.GetChildList();
				if (sublist is not null)
				{
					foreach (var subNode in sublist)
					{
						if (subNode is IdentifiedExtensionType) break; //look for the next node
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