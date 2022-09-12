

//using SDC;
namespace SDC.Schema
{
	public static class ActionsTypeExtensions
	{
		public static ActActionType AddActAction(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActActionType(at), insertPosition);
		}
		public static RuleSelectMatchingListItemsType AddActSelectMatchingListItems(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new RuleSelectMatchingListItemsType(at), insertPosition);
		}
		//public abstract ActSetPropertyType AddSetProperty(ActionsType at);
		public static ActAddCodeType AddActAddCode(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActAddCodeType(at), insertPosition);
		}
		//public abstract ActSetValueType AddSetValue(ActionsType at);
		public static ActInjectType AddActInject(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActInjectType(at), insertPosition);
		}
		//public static CallFuncActionType AddActShowURL(this ActionsType at, int insertPosition = -1)
		//{
		//    return AddAction(at, new CallFuncActionType(at), insertPosition);
		//}
		public static ActSaveResponsesType AddActSaveResponses(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActSaveResponsesType(at), insertPosition);
		}
		public static ActSendReportType AddActSendReport(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActSendReportType(at), insertPosition);
		}
		public static ActSendMessageType AddActSendMessage(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActSendMessageType(at), insertPosition);
		}
		public static ActSetAttributeType AddActSetAttributeValue(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActSetAttributeType(at), insertPosition);
		}
		public static ActSetAttrValueScriptType AddActSetAttributeValueScript(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActSetAttrValueScriptType(at), insertPosition);
		}
		public static ActSetBoolAttributeValueCodeType AddActSetBoolAttributeValueCode(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActSetBoolAttributeValueCodeType(at), insertPosition);
		}
		public static ActShowFormType AddActShowForm(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActShowFormType(at), insertPosition);
		}
		public static ActShowMessageType AddActShowMessage(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActShowMessageType(at), insertPosition);
		}
		public static ActShowReportType AddActShowReport(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActShowReportType(at), insertPosition);
		}
		public static ActPreviewReportType AddActPreviewReport(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActPreviewReportType(at), insertPosition);
		}
		public static ActValidateFormType AddActValidateForm(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ActValidateFormType(at), insertPosition);
		}
		public static ScriptCodeAnyType AddActRunCode(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new ScriptCodeAnyType(at), insertPosition);
		}
		//public static CallFuncActionType AddActCallFunction(this ActionsType at, int insertPosition = -1)
		//{
		//    return AddAction(at, new CallFuncActionType(at), insertPosition);
		//}
		public static PredActionType AddActConditionalGroup(this ActionsType at, int insertPosition = -1)
		{
			return at.AddAction(new PredActionType(at), insertPosition);
		}

		private static T AddAction<T>(this ActionsType at, T action, int insertPosition = -1) where T : ExtensionBaseType
		{
			var p = at;
			var lst = (IList<BaseType>)p.Items;
			int c = lst.Count;
			if (insertPosition > -1 && insertPosition < c) lst.Insert(insertPosition, action);
			else lst.Insert(c, action);
			return action;
		}
	}
}
