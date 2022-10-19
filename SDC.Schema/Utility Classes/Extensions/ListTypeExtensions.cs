

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class ListTypeExtensions

	{
		public static ListItemType AddListItem(this ListType lt, string id, string? defTitle = null, int insertPosition = -1) //check that no ListItemResponseField object is present
		{
			ListItemType li = new ListItemType(lt, id);
			li.title = defTitle;
			var count = lt.QuestionListMembers.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			lt.QuestionListMembers.Insert(insertPosition, li);
			return li;
		}

		public static ListItemType AddListItemResponse(this ListType lt,
			string id,
			string? defTitle = null,
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			bool responseRequired = false,
			string? textAfterResponse = null,
			string? units = null,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null)
		{
			var li = lt.AddListItem(id, defTitle, insertPosition);
			var lirf = li.AddListItemResponseField();
			lirf.AddDataType(dt, dtQuant, valDefault);
			lirf.responseRequired = responseRequired;
			if(units is not null) lirf.AddResponseUnits(units);
			lirf.AddTextAfterResponse(textAfterResponse);

			return li;
		} //check that no ListFieldType object is present
		public static DisplayedType AddDisplayedType(this ListType list, string id, string? defTitle = null, int insertPosition = -1)
		{
			var di = new DisplayedType(list, id) { title = defTitle };
			var count = list.QuestionListMembers.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			list.QuestionListMembers.Insert(insertPosition, di);

			return di;
		}
	}
}
