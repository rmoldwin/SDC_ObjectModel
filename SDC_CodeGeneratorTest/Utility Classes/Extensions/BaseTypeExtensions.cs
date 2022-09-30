//using SDC;
using System.ComponentModel;
using System.Reflection;

namespace SDC.Schema
{
	/// <summary>
	/// Extensions that will appear on all <see cref="BaseClass"/> objects
	/// </summary>
	public static class BaseTypeExtensions
	{

		/// <summary>
		/// Retrieve a List&lt;BaseType>? containing all child element nodes of the current SDC node 
		/// </summary>
		/// <param name="bt"></param>
		/// <returns></returns>
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
		/// <summary>
		/// Determine if the current node is an ancestor (i.e., a node closer to the root node) of parameter <paramref name="descendantNode"/>.
		/// </summary>
		/// <param name="ancestorNode"></param>
		/// <param name="descendantNode"></param>
		/// <returns>true if the current node is an ancestor of descendantNode.</returns>
		public static bool IsAncestorOf(this BaseType ancestorNode, BaseType descendantNode)
		{
			if (descendantNode is null || ancestorNode is null || descendantNode == ancestorNode) return false;

			var par = descendantNode.ParentNode;
			while (par != null)
			{ if (par.Equals(ancestorNode)) return true; }

			return false;
		}
		/// <summary>
		///  Determine if the current node is the direct parent node of parameter <paramref name="childNode"/>.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="childNode"></param>
		/// <returns></returns>
		public static bool IsParentOf(this BaseType parentNode, BaseType childNode)
		{
			if (childNode.ParentNode == parentNode) return true;
			return false;
		}
		/// <summary>
		/// Determine if the current node is a direct child node <paramref name="parentNode"/>.
		/// </summary>
		/// <param name="childNode"></param>
		/// <param name="parentNode"></param>
		/// <returns>true if the current node is a direct child of parameter <paramref name="parentNode"/>.</returns>
		public static bool IsChildOf(this BaseType childNode, BaseType parentNode)
		{
			if (childNode.ParentNode == parentNode) return true;
			return false;
		}


		/// <summary>
		/// Determine if the current node is a descendant of parameter <paramref name="ancestorNode"/>.
		/// </summary>
		/// <param name="descendantNode"></param>
		/// <param name="ancestorNode"></param>
		/// <returns>true if the current node is a descendant of parameter <paramref name="ancestorNode"/>.</returns>
		public static bool IsDescendantOf(this BaseType descendantNode, BaseType ancestorNode)
		{
			if (ancestorNode is null || descendantNode is null || ancestorNode == descendantNode) return false;

			var par = descendantNode.ParentNode;
			while (par != null)
			{ if (par.Equals(ancestorNode)) return true; }
			return false;
		}

		/// <summary>
		/// For the current node, retrieves a <see cref="List&lt;AttributeInfo>"/> containing <see cref="AttributeInfo"/> (AI) definitions for all XML attributes of the current node that will be serialized to XML. <br/>
		/// Each AI struct can be used to obtain the type, name and other features of each attribute.<br/>
		/// Also, each AI can be used to create an instance of the object by calling the underlying PropertyInfo object:<br/>
		/// AI.AttributePropInfo.GetValue(parentObject)
		/// </summary>
		/// <param name="bt"></param>
		/// <returns><b>List&lt;AttributeInfo></b></returns>
		public static List<AttributeInfo> GetXmlAttributesSerialized(this BaseType bt)
		{
			return SdcUtil.ReflectChildXmlAttributes(bt, getAllXmlAttributes: false);
		}
		/// <summary>
		/// For the current node, retrieves a <see cref="List&lt;AttributeInfo>"/> containing <see cref="AttributeInfo"/> (AI) definitions <br/>
		/// for all XML attributes of the current node, whether or not the attributes are populated with values. 
		/// </summary>
		/// <param name="bt"></param>
		/// <returns><b>List&lt;AttributeInfo></b> </returns>
		public static List<AttributeInfo> GetXmlAttributesAll(this BaseType bt)
		{
			return SdcUtil.ReflectChildXmlAttributes(bt);
		}
		/// <summary>
		/// Retrieve the <see cref="PropertyInfoMetadata"/> struct describing the current node
		/// </summary>
		/// <param name="bt"></param>
		/// <returns></returns>
		public static PropertyInfoMetadata GetPropertyInfoMetaData(this BaseType bt)
		{
			return SdcUtil.GetElementPropertyInfoMeta(bt);
		}
		/// <summary>
		/// Starting with the current node, retrieve all descendant nodes in List&lt;BaseType>
		/// </summary>
		/// <param name="bt"></param>
		/// <returns></returns>
		public static List<BaseType> GetSubtreeList(this BaseType bt)
		{
			return SdcUtil.GetSortedSubtreeList(bt);
		}
		/// <summary>
		/// Get a sorted list containing the current node, plus of all of its sub-elements, up to but not including the next <see cref="IdentifiedExtensionType"/> node.
		/// </summary>
		/// <param name="bt">The node whose subtree we are retrieving </param>
		/// <returns></returns>
		public static List<BaseType> GetSortedSubtreeIETList(this BaseType bt)
		{
			return SdcUtil.GetSortedSubtreeIET(bt);
		}

		/// <summary>
		/// Retrieve the default value (if one exists) for the SDC XML attribute identified by <paramref name="attributeName"/>.<br/>
		/// The default value is obtained from the <see cref="DefaultValueAttribute"/> on the current node property that represents <paramref name="attributeName"/>.
		/// </summary>
		/// <param name="bt">The current SDC node on which we are searching for an attribute's default value (if present)</param>
		/// <param name="attributeName">The name of the attribute as it appears in SDX XML</param>
		/// <returns>Nullable <see cref="object"/> containing the attribute's default value, or null if no default value exists.<br/>
		/// <see cref="InvalidDataException"/> is thrown if <paramref name="attributeName"/> is not the name of an existing attribute property. 
		/// </returns>
		/// <exception cref="InvalidDataException"/>
		public static object? GetAttributeDefaultValue(this BaseType bt, string attributeName)
		{
			var p = bt.GetType().GetProperty(attributeName);
			if (p is null) throw new InvalidDataException("The attributeName parameter was not found on a property if this object");
			var defVal = p.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault();
			return defVal;
		}
		/// <summary>
		/// Get a sorted List&lt;BaseType> containing all sibling nodes of the current node
		/// </summary>
		/// <param name="bt"></param>
		/// <returns></returns>
		public static List<BaseType>? GetSibs(this BaseType bt)
		{
			var topNode = (_ITopNode)bt.TopNode;
			var par = bt?.ParentNode;
			if (par is null) return null;
			if (topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs))
				return sibs;

			return null;
		}

		/// <summary>
		/// This method determines if a BaseType object (bt) has a hidden _shouldSerializePropertyName field 
		/// for a public value-type <paramref name="property"/> ("PropertyName"), <br/>
		/// and if so, sets it (using the <paramref name="shouldSerialize"/> parameter) to the desired true or false default) value.<br/>
		/// The hidden _shouldSerializePropertyName field exists only on simple non-nullable types (structs), <br/>
		/// including all numeric, DateTime, and TimeSpan (duration) types.<br/>
		/// If the _shouldSerializePropertyName field exists, setting it to true will force the PropertyName to be serialized, <br/>
		/// unless if it contains its DefaultValue specified in <see cref="DefaultValueAttribute.Value"/>.<br/>
		/// Whenever a property is set with a backing field (e.g., using _propertyName) in code, _shouldSerializePropertyName should also be set to true. <br/> 
		/// Setting _shouldSerializePropertyName to false will prevent the <paramref name="property"/> <br/>
		/// on the current object from being serialized if it contains the default value of its detatype, but it otherwise will be serialized.<br/>
		/// In general, this method need only be called to set _shouldSerializePropertyName to false, 
		/// and thus the default value of serializeDefaultValue is false. <br/>
		/// It is most useful for suppressing the serialization of datatype default values (e.g., 0) from numeric types from byte to decimal.  <br/>
		/// When .NET Core 7 (C# 11) is available with constraints for numeric values (e.g., Tin: INumeric), <br/>
		/// this method may be updated to support that additional constraint, to exclude, e.g., enums etc.
		/// </summary>
		/// <typeparam name="Tin">Tin represents any <paramref name="property"/> of a struct type</typeparam>
		/// <param name="bt">The current BaseType object that contains <paramref name="property"/></param>
		/// <param name="property">The <paramref name="property"/> value-type object 
		/// for which we want to set its private _shouldSerialize field</param>
		/// <param name="shouldSerialize">Set to true to serialize the default value of <paramref name="property"/>; 
		/// set to false to omit the default value of <paramref name="property"/> 
		/// (i.e., do not serialize <paramref name="property"/> if it holds its default value).</param>
		/// <returns>true for success, false if a _shouldSerializePropertyName field does not exist in the current node, 
		/// or if the <paramref name="property"/> itself was not found in the current node</returns>
		public static bool SetShouldSerialize<Tin>(this BaseType bt, Tin property, bool shouldSerialize = false)
			where Tin : struct
		{
			var pi = bt.GetType().GetField("_shouldSerialize" + property.GetType().Name);
			if (pi is null) return false;
			pi.SetValue(bt, shouldSerialize);
			return true;
		}
		public static bool X_IsItemChangeAllowed_(this IdentifiedExtensionType iet, IdentifiedExtensionType targetType)
		{
			throw new NotImplementedException();

		}


	}
}
