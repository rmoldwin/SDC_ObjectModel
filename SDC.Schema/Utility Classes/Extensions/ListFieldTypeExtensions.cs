using System;
using System.Data.SqlTypes;
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
		public static LookupEndPointType GetLookupEndpoint(this ListFieldType lf)
		{
			if (lf.List is not null)
				throw new InvalidOperationException
					("You can only add a LookupEndpoint to ListField if a List object is not present on ListField");

			if (lf.LookupEndpoint is not null) 
				return lf.LookupEndpoint;
			else return new LookupEndPointType(lf);
 		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="lf"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static ListType GetList(this ListFieldType lf)
		{
			if(lf.LookupEndpoint is not null)
				throw new InvalidOperationException
					("You can only add a List to a ListField if a LookupEndpoint object is not present on the ListField");
			ListType list;  //this is not the .NET List class; It's an answer list
			if (lf.List is null) list = new ListType(lf);

			//The "list" item contains a list<DisplayedType>, to which the ListItems and ListNotes (DisplayedItems) are added.
			lf.List!.QuestionListMembers ??= new List<DisplayedType>();

			return lf.List;
		}

	}
}
