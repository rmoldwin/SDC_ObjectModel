

//using SDC;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace SDC.Schema
{
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
		public static BaseType? GetNodeNext(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.ReflectNextElement((BaseType)n); else return null; }
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


		public static List<IdentifiedExtensionType>? GetIETChildren(this INavigate n)
		{
			var chList =  (n as IChildItemsParent)?.ChildItemsNode?.ChildItemsList;
			if (chList is null) return null;

			List<IdentifiedExtensionType> newList = new();
			//do not return the original List, since the recipient code could modify it(move, remove) in ways that invalidate the _ITopNpde dictionaries.
			newList.AddRange(chList); 
			return newList;



			return FindIETChildren((BaseType)n);

			List<IdentifiedExtensionType>? FindIETChildren(BaseType node)
			{
				var IETChildren = new List<IdentifiedExtensionType>();
				var children = node.GetChildList();
				foreach (var child in children ?? new List<BaseType>())
				{
					if (child is IdentifiedExtensionType iet)
					{
						IETChildren.Add(iet);
					}
					else
					{
						var ietChildren = FindIETChildren(child);
						if(ietChildren is not null)
							IETChildren.AddRange(ietChildren);
					}
				}

				return IETChildren;
			}
		}

	}
}
