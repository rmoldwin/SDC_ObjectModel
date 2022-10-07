namespace SDC.Schema
{
	[Flags]
    public enum QuestionEnum
    {
        QuestionRaw = ItemTypeEnum.QuestionRaw,
        QuestionSingle = ItemTypeEnum.QuestionSingle,
        QuestionMultiple = ItemTypeEnum.QuestionMultiple,
        QuestionSingleOrMultiple = ItemTypeEnum.QuestionSingleOrMultiple,
        QuestionFill = ItemTypeEnum.QuestionResponse,
        QuestionLookup = ItemTypeEnum.QuestionLookup, //generic QR
        QuestionLookupSingle = ItemTypeEnum.QuestionLookupSingle,
        QuestionLookupMultiple = ItemTypeEnum.QuestionLookupMultiple,
        QuestionGroup = ItemTypeEnum.QuestionGroup //generic Q
    }

}