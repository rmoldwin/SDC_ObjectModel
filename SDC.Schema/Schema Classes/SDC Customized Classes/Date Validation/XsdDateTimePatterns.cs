// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SDC.Schema
{
	/// <summary>
	/// Single source of truth for the XML Schema date / date-part <b>lexical</b> patterns, their
	/// canonical human-readable forms, concrete valid examples, and the helpful-message builder used
	/// by the soft-reject date validation (companion to issue #8 numeric soft-reject).
	/// <para>
	/// The <c>xs:date</c>/<c>xs:dateTime</c>/<c>xs:time</c>/<c>g*</c> patterns were promoted verbatim
	/// from the authoritative regexes already present in <c>IDataHelpers.AddDataTypesDE</c> and then
	/// <b>anchored</b> (<c>^…$</c>) so they reject trailing garbage; the duration-family patterns are
	/// full ISO-8601 forms authored here (the prior generated regexes were weak/partial).
	/// </para>
	/// </summary>
	public static class XsdDateTimePatterns
	{
		// Shared timezone fragment: Z or ±hh:mm within the XSD-legal range -14:00 … +14:00.
		// (Hours 00-13 with any minutes, or exactly 14:00.)
		private const string Tz = @"(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))";

		// Year: 4+ digits, no leading zero unless < 1000 (CCYY, optional leading minus).
		private const string Year = @"-?([1-9][0-9]{3,}|0[0-9]{3})";
		private const string Month = @"(0[1-9]|1[0-2])";
		private const string Day = @"(0[1-9]|[12][0-9]|3[01])";
		// Time: hh:mm:ss[.fff], allowing the XSD-legal end-of-day 24:00:00.
		private const string TimeOfDay = @"(([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\.[0-9]+)?|(24:00:00(\.0+)?))";

		#region Anchored lexical patterns (single source of truth)

		/// <summary><c>xs:date</c> — <c>[-]CCYY-MM-DD[Z|±hh:mm]</c>. A time component is rejected.</summary>
		public const string Date = "^" + Year + "-" + Month + "-" + Day + Tz + "?$";

		/// <summary><c>xs:dateTime</c> — <c>[-]CCYY-MM-DDThh:mm:ss[.fff][Z|±hh:mm]</c>.</summary>
		public const string DateTime = "^" + Year + "-" + Month + "-" + Day + "T" + TimeOfDay + Tz + "?$";

		/// <summary><c>xs:time</c> — <c>hh:mm:ss[.fff][Z|±hh:mm]</c>. A date component is rejected.</summary>
		public const string Time = "^" + TimeOfDay + Tz + "?$";

		/// <summary><c>xs:dateTimeStamp</c> — an <c>xs:dateTime</c> whose timezone is <b>required</b>.</summary>
		public const string DateTimeStamp = "^" + Year + "-" + Month + "-" + Day + "T" + TimeOfDay + Tz + "$";

		/// <summary><c>xs:gYear</c> — <c>[-]CCYY[Z|±hh:mm]</c>.</summary>
		public const string GYear = "^" + Year + Tz + "?$";

		/// <summary><c>xs:gYearMonth</c> — <c>[-]CCYY-MM[Z|±hh:mm]</c>.</summary>
		public const string GYearMonth = "^" + Year + "-" + Month + Tz + "?$";

		/// <summary><c>xs:gMonth</c> — <c>--MM[Z|±hh:mm]</c>.</summary>
		public const string GMonth = "^--" + Month + Tz + "?$";

		/// <summary><c>xs:gMonthDay</c> — <c>--MM-DD[Z|±hh:mm]</c>.</summary>
		public const string GMonthDay = "^--" + Month + "-" + Day + Tz + "?$";

		/// <summary><c>xs:gDay</c> — <c>---DD[Z|±hh:mm]</c>.</summary>
		public const string GDay = "^---" + Day + Tz + "?$";

		// Duration family: a leading lookahead forces at least one component (rejects a bare "P"),
		// and the T-group lookahead forces a time component to follow "T" (rejects a bare "PT").
		/// <summary><c>xs:duration</c> — <c>[-]PnYnMnDTnHnMnS</c>, at least one component, no bare "P"/"T".</summary>
		public const string Duration =
			@"^-?P(?=[0-9T])([0-9]+Y)?([0-9]+M)?([0-9]+D)?(T(?=[0-9])([0-9]+H)?([0-9]+M)?([0-9]+(\.[0-9]+)?S)?)?$";

		/// <summary><c>xs:dayTimeDuration</c> — duration with day + time parts only (no Y, no date-M).</summary>
		public const string DayTimeDuration =
			@"^-?P(?=[0-9T])([0-9]+D)?(T(?=[0-9])([0-9]+H)?([0-9]+M)?([0-9]+(\.[0-9]+)?S)?)?$";

		/// <summary><c>xs:yearMonthDuration</c> — duration with year + month parts only.</summary>
		public const string YearMonthDuration =
			@"^-?P(?=[0-9])([0-9]+Y)?([0-9]+M)?$";

		/// <summary>
		/// Standalone timezone offset (the <c>timeZone</c> property): <c>Z</c> or <c>±hh:mm</c> within
		/// -14:00 … +14:00. Also tolerates the .NET <see cref="TimeSpan"/> textual form
		/// <c>±h:mm:ss</c> that <c>IDataHelpers</c> currently emits (range still enforced separately).
		/// </summary>
		public const string TimezoneOffset = "^" + Tz + "$";

		#endregion

		// Lowest / highest XSD-legal timezone offset, in minutes.
		internal const int MinOffsetMinutes = -14 * 60;
		internal const int MaxOffsetMinutes = 14 * 60;

		/// <summary>Returns the anchored lexical regex pattern for <paramref name="kind"/>.</summary>
		public static string PatternFor(XsdDateKind kind) => kind switch
		{
			XsdDateKind.Date => Date,
			XsdDateKind.DateTime => DateTime,
			XsdDateKind.Time => Time,
			XsdDateKind.DateTimeStamp => DateTimeStamp,
			XsdDateKind.GYear => GYear,
			XsdDateKind.GYearMonth => GYearMonth,
			XsdDateKind.GMonth => GMonth,
			XsdDateKind.GMonthDay => GMonthDay,
			XsdDateKind.GDay => GDay,
			XsdDateKind.Duration => Duration,
			XsdDateKind.DayTimeDuration => DayTimeDuration,
			XsdDateKind.YearMonthDuration => YearMonthDuration,
			_ => throw new ArgumentOutOfRangeException(nameof(kind))
		};

		// Compiled regex cache so registry validation and the parse path don't recompile per call.
		private static readonly Dictionary<XsdDateKind, Regex> _compiled = new();
		private static readonly object _compiledLock = new();

		/// <summary>Returns (and caches) a compiled <see cref="Regex"/> for <paramref name="kind"/>.</summary>
		public static Regex RegexFor(XsdDateKind kind)
		{
			lock (_compiledLock)
			{
				if (!_compiled.TryGetValue(kind, out var rx))
				{
					rx = new Regex(PatternFor(kind), RegexOptions.CultureInvariant);
					_compiled[kind] = rx;
				}
				return rx;
			}
		}

		/// <summary><see langword="true"/> when <paramref name="value"/> is a legal lexical form of <paramref name="kind"/>.</summary>
		public static bool IsMatch(XsdDateKind kind, string value) => RegexFor(kind).IsMatch(value);

		/// <summary>The XSD type name, e.g. <c>xs:dateTimeStamp</c>.</summary>
		public static string XsdName(XsdDateKind kind) => kind switch
		{
			XsdDateKind.Date => "xs:date",
			XsdDateKind.DateTime => "xs:dateTime",
			XsdDateKind.Time => "xs:time",
			XsdDateKind.DateTimeStamp => "xs:dateTimeStamp",
			XsdDateKind.GYear => "xs:gYear",
			XsdDateKind.GYearMonth => "xs:gYearMonth",
			XsdDateKind.GMonth => "xs:gMonth",
			XsdDateKind.GMonthDay => "xs:gMonthDay",
			XsdDateKind.GDay => "xs:gDay",
			XsdDateKind.Duration => "xs:duration",
			XsdDateKind.DayTimeDuration => "xs:dayTimeDuration",
			XsdDateKind.YearMonthDuration => "xs:yearMonthDuration",
			_ => "xs:?"
		};

		// Canonical human-readable form for each kind.
		private static string CanonicalForm(XsdDateKind kind) => kind switch
		{
			XsdDateKind.Date => "CCYY-MM-DD with an optional timezone",
			XsdDateKind.DateTime => "CCYY-MM-DDThh:mm:ss with an optional fractional second and optional timezone",
			XsdDateKind.Time => "hh:mm:ss with an optional fractional second and optional timezone",
			XsdDateKind.DateTimeStamp => "CCYY-MM-DDThh:mm:ss with a REQUIRED timezone",
			XsdDateKind.GYear => "CCYY (4 or more digits) with an optional timezone",
			XsdDateKind.GYearMonth => "CCYY-MM with an optional timezone",
			XsdDateKind.GMonth => "--MM with an optional timezone",
			XsdDateKind.GMonthDay => "--MM-DD with an optional timezone",
			XsdDateKind.GDay => "---DD with an optional timezone",
			XsdDateKind.Duration => "PnYnMnDTnHnMnS with at least one component",
			XsdDateKind.DayTimeDuration => "PnDTnHnMnS (day and time parts only)",
			XsdDateKind.YearMonthDuration => "PnYnM (year and month parts only)",
			_ => "(unknown)"
		};

		// One concrete valid example per kind.
		private static string Example(XsdDateKind kind) => kind switch
		{
			XsdDateKind.Date => "2026-06-22 or 2026-06-22Z",
			XsdDateKind.DateTime => "2026-06-22T10:00:00 or 2026-06-22T10:00:00Z",
			XsdDateKind.Time => "10:00:00 or 10:00:00Z",
			XsdDateKind.DateTimeStamp => "2026-06-22T10:00:00Z or 2026-06-22T10:00:00-05:00",
			XsdDateKind.GYear => "2026",
			XsdDateKind.GYearMonth => "2026-06",
			XsdDateKind.GMonth => "--06",
			XsdDateKind.GMonthDay => "--06-22",
			XsdDateKind.GDay => "---22",
			XsdDateKind.Duration => "P3Y6M4DT12H30M5S or PT15M",
			XsdDateKind.DayTimeDuration => "P5DT12H",
			XsdDateKind.YearMonthDuration => "P3Y6M",
			_ => "(none)"
		};

		/// <summary>
		/// Builds the exceptionally-helpful soft-reject message: quotes the offending value, names the
		/// xs: type, gives the canonical form with a concrete example, and pinpoints the specific
		/// violation when one can be determined.
		/// </summary>
		public static string BuildLexicalErrorMessage(XsdDateKind kind, string? value)
		{
			string v = value ?? "null";
			string specific = AnalyzeViolation(kind, value);
			string head =
				$"'{v}' is not a valid {XsdName(kind)}. The correct form is {CanonicalForm(kind)} " +
				$"(for example, {Example(kind)}).";
			return specific.Length == 0 ? head : head + " " + specific;
		}

		// Best-effort pinpointing of the most common, teachable violations. Returns "" when no
		// specific cause is confidently identified (the canonical form + example still teaches the form).
		private static string AnalyzeViolation(XsdDateKind kind, string? value)
		{
			string v = value ?? string.Empty;

			switch (kind)
			{
				case XsdDateKind.DateTimeStamp:
					// Same as dateTime but timezone is mandatory: if it matches dateTime-without-zone, say so.
					if (IsMatch(XsdDateKind.DateTime, v) && !HasTimezone(v))
						return "A dateTimeStamp is a dateTime that REQUIRES a timezone (e.g. ...Z or ...-05:00); your value has no timezone.";
					break;

				case XsdDateKind.Date:
					// A stray time component is the classic mistake.
					if (v.Contains('T'))
						return "An xs:date must not carry a time component; supply the date only (use xs:dateTime if you need a time).";
					return MonthDayViolation(v);

				case XsdDateKind.Time:
					if (v.Contains('-') && v.Contains('T'))
						return "An xs:time must not carry a date component; supply the time only (use xs:dateTime if you need a date).";
					break;

				case XsdDateKind.DateTime:
					return MonthDayViolation(v);

				case XsdDateKind.GMonth:
				case XsdDateKind.GMonthDay:
				case XsdDateKind.GYearMonth:
					return MonthDayViolation(v);

				case XsdDateKind.GDay:
					return DayOnlyViolation(v);

				case XsdDateKind.Duration:
					if (v == "P" || v == "-P") return "An empty duration 'P' is not allowed; supply at least one component.";
					if (v.EndsWith("T", StringComparison.Ordinal)) return "A 'T' must be followed by at least one time component (H, M, or S).";
					break;

				case XsdDateKind.DayTimeDuration:
					if (HasYearOrDateMonthPart(v))
						return "This type allows only day and time parts (D, H, M, S); Year/Month parts are not permitted. You supplied 'Y' or a date 'M'.";
					if (v == "P" || v == "-P") return "An empty duration 'P' is not allowed; supply at least one component.";
					break;

				case XsdDateKind.YearMonthDuration:
					if (HasDayOrTimePart(v))
						return "This type allows only year and month parts (Y, M); day/time parts (D, T, H, S) are not permitted.";
					if (v == "P" || v == "-P") return "An empty duration 'P' is not allowed; supply at least one component.";
					break;
			}
			return string.Empty;
		}

		// Extract the month token where the lexical form is .*-MM(-DD)? or --MM(-DD)? and report if out of 01-12.
		private static string MonthDayViolation(string v)
		{
			var m = Regex.Match(v, @"-(?<mm>[0-9]{2})(-(?<dd>[0-9]{2}))?(?![0-9])");
			if (m.Success && int.TryParse(m.Groups["mm"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int mm))
			{
				if (mm < 1 || mm > 12)
					return $"The month must be 01-12; you supplied '{m.Groups["mm"].Value}'.";
				if (m.Groups["dd"].Success
					&& int.TryParse(m.Groups["dd"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int dd)
					&& (dd < 1 || dd > 31))
					return $"The day must be 01-31; you supplied '{m.Groups["dd"].Value}'.";
			}
			return string.Empty;
		}

		private static string DayOnlyViolation(string v)
		{
			var m = Regex.Match(v, @"---(?<dd>[0-9]{2})");
			if (m.Success && int.TryParse(m.Groups["dd"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int dd)
				&& (dd < 1 || dd > 31))
				return $"The day must be 01-31; you supplied '{m.Groups["dd"].Value}'.";
			return string.Empty;
		}

		private static bool HasTimezone(string v) => v.EndsWith("Z", StringComparison.Ordinal)
			|| Regex.IsMatch(v, @"(\+|-)[0-9]{2}:[0-9]{2}$");

		// In the time portion (after T) an "M" is minutes; before T (or with no T) an "M" is a month.
		// A "Y" is never legal in a dayTimeDuration, regardless of position.
		private static bool HasYearOrDateMonthPart(string v)
		{
			int t = v.IndexOf('T');
			string datePart = t < 0 ? v : v.Substring(0, t);
			return v.IndexOf('Y') >= 0 || datePart.IndexOf('M') >= 0;
		}

		private static bool HasDayOrTimePart(string v) =>
			v.IndexOf('D') >= 0 || v.IndexOf('T') >= 0 || v.IndexOf('H') >= 0 || v.IndexOf('S') >= 0;

		#region Timezone offset (standalone timeZone property)

		/// <summary>
		/// Validates a standalone timezone offset string. Accepts <c>Z</c>, the XSD <c>±hh:mm</c> form,
		/// and the .NET <see cref="TimeSpan"/> textual form <c>±h:mm:ss</c> (which <c>IDataHelpers</c>
		/// currently produces), enforcing the XSD offset range -14:00 … +14:00 in all cases.
		/// </summary>
		public static bool IsValidTimezoneOffset(string? value)
		{
			if (string.IsNullOrEmpty(value)) return true; // unset/empty is allowed
			if (value == "Z") return true;

			// Strict XSD ±hh:mm
			if (Regex.IsMatch(value, @"^(\+|-)[0-9]{2}:[0-9]{2}$"))
				return OffsetWithinRange(value);

			// Lenient .NET TimeSpan form ±h:mm:ss (seconds must be 0 for a legal offset)
			if (TimeSpan.TryParse(value.TrimStart('+'), CultureInfo.InvariantCulture, out var ts))
			{
				bool neg = value.StartsWith("-", StringComparison.Ordinal);
				int minutes = (int)(neg ? -ts.TotalMinutes : ts.TotalMinutes);
				return ts.Seconds == 0 && minutes >= MinOffsetMinutes && minutes <= MaxOffsetMinutes;
			}
			return false;
		}

		private static bool OffsetWithinRange(string hhmm)
		{
			bool neg = hhmm.StartsWith("-", StringComparison.Ordinal);
			int hh = int.Parse(hhmm.Substring(1, 2), CultureInfo.InvariantCulture);
			int mm = int.Parse(hhmm.Substring(4, 2), CultureInfo.InvariantCulture);
			int minutes = (neg ? -1 : 1) * (hh * 60 + mm);
			return minutes >= MinOffsetMinutes && minutes <= MaxOffsetMinutes;
		}

		/// <summary>Builds the helpful message for an invalid standalone timezone offset.</summary>
		public static string BuildTimezoneErrorMessage(string? value) =>
			$"'{value ?? "null"}' is not a valid timezone. Offsets must be ±hh:mm within -14:00..+14:00; " +
			$"you supplied '{value ?? "null"}'.";

		/// <summary>Normalizes a timezone token to the canonical <c>±hh:mm</c> form used by the OM.</summary>
		public static bool TryNormalizeTimeZoneToken(string? value, out string? normalized)
		{
			normalized = null;
			if (string.IsNullOrWhiteSpace(value)) return true;

			var trimmed = value.Trim();
			if (trimmed.Equals("Z", StringComparison.OrdinalIgnoreCase))
			{
				normalized = "+00:00";
				return true;
			}

			if (!Regex.IsMatch(trimmed, @"^([+-])([0-9]{2}):([0-9]{2})$", RegexOptions.CultureInvariant))
				return false;

			var signChar = trimmed[0];
			var hh = int.Parse(trimmed.Substring(1, 2), CultureInfo.InvariantCulture);
			var mm = int.Parse(trimmed.Substring(4, 2), CultureInfo.InvariantCulture);
			if (hh < 0 || hh > 14 || mm < 0 || mm > 59 || (hh == 14 && mm != 0))
				return false;

			normalized = $"{signChar}{hh:D2}:{mm:D2}";
			return true;
		}

		/// <summary>Parses a canonical or legacy timezone token into a <see cref="TimeSpan"/> offset.</summary>
		public static bool TryParseOffset(string? value, out TimeSpan? offset)
		{
			offset = null;
			if (string.IsNullOrWhiteSpace(value)) return true;
			if (!TryNormalizeTimeZoneToken(value, out var normalized) || normalized is null)
				return false;

			var sign = normalized[0] == '-' ? -1 : 1;
			var hh = int.Parse(normalized.Substring(1, 2), CultureInfo.InvariantCulture);
			var mm = int.Parse(normalized.Substring(4, 2), CultureInfo.InvariantCulture);
			var totalMinutes = sign * (hh * 60 + mm);
			offset = TimeSpan.FromMinutes(totalMinutes);
			return true;
		}

		/// <summary>Formats a <see cref="TimeSpan"/> offset as a canonical <c>±hh:mm</c> token.</summary>
		public static string FormatOffset(TimeSpan offset)
		{
			if (offset.Ticks % TimeSpan.FromMinutes(1).Ticks != 0)
				throw new ArgumentOutOfRangeException(nameof(offset), offset, "Timezone offsets must have minute precision.");

			var totalMinutes = offset.Ticks / TimeSpan.FromMinutes(1).Ticks;
			if (totalMinutes < MinOffsetMinutes || totalMinutes > MaxOffsetMinutes)
				throw new ArgumentOutOfRangeException(nameof(offset), offset, "Timezone offsets must be within -14:00..+14:00.");

			var sign = totalMinutes < 0 ? "-" : "+";
			var absMinutes = Math.Abs(totalMinutes);
			var hh = (int)(absMinutes / 60);
			var mm = (int)(absMinutes % 60);
			return $"{sign}{hh:D2}:{mm:D2}";
		}

		#endregion

		#region Lexical → DateTime parsing (for the DateTime-backed types' SetLexicalValue)

		/// <summary>
		/// Returns the trailing timezone token (<c>Z</c> or <c>±hh:mm</c>) of a date/time lexical
		/// string, or <see langword="null"/> when none is present. Preserving the original token keeps
		/// the <c>Z</c>-vs-<c>+00:00</c> distinction that a parsed offset would lose.
		/// </summary>
		public static string? ExtractTimezoneToken(string? lexical)
		{
			if (string.IsNullOrEmpty(lexical)) return null;
			var m = Regex.Match(lexical!, @"(Z|(\+|-)[0-9]{2}:[0-9]{2})$");
			return m.Success ? m.Value : null;
		}

		/// <summary>
		/// Parses an already-lexically-valid date/dateTime/time/dateTimeStamp string into the
		/// <see cref="DateTime"/> backing value plus its timezone token. Handles the XSD end-of-day
		/// <c>24:00:00</c> (which .NET cannot store as hour 24) by normalizing to <c>00:00:00</c> of the
		/// next day, and converts <c>dateTimeStamp</c> to UTC (mirroring the existing parse path).
		/// Returns <see langword="false"/> only when the value is lexically legal but not representable
		/// as a <see cref="DateTime"/>.
		/// </summary>
		public static bool TryParseDateTime(XsdDateKind kind, string? lexical, out DateTime value, out string? timezone)
		{
			value = default;
			var extracted = ExtractTimezoneToken(lexical);
			timezone = TryNormalizeTimeZoneToken(extracted, out var normalized) ? normalized : null;
			if (string.IsNullOrEmpty(lexical)) return false;

			string body = extracted is null ? lexical! : lexical!.Substring(0, lexical!.Length - extracted.Length);
			bool endOfDay = body.Contains("24:00:00");
			if (endOfDay) body = body.Replace("24:00:00", "00:00:00");

			if (kind == XsdDateKind.DateTimeStamp)
			{
				if (!DateTimeOffset.TryParse(lexical, CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dto))
					return false;
				value = dto.UtcDateTime;
				if (endOfDay) value = value.AddDays(1);
				return true;
			}

			// xs:time has no date; anchor an arbitrary date so DateTime.TryParse is stable.
			string toParse = kind == XsdDateKind.Time ? "1970-01-01T" + body : body;
			if (!System.DateTime.TryParse(toParse, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out value))
				return false;
			if (endOfDay) value = value.AddDays(1);
			if (kind == XsdDateKind.Date) value = value.Date;
			return true;
		}

		#endregion
	}
}
