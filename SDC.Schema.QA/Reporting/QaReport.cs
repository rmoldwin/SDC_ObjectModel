namespace SDC.Schema.QA.Reporting;

/// <summary>
/// Aggregates <see cref="QaFinding"/>s from one QA/QC run and renders them in
/// both AI-agent-friendly (Markdown) and human-friendly (single-file HTML)
/// formats, per the SDC Best Practices Guide reporting convention.
/// </summary>
public sealed class QaReport
{
    public DateTimeOffset GeneratedAt { get; }
    public string? SubjectDescription { get; }
    public IReadOnlyList<QaFinding> Findings { get; }

    public QaReport(IEnumerable<QaFinding> findings, string? subjectDescription = null)
    {
        Findings = findings.ToList();
        SubjectDescription = subjectDescription;
        GeneratedAt = DateTimeOffset.Now;
    }

    public int ErrorCount => Findings.Count(f => f.Severity == QaSeverity.Error);
    public int WarningCount => Findings.Count(f => f.Severity == QaSeverity.Warning);
    public int InfoCount => Findings.Count(f => f.Severity == QaSeverity.Info);
    public bool IsClean => ErrorCount == 0 && WarningCount == 0;

    /// <summary>Renders a compact, machine- and AI-agent-readable Markdown report.</summary>
    public string ToMarkdown()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# SDC OM QA/QC Report");
        sb.AppendLine();
        if (SubjectDescription is not null)
            sb.AppendLine($"**Subject:** {SubjectDescription}  ");
        sb.AppendLine($"**Generated:** {GeneratedAt:O}  ");
        sb.AppendLine($"**Summary:** {ErrorCount} error(s), {WarningCount} warning(s), {InfoCount} info  ");
        sb.AppendLine();
        if (Findings.Count == 0)
        {
            sb.AppendLine("No findings. Tree conforms to all checked best practices.");
            return sb.ToString();
        }

        foreach (var group in Findings.GroupBy(f => f.Convention).OrderBy(g => g.Key))
        {
            sb.AppendLine($"## {(group.Key == QaConvention.Cap ? "CAP-specific conventions" : "Generic SDC best practices")}");
            sb.AppendLine();
            sb.AppendLine("| Rule | Severity | Node | Message | Recommendation | Auto-fixable |");
            sb.AppendLine("|------|----------|------|---------|-----------------|--------------|");
            foreach (var f in group.OrderByDescending(f => f.Severity))
            {
                sb.AppendLine($"| {f.RuleId} | {f.Severity} | {f.NodeId ?? f.NodeObjectGuid?.ToString() ?? "(tree)"} | {f.Message} | {f.Recommendation ?? ""} | {(f.IsAutoFixable ? "yes" : "no")} |");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>Renders a single self-contained HTML file (inline CSS, no external assets) for human review.</summary>
    public string ToHtml()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>SDC OM QA/QC Report</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:Segoe UI,Arial,sans-serif;margin:2em;color:#1a1a1a;}");
        sb.AppendLine("h1{font-size:1.4em;} h2{font-size:1.15em;margin-top:1.5em;}");
        sb.AppendLine("table{border-collapse:collapse;width:100%;margin-top:0.5em;}");
        sb.AppendLine("th,td{border:1px solid #ccc;padding:6px 8px;text-align:left;font-size:0.92em;vertical-align:top;}");
        sb.AppendLine("th{background:#f2f2f2;}");
        sb.AppendLine(".sev-Error{color:#a30000;font-weight:bold;} .sev-Warning{color:#8a6100;} .sev-Info{color:#345;}");
        sb.AppendLine(".badge{display:inline-block;padding:2px 8px;border-radius:10px;font-size:0.85em;margin-right:6px;}");
        sb.AppendLine(".badge-cap{background:#eef;border:1px solid #99f;} .badge-generic{background:#efe;border:1px solid #9c9;}");
        sb.AppendLine(".summary{background:#fafafa;border:1px solid #ddd;padding:0.75em 1em;border-radius:6px;display:inline-block;}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<h1>SDC OM QA/QC Report</h1>");
        if (SubjectDescription is not null)
            sb.AppendLine($"<p><b>Subject:</b> {System.Net.WebUtility.HtmlEncode(SubjectDescription)}</p>");
        sb.AppendLine($"<p class=\"summary\"><b>Generated:</b> {GeneratedAt:O}<br>" +
                       $"<span class=\"sev-Error\">{ErrorCount} error(s)</span> &nbsp; " +
                       $"<span class=\"sev-Warning\">{WarningCount} warning(s)</span> &nbsp; " +
                       $"<span class=\"sev-Info\">{InfoCount} info</span></p>");

        if (Findings.Count == 0)
        {
            sb.AppendLine("<p>No findings. Tree conforms to all checked best practices.</p>");
        }
        else
        {
            foreach (var group in Findings.GroupBy(f => f.Convention).OrderBy(g => g.Key))
            {
                var badgeClass = group.Key == QaConvention.Cap ? "badge-cap" : "badge-generic";
                var label = group.Key == QaConvention.Cap ? "CAP-specific conventions" : "Generic SDC best practices";
                sb.AppendLine($"<h2><span class=\"badge {badgeClass}\">{label}</span></h2>");
                sb.AppendLine("<table><tr><th>Rule</th><th>Severity</th><th>Node</th><th>Message</th><th>Recommendation</th><th>Auto-fixable</th></tr>");
                foreach (var f in group.OrderByDescending(f => f.Severity))
                {
                    var node = System.Net.WebUtility.HtmlEncode(f.NodeId ?? f.NodeObjectGuid?.ToString() ?? "(tree)");
                    sb.AppendLine($"<tr><td>{f.RuleId}</td><td class=\"sev-{f.Severity}\">{f.Severity}</td><td>{node}</td>" +
                                  $"<td>{System.Net.WebUtility.HtmlEncode(f.Message)}</td>" +
                                  $"<td>{System.Net.WebUtility.HtmlEncode(f.Recommendation ?? "")}</td>" +
                                  $"<td>{(f.IsAutoFixable ? "yes" : "no")}</td></tr>");
                }
                sb.AppendLine("</table>");
            }
        }
        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}
