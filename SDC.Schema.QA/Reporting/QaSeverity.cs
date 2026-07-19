namespace SDC.Schema.QA.Reporting;

/// <summary>
/// Severity of a single QA/QC finding. Mirrors the SdcValidationSeverity split
/// (Error/Warning) used elsewhere in SDC.Schema, plus an Info level for
/// best-practice suggestions that are not correctness issues.
/// </summary>
public enum QaSeverity
{
    Info,
    Warning,
    Error
}
