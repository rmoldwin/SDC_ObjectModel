// SDC-CUSTOM: do not overwrite
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SDC.Schema.Tests")]

namespace SDC.Schema
{
	/// <summary>
	/// Severity level for an SDC validation event.
	/// </summary>
	public enum SdcValidationSeverity
	{
		/// <summary>
		/// Informational notice only; no corrective action is required.
		/// </summary>
		Info,
		/// <summary>
		/// The value was applied, but the result may lead to a coherence issue or other follow-up concern.
		/// </summary>
		Warning,
		/// <summary>
		/// The validation failed and the attempted value was not applied.
		/// </summary>
		Error
	}

	/// <summary>
	/// Event arguments for a single SDC validation issue raised by
	/// <see cref="SdcValidationEvents"/>. Carries the node identity, the
	/// property that failed, the value that was rejected (when applicable),
	/// a human-readable message, a severity classification, and the raw
	/// DataAnnotations results from <see cref="Validator"/>.
	/// </summary>
	public class SdcValidationEventArgs : EventArgs
	{
		/// <summary>The node's <c>ID</c> value when available, or null if the source node has no stable ID.</summary>
		public string? NodeID { get; init; }

		/// <summary>Name of the property that triggered the validation issue.</summary>
		public string? PropertyName { get; init; }

		/// <summary>The value that was rejected (may be null for non-setter events).</summary>
		public object? AttemptedValue { get; init; }

		/// <summary>Human-readable description of the validation issue.</summary>
		public string Message { get; init; } = string.Empty;

		/// <summary>Severity of the validation issue.</summary>
		public SdcValidationSeverity Severity { get; init; } = SdcValidationSeverity.Error;

		/// <summary>
		/// Raw DataAnnotations validation results, when the event originates from
		/// a setter that uses <see cref="Validator.TryValidateProperty"/>.
		/// Empty for events raised from other code paths (e.g., AddDataTypesDE).
		/// </summary>
		public IReadOnlyList<ValidationResult> Results { get; init; } = Array.Empty<ValidationResult>();
	}

	/// <summary>
	/// Central, static event hub for SDC validation issues.
	/// </summary>
	/// <remarks>
	/// Subscribe to <see cref="ValidationOccurred"/> before performing operations that may validate.
	/// All scoped validation entry points in this cleanup pass—
	/// <see cref="SdcDataTypeBuilder.AddDataTypesDE"/>,
	/// <see cref="SdcUtil.ValidateAndRaise(object?, ValidationContext)"/>, and
	/// <see cref="SdcUtil.ValidateLexicalAndRaise(BaseType, string, string?, XsdDateKind)"/>—
	/// converge here so callers can observe one consistent stream of non-throwing validation notifications.
	/// Rejected-value recording remains separate and is preserved even when
	/// <see cref="SdcUtil.SuppressValidation"/> silences this event.
	/// </remarks>
	public static class SdcValidationEvents
	{
		/// <summary>
		/// Raised whenever an SDC validation issue is detected.
		/// The <c>sender</c> argument is always <see langword="null"/> because the event is module-level.
		/// </summary>
		/// <seealso cref="SdcUtil.ValidateAndRaise(object?, ValidationContext)"/>
		/// <seealso cref="SdcUtil.ValidateLexicalAndRaise(BaseType, string, string?, XsdDateKind)"/>
		public static event EventHandler<SdcValidationEventArgs>? ValidationOccurred;

		/// <summary>
		/// Raises a validation event using a preconstructed payload.
		/// </summary>
		/// <param name="e">The validation event arguments to publish.</param>
		/// <seealso cref="ValidationOccurred"/>
		internal static void Raise(SdcValidationEventArgs e) =>
			ValidationOccurred?.Invoke(null, e);

		/// <summary>
		/// Convenience overload that raises a simple validation event from raw message/context values.
		/// </summary>
		/// <param name="message">Human-readable description of the issue.</param>
		/// <param name="nodeID">Optional: the affected node's <c>ID</c> value when one is available.</param>
		/// <param name="propertyName">Optional: the property associated with the validation issue.</param>
		/// <param name="attemptedValue">Optional: the value that was rejected or flagged.</param>
		/// <param name="severity">Severity classification for the event.</param>
		/// <seealso cref="Raise(SdcValidationEventArgs)"/>
		internal static void Raise(
			string message,
			string? nodeID = null,
			string? propertyName = null,
			object? attemptedValue = null,
			SdcValidationSeverity severity = SdcValidationSeverity.Error)
		{
			Raise(new SdcValidationEventArgs
			{
				Message       = message,
				NodeID        = nodeID,
				PropertyName  = propertyName,
				AttemptedValue = attemptedValue,
				Severity      = severity
			});
		}
	}
}
