

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class ExtensionBaseTypeExtensions
	{
		public static bool HasExtensionBaseMembers(this ExtensionBaseType ebt) //Has Extension, Property or Comment sub-elements
		{
			if (ebt?.Property?.Count > 0) return true;
			if (ebt?.Comment?.Count > 0) return true; 
			if (ebt?.Extension?.Count > 0) return true;

			return false;
		}
		public static ExtensionType AddExtension(this ExtensionBaseType ebtParent, int insertPosition = -1)
		{
			return new ExtensionType(ebtParent, insertPosition);
		}
		public static CommentType AddComment(this ExtensionBaseType ebtParent, int insertPosition = -1)
		{
			return new CommentType(ebtParent, insertPosition);
		}
		public static PropertyType AddProperty(this ExtensionBaseType ebtParent, int insertPosition = -1)
		{
			return new PropertyType(ebtParent, insertPosition);
		}
	}
}
