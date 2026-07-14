// SDC-CUSTOM: do not overwrite

namespace SDC.Schema
{
	internal sealed class SdacRule : ISdcCoherenceRule
	{
		public string RuleId => "SDAC";

		public IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode)
		{
			foreach (var listItem in SdcCoherenceRuleHelpers.GetSelectedListItems(topNode, li => li.selectionDisablesChildren))
			{
				var affectedNodes = SdcCoherenceRuleHelpers.GetNodesWithUserEnteredData(
					SdcCoherenceRuleHelpers.GetDescendants(listItem));

				if (affectedNodes.Count == 0)
					continue;

				yield return SdcCoherenceRuleHelpers.CreateListItemIssue(
					listItem,
					RuleId,
					$"{SdcCoherenceRuleHelpers.DescribeListItem(listItem)} is selected with selectionDisablesChildren=true and will deactivate {affectedNodes.Count} descendant node(s) that currently hold user-entered data. That data is at risk; the client should consider allowing undo before it is discarded.");
			}
		}
	}
}
