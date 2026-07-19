using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules.Mutation;

/// <summary>
/// BP-MUT-001: Flags <see cref="IdentifiedExtensionType"/> nodes whose <c>ID</c> is not
/// unique within their own tree.
///
/// ID-based lookup helpers (e.g. <c>ITopNodeExtensions.GetQuestionByID</c>) resolve a node
/// by scanning <c>ITopNode.Nodes</c> with <c>FirstOrDefault()</c> — if two nodes share an
/// <c>ID</c>, such a lookup silently returns whichever one happens to be enumerated first,
/// not necessarily the one the caller meant. Unlike <c>ObjectGUID</c> (always
/// tree-unique by construction), <c>ID</c> is a plain string supplied by the caller to the
/// <c>Add*</c> builder methods (e.g. <c>AddChildDisplayedItem(id, ...)</c>), so nothing
/// currently stops two different builder calls from being given the same id string — this is
/// exactly the kind of accidental collision an AI-agent-driven construction/mutation session
/// could introduce (e.g. copy/pasting a builder call and forgetting to change the id
/// argument), as opposed to the legitimate, automatically-suffixed duplication produced by
/// <c>Copy()</c>/<c>Move(..., RefreshMode.CloneAndRepeatSubtree)</c> (see
/// guide/02-mutation.md, "Repeating a section or question").
///
/// Uses only the public API: <see cref="IdentifiedExtensionType.ID"/> and
/// <see cref="ITopNode.Nodes"/> for enumeration.
/// </summary>
public sealed class DuplicateIdRule : IQaRule
{
    public string RuleId => "BP-MUT-001";
    public string Description => "Every IdentifiedExtensionType node's ID must be unique within its tree.";
    public QaConvention Convention => QaConvention.Generic;

    public IEnumerable<QaFinding> Evaluate(ITopNode topNode)
    {
        var findings = new List<QaFinding>();
        var byId = new Dictionary<string, List<IdentifiedExtensionType>>(StringComparer.Ordinal);

        foreach (var kvp in topNode.Nodes)
        {
            if (kvp.Value is not IdentifiedExtensionType iet) continue;
            if (string.IsNullOrEmpty(iet.ID)) continue;

            if (!byId.TryGetValue(iet.ID, out var list))
            {
                list = new List<IdentifiedExtensionType>();
                byId[iet.ID] = list;
            }
            list.Add(iet);
        }

        foreach (var group in byId.Values)
        {
            if (group.Count <= 1) continue;

            foreach (var node in group)
            {
                findings.Add(new QaFinding(
                    RuleId,
                    Convention,
                    QaSeverity.Error,
                    NodeId: node.ID,
                    NodeObjectGuid: node.ObjectGUID,
                    Message: $"ID '{node.ID}' is shared by {group.Count} nodes in this tree. ID-based lookup helpers use FirstOrDefault() and will silently resolve to whichever node happens to come first, not necessarily this one.",
                    Recommendation: "Assign each node a distinct id argument when calling the Add* builder methods. If this duplication came from deliberately repeating a section/question, use Copy()/Move(..., RefreshMode.CloneAndRepeatSubtree) instead of hand-duplicating the id — it auto-suffixes both ID and name with \"__N\" so they stay unique.",
                    IsAutoFixable: false));
            }
        }

        return findings;
    }

    // Resolving a duplicate ID means choosing which node keeps the original ID and what the
    // other one's new ID should be — that is a business decision this rule cannot safely
    // guess, so there is no automatic fix.
    public bool TryFix(ITopNode topNode, QaFinding finding) => false;
}
