using System;
using System.Linq;

namespace SDC.Schema.Extensions
{
	public static class ListFieldTypeExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lf"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static LookupEndPointType AddEndpoint(this ListFieldType lf)
		{
			if (lf.List is null)
			{
				var lep = new LookupEndPointType(lf);
				lf.LookupEndpoint = lep;
				return lep;
			}
			else throw new InvalidOperationException
					("You can only add a LookupEndpoint to ListField if a List object is not present on ListField");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lf"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static ListType AddList(this ListFieldType lf)
		{
			if(lf.LookupEndpoint is not null)
				throw new InvalidOperationException
					("You can only add a List to a ListField if a LookupEndpoint object is not present on the ListField");
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
