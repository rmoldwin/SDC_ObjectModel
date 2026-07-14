// SDC-CUSTOM: do not overwrite

namespace SDC.Schema
{
	internal sealed class SdsRule : ISdcCoherenceRule
	{
		public string RuleId => "SDS";

		public IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode)
		{
			foreach (var triggeringListItem in SdcCoherenceRuleHelpers.GetSelectedListItems(topNode, li => li.selectionDeselectsSiblings))
			{
				foreach (var sibling in SdcCoherenceRuleHelpers.GetSiblingListItems(triggeringListItem))
				{
					var siblingNodes = new[] { sibling }.Concat(SdcCoherenceRuleHelpers.GetDescendants(sibling));
					var affectedNodes = SdcCoherenceRuleHelpers.GetNodesWithUserEnteredData(siblingNodes);

					if (affectedNodes.Count == 0)
						continue;

					yield return SdcCoherenceRuleHelpers.CreateListItemIssue(
						triggeringListItem,
						RuleId,
						$"{SdcCoherenceRuleHelpers.DescribeListItem(triggeringListItem)} is selected with selectionDeselectsSiblings=true and will auto-deselect sibling {SdcCoherenceRuleHelpers.DescribeListItem(sibling)}, which currently contains {affectedNodes.Count} node(s) with user-entered data at risk. The client should consider allowing undo before that sibling's data is discarded.");
				}
			}
		}
	}
}
