

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class IHasActionsNodeExtensions
	{
		public static ActionsType AddActionsNode(this IHasActionsNode han)
		{
			var actions = new ActionsType((ExtensionBaseType)han);
			if (han is PredActionType p)
			{
				p.Actions = actions;
				return p.Actions;
			}
			else
			{
				if (han is EventType pe)
				{
					pe.Actions = actions;
					return pe.Actions;
				}
			}
			throw new InvalidCastException("The parent node must be of type EventType or PredActionType");
		}
	}
}
