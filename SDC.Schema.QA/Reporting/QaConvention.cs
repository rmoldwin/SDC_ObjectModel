namespace SDC.Schema.QA.Reporting;

/// <summary>
/// Distinguishes best-practice rules that are generic to the IHE SDC standard
/// from rules that encode College of American Pathologists (CAP)-specific
/// conventions. Every <see cref="Rules.IQaRule"/> must declare which bucket it
/// belongs in so reports and the best-practices guide can keep the two
/// clearly separated, per project requirements.
/// </summary>
public enum QaConvention
{
    /// <summary>Applies to any SDC OM consumer, regardless of organization.</summary>
    Generic,

    /// <summary>Encodes a College of American Pathologists-specific convention or preference.</summary>
    Cap
}
