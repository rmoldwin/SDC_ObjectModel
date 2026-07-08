using SDC.Schema.Extensions;
using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules.Serialization;

/// <summary>
/// BP-SER-001: Detects the "internal implementation state leaking into JSON" finding
/// documented in guide/03-serialization-roundtrip.md — the tree's internal
/// ReaderWriterLockSlim (TreeRwLock, used only by the opt-in TreeLockScope thread-safety
/// mechanism) is currently serialized into GetJson() output. This bloats payloads with
/// meaningless post-round-trip lock state and is a minor internal-detail leak.
///
/// This rule does not attempt to inspect internal state directly (that would violate the
/// public-API-only constraint); instead it does exactly what an external consumer could
/// do — call the public GetJson() extension method and text-search the result for the
/// known offending property name. This is intentionally a blunt, string-based check: it
/// will need updating if/when the underlying property is renamed or [JsonIgnore]'d (at
/// which point this rule should stop firing and can eventually be retired).
/// </summary>
public sealed class NoInternalStateInJsonRule : IQaRule
{
    public string RuleId => "BP-SER-001";
    public string Description => "Serialized JSON output must not contain the internal TreeRwLock (ReaderWriterLockSlim) implementation object.";
    public QaConvention Convention => QaConvention.Generic;

    private const string OffendingPropertyName = "\"TreeRwLock\"";

    public IEnumerable<QaFinding> Evaluate(ITopNode topNode)
    {
        string json;
        try
        {
            json = topNode.GetJson();
        }
        catch
        {
            // If the tree can't be serialized at all, that's a different rule's concern.
            yield break;
        }

        if (json.Contains(OffendingPropertyName, StringComparison.Ordinal))
        {
            yield return new QaFinding(
                RuleId,
                Convention,
                QaSeverity.Warning,
                NodeId: SafeId(topNode as BaseType),
                NodeObjectGuid: (topNode as BaseType)?.ObjectGUID,
                Message: "GetJson() output contains the internal TreeRwLock (ReaderWriterLockSlim) object. This is internal thread-safety implementation state, not tree/form data, and should not appear in a serialized payload.",
                Recommendation: "Known upstream gap — see guide/03-serialization-roundtrip.md and guide/07-known-gaps-and-future-work.md item 4. Suggested SDC.Schema fix: mark the TreeRwLock property [JsonIgnore] (and confirm [XmlIgnore]) so it is excluded from all serialized formats. No safe auto-fix is possible from outside the library (this rule cannot rewrite already-serialized JSON without risking corrupting other content), so IsAutoFixable is false.",
                IsAutoFixable: false);
        }
    }

    public bool TryFix(ITopNode topNode, QaFinding finding) => false;

    private static string? SafeId(BaseType? node)
    {
        if (node is null) return null;
        try { return node.BaseName; } catch { return null; }
    }
}
