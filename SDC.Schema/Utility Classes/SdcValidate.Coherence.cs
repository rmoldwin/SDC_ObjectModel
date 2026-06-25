// SDC-CUSTOM: do not overwrite
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SDC.Schema
{
	public static partial class SdcValidate
	{
		// ──── Private reflection helpers ────────────────────────────────────────────

		// Retrieves a property value from obj and converts it to decimal.
		// Returns null if the property doesn't exist, is null, or cannot be converted.
		private static decimal? GetDecimalProp(object obj, string propName)
		{
			var pi = obj.GetType().GetProperty(propName);
			if (pi == null) return null;
			var v = pi.GetValue(obj);
			if (v == null) return null;
			try { return Convert.ToDecimal(v); } catch { return null; }
		}

		// Retrieves a property value from obj and converts it to ulong.
		// Returns null if the property doesn't exist, is null, or cannot be converted.
		private static ulong? GetUlongProp(object obj, string propName)
		{
			var pi = obj.GetType().GetProperty(propName);
			if (pi == null) return null;
			var v = pi.GetValue(obj);
			if (v == null) return null;
			try { return Convert.ToUInt64(v); } catch { return null; }
		}

		// Retrieves a string property value from obj.
		// Returns null if the property doesn't exist or the value is null.
		private static string? GetStringProp(object obj, string propName)
		{
			var pi = obj.GetType().GetProperty(propName);
			if (pi == null) return null;
			return pi.GetValue(obj) as string;
		}

		// Returns true if the constraint named propName has been explicitly set on obj.
		// Calls ShouldSerialize{propName}() when available; falls back to a non-default check.
		private static bool IsConstraintSet(object obj, string propName)
		{
			var method = obj.GetType().GetMethod(
				"ShouldSerialize" + propName,
				BindingFlags.Public | BindingFlags.Instance);
			if (method != null)
				return method.Invoke(obj, null) is true;

			// Fallback when no ShouldSerialize method: check null or value-type default.
			var pi = obj.GetType().GetProperty(propName);
			if (pi == null) return false;
			var val = pi.GetValue(obj);
			if (val == null) return false;
			var defaultVal = val.GetType().IsValueType ? Activator.CreateInstance(val.GetType()) : null;
			return !val.Equals(defaultVal);
		}

		// Attempts to convert value to decimal. Returns false for null or string values
		// (strings are handled via the string path) and for unconvertible objects.
		private static bool TryToDecimal(object? value, out decimal result)
		{
			result = 0m;
			if (value == null || value is string) return false;
			try { result = Convert.ToDecimal(value); return true; }
			catch { return false; }
		}

		// Attempts to convert value to ulong. Returns false for null or unconvertible objects.
		private static bool TryToUlong(object? value, out ulong result)
		{
			result = 0UL;
			if (value == null) return false;
			try { result = Convert.ToUInt64(value); return true; }
			catch { return false; }
		}

		// ──── Coherence violation reporter ──────────────────────────────────────────

		// Records the violation and fires the event/collector according to severity semantics.
		// Error → records rejected value, fires event (if not suppressed), returns false.
		// Warning → records rejected value, fires event (if not suppressed), returns true.
		private static bool RaiseCoherenceViolation(
			BaseType node,
			string memberName,
			object? attemptedValue,
			string message,
			SdcValidationSeverity severity = SdcValidationSeverity.Error)
		{
			var results = new List<ValidationResult>
			{
				new ValidationResult(message, new[] { memberName })
			};

			// Unconditionally record the offending value — mirrors SdcUtil.RaiseAndRecord.
			SdcUtil.RecordRejectedValue(node, new SdcRejectedValue
			{
				PropertyName   = memberName,
				AttemptedValue = attemptedValue,
				Message        = message,
				RejectedAt     = DateTimeOffset.Now,
				Results        = results.AsReadOnly()
			});

			// Gated: events and collector are suppressed during non-validating deserialization.
			if (!SdcUtil.SuppressValidation.Value)
			{
				string nodeID   = node?.sGuid ?? "(unknown)";
				string nodeType = node?.GetType().Name ?? "(unknown)";

				var issue = new SdcNodeValidationIssue
				{
					NodeID         = nodeID,
					NodeType       = nodeType,
					PropertyName   = memberName,
					AttemptedValue = attemptedValue,
					Message        = message,
					Severity       = severity,
					Results        = results.AsReadOnly()
				};

				SdcUtil.ValidationCollector.Value?.Add(issue);

				SdcValidationEvents.Raise(
					message,
					nodeID:        nodeID,
					propertyName:  memberName,
					attemptedValue: attemptedValue,
					severity:      severity);
			}

			// Error → false (reject the attempted value), Warning → true (accept with warning)
			return severity != SdcValidationSeverity.Error;
		}

		// ──── Public API ────────────────────────────────────────────────────────────

		/// <summary>
		/// Validates <paramref name="newVal"/> against all constraint facets currently set on
		/// <paramref name="node"/> (minInclusive, maxInclusive, minExclusive, maxExclusive for
		/// numeric types; pattern, minLength, maxLength for string types; totalDigits for decimal
		/// types). Returns <see langword="true"/> if <paramref name="newVal"/> is coherent with
		/// all constraints currently set on the node. On violation, fires
		/// <see cref="SdcValidationEvents.ValidationOccurred"/> (unless
		/// <see cref="SdcUtil.SuppressValidation"/> is <see langword="true"/>).
		/// Must be called AFTER property-level DataAnnotations validation succeeds.
		/// Is a no-op (returns <see langword="true"/>) when SuppressValidation is
		/// <see langword="true"/>.
		/// </summary>
		public static bool CheckValAgainstConstraints(BaseType node, string memberName, object? newVal)
		{
			if (SdcUtil.SuppressValidation.Value) return true;
			if (node == null || newVal == null) return true;

			bool allOk = true;

			// ── Numeric value path ───────────────────────────────────────────────────
			if (TryToDecimal(newVal, out decimal numVal))
			{
				if (IsConstraintSet(node, "minInclusive") &&
					GetDecimalProp(node, "minInclusive") is decimal minInc &&
					numVal < minInc)
				{
					allOk &= RaiseCoherenceViolation(node, memberName, newVal,
						$"Value {numVal} is below minInclusive ({minInc}).");
				}

				if (IsConstraintSet(node, "maxInclusive") &&
					GetDecimalProp(node, "maxInclusive") is decimal maxInc &&
					numVal > maxInc)
				{
					allOk &= RaiseCoherenceViolation(node, memberName, newVal,
						$"Value {numVal} exceeds maxInclusive ({maxInc}).");
				}

				if (IsConstraintSet(node, "minExclusive") &&
					GetDecimalProp(node, "minExclusive") is decimal minExc &&
					numVal <= minExc)
				{
					allOk &= RaiseCoherenceViolation(node, memberName, newVal,
						$"Value {numVal} must be strictly greater than minExclusive ({minExc}).");
				}

				if (IsConstraintSet(node, "maxExclusive") &&
					GetDecimalProp(node, "maxExclusive") is decimal maxExc &&
					numVal >= maxExc)
				{
					allOk &= RaiseCoherenceViolation(node, memberName, newVal,
						$"Value {numVal} must be strictly less than maxExclusive ({maxExc}).");
				}

				// totalDigits: count significant digits in the absolute value representation.
				if (IsConstraintSet(node, "totalDigits") &&
					GetUlongProp(node, "totalDigits") is ulong td && td > 0)
				{
					string digits = Math.Abs(numVal)
						.ToString("G", System.Globalization.CultureInfo.InvariantCulture)
						.Replace(".", "")
						.TrimStart('0');
					if (digits.Length == 0) digits = "0";
					if ((ulong)digits.Length > td)
					{
						allOk &= RaiseCoherenceViolation(node, memberName, newVal,
							$"Value {numVal} has {digits.Length} significant digit(s), exceeding totalDigits limit ({td}).");
					}
				}
			}
			// ── String value path ────────────────────────────────────────────────────
			else if (newVal is string strVal)
			{
				ulong strLen = (ulong)strVal.Length;

				if (IsConstraintSet(node, "minLength") &&
					GetUlongProp(node, "minLength") is ulong minLen &&
					strLen < minLen)
				{
					allOk &= RaiseCoherenceViolation(node, memberName, newVal,
						$"String length {strLen} is less than minLength ({minLen}).");
				}

				if (IsConstraintSet(node, "maxLength") &&
					GetUlongProp(node, "maxLength") is ulong maxLen &&
					strLen > maxLen)
				{
					allOk &= RaiseCoherenceViolation(node, memberName, newVal,
						$"String length {strLen} exceeds maxLength ({maxLen}).");
				}

				string? pattern = GetStringProp(node, "pattern");
				if (!string.IsNullOrEmpty(pattern))
				{
					try
					{
						if (!Regex.IsMatch(strVal, pattern))
						{
							allOk &= RaiseCoherenceViolation(node, memberName, newVal,
								$"Value does not match the required pattern '{pattern}'.");
						}
					}
					catch (ArgumentException)
					{
						// Invalid regex in the pattern property — warn rather than reject val.
						// allOk is not modified: an uncompilable pattern does not reject val.
						RaiseCoherenceViolation(node, memberName, newVal,
							$"The pattern '{pattern}' is not a valid regular expression; val coherence check skipped.",
							SdcValidationSeverity.Warning);
					}
				}
			}

			return allOk;
		}

		/// <summary>
		/// Validates that a proposed new constraint value is coherent with other currently-set
		/// constraints and the current <c>val</c>.<br/>
		/// Returns <see langword="false"/> (Error) for constraint-vs-constraint incoherence
		/// (e.g. minInclusive &gt; maxInclusive).<br/>
		/// Returns <see langword="true"/> with a Warning when the constraint is internally
		/// consistent but would invalidate the current <c>val</c> (val may be updated separately;
		/// rejecting the constraint would be too aggressive).<br/>
		/// Is a no-op (returns <see langword="true"/>) when
		/// <see cref="SdcUtil.SuppressValidation"/> is <see langword="true"/>.
		/// </summary>
		public static bool CheckConstraintCoherence(BaseType node, string constraintName, object? newConstraintValue)
		{
			if (SdcUtil.SuppressValidation.Value) return true;
			if (node == null) return true;

			// ── pattern constraint ───────────────────────────────────────────────────
			if (constraintName == "pattern")
			{
				string? patternStr = newConstraintValue as string;
				if (string.IsNullOrEmpty(patternStr)) return true;

				// Constraint-vs-constraint: pattern must be a valid compilable regex.
				try { _ = new Regex(patternStr); }
				catch (ArgumentException ex)
				{
					return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
						$"Pattern '{patternStr}' is not a valid regular expression: {ex.Message}");
				}

				// Constraint-vs-val: warn if the current string val does not match the new pattern.
				string? curVal = GetStringProp(node, "val");
				if (curVal != null && IsConstraintSet(node, "val"))
				{
					try
					{
						if (!Regex.IsMatch(curVal, patternStr))
						{
							RaiseCoherenceViolation(node, "val", curVal,
								$"Current val '{curVal}' does not match the new pattern '{patternStr}'.",
								SdcValidationSeverity.Warning);
						}
					}
					catch { /* pattern already validated above, so this shouldn't happen */ }
				}
				return true;
			}

			// ── minLength / maxLength constraints ────────────────────────────────────
			if (constraintName == "minLength" || constraintName == "maxLength")
			{
				if (!TryToUlong(newConstraintValue, out ulong newLenVal))
					return true;

				if (constraintName == "minLength" &&
					IsConstraintSet(node, "maxLength") &&
					GetUlongProp(node, "maxLength") is ulong curMaxLen &&
					newLenVal > curMaxLen)
				{
					return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
						$"minLength ({newLenVal}) would exceed current maxLength ({curMaxLen}).");
				}

				if (constraintName == "maxLength" &&
					IsConstraintSet(node, "minLength") &&
					GetUlongProp(node, "minLength") is ulong curMinLen &&
					newLenVal < curMinLen)
				{
					return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
						$"maxLength ({newLenVal}) would be less than current minLength ({curMinLen}).");
				}

				// Constraint-vs-val: warn if the current val length would violate the new constraint.
				string? curValStr = GetStringProp(node, "val");
				if (curValStr != null && IsConstraintSet(node, "val"))
				{
					ulong curLen = (ulong)curValStr.Length;
					if (constraintName == "minLength" && curLen < newLenVal)
					{
						RaiseCoherenceViolation(node, "val", curValStr,
							$"Current val length {curLen} would be below the new minLength ({newLenVal}).",
							SdcValidationSeverity.Warning);
					}
					else if (constraintName == "maxLength" && curLen > newLenVal)
					{
						RaiseCoherenceViolation(node, "val", curValStr,
							$"Current val length {curLen} would exceed the new maxLength ({newLenVal}).",
							SdcValidationSeverity.Warning);
					}
				}
				return true;
			}

			// ── Numeric constraints (minInclusive, maxInclusive, minExclusive, maxExclusive) ──
			if (!TryToDecimal(newConstraintValue, out decimal newNumVal))
				return true; // cannot convert — no coherence rules applicable

			switch (constraintName)
			{
				case "minInclusive":
				{
					// Constraint-vs-constraint: minInclusive must be ≤ maxInclusive.
					if (IsConstraintSet(node, "maxInclusive") &&
						GetDecimalProp(node, "maxInclusive") is decimal curMaxInc &&
						newNumVal > curMaxInc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"minInclusive ({newNumVal}) would exceed current maxInclusive ({curMaxInc}).");
					}

					// Constraint-vs-constraint: minInclusive must be < maxExclusive.
					if (IsConstraintSet(node, "maxExclusive") &&
						GetDecimalProp(node, "maxExclusive") is decimal curMaxExc &&
						newNumVal >= curMaxExc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"minInclusive ({newNumVal}) must be strictly less than current maxExclusive ({curMaxExc}).");
					}

					// Constraint-vs-val: warn if the current val would be below the new minInclusive.
					if (IsConstraintSet(node, "val") &&
						GetDecimalProp(node, "val") is decimal curVal &&
						curVal < newNumVal)
					{
						RaiseCoherenceViolation(node, "val", curVal,
							$"Current val ({curVal}) is below the new minInclusive ({newNumVal}).",
							SdcValidationSeverity.Warning);
					}
					return true;
				}

				case "maxInclusive":
				{
					// Constraint-vs-constraint: maxInclusive must be ≥ minInclusive.
					if (IsConstraintSet(node, "minInclusive") &&
						GetDecimalProp(node, "minInclusive") is decimal curMinInc &&
						newNumVal < curMinInc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"maxInclusive ({newNumVal}) would be less than current minInclusive ({curMinInc}).");
					}

					// Constraint-vs-constraint: maxInclusive must be > minExclusive.
					if (IsConstraintSet(node, "minExclusive") &&
						GetDecimalProp(node, "minExclusive") is decimal curMinExc &&
						newNumVal <= curMinExc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"maxInclusive ({newNumVal}) must be strictly greater than current minExclusive ({curMinExc}).");
					}

					// Constraint-vs-val: warn if the current val would exceed the new maxInclusive.
					if (IsConstraintSet(node, "val") &&
						GetDecimalProp(node, "val") is decimal curVal &&
						curVal > newNumVal)
					{
						RaiseCoherenceViolation(node, "val", curVal,
							$"Current val ({curVal}) exceeds the new maxInclusive ({newNumVal}).",
							SdcValidationSeverity.Warning);
					}
					return true;
				}

				case "minExclusive":
				{
					// Constraint-vs-constraint: minExclusive must be < maxExclusive.
					if (IsConstraintSet(node, "maxExclusive") &&
						GetDecimalProp(node, "maxExclusive") is decimal curMaxExc &&
						newNumVal >= curMaxExc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"minExclusive ({newNumVal}) must be strictly less than current maxExclusive ({curMaxExc}).");
					}

					// Constraint-vs-constraint: minExclusive must be < maxInclusive.
					if (IsConstraintSet(node, "maxInclusive") &&
						GetDecimalProp(node, "maxInclusive") is decimal curMaxInc &&
						newNumVal >= curMaxInc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"minExclusive ({newNumVal}) must be strictly less than current maxInclusive ({curMaxInc}).");
					}

					// Constraint-vs-val: warn if the current val would be at or below the new minExclusive.
					if (IsConstraintSet(node, "val") &&
						GetDecimalProp(node, "val") is decimal curVal &&
						curVal <= newNumVal)
					{
						RaiseCoherenceViolation(node, "val", curVal,
							$"Current val ({curVal}) is not strictly greater than the new minExclusive ({newNumVal}).",
							SdcValidationSeverity.Warning);
					}
					return true;
				}

				case "maxExclusive":
				{
					// Constraint-vs-constraint: maxExclusive must be > minExclusive.
					if (IsConstraintSet(node, "minExclusive") &&
						GetDecimalProp(node, "minExclusive") is decimal curMinExc &&
						newNumVal <= curMinExc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"maxExclusive ({newNumVal}) must be strictly greater than current minExclusive ({curMinExc}).");
					}

					// Constraint-vs-constraint: maxExclusive must be > minInclusive.
					if (IsConstraintSet(node, "minInclusive") &&
						GetDecimalProp(node, "minInclusive") is decimal curMinInc &&
						newNumVal <= curMinInc)
					{
						return RaiseCoherenceViolation(node, constraintName, newConstraintValue,
							$"maxExclusive ({newNumVal}) must be strictly greater than current minInclusive ({curMinInc}).");
					}

					// Constraint-vs-val: warn if the current val would be at or above the new maxExclusive.
					if (IsConstraintSet(node, "val") &&
						GetDecimalProp(node, "val") is decimal curVal &&
						curVal >= newNumVal)
					{
						RaiseCoherenceViolation(node, "val", curVal,
							$"Current val ({curVal}) is not strictly less than the new maxExclusive ({newNumVal}).",
							SdcValidationSeverity.Warning);
					}
					return true;
				}
			}

			return true; // Unknown numeric constraint — no coherence rules apply
		}
	}
}
