

//using SDC;
using System.CodeDom;

namespace SDC.Schema.Extensions
{
	public static class ListTypeExtensions

	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lt"></param>
		/// <param name="id">SDC ID attribute for the new node</param>
		/// <param name="title">Default title attribute for the the new node</param>
		/// <param name="insertPosition">The 0-based position where the new node should be inserted.  
		/// The default value (-1) will insert the new node as the last node.</param>
		/// <returns>New ListItem node</returns>
		public static ListItemType AddListItem(this ListType lt, string id, string? title = null, int insertPosition = -1) //check that no ListItemResponseField object is present
		{
			ListItemType li = new ListItemType(lt, id);
			li.title = title;
			//if (lt.QuestionListMembers is null)
			//	lt.QuestionListMembers = new();
			//var count = lt.QuestionListMembers.Count;
			//if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			//lt.QuestionListMembers.Insert(insertPosition, li);
			return li;
		}
		/// <summary>
		/// 
		/// 
		/// </summary>
		/// <param name="lt"></param>
		/// <param name="id">SDC ID attribute for the new node</param>
		/// <param name="title">title attribute for the the new node</param>
		/// <param name="insertPosition">The 0-based position where the new node should be inserted.  
		/// The default value (-1) will insert the new node as the last node.</param>
		/// <param name="dt"></param>
		/// <param name="responseRequired"></param>
		/// <param name="textAfterResponse"></param>
		/// <param name="units"></param>
		/// <param name="dtQuant"></param>
		/// <param name="valDefault"></param>
		/// <returns>New ListItemResponse node</returns>
		public static ListItemType AddListItemResponse(this ListType lt,
			string id,
			string? title = null,
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			bool responseRequired = false,
			string? textAfterResponse = null,
			string? units = null,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null)
		{
			var li = lt.AddListItem(id, title, insertPosition);
			var lirf = li.AddListItemResponseField();
			lirf.AddDataType(dt, dtQuant, valDefault);
			lirf.responseRequired = responseRequired;
			if(!string.IsNullOrWhiteSpace(units)) 
				lirf.AddResponseUnits(units);
			if (!string.IsNullOrWhiteSpace(textAfterResponse)) 
				lirf.AddTextAfterResponse(textAfterResponse);

			return li;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="list"></param>
		/// <param name="id">SDC ID attribute for the new node</param>
		/// <param name="title">Default title attribute for the the new node</param>
		/// <param name="insertPosition">The 0-based position where the new node should be inserted.  
		/// The default value (-1) will insert the new node as the last node.</param>
		/// <returns>New DisplayedType node</returns>
		public static DisplayedType AddDisplayedType(this ListType list, string id, string? title = null, int insertPosition = -1)
		{
			return new DisplayedType(list, id, insertPosition) { title = title };
		}

	}
}
