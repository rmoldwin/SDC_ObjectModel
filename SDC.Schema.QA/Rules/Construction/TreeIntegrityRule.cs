using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules.Construction;

/// <summary>
/// BP-GEN-001: Verifies basic parent/child tree integrity using only the
/// public API surface (<see cref="ITopNode.Nodes"/> and
/// <see cref="IBaseType.ParentNode"/>) — the same integrity check any
/// external consumer should run after a batch of programmatic construction
/// or mutation, without needing access to SDC.Schema's internal dictionaries.
///
/// This is intentionally a "public API" reflection of the internal
/// TreeValidationHelper used by SDC.Schema.Tests: it can't see everything
/// TreeValidationHelper can (e.g. _ParentNodes/_ChildNodes dictionaries are
/// internal), but it catches the most common and highest-impact integrity
/// break an agent or developer could introduce: a node present in the tree's
/// Nodes dictionary whose ParentNode is null or whose ParentNode is not
/// itself part of the same tree.
/// </summary>
public sealed class TreeIntegrityRule : IQaRule
{
    public string RuleId => "BP-GEN-001";
    public string Description => "Every non-root node in the tree must have a ParentNode that is itself a member of the same tree.";
    public QaConvention Convention => QaConvention.Generic;

    public IEnumerable<QaFinding> Evaluate(ITopNode topNode)
    {
        var findings = new List<QaFinding>();
        var rootAsBaseType = topNode as BaseType;

        foreach (var kvp in topNode.Nodes)
        {
            BaseType node = kvp.Value;

            // The root node of the tree is allowed to have no parent.
            if (ReferenceEquals(node, rootAsBaseType))
                continue;

            if (node.ParentNode is null)
            {
                findings.Add(new QaFinding(
                    RuleId,
                    Convention,
                    QaSeverity.Error,
                    NodeId: SafeId(node),
                    NodeObjectGuid: node.ObjectGUID,
                    Message: "Node is registered in the tree's Nodes dictionary but has no ParentNode. It is effectively orphaned but still discoverable — likely left behind by a manual property assignment instead of Move()/RemoveRecursive().",
                    Recommendation: "Re-parent the node via Move(), or fully detach it via RemoveRecursive(cancelIfChildNodes:false).",
                    IsAutoFixable: false));
                continue;
            }

            if (!topNode.Nodes.ContainsKey(node.ParentNode.ObjectGUID))
            {
                findings.Add(new QaFinding(
                    RuleId,
                    Convention,
                    QaSeverity.Error,
                    NodeId: SafeId(node),
                    NodeObjectGuid: node.ObjectGUID,
                    Message: "Node's ParentNode is not itself a member of this tree's Nodes dictionary (cross-tree dangling reference).",
                    Recommendation: "This usually indicates a node was moved cross-tree without RefreshMode.UpdateNodeIdentity, or a partial/interrupted mutation. Re-run Move() with the correct RefreshMode, or rebuild the subtree.",
                    IsAutoFixable: false));
            }
        }

        return findings;
    }

    public bool TryFix(ITopNode topNode, QaFinding finding) => false;

    private static string? SafeId(BaseType node)
    {
        try { return node.BaseName; }
        catch { return null; }
    }
}
