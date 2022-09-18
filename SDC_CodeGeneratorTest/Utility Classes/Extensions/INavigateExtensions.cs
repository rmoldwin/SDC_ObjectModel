

//using SDC;
using System.Collections.ObjectModel;

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
		public static bool HasChildren(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.HasChildElement((BaseType)n); else return false; }

		public static ReadOnlyCollection<BaseType>? GetChildList(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetChildElements((BaseType)n); else return null; }

		public static List<BaseType>? GetSubtreeList(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetSortedSubtreeList((BaseType)n); else return null; }
		public static Dictionary<Guid, BaseType>? GetSubtreeDictionary(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetSubtreeDictionary((BaseType)n); else return null; }
		public static PropertyInfoMetadata? GetPropertyInfo(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetElementPropertyInfoMeta((BaseType)n); else return null; }

	}
}
