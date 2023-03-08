using FastSerialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

//using SDC.Schema;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class MoveTests
	{
		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
		[TestMethod]
		public void MoveListItemInList()
		{
			Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			//FD.TopNode.ReorderNodes();
			var li = Setup.FD.Nodes.Where(n =>
				n.Value is ListItemType liTest &&
				liTest.ID == "38493.100004300").FirstOrDefault().Value
				as ListItemType;
			Assert.IsTrue(li is ListItemType);

			List<BaseType> lst1;
			List<BaseType> lst2;
			List<BaseType> lst3;

			lst1 = SdcUtil.ReflectChildElements(Setup.FD.GetListItemByID("51689.100004300"));
			lst2 = SdcUtil.ReflectChildElements(Setup.FD.GetListItemByID("38493.100004300"));
			lst3 = SdcUtil.ReflectChildElements(Setup.FD.GetNodeByName("lst_44135_3"));

			lst3 = SdcUtil.ReflectRefreshSubtreeList(Setup.FD.GetSectionByID("43969.100004300"));
			//foreach (var n in lst3) Debug.Print(n.name);
			var tc = new TreeComparer();
			lst3.Sort(tc);
			foreach (var n in lst3) Debug.Print(n.name + ": " + n.ElementName + ", " + n.ObjectID);

			var lst4 = Setup.FD.Nodes.Values.ToList();
			var res = lst4[0].GetType().GetProperties()
				.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Count() > 0 && p.GetValue(lst4[0]) != null)
				.Select(p => p.GetValue(lst4[0])).ToList();

			var propList = new List<BaseType>();

			while (false)
			{
				foreach (object o in res)
				{
					var bt = o as BaseType;
					if (bt != null)
					{
						Debug.Print(bt.name);
						propList.Add(bt);
					}
					else
					if (o is IList il) foreach (var n in il.OfType<BaseType>())
						{
							Debug.Print(n.name);
							propList.Add(n);
						}
				}
			}

			propList.Sort(new TreeComparer());
			int i = 0;
			foreach (var n in propList) Debug.Print((i++).ToString() + ": " + n.name);

			SdcUtil.ReflectRefreshSubtreeList(lst4[0], true, true);
			foreach (var n in lst4) Debug.Print(n.name + ": " + n.ElementName + ", " + n.ObjectID.ToString() + ", order:" + n.order.ToString());


			lst4.Sort(tc);
			var list = (ListType)li.ParentNode;



			li.Move(list, 6);
			Assert.IsTrue(SdcUtil.GetElementPropertyInfoMeta(li, li.ParentNode).ItemIndex == 6);

			li.Move(list, 99);
			Assert.IsTrue(SdcUtil.GetElementPropertyInfoMeta(li, li.ParentNode).ItemIndex == list.Items.Count() - 1);

			li.Move(list, 0);
			Assert.IsTrue(SdcUtil.GetElementPropertyInfoMeta(li, li.ParentNode).ItemIndex == 0);

			li.Move(list);
			Assert.IsTrue(SdcUtil.GetElementPropertyInfoMeta(li, li.ParentNode).ItemIndex == list.Items.Count() - 1);
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Setup.Reset(); //reset after moving nodes.
		}
		[TestMethod]
		public void MoveListItemToOtherList()
		{
			Setup.Reset();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var li = Setup.FD.Nodes.Where(n =>
				n.Value is ListItemType liTest &&
				liTest.ID == "38493.100004300").FirstOrDefault().Value
				as ListItemType;
			Assert.IsTrue(li is ListItemType);
			var list = (ListType)li.ParentNode;

			var list2 = Setup.FD.Nodes.Where(n =>
				n.Value is ListType liTest &&
				liTest.name == "lst_58267_3").FirstOrDefault().Value
				as ListType;
			Assert.IsTrue(list2 is ListType);

			//Move to different List (list2)
			li.Move(list2, 2);
			Assert.IsTrue(SdcUtil.GetElementPropertyInfoMeta(li, li.ParentNode).ItemIndex == 2);
			Assert.AreEqual(list2, li.ParentNode);
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Setup.Reset(); //reset after moving nodes.
		}

		[TestMethod]
		public void ClearChildItemsAfterDropOver()
		{
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();

			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");

			FormDesignType FD = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(path);
			var myXML = TopNodeSerializer<FormDesignType>.GetXml(FD);

			var S_16182 = FD.GetSectionByName("S_16182");
			var Q_58807 = FD.GetQuestionByName("Q_58807");
			var LI_40307 = FD.GetListItemByName("LI_40307");
			var LI_39079 = FD.GetListItemByName("LI_39079");
			var LIR_16195 = FD.GetListItemByName("LI_16195");
			var LI_16196 = FD.GetListItemByName("LI_16196");
			var Q_16214 = FD.GetQuestionByName("Q_16214");
			var Q_16250 = FD.GetQuestionByName("Q_16250");

			Assert.IsTrue(LI_40307.ChildItemsNode is null);

			//Move qLat under liExci
			Move(Q_16214, LI_40307, DropPosition.Over);

			Assert.IsTrue(LI_40307.GetChildNodes().Count == 1);
			Assert.IsTrue(LI_40307.ChildItemsNode is not null);

			//Move qLat under liMast
			Move(Q_16214, LI_39079, DropPosition.Over);
			Assert.IsTrue(LI_39079.GetChildNodes().Count == 2); //Has a Property and a ChildItems node
			Assert.IsTrue(LI_40307.ChildItemsNode is null);

			//Move qLat under liOth
			Move(Q_16214, LIR_16195, DropPosition.Over);
			Assert.IsTrue(LIR_16195.GetChildNodes().Count == 3);//Has Property, and a ChildItems node
			Assert.IsTrue(LI_40307.ChildItemsNode is null);
			Assert.IsTrue(LI_39079.ChildItemsNode is null);

			//Move qLat under liExci - should cause error
			Move(Q_16214, LI_40307, DropPosition.Over);
			Assert.IsTrue(LI_40307.GetChildNodes().Count == 1);
			Assert.IsTrue(LIR_16195.ChildItemsNode is null);
			Assert.IsTrue(LI_39079.ChildItemsNode is null);


			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
			Setup.Reset(); //reset after moving nodes.
		}
		[TestMethod]
		public void CountNodesAfterDropAfter()
		{
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();

			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");

			FormDesignType FD = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(path);
			var myXML = TopNodeSerializer<FormDesignType>.GetXml(FD);

			SectionItemType? S_16182 = FD.GetSectionByName("S_16182"); //SPECIMEN
			var Q_58807 = FD.GetQuestionByName("Q_58807"); //Procedure (Note A)
			var LI_40307 = FD.GetListItemByName("LI_40307");
			var LI_39079 = FD.GetListItemByName("LI_39079");
			var LIR_16195 = FD.GetListItemByName("LI_16195");
			var LI_16196 = FD.GetListItemByName("LI_16196");

			var Q_16214 = FD.GetQuestionByName("Q_16214"); //QS: Specimen Laterality
			var LI_16215 = FD.GetListItemByName(name: "LI_16215");
			var LI_16216 = FD.GetListItemByName("LI_16216");
			var LI_16218 = FD.GetListItemByName("LI_16218");

			var S_16249 = FD.GetSectionByName("S_16249"); //S:TUMOR

			var Q_16250 = FD.GetQuestionByName("Q_16250"); //QS: Tumor Site (Note B)
			var p_rptTxt_16250_1 = FD.GetPropertyByName("p_rptTxt_16250_1"); //Report Text
			var LI_16251 = FD.GetListItemByName("LI_16251");
			var LI_16252 = FD.GetListItemByName(name: "LI_16252");
			var LI_16253 = FD.GetListItemByName("LI_16253");
			var LI_16254 = FD.GetListItemByName(name: "LI_16254");
			var LI_16255 = FD.GetListItemByName("LI_16255");
			var LI_16256 = FD.GetListItemByName("LI_16256");

			var Q_52840 = FD.GetQuestionByName("Q_52840"); //QR: Specify Distance from Nipple in Centimeters (cm)
			var p_rptTxt_52840_1 = FD.GetPropertyByName("p_rptTxt_52840_1"); //p: Distance from Nipple (Centimeters)
			var rf_52840_2 = FD.GetNodeByName("rf_52840_2");
			var rsp_52840_3 = FD.GetNodeByName(name: "rsp_52840_3");
			var dec_52840_4 = FD.GetNodeByName(name: "dec_52840_4");

			var LI_16257 = FD.GetListItemByName("LI_16257");//LIR: Other (specify)			
			var p_rptTxt_16257_1 = FD.GetPropertyByName("p_rptTxt_16257_1"); //Distance from Nipple (Centimeters)
			var lirf_16257_2 = FD.GetNodeByName("lirf_16257_2");
			var rsp_16257_3 = FD.GetNodeByName(name: "rsp_16257_3");
			var str_16257_4 = FD.GetNodeByName(name: "str_16257_4");




			Assert.IsTrue(Q_58807?.GetListItems()?.Count == 4);
			Assert.IsTrue(Q_16214?.GetListItems()?.Count == 3);

			Move(LI_16215!, LI_40307, DropPosition.After);
			Assert.IsTrue(Q_58807?.GetListItems()?.Count == 5);
			Assert.IsTrue(Q_16214?.GetListItems()?.Count == 2);

			Move(LI_16216, LI_40307, DropPosition.After);
			Assert.IsTrue(Q_58807?.GetListItems()?.Count == 6);
			Assert.IsTrue(Q_16214?.GetListItems()?.Count == 1);

			Move(LI_16218, LI_40307, DropPosition.After);
			Assert.IsTrue(Q_58807?.GetListItems()?.Count == 7);
			Assert.IsTrue(Q_16214?.GetListItems().Count == 0);

			Move(LI_16218, Q_16214, DropPosition.Over); //Move LI_16218 back home to Q_16214
			Assert.IsTrue(Q_16214?.GetListItems()?.Count == 1);

			Assert.IsTrue(S_16182?.ChildItemsNode.ChildItemsList.Count == 2);
			Move(S_16249, S_16182, DropPosition.Over);  //Drop Tumor onto Specimen node to make Tumor a child section
			Assert.IsTrue(S_16182?.ChildItemsNode.ChildItemsList.Count == 3);

			Move(S_16249, S_16182, DropPosition.After);  //Drop Tumor onto Specimen node to make Tumor a child section
			Assert.IsTrue(S_16182?.ChildItemsNode.ChildItemsList.Count == 2);

			// Drop LI ("Central") Over Q ("Procedure (Note A)")
			Move(LI_16255, Q_16250, DropPosition.Over);  //Drop LI on Q, and check to ensure that the LI is now is position 0 in ChildNodes & right after the Q in &IETnodes.
			var lst = FD.IETnodes.ToList();
			var lst1 = (IList)FD.IETnodes;
			var qPos = lst1.IndexOf(Q_16250);
			var liPos = lst1.IndexOf(LI_16255);
			Assert.IsTrue(liPos - qPos == 1);

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
			Setup.Reset(); //reset after moving nodes.
		}
		[TestMethod]
		public void _MoveListDIinList()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void _MoveListDItoOtherList()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void _MoveListDIQuestionChild()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void _MoveQuestionInChildItems()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void _MoveQuestionToNewChildItems()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		public void _MoveSectionToNewChildItems()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}


		[TestMethod]
		public void CloneSdcSubtreeXmlTest()
		{
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var FD = FormDesignType.DeserializeFromXmlPath(path);
			var S1 = FD.IETnodes.OfType<SectionItemType>().Take(3).ToList()[1]; //ID = 16079, BaseName = "y1bxHm"
			var S2 = FD.IETnodes.OfType<SectionItemType>().Take(3).ToList()[2]; //ID = 16182, BaseName = "HLz19G"

			var xml1 = SdcSerializer<SectionItemType>.Serialize(S1);
			var xml2 = SdcSerializer<SectionItemType>.Serialize(S2);


			var Clone1 = SdcSerializer<SectionItemType>.Deserialize(xml1); //Clone of S1 subtree
			var Clone2 = SdcSerializer<SectionItemType>.Deserialize(xml2); //Clone of S2 subtree

			//Move the Clone2 subtree under S1 in FD.  FD will have a new copy of the Clone2 subtree;
			//This is not a Move but actually a copy (Clone2 is a new copy of the S2 subtree)
			//The Clone2 subtree will be found in FD.ChildItemsNode.Last(), but with all new identifiers:
			//The Clone2 copy in FD will have new sGuids, name, ID for each node.

			Clone2.Move(S1.ChildItemsNode, -1, false, true);

			var newXml = FD.GetXml();
			var newXElement = newXml.ToXmlElement();
			Assert.IsTrue(S1.ID != S2.ID);
			Assert.IsTrue(Clone2.ID != S2.ID);
			Assert.IsTrue(Clone2.name != S2.name);
			Assert.IsTrue(Clone2.sGuid != S2.sGuid);
			var childNodeClone2 = Clone2.GetChildItemsList()![0]; //new Q
			var childNodeS2 = S2.GetChildItemsList()![0]; //original Q


			Assert.IsTrue(childNodeClone2.sGuid != childNodeS2.sGuid);

			Assert.IsTrue(childNodeClone2.ID != childNodeS2.ID);
			Assert.IsTrue(childNodeClone2.name != childNodeS2.name);




			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void CloneSdcSubtreeBsonTest()
		{
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");



			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void CloneSdcSubtreeMpackTest()
		{
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");



			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void RefreshSdcSubtreeOMTest()
		{
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");



			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}












		public bool Move(BaseType sourceNode, BaseType targetNode, DropPosition position)
		{
			//TODO: Debug.WriteLine does not output to Blazor WASM ouput window.  Using Console.Writeline instead, until this is fixed.

			try
			{
				Console.WriteLine("-------------------------------------------");
				Console.WriteLine($"{position.ToString().ToUpper()}: Source: {sourceNode.ElementName}: {sourceNode.As<DisplayedType>().title ?? "null"}, Target: {targetNode.ElementName}: {targetNode.As<DisplayedType>().title ?? "null"} ");

				//!Handle some common illegal Move cases
				if (sourceNode is null) return false;
				if (sourceNode.ParentNode is null) return false;
				if (targetNode is null) return false;

				if (sourceNode is ListItemType && (targetNode is SectionItemType)) //can't drop LI on, before or after S
					return false;
				if (targetNode.IsDescendantOf(sourceNode))
					return false;
				if (targetNode.GetType() == typeof(DisplayedType) && position == DropPosition.Over)
					return false;

				//!Begin SETUP_______________________________________________________            

				IChildItemsParent? targetAsCIP = null; //This will not be null if the targetNode can subsume a ChildItems node
				BaseType? targetAttachementSite;  //The object where sourceNode should be attached.  It will be either a List.Items or ChildItems.Items object, which contain a "List<T> Items" attachment property

				//!Test if target is QS or QM
				QuestionItemType? qsqmTarget = null; //qsqmTarget will not be null if targetNode is QS or QM
				if (targetNode is QuestionItemType q &&
						(q.GetQuestionSubtype() & QuestionEnum.QuestionSingleOrMultiple) > 0) //"Bitwise And" test for QS or QM
					qsqmTarget = q;

				//!Test if source is LI or DI
				DisplayedType? listItemNodeSource = null; //source is LI or DI
				if (sourceNode is ListItemType || sourceNode.GetType() == typeof(DisplayedType))
					listItemNodeSource = (DisplayedType)sourceNode;

				int targetIndex = 0; //drop in first position of target (List or ChildItems node)
				int sourceIndex = 0; //drop in first position of source (List or ChildItems node)
				PropertyInfoMetadata pimTarget;
				PropertyInfoMetadata pimSource;

				ChildItemsType? sourceChildItemsNode = null;
				ListType? sourceListNode = null;
				if (sourceNode.ParentNode is ChildItemsType cit)
					sourceChildItemsNode = cit;
				if (sourceNode.ParentNode is ListType lt)
					sourceListNode = lt;

				//!End SETUP_______________________________________________________      

				//Determine Drop Type:
				if (position == DropPosition.Over) //Add as the first child item, at the top of the list (itemIndex = 0)
				{
					targetAsCIP = targetNode as IChildItemsParent; //childItemsParent is null only if targetNode is DI

					if (targetNode is ListItemType li && sourceNode is ListItemType)
						return false;

					else if (qsqmTarget is not null && listItemNodeSource is not null) //If we drop LI/DI on QS/QM, add to Q LIST node
					{
						Console.WriteLine("(qsqmTarget is not null &&  listItemNodeSource is not null");
						if (sourceNode is QuestionItemType || sourceNode is SectionItemType)
							Debugger.Break(); //We should never get here

						ListType? targetTest = qsqmTarget.ListField_Item?.List;
						if (targetTest is null)
						{
							Debugger.Break(); //We should never get here
							qsqmTarget.GetListField().GetList();
						}
						targetAttachementSite = qsqmTarget.ListField_Item?.List;
						if (targetAttachementSite is null) throw new NullReferenceException("targetAttachementSite (qsqmTarget.ListField_Item) cannot be null");
					}
					else if (targetAsCIP is not null) //i.e., targetNode != DI //includes all other IChildItemsParent drop targets,with any source type
					{
						Console.WriteLine("Over: childItemsParent is not null");
						if (sourceNode is ListItemType) return false;

						targetAttachementSite = targetAsCIP.GetChildItemsNode(); //Create ChildItemsNode only when needed 

					}
					else //any other non-IChildItemsParent target nodes (only DI target nodes are left)
					{
						Console.WriteLine("nop");

						if (targetNode.GetType() != typeof(DisplayedType))
							Debugger.Break(); //we should never get here
						return false;

					} // if we get here, user tried to drop on a DisplayedType node (not IChildItemsParent)
				}
				else if (position == DropPosition.After)
				{
					Console.WriteLine("if (position == DropPosition.After)");
					pimTarget = targetNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains targetNode
					targetIndex = pimTarget.ItemIndex + 1;
					pimSource = sourceNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains targetNode
					sourceIndex = pimSource.ItemIndex;
					if (sourceNode.ParentNode == targetNode.ParentNode
						&& targetIndex > sourceIndex) targetIndex--;

					if (targetNode.ParentNode is ListType) //targetNode is LI or DI
					{
						if (listItemNodeSource is not null)
						{
							Console.WriteLine("A0");
							targetAttachementSite = targetNode.ParentNode;  //(the List node)
						}
						else
						{
							Console.WriteLine("A1");
							return false; //The source node is not LI or DI
						}
					}
					else if (sourceNode is ListItemType)
					{
						Console.WriteLine("A2");
						return false; //Can't drop LI before or after a non-LI target
					}
					else
					{
						Console.WriteLine("A3");
						targetAttachementSite = targetNode.ParentNode; //(ChildItemsNode)
					}
				}
				else if (position == DropPosition.Before)
				{
					Console.WriteLine("if (position == DropPosition.Before)");
					pimTarget = targetNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains targetNode
					targetIndex = pimTarget.ItemIndex;
					pimSource = sourceNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains sourceNode
					sourceIndex = pimSource.ItemIndex;
					if (sourceNode.ParentNode == targetNode.ParentNode
						&& targetIndex > sourceIndex) targetIndex--;  //we removed the sourceNode before re-adding in the new position, so we decrement index by one

					if (targetNode is ListItemType)
					{
						if (listItemNodeSource is not null)
						{
							Console.WriteLine("B0");
							targetAttachementSite = targetNode.ParentNode;  //(the List node)
						}
						else
						{
							Console.WriteLine("B1");
							return false; //The source node is not LI or DI
						}
					}
					else if (sourceNode is ListItemType)
					{
						Console.WriteLine("B2");
						return false; //Can't drop LI before or after a non-LI target
					}
					else
					{
						Console.WriteLine("B3");
						targetAttachementSite = targetNode.ParentNode; //(ChildItemsNode)		
					}
				}
				else { Console.WriteLine("No position"); return false; }

				Console.WriteLine($"targetAsCIP: {targetAsCIP?.As<DisplayedType>()?.title ?? "null"}");
				Console.WriteLine($"targetNode: {targetNode?.As<DisplayedType>()?.title ?? "null"}");
				Console.WriteLine($"qsqmTarget: {(qsqmTarget?.As<DisplayedType>()?.title ?? "null")}; NewTarget: {targetNode?.ElementName ?? "null"}: {targetNode?.As<DisplayedType>()?.title ?? "null"}");
				Console.WriteLine($"targetAttachementSite: {targetAttachementSite?.ElementName ?? targetAttachementSite?.GetType().Name ?? "null"}");

				if (targetAttachementSite is null) throw new InvalidOperationException("Could not determine targetAttachementSite");

				bool result = false;
				bool deleteEmptyParentNode = false;
				if (targetAttachementSite is ChildItemsType) deleteEmptyParentNode = true; //delete the ChildItems node is it is "childless"

				Console.WriteLine("Before Move");

				result = sourceNode.Move(targetAttachementSite, targetIndex, deleteEmptyParentNode);

				Console.WriteLine("After Move");
				Console.WriteLine("result: " + result);

				var subTree = targetAttachementSite.GetSubtreeIETList();
				if (subTree is not null && subTree.Count > 0)
				{
					Console.WriteLine(subTree.Count);
					foreach (IdentifiedExtensionType n in subTree)
						Console.WriteLine(n.ElementPrefix + ": " + n.As<DisplayedType>().title ?? "(null)" + "; ");
				}
				else Console.WriteLine("subTree.Count == 0");

				Console.WriteLine("END:--------------------------------------");
				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ex: {ex.Message}\r\n Inner Ex:{ex.InnerException?.Message}\r\nStack:\r\n{ex.StackTrace}");
				if (ex.InnerException?.Data is not null)
				{
					foreach (DictionaryEntry kv in ex.InnerException.Data)
						Console.WriteLine($"Key: {kv.Key}, Value: {kv.Value}");
				}
				return false;
			}
		}
		public enum DropPosition
		{
			Before,
			Over,
			After
		}
	}
}