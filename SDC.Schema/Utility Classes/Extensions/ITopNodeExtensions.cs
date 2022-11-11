using CSharpVitamins;
using SDC.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;



//using SDC;
namespace SDC.Schema.Extensions
{
	/// <summary>
	/// Extension methods for nodes implementing the SDC ITopNode interface, e.g., FormDesignType, RetrieveFormPackageType, etc.
	/// </summary>
	public static class ITopNodeExtensions
	{
		/// <summary>
		/// Convenience method to return (_ITopNode)itn, providing quick access to the _ITopNode interface methods
		/// </summary>
		/// <param name="itn"></param>
		/// <returns></returns>
		private static _ITopNode topNode(this ITopNode itn)
		{ return (_ITopNode)itn; }
		/// <summary>
		/// Convenience method to return ((_ITopNode)itn)._Nodes
		/// </summary>
		/// <param name="itn"></param>
		/// <returns></returns>
		private static Dictionary<Guid, BaseType> _Nodes(this ITopNode itn)
		{ return ((_ITopNode)itn)._Nodes; }

		/// <summary>
		/// Traverse the entire ITopNode tree by reflection, optionally rewriting @order for each node, <br/>
		/// and optionally re-registering all nodes in the _ITopNode dictionaries
		/// and optionally creating new @name attributes for each node
		/// and optionally assigning the ElementName (the name of the serialized XML element) property for each node
		/// </summary>
		/// <param name="itn"></param>
		/// <param name="reOrderNodes">bool; true will add new sequential @order values to each node</param>
		/// <param name="reRegisterNodes">bool; Clear and re-write all _ITopNode dictionaries</param>
		/// <param name="startReorder">int; the starting number for the first node if reOrderNodes = true</param>
		/// <param name="orderInterval">int; The interval between @order values in sequential nodes, if reOrderNodes = true</param>
		/// <returns>A sorted <see cref="List&lt;T>"/> of type <see cref="BaseType"/> containing references to every tree node</returns>
		public static List<BaseType> RefreshTree(this ITopNode itn, bool reOrderNodes =  true, bool reRegisterNodes = true, int startReorder = 0, int orderInterval = 1)
		{	//no dictionaries are used for sorting here:
			return SdcUtil.ReflectRefreshSubtreeList((BaseType)itn.TopNode, reOrder: reOrderNodes, reRegisterNodes: reRegisterNodes, startReorder: 0, orderInterval: 1 );
		}
		/// <summary>
		/// Assign the ElementName (the name of the serialized XML element) property for each node, <br/>
		/// starting from this ITopNode node, using reflection
		/// </summary>
		/// <param name="itn"></param>
		/// <returns></returns>
		public static void AssignElementNamesByReflection(this ITopNode itn)
		{
			foreach (var kvp in _Nodes(itn))
			{
				BaseType bt;
				bt = kvp.Value;
				bt.ElementName = bt.GetPropertyInfoMetaData().XmlElementName ?? "";
			}
			//return true;
		}

		/// <summary>
		/// Returns a sorted <see cref="ObservableCollection&lt;BaseType>" /> of type <see cref="BaseType"/> containing <br/>
		/// all nodes with a root node at the current ITopNode node.
		/// </summary>
		/// <param name="itn"></param>
		/// <returns><see cref="ObservableCollection{BaseType}"/></returns>
		public static ObservableCollection<BaseType> GetSortedNodesObsCol(this ITopNode itn)
		=> new (itn.GetSortedNodes());

		/// <summary>
		/// Attempt to retrieve an <see cref="IdentifiedExtensionType"/> node by its SDC @ID attribute.
		/// </summary>
		/// <param name="itn">The ITopNode node instance</param>
		/// <param name="id">The SDC @ID attribute value for which we are attempting to retrieve the node</param>
		/// <param name="iet">An out parameter containing the retrieved <see cref="IdentifiedExtensionType"/> node, if the retrieval was successful</param>
		/// <returns>true if successful; false if not the <see cref="IdentifiedExtensionType"/> node was not found by @ID</returns>
		public static bool TryGetIETnodeByID(this ITopNode itn, string id, out IdentifiedExtensionType? iet)
		{
			iet = topNode(itn).GetIETnodeByID(id);
			if (iet is null) return false;
			return true;
		}
		/// <summary>
		/// Attempt to retrieve an <see cref="BaseType"/> node by its SDC @name attribute.
		/// </summary>
		/// <param name="itn">The ITopNode node instance</param>
		/// <param name="name">The SDC @name attribute value for which we are attempting to retrieve the node</param>
		///  <param name="node">An out parameter containing the retrieved <see cref="BaseType"/> node, if the retrieval was successful</param>
		/// <returns>true if successful; false if not the <see cref="BaseType"/> node was not found by @name</returns>
		public static bool TryGetNodeByName(this ITopNode itn, string name, out BaseType? node)
		{
			node = topNode(itn).GetNodeByName(name);
			if (node is null) return false;
			return true;
		}
		public static bool TryGetNodeByShortGuid(this ITopNode itn, string sGuid, out BaseType? node)
		{	node = topNode(itn).GetNodeByShortGuid(sGuid);
			if (node is null) return false;
			return true;
		}
		public static bool TryGetNodeByIndex(this ITopNode itn, int index, out BaseType? node)
		{
			node = topNode(itn).GetNodeByPositionIndex(index);
			if (node is null) return false;
			return true;
		}
		public static bool TryGetNodeByObjectID(this ITopNode itn, int ObjectID, out BaseType? node)
		{
			node = topNode(itn).GetNodeByObjectID(ObjectID);
			if (node is null) return false;
			return true;
		}
		public static IdentifiedExtensionType? GetIETnodeByID(this ITopNode itn, string id)=>
			topNode(itn)._IETnodes
				.Where(n => n.ID.Trim() == id.Trim()).FirstOrDefault();
		public static BaseType? GetNodeByName(this ITopNode itn, string name)=>
			topNode(itn)._Nodes.Values
				.Where(n => n?.name?.Trim() == name.Trim()).FirstOrDefault();
		public static BaseType? GetNodeByShortGuid(this ITopNode itn, string sGuid) =>
			topNode(itn)._Nodes.Values
			.Where(n => n.sGuid == sGuid.Trim()).FirstOrDefault();
		public static BaseType? GetNodeByObjectGUID(this ITopNode itn, Guid objectGUID)
		{
			topNode(itn)._Nodes.TryGetValue(objectGUID, out BaseType? n);
			return n;
		}

		public static BaseType? GetNodeByPositionIndex(this ITopNode itn, int index) =>
			topNode(itn)?.GetSortedNodes()[index];
		public static BaseType? GetNodeByObjectID(this ITopNode itn, int ObjectID)=>	
			topNode(itn)?._Nodes.Values.Where(n => n.ObjectID == ObjectID).FirstOrDefault();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="itn"></param>
		/// <returns></returns>
		public static List<BaseType> GetSortedNodes(this ITopNode itn)
		{
			return SdcUtil.GetSortedTreeList(itn);
		}

		/// <summary>
		/// Assign the ElementName (the name of the serialized XML element) property for each node, <br/>
		/// by comparison with the source XML document that was used to hydrate the SDC object tree
		/// </summary>
		/// <param name="itn"></param>
		/// <param name="sdcXml"></param>
		private static void X_AssignElementNamesFromXmlDoc(this ITopNode itn, string sdcXml)
		{
			//read as XMLDocument to walk tree
			var x = new XmlDocument();
			x.LoadXml(sdcXml);
			XmlNodeList? xmlNodeList = x.SelectNodes("//*");
			int iXmlNode = 0;
			XmlNode? xmlNode;

			foreach (BaseType bt in _Nodes(itn).Values)
			{   //As we iterate through the nodes, we will need code to skip over any non-element node, 
				//and still stay in sync with FD (using iFD). For now, we assume that every nodeList node is an element.
				//https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlnodetype?view=netframework-4.8
				//https://docs.microsoft.com/en-us/dotnet/standard/data/xml/types-of-xml-nodes
				xmlNode = xmlNodeList?[iXmlNode];
				while (xmlNode?.NodeType.ToString() != "Element")
				{
					iXmlNode++;
					xmlNode = xmlNodeList?[iXmlNode];
				}

				var e = (XmlElement)xmlNode;
				bt.ElementName = e.LocalName;
				iXmlNode++;
			}
		}

		#region Utilities        
		/// <summary>
		/// Retrieve a dictionary containing information about the populated SDC attributes <br/>
		/// on each <see cref="BaseType"/> node in the <see cref="ITopNode"/> tree.<br/>
		/// Populated attributes are XML attributes that will be serialized to XML.
		/// The dictionary format is <see cref="Dictionary{TKey, TValue}"/> where TKey is the SDC <see cref="BaseType.sGuid"/> attribute, <br/>
		/// and TValue is <see cref="List{AttributeInfo}"/>, where T is <see cref="AttributeInfo"/>. <br/>
		/// Each <see cref="List{AttributeInfo}"/> contains an <see cref="AttributeInfo"/> entry for each populated attributed for a single node.<br/> 
		/// </summary>
		/// <param name="itn">The <see cref="ITopNode"/> object that is the top node that defines the root of the SDC tree to analyze </param>
		/// <param name="log"><paramref name="log"/> is a string "out" parameter listing all <<see cref="BaseType"/>nodes <br/>
		/// and their serializable SDC XML attributes in the order they will appear in the XML.<br/>
		/// The log string is populated only if <paramref name="doLog"/> is set to true</param>
		/// <param name="doLog">Set <paramref name="doLog"/> to true to produce a log of all populated attributes in the out parameter <paramref name="log"/>
		/// </param>
		public static SortedList<string, Dictionary<string, List<AttributeInfo>>> GetXmlAttributesFilled(this ITopNode itn, out string log, bool doLog = false)
		{
			SdcUtil.ReflectRefreshTree(itn, out _);
			StringBuilder sb= new();

			SortedList<string, Dictionary<string, List<AttributeInfo>>> dictAttr = new();

			char gt = ">"[0];
			//  ------------------------------------------------------------------------------------
			foreach (IdentifiedExtensionType n in itn.IETnodes)
			{
				var en = n.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;

				if(doLog) sb.Append ($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}\r\n");
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  IET Node: {en}   {"".PadRight(pad, gt)}");
				var sublist = n.GetSubtreeIETList();

				Dictionary<string, List<AttributeInfo>> dlai = new();

				foreach (var subNode in sublist)
				{
					var lai = subNode.GetXmlAttributesSerialized();
					Log(subNode, lai);
					dlai.Add(subNode.sGuid, lai);

				}
				dictAttr.Add(n.sGuid, dlai);
			}
			log = sb.ToString();
			return dictAttr;

			//  ------------------------------------------------------------------------------------
			void Log(BaseType subNode, List<AttributeInfo> lai)
			{
				var en = subNode.ElementName;
				int enLen = 36 - en.Length;
				int pad = (enLen > 0) ? enLen : 0;
				if (doLog) sb.Append($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {"".PadRight(pad, gt)}\r\n");
				if (doLog) sb.Append("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>\r\n");
				Debug.Print($"<<<<<<<<<<<<<<<<<<<<<<<  SubNode: {en}    {"".PadRight(pad, gt)}");
				Debug.Print("<==<==<== Attr ==>==>==>| Default Val |<==<==<==<==<== Val ==>==>==>==>==>");
				foreach (AttributeInfo ai in lai)
				{
					if (doLog) sb.Append($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? "").PadRight(13)}| {ai.AttributeValue?.ToString()}\r\n");
					Debug.Print($"{ai.Name.PadRight(24)}|{(ai.DefaultValue?.ToString() ?? "").PadRight(13)}| {ai.AttributeValue?.ToString()}");
				}
			}
		}

		private static ITopNode InitParentNodesFromXml(this ITopNode itn, string sdcXml)
		{
			//read as XMLDocument to walk tree
			var x = new System.Xml.XmlDocument();
			x.LoadXml(sdcXml);
			XmlNodeList? xmlNodeList = x.SelectNodes("//*");
			if (xmlNodeList is null) return null;
			var dX_obj = new Dictionary<int, Guid>(); //the index is iXmlNode, value is FD ObjectGUID
			int iXmlNode = 0;
			XmlNode? xmlNode;

			foreach (BaseType bt in itn.Nodes.Values)
			{   //As we interate through the nodes, we will need code to skip over any non-element node, 
				//and still stay in sync with FD (using iFD). For now, we assume that every nodeList node is an element.
				//https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlnodetype?view=netframework-4.8
				//https://docs.microsoft.com/en-us/dotnet/standard/data/xml/types-of-xml-nodes
				xmlNode = xmlNodeList[iXmlNode];
				while (xmlNode?.NodeType.ToString() != "Element")
				{
					iXmlNode++;
					xmlNode = xmlNodeList[iXmlNode];
				}
				//Create a new attribute node to hold the node's index in xmlNodeList
				XmlAttribute a = x.CreateAttribute("index");
				a.Value = iXmlNode.ToString();
				var e = (XmlElement)xmlNode;
				e.SetAttributeNode(a);

				//Set the correct Element Name, in case we have errors in the SDC object tree logic
				bt.ElementName = e.LocalName;

				//Create  dictionary to track the matched indexes of the XML and FD node collections
				dX_obj[iXmlNode] = bt.ObjectGUID;
				//Debug.Print("iXmlNode: " + iXmlNode + ", ObjectID: " + bt.ObjectID);

				//Search for parents:
				int parIndexXml = -1;
				Guid parObjectGUID = default;
				bool parExists = false;
				BaseType btPar;
				XmlNode? parNode;
				btPar = null!;

				parNode = xmlNode.ParentNode;
				parExists = int.TryParse(parNode?.Attributes?.GetNamedItem("index")?.Value, out parIndexXml);//The index of the parent XML node
				if (parExists)
				{
					parExists = dX_obj.TryGetValue(parIndexXml, out parObjectGUID);// find the matching parent SDC node Object ID
					if (parExists) { parExists = itn.Nodes.TryGetValue(parObjectGUID, out btPar!); } //Find the parent node in FD
					if (parExists)
					{
						//bt.IsLeafNode = true;
						bt.RegisterNodeAndParent(btPar!, childNodesSort: false);
						//Debug.WriteLine($"The node with ObjectID: {bt.ObjectID} is leaving InitializeNodesFromSdcXml. Item type is {bt.GetType().Name}.  " +
						//            $"Parent ObjectID is {bt?.ParentID}, ParentIETypeID: {bt?.ParentIETypeID}, ParentType: {btPar.GetType().Name}");
					}
					else { throw new KeyNotFoundException("No parent object was returned from the SDC tree"); }
				}
				else
				{
					//bt.IsLeafNode = false;
					//Debug.WriteLine($"The node with ObjectID: {bt.ObjectID} is leaving InitializeNodesFromSdcXml. Item type is {bt.GetType()}.  " +
					//                $", No Parent object exists");
				}

				iXmlNode++;
			}
			return itn;

		}

		#endregion

		#region GetItems

		public static Dictionary<Guid, BaseType> GetDescendantDictionary(this ITopNode itn, BaseType topNode)
		{
			var d = new Dictionary<Guid, BaseType>();
			getKids(topNode);

			void getKids(BaseType node)
			{
				//For each child, get all their children recursively, then add to dictionary
				var vals = (Dictionary<Guid, BaseType>)_Nodes(itn).Values.Where(n => n.ParentID == node.ParentID);
				foreach (var n in vals)
				{
					getKids(n.Value); //recurse
					d.Add(n.Key, n.Value);
				}
			}
			return d;
		}
		public static List<BaseType> GetDescendantList(this ITopNode itn, BaseType topNode)
		{
			var curNode = itn;
			var lst = new List<BaseType>();
			getKids((BaseType)itn);

			void getKids(BaseType node)
			{
				//For each child, get all their children recursively, then add to dictionary
				var vals = (Dictionary<int, BaseType>)_Nodes(itn).Values.Where(n => n.ParentID == curNode.ParentID);
				foreach (var n in vals)
				{
					getKids(n.Value); //recurse
					lst.Add(n.Value);
				}
			}

			return lst;
		}
		private static IdentifiedExtensionType? X_GetNodeByID(this ITopNode itn, string id)
		{
			IdentifiedExtensionType? iet;
			iet = (IdentifiedExtensionType?)_Nodes(itn).Values.Where(
				n => n is IdentifiedExtensionType nIet && nIet.ID == id).FirstOrDefault();
			return iet;
		}
		//public static BaseType? GetItemByName(this ITopNode itn, string name)
		//{
		//	BaseType? bt;
		//	bt = _Nodes(itn).Values.Where(
		//		n => n.name == name).FirstOrDefault();
		//	return bt;
		//}
		public static QuestionItemType? GetQuestionByID(this ITopNode itn, string id)
		{
			QuestionItemType? q;
			q = (QuestionItemType?)_Nodes(itn).Values.Where(
					t => t is QuestionItemType q && q.ID == id).FirstOrDefault();
			return q;
		}
		public static QuestionItemType? GetQuestionByName(this ITopNode itn, string name)
		{
			QuestionItemType? q;
			q = (QuestionItemType?)_Nodes(itn).Values.Where(
				t => t is QuestionItemType q && q.name == name).FirstOrDefault();
			return q;
		}
		public static DisplayedType? GetDisplayedTypeByID(this ITopNode itn, string id)
		{
			DisplayedType? d;
			d = (DisplayedType?)_Nodes(itn).Values.Where(
				t => t is DisplayedType d && d.ID == id).FirstOrDefault();
			return d;
		}
		public static DisplayedType? GetDisplayedTypeByName(this ITopNode itn, string name)
		{
			DisplayedType? d;
			d = (DisplayedType?)_Nodes(itn).Values.Where(
				t => t is DisplayedType d && d.name == name).FirstOrDefault();
			return d;
		}
		public static SectionItemType? GetSectionByID(this ITopNode itn, string id)
		{
			SectionItemType? s;
			s = (SectionItemType?)_Nodes(itn).Values.Where(
				t => t is SectionItemType s && s.ID == id).FirstOrDefault();
			return s;
		}
		public static SectionItemType? GetSectionByName(this ITopNode itn, string name)
		{
			SectionItemType? s;
			s = (SectionItemType?)_Nodes(itn).Values.Where(
				t => t is SectionItemType s && s.name == name).FirstOrDefault();
			return s;
		}
		public static ListItemType? GetListItemByID(this ITopNode itn, string id)
		{
			ListItemType? li;
			li = (ListItemType?)_Nodes(itn).Values.Where(
				t => t is ListItemType li && li.ID == id).FirstOrDefault();
			return li;
		}
		public static ListItemType? GetListItemByName(this ITopNode itn, string name)
		{
			ListItemType? li;
			li = (ListItemType?)_Nodes(itn).Values.Where(
				t => t is ListItemType li && li.name == name).FirstOrDefault();
			return li;
		}

		public static ButtonItemType? GetButtonByID(this ITopNode itn, string id)
		{
			ButtonItemType? b;
			b = (ButtonItemType?)_Nodes(itn).Values.Where(
				t => t is ButtonItemType b && b.ID == id).FirstOrDefault();
			return b;
		}
		public static ButtonItemType? GetButtonByName(this ITopNode itn, string name)
		{
			ButtonItemType? b;
			b = (ButtonItemType?)_Nodes(itn).Values.Where(
				t => t is ButtonItemType b && b.name == name).FirstOrDefault();
			return b;
		}
		public static InjectFormType? GetInjectFormByID(this ITopNode itn, string id)
		{
			InjectFormType? inj;
			inj = (InjectFormType?)_Nodes(itn).Values.Where(
				t => t is InjectFormType inj && inj.ID == id).FirstOrDefault();
			return inj;
		}
		public static InjectFormType? GetInjectFormByName(this ITopNode itn, string name)
		{
			InjectFormType? inj;
			inj = (InjectFormType?)_Nodes(itn).Values.Where(
				t => t is InjectFormType inj && inj.name == name).FirstOrDefault();
			return inj;
		}
		public static ResponseFieldType? GetResponseFieldByName(this ITopNode itn, string name)
		{
			ResponseFieldType? rf;
			rf = (ResponseFieldType?)_Nodes(itn).Values.Where(
				t => t is ResponseFieldType rf && rf.name == name).FirstOrDefault();
			return rf;
		}

		//BaseType GetResponseValByQuestionID(string id)
		//{

		//    var Q = GetQuestion(id);
		//    return Q.ResponseField_Item.Response.Item;

		//}
		public static PropertyType? GetPropertyByName(this ITopNode itn, string name)
		{
			PropertyType? p;
			p = (PropertyType?)_Nodes(itn).Values.Where(
				t => t is PropertyType p && p.name == name).FirstOrDefault();
			return p;
		}
		public static ExtensionType? GetExtensionByName(this ITopNode itn, string name)
		{
			ExtensionType? e;
			e = (ExtensionType?)_Nodes(itn).Values.Where(
				t => t is ExtensionType e && e.name == name).FirstOrDefault();
			return e;
		}
		public static CommentType? GetCommentByName(this ITopNode itn, string name)
		{
			CommentType? c;
			c = (CommentType?)_Nodes(itn).Values.Where(
				t => t is CommentType c && c.name == name).FirstOrDefault();
			return c;
		}
		public static ContactType? GetContactByName(this ITopNode itn, string name)
		{
			ContactType? c;
			c = (ContactType?)_Nodes(itn).Values.Where(
				t => t is ContactType c && c.name == name).FirstOrDefault();
			return c;
		}
		public static LinkType? GetLinkByName(this ITopNode itn, string name)
		{
			LinkType? l;
			l = (LinkType?)_Nodes(itn).Values.Where(
				t => t is LinkType l && l.name == name).FirstOrDefault();
			return l;
		}
		public static BlobType? GetBlobByName(this ITopNode itn, string name)
		{
			BlobType? b;
			b = (BlobType?)_Nodes(itn).Values.Where(
				t => t is BlobType b && b.name == name).FirstOrDefault();
			return b;
		}
		public static CodingType? GetCodedValueByName(this ITopNode itn, string name)
		{
			CodingType? c;
			c = (CodingType?)_Nodes(itn).Values.Where(
				t => t is CodingType c && c.name == name).FirstOrDefault();
			return c;
		}


		#endregion
	}
}
