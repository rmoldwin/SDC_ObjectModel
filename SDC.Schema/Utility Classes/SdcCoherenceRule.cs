// SDC-CUSTOM: do not overwrite
using System.Collections.ObjectModel;

namespace SDC.Schema
{
	public interface ISdcCoherenceRule
	{
		string RuleId { get; }
		IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode);
	}

	public static class SdcCoherenceRuleRegistry
	{
		private static readonly object SyncRoot = new();
		private static readonly Dictionary<string, ISdcCoherenceRule> Rules = new(StringComparer.OrdinalIgnoreCase);

		static SdcCoherenceRuleRegistry() => ResetToBuiltIns();

		internal static IReadOnlyList<ISdcCoherenceRule> ActiveRules
		{
			get
			{
				lock (SyncRoot)
				{
					return new ReadOnlyCollection<ISdcCoherenceRule>(Rules.Values.ToList());
				}
			}
		}

		public static void Register(ISdcCoherenceRule rule)
		{
			ArgumentNullException.ThrowIfNull(rule);
			if (string.IsNullOrWhiteSpace(rule.RuleId))
				throw new ArgumentException("RuleId must be a non-empty string.", nameof(rule));

			lock (SyncRoot)
			{
				Rules[rule.RuleId] = rule;
			}
		}

		public static void Unregister(string ruleId)
		{
			if (string.IsNullOrWhiteSpace(ruleId)) return;

			lock (SyncRoot)
			{
				Rules.Remove(ruleId);
			}
		}

		internal static void ResetToBuiltIns()
		{
			lock (SyncRoot)
			{
				Rules.Clear();
				Rules["SDAC"] = new SdacRule();
				Rules["SDS"] = new SdsRule();
			}
		}
	}
}
