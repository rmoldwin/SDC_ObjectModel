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
	/// Immutable record struct to hold information about SDC object properties that will become XML attributes when serialized to XML.
	/// </summary>
	public readonly record struct AttributeInfo
	{
		/// <summary>
		/// Constructor for <see cref="AttributeInfo"/><br/>
		/// A default value of "AAAAAAAAAAAAAAAAAAAAAA" for <paramref name="sGuid"/> indicates that the attribute does not exist because its SDC node (with a default sGuid value) does not exist.<br/>
		/// If the <paramref name="sGuid"/> is valid, the node exists, but the attribute either does not exist, or is present at its default value.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="sGuid">The ShortGuid (sGuid) property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.</param>
		/// <param name="attributeValue">The value of this attribute instance.</param>
		/// <param name="attributePropInfo">The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element</param>
		public AttributeInfo(BaseType? parentNode, 
			ShortGuid sGuid, 
			object? attributeValue, 
			PropertyInfo? attributePropInfo, 
			int order)
		{			
			if(sGuid == "AAAAAAAAAAAAAAAAAAAAAA" || attributePropInfo is null)
			{ this.IsEmpty = true; }
			var parentIETNode = parentNode?.ParentIETnode;
			this.sGuid = sGuid;
			this.ParentNodesGuid = parentNode?.sGuid;
			this.ParentIETNodesGuid = parentIETNode?.sGuid;
			this.ParentNodeObjectID = parentNode?.ObjectID??0;
			this.ParentIETNodeObjectID = parentIETNode?.ObjectID;
			this.Value = attributeValue;
			if (attributePropInfo is not null)
			{
				this.DefaultValue = SdcUtil.GetAttributeDefaultValue(attributePropInfo);
				this.AttributePropInfo = attributePropInfo;
				this.Name = AttributePropInfo.Name;
			}
			this.Order = order;
		}


		/// <summary>
		/// The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).
		/// </summary>
		public PropertyInfo? AttributePropInfo { get; }

		/// <summary>
		/// The value of this attribute instance.
		/// </summary>
		public object? Value { get; }

		/// <summary>
		/// The value of the DefaultValueAttribute which is present on some XML attribute properties.
		/// The Value property of DefaultValueAttribute contains the property default value. 
		/// </summary>
		public object? DefaultValue { get; }

		/// <summary>
		/// The ShortGuid property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.
		/// The sGuid can be used to retrieve an SDC object, while not holding onto an object reference inside this struct.
		/// </summary>
		public ShortGuid? ParentNodesGuid { get; }
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
		public string? Name { get; }
		/// <summary>
		/// The serialized ordinal position of the attribute in the current element
		/// </summary>
		public int Order { get; }
		public ShortGuid? sGuid { get; }
		/// <summary>
		/// If true, then the source node's sGuid is populated, then the attribute is not serialized, and all other fields are uninitialized. <br/>
		/// if sGuid has a default value ("AAAAAAAAAAAAAAAAAAAAAA"), then the node is also null.  This is a default value for this struct. 
		/// </summary>
		public bool IsEmpty { get; }

	}
	/// <summary>
	/// Immutable record struct to hold information about SDC object properties that will become XML attributes when serialized to XML.<br/>
	/// Does not include information about the SDC object itself, or any parent node properties
	/// </summary>
	public readonly record struct AttributeInfoLite
	{
		/// <summary>
		/// Constructor for <see cref="AttributeInfo"/>
		/// </summary>
		/// <param name="attributeValue">The value of this attribute instance.</param>
		/// <param name="attributePropInfo">The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element</param>
		public AttributeInfoLite(
			object? attributeValue,
			PropertyInfo attributePropInfo,
			int order)
		{
			this.AttributeValue = attributeValue;
			this.DefaultValue = SdcUtil.GetAttributeDefaultValue(attributePropInfo);
			this.AttributePropInfo = attributePropInfo;
			this.Name = AttributePropInfo.Name;
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
		/// Name of the attribute, as it will appear in XML
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The serialized ordinal position of the attribute in the current element
		/// </summary>
		public int Order { get; }

		/// <summary>
		/// If true, then the source node's sGuid is populated, then the attribute is not serialized, and all other fields are uninitialized. <br/>
		/// if sGuid has a default value ("AAAAAAAAAAAAAAAAAAAAAA"), then the node is also null.  This is a default value for this struct. 
		/// </summary>
		public bool IsEmpty { get; }

	}
}
