

namespace SDC.Schema.Extensions
{
	public static class X_IExtensionBaseTypeMemberExtensions
	{
		//!+TODO: Handle Dictionary updates
		private static bool X_Move(this IExtensionBaseTypeMember iebt, ExtensionType extension, ExtensionBaseType ebtTarget, int newListIndex = -1)
		{
			if (extension == null) return false;

			var ebt = (ExtensionBaseType)extension.ParentNode;  //get the list that comment is attached to          
			if (ebtTarget == null) ebtTarget = ebt;  //attach to the original parent
			bool b = ebt.Extension.Remove(extension);
			if (b) ebtTarget.Extension.Insert(newListIndex, extension);
			var count = ebtTarget.Extension.Count;
			if (newListIndex < 0 || newListIndex > count) newListIndex = count;
			if (ebtTarget.Extension[newListIndex] == extension) return true; //success
			return false;
		}
        private static bool X_Move(this IExtensionBaseTypeMember iebt, CommentType comment, ExtensionBaseType ebtTarget, int newListIndex)
		{
			if (comment == null) return false;

			var ebt = (ExtensionBaseType)comment.ParentNode;  //get the list that comment is attached to          
			if (ebtTarget == null) ebtTarget = ebt;  //attach to the original parent
			bool b = ebt.Comment.Remove(comment);
			var count = ebt.Comment.Count;
			if (newListIndex < 0 || newListIndex > count) newListIndex = count;
			if (b) ebtTarget.Comment.Insert(newListIndex, comment);
			if (ebtTarget.Comment[newListIndex] == comment) return true; //success
			return false;
		}
        private static bool X_Move(this IExtensionBaseTypeMember iebt, PropertyType property, ExtensionBaseType ebtTarget, int newListIndex)
		{
			if (property == null) return false;

			var ebt = (ExtensionBaseType)property.ParentNode;  //get the list that comment is attached to          
			if (ebtTarget == null) ebtTarget = ebt;  //attach to the original parent
			bool b = ebt.Property.Remove(property);
			var count = ebt.Property.Count;
			if (newListIndex < 0 || newListIndex > count) newListIndex = count;
			if (b) ebtTarget.Property.Insert(newListIndex, property);
			if (ebtTarget.Property[newListIndex] == property) return true; //success
			return false;
		}
	}
}
