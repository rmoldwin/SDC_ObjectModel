// SDC-CUSTOM: do not overwrite
using SDC.Schema.Extensions;
using System.Reflection;

namespace SDC.Schema
{
	internal static class SdcCoherenceRuleHelpers
	{
		private const string SelectedPropertyName = nameof(ListItemBaseType.selected);

		public static IEnumerable<ListItemBaseType> GetSelectedListItems(ITopNode topNode, Func<ListItemBaseType, bool> predicate)
			=> topNode.Nodes.Values
				.OfType<ListItemBaseType>()
				.Where(li => li.selected && predicate(li));

		public static IReadOnlyList<BaseType> GetDescendants(ListItemBaseType listItem)
			=> listItem.GetSubtreeList()?.Skip(1).ToList() ?? [];

		public static IEnumerable<ListItemBaseType> GetSiblingListItems(ListItemBaseType listItem)
			=> listItem.ParentNode?.GetChildNodes()?.OfType<ListItemBaseType>().Where(sibling => !ReferenceEquals(sibling, listItem))
				?? Enumerable.Empty<ListItemBaseType>();

		public static IReadOnlyList<BaseType> GetNodesWithUserEnteredData(IEnumerable<BaseType> nodes)
			=> nodes.Where(HasUserEnteredData).ToList();

		public static bool HasUserEnteredData(BaseType node)
		{
			if (node is ListItemBaseType listItem && listItem.selected)
				return true;

			return HasSerializedVal(node);
		}

		public static SdcNodeValidationIssue CreateListItemIssue(
			ListItemBaseType sourceListItem,
			string ruleCode,
			string message)
			=> new()
			{
				NodeID = sourceListItem.sGuid,
				NodeType = sourceListItem.GetType().Name,
				PropertyName = SelectedPropertyName,
				AttemptedValue = sourceListItem.selected,
				RuleCode = ruleCode,
				Message = message,
				Severity = SdcValidationSeverity.Warning
			};

		public static string DescribeListItem(ListItemBaseType listItem)
		{
			var id = listItem.ID;
			var title = listItem.title;

			if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(title))
				return $"ListItem '{id}' ({title})";

			if (!string.IsNullOrWhiteSpace(id))
				return $"ListItem '{id}'";

			if (!string.IsNullOrWhiteSpace(title))
				return $"ListItem '{title}'";

			return $"{listItem.GetType().Name} '{listItem.sGuid}'";
		}

		private static bool HasSerializedVal(BaseType node)
		{
			var nodeType = node.GetType();
			var valProperty = nodeType.GetProperty("val", BindingFlags.Instance | BindingFlags.Public);
			if (valProperty is null || !valProperty.CanRead || valProperty.GetIndexParameters().Length > 0)
				return false;

			var shouldSerializeVal = nodeType.GetMethod("ShouldSerializeval", BindingFlags.Instance | BindingFlags.Public);
			if (shouldSerializeVal is not null && shouldSerializeVal.ReturnType == typeof(bool))
			{
				try
				{
					return (bool)shouldSerializeVal.Invoke(node, null)!;
				}
				catch
				{
					// Fall back to direct value inspection if a custom ShouldSerializeval throws unexpectedly.
				}
			}

			var value = valProperty.GetValue(node);
			return value switch
			{
				null => false,
				string s => !string.IsNullOrWhiteSpace(s),
				Array a => a.Length > 0,
				_ => true
			};
		}
	}
}
