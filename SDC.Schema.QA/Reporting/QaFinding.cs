namespace SDC.Schema.QA.Reporting;

/// <summary>
/// A single QA/QC finding produced by an <see cref="Rules.IQaRule"/> against one
/// node (or the tree as a whole) of a hydrated SDC OM.
/// </summary>
/// <param name="RuleId">Stable, short identifier for the rule that produced this finding (e.g. "BP-MUT-001").</param>
/// <param name="Convention">Whether this rule is a generic SDC best practice or a CAP-specific convention.</param>
/// <param name="Severity">How serious the finding is.</param>
/// <param name="NodeId">The SDC ID of the offending node, if applicable, for human navigation.</param>
/// <param name="NodeObjectGuid">The ObjectGUID of the offending node, if applicable, for programmatic navigation.</param>
/// <param name="Message">Human-readable description of the issue.</param>
/// <param name="Recommendation">What to do about it (a fix suggestion, or "auto-fixable" pointer).</param>
/// <param name="IsAutoFixable">True if <see cref="Rules.IQaRule.TryFix"/> can resolve this finding automatically.</param>
public sealed record QaFinding(
    string RuleId,
    QaConvention Convention,
    QaSeverity Severity,
    string? NodeId,
    Guid? NodeObjectGuid,
    string Message,
    string? Recommendation = null,
    bool IsAutoFixable = false);
