

//using SDC;
namespace SDC.Schema
{
	public static class IChildItemsParentExtensions
	{
		public static SectionItemType AddChildSection<T>(this IChildItemsParent<T> T_Parent, string id, string? defTitle = null, int insertPosition = -1) where T : BaseType, IChildItemsParent<T>
		{
			var childItems = T_Parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var sNew = new SectionItemType(childItems, id);
			sNew.title = defTitle;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, sNew);

			return sNew;
		}
		public static QuestionItemType AddChildQuestion<T>(this IChildItemsParent<T> T_Parent, QuestionEnum qType, string id, string title = null, int insertPosition = -1) where T : BaseType, IChildItemsParent<T>
		{
			var childItems = T_Parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var qNew = new QuestionItemType(childItems, id);
			//ListFieldType lf;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, qNew);

			switch (qType)
			{
				case QuestionEnum.QuestionSingle:
					qNew.AddListFieldToQuestion().AddList();
					break;
				case QuestionEnum.QuestionMultiple:
					qNew.AddListFieldToQuestion().AddList();
					qNew.ListField_Item.maxSelections = 0;
					break;
				case QuestionEnum.QuestionFill:
					qNew.AddQuestionResponseField(out DataTypes_DEType _);
					break;
				case QuestionEnum.QuestionLookupSingle:
					qNew.AddListFieldToQuestion().AddEndpoint();
					break;
				case QuestionEnum.QuestionLookupMultiple:
					qNew.AddListFieldToQuestion().AddEndpoint();

					break;
				default:
					throw new NotSupportedException($"{qType} is not supported");
			}
			qNew.title = title;
			return qNew;
		}

		public static QuestionItemType AddChildQuestionResponse<T>(this IChildItemsParent<T> T_Parent,
			string id,
			out DataTypes_DEType deType,
			string defTitle = null,
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			string textAfterResponse = null,
			string units = null,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object valDefault = null) where T : BaseType, IChildItemsParent<T>
		{
			var childItems = T_Parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var qNew = new QuestionItemType(childItems, id);
			qNew.title = defTitle;

			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, qNew);
			var rf = qNew.AddQuestionResponseField(out deType, dt, dtQuant, valDefault);
			rf.AddResponseUnits(units);
			rf.AddTextAfterResponse(textAfterResponse);

			return qNew;

		}
		public static DisplayedType AddChildDisplayedItem<T>(this IChildItemsParent<T> T_Parent, string id, string defTitle = null, int insertPosition = -1) where T : BaseType, IChildItemsParent<T>
		{
			var childItems = T_Parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var dNew = new DisplayedType(childItems, id);
			dNew.title = defTitle;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, dNew);

			return dNew;
		}
		public static ButtonItemType AddChildButtonAction<T>(this IChildItemsParent<T> T_Parent, string id, string defTitle = null, int insertPosition = -1) where T : BaseType, IChildItemsParent<T>
		{
			var childItems = T_Parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var btnNew = new ButtonItemType(childItems, id);
			btnNew.title = defTitle;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, btnNew);

			// TODO: Add AddButtonActionTypeItems(btnNew);
			return btnNew;
		}
		public static InjectFormType AddChildInjectedForm<T>(this IChildItemsParent<T> T_Parent, string id, int insertPosition = -1) where T : BaseType, IChildItemsParent<T>
		{
			var childItems = T_Parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var injForm = new InjectFormType(childItems, id);
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, injForm);
			//TODO: init this InjectForm object

			return injForm;
		}
		public static bool HasChildItems<T>(this IChildItemsParent<T> T_Parent) where T : BaseType, IChildItemsParent<T>
		{
			{
				if (T_Parent?.ChildItemsNode?.ChildItemsList != null)
				{
					foreach (var n in T_Parent.ChildItemsNode.ChildItemsList)
					{ if (n != null) return true; }
				}
			}
			return false;
		}
		public static ChildItemsType AddChildItemsNode<T>(this IChildItemsParent<T> T_Parent) where T : BaseType, IChildItemsParent<T>
		{
			ChildItemsType childItems = null;  //this class contains an "Items" list
			if (T_Parent == null)
				throw new ArgumentNullException(nameof(T_Parent));
			//return childItems; 
			else if (T_Parent.ChildItemsNode == null)
			{
				childItems = new ChildItemsType(T_Parent as BaseType);
				T_Parent.ChildItemsNode = childItems;  //This may be null for the Header, Body and Footer  - need to check this
													   //SdcUtil.AssignXmlElementAndOrder(childItems);
			}
			else //(T_Parent.ChildItemsNode != null)
				childItems = T_Parent.ChildItemsNode;

			if (childItems.ChildItemsList == null)
				childItems.ChildItemsList = new List<IdentifiedExtensionType>();

			return childItems;
		}
	}
}
