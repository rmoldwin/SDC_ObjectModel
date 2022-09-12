

//using SDC;
namespace SDC.Schema
{
	public class PropertyInfoOrderedComparer : Comparer<PropertyInfoOrdered>
	{
		public override int Compare(PropertyInfoOrdered pioA, PropertyInfoOrdered pioB)
		{
			//In XML Schemas, it appears that base class (Schema base type) xml elements always come before subclass elements, regardless of the XmlElementAttribute Order value.
			if (pioA.PropertyInfo.DeclaringType.IsSubclassOf(pioB.PropertyInfo.DeclaringType))
				return 1;  //base class xml orders come before subclasses; ancNodeA is the base type here
			if (pioB.PropertyInfo.DeclaringType.IsSubclassOf(pioA.PropertyInfo.DeclaringType))
				return -1; //base class xml orders come before subclasses; ancNodeB is the base type here

			//Determine the comparison based on the xmlOrder in the XmlElementAttributes
			if (pioA.XmlOrder < pioB.XmlOrder)
				return -1;
			if (pioB.XmlOrder < pioA.XmlOrder)
				return 1;
			else return 0;
		}
	}
}
