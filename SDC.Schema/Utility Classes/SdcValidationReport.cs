// SDC-CUSTOM: do not overwrite
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SDC.Schema
{
	/// <summary>
	/// One validation issue detected on a single property of a single SDC node.
	/// Produced by <see cref="SdcUtil.ValidateAndRaise"/> and accumulated by
	/// <see cref="SdcValidationReport"/> during validating deserialization or explicit
	/// tree-sweep calls.
	/// </summary>
	public class SdcNodeValidationIssue
	{
		/// <summary>
		/// <see cref="BaseType.sGuid"/> of the node being validated, or <c>"(unknown)"</c>
		/// if the node context is unavailable.
		/// </summary>
		public string NodeID { get; init; } = "(unknown)";

		/// <summary>Short type name of the node (e.g. <c>"integer_DEtype"</c>).</summary>
		public string NodeType { get; init; } = "(unknown)";

		/// <summary>Name of the property whose value failed validation.</summary>
		public string? PropertyName { get; init; }

		/// <summary>The value that was rejected (may be null for non-setter events).</summary>
		public object? AttemptedValue { get; init; }

		/// <summary>
		/// Human-readable description of the issue; combines all
		/// <see cref="ValidationResult.ErrorMessage"/> values when multiple constraints fail.
		/// </summary>
		public string Message { get; init; } = string.Empty;

		/// <summary>Severity of the issue.</summary>
		public SdcValidationSeverity Severity { get; init; } = SdcValidationSeverity.Error;

		/// <summary>
		/// Raw DataAnnotations results when the issue originates from
		/// <see cref="Validator.TryValidateProperty"/> or <see cref="Validator.TryValidateObject"/>.
		/// Empty for issues raised from non-DataAnnotations code paths.
		/// </summary>
		public IReadOnlyList<ValidationResult> Results { get; init; } = System.Array.Empty<ValidationResult>();

		public override string ToString() =>
			$"[{Severity}] {NodeType}/{NodeID} .{PropertyName} = {AttemptedValue ?? "null"} — {Message}";
	}

	/// <summary>
	/// Aggregated collection of <see cref="SdcNodeValidationIssue"/> entries produced
	/// during a validating deserialization pass or an explicit
	/// <see cref="SdcValidate.ValidateTree"/> / <see cref="SdcValidate.ValidateNode"/> sweep.<br/>
	/// <br/>
	/// <b>IsValid</b> is <see langword="true"/> when there are no
	/// <see cref="SdcValidationSeverity.Error"/>-level issues.
	/// Warnings and Info items do not affect <see cref="IsValid"/>.
	/// </summary>
	public sealed class SdcValidationReport
	{
		private readonly List<SdcNodeValidationIssue> _issues = new();

		/// <summary>All issues recorded during the operation that produced this report.</summary>
		public IReadOnlyList<SdcNodeValidationIssue> Issues => _issues;

		/// <summary>
		/// <see langword="true"/> when no Error-severity issues were detected.
		/// Warnings and Info issues do not make the report invalid.
		/// </summary>
		public bool IsValid => _issues.All(i => i.Severity < SdcValidationSeverity.Error);

		/// <summary>Number of Error-severity issues.</summary>
		public int ErrorCount => _issues.Count(i => i.Severity == SdcValidationSeverity.Error);

		/// <summary>Number of Warning-severity issues.</summary>
		public int WarningCount => _issues.Count(i => i.Severity == SdcValidationSeverity.Warning);

		/// <summary>Number of Info-severity issues.</summary>
		public int InfoCount => _issues.Count(i => i.Severity == SdcValidationSeverity.Info);

		/// <summary>Total number of distinct SDC nodes that have at least one issue.</summary>
		public int AffectedNodeCount => _issues.Select(i => i.NodeID).Distinct().Count();

		/// <summary>One-line summary suitable for logging or exception messages.</summary>
		public string Summary =>
			$"{ErrorCount} error(s), {WarningCount} warning(s) across {AffectedNodeCount} node(s).";

		/// <summary>Appends an issue to this report. Called by <see cref="SdcUtil.ValidateAndRaise"/>.</summary>
		internal void Add(SdcNodeValidationIssue issue) => _issues.Add(issue);

		/// <summary>Merges all issues from <paramref name="other"/> into this report.</summary>
		public void MergeFrom(SdcValidationReport other)
		{
			foreach (var issue in other._issues)
				_issues.Add(issue);
		}
	}
}
