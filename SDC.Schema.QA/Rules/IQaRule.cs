using SDC.Schema.QA.Reporting;

namespace SDC.Schema.QA.Rules;

/// <summary>
/// A single best-practice check that can be run against a hydrated SDC OM
/// tree (rooted at any <see cref="ITopNode"/>). Implementations must use only
/// the public API surface of SDC.Schema — the same surface available to any
/// external consumer of the compiled DLL — so that the checks (and any
/// auto-fixes) double as executable documentation of best practice.
/// </summary>
public interface IQaRule
{
    /// <summary>Stable short identifier, e.g. "BP-MUT-001". Referenced from the best-practices guide.</summary>
    string RuleId { get; }

    /// <summary>One-line description of what this rule checks.</summary>
    string Description { get; }

    /// <summary>Whether this is a generic SDC best practice or a CAP-specific convention.</summary>
    QaConvention Convention { get; }

    /// <summary>
    /// Evaluate the rule against the given tree (identified by its TopNode) and
    /// yield zero or more findings. Must not mutate the tree.
    /// </summary>
    IEnumerable<QaFinding> Evaluate(ITopNode topNode);

    /// <summary>
    /// Attempt to automatically resolve a finding previously produced by
    /// <see cref="Evaluate"/>. Returns false if this rule has no automatic fix,
    /// or if the fix could not be safely applied to the current tree state.
    /// Implementations must use only the public mutation API (e.g. Move,
    /// RemoveRecursive, the Add* builder extensions) — never reflection into
    /// internal state.
    /// </summary>
    bool TryFix(ITopNode topNode, QaFinding finding);
}
