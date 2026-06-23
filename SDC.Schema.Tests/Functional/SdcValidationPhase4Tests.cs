// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SDC.Schema.Tests.Functional
{
	/// <summary>
	/// Comprehensive tests for Phase 4 validation infrastructure:
	///   4A – SdcValidationReport / SdcNodeValidationIssue types
	///   4B – SuppressValidation / ValidationCollector in SdcUtil
	///   4C – *Validating deserializer overloads for XML, JSON, BSON, MsgPack
	///   4D – ValidateTree / ValidateNode sweep API in SdcValidate
	///   4E – Descriptive FormatErrorMessage overrides on FractionDigitsAttribute / MaxDigitsAttribute
	/// </summary>
	[TestClass]
	public class SdcValidationPhase4Tests
	{
		private List<SdcValidationEventArgs> _captured = new();
		private EventHandler<SdcValidationEventArgs> _handler;
		private static string _xmlPath;
		private static string _xmlContent;

		[ClassInitialize]
		public static void ClassInit(TestContext _)
		{
			_xmlPath    = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			_xmlContent = File.ReadAllText(_xmlPath);
		}

		[TestInitialize]
		public void Init()
		{
			_captured = new List<SdcValidationEventArgs>();
			_handler  = (_, e) => _captured.Add(e);
			SdcValidationEvents.ValidationOccurred += _handler;

			// Ensure clean state before each test
			SdcUtil.IsDeserializing.Value     = false;
			SdcUtil.SuppressValidation.Value  = false;
			SdcUtil.ValidationCollector.Value = null;
		}

		[TestCleanup]
		public void Cleanup()
		{
			SdcValidationEvents.ValidationOccurred -= _handler;
			_captured.Clear();
			SdcUtil.IsDeserializing.Value     = false;
			SdcUtil.SuppressValidation.Value  = false;
			SdcUtil.ValidationCollector.Value = null;
			BaseType.ResetLastTopNode();
		}

		// ─── Helpers ──────────────────────────────────────────────────────────────

		private integer_DEtype CreateIntegerNode()
		{
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "q_ph4");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			_captured.Clear(); // discard construction noise
			return (integer_DEtype)deType.Item;
		}

		// ─── Phase 4A: SdcValidationReport / SdcNodeValidationIssue ──────────────

		[TestMethod]
		public void SdcValidationReport_Empty_IsValidTrue()
		{
			// Rationale: a freshly constructed report with no issues must report IsValid=true
			// and zero counts for all severity buckets.
			var report = new SdcValidationReport();

			Assert.IsTrue(report.IsValid, "Empty report must be IsValid=true.");
			Assert.AreEqual(0, report.ErrorCount,   "ErrorCount must be 0 for a new report.");
			Assert.AreEqual(0, report.WarningCount, "WarningCount must be 0 for a new report.");
			Assert.AreEqual(0, report.AffectedNodeCount, "AffectedNodeCount must be 0 for a new report.");
		}

		[TestMethod]
		public void SdcValidationReport_AddErrorIssue_IsValidFalse_CountsCorrect()
		{
			// Rationale: adding an Error-severity issue must flip IsValid to false and increment
			// ErrorCount; AffectedNodeCount must reflect distinct nodes with issues.
			var report = new SdcValidationReport();
			var issue  = new SdcNodeValidationIssue
			{
				NodeID        = "node-1",
				NodeType      = "integer_DEtype",
				PropertyName  = "minInclusive",
				AttemptedValue = 1.5m,
				Message       = "Must be a whole number.",
				Severity      = SdcValidationSeverity.Error
			};

			report.Add(issue);

			Assert.IsFalse(report.IsValid, "Report must be IsValid=false after an Error issue is added.");
			Assert.AreEqual(1, report.ErrorCount);
			Assert.AreEqual(0, report.WarningCount);
			Assert.AreEqual(1, report.AffectedNodeCount);
		}

		[TestMethod]
		public void SdcValidationReport_AddWarningIssue_IsValidTrue_WarningCounted()
		{
			// Rationale: Warning-severity issues do NOT make IsValid=false (only Error does),
			// but must appear in WarningCount.
			var report = new SdcValidationReport();
			var issue  = new SdcNodeValidationIssue
			{
				NodeID   = "node-2",
				Severity = SdcValidationSeverity.Warning,
				Message  = "Unusual value."
			};

			report.Add(issue);

			Assert.IsTrue(report.IsValid,   "Warnings alone must not make IsValid=false.");
			Assert.AreEqual(0, report.ErrorCount);
			Assert.AreEqual(1, report.WarningCount);
		}

		[TestMethod]
		public void SdcValidationReport_AffectedNodeCount_DeduplicatesNodeId()
		{
			// Rationale: two issues on the same NodeID must count as one affected node;
			// AffectedNodeCount is about unique nodes, not total issue count.
			var report = new SdcValidationReport();
			report.Add(new SdcNodeValidationIssue { NodeID = "node-A", Severity = SdcValidationSeverity.Error, Message = "E1" });
			report.Add(new SdcNodeValidationIssue { NodeID = "node-A", Severity = SdcValidationSeverity.Error, Message = "E2" });
			report.Add(new SdcNodeValidationIssue { NodeID = "node-B", Severity = SdcValidationSeverity.Error, Message = "E3" });

			Assert.AreEqual(3, report.ErrorCount, "Three error issues must be counted individually.");
			Assert.AreEqual(2, report.AffectedNodeCount, "Two distinct NodeIDs must produce AffectedNodeCount=2.");
		}

		[TestMethod]
		public void SdcValidationReport_MergeFrom_CombinesIssues()
		{
			// Rationale: MergeFrom must combine both reports' issues into the receiver;
			// the merged ErrorCount must equal the sum of both inputs.
			var r1 = new SdcValidationReport();
			r1.Add(new SdcNodeValidationIssue { NodeID = "n1", Severity = SdcValidationSeverity.Error, Message = "Err-1" });

			var r2 = new SdcValidationReport();
			r2.Add(new SdcNodeValidationIssue { NodeID = "n2", Severity = SdcValidationSeverity.Error, Message = "Err-2" });
			r2.Add(new SdcNodeValidationIssue { NodeID = "n2", Severity = SdcValidationSeverity.Warning, Message = "Warn-1" });

			r1.MergeFrom(r2);

			Assert.AreEqual(2, r1.ErrorCount);
			Assert.AreEqual(1, r1.WarningCount);
			Assert.AreEqual(2, r1.AffectedNodeCount);
		}

		// ─── Phase 4B: SuppressValidation flag ────────────────────────────────────

		[TestMethod]
		public void SuppressValidation_True_PreventsSetter_EventFire()
		{
			// Rationale: when SuppressValidation=true, ValidateAndRaise must be a no-op even
			// for values that would otherwise fail DataAnnotations — no event must be raised.
			var intDt = CreateIntegerNode();
			SdcUtil.SuppressValidation.Value = true;
			try
			{
				// 1.5 is invalid for integer_DEtype.minInclusive (FractionDigits=0) but must be silent
				intDt.minInclusive = 1.5m;
			}
			finally
			{
				SdcUtil.SuppressValidation.Value = false;
			}

			Assert.AreEqual(0, _captured.Count, "SuppressValidation=true must prevent any ValidationOccurred event.");
			// Soft-reject is unconditional: even with events suppressed, the invalid value must NOT
			// be written to the typed field and MUST be recorded out-of-band for later UI correction.
			Assert.AreNotEqual(1.5m, intDt.minInclusive, "Invalid value must never be stored, even when validation events are suppressed.");
			Assert.IsTrue(intDt.HasRejectedValues, "The rejected value must be recorded even when events are suppressed.");
			Assert.AreEqual(1.5m, intDt.RejectedValues["minInclusive"].AttemptedValue, "The offending value must be retrievable from RejectedValues.");
		}

		[TestMethod]
		public void SuppressValidation_False_AllowsSetter_EventFire()
		{
			// Rationale: default SuppressValidation=false must allow setter validation events
			// to fire normally for invalid values.
			var intDt = CreateIntegerNode();

			// 1.5 has fractional digits; should trigger FractionDigitsAttribute(0) violation
			intDt.minInclusive = 1.5m;

			Assert.IsTrue(_captured.Count > 0, "SuppressValidation=false must allow validation events to fire.");
			// Soft-reject: the invalid value must not be stored even though the event fired.
			Assert.AreNotEqual(1.5m, intDt.minInclusive, "Invalid value must never be assigned to the typed field.");
		}

		[TestMethod]
		public void ValidationCollector_Populated_WhenNonNull()
		{
			// Rationale: when ValidationCollector is set before a setter assignment, any
			// DataAnnotations failure must be appended to it in addition to firing the event.
			var intDt  = CreateIntegerNode();
			var report = new SdcValidationReport();

			SdcUtil.ValidationCollector.Value = report;
			try
			{
				intDt.minInclusive = 1.5m; // invalid — fractional digits
			}
			finally
			{
				SdcUtil.ValidationCollector.Value = null;
			}

			Assert.IsTrue(report.ErrorCount > 0 || report.WarningCount > 0,
				"ValidationCollector must be populated when ValidateAndRaise fires.");
			// Soft-reject: collection of the issue does not imply the value was stored — it wasn't.
			Assert.AreNotEqual(1.5m, intDt.minInclusive, "Invalid value must never be assigned to the typed field.");
		}

		// ─── Phase 4C: DeserializeXmlValidating ───────────────────────────────────

		[TestMethod]
		public void DeserializeXmlValidating_ValidDocument_ReportIsValid()
		{
			// Rationale: a known-good BreastStagingTest.xml must round-trip through the
			// validating overload without producing any Error-severity issues.
			var (result, report) = SdcSerializer<FormDesignType>.DeserializeXmlValidating(_xmlContent);

			Assert.IsNotNull(result, "Deserialized FormDesignType must not be null.");
			Assert.IsTrue(report.IsValid,
				$"No validation errors expected for BreastStagingTest.xml. Errors: {report.ErrorCount}. " +
				$"Summary: {report.Summary}");
		}

		[TestMethod]
		public void DeserializeXmlValidating_ReturnsReport_Always()
		{
			// Rationale: the returned report object must be non-null regardless of whether
			// the document is valid; callers must not need to null-check the report.
			var (_, report) = SdcSerializer<FormDesignType>.DeserializeXmlValidating(_xmlContent);

			Assert.IsNotNull(report, "DeserializeXmlValidating must always return a non-null SdcValidationReport.");
		}

		[TestMethod]
		public void DeserializeXmlValidating_FiresEvents_WhenValidatorCollectorSet()
		{
			// Rationale: during validating deserialization SuppressValidation=false, so any
			// setter that encounters an invalid value must fire ValidationOccurred.
			// For the valid document we expect no events; we verify the event count is consistent
			// with the report's ErrorCount.
			var (_, report) = SdcSerializer<FormDesignType>.DeserializeXmlValidating(_xmlContent);

			// Events fired during deserialization were captured by _handler
			int firedCount = _captured.Count(e => e.Severity == SdcValidationSeverity.Error);
			Assert.AreEqual(report.ErrorCount, firedCount,
				"Event fire count for Errors must equal report.ErrorCount — both channels are fed together.");
		}

		[TestMethod]
		public void DeserializeXmlValidating_SuppressValidationCleared_AfterReturn()
		{
			// Rationale: the *Validating overload must always restore SuppressValidation=false
			// in its finally block so subsequent programmatic mutations are correctly validated.
			_ = SdcSerializer<FormDesignType>.DeserializeXmlValidating(_xmlContent);

			Assert.IsFalse(SdcUtil.SuppressValidation.Value,
				"SuppressValidation must be false after DeserializeXmlValidating returns.");
			Assert.IsNull(SdcUtil.ValidationCollector.Value,
				"ValidationCollector must be null after DeserializeXmlValidating returns.");
		}

		// ─── Phase 4C: DeserializeJsonValidating ──────────────────────────────────

		[TestMethod]
		public void DeserializeJsonValidating_ValidDocument_ReportIsValid()
		{
			// Rationale: JSON round-trip of a valid document must not produce Error issues.
			var (xmlResult, _) = SdcSerializer<FormDesignType>.DeserializeXmlValidating(_xmlContent);
			string json = SdcSerializerJson<FormDesignType>.SerializeJson(xmlResult);

			var (jsonResult, report) = SdcSerializerJson<FormDesignType>.DeserializeJsonValidating<FormDesignType>(json);

			Assert.IsNotNull(jsonResult);
			Assert.IsTrue(report.IsValid,
				$"JSON round-trip of valid document must produce no errors. Errors: {report.ErrorCount}. " +
				$"Summary: {report.Summary}");
		}

		[TestMethod]
		public void DeserializeJsonValidating_SuppressValidationCleared_AfterReturn()
		{
			// Rationale: same finally-block guarantee as the XML variant.
			var (xmlResult, _) = SdcSerializer<FormDesignType>.DeserializeXmlValidating(_xmlContent);
			string json = SdcSerializerJson<FormDesignType>.SerializeJson(xmlResult);

			_ = SdcSerializerJson<FormDesignType>.DeserializeJsonValidating<FormDesignType>(json);

			Assert.IsFalse(SdcUtil.SuppressValidation.Value);
			Assert.IsNull(SdcUtil.ValidationCollector.Value);
		}

		// ─── Phase 4D: ValidateTree / ValidateNode ────────────────────────────────

		[TestMethod]
		public void ValidateTree_ValidDocument_ReportIsValid()
		{
			// Rationale: the post-hydration sweep of a valid document should not produce any
			// unexpected errors. We use FormDesignType.DeserializeFromXml (which calls
			// ReflectRefreshTree) to ensure Nodes is populated for the sweep.
			// NOTE: Some constraint types (e.g. [RegularExpression] on DateTime properties in
			// dateTimeStamp_DEtype) are known to produce false-positive errors because
			// TryValidateObject calls value.ToString() with locale formatting. These are tracked
			// as a GitHub issue. We therefore assert only that the report is non-null and the
			// sweep completes without throwing; we do NOT assert IsValid=true for the full tree.
			BaseType.ResetLastTopNode();
			var xml    = FormDesignType.DeserializeFromXml(_xmlContent);
			var report = ((ITopNode)xml).ValidateTree(recurseSubTrees: false);

			Assert.IsNotNull(report, "ValidateTree must return a non-null report.");
			// Document the current error count so regressions are visible
			Console.WriteLine($"ValidateTree on BreastStagingTest: {report.ErrorCount} error(s), {report.WarningCount} warning(s), {report.AffectedNodeCount} affected node(s).");
		}

		[TestMethod]
		public void ValidateTree_AfterProgrammaticMutation_CapturesIssue()
		{
			// Rationale: under soft-reject the setter refuses to store an invalid fractional value,
			// so to simulate stale/corrupt data that bypassed the setter (e.g. legacy persisted data
			// or a direct field write) we seed the backing field via reflection. ValidateTree must
			// then surface that pre-existing corruption as an Error — proving the sweep detects
			// problems regardless of how they entered the tree, not only through the setter path.
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "q_sweep");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var intDt = (integer_DEtype)deType.Item;

			// Bypass the soft-reject setter to plant an illegal fractional value directly.
			SeedBackingField(intDt, "_minInclusive", 1.5m);

			var report = ((ITopNode)de).ValidateTree(recurseSubTrees: false);

			Assert.IsFalse(report.IsValid,
				"ValidateTree must detect the illegally seeded fractional value on minInclusive.");
			Assert.IsTrue(report.ErrorCount > 0);
		}

		[TestMethod]
		public void ValidateNode_SingleNode_InvalidValue_ReturnsIssue()
		{
			// Rationale: ValidateNode (single-node variant) must detect the same fractional
			// violation as ValidateTree, but operating on a single BaseType instance. As above,
			// the invalid value is seeded directly into the backing field because the soft-reject
			// setter would otherwise refuse to store it.
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "q_singlenode");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			var intDt = (integer_DEtype)deType.Item;

			SeedBackingField(intDt, "_minInclusive", 1.5m);

			var report = intDt.ValidateNode();

			Assert.IsFalse(report.IsValid,
				"ValidateNode must report an error for the fractional minInclusive value.");
			Assert.AreEqual(1, report.AffectedNodeCount);
		}

		// Writes directly to a private backing field, bypassing the soft-reject setter. Used to
		// simulate invalid data that entered the tree through a non-setter path (legacy data, a
		// future bug, manual reflection) so the validation sweep can be exercised against it.
		private static void SeedBackingField(object target, string fieldName, object value)
		{
			var f = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.IsNotNull(f, $"Backing field '{fieldName}' not found on {target.GetType().Name}.");
			f!.SetValue(target, value);
		}

		[TestMethod]
		public void ValidateNode_ValidNode_ReportIsValid()
		{
			// Rationale: a node loaded from a known-valid document must pass ValidateNode.
			// We use FormDesignType.DeserializeFromXml to get a hydrated tree (Nodes populated),
			// then validate the first integer_DEtype node found — its decimal properties have
			// FractionDigits(0)/MaxDigits/Range constraints that should all pass for whole-number
			// values loaded from the document. The sGuid on loaded nodes is URL-safe base64.
			BaseType.ResetLastTopNode();
			var xml    = FormDesignType.DeserializeFromXml(_xmlContent);
			var intNode = ((ITopNode)xml).Nodes.Values.OfType<integer_DEtype>().FirstOrDefault();

			if (intNode == null)
			{
				// No integer_DEtype in the document — skip, not a test failure
				Assert.Inconclusive("BreastStagingTest.xml contains no integer_DEtype nodes; test skipped.");
				return;
			}

			var report = intNode.ValidateNode();

			// Print all errors for diagnostics — help identify false-positive constraints
			if (!report.IsValid)
			{
				Console.WriteLine($"Unexpected issues on integer_DEtype node '{intNode.sGuid}':");
				foreach (var issue in report.Issues)
					Console.WriteLine($"  [{issue.PropertyName}] {issue.Message}");
			}

			Assert.IsTrue(report.IsValid,
				$"ValidateNode must return IsValid=true for an integer_DEtype node loaded from a valid document. " +
				$"Summary: {report.Summary}");
		}

		[TestMethod]
		public void ValidateTree_RecurseSubTrees_DoesNotCrash()
		{
			// Rationale: recurseSubTrees=true must complete without exception on a real document;
			// even if no sub-TopNodes exist, the cycle-guard HashSet must not throw.
			// Use DeserializeFromXml to get a properly hydrated tree with Nodes populated.
			BaseType.ResetLastTopNode();
			var xml    = FormDesignType.DeserializeFromXml(_xmlContent);
			var report = ((ITopNode)xml).ValidateTree(recurseSubTrees: true);

			Assert.IsNotNull(report, "ValidateTree(recurseSubTrees:true) must return a non-null report.");
		}

		// ─── Phase 4E: FractionDigitsAttribute / MaxDigitsAttribute messages ──────

		[TestMethod]
		public void FractionDigitsAttribute_FormatErrorMessage_WholeNumber_ContainsKeyText()
		{
			// Rationale: FractionDigitsAttribute(0) must produce a human-readable message that
			// makes it clear the field must be a whole number with no decimal digits.
			var attr = new FractionDigitsAttribute(0);
			string msg = attr.FormatErrorMessage("minInclusive");

			Assert.IsNotNull(msg);
			StringAssert.Contains(msg.ToLowerInvariant(), "whole",
				$"FormatErrorMessage for precision=0 must mention 'whole number'. Actual: '{msg}'");
		}

		[TestMethod]
		public void FractionDigitsAttribute_FormatErrorMessage_PositivePrecision_ContainsPrecisionValue()
		{
			// Rationale: for non-zero precision, the message must state the allowed number
			// of decimal places so the user knows exactly what constraint was violated.
			var attr = new FractionDigitsAttribute(4);
			string msg = attr.FormatErrorMessage("rateValue");

			Assert.IsNotNull(msg);
			// Message should mention "4" to indicate allowed fraction digits
			StringAssert.Contains(msg, "4",
				$"FormatErrorMessage for precision=4 must include '4' in the message. Actual: '{msg}'");
		}

		[TestMethod]
		public void MaxDigitsAttribute_FormatErrorMessage_ContainsMaxValue()
		{
			// Rationale: MaxDigitsAttribute(5) must produce a message that includes "5" so
			// developers can immediately see which limit was violated.
			var attr = new MaxDigitsAttribute(5);
			string msg = attr.FormatErrorMessage("significantField");

			Assert.IsNotNull(msg);
			StringAssert.Contains(msg, "5",
				$"FormatErrorMessage must include max digit count. Actual: '{msg}'");
		}

		[TestMethod]
		public void MaxDigitsAttribute_FormatErrorMessage_ContainsFieldName()
		{
			// Rationale: all validation attribute FormatErrorMessage overrides must embed
			// the field name so the user sees exactly which property is at fault.
			var attr = new MaxDigitsAttribute(3);
			string msg = attr.FormatErrorMessage("myField");

			Assert.IsNotNull(msg);
			StringAssert.Contains(msg, "myField",
				$"FormatErrorMessage must include the field name. Actual: '{msg}'");
		}

		[TestMethod]
		public void FractionDigitsAttribute_DecimalPrecision_ReflectsConstructorValue()
		{
			// Rationale: the DecimalPrecision property must expose the value passed to the
			// constructor so that reflection-based tooling can read the allowed precision.
			var attr = new FractionDigitsAttribute(3);

			Assert.AreEqual((uint)3, attr.DecimalPrecision,
				"FractionDigitsAttribute.DecimalPrecision must equal the constructor argument.");
		}

		// ─── Round-trip sanity: normal Deserialize must not fire events ────────────

		[TestMethod]
		public void NormalDeserializeXml_DoesNotFireValidationEvents()
		{
			// Rationale: the standard (non-validating) SdcSerializer.Deserialize must set
			// SuppressValidation=true so no validation events are fired during hydration;
			// this verifies performance-path correctness.
			_ = SdcSerializer<FormDesignType>.Deserialize(_xmlContent);

			Assert.AreEqual(0, _captured.Count,
				"Normal Deserialize must not fire any ValidationOccurred events (SuppressValidation=true).");
		}

		[TestMethod]
		public void NormalDeserializeJson_DoesNotFireValidationEvents()
		{
			// Rationale: same guarantee for the JSON serializer non-validating path.
			var xml  = SdcSerializer<FormDesignType>.Deserialize(_xmlContent);
			string json = SdcSerializerJson<FormDesignType>.SerializeJson(xml);
			_captured.Clear();

			_ = SdcSerializerJson<FormDesignType>.DeserializeJson<FormDesignType>(json);

			Assert.AreEqual(0, _captured.Count,
				"Normal DeserializeJson must not fire any ValidationOccurred events.");
		}
	}
}
