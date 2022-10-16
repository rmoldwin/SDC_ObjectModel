using System;
using System.Linq;

namespace SDC.Schema
{
	public static class ListFieldTypeExtensions
	{
		public static LookupEndPointType AddEndpoint(this ListFieldType lf)
		{
			if (lf.List == null)
			{
				var lep = new LookupEndPointType(lf);
				lf.LookupEndpoint = lep;
				return lep;
			}
			else throw new InvalidOperationException("Can only add LookupEndpoint to ListField if List object is not present");
		}
		public static ListType AddList(this ListFieldType lf)
		{
			ListType list;  //this is not the .NET List class; It's an answer list
			if (lf.List is null)
			{
				list = new ListType(lf);
				lf.List = list;
			}
			else list = lf.List;

			//The "list" item contains a list<DisplayedType>, to which the ListItems and ListNotes (DisplayedItems) are added.
			list.QuestionListMembers ??= new List<DisplayedType>();

			return list;
		}

	}
}
