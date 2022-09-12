using System.Reflection;



//using SDC;
namespace SDC.Schema
{
	public struct PropertyInfoMetadata
	{
		public PropertyInfoMetadata(
			PropertyInfo? propertyInfo,
			string? propName,
			int itemIndex,
			IEnumerable<BaseType>? ieItems,
			int xmlOrder,
			int maxXmlOrder,
			string? xmlElementName
			)

		{
			PropertyInfo = propertyInfo;
			PropName = propName;
			ItemIndex = itemIndex;
			IeItems = ieItems;
			XmlOrder = xmlOrder;
			MaxXmlOrder = maxXmlOrder;
			XmlElementName = xmlElementName;


		}
		public PropertyInfo? PropertyInfo { get; }
		/// <summary>
		/// Name of the class property that contains the requested object
		/// </summary>
		public string? PropName { get; }
		/// <summary>
		/// If the requested object is held by a parent IEnumerable (usually an array of List), itemIndex contains the index of the requested object inside ieItems
		/// If the requested object is represented by a non-IEnumerable property, then itemIndex = -1 and ieItems is null.
		/// </summary>
		public int ItemIndex { get; }
		/// <summary>
		/// If the requested object is held by a parent IEnumerable (usually an array of List), the IEnumerable is retuurned as ieItems.
		/// If the requested object is represented by a non-IEnumerable property, then itemIndex = -1 and ieItems is null.
		/// </summary>
		public IEnumerable<BaseType>? IeItems { get; }
		/// <summary>
		/// xmlOrder is the Order found in the XmlElementAttribute's Order property.  
		/// xmlOrder represents the element order in the SDC XML.  
		/// However, all inherited properties of the requested object occur as SDC XML elements that precede the current object's XML element, 
		/// even when the inherited properties have a higher xmlOrder value.
		/// </summary>
		public int XmlOrder { get; }
		/// <summary>
		/// maxXmlOrder is the maximum xmlOrder value found on properties in the XmlElementAttribute's Order field of the requested object's parent class 
		/// The parent class codes for the property member that represents the requested object. 
		/// Property members in the parent class may be decorated with XmlElementAttribute attributes, and these attributes have an "Order" field
		/// </summary>
		public int MaxXmlOrder { get; }
		/// <summary>
		/// Name of the XML element that is used to represent the requested object
		/// </summary>
		public string? XmlElementName { get; }

		public override string ToString()
		{
			return @$"PropertyInfoMetadata:
---------------------------------------
IeItems.Count   {IeItems?.Count() ?? 0}
ItemIndex:      {ItemIndex}
PropName:       {PropName}
XmlOrder:       {XmlOrder}
MaxXmlOrder:    {MaxXmlOrder}
XmlElementName: {XmlElementName}
---------------------------------------";
		}

	}
}
