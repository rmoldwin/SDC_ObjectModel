

//using SDC;
namespace SDC.Schema
{
	public static class ListItemTypeExtensions
	{
		public static ListItemResponseFieldType AddListItemResponseField(this ListItemType li)
		{
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
	}
}
