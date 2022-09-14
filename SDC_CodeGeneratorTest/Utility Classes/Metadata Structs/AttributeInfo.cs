using CSharpVitamins;
using SDC.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDC_CodeGeneratorTest.Utility_Classes.Metadata_Structs
{
	/// <summary>
	/// Sruct to hold information about SDC object properties that will become XML attributes when serialized to XML.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public readonly struct AttributeInfo
	{
		/// <summary>
		/// Constructor for AttributeInfo&lt;T>
		/// </summary>
		/// <param name="sdcElementNodeSguid">The ShortGuid property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.</param>
		/// <param name="attributeValue">The value of this attribute instance.</param>
		/// <param name="attributePropInfo">The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element</param>
		public AttributeInfo(ShortGuid sdcElementNodeSguid, object? attributeValue, PropertyInfo attributePropInfo, int order)
		{
			this.SdcElementNodesGuid = sdcElementNodeSguid;
			this.AttributeValue = attributeValue;
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
		/// The ShortGuid property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.
		/// The sGuid can be used to retrieve an SDC object, while not holding onto an object reference inside this struct.
		/// </summary>
		public ShortGuid SdcElementNodesGuid { get; }
		
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
