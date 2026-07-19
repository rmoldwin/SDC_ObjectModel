namespace SDC.Schema
{
	// Hand-authored partial-class companions documenting serializer divergences on the unsigned-long
	// and decimal response types. Kept OUTSIDE the auto-generated *_Stype files so the remarks survive
	// Xsd2Code++ regeneration.
	// Canonical reference: SDC.Schema.Tests/Documentation/NumericRange_XSD_vs_NET.md

	/// <remarks>
	/// <b>Divergence E — BSON cannot serialize unsigned values above long.MaxValue.</b> BSON integer
	/// types are signed-only, so <c>val = ulong.MaxValue</c> throws on write
	/// ("Value is too large to fit in a signed 64 bit integer. BSON does not support unsigned values.").
	/// XML, JSON, and MsgPack preserve <c>ulong.MaxValue</c>; BSON throws. <c>val</c> covers the full
	/// XSD <c>xs:unsignedLong</c> range (0 … 18446744073709551615) for the non-BSON serializers.
	/// </remarks>
	public partial class unsignedLong_Stype { }

	/// <remarks>
	/// <para><b>XSD-vs-.NET range narrowing.</b> XSD <c>xs:decimal</c> is arbitrary precision/range; the
	/// OM backs <c>val</c> with .NET <see cref="decimal"/> (≈ ±7.92e28, 28–29 significant digits), the
	/// binding constraint.</para>
	/// <para><b>Divergence D — JSON cannot round-trip large whole-number decimals.</b> A whole-number
	/// decimal beyond ulong range serializes to a bare JSON integer and deserializes as
	/// <c>System.Numerics.BigInteger</c>, which cannot be assigned to <c>val</c> (InvalidCastException).
	/// Values with a fractional part, or within ulong range, round-trip normally.</para>
	/// <para><b>Divergence F — BSON loses decimal precision.</b> BSON has no native decimal type;
	/// Newtonsoft encodes <c>decimal</c> as a 64-bit IEEE <c>double</c>, rounding values that need more
	/// than ~15–17 significant digits. XML, JSON, and MsgPack preserve full precision; BSON does not.</para>
	/// </remarks>
	public partial class decimal_Stype { }
}
