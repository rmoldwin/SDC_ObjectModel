using SDC_CodeGeneratorTest.Utility_Classes.Metadata_Structs;
using System.Reflection;



//using SDC;
namespace SDC.Schema
{
	public static class BaseTypeExtensions
	{

		public static List<BaseType> GetChildList(this BaseType bt)
		{
			var cn = bt?.TopNode?.ChildNodes;
			if (cn is null) return null;
			if (bt.ParentNode != null)
				if (cn.TryGetValue(bt.ParentNode.ObjectGUID, out List<BaseType> childList))
					return childList;
			//if (bt is FormDesignType fd) return fd.ChildItemsNode.ChildItemsList;
			if (cn.TryGetValue(bt.ObjectGUID, out List<BaseType> childList2))
				return childList2;
			return null;
		}
		public static bool IsAncestorOf(this BaseType ancestorNode, BaseType descendantNode)
		{
			if (descendantNode is null || ancestorNode is null || descendantNode == ancestorNode) return false;

			var par = descendantNode.ParentNode;
			while (par != null)
			{ if (par.Equals(ancestorNode)) return true; }

			return false;
		}
		public static bool IsParentOf(this BaseType parentNode, BaseType childNode)
		{
			if (childNode.ParentNode == parentNode) return true;
			return false;
		}
		public static bool IsChildOf(this BaseType childNode, BaseType parentNode)
		{
			if (childNode.ParentNode == parentNode) return true;
			return false;
		}


		public static bool IsDescendantOf(this BaseType descendantNode, BaseType ancestorNode)
		{
			if (ancestorNode is null || descendantNode is null || ancestorNode == descendantNode) return false;

			var par = descendantNode.ParentNode;
			while (par != null)
			{ if (par.Equals(ancestorNode)) return true; }
			return false;
		}

		/// <summary>
		/// Provides PropertyInfo (PI) definitions for all bt attributes that will be serialized to XML
		/// Each PI can be used to obtain the type, name and other features of each attribute
		/// Also, each PI can be used to create an instance of the object by calling PI.GetValue(parentObject)
		/// </summary>
		/// <param name="bt"></param>
		/// <returns>List&lt;PropertyInfo></returns>
		public static List<AttributeInfo> GetXmlAttributesSerialized(this BaseType bt)
		{
			return SdcUtil.ReflectXmlAttributes(bt, getAllAttributes: false);
		}
		/// <summary>
		/// Provides PropertyInfo (PI) definitions for all XML attributes of an SDC node
		/// </summary>
		/// <param name="bt"></param>
		/// <returns><b>List&lt;PropertyInfo></b> </returns>
		public static List<AttributeInfo> GetXmlAttributesAll(this BaseType bt)
		{
			return SdcUtil.ReflectXmlAttributes(bt);
		}
		public static PropertyInfoMetadata GetPropertyInfoMetaData(this BaseType bt)
		{
			return SdcUtil.GetPropertyInfoMeta(bt);
		}
		public static List<BaseType> GetSubtreeList(this BaseType bt)
		{
			return SdcUtil.GetSortedSubtreeList(bt);
		}
		public static List<BaseType>? GetSibs(this BaseType bt)
		{
			var par = bt?.ParentNode;
			if (par is null) return null;
			if (bt.TopNode.ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs))
				return sibs;

			return null;
		}
		public static bool X_IsItemChangeAllowed_(this IdentifiedExtensionType iet, IdentifiedExtensionType targetType)
		{
			throw new NotImplementedException();

		}

		/// <summary>
		/// This method determines if a BaseType object (bt) has a private _shouldSerializePropertyName field for a public struct property ("PropertyName"), 
		/// and if so, sets it (using the serializeDefaultValue parameter) to the desired true or false value.
		/// The _shouldSerializePropertyName field exists only on simple non-nullable types (structs), including all numeric, DateTime, and TimeSpan (duration) types.
		/// If the private _shouldSerializePropertyName field exists, setting it to true will force the PropertyName to be serialized, 
		/// even if it contains its default value.  
		/// Whenever a property is set in code, _shouldSerializePropertyName is set to true.  
		/// Setting _shouldSerializePropertyName to false will prevent the property on the bt object from being serialized if it contains its default value.
		/// In general, this method need only be called to set _shouldSerializePropertyName to false, and thus the default value of serializeDefaultValue = false. 
		/// It is most useful for forcing or suppressing the serialization of default values (e.g., 0) from numeric types from byte to decimal.  
		/// When .NET Core 7 (C# 11) is available with constraints for numeric values (e.g., Tin: INumeric), 
		/// this method may be updated to support that additional constraint, to exclude, e.g., enums etc.
		/// </summary>
		/// <typeparam name="Tin">Tin represents any property of a struct type</typeparam>
		/// <param name="bt">The BaseType object that contains property</param>
		/// <param name="property">The property struct object for which we want to set its private _shouldSerialize field</param>
		/// <returns>true for success, false if _shouldSerializePropertyName does not exist on bt, or if an exception occurs</returns>
		public static bool ResetShouldSerialize<Tin>(this BaseType bt, Tin property)
			where Tin : struct
		{
			var pi = bt.GetType().GetField("_shouldSerialize" + property.GetType().Name);
			if (pi is null) return false;
			pi.SetValue(bt, false);
			return true;
		}
	}
}
