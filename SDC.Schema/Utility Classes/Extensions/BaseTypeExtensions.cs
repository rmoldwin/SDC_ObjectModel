//using SDC;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using static SDC.Schema.SdcUtil;

namespace SDC.Schema.Extensions
{
	/// <summary>
	/// Extensions that will appear on all <see cref="BaseType"/> objects
	/// </summary>
	public static class BaseTypeExtensions
	{

		///// <summary>
		///// Retrieve a List&lt;BaseType>? containing all child element nodes of the current SDC node 
		///// </summary>
		///// <param name="bt"></param>
		///// <returns></returns>
		//public static ReadOnlyCollection<BaseType>? GetChildNodes(this BaseType bt)
		//{
		//	return SdcUtil.GetChildElements(bt);
		//}
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
			{ if (par.Equals(ancestorNode)) 
					return true;
				par = par.ParentNode;
			}
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
		/// Determine if the current node is a direct child node of <paramref name="parentNode"/>.
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
		/// Determine if the current node is a sibling node of <paramref name="siblingNode"/>
		/// </summary>
		/// <param name="node"></param>
		/// <param name="siblingNode"></param>
		/// <returns>true if the current node is a sibling of parameter <paramref name="siblingNode"/>.</returns>
		public static bool IsSiblingOf(this BaseType node, BaseType siblingNode)
		{
			if (node.ParentNode == siblingNode.ParentNode) return true;
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
			{ 
				if (par.Equals(ancestorNode)) 
					return true;
				par = par.ParentNode;
			}
			return false;
		}

		/// <summary>
		/// For the current node, retrieves a <see cref="List&lt;AttributeInfo>"/> containing <see cref="AttributeInfo"/> (AI) definitions <br/>
		/// for all XML attributes of the current node that will be serialized to XML. <br/>
		/// Each AI struct can be used to obtain the type, name and other features of each attribute.<br/>
		/// Also, each AI can be used to create a reference to the object by calling the underlying PropertyInfo object:<br/>
		/// AI.AttributePropInfo.GetValue(parentObject)
		/// </summary>
		/// <param name="bt"></param>
		/// <returns><b>List&lt;AttributeInfo></b></returns>
		public static List<AttributeInfo> GetXmlAttributesSerialized(this BaseType bt)
		{
			return SdcUtil.ReflectNodeXmlAttributes(bt, getAllXmlAttributes: false);
		}

		/// <summary>
		/// For the current node, retrieves a <see cref="List&lt;AttributeInfo>"/> containing <see cref="AttributeInfo"/> (AI) definitions <br/>
		/// for all XML attributes, of the current node and all of its subnodes, until an IET subnode is encounters. <br/>
		/// Each AI struct can be used to obtain the type, name and other features of each attribute.<br/>
		/// Also, each AI can be used to create a reference to the object by calling the underlying PropertyInfo object:<br/>
		/// AI.AttributePropInfo.GetValue(parentObject)
		/// </summary>
		/// <param name="bt"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetIETXmlAttributesSerialized(this IdentifiedExtensionType iet)
		{
			var lai = new List<AttributeInfo>();
			foreach(var n in GetSubtreeIETList(iet))
			{
				//if (n.ElementName.Intersect(new {"a"}) is not null)
				lai.AddRange( SdcUtil.ReflectNodeXmlAttributes(n, getAllXmlAttributes: false));
			}			
			return lai;
		}

		/// <summary>
		/// For the current node, retrieves a <see cref="List&lt;AttributeInfo>"/> containing <see cref="AttributeInfo"/> (AI) definitions <br/>
		/// for all XML attributes of the current node, whether or not the attributes are populated with values. 
		/// </summary>
		/// <param name="bt"></param>
		/// <returns><b>List&lt;AttributeInfo></b> </returns>
		public static List<AttributeInfo> GetXmlAttributesAll(this BaseType bt)
		{
			return SdcUtil.ReflectNodeXmlAttributes(bt);
		}
		/// <summary>
		/// Retrieve the <see cref="PropertyInfoMetadata"/> struct describing the current node
		/// </summary>
		/// <param name="bt"></param>
		/// <param name="parentNode">Unless bt is teh root node, parrentIndfo must not be null</param>
		/// <returns></returns>
		public static PropertyInfoMetadata GetPropertyInfoMetaData(this BaseType bt, BaseType? parentNode)
		{
			return SdcUtil.GetElementPropertyInfoMeta(bt, parentNode);
		}
		/// <summary>
		/// Starting with the current node, retrieve all descendant nodes in List&lt;BaseType. The list is sorted in node order.>
		/// </summary>
		/// <param name="bt"></param>
		/// <returns></returns>
		public static List<BaseType> GetSubtreeList(this BaseType bt)
		{
			return SdcUtil.GetSortedSubtreeList(bt);
		}
		/// <summary>
		/// Get a sorted list containing the current node, plus of all of its sub-elements.
		/// </summary>
		/// <param name="bt">The node whose subtree we are retrieving </param>
		/// <returns></returns>
		public static List<IdentifiedExtensionType> GetSubtreeIETList(this BaseType bt)
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
		public static IReadOnlyCollection<BaseType>? GetSibNodes(this BaseType bt)
		{
			if (bt.ParentNode is not null)
				return SdcUtil.GetChildElements(bt.ParentNode);
			else return null;

			//var topNode = (_ITopNode)bt.TopNode;
			//var par = bt?.ParentNode;
			//if (par is null) return null;
			//var sibListCopy = new List<BaseType>();  //we use this list to avoid returning a reference to Value List inside _ChildNodes.
			//if (topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? sibs))
			//{
			//	sibListCopy.AddRange(sibs);
			//	return sibListCopy;
			//}
			//return null;
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

		///	<summary>
		/// If the requested object is held by a parent IEnumerable (usually an array of List), the IEnumerable is retuurned as ieItems.
		/// If the requested object is represented by a non-IEnumerable property, then itemIndex = -1 and ieItems is null.
		/// </summary>
		public static int GetListIndex(this BaseType bt, BaseType parentNode)
		=> SdcUtil.GetElementPropertyInfoMeta(bt, parentNode, false).ItemIndex;
        ///	<summary>
        /// If the requested object is held by a parent IEnumerable (usually an array of List), the IEnumerable is retuurned as ieItems.
        /// If the requested object is represented by a non-IEnumerable property, then itemIndex = -1 and ieItems is null.
		/// The method will throw an exception if the calling node (<paramref name="bt"/>) has a null ParentNode (<see cref="BaseType.ParentNode"/>)
        /// </summary>
        public static int GetListIndex(this BaseType bt)
        => SdcUtil.GetElementPropertyInfoMeta(bt, bt.ParentNode, false).ItemIndex;

        /// <summary>
        /// Walk up the SDC object tree to find the first ITopNode ancestor of the current node.<br/>
        /// Requires that all nodes have their ParentNode property assigned correctly.
        /// If the current node implements ITopNode and has no ancestors, returns the current node.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ITopNode? FindTopNode(this BaseType bt)
		{
			BaseType? n = bt.ParentNode;
			int i = 0;
			if (n is null)
			{
				if (bt is ITopNode itn) return itn;
				else return null;
				//throw new InvalidOperationException ("The current node has no parent node, and does not implement ITopNode");
			}
			while (n is not null)
			{
				i++; if (i == 1000000) throw new InvalidOperationException("Lookup exceeded 1000000 loops, possibly due to circular parent node references");
				if (n is ITopNode itn) return itn;
				n = n.ParentNode;
			}
			return null;
			//throw new InvalidOperationException ("The current node has no parent node that implements ITopNode");
		}
		/// <summary>
		/// Walk up the SDC object tree to find the root BaseType ancestor of the current node.<br/>
		/// Requires that all nodes have their ParentNode property assigned correctly.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static BaseType? FindRootNode(this BaseType bt)
		{
			BaseType? n = bt;
			int i = 0;
			while (n is not null)
			{
				i++; if (i == 1000000) throw new InvalidOperationException("Lookup exceeded 1000000 loops, possibly due to circular parent node references");
				if (n.ParentNode is null) return n;
				n = n.ParentNode;
			}
			return null;
		}
		/// <summary>
		/// Assigns a name to the SDC node.<br/>
		/// <inheritdoc cref="SdcUtil.CreateSimpleName(BaseType)"/>
		/// </summary>
		/// <param name="bt"></param>
		/// <returns>The name assigned to the node</returns>
		public static string AssignSimpleName(this BaseType bt)
		{
			bt.name = SdcUtil.CreateSimpleName(bt);
			return bt.name;
		}

		/// <summary>
		/// <inheritdoc cref="SdcUtil.TryGetChildElements(BaseType, out ReadOnlyCollection{BaseType}?)"/>
		/// </summary>
		/// <param name="n"></param>
		/// <param name="kids"></param>
		/// <returns></returns>
		public static bool TryGetChildElements(this BaseType n, out ReadOnlyCollection<BaseType>? kids)
		=> SdcUtil.TryGetChildElements(n, out kids);

        /// <summary>
        /// Generate a new copy of an SDC tree or subtree, starting with <paramref name="rootNode"/>
        /// </summary>
        /// <typeparam name="T">Must be subtype of <see cref="BaseType"/></typeparam>
        /// <param name="rootNode">The root for the subtree to clone.  Must be subtype of <see cref="BaseType"/></param>
        /// <returns>SDC subtree that is a deep clone of the <paramref name="rootNode"/> subtree, <br/>
		/// but stripped of all non-public values, including all TopNode dictionary and collection entries.<br/>
		/// This clone may be grafted onto a valid location on any SDC tree node using <br/>
		/// the extension method <see cref="IMoveRemoveExtensions.Move"/>:<br/>
		/// cloneRootNode.Move(<b>newParent: targetNode</b>, newListIndex: -1, deleteEmptyParentNode: false, <b>updateMetadata: true</b>)
		/// 
		/// </returns>
        public static BaseType Clone<T>(this T rootNode) where T:BaseType
		{
            var xml = SdcSerializer<T>.Serialize(rootNode);
            return SdcSerializer<T>.Deserialize(xml); //Clone of rootNode subtree
        }

        /// <summary>
        /// Set the name property on any BaseType node only if it does not already exist for another node <br/>
        /// in the current tree's _UniqueNames hashable. <br/>
        /// Returns false if the new name already already exists for a different node in _UniqueNames.<br/>
        /// Returns true if the new name is the same as the old name.<br/>
        /// Returns false if the new name is null or empty.
        /// </summary>
		static bool TrySetName(this BaseType bt, string newName)
        {//TEST TrySetName

            if (bt.TopNode is null || newName == "") return false;
            if (bt.name == newName) return true;
            if (!newName.IsValidVariableName()) return false;
			

            var tn = (_ITopNode)bt.TopNode;
            if (tn._UniqueNames.TryGetValue(newName, out _) == false) return false;

            bt.name = newName;
            return true;
        }
  //      /// <summary>
  //      /// Set the BaseName property on any BaseType node only if it does not already exist for another node <br/>
  //      /// in the current tree's _UniqueBaseNames hashable. <br/>
  //      /// Returns false if the new BaseName already already exists for a different node in _UniqueBaseNames.<br/>
  //      /// Returns true if the new BaseName is the same as the old BaseName.<br/>
  //      /// Returns false if the new BaseName is null or empty.
  //      /// </summary>
		//static bool TrySetBaseName(this BaseType bt, string newBaseName)
  //      {
  //          if (bt.TopNode is null || newBaseName == "") return false;
  //          if (bt.BaseName == newBaseName) return true;
  //          if (!newBaseName.IsValidVariableName()) return false;

  //          var tn = (_ITopNode)bt.TopNode;
  //          if (tn._UniqueBaseNames.TryGetValue(newBaseName, out _) == false) return false;
  //          bt.BaseName = newBaseName;
  //          return true;
  //      }
	}
}
