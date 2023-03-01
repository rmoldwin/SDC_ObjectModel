

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class ListItemTypeExtensions
	{
		public static ListItemResponseFieldType AddListItemResponseField(this ListItemType li)
		{
			if (li.ListItemResponseField is not null) 
				throw new InvalidOperationException
					("A ListItemResponseField object (lirf) already exists.  " +
					"Run the RemoveRecursive() method on that ListItemResponseField object before replacing it");

			return new ListItemResponseFieldType(li);
		}
		public static EventType AddOnDeselect(this ListItemType li)
		{
			return new EventType(li, -1, "OnDeselect");
		}
		public static EventType AddOnSelect(this ListItemType li)
		{
			return new EventType(li, -1, "OnSelect"); ;
		}
		public static PredGuardType AddSelectIf(this ListItemType li)
		{
			return new PredGuardType(li, -1, "SelectIf");
		}
		public static PredGuardType AddDeselectIf(this ListItemType li)
		{
			return new PredGuardType(li, -1, "DeselectIf");
		}
		/// <summary>
		/// Retrieve the node that holds an SDC data type object.  
		/// After retrieval, the node should be cast to an appropriate SDC data type, e.g. string_DEtype, int_DEtype.
		/// </summary>
		/// <returns>A DataTypeDE_Item node or null if the node does not exist.</returns>
		public static BaseType? GetResponseDataTypeNode(this ListItemType li)
		{
			return li.ListItemResponseField?.Response?.DataTypeDE_Item;
		}
	}
}
