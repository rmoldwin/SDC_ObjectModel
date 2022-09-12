

//using SDC;
namespace SDC.Schema
{
	public static class INavigateExtensions
	{
		public static BaseType? GetNodeFirstSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetFirstSib((BaseType)n); else return null; }
		public static BaseType? GetNodeLastSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetLastSib((BaseType)n); else return null; }
		public static BaseType? GetNodePreviousSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetPrevSib((BaseType)n); else return null; }
		public static BaseType? GetNodeNextSib(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetNextSib((BaseType)n); else return null; }
		public static BaseType? GetNodePrevious(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetPrevElement((BaseType)n); else return null; }
		public static BaseType? GetNodeNext(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.ReflectNextElement((BaseType)n); else return null; }
		public static BaseType? GetNodeFirstChild(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetFirstChild((BaseType)n); else return null; }
		public static BaseType? GetNodeLastChild(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetLastChild((BaseType)n); else return null; }
		public static BaseType? GetNodeLastDescendant(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetLastDescendantSimple((BaseType)n); else return null; }
		public static bool HasChildren(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.HasChild((BaseType)n); else return false; }

		public static List<BaseType>? GetChildList(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetChildList((BaseType)n); else return null; }

		public static List<BaseType>? GetSubtreeList(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetSortedSubtreeList((BaseType)n); else return null; }
		public static Dictionary<Guid, BaseType>? GetSubtreeDictionary(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetSubtreeDictionary((BaseType)n); else return null; }
		public static PropertyInfoMetadata? GetPropertyInfo(this INavigate n)
		{ if (n is not null && n is BaseType) return SdcUtil.GetPropertyInfoMeta((BaseType)n); else return null; }

	}
}
