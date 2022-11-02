using CSharpVitamins;
using MsgPack.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Buffers;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Runtime.InteropServices;
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
	/// <summary>
	/// This class is primarily used as source material for creating consistent type and interface-specific SDC extension methods
	/// </summary>
	public static class SdcUtil
	{
		private static Dictionary<Guid, BaseType> Get_Nodes(BaseType n)
		{ return Get_ITopNode(n)._Nodes; }
		private static Dictionary<Guid, List<BaseType>> Get_ChildNodes(BaseType n)
		{ return Get_ITopNode(n)._ChildNodes; }
		private static Dictionary<Guid, BaseType> Get_ParentNodes(BaseType n)
		{ return Get_ITopNode(n)._ParentNodes; }
		private static ObservableCollection<IdentifiedExtensionType> Get_IETnodes(BaseType n)
		{ return Get_ITopNode(n)._IETnodes; }


		/// <summary>
		/// This SortedSet contains the ObjectID of each node that has been sorted by ITreeSibComparer.  
		/// Each entry in this SortedSet indicates that nodes child nodes have already been sorted.  
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
		/// so that IComparer does not need to run again for that parent node's child nodes (until TreeSort_ClearNodeIds is called again).
		/// Code can test for the presence of a flagged parent node (i.e., with child nodes already-sorted) with TreeSort_IsSorted.  
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
		/// <summary>
		/// Create new array with nulls removed
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <returns></returns>
		public static T[] RemoveArrayNullsNew<T>(T[] array) where T:class
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

#region Delegates


		/// <summary>
		/// Delegate that points to a single method for creating a new <see cref="BaseType.name"/> value for the designated <paramref name="node"/>.
		/// </summary>
		/// <returns>The new name that was used to refresh <see cref="BaseType.name"/> on <paramref name="node">.</paramref></returns>
		public delegate string CreateName(BaseType node);

		/// <summary>
		/// Reserved for future use. <paramref name=""/>
		/// </summary>
		/// <param name="node"></param>
		/// <param name="icon"></param>
		/// <param name="html"></param>
		/// <returns></returns>
		public delegate string NodeAnnotation(BaseType node, byte[] icon, string html);

# endregion

		/// <summary>
		/// If <paramref name="refreshTree"/> is true (default),
		///	this method uses reflection to refresh: <br/>
		///		_TTopNode._Nodes, _ITopNode_ParentNodes and _ITopNode_ChildNodes dictionaries, with all nodes in the proper order.<br/>
		///		Some BaseType properties are updated: <br/>
		///		SGuid properties are created if missing, and name and order properties are created/updated as needed.  <br/>
		///		@name properties will be overwritten with new @name values that may not match the original.<br/>
		/// If <paramref name="refreshTree"/> is false, this method returns an ordered List&lt;BaseType>, <br/>
		/// but none of the above refresh actions are performed.
		/// </summary>
		/// <param name="topNode">The ITopNode SDC node that will have its tree refreshed<br/>
		/// Ideally, this should be the top root node, not a subsumed ITopNode node. 
		/// </param>
		/// <param name="treeText">An "out" variable containing, if <paramref name="print"/> is true, a text representation of some important properties of each node</param>
		/// <param name="print">If <paramref name="print"/> is true, then <paramref name="treeText"/> will be generated.  
		/// If <paramref name="print"/> is false, <paramref name="treeText"/> will be null.
		/// The default is false.</param>
		/// <param name="refreshTree">Determines whether to refresh the Dictionaries and BaseType properties, as described in the summary.
		/// The default is true.</param>
		/// <param name="createNodeName">A delegate to represent a single function that will create a <see cref="BaseType.name"/> for each refreshed node in the ITopNode tree.</param>
		/// <returns>List&lt;BaseType> containing all of the SDC tree nodes in sorted top-bottom order</returns>
		public static List<BaseType> ReflectRefreshTree(ITopNode topNode, out string? treeText, bool print = false, bool refreshTree = true, CreateName? createNodeName = null)
		{
			TreeSort_ClearNodeIds();
			int counter = 0;
			int indent = 0;
			int order = 0;
			List<BaseType> SortedNodes = new();  //this will be the returned object from this method

			var sbTreeText = new StringBuilder();
			var newPropsText = new StringBuilder();
			//If the initial topNode has subsumed other ITopNode subtrees,
			//currentTopNode will track the current subtree ITopNode 
			var current_ITopNode = (_ITopNode)topNode;
			BaseType btNode = (BaseType)topNode;
			btNode.order = 0;

			{
				if (current_ITopNode is DemogFormDesignType dfd)  //DemogForm is also a FormDesignType, so it must come first 
				{
					dfd.ElementName = "DemogFormDesign";
					dfd.name = Regex.Replace(dfd.ID, @"\W+", ""); //replaces any characters that are not numbers, letters or "_"
				}
				else if (current_ITopNode is FormDesignType fd)
				{
					fd.ElementName = "FormDesign";
					fd.name = Regex.Replace(fd.ID, @"\W+", "");
				}

				else if (current_ITopNode is DataElementType de)
				{
					de.ElementName = "DataElement";
					de.name = Regex.Replace(de.ID, @"\W+", "");
				}
				else if (current_ITopNode is RetrieveFormPackageType rfp)
				{
					rfp.ElementName = "RetrieveFormPackage";
					if (!rfp.instanceID.IsNullOrWhitespace()) rfp.name = Regex.Replace(rfp.instanceID, @"\W+", "");
					else rfp.name = Regex.Replace(rfp.packageID, @"\W+", "");
				}
				else if (current_ITopNode is MappingType mp)
				{
					mp.ElementName = "MappingType";
					if (!mp.templateID.IsNullOrWhitespace()) mp.name = Regex.Replace(mp.templateID, @"\W+", "");
					else mp.name = Regex.Replace(mp.templateID, @"\W+", "");
				}
			}
			//Set _currentTopNode; If other ITopNode nodes are subsumed in this tree, current_ITopNode will be adjusted to the subsumed node(s)
			if (refreshTree)
			{
				Init_ITopNode(current_ITopNode);
				Fill_NodesAnd_IETnodes(btNode, null);
				if (btNode.ParentNode is null) btNode.TopNode = current_ITopNode; //points to itself, indicating this is the root node
				else btNode.TopNode = btNode.ParentNode.TopNode;
			}

			SortedNodes.Add(btNode);

			if (print) sbTreeText.Append($"({btNode.DotLevel})#{counter}; OID: {btNode.ObjectID}; name: {btNode.name}{content(btNode)}");

			//DoTree(currentTopNode as BaseType);
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
					//piChildProperty is the PropertyInfo object from the btProp property
					//Neither piChildProperty nor btProp reliably contains the XML element name for the btProp node
					//In some cases, it can be obtained by looking at the parentNode,
					//finding the IEnumerable<> Property that contains btProp, and then looking for 
					//the enum value that contains the XML element name.
					//The enum location is found in XmlChoiceIdentifierAttribute on the IEnumerable Property
					//This should be handled in ReflectSdcElement
					btProp.TopNode = current_ITopNode;

					if (btProp is _ITopNode itn) //we have a subsumed ITopNode node
						current_ITopNode = Init_ITopNode(itn);				
					
					//Refill the node dictionaries with the current node
					Fill_NodesAnd_IETnodes(btProp, parentNode);
					btProp.RegisterParent(parentNode, childNodesSort: false);
					Debug.Print(btProp.sGuid + "; Obj ID: " + btProp.ObjectID);

					//Mark parentNode as having its child nodes already sorted
					TreeSort_NodeIds.Add(parentNode.ObjectID);  //Change ObjectID to ObjectGUID?
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
					if (elementName.IsNullOrWhitespace()) Debugger.Break();
					btProp.ElementName = elementName;
					btProp.ElementOrder = elementOrder;
					btProp.order = ++order;

					if (btProp.sGuid is null)
					{
						if (btProp.ObjectGUID == Guid.Empty) btProp.ObjectGUID = new Guid();
						btProp.sGuid = new CSharpVitamins.ShortGuid(btProp.ObjectGUID).Value;
					}
					if (btProp is IdentifiedExtensionType iet)
					{
						if (iet.ID.IsNullOrEmpty()) iet.ID = iet.sGuid;
					}
					if (createNodeName is not null) btProp.name = createNodeName(btProp) ?? btProp.name;

					if (print)
					{
						sbTreeText.Append($"ElementName: {btProp.ElementName}; ElementOrder: {btProp.ElementOrder}; order: {btProp.order}; name: {btProp.name}; sGuid = {btProp.sGuid}");
						sbTreeText.Append("\r\n");
					}
				}
			}
			_ITopNode Init_ITopNode(_ITopNode new_ITopNode)
			{
				new_ITopNode.ClearDictionaries();			
				return new_ITopNode;
			}
			void Fill_NodesAnd_IETnodes(BaseType btNode, BaseType? parentNode)
			{//current_ITopNode here points to the ITopNode ancestor of btNode

				//First, check sGuid and ObjectGUID status:
				if (btNode.sGuid.IsNullOrWhitespace())
				{
					if (btNode.ObjectGUID == default(Guid)) //Empty ObjectGUID
						btNode.ObjectGUID = Guid.NewGuid();
					btNode.sGuid = ShortGuid.Encode(btNode.ObjectGUID);
				}
				else
				{	//sGuid and ObjectGUID ideally should match before adding to dictionaries.
					var decodedShortGuid = ShortGuid.Decode(btNode.sGuid);
					if (btNode.ObjectGUID != decodedShortGuid)
						btNode.ObjectGUID = decodedShortGuid;
				}

				current_ITopNode._Nodes.Add(btNode.ObjectGUID, btNode);
				if (btNode is IdentifiedExtensionType iet)
					current_ITopNode._IETnodes.Add(iet);

				if (btNode is _ITopNode itn && parentNode is not null) 
				{   //also store the node in the current node's parent ITopNode dictionaries

					//Find current node's parent ITopNode dictionaries
					_ITopNode par_ITopNode;
					if (parentNode is ITopNode ptn) 
						par_ITopNode = (_ITopNode)parentNode;//only occurs in RetrieveFormPackage under RetrieveFormPackage
					else par_ITopNode = (_ITopNode)parentNode.TopNode;

					par_ITopNode._Nodes.Add(btNode.ObjectGUID, btNode);
					if (btNode is IdentifiedExtensionType ietPar)
						par_ITopNode._IETnodes.Add(ietPar);
				}
			}

			//The "content" function is a temporary kludge to generate printable output.  
			//It should be easy to create a tree walker to create any desired output by visiting each node.
			string content(BaseType n)
			{
				string s;
				if (n is DisplayedType) s = "; title: " + (n as DisplayedType)?.title ?? "";
				else if (n is PropertyType) s = "; " + (n as PropertyType)?.propName ?? "" + ": " + (n as PropertyType)?.val ?? "";
				else s = $"; type: {n.GetType().Name}";
				return s;
			}
			
			//We should instead return SortedNodes, and provide the treeText as an out parameter
			if (print) treeText = sbTreeText.ToString();
			else treeText = null;
			return SortedNodes;

		}
		/// <summary>
		/// Reflects the SDC tree and re-registers all nodes in the tree in the main SDC OM dictionaries: _ITopNode._Nodes, _ITopNode._ParentNodes, _ITopNode._ChildNodes.
		/// </summary>
		/// <param name="tn"></param>
		/// <returns> Sorted List<BaseType> containing all nodes subsumed under <paramref name="tn"/></returns>
		public static List<BaseType> ReflectUpdateTreeDictionaries(ITopNode tn)
		{
			return ReflectRefreshSubtreeList((BaseType)tn.TopNode);
		}
		/// <summary>
		/// Lighter-weight reflection tree walker that uses only reflection, and no node dictionaries.
		/// Does not update any tree nodes or dictionaries.
		/// </summary>
		/// <param name="topNode"></param>
		/// <param name="print">Set to true to create nested text output in treeText</param>
		/// <param name="treeText">Provides nested text output of the tree, when <paramref name="print"/> is set to true.</param>
		/// <returns>Sorted List&lt;BaseType> containing all nodes in the tree</returns>
		public static List<BaseType> ReflectTreeList(ITopNode topNode, out string treeText, bool print = false)
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
				var kids = ReflectChildElements(n);
				//BaseType?[]? kids = ReflectChildElements(n)?.ToArray<BaseType?>();

				if (kids is not null)
				{
					foreach (BaseType kid in kids)
					{
						outList.Add(n); //if not creating treeText, we could use AddRange instead
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
					ReflectNextSibElement(n);
					MoveNext(n);
				}
			}
			//TreeSort_ClearNodeIds();
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
		public static List<BaseType> GetSortedTreeList(ITopNode tn)
		{
			return GetSortedSubtreeList((BaseType)tn.TopNode);
		}

		/// <summary>
		/// Includes the input node n.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="startReorder"></param>
		/// <param name="orderInterval"></param>
		/// <param name="ResetSortFlags"></param>
		/// <returns><see cref="List{BaseType}"/> where T = <see href="BaseType"/></returns>
		public static List<BaseType> GetSortedSubtreeList(BaseType n, int startReorder = 0, int orderInterval = 1, bool ResetSortFlags = true)
		{
			//var nodes = n.TopNode.Nodes;
			//var topNode = Get_ITopNode(n);
			var cn = Get_ChildNodes(n);// topNode._ChildNodes;
			int i = 0;
			var sortedList = new List<BaseType>();
			if (ResetSortFlags) TreeSort_ClearNodeIds();

			MoveNext(n);

			void MoveNext(BaseType n)
			{
				sortedList.Add(n);
				if (startReorder >= 0)
				{
					n.order = i;
					i += orderInterval;
				}

				//shorter code option:
				//List<BaseType>? childList = SortElementKids(n);
				//if (childList != null)
				//	foreach (var child in childList)
				//		MoveNext(child);

				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						SortElementKids(n, childList);
						foreach (var child in childList)
							MoveNext(child);
					}
				}
			}
			return sortedList;
		}


		/// <summary>
		/// Get a sorted list of node n, plus of all of node n's sub-elements, <br/>
		/// Includes the input node n, only if it is an IdentifiedExtensionType node.
		/// </summary>
		/// <param name="n">The node whose subtree we are retrieving</param>
		/// <param name="resortChildNodes">Set to true if the child nodes may be incoreectly sorted.  This should not be needed.</param>
		/// <param name="resetSortFlags">If true, the method will call <see cref="SdcUtil.TreeSort_ClearNodeIds"/> </param>
		/// <returns></returns>
		public static List<IdentifiedExtensionType> GetSortedSubtreeIET(BaseType n, bool resortChildNodes = false, bool resetSortFlags = true)
		{
			//var topNode = Get_ITopNode(n);
			var cn = Get_ChildNodes(n);// topNode._ChildNodes;
			var sortedList = new List<IdentifiedExtensionType>();
			int i = -1;
			if (resortChildNodes && resetSortFlags) TreeSort_ClearNodeIds();

			MoveNext(n);

			void MoveNext(BaseType n)
			{
				if (n is IdentifiedExtensionType iet)
				{
					sortedList.Add(iet);
					//Console.WriteLine("SdcUtil.GetSortedSubtreeIET");
					//Console.WriteLine(n.ElementPrefix + ": " + n.As<DisplayedType>().title ?? "(null)" + "; ");
					//if (iet is QuestionItemType q1 && q1.name == "Procedure")
					//{
					//	Debugger.Break();
					//	Console.WriteLine("SdcUtil.GetSortedSubtreeIET");
					//	Console.WriteLine(n.ElementPrefix + ": " + n.As<DisplayedType>().title ?? "(null)" + "; ");
					//}
				}

				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						if (resortChildNodes) SortElementKids(n, childList);
						foreach (var child in childList)
							MoveNext(child);
					}
				}
			}
			return sortedList;
		}




		//!Should not need sorting of child nodes
		//
		/// <summary>
		/// Traverse an SDC tree by reflection to optionally reset @order and/or to refresh the TopNode dictionaries encounterd in the tree
		/// More tree traversal changes can be added by proving delagates to nodeWorker functions.
		/// </summary>
		/// <param name="startNode"></param>
		/// <param name="reOrder">Setting to true: refresh all @order properties in sequential order</param>
		/// <param name="reRegisterNodes">Setting to true: Reregister all nodes in the TopNodes dictionaries</param>
		/// <param name="startReorder">The starting number when reordering nodes with @order</param>
		/// <param name="orderInterval">The interval between new @order properties</param>
		/// <param name="resetObjectGUID">Setting to true: Create new ObjectGUID and matching sGuid.  Will always treat reRegisterNodes as true.</param>
		/// <param name="resetObjectID">Setting to true: Create a new ObjectID from TopNode.MaxObjectIDint</param>
		/// <param name="resetID">Setting to true: Create new ID for all nodes; Currently based on sGuid</param>
		/// <param name="resetName">Setting to true: Create new ID for all nodes; Currently based on the createNodeName delegate parameter</param>
		/// <param name="createNodeName">An Sdc.Util.CreateName delegate pointing to a function that can create a new @name property for each node</param>
		/// <param name="nodeWorkerFirst">A function pointer (delegate) to a method that can modify a BaseType node.<br/>  
		/// It will run before any other work on the vistied nodes, e.g., or changing @order or refreshing dictionaries.<br/> 
		/// Returns true for success, false for failure.
		/// </param>
		/// <param name="nodeWorkerLast">A function pointer (delegate) to a method that can modify a BaseType node.<br/>  
		/// It will run after all other work on the visted nodes.<br/> 
		/// Returns true for success, false for failure.
		/// </param>
		/// <returns>null if the method fails at any point;</returns>
		public static List<BaseType>? ReflectRefreshSubtreeList(BaseType startNode, 
			bool reOrder = false, 
			bool reRegisterNodes = false, 
			int startReorder = 0, 
			byte orderInterval = 1, 
			bool resetObjectGUID = false,
			bool resetObjectID = false,
			bool resetID = false,
			bool resetName = false,
			CreateName? createNodeName = null,
			Func<BaseType, bool>? nodeWorkerFirst = null, 
			Func<BaseType, bool>? nodeWorkerLast = null)
		{
			if (startNode is null) throw new InvalidOperationException("Parameter 'startNode' cannot be null");
			if(resetName is true && createNodeName is null) throw new InvalidOperationException("If resetName is true, then createNodeName cannot be null");
			
			var i = startReorder;
			var nodeList = new List<BaseType>();
			BaseType? par = startNode.ParentNode;

			//Process the root of the subtree
			NodeWorker(startNode, par);

			ReflectSubtree(startNode);

			void ReflectSubtree(BaseType par)
			{
				var kids = ReflectChildElements(par);
				if (kids is not null)
				{
					foreach (BaseType kid in kids)
					{
						NodeWorker(kid, par);						
						ReflectSubtree(kid);
					}
				}
			}
			return nodeList;

			//!______________________________________________________________________________

			void NodeWorker(BaseType n, BaseType? parentNode)
			{
				if(parentNode is null && n != startNode) 
					throw new InvalidOperationException("A null parentNode was passed to NodeWorker.  Only the startNode may have a null parentNode");

				var tn = (_ITopNode)n.TopNode!;

				if (nodeWorkerFirst is not null)
					if (nodeWorkerFirst(n) is false) throw new InvalidOperationException($"Method failed at nodeWorkerFirst(n), at sGuid {n.sGuid}");				

				//______________________________________________________________________________
				if (reRegisterNodes || resetObjectGUID)
					n.UnRegisterParent();

				//!Special actions:
				{
					if (resetObjectID) n.ObjectID = tn._MaxObjectIDint++;
					if (resetObjectGUID)
					{
						n.ObjectGUID = Guid.NewGuid();
						n.sGuid = ShortGuid.Encode(n.ObjectGUID);
					}
					if (n is IdentifiedExtensionType iet) iet.ID = n.sGuid;
					if (resetName && createNodeName is not null) n.name = createNodeName(node: n);

					if (reOrder)
					{
						n.order = i;
						i += orderInterval;
					}
				}
				if (reRegisterNodes || resetObjectGUID)
					if(parentNode is not null) n.RegisterParent(parentNode);
				//______________________________________________________________________________

				if (nodeWorkerLast is not null)
					if (nodeWorkerLast(n) is false) throw new InvalidOperationException($"Method failed at nodeWorkerLast(n), at sGuid {n.sGuid}");

				nodeList.Add(n);
			}
		}

		/// <summary>
		/// Includes the input node n.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="startReorder"></param>
		/// <param name="orderMultiplier"></param>
		/// <returns><see cref="Dictionary{Guid, BaseType}"/></returns>
		public static Dictionary<Guid, BaseType> GetSubtreeDictionary(BaseType n, int startReorder = -1, int orderMultiplier = 1)
		{
			//var nodes = n.TopNode.Nodes;
			//var topNode = Get_ITopNode(n);
			var cn = Get_ChildNodes(n);// topNode._ChildNodes;
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

				//shorter code:
				//List<BaseType>? childList = SortElementKids(n);
				//if (childList != null)
				//	foreach (var child in childList) MoveNext(child);

				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						SortElementKids(n, childList);
						foreach (var child in childList) MoveNext(child);
					}
				}
			}
			return dict;
		}

		/// <summary>
		/// Includes the input node n.  Creates new @order values for the subtree starting with node n.
		/// </summary>
		/// <param name="n">The root of the nodes to resort</param>
		/// <returns><see cref="List{BaseType}"/></returns>
		public static List<BaseType> GetSubtreeReOrderNodesList(BaseType n)
		{
			return GetSortedSubtreeList(n, 0, 1);
		}

		public static BaseType? GetNextElement(BaseType item)
		{
			if (item is null) return null;

			var firstKid = GetFirstChildElement(item);
			if (firstKid != null) return firstKid;

			var n = item;
			do
			{
				var nextSib = GetNextSibElement(n);
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
			nextNode = ReflectNextSibElement(item);
			if (nextNode is not null) return nextNode;
			if (item.ParentNode is null) return null;
			nextNode = ReflectNextSibElement(item.ParentNode);
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
				nextNode = ReflectNextSibElement(item);
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
		/// <param name="startSibNode">The node for which we want to find the next sibling node</param>
		/// <param name="parentNode">Only required if the ParentNodes dictionary has not been populated or is corrupted or stale</param>
		/// <returns>The ruturned node will be of type BaseType, or null if no next sibling node is found</returns>
		public static BaseType? ReflectNextSibElement(BaseType startSibNode, BaseType? parentNode = null)
		{
			parentNode ??= startSibNode.ParentNode;
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
									int i = IndexOf(ie, startSibNode!);
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
								else if (ReferenceEquals(o, startSibNode))
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

		public static BaseType? X_ReflectNextSibElement(BaseType n)
		{
			var par = n.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElements(par);
			var myIndex = lst?.IndexOf(n) ?? -1;
			if (myIndex < 0 || myIndex == lst?.Count - 1) return null;
			return lst?[myIndex + 1] ?? null;
		}



		public static BaseType? GetFirstSibElement(BaseType n)
		{
			var par = n.ParentNode;
			var topNode = Get_ITopNode(n);
			if (par is null) return null;
			topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is not null) SortElementKids(n, sibs);
			return sibs?[0];
		}
		public static BaseType? ReflectFirstSibElement(BaseType n)
		{
			var par = n.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElements(par);
			return lst.FirstOrDefault();
		}
		public static BaseType? GetNextSibElement(BaseType n)
		{
			var topNode = Get_ITopNode(n);
			var par = n.ParentNode;
			if (par is null) return null;
			topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is null) return null;
			SortElementKids(n, sibs);
			var index = sibs.IndexOf(n);
			if (index == sibs.Count - 1) return null; //item is the last item
			return sibs[index + 1];
		}
		public static BaseType? GetLastSibElement(BaseType n)
		{
			var par = n.ParentNode;
			var topNode = Get_ITopNode(n);
			if (par is null) return null;
			topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is not null) SortElementKids(n, sibs);
			return sibs?.Last();
		}
		public static BaseType? ReflectLastSibElement(BaseType n)
		{
			var par = n.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElements(par);
			return lst?.Last();
		}
		/// <summary>
		/// Retrieve the previous <see cref="BaseType"/> SDC element node using _TopNode dictionaries.<br/>
		/// This node may be a previous sibling, or a non-sibling node higher up in the SDC tree (closer to the SDC root node), under a different parent node.		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static BaseType? GetPrevElement(BaseType n)
		{
			if (n is null) return null;
			BaseType? par = n.ParentNode;
			BaseType? lastDesc;

			var prevSib = GetPrevSibElement(n);
			if (prevSib is not null)
			{
				lastDesc = GetLastDescendantElement(prevSib);
				if (lastDesc is not null) return lastDesc;
				return prevSib;
			}

			if (par is null) return null; //item is the top node

			return par;
		}
		/// <summary>
		/// Retrieve the previous <see cref="BaseType"/> SDC element node by reflection.<br/>
		/// This node may be a previous sibling, or a non-sibling node higher up in the SDC tree (closer to the SDC root node), under a different parent node.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static BaseType? ReflectPrevElement(BaseType n)
		{
			if (n is null) return null;
			BaseType? par = n.ParentNode;
			BaseType? lastDesc;

			var prevSib = ReflectPrevSibElement(n);
			if (prevSib is not null)
			{
				lastDesc = ReflectLastDescendantElement(prevSib);
				if (lastDesc is not null) return lastDesc;
				return prevSib;
			}

			if (par is null) return null; //item is the top node

			lastDesc = ReflectLastDescendantElement(par);
			if (lastDesc is not null) return lastDesc;

			return par;
		}
		/// <summary>
		/// Retrieve the previous <see cref="BaseType"/> sibling SDC element node, using the _ChildNodes dictionary.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static BaseType? GetPrevSibElement(BaseType n)
		{
			var par = n.ParentNode;
			var topNode = Get_ITopNode(n);
			if (par is null) return null;
			topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is null) return null;
			SortElementKids(n, sibs);

			var index = sibs?.IndexOf(n) ?? -1;
			if (index == 0) return null; //item is the first item
			return sibs?[index - 1];
		}
		/// <summary>
		/// Retrieve the previous <see cref="BaseType"/> sibling SDC element node by reflection from the parent node.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static BaseType? ReflectPrevSibElement(BaseType n)
		{
			var par = n.ParentNode;
			if (par is null) return null;

			var lst = ReflectChildElements(par);
			if (lst is null) return null;
			var myIndex = lst?.IndexOf(n) ?? -1;
			if (myIndex < 1) return null;
			return lst?[myIndex - 1];
		}

		public static BaseType? GetLastChildElement(BaseType n)
		{
			//var topNode = Get_ITopNode(n);
			//topNode._ChildNodes.TryGetValue(n.ObjectGUID, out List<BaseType>? kids);
			Get_ChildNodes(n).TryGetValue(n.ObjectGUID, out List<BaseType>? kids);
			if (kids is not null) SortElementKids(n, kids);
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
		public static BaseType? ReflectLastChildElement(BaseType parentNode)
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
				foreach (var pi in q)  //moves backwards thorugh the queue, since the last property with a value holds our desired object
				{
					object? o = pi.GetValue(parentNode);
					if (o is not null)
					{
						if (o is BaseType bt) return bt;
						if (o is IEnumerable<BaseType> ie && ie.Any()) return ie.Last();
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Given a <see cref="BaseType"/> item node, retrieve its first <see cref="BaseType"/> child note, if one is present.
		/// </summary>
		/// <param name="n">The node from which the first <see cref="BaseType"/>  child node will be retrieved.</param>
		/// <returns>The first <see cref="BaseType"/> child node</returns>
		public static BaseType? GetFirstChildElement(BaseType n)
		{
			var topNode = Get_ITopNode(n);
			topNode._ChildNodes.TryGetValue(n.ObjectGUID, out List<BaseType>? kids);

			if (kids is not null) SortElementKids(n, kids);
			return kids?[0];
		}
		/// <summary>
		/// Given a parent node, retrieve the list of child nodes, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns>BaseType? containing the first child node</returns>
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
		/// <summary>
		/// Given a <see cref="BaseType"/> <paramref name="n"/> object, retrieve a <see cref="ReadOnlyCollection&lt;BaseType>"/> of its child <see cref="BaseType"/>  nodes
		/// </summary>
		/// <param name="n">The node from which we want to retrieve its child nodes</param>
		/// <returns><see cref="ReadOnlyCollection&lt;BaseType>"/></returns>
		public static ReadOnlyCollection<BaseType>? GetChildElements(BaseType n)
		{
			var topNode = Get_ITopNode(n);
			topNode._ChildNodes.TryGetValue(n.ObjectGUID, out List<BaseType>? kids);
			if (kids is not null && kids.Count > 0) SortElementKids(n, kids);
			return kids?.AsReadOnly();
		}

		/// <summary>
		/// Given a parent node, retrieve the list of correctly-ordered child Xml Element nodes, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns>List&lt;BaseType>? containing the child nodes</returns>
		public static List<BaseType>? ReflectChildElements(BaseType parentNode)
		{
			if (parentNode is null) return null; //You can't have sibs without a parent
			List<BaseType>? childNodes = new();
			IEnumerable<PropertyInfo>? piIE = null;
			int nodeIndex = -1;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = parentNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);
			if (parentNode is QuestionItemType q && q.name == "Procedure") Debugger.Break();
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
			if (parentNode is QuestionItemType q1 && q1.name == "Procedure")
			{
				Debugger.Break();
				Console.WriteLine("SdcUtil.ReflectChild Elements");
				foreach (IdentifiedExtensionType n1 in childNodes) Console.WriteLine(n1.ElementPrefix + ": " + n1.As<DisplayedType>().title ?? "(null)" + "; ");
			}
			return childNodes;
		}

		/// <summary>
		/// Given a parent SDC <see cref="BaseType"/> node (<paramref name="n"/>), <br/>retrieve the list of correctly-ordered 
		/// child <see cref="XmlAttribute"/> nodes <br/>(i.e., those with properties decorated with <see cref="XmlAttribute"/>), if present.<br/>
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="getAllXmlAttributes">If true (the default), returns all attributes.    
		/// If false, returns only those attributes which have values that will be serialized</param>
		/// <param name="omitDefaultValues"> if <paramref name="getAllXmlAttributes"/> is false, 
		/// setting <b><paramref name="omitDefaultValues"/></b> to <b>false</b> will include those XmlAttribute properties that are set to their 
		/// default values, as designated in a <see cref="DefaultValueAttribute"/> attribute decorating the property accessor.
		/// </param>
		/// <param name="attributesToOmit">string array containing the names of SDC XML attributes to omit from the returned List</param>
		/// <param name="attributesToInclude">string array containing the names of SDC XML attributes to include in the returned List</param>
		/// <returns> <see cref="List{AttributeInfo}"/> containing the child nodes</returns>
		public static List<AttributeInfo> ReflectChildXmlAttributes(BaseType n
			, bool getAllXmlAttributes = true
			, bool omitDefaultValues = true
			, string[]? attributesToOmit = null
			, string[]? attributesToInclude = null)
		{
			if (n is null) throw new NullReferenceException("n cannot be null"); //You can't have sibs without a parent

			List<AttributeInfo> attributes = new();
			IEnumerable<PropertyInfo>? piIE = null;
			int nodeIndex = -1;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = n.GetType();
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
						.Where(pi => pi.GetCustomAttributes<XmlAttributeAttribute>().Any());
				foreach (var p in piIE)
				{
					nodeIndex++;
					if (getAllXmlAttributes) AddAttribute();
					else
					{
						var sspn = t.GetMethod("ShouldSerialize" + p.Name)?.Invoke(n, null);
						//if (sspn is null) Debugger.Break();
						var pVal = p.GetValue(n);
						var attDefVal = GetAttributeDefaultValue(p);

						//if (p.Name == "minCard") Debugger.Break();  //&& (pVal?.Equals(0)??false)
						if (pVal is not null) //if pVal is null, there is nothing to serialize
						{
							if (attributesToOmit is not null && attributesToOmit.Contains(p.Name)) continue;
							if (attributesToInclude is not null && !attributesToInclude.Contains(p.Name)) continue;
							bool pValIsAttributeDefault = false;

							if (omitDefaultValues)  //The XML serializer emits all properties set to the Value of the DefaultValueAttribute, assuming DefaultValueAttribute decorates the property
							{
								if (attDefVal is not null)
									if (attDefVal.Equals(pVal))
										pValIsAttributeDefault = true;  //true prevents serialization of this property
							}

							if (sspn is bool shouldSerialize && shouldSerialize) //if(_shouldSerializePropertyName is true);	
							{

								if (!pValIsAttributeDefault)  //Make sure the property does not hold a default value (based on DefaultValueAttribute's Value property)
									AddAttribute();
							}
							else if (sspn is null)  // ShouldSerializePropertyName idoes not exist for property p.  This can occur for properties like byte[], HTML/XML types, etc.
							{

								if (attDefVal is not null) //Test if the property's DefaultValueAttribute (it's unlikely if this is present) value does not match the current property value,
								{
									if (!pValIsAttributeDefault)
										AddAttribute();
								}
								else //There was no DefaultValueAttribute found (i.e., attDefVal is null and thus pValIsAttributeDefault is false),
									 //so now we see if we have a non-default (e.g., non-null for reference types) property value (obtained from GetTypeDefaultValue) for its datatype.
								{
									var typeDefaultVal = GetTypeDefaultValue(pVal.GetType());
									if (!pVal.Equals(typeDefaultVal))
										AddAttribute();
								}
							}
						}
					}
					void AddAttribute()
					{
						nodeIndex++;
						attributes.Add(FillAttributeInfo(p, n));
					}
				}
			}
			return attributes;

			AttributeInfo FillAttributeInfo(PropertyInfo p, BaseType elementNode) =>
				new(elementNode, elementNode.sGuid, p!.GetValue(elementNode), p, nodeIndex);

		}



		/// <summary>
		/// Retrieve the SDC default value (if one exists) for the SDC XML attribute identified by <paramref name="pi"/>.  
		/// The SDC default value is obtained from the <see cref="DefaultValueAttribute"/> that may be present on <paramref name="pi"/>.
		/// </summary>
		/// <param name="pi">The <see cref="PropertyInfo"/> object that represents the SDC XML attribute of interest</param>
		/// <returns>Nullable <see cref="object"/> containing the attribute's default value, or null if no default value exists</returns>
		public static object? GetAttributeDefaultValue(PropertyInfo pi)
		{
			if (pi is null) throw new InvalidDataException("The attributeName parameter was not found on a property if this object");
			var defVal = pi.GetCustomAttributes<DefaultValueAttribute>()?.FirstOrDefault()?.Value;
			return defVal;
		}

		/// <summary>
		/// Given a <paramref name="propertyName"/>, retrieve the property object, if it exists, from the <paramref name="parent"/> object
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static object? GetPropertyObject(BaseType parent, string propertyName)
		{
			var pi = parent.GetType().GetProperty(propertyName);
			if (pi is null) return null;
			return pi.GetValue(parent);
		}

		/// <summary>
		/// Get the default value of a type retrieved at runtime
		/// </summary>
		/// <see cref="GetTypeDefaultValue" href="https://stackoverflow.com/questions/1281161/how-to-get-the-default-value-of-a-type-if-the-type-is-only-known-as-system-type"/>
		/// <param name="type"></param>
		/// <returns>Nullable object?, set to its default value</returns>
		public static object? GetTypeDefaultValue(Type type) =>
			type.IsValueType ? Activator.CreateInstance(type) : null;

		public static bool TryGetChildElements(BaseType n, out ReadOnlyCollection<BaseType>? kids)
		{
			var topNode = Get_ITopNode(n);
			bool tfResult = topNode._ChildNodes.TryGetValue(n.ObjectGUID, out List<BaseType>? kidsOut);
			kids = kidsOut?.AsReadOnly();
			return tfResult;
			//if (kidsOut is null || !kidsOut.Any()) return false;
			//return true;
		}


		public static BaseType? GetLastDescendantElement(BaseType n, BaseType? stopNode = null)
		{
			BaseType? node = n;
			while (node is not null)
			{
				var topNode = Get_ITopNode(node);
				topNode._ChildNodes.TryGetValue(node.ObjectGUID, out List<BaseType>? kids);
				if (kids is not null && kids.Count > 0)
				{
					SortElementKids(node, kids);
					//option to abort search just before stopNode: check for stopNode in sibling list.
					if (stopNode is not null)
					{
						var snIndex = kids?.IndexOf(stopNode) ?? -1;
						if (snIndex > 0) return kids?[snIndex - 1];
					}
					node = kids?.Last();
				}
				else return node;
			}
			return node;
		}

		public static BaseType? GetLastDescendantElementSimple(BaseType n)
		{
			var topNode = Get_ITopNode(n);
			//if (topNode is null) return null;
			var cn = topNode._ChildNodes;

			BaseType? lastNode = null;
			//bool doSibs = false;

			MoveNext(n);

			void MoveNext(BaseType n)
			{
				if (cn.TryGetValue(n.ObjectGUID, out List<BaseType>? childList))
				{
					if (childList != null)
					{
						SortElementKids(n, childList);
						lastNode = childList.Last();
						MoveNext(lastNode);
					}
				}
			}
			return lastNode;
		}


		public static BaseType? ReflectLastDescendantElement(BaseType bt, BaseType? stopNode)
		{
			if (bt is null) return null;
			BaseType? lastKid = null;

			FindLastKid(bt);
			//!+-------Local Method--------------------------
			void FindLastKid(BaseType bt)
			{
				List<BaseType>? kids = ReflectChildElements(bt);
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
		public static BaseType? ReflectLastDescendantElement(BaseType bt)
		{
			if (bt is null) return null;
			BaseType? lastKid = null;

			FindLastKid(bt);

			void FindLastKid(BaseType bt)
			{
				var testLast = ReflectLastChildElement(bt);
				if (testLast is null) return; //we ran out of kids to check, so lastKid is the last descendant                

				lastKid = testLast;
				FindLastKid(lastKid);
			}
			return lastKid;
		}

		private static int GetMaxOrderFromXmlElementAttributes(BaseType item)
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

		public static PropertyInfoMetadata GetElementPropertyInfoMeta(BaseType item, bool getNames = true)
		{
			PropertyInfo? pi = GetElementPropertyInfo(
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
		private static PropertyInfo? GetElementPropertyInfo(
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
		private static int GetElementItemIndex(BaseType item, out IEnumerable<BaseType>? ieItems, out PropertyInfo? piItemOut, out string errorMsg)
		=> GetElementItemIndex(item, null, out ieItems, out piItemOut, out errorMsg);

		private static int GetElementItemIndex(BaseType item, IEnumerable<PropertyInfo>? ieParProps, out IEnumerable<BaseType>? ieItems, out PropertyInfo? piItemOut, out string errorMsg)
		{
			errorMsg = "";
			ieItems = null;
			piItemOut = null;
			BaseType? par = item.ParentNode;

			if (par is null)
			{ errorMsg = $"{nameof(GetElementItemIndex)}: the ParentNode of n cannot be null"; return -1; }
			if (ieParProps is null)
			{
				ieParProps = par.GetType().GetProperties()
						.Where(p => typeof(IEnumerable<BaseType>)
						.IsAssignableFrom(p.PropertyType)
						&& p.GetCustomAttributes(typeof(XmlElementAttribute)).Any()  //We must confirm that our IEnumerable has a XmlElementAttribute,
																					 //since we added some shadow properties in the partial classes
																					 //like "ChildItems_List" for "Items"
																					 //&& p.GetValue(par) is not null			
																					 //This may be good for a future refactoring of the lambda expression; it will get the matched property directly and concisely.
						);

				if (ieParProps is null || !ieParProps.Any())
				{ errorMsg = $"{nameof(GetElementItemIndex)}: the ParentNode of n does not contain an IEnumerable<BaseType> that contains the the target n n"; return -1; }
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

		private static string ReflectSdcElement(BaseType item, out IEnumerable<BaseType>? ieItems, out int xmlOrder, out int maxXmlOrder, out int itemIndex, out PropertyInfo piItem, out string? errorMsg)
		{
			string? elementName = ReflectSdcElement(null, item, out ieItems, out xmlOrder, out maxXmlOrder, out itemIndex, out piItem, out errorMsg);
			return elementName;


		}

		/// <summary>
		/// If <paramref name="piItem"/> is passed as null, the method will retrieve it from <paramref name="item"/>
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
		private static string ReflectSdcElement(PropertyInfo? piItem, BaseType item, out IEnumerable<BaseType>? ieItems, out int xmlOrder, out int maxXmlOrder, out int itemIndex, out PropertyInfo? piItemOut, out string? errorMsg)
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
					throw new InvalidOperationException($"{nameof(ReflectSdcElement)} could not find a name for the n parameter.  This may occur if the n node has no parent object");
				return xmlElementName;
			}
					
			if (piItem is null) //piItem was not supplied, so let's try to find it.
			{
				parType = par.GetType();
				PropertyInfo[] parProps = parType.GetProperties();
				//Look for a direct item-to-property match, so we can assign propName from the par object		
				piItemOut = parProps
					.Where(pi => pi.GetCustomAttributes(typeof(XmlElementAttribute)).Any()  //all serialized properties must have the XmlElementAttribute attribute
					&& ! typeof(IEnumerable<BaseType>).IsAssignableFrom(pi.PropertyType)     //the property is not an IEnumerable (i.e., an Array, List etc.)
					&& ReferenceEquals(pi?.GetValue(par), item))?.FirstOrDefault();          //There can be, at most, one match to our item object

				if (piItemOut is not null) 
					piItem = piItemOut; // piItem is not an IEnumerable here

				else if (piItem is null) // piItem is still null; let's try again, in IEnumerable properties
				{
					//Now we look in IEnumerable properties, to see if "item" is contained inside it.
					//Let's see if our item object lives in an IEnumerable<BaseClassSubtype> 
					itemIndex = GetElementItemIndex(item, out ieItems, out piItemOut, out errorMsg);
					// piItemOut will be null if item is not an IEnumerable, we only want to use it if piItemOut is null.
					if (piItemOut is null)
						throw new NullReferenceException($"{nameof(ReflectSdcElement)} could not obtain a PropertyInfo object from the supplied node parameter. \r\n" + errorMsg);
					else
						piItem = piItemOut; // piItem is an IEnumerable here
				}
			}


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

			throw new InvalidOperationException("Could not find a name for the n parameter.");

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

			var enumObj = item.ParentNode?.GetType()?.GetProperty(enumName)?.GetValue(item.ParentNode);
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
			if (Get_Nodes(item)[newParent.ObjectGUID] is null) return false;
			if (newParent.IsDescendantOf(item)) return false;

			Type itemType = item.GetType();
			var thisPi = SdcUtil.GetElementPropertyInfoMeta(item);
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
				//TODO: Also try matching element names found in the ItemChoiceType enums.  This is more reliable that using data types.

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
		/// Reflect the object tree to determine if the supplied <paramref name="item"/> item node can be attached to new parent node,
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
			if (itemName is null || itemName.IsNullOrWhitespace()) itemName = item.GetPropertyInfoMetaData().XmlElementName;

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
		#region TE Helpers
		//!+There probably is no need for CreateNewId, since the SDC OM assigns new IDs upon object creation, using the sGuid property
		public static string CreateNewId()
		{
			return Guid.NewGuid().ToString();
		}

		public static ItemTypeEnum GetItemType(IdentifiedExtensionType node)
		{
			//if (typeof(SectionItemType).IsInstanceOfType(node))
			if (node is SectionItemType) return ItemTypeEnum.Section;

			//if (typeof(QuestionItemType).IsInstanceOfType(node))
			if (node is QuestionItemType q)
			{
				if (q.ListField_Item != null) return q.ListField_Item.maxSelections == 0 ? ItemTypeEnum.QuestionMultiple : ItemTypeEnum.QuestionSingle;
				if (q.ResponseField_Item != null) return ItemTypeEnum.QuestionResponse;
				return ItemTypeEnum.QuestionRaw;
			}

			//if (typeof(ListItemType).IsInstanceOfType(node))
			if (node is ListItemType li)
				return li.ShouldSerializeListItemResponseField() ? ItemTypeEnum.ListItemResponse : ItemTypeEnum.ListItem;

			//if (typeof(DisplayedType).IsInstanceOfType(node))
			if (node is DisplayedType) return ItemTypeEnum.DisplayedItem;
			if(node is InjectFormType) return ItemTypeEnum.InjectForm;
			if (node is ButtonItemType) return ItemTypeEnum.Button;

			return ItemTypeEnum.None;
		}
		#endregion
		#region Helpers

		/// <summary>
		/// This is a method for creating a new <see cref="BaseType.name"/> value for the designated <paramref name="node"/>.<br/>
		/// For each SDC node, the CAP Ckey-formatted ID of the closest <see cref="IdentifiedExtensionType"/> ancestor is used to create a "nameBody" <br/>
		/// for all of its non-<see cref="IdentifiedExtensionType"/> child elements.  <br/>
		/// This short id is used in conjunction with other node properties of the object <br/>
		/// (e.g., prefix, propName (for Properties) etc.) to create a unique @name attribute <br/>
		/// for every SDC node (and thus each serialized XML element).
		/// If a Ckey-formatted ID is not present, the sGuid or a new short Guid for the <see cref="IdentifiedExtensionType"/> ancestor is truncated and used instead.
		/// If the input node name begins with "_", the method will bypass new name creation and just return teh existting node.name with no modifications
		/// </summary>
		/// <returns>The new name that was used to refresh <see cref="BaseType.name"/> on <paramref name="node">.</paramref></returns>
		public static string CreateElementNameCAP(this BaseType node)
		{
			string namePrefix;
			string nameBody = "";
			string nameSuffix;

			if (node.name?.Length > 0 && node.name.AsSpan(0, 1) == "_") return node.name;  //Return the existing name
			//+namePrefix
			{
				//+Question Prefix
				if (node is QuestionItemType Q) 
				{
					var st = Q.GetQuestionSubtype();
					switch (st)
					{
						case QuestionEnum.QuestionRaw:
							namePrefix = "Q";
							break;
						case QuestionEnum.QuestionSingle:
							namePrefix = "QS";
							break;
						case QuestionEnum.QuestionMultiple:
							namePrefix = "QM";
							break;
						case QuestionEnum.QuestionSingleOrMultiple:
							namePrefix = "QSM";
							break;
						case QuestionEnum.QuestionFill:
							namePrefix = "QR";
							break;
						case QuestionEnum.QuestionLookup:
							namePrefix = "QL";
							break;
						case QuestionEnum.QuestionLookupSingle:
							namePrefix = "QLS";
							break;
						case QuestionEnum.QuestionLookupMultiple:
							namePrefix = "QLM";
							break;
						case QuestionEnum.QuestionGroup:
							throw new NotImplementedException("Could not determine Question Subtype (QuestionEnum.QuestionGroup)");
						default:
							throw new NotImplementedException("Could not determine Question Subtype");
					}
				}
				else //+Property Prefix
				if (node is PropertyType pt && pt.propName is not null) 
					namePrefix = pt.ElementPrefix ?? "prop" + "_" + pt.propName.AsSpan(0, 6).ToString() + "_";				
				else //+Other Prefix
				{
					namePrefix = node.ElementPrefix ??
						node.ElementName.TakeWhile(c => Char.IsUpper(c)).ToString()?.ToLower() ?? ""; //backup method for ElementPrefix: use uppercase letters in ElementName
				}
			}
			
			//+nameBody
			{
				//Try using the closest IdentifiedExtensionType node's Ckey-formatted (decimal format) ID to generate nameBody
				//Use special names for headr/body/footer nodes
				//use an sGuid if the Ckey/ID approach does not work.
				string nameSpace = ".100004300";
				string tempsGuid6;

				if (node is IdentifiedExtensionType iet)
				{
					if (iet.ID.Contains(nameSpace) && iet.ID.Length < 10)
						nameBody = "_" + Regex.Replace(iet.ID.Replace(nameSpace, "") ?? "", @"\W+", ""); //remove namespace and special characters
					else nameBody = "_" + MakeNameBodyFromsGuid();

					if (iet.name?.ToLower() == "body") nameBody = "_body" + nameBody;
					else if (iet.name?.ToLower() == "footer") nameBody = "_footer" + nameBody;
					else if (iet.name?.ToLower() == "header") nameBody = "_header" + nameBody;
				}
				else //not IdentifiedExtensionType
				{
					IdentifiedExtensionType? ancestorIet = node.ParentIETypeNode;
					if (ancestorIet is not null)
						if (ancestorIet?.ID?.Contains(nameSpace) ?? false)
							nameBody = "_" + Regex.Replace(
								node.ParentIETypeNode?.ID.Replace(nameSpace, "") ?? "", @"\W+", ""); //remove namespace and special characters
						else nameBody = "_" + MakeNameBodyFromsGuid();
				}

				string MakeNameBodyFromsGuid()
				{
					//+sGuid
					{
					//If we can't get a nicely formatted CAP Ckey ID from the closest IET node, we can process the sGuid, or a use new short guid instead.
					//We will grab teh first six characters from the sGuid or new short guid to use in nameBody if needed.
					//For nameBody; use sGuid from the ancestor IET node, it it exists
					//In this way, all IET descendants (until another IET node is hit) will share the nameBody part of the name
						BaseType sGuidNode;
						if (node is IdentifiedExtensionType) sGuidNode = node;
						else if (node.ParentIETypeNode is not null) sGuidNode = node.ParentIETypeNode;
						else sGuidNode = node;

						if (sGuidNode.sGuid.IsNullOrWhitespace())
							tempsGuid6 = ShortGuid.Encode(new Guid()).AsSpan(0, 6).ToString();
						else tempsGuid6 = sGuidNode.sGuid.AsSpan(0, 6).ToString();
					}
					return tempsGuid6;
				}
			}
			//+nameSuffix
			{
				if (node is IdentifiedExtensionType)
					nameSuffix = "";
				else
				{
					try
					{ nameSuffix = "_" + node.SubIETcounter.ToString(); }
					catch
					{ nameSuffix = ""; }
				}
			}
			//+Append "_" to namePrefix, if needed
			if (namePrefix.Length > 0 &&
			(nameBody.Length > 0 || nameSuffix.Length > 0)) namePrefix += "_";

			return namePrefix + nameBody + nameSuffix;
			}

		//TODO: names prefixed with "_" will be preserved.
		//TODO: names and IDs will be added to internal dictionaries to ensure uniqueness within a template.
		//TODO: create a default naming system that uses the first 6 good characters of the sGuid instead of the ID, for the "ID part" of the name.
		//			skip: starting numbers, -, _, 0 and move on to the next letter; then change to all lower case.
		//TODO: check for unacceptable words in sGuids and names; this provides almost 2 billion choices for each template 

		/// <summary>
		/// Process the characters in a node's short Guid (sGuid) to create a alphanumeric string suiatable for use 
		/// as a programming variable name, or for part of such a name, or for us as part/all of an <see cref="IdentifiedExtensionType.ID"/>.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="nameBaseLength">The length of the alphanumeric string to return.
		/// In some cases, the string may be shorter than this length, due to removal of illegal characters (0, -, and _), 
		/// as well as any numbers at the first character of the string.</param>
		/// <param name="allLowerCase">If set to true, the method returns a lower case alphanumeric string.  
		/// If false (the default), the method returns an alphanumeric string not converted to lower case</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static string CreateNameBaseFromsGuid(BaseType n, int nameBaseLength = 6, bool allLowerCase = false)
		{
			if (nameBaseLength > 20 || nameBaseLength < 1) throw new ArgumentException("nameBaseLength must be > 0 and < 21");
			string sg = new(n.sGuid);
			Regex pattern = new("^[a-zA-Z0-9-_]{22}");

			if (!pattern.IsMatch(sg))
				if (sg.IsNullOrWhitespace() || sg.Length != 22 || !pattern.IsMatch(sg)) throw new ArgumentException("The supplied n does not have a valid sGuid");
			var sgl = sg.ToList();
			int i = -1;
			do
			{ //remove any integer, -, or _ in the first position, as these are illegal for variable names
				i++;
				char c = sgl[0];
				if ((c >= '0' && c <= '9') || c == '_' || c == '-')
					sgl.RemoveAt(0);
				else break;
			} while (i < sgl.Count - 1);

			i = 0;
			do
			{ //remove any 0, -, or _ in any remaining position, as these do not make nice variable names
				char c2 = sgl[i];
				if (c2 == '0' || c2 == '_' || c2 == '-')
					sgl.RemoveAt(i);
				else i++;

			} while (i <= nameBaseLength && i < sgl.Count);

			var sb = new StringBuilder();
			foreach (var c in sgl.Take(nameBaseLength)) sb.Append(c);
			if (allLowerCase) return sb.ToString().ToLower();

			return sb.ToString();


		}
		/// <summary>
		/// Given a parent SDC node, this method will sort the child nodes (kids)
		/// This method is used to keep lists of sibling nodes in the same order as the SDC object tree
		/// </summary>
		/// <param name="parentItem">The parent SDC node</param>
		/// <param name="kids">"kids" is a List&lt;BaseType> containing all the child nodes under parentItem.
		/// This is generally obtained from the parentItem using the _ITopNode._ChildNodes Dictionary object
		/// If it is not supplied, it will be obtained below from parentItem</param>
		/// <returns>List&lt;BaseType>? containing ordered list of child nodes, or null if no child nodes are present</returns>
		private static List<BaseType>? SortElementKids(BaseType parentItem, List<BaseType>? kids = null)
		{
			//Sorting uses reflection, and this is an expensive operation, so we only sort once per parent node
			//TreeSort_NodeIds is a SortedSet that holds the ObjectIDs of parent nodes whose children have already been sorted.
			//If a parent node's children have already been sorted, it appears in TreeSort_NodeIds, and we can skip sorting it again. 

			//(This method is NOT used by IMoveRemoveExtensions.RegisterParentNode, which uses the reflection-based TreeSibComparer directly for child nodes - 
			//This ensures that the node dictionaries (_Nodes, _ParentNodes and _ChildNodes) are kept sorted in the same order as they will be serialized in XML.)

			if (kids is null || kids.Count == 0)
				//if (!((_ITopNode)parentItem.TopNode)._ChildNodes.TryGetValue(parentItem.ObjectGUID, out kids) && kids?.Count > 0) return null;
				//if (!(Get_ITopNode(parentItem))._ChildNodes.TryGetValue(parentItem.ObjectGUID, out kids) && kids?.Count > 0) return null;
				if (!Get_ChildNodes(parentItem).TryGetValue(parentItem.ObjectGUID, out kids) && kids?.Count > 0) return null;

			if (!TreeSort_NodeIds.Contains(parentItem.ObjectID) && kids is not null)
			{
				kids.Sort(new TreeSibComparer());
				TreeSort_NodeIds.Add(parentItem.ObjectID);
			}
			return kids;
		}

		/// <summary>
		/// Returns formatted XML a minified or poorly formatted XML string
		/// </summary>
		/// <param name="Xml">The input XML to be formatted</param>
		/// <returns></returns>
		public static string XmlFormat(string Xml)
		{
			return System.Xml.Linq.XDocument.Parse(Xml).ToString();  //prettify the minified doc XML 
		}

		/// <summary>
		/// Write a new order attribute and value into every element of an Xml file
		/// </summary>
		/// <param name="Xml">The input XML, to which the @order attributes will be written</param>
		/// <param name="orderMultiplier">if an order interval greater than 1 is desired, enter the interval</param>
		/// <returns>XMl with populated @order attributes</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static string XmlReorder(string Xml, int orderMultiplier = 1)
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
					node.Attributes["order"]!.Value = (j * orderMultiplier).ToString();
					j++;
				}
			}
			return doc.OuterXml;
		}

		/// <summary>
		/// For the current <paramref name="n"/>, retrieve the ancestor _ITopNode object that contains the subtree dictionaries<br/>
		/// e.g., Nodes, ChildNodes, IETNodes. <br/>
		/// If this node (<paramref name="n"/>) implements <see cref="_ITopNode"/>, the method returns (_ITopNode)n
		/// </summary>
		/// <param name="n">The node for which we need to retrieve _ITopNode dictionaries</param>
		/// <returns>A reference to an _ITopNode object</returns>
		internal static _ITopNode Get_ITopNode(BaseType n)
		{
			if (n is _ITopNode itn) return itn;
			return (_ITopNode)n.TopNode;
		}

		#endregion
		#region Retired
		private static List<BaseType>? X_ReflectChildList(BaseType bt)
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
			SortElementKids(bt, kids);
			return kids;
		}

		private static BaseType? X_ReflectFirstChild(BaseType item)
		{
			if (item is null) return null;

			var lst = ReflectChildElements(item);
			return lst?.FirstOrDefault();
		}
		private static BaseType? X_ReflectLastChild(BaseType item)
		{
			if (item is null) return null;

			var lst = ReflectChildElements(item);
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

		private static List<T> X_GetStatedListParent<T>(T item)
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
					throw new ArgumentException("Unknown input n:" + item.ElementName ?? "\"\"");
			}

			return null;
		}

		private static bool X_IsItemChangeAllowed<S, T>(S source, T target)
			where S : notnull, IdentifiedExtensionType
			where T : notnull, IdentifiedExtensionType
		{
			ChildItemsType? ci;
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
					ci = source as ChildItemsType;
					switch (target)
					{
						case QuestionItemType _:
							//probably should not allow changing Q types
							return false;
						default: return false;
					}

				case ListItemType _:
					ci = source as ChildItemsType;
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
			var pi = SdcUtil.GetElementPropertyInfoMeta(bt);
			bt.ElementName = pi.XmlElementName ?? "";
			bt.ElementOrder = pi.XmlOrder;
		}
		private static object? X_GetPropertyObject(BaseType parent, PropertyInfo piProperty) =>
			piProperty.GetValue(parent);

		#endregion

	}
}
