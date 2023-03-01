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
		public static QuestionItemType AddQuestion(this DataElementType de, QuestionEnum qType, string id, string title = "", int insertPosition = -1)
		{
			var qNew = new QuestionItemType(de, id, insertPosition);			

			switch (qType)
			{
				case QuestionEnum.QuestionRaw:
					break;
				case QuestionEnum.QuestionSingle:
					qNew.GetListField().GetList(); //creates ListField and List
					break;
				case QuestionEnum.QuestionMultiple:
					//qNew.GetListField().GetList();
					qNew.GetListField().maxSelections = 0; //creates ListField and List
					break;
				case QuestionEnum.QuestionFill:
					qNew.AddQuestionResponseField(out DataTypes_DEType dtDE); //Creates Response node
					break;
				case QuestionEnum.QuestionLookupSingle:
					qNew.GetListField().GetLookupEndpoint(); //creates ListField and Lookup node
					break;
				case QuestionEnum.QuestionLookupMultiple: //creates ListField and Lookup node
					qNew.GetListField().GetLookupEndpoint();
					qNew.GetListField().maxSelections = 0;
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
			string defTitle = "",
			int insertPosition = -1,
			ItemChoiceType dt = ItemChoiceType.@string,
			string textAfterResponse = "",
			string units = "",
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null) //where T : BaseType, IChildItemsParent<T>
		{
			var qNew = new QuestionItemType(de, id, insertPosition);
			if(!defTitle.IsNullOrWhitespace()) qNew.title = defTitle;

			var rf = qNew.AddQuestionResponseField(out deType, dt, dtQuant, valDefault);
			if (!units.IsNullOrWhitespace()) rf.AddResponseUnits(units);
			if (!textAfterResponse.IsNullOrWhitespace()) rf.AddTextAfterResponse(textAfterResponse);

			return qNew;
		}

		public static SectionItemType AddSection(this DataElementType de, string id, string defTitle = "", int insertPosition = -1)
		{
			var sNew = new SectionItemType(de, id, insertPosition);
			if (!defTitle.IsNullOrWhitespace()) sNew.title = defTitle;
			
			return sNew;
		}

		public static DisplayedType AddDisplayedItem(this DataElementType de, string id, string defTitle = "", int insertPosition = -1)
		{
			var dNew = new DisplayedType(de, id, insertPosition);
			if (!defTitle.IsNullOrWhitespace()) dNew.title = defTitle;
			
			return dNew;
		}

		public static ButtonItemType AddButtonAction(this DataElementType de, string id, string defTitle = "", int insertPosition = -1)
		{
			var btnNew = new ButtonItemType(de, id, insertPosition);
			if (!defTitle.IsNullOrWhitespace()) btnNew.title = defTitle;

			// TODO: Add AddButtonActionTypeItems(btnNew);
			return btnNew;
		}

		public static InjectFormType AddInjectedForm(this DataElementType de, string id, int insertPosition = -1)
		{
			var injForm = new InjectFormType(de, id, insertPosition);

			//TODO: init this InjectForm object
			return injForm;
		}

	}
}
