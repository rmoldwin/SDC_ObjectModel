
using System.Collections.Immutable;
using System.Drawing;
using SDC.Schema.Extensions;

namespace SDC.Schema
{
	public static class QuestionItemTypeExtensions
	{
		private static QuestionItemType ConvertToQR_(this QuestionItemType q, bool testOnly = false)
		{ throw new NotImplementedException(); } //abort if children present
		private static QuestionItemType ConvertToQS_(this QuestionItemType q, bool testOnly = false)
		{ throw new NotImplementedException(); }
		private static QuestionItemType ConvertToQM_(this QuestionItemType q, int maxSelections = 0, bool testOnly = false)
		{ throw new NotImplementedException(); }
		//private static DisplayedType ConvertToDI_(this QuestionItemType q, bool testOnly = false)
		//{ throw new NotImplementedException(); } //abort if LIs or children present
		//private static QuestionItemType ConvertToSection_(this QuestionItemType q, bool testOnly = false)
		//{ throw new NotImplementedException(); }
		//private static QuestionItemType ConvertToLookup_(this QuestionItemType q, bool testOnly = false)
		//{ throw new NotImplementedException(); }//abort if LIs present

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		public static QuestionEnum GetQuestionSubtype(this QuestionItemType q)
		{
			if (q.ResponseField_Item != null) return QuestionEnum.QuestionFill;
			if (q.ListField_Item is null) return QuestionEnum.QuestionRaw;
			if (q.ListField_Item.LookupEndpoint == null && q.ListField_Item.maxSelections == 1) return QuestionEnum.QuestionSingle;
			if (q.ListField_Item.LookupEndpoint == null && q.ListField_Item.maxSelections != 1) return QuestionEnum.QuestionMultiple;
			if (q.ListField_Item.LookupEndpoint != null && q.ListField_Item.maxSelections == 1) return QuestionEnum.QuestionLookupSingle;
			if (q.ListField_Item.LookupEndpoint != null && q.ListField_Item.maxSelections != 1) return QuestionEnum.QuestionLookupMultiple;
			if (q.ListField_Item.LookupEndpoint != null) return QuestionEnum.QuestionLookup;

			return QuestionEnum.QuestionGroup;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <param name="id"></param>
		/// <param name="defTitle"></param>
		/// <param name="insertPosition"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		private static ListItemType X_AddListItem(this QuestionItemType q, string id, string? defTitle = null, int insertPosition = -1)
		{  //Check for QS/QM first!
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)
			{
				var lf = q.GetListField();
				var list = lf.GetList();  //AddList checks for pre-existing List object

				ListItemType li = new ListItemType(list, id);
				//ListItemType li = new ListItemType(null, id);
				li.title = defTitle;
				int count = list?.QuestionListMembers.Count ?? 0;
				if (insertPosition < 0 || insertPosition > count) insertPosition = count;
				list?.QuestionListMembers.Insert(insertPosition, li);

				return li;
			}
			else throw new InvalidOperationException("You can only add a ListItem to a QuestionSingle or QuestionMultiple");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <param name="id"></param>
		/// <param name="defTitle"></param>
		/// <param name="insertPosition"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static ListItemType AddListItem(this QuestionItemType q, string id, string? defTitle = null, int insertPosition = -1)
		{  //Check for QS/QM first!
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)
			{
				ListFieldType lf = q.GetListField();
				ListType list = lf.GetList(); 

				//ListItemType li = new ListItemType(list, id);
				ListItemType li = new ListItemType(list, id);  //register node with null parent.  This prevents the node from being registered in any TopNode dictionaries
				li.title = defTitle;
				int count = list.QuestionListMembers.Count;
				if (insertPosition < 0 || insertPosition > count) insertPosition = count;
				list.QuestionListMembers.Insert(insertPosition, li);

				//li.RegisterNodeAndParent(list);
				return li;
			}
			else throw new InvalidOperationException("You can only add a ListItem to a QuestionSingle or QuestionMultiple");
		}







		/// <summary>
		/// Add a new ListItemResponse (LIR) to a Question.  <br/>
		/// The supplied Question (<paramref name="q"/>) must be a QuestionSingle or QuestionMultiple.
		/// </summary>
		/// <param name="q"></param>
		/// <param name="id"></param>
		/// <param name="deType">An out parameter containing the added SDC datatype object, e.g., a <see cref="string_DEtype"/> object.</param>
		/// <param name="defTitle">The title attribute of the LIR</param>
		/// <param name="insertPosition"></param>
		/// <param name="dt">The datatype for the LIR.</param>
		/// <param name="responseRequired"></param>
		/// <param name="textAfterResponse"></param>
		/// <param name="units"></param>
		/// <param name="dtQuant"></param>
		/// <param name="valDefault"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static ListItemType AddListItemResponse(this QuestionItemType q,
			string id,
			out DataTypes_DEType deType,
			string defTitle = "",
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			bool responseRequired = false,
			string textAfterResponse = null,
			string units = null,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object valDefault = null
			)
		{
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw) //TODO: handle the last case
			{
				var li = q.AddListItem(id, defTitle, insertPosition);
				var lirf = li.AddListItemResponseField();
				var rsp = lirf.AddDataType(dt, dtQuant, valDefault);

				lirf.responseRequired = responseRequired;
				lirf.AddResponseUnits(units);
				lirf.AddTextAfterResponse(textAfterResponse);

				deType = IDataHelpers.AddDataTypesDE(lirf, dt, dtQuant, valDefault);
				return li;

			}
			else throw new InvalidOperationException("You can only add a ListItem to a QuestionSingle or QuestionMultiple");
		}
		private static ListItemType X_AddListItemResponse(this QuestionItemType q,
			string id,
			out DataTypes_DEType deType,
			string defTitle = null,
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			bool responseRequired = false,
			string textAfterResponse = null,
			string units = null,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object valDefault = null
			)
		{
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw) //TODO: handle the last case
			{
				var li = q.AddListItem(id, defTitle, insertPosition);
				var lirf = li.AddListItemResponseField();
				var rsp = lirf.AddDataType(dt, dtQuant, valDefault);

				lirf.responseRequired = responseRequired;
				lirf.AddResponseUnits(units);
				lirf.AddTextAfterResponse(textAfterResponse);

				deType = IDataHelpers.AddDataTypesDE(lirf, dt, dtQuant, valDefault);
				return li;

			}
			else throw new InvalidOperationException("You can only add a ListItem to a QuestionSingle or QuestionMultiple");
		}
		public static DisplayedType AddDisplayedTypeToList(this QuestionItemType q,
			string id,
			string? title = null,
			int insertPosition = -1)
		{
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)//TODO: handle the last case
			{
				if (q.ListField_Item is null) q.GetListField();
				ListType? list = q.ListField_Item!.List;
				list ??= q.ListField_Item.GetList();

				return list.AddDisplayedType(id, title, insertPosition);
			}
			else throw new InvalidOperationException("You can only add a DisplayedItem to a QuestionSingle or QuestionMultiple");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="q"></param>
		/// <param name="deType"></param>
		/// <param name="dataType"></param>
		/// <param name="dtQuant"></param>
		/// <param name="valDefault"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static ResponseFieldType AddQuestionResponseField(this QuestionItemType q,
			out DataTypes_DEType deType,
			ItemChoiceType dataType = ItemChoiceType.@string,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null)
		{
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)
			{
				var rf = new ResponseFieldType(q);
				q.ResponseField_Item = rf;
				//_ = new DataTypes_DEType(rf);
				deType = rf.AddDataType(dataType, dtQuant, valDefault);
				return rf;
			}
			else throw new Exception("A Question subtype has already been assigned to the Question.");
		}

		/// <summary>
		/// This method returns the non-null ListField of the supplied question.
		/// </summary>
		/// <param name="q"></param>
		/// <returns></returns>
		public static ListFieldType GetListField(this QuestionItemType q)
		{
			if (q.ListField_Item == null)
			{
				var listField = new ListFieldType(q);
				q.ListField_Item = listField;
			}
			return q.ListField_Item; //TODO: handle error if not Qraw
		}
		/// <summary>
		/// In a QuestionSingle (QS) or QuestionMultiple (QM), retrieves an ordered List&lt;DisplayedType> of all ListItems and DisplayedItems owned by the QS or QM. <br/>
		/// If the List&lt;DisplayedType> object was null, a new empty List&lt;DisplayedType> is created and returned.<br/>
		/// If the List&lt;DisplayedType> object contained no elements, the empty List&lt;DisplayedType> is returned.<br/>
		/// If the supplied Question object (<paramref name="q"/> ) was not a QS, or QM (i.e., it did not contain a ListField child object), <br/>
		/// then null will be returned.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>Sorted ImmutableList&lt;DisplayedType> or null if the Question has no ListField object</returns>
		public static ImmutableList<DisplayedType>? GetListItems(this QuestionItemType q)
		{
			//return q?.ListField_Item?.List?.GetChildNodes()?.Cast<DisplayedType>().ToList();
			return q?.ListField_Item?.List?.Items.ToImmutableList();
		}
		/// <summary>
		/// In a QuestionResponse (QR) node, retrieve the DataTypeDE_Item (e.g., &lt;string/>, &lt;decimal/>)
		/// </summary>
		/// <param name="q"></param>
		/// <returns>I a QR node, returns DataTypeDE_Item.  Otherwise returns null </returns>
		public static BaseType? GetResponseDataTypeNode(this QuestionItemType q) =>
			q?.ResponseField_Item?.Response?.DataTypeDE_Item;

		/// <summary>
		/// Retrieves all ListItem and DisplayedItem nodes under the List node, <br/>
		/// as well as all IET nodes under the ChildItems node, in sorted order.
		/// </summary>
		/// <param name="q"></param>
		/// <returns> List&lt;IdentifiedExtensionType> containing all nodes, or null if no nodes are present. </returns>
		public static List<IdentifiedExtensionType>? GetListAndChildItemsList(this QuestionItemType q)
		{
			List<IdentifiedExtensionType> lst = new();

			var liLst = q.GetListItems()?.Cast<IdentifiedExtensionType>();
			if(liLst is not null) lst.AddRange(liLst);

			var ciLst = q.GetChildItemsList();
			if (ciLst is not null) lst?.AddRange(ciLst);
			return lst;
		}

		/// <summary>
		/// Returns true if any List IET nodes and other child IET nodes are present under the question node;
		/// nodeList will contain all ListItem and DisplayedItem nodes udner the List node, <br/>
		/// as well as all IET nodes under the ChildItems node, in sorted order.
		/// </summary>
		/// <param name="q"></param>
		/// <param name="nodeList">Contains all ListItems and IET nodes under ChildItems, in sorted order</param>
		/// <returns>Returns true if any ListItem or other child IET nodes are present under the question node.<br/>
		/// Returns false if no child nodes are found
		/// </returns>
		public static bool TryGetListAndChildIETNodes(this QuestionItemType q, out List<IdentifiedExtensionType>? nodeList)
		{
			nodeList = GetListAndChildItemsList(q);
			return nodeList?.Any()??false;

		}

	}

}
