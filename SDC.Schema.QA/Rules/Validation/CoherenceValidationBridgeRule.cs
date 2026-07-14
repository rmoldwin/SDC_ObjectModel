using SDC.Schema.Extensions;
using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules.Validation;

/// <summary>
/// BP-VAL-002: Bridges public <see cref="SdcValidate.ValidateTree(ITopNode, bool)"/> coherence
/// findings into the QA reporting pipeline, so built-in SDAC/SDS warnings and any future
/// custom <see cref="ISdcCoherenceRule"/> outputs appear in <see cref="QaReport"/> without
/// duplicating the underlying detection logic here.
/// </summary>
public sealed class CoherenceValidationBridgeRule : IQaRule
{
    public string RuleId => "BP-VAL-002";
    public string Description => "Any coherence-rule finding emitted by ValidateTree() should also surface in the QA report.";
    public QaConvention Convention => QaConvention.Generic;

    public IEnumerable<QaFinding> Evaluate(ITopNode topNode)
    {
        foreach (var issue in topNode.ValidateTree().Issues.Where(i => !string.IsNullOrWhiteSpace(i.RuleCode)))
        {
            yield return new QaFinding(
                RuleId,
                Convention,
                MapSeverity(issue.Severity),
                NodeId: issue.NodeID,
                NodeObjectGuid: TryResolveNodeObjectGuid(topNode, issue.NodeID),
                Message: issue.Message,
                Recommendation: BuildRecommendation(issue),
                IsAutoFixable: false);
        }
    }

    public bool TryFix(ITopNode topNode, QaFinding finding) => false;

    private static QaSeverity MapSeverity(SdcValidationSeverity severity) => severity switch
    {
        SdcValidationSeverity.Info => QaSeverity.Info,
        SdcValidationSeverity.Warning => QaSeverity.Warning,
        SdcValidationSeverity.Error => QaSeverity.Error,
        _ => QaSeverity.Warning
    };

    private static Guid? TryResolveNodeObjectGuid(ITopNode topNode, string? shortGuid)
    {
        if (string.IsNullOrWhiteSpace(shortGuid))
            return null;

        return topNode.TryGetNodeByShortGuid(shortGuid, out var node)
            ? node?.ObjectGUID
            : null;
    }

    private static string BuildRecommendation(SdcNodeValidationIssue issue)
    {
        var focus = string.IsNullOrWhiteSpace(issue.PropertyName)
            ? "Review the affected node state and decide whether to undo or correct it before persisting the tree."
            : $"Review the node's '{issue.PropertyName}' state and decide whether to undo or correct it before persisting the tree.";

        return $"Originated from SDC.Schema.ValidateTree() coherence rule '{issue.RuleCode}' on node type '{issue.NodeType}'. {focus}";
    }
}
