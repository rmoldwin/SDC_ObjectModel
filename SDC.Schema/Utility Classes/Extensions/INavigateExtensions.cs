

//using SDC;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace SDC.Schema.Extensions
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class INavigateExtensions
	{
		public static BaseType? GetNodeFirstSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetFirstSibElement((BaseType)n); else return null; }
		public static BaseType? GetNodeLastSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetLastSibElement((BaseType)n); else return null; }
		public static BaseType? GetNodePreviousSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetPrevSibElement((BaseType)n); else return null; }
		public static BaseType? GetNodeNextSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetNextSibElement((BaseType)n); else return null; }
		public static BaseType? GetNodePrevious(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetPrevElement((BaseType)n); else return null; }

		/// <summary>
		/// <inheritdoc cref="SdcUtil.GetPrevElementIET(BaseType)"/>
		/// </summary>
		/// <param name="n"></param>
		/// <returns>The previous IdentifiedExtensionType node, or null if one can be located</returns>
		public static IdentifiedExtensionType? GetNodePreviousIET(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetPrevElementIET((BaseType)n); else return null; }
		public static BaseType? GetNodeReflectNext(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.ReflectNextElement((BaseType)n); else return null; }
		public static BaseType? GetNodeNext(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetNextElement((BaseType)n); else return null; }
		public static BaseType? GetNodeFirstChild(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetFirstChildElement((BaseType)n); else return null; }
		public static BaseType? GetNodeLastChild(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetLastChildElement((BaseType)n); else return null; }
		public static BaseType? GetNodeLastDescendant(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetLastDescendantElementSimple((BaseType)n); else return null; }
		public static bool TryGetChildNodes(this INavigate n, out ReadOnlyCollection<BaseType>? kids)
		{
			kids = null;
			if (n is not null && n is BaseType) return SdcUtil.TryGetChildElements((BaseType)n, out kids);
			else return false;
		}

		public static ReadOnlyCollection<BaseType>? GetChildList(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetChildElements((BaseType)n); else return null; }
		public static List<BaseType>? GetSubtreeList(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetSortedSubtreeList((BaseType)n); else return null; }
		public static Dictionary<Guid, BaseType>? GetSubtreeDictionary(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetSubtreeDictionary((BaseType)n); else return null; }
		public static PropertyInfoMetadata? GetPropertyInfo(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetElementPropertyInfoMeta((BaseType)n); else return null; }


		//public static List<IdentifiedExtensionType>? GetIETChildren(this INavigate n)
		//{
		//	var chList = (n as IChildItemsParent)?.ChildItemsNode?.ChildItemsList;
		//	if (chList is null) return null;

		//	List<IdentifiedExtensionType> newList = new();

		//	var IETChildren = new List<IdentifiedExtensionType>();
		//	return FindIETChildren((BaseType)n);

		//	List<IdentifiedExtensionType>? FindIETChildren(BaseType node)
		//	{
		//		var children = node.GetChildNodes()?.ToList();
		//		foreach (var child in children ?? new List<BaseType>())
		//		{
		//			if (child is IdentifiedExtensionType iet)
		//			{
		//				IETChildren.Add(iet);
		//			}
		//			FindIETChildren(child);
		//			//else
		//			//{
		//			//	var ietChildren = FindIETChildren(child);
		//			//	if(ietChildren is not null)
		//			//		IETChildren.AddRange(ietChildren);
		//			//}
		//		}
		//		return IETChildren;
		//	}
		//}

		//private static List<IdentifiedExtensionType>? GetIETDescendants_(this INavigate n, bool includeCurrentIETNode)
		//{ throw new NotImplementedException();
		//	var chList = (n as IChildItemsParent)?.ChildItemsNode?.ChildItemsList;
		//	if (chList is null) return null;

		//	List<IdentifiedExtensionType> newList = new();

		//	var IETChildren = new List<IdentifiedExtensionType>();
		//	return FindIETChildren((BaseType)n);

		//	List<IdentifiedExtensionType>? FindIETChildren(BaseType node)
		//	{
		//		var children = node.GetChildNodes()?.ToList();
		//		foreach (var child in children ?? new List<BaseType>())
		//		{
		//			if (child is IdentifiedExtensionType iet)
		//			{
		//				IETChildren.Add(iet);
		//			}
		//			FindIETChildren(child);
		//			//else
		//			//{
		//			//	var ietChildren = FindIETChildren(child);
		//			//	if(ietChildren is not null)
		//			//		IETChildren.AddRange(ietChildren);
		//			//}
		//		}
		//		return IETChildren;
		//	}



		//}
		private static List<ITopNode>? GetITopNodeDescendants_(this INavigate n, bool includeCurrentITopNode)
		{ throw new NotImplementedException(); }
		public static List<BaseType>? GetFullTree(this INavigate n)
		{
			var root = ((BaseType)n).FindRootNode() as ITopNode;
			if (root is not null)
				return SdcUtil.GetSortedTreeList(root);
			throw new InvalidOperationException("Could not obtain the root node from SdcUtil.FindRootNode");
		}
	}
}
