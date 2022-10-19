
using System.Collections.Immutable;
using System.Drawing;
using SDC.Schema.Extensions;

namespace SDC.Schema
{
	public static class QuestionItemTypeExtensions
	{
		public static QuestionItemType ConvertToQR_(this QuestionItemType q, bool testOnly = false)
		{ throw new NotImplementedException(); } //abort if children present
		public static QuestionItemType ConvertToQS_(this QuestionItemType q, bool testOnly = false)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ConvertToQM_(this QuestionItemType q, int maxSelections = 0, bool testOnly = false)
		{ throw new NotImplementedException(); }
		//public static DisplayedType ConvertToDI_(this QuestionItemType q, bool testOnly = false)
		//{ throw new NotImplementedException(); } //abort if LIs or children present
		//public static QuestionItemType ConvertToSection_(this QuestionItemType q, bool testOnly = false)
		//{ throw new NotImplementedException(); }
		//public static QuestionItemType ConvertToLookup_(this QuestionItemType q, bool testOnly = false)
		//{ throw new NotImplementedException(); }//abort if LIs present

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
		public static ListItemType AddListItem(this QuestionItemType q, string id, string? defTitle = null, int insertPosition = -1)
		{  //Check for QS/QM first!
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)
			{
				if (q.ListField_Item is null) q.AddListFieldToQuestion();
				ListType? list = q.ListField_Item?.List;
				if (list is null) q.ListField_Item?.AddList();

				ListItemType li = new ListItemType(list!, id);
				li.title = defTitle;
				int count = list?.QuestionListMembers.Count ?? 0;
				if (insertPosition < 0 || insertPosition > count) insertPosition = count;
				list?.QuestionListMembers.Insert(insertPosition, li);

				return li;
			}
			else throw new InvalidOperationException("Can only add ListItem to QuestionSingle or QuestionMultiple");
		}
		public static ListItemType AddListItemResponse(this QuestionItemType q,
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
			else throw new InvalidOperationException("Can only add ListItem to QuestionSingle or QuestionMultiple");
		}
		public static DisplayedType AddDisplayedTypeToList(this QuestionItemType q,
			string id,
			string defTitle = null,
			int insertPosition = -1)
		{
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionMultiple ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionSingle ||
				q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)//TODO: handle the last case
			{
				if (q.ListField_Item is null) q.AddListFieldToQuestion();
				ListType list = q.ListField_Item.List;
				if (list is null) list = q.ListField_Item.AddList();

				return list.AddDisplayedType(id, defTitle, insertPosition);
			}
			else throw new InvalidOperationException("Can only add DisplayedItem to QuestionSingle or QuestionMultiple");
		}

		public static ResponseFieldType AddQuestionResponseField(this QuestionItemType q,
			out DataTypes_DEType deType,
			ItemChoiceType dataType = ItemChoiceType.@string,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object valDefault = null)
		{
			if (q.GetQuestionSubtype() == QuestionEnum.QuestionRaw)
			{
				var rf = new ResponseFieldType(q);
				q.ResponseField_Item = rf;
				_ = new DataTypes_DEType(rf);
				deType = rf.AddDataType(dataType, dtQuant, valDefault);
				return rf;
			}
			else throw new Exception("A Question subtype has already been assigned to the Question.");
		}

		public static ListFieldType AddListFieldToQuestion(this QuestionItemType q)
		{
			if (q.ListField_Item == null)
			{
				var listField = new ListFieldType(q);
				q.ListField_Item = listField;
			}
			return q.ListField_Item; //TODO: handle error if not Qraw
		}
		/// <summary>
		/// In a QuestionSingle (QS) or QuestionMultiple (QR), retrieve an ordered List&lt;DisplayedType> of all ListItems and DisplayedItems owned by the QS or QM.
		/// </summary>
		/// <param name="q"></param>
		/// <returns>Ordered List&lt;DisplayedType> or null if the Question has no child ListItem or DisplayedType nodes</returns>
		static ImmutableList<DisplayedType>? ListItems(this QuestionItemType q)
		{
			return q?.ListField_Item?.List?.GetChildNodes()?.Cast<DisplayedType>().ToImmutableList();
		}
		/// <summary>
		/// In a QuestionResponse (QR) node, retrieve the DataTypeDE_Item (e.g., &lt;string/>, &lt;decimal/>)
		/// </summary>
		/// <param name="q"></param>
		/// <returns>I a QR node, returns DataTypeDE_Item.  Otherwise returns null </returns>
		static BaseType? ResponseDataTypeNode(this QuestionItemType q) =>
			q?.ResponseField_Item?.Response?.DataTypeDE_Item;

	}

}
