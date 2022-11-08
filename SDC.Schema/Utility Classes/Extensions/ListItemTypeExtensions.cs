

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
					"Run the Remove() method on that ListItemResponseField object before replacing it");
			var liRF = new ListItemResponseFieldType(li);
			li.ListItemResponseField = liRF;

			return liRF;
		}
		public static EventType AddOnDeselect(this ListItemType li)
		{
			var ods = new EventType(li);
			li.OnDeselect.Add(ods);
			return ods;
		}
		public static EventType AddOnSelect(this ListItemType li)
		{
			var n = new EventType(li);
			li.OnSelect.Add(n);
			return n;
		}
		public static PredGuardType AddSelectIf(this ListItemType li)
		{
			var n = new PredGuardType(li);
			li.SelectIf = n;
			return n;
		}
		public static PredGuardType AddDeSelectIf(this ListItemType li)
		{
			var n = new PredGuardType(li);
			li.DeselectIf = n;
			return n;
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
