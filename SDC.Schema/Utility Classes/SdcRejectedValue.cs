// SDC-CUSTOM: do not overwrite
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema
{
	/// <summary>
	/// Records a single value that was <b>rejected</b> by a property setter (or by a
	/// deserialization path) because it failed DataAnnotations validation.<br/>
	/// <br/>
	/// Under the soft-reject contract (issue #8) an invalid value is <b>never</b> written to the
	/// strongly-typed backing field — the field keeps its prior/unset value. So that the offending
	/// value is not lost (and can be surfaced to a user for correction), it is recorded here,
	/// out-of-band, keyed by node and property via
	/// <see cref="SdcUtil.GetRejectedValues(BaseType)"/>.<br/>
	/// <br/>
	/// This record is held in a <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey,TValue}"/>
	/// keyed by the owning node, so it is never serialized and is garbage-collected with the node.
	/// </summary>
	public sealed class SdcRejectedValue
	{
		/// <summary>Name of the property whose assignment was rejected.</summary>
		public string PropertyName { get; init; } = string.Empty;

		/// <summary>The offending value that failed validation and was not stored.</summary>
		public object? AttemptedValue { get; init; }

		/// <summary>
		/// Human-readable description of why the value was rejected; includes the offending value.
		/// </summary>
		public string Message { get; init; } = string.Empty;

		/// <summary>Timestamp when the rejection occurred (most recent attempt wins per property).</summary>
		public DateTimeOffset RejectedAt { get; init; } = DateTimeOffset.Now;

		/// <summary>Raw DataAnnotations results produced by <see cref="Validator.TryValidateProperty"/>.</summary>
		public IReadOnlyList<ValidationResult> Results { get; init; } = Array.Empty<ValidationResult>();

		public override string ToString() =>
			$".{PropertyName} = {AttemptedValue ?? "null"} — {Message}";
	}
}
