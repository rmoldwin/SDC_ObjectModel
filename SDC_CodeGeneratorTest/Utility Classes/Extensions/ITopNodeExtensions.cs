using CSharpVitamins;
using SDC.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;



//using SDC;
namespace SDC.Schema
{
	public static class ITopNodeExtensions
	{
		private static ITopNode topNode(this ITopNodePublic itn)
		{ return (ITopNode)itn; }

		public static List<BaseType> ReorderNodes(this ITopNodePublic itn)
		{
			return SdcUtil.ReflectSubtreeList((BaseType)itn.TopNode, reOrder: true, reRegisterNodes: true);
		}
		public static bool AssignElementNamesByReflection(this ITopNodePublic itn)
		{
			foreach (var kvp in itn.Nodes)
			{
				BaseType bt;
				bt = kvp.Value;
				bt.ElementName = bt.GetPropertyInfoMetaData().XmlElementName ?? "";
			}
			return true;
		}

		public static void AssignElementNamesFromXmlDoc(this ITopNodePublic itn, string sdcXml)
		{
			//read as XMLDocument to walk tree
			var x = new XmlDocument();
			x.LoadXml(sdcXml);
			XmlNodeList xmlNodeList = x.SelectNodes("//*");
			int iXmlNode = 0;
			XmlNode xmlNode;

			foreach (BaseType bt in itn.Nodes.Values)
			{   //As we interate through the nodes, we will need code to skip over any non-element node, 
				//and still stay in sync with FD (using iFD). For now, we assume that every nodeList node is an element.
				//https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlnodetype?view=netframework-4.8
				//https://docs.microsoft.com/en-us/dotnet/standard/data/xml/types-of-xml-nodes
				xmlNode = xmlNodeList[iXmlNode];
				while (xmlNode.NodeType.ToString() != "Element")
				{
					iXmlNode++;
					xmlNode = xmlNodeList[iXmlNode];
				}

				var e = (XmlElement)xmlNode;
				bt.ElementName = e.LocalName;
				iXmlNode++;
			}
		}
		public static List<BaseType> GetSortedNodes(this ITopNodePublic itn)
		{
			return SdcUtil.GetSortedTreeList(itn);
		}

		public static ObservableCollection<BaseType> GetSortedNodesObsCol(this ITopNodePublic itn)
		=> new ObservableCollection<BaseType>(itn.GetSortedNodes());

		public static bool TryGetIetNodeByID(this ITopNodePublic itn, string id, out IdentifiedExtensionType? iet)
		{
			iet = topNode(itn).GetIetNodeByID(id);
			if (iet is null) return false;
			return true;
		}

		public static bool TryGetNodeByName(this ITopNodePublic itn, string name, out BaseType? node)
		{
			node = topNode(itn).GetNodeByName(name);
			if (node is null) return false;
			return true;
		}
		public static bool TryGetNodeByShortGuid(this ITopNodePublic itn, string sGuid, out BaseType? node)
		{	node = topNode(itn).GetNodeByShortGuid(sGuid);
			if (node is null) return false;
			return true;
		}
		public static bool TryGetNodeByIndex(this ITopNodePublic itn, int index, out BaseType? node)
		{
			node = topNode(itn).GetNodeByPositionIndex(index);
			if (node is null) return false;
			return true;
		}
		public static bool TryGetNodeByObjectID(this ITopNodePublic itn, int ObjectID, out BaseType? node)
		{
			node = topNode(itn).GetNodeByObjectID(ObjectID);
			if (node is null) return false;
			return true;
		}
		public static IdentifiedExtensionType? GetIetNodeByID(this ITopNodePublic itn, string id)=>
			topNode(itn).IETnodes
				.Where(n => n.ID.Trim() == id.Trim()).FirstOrDefault();
		public static BaseType? GetNodeByName(this ITopNodePublic itn, string name)=>
			topNode(itn).Nodes.Values
				.Where(n => n.name.Trim() == name.Trim()).FirstOrDefault();
		public static BaseType? GetNodeByShortGuid(this ITopNodePublic itn, string sGuid) =>
			topNode(itn).Nodes.Values
			.Where(n => n.sGuid.Trim() == sGuid.Trim()).FirstOrDefault();
		public static BaseType? GetNodeByObjectGUID(this ITopNodePublic itn, Guid objectGUID)
		{
			topNode(itn).Nodes.TryGetValue(objectGUID, out BaseType? n);
			return n;
		}

		public static BaseType? GetNodeByPositionIndex(this ITopNodePublic itn, int index) =>
			topNode(itn)?.GetSortedNodes()[index];
		public static BaseType? GetNodeByObjectID(this ITopNodePublic itn, int ObjectID)=>	
			topNode(itn)?.Nodes.Values.Where(n => n.ObjectID == ObjectID).FirstOrDefault();

		#region Utilities        
		/// <summary>
		/// Runs method BaseType.ResetSdcImport(), which resets TopNodeTemp, allowing the addition of a new TopNode for newly added BaseType objects.
		/// </summary>
		/// <param name="itn">The itn.</param>
		public static void ResetSdcImport(this ITopNodePublic itn) => BaseType.ResetSdcImport();

		#endregion

		#region GetItems

		public static Dictionary<Guid, BaseType> GetDescendantDictionary(this ITopNodePublic itn, BaseType topNode)
		{
			var d = new Dictionary<Guid, BaseType>();
			getKids(topNode);

			void getKids(BaseType node)
			{
				//For each child, get all their children recursively, then add to dictionary
				var vals = (Dictionary<Guid, BaseType>)itn.Nodes.Values.Where(n => n.ParentID == node.ParentID);
				foreach (var n in vals)
				{
					getKids(n.Value); //recurse
					d.Add(n.Key, n.Value);
				}
			}
			return d;
		}
		public static List<BaseType> GetDescendantList(this ITopNodePublic itn, BaseType topNode)
		{
			var curNode = itn;
			var lst = new List<BaseType>();
			getKids((BaseType)itn);

			void getKids(BaseType node)
			{
				//For each child, get all their children recursively, then add to dictionary
				var vals = (Dictionary<int, BaseType>)itn.Nodes.Values.Where(n => n.ParentID == curNode.ParentID);
				foreach (var n in vals)
				{
					getKids(n.Value); //recurse
					lst.Add(n.Value);
				}
			}

			return lst;
		}
		public static IdentifiedExtensionType? GetItemByID(this ITopNodePublic itn, string id)
		{
			IdentifiedExtensionType? iet;
			iet = (IdentifiedExtensionType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(IdentifiedExtensionType)).Where(
					t => ((IdentifiedExtensionType)t).ID == id).FirstOrDefault();
			return iet;
		}
		public static BaseType? GetItemByName(this ITopNodePublic itn, string name)
		{
			BaseType? bt;
			bt = itn.Nodes.Values.Where(
				n => n.name == name).FirstOrDefault();
			return bt;
		}
		public static QuestionItemType? GetQuestionByID(this ITopNodePublic itn, string id)
		{
			QuestionItemType? q;
			q = (QuestionItemType?)itn.Nodes.Values.Where(
					n => (n as QuestionItemType)?.ID == id).FirstOrDefault();
			return q;
		}
		public static QuestionItemType? GetQuestionByName(this ITopNodePublic itn, string name)
		{
			QuestionItemType? q;
			q = (QuestionItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(QuestionItemType)).Where(
					t => ((QuestionItemType)t).name == name).FirstOrDefault();
			return q;
		}
		public static DisplayedType? GetDisplayedTypeByID(this ITopNodePublic itn, string id)
		{
			DisplayedType? d;
			d = (DisplayedType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(DisplayedType)).Where(
					t => ((DisplayedType)t).ID == id).FirstOrDefault();
			return d;
		}
		public static DisplayedType? GetDisplayedTypeByName(this ITopNodePublic itn, string name)
		{
			DisplayedType? d;
			d = (DisplayedType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(DisplayedType)).Where(
					t => ((DisplayedType)t).name == name).FirstOrDefault();
			return d;
		}
		public static SectionItemType? GetSectionByID(this ITopNodePublic itn, string id)
		{
			SectionItemType? s;
			s = (SectionItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(SectionItemType)).Where(
					t => ((SectionItemType)t).ID == id).FirstOrDefault();
			return s;
		}
		public static SectionItemType? GetSectionByName(this ITopNodePublic itn, string name)
		{
			SectionItemType? s;
			s = (SectionItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(SectionItemType)).Where(
					t => ((SectionItemType)t).name == name).FirstOrDefault();
			return s;
		}
		public static ListItemType? GetListItemByID(this ITopNodePublic itn, string id)
		{
			ListItemType? li;
			li = (ListItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(ListItemType)).Where(
					t => ((ListItemType)t).ID == id).FirstOrDefault();
			return li;
		}
		public static ListItemType? GetListItemByName(this ITopNodePublic itn, string name)
		{
			ListItemType? li;
			li = (ListItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(ListItemType)).Where(
					t => ((ListItemType)t).name == name).FirstOrDefault();
			return li;
		}

		public static ButtonItemType? GetButtonByID(this ITopNodePublic itn, string id)
		{
			ButtonItemType? b;
			b = (ButtonItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(ButtonItemType)).Where(
					t => ((ButtonItemType)t).ID == id).FirstOrDefault();
			return b;
		}
		public static ButtonItemType? GetButtonByName(this ITopNodePublic itn, string name)
		{
			ButtonItemType? b;
			b = (ButtonItemType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(ButtonItemType)).Where(
					t => ((ButtonItemType)t).name == name).FirstOrDefault();
			return b;
		}
		public static InjectFormType? GetInjectFormByID(this ITopNodePublic itn, string id)
		{
			InjectFormType? inj;
			inj = (InjectFormType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(InjectFormType)).Where(
					t => ((InjectFormType)t).ID == id).FirstOrDefault();
			return inj;
		}
		public static InjectFormType? GetInjectFormByName(this ITopNodePublic itn, string name)
		{
			InjectFormType? inj;
			inj = (InjectFormType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(InjectFormType)).Where(
					t => ((InjectFormType)t).name == name).FirstOrDefault();
			return inj;
		}
		public static ResponseFieldType? GetResponseFieldByName(this ITopNodePublic itn, string name)
		{
			ResponseFieldType? rf;
			rf = (ResponseFieldType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(ResponseFieldType)).Where(
					t => ((ResponseFieldType)t).name == name).FirstOrDefault();
			//rf.Response.Item.GetType().GetProperty("val").ToString();
			return rf;
		}
		//BaseType GetResponseValByQuestionID(string id)
		//{

		//    var Q = GetQuestion(id);
		//    return Q.ResponseField_Item.Response.Item;

		//}
		public static PropertyType? GetPropertyByName(this ITopNodePublic itn, string name)
		{
			PropertyType? p;
			p = (PropertyType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(PropertyType)).Where(
					t => ((PropertyType)t).name == name).FirstOrDefault();
			return p;
		}
		public static ExtensionType? GetExtensionByName(this ITopNodePublic itn, string name)
		{
			ExtensionType? e;
			e = (ExtensionType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(ExtensionType)).Where(
					t => ((ExtensionType)t).name == name).FirstOrDefault();
			return e;
		}
		public static CommentType? GetCommentByName(this ITopNodePublic itn, string name)
		{
			CommentType? c;
			c = (CommentType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(CommentType)).Where(
					t => ((CommentType)t).name == name).FirstOrDefault();
			return c;
		}
		public static ContactType? GetContactByName(this ITopNodePublic itn, string name)
		{
			ContactType? c;
			c = (ContactType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(CommentType)).Where(
					t => ((ContactType)t).name == name).FirstOrDefault();
			return c;
		}
		public static LinkType? GetLinkByName(this ITopNodePublic itn, string name)
		{
			LinkType? l;
			l = (LinkType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(LinkType)).Where(
					t => ((LinkType)t).name == name).FirstOrDefault();
			return l;
		}
		public static BlobType? GetBlobByName(this ITopNodePublic itn, string name)
		{
			BlobType? b;
			b = (BlobType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(BlobType)).Where(
					t => ((BlobType)t).name == name).FirstOrDefault();
			return b;
		}
		public static CodingType? GetCodedValueByName(this ITopNodePublic itn, string name)
		{
			CodingType? c;
			c = (CodingType?)itn.Nodes.Values.Where(
				t => t.GetType() == typeof(CodingType)).Where(
					t => ((CodingType)t).name == name).FirstOrDefault();
			return c;
		}


		#endregion
	}
}
