// SDC-CUSTOM: do not overwrite
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SDC.Schema
{
	/// <summary>
	/// Extension methods on <see cref="BaseType"/> that provide a safe, expression-based API for
	/// pre-checking and attempting property assignments without duplicating the validation logic
	/// embedded in the generated setters.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class lives in the <c>SDC.Schema</c> namespace — the same namespace as
	/// <see cref="BaseType"/> — so callers get both methods automatically without an extra
	/// <c>using</c> directive.
	/// </para>
	/// <para>
	/// The two entry points have distinct contracts:
	/// <list type="bullet">
	///   <item><description>
	///     <see cref="WouldBeValid{TNode,TVal}(TNode, Expression{Func{TNode,TVal}}, TVal)"/>
	///     is a <b>pure predicate</b> — it runs DataAnnotations validation but has
	///     <b>zero side effects</b>: no event is fired, no rejection is recorded, and the
	///     <see cref="SdcUtil.ValidationCollector"/> is untouched.
	///   </description></item>
	///   <item><description>
	///     <see cref="TryAssignValue{TNode,TVal}(TNode, Expression{Func{TNode,TVal}}, TVal, out SdcRejectedValue)"/>
	///     <b>actually assigns</b> through the real property setter, which fires
	///     <see cref="SdcValidationEvents.ValidationOccurred"/>, records a rejection on failure,
	///     calls <c>CheckValAgainstConstraints</c>, and calls <c>CheckConstraintCoherence</c> —
	///     exactly as direct C# assignment would.
	///   </description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public static class SdcValidationExtensions
	{
		// ──── WouldBeValid ────────────────────────────────────────────────────────────

		/// <summary>
		/// Returns <see langword="true"/> when <paramref name="value"/> would pass DataAnnotations
		/// validation for the selected property on <paramref name="node"/>, without modifying any
		/// state.
		/// </summary>
		/// <typeparam name="TNode">The concrete <see cref="BaseType"/> subtype that owns the property.</typeparam>
		/// <typeparam name="TVal">The property's CLR type.</typeparam>
		/// <param name="node">The node instance that owns the property.</param>
		/// <param name="property">
		/// A simple property-selector expression, e.g. <c>d =&gt; d.minInclusive</c>.
		/// Chained or computed expressions are not supported and will throw.
		/// </param>
		/// <param name="value">The candidate value to check.</param>
		/// <returns>
		/// <see langword="true"/> when the value satisfies all DataAnnotations constraints declared
		/// on (or registered for) the property; <see langword="false"/> when any constraint fails.
		/// </returns>
		/// <remarks>
		/// This overload has <b>zero side effects</b>: no <see cref="SdcValidationEvents.ValidationOccurred"/>
		/// event, no <see cref="SdcUtil.GetRejectedValues"/> entry, no
		/// <see cref="SdcUtil.ValidationCollector"/> activity.
		/// <para>
		/// Honors any <see cref="SdcValidationRuleRegistry"/> override for the property, mirroring
		/// the exact rule-lookup used by <see cref="SdcUtil.ValidateAndRaise"/>.
		/// </para>
		/// <code>
		/// var node = /* decimal_DEtype */;
		/// if (node.WouldBeValid(d => d.minInclusive, 5m))
		///     node.minInclusive = 5m;
		/// </code>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="property"/> is not a direct property-selector expression
		/// (e.g. it is a chained path or a constant).
		/// </exception>
		public static bool WouldBeValid<TNode, TVal>(
			this TNode node,
			Expression<Func<TNode, TVal>> property,
			TVal value)
			where TNode : BaseType
			=> WouldBeValid(node, property, value, out _);

		/// <summary>
		/// Returns <see langword="true"/> when <paramref name="value"/> would pass DataAnnotations
		/// validation for the selected property on <paramref name="node"/>, without modifying any
		/// state. Also returns a human-readable failure message.
		/// </summary>
		/// <typeparam name="TNode">The concrete <see cref="BaseType"/> subtype that owns the property.</typeparam>
		/// <typeparam name="TVal">The property's CLR type.</typeparam>
		/// <param name="node">The node instance that owns the property.</param>
		/// <param name="property">
		/// A simple property-selector expression, e.g. <c>d =&gt; d.minInclusive</c>.
		/// </param>
		/// <param name="value">The candidate value to check.</param>
		/// <param name="message">
		/// When the method returns <see langword="false"/>, contains a human-readable summary of
		/// the validation failures (multiple messages joined with <c>"; "</c>).
		/// <see langword="null"/> when the value is valid.
		/// </param>
		/// <returns>
		/// <see langword="true"/> when the value satisfies all DataAnnotations constraints;
		/// <see langword="false"/> otherwise.
		/// </returns>
		/// <remarks>
		/// This overload has <b>zero side effects</b>: no event, no rejection store update,
		/// no collector activity.
		/// <para>
		/// Honors any <see cref="SdcValidationRuleRegistry"/> override for the property.
		/// </para>
		/// <code>
		/// if (!node.WouldBeValid(d => d.val, -1m, out string? msg))
		///     Console.WriteLine($"Rejected: {msg}");
		/// </code>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="property"/> is not a direct property-selector expression.
		/// </exception>
		public static bool WouldBeValid<TNode, TVal>(
			this TNode node,
			Expression<Func<TNode, TVal>> property,
			TVal value,
			out string? message)
			where TNode : BaseType
		{
			if (node is null) throw new ArgumentNullException(nameof(node));
			if (property is null) throw new ArgumentNullException(nameof(property));

			string memberName = GetMemberName(property);
			var ctx = new ValidationContext(node) { MemberName = memberName };
			var results = new List<ValidationResult>();

			bool ok;
			if (SdcValidationRuleRegistry.TryGet(node.GetType(), memberName, out var registered))
				ok = Validator.TryValidateValue(value!, ctx, results, registered);
			else
				ok = Validator.TryValidateProperty(value, ctx, results);

			message = ok ? null : string.Join("; ", results.Select(r => r.ErrorMessage ?? "(validation error)"));
			return ok;
		}

		// ──── TryAssignValue ──────────────────────────────────────────────────────────

		/// <summary>
		/// Attempts to assign <paramref name="value"/> to the selected property on
		/// <paramref name="node"/> by invoking the real property setter via reflection.
		/// Returns <see langword="true"/> when the assignment succeeds (no rejection recorded).
		/// </summary>
		/// <typeparam name="TNode">The concrete <see cref="BaseType"/> subtype that owns the property.</typeparam>
		/// <typeparam name="TVal">The property's CLR type.</typeparam>
		/// <param name="node">The node instance to mutate.</param>
		/// <param name="property">
		/// A simple property-selector expression, e.g. <c>d =&gt; d.val</c>.
		/// </param>
		/// <param name="value">The value to assign.</param>
		/// <returns>
		/// <see langword="true"/> when the setter accepted the value (no rejection was recorded);
		/// <see langword="false"/> when the setter rejected the value or a type-mismatch prevented
		/// the call.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The assignment goes through the real setter, so all side effects that direct
		/// C# assignment would produce are preserved:
		/// <see cref="SdcValidationEvents.ValidationOccurred"/>, rejection recording,
		/// <c>CheckValAgainstConstraints</c>, and <c>CheckConstraintCoherence</c>.
		/// </para>
		/// <para>
		/// Any stale rejection for the property is cleared before the attempt so the rejection
		/// store reflects only this call's outcome.
		/// </para>
		/// <para>
		/// A CLR type-mismatch (e.g., passing an <c>int</c> to a <c>decimal</c> property) is
		/// caught and reported as a soft rejection — no exception is propagated to the caller.
		/// </para>
		/// <code>
		/// bool ok = node.TryAssignValue(d => d.val, 42m);
		/// if (!ok)
		///     Console.WriteLine("Assignment failed.");
		/// </code>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="property"/> is not a direct property-selector expression,
		/// or when the named property does not exist on <paramref name="node"/>'s type.
		/// </exception>
		public static bool TryAssignValue<TNode, TVal>(
			this TNode node,
			Expression<Func<TNode, TVal>> property,
			TVal value)
			where TNode : BaseType
			=> TryAssignValue(node, property, value, out _);

		/// <summary>
		/// Attempts to assign <paramref name="value"/> to the selected property on
		/// <paramref name="node"/> by invoking the real property setter via reflection.
		/// Also returns the <see cref="SdcRejectedValue"/> recorded by the setter on failure.
		/// </summary>
		/// <typeparam name="TNode">The concrete <see cref="BaseType"/> subtype that owns the property.</typeparam>
		/// <typeparam name="TVal">The property's CLR type.</typeparam>
		/// <param name="node">The node instance to mutate.</param>
		/// <param name="property">
		/// A simple property-selector expression, e.g. <c>d =&gt; d.val</c>.
		/// </param>
		/// <param name="value">The value to assign.</param>
		/// <param name="rejection">
		/// When the method returns <see langword="false"/>, contains the <see cref="SdcRejectedValue"/>
		/// that the setter recorded; <see langword="null"/> on success.
		/// </param>
		/// <returns>
		/// <see langword="true"/> when the setter accepted the value; <see langword="false"/>
		/// otherwise.
		/// </returns>
		/// <remarks>
		/// <para>
		/// All setter side effects are preserved: <see cref="SdcValidationEvents.ValidationOccurred"/>,
		/// rejection recording, <c>CheckValAgainstConstraints</c>, <c>CheckConstraintCoherence</c>.
		/// </para>
		/// <para>
		/// Any stale rejection for the property is cleared before the attempt.
		/// </para>
		/// <para>
		/// A CLR type-mismatch (e.g., passing an <c>int</c> to a <c>decimal</c> property) is
		/// caught and reported as a soft rejection without propagating an exception.
		/// </para>
		/// <code>
		/// if (!node.TryAssignValue(d => d.val, badValue, out SdcRejectedValue? rej))
		///     Console.WriteLine(rej!.Message);
		/// </code>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="property"/> is not a direct property-selector expression,
		/// or when the named property does not exist on <paramref name="node"/>'s type.
		/// </exception>
		public static bool TryAssignValue<TNode, TVal>(
			this TNode node,
			Expression<Func<TNode, TVal>> property,
			TVal value,
			out SdcRejectedValue? rejection)
			where TNode : BaseType
		{
			if (node is null) throw new ArgumentNullException(nameof(node));
			if (property is null) throw new ArgumentNullException(nameof(property));

			string memberName = GetMemberName(property);

			// Verify the property exists on the node's runtime type.
			var pi = node.GetType().GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
			if (pi is null)
				throw new ArgumentException(
					$"Property '{memberName}' was not found on type '{node.GetType().Name}'.",
					nameof(property));

			// Clear any stale rejection so the store reflects only this call's outcome.
			SdcUtil.ClearRejectedValue(node, memberName);

			try
			{
				pi.SetValue(node, value);
			}
			catch (Exception ex) when (ex is TargetInvocationException || ex is ArgumentException || ex is InvalidCastException)
			{
				// Unwrap TargetInvocationException so the inner cause is accessible.
				var inner = ex is TargetInvocationException tie ? tie.InnerException ?? ex : ex;
				string msg = $"Type mismatch or invalid value for '{memberName}': {inner.Message}";
				rejection = new SdcRejectedValue
				{
					PropertyName   = memberName,
					AttemptedValue = value,
					Message        = msg,
					RejectedAt     = DateTimeOffset.Now,
					Results        = Array.Empty<ValidationResult>()
				};
				return false;
			}

			// Determine success by checking whether the value was actually stored.
			// Rationale: the DataAnnotations soft-reject path does NOT store the value, so
			// the backing field retains the prior value when rejected. A coherence violation,
			// by contrast, runs AFTER the value is stored, so the backing field holds 'value'.
			// We therefore read the property back and compare rather than checking the rejection
			// store (which can hold coherence entries even for a successfully stored value).
			object? storedValue = pi.GetValue(node);
			bool valueWasStored = object.Equals(storedValue, (object?)value);

			if (!valueWasStored)
			{
				// DataAnnotations rejected the value — retrieve whatever rejection the setter recorded.
				var recorded = SdcUtil.GetRejectedValues(node);
				rejection = recorded.TryGetValue(memberName, out var entry) ? entry : null;
				return false;
			}

			rejection = null;
			return true;
		}

		// ──── Private helpers ─────────────────────────────────────────────────────────

		/// <summary>
		/// Extracts the simple member name from a direct property-selector lambda.
		/// Handles both plain <see cref="MemberExpression"/> and
		/// <see cref="UnaryExpression"/> (boxing of value types).
		/// </summary>
		private static string GetMemberName<TNode, TVal>(Expression<Func<TNode, TVal>> expr)
		{
			// UnaryExpression wraps value types when the return type is declared as object/TVal.
			var body = expr.Body is UnaryExpression u ? u.Operand : expr.Body;
			if (body is MemberExpression m && m.Expression is ParameterExpression)
				return m.Member.Name;
			throw new ArgumentException(
				$"Expression must be a simple property selector (e.g. d => d.minInclusive), not '{expr}'.",
				nameof(expr));
		}
	}
}
