using System.Reflection;



//using SDC;
namespace SDC.Schema
{
	public class AttributeComparer : Comparer<PropertyInfo>
	{
		public override int Compare(PropertyInfo? piA, PropertyInfo? piB)
		{
			//In XML Schemas, it appears that base class (Schema base type) xml elements always come before subclass elements, regardless of the XmlElementAttribute Order value.
			if (piA.DeclaringType.IsSubclassOf(piB.DeclaringType))
				return 1;  //base class xml orders come before subclasses
			if (piB.DeclaringType.IsSubclassOf(piA.DeclaringType))
				return -1; //base class xml orders come before subclasses

			return piA.Name.CompareTo(piB.Name);
		}
	}
}
