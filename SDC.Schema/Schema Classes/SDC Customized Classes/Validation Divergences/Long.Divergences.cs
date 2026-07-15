namespace SDC.Schema
{
	// Hand-authored partial-class companion documenting a constraint-facet divergence on long_DEtype.
	// Kept OUTSIDE the auto-generated *_DEtype file so the remarks survive Xsd2Code++ regeneration.
	// Canonical reference: SDC.Schema.Tests/Documentation/NumericRange_XSD_vs_NET.md

	/// <remarks>
	/// <b>Divergence B — exclusive facets unenforceable at the type extreme.</b>
	/// <c>[Range(-9223372036854775807, 9223372036854775807)]</c> on <c>minExclusive</c>/<c>maxExclusive</c>
	/// binds the <c>RangeAttribute(double, double)</c> overload. <c>long.MaxValue</c> and
	/// <c>long.MaxValue − 1</c> round to the same <see cref="double"/>, so the facet boundary cannot be
	/// enforced at the extreme — assigning <c>long.MaxValue</c> does not throw. Inner long values
	/// validate normally. <c>val</c> itself (an <see cref="long"/>) is unaffected and covers the full
	/// XSD <c>xs:long</c> range.
	/// </remarks>
	public partial class long_DEtype { }
}
