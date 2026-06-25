// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SDC.Schema.Tests.Validation
{
	/// <summary>
	/// Phase 10 tests — three gaps filled:
	///   Gap 1: integer-family Stype val setters now call CheckValAgainstConstraints
	///   Gap 2: duration-family Stype val setters now call CheckValAgainstConstraints
	///   Gap 3: ValidateTree extended to run a post-sweep coherence check (report-only)
	/// </summary>
	[TestClass]
	public class SdcPhase10Tests
	{
		private List<SdcValidationEventArgs> _captured = new();
		private EventHandler<SdcValidationEventArgs> _handler = null!;

		[TestInitialize]
		public void Init()
		{
			_captured = new List<SdcValidationEventArgs>();
			_handler  = (_, e) => _captured.Add(e);
			SdcValidationEvents.ValidationOccurred += _handler;
			SdcUtil.SuppressValidation.Value  = false;
			SdcUtil.ValidationCollector.Value = null;
			SdcUtil.IsDeserializing.Value     = false;
		}

		[TestCleanup]
		public void Cleanup()
		{
			SdcValidationEvents.ValidationOccurred -= _handler;
			_captured.Clear();
			SdcUtil.SuppressValidation.Value  = false;
			SdcUtil.ValidationCollector.Value = null;
			SdcUtil.IsDeserializing.Value     = false;
			BaseType.ResetLastTopNode();
		}

		// ─── Shared helpers ────────────────────────────────────────────────────────

		private static T CreateNode<T>(ItemChoiceType ict) where T : BaseType
		{
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, $"q_ph10_{typeof(T).Name}");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ict);
			return (T)deType.Item;
		}

		// ─── Gap 1: integer-family Stype val setters call CheckValAgainstConstraints ─

		[TestMethod]
		public void IntegerStype_ValSetter_FiresCoherenceEvent_WhenValBelowMinInclusive()
		{
			// Rationale: Phase 10 adds CheckValAgainstConstraints to the integer_Stype.val setter.
			// Setting val below a runtime minInclusive must fire an Error-severity coherence event
			// even though val still passes DataAnnotations ([Range] passes 50 as valid integer).
			var node = CreateNode<integer_DEtype>(ItemChoiceType.integer);
			node.minInclusive = 100m;
			_captured.Clear(); // clear the Warning from minInclusive-vs-val check

			node.val = 50m;

			// val IS stored (the coherence check doesn't gate the assignment — that's the design)
			Assert.AreEqual(50m, node.val,
				"val must still be stored even when it violates a runtime minInclusive constraint.");
			// But the coherence event must have fired
			Assert.IsTrue(_captured.Count > 0,
				"An Error-severity coherence event must fire when val is set below minInclusive.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity,
				"val-vs-constraint violations are always Error severity.");
		}

		[TestMethod]
		public void NegativeIntegerStype_ValSetter_FiresCoherenceEvent_WhenValAboveMaxInclusive()
		{
			// Rationale: negativeInteger_Stype inherits the updated val setter. Setting val=-1
			// when maxInclusive=-10 is a coherence violation (val > maxInclusive) and must fire an
			// Error event. DataAnnotations passes (Range allows -1 for negativeInteger).
			var node = CreateNode<negativeInteger_DEtype>(ItemChoiceType.negativeInteger);
			node.maxInclusive = -10m;
			_captured.Clear();

			node.val = -1m; // valid for negativeInteger DataAnnotations; violates maxInclusive=-10

			Assert.AreEqual(-1m, node.val,
				"val must be stored despite violating maxInclusive — coherence check is advisory only.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error-severity coherence event must fire when val exceeds maxInclusive.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void NonNegativeIntegerStype_ValSetter_FiresCoherenceEvent_WhenValBelowMinExclusive()
		{
			// Rationale: nonNegativeInteger_Stype val setter must now call CheckValAgainstConstraints.
			// minExclusive=50 means val must be strictly > 50. val=50 is at the exclusive boundary
			// and must be rejected by the coherence checker.
			var node = CreateNode<nonNegativeInteger_DEtype>(ItemChoiceType.nonNegativeInteger);
			node.minExclusive = 50m;
			_captured.Clear();

			node.val = 50m; // equal to minExclusive — exclusive boundary violation

			Assert.AreEqual(50m, node.val,
				"val is stored; CheckValAgainstConstraints fires an advisory event but does not block.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire for a val that equals minExclusive (exclusive boundary violated).");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void NonPositiveIntegerStype_ValSetter_FiresCoherenceEvent_WhenValExceedsMaxExclusive()
		{
			// Rationale: nonPositiveInteger_Stype val setter must call CheckValAgainstConstraints.
			// maxExclusive=-5 means val must be strictly < -5. val=-5 is at the exclusive boundary
			// and must fire an Error coherence event.
			var node = CreateNode<nonPositiveInteger_DEtype>(ItemChoiceType.nonPositiveInteger);
			node.maxExclusive = -5m;
			_captured.Clear();

			node.val = -5m; // equal to maxExclusive — exclusive boundary violation

			Assert.AreEqual(-5m, node.val,
				"val is stored despite coherence violation; coherence check does not block assignment.");
			Assert.IsTrue(_captured.Count > 0,
				"An Error event must fire when val equals maxExclusive (exclusive upper boundary violated).");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		[TestMethod]
		public void PositiveIntegerStype_ValSetter_NoEvent_WhenValWithinConstraints()
		{
			// Rationale: positiveInteger_Stype val setter must call CheckValAgainstConstraints,
			// but must not fire any event when val is within all active runtime constraints.
			// This verifies no false-positive event for a valid combination.
			var node = CreateNode<positiveInteger_DEtype>(ItemChoiceType.positiveInteger);
			node.minInclusive = 10m;
			node.maxInclusive = 100m;
			_captured.Clear();

			node.val = 50m; // within [10, 100] — no coherence violation expected

			Assert.AreEqual(50m, node.val,
				"val within constraints must be stored normally.");
			Assert.AreEqual(0, _captured.Count,
				"No coherence event must fire when val is within all active runtime constraints.");
		}

		[TestMethod]
		public void IntegerStype_ValSetter_SkipsCoherenceCheck_DuringDeserialization()
		{
			// Rationale: CheckValAgainstConstraints is guarded by !IsDeserializing to avoid
			// false positives when XML properties arrive before their constraint siblings.
			// During deserialization the check must be silently skipped; no event must fire.
			var node = CreateNode<integer_DEtype>(ItemChoiceType.integer);
			node.minInclusive = 100m;
			_captured.Clear();
			SdcUtil.IsDeserializing.Value = true;

			node.val = 50m; // would violate minInclusive, but deserialization guard must suppress it

			Assert.AreEqual(0, _captured.Count,
				"CheckValAgainstConstraints must not fire during deserialization (IsDeserializing=true).");
		}

		// ─── Gap 2: duration-family Stype val setters call CheckValAgainstConstraints ──

		[TestMethod]
		public void DurationStype_ValSetter_DoesNotCrash_WithNoMatchingConstraints()
		{
			// Rationale: duration_Stype val setter now calls CheckValAgainstConstraints.
			// For string-type vals, the checker uses the string path (minLength/maxLength/pattern).
			// duration_DEtype has no minLength/maxLength/pattern constraints, so the check
			// must return true (no violation) and must not throw or fire any event.
			var node = CreateNode<duration_DEtype>(ItemChoiceType.duration);
			_captured.Clear();

			node.val = "P1Y2M3D";

			Assert.AreEqual("P1Y2M3D", node.val,
				"A valid duration string must be stored normally.");
			Assert.AreEqual(0, _captured.Count,
				"No coherence event must fire for a duration val when no string constraints are set.");
		}

		[TestMethod]
		public void DayTimeDurationStype_ValSetter_DoesNotCrash_WithNoMatchingConstraints()
		{
			// Rationale: dayTimeDuration_Stype val setter now calls CheckValAgainstConstraints.
			// Same as duration: the string path finds no minLength/maxLength/pattern on the
			// dayTimeDuration_DEtype, so the check must be a no-op (true, no event).
			var node = CreateNode<dayTimeDuration_DEtype>(ItemChoiceType.dayTimeDuration);
			_captured.Clear();

			node.val = "P5DT12H30M"; // valid dayTimeDuration — no Y or M components

			Assert.AreEqual("P5DT12H30M", node.val,
				"A valid dayTimeDuration string must be stored normally.");
			Assert.AreEqual(0, _captured.Count,
				"No coherence event must fire for a dayTimeDuration val when no string constraints are set.");
		}

		[TestMethod]
		public void YearMonthDurationStype_ValSetter_DoesNotCrash_WithNoMatchingConstraints()
		{
			// Rationale: yearMonthDuration_Stype val setter now calls CheckValAgainstConstraints.
			// Same as duration: no minLength/maxLength/pattern on yearMonthDuration_DEtype, so
			// the check must be a no-op (true, no event).
			var node = CreateNode<yearMonthDuration_DEtype>(ItemChoiceType.yearMonthDuration);
			_captured.Clear();

			node.val = "P3Y6M"; // valid yearMonthDuration — no D, T, H, or S components

			Assert.AreEqual("P3Y6M", node.val,
				"A valid yearMonthDuration string must be stored normally.");
			Assert.AreEqual(0, _captured.Count,
				"No coherence event must fire for a yearMonthDuration val when no string constraints are set.");
		}

		// ─── Gap 3: ValidateTree extended with post-sweep coherence check ───────────

		[TestMethod]
		public void ValidateTree_PostSweep_DetectsCoherenceViolation()
		{
			// Rationale: ValidateTree now calls CheckValAgainstConstraints for each node that
			// has a val property. Setting val=50 with minInclusive=100 (val < minInclusive)
			// is a coherence violation that must be recorded in the returned report.
			var de   = new DataElementType(null);
			var q    = new QuestionItemType(de, "q_sweep_coherence");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var node = (integer_DEtype)deType.Item;

			node.val          = 50m;  // passes DataAnnotations
			node.minInclusive = 100m; // now val < minInclusive — coherence violation
			_captured.Clear();        // clear the Warning from the constraint-vs-val setter path

			var report = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.IsFalse(report.IsValid,
				"ValidateTree must report IsValid=false when val violates a runtime minInclusive constraint.");
			Assert.IsTrue(report.ErrorCount > 0,
				"The val-vs-minInclusive coherence violation must be recorded as an Error in the report.");
		}

		[TestMethod]
		public void ValidateTree_PostSweep_DoesNotDiscardStoredValue()
		{
			// Rationale: the post-sweep coherence check is purely reportorial. Values are already
			// stored in the object model and must NOT be discarded when a violation is detected.
			// ValidateTree may flag, but never modify, stored node state.
			var de   = new DataElementType(null);
			var q    = new QuestionItemType(de, "q_sweep_preserve");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var node = (integer_DEtype)deType.Item;

			node.val          = 50m;
			node.minInclusive = 100m;
			_captured.Clear();

			_ = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.AreEqual(50m, node.val,
				"ValidateTree must not discard or modify the stored val even when a coherence violation is detected.");
			Assert.AreEqual(100m, node.minInclusive,
				"ValidateTree must not discard or modify the stored minInclusive constraint.");
		}

		[TestMethod]
		public void ValidateTree_PostSweep_PopulatesValidationCollector()
		{
			// Rationale: ValidateTree sets up ValidationCollector before the sweep; coherence
			// violations detected via CheckValAgainstConstraints must be recorded in the returned
			// SdcValidationReport (which IS the ValidationCollector during the sweep).
			var de   = new DataElementType(null);
			var q    = new QuestionItemType(de, "q_sweep_collector");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var node = (integer_DEtype)deType.Item;

			node.val          = 50m;
			node.minInclusive = 100m;
			_captured.Clear();

			var report = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.IsTrue(report.ErrorCount > 0,
				"Coherence violations found during the sweep must be appended to ValidationCollector " +
				"(the report object), so report.ErrorCount must reflect the detected violation.");
		}

		[TestMethod]
		public void ValidateTree_PostSweep_FiresValidationEvents()
		{
			// Rationale: ValidateTree forces SuppressValidation=false for the sweep duration,
			// so ValidationOccurred events must be fired for every coherence violation found.
			var de   = new DataElementType(null);
			var q    = new QuestionItemType(de, "q_sweep_events");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var node = (integer_DEtype)deType.Item;

			node.val          = 50m;
			node.minInclusive = 100m;
			_captured.Clear();

			_ = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.IsTrue(_captured.Count > 0,
				"ValidationOccurred events must fire during the sweep when SuppressValidation is false.");
		}

		[TestMethod]
		public void ValidateTree_AlwaysRunsCoherenceCheck_RegardlessOfAmbientSuppressValidation()
		{
			// Rationale: the caller may have set SuppressValidation=true before calling ValidateTree
			// (e.g., during a quiet batch operation). ValidateTree forces SuppressValidation=false
			// internally so coherence checks always populate the report — an explicit sweep must
			// always produce results; suppression should not silently hide violations.
			var de   = new DataElementType(null);
			var q    = new QuestionItemType(de, "q_sweep_suppress_override");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var node = (integer_DEtype)deType.Item;

			node.val          = 50m;
			node.minInclusive = 100m;
			_captured.Clear();
			SdcUtil.SuppressValidation.Value = true; // caller had suppression on

			var report = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.IsTrue(report.ErrorCount > 0,
				"ValidateTree must produce coherence errors even when the caller had SuppressValidation=true " +
				"before the call; an explicit sweep overrides ambient suppression.");
		}

		[TestMethod]
		public void ValidateTree_RestoresSuppressValidation_AfterCompletion()
		{
			// Rationale: ValidateTree forces SuppressValidation=false internally but must restore
			// the caller's original value in its finally block. If the caller had SuppressValidation=true,
			// ValidateTree must not leave it permanently cleared.
			var de   = new DataElementType(null);
			var q    = new QuestionItemType(de, "q_sweep_restore");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);

			SdcUtil.SuppressValidation.Value = true; // caller's ambient setting

			_ = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.IsTrue(SdcUtil.SuppressValidation.Value,
				"ValidateTree must restore SuppressValidation to the caller's original value (true) after returning.");
			Assert.IsNull(SdcUtil.ValidationCollector.Value,
				"ValidateTree must clear ValidationCollector in its finally block.");
		}
	}
}
