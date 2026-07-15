// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema
{
	/// <summary>
	/// DataAnnotations validator for an XML Schema date / date-part <b>lexical string</b>.
	/// Used by the date-validation rule registry (for string-backed types whose <c>val</c> is the
	/// XSD lexical string) and by the lexical entry points for DateTime-backed types.
	/// <para>
	/// Produces an exceptionally helpful error message via
	/// <see cref="XsdDateTimePatterns.BuildLexicalErrorMessage(XsdDateKind, string?)"/> — it quotes
	/// the offending value, names the xs: type, gives the canonical form with a concrete example, and
	/// pinpoints the specific violation. An empty/unset value is treated as valid (use a separate
	/// required-ness rule if a value is mandatory).
	/// </para>
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false)]
	public sealed class XsdDateLexicalAttribute : ValidationAttribute
	{
		/// <summary>The XSD date/date-part lexical space this value must conform to.</summary>
		public XsdDateKind Kind { get; }

		public XsdDateLexicalAttribute(XsdDateKind kind) => Kind = kind;

		public override bool IsValid(object? value)
		{
			// Null / empty is considered valid here (an unset value is not a malformed value).
			if (value is null) return true;
			string s = value as string ?? value.ToString() ?? string.Empty;
			if (s.Length == 0) return true;
			return XsdDateTimePatterns.IsMatch(Kind, s);
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (IsValid(value)) return ValidationResult.Success;
			string s = value is null ? null! : (value as string ?? value.ToString());
			return new ValidationResult(XsdDateTimePatterns.BuildLexicalErrorMessage(Kind, s));
		}
	}

	/// <summary>
	/// DataAnnotations validator for a standalone XML Schema timezone offset string (the
	/// <c>timeZone</c> property on the date types). Accepts <c>Z</c>, the XSD <c>±hh:mm</c> form, and
	/// the .NET <see cref="System.TimeSpan"/> textual form, always enforcing the legal offset range
	/// -14:00 … +14:00. An empty/unset value is treated as valid.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field, AllowMultiple = false)]
	public sealed class XsdTimezoneOffsetAttribute : ValidationAttribute
	{
		public override bool IsValid(object? value)
		{
			if (value is null) return true;
			string s = value as string ?? value.ToString() ?? string.Empty;
			return XsdDateTimePatterns.IsValidTimezoneOffset(s);
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (IsValid(value)) return ValidationResult.Success;
			string s = value as string ?? value?.ToString();
			return new ValidationResult(XsdDateTimePatterns.BuildTimezoneErrorMessage(s));
		}
	}
}
