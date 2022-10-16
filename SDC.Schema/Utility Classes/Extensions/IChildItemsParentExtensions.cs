

//using SDC;
using System.Collections.Immutable;

namespace SDC.Schema
{
	public static class IChildItemsParentExtensions
	{
		public static SectionItemType AddChildSection(this IChildItemsParent parent, string id, string? defTitle = null, int insertPosition = -1) 
			//where T : BaseType, IChildItemsParent<T>
		{
			var childItems = parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var sNew = new SectionItemType(childItems, id);
			sNew.title = defTitle;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, sNew);

			return sNew;
		}
		public static QuestionItemType AddChildQuestion(this IChildItemsParent parent, QuestionEnum qType, string id, string title = null, int insertPosition = -1) 
			//where T : BaseType, IChildItemsParent<T>
		{
			var childItems = parent.AddChildItemsNode();
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
					qNew.AddQuestionResponseField(out DataTypes_DEType dtDE);
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

		public static QuestionItemType AddChildQuestionResponse(this IChildItemsParent parent,
			string id,
			out DataTypes_DEType deType,
			string defTitle = null,
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			string textAfterResponse = null,
			string units = null,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object valDefault = null) //where T : BaseType, IChildItemsParent<T>
		{
			var childItems = parent.AddChildItemsNode();
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
		public static DisplayedType AddChildDisplayedItem(this IChildItemsParent parent, string id, string defTitle = null, int insertPosition = -1) 
			//where T : BaseType, IChildItemsParent<T>
		{
			var childItems = parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var dNew = new DisplayedType(childItems, id);
			dNew.title = defTitle;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, dNew);

			return dNew;
		}
		public static ButtonItemType AddChildButtonAction(this IChildItemsParent parent, string id, string defTitle = null, int insertPosition = -1) 
			//where T : BaseType, IChildItemsParent<T>
		{
			var childItems = parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var btnNew = new ButtonItemType(childItems, id);
			btnNew.title = defTitle;
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, btnNew);

			// TODO: Add AddButtonActionTypeItems(btnNew);
			return btnNew;
		}
		public static InjectFormType AddChildInjectedForm(this IChildItemsParent parent, string id, int insertPosition = -1) 
			//where T : BaseType, IChildItemsParent<T>
		{
			var childItems = parent.AddChildItemsNode();
			var childItemsList = childItems.ChildItemsList;
			var injForm = new InjectFormType(childItems, id);
			var count = childItemsList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			childItemsList.Insert(insertPosition, injForm);
			//TODO: init this InjectForm object

			return injForm;
		}
		public static bool HasChildItems(this IChildItemsParent parent) 
			//where T : BaseType, IChildItemsParent<T>
		{
			{
				if (parent?.ChildItemsNode?.ChildItemsList != null)
				{
					foreach (var n in parent.ChildItemsNode.ChildItemsList)
					{ if (n != null) return true; }
				}
			}
			return false;
		}
		/// <summary>
		/// Retrieve all DisplayedTypes subsumed under the ChildItems node
		/// </summary>
		/// <returns>ImmutableList&lt;DisplayedType> or null if the ChildItems node is null or has no descendants </returns>
		static ImmutableList<DisplayedType>? GetChildDisplayedTypes(this IChildItemsParent parent) 
			//where T : BaseType, IChildItemsParent<T>
		{
			return parent.ChildItemsNode?.ChildItemsList?.Cast<DisplayedType>()?.ToImmutableList();
		}
		public static ChildItemsType AddChildItemsNode(this IChildItemsParent parent) 
			//where T : BaseType, IChildItemsParent
		{
			ChildItemsType childItems;  //this class contains an "Items" list
			if (parent == null)
				throw new ArgumentNullException(nameof(parent));
			//return childItems; 
			else if (parent.ChildItemsNode == null)
			{
				childItems = new ChildItemsType((BaseType)parent);
				parent.ChildItemsNode = childItems;  //This may be null for the Header, Body and Footer  - need to check this
													   //SdcUtil.AssignXmlElementAndOrder(childItems);
			}
			else //(parent.ChildItemsNode != null)
				childItems = parent.ChildItemsNode;

			if (childItems.ChildItemsList == null)
				childItems.ChildItemsList = new List<IdentifiedExtensionType>();

			return childItems;
		}
	}
}
