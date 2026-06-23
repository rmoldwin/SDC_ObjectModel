namespace SDC.Schema
{
	// Hand-authored partial-class companions that document XSD-vs-.NET range divergences for the
	// decimal-backed integer family. Kept OUTSIDE the auto-generated *_Stype files so the remarks
	// survive Xsd2Code++ regeneration. Canonical reference:
	// SDC.Schema.Tests/Documentation/NumericRange_XSD_vs_NET.md

	/// <remarks>
	/// <para><b>XSD-vs-.NET range narrowing.</b> XSD <c>xs:integer</c> is unbounded; the OM backs
	/// <c>val</c> with <see cref="decimal"/> and caps it via <c>[Range(±7.9228e28)]</c>. This .NET
	/// limit is the binding constraint and is intentionally narrower than the XSD value space.</para>
	/// <para><b>Divergence A — MaxDigitsAttribute(29) counts the sign.</b> Validation checks
	/// <c>value.ToString().Length &lt;= 29</c>, so the leading '-' on a negative consumes one slot:
	/// negatives are limited to 28 significant digits, positives to 29. Thus <c>decimal.MaxValue</c>
	/// (29 digits) is accepted but <c>decimal.MinValue</c> (30 chars) is <b>soft-rejected</b>: the
	/// setter keeps the prior/unset value, does not throw, and records the offending value on the
	/// node (see <see cref="BaseType.RejectedValues"/>). See issue #8.</para>
	/// <para><b>Divergence D — JSON cannot round-trip large whole numbers.</b> A whole-number value
	/// beyond ulong range serializes to a bare JSON integer and deserializes as
	/// <c>System.Numerics.BigInteger</c>, which cannot be assigned to the decimal <c>val</c>
	/// (InvalidCastException). XML preserves these values; JSON currently throws.</para>
	/// </remarks>
	public partial class integer_Stype { }

	/// <remarks>
	/// Decimal-backed integer subtype. See <see cref="integer_Stype"/> remarks for the shared
	/// XSD-vs-.NET range narrowing, MaxDigitsAttribute sign-counting (Divergence A), and the JSON
	/// large-whole-number round-trip limitation (Divergence D). Effective range: ≈ −7.92e27 … −1.
	/// </remarks>
	public partial class negativeInteger_Stype { }

	/// <remarks>
	/// Decimal-backed integer subtype. See <see cref="integer_Stype"/> remarks for the shared
	/// divergences. Effective range: 0 … ≈ 7.92e28.
	/// </remarks>
	public partial class nonNegativeInteger_Stype { }

	/// <remarks>
	/// Decimal-backed integer subtype. See <see cref="integer_Stype"/> remarks for the shared
	/// divergences. Effective range: 1 … ≈ 7.92e28.
	/// </remarks>
	public partial class positiveInteger_Stype { }

	/// <remarks>
	/// Decimal-backed integer subtype. See <see cref="integer_Stype"/> remarks for the shared
	/// divergences. Effective range: ≈ −7.92e27 … 0.
	/// </remarks>
	public partial class nonPositiveInteger_Stype { }
}
