using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules.Validation;

/// <summary>
/// BP-VAL-001: Flags nodes with one or more unresolved entries in
/// <see cref="BaseType.RejectedValues"/>.
///
/// Under SDC.Schema's soft-reject validation contract (guide/05-validation-coherence.md), a
/// property setter never stores an invalid value and never throws — instead the offending
/// value is recorded out-of-band in <c>RejectedValues</c>, keyed by property name, so it can
/// be surfaced to a user/agent for correction. That contract only does half the job if the
/// rejection is then never looked at: the backing property silently keeps its prior (possibly
/// stale/default) value while the caller's real intent sits unresolved in
/// <c>RejectedValues</c>, invisible to anything that isn't specifically checking for it. This
/// rule is exactly that check, generalized across an entire tree, so an AI-agent-driven
/// mutation pass can catch "I tried to set something and it got silently dropped" before
/// handing the tree back to a human or persisting it — reinforcing the guide's own
/// recommendation to prefer <c>WouldBeValid(...)</c> pre-checks over set-then-inspect.
///
/// Uses only the public API: <see cref="BaseType.HasRejectedValues"/> and
/// <see cref="BaseType.RejectedValues"/> (both public, non-internal members), and
/// <see cref="ITopNode.Nodes"/> for enumeration.
/// </summary>
public sealed class UnresolvedRejectedValuesRule : IQaRule
{
    public string RuleId => "BP-VAL-001";
    public string Description => "A node must not be left with unresolved RejectedValues entries from the soft-reject validation contract.";
    public QaConvention Convention => QaConvention.Generic;

    public IEnumerable<QaFinding> Evaluate(ITopNode topNode)
    {
        var findings = new List<QaFinding>();

        foreach (var kvp in topNode.Nodes)
        {
            BaseType node = kvp.Value;
            if (!node.HasRejectedValues) continue;

            foreach (var rejected in node.RejectedValues.Values)
            {
                findings.Add(new QaFinding(
                    RuleId,
                    Convention,
                    QaSeverity.Warning,
                    NodeId: SafeId(node),
                    NodeObjectGuid: node.ObjectGUID,
                    Message: $"Property '{rejected.PropertyName}' has an unresolved rejected value ({rejected.AttemptedValue ?? "null"}): {rejected.Message}",
                    Recommendation: "Call WouldBeValid(...) before attempting to set a value from an untrusted/AI-derived source rather than set-then-inspect. Once the situation is addressed (either a corrected value is set, or the rejection is intentionally acknowledged), call ClearRejectedValue(propertyName)/ClearRejectedValues() so it stops being flagged. See guide/05-validation-coherence.md.",
                    IsAutoFixable: false));
            }
        }

        return findings;
    }

    // Clearing a rejected value without first supplying (or deliberately accepting the
    // absence of) a corrected replacement would just hide the problem, not fix it — that
    // judgement call belongs to the caller, so there is no automatic fix.
    public bool TryFix(ITopNode topNode, QaFinding finding) => false;

    private static string? SafeId(BaseType node)
    {
        try { return node.BaseName; } catch { return null; }
    }
}
