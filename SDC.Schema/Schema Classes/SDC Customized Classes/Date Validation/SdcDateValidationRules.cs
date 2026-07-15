// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SDC.Schema
{
	/// <summary>
	/// Populates <see cref="SdcValidationRuleRegistry"/> with the date / date-part validation rules at
	/// assembly load (via <see cref="ModuleInitializerAttribute"/>, so there is no static-ordering
	/// dependency). All rules live here in editable code; no auto-generated file is touched.
	/// </summary>
	public static class SdcDateValidationRules
	{
		private static bool _registered;
		private static readonly object _lock = new();

		[ModuleInitializer]
		public static void Initialize() => EnsureRegistered();

		/// <summary>Idempotently registers every date/date-part rule. Safe to call repeatedly.</summary>
		public static void EnsureRegistered()
		{
			lock (_lock)
			{
				if (_registered) return;
				_registered = true;

				// --- String-backed types: val IS the XSD lexical string → validate it directly. ---
				SdcValidationRuleRegistry.Register(typeof(gYear_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.GYear));
				SdcValidationRuleRegistry.Register(typeof(gYearMonth_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.GYearMonth));
				SdcValidationRuleRegistry.Register(typeof(gMonth_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.GMonth));
				SdcValidationRuleRegistry.Register(typeof(gMonthDay_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.GMonthDay));
				SdcValidationRuleRegistry.Register(typeof(gDay_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.GDay));

				SdcValidationRuleRegistry.Register(typeof(duration_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.Duration));
				// dayTimeDuration/yearMonthDuration setters already validate (weak generated regex);
				// registering here REPLACES that weak regex with the full ISO-8601 form.
				SdcValidationRuleRegistry.Register(typeof(dayTimeDuration_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.DayTimeDuration));
				SdcValidationRuleRegistry.Register(typeof(yearMonthDuration_Stype), "val", new XsdDateLexicalAttribute(XsdDateKind.YearMonthDuration));

				// --- dateTimeStamp (issue I-1 regression fix). ---
				// The generated DateTime members carry an impossible [RegularExpression] that can never
				// match DateTime.ToString(), so EVERY value is dropped. A DateTime cannot encode the
				// "timezone required" lexical rule, so we register an EMPTY rule set to neutralize the
				// broken regex; the timezone-required rule is enforced at the lexical string boundary
				// (SetLexicalValue / IDataHelpers parse path).
				var none = System.Array.Empty<ValidationAttribute>();
				SdcValidationRuleRegistry.Register(typeof(dateTimeStamp_Stype), "val", none);
				SdcValidationRuleRegistry.Register(typeof(dateTimeStamp_DEtype), "minInclusive", none);
				SdcValidationRuleRegistry.Register(typeof(dateTimeStamp_DEtype), "maxInclusive", none);
				SdcValidationRuleRegistry.Register(typeof(dateTimeStamp_DEtype), "minExclusive", none);
				SdcValidationRuleRegistry.Register(typeof(dateTimeStamp_DEtype), "maxExclusive", none);
			}
		}
	}
}
