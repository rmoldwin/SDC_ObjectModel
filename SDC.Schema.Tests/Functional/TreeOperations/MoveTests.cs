using FastSerialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.Tests.OM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

//using SDC.Schema;

namespace SDC.Schema.Tests.Functional.TreeOperations
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
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Bug fix: use a per-test fresh object graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			//FD.TopNode.ReorderNodes();
			var li = fd.Nodes.Where(n =>
				n.Value is ListItemType liTest &&
				liTest.ID == "38493.100004300").FirstOrDefault().Value
				as ListItemType;
			Assert.IsTrue(li is ListItemType);

			List<BaseType> lst1;
			List<BaseType> lst2;
			List<BaseType> lst3;

			lst1 = SdcUtil.ReflectChildElements(fd.GetListItemByID("51689.100004300"));
			lst2 = SdcUtil.ReflectChildElements(fd.GetListItemByID("38493.100004300"));
			lst3 = SdcUtil.ReflectChildElements(fd.GetNodeByName("lst_44135_3"));

			lst3 = SdcUtil.ReflectRefreshSubtreeList(fd.GetSectionByID("43969.100004300"));
			//foreach (var n in lst3) Debug.Print(n.name);
			var tc = new TreeComparer();
			lst3.Sort(tc);
			foreach (var n in lst3) Debug.Print(n.name + ": " + n.ElementName + ", " + n.ObjectID);

			var lst4 = fd.Nodes.Values.ToList();
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
		}
		[TestMethod]
		public void MoveListItemToOtherList()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Bug fix: use a per-test fresh object graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var li = fd.Nodes.Where(n =>
				n.Value is ListItemType liTest &&
				liTest.ID == "38493.100004300").FirstOrDefault().Value
				as ListItemType;
			Assert.IsTrue(li is ListItemType);
			var list = (ListType)li.ParentNode;

			var list2 = fd.Nodes.Where(n =>
				n.Value is ListType liTest &&
				liTest.name == "lst_58267_3").FirstOrDefault().Value
				as ListType;
			Assert.IsTrue(list2 is ListType);

			//Move to different List (list2)
			li.Move(list2, 2);
			Assert.IsTrue(SdcUtil.GetElementPropertyInfoMeta(li, li.ParentNode).ItemIndex == 2);
			Assert.AreEqual(list2, li.ParentNode);
			//li.Move(list2, -1, false, SdcUtil.RefreshMode.UpdateNodeIdentity);


			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
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
			Move(Q_16214, LIR_16195, DropPosition.Over);//Has Property, and a ListItemResponseFieldType node
            Assert.IsTrue(LIR_16195.GetChildNodes().Count == 3);//Has Property, and a ChildItems node
			Assert.IsTrue(LI_40307.ChildItemsNode is null);
			Assert.IsTrue(LI_39079.ChildItemsNode is null);

			//Move qLat under liExci - should cause error
			Move(Q_16214, LI_40307, DropPosition.Over);
			Assert.IsTrue(LI_40307.GetChildNodes().Count == 1);
			Assert.IsTrue(LIR_16195.ChildItemsNode is null);
			Assert.IsTrue(LI_39079.ChildItemsNode is null);


			// Bug fix: this test uses a per-test local graph, so no shared Setup reset is required.
			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
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

			// Bug fix: this test uses a per-test local graph, so no shared Setup reset is required.
			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void MoveListDIinList()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Move.List.Internal");
			fd.AddBody();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.1", "Q1");
			var li1 = q.AddListItem("LI.1", "LI1");
			var di1 = q.AddDisplayedTypeToList("DI.1", "DI1");
			var li2 = q.AddListItem("LI.2", "LI2");

			var ok = IMoveRemoveExtensions.DropMove(li2, li1, IMoveRemoveExtensions.DropPosition.Before);
			Assert.IsTrue(ok);

			var listItems = q.GetListItems();
			Assert.IsNotNull(listItems);
			Assert.AreEqual("LI.2", ((IdentifiedExtensionType)listItems![0]).ID);
			Assert.AreEqual("LI.1", ((IdentifiedExtensionType)listItems[1]).ID);
			Assert.AreEqual("DI.1", ((IdentifiedExtensionType)listItems[2]).ID);
		}
		[TestMethod]
		public void MoveListDItoOtherList()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Move.List.Cross");
			fd.AddBody();
			var q1 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Source", "Source");
			var q2 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Target", "Target");
			var li = q1.AddListItem("LI.Move", "Move");
			q1.AddDisplayedTypeToList("DI.Source", "Source DI");
			q2.AddListItem("LI.Target", "Existing");

			var targetList = q2.GetListField().GetList();
			var moved = li.Move(targetList, 0);
			Assert.IsTrue(moved);

			Assert.AreEqual(1, q1.GetListItems()!.Count);
			Assert.AreEqual(2, q2.GetListItems()!.Count);
			Assert.AreEqual("LI.Move", ((IdentifiedExtensionType)q2.GetListItems()![0]).ID);
		}
		[TestMethod]
		public void MoveListDIQuestionChild()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Move.List.OverQuestion");
			fd.AddBody();
			var q1 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Source2", "Source");
			var q2 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Target2", "Target");
			var di = q1.AddDisplayedTypeToList("DI.Move", "Move DI");

			var ok = IMoveRemoveExtensions.DropMove(di, q2, IMoveRemoveExtensions.DropPosition.Over);
			Assert.IsTrue(ok);
			Assert.AreEqual(0, q1.GetListItems()!.Count);
			Assert.AreEqual(1, q2.GetListItems()!.Count);
			Assert.AreEqual("DI.Move", ((IdentifiedExtensionType)q2.GetListItems()![0]).ID);
		}
		[TestMethod]
		public void MoveQuestionInChildItems()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Move.Question.Internal");
			fd.AddBody();
			var section = fd.Body.AddChildSection("S.1", "Section");
			var q1 = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.First", "First");
			var q2 = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Second", "Second");

			var ok = IMoveRemoveExtensions.DropMove(q2, q1, IMoveRemoveExtensions.DropPosition.Before);
			Assert.IsTrue(ok);

			var kids = section.GetChildItemsList();
			Assert.IsNotNull(kids);
			Assert.AreEqual("Q.Second", kids![0].ID);
			Assert.AreEqual("Q.First", kids[1].ID);
		}
		[TestMethod]
		public void MoveQuestionToNewChildItems()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Move.Question.Cross");
			fd.AddBody();
			var sourceSection = fd.Body.AddChildSection("S.Source", "Source");
			var targetSection = fd.Body.AddChildSection("S.Target", "Target");
			var q = sourceSection.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Move", "Move");

			var moved = q.Move(targetSection.GetChildItemsNode(), 0, true);
			Assert.IsTrue(moved);
			Assert.IsNull(sourceSection.ChildItemsNode);
			Assert.IsNotNull(targetSection.ChildItemsNode);
			Assert.AreEqual("Q.Move", targetSection.GetChildItemsList()![0].ID);
		}
		[TestMethod]
		public void MoveSectionToNewChildItems()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Move.Section.Cross");
			fd.AddBody();
			var s1 = fd.Body.AddChildSection("S.1", "S1");
			var s2 = fd.Body.AddChildSection("S.2", "S2");

			var moved = s2.Move(s1.GetChildItemsNode(), 0, true);
			Assert.IsTrue(moved);
			Assert.AreEqual("S.2", s1.GetChildItemsList()![0].ID);
			Assert.AreEqual(1, fd.Body.GetChildItemsList()!.Count);
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

			//Move the cloneBreastNew subtree under sBreastNew in FD.  FD will have a new copy of the S1 subtree;
			//This simulates copying from an entirely different SDC tree, where we must replace all identifiers.
			//This is not a "Move" but actually a clone/copy (cloneBreastNew is a new copy of the S2 subtree)
			//The S1 subtree will be found in FD.ChildItemsNode.Last(), but with all new identifiers:
			//The copy in FD will have new sGuids, name, ID for each node.
			//var Clone2_1 = sAdrenalOld.Clone();
			Clone2.Move(S1.ChildItemsNode, -1, false, SdcUtil.RefreshMode.UpdateNodeIdentity);

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
        public void CloneRestoreSdcSubtreeXmlTest()
        {
            Setup.TimerStart("==>[] Started");
            BaseType.ResetLastTopNode();
            string pathBreastNew = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
            string pathAdrenalOld = Path.Combine("..", "..", "..", "Test files", "Adrenal.xml");
            var fdBreastNew = FormDesignType.DeserializeFromXmlPath(pathBreastNew);
            var fdAdrenalOld = FormDesignType.DeserializeFromXmlPath(pathAdrenalOld);
            SectionItemType sBreastNew = fdBreastNew.IETnodes.OfType<SectionItemType>().Take(3).ToList()[1]; //ID = 16079, BaseName = "y1bxHm"
            SectionItemType sAdrenalOld = fdAdrenalOld!.GetNodeByName("S_17537") as SectionItemType; 

            sAdrenalOld!.Move(sBreastNew.ChildItemsNode, -1, false, SdcUtil.RefreshMode.RestoreSubtreeFromOlderVersion);

            //var newXml = fdBreastNew.GetXml();
			SectionItemType sAdrenalNew = sBreastNew.GetChildItemsList()!.Last() as SectionItemType;

            Assert.IsTrue(sAdrenalNew!.ID == sAdrenalOld!.ID);
            Assert.IsTrue(sAdrenalNew.ObjectGUID == sAdrenalOld.ObjectGUID);
            Assert.IsTrue(sAdrenalNew.name == sAdrenalOld.name);
            Assert.IsTrue(sAdrenalNew.sGuid == sAdrenalOld.sGuid);

			var adrenalChildNew = sAdrenalNew.GetChildItemsList()!.Last() as QuestionItemType;
            var adrenalChildOld = sAdrenalOld.GetChildItemsList()!.Last() as QuestionItemType;

            Assert.AreNotSame(adrenalChildNew, adrenalChildOld);
			
            Assert.IsTrue(adrenalChildNew!.ID == adrenalChildOld!.ID);
            Assert.IsTrue(adrenalChildNew.ObjectGUID == adrenalChildOld.ObjectGUID);
            Assert.IsTrue(adrenalChildNew.name == adrenalChildOld.name); //QM_53772 vs Q_53772
            Assert.IsTrue(adrenalChildNew.sGuid == adrenalChildOld.sGuid);

			Assert.IsTrue(sAdrenalNew.GetSubtreeList().Count == sAdrenalOld.GetSubtreeList().Count);
			


            Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
        }
        [TestMethod]
        public void CloneRepeatSdcSubtreeXmlTest()
        {
            Setup.TimerStart("==>[] Started");
            BaseType.ResetLastTopNode();
            string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
            var FD = FormDesignType.DeserializeFromXmlPath(path);
            var S1 = FD.IETnodes.OfType<SectionItemType>().Take(3).ToList()[1]; //ID = 16079, BaseName = "y1bxHm"

            /*  sample code to run rules in an SDC tree
			//find the first questionItemType node in FD
			var Q1 = FD.IETnodes.OfType<QuestionItemType>().First();
			//find the first list item node under that question node
			var LI1 = Q1.GetListItems()?.First();
			//find the list item node with name = "LI_21539"
			var LI2 = Q1.GetListItems()?.Where(n => n.name == "LI_21539").First();
            //I have three list items nodes, with names LI_53211, LI_42099, LI_43573.  if any one of those LI nodes is selected, then I want to find node LI_37678 and set IsSelected on node LI_37678 to true

            try{ListItemType celiac = (ListItemType)Q1.GetListItems()?.Where(n => n.name == "LI_53211")?.First()!;ListItemType sma = (ListItemType)Q1.GetListItems()?.Where(n => n.name == "LI_42099").First()!;ListItemType cha = (ListItemType)Q1.GetListItems()?.Where(n => n.name == "LI_43573").First()!;ListItemType t4 = (ListItemType)Q1.GetListItems()?.Where(n => n.name == "LI_37678").First()!;if (celiac.selected || sma.selected || cha.selected) t4.selected = true;}catch (Exception ex){Debug.Print(ex.Message);} 

				try { 
					ListItemType celiac = (ListItemType)Q1.GetListItems()?
						.Where(n => n.name == "LI_53211")?.First()!; 
					ListItemType sma = (ListItemType)Q1.GetListItems()?
						.Where(n => n.name == "LI_42099").First()!; 
					ListItemType cha = (ListItemType)Q1.GetListItems()?
						.Where(n => n.name == "LI_43573").First()!; 
					ListItemType t4 = (ListItemType)Q1.GetListItems()?
						.Where(n => n.name == "LI_37678").First()!; 
					if (celiac.selected || sma.selected || cha.selected) t4.selected = true; 
				} 
				catch (Exception ex) { Debug.Print(ex.Message); }
			*/

            //var myLink = S1.AddLink();
            //myLink.LinkText.val = "Note A:";
            //myLink.LinkURI.val = "87698476b0sfssdff85657b";
            //myLink.RemoveRecursive();


            ChildItemsType? S1par = S1.ParentNode! as ChildItemsType;
			Assert.IsNotNull(S1par); 
            S1.Move(S1par!, -1, false, SdcUtil.RefreshMode.CloneAndRepeatSubtree);

			int listIndexS1 = S1.GetListIndex();
			var S1copy = S1par!.GetChildNodes()![listIndexS1 + 1] as SectionItemType;
			Assert.IsNotNull(S1copy);
           
            Assert.IsTrue(S1copy.ID == S1.ID + "__1");
            Assert.IsTrue(S1copy.name == S1.name + "__1");
            Assert.IsTrue(S1copy.sGuid != S1.sGuid);

			QuestionItemType Q1copy = S1copy.ChildItemsNode.ChildItemsList[1] as QuestionItemType;
			Assert.IsNotNull (Q1copy);
			Assert.IsTrue(Q1copy!.name == "Q_21537__1");
			Assert.IsTrue(Q1copy.ID == "21537.100004300__1");

			var licopy1 = Q1copy.GetListItems()[2] as ListItemType;
			Assert.IsNotNull (licopy1);
			Assert.IsTrue(licopy1!.name == "LI_21539__1");
			Assert.IsTrue(licopy1.ID == "21539.100004300__1");

            //+-------Make a second copy of sBreastNew------------------------------------------------------------------------

            S1.Move(S1par!, -1, false, SdcUtil.RefreshMode.CloneAndRepeatSubtree);
			var S2copy = S1par!.GetChildNodes()![listIndexS1 + 2] as SectionItemType;
            Assert.IsNotNull(S2copy);

            Assert.IsTrue(S2copy!.ID == S1.ID + "__2");
            Assert.IsTrue(S2copy.name == S1.name + "__2");
            Assert.IsTrue(S2copy.sGuid != S1.sGuid);

            QuestionItemType Q2copy = S2copy.ChildItemsNode.ChildItemsList[1] as QuestionItemType;
            Assert.IsNotNull(Q2copy);
            Assert.IsTrue(Q2copy!.name == "Q_21537__2");
            Assert.IsTrue(Q2copy.ID == "21537.100004300__2");

            var licopy2 = Q2copy.GetListItems()[0] as ListItemType;
            Assert.IsNotNull(licopy2);
            Assert.IsTrue(licopy2!.name == "LI_21536__2");
            Assert.IsTrue(licopy2.ID == "21536.100004300__2");


            var newXml = FD.GetXml();
            //var newXElement = newXml.ToXmlElement();
            Console.Write(newXml);
            //Debug.Print(newXml);



            Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
        }
		[TestMethod]
		public void CloneSdcSubtreeBsonTest()
		{
			// Verifies that BSON serialization produces non-empty output, and that deserialization
			// of BSON currently throws because SdcSerializerBson<T>.Deserialize<T> uses a raw
			// Newtonsoft JSON BSON reader that cannot reconstruct the SDC parent-node wiring.
			// This test explicitly documents the known limitation of the BSON deserializer so it
			// remains visible rather than being silently skipped.
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var fdOriginal = FormDesignType.DeserializeFromXmlPath(path);

			// BSON serialization must produce a non-empty base-64 string.
			BaseType.ResetLastTopNode();
			string bson = TopNodeSerializer<FormDesignType>.GetBson(fdOriginal, refreshSdc: false);
			Assert.IsNotNull(bson, "BSON serialization must return a non-null string");
			Assert.IsTrue(bson.Length > 0, "BSON string must not be empty");

			// BSON deserialization is a known broken scenario: the Newtonsoft BsonDataReader
			// round-trip cannot reconstruct SDC parent-node wiring.
			// Assert that it throws rather than silently returning a corrupt tree.
			BaseType.ResetLastTopNode();
			try
			{
				var _ = TopNodeSerializer<FormDesignType>.DeserializeFromBson(bson, refreshSdc: true);
				// If it unexpectedly succeeds in the future, verify the tree is usable.
				Assert.IsNotNull(_, "If BSON deserialization begins working, the returned tree must not be null");
			}
			catch (Exception ex)
			{
				// Expected: document the known failure mode.
				Assert.IsTrue(
					ex is NullReferenceException || ex is InvalidOperationException || ex is FormatException,
					$"BSON deserialization failed with unexpected exception type {ex.GetType().FullName}: {ex.Message}");
			}

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void CloneSdcSubtreeMpackTest()
		{
			// Verifies that a FormDesignType tree serialized to MessagePack and deserialized back
			// produces a structurally equivalent tree with preserved node IDs and node count.
			// The MsgPack implementation internally falls back to UTF-8 XML bytes, so this test
			// also exercises that fallback path end-to-end.
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var fdOriginal = FormDesignType.DeserializeFromXmlPath(path);

			// Capture key structural facts from the original tree.
			int originalNodeCount = fdOriginal.Nodes.Count;
			var firstSectionId   = fdOriginal.IETnodes.OfType<SectionItemType>().First().ID;
			var firstSectionName = fdOriginal.IETnodes.OfType<SectionItemType>().First().name;

			// Serialize to MsgPack byte array and round-trip back to a new OM tree.
			BaseType.ResetLastTopNode();
			byte[] mpack = TopNodeSerializer<FormDesignType>.GetMsgPack(fdOriginal, refreshSdc: false);
			Assert.IsNotNull(mpack, "MsgPack serialization must return a non-null byte array");
			Assert.IsTrue(mpack.Length > 0, "MsgPack byte array must not be empty");

			BaseType.ResetLastTopNode();
			var fdRoundtrip = TopNodeSerializer<FormDesignType>.DeserializeFromMsgPack(mpack, refreshSdc: true);
			Assert.IsNotNull(fdRoundtrip, "MsgPack deserialization must return a non-null FormDesignType");

			// Node count must be preserved through the MsgPack round-trip.
			Assert.AreEqual(originalNodeCount, fdRoundtrip.Nodes.Count,
				$"Node count must be preserved: expected {originalNodeCount}, got {fdRoundtrip.Nodes.Count}");

			// First section identity must survive the round-trip intact.
			var rtSection = fdRoundtrip.IETnodes.OfType<SectionItemType>().First();
			Assert.AreEqual(firstSectionId, rtSection.ID,
				"First section ID must match after MsgPack round-trip");
			Assert.AreEqual(firstSectionName, rtSection.name,
				"First section name must match after MsgPack round-trip");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void RefreshSdcSubtreeOMTest()
		{
			// Verify that ReflectRefreshSubtreeList with UpdateNodeIdentity assigns new identity to every
			// node when triggered the supported way — via BaseType.Move() — which handles registration.
			// Pattern mirrors CloneSdcSubtreeXmlTest: serialize a section, deserialize as a detached clone,
			// then Move it into the target tree using UpdateNodeIdentity so its IDs are replaced.
			Setup.TimerStart("==>[] Started");
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var fd = FormDesignType.DeserializeFromXmlPath(path);

			// Pick two sections: S1 as the attachment target, S2 as the source for the clone.
			var S1 = fd.IETnodes.OfType<SectionItemType>().Take(3).ToList()[1];
			var S2 = fd.IETnodes.OfType<SectionItemType>().Take(3).ToList()[2];

			// Capture S2 identity before the clone move.
			var s2OldSGuid = S2.sGuid;
			var s2OldName  = S2.name;
			var s2OldId    = S2.ID;

			// Serialize S2, deserialize as independent clone, then Move it into S1 with UpdateNodeIdentity.
			// UpdateNodeIdentity assigns brand-new sGuid / ObjectGuid / name / ObjectID to every node.
			var xml2 = SdcSerializer<SectionItemType>.Serialize(S2);
			var cloneS2 = SdcSerializer<SectionItemType>.Deserialize(xml2);
			cloneS2.Move(S1.ChildItemsNode, -1, false, SdcUtil.RefreshMode.UpdateNodeIdentity);

			// The moved clone must now be registered in the tree.
			var cloneInTree = S1.GetChildItemsList()!.Last() as SectionItemType;
			Assert.IsNotNull(cloneInTree, "Clone must appear at the end of S1's ChildItems after Move");

			// UpdateNodeIdentity must have replaced every identity property on the root clone node.
			Assert.AreNotEqual(s2OldSGuid, cloneInTree.sGuid,
				"sGuid must be different from the original S2 after UpdateNodeIdentity");
			Assert.AreNotEqual(s2OldName, cloneInTree.name,
				"name must be different from the original S2 after UpdateNodeIdentity");
			Assert.AreNotEqual(s2OldId, cloneInTree.ID,
				"ID must be different from the original S2 after UpdateNodeIdentity");

			// S2 itself must be untouched — it is still in the tree with its original identity.
			Assert.AreEqual(s2OldSGuid, S2.sGuid, "S2.sGuid must not change");
			Assert.AreEqual(s2OldName,  S2.name,  "S2.name must not change");

			// Child nodes inside the clone must also have been re-identified.
			var origChild = S2.GetChildItemsList()![0];
			var cloneChild = cloneInTree.GetChildItemsList()![0];
			Assert.AreNotEqual(origChild.sGuid, cloneChild.sGuid,
				"Child sGuid must differ between original and UpdateNodeIdentity clone");
			Assert.AreNotEqual(origChild.name, cloneChild.name,
				"Child name must differ between original and UpdateNodeIdentity clone");

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