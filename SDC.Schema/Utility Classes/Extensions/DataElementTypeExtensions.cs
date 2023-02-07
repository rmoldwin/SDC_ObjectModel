using SDC.Schema.Extensions;
using System;
using System.Linq;

namespace SDC.Schema.UtilityClasses.Extensions
{
	public static class DataElementTypeExtensions
	{
		//See IChildItemsParentExtensions for similar methods


		/// <summary>
		/// Note: If the caller is creating a QuestionResponse (QF/QR), it will be created with default values and a string data type.<br/>
		/// Use AddQuestionResponse for more control over QR creation.
		/// </summary>
		/// <param name="de"></param>
		/// <param name="qType"></param>
		/// <param name="id"></param>
		/// <param name="title"></param>
		/// <param name="insertPosition"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		public static QuestionItemType AddQuestion(this DataElementType de, QuestionEnum qType, string id, string title = null, int insertPosition = -1)
		//where T : BaseType, IChildItemsParent<T>
		{
			de.DataElement_Items ??= new();
			var deList = de.DataElement_Items;
			var qNew = new QuestionItemType(de, id);
			//ListFieldType lf;
			var count = deList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			deList.Insert(insertPosition, qNew);

			switch (qType)
			{
				case QuestionEnum.QuestionSingle:
					qNew.GetListField().GetList();
					break;
				case QuestionEnum.QuestionMultiple:
					qNew.GetListField().GetList();
					qNew.ListField_Item.maxSelections = 0;
					break;
				case QuestionEnum.QuestionFill:
					qNew.AddQuestionResponseField(out DataTypes_DEType dtDE);
					break;
				case QuestionEnum.QuestionLookupSingle:
					qNew.GetListField().AddEndpoint();
					break;
				case QuestionEnum.QuestionLookupMultiple:
					qNew.GetListField().AddEndpoint();

					break;
				default:
					throw new NotSupportedException($"{qType} is not supported");
			}
			qNew.title = title;
			return qNew;
		}

		public static QuestionItemType AddQuestionResponse(this DataElementType de,
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
			de.DataElement_Items ??= new();
			var deList = de.DataElement_Items;

			var qNew = new QuestionItemType(de, id);
			qNew.title = defTitle;

			var count = deList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			deList.Insert(insertPosition, qNew);
			var rf = qNew.AddQuestionResponseField(out deType, dt, dtQuant, valDefault);
			rf.AddResponseUnits(units);
			rf.AddTextAfterResponse(textAfterResponse);

			return qNew;

		}

		public static SectionItemType AddSection(this DataElementType de, string id, string? defTitle = null, int insertPosition = -1)
		{
			de.DataElement_Items ??= new();
			var deList = de.DataElement_Items;
			var sNew = new SectionItemType(de, id);
			sNew.title = defTitle;
			var count = deList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			deList.Insert(insertPosition, sNew);

			return sNew;
		}

		public static DisplayedType AddDisplayedItem(this DataElementType de, string id, string defTitle = null, int insertPosition = -1)
		//where T : BaseType, IChildItemsParent<T>
		{
			de.DataElement_Items ??= new();
			var deList = de.DataElement_Items;
			var dNew = new DisplayedType(de, id);
			dNew.title = defTitle;
			var count = deList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			deList.Insert(insertPosition, dNew);

			return dNew;
		}

		public static ButtonItemType AddButtonAction(this DataElementType de, string id, string defTitle = null, int insertPosition = -1)
		//where T : BaseType, IChildItemsParent<T>
		{
			de.DataElement_Items ??= new();
			var deList = de.DataElement_Items;
			var btnNew = new ButtonItemType(de, id);
			btnNew.title = defTitle;
			var count = deList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			deList.Insert(insertPosition, btnNew);

			// TODO: Add AddButtonActionTypeItems(btnNew);
			return btnNew;
		}

		public static InjectFormType AddInjectedForm(this DataElementType de, string id, int insertPosition = -1)
		//where T : BaseType, IChildItemsParent<T>
		{
			de.DataElement_Items ??= new();
			var deList = de.DataElement_Items;
			var injForm = new InjectFormType(de, id);
			var count = deList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			deList.Insert(insertPosition, injForm);
			//TODO: init this InjectForm object

			return injForm;
		}

	}
}
