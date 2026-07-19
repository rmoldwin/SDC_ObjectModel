// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
namespace SDC.Schema
{
	// Regen-safe set+validate entry points (Plan §8.4) for the DateTime-backed date/date-part types.
	// These types store a System.DateTime, so XSD lexical rules (timezone presence, stray time/date
	// component, end-of-day 24:00:00, required-timezone on dateTimeStamp) cannot be enforced on the
	// DateTime itself — they must be checked on the original *string*. SetLexicalValue is that string
	// boundary: it routes the lexical value through the issue #8 soft-reject pipeline
	// (SdcUtil.ValidateLexicalAndRaise) and only assigns val/timeZone when the value is legal. An
	// invalid value is never stored, never throws, and is recorded in RejectedValues with a helpful,
	// XSD-accurate message — identical semantics to the generated soft-reject setters.

	public partial class date_Stype
	{
		/// <summary>
		/// Validates <paramref name="lexicalValue"/> as an <c>xs:date</c> and, when legal, assigns
		/// <see cref="val"/> (and <see cref="timeZone"/>). Returns <see langword="false"/> and records a
		/// helpful soft-reject message (leaving the prior value intact) when it is not.
		/// </summary>
		public bool SetLexicalValue(string? lexicalValue)
		{
			if (!SdcUtil.ValidateLexicalAndRaise(this, nameof(val), lexicalValue, XsdDateKind.Date))
				return false;
			if (XsdDateTimePatterns.TryParseDateTime(XsdDateKind.Date, lexicalValue, out var dt, out var tz))
			{
				val = dt;
				timeZone = XsdDateTimePatterns.TryNormalizeTimeZoneToken(tz, out var normalized) ? normalized : tz;
			}
			return true;
		}
	}

	public partial class dateTime_Stype
	{
		/// <summary>
		/// Validates <paramref name="lexicalValue"/> as an <c>xs:dateTime</c> and, when legal, assigns
		/// <see cref="val"/> (and <see cref="timeZone"/>). Soft-rejects otherwise.
		/// </summary>
		public bool SetLexicalValue(string? lexicalValue)
		{
			if (!SdcUtil.ValidateLexicalAndRaise(this, nameof(val), lexicalValue, XsdDateKind.DateTime))
				return false;
			if (XsdDateTimePatterns.TryParseDateTime(XsdDateKind.DateTime, lexicalValue, out var dt, out var tz))
			{
				val = dt;
				timeZone = XsdDateTimePatterns.TryNormalizeTimeZoneToken(tz, out var normalized) ? normalized : tz;
			}
			return true;
		}
	}

	public partial class time_Stype
	{
		/// <summary>
		/// Validates <paramref name="lexicalValue"/> as an <c>xs:time</c> and, when legal, assigns
		/// <see cref="val"/> (and <see cref="timeZone"/>). Soft-rejects otherwise.
		/// </summary>
		public bool SetLexicalValue(string? lexicalValue)
		{
			if (!SdcUtil.ValidateLexicalAndRaise(this, nameof(val), lexicalValue, XsdDateKind.Time))
				return false;
			if (XsdDateTimePatterns.TryParseDateTime(XsdDateKind.Time, lexicalValue, out var dt, out var tz))
			{
				val = dt;
				timeZone = XsdDateTimePatterns.TryNormalizeTimeZoneToken(tz, out var normalized) ? normalized : tz;
			}
			return true;
		}
	}

	public partial class dateTimeStamp_Stype
	{
		/// <summary>
		/// Validates <paramref name="lexicalValue"/> as an <c>xs:dateTimeStamp</c> (a dateTime whose
		/// timezone is REQUIRED) and, when legal, assigns the UTC-normalized <see cref="val"/>.
		/// Soft-rejects otherwise. This is the string boundary that enforces the timezone-required rule
		/// the DateTime field cannot encode (the I-1 regression fix).
		/// </summary>
		public bool SetLexicalValue(string? lexicalValue)
		{
			if (!SdcUtil.ValidateLexicalAndRaise(this, nameof(val), lexicalValue, XsdDateKind.DateTimeStamp))
				return false;
			if (XsdDateTimePatterns.TryParseDateTime(XsdDateKind.DateTimeStamp, lexicalValue, out var dt, out _))
				val = dt;
			return true;
		}
	}
}
