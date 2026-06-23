namespace SDC.Schema
{
	// Hand-authored partial-class companions documenting the negative-zero divergence shared by the
	// IEEE floating-point response types. Kept OUTSIDE the auto-generated *_Stype files so the remarks
	// survive Xsd2Code++ regeneration.
	// Canonical reference: SDC.Schema.Tests/Documentation/NumericRange_XSD_vs_NET.md

	/// <remarks>
	/// <b>Divergence C — sign of zero (−0) is not preserved from default.</b> The generated <c>val</c>
	/// setter guards with <c>if (_val.Equals(value) != true)</c>. A fresh node's <c>_val</c> is
	/// <c>+0.0f</c> and <c>(+0.0f).Equals(−0.0f)</c> is <c>true</c>, so assigning <c>−0</c> to an
	/// untouched node is skipped and the stored value remains <c>+0</c>. Numeric equality is preserved;
	/// only the sign bit of zero is lost. <c>NaN</c>, <c>+INF</c>, and <c>−INF</c> are preserved through
	/// get/set and XML/JSON round-trips, matching the XSD <c>xs:float</c> value space.
	/// </remarks>
	public partial class float_Stype { }

	/// <remarks>
	/// See <see cref="float_Stype"/> remarks. The same Divergence C (−0 collapses to +0 from default)
	/// applies to <c>double</c>; <c>NaN</c>/<c>±INF</c> are preserved, matching XSD <c>xs:double</c>.
	/// </remarks>
	public partial class double_Stype { }
}
