using MsgPack.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDC.Schema;
using System;
using System.Buffers;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Security.Cryptography;
//using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace SDC.Schema
{
	//public static class SdcSerialization
	//{         //TODO: why are these internal static methods in BaseType?  Should they be in SdcUtil or another helper class?
	//    //!+XML
	//    internal static T GetSdcObjectFromXmlPath<T>(string path) where T : ITopNode
	//    {
	//        string sdcXml = System.IO.File.ReadAllText(path);  // System.Text.Encoding.UTF8);
	//        return GetSdcObjectFromXml<T>(sdcXml);
	//    }
	//    internal static T GetSdcObjectFromXml<T>(string sdcXml) where T : ITopNode
	//    {
	//        T obj = SdcEntityBase<T>.Deserialize(sdcXml);
	//        return InitParentNodesFromXml<T>(sdcXml, obj); ;
	//    }
	//    //!+JSON
	//    internal static T GetSdcObjectFromJsonPath<T>(string path) where T : ITopNode
	//    {
	//        string sdcJson = System.IO.File.ReadAllText(path);
	//        return GetSdcObjectFromJson<T>(sdcJson);
	//    }
	//    internal static T GetSdcObjectFromJson<T>(string sdcJson) where T : ITopNode
	//    {
	//        T obj = SdcEntityBase<T>.DeserializeJson(sdcJson);
	//        //InitParentNodesFromXml<T>(obj.GetXml(), obj);
	//        return InitParentNodesFromXml<T>(obj.GetXml(), obj); ;
	//    }
	//    //!+MsgPack
	//    internal static T GetSdcObjectFromMsgPackPath<T>(string path) where T : ITopNode
	//    {
	//        byte[] sdcMsgPack = System.IO.File.ReadAllBytes(path);
	//        return GetSdcObjectFromMsgPack<T>(sdcMsgPack);
	//    }
	//    internal static T GetSdcObjectFromMsgPack<T>(byte[] sdcMsgPack) where T : ITopNode
	//    {
	//        T obj = SdcEntityBase<T>.DeserializeMsgPack(sdcMsgPack);
	//        return InitParentNodesFromXml<T>(obj.GetXml(), obj);
	//    }

	//    private static T InitParentNodesFromXml<T>(string sdcXml, T obj) where T : ITopNode
	//    {
	//        //read as XMLDocument to walk tree
	//        var x = new System.Xml.XmlDocument();
	//        x.LoadXml(sdcXml);
	//        XmlNodeList xmlNodeList = x.SelectNodes("//*");

	//        var dX_obj = new Dictionary<int, Guid>(); //the index is iXmlNode, value is FD ObjectGUID
	//        int iXmlNode = 0;
	//        XmlNode xmlNode;

	//        foreach (BaseType bt in obj.Nodes.Values)
	//        {   //As we interate through the nodes, we will need code to skip over any non-element node, 
	//            //and still stay in sync with FD (using iFD). For now, we assume that every nodeList node is an element.
	//            //https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlnodetype?view=netframework-4.8
	//            //https://docs.microsoft.com/en-us/dotnet/standard/data/xml/types-of-xml-nodes
	//            xmlNode = xmlNodeList[iXmlNode];
	//            while (xmlNode.NodeType.ToString() != "Element")
	//            {
	//                iXmlNode++;
	//                xmlNode = xmlNodeList[iXmlNode];
	//            }
	//            //Create a new attribute node to hold the node's index in xmlNodeList
	//            XmlAttribute a = x.CreateAttribute("index");
	//            a.Value = iXmlNode.ToString();
	//            var e = (XmlElement)xmlNode;
	//            e.SetAttributeNode(a);

	//            //Set the correct Element Name, in case we have errors in the SDC object tree logic
	//            bt.ElementName = e.LocalName;

	//            //Create  dictionary to track the matched indexes of the XML and FD node collections
	//            dX_obj[iXmlNode] = bt.ObjectGUID;
	//            //Debug.Print("iXmlNode: " + iXmlNode + ", ObjectID: " + bt.ObjectID);

	//            //Search for parents:
	//            int parIndexXml = -1;
	//            Guid parObjectGUID = default;
	//            bool parExists = false;
	//            BaseType btPar;
	//            XmlNode parNode;
	//            btPar = null;

	//            parNode = xmlNode.ParentNode;
	//            parExists = int.TryParse(parNode?.Attributes?.GetNamedItem("index")?.Value, out parIndexXml);//The index of the parent XML node
	//            if (parExists)
	//            {
	//                parExists = dX_obj.TryGetValue(parIndexXml, out parObjectGUID);// find the matching parent SDC node Object ID
	//                if (parExists) { parExists = obj.Nodes.TryGetValue(parObjectGUID, out btPar); } //Find the parent node in FD
	//                if (parExists)
	//                {
	//                    //bt.IsLeafNode = true;
	//                    bt.RegisterParent(btPar);
	//                    //Debug.WriteLine($"The node with ObjectID: {bt.ObjectID} is leaving InitializeNodesFromSdcXml. Item type is {bt.GetType().Name}.  " +
	//                    //            $"Parent ObjectID is {bt?.ParentID}, ParentIETypeID: {bt?.ParentIETypeID}, ParentType: {btPar.GetType().Name}");
	//                }
	//                else { throw new KeyNotFoundException("No parent object was returned from the SDC tree"); }
	//            }
	//            else
	//            {
	//                //bt.IsLeafNode = false;
	//                //Debug.WriteLine($"The node with ObjectID: {bt.ObjectID} is leaving InitializeNodesFromSdcXml. Item type is {bt.GetType()}.  " +
	//                //                $", No Parent object exists");
	//            }

	//            iXmlNode++;
	//        }
	//        return obj;

	//    }
	//}

	public static class SdcUtil  //!+Convert many ArrayHelpers and SDC Helpers to extension methods
	{

		/// <summary>
		/// This SortedSet contains the ObjectID of each node that has been sorted by ITreeSibComparer.  
		/// Each entry in this SortedSet ndicates that that nodes child nodes have already been sorted.  
		/// Checking for a parent node in this SortedSet is used to bypass the resorting of child nodes during a tree sorting operation.  
		/// The SortedList is cleared after the conclusion of the sorting operation, using TreeSort_ClearNodeIds().
		/// </summary>
		static readonly SortedSet<int> TreeSort_NodeIds = new();

		/// <summary>
		/// List-sorting code can test for the presence of a flagged parent node in TreeSort_NodeIds with TreeSort_IsSorted. 
		/// If TreeSort_IsSorted returns true,
		/// then the child-list-sorting code should use the ChildNodes dictionary to retrieve the sorted child nodes.  If it returns false, 
		/// the code should use ITreeSibComparer to sort the child nodes (in a List&lt;BaseType>) before using accessing the nodes in sorted order.
		/// </summary>
		/// <param name="ObjectID"></param>
		/// <returns></returns>
		static bool TreeSort_IsSorted(int ObjectID)
		{
			if (TreeSort_NodeIds.Contains(ObjectID)) return true;
			return false;
		}

		/// <summary>
		/// TreeSort_ClearNodeIds is used when starting a node traversal by reflection using the reflection-based ITreeSibComparer.  
		/// Calling this method will cause the node comparison method to use ITreeSibComparer to sort sets of sibling nodes, 
		/// rather than looking in the ChildNodes dictionary for the list of sorted nodes.  
		/// As each set of child nodes is sorted, the parent node is flagged in the TreeSort_NodeIds (a SortedSet type), 
		/// so that IComparer does not need to run again for that parent node's child nodes again (until TreeSort_ClearNodeIds is called again).
		/// Code can test for the presence of a flagged parent node (i.e., with child nodes already-sorted)  with TreeSort_IsSorted.  
		/// If it returns true, then the sorting code should use the ChildNodes dictionary to retrieve the sorted child nodes.  
		/// If it returns false, the sorting code should use IComparer to sort the child nodes List&lt;BaseType> 
		/// before using accessing the child nodes list in sorted order.
		/// </summary>
		public static void TreeSort_ClearNodeIds()
		{
			TreeSort_NodeIds.Clear();
		}

		#region ArrayHelpers 
		/// <summary>
		/// Determines if the object o parameter is a List&lt;T>
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsGenericList(object o)
		{
			if (o == null) return false;
			return o is IList &&
				o.GetType().IsGenericType &&
				o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
		}
		/// <summary>
		/// Given an array of type T, this method finds the first null entry and returns its index in the array.
		/// If it does not find a null entry before reaching the end of the array, 
		/// it increases the size of the array by copying copying the array into a larger array, 
		/// with its larger size determined by the supplied growthIncrement (default = 3).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="growthIncrement"></param>
		/// <returns></returns>
		public static int GetFirstNullArrayIndex<T>(T[] array, int growthIncrement = 3)
		{
			int i = 0;
			array ??= new T[growthIncrement];
			foreach (T n in array)
			{
				if (n is null) return i;
				i++;
			}
			Array.Resize(ref array, array.Length + growthIncrement);
			return i;
		}
		/// <summary>
		/// Given an object o, this methood returns its location as the index position in of the IEnumerable.
		/// </summary>
		/// <param name="ie">The IEnumerable to search</param>
		/// <param name="obj">The object to find in the IEnumerable</param>
		/// <returns></returns>
		public static int GetIndexFromIEnumerableObject(IEnumerable ie, object obj)
		{
			int i = 0;
			foreach (object o in ie)
			{
				if (ReferenceEquals(o, obj)) return i;
				i++;
				if (i == int.MaxValue) return -1;
			}
			return -1;
		}
		/// <summary>
		/// Given an index i in an IEnumerable ie, return the object o at that position
		/// </summary>
		/// <param name="ie">The IEnumerable to search</param>
		/// <param name="index">The index of the object to return</param>
		/// <returns>The object in IEnumerable ie, at index i</returns>
		public static object GetObjectFromIEnumerableIndex(IEnumerable ie, int index)
		{
			int i = -1;
			foreach (object o in ie)
			{
				i++;
				if (index == i) return o;
			}
			return null;
		}
		//public static int GetListIndex<T>(List<T> list, T node) where T : notnull //TODO: could make this an interface feature of all list children
		//{
		//	int i = 0;
		//	foreach (T n in list.)
		//	{
		//		if ((object)n == (object)node) return i;
		//		i++;
		//	}
		//	return -1; //object was not found in list
		//}
		public static int IndexOf(IEnumerable? array, object? item)
		{
			int i = 0;
			if (array is null || item is null) return -1;
			foreach (object n in array)
			{
				if (ReferenceEquals(n, item)) return i;
				i++;
			}
			return -1;
		}
		public static object? ObjectAtIndex(IEnumerable ie, int index)
		{
			//get Dictionary and similar object by index.  
			//This is not a reliable or effficient algorithm, since there is no defined sort order.  
			//Need to use an indexed Dictionary/Collection
			if (index < 0) return null;
			int i = 0;
			foreach (var n in ie)
			{
				if (i == index) return n;
				i++;
			}
			return null;
		}
		public static T IEnumerableCopy<T>(T source, out T copy)
			where T : IEnumerable<T>, new()
		{
			copy = new T();

			foreach (var n in source)
			{
				_ = copy.Append(n);
			}
			return copy;
		}
		public static T[] ArrayAddItemReturnArray<T>(T[] array, T itemToAdd, int growthIncrement = 3)
		{
			int i = GetFirstNullArrayIndex(array, growthIncrement);
			array[i] = itemToAdd;
			return array;

		}
		public static T ArrayAddReturnItem<T>(T[] array, T itemToAdd, int growthIncrement = 3)
		{
			ArrayAddItemReturnArray(array, itemToAdd, growthIncrement);
			return itemToAdd;

		}
		public static T[] RemoveArrayNullsNew<T>(T[] array)
		{
			int i = 0;
			var newarray = new T[array.Length - 1];

			foreach (var n in array)
			{
				if (n != null) newarray[i] = n;
				i++;
			}
			return newarray;
		}

		#endregion

		#region SDC Helpers

		/// <summary>
		/// If refreshTree is true,
		///		this method uses reflection to refresh the Nodes, ParentNodes and ChildNodes dictionaries in ITopNode, with all nodes in the proper .
		///		Some BaseType properties are updated:
		///		SGuid properties are refreshed (but not replaced), and name and order properties are updated (and replaced when needed), 
		/// If refreshTree is false, this method returns an ordered IList&lt;BaseType>, but none of the above refresh actions are performed
		/// </summary>
		/// <param name="topNode">The ITopNode SDC node that will have its tree refreshed</param>
		/// <param name="treeText">An "out" variable containing, if print is true, a text representation of some important properties of each node</param>
		/// <param name="print">If print is true, then treeText will be generated.  If print is false, treeText will be null.
		/// The default is false.</param>
		/// <param name="refreshTree">Determines whether to refresh the Dictionaries and BaseType properties, as described in the summary.
		/// The default is true.</param>
		/// <returns>IList&lt;BaseType> containing all of the SDC tree nodes in sorted top-bottom order</returns>
		public static IList<BaseType> RefreshReflectedTree(ITopNodePublic topNode, out string? treeText, bool print = false, bool refreshTree = true)
		{
			try
			{
				TreeSort_ClearNodeIds();
				int counter = 0;
				int indent = 0;
				int order = 0;
				IdentifiedExtensionType lastIet;
				List<BaseType> SortedNodes = new();
				
				var sbTreeText = new StringBuilder();
				var newPropsText = new StringBuilder();
				BaseType btNode = (BaseType)topNode;
				btNode.order = 0;
				{
					if (topNode is FormDesignType fd)
					{
						fd.ElementName = "FormDesign";
						fd.name = Regex.Replace(fd.ID, @"\W+", "");  //replaces any characters that are not numbers, letters or "_"
					}
					else if (topNode is DemogFormDesignType dfd)
					{
						dfd.ElementName = "DemogFormDesign";
						dfd.name = Regex.Replace(dfd.ID, @"\W+", "");
					}
					else if (topNode is DataElementType de)
					{
						de.ElementName = "DataElement";
						de.name = Regex.Replace(de.ID, @"\W+", "");
					}
					else if (topNode is RetrieveFormPackageType rfp)
					{
						rfp.ElementName = "RetrieveFormPackage";
						if (!rfp.instanceID.IsNullOrWhitespace()) rfp.name = Regex.Replace(rfp.instanceID, @"\W+", "");
						else rfp.name = Regex.Replace(rfp.packageID, @"\W+", "");
					}
				}  //Set TopNode
				if (refreshTree)
				{
					topNode.Nodes.Clear();
					topNode.ParentNodes.Clear();
					topNode.ChildNodes.Clear();
				}
				topNode.Nodes.Add(btNode.ObjectGUID, btNode);
				SortedNodes.Add(btNode);

				if (print) sbTreeText.Append($"({btNode.DotLevel})#{counter}; OID: {btNode.ObjectID}; name: {btNode.name}{content(btNode)}");

				//DoTree(topNode as BaseType);
				DoTree(btNode);
				//-------------------------------------------

				void DoTree(BaseType node)
				{
					indent++;  //indentation level of the node for output formatting
					counter++; //simple integer counter, incremented with each node; should match the ObjectID assigned during XML deserialization
					BaseType? btProp = null;  //holds the current property
					if (print) sbTreeText.Append("\r\n");

					//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
					//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
					Type t = node.GetType();
					var s = new Stack<Type>();
					s.Push(t);

					do
					{//build the stack of inherited types
						t = t.BaseType!;
						if (t.IsSubclassOf(typeof(BaseType))) s.Push(t);
						else break; //quit when we hit a non-BaseType type
					} while (true);

					while (s.Count > 0)
					{
						var props = s.Pop().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
							.Where(p => p.IsDefined(typeof(XmlElementAttribute)))
							//.OrderBy(p => p.GetCustomAttributes<XmlElementAttribute>()  //ordering is not currently needed to retrieve
							//.First().Order)											  //properties in XML Element order, but this could change
							;
						foreach (var p in props)
						{
							var prop = p.GetValue(node);
							if (prop != null)
							{
								if (prop is BaseType)
								{
									btProp = (BaseType)prop;
									if (refreshTree) RefreshTree(parentNode: node, piChildProperty: p);
									if (print) sbTreeText.Append($"{"".PadRight(indent, '.')}({btProp.DotLevel})#{counter}; OID: {btProp.ObjectID}; name: {btProp.name}{content(btProp)}");
									//Debug.Assert(btProp.ObjectID == counter);
									SortedNodes.Add(btProp);
									DoTree(btProp);
								}
								else if (prop is IEnumerable<BaseType> ieProp)
								{
									foreach (BaseType btItem in ieProp)
									{
										btProp = btItem;
										if (refreshTree) RefreshTree(parentNode: node, piChildProperty: p);
										if (print) sbTreeText.Append($"{"".PadRight(indent, '.')}({btProp.DotLevel})#{counter}; OID: {btProp.ObjectID}; name: {btProp.name}{content(btItem)}");
										//Debug.Assert(btItem.ObjectID == counter);
										SortedNodes.Add(btProp);
										DoTree(btProp);
									}
								}
							}
						}
					}
					indent--;
					//!-------------------------------------------------------------------------------------------------------------------------------

					void RefreshTree(BaseType parentNode, PropertyInfo piChildProperty)
					{
						//pi is the PropertyInfo object from the btProp property
						//Neither pi nor btProp reliably contains the XML element name for the btProp node
						//In some cases, it can be obtained by looking at the parentNode,
						//finding the IEnumerable<> Property that contains btProp, and then looking for 
						//the enum value that contains the XML element name.
						//The enum location is found in XmlChoiceIdentifierAttribute on the IEnumerable Property
						//This should be handled in ReflectSdcElement

						//Refill the node dictionaries with the current node
						topNode.Nodes.Add(btProp.ObjectGUID, btProp);
						btProp.RegisterParent(parentNode);

						AssignSdcProperties(parentNode, piChildProperty);

					}

					void AssignSdcProperties(BaseType parentNode, PropertyInfo pi)
					{

						string elementName;
						int elementOrder = -1;
						string suffix = "";
						//Fill some useful properties, while it's efficient to do so, 
						//because we have the PropertyInfo object (pi) and the actual property object (btProp) already available.
						elementName = SdcUtil.ReflectSdcElement(pi, btProp, out _, out elementOrder, out _, out _, out _, out string? errorMsg);
						if (elementName.IsEmpty()) Debugger.Break();
						btProp.ElementName = elementName;
						btProp.ElementOrder = elementOrder;
						btProp.order = order++;

						if (btProp.sGuid is null)
						{
							if (btProp.ObjectGUID == Guid.Empty) btProp.ObjectGUID = new Guid();
							btProp.sGuid = new CSharpVitamins.ShortGuid(btProp.ObjectGUID).Value;
						}
						if (btProp is IdentifiedExtensionType iet)
						{
							if (iet.ID.IsNullOrEmpty()) iet.ID = iet.sGuid;
							lastIet = iet;
							//subIetCounter = 0;
						}
						else
						{
							//subIetCounter++;
							//suffix = "_" + subIetCounter.ToString();
							suffix = "_" + btProp.SubIETcounter.ToString();
							if (suffix is null) Debugger.Break();
						}
						btProp.name = btProp.CreateNameCAP(".100004300", suffix);

						if (print)
						{
							sbTreeText.Append($"ElementName: {btProp.ElementName}; ElementOrder: {btProp.ElementOrder}; order: {btProp.order}; name: {btProp.name}; sGuid = {btProp.sGuid}");
							sbTreeText.Append("\r\n");
						}
					}
				}


				//This is a temporary kludge to generate printable output.  
				//It should be easy to create a tree walker to create any desired output by visiting each node.
				string content(BaseType n)
				{
					string s;
					if (n is DisplayedType) s = "; title: " + (n as DisplayedType).title;
					else if (n is PropertyType) s = "; " + (n as PropertyType).propName + ": " + (n as PropertyType).val;
					else s = $"; type: {n.GetType().Name}";
					return s;
				}
				//We should instead return SortedBodes, and provide the treeText as an out parameter

				if (print) treeText = sbTreeText.ToString();
				else treeText = null;
				return SortedNodes;
			}
			catch { throw; }
			finally
			{
				TreeSort_ClearNodeIds();
			}
		}
		/// <summary>
		/// Lighter weight tree walker that uses only reflection, and no node dictionaries
		/// </summary>
		/// <param name="topNode"></param>
		/// <param name="print"></param>
		/// <param name="treeText"></param>
		/// <returns></returns>
		public static List<BaseType> ReflectTreeList(ITopNodePublic topNode, out string treeText, bool print = false)
		{
			TreeSort_ClearNodeIds();
			List<BaseType> outList = new();
			StringBuilder sbTreeText = new();
			int counter; //used to count each node sequentially
			
			BaseType n = (BaseType)topNode;
			
			

			void MoveNext(BaseType? n)
			{
				if (n is null) return;
				int indent = 0; //used for padding printed output; increments each time will evaluate a set of child nodes
				BaseType?[]? kids = ReflectChildElementList(n)?.ToArray<BaseType?>();

				if (kids is not null)
				{
					foreach (BaseType kid in kids)
					{
						outList.Add(n);
						indent++;  //indentation level of the node for output formatting
						MoveNext(kid);
						indent--;
					}
				}
				else if (n is not null)
				{										
					outList.Add(n);
					if (print) sbTreeText.Append($"({n.DotLevel})#{counter}; OID: {n.ObjectID}; name: {n.name}{content(n)}");					
					counter++; //simple integer counter, incremented with each node; should match the ObjectID assigned during XML deserialization
					ReflectNextSib(n);
					MoveNext(n);				}

			}
			TreeSort_ClearNodeIds();
			treeText = sbTreeText.ToString();
			return outList;

			//This is a temporary kludge to generate printable output.  
			//It should be easy to create a tree walker to create any desired output by visiting each node.
			string content(BaseType n)
			{
				string s;
				if (n is DisplayedType) s = "; title: " + (n as DisplayedType)?.title;
				else if (n is PropertyType) s = "; " + (n as PropertyType)?.propName + ": " + (n as PropertyType)?.val;
				else s = $"; type: {n.GetType().Name}";
				return s;
			}
		}
		public static List<BaseType> GetSortedTreeList(ITopNodePublic tn)
		{
			return GetSortedSubtreeList((BaseType)tn.TopNode);			
		}
		public static List<BaseType> ReflectSortedTreeList(ITopNodePublic tn)
		{
			return ReflectSubtreeList((BaseType)tn.TopNode);
		}

		public static List<BaseType> GetSortedSubtreeList(BaseType n, int startReorder = -1, int orderMultiplier = 1)
		{
			//var nodes = n.TopNode.Nodes;
			var cn = n.TopNode.ChildNodes;
			int i = 0;
			var sortedList = new List<BaseType>();

			MoveNext(n);

			void MoveNext(BaseType n)
			{
				sortedList.Add(n);
				if (startReorder >= 0)
				{
					n.order = i * orderMultiplier;
					i++;
				}

				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						SortKids(n, childList);
						foreach (var child in childList)
							MoveNext(child);
					}
				}
			}
			return sortedList;
		}

		//!Should not need sorting of child nodes
		//Consider this parameter list: ReflectSubtreeList(BaseType bt, int startReorder = -1, int orderMultiplier = 1)
		public static List<BaseType> ReflectSubtreeList(BaseType bt, bool reOrder = false, bool reRegisterNodes = false)
		{
			if (bt is null) return null;
			var i = 0;
			var kids = new List<BaseType>();
			kids.Add(bt); //root node
			if (reOrder) bt.order = i++;

			ReflectSubtree2(bt);
			void ReflectSubtree2(BaseType bt)
			{
				var cList = ReflectChildElementList(bt);
				if (cList is not null)
				{
					foreach (BaseType c in cList)
					{
						kids.Add(c);

						if (reRegisterNodes)
						{
							c.UnRegisterParent();
							c.RegisterParent(bt);
						}
						if (reOrder) c.order = i++;

						ReflectSubtree2(c);
					}
				}
			}
			return kids;

		}

		public static Dictionary<Guid, BaseType> GetSubtreeDictionary(BaseType n, int startReorder = -1, int orderMultiplier = 1)
		{
			//var nodes = n.TopNode.Nodes;
			var cn = n.TopNode.ChildNodes;
			int i = 0;
			var dict = new Dictionary<Guid, BaseType>();
			MoveNext(n);

			void MoveNext(BaseType n)
			{
				dict.Add(n.ObjectGUID, n);
				if (startReorder >= 0)
				{
					n.order = i * orderMultiplier;
					i++;
				}

				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						SortKids(n, childList);
						foreach (var child in childList)
							MoveNext(child); 
					}
				}
			}
			return dict;
		}

		public static List<BaseType> GetSubtreeReOrderNodesList(BaseType n)
		{
			return GetSortedSubtreeList(n, 0, 1);
		}

		public static BaseType? GetNextElement(BaseType item)
		{
			if (item is null) return null;

			var firstKid = GetFirstChild(item);
			if (firstKid != null) return firstKid;

			var n = item;
			do
			{
				var nextSib = GetNextSib(n);
				if (nextSib != null) return nextSib;

				n = n.ParentNode;
			} while (n != null);

			return null;
		}
		public static BaseType? ReflectNextElement2(BaseType item)
		{
			if (item is null) return null;
			BaseType? nextNode;

			//Does item have any child nodes?  If yes, find the first non-null property in the XML element order
			nextNode = ReflectFirstChild(item);
			if (nextNode is not null) return nextNode;

			//No child items contained the next node, so let's look at other properties inside the parent object
			//Is next item part of a parent property that follows our item?				
			nextNode = ReflectNextSib(item);
			if (nextNode is not null) return nextNode;
			if (item.ParentNode is null) return null;
			nextNode = ReflectNextSib(item.ParentNode);
			return null;
		}



		public static BaseType? ReflectNextElement(BaseType item)
		{
			if (item is null) return null;
			BaseType? par = item.ParentNode;
			BaseType? nextNode = null;
			bool doDescendants = true;
			par ??= item; //We have the top node here

			while (par != null)
			{
				//Does item have any children of its own?  If yes, return the first child
				if (doDescendants)
				{
					nextNode = ReflectFirstChild(item);
					if (nextNode != null)
						return nextNode;
				}
				nextNode = ReflectNextSib(item);
				if (nextNode != null)
					return nextNode;

				//We did not find a next item, so let's move up one parent level in this while loop and check the properties under the parent.
				//We keep climbing upwards in the tree until we find a parent with a next item, or we hit the top ancester node and return null.
				item = par;
				par = item.ParentNode;
				doDescendants = false;
			}
			return null;

		}

		/// <summary>
		/// Given a current node (startAfterNode), use reflection to find the next sibling node.
		/// No node dictionaries are used for this method.
		/// </summary>
		/// <param name="startAfterNode">The node for which we want to find the next sibling node</param>
		/// <param name="parentNode">Only required if the ParentNodes dictionary has not been populated or is corrupted or stale</param>
		/// <returns>The ruturned node will be of type BaseType, or null if no next sibling node is found</returns>
		public static BaseType? ReflectNextSib(BaseType startAfterNode, BaseType? parentNode = null)
		{
			parentNode ??= startAfterNode.ParentNode;
			if (parentNode is null) return null; //You can't have sibs without a parent
			BaseType? nextNode = null;
			int lowestOrder = 10000;  //Order in XmlElementAttribute, for finding the next property to return; start with a huge value.
			IEnumerable<PropertyInfo>? piIE = null;

			//the following flag improves efficency by delaying object tree assessment until startAfterNode has been passed
			bool startAfterNodeWasHit = false;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = parentNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);

			do
			{//build the stack of inherited types
				t = t.BaseType!;
				if (t.IsSubclassOf(typeof(BaseType))) s.Push(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);

			//starting with the least-derived inherited type (BaseType), look for any non-null properties of targetNode
			while (s.Count > 0)
			{
				lowestOrder = 10000;
				piIE = s.Pop().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(p =>
					{
						var atts = (XmlElementAttribute[])p.GetCustomAttributes<XmlElementAttribute>();
						if (atts.Length == 0) return false;
						XmlElementAttribute a = atts[0];
						object? o = null;
						if (a.Order < lowestOrder)
						{
							if (startAfterNodeWasHit)
							{
								o = p.GetValue(parentNode);

								if (o is null) return false;
								if (o is BaseType bt)
								{
									lowestOrder = a.Order;
									nextNode = bt;
									return true;
								}
								if (o is IEnumerable<BaseType> ie && ie.Any())
								{
									lowestOrder = a.Order;
									nextNode = (BaseType)((IList)o)[0]!;
									return true;
								}
							}
							else //if (!startAfterNodeWasHit)
							{
								o = p.GetValue(parentNode);

								if (o is IEnumerable<BaseType> ie && ie.Any())								
								{
									int i = IndexOf(ie, startAfterNode!);
									if (i > -1)
									{
										startAfterNodeWasHit = true; //start looking for nextNode now
										if (i < ie.Count() - 1)
										{
											lowestOrder = a.Order;
											nextNode = (BaseType)GetObjectFromIEnumerableIndex(ie, i + 1);
											return true;
										}
									}
								}
								else if (ReferenceEquals(o, startAfterNode))
									startAfterNodeWasHit = true; //start looking for nextNode now
							}
						}
						return false;
					}).ToList();

				//var piIeOrdered = piIE?.OrderBy(p => p.GetCustomAttributes<XmlElementAttribute>().FirstOrDefault()?.Order); //sort pi list by XmlElementAttribute.Order
				//PropertyInfo piFirst = GetObjectFromIEnumerableIndex(piIeOrdered, 0) as PropertyInfo; //Get the property whose XmlElementAttribute has the smallest order

				if (nextNode != null)
					return nextNode;
			}
			return null;
		}
		public static BaseType? GetFirstSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;
			item.TopNode.ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is not null) SortKids(item, sibs);
			return sibs?[0];
		}
		public static BaseType? ReflectFirstSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElementList(par);
			return lst.FirstOrDefault();
		}
		public static BaseType? GetNextSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;
			var sibs = item.TopNode.ChildNodes[par.ObjectGUID];
			if (sibs is null) return null;
			SortKids(item, sibs);
			var index = sibs.IndexOf(item);
			if (index == sibs.Count - 1) return null; //item is the last item
			return sibs[index + 1];
		}
		public static BaseType? ReflectNextSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElementList(par);
			var myIndex = lst?.IndexOf(item) ?? -1;
			if (myIndex < 0 || myIndex == lst?.Count - 1) return null;
			return lst[myIndex + 1]??null;
		}
		public static BaseType? GetLastSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;
			item.TopNode.ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is not null) SortKids(item, sibs);
			return sibs?.Last();
		}
		public static BaseType? ReflectLastSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElementList(par);
			return lst?.Last();
		}
		public static BaseType? GetPrevElement(BaseType? item)
		{
			if (item is null) return null;
			BaseType? par = item.ParentNode;
			BaseType? lastDesc;

			var prevSib = GetPrevSib(item);
			if (prevSib is not null)
			{
				lastDesc = GetLastDescendant(prevSib);
				if (lastDesc is not null) return lastDesc;
				return prevSib;
			}

			if (par is null) return null; //item is the top node

			return par;       
		}
		public static BaseType? ReflectPrevElement(BaseType? item)
		{
			if (item is null) return null;
			BaseType? par = item.ParentNode;
			BaseType? lastDesc;

			var prevSib = ReflectPrevSib(item);
			if (prevSib is not null)
			{
				lastDesc = ReflectLastDescendant(prevSib);
				if (lastDesc is not null) return lastDesc;
				return prevSib;
			}

			if (par is null) return null; //item is the top node

			lastDesc = ReflectLastDescendant(par);
			if (lastDesc is not null) return lastDesc;

			return par;    
		}
		public static BaseType? GetPrevSib(BaseType item)
		{
			var par = item?.ParentNode;
			if (par is null) return null;
			item!.TopNode.ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is null) return null;
			SortKids(item, sibs);

			var index = sibs?.IndexOf(item)??-1;
			if (index == 0) return null; //item is the first item
			return sibs?[index - 1];
		}
		public static BaseType? ReflectPrevSib(BaseType item)
		{
			var par = item.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElementList(par);
			if (lst is null) return null;
			var myIndex = lst?.IndexOf(item) ?? -1;
			if (myIndex < 1) return null;
			return lst?[myIndex - 1];
		}

		public static BaseType? GetLastChild(BaseType item)
		{
			item.TopNode.ChildNodes.TryGetValue(item.ObjectGUID, out List<BaseType>? kids);
			if (kids is not null) SortKids(item, kids);
			return kids?.Last();
		}
		/// <summary>
		/// Given a parent node, find the last child node, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// Minimizes the use of reflection with a backwards search of PropertyInfo objects
		/// Finding the last child node helps with rapidly walking down to the deepest descendant of a tree branch, using onluy reflection
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns>List&lt;BaseType>? containing the child nodes</returns>
		public static BaseType? ReflectLastChild(BaseType parentNode)
		{
			if (parentNode is null) return null; //You can't have sibs without a parent
			IEnumerable<PropertyInfo>? piIE = null;

			//Create a FIFO Queue of the targetNode inheritance hierarchy.  The queue's top level type (the first type enqueued) will always be the most-derived BaseType subtype

			Type t = parentNode.GetType();
			var s = new Queue<Type>();
			s.Enqueue(t);
			do
			{//continue to build the FIFO Queue of inherited types from parentNode
				t = t.BaseType!;
				if (t.IsSubclassOf(typeof(BaseType))) s.Enqueue(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);

			//starting with the most-derived inherited type (BaseType subtypes), look for any non-null properties of parentNode
			while (s.Count > 0)
			{
				piIE = s.Dequeue()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Any());
				Queue<PropertyInfo> q = new();
				foreach (var pi in piIE) q.Enqueue(pi);  //We need to reverse the order of piIE without copying, so we can iterate backwards
				foreach(var pi in q)  //moves backwards thorugh the queue, since the last property with a value holds our desired object
				{
					object? o = pi.GetValue(parentNode);
					if (o is not null)
					{
						if (o is BaseType bt)	return bt;
						if (o is IEnumerable<BaseType> ie && ie.Any())	return ie.Last();
					}
				}
			}
			return null;
		}
		public static BaseType? GetFirstChild(BaseType item)
		{
			item.TopNode.ChildNodes.TryGetValue(item.ObjectGUID, out List<BaseType>? kids);

			if (kids is not null) SortKids(item, kids);
			return kids?[0];
		}
		/// <summary>
		/// Given a parent node, retrieve the list of child nodes, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns>List&lt;BaseType>? containing the child nodes</returns>
		public static BaseType? ReflectFirstChild(BaseType parentNode)
		{
			if (parentNode is null) return null; //You can't have sibs without a parent
			IEnumerable<PropertyInfo>? piIE = null;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time

			Type t = parentNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);

			do
			{//build the stack of inherited types from parentNode
				t = t.BaseType!;
				if (t.IsSubclassOf(typeof(BaseType))) s.Push(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);

			//starting with the least-derived inherited type (BaseType), look for any non-null properties of targetNode
			while (s.Count > 0)
			{
				piIE = s.Pop()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Any());
				foreach (var p in piIE)
				{
					object? o = p.GetValue(parentNode);
					if (o is not null)
					{
						if (o is BaseType bt) return bt;
						if (o is IEnumerable<BaseType> ie && ie.Any())
							return ie.First();
					}
				}
			}
			return null;
		}
		public static List<BaseType>? GetChildList(BaseType item)
		{
			item.TopNode.ChildNodes.TryGetValue(item.ObjectGUID, out List<BaseType>? kids);
			if(kids is not null) SortKids(item, kids);
			return kids;
		}

		/// <summary>
		/// Given a parent node, retrieve the list of correctly-ordered child Xml Element nodes, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns>List&lt;BaseType>? containing the child nodes</returns>
		public static List<BaseType>? ReflectChildElementList(BaseType parentNode)
		{
			if (parentNode is null) return null; //You can't have sibs without a parent
			List<BaseType>? childNodes = new ();
			IEnumerable<PropertyInfo>? piIE = null;
			int nodeIndex = -1;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = parentNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);

			do
			{//build the stack of inherited types from parentNode
				t = t.BaseType!;
				if (t.IsSubclassOf(typeof(BaseType))) s.Push(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);

			//starting with the least-derived inherited type (BaseType), look for any non-null properties of targetNode
			while (s.Count > 0)
			{
				piIE = s.Pop()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Any());
				foreach (var p in piIE)
				{
					nodeIndex++;
					object? o = p.GetValue(parentNode);
					if (o is not null)
					{
						if (o is BaseType bt)
							childNodes.Add(bt);
						if (o is IEnumerable<BaseType> ie && ie.Any())
							childNodes.AddRange((IEnumerable<BaseType>)o);
					}
				}
			}
			return childNodes;
		}


		/// <summary>
		/// Given a parent node, retrieve the list of correctly-ordered child Xml Attribute nodes, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="elementNode"></param>
		/// <param name="getAllAttributes">If true (the default), returns all attributes.    
		/// If false, returns only those attributes which have values that will be serialized</param>
		/// <returns>List&lt;BaseType>? containing the child nodes</returns>
		public static List<AttributeInfo> ReflectXmlAttributes(BaseType elementNode, bool getAllAttributes = true)
		{
			if (elementNode is null) throw new NullReferenceException("elementNode cannot be null"); //You can't have sibs without a parent
			//List<PropertyInfo> attributesX = new();
			List<AttributeInfo> attributes = new();
			IEnumerable<PropertyInfo>? piIE = null;
			int nodeIndex = -1;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = elementNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);

			do
			{//build the stack of inherited types from parentNode
				t = t.BaseType!;
				if (t.IsAssignableTo(typeof(BaseType))) s.Push(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);

			//starting with the least-derived inherited type (BaseType), look for any non-null properties of targetNode
			while (s.Count > 0)
			{
				t = s.Pop();
				piIE = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
						.Where(p => p.GetCustomAttributes<XmlAttributeAttribute>().Any());
				foreach (var p in piIE)
				{
					nodeIndex++;
					if (getAllAttributes)
					{
						nodeIndex++;
						attributes.Add(FillAttributeInfo(p, elementNode));
					}
					else
					{
						var att = t.GetMethod("ShouldSerialize" + p.Name)?.Invoke(elementNode, null);
						if (att is bool shouldSerialize && shouldSerialize)
						{
							nodeIndex++;
							attributes.Add(FillAttributeInfo(p, elementNode));
						}
					}
				}
			}
			return attributes;

			AttributeInfo FillAttributeInfo(PropertyInfo p, BaseType elementNode) =>
				new (elementNode, elementNode.sGuid, p!.GetValue(elementNode), p, nodeIndex);

		}



		public static bool HasChild(BaseType item)
		{
			item.TopNode.ChildNodes.TryGetValue(item.ObjectGUID, out List<BaseType>? kids);
			if (kids is null || kids.Any()) return false;
			return true;
		}


		public static BaseType? GetLastDescendant(BaseType item, BaseType? stopNode = null)
		{
			BaseType? last = null;
			BaseType? n = item;
			while (n is not null)
			{
				item.TopNode.ChildNodes.TryGetValue(n.ObjectGUID, out List<BaseType>? kids);
				if (kids is not null && kids.Count > 0)
				{

					SortKids(n, kids);

					//option to abort search just before stopNode: check for stopNode in sibling list.
					if (stopNode is not null)
					{
						var snIndex = kids?.IndexOf(stopNode) ?? -1;
						if (snIndex > 0) return kids?[snIndex - 1];
						//if (snIndex == 0) return last; //removed rm 2022_08_09; if stopNode was not found, we should not return "last,"
						//but instead continue to check the child nodes
					}

					n = kids?.Last(); //go to the last kid node
					if (n is not null) last = n;
					else return last;  //this should never be executed if n is not null
				}
				else return last;
			}
			return last;
		}

		public static BaseType? GetLastDescendantSimple(BaseType n)
		{
			//var nodes = n.TopNode.Nodes;
			var cn = n.TopNode.ChildNodes;

			BaseType lastNode = null;
			//bool doSibs = false;

			MoveNext(n);

			void MoveNext(BaseType n)
			{
				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						SortKids(n, childList);
						lastNode = childList.Last();
						MoveNext(lastNode);
					}
				}
			}
			return lastNode;
		}


		public static BaseType? ReflectLastDescendant(BaseType bt, BaseType? stopNode)
		{
			if (bt is null) return null;
			BaseType? lastKid = null;

			FindLastKid(bt);
			//!+-------Local Method--------------------------
			void FindLastKid(BaseType bt)
			{
				List<BaseType>? kids = ReflectChildElementList(bt);
				var testLast = kids?.Last();
				if (testLast is null) return; //we ran out of kids to check, so lastKid is the last descendant                

				if (stopNode != null)
				{
					var pos = kids?.IndexOf(stopNode) ?? 0;
					if (pos == 0) return;
					if (pos > 0)
					{
						lastKid = kids?[pos - 1];
						return;
					}
				}
				lastKid = testLast;
				FindLastKid(lastKid);
			}
			return lastKid;
		}
		public static BaseType? ReflectLastDescendant(BaseType bt)
		{
			if (bt is null) return null;
			BaseType? lastKid = null;

			FindLastKid(bt);

			void FindLastKid(BaseType bt)
			{
				var testLast = ReflectLastChild(bt);
				if (testLast is null) return; //we ran out of kids to check, so lastKid is the last descendant                

				lastKid = testLast;
				FindLastKid(lastKid);
			}
			return lastKid;
		}

		internal static int GetMaxOrderFromXmlElementAttributes(BaseType item)
		{
			var props = item.GetType().GetProperties();

			//Get the max Order among all the XmlElementAttributes in props.  This will be an upper bound for later searching
			int maxOrder = -1;
			int? tempMax = -1;
			foreach (var pi in props)
			{
				if (pi.GetCustomAttributes<XmlElementAttribute>().Any())
				{
					tempMax = pi.GetCustomAttributes<XmlElementAttribute>()?.Where(a => a.Order > -1)?.First()?.Order;
					if (tempMax > maxOrder) maxOrder = tempMax ?? -1;
				}
			}
			return maxOrder;

		}

		public static PropertyInfoMetadata GetPropertyInfoMeta(BaseType item, bool getNames = true)
		{
			PropertyInfo? pi = GetPropertyInfo(
				item,
				out string? propName,
				out int itemIndex,
				out IEnumerable<BaseType>? ieItems,
				out int xmlOrder,
				out int maxXmlOrder,
				out string? xmlElementName,
				getNames);

			return new PropertyInfoMetadata(pi, propName, itemIndex, ieItems, xmlOrder, maxXmlOrder, xmlElementName);

		}
		/// <summary>
		/// Get the PropertyInfo object that represents the "item" property in the item's ParentNode
		/// This PropertyInfo object may be decorated with important XML annnotations such as XmlElementAttribute
		/// The returned PropertyInfo object may refer to a BaseType or the IEnumerables List&lt;BaseType> and Array&lt;BaseType> 
		/// If a wrapper property was created in an SDC parrtial class, only the inner property (i.e., the one with XML attributes) is returned
		/// </summary>
		/// <param name="item"></param>
		/// <param name="propName"></param>
		/// <param name="itemIndex"></param>
		/// <param name="ieItems"></param>
		/// <param name="xmlOrder"></param>
		/// <param name="maxXmlOrder"></param>
		/// <param name="xmlElementName"></param>
		/// <param name="getNames">if true, element names will be determined</param>
		/// <returns>
		/// propName: name of the property is returned as an out parameter
		/// ieItems: if the property is IEnumerable&lt;BaseType>, the IEnumerable property object is returned as an out parameter, otherwise it is null
		/// itemIndex: the index of "item" in "ieItems" is returned as an out parameter, otherwise it is -1
		/// </returns>
		private static PropertyInfo? GetPropertyInfo(
			BaseType item,
			out string? propName,
			out int itemIndex,
			out IEnumerable<BaseType>? ieItems,
			out int xmlOrder,
			out int maxXmlOrder,
			out string? xmlElementName,
			bool getNames = true)
		{
			xmlOrder = -2;
			maxXmlOrder = -1;
			propName = null;
			xmlElementName = null;
			itemIndex = -1;
			ieItems = null;
			if (item is null) return null;
			BaseType? par = item.ParentNode;
			if (par is null)
			{
				par = item;  //we are at the top node
				var t = item.GetType();
				xmlElementName = t.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
				xmlOrder = -1; // -1 is a special case indicating root node
				return null;
			}

			maxXmlOrder = GetMaxOrderFromXmlElementAttributes(par);

			xmlElementName = ReflectSdcElement(item, out ieItems, out xmlOrder, out maxXmlOrder, out itemIndex, out PropertyInfo? piItemOut, out _);
			propName = piItemOut.Name;
			return piItemOut;



		}
		private static int GetItemIndex(BaseType item, out IEnumerable<BaseType>? ieItems, out PropertyInfo? piItemOut, out string errorMsg)
		=> GetItemIndex(item, null, out ieItems, out piItemOut, out errorMsg);

		private static int GetItemIndex(BaseType item, IEnumerable<PropertyInfo>? ieParProps, out IEnumerable<BaseType>? ieItems, out PropertyInfo? piItemOut, out string errorMsg)
		{
			errorMsg = "";
			ieItems = null;
			piItemOut = null;
			BaseType? par = item.ParentNode;

			if (par is null)
			{ errorMsg = "the ParentNode of item cannot be null"; return -1; }
			if (ieParProps is null)
			{
				ieParProps = par.GetType().GetProperties()
						.Where(p => typeof(IEnumerable<BaseType>)
						.IsAssignableFrom(p.PropertyType)
						&& p.GetCustomAttributes(typeof(XmlElementAttribute)).Any()  //We must confirm that our IEnumerable has a XmlElementAttribute,
																					 //since we added some shadow properties in the partial classes
																					 //like "ChildItems_List" for "Items"
						//&& p.GetValue(par) is not null			//This will be good for a future refactoring of the lambda expression; it will get the matched property directly and concisely.
						);

				if (ieParProps is null || ! ieParProps.Any())
				{ errorMsg = "the ParentNode of item does not contain an IEnumerable<BaseType> that contains the the target item node"; return -1; }
			}
			foreach (var propInfo in ieParProps!) //loop through IEnumerable PropertyInfo objects in par
			{   //Reflect each propInfo to see if our item parameter lives in it
				ieItems = (IEnumerable<BaseType>?)propInfo.GetValue(par);
				if (ieItems is not null && ieItems.Any())
				{
					piItemOut = propInfo;
					return IndexOf(ieItems, item); //search for item
				}
			}
			return -1;
		}

		public static string ReflectSdcElement(BaseType item, out IEnumerable<BaseType>? ieItems, out int xmlOrder, out int maxXmlOrder, out int itemIndex, out PropertyInfo piItem, out string? errorMsg)
		{
			string? elementName = ReflectSdcElement(null, item, out ieItems, out xmlOrder, out maxXmlOrder, out itemIndex, out piItem, out errorMsg);
			return elementName;


		}

		/// <summary>
		/// If piItem is passed as null, the method will retrieve it
		/// </summary>
		/// <param name="piItem"></param>
		/// <param name="item"></param>
		/// <param name="ieItems"></param>
		/// <param name="xmlOrder"></param>
		/// <param name="maxXmlOrder"></param>
		/// <param name="itemIndex"></param>
		/// <param name="piItemOut"></param>
		/// <param name="errorMsg"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		public static string ReflectSdcElement(PropertyInfo? piItem, BaseType item, out IEnumerable<BaseType>? ieItems, out int xmlOrder, out int maxXmlOrder, out int itemIndex, out PropertyInfo? piItemOut, out string? errorMsg)
		{
			string? xmlElementName;
			ieItems = null;
			xmlOrder = -1;
			maxXmlOrder = -1;
			itemIndex = -1;
			errorMsg = null;
			piItemOut = null;
			Type itemType = item.GetType();
			Type? parType = null;

			BaseType? par = item.ParentNode;
			if (par is null)
			{
				//we are at the top node
				xmlElementName = itemType.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
				if (xmlElementName is null) 
					throw new InvalidOperationException("Could not find a name for the item parameter.  This may occur if the item has no parent object");
				return xmlElementName;
			}

			parType = par.GetType();
			PropertyInfo[] parProps = parType.GetProperties();

			if (piItem is null)
			{
				//Look for a direct item-to-property match, so we can assign propName from the par object		
				piItemOut = parProps
					.Where(pi => pi.GetCustomAttributes(typeof(XmlElementAttribute)).Any()  //all serialized properties must have the XmlElementAttribute attribute
					&& ! typeof(IEnumerable<BaseType>).IsAssignableFrom(pi.PropertyType)     //the property is not an IEnumerable (i.e., an Array, List etc.)
					&& ReferenceEquals(pi?.GetValue(par), item))?.FirstOrDefault();          //There can be, at most, one match to our item object
				
				if (piItemOut is not null) piItem = piItemOut;
			}
			else piItemOut = piItem;
			piItem ??= piItemOut;

			//Now we look in IEnumerable properties, to see if "item" is contained inside it.
			//Let's see if our item object lives in an IEnumerable<BaseClassSubtype> 
			itemIndex = GetItemIndex(item, out ieItems, out PropertyInfo? piOut, out errorMsg);
			piItemOut ??= piOut;  //Since piOut will be null if item is not an IEnumerable, we only want to use it if piItemOut is null.
			if (piItemOut is null) 
				throw new NullReferenceException("Could not obtain PropertyInfo object from the item parameter");
			piItem ??= piItemOut;

			//Find XmlElementAttribute-tagged properties in the pi that matches our par object
			//XmlElementAttribute[]? xeAtts = (XmlElementAttribute[])Attribute.GetCustomAttributes(piItem, typeof(XmlElementAttribute));
			XmlElementAttribute[]? xeAtts = (XmlElementAttribute[])piItem.GetCustomAttributes<XmlElementAttribute>(true);
			if (xeAtts?.Length > 0) xmlOrder = xeAtts.ToArray()[0].Order;


			//Look for "Item" or "Items" properties with an attribute similar to this: [XmlChoiceIdentifierAttribute("ItemsElementName")]
			//This indicates that the element name must be retrieved from an enum,
			//which is a special property in the same class (named, e.g., "ItemsElementName"),
			//and with a type of an enum subclass, or an Ienumerable<enumSubclass>
			//This is handled in the next method call:
			xmlElementName = GetElementNameFromEnum(piItem, item, itemIndex, out errorMsg);
			if (xmlElementName?.Length > 0) return xmlElementName;

			//If there is only one XmlElementAttribute, try to get elementName directly from the attribute.
			if (xeAtts?.Length == 1)
			{
				xmlElementName = xeAtts.ToArray()[0].ElementName;
				if (xmlElementName?.Length > 0)
					return xmlElementName;
			}

			//Return ElementName based on data type match in the XMLAttribute, but only if there is one (and only one) match to the data type
			var dtAtts = xeAtts?.Where(a => a.Type == item.GetType()).ToArray();
			if (dtAtts?.Length == 1)
			{
				xmlElementName = dtAtts.ToArray()[0].ElementName;
				if (xmlElementName.Length > 0)
					return xmlElementName;
			}

			//Perhaps the item is inside an IEnumerable<BaseTypeSubClass>, and does not use an ItemChoiceType enum or IEnumerable<EnumSubclass>
			if (xeAtts?.Length > 1 && itemIndex > -1)
			{
				//int index = GetItemIndex(piItem, item, out errorMsg);
				xmlElementName = xeAtts.ToArray()[itemIndex].ElementName;
				if (xmlElementName?.Length > 0)
					return xmlElementName;
			}

			//There was no ElementName to extract from an XmlElementAttribute or enum, so we get it directly from the propName.
			if (piItem.Name == "Item") Debugger.Break();
			return piItem.Name;

			throw new InvalidOperationException("Could not find a name for the item parameter.");

		}


		private static string? GetElementNameFromEnum(PropertyInfo piItem, BaseType item, int itemIndex, out string? errorMsg)
		{
			itemIndex = -1;
			errorMsg = null;
			//itemIndex = GetItemIndex(piItem, item, out errorMsg, out ieItems);
			object? choiceIdentifierObject = GetItemChoiceEnumProperty(piItem, item);
			if (choiceIdentifierObject is null)
				return null; //An enum is not used to determine the XML Element name			

			//If itemIndex == -1, then item is not contained in an IEnumerable List or Array, so
			//it should be in an enum subclass:
			if (choiceIdentifierObject is System.Enum e)
				return e.ToString();

			if (choiceIdentifierObject is IEnumerable ie && itemIndex > -1)
			{
				return ((System.Enum)GetObjectFromIEnumerableIndex(ie, itemIndex)).ToString();
			}
			return null;
		}

		private static object? GetItemChoiceEnumProperty(PropertyInfo piItem, BaseType item)
		{//old name: ItemChoiceEnum
			string? enumName = GetElementNameFromItemChoiceType(piItem);
			if (enumName == null) return null!;

			var enumObj = item.ParentNode.GetType()?.GetProperty(enumName)?.GetValue(item.ParentNode);
			if (enumObj is Enum e) return e;
			if (enumObj is IEnumerable ie) return ie;
			return null;
		}

		private static string? GetElementNameFromItemChoiceType(PropertyInfo piItem)
		{//old name: ItemChoiceEnumName
			XmlChoiceIdentifierAttribute? xci = (XmlChoiceIdentifierAttribute?)piItem.GetCustomAttribute(typeof(XmlChoiceIdentifierAttribute));
			if (xci is null) return null;
			return xci.MemberName;
		}

		/// <summary>
		/// Reflect the object tree to determine if <paramref name="item"/> can be attached to <paramref name="newParent"/>.   
		/// We must find an <em>exact</em> match for <paramref name="item"/>'s element name and the data type in <paramref name="newParent"/> to allow the move.
		/// </summary>
		/// <param name="item">The SDC node to test for its ability to be attached to the <paramref name="newParent"/> node.</param>
		/// <param name="newParent">The node to which the <paramref name="item"/> node should be moved.</param>
		/// <param name="pObj">The property object on <paramref name="newParent"/> that would attach to <paramref name="item"/> (hold its object reference).
		/// pObj may be a List&lt;> or a non-List object.</param>
		/// <returns>True for allowed parent nodes, false for disallowed not allowed</returns>
		internal static bool IsParentNodeAllowed(BaseType item, BaseType newParent, out object? pObj)
		{
			pObj = null;  //the property object to which item would be attached; it may be a List<> or a non-List object.

			if (newParent is null) return false;
			//make sure that item and target are not null and are part of the same tree
			if (item.TopNode.Nodes[newParent.ObjectGUID] is null) return false;

			Type itemType = item.GetType();
			var thisPi = SdcUtil.GetPropertyInfoMeta(item);
			string? itemName = thisPi.XmlElementName;

			foreach (var p in newParent.GetType().GetProperties())
			{
				var pAtts = p.GetCustomAttributes<XmlElementAttribute>();
					pObj = p.GetValue(newParent);  //object that item can be attached to; it may be a List or Array to attach "item" as an element, or another BaseType subclass object to which item can be attached"
					if (pObj is not null)
					{
						foreach (var a in pAtts)
						{
							if (a.ElementName == itemName)
							{
								if (a.Type == itemType)
									return true; //if type matches, then ElementName will match, unless XmlChoiceIdentifierAttribute exists on the property.  This is the most common case.

								if (a.Type is null && p.PropertyType == itemType) return true;

								if (p.PropertyType.IsGenericType &&
									p.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) &&
									(p.PropertyType.GetGenericArguments()[0] == itemType //this may not work unless it's an exact type match
										|| p.PropertyType.GetGenericArguments()[0].IsAssignableFrom(itemType))
									) //e.g., like: List<ExtensionBaseType> Items, with [XmlElement("SelectionTest", typeof(PredSelectionTestType), Order=0)]
								return true;

							if (p.PropertyType.IsArray &&
								(p.PropertyType.GetElementType() == itemType //this may not work unless it's an exact type match
									|| p.PropertyType.GetElementType()!.IsAssignableFrom(itemType))
								)//e.g., like: ExtensionBaseType[] Items, with [XmlElement("ValidateForm", typeof(ActValidateFormType), Order=0)]
								return true;
						}
					}
				}
				//Also try matching element names found in the ItemChoiceType enums.  This is more reliable that using data types.

				//if none of the XmlElementAttributes had a matching Type an ElementName, perhaps the property Type will match directly
				if (p.Name == itemName)
				{
					if (p.PropertyType == itemType)
						return true;

					if (p.PropertyType.IsGenericType &&
						(p.PropertyType.GetGenericArguments()[0] == itemType //this may not work unless it's an exact type match
							|| p.PropertyType.GetGenericArguments()[0].IsAssignableFrom(itemType))
						) //e.g., like: List<ExtensionBaseType> Items, with [XmlElement("SelectionTest", typeof(PredSelectionTestType), Order=0)]
						return true;

					if (p.PropertyType.IsArray &&
						(p.PropertyType.GetElementType() == itemType //this may not work unless it's an exact type match
							|| p.PropertyType.GetElementType()!.IsAssignableFrom(itemType))
						)//e.g., like: ExtensionBaseType[] Items, with [XmlElement("ValidateForm", typeof(ActValidateFormType), Order=0)]
						return true;
				}
				
			}
			pObj = null;
			return false;
		}

		/// <summary>
		/// Reflect the object tree to determine if <paramref name="item"/> can be attached to new parent node,
		/// which is defined by the <paramref name="piNewParentProperty"/> PropertyInfo type.   
		/// We must find an <em>exact</em> match for <paramref name="item"/>'s element name and the data type in <paramref name="piNewParentProperty"/> to allow the move.
		/// </summary>
		/// <param name="item">The SDC node to test for its ability to be attached to the <paramref name="piNewParentProperty"/> node.</param>
		/// <param name="piNewParentProperty">The PropertyInfo object that defines the parent node to which 
		/// the <paramref name="item"/> node should be moved.</param>
		/// <param name="itemName">The XML Element name for item name.  If null (the default), the method will attempt to determine it.</param>
		/// <returns>True for allowed parent nodes, false for disallowed parent nodes, 
		/// where the parent node is defined by <paramref name="piNewParentProperty"/>.</returns>
		internal static bool IsParentNodeAllowed(BaseType item, PropertyInfo piNewParentProperty, string? itemName = null)
		{

			if (item is null) throw new ArgumentNullException(nameof(item), "Argument cannot be null.");
			if (piNewParentProperty is null) throw new ArgumentNullException(nameof(piNewParentProperty), "Argument cannot be null.");

			Type itemType = item.GetType();
			if (itemName is null || itemName.IsEmpty()) itemName = item.GetPropertyInfoMetaData().XmlElementName;

			var pAtts = piNewParentProperty.GetCustomAttributes<XmlElementAttribute>();

			if (pAtts.Any())
			{
				foreach (var a in pAtts)
				{
					if (a.ElementName == itemName)
					{
						if (a.Type == itemType)
							return true; //if type matches, then ElementName must also match.  This is the most common case.

						if (a.Type is null &&
							piNewParentProperty.PropertyType == itemType
							)
							return true;

						if (piNewParentProperty.PropertyType.IsGenericType &&
							piNewParentProperty.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) &&
							piNewParentProperty.PropertyType.GetGenericArguments()[0] == itemType
							)
							return true;

						if (piNewParentProperty.PropertyType.IsArray &&
							piNewParentProperty.PropertyType.GetElementType() == itemType
							)
							return true;
					}
				}

				//Also, we could try matching element names found in the ItemChoiceType enums.  This is more reliable that using data types.

				//if none of the XmlElementAttributes had a matching Type an ElementName, perhaps the property Type will match directly
				//TODO: However, it's not clear we need an "expensive" type match if the item name matches the property name.
				if (piNewParentProperty.Name == itemName)
				{
					if (piNewParentProperty.PropertyType == itemType)
						return true;

					if (piNewParentProperty.PropertyType.IsGenericType &&
						piNewParentProperty.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) &&
						piNewParentProperty.PropertyType.GetGenericArguments()[0] == itemType
						)
						return true;

					if (piNewParentProperty.PropertyType.IsArray &&
						piNewParentProperty.PropertyType.GetElementType() == itemType
						)
						return true;
				}
			}
			return false;
		}
		#endregion
		#region Retired
		public static List<BaseType>? X_ReflectChildList(BaseType bt)
		{
			if (bt is null) return null;
			var kids = new List<BaseType>();
			foreach (var p in bt.GetType().GetProperties()
				.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Any()))
			{				
				var kid = p.GetValue(bt);
				if (kid != null)
				{
					if (kid is BaseType btKid) kids.Add(btKid);
					else if (kid is IList kidList)
					{
						var pList = kidList.OfType<BaseType>().ToList();  //.Cast<BaseType>();
						kids.AddRange(pList);
					}
				}
			}
			SortKids(bt, kids);
			return kids;
		}



		public static BaseType? X_ReflectFirstChild(BaseType item)
		{
			if (item is null) return null;

			var lst = ReflectChildElementList(item);
			return lst?.FirstOrDefault();
		}
		public static BaseType? X_ReflectLastChild(BaseType item)
		{
			if (item is null) return null;

			var lst = ReflectChildElementList(item);
			return lst?.Last();
		}
		private static List<T> X_GetStatedEventParent<T>(T item)
			where T : EventType
		{
			var elementName = item.ElementName;
			var pn = item.ParentNode;
			if (pn is null) return null;
			List<T> list;


			if (item is FormDesignType)
			{
				switch (elementName)
				{
					case "OnEvent":
						list = (pn as FormDesignType).OnEvent as List<T>;
						return list;
				}
				if (item is ListItemBaseType)
					switch (elementName)
					{
						case "OnSelect":
							list = (pn as ListItemBaseType).OnSelect as List<T>;
							if (list != null) return list;
							break;
						case "OnDeselect":
							list = (pn as ListItemBaseType).OnDeselect as List<T>;
							if (list != null) return list;
							break;
					}
				if (item is DisplayedType)
				{
					switch (elementName)
					{
						case "OnEvent":
							list = (pn as DisplayedType).OnEvent as List<T>;
							if (list != null) return list;
							break;
						case "OnEnter":
							list = (pn as DisplayedType).OnEnter as List<T>;
							if (list != null) return list;
							break;
						case "OnExit":
							list = (pn as DisplayedType).OnExit as List<T>;
							if (list != null) return list;
							break;
						default:
							throw new ArgumentException("Unknown ElementName:" + elementName ?? "\"\"");
					}

					if (item is ResponseFieldType)
						switch (elementName)
						{
							case "OnEvent":
								list = (pn as ResponseFieldType).OnEvent as List<T>;
								if (list != null) return list;
								break;
							case "AfterChange":
								list = (pn as ResponseFieldType).AfterChange as List<T>;
								if (list != null) return list;
								break;
							default:
								throw new ArgumentException("Unknown ElementName:" + elementName ?? "\"\"");
						}
				}
			}
			return null;
		}
		private static List<BaseType> X_GetStatedListParent(BaseType item, string elementName)
		{   //get the list object that points to the item node
			//Only works for SDC List<BaseType> derivitives.   Does not work e.g., for XML types, derived from XmlElement.
			//Work out how to return a list of the exact type <T>.

			//TODO: trap errors here: loook for null parent...
			var pn = item.ParentNode;
			List<BaseType> list;

			switch (item.GetType().Name)
			{
				case "Extension":
					list = (pn as ExtensionBaseType).Extension.Cast<BaseType>().ToList();
					return list;
				case "Comment":
					list = (pn as ExtensionBaseType).Comment.Cast<BaseType>().ToList();
					return list;
				case "Property":
					list = (pn as ExtensionBaseType).Property.Cast<BaseType>().ToList();
					return list;
				case "OnEvent":
					list = (pn as FormDesignType).OnEvent.Cast<BaseType>().ToList();
					if (list != null) return list;
					list = (pn as DisplayedType).OnEvent.Cast<BaseType>().ToList();
					if (list != null) return list;
					break;
				case "Section":
				case "ListItem":
				case "Question":
				case "Header":
				case "Body":
				case "Footer":
					list = (pn as ChildItemsType).Items.Cast<BaseType>().ToList();
					return list;
				case "BlobContent":
					list = (pn as DisplayedType).BlobContent.Cast<BaseType>().ToList();
					return list;
				case "CodedValue":
					list = (pn as DisplayedType).CodedValue.Cast<BaseType>().ToList();
					return list;
				case "Contact":
					list = (pn as DisplayedType).Contact.Cast<BaseType>().ToList();
					return list;
				case "Link":
					list = (pn as DisplayedType).Link.Cast<BaseType>().ToList();
					return list;
				case "OnEnter":
					list = (pn as DisplayedType).OnEnter.Cast<BaseType>().ToList();
					return list;
				case "OnSelect":
					list = (pn as ListItemBaseType).OnSelect.Cast<BaseType>().ToList();
					return list;
				case "OnDeselect":
					list = (pn as ListItemBaseType).OnDeselect.Cast<BaseType>().ToList();
					return list;
				case "xx":
					list = (pn as DisplayedType).BlobContent.Cast<BaseType>().ToList();
					return list;



				default:
					break;



			}


			return null;
		}


		internal static List<T> X_GetStatedListParent<T>(T item)
	where T : BaseType
		{   //get the list object that points to the item node
			//Only works for SDC List<BaseType> derivitives.   Does not work e.g., for XML types, derived from XmlElement.
			//Work out how to return a list of the exact type <T>.

			var pn = item.ParentNode;
			if (pn is null) return null;

			switch (item)
			{
				case ExtensionType et:
					return (pn as ExtensionBaseType).Extension as List<T>;
				case CommentType ct:
					return (pn as ExtensionBaseType).Comment as List<T>;
				case PropertyType pt:
					return (pn as ExtensionBaseType).Property as List<T>;
				case EventType ev:
					return X_GetStatedEventParent(ev).Cast<T>().ToList();
				case SectionItemType s:
				case ListItemType li:
				case QuestionItemType q:
					return (pn as ChildItemsType).Items as List<T>;
				case BlobType bt:
					return (pn as DisplayedType).BlobContent as List<T>;
				case CodingType ct:
					return (pn as DisplayedType).CodedValue as List<T>;
				case ContactType ctt:
					return (pn as DisplayedType).Contact as List<T>;
				case LinkType lt:
					return (pn as DisplayedType).Link as List<T>;
				case "xx":
					return (pn as DisplayedType).BlobContent as List<T>;

				default:
					throw new ArgumentException("Unknown input item:" + item.ElementName ?? "\"\"");
			}

			return null;
		}

		public static bool X_IsItemChangeAllowed<S, T>(S source, T target)
			where S : notnull, IdentifiedExtensionType
			where T : notnull, IdentifiedExtensionType
		{
			ChildItemsType ci;
			switch (source)
			{
				case SectionItemType _:
					ci = (source as ChildItemsType);
					switch (target)
					{
						case SectionItemType _:
						case QuestionItemType _:
							return true;
						case ButtonItemType _:
						case DisplayedType _:
							if (ci is null) return true;
							if (ci.ChildItemsList is null) return true;
							if (ci.ChildItemsList.Count == 0) return true;
							return false;
						case InjectFormType j:
							return false;
						default: return false;
					}

				case QuestionItemType q:
					ci = (source as ChildItemsType);
					switch (target)
					{
						case QuestionItemType _:
							//probably should not allow changing Q types
							return false;
						default: return false;
					}

				case ListItemType _:
					ci = (source as ChildItemsType);
					switch (target)
					{
						case SectionItemType _:
						case QuestionItemType _:
						case ListItemType _:
						case InjectFormType _:
							return false;
						case ButtonItemType _:
						case DisplayedType _:
							if (ci is null) return true;
							if (ci.ChildItemsList is null) return true;
							if (ci.ChildItemsList.Count == 0) return true;
							return false;
						default: return false;
					}

				case ButtonItemType b:
					return false;
				case DisplayedType d:
					return true;
				case InjectFormType j:
					return false;
				default:
					break;
			}
			return false;
		}

		private static void X_AssignXmlElementAndOrder<T>(T bt) where T : notnull, BaseType
		{
			var pi = SdcUtil.GetPropertyInfoMeta(bt);
			bt.ElementName = pi.XmlElementName;
			bt.ElementOrder = pi.XmlOrder;
		}

		#endregion

		#region Helpers




		/// <summary>
		/// For each SDC node, the ID of the closest IdentifiedExtensionType ancestor is used to create a "shortID" 
		/// for all of its non-IdentifiedExtensionType child elements.  
		/// This shortID is used in conjuntion with other node properties of the object 
		/// (e.g., prefix, propName (for Properties) etc.)
		/// to create a unique name attribute for every SDC node (and thus each serialized XML element).
		/// </summary>
		/// <param name="bt">an object of type BaseType</param>
		/// <param name="nameSpace">The namespace part of BaseType.ID. Must be ".100004300" for CAP </param>
		/// <param name="nameSuffix">the last part of the new name</param>
		/// <returns></returns>
		public static string CreateNameCAP(this BaseType bt, string nameSpace, string nameSuffix)
		{
			string prefix = bt.ElementPrefix;
			string shortID = "";
			if (bt is PropertyType pt && pt.propName is not null)
			{
				prefix = prefix + "_" + pt.propName + "_";
			}
			if (bt is IdentifiedExtensionType iet)
			{
				shortID = "_" + Regex.Replace(iet.ID.Replace(nameSpace, "") ?? "", @"\W+", ""); //remove namespace and special characters
				if (iet.name?.ToLower() == "body") shortID = "body" + shortID;
				else if (iet.name?.ToLower() == "footer") shortID = "footer" + shortID;
				else if (iet.name?.ToLower() == "header") shortID = "header" + shortID;
				else shortID = iet.ID.Replace(nameSpace, "") ?? "";
			}			
			else
			{
				shortID = Regex.Replace(
				bt.ParentIETypeNode?.ID.Replace(nameSpace, "") ?? "",
				@"\W+", ""); //remove namespace and special characters
			}
			if (prefix.Length > 0 && 
				(shortID.Length > 0 || nameSuffix.Length > 0)) prefix += "_";
			//Debug.Write(prefix + shortID + nameSuffix + "\r\n");
			return prefix + shortID + nameSuffix;
		}
		/// <summary>
		/// Given a parent SDC node, this method will sort the child nodes (kids)
		/// </summary>
		/// <param name="parentItem">The parent SDC node</param>
		/// <param name="kids">"kids" is a List&lt;BaseType> containing all the child nodes under parentItem.
		/// This is generally obtained from TopNode.ChildNodes Dictionary object</param>
		private static void SortKids(BaseType parentItem, List<BaseType> kids)
		{
			if (!TreeSort_NodeIds.Contains(parentItem.ObjectID))
			{//Sorting is a very expensive operation, so we only sort once per parent node
				TreeSort_NodeIds.Add(parentItem.ObjectID);
				kids.Sort(new TreeSibComparer());
			}
		}

		public static string XmlFormat(string Xml)
		{
			return System.Xml.Linq.XDocument.Parse(Xml).ToString();  //prettify the minified doc XML 
		}

		/// <summary>
		/// Write a new order attribute and value into every element of an Xml file
		/// </summary>
		/// <param name="Xml"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static string XmlReorder(string Xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(Xml);
			if (doc is null) throw new InvalidOperationException("the Xml string could not be loaded into an XmlDocument");
			var xmlNodeList = doc.SelectNodes("//*");
			if (xmlNodeList is null) throw new InvalidOperationException("the Xml string could not be loaded into an xmlNodeList");
			int j = 0;
			foreach (XmlNode node in xmlNodeList)
			{   //renumber the XML elements in Node order
				if (node.NodeType == XmlNodeType.Element)
				{
					if (node.Attributes!.GetNamedItem("order") is null)
					{
						var attOrder = doc.CreateAttribute("order");
						node.Attributes.Append(attOrder);
					}
					node.Attributes!["order"]!.Value = j++.ToString();
				}
			}
			return doc.OuterXml;
		}

		#endregion



	}

	#region Empty Interface Extension Classes	 - Not yet used
	public static class INewTopLevelExtensions { } //Empty
	public static class IPackageExtensions { } //Empty
	public static class IDataElementExtensions { } //Empty
	public static class IDemogFormExtensions { } //Empty
	public static class IMapExtensions { } //Empty
	public static class RetrieveFormPackageTypeExtensions //Not Implemented
	{
		public static LinkType AddFormURL_(this RetrieveFormPackageType rfp)
		{ throw new NotImplementedException(); }
		public static HTMLPackageType AddHTMLPackage_(this RetrieveFormPackageType rfp)
		{ throw new NotImplementedException(); }
		public static XMLPackageType AddXMLPackage_(this RetrieveFormPackageType rfp)
		{ throw new NotImplementedException(); }
	}
	public static class IChildItemsMemberExtensions
	{
		//!    public static bool X_IsMoveAllowedToChild<U>(U Utarget, out string error)
		//where U : notnull, IdentifiedExtensionType
		//        //where T : notnull, IdentifiedExtensionType
		//    {
		//        Tchild Tsource = this as Tchild;
		//        var errorSource = "";
		//        var errorTarget = "";
		//        error = "";
		//        bool sourceOK = false;
		//        bool targetOK = false;

		//        if (Tsource is null) { error = "source is null"; return false; }
		//        if (Utarget is null) { error = "target is null"; return false; }
		//        if (Utarget is ButtonItemType) { error = "ButtonItemType is not allowed as a target"; return false; }
		//        if (Utarget is InjectFormType) { error = "InjectFormType is not allowed as a target"; return false; }
		//        if (Utarget is DisplayedType) { error = "DisplayedItem is not allowed as a target"; return false; }

		//        if (Tsource is ListItemType && !(Utarget is QuestionItemType) && !(Utarget is ListItemType)) { error = "A ListItem can only be moved into a Question List"; return false; };

		//        //special case to allow LI to drop on a Q and be added to the Q's List, rather than under ChildItem (which would be illegal)
		//        if (Tsource is ListItemType &&
		//            Utarget is QuestionItemType &&
		//            !((Utarget as QuestionItemType).GetQuestionSubtype() == QuestionEnum.QuestionSingle) &&
		//            !((Utarget as QuestionItemType).GetQuestionSubtype() == QuestionEnum.QuestionMultiple))
		//        { error = "A Question target must be a QuestionSingle or QuestionMultiple"; return false; }


		//        if (Tsource is DisplayedType || Tsource is InjectFormType) sourceOK = true;
		//        if (Utarget is QuestionItemType || Utarget is SectionItemType || Utarget is ListItemType) targetOK = true;

		//        if (!sourceOK || !targetOK)
		//        {
		//            if (!sourceOK) errorSource = "Illegal source object";
		//            if (!targetOK) errorTarget = "Illegal target object";
		//            if (errorTarget.Length > 0) errorTarget += " and ";
		//            error = errorSource + errorTarget;
		//        }


		//        return sourceOK & targetOK;
		//    }
		//!    public static bool X_MoveAsChild<S, T>(S source, T target, int newListIndex)
		//        where S : notnull, IdentifiedExtensionType    //, IChildItemMember
		//        where T : DisplayedType, IChildItemsParent<T>
		//    {
		//        if (source is null) return false;
		//        if (source.ParentNode is null) return false;
		//        if (source is ListItemType && !(target is QuestionItemType)) return false;  //ListItem can only be moved to a question.

		//        List<BaseType> sourceList;
		//        BaseType newParent = target;

		//        switch (source)  //get the sourceList from the parent node
		//        {
		//            case QuestionItemType _:
		//            case SectionItemType _:
		//            case InjectFormType _:
		//            case ButtonItemType _:
		//                sourceList = (source.ParentNode as ChildItemsType)?.Items.ToList<BaseType>();
		//                //sourceList = (source.ParentNode as ChildItemsType).Items.Cast<BaseType>().ToList(); //alternate method
		//                break;
		//            case ListItemType _:
		//                sourceList = (source.ParentNode as ListType)?.Items.ToList<BaseType>();
		//                break;
		//            case DisplayedType _:
		//                sourceList = (source.ParentNode as ChildItemsType)?.Items.ToList<BaseType>();
		//                if (sourceList is null)
		//                    sourceList = (source.ParentNode as ListType)?.Items.ToList<BaseType>();
		//                else return false;
		//                break;
		//            default:
		//                return false; //error in source type
		//        }

		//        if (sourceList is null) return false;

		//        List<BaseType> targetList = null;

		//        if (target != null)
		//        {
		//            switch (target)  //get the targetList from the child node
		//            {
		//                case QuestionItemType q:
		//                    //This is an exception - if we drop a source LI on a QS/QM, we will want to add it ant the end of the Q's List object
		//                    if (source is ListItemType)
		//                    {
		//                        if (q.GetQuestionSubtype() != QuestionEnum.QuestionSingle &&
		//                            q.GetQuestionSubtype() != QuestionEnum.QuestionMultiple &&
		//                            q.GetQuestionSubtype() != QuestionEnum.QuestionRaw) return false;  //QR, and QL cannot have child LI nodes
		//                        if (q.ListField_Item is null)  //create new targetList
		//                        {
		//                            targetList = IQuestionBuilder.AddListToListField(IQuestionBuilder.AddListFieldToQuestion(q)).Items.ToList<BaseType>();
		//                            if (targetList is null) return false;
		//                            break;
		//                        }
		//                        newParent = q.ListField_Item.List;
		//                        targetList = q.ListField_Item.List.Items.ToList<BaseType>();
		//                    }
		//                    else //use the ChildItems node instead as the targetList
		//                    {
		//                        (q as IChildItemsParent<QuestionItemType>).AddChildItemsNode(q);
		//                        targetList = q.ChildItemsNode.Items.ToList<BaseType>();
		//                    }
		//                    break;
		//                case SectionItemType s:
		//                    (s as IChildItemsParent<SectionItemType>).AddChildItemsNode(s);
		//                    targetList = s.ChildItemsNode.Items.ToList<BaseType>();
		//                    break;
		//                case ListItemType l:
		//                    (l as IChildItemsParent<ListItemType>).AddChildItemsNode(l);
		//                    targetList = l.ChildItemsNode.Items.ToList<BaseType>();
		//                    break;
		//                default:
		//                    return false; //error in source type
		//            }
		//        }
		//        else targetList = sourceList;
		//        if (targetList is null) return false;


		//        var count = targetList.Count;
		//        if (newListIndex < 0 || newListIndex > count) newListIndex = count; //add to end  of list

		//        var indexSource = sourceList.IndexOf(source);  //save the original source index in case we need to replace the source node back to its origin
		//        bool b = sourceList.Remove(source); if (!b) return false;
		//        targetList.Insert(newListIndex, source);
		//        if (targetList[newListIndex] == source) //check for success
		//        {
		//            source.TopNode.ParentNodes[source.ObjectGUID] = newParent;
		//            return true;
		//        }
		//        //Error - the source item is now disconnected from the list.  Lets add it back to the end of the list.
		//        sourceList.Insert(indexSource, source); //put source back where it came from; the move failed
		//        return false;
		//    }
		//!    public static bool X_MoveAfterSib<S, T>(S source, T target, int newListIndex, bool moveAbove)
		//        where S : notnull, IdentifiedExtensionType
		//        where T : notnull, IdentifiedExtensionType
		//    {
		//        //iupdate TopNode.ParentNodes
		//        throw new Exception(String.Format("Not Implemented"));
		//    }
	} //Empty
	public static class ListFieldTypeExtensions
	{
		public static LookupEndPointType AddEndpoint(this ListFieldType lf)
		{
			if (lf.List == null)
			{
				var lep = new LookupEndPointType(lf);
				lf.LookupEndpoint = lep;
				return lep;
			}
			else throw new InvalidOperationException("Can only add LookupEndpoint to ListField if List object is not present");
		}
		public static ListType AddList(this ListFieldType lf)
		{
			ListType list;  //this is not the .NET List class; It's an answer list
			if (lf.List == null)
			{
				list = new ListType(lf);
				lf.List = list;
			}
			else list = lf.List;

			//The "list" item contains a list<DisplayedType>, to which the ListItems and ListNotes (DisplayedItems) are added.
			list.QuestionListMembers ??= new List<DisplayedType>();

			return list;
		}

	}
	public static class IQuestionBaseExtensions { } //Empty
	//public static class ISectionExtensions { } //Empty
	public static class ButtonItemTypeExtensions //Not Implemented
	{
		public static EventType AddOnClick_(this ButtonItemType bf)
		{ throw new NotImplementedException(); }
	}
	public static class InjectFormTypeExtensions //Not Implemented
	{  //ChildItems.InjectForm - this is mainly useful for a DEF injecting items based on the InjectForm URL
		//Item types choice under ChildItems
		public static FormDesignType AddFormDesign_(this InjectFormType ijt)
		{ throw new NotImplementedException(); }
		public static QuestionItemType AddQuestion_(this InjectFormType ijt)
		{ throw new NotImplementedException(); }
		public static SectionItemType AddSection_(this InjectFormType ijt)
		{ throw new NotImplementedException(); }

	}
	public static class DisplayedTypeChangesExtensions
	{
		public static QuestionItemType ChangeToQuestionMultiple_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionSingle_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionResponse_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionLookup_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static SectionItemType ChangeToSection_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static ButtonItemType ChangeToButtonAction_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static InjectFormType ChangeToInjectForm_(DisplayedType source)
		{ throw new NotImplementedException(); }

		public static DisplayedType ChangeToDisplayedItem_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionMultiple_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionSingle_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionResponse_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionLookup_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static ButtonItemType ChangeToButtonAction_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static InjectFormType ChangeToInjectForm_(SectionItemType source)
		{ throw new NotImplementedException(); }


		public static DisplayedType ChangeToDisplayedItem_(ListItemType source)
		{ throw new NotImplementedException(); }

		//ListItemType ChangeToListItem
		//ListItemType ChangeToListItemResponse
		//SectionItemType ChangeToSection()
		//ChangeToButtonAction
		//ChangeToInjectForm
		//etc.


		//Question
		public static SectionItemType ChangeToSection_(QuestionItemType source)
		{ throw new NotImplementedException(); }
		public static DisplayedType ChangeToDisplayedType_(QuestionItemType source)
		{ throw new NotImplementedException(); }
	}
	//public static class IDisplayedTypeMemberExtensions { }//Empty; for LinkType, BlobType, ContactType, CodingType, EventType, OnEventType, PredGuardType
	public static class BlobExtensions //Not Implemented
	{
		//DisplayedItem.BlobType
		//Uses Items types choice
		public static bool AddBinaryMedia_(this BlobType b)
		{ throw new NotImplementedException(); } //Empty

		public static bool AddBlobURI_(this BlobType b)
		{ throw new NotImplementedException(); }
	}
	public static class IdentifiedExtensionTypeExtensions //Not Implemented
	{
		public static string GetNewCkey_(this IdentifiedExtensionType i)
		{ throw new NotImplementedException(); }
	}
	public static class IResponseExtensions //Not Implemented
	{
		//UnitsType AddUnits(ResponseFieldType rfParent);
		//public static UnitsType AddUnits(this IResponse _, ResponseFieldType rfParent)
		//{
		//    UnitsType u = new UnitsType(rfParent);
		//    rfParent.ResponseUnits = u;
		//    return u;
		//}
		public static RichTextType AddTextAfterResponse_()
		{ throw new NotImplementedException(); }
		public static BaseType GetDataTypeObject_()
		{ throw new NotImplementedException(); }
	}
	public static class IValExtensions
	{//Implemented by data types, which have a strongly-typed val attribute.  Not implemented by anyType, XML, or HTML  
	} //Empty
	public static class IValNumericExtensions
	{//Implemented by numeric data types, which have a strongly-type val attribute.

	} //Empty
	public static class IValDateTimeExtensions
	{//Implemented by DateTime data types, which have a strongly-type val attribute.

	} //Empty
	public static class IValIntegerExtensions
	{//Implemented by Integer data types, which have a strongly-type val attribute.  Includes byte, short, long, positive, no-positive, negative and non-negative types
	} //Empty
	public static class IAddCodingExtensions //Not Implemented
	{
		public static CodingType AddCodedValue_(this IAddCoding ac, DisplayedType dt, int insertPosition)
		{
			throw new NotImplementedException();
		}

		public static CodingType AddCodedValue_(this IAddCoding ac, LookupEndPointType lep, int insertPosition)
		{
			throw new NotImplementedException();
		}
		public static UnitsType AddUnits(this IAddCoding ac, CodingType ctParent)
		{
			var u = new UnitsType(ctParent);
			ctParent.Units = u;
			return u;
		}
	}
	public static class IAddOrganizationExtension
	{
		public static OrganizationType AddOganization_(this IAddOrganization ao)
		{ throw new NotImplementedException(); }

		public static OrganizationType AddOrganization(this IAddOrganization ao, ContactType contactParent)
		{
			var ot = new OrganizationType(contactParent);
			contactParent.Organization = ot;

			return ot;
		}
		public static OrganizationType AddOrganization(this IAddOrganization ao, JobType jobParent)
		{
			var ot = new OrganizationType(jobParent);
			jobParent.Organization = ot;

			return ot;
		}
		public static OrganizationType AddOrganizationItems_(this IAddOrganization ao, OrganizationType ot)
		{ throw new NotImplementedException(); }
	}  //Not Implemented
	public static class IEventExtension  //Not Implemented  //Used for events (PredActionType)
	{
		public static PredEvalAttribValuesType AddAttributeVal(this IEvent ae)
		{
			var pgt = (PredActionType)ae;
			var av = new PredEvalAttribValuesType(pgt);
			pgt.Items.Add(av);
			return av;
		}
		//public static ScriptBoolFuncActionType AddScriptBoolFunc_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		//public static CallFuncBoolActionType AddCallBoolFunction_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		//public static MultiSelectionsActionType AddMultiSelections_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		//public static SelectionSetsActionType AddSelectionSets_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		public static PredSelectionTestType AddSelectionTest_(this IEvent ae)
		{ throw new NotImplementedException(); }
		//PredAlternativesType AddItemAlternatives();
		public static RuleSelectMatchingListItemsType SelectMatchingListItems_(this IEvent ae)
		{ throw new NotImplementedException(); }
		public static PredGuardType AddGroup_(this IEvent ae)
		{ throw new NotImplementedException(); }
	}
	public static class IPredGuardExtensions //Not Implemented //used by Guards on ListItem, Button, e.g., SelectIf, DeselectIf
	{
		public static PredEvalAttribValuesType AddAttributeVal(this IPredGuard ipg)
		{
			var pgt = (PredGuardType)ipg;
			var av = new PredEvalAttribValuesType(pgt);
			pgt.Items.Add(av);
			return av;
		}
		//public static ScriptBoolFuncActionType AddScriptBoolFunc_(this IPredGuard ipg)
		//{ throw new NotImplementedException(); }
		//public static CallFuncBoolActionType AddCallBoolFunction_(this IPredGuard ipg) 
		//{ throw new NotImplementedException(); }
		//public static MultiSelectionsActionType AddMultiSelections_(this IPredGuard ipg) 
		//{ throw new NotImplementedException(); }
		public static PredSelectionTestType AddSelectionTest_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }
		public static PredGuardTypeSelectionSets AddSelectionSets_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }
		public static PredAlternativesType AddItemAlternatives_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }
		public static PredGuardType AddGroup_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }


	}
	public static class IRuleExtensions
	{
		public static RuleAutoActivateType AddAutoActivation_(this IRule r)
		{ throw new NotImplementedException(); }
		public static RuleAutoSelectType AddAutoSelection_(this IRule r)
		{ throw new NotImplementedException(); }
		public static PredActionType AddConditionalActions_(this IRule r)
		{ throw new NotImplementedException(); }
		//public static CallFuncActionType AddExternalRule_(this IRule r)
		//{ throw new NotImplementedException(); }
		public static ScriptCodeAnyType AddScriptedRule_(this IRule r)
		{ throw new NotImplementedException(); }
		public static RuleSelectMatchingListItemsType AddSelectMatchingListItems_(this IRule r)
		{ throw new NotImplementedException(); }
		public static ValidationType AddValidation_(this IRule r)
		{ throw new NotImplementedException(); }
	} //Not Implemented
	public static class IHasConditionalActionsNodeExtensions
	{
		public static PredActionType AddConditionalActionsNode_(this IHasConditionalActionsNode hcan)
		{ throw new NotImplementedException(); }
	} //Not Implemented
	public static class IHasParameterGroupExtensions //Not Implemented
	{
		public static ParameterItemType AddParameterRefNode_(this IHasParameterGroup hpg)
		{ throw new NotImplementedException(); }
		public static ListItemParameterType AddListItemParameterRefNode_(this IHasParameterGroup hpg)
		{ throw new NotImplementedException(); }
		public static ParameterValueType AddParameterValueNode_(IHasParameterGroup hpg)
		{ throw new NotImplementedException(); }
	}
	public static class IHasDataType_STypeExtensions //Not Implemented
	{
		public static DataTypes_SType AddDataTypes_SType_(this DataTypes_SType S)
		{ throw new NotImplementedException(); }
	}
	public static class IHasDataType_DETypeExtensions
	{
		public static DataTypes_DEType AddDataTypes_DEType_(this DataTypes_DEType DE)
		{ throw new NotImplementedException(); }
	} //Not Implemented
	public static class IHasActionElseGroupExtensions { } //Empty
	//public static class IHasElseNodeExtensions
	//{
	//    public static PredActionType AddElseNode(this IHasElseNode hen)
	//    {
	//        if (hen is null) return null;
	//        var elseNode = new PredActionType((BaseType)hen);

	//        switch (hen)
	//        {
	//            case PredActionType pe:
	//                pe.Else.Add(elseNode); return elseNode;
	//            case CallFuncBoolActionType cfb:
	//                return (PredActionType)SdcUtil.ArrayAddReturnItem(cfb.Items1, elseNode);
	//            case ScriptBoolFuncActionType sb:
	//                return (PredActionType)SdcUtil.ArrayAddReturnItem(sb.Items, elseNode);
	//            case AttributeEvalActionType ae:
	//                ae.Else.Add(elseNode); return elseNode;
	//            case MultiSelectionsActionType ms:
	//                ms.Else.Add(elseNode); return elseNode;
	//            case SelectionSetsActionType ss:
	//                ss.Else.Add(elseNode); return elseNode;
	//            case SelectionTestActionType st:
	//                st.Else.Add(elseNode); return elseNode;
	//            default:
	//                break;
	//        }
	//        throw new InvalidCastException();
	//        //return new Els
	//    }
	//}
	public static class IActionsMemberExtensions { } //Empty
	public static class ActSendMessageTypeExtensions //Not Implemented
	{
		//List<ExtensionBaseType> Items
		//Supports ActSendMessageType and ActSendReportType
		public static EmailAddressType AddEmail_(this ActSendMessageType smr)
		{ throw new NotImplementedException(); }
		public static PhoneNumberType AddFax_(this ActSendMessageType smr)
		{ throw new NotImplementedException(); }
		//public static CallFuncActionType AddWebService_(this ActSendMessageType smr)
		//{ throw new NotImplementedException(); }
	}
	public static class CallFuncBaseTypeExtensions //Not Implemented
	{
		//anyURI_Stype Item (choice)
		public static anyURI_Stype AddFunctionURI_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
		public static anyURI_Stype AddLocalFunctionName_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }

		//List<ExtensionBaseType> Items
		public static ListItemParameterType AddListItemParameterRef_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
		public static ParameterItemType AddParameterRef_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
		public static ParameterValueType AddParameterValue_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
	}


	//public static class CallFuncActionTypeExtensions
	//{
	//    public static anyURI_Stype AddConnditionalActions_(this CallFuncActionType cfat)
	//    { throw new NotImplementedException(); }
	//}
	//public static class ScriptBoolFuncActionTypeExtensions
	//{
	//    //ExtensionBaseType[] Items 
	//    public static ActionsType AddActions_(this ScriptBoolFuncActionType sbfa)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddConditionalActions_(this ScriptBoolFuncActionType sbfa)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddElse_(this ScriptBoolFuncActionType sbfa)
	//    { throw new NotImplementedException(); }
	//} 
	//public static class CallFuncBoolActionTypeExtensions
	//{
	//    //ExtensionBaseType[] Items1
	//    //see IScriptBoolFuncAction, which is identical except that this interface implementation must use "Item1", not "Item"
	//    //Implementations using Item1:
	//    public static ActionsType AddActions_(this CallFuncBoolActionType cfba)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddConditionalActions_(this CallFuncBoolActionType cfba)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddElse_(this CallFuncBoolActionType cfba)
	//    { throw new NotImplementedException(); }
	//}
	public static class IValidationTestsExtensions //Not Implemented
	{
		public static PredAlternativesType AddItemAlternatives_(this IValidationTests vt)
		{ throw new NotImplementedException(); }
		public static ValidationTypeSelectionSets AddSelectionSets_(this IValidationTests vt)
		{ throw new NotImplementedException(); }
		public static ValidationTypeSelectionTest AddSelectionTest_(this IValidationTests vt)
		{ throw new NotImplementedException(); }
	}
	public static class ICloneExtensions// Probably belongs on IBaseType 
	{
		public static BaseType CloneSubtree_(this IClone c, BaseType top)
		{ throw new NotImplementedException(); }
	}
	public static class IHtmlPackageExtensions //Not Implemented
	{
		public static base64Binary_Stype AddHTMLbase64_(this IHtmlPackage hp)
		{ throw new NotImplementedException(); }
	}
	public static class RegistrySummaryTypeExtensions //Not Implemented
	{
		//BaseType[] Items
		//Attach to Admin.RegistryData as OriginalRegistry and/or CurrentRegistry

		public static ContactType AddContact_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static FileType AddManual_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static string_Stype AddReferenceStandardIdentifier_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static InterfaceType AddRegistryInterfaceType_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static string_Stype AddRegistryName_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static FileType AddRegistryPurpose_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static FileType AddServiceLevelAgreement_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
	}
	#endregion

}
