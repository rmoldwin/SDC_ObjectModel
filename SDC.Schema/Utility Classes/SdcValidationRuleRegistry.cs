// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema
{
	/// <summary>
	/// Regen-safe registry mapping <c>(declaring Type, member name) → ValidationAttribute[]</c>.
	/// <para>
	/// <b>Why this exists.</b> Issue #8 routes every facet-constrained setter through
	/// <see cref="SdcUtil.ValidateAndRaise(object?, ValidationContext)"/>. For the date / date-part
	/// types we must add (or, for <c>dateTimeStamp</c>, <i>replace</i>) validation rules <b>without</b>
	/// editing the auto-generated <c>*_Stype</c>/<c>*_DEtype</c> files. When a member is registered
	/// here, <see cref="SdcUtil.ValidateAndRaise"/> validates against the registered attributes
	/// (via <see cref="Validator.TryValidateValue"/>) <b>instead of</b> the attributes physically
	/// declared on the generated property — so a registered rule overrides the generated one. This is
	/// what lets us neutralize the impossible <c>[RegularExpression]</c> on the <c>dateTimeStamp</c>
	/// <see cref="DateTime"/> members (the issue I-1 regression) and strengthen the weak duration
	/// regexes, with all rules living in editable code.
	/// </para>
	/// <para>
	/// <c>[MetadataType]</c> buddy classes were rejected for this purpose because
	/// <see cref="Validator.TryValidateProperty"/> does not honor buddy metadata without registering an
	/// <c>AssociatedMetadataTypeTypeDescriptionProvider</c>; this explicit registry is deterministic,
	/// unit-testable, and survives xsd2code++ regeneration by construction.
	/// </para>
	/// </summary>
	public static class SdcValidationRuleRegistry
	{
		private static readonly Dictionary<(Type, string), ValidationAttribute[]> _rules = new();
		private static readonly object _lock = new();

		/// <summary>
		/// Registers (replacing any prior entry) the validation attributes that apply to
		/// <paramref name="memberName"/> on <paramref name="declaringType"/>. Pass an empty array to
		/// <b>neutralize</b> the attributes declared on the generated member (e.g. an impossible regex).
		/// </summary>
		public static void Register(Type declaringType, string memberName, params ValidationAttribute[] attributes)
		{
			if (declaringType is null) throw new ArgumentNullException(nameof(declaringType));
			if (string.IsNullOrEmpty(memberName)) throw new ArgumentNullException(nameof(memberName));
			lock (_lock) { _rules[(declaringType, memberName)] = attributes ?? Array.Empty<ValidationAttribute>(); }
		}

		/// <summary>
		/// Looks up the registered attributes for <paramref name="memberName"/>, walking the type
		/// hierarchy so a rule registered on a base <c>*_Stype</c> also applies to a derived
		/// <c>*_DEtype</c> that inherits the member. Returns <see langword="false"/> when no rule is
		/// registered for the member anywhere in the hierarchy.
		/// </summary>
		public static bool TryGet(Type instanceType, string memberName, out ValidationAttribute[] attributes)
		{
			lock (_lock)
			{
				for (Type? t = instanceType; t is not null; t = t.BaseType)
				{
					if (_rules.TryGetValue((t, memberName), out var found))
					{
						attributes = found;
						return true;
					}
				}
			}
			attributes = Array.Empty<ValidationAttribute>();
			return false;
		}

		/// <summary><see langword="true"/> when a rule is registered for the member anywhere in the hierarchy.</summary>
		public static bool IsRegistered(Type instanceType, string memberName) =>
			TryGet(instanceType, memberName, out _);
	}
}
