using SDC.Schema.QA.Reporting;
using SDC.Schema.QA.Rules.AdHocAttributes;
using SDC.Schema.QA.Rules.Construction;
using SDC.Schema.QA.Rules.Mutation;
using SDC.Schema.QA.Rules.Serialization;
using SDC.Schema.QA.Rules.Validation;

namespace SDC.Schema.QA.Rules;

/// <summary>
/// Runs a set of <see cref="IQaRule"/>s against a hydrated SDC OM tree and
/// produces an aggregated <see cref="QaReport"/>. Also supports an
/// "auto-fix and re-check" loop for rules that declare fixes.
/// </summary>
public sealed class QaEngine
{
    private readonly List<IQaRule> _rules;

    public QaEngine()
        : this(CreateDefaultRules())
    {
    }

    public QaEngine(IEnumerable<IQaRule> rules)
    {
        _rules = rules.ToList();
    }

    public IReadOnlyList<IQaRule> Rules => _rules;

    public static IReadOnlyList<IQaRule> CreateDefaultRules() =>
    [
        new TreeIntegrityRule(),
        new DuplicateIdRule(),
        new UnresolvedRejectedValuesRule(),
        new CoherenceValidationBridgeRule(),
        new EmptyExtensionRule(),
        new NoInternalStateInJsonRule()
    ];

    /// <summary>Runs every registered rule against the tree and returns all findings.</summary>
    public QaReport Run(ITopNode topNode, string? subjectDescription = null)
    {
        var findings = new List<QaFinding>();
        foreach (var rule in _rules)
        {
            findings.AddRange(rule.Evaluate(topNode));
        }
        return new QaReport(findings, subjectDescription);
    }

    /// <summary>
    /// Runs all rules, attempts auto-fix on every auto-fixable finding, then
    /// re-runs all rules to confirm the fixes took effect and didn't
    /// introduce regressions. Returns the final (post-fix) report.
    /// </summary>
    public QaReport RunAndAutoFix(ITopNode topNode, string? subjectDescription = null, int maxPasses = 3)
    {
        QaReport report = Run(topNode, subjectDescription);
        for (int pass = 0; pass < maxPasses; pass++)
        {
            var fixable = report.Findings.Where(f => f.IsAutoFixable).ToList();
            if (fixable.Count == 0) break;

            bool anyFixed = false;
            foreach (var finding in fixable)
            {
                var rule = _rules.FirstOrDefault(r => r.RuleId == finding.RuleId);
                if (rule is not null && rule.TryFix(topNode, finding))
                    anyFixed = true;
            }
            if (!anyFixed) break;
            report = Run(topNode, subjectDescription);
        }
        return report;
    }
}
