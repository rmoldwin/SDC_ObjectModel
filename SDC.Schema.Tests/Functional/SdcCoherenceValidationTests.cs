// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;

namespace SDC.Schema.Tests.Functional
{
	/// <summary>
	/// Phase 3 coherence validator tests. Covers cross-property constraint checking:
	///   CheckValAgainstConstraints — val vs existing constraints (numeric and string types)
	///   CheckConstraintCoherence  — new constraint vs existing constraints and current val
	/// </summary>
	[TestClass]
	public class SdcCoherenceValidationTests
	{
		private List<SdcValidationEventArgs> _captured = new();
		private EventHandler<SdcValidationEventArgs> _handler;

		[TestInitialize]
		public void Init()
		{
			_captured = new List<SdcValidationEventArgs>();
			_handler  = (_, e) => _captured.Add(e);
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
			BaseType.ResetLastTopNode();
		}

		/// <summary>
		/// Creates a minimal integer_DEtype in a valid SDC node hierarchy.
		/// Events from construction are cleared so tests start from a clean slate.
		/// </summary>
		private integer_DEtype CreateIntegerNode()
		{
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "q_coh_int");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			_captured.Clear();
			return (integer_DEtype)deType.Item;
		}

		/// <summary>
		/// Creates a minimal string_DEtype in a valid SDC node hierarchy.
		/// Events from construction are cleared so tests start from a clean slate.
		/// </summary>
		private string_DEtype CreateStringNode()
		{
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "q_coh_str");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.@string);
			_captured.Clear();
			return (string_DEtype)deType.Item;
		}

		// ─── CheckValAgainstConstraints: Numeric ─────────────────────────────────

		[TestMethod]
		public void IntegerVal_BelowMinInclusive_ReturnsFalseAndFiresEvent()
		{
			// Rationale: when minInclusive=100 is set, val=50 is below the inclusive minimum.
			// The coherence check must return false (rejecting the value) and fire an
			// Error-severity event so callers can surface the violation.
			var node = CreateIntegerNode();
			node.minInclusive = 100m; // valid whole-number constraint — no event fires

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 50m);

			Assert.IsFalse(result, "val below minInclusive must be rejected (returns false).");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must be fired for the minInclusive coherence violation.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity,
				"val-vs-constraint violations must always be Error severity.");
		}

		[TestMethod]
		public void IntegerVal_AboveMaxInclusive_ReturnsFalseAndFiresEvent()
		{
			// Rationale: when maxInclusive=50 is set, val=100 exceeds the inclusive maximum.
			// The check must return false and fire an Error event.
			var node = CreateIntegerNode();
			node.maxInclusive = 50m;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 100m);

			Assert.IsFalse(result, "val above maxInclusive must be rejected (returns false).");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must be fired for the maxInclusive coherence violation.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void IntegerVal_EqualToMinInclusive_ReturnsTrue()
		{
			// Rationale: minInclusive is INCLUSIVE — a val exactly equal to minInclusive
			// satisfies the constraint and must pass (returns true, no event).
			var node = CreateIntegerNode();
			node.minInclusive = 100m;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 100m);

			Assert.IsTrue(result, "val equal to minInclusive must be accepted (inclusive boundary).");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire when val exactly meets the inclusive minimum.");
		}

		[TestMethod]
		public void IntegerVal_EqualToMaxInclusive_ReturnsTrue()
		{
			// Rationale: maxInclusive is INCLUSIVE — a val exactly equal to maxInclusive
			// satisfies the constraint and must pass (returns true, no event).
			var node = CreateIntegerNode();
			node.maxInclusive = 100m;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 100m);

			Assert.IsTrue(result, "val equal to maxInclusive must be accepted (inclusive boundary).");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire when val exactly meets the inclusive maximum.");
		}

		[TestMethod]
		public void IntegerVal_AtMinExclusive_ReturnsFalse()
		{
			// Rationale: minExclusive is EXCLUSIVE — val must be STRICTLY GREATER than the bound.
			// val equal to minExclusive is therefore invalid and must be rejected.
			var node = CreateIntegerNode();
			node.minExclusive = 50m;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 50m);

			Assert.IsFalse(result,
				"val equal to minExclusive must be rejected (exclusive boundary — strict inequality required).");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for the exclusive lower-bound violation.");
		}

		[TestMethod]
		public void IntegerVal_AtMaxExclusive_ReturnsFalse()
		{
			// Rationale: maxExclusive is EXCLUSIVE — val must be STRICTLY LESS than the bound.
			// val equal to maxExclusive is therefore invalid and must be rejected.
			var node = CreateIntegerNode();
			node.maxExclusive = 100m;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 100m);

			Assert.IsFalse(result,
				"val equal to maxExclusive must be rejected (exclusive boundary — strict inequality required).");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for the exclusive upper-bound violation.");
		}

		[TestMethod]
		public void NoConstraintsSet_AnyVal_ReturnsTrue()
		{
			// Rationale: when no constraints have been set on the node, any val is trivially
			// coherent — the check must return true without firing any events.
			var node = CreateIntegerNode();

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", 42m);

			Assert.IsTrue(result, "With no constraints set, any val must pass coherence.");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire when there are no constraints to check against.");
		}

		[TestMethod]
		public void WhenSuppressValidation_True_ReturnsTrue_NoEventFired()
		{
			// Rationale: CheckValAgainstConstraints is a no-op when SuppressValidation=true,
			// enabling deserialization to set properties in document order without false
			// coherence violations (minInclusive may be set before val during XML hydration).
			var node = CreateIntegerNode();
			node.minInclusive = 100m;
			_captured.Clear();

			SdcUtil.SuppressValidation.Value = true;
			bool result;
			try   { result = SdcValidate.CheckValAgainstConstraints(node, "val", 50m); }
			finally { SdcUtil.SuppressValidation.Value = false; }

			Assert.IsTrue(result,
				"SuppressValidation=true must make CheckValAgainstConstraints return true unconditionally.");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire while SuppressValidation is true.");
		}

		// ─── CheckValAgainstConstraints: String ──────────────────────────────────

		[TestMethod]
		public void StringVal_BelowMinLength_ReturnsFalse()
		{
			// Rationale: a string shorter than minLength violates the length constraint.
			// The check must return false and fire an Error event.
			var node = CreateStringNode();
			node.minLength = 10UL;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", "hi");

			Assert.IsFalse(result, "String shorter than minLength must be rejected.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must be fired for the minLength coherence violation.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void StringVal_AboveMaxLength_ReturnsFalse()
		{
			// Rationale: a string longer than maxLength violates the length constraint.
			// The check must return false and fire an Error event.
			var node = CreateStringNode();
			node.maxLength = 5UL;

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", "toolongstring");

			Assert.IsFalse(result, "String longer than maxLength must be rejected.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must be fired for the maxLength coherence violation.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void StringVal_MatchesPattern_ReturnsTrue()
		{
			// Rationale: a string that satisfies the pattern constraint is valid.
			// The check must return true and fire no events.
			var node = CreateStringNode();
			node.pattern = @"^\w+$"; // matches single-word strings (no spaces)

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", "hello");

			Assert.IsTrue(result, "String matching the pattern must be accepted.");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire when the string satisfies the pattern constraint.");
		}

		[TestMethod]
		public void StringVal_ViolatesPattern_ReturnsFalse()
		{
			// Rationale: a string that does not match the required pattern is invalid.
			// The check must return false and fire an Error event.
			var node = CreateStringNode();
			node.pattern = @"^\w+$"; // space in "hello world" violates \w+

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", "hello world");

			Assert.IsFalse(result, "String not matching the pattern must be rejected.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must be fired for the pattern coherence violation.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void InvalidRegexPattern_ValNotRejected_WarningFired()
		{
			// Rationale: if the pattern property contains a syntactically invalid regex,
			// the defect is in the constraint, not in the val. The val must not be rejected
			// (returns true) but a Warning event must fire to surface the bad pattern.
			var node = CreateStringNode();
			node.pattern = "[invalid_regex("; // malformed regex

			var result = SdcValidate.CheckValAgainstConstraints(node, "val", "anything");

			Assert.IsTrue(result,
				"An invalid regex pattern must not cause val to be rejected (defect is in the constraint, not val).");
			Assert.IsTrue(_captured.Count > 0,
				"A Warning event must fire to surface the invalid regex pattern.");
			Assert.AreEqual(SdcValidationSeverity.Warning, _captured[0].Severity,
				"The notification for an invalid pattern must be Warning, not Error.");
		}

		// ─── CheckConstraintCoherence: Constraint vs Constraint ──────────────────

		[TestMethod]
		public void MinInclusive_ExceedsMaxInclusive_ReturnsFalse()
		{
			// Rationale: minInclusive > maxInclusive defines an empty range — no value
			// could satisfy both simultaneously. This constraint-vs-constraint incoherence
			// must be rejected with Error severity so the OM cannot enter a contradictory state.
			var node = CreateIntegerNode();
			node.maxInclusive = 100m;

			var result = SdcValidate.CheckConstraintCoherence(node, "minInclusive", 200m);

			Assert.IsFalse(result, "minInclusive > maxInclusive must be rejected as an incoherent constraint pair.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for constraint-vs-constraint incoherence.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void MinExclusive_EqualsMaxExclusive_ReturnsFalse()
		{
			// Rationale: when minExclusive == maxExclusive, no value x can satisfy
			// both x > V and x < V simultaneously — the range is empty and incoherent.
			var node = CreateIntegerNode();
			node.maxExclusive = 50m;

			var result = SdcValidate.CheckConstraintCoherence(node, "minExclusive", 50m);

			Assert.IsFalse(result,
				"minExclusive == maxExclusive must be rejected (no value can satisfy both exclusive bounds).");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for the empty-range exclusive bound pair.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void MinLength_ExceedsMaxLength_ReturnsFalse()
		{
			// Rationale: minLength > maxLength means no string can satisfy both length
			// constraints simultaneously — the constraint pair is incoherent and must be rejected.
			var node = CreateStringNode();
			node.maxLength = 10UL;

			var result = SdcValidate.CheckConstraintCoherence(node, "minLength", 20UL);

			Assert.IsFalse(result, "minLength > maxLength must be rejected as an incoherent constraint pair.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for the incoherent length constraint pair.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void Pattern_InvalidRegex_ReturnsFalse()
		{
			// Rationale: a pattern that cannot be compiled is an invalid constraint — it can
			// never be evaluated against any val and is therefore incoherent. Must be rejected
			// with Error severity so the node does not record an unusable pattern.
			var node = CreateStringNode();

			var result = SdcValidate.CheckConstraintCoherence(node, "pattern", "[invalid(");

			Assert.IsFalse(result, "An invalid regex pattern must be rejected as a constraint.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for the uncompilable pattern constraint.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void CoherentConstraints_ReturnsTrue()
		{
			// Rationale: setting a constraint that is fully coherent with all existing
			// constraints must return true without firing any events.
			var node = CreateIntegerNode();
			node.maxInclusive = 200m; // pre-existing upper bound

			var result = SdcValidate.CheckConstraintCoherence(node, "minInclusive", 100m);

			Assert.IsTrue(result, "A coherent minInclusive ≤ maxInclusive must be accepted.");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire when the new constraint is coherent with all existing constraints.");
		}

		// ─── CheckConstraintCoherence: Constraint vs Val ─────────────────────────

		[TestMethod]
		public void NewMinInclusive_ExceedsCurrentVal_ReturnsTrue_WarningFired()
		{
			// Rationale: setting minInclusive above the current val makes val invalid, but
			// the constraint itself is internally consistent. The constraint is accepted
			// (returns true) so the caller can update val separately; a Warning is fired
			// to signal that val is now out of range.
			var node = CreateIntegerNode();
			node.val = 50m;
			_captured.Clear(); // discard events from the val assignment

			var result = SdcValidate.CheckConstraintCoherence(node, "minInclusive", 200m);

			Assert.IsTrue(result,
				"A new minInclusive that invalidates current val must still be accepted (Warning, not Error).");
			Assert.IsTrue(_captured.Count > 0,
				"A Warning event must fire to signal that current val is now below the new minInclusive.");
			Assert.AreEqual(SdcValidationSeverity.Warning, _captured[0].Severity,
				"Constraint-vs-val violations must be Warning severity (constraint is coherent, val needs update).");
		}

		[TestMethod]
		public void NewMaxInclusive_BelowCurrentVal_ReturnsTrue_WarningFired()
		{
			// Rationale: setting maxInclusive below the current val makes val invalid, but
			// the constraint itself is coherent. Returns true with a Warning so val can
			// be corrected in a subsequent step without the constraint being blocked.
			var node = CreateIntegerNode();
			node.val = 200m;
			_captured.Clear();

			var result = SdcValidate.CheckConstraintCoherence(node, "maxInclusive", 100m);

			Assert.IsTrue(result,
				"A new maxInclusive below current val must still be accepted (Warning, not Error).");
			Assert.IsTrue(_captured.Count > 0,
				"A Warning event must fire to signal that current val exceeds the new maxInclusive.");
			Assert.AreEqual(SdcValidationSeverity.Warning, _captured[0].Severity);
		}

		[TestMethod]
		public void NewPattern_DoesNotMatchCurrentVal_ReturnsTrue_WarningFired()
		{
			// Rationale: setting a valid pattern that the current val does not satisfy
			// makes val invalid relative to the new constraint, but the pattern itself is
			// coherent. Returns true with a Warning so val can be corrected separately.
			var node = CreateStringNode();
			node.val = "hello world"; // contains a space — will not match ^\w+$
			_captured.Clear();

			var result = SdcValidate.CheckConstraintCoherence(node, "pattern", @"^\w+$");

			Assert.IsTrue(result,
				"A valid pattern that invalidates current val must still be accepted (Warning, not Error).");
			Assert.IsTrue(_captured.Count > 0,
				"A Warning event must fire when the new pattern does not match the current val.");
			Assert.AreEqual(SdcValidationSeverity.Warning, _captured[0].Severity,
				"Pattern-vs-val mismatch must be Warning severity (pattern is valid; val needs update).");
		}

		[TestMethod]
		public void WhenSuppressValidation_True_AlwaysReturnsTrue_NoEvent()
		{
			// Rationale: CheckConstraintCoherence is a no-op when SuppressValidation=true,
			// enabling deserialization to set constraints in document order without false
			// rejections (e.g. maxInclusive may appear before minInclusive in XML).
			var node = CreateIntegerNode();
			node.maxInclusive = 100m;
			_captured.Clear();

			SdcUtil.SuppressValidation.Value = true;
			bool result;
			try   { result = SdcValidate.CheckConstraintCoherence(node, "minInclusive", 200m); }
			finally { SdcUtil.SuppressValidation.Value = false; }

			Assert.IsTrue(result,
				"SuppressValidation=true must make CheckConstraintCoherence return true unconditionally.");
			Assert.AreEqual(0, _captured.Count,
				"No event must fire while SuppressValidation is true.");
		}
	}
}
