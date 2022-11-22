using Microsoft.VisualStudio.TestTools.UnitTesting;
using MsgPack.Serialization.CollectionSerializers;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml;
using System.Xml.Linq;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class NavigationTests
	{
		private TestContext testContextInstance;

		public NavigationTests()
		{
			//previous test runs in MoveTests will change locations of some SDC nodes 
			//This can cause some Assert methods, whch depend on the order of ObjectIDs, to fail.
			//So we Reset the source SDC xml before starting this test suite
			Setup.Reset();
		}
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
		public void ReflectRefreshTree_X1_NoPrint()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			
			//Create new BaseType names
			SdcUtil.CreateName? delCreateName = SdcUtil.CreateCAPname;
			//delCreateName = null;
			
			var sdcList = SdcUtil.ReflectRefreshTree(Setup.FD, out string? s, true, true, delCreateName);
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Debug.Print(s);
			Debug.Print(Setup.FD.GetXml());

		}
		[TestMethod]
		public void ReflectRefreshTree_X3()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			SdcUtil.ReflectRefreshTree(Setup.FD, out _);
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			SdcUtil.ReflectRefreshTree(Setup.FD, out _);
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var sdcList = SdcUtil.ReflectRefreshTree(Setup.FD, out string? s, true);
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Debug.Print(s);

		}
		[TestMethod]
		public void MoveNext_ReflectNextElement()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			int i = 0;
			BaseType n = Setup.FD;
			string content;
			while (n != null)
			{
				if (n is DisplayedType) content = ": title: " + (n as DisplayedType).title;
				else if (n is PropertyType) content = ": " + (n as PropertyType).propName + ": " + (n as PropertyType).val;
				else content = "";

				Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);
				n = SdcUtil.ReflectNextElement(n);
				i++;
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void MoveNext_ReflectNextElement2()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			int i = 0;
			BaseType n = Setup.FD;
			string content;
			while (n != null)
			{
				if (n is DisplayedType) content = ": title: " + (n as DisplayedType).title;
				else if (n is PropertyType) content = ": " + (n as PropertyType).propName + ": " + (n as PropertyType).val;
				else content = "";

				Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);
				n = SdcUtil.ReflectNextElement2(n);
				i++;
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

		}

		[TestMethod]
		public void MoveNext_ReflectNextElement_X()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Stopwatch.StartNew();

			var a = (float)Stopwatch.GetTimestamp();

			var total = a;
			int i = -1;
			string content;

			List<BaseType> sortedNodes = new();
			BaseType? bt = Setup.FD;
			sortedNodes.Add(bt);

			while (bt != null)
			{
				bt = SdcUtil.ReflectNextElement(bt);
				if (bt is not null) sortedNodes.Add(bt);
			}

			Debug.Print("Seconds to Create Node Array: " + ((Stopwatch.GetTimestamp() - a) / Stopwatch.Frequency).ToString());
			Debug.Print("Seconds per Node: " + (((Stopwatch.GetTimestamp() - a) / Stopwatch.Frequency) / Setup.FD.Nodes.Count).ToString());
			a = (float)Stopwatch.GetTimestamp();

			i = -1;
			foreach (var n in sortedNodes)
			{
				i++;
				if (n is DisplayedType)
					content = ": title: " + (n as DisplayedType)?.title;
				else if (n is PropertyType)
					content = ": " + (n as PropertyType)?.propName + ": " + (n as PropertyType)?.val;
				else content = "";

				Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);
			}

			Debug.Print("Output Time" + ((Stopwatch.GetTimestamp() - a) / Stopwatch.Frequency).ToString());
			Debug.Print("Seconds per Node" + (((Stopwatch.GetTimestamp() - a) / Stopwatch.Frequency) / Setup.FD.Nodes.Count).ToString());
			Debug.Print("Total Time: " + ((Stopwatch.GetTimestamp() - total) / Stopwatch.Frequency).ToString());


			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void MoveNext_GetNextElement()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			int i = 0;
			BaseType? n = Setup.FD;
			string content;
			while (n != null)
			{
				if (n is DisplayedType) content = ": title: " + (n as DisplayedType).title;
				else if (n is PropertyType) content = ": " + (n as PropertyType).propName + ": " + (n as PropertyType).val;
				else content = "";

				Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);

				n = SdcUtil.GetNextElement(n);
				i++;
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void MoveNext_Nodes()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			int i = 0;
			BaseType n = Setup.FD;
			string content;
			//var topNode; //= (ITopNode)bt;
			//var cn = topNode.ChildNodes;

			//loop through ChildNodes
			BaseType? firstChild;
			BaseType nextSib;

			MoveNext(n);
			void MoveNext(BaseType n)
			{
				firstChild = null;
				nextSib = null;
				btPrint(n);
				n.order = i;  //almost instananeous
				Assert.IsTrue(n.ObjectID == i);//very fast
				i++;


				//if (n.TryGetChildNodes(out ReadOnlyCollection<BaseType> kids) )
				//{
					firstChild = n.GetChildNodes()?.First();
					if (firstChild != null)
						MoveNext(firstChild);
				//}


				var par = n.ParentNode;
				if (par != null)
				{
					//if (par.TryGetChildNodes(out ReadOnlyCollection<BaseType> sibList))
					//{
					var sibList = par.GetChildNodes()?.ToList();
					if (sibList is not null)
					{
						int index = sibList.IndexOf(n);
						if (index < sibList.Count - 1)
						{
							nextSib = sibList[index + 1];
							if (nextSib != null)
								MoveNext(nextSib);
						}
					}
					//}
				}
			}



			void btPrint(BaseType n)
			{
				if (n is DisplayedType) content = ": title: " + (n as DisplayedType)?.title;
				else if (n is PropertyType) content = ": " + (n as PropertyType)?.propName + ": " + (n as PropertyType)?.val;
				else content = "";

				Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void MoveNext_Iterator()
		{
			Setup.Reset();

			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			int i = 0;

			foreach (BaseType node in NodeIterator(Setup.FD))
			{
				btPrint(node);
			}

			IEnumerable<BaseType> NodeIterator(BaseType? n)
			{
				while (n is not null)
				{
					n = MoveNext(n);
					if (n is not null)
						yield return n;
					else yield break;
				}
				yield break;
			}

			BaseType? MoveNext(BaseType n)
			{
				//var topNode = (ITopNode)n;
				//Dictionary<Guid, List<BaseType>> cn = n.GetChildList();
				BaseType? nextNode;

				n.order = i;  //almost instananeous
				Assert.IsTrue(n.ObjectID == i);//very fast
				i++;
				//if n has child nodes, the next node is the first child node of n.
				if (n.TryGetChildNodes(out var childList))
				{
					nextNode = childList?[0];
					if (nextNode is not null) return nextNode;
				}

				//n has no child nodes, so walk up the tree to the parent node.
				//When we walk back up the object graph, prevPar is the original starting node (deeper in the tree),
				//while par is the parent of prevPar, more superficial in the tree, and closer to the top node
				//We then check the childList of par, to see if prePar can be found in that childList
				//If prevPar is in childList, then try to retrieve the next node in childList
				//IF we cant get the nextNode from childList, then move up to one higher parent level
				var prevPar = n;
				var par = prevPar.ParentNode;

				while (par is not null)
				{
					if (par.TryGetChildNodes(out childList))
					{
						var index = childList?.IndexOf(prevPar)??-1;
						if (index < childList?.Count - 1)
						{
							nextNode = childList?[index + 1];
							if (nextNode is not null) return nextNode;
						}
						//the next node is not located yet, so walk up to a previous ancestor and try again,
						//looking in that ancestors childList for a nextNode candidate
						prevPar = par;
						par = prevPar.ParentNode;
					}
				}
				return null;
			}

			void btPrint(BaseType n)
			{
				string content;
				if (n is not null)
				{
					if (n is DisplayedType) content = ": title: " + (n as DisplayedType)?.title;
					else if (n is PropertyType) content = ": " + (n as PropertyType)?.propName + ": " + (n as PropertyType)?.val;
					else content = "";

					Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);
				}
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void MoveNext_NodesToSortedList()
		{


			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			int i = 0;
			BaseType n = Setup.FD;
			string content;
			//var cn = n.TopNode.ChildNodes;

			//loop through ChildNodes
			BaseType? firstChild;
			BaseType? nextSib;

			var sortedList = new List<BaseType>();
			BaseType[] sortedArray = new BaseType[Setup.FD.Nodes.Count];

			MoveNext(n);

			void MoveNext(BaseType n)
			{
				firstChild = null;
				nextSib = null;
				//btPrint(n);
				n.order = i;  //almost instananeous
				sortedList.Add(n);
				//if (i == 0 || i == Setup.FD.Nodes.Count ) Debugger.Break();
				sortedArray[i] = n;
				Assert.IsTrue(n.ObjectID == i);//very fast
				i++;


				if (n.TryGetChildNodes( out var childList))
				{
					firstChild = childList?[0];
					if (firstChild != null)
						MoveNext(firstChild);
				}


				var par = n.ParentNode;
				if (par != null)
				{
					if (par.TryGetChildNodes( out var sibList))
					{
						var index = sibList.IndexOf(n);
						if (index < sibList.Count - 1)
						{
							nextSib = sibList[index + 1];
							if (nextSib != null)
								MoveNext(nextSib);	
						}
					}
				}
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void MovePrev_GetPrevElement()
		{

			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			SdcUtil.TreeSort_ClearNodeIds();

			BaseType? n = SdcUtil.GetLastDescendantElement(Setup.FD);
			int i = Setup.FD.Nodes.Count - 1;
			string content;

			while (n != null)
			{
				if (n is DisplayedType) content = ": title: " + (n as DisplayedType)?.title;
				else if (n is PropertyType) content = ": " + (n! as PropertyType)?.propName + ": " + (n as PropertyType)?.val;
				else content = "";

				Debug.Print(n.ObjectID.ToString().PadLeft(4) + ": " + i.ToString().PadLeft(4) + ": " + (n.name ?? "").PadRight(20) + ": " + (n.ElementName ?? "").PadRight(25) + content);

				n = SdcUtil.GetPrevElement(n);
				i--;
			}
			SdcUtil.TreeSort_ClearNodeIds();
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void GetLastDescendant()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			var n = SdcUtil.GetLastDescendantElement(Setup.FD.Body);
			Assert.IsTrue(n.ElementName == "LocalFunctionName" && n.type == "submit");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}


		[TestMethod]
		public void SLOW_MoveNext_NodesToSortedListByTreeComparer()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			var sa = Setup.FD.Nodes.Values.ToList();
			sa.Sort(new TreeComparer());

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void IsList()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void GetNamedItem()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void GetPropertyInfoMetadata()
		{ //given an SDC item, find the property that references it in the item.ParentNode class


			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var t = Setup.FD;

			var qList = t.Nodes.Where(n => n.Value is QuestionItemType).Select(n => n.Value).ToList();
			var sList = t.Nodes.Where(n => n.Value is SectionItemType).Select(n => n.Value).ToList();
			var aList = t.Nodes.Where(n => n.Value is ListItemType).Select(n => n.Value).ToList();
			var cList = t.Nodes.Where(n => n.Value is ChildItemsType).Select(n => n.Value).ToList();
			var pList = t.Nodes.Where(n => n.Value is PropertyType).Select(n => n.Value).ToList();


			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(qList[1]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(sList[1]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(aList[1]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(cList[1]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(pList[1]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(qList[10]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(sList[10]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(aList[10]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(cList[10]).ToString());
			Debug.Print(SdcUtil.GetElementPropertyInfoMeta(pList[10]).ToString());
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void GetPropertyInfoMetadata_Complete()
		{

			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			Stopwatch.StartNew();
			var a = Stopwatch.GetTimestamp();

			foreach (var n in Setup.FD.Nodes)
			{
				SdcUtil.GetElementPropertyInfoMeta(n.Value);
				//Debug.Print(ISdcUtil.GetPropertyInfo(n.Value).ToString());
			}
			Debug.Print(((((float)Stopwatch.GetTimestamp() - a) / Stopwatch.Frequency) / Setup.FD.Nodes.Count).ToString());
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void TreeComparer()
		{

			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			BaseType.ResetRootNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Adrenal.Bx.Res.129_3.004.001.REL_sdcFDF_test.xml");
			var FDbad = FormDesignType.DeserializeFromXmlPath(path); //used to compare nodes in another tree
			var adr = FDbad.Nodes.Values.ToArray(); //this creates shallow copies with do not retain ParentNode refs, etc.


			var tc = new TreeComparer();
			var n = Setup.FD.Nodes.Values.ToArray();//this creates shallow copies with do not retain ParentNode refs, etc.

			Stopwatch.StartNew();
			var a = Stopwatch.GetTimestamp();
			Debug.Print("Returns -1 \r\n");
			Assert.AreEqual(tc.Compare(n[0], n[1]), -1);
			Assert.AreEqual(tc.Compare(n[0], n[2]), -1);
			Assert.AreEqual(tc.Compare(n[0], n[3]), -1);
			Assert.AreEqual(tc.Compare(n[0], n[10]), -1);
			Assert.AreEqual(tc.Compare(n[0], n[3]), -1);
			Assert.AreEqual(tc.Compare(n[0], n[8]), -1);
			Assert.AreEqual(tc.Compare(n[1], n[8]), -1);
			Debug.Print("Returns 0 \r\n");
			Assert.AreEqual(tc.Compare(n[1], n[1]), 0);
			Assert.AreEqual(tc.Compare(n[2], n[2]), 0);
			Assert.AreEqual(tc.Compare(n[3], n[3]), 0);
			Assert.AreEqual(tc.Compare(n[10], n[10]), 0);
			Assert.AreEqual(tc.Compare(n[20], n[20]), 0);
			Assert.AreEqual(tc.Compare(n[30], n[30]), 0);
			Assert.AreEqual(tc.Compare(n[50], n[50]), 0);
			Debug.Print("\r\n expected results: -1, 1, -1, 1, -1, 1, -1 ");
			Assert.AreEqual(tc.Compare(n[1], n[2]), -1);// -1
			Assert.AreEqual(tc.Compare(n[2], n[1]), 1);// 1
			Assert.AreEqual(tc.Compare(n[33], n[34]), -1);// -1
			Assert.AreEqual(tc.Compare(n[20], n[10]), 1);// 1
			Assert.AreEqual(tc.Compare(n[199], n[201]), -1);// -1
			Assert.AreEqual(tc.Compare(n[201], n[200]), 1);// 1
			Assert.AreEqual(tc.Compare(n[29], n[32]), -1);// -1
			Debug.Print("\r\n expected results: -1, 1, -1, 1, -1, 1, -1 ");
			Assert.AreEqual(tc.Compare(n[299], n[301]), -1);// -1
			Assert.AreEqual(tc.Compare(n[401], n[300]), 1);// 1
			Assert.AreEqual(tc.Compare(n[39], n[42]), -1);// -1
			Assert.AreEqual(tc.Compare(n[21], n[11]), 1);// 1
			Assert.AreEqual(tc.Compare(n[11], n[12]), -1);// -1
			Assert.AreEqual(tc.Compare(n[341], n[133]), 1);// 1
			Assert.AreEqual(tc.Compare(n[101], n[120]), -1);// -1


			Debug.Print("\r\n");
			Assert.AreEqual(tc.Compare(n[2], n[1]), 1);
			Assert.AreEqual(tc.Compare(n[4], n[0]), 1);
			Assert.AreEqual(tc.Compare(n[6], n[4]), 1);
			Assert.AreEqual(tc.Compare(n[20], n[2]), 1);
			Assert.AreEqual(tc.Compare(n[40], n[0]), 1);
			Assert.AreEqual(tc.Compare(n[60], n[0]), 1);
			Assert.AreEqual(tc.Compare(n[100], n[0]), 1);
			Debug.Print("\r\n");

			try { Debug.Print(tc.Compare(n[100], adr[0]).ToString()); } catch { Debug.Print("error caught"); }
			try { Debug.Print(tc.Compare(adr[0], n[100]).ToString()); } catch { Debug.Print("error caught"); }
			try { Debug.Print(tc.Compare(n[10], adr[12]).ToString()); } catch { Debug.Print("error caught"); }
			try { Debug.Print(tc.Compare(adr[100], adr[100]).ToString()); } catch { Debug.Print("error caught"); }




			Debug.Print(((float)(Stopwatch.GetTimestamp() - a) / ((float)Stopwatch.Frequency)).ToString());


			//Seconds per comparison: @ 0.0006 sec/comparison
			a = Stopwatch.GetTimestamp();
			for (int i = 0; i < 100; i++)
			{
				tc.Compare(n[299], n[301]);
				tc.Compare(n[2101], n[120]);
			}
			Debug.Print(((float)(Stopwatch.GetTimestamp() - a) / ((float)Stopwatch.Frequency) / 200).ToString());
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void RefreshParentNodesFromXml()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			BaseType.ResetRootNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Adrenal.Bx.Res.129_3.004.001.REL_sdcFDF_test.xml");
			var FDbad = FormDesignType.DeserializeFromXmlPath(path); //used to compare nodes in another tree
			var adr = FDbad.Nodes.Values.ToArray<BaseType>();

			foreach (var n in adr)
			{
				Debug.Print(n.name + ", par: " + n.ParentNode?.name);
			}
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void GetEventParent()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void IsItemChangeAllowed()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		
		[TestMethod]
		public void ReflectChildList()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var lst = SdcUtil.ReflectChildElements(Setup.FD);
			foreach (var n in lst)
				Debug.Print($"{n.order}: \t Name: {n.name}");
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void ReflectSubtree()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			var lst = Setup.FD.TopNode.GetNodeByName("S_57219")
				.GetSubtreeList();
			//Setup.FD._
			foreach (var n in lst)
				Debug.Print($"{n.order}: \t Name: {n.name}");
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		
		[TestMethod]
		public void Misc()
		{
			//SectionItemType S;
			//need AddActionNode
			//Prevent adding nodes without  going through dictionaries
			//Ensure that all add/remove functions use dictionaries   
			//
			//S.AddOnExit().Actions.AddActSendReport();
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}


	}
}