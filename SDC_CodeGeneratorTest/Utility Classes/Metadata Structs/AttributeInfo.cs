using CSharpVitamins;
using SDC.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
	/// <summary>
	/// Struct to hold information about SDC object properties that will become XML attributes when serialized to XML.
	/// </summary>
	public readonly struct AttributeInfo
	{
		/// <summary>
		/// Constructor for AttributeInfo&lt;T>
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="sdcElementNodeSguid">The ShortGuid property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.</param>
		/// <param name="attributeValue">The value of this attribute instance.</param>
		/// <param name="attributePropInfo">The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element</param>
		public AttributeInfo(BaseType parentNode, 
			ShortGuid sdcElementNodeSguid, 
			object? attributeValue, 
			PropertyInfo attributePropInfo, 
			int order)
		{
			var parentIETNode = parentNode.ParentIETypeNode;
			this.ParentNodesGuid = parentNode.sGuid;
			this.ParentIETNodesGuid = parentIETNode?.sGuid;
			this.ParentNodeObjectID = parentNode.ObjectID;
			this.ParentIETNodeObjectID = parentIETNode?.ObjectID;
			this.AttributeValue = attributeValue;
			this.DefaultValue = SdcUtil.GetAttributeDefaultValue(attributePropInfo);
			this.AttributePropInfo = attributePropInfo;
			this.Order = order;
		}


		/// <summary>
		/// The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).
		/// </summary>
		public PropertyInfo AttributePropInfo { get; }

		/// <summary>
		/// The value of this attribute instance.
		/// </summary>
		public object? AttributeValue { get; }

		/// <summary>
		/// The value of the DefaultValueAttribute which is present on some XML attribute properties.
		/// The Value property of DefaultValueAttribute contains the property default value. 
		/// </summary>
		public object? DefaultValue { get; }

		/// <summary>
		/// The ShortGuid property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.
		/// The sGuid can be used to retrieve an SDC object, while not holding onto an object reference inside this struct.
		/// </summary>
		public ShortGuid ParentNodesGuid { get; }
		/// <summary>
		/// The ObjectID property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.
		/// The ObjectID can be used to retrieve an SDC object, while not holding an object reference inside this struct.
		/// </summary>
		public int ParentNodeObjectID { get; }


		/// <summary>
		/// The ShortGuid property of the ParentIEType
		/// The sGuid can be used to retrieve an SDC object, while not holding onto an object reference inside this struct.
		/// The ParentIETNode may be null, generating a null value for ParentIETNodesGuid.
		/// </summary>
		public ShortGuid? ParentIETNodesGuid { get; }
		/// <summary>
		/// The ObjectID property of the ParentIEType
		/// The ObjectID can be used to retrieve an SDC object, while not holding an object reference inside this struct.
		/// The ParentIETNode may be null, generating a null value for ParentIETNodeObjectID.
		/// </summary>
		public int? ParentIETNodeObjectID { get; }


		/// <summary>
		/// Name of the attribute, as it will appear in XML
		/// </summary>
		public string Name { get=> AttributePropInfo.Name; }
		/// <summary>
		/// The serialized ordinal position of the attribute in the current element
		/// </summary>
		public int Order { get; }


	}
}
