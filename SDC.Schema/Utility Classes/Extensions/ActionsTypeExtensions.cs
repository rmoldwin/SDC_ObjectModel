

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class ActionsTypeExtensions
	{
		public static ActActionType AddActAction(this ActionsType at, int insertPosition = -1)
		{
			return new ActActionType(at, insertPosition);
		}
		public static RuleSelectMatchingListItemsType AddActSelectMatchingListItems(this ActionsType at, int insertPosition = -1)
		{
			return new RuleSelectMatchingListItemsType(at, insertPosition);
			//return at.AddAction(new (at), insertPosition);
		}
		public static ActAddCodeType AddActAddCode(this ActionsType at, int insertPosition = -1)
		{
			return new ActAddCodeType(at, insertPosition);
			//return at.AddAction(new (at), insertPosition);
		}
		public static ActInjectType AddActInject(this ActionsType at, string id = "", int insertPosition = -1)
		{
			return new ActInjectType(at, id, insertPosition);
			//return at.AddAction(new ActInjectType(at), insertPosition);
		}
		public static ActSaveResponsesType AddActSaveResponses(this ActionsType at, int insertPosition = -1)
		{
			return new ActSaveResponsesType(at, insertPosition);
		}
		public static ActSendReportType AddActSendReport(this ActionsType at, int insertPosition = -1)
		{
			return new ActSendReportType(at, insertPosition);
		}
		public static ActSendMessageType AddActSendMessage(this ActionsType at, int insertPosition = -1)
		{
			return new ActSendMessageType(at, insertPosition);
		}
		public static ActSetAttributeType AddActSetAttributeValue(this ActionsType at, int insertPosition = -1)
		{
			return new ActSetAttributeType(at, insertPosition);
		}
		public static ActSetAttrValueScriptType AddActSetAttributeValueScript(this ActionsType at, int insertPosition = -1)
		{
			return new ActSetAttrValueScriptType(at, insertPosition);
		}
		public static ActSetBoolAttributeValueCodeType AddActSetBoolAttributeValueCode(this ActionsType at, int insertPosition = -1)
		{
			return new (at, insertPosition);
		}
		public static ActShowFormType AddActShowForm(this ActionsType at, int insertPosition = -1)
		{
			return new (at, insertPosition);
		}
		public static ActShowMessageType AddActShowMessage(this ActionsType at, int insertPosition = -1)
		{
			return new ActShowMessageType(at, insertPosition);
		}
		public static ActShowReportType AddActShowReport(this ActionsType at, int insertPosition = -1)
		{
			return new ActShowReportType(at, insertPosition);
		}
		public static ActPreviewReportType AddActPreviewReport(this ActionsType at, int insertPosition = -1)
		{
			return new ActPreviewReportType(at, insertPosition);
		}
		public static ActValidateFormType AddActValidateForm(this ActionsType at, int insertPosition = -1)
		{
			return new ActValidateFormType(at, insertPosition);
		}
		public static ScriptCodeAnyType AddActRunCode(this ActionsType at, int insertPosition = -1)
		{
			return new ScriptCodeAnyType(at, insertPosition, "RunCode");
		}
		public static PredActionType AddActConditionalGroup(this ActionsType at, int insertPosition = -1)
		{
			return new PredActionType(at, insertPosition, "ConditionalGroupAction");
		}

	}
}
