// SDC-CUSTOM: do not overwrite
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema
{
	/// <summary>
	/// Severity level for an SDC validation event.
	/// </summary>
	public enum SdcValidationSeverity
	{
		Info,
		Warning,
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
		/// <summary><see cref="BaseType.ID"/> of the node being validated, or null if unknown.</summary>
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
	/// Central, static event hub for SDC validation issues.<br/>
	/// Subscribe to <see cref="ValidationOccurred"/> to receive non-throwing validation
	/// notifications from any part of the SDC OM (property setters, node-builder helpers, etc.).<br/>
	/// Subscribers must be added before calling the code that may raise events.
	/// </summary>
	/// <remarks>
	/// Design goals:<br/>
	/// • Non-throwing — callers decide whether to abort, log, or surface errors in UI.<br/>
	/// • Lightweight — a single static event; no DI container required.<br/>
	/// • Deserialization-safe — callers should gate raises with
	///   <c>if (!SdcUtil.IsDeserializing.Value)</c> to avoid noise during round-trips.<br/>
	/// • Subscribable by UI view-models, loggers, and <see cref="SdcValidate"/> alike.
	/// </remarks>
	public static class SdcValidationEvents
	{
		/// <summary>
		/// Raised whenever an SDC validation issue is detected.
		/// The <c>sender</c> argument is always <see langword="null"/>
		/// (the event is module-level, not instance-level).
		/// </summary>
		public static event EventHandler<SdcValidationEventArgs>? ValidationOccurred;

		/// <summary>
		/// Raise a validation event.  Intended to be called by SDC OM internals;
		/// external callers may also use it to inject custom validation results.
		/// </summary>
		/// <param name="e">The validation event arguments.</param>
		internal static void Raise(SdcValidationEventArgs e) =>
			ValidationOccurred?.Invoke(null, e);

		/// <summary>
		/// Convenience overload: raise a simple error with a message and optional node/property context.
		/// </summary>
		/// <param name="message">Human-readable description of the issue.</param>
		/// <param name="nodeID">Optional: <see cref="BaseType.ID"/> of the affected node.</param>
		/// <param name="propertyName">Optional: name of the failing property.</param>
		/// <param name="attemptedValue">Optional: the value that was rejected.</param>
		/// <param name="severity">Severity level (defaults to <see cref="SdcValidationSeverity.Error"/>).</param>
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
