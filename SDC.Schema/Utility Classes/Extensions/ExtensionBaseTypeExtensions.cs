

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class ExtensionBaseTypeExtensions
	{
		public static bool HasExtensionBaseMembers(this ExtensionBaseType ebt) //Has Extension, Property or Comment sub-elements
		{
			//var ebt = ieb as ExtensionBaseType;
			if (ebt?.Property?.Count > 0)
			{
				foreach (var n in ebt.Property)
				{ if (n != null) return true; }
			}
			if (ebt?.Comment?.Count > 0)
			{
				foreach (var n in ebt.Comment)
				{ if (n != null) return true; }
			}
			if (ebt?.Extension?.Count > 0)
			{
				foreach (var n in ebt.Extension)
				{ if (n != null) return true; }
			}
			return false;
		}
		public static ExtensionType AddExtension(this ExtensionBaseType ebtParent, int insertPosition = -1)
		{
			//var ebtParent = ieb as ExtensionBaseType;
			var e = new ExtensionType(ebtParent);
			if (ebtParent.Extension == null) ebtParent.Extension = new List<ExtensionType>();
			var count = ebtParent.Extension.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			ebtParent.Extension.Insert(insertPosition, e);
			return e;
		}
		public static CommentType AddComment(this ExtensionBaseType ebtParent, int insertPosition = -1)
		{
			//var ebtParent = ieb as ExtensionBaseType;
			if (ebtParent.Comment == null) ebtParent.Comment = new List<CommentType>();
			CommentType ct = null;
			var count = ebtParent.Comment.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			ebtParent.Comment.Insert(insertPosition, ct);  //return new empty Comment object for caller to fill
			return ct;
		}
		public static PropertyType AddProperty(this ExtensionBaseType ebtParent, int insertPosition = -1)
		{
			//var ebtParent = ieb as ExtensionBaseType;
			var prop = new PropertyType(ebtParent);
			if (ebtParent.Property == null) ebtParent.Property = new List<PropertyType>();
			var count = ebtParent.Property.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			ebtParent.Property.Insert(insertPosition, prop);

			return prop;
		}
	}
}
