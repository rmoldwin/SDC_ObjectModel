using System.Reflection;



//using SDC;
namespace SDC.Schema
{
	public readonly struct X_PropertyInfoOrdered
	{
		public X_PropertyInfoOrdered(PropertyInfo propertyInfo, int xmlOrder)

		{
			PropertyInfo = propertyInfo;
			XmlOrder = xmlOrder;
		}
		public PropertyInfo PropertyInfo { get; }

		/// <summary>
		/// xmlOrder is the Order found in the XmlElementAttribute's Order property.  
		/// xmlOrder represents the element order in the SDC XML.  
		/// However, all inherited properties of the requested object occur as SDC XML elements that precede the current object's XML element, 
		/// even when the inherited properties have a higher xmlOrder value.
		/// </summary>
		public int XmlOrder { get; }
	}
}
