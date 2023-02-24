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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.IsolatedStorage;
using System.Numerics;
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
using static System.Formats.Asn1.AsnWriter;


namespace SDC.Schema
{
	/// <summary>
	/// This class is primarily used as source material for creating consistent type and interface-specific SDC extension methods
	/// </summary>
	public static class SdcUtil
	{
		#region Local
		internal static Dictionary<Guid, BaseType> Get_Nodes(BaseType n)
		{ return Get_ITopNode(n)._Nodes; }
		internal static Dictionary<Guid, List<BaseType>> Get_ChildNodes(BaseType n)
		{ return Get_ITopNode(n)._ChildNodes; }
		internal static Dictionary<Guid, BaseType> Get_ParentNodes(BaseType n)
		{ return Get_ITopNode(n)._ParentNodes; }
		internal static ObservableCollection<IdentifiedExtensionType> Get_IETnodes(BaseType n)
		{ return Get_ITopNode(n)._IETnodes; }
		private static HashSet<string> xUniqueBaseNames = new(); //key is BaseName, value is sGuid; ensure that all BaseNames are unique


		/// <summary>
		/// This SortedSet contains the ObjectID of each node that has been sorted by ITreeSibComparer.  
		/// Each entry in this HashSet indicates that nodes child nodes have already been sorted.  
		/// Checking for a parent node in this HashSet is used to bypass the resorting of child nodes during a tree sorting operation.  
		/// The SortedList is cleared after the conclusion of the sorting operation, using TreeSort_ClearNodeIds().
		/// </summary>
		//private static readonly HashSet<int> TreeSort_NodeIds = new();

		/// <summary>
		/// List-sorting code can test for the presence of a flagged parent node in TreeSort_NodeIds with TreeSort_IsSorted. 
		/// If TreeSort_IsSorted returns true,
		/// then the child-list-sorting code should use the ChildNodes dictionary to retrieve the sorted child nodes.  If it returns false, 
		/// the code should use ITreeSibComparer to sort the child nodes (in a List&lt;BaseType>) before using accessing the nodes in sorted order.
		/// </summary>
		/// <param name="parentItem"></param>
		/// <returns></returns>
		private static bool TreeSort_IsSorted(BaseType parentItem)
		{
			var _topNode = Get_ITopNode(parentItem);
			if (_topNode is null) throw new NullReferenceException($"{nameof(_topNode)} cannot be null");
			if (_topNode._TreeSort_NodeIds.Contains(parentItem.ObjectID)) return true;
			return false;
		}

		private static void TreeSort_Add(BaseType parentItem)
		{
			var _topNode = Get_ITopNode(parentItem);
			if (_topNode is null) throw new NullReferenceException($"{nameof(_topNode)} cannot be null");
			_topNode._TreeSort_NodeIds.Add(parentItem.ObjectID);
		}

		/// <summary>
		/// Dictionary to cache PropertyInfo objects to speed reflection of SDC Element nodes
		/// </summary>
		private static readonly Dictionary<Type, IEnumerable<PropertyInfo>?> dListPropInfoElements = new();
		/// <summary>
		/// Dictionary to cache PropertyInfo objects to speed reflection of SDC Attribute nodes
		/// </summary>
		private static readonly Dictionary<Type, IEnumerable<PropertyInfo>?> dListPropInfoAttributes = new();



		///// <summary>
		///// Clear the dListPropInfo Dictionary, which hold cached PropertyInfo objects that are used to speed up SDC node reflection.<br/>
		///// Used for performance testing.
		///// </summary>
		//internal static void CleardPropInfoDictionary()
		//{ dListPropInfo.Clear(); }

		/// <summary>
		/// Cache XmlRootAttribute objects
		/// </summary>
		private static readonly Dictionary<Type, List<XmlRootAttribute>?> dXmlRootAtts = new();
		/// <summary>
		/// Cache XmlElementAttribute objects
		/// </summary>
		private static readonly Dictionary<Type, List<XmlElementAttribute>?> dXmlElementAtts = new();
		/// <summary>
		/// Cache XmlChoiceIdentifierAttribute objects
		/// </summary>
		private static readonly Dictionary<Type, List<XmlChoiceIdentifierAttribute>?> dXmlChoiceIdentifierAtts = new();
		/// <summary>
		/// Cache XmlAttributeAttribute objects
		/// </summary>
		private static readonly Dictionary<Type, List<XmlAttributeAttribute>?> dXmlAttAtts = new();
		/// <summary>
		/// Cache XmlAttributeAttribute objects
		/// </summary>
		private static readonly Dictionary<Type, List<AttributeInfo>?> dListAttInfo = new();



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
		public static void TreeSort_ClearNodeIds(BaseType n)
		{//TODO: Make this internal  (or Friend) as it's only used for testing
			var _topNode = Get_ITopNode(n);
			if (_topNode is null) throw new NullReferenceException($"{nameof(_topNode)} cannot be null");
			_topNode._TreeSort_NodeIds.Clear();
		}

		/// <summary>
		/// Walk up parent nodes to find the first ITopNode node.
		/// Returns the current node if it is ITopNode.
		/// Returns null if an ITopNode node is not found.
		/// </summary>
		/// <param name="node"></param>
		/// <returns>ITopNode? node</returns>
		public static ITopNode? FindTopNode(BaseType node)
		{
			do
			{
				var par = node.ParentNode;
				if (par is null) return null;
				node = par;
				if (node is ITopNode itn) return itn;

			} while (true);

		}
		#endregion



		#region Delegates


		/// <summary>
		/// Delegate that points to a single method for creating a new <see cref="BaseType.name"/> value for the designated <paramref name="node"/>.<br/>
		/// The caller can supply a method for generating the @name property of each SDC node.  <br/>
		/// The following methods are available:
		/// <br/>
		/// <br/><see cref="SdcUtil.CreateCAPname"/><br/>
		/// A method that generates a CKey/ID aware name for CAP use.  Names reference a parent <see cref="IdentifiedExtensionType"/> (IET) node when applicable.<br/>
		/// <br/>
		/// <see cref="SdcUtil.CreateSimpleName"/><br/>
		/// A generic name generation method that uses the sGuid of each node.<br/><br/>
		/// Users may create their own method that returns a name value and matches the <see cref="SdcUtil.CreateName"/> delegate.
		/// </summary>
		/// <param name="node">The SDC node for which a name will be generated. </param>
		/// <param name="initialTextToSkip">If an existing name value starts with this string, the existing name will be reused, and will not be replaced with a new value.</param>
		/// <param name="changeType">Enum value contolling how new names are assigned. See <see cref="NameChangeEnum"/> for values.</param>		
		/// <returns>The new name that will be used to refresh <see cref="BaseType.name"/> on <paramref name="node">.</paramref></returns>
		public delegate string CreateName(BaseType node, string initialTextToSkip = "", NameChangeEnum changeType = NameChangeEnum.Normal);
		//public delegate string CreateName(BaseType node, string initialStringToSkip = "");

		/// <summary>
		/// Reserved for future use. <paramref name=""/>
		/// </summary>
		/// <param name="node"></param>
		/// <param name="icon"></param>
		/// <param name="html"></param>
		/// <returns></returns>
		public delegate string NodeAnnotation(BaseType node, byte[] icon, string html);

		#endregion

		#region Navigation


		/// <summary>
		/// If <paramref name="refreshTree"/> is true (default),
		///	this method uses reflection to refresh: <br/><br/>
		///		_TTopNode._Nodes, _ITopNode_ParentNodes and _ITopNode_ChildNodes dictionaries, with all nodes in the proper order.<br/>
		///		Some BaseType properties are updated: <br/>
		///		SGuid properties are created if missing, and name and order properties are created/updated as needed.  <br/>
		///		@name properties will be overwritten with new @name values that may not match the original.<br/><br/>
		/// If <paramref name="refreshTree"/> is false, this method returns an ordered List&lt;BaseType>, <br/><br/>
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
		/// <param name="createNodeName">A method that will create a <see cref="BaseType.name"/> for each refreshed node in the ITopNode tree.</param>
		/// <returns>List&lt;BaseType> containing all of the SDC tree nodes in sorted top-bottom order</returns>
		/// <param name="orderStart">The starting number for the @order attribute</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/>
		/// A value of 0 will prevent @order from being generated and serialized.</param>		
		public static List<BaseType> ReflectRefreshTree(ITopNode topNode
			, out string? treeText
			, bool print = false
			, bool refreshTree = true
			, CreateName? createNodeName = null
			, int orderStart = 0
			, int orderGap = 10)

		{
			TreeSort_ClearNodeIds((BaseType)topNode);
			int counter = 0;
			int indent = 0;
			int order = orderStart;
			List<BaseType> SortedNodes = new();  //this will be the returned object from this method

			var sbTreeText = new StringBuilder();
			var newPropsText = new StringBuilder();
			//If the initial topNode has subsumed other ITopNode subtrees,
			//currentTopNode will track the current subtree ITopNode 
			var current_ITopNode = (_ITopNode)topNode;
			BaseType btNode = (BaseType)topNode;

			if (refreshTree)
			{
				if (orderGap == 0)
				{
					btNode.order = default;
					btNode._shouldSerializeorder = false;
				}
				else btNode.order = 0;

				{//Braces here only for grouping, not for scope
					if (current_ITopNode is DemogFormDesignType dfd)  //DemogForm is also a FormDesignType, so it must come first 
					{
						dfd.ElementName = "DemogFormDesign";
						dfd.name = Regex.Replace(dfd.ID, @"\W+", ""); //replaces any characters that are not numbers, letters or "_"
					}
					else if (current_ITopNode is FormDesignType fd)
					{
						fd.ElementName = "FormDesign";
						//fd.name = Regex.Replace(fd.ID, @"\W+", "");
						if (createNodeName is not null)
							fd.name = createNodeName(fd) ?? Regex.Replace(fd.ID, @"\W+", "");
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
			//}
			//Set _currentTopNode; If other ITopNode nodes are subsumed in this tree, current_ITopNode will be adjusted to the subsumed node(s)
			//if (refreshTree)
			//{
				Init_ITopNode(current_ITopNode);
				//current_ITopNode.ClearDictionaries();
				SdcUtil.AssignGuid_sGuid_BaseName(btNode);

				if (btNode.ParentNode is null) btNode.TopNode = current_ITopNode; //points to itself, indicating this is the root node
				else btNode.TopNode = btNode.ParentNode.TopNode;

				btNode.RegisterNodeAndParent(btNode.ParentNode, childNodesSort: false);
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
					IEnumerable<PropertyInfo>? props;
					Type sPop = s.Pop();

					if (! dListPropInfoElements.TryGetValue(sPop, out props))
					{
						props = sPop.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
						.Where(p => p.IsDefined(typeof(XmlElementAttribute))).ToList();
						//.OrderBy(p => p.GetCustomAttributes<XmlElementAttribute>()  //ordering is not currently needed to retrieve
						//.First().Order)											  //properties in XML Element order, but this could change
						;

						dListPropInfoElements.Add(sPop, props);
					}

					foreach (var p in props)
					{
						var prop = p.GetValue(node);
						if (prop != null)
						{
							if (prop is BaseType)
							{
								btProp = (BaseType)prop;
								if (refreshTree) RefreshTree(parentNode: node, piChildProperty: p, btProp, ref current_ITopNode);
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
									if (refreshTree) RefreshTree(parentNode: node, piChildProperty: p, btProp, ref current_ITopNode);
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

				void RefreshTree(BaseType parentNode, PropertyInfo piChildProperty, BaseType btProp, ref _ITopNode? current_ITopNode)
				{
					//piChildProperty is the PropertyInfo object from the btProp property
					//Neither piChildProperty nor btProp reliably contains the XML element name for the btProp node
					//In some cases, it can be obtained by looking at the parentNode,
					//finding the IEnumerable<> Property that contains btProp, and then looking for 
					//the enum value that contains the XML element name.
					//The enum location is found in XmlChoiceIdentifierAttribute on the IEnumerable Property
					//This should be handled in ReflectSdcElement
					

					if (btProp is _ITopNode itn) //we have a subsumed ITopNode node
					{
						current_ITopNode = Init_ITopNode(itn);
						btProp.TopNode = parentNode.TopNode;
					}
					else btProp.TopNode = current_ITopNode;

					SdcUtil.AssignGuid_sGuid_BaseName(btProp);  //check if thread-safe - may rely on parent IET

					//Refill the node dictionaries with the current node
					btProp.RegisterNodeAndParent(parentNode, childNodesSort: false); //we are adding nodes in reflection-sorted order
																					 //Debug.Print(btProp.sGuid + "; Obj ID: " + btProp.ObjectID);
																					 //Adding is not thread-safe - need ConcurrentDictionary
																					 //Mark parentNode as having its child nodes already sorted
					TreeSort_Add(parentNode);  //Change ObjectID to ObjectGUID?  //Probably thread-safe, as it's a hashtable, but may need Concurrent Hashtable?
					AssignSdcProperties(parentNode, piChildProperty, btProp, current_ITopNode, ref order, orderGap, print, sbTreeText, createNodeName);
				}

				void AssignSdcProperties(BaseType parentNode, PropertyInfo pi, BaseType btProp, ITopNode? current_ITopNode, ref int order, int orderGap, bool print, StringBuilder sbTreeText, CreateName? createNodeName)
				{
					string elementName;
					int elementOrder = -1;
					//Fill some useful properties, while it's efficient to do so, 
					//because we have the PropertyInfo object (pi) and the actual property object (btProp) already available.
					elementName = SdcUtil.ReflectSdcElement(pi, btProp, parentNode, out _, out elementOrder, out _, out _, out _, out string? errorMsg);
					if (elementName.IsNullOrWhitespace()) Debugger.Break();
					btProp.ElementName = elementName;
					btProp.ElementOrder = elementOrder;

					if (orderGap == 0)
					{
						btProp.order = default;
						btProp._shouldSerializeorder = false;
					}
					else btProp.order = ++order * orderGap;

					if (btProp is IdentifiedExtensionType iet)
					{
						if (iet.ID.IsNullOrEmpty()) 
							iet.ID = $"___{iet.sGuid}";
					}
					if (createNodeName is not null)
						btProp.name = createNodeName(btProp) ?? btProp.name;

					if (print)
					{
						sbTreeText.Append($"ElementName: {btProp.ElementName}; ElementOrder: {btProp.ElementOrder}; order: {btProp.order}; name: {btProp.name}; sGuid = {btProp.sGuid}");
						sbTreeText.Append("\r\n");
					}
				}
			}

			_ITopNode Init_ITopNode(_ITopNode new_ITopNode)
			{
				new_ITopNode._ClearDictionaries();
				return new_ITopNode;
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
		/// <returns> Sorted <b>List&lt;BaseType></b> containing all nodes subsumed under <paramref name="tn"/></returns>
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
			//TreeSort_ClearNodeIds();
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
			if (ResetSortFlags) TreeSort_ClearNodeIds(n);

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
		/// Returns the input node n and all non-IET subnodes. Subnode search breaks at all IET nodes, <br/>
		/// so that IET subnodes and their descendants are not included.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="startReorder"><br/>If less than 0, no reordering will be performed.<br/></param>
		/// <param name="orderInterval"></param>
		/// <param name="ResetSortFlags"></param>
		/// <returns><see cref="List{BaseType}"/> where T = <see href="BaseType"/></returns>
		public static List<BaseType> GetSortedNonIETsubtreeList(BaseType n, int startReorder = 0, int orderInterval = 1, bool ResetSortFlags = true)
		{
			//var nodes = n.TopNode.Nodes;
			//var topNode = Get_ITopNode(n);
			var cn = Get_ChildNodes(n);// topNode._ChildNodes;
			int i = 0;
			var sortedList = new List<BaseType>();
			if (ResetSortFlags) TreeSort_ClearNodeIds(n);

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
							if(child is not IdentifiedExtensionType) MoveNext(child);
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
			if (resortChildNodes && resetSortFlags) TreeSort_ClearNodeIds(n);

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
		/// Traverse an SDC tree by reflection to optionally reset @order and/or to refresh the TopNode dictionaries encountered in the tree
		/// More tree traversal changes can be added by proving delagates to nodeWorker functions.
		/// </summary>
		/// <param name="startNode">The root of the subtree to modify</param>
		/// <param name="singleNode">Limit changes to the single startNode.  Do not process the subtree</param>
		/// <param name="reOrder">Setting to true: refresh all subTree @order properties in sequential order.<br/>
		/// Setting this to true may cause non-subTree parts of the whole tree to become incorrectly ordered relative to the subtree.
		/// </param>
		/// <param name="reRegisterNodes">Setting to true: Reregister all nodes in the TopNodes dictionaries</param>
		/// <param name="startReorder">The starting number when reordering nodes with @order.  <br/>
		/// In general, this should be set to startNode.order
		/// </param>
		/// <param name="orderInterval">The interval between new @order properties.</param>
		/// <param name="resetNodeIdentity">Setting to true creates new vaues for ObjectGUID, sGuid (which matches ObjectGuid), BaseName, @name and ID<br/>
		/// This is useful when the caller wants to reuse subtree with new identities, e.g., when cloning an SDC template
		/// This option will also reregister all nodes in the TopNodes dictionaries</param>
		/// <param name="createNodeName">An Sdc.Util.CreateName delegate pointing to a function that will refresh the @name property for each node.<br/>
		/// The default value (null) will not chagne any @name values.
		/// </param>
		/// <param name="nodeWorkerFirst">A function pointer (delegate) to a method that can modify a BaseType node.<br/>  
		/// It will run before any other work on the visited nodes, e.g., before changing @order or refreshing dictionaries.<br/> 
		/// Returns true for success, false for failure.
		/// </param>
		/// <param name="nodeWorkerLast">A function pointer (delegate) to a method that can modify a BaseType node.<br/>  
		/// It will run after all other work on the visted nodes.<br/> 
		/// Returns true for success, false for failure.
		/// </param>
		/// <returns>null if the method fails at any point;</returns>
		public static List<BaseType>? ReflectRefreshSubtreeList(BaseType startNode,

			bool singleNode = false,
			bool reOrder = false,
			bool reRegisterNodes = false,
			int startReorder = 0,
			byte orderInterval = 1,
			bool resetNodeIdentity = false,
			CreateName? createNodeName = null,
			Func<BaseType, bool>? nodeWorkerFirst = null,
			Func<BaseType, bool>? nodeWorkerLast = null)
		{
			if (startNode is null) throw new InvalidOperationException($"Parameter '{nameof(startNode)}' cannot be null");
			//if(resetNodeIdentity is true && createNodeName is null) throw new InvalidOperationException("If resetName is true, then createNodeName cannot be null");

			var i = startReorder;
			var nodeList = new List<BaseType>();
			BaseType? par = startNode.ParentNode;

			//Process the root of the subtree
			NodeWorker(startNode, par);
			if (singleNode) return nodeList;

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
				if (parentNode is null && n != startNode)
					throw new InvalidOperationException("A null parentNode was passed to NodeWorker.  Only the startNode may have a null parentNode");

				var tn = (_ITopNode)n.TopNode!;

				if (nodeWorkerFirst is not null)
					if (nodeWorkerFirst(n) is false) throw new InvalidOperationException($"Method failed at nodeWorkerFirst(n), at sGuid {n.sGuid}");

				//______________________________________________________________________________

				if (reRegisterNodes || resetNodeIdentity)
					n.UnRegisterNodeAndParent();

				//!START Special actions-------------------------------:
				{
					//Reset node identity: ObjectGUID, sGuid, BaseName, @name and ID
					if (resetNodeIdentity)
					{
						//n.ObjectGUID = Guid.NewGuid();
						//n.sGuid = ShortGuid.Encode(n.ObjectGUID);
						//n.BaseName = CreateBaseNameFromsGuid(n.sGuid);
						SdcUtil.AssignGuid_sGuid_BaseName(n);

						if (n is IdentifiedExtensionType iet)
							iet.ID = $"___{n.BaseName}";

						if (createNodeName is not null)
							n.name = createNodeName(node: n);
						else n.name = CreateCAPname(n);
					}

					if (reOrder)
					{
						n.order = i;
						i += orderInterval;
					}
				}

				if (reRegisterNodes || resetNodeIdentity)
					n.RegisterNodeAndParent(parentNode);
				//!END Special actions-------------------------------:

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
		/// <param name="startReorder"></param>
		/// <param name="orderInterval"></param>
		/// <returns><see cref="List{BaseType}"/></returns>
		public static List<BaseType> GetSubtreeReOrderNodesList(BaseType n, int startReorder = 0, int orderInterval = 1)
		{
			return GetSortedSubtreeList(n, startReorder, orderInterval);
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

		private static BaseType? X_ReflectNextSibElement(BaseType n)
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
		/// Retrieve the previous <see cref="BaseType"/> SDC element node using _ITopNode dictionaries.<br/>
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
		/// Retrieve the previous <see cref="IdentifiedExtensionType"/> SDC element node using _ITopNode dictionaries.<br/>
		/// This node may be a previous sibling, or a non-sibling node higher up in the SDC tree (closer to the SDC root node), under a different parent node.		
		/// /// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static IdentifiedExtensionType? GetPrevElementIET(BaseType n)
		{ 
			BaseType? bt = n;
			do
			{
				bt = bt.GetNodePrevious();
				if (bt is IdentifiedExtensionType iet) return iet;

			} while(bt is not null);

			return null;
		}

		/// <summary>
		/// Retrieve the previous <see cref="IdentifiedExtensionType"/> SDC element node by reflections.<br/>
		/// This node may be a previous sibling, or a non-sibling node higher up in the SDC tree (closer to the SDC root node), under a different parent node.		
		/// /// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static IdentifiedExtensionType? ReflectPrevElementIET(BaseType n)
		{
			BaseType? bt = n;
			do
			{
				bt = ReflectPrevElement(n);
				if (bt is IdentifiedExtensionType iet) return iet;

			} while (bt is not null);

			return null;
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
			if (topNode is null) throw new InvalidOperationException("topNode could not obtained from the input node n");			
			topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs);
			if (sibs is null) return null;
			SortElementKids(par, sibs);

			var index = sibs?.IndexOf(n) ?? -1; //
			if (index == 0) return null; //item is the first item
			return sibs?[index - 1]; //throws exception if index = -1 (child not found in the list of sibs)
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
		public static ImmutableList<BaseType>? GetChildElements(BaseType n)
		{
			var topNode = Get_ITopNode(n);
			List<BaseType>? kids = null;
			topNode?._ChildNodes.TryGetValue(n.ObjectGUID, out kids);
			if (kids is not null && kids.Count > 0)
			{
				SortElementKids(n, kids);
				return kids.ToImmutableList<BaseType>();
			}
			return null;
			//return kids?.AsReadOnly();
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
		/// <param name="attributesToExclude">string array containing the names of SDC XML attributes to omit from the returned List</param>
		/// <param name="attributesToInclude">string array containing the names of SDC XML attributes to include in the returned List</param>
		/// <returns> <see cref="List{AttributeInfo}"/> containing the child nodes</returns>
		private static List<AttributeInfo> X_ReflectChildXmlAttributes(BaseType n
			, bool getAllXmlAttributes = true
			, bool omitDefaultValues = true
			, string[]? attributesToExclude = null
			, string[]? attributesToInclude = null)
		{
			if (n is null) throw new NullReferenceException("n cannot be null"); //You can't have sibs without a parent

			List<AttributeInfo> attributes = new();
			IEnumerable<PropertyInfo>? piIE = null;
			int nodeIndex = -1;
			var attMethods = new AttributeMethods();
			IList<AttributeInfo>? atts;

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
				if (false)
				{
					if (t == typeof(BaseType))
						atts = attMethods.GetTypeFilledAttributes(t, n, new string[] { "name", "sGuid", "order" });
					else
						atts = attMethods.GetTypeFilledAttributes(t, n);
					if (atts is not null)
					{
						attributes.AddRange(atts);
						continue;
					}
				}

				piIE = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
						.Where(pi => pi.GetCustomAttributes<XmlAttributeAttribute>().Any());
				foreach (var p in piIE)
				{
					nodeIndex++;
					if (getAllXmlAttributes) AddAttribute();
					else
					{
						var sspn = t.GetMethod("ShouldSerialize" + p.Name)?.Invoke(n, null); //sspn == ShouldSerialize*PropertyName*

						var pVal = p.GetValue(n);
						var attDefVal = GetAttributeDefaultValue(p);

						if (pVal is not null) //if pVal is null, there is nothing to serialize
						{
							if (attributesToExclude is not null && attributesToExclude.Contains(p.Name)) continue;
							if (attributesToInclude is not null && !attributesToInclude.Contains(p.Name)) continue;

							//if (p.Name == "showInReport") Debugger.Break();
							bool IsAttDefValMatch = attDefVal?.Equals(pVal) ?? false; //Does pVal match its DefaultValueAttribute (i.e., attDefVal); false if DefaultValueAttribute not present (null)

							if (sspn is bool shouldSerialize && shouldSerialize) //if(_shouldSerializePropertyName is true);	and pVal does not match its DefaultValueAttribute (i.e., attDefVal)
							{
								//Make sure the property does not hold a default value (based on DefaultValueAttribute's Value property)
								//sspn does NOT overide DefaultValueAttribute setting, the serializer will not produce output if DefaultValueAttribute matches the current value.
								//The only easy way to overide it (force XML output) is to remove the DefaultValueAttribute, or create the serializer with an XmlAttributeOverrides instruction in its constructor
								//see https://stackoverflow.com/questions/28054335/force-xml-serialization-of-xmldefaultvalue-values, and
								//https://learn.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlattributes.xmldefaultvalue?view=net-7.0

								if (!IsAttDefValMatch)
								{
									//if (p.Name == "showInReport") Debugger.Break();
									AddAttribute();
								}
							}
							else if (omitDefaultValues)  //The XML serializer omits all properties set to the Value of the DefaultValueAttribute, assuming DefaultValueAttribute decorates the property
							{
								if (attDefVal is not null) //Check the DefaultValueAttribute
								{
									if (!IsAttDefValMatch)
									{
										//if (p.Name == "showInReport") Debugger.Break();
										AddAttribute();
									}
								}
								else //Check the data type's intrinsic default value set by .NET
								{
									var typeDefaultVal = GetTypeDefaultValue(pVal.GetType());
									if (!pVal.Equals(typeDefaultVal))
										AddAttribute();
								}
								//else if (pVal.IsNumeric() && (double)pVal == 0)  //pVal is a numeric type
								//	pValIsAttributeDefault = true;
								//else if (pVal is string str && str == "") 
								//	pValIsAttributeDefault = true;
								//else if (pVal is DateTime dtm && dtm == default)
								//	pValIsAttributeDefault = true;
								//else if (pVal is DateOnly dt && dt == default)
								//	pValIsAttributeDefault = true;
								//else if (pVal is TimeOnly tm && tm == default)
								//	pValIsAttributeDefault = true;
								//else if (pVal is TimeSpan ts && ts == default)
								//	pValIsAttributeDefault = true;
								//else if (pVal is bool b && b == default)
								//	pValIsAttributeDefault = true;
								//?TODO: Add default tests for other SDC data types
							}
							else AddAttribute();
							//else if (sspn is null)  // ShouldSerializePropertyName does not exist for property p.  This can occur for properties like byte[], HTML/XML types, etc.
							//{

							//	if (attDefVal is not null) //Test if the property's DefaultValueAttribute (it's unlikely if this is present) value does not match the current property value,
							//	{
							//		if (!pValIsAttributeDefault)
							//			AddAttribute();
							//	}
							//	else //There was no DefaultValueAttribute found (i.e., attDefVal is null and thus pValIsAttributeDefault is false),
							//		 //so now we see if we have a non-default (e.g., non-null for reference types) property value (obtained from GetTypeDefaultValue) for its datatype.
							//	{
							//		if (!pValIsAttributeDefault)
							//			AddAttribute();
							//	}
							//}
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
				new(elementNode, p!.GetValue(elementNode), p, nodeIndex);

		}

		private static AttributeMethods attMethods = new();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="n">The see<see cref="BaseType"/> node for which attribtues will be determined</param>
		/// <param name="getAllXmlAttributes"></param>
		/// <param name="omitDefaultValues"></param>
		/// <param name="attributesToExclude">By default, "name", "sGuid", and "order" attributes are excluded and will not be returned. <br/>
		/// By default, the values of these attribtues are assifned by the SDC object model, and need not be changed manually<br/>
		/// To include these nodes, pass an empty string array: string[], or any other non-null string array<br/>
		/// Excluded attributes takes precedence over included attributes</param>
		/// <param name="attributesToInclude"></param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public static List<AttributeInfo> ReflectNodeXmlAttributes(BaseType n
			, bool getAllXmlAttributes = true
			, bool omitDefaultValues = true
			, string[]? attributesToExclude = null
			, string[]? attributesToInclude = null)
		{
			if (n is null) throw new NullReferenceException("n cannot be null"); //You can't have sibs without a parent

			List<AttributeInfo> attributes = new();
			IEnumerable<PropertyInfo>? piIE = null;
			int nodeIndex = -1; 
			IList<AttributeInfo>? atts;
			attributesToExclude??= new string[] { "name", "sGuid", "order" };

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

				//Look in filled attributes in hard-coded SDC types
				if (t == typeof(BaseType))
					atts = attMethods.GetTypeFilledAttributes(t, n, attributesToExclude);
				else
					atts = attMethods.GetTypeFilledAttributes(t, n);
				if (atts is not null)
				{
					attributes.AddRange(atts);
					continue;
				}

				if( ! dListPropInfoAttributes.TryGetValue(t, out piIE))  //look in cache to bypass slow PropertyInfo lookup
				{
					piIE = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
							.Where(pi => pi.GetCustomAttributes<XmlAttributeAttribute>().Any());
					dListPropInfoAttributes.Add(t, piIE); //cache for next time
				}
				if (piIE is null) continue;

				foreach (var p in piIE)
				{
					nodeIndex++;
					if (getAllXmlAttributes) AddAttribute(p, n);
					else
					{	
						var sspn = t.GetMethod("ShouldSerialize" + p.Name)?.Invoke(n, null); //sspn == ShouldSerialize*PropertyName*

						var pVal = p.GetValue(n);
						var attDefVal = GetAttributeDefaultValue(p);

						if (pVal is not null) //if pVal is null, there is nothing to serialize
						{
							if (attributesToExclude is not null && attributesToExclude.Contains(p.Name)) continue;
							if (attributesToInclude is not null && !attributesToInclude.Contains(p.Name)) continue;

							//if (p.Name == "showInReport") Debugger.Break();
							bool IsAttDefValMatch = attDefVal?.Equals(pVal) ?? false; //Does pVal match its DefaultValueAttribute (i.e., attDefVal); false if DefaultValueAttribute not present (null)

							if (sspn is bool shouldSerialize && shouldSerialize) //if(_shouldSerializePropertyName is true);	and pVal does not match its DefaultValueAttribute (i.e., attDefVal)
							{
								//Make sure the property does not hold a default value (based on DefaultValueAttribute's Value property)
								//sspn (shouldSerializePropertyName) does NOT overide DefaultValueAttribute setting, the serializer will not produce output if DefaultValueAttribute matches the current value.
								//The only easy way to overide it (force XML output) is to remove the DefaultValueAttribute, or create the serializer with an XmlAttributeOverrides instruction in its constructor
								//see https://stackoverflow.com/questions/28054335/force-xml-serialization-of-xmldefaultvalue-values, and
								//https://learn.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlattributes.xmldefaultvalue?view=net-7.0

								if (!IsAttDefValMatch)
								{
									//if (p.Name == "showInReport") Debugger.Break();
									AddAttribute(p, n);
								}
							}
							else if (omitDefaultValues)  //The XML serializer omits all properties set to the Value of the DefaultValueAttribute, assuming DefaultValueAttribute decorates the property
							{
								if (attDefVal is not null) //Check the DefaultValueAttribute
								{
									if (!IsAttDefValMatch)
									{
										//if (p.Name == "showInReport") Debugger.Break();
										AddAttribute(p, n);
									}
								}
								else //Check the data type's intrinsic default value set by .NET
								{
									var typeDefaultVal = GetTypeDefaultValue(pVal.GetType());
									if (!pVal.Equals(typeDefaultVal))
										AddAttribute(p, n);
								}
								//?TODO: Add default tests for other SDC data types
							}
							else AddAttribute(p, n);
						}
					}

					void AddAttribute(PropertyInfo p, BaseType n)
					{
						nodeIndex++;
						attributes.Add(FillAttributeInfo(p, n));
					}
				}
			}
			return attributes;

			AttributeInfo FillAttributeInfo(PropertyInfo p, BaseType elementNode) =>
				new(elementNode, p.GetValue(elementNode), p, nodeIndex);

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

		/// <summary>
		/// Returns true if child nodes are present.  <br/>
		/// The <b><paramref name="kids"/></b> out parameter will be populated if child nodes are present, and will be null if no nodes are found.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="kids"></param>
		/// <returns></returns>
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

		public static PropertyInfoMetadata GetElementPropertyInfoMeta(BaseType item, BaseType? parentNode, bool getNames = true)
		{
			PropertyInfo? pi = GetElementPropertyInfo(
				item,
				parentNode,
				out string? propName,
				out int itemIndex,
				out IEnumerable<BaseType>? ieItems,
				out int xmlOrder,
				out int maxXmlOrder,
				out string? xmlElementName,
				getNames);

			return new PropertyInfoMetadata(pi, propName, itemIndex, ieItems, xmlOrder, maxXmlOrder, xmlElementName);

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
		/// <summary>
		/// Get the PropertyInfo object that represents the "item" property in the item's ParentNode
		/// This PropertyInfo object may be decorated with important XML annnotations such as XmlElementAttribute
		/// The returned PropertyInfo object may refer to a BaseType or the IEnumerables List&lt;BaseType> and Array&lt;BaseType> 
		/// If a wrapper property was created in an SDC parrtial class, only the inner property (i.e., the one with XML attributes) is returned
		/// </summary>
		/// <param name="item"></param>
		/// <param name="parentNode"></param>
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
			BaseType? parentNode,
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
			//BaseType? parentNode = item.ParentNode;
			if (parentNode is null)
			{
				parentNode = item;  //we are at the top node
				var t = item.GetType();
				xmlElementName = t.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
				xmlOrder = -1; // -1 is a special case indicating root node
				return null;
			}

			maxXmlOrder = GetMaxOrderFromXmlElementAttributes(parentNode);

			xmlElementName = ReflectSdcElement(item, parentNode, out ieItems, out xmlOrder, out maxXmlOrder, out itemIndex, out PropertyInfo? piItemOut, out _);
			propName = piItemOut.Name;
			return piItemOut;



		}
		private static int GetElementItemIndex(BaseType item, BaseType parentNode, out IEnumerable<BaseType>? ieItems, out PropertyInfo? piItemOut, out string errorMsg)
		=> GetElementItemIndex(item, parentNode, null, out ieItems, out piItemOut, out errorMsg);

		private static int GetElementItemIndex(BaseType item, BaseType parentNode, IEnumerable<PropertyInfo>? ieParProps, out IEnumerable<BaseType>? ieItems, out PropertyInfo? piItemOut, out string errorMsg)
		{
			errorMsg = "";
			ieItems = null;
			piItemOut = null;
			//BaseType? parentNode = item.ParentNode;

			if (parentNode is null)
			{ errorMsg = $"{nameof(GetElementItemIndex)}: {nameof(parentNode)} cannot be null"; return -1; }
			if (ieParProps is null)
			{
				ieParProps = parentNode.GetType().GetProperties()
						.Where(p => typeof(IEnumerable<BaseType>)
						.IsAssignableFrom(p.PropertyType)
						&& p.GetCustomAttributes(typeof(XmlElementAttribute)).Any()  //We must confirm that our IEnumerable has a XmlElementAttribute,
																					 //since we added some shadow properties in the partial classes
																					 //like "ChildItems_List" for "Items"
																					 //&& p.GetValue(par) is not null			
																					 //This may be good for a future refactoring of the lambda expression; it will get the matched property directly and concisely.
						);

				if (ieParProps is null || !ieParProps.Any())
				{ errorMsg = $"{nameof(GetElementItemIndex)}: the ParentNode of {nameof(item)} does not contain an IEnumerable<BaseType> that contains the the target {nameof(item)}"; return -1; }
			}
			foreach (var propInfo in ieParProps!) //loop through IEnumerable PropertyInfo objects in par
			{   //Reflect each propInfo to see if our item parameter lives in it
				ieItems = (IEnumerable<BaseType>?)propInfo.GetValue(parentNode);
				if (ieItems is not null && ieItems.Any())
				{
					piItemOut = propInfo;
					return IndexOf(ieItems, item); //search for item
				}
			}
			return -1;
		}

		private static string ReflectSdcElement(BaseType item, BaseType? parentNode, out IEnumerable<BaseType>? ieItems, out int xmlOrder, out int maxXmlOrder, out int itemIndex, out PropertyInfo piItem, out string? errorMsg)
		{
			string? elementName = ReflectSdcElement(null, item, parentNode, out ieItems, out xmlOrder, out maxXmlOrder, out itemIndex, out piItem, out errorMsg);
			return elementName;


		}

		/// <summary>
		/// If <paramref name="piItem"/> is passed as null, the method will retrieve it from <paramref name="item"/>
		/// </summary>
		/// <param name="piItem"></param>
		/// <param name="item"></param>
		/// <param name="parentNode"></param>
		/// <param name="ieItems"></param>
		/// <param name="xmlOrder"></param>
		/// <param name="maxXmlOrder"></param>
		/// <param name="itemIndex"></param>
		/// <param name="piItemOut"></param>
		/// <param name="errorMsg"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		private static string ReflectSdcElement(PropertyInfo? piItem, BaseType item, BaseType? parentNode, out IEnumerable<BaseType>? ieItems, out int xmlOrder, out int maxXmlOrder, out int itemIndex, out PropertyInfo? piItemOut, out string? errorMsg)
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
			//BaseType? parentNode = item.ParentNode;

			if (parentNode is null)
			{
				//we are at the top node
				xmlElementName = itemType.GetCustomAttribute<XmlRootAttribute>()?.ElementName;
				if (xmlElementName is null)
					throw new InvalidOperationException($"{nameof(ReflectSdcElement)} could not find a name for the {nameof(item)} parameter.  This may occur if {nameof(item)} is not an SDC ITopNode and has no parent object");
				return xmlElementName;
			}

			if (piItem is null) //piItem was not supplied, so let's try to find it.
			{
				parType = parentNode.GetType();
				PropertyInfo[] parProps = parType.GetProperties();
				//Look for a direct item-to-property match, so we can assign propName from the par object		
				piItemOut = parProps
					.Where(pi => pi.GetCustomAttributes(typeof(XmlElementAttribute)).Any()  //all serialized properties must have the XmlElementAttribute attribute
					&& ! typeof(IEnumerable<BaseType>).IsAssignableFrom(pi.PropertyType)     //the property is not an IEnumerable (i.e., an Array, List etc.)
					&& ReferenceEquals(pi?.GetValue(parentNode), item))?.FirstOrDefault();          //There can be, at most, one match to our item object

				if (piItemOut is not null)
					piItem = piItemOut; // piItem is not an IEnumerable here

				else if (piItem is null) // piItem is still null; let's try again, in IEnumerable properties
				{
					//Now we look in IEnumerable properties, to see if "item" is contained inside it.
					//Let's see if our item object lives in an IEnumerable<BaseClassSubtype> 
					itemIndex = GetElementItemIndex(item, parentNode, out ieItems, out piItemOut, out errorMsg); //item.ParentNode can't be null here
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
			//which is a special property in the same class (named, i.e., "ItemElementName" or "ItemsElementName"),
			//and with a type of an enum subclass, or an Ienumerable<enumSubclass>
			//The type of the enum object (Item(s)ElementName) holds an enum value with the element's name.
			//This enum field (Item(s)ElementName) may be an array holding a list of ElementNames, in the same order that the elements appear in the Items List
			//This is handled in the next method call:
			xmlElementName = GetElementNameFromItemChoiceEnum(piItem, item, parentNode, ref itemIndex, out errorMsg);
			if (xmlElementName?.Length > 0) return xmlElementName;

			//If there is only one XmlElementAttribute, try to get elementName directly from the XmlElementAttribute.
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
			//THis case was probably handled already inside GetElementNameFromEnum
			if (xeAtts?.Length > 1 && itemIndex > -1)
			{
				//int index = GetItemIndex(piItem, item, out errorMsg);
				xmlElementName = xeAtts.ToArray()[itemIndex].ElementName;
				if (xmlElementName?.Length > 0)
					return xmlElementName;
			}

			//There was no ElementName to extract from an XmlElementAttribute or enum, so we get it directly from the propName.
			if (piItem.Name == "Item") Debugger.Break();
			if (piItem.Name == "Items") Debugger.Break();
			return piItem.Name;

			throw new InvalidOperationException("Could not find a name for the n parameter.");
		}


		internal static bool TryAttachNewNode(BaseType newNode, string newNodeElementName, BaseType parentTarget, 
			out Object? targetPropertyObject
			, out String errorMsg
			, bool attachSourceToParentTarget = true
			, int insertPosition = -1
			, bool overwriteExistingObject = false
			, bool cancelWhenChildNodes = false
			)
		{/*
		  * 1) If itemElementName populated?
			1.1) Look in newParent for Item(s)ElementName to find name of Item(s)ChoiceType#
			1.2)	For item, look in ItemChoiceType# enum to find matching ElementName.
				1.2.1)	Set newParent = newItem, 
						Set ItemChoiceType# to match the ElementName and 
						Return true; 
			1.3)	For Items, look in ItemsChoiceType# enum values to find matching ElementName.
				1.3.1)	Execute Items.Insert(newItem, insertPostion), 
						Set ItemsChoiceType#[insertPostion] to match ElementName and 
						Return true.
			2) If itemElementName is not populated:
			2.1) Look for the first newItem datatype match in newParent.  If no ItemElementName, set the item.  If ItemElementName is present, throw...
			*/
			targetPropertyObject = null;
			errorMsg = "";
			Type? enumType = null;
			bool result = false;
			int matchCount = 0;

			PropertyInfo? piTarget;
			Type sourceType = newNode.GetType();

			//+Try to match source to a parentTarget property, based on source's datatype
			if (newNodeElementName.IsNullOrWhitespace())
			{  //no elementName supplied, so let's see if we can match to one and only one XmlElementAttribute by its datatype
				piTarget = parentTarget.GetType().GetProperties()
					.Where
					(n =>
					{
						matchCount += n.GetCustomAttributes<XmlElementAttribute>()
						.Where(a => a.Type == sourceType).Count();
						return (matchCount > 0);
					})?
					.FirstOrDefault();

				if (matchCount > 1)
				{   //elementName is empty ("") here
					errorMsg += $"{nameof(newNodeElementName)} was not supplied, and multiple XmlElementAttribute names matched {nameof(newNode)}'s datatype.";
					return false;
				}
			}
			else
			{//+Try to match source to a parent property, based on source's SDC XML elementName
				piTarget = parentTarget.GetType().GetProperties()
							.Where(n => n.GetCustomAttributes<XmlElementAttribute>()
							.Where(a => a.ElementName == newNodeElementName).Any())?
							.First();
			}

			if (piTarget is null)  //could not find a parentTarget property to attach source
			{
				errorMsg += $"Could not find a property in {nameof(parentTarget)} to bind to {nameof(newNode)}. Check the value of {nameof(newNodeElementName)} (if supplied), and ensure that valid {nameof(newNode)} and {nameof(parentTarget)} objects were supplied.";
				return false; //can't find a property to bind to source
			}
			if (!attachSourceToParentTarget) return true;

			//!+---------Attach Source to targetParent-----------------------------------

			//targetPropertyObject could be null here, as it may not have been instantiated yet.
			//It may return a single property or a List<> of properties
			targetPropertyObject = piTarget.GetValue(parentTarget); 
			result = true;			

			//+Try to find Item(s)ChoiceType object for piTarget, if it exists
			//piChoiceEnum will tell us if Item(s)ChoiceType is defined as a property.  choiceEnum will be non-null if Item(s)ChoiceType has been instantiated
			object? choiceEnum = GetItemChoiceEnumFromItemChoiceIdentifier(piTarget, newNode, out var piChoiceEnum);

			if (piChoiceEnum is not null)  //We need to process an Item(s)ChoiceEnum object
			{
				if (choiceEnum is null)
				{ //Create a new Enum or List<Enum> object, and attach it to parentTarget:
					choiceEnum = Activator.CreateInstance(piChoiceEnum.PropertyType);
					piChoiceEnum.SetValue(parentTarget, choiceEnum);
				}
				if (choiceEnum is IList itemsChoiceType) //itemsChoiceType is always List<EnumSubtype> 
				{
					enumType = itemsChoiceType.GetType().GetElementType();
					result = Enum.TryParse(enumType!, newNodeElementName, out object? newEnumObj);

					if (insertPosition == -1 || insertPosition > itemsChoiceType.Count - 1)
						insertPosition = itemsChoiceType.Count - 1;
					itemsChoiceType.Insert(insertPosition, newEnumObj);


					//Create target list object if not present
					if (targetPropertyObject is null)
					{
						targetPropertyObject = Activator.CreateInstance(piTarget.PropertyType);
						piTarget.SetValue(parentTarget, targetPropertyObject);
					}
					((IList)targetPropertyObject!).Insert(insertPosition, newNode);

					return result;
				}
				else if (choiceEnum is Enum itemChoiceType) //itemChoiceType is a simple Enum subtype.
				{
					enumType = itemChoiceType.GetType();
					result = Enum.TryParse(enumType, newNodeElementName, out object? newEnumObj);

					enumType = itemChoiceType.GetType();
					piChoiceEnum.SetValue(parentTarget, newEnumObj);
					if (targetPropertyObject is not null) targetPropertyObject.As<BaseType>().RemoveRecursive();
					piTarget.SetValue(parentTarget, newNode);
					return result;
				}
			}
			//piChoiceEnum is null here, so we can add source to target without populating an item(s)ChoiceType Enum object
			if (piTarget.PropertyType is IList lst)
			{
				if (insertPosition == -1 || insertPosition > lst.Count - 1)
					insertPosition = lst.Count - 1;

				//Create target List object if not present
				if (targetPropertyObject is null)
				{
					targetPropertyObject = Activator.CreateInstance(piTarget.PropertyType);
					piTarget.SetValue(parentTarget, targetPropertyObject);
				}
				((IList)targetPropertyObject!).Insert(insertPosition, newNode);

				return true;
			}
			else //the target property is not a List<BaseTypeSubtype>;  
			{	//if targetPropertyObject is not null, we need to remove it, along with any descendant nodes.
				if (targetPropertyObject is not null) targetPropertyObject.As<BaseType>().RemoveRecursive();
				piTarget.SetValue(parentTarget, newNode);
				return true;
			}
		}


		private static string? GetElementNameFromItemChoiceEnum(PropertyInfo piItem, BaseType item, BaseType parentNode, ref int itemIndex, out string? errorMsg)
		{
			errorMsg = null;
			object? choiceIdentifierObject = GetItemChoiceEnumFromItemChoiceIdentifier(piItem, item, out _);
			if (choiceIdentifierObject is null)
				return null; //An enum is not used to determine the XML Element name			

			if(itemIndex == -1)
				itemIndex = GetElementItemIndex(item, parentNode, out _, out _, out errorMsg); //item must have non-null item.ParentNode

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

		private static object? GetItemChoiceEnumFromItemChoiceIdentifier(PropertyInfo piItem, BaseType item, out PropertyInfo? piChoiceEnum)
		{//old name: ItemChoiceEnum
			string? enumName = GetItemChoiceEnumFromAttribute(piItem);
			piChoiceEnum = null;
			if (enumName == null) return null!;
			piChoiceEnum = item.ParentNode?.GetType()?.GetProperty(enumName);
			var choiceEnumObj = piChoiceEnum?.GetValue(item.ParentNode);
			if (choiceEnumObj is Enum e) return e;
			if (choiceEnumObj is IEnumerable ie) return ie;
			return null;
		}

		/// <summary>
		/// Gets the enumeration to use when detecting types
		/// </summary>
		/// <param name="piItem"></param>
		/// <returns></returns>
		private static string? GetItemChoiceEnumFromAttribute(PropertyInfo piItem)
		{//old name: ItemChoiceEnumName
			XmlChoiceIdentifierAttribute? xci = (XmlChoiceIdentifierAttribute?)piItem.GetCustomAttribute(typeof(XmlChoiceIdentifierAttribute));
			if (xci is null) return null;
			return xci.MemberName;
		}

		/// <summary>
		/// Reflect the object tree to determine if <paramref name="newItem"/> can be attached to <paramref name="newParent"/>.   
		/// We must find an <em>exact</em> match for <paramref name="newItem"/>'s element name and the data type in <paramref name="newParent"/> to allow the move.
		/// </summary>
		/// <param name="newItem">The SDC node to test for its ability to be attached to the <paramref name="newParent"/> node.</param>
		/// <param name="newParent">The node to which the <paramref name="newItem"/> node should be moved.</param>
		/// <param name="pObj">The property object on <paramref name="newParent"/> that would attach to <paramref name="newItem"/> (hold its object reference).
		/// pObj may be a List&lt;> or a non-List object.</param>
		/// <param name="itemElementName"></param>
		/// <returns>True for allowed parent nodes, false for disallowed not allowed</returns>
		//internal static bool IsParentNodeAllowed(BaseType item, BaseType newParent,
		//out PropertyIno pi, out object? pObj, string targetElementName = "")
		internal static bool IsParentNodeAllowed(BaseType newItem, BaseType newParent, out object? pObj, string itemElementName = "")
		{
			/* 
			item is the node we want to add to newParent
			If the newParent property is not a List<BaseType>, the matching newParent property could be null or occupied
			If it's occupied, that node-subtree must be removed from all dictionaries
			 - this is handled later by IMoveRemoveExtensions
			If the parent node is a List<> object, it may be null, and if non-null, it may have list entries
			If item has been added to a parent previously, we can find the current element name, and then
			look for an appropriate element name and object type match on newParent
			If item has not yet been attached to a parent node, there are cases when it could adopt more than one element name.

			In SDC, those multi-named SDC types are:
			     FileType (in RegistrySummaryType) ItemsChoiceType1[]: Manual, RegistryPurpose, ServiceLevelAgreement
			     CallFuncType (in ActionsType) ItemsChoiceType[]: CallFunction, ShowURL, WebService, ExternalRule
			     anyURI_Stype (in CallFuncBaseType) ItemChoiceType1: FunctionURI, LocalFunctionName
			     gMonth_DEtype (in DataTypes_DEType) ItemChoiceType2: gMonth, gYearMonth
			     gMonth_Stype (in DataTypes_SType) ItemChoiceType: gMonth, gYearMonth
				 gMonth_Stype (in DataTypesDateTime_SType) ItemChoiceType3: gMonth, gYearMonth
				 gMonth_DEtype (in DataTypesDateTime_DEType) 

			In all other cases, each SDC type has a unique ElementName that is generally hard-coded into the SDC partial classes
			In rare cases, a given item type can be attached to more than one property in a parent object.
			The best (and perhaps only) examples of multi-positional SDC types is EventType:
			     DI: EventType: OnEnter, OnExit
			     LI: EventTime: OnSelect, OnDeselect

			If item's current parent has NOT been set, then in these cases, the intended element name must be specified to determine if attachment is legal,
			and to return the attaching object in newParent.

			If item's current parent has been set, the current ElementName may be determined by refelction,
			and this name may be used for moving the node to newParent,  However, it's also possible that the calling code
			wants to move item to a property with a different element name, but a matching type,  For this reason, it may
			be necessary to pass in elementName whenever item is one of multi-named or multi-positional SDC types listed above.

			pObj the newParent p isroperty to which we want to attach item.  It is located by reflection, and it might be nuill.
			If null, it cannot be passed out by reference, but it can be attached to item by reflection.
			Alternatively, the PropertyInfo object can be exported, and the caller can attach item to newParent.
			In many cases, this requires setting enum values for ItemElementName.
			These properties have this attribute: [XmlChoiceIdentifierAttribute("ItemElementName")]

			This method should thus be split into 2: FindParentAttachmentObject (useful for deserialization and moving),
			and IsParentNodeAllowed (useful for adding new nodes to a parent node).  The latter requires that the
			calling code specify the intended ElementName for attachement of ambiguous types


			Hard code itemElementName for

			1) If itemElementName populated?
			1.1) Look in newParent for Item(s)ElementName to find name of Item(s)ChoiceType#
			1.2)	For item, look in ItemChoiceType# enum to find matching ElementName.
				1.2.1)	Set newParent = newItem, 
						Set ItemChoiceType# to match the ElementName and 
						Return true; 
			1.3)	For Items, look in ItemsChoiceType# enum values to find matching ElementName.
				1.3.1)	Execute Items.Insert(newItem, insertPostion), 
						Set ItemsChoiceType#[insertPostion] to match ElementName and 
						Return true.
			2) If itemElementName is not populated:
			2.1) Look for the first newItem datatype match in newParent.  If no ItemElementName, set the item.  If ItemElementName is present, throw...
			*/



			pObj = null;  //the property object to which item would be attached; it may be a List<> or a non-List object.

			if (newParent is null) return false;
			//make sure that item and target are not null and are part of the same tree
			//we'll allow moving from one tree to another, so the following line is commented out.
			//if (Get_Nodes(item)[newParent.ObjectGUID] is null) return false;

			if (newParent.IsDescendantOf(newItem)) return false;

			Type itemType = newItem.GetType();
			var thisPi = SdcUtil.GetElementPropertyInfoMeta(newItem, newParent); //This will throw if item is not currently referenced to some tree node
			string? itemName = thisPi.XmlElementName;
			if (itemName is null) return false;

			foreach (var p in newParent.GetType().GetProperties())
			{ //loop through parent properties

				var pAtts = p.GetCustomAttributes<XmlElementAttribute>();
				pObj = p.GetValue(newParent);  //object that item can be attached to; it may be a List or Array to attach "item" as an element, or another BaseType subclass object to which item can be attached"
				if (pObj is not null) //the proposed parent proprty is populated with some SDC object
				{
					foreach (var a in pAtts)  
						//the parent property can have multiple possible child object types
						//look for the one with a name that matches item's current name, if available
						//If the name is not availablke (perhaps item is not yet attached to a parent,
						//TODO: we perhaps can use an element name passed in bu the caller
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
								(p.PropertyType.GetElementType() == itemType //this will not work unless it's an exact type match
									|| p.PropertyType.GetElementType()!.IsAssignableFrom(itemType))
								)//e.g., like: ExtensionBaseType[] Items, with [XmlElement("ValidateForm", typeof(ActValidateFormType), Order=0)]
								return true;
						}
					}
				}
				//TODO: Also try matching element names found in the ItemChoiceType enums.  This is more reliable that using data types.

				//if none of the XmlElementAttributes had a matching Type an ElementName, perhaps the property Type will match directly
				//if (p.Name == itemName)
				//{
				//BUG: Need to do ItemChoiceType name checking here!!
				//BUG: Should be able to approve attaching @string_DEtype to Response node, which is a BaseType (DataTypes_DEType.Items,
				//BUG: also expressed as helper property DataTypes_DEType_Items)
				if (p.Name == itemName)  //In many cases, the property name will directly the itemName,
										 //TODO: or a value passed in with the desired element name will
				{
					if (p.PropertyType == itemType)
					{
						pObj = itemType;
						return true;
					}

					if (p.PropertyType.IsGenericType &&
						(p.PropertyType.GetGenericArguments()[0] == itemType //this will not work unless it's an exact type match
							|| p.PropertyType.GetGenericArguments()[0].IsAssignableFrom(itemType))
						) //e.g., like: List<ExtensionBaseType> Items, with [XmlElement("SelectionTest", typeof(PredSelectionTestType), Order=0)]
					{
						pObj = itemType;
						return true;
					}

					if (p.PropertyType.IsArray &&
						(p.PropertyType.GetElementType() == itemType //this will not work unless it's an exact type match
							|| p.PropertyType.GetElementType()!.IsAssignableFrom(itemType))
						)//e.g., like: ExtensionBaseType[] Items, with [XmlElement("ValidateForm", typeof(ActValidateFormType), Order=0)]
					{
						pObj = itemType;
						return true;
					}
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
		/// <param name="parentNode">Parent node of <paramref name="item"/>. </param>
		/// <param name="piNewParentProperty">The PropertyInfo object that defines the parent node to which 
		/// the <paramref name="item"/> node should be moved.</param>
		/// <param name="itemElementName">The XML Element name for item name.  If null (the default), the method will attempt to determine it.</param>
		/// <returns>True for allowed parent nodes, false for disallowed parent nodes, 
		/// where the parent node is defined by <paramref name="piNewParentProperty"/>.</returns>
		internal static bool IsParentNodeAllowed(BaseType item, BaseType? parentNode, PropertyInfo piNewParentProperty, string? itemElementName = null)
		{
			if (item is null) throw new ArgumentNullException(nameof(item), "Argument cannot be null.");
			if (piNewParentProperty is null) throw new ArgumentNullException(nameof(piNewParentProperty), "Argument cannot be null.");

			Type itemType = item.GetType();
			if (itemElementName is null || itemElementName.IsNullOrWhitespace()) itemElementName = item.GetPropertyInfoMetaData(parentNode).XmlElementName;

			var pAtts = piNewParentProperty.GetCustomAttributes<XmlElementAttribute>();

			if (pAtts.Any())
			{
				foreach (var a in pAtts)
				{
					if (a.ElementName == itemElementName)
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
				if (piNewParentProperty.Name == itemElementName)
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
		public static T[] RemoveArrayNullsNew<T>(T[] array) where T : class
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

		#region Helpers
		#region TE Helpers
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
			if (node is InjectFormType) return ItemTypeEnum.InjectForm;
			if (node is ButtonItemType) return ItemTypeEnum.Button;

			return ItemTypeEnum.None;
		}
		#endregion

		/// <summary>
		/// Enum used in <see cref="CreateCAPname"/> to control the  way that new SDC node names are assigned.
		/// </summary>
		public enum NameChangeEnum {
			/// <summary>
			/// Rename all auto-generated and null names, preserving all custom names.<br/>
			/// Auto-generated names start with the default ElementPrefix, followed by "_", <br/>
			/// e.g., the prefix "p_" is used for PropertyType nodes.<br/>
			/// Auto-generated Question names begin with 1-3 letters specific to the question type, <br/>
			/// (e.g., Q, QS, QR, QLS, QLM), followed by "_". 
			/// </summary>
			Normal, 
			/// <summary>
			/// Give every node a new name.
			/// </summary>
			RenameAll,
			/// <summary>
			/// Preserve all existing names.<br/>
			/// Assign new names if the current name is null or empty.
			/// </summary>
			PreserveAll
		}
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
		/// <returns>A consistent, CKey-aware value, for <see cref="BaseType.name"/> on <paramref name="node">.</paramref></returns>
		public static string CreateCAPname(this BaseType node, string initialTextToSkip = "", NameChangeEnum changeType = NameChangeEnum.Normal)
		{
			string nodeName = node.name ?? "";
			string nodeBaseName = node.BaseName ?? "";
			string nodePrefix = node.ElementPrefix ?? "";
			
			string namePrefix;
			string nameBody;
			string nameSuffix;
			Type nodeType = node.GetType();
			namePrefix = GetNamePrefix(node);

			if (changeType == NameChangeEnum.PreserveAll
				&& ! nodeName.IsNullOrWhitespace())
				return nodeName;


			if (changeType == NameChangeEnum.Normal)
			{
				int nodePrefixLength = nodePrefix.Length + 1;
				int namePrefixLength = namePrefix.Length + 1;

				//Check for a custom node name that should be preserved unchanged.
				//Custom node names are strings that do not start with node.ElementPrefix + "_"
				if (nodeType != typeof(QuestionItemType)
					&& nodeName.Length > nodePrefixLength
					&& nodeName[..nodePrefixLength] != $"{nodePrefix}_")
					return nodeName;
				//Check for a custom Question node name that should be preserved unchanged.
				//Auto-generated QuestionItemType nodes have the namePrefix morphed according to the type of question (e.g., "QM", QS", "QR")
				//so we can't rely on the original nodePrefix ("Q" by default).
				//If we don't detect an auto-generated name (i.e., one that uses the auto-generated namePrefix), then it's a custom name and we'll preserve it unchanged.
				else if ( //it's a QuestionItemType here...
					nodeName.Length > namePrefixLength
					&& nodeName[..namePrefixLength] != $"{namePrefix}_")
					return nodeName;

				//check for initialTextToSkip
				int len = initialTextToSkip.Length;
				if (node.name?.Length > 0 && node.name.AsSpan(0, len) == initialTextToSkip)
					return nodeName;  //Return the existing name, if it starts with "_"
			}
			//!nameBody
			{
				//Try using the closest IdentifiedExtensionType node's Ckey-formatted (decimal format) ID to generate nameBody
				//Use special names for headr/body/footer nodes
				//use an sGuid if the Ckey/ID approach does not work.
				const string nameSpace = ".100004300";

				if (node is IdentifiedExtensionType iet)
				{
					if (iet.ID.Contains(nameSpace)) //&& iet.ID.Length > 10)
						nameBody = Regex.Replace(iet.ID.Replace(nameSpace, "") ?? "", @"\W+", ""); //remove namespace and special characters
					else nameBody = (nodeBaseName != "") ? nodeBaseName : CreateBaseNameFromsGuid(node);

					if (nodeName.ToLower() == "body") nameBody = $"body.{nameBody}";
					else if (nodeName.ToLower() == "footer") nameBody = $"footer{nameBody}";
					else if (nodeName.ToLower() == "header") nameBody = $"header.{nameBody}";
				}
				else //not IdentifiedExtensionType
				{
					IdentifiedExtensionType? ancestorIet = node.ParentIETnode;
					if (ancestorIet is not null)
					{
						string ancBaseName = ancestorIet.BaseName ?? "";
						if (ancestorIet.ID?.Contains(nameSpace) ?? false)
							nameBody = Regex.Replace(node.ParentIETnode?.ID.Replace(nameSpace, "") ?? "", @"\W+", ""); //remove namespace and special characters
						else if (ancBaseName != "") nameBody = ancBaseName;
						else nameBody = (nodeBaseName != "") ? nodeBaseName : CreateBaseNameFromsGuid(node);
					}
					else //ancestorIet is null
						nameBody = (nodeBaseName != "") ? nodeBaseName : CreateBaseNameFromsGuid(node);
				}
			}
			
			nameSuffix = node.SubIETcounter.ToString(); 
			if(nameSuffix == "0")
				return $"{namePrefix}_{nameBody}";
			else 
				return $"{namePrefix}_{nameBody}_{nameSuffix}";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static string GetNamePrefix(BaseType node)
		{
			string namePrefix;
			string nodeName = node.name ?? "";
			string nodePrefix = node.ElementPrefix ?? "";
			//!Question Prefix
			if (node is QuestionItemType Q)
			{
				var st = Q.GetQuestionSubtype();
				switch (st)
				{
					case QuestionEnum.QuestionRaw:
						return "Q";
					case QuestionEnum.QuestionSingle:
						return "QS";
					case QuestionEnum.QuestionMultiple:
						return "QM";
					case QuestionEnum.QuestionSingleOrMultiple:
						return "QSM";
					case QuestionEnum.QuestionFill:
						return "QR";
					case QuestionEnum.QuestionLookup:
						return "QL";
					case QuestionEnum.QuestionLookupSingle:
						return "QLS";
					case QuestionEnum.QuestionLookupMultiple:
						return "QLM";
					case QuestionEnum.QuestionGroup:
						//return "Q";
						throw new InvalidOperationException("Could not determine Question Subtype (QuestionEnum.QuestionGroup)");
					default:
						//return "Q";
						throw new InvalidOperationException("Could not determine Question Subtype");
				}
			}
			else //!Property Prefix
			if (node is PropertyType pt)				
			{
				string propName = pt.propName ?? "";
				//special cases:
				if (nodeName.Length > 1 && nodeName.Substring(0, 2) != $"{nodePrefix}_") return "";
				else if (propName == "reportText") return "p_rptText";
				else if (propName == "altText") return "p_altText";
				else return $"{nodePrefix}_{propName.AsSpan(0, Math.Min(8, propName.Length)).ToString()}";
			}
			else //!Other Prefix
			{
				namePrefix = node.ElementPrefix ?? "";
				if (!namePrefix.IsNullOrWhitespace()) return namePrefix;

				string elementName = node.ElementName ?? "";
				if (elementName.IsNullOrWhitespace()) elementName = node.GetType().Name;
				namePrefix = $"{elementName.TakeWhile(c => Char.IsUpper(c)).ToString()?.ToLower()}"; //backup method for ElementPrefix: use uppercase letters in ElementName
				if (namePrefix.Length < 3) return elementName.Substring(0, Math.Min(6, elementName.Length)).ToLower();
			}
			return namePrefix ?? "";
		}


		//
		/// <summary>
		/// Create a consistent unique name for the passed SDC node. The name is formatted like:<code><br/>
		///	ElementPrefix_BaseNameOfParentIETNode_SubIETcounter</code><br/>
		/// Requires that ElementPrefix and either sGuid or BaseName have values.  <br/>
		/// To work properly, SubIETcounter requires that ancestor nodes are registered in their TopNode Dictionaries.
		/// </summary>
		/// <param name="bt">The node for which the name will be created.</param>
		/// <param name="initialTextToSkip">If an existing name value starts with this string, the existing name will be reused, and will not be replaced with a new value. </param>
		/// <param name="changeType">The value is ignored</param>
		/// <returns>A consistent value for the @name attribute.</returns>
		public static string CreateSimpleName(BaseType bt, string initialTextToSkip = "", NameChangeEnum changeType = NameChangeEnum.Normal)
		{
			if (bt.name?.Length > 0 && bt.name.AsSpan(0, 1) == initialTextToSkip) return bt.name;  //Return the existing name, if it starts with "_"
			string baseName = "";
			if (bt is not IdentifiedExtensionType)
			{
				var iet = bt.ParentIETnode;
				if (iet is not null)
					baseName = iet.BaseName;
				else
					if(bt.BaseName is not null) 
						baseName = bt.BaseName;
			}
			else baseName = bt.BaseName;

			if (baseName.IsNullOrWhitespace())
				if (bt.sGuid is not null)
				{
					//baseName = CreateBaseNameFromsGuid(bt.sGuid);
					//baseName = 
					AssignGuid_sGuid_BaseName(bt);
					//bt.BaseName = baseName;
				}
				else
					throw new InvalidOperationException("supplied node did not have sGuid assigned.");
			
			string name = new StringBuilder(bt.ElementPrefix)
						.Append('_')
						.Append(baseName)
						.Append('_')
						.Append(bt.SubIETcounter).ToString();
			return name;
		}

		//TODO: names prefixed with "_" will be preserved.
		//TODO: names and IDs will be added to internal dictionaries to ensure uniqueness within a template.
		//TODO: create a default naming system that uses the first 6 good characters of the sGuid instead of the ID, for the "ID part" of the name.
		//			skip: starting numbers, -, _, 0 and move on to the next letter; then change to all lower case.
		//TODO: check for unacceptable words in sGuids and names; this provides almost 2 billion choices for each template 

		/// <summary>
		/// Process the characters in a node's short Guid (sGuid) to create a alphanumeric string suiatable for use 
		/// as a programming variable name, or for part of such a name, or for use as part/all of an <see cref="IdentifiedExtensionType.ID"/>.
		/// </summary>
		/// <param name="sGuid">A ShortGuid used to generate a BaseName</param>
		/// <param name="node">The <see cref="BaseType"/>node for which we want to create a BaseName.  This node must have a valid sGuid. </param>
		/// <param name="minNameBaseLength">The length of the alphanumeric string to return.<br/>
		/// In some cases, the string may be shorter or longer than this length, due to removal of illegal characters (0, -, and _), <br/>
		/// as well as removal of any numbers at the first character of the string.  <br/>
		/// In addition, if a name collision occurs in the hashtable <see cref="UniqueBaseNames"/>, sGuid characters will be added <br/>
		/// until there is no longer a collision, and thus the returned BaseName string may be longer than  <paramref name="minNameBaseLength"/></param>
		/// <returns>The method tries to find an sGuid-derived string of length <paramref name="minNameBaseLength" />, more or less, that does not contain unusual characters.<br/>
		/// May rarely return an empty string or a string shorter or longer than <paramref name="minNameBaseLength" /> if a name collision occurs in the hashtable <see cref="UniqueBaseNames"/>.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static string CreateBaseNameFromsGuid(BaseType node, int minNameBaseLength = 6)
		{ //TODO: change to private after all testing complete
		  //Basic check for sGuid validity
		  //if (nameBaseLength > 22 || nameBaseLength < 1)
		  //	throw new ArgumentException("nameBaseLength must be > 0 and < 23");

			//Regex pattern = new("^[a-zA-Z0-9-_]{22}");
			//if (!pattern.IsMatch(sGuid))
			//	if (sGuid.IsNullOrWhitespace() || sGuid.Length != 22 || !pattern.IsMatch(sGuid)) 
			//		throw new ArgumentException("The supplied sGuid is not valid");
			//var newGuid = Guid.NewGuid();
			//var sGuid = ShortGuid.Encode(newGuid);

			string sGuid = node.sGuid;

			if (!ShortGuid.TryParse(sGuid, out ShortGuid _))
				throw new ArgumentException("The supplied sGuid is not valid");

			var sgl = sGuid.ToList();
			var UniqueBaseNames = ((_ITopNode)node.TopNode!)._UniqueBaseNames;
			int i = -1;
			do
			{ //remove any integer, -, or _ in the first position, as these are illegal for variable names
				i++;
				char c = sgl[0];
				if ((c >= '0' && c <= '9') || c == '_' || c == '-')
					sgl.RemoveAt(0);
				else break;
			} while (i < sgl.Count);

			i = 0;
			do
			{ //remove any 0, -, or _ in any remaining position, as these do not make nice variable names
				char c2 = sgl[i];
				if (c2 == '0' || c2 == '_' || c2 == '-')
					sgl.RemoveAt(i);
				else i++;

			} while (i < sgl.Count);
			// while (i <= minNameBaseLength && i<sgl.Count);
			//++-----------------------------

			var sb = new StringBuilder().Append(sgl.ToArray()[0..minNameBaseLength]);
			//foreach (var c in sgl.Take(minNameBaseLength)) sb.Append(c);

			string newBaseName = sb.ToString();


			if (! UniqueBaseNames.TryGetValue(newBaseName, out _))
			{
				UniqueBaseNames.Add(newBaseName);
				return newBaseName;
			}

			//add to newBaseName one char at a time until it is unique within UniqueBaseNames
			//it's unlikely that we'll get here
			while (sb.Length < sgl.Count)
			{
				
				sb.Append(sgl[minNameBaseLength]);
				newBaseName = sb.ToString();
				if (! UniqueBaseNames.TryGetValue(newBaseName, out _))
				{
					UniqueBaseNames.Add(newBaseName);
					return newBaseName;
				}
				minNameBaseLength++;
			}
			return "";  //hopefully, we'll never get here

		}

		/// <summary>
		/// Assign <see cref="BaseType.sGuid"/>, <see cref="BaseType.ObjectGUID"/>, <see cref="BaseType.BaseName"/> and <see cref="BaseType.ObjectID"/><br/>
		/// and return a BaseName, based on an existing sGuid, if present.<br/>
		/// If <see cref="BaseType.sGuidl"/>  is null, assigns <see cref="BaseType.sGuid"/>, <see cref="BaseType.ObjectGUID"/>, <see cref="BaseType.BaseName"/> and <see cref="BaseType.ObjectID"/>
		/// </summary>
		/// <param name="bt">The input node for which we want to assiggn, if needed, <see cref="BaseType.sGuid"/>, <see cref="BaseType.ObjectGUID"/>, <see cref="BaseType.BaseName"/> and <see cref="BaseType.ObjectID"/>.</param>
		/// <param name="forceNewGuid">If true, this method will assign new <see cref="BaseType.sGuid"/>, <see cref="BaseType.ObjectGUID"/>, <see cref="BaseType.BaseName"/> and <see cref="BaseType.ObjectID"/>, even if an sGuid already exists for <paramref name="bt"/>.</param>
		/// <param name="minNameBaseLength">If a new sGuid wil be created, <paramref name="minNameBaseLength"/> is the requested length of BaseName that is derived from the sGuid.<br/>
		/// If the value is less then 4, a value of 4 will be used.  If the value is greater than 10, 10 will be used</param>
		/// <returns>BaseName</returns>
		internal static string AssignGuid_sGuid_BaseName(BaseType bt, bool forceNewGuid = false, int minNameBaseLength = 6)
		{
			if (minNameBaseLength < 4) minNameBaseLength = 4;
			string tempName;
			if (bt.ObjectID == -1 && bt.TopNode is not null && bt is not ITopNode) 
				bt.ObjectID = ((_ITopNode)bt.TopNode)._MaxObjectID++;

			if (! forceNewGuid && ShortGuid.TryParse(bt.sGuid, out Guid guid))
			{
				bt.ObjectGUID = guid;
				bt.BaseName = CreateBaseNameFromsGuid(bt, minNameBaseLength);
				return bt.BaseName;
			}
			//!+------We need a new GUID, sGuid and BaseName-------------------------------------------------------------
			Guid newGuid; 
			string sGuid;
			do
			{ //make sure ObjectGuid results is a nice tempName string; this should take only 1 iteration thru the loop
				newGuid = Guid.NewGuid();
				sGuid = ShortGuid.Encode(newGuid);
				bt.ObjectGUID = newGuid;
				bt.sGuid = sGuid;
				tempName = CreateBaseNameFromsGuid(bt, minNameBaseLength);
			} while (tempName.Length != minNameBaseLength);  //pick an sGuid that is capable of producing a BaseName of the requested length (minNameBaseLength)

			bt.BaseName = tempName;
			//UniqueBaseNames.Add(tempName); //TODO: Add all BaseNames to UniqueBaseNames for all new nodes, and after deserializing an SDC tree
			return tempName;
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

			if (!TreeSort_IsSorted(parentItem) && kids is not null)
			{
				kids.Sort(new TreeSibComparer());
				TreeSort_Add(parentItem);
			}
			return kids;
		}

		/// <summary>
		/// Returns formatted XML a minified or poorly formatted XML string
		/// </summary>
		/// <param name="Xml">The input XML to be formatted</param>
		/// <returns></returns>
		public static string FormatXml(string Xml)
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
		public static string ReorderXml(string Xml, int orderMultiplier = 1)
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
		internal static _ITopNode? Get_ITopNode(BaseType n)
		{
			if (n is _ITopNode itn) return itn;
			return n.TopNode as _ITopNode;
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

		private static void X_AssignXmlElementAndOrder<T>(T bt, BaseType parentNode) where T : notnull, BaseType
		{
			var pi = SdcUtil.GetElementPropertyInfoMeta(bt, parentNode);
			bt.ElementName = pi.XmlElementName ?? "";
			bt.ElementOrder = pi.XmlOrder;
		}
		private static object? X_GetPropertyObject(BaseType parent, PropertyInfo piProperty) =>
			piProperty.GetValue(parent);

		#endregion

	}

}
