using SDC.Schema.Extensions;
using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules.AdHocAttributes;

/// <summary>
/// BP-ADH-001: Flags `&lt;Extension&gt;` elements (ExtensionType nodes) that carry no
/// ad-hoc attributes, no ad-hoc elements, and no comment/property children — i.e. an
/// inert extension node that adds tree size/noise without adding any actual custom data.
///
/// This exists to catch a real mistake found while building this guide's own example
/// generator: an ExtensionType can be created via AddExtension() and then never actually
/// populated (e.g. because the caller mistakenly tried to add ad-hoc attributes to the
/// *parent* node instead — see guide/04-adhoc-attributes-namespaces.md). The empty
/// &lt;Extension&gt; element that results is harmless to the schema but is dead weight
/// and usually indicates a forgotten follow-up call.
///
/// Uses only the public API: ExtensionType.Any / ExtensionType.AnyAttr (both public
/// properties), and topNode.Nodes for enumeration.
/// </summary>
public sealed class EmptyExtensionRule : IQaRule
{
    public string RuleId => "BP-ADH-001";
    public string Description => "An <Extension> element should not be left with zero ad-hoc attributes and zero ad-hoc elements.";
    public QaConvention Convention => QaConvention.Generic;

    public IEnumerable<QaFinding> Evaluate(ITopNode topNode)
    {
        var findings = new List<QaFinding>();

        foreach (var kvp in topNode.Nodes)
        {
            if (kvp.Value is not ExtensionType ext)
                continue;

            bool hasAnyAttr = ext.AnyAttr is { Count: > 0 };
            bool hasAnyElement = ext.Any is { Count: > 0 };

            if (!hasAnyAttr && !hasAnyElement)
            {
                findings.Add(new QaFinding(
                    RuleId,
                    Convention,
                    QaSeverity.Warning,
                    NodeId: SafeId(ext),
                    NodeObjectGuid: ext.ObjectGUID,
                    Message: "This <Extension> element has no ad-hoc attributes (AnyAttr) and no ad-hoc elements (Any). It is inert.",
                    Recommendation: "Either populate it via AddOrUpdateAdHocAttribute()/AddComment()/AddProperty() on THIS ExtensionType instance (not the parent node — see guide/04-adhoc-attributes-namespaces.md), or remove it with RemoveRecursive() if it was created but never needed.",
                    IsAutoFixable: true));
            }
        }

        return findings;
    }

    public bool TryFix(ITopNode topNode, QaFinding finding)
    {
        if (finding.RuleId != RuleId) return false;
        if (finding.NodeObjectGuid is not Guid guid) return false;
        if (!topNode.Nodes.TryGetValue(guid, out var node)) return false;
        if (node is not ExtensionType ext) return false;

        // Safe auto-fix: an inert extension carries no data, so removing it cannot lose
        // any information. Re-confirm inertness at fix time in case the tree changed
        // between Evaluate() and TryFix().
        bool stillEmpty = (ext.AnyAttr is null || ext.AnyAttr.Count == 0)
                        && (ext.Any is null || ext.Any.Count == 0);
        if (!stillEmpty) return false;

        return ext.RemoveRecursive(cancelIfChildNodes: false);
    }

    private static string SafeId(BaseType node)
    {
        try { return node.BaseName; } catch { return string.Empty; }
    }
}
