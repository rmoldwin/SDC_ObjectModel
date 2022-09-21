//using SDC;
using System.ComponentModel;
using System.Reflection;

namespace SDC.Schema
{
	public static class BaseTypeExtensions
	{

		public static List<BaseType>? GetChildList(this BaseType bt)
		{
			var topNode = (_ITopNode)bt.TopNode;
			var cn = topNode?._ChildNodes;
			if (cn is null) return null;
			if (bt.ParentNode != null)
				if (cn.TryGetValue(bt.ParentNode.ObjectGUID, out List<BaseType>? childList))
					return childList;
			//if (bt is FormDesignType fd) return fd.ChildItemsNode.ChildItemsList;
			if (cn.TryGetValue(bt.ObjectGUID, out List<BaseType>? childList2))
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
			return SdcUtil.ReflectChildAttributes(bt, getAllAttributes: false);
		}
		/// <summary>
		/// Provides PropertyInfo (PI) definitions for all XML attributes of an SDC node
		/// </summary>
		/// <param name="bt"></param>
		/// <returns><b>List&lt;PropertyInfo></b> </returns>
		public static List<AttributeInfo> GetXmlAttributesAll(this BaseType bt)
		{
			return SdcUtil.ReflectChildAttributes(bt);
		}
		public static PropertyInfoMetadata GetPropertyInfoMetaData(this BaseType bt)
		{
			return SdcUtil.GetElementPropertyInfoMeta(bt);
		}
		public static List<BaseType> GetSubtreeList(this BaseType bt)
		{
			return SdcUtil.GetSortedSubtreeList(bt);
		}
		/// <summary>
		/// Get a sorted list of node bt, plus of all of node bt's sub-elements, up to but not including the next ChildItemsType node
		/// </summary>
		/// <param name="bt">The node whose subtree we are retrieving </param>
		/// <returns></returns>
		public static List<BaseType> GetSortedSubtreeIETList(this BaseType bt)
		{
			return SdcUtil.GetSortedSubtreeIET(bt);
		}

		/// <summary>
		/// Retrieve the default value (if one exists) for the SDC XML attribute identified by <paramref name="attributeName"/>.  
		/// The default value is obtained from the <see cref="DefaultValueAttribute"/> on the property that represents <paramref name="attributeName"/>.
		/// </summary>
		/// <param name="bt">The SDC node on which we are searching for an attribute's default value (if present)</param>
		/// <param name="attributeName">The name of the attribute as it appears in SDX XML</param>
		/// <returns>Nullable <see cref="object"/> containing the attribute's default value, or null if no default value exists</returns>
		/// <exception cref="InvalidDataException">Thrown if <paramref name="attributeName"/> is not the name of an existing attribute property, </exception>
		public static object? GetAttributeDefaultValue(this BaseType bt, string attributeName)
		{
			var p = bt.GetType().GetProperty(attributeName);
			if (p is null) throw new InvalidDataException("The attributeName parameter was not found on a property if this object");
			var defVal = p.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault();
			return defVal;
		}
		public static List<BaseType>? GetSibs(this BaseType bt)
		{
			var topNode = (_ITopNode)bt.TopNode;
			var par = bt?.ParentNode;
			if (par is null) return null;
			if (topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs))
				return sibs;

			return null;
		}
		public static bool X_IsItemChangeAllowed_(this IdentifiedExtensionType iet, IdentifiedExtensionType targetType)
		{
			throw new NotImplementedException();

		}

		/// <summary>
		/// This method determines if a BaseType object (bt) has a private _shouldSerializePropertyName field 
		/// for a public value-type <paramref name="property"/> ("PropertyName"), 
		/// and if so, sets it (using the serializeDefaultValue parameter) to the desired true or false default) value.
		/// The _shouldSerializePropertyName field exists only on simple non-nullable types (structs), 
		/// including all numeric, DateTime, and TimeSpan (duration) types.
		/// If the private _shouldSerializePropertyName field exists, setting it to true will force the PropertyName to be serialized, 
		/// even if it contains its default value.  
		/// Whenever a property is set in code, _shouldSerializePropertyName is set to true.  
		/// Setting _shouldSerializePropertyName to false will prevent the <paramref name="property"/> 
		/// on the bt object from being serialized if it contains its default value.
		/// In general, this method need only be called to set _shouldSerializePropertyName to false, 
		/// and thus the default value of serializeDefaultValue = false. 
		/// It is most useful for forcing or suppressing the serialization of default values (e.g., 0) from numeric types from byte to decimal.  
		/// When .NET Core 7 (C# 11) is available with constraints for numeric values (e.g., Tin: INumeric), 
		/// this method may be updated to support that additional constraint, to exclude, e.g., enums etc.
		/// </summary>
		/// <typeparam name="Tin">Tin represents any <paramref name="property"/> of a struct type</typeparam>
		/// <param name="bt">The BaseType object that contains <paramref name="property"/></param>
		/// <param name="property">The <paramref name="property"/> value-type object 
		/// for which we want to set its private _shouldSerialize field</param>
		/// <param name="shouldSerialize">Set to true to serialize the default value of <paramref name="property"/>; 
		/// set to false to omit the default value of <paramref name="property"/> 
		/// (i.e., do not serialize <paramref name="property"/> if it holds its default value).</param>
		/// <returns>true for success, false if a _shouldSerializePropertyName field does not exist in bt, 
		/// or if the <paramref name="property"/> itself was not found in bt</returns>
		public static bool SetShouldSerialize<Tin>(this BaseType bt, Tin property, bool shouldSerialize = false)
			where Tin : struct
		{
			var pi = bt.GetType().GetField("_shouldSerialize" + property.GetType().Name);
			if (pi is null) return false;
			pi.SetValue(bt, shouldSerialize);
			return true;
		}


	}
}
