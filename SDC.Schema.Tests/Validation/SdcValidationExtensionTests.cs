// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace SDC.Schema.Tests.Validation
{
	/// <summary>
	/// Unit tests for <see cref="SdcValidationExtensions"/> — the Phase 9
	/// <c>TryAssignValue</c> and <c>WouldBeValid</c> extension methods on
	/// <see cref="BaseType"/>.
	///
	/// Covers all 17 required scenarios:
	///   WouldBeValid: pure predicate (1–4)
	///   TryAssignValue: real-setter assignment (5–10)
	///   Bad expression form: ArgumentException (11–12)
	///   Type mismatch: soft reject (13)
	///   RuleRegistry integration (14)
	///   Constraint coherence (15–16)
	///   SuppressValidation behaviour (17)
	/// </summary>
	[TestClass]
	public class SdcValidationExtensionTests
	{
		private List<SdcValidationEventArgs> _captured = new();
		private EventHandler<SdcValidationEventArgs> _handler = null!;

		[TestInitialize]
		public void Init()
		{
			BaseType.ResetLastTopNode();
			_captured = new List<SdcValidationEventArgs>();
			_handler = (_, e) => _captured.Add(e);
			SdcValidationEvents.ValidationOccurred += _handler;
			SdcUtil.SuppressValidation.Value  = false;
			SdcUtil.ValidationCollector.Value = null;
		}

		[TestCleanup]
		public void Cleanup()
		{
			SdcValidationEvents.ValidationOccurred -= _handler;
			_captured.Clear();
			SdcUtil.SuppressValidation.Value  = false;
			SdcUtil.ValidationCollector.Value = null;
			// Neutralize any registry override registered for decimal_DEtype.mask in test 14.
			SdcValidationRuleRegistry.Register(typeof(decimal_DEtype), "mask"
				/* no attributes → neutralize */);
			BaseType.ResetLastTopNode();
		}

		// ──── Node creation helpers ──────────────────────────────────────────────────

		/// <summary>
		/// Creates a fresh <see cref="integer_DEtype"/> inside a minimal form tree.
		/// <c>integer_DEtype.minInclusive</c> is annotated with
		/// <c>[FractionDigitsAttribute(0)]</c>, <c>[MaxDigitsAttribute(29)]</c>, and
		/// <c>[RangeAttribute(...)]</c>, making it ideal for constraint-violation tests.
		/// </summary>
		private integer_DEtype CreateIntegerNode()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.V9.Test");
			fd.AddBody();
			fd.Body.AddChildQuestionResponse("Q.V9.Test", out var deType, "Test question", dt: ItemChoiceType.integer);
			_captured.Clear(); // discard construction events
			return (integer_DEtype)deType.DataTypeDE_Item!;
		}

		/// <summary>
		/// Creates a fresh <see cref="decimal_DEtype"/> inside a minimal form tree.
		/// <c>decimal_DEtype.minInclusive</c> has no physical DataAnnotations, so it
		/// accepts any decimal value. Used for constraint-coherence and constraint-property tests.
		/// </summary>
		private decimal_DEtype CreateDecimalNode()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.V9.Dec");
			fd.AddBody();
			fd.Body.AddChildQuestionResponse("Q.V9.Dec", out var deType, "Decimal question", dt: ItemChoiceType.@decimal);
			_captured.Clear();
			return (decimal_DEtype)deType.DataTypeDE_Item!;
		}

		// ──── 1. WouldBeValid — valid value returns true, message is null ────────────

		[TestMethod]
		public void WouldBeValid_ValidConstraintValue_ReturnsTrue_MessageIsNull()
		{
			// Rationale: a value that satisfies all DataAnnotations (no fractional digits,
			// within range) must report true and produce no failure message.
			var node = CreateIntegerNode();

			bool result = node.WouldBeValid(d => d.minInclusive, 100m, out string? msg);

			// True → value is valid; no failure message should be produced.
			Assert.IsTrue(result, "100m has no fractional digits and is within range — must report valid.");
			Assert.IsNull(msg, "No failure message should be produced for a valid value.");
		}

		// ──── 2. WouldBeValid — invalid value (violates [FractionDigits]) ────────────

		[TestMethod]
		public void WouldBeValid_InvalidValue_ViolatesFractionDigits_ReturnsFalse_MessageNonEmpty()
		{
			// Rationale: integer_DEtype.minInclusive has [FractionDigitsAttribute(0)],
			// so 1.5m is invalid. WouldBeValid must return false and provide a non-empty
			// message naming the constraint violation so callers can surface it to users.
			var node = CreateIntegerNode();

			bool result = node.WouldBeValid(d => d.minInclusive, 1.5m, out string? msg);

			// False → value violates FractionDigits(0).
			Assert.IsFalse(result, "1.5m has fractional digits — must be rejected by FractionDigitsAttribute(0).");
			// Message must be non-empty so the UI can explain the problem.
			Assert.IsNotNull(msg, "A non-null failure message must be returned for an invalid value.");
			Assert.IsTrue(msg!.Length > 0, "Failure message must not be empty.");
		}

		// ──── 3. WouldBeValid — rejection store unchanged after call ─────────────────

		[TestMethod]
		public void WouldBeValid_RejectionStoreUnchanged_AfterInvalidCheck()
		{
			// Rationale: WouldBeValid must have ZERO side effects — specifically, it must
			// never write to the rejection store even when the value fails validation.
			// The rejection store is the authoritative record of what the setter rejected.
			var node = CreateIntegerNode();

			// Pre-condition: node starts with no rejected values.
			Assert.AreEqual(0, node.RejectedValues.Count,
				"Pre-condition: rejection store must be empty before the call.");

			// Call WouldBeValid with an invalid value.
			node.WouldBeValid(d => d.minInclusive, 1.5m, out _);

			// Post-condition: rejection store must remain empty.
			Assert.AreEqual(0, node.RejectedValues.Count,
				"WouldBeValid must not write to the rejection store — it is a pure predicate.");
		}

		// ──── 4. WouldBeValid — ValidationOccurred event NOT fired ───────────────────

		[TestMethod]
		public void WouldBeValid_InvalidValue_ValidationOccurredNotFired()
		{
			// Rationale: WouldBeValid is a pure predicate — it must not fire the
			// ValidationOccurred event under any circumstances. Firing it would be an
			// observable side effect that could confuse UI error handlers.
			var node = CreateIntegerNode();

			node.WouldBeValid(d => d.minInclusive, 1.5m, out _);

			// Event list must remain empty; no ValidationOccurred should have been raised.
			Assert.AreEqual(0, _captured.Count,
				"WouldBeValid must never raise ValidationOccurred — it has zero side effects.");
		}

		// ──── 5. TryAssignValue — valid value returns true, property is updated ──────

		[TestMethod]
		public void TryAssignValue_ValidValue_ReturnsTrue_PropertyUpdated()
		{
			// Rationale: when the candidate value is valid, TryAssignValue must return true
			// and the property setter must have stored the new value — verifying that the
			// real setter path was actually invoked, not bypassed.
			var node = CreateIntegerNode();

			bool ok = node.TryAssignValue(d => d.minInclusive, 100m, out SdcRejectedValue? rej);

			// Return value must indicate success.
			Assert.IsTrue(ok, "Valid value 100m must be accepted and return true.");
			// No rejection should have been recorded.
			Assert.IsNull(rej, "Rejection detail must be null on success.");
			// The setter must have actually stored the value.
			Assert.AreEqual(100m, node.minInclusive,
				"The property must hold the newly assigned value after a successful TryAssignValue.");
		}

		// ──── 6. TryAssignValue — invalid value: false, unchanged, rejection populated

		[TestMethod]
		public void TryAssignValue_InvalidValue_ReturnsFalse_PropertyUnchanged_RejectionPopulated()
		{
			// Rationale: when the value violates DataAnnotations, TryAssignValue must return
			// false, the property must retain its prior value (soft-reject contract), and the
			// rejection detail must carry the offending value and a descriptive message.
			var node = CreateIntegerNode();
			decimal priorValue = node.minInclusive; // record current value before attempt

			bool ok = node.TryAssignValue(d => d.minInclusive, 1.5m, out SdcRejectedValue? rej);

			// Return value must indicate failure.
			Assert.IsFalse(ok, "Invalid value 1.5m (violates FractionDigits) must return false.");
			// The property must retain its prior value (soft-reject contract).
			Assert.AreEqual(priorValue, node.minInclusive,
				"Soft-reject: invalid value must not overwrite the stored property value.");
			// Rejection detail must be populated.
			Assert.IsNotNull(rej, "A SdcRejectedValue must be returned when the assignment fails.");
			// The rejection detail must record the offending value.
			Assert.AreEqual(1.5m, rej!.AttemptedValue,
				"Rejection must record the attempted value (1.5m) for user-facing error display.");
			// Message must be non-empty so the UI can explain the failure.
			Assert.IsTrue(rej.Message.Length > 0,
				"Rejection message must be non-empty.");
		}

		// ──── 7. TryAssignValue — clears stale prior rejection before attempt ────────

		[TestMethod]
		public void TryAssignValue_ClearsStaleRejection_BeforeAttempt()
		{
			// Rationale: TryAssignValue always clears the prior rejection for the same
			// property before calling the setter. This guarantees that after the call the
			// rejection store reflects ONLY this attempt — preventing a stale rejection from
			// a previous call from masking the outcome of the current attempt.
			var node = CreateIntegerNode();

			// Plant a stale rejection by attempting an invalid assignment directly.
			node.minInclusive = 1.5m; // rejected by setter; stale entry now in store
			Assert.IsTrue(node.RejectedValues.ContainsKey("minInclusive"),
				"Pre-condition: stale rejection must be present before TryAssignValue.");

			// Now call TryAssignValue with the SAME invalid value. Because we cleared the
			// stale entry first, the resulting rejection is freshly recorded by this call.
			bool ok = node.TryAssignValue(d => d.minInclusive, 1.5m, out SdcRejectedValue? rej);

			// The assignment still fails for the same reason.
			Assert.IsFalse(ok, "1.5m must still fail after clearing stale rejection.");
			// A new rejection must be present for the property.
			Assert.IsNotNull(rej, "New rejection must be populated after clearing stale entry.");
		}

		// ──── 8. TryAssignValue — valid after prior invalid clears rejection store ────

		[TestMethod]
		public void TryAssignValue_ValidAfterInvalid_RejectionCleared()
		{
			// Rationale: after a prior invalid attempt leaves a rejection, a subsequent
			// TryAssignValue with a valid value must succeed (return true) and the rejection
			// store entry for the property must be gone — reflecting the successful setter
			// call which internally calls ClearRejectedValue on success.
			var node = CreateIntegerNode();

			// First, plant a rejection via an invalid TryAssignValue.
			node.TryAssignValue(d => d.minInclusive, 1.5m, out _);
			Assert.IsTrue(node.RejectedValues.ContainsKey("minInclusive"),
				"Pre-condition: rejection entry must exist after the invalid attempt.");

			// Now attempt a valid assignment.
			bool ok = node.TryAssignValue(d => d.minInclusive, 50m, out SdcRejectedValue? rej);

			// Valid assignment must succeed.
			Assert.IsTrue(ok, "50m is valid (no fractional digits, within range) — must return true.");
			Assert.IsNull(rej, "No rejection detail on success.");
			// Setter internally calls ClearRejectedValue on success, so the store is empty.
			Assert.IsFalse(node.RejectedValues.ContainsKey("minInclusive"),
				"Rejection store must be cleared after a successful assignment.");
		}

		// ──── 9. TryAssignValue — ValidationOccurred event DOES fire on failure ──────

		[TestMethod]
		public void TryAssignValue_InvalidValue_ValidationOccurredFired()
		{
			// Rationale: TryAssignValue invokes the real setter, which routes through
			// SdcUtil.ValidateAndRaise. On failure, ValidateAndRaise fires
			// ValidationOccurred so subscribers (UI loggers, test fixtures) are notified
			// of the invalid attempt — the same behavior as a direct setter assignment.
			var node = CreateIntegerNode();

			node.TryAssignValue(d => d.minInclusive, 1.5m, out _);

			// At least one event must have fired for the failed assignment.
			Assert.IsTrue(_captured.Count > 0,
				"ValidationOccurred must fire when TryAssignValue causes a validation failure.");
			// The event must identify the property that was rejected.
			Assert.AreEqual("minInclusive", _captured[0].PropertyName,
				"Event must name the property whose assignment was rejected.");
		}

		// ──── 10. TryAssignValue — ValidationOccurred NOT fired on success ───────────

		[TestMethod]
		public void TryAssignValue_ValidValue_ValidationOccurredNotFired()
		{
			// Rationale: a successful assignment must not fire ValidationOccurred — doing
			// so would flood subscribers with false-positive error notifications every time
			// a normal, valid setter call is made.
			var node = CreateIntegerNode();

			node.TryAssignValue(d => d.minInclusive, 50m, out _);

			// No event should be captured for a valid assignment.
			Assert.AreEqual(0, _captured.Count,
				"ValidationOccurred must not fire for a valid TryAssignValue assignment.");
		}

		// ──── 11. Chained expression → ArgumentException ──────────────────────────────

		[TestMethod]
		public void WouldBeValid_ChainedMemberExpression_ThrowsArgumentException()
		{
			// Rationale: accepting a chained path (d.TopNode.order) would be ambiguous
			// because the node instance in scope is 'd', not 'd.TopNode'. The method must
			// reject non-direct selectors at call time rather than silently using the wrong
			// property or instance.
			var node = CreateIntegerNode();

			// d.ParentNode.order is a chained path: d → ParentNode (MemberExpression) → order
			// The inner expression is MemberExpression(d.ParentNode), not ParameterExpression.
			Assert.Throws<ArgumentException>(
				() => node.WouldBeValid(d => d.ParentNode!.order, 5m),
				"A chained member access must throw ArgumentException — only direct selectors are supported.");
		}

		// ──── 12. Non-member expression → ArgumentException ──────────────────────────

		[TestMethod]
		public void TryAssignValue_NonMemberExpression_ThrowsArgumentException()
		{
			// Rationale: a constant expression like d => 42m does not name any property
			// on the node. Accepting it would silently do nothing, so it must be rejected
			// with ArgumentException to surface the programming error immediately.
			var node = CreateIntegerNode();

			// d => 42m is a ConstantExpression — no member is selected.
			Assert.Throws<ArgumentException>(
				() => node.TryAssignValue<integer_DEtype, decimal>(d => 42m, 99m),
				"A constant expression must throw ArgumentException — it names no member.");
		}

		// ──── 13. Type mismatch → soft reject, no throw ───────────────────────────────

		[TestMethod]
		public void TryAssignValue_TypeMismatch_SoftReject_NoThrow()
		{
			// Rationale: a string value assigned to a decimal property via reflection
			// causes SetValue to throw ArgumentException. TryAssignValue must catch this
			// and return a synthetic SdcRejectedValue rather than letting the exception
			// propagate — keeping the caller's try/catch burden zero.
			var node = CreateIntegerNode();

			// TVal=object: pass a string where a decimal is expected.
			// The boxing UnaryExpression (object)d.minInclusive is correctly handled by
			// GetMemberName, which unwraps the UnaryExpression to find "minInclusive".
			bool ok = node.TryAssignValue<integer_DEtype, object>(
				d => (object)d.minInclusive, "not-a-decimal", out SdcRejectedValue? rej);

			// Must return false (type mismatch → rejection).
			Assert.IsFalse(ok, "A string value for a decimal property must be soft-rejected.");
			// Must not throw — exception is caught and converted to a rejection.
			Assert.IsNotNull(rej, "A synthetic SdcRejectedValue must be returned for a type mismatch.");
			// The offending value must be recorded in the rejection.
			Assert.AreEqual("not-a-decimal", rej!.AttemptedValue,
				"Rejection must record the offending (string) value for diagnostics.");
			// Message must mention the property name.
			Assert.IsTrue(rej.Message.Contains("minInclusive"),
				"Synthetic rejection message must name the property that was targeted.");
		}

		// ──── 14. WouldBeValid respects SdcValidationRuleRegistry override ───────────

		[TestMethod]
		public void WouldBeValid_RespectsRuleRegistryOverride()
		{
			// Rationale: SdcValidationRuleRegistry allows hand-authored rules to replace
			// the DataAnnotations on generated properties without editing generated files.
			// WouldBeValid must honor these registered rules (using TryValidateValue instead
			// of TryValidateProperty when a registration exists), exactly mirroring the
			// behavior of SdcUtil.ValidateAndRaise.
			var node = CreateDecimalNode();

			// Register [Required] for decimal_DEtype.mask (which normally has no annotations).
			// After this, null must be rejected by WouldBeValid.
			SdcValidationRuleRegistry.Register(
				typeof(decimal_DEtype), "mask",
				new RequiredAttribute { ErrorMessage = "mask is required by registry rule" });

			// null should fail [Required] via the registered rule.
			bool nullResult = node.WouldBeValid(d => d.mask, null, out string? nullMsg);
			// Non-null should pass.
			bool validResult = node.WouldBeValid(d => d.mask, "some-mask", out string? validMsg);

			// null must be rejected because the registry [Required] rule applies.
			Assert.IsFalse(nullResult,
				"null must fail the registered [Required] rule for decimal_DEtype.mask.");
			Assert.IsNotNull(nullMsg,
				"Failure message must be provided when registry rule rejects the value.");

			// A non-null string must pass.
			Assert.IsTrue(validResult,
				"A non-null mask value must pass the [Required] rule.");
			Assert.IsNull(validMsg,
				"No failure message should be produced for a valid value.");
		}

		// ──── 15. TryAssignValue — coherence check fires for constraint setter ────────

		[TestMethod]
		public void TryAssignValue_CoherenceCheckFires_WhenConstraintsConflict()
		{
			// Rationale: the constraint setters in decimal_DEtype call
			// CheckConstraintCoherence after a valid assignment. When minInclusive > maxInclusive,
			// an advisory event is raised. TryAssignValue invokes the real setter so this
			// post-assignment coherence check fires, just as direct assignment would.
			var node = CreateDecimalNode();
			// Establish maxInclusive = 50 first.
			node.maxInclusive = 50m;
			_captured.Clear(); // discard the maxInclusive assignment events

			// Assign minInclusive = 100 (> maxInclusive=50): coherence violation.
			bool ok = node.TryAssignValue(d => d.minInclusive, 100m, out _);

			// The assignment itself is valid (decimal_DEtype.minInclusive has no Range constraint).
			Assert.IsTrue(ok,
				"The minInclusive assignment is structurally valid — TryAssignValue must return true.");
			// But the coherence advisory event must have fired.
			Assert.IsTrue(_captured.Count > 0,
				"CheckConstraintCoherence must raise a ValidationOccurred advisory event " +
				"when minInclusive > maxInclusive — TryAssignValue must not bypass this.");
		}

		// ──── 16. Both methods work on constraint properties (not just val) ───────────

		[TestMethod]
		public void TryAssignValue_WorksOnConstraintProperty_NotJustVal()
		{
			// Rationale: TryAssignValue and WouldBeValid are generic — they work on ANY
			// property, not just 'val'. This test exercises them against decimal_DEtype
			// constraint properties (minInclusive, maxInclusive) to confirm that the
			// expression-tree member extraction and reflection path work for these too.
			var node = CreateDecimalNode();

			// WouldBeValid on minInclusive (decimal property, no DataAnnotations → always valid).
			bool wouldBeValid = node.WouldBeValid(d => d.minInclusive, 25m);
			Assert.IsTrue(wouldBeValid,
				"WouldBeValid must work on constraint properties like minInclusive.");

			// TryAssignValue on minInclusive.
			bool ok = node.TryAssignValue(d => d.minInclusive, 25m, out SdcRejectedValue? rej);
			Assert.IsTrue(ok,
				"TryAssignValue must work on constraint properties like minInclusive.");
			Assert.IsNull(rej,
				"No rejection for a valid decimal constraint value.");
			Assert.AreEqual(25m, node.minInclusive,
				"minInclusive must hold the assigned value after TryAssignValue.");
		}

		// ──── 17. SuppressValidation = true: rejected but no event fired ──────────────

		[TestMethod]
		public void TryAssignValue_SuppressValidation_RejectsButNoEventFired_RejectionPopulated()
		{
			// Rationale: SuppressValidation silences the ValidationOccurred event and
			// ValidationCollector output to avoid noise during non-validating deserialization.
			// However, the soft-reject contract says invalid values are NEVER stored and the
			// rejection store is ALWAYS updated — SuppressValidation does not override this.
			// TryAssignValue must therefore return false, populate 'rejection', and leave
			// the rejection store entry in place, while NOT firing any event.
			var node = CreateIntegerNode();
			SdcUtil.SuppressValidation.Value = true;

			bool ok = node.TryAssignValue(d => d.minInclusive, 1.5m, out SdcRejectedValue? rej);

			// Restore SuppressValidation before assertions so cleanup doesn't interfere.
			SdcUtil.SuppressValidation.Value = false;

			// Assignment must still be rejected (SuppressValidation never permits invalid values).
			Assert.IsFalse(ok,
				"SuppressValidation must not allow an invalid value to be stored; must still return false.");
			// Rejection detail must be populated.
			Assert.IsNotNull(rej,
				"SdcRejectedValue must be returned even under SuppressValidation.");
			// The rejection store must have recorded the failure.
			Assert.IsTrue(node.RejectedValues.ContainsKey("minInclusive"),
				"Rejection store must be updated regardless of SuppressValidation.");
			// No event must have fired (SuppressValidation silences the event).
			Assert.AreEqual(0, _captured.Count,
				"ValidationOccurred must NOT fire when SuppressValidation is true.");
		}
	}
}
