// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace SDC.Schema.Tests.Functional
{
	/// <summary>
	/// Comprehensive tests for the Phase 2 SdcValidationEvents hub, the Phase 3
	/// IsDeserializing-gated setter validation, and the Gap-1/Gap-2 serializer fixes.
	/// </summary>
	[TestClass]
	public class SdcValidationEventsTests
	{
		private List<SdcValidationEventArgs> _captured = new();
		private EventHandler<SdcValidationEventArgs> _handler;
		private static string _xmlPath;

		[ClassInitialize]
		public static void ClassInit(TestContext _)
		{
			_xmlPath = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
		}

		[TestInitialize]
		public void Init()
		{
			_captured = new List<SdcValidationEventArgs>();
			SdcUtil.IsDeserializing.Value = false;
			_handler = (_, e) => _captured.Add(e);
			SdcValidationEvents.ValidationOccurred += _handler;
		}

		[TestCleanup]
		public void Cleanup()
		{
			SdcValidationEvents.ValidationOccurred -= _handler;
			_captured.Clear();
			SdcUtil.IsDeserializing.Value = false;
			BaseType.ResetLastTopNode();
		}

		/// <summary>
		/// Creates a minimal integer_DEtype in a valid SDC node hierarchy.
		/// Clears any events from construction so callers start from a clean slate.
		/// </summary>
		private integer_DEtype CreateIntegerNode()
		{
			var de = new DataElementType(null);
			var q = new QuestionItemType(de, "q_valtest");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.integer);
			_captured.Clear(); // Discard construction noise — tests care only about their own assertions
			return (integer_DEtype)deType.Item;
		}

		/// <summary>Minimal POCO with a Range constraint for isolated ValidateAndRaise testing.</summary>
		private class RangedHelper
		{
			[Range(0, 100, ErrorMessage = "Value must be between 0 and 100")]
			public int Count { get; set; }
		}

		// ─── Hub direct-raise tests ────────────────────────────────────────────────

		[TestMethod]
		public void Raise_EventArgs_SubscriberReceivesExactInstance()
		{
			// Rationale: Raise(SdcValidationEventArgs) must relay the exact args object
			// to every subscribed handler unchanged; no wrapping or copying is permitted.
			var args = new SdcValidationEventArgs
			{
				NodeID        = "node-123",
				PropertyName  = "TestProp",
				AttemptedValue = 99,
				Message       = "Direct raise test",
				Severity      = SdcValidationSeverity.Warning
			};

			SdcValidationEvents.Raise(args);

			Assert.AreEqual(1, _captured.Count, "Exactly one event must be raised by a direct Raise() call.");
			Assert.AreSame(args, _captured[0], "The exact SdcValidationEventArgs instance must arrive at the subscriber.");
			Assert.AreEqual("node-123",                  _captured[0].NodeID);
			Assert.AreEqual("TestProp",                  _captured[0].PropertyName);
			Assert.AreEqual(99,                          _captured[0].AttemptedValue);
			Assert.AreEqual("Direct raise test",         _captured[0].Message);
			Assert.AreEqual(SdcValidationSeverity.Warning, _captured[0].Severity);
		}

		[TestMethod]
		public void Raise_ConvenienceOverload_PopulatesAllFields()
		{
			// Rationale: the string-message convenience overload must correctly map each
			// named parameter to the corresponding SdcValidationEventArgs property.
			SdcValidationEvents.Raise(
				"Convenience overload test",
				nodeID:        "nid-1",
				propertyName:  "PropX",
				attemptedValue: "badVal",
				severity:      SdcValidationSeverity.Error);

			Assert.AreEqual(1, _captured.Count);
			Assert.AreEqual("Convenience overload test", _captured[0].Message);
			Assert.AreEqual("nid-1",                     _captured[0].NodeID);
			Assert.AreEqual("PropX",                     _captured[0].PropertyName);
			Assert.AreEqual("badVal",                    _captured[0].AttemptedValue);
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity);
		}

		// ─── ValidateAndRaise tests ────────────────────────────────────────────────

		[TestMethod]
		public void ValidateAndRaise_InvalidValue_FiresEventWithCorrectContext()
		{
			// Rationale: SdcUtil.ValidateAndRaise must collect DataAnnotations failures and
			// relay them through SdcValidationEvents so subscribers see the full context.
			var obj = new RangedHelper();
			var ctx = new ValidationContext(obj, null, null) { MemberName = nameof(RangedHelper.Count) };

			SdcUtil.ValidateAndRaise(-5, ctx); // -5 violates [Range(0, 100)]

			Assert.AreEqual(1, _captured.Count,
				"Exactly one validation event must fire when one constraint is violated.");
			Assert.AreEqual(nameof(RangedHelper.Count), _captured[0].PropertyName,
				"PropertyName must match the MemberName set on the ValidationContext.");
			Assert.AreEqual(-5, _captured[0].AttemptedValue,
				"AttemptedValue must carry the rejected value so subscribers can log or display it.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity,
				"Validation failures from DataAnnotations must always be reported as Error severity.");
			Assert.IsTrue(_captured[0].Results.Count > 0,
				"Results must contain at least one ValidationResult describing the failure.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(_captured[0].Message),
				"Message must be non-empty and contain the error description.");
		}

		[TestMethod]
		public void ValidateAndRaise_ValidValue_FiresNoEvent()
		{
			// Rationale: the hub must remain silent for values that satisfy every constraint,
			// since spurious events would force subscribers to implement noise filtering.
			var obj = new RangedHelper();
			var ctx = new ValidationContext(obj, null, null) { MemberName = nameof(RangedHelper.Count) };

			SdcUtil.ValidateAndRaise(50, ctx); // 50 is within [0, 100]

			Assert.AreEqual(0, _captured.Count,
				"No validation event must fire when the value satisfies all constraints.");
		}

		// ─── Setter integration tests ──────────────────────────────────────────────

		[TestMethod]
		public void IntegerDEtype_SetFractionalMinInclusive_FiresValidationEvent()
		{
			// Rationale: integer_DEtype.minInclusive carries [FractionDigitsAttribute(0)].
			// Math.Round(1.5, 0) = 2 ≠ 1.5, so the attribute must reject the value
			// and SdcValidationEvents must fire via the Phase 3 setter guard.
			var intDt = CreateIntegerNode();

			intDt.minInclusive = 1.5m;

			Assert.IsTrue(_captured.Count > 0,
				"At least one validation event must fire when a fractional value is set on minInclusive.");
			Assert.AreEqual("minInclusive", _captured[0].PropertyName,
				"PropertyName must identify which property was validated.");
			Assert.AreEqual(1.5m, _captured[0].AttemptedValue,
				"AttemptedValue must carry the rejected decimal.");
		}

		[TestMethod]
		public void IntegerDEtype_SetFractionalMinInclusive_ValueIsAssignedDespiteFailure()
		{
			// Rationale: Q1 decision — assign-and-raise semantics. The backing field must be
			// updated unconditionally so the OM is never left in an inconsistent state after
			// a validation event.
			var intDt = CreateIntegerNode();

			intDt.minInclusive = 1.5m;

			Assert.AreEqual(1.5m, intDt.minInclusive,
				"Assign-and-raise: the value must be stored even when validation fails.");
		}

		[TestMethod]
		public void IntegerDEtype_SetWholeNumberMinInclusive_FiresNoEvent()
		{
			// Rationale: a whole-number decimal satisfies FractionDigitsAttribute(0)
			// (Math.Round(5, 0) == 5); no validation event must fire.
			var intDt = CreateIntegerNode();

			intDt.minInclusive = 5m;

			Assert.AreEqual(0, _captured.Count,
				"No validation event must fire for a whole-number value on integer_DEtype.minInclusive.");
			Assert.AreEqual(5m, intDt.minInclusive, "The valid value must be stored correctly.");
		}

		// ─── IsDeserializing guard tests ───────────────────────────────────────────

		[TestMethod]
		public void IntegerDEtype_SetFractional_WhenIsDeserializingTrue_NoEventFires()
		{
			// Rationale: during deserialization IsDeserializing.Value is true; the Phase 3
			// setter guard must suppress ValidateAndRaise so round-trips never spam subscribers.
			var intDt = CreateIntegerNode();

			SdcUtil.IsDeserializing.Value = true;
			try
			{
				intDt.minInclusive = 1.5m; // would normally fire an event
			}
			finally
			{
				SdcUtil.IsDeserializing.Value = false; // reset before any assertion
			}

			Assert.AreEqual(0, _captured.Count,
				"No validation event must fire while SdcUtil.IsDeserializing is true.");
		}

		[TestMethod]
		public void IntegerDEtype_SetFractional_WhenIsDeserializingTrue_ValueIsStillAssigned()
		{
			// Rationale: suppressing events during deserialization must not prevent the backing
			// field from being updated — the graph must still reconstruct faithfully.
			var intDt = CreateIntegerNode();

			SdcUtil.IsDeserializing.Value = true;
			try
			{
				intDt.minInclusive = 1.5m;
			}
			finally
			{
				SdcUtil.IsDeserializing.Value = false;
			}

			Assert.AreEqual(1.5m, intDt.minInclusive,
				"Backing field must be written even when IsDeserializing suppresses the event.");
		}

		// ─── Serializer round-trip / gap-fix tests ─────────────────────────────────

		[TestMethod]
		public void XmlDeserialization_ValidDocument_FiresNoValidationEvents()
		{
			// Rationale: Gap-1 fix — SdcSerializer.Deserialize() now sets IsDeserializing=true
			// so that valid data loaded from XML does not trigger spurious validation events.
			var fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(_xmlPath);
			Assert.IsNotNull(fd, "FormDesignType must deserialize successfully from BreastStagingTest.xml.");

			Assert.AreEqual(0, _captured.Count,
				"XML deserialization of valid data must produce zero validation events (Gap-1 fix).");
		}

		[TestMethod]
		public void JsonDeserialization_ValidDocument_FiresNoValidationEvents()
		{
			// Rationale: IsDeserializing must be set for the JSON path; valid data round-tripped
			// through JSON must not raise any validation events.
			var fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(_xmlPath);
			string json = SdcSerializerJson<FormDesignType>.SerializeJson(fd);
			_captured.Clear(); // Clear events from XML load
			BaseType.ResetLastTopNode();

			var fd2 = SdcSerializerJson<FormDesignType>.DeserializeJson<FormDesignType>(json);

			Assert.IsNotNull(fd2, "JSON round-trip must produce a non-null FormDesignType.");
			Assert.AreEqual(0, _captured.Count,
				"JSON deserialization of valid data must produce zero validation events.");
		}

		[TestMethod]
		public void JsonDeserialization_OnException_IsDeserializingIsCleared()
		{
			// Rationale: Gap-2 fix — an inner try/finally in SdcSerializerJson.DeserializeJson
			// guarantees that IsDeserializing.Value is cleared even when deserialization throws,
			// preventing permanent suppression of validation on the same async context.
			SdcUtil.IsDeserializing.Value = false;
			Exception caught = null;

			try
			{
				SdcSerializerJson<FormDesignType>.DeserializeJson<FormDesignType>("{{{ intentionally malformed json");
			}
			catch (Exception ex)
			{
				caught = ex;
			}

			Assert.IsNotNull(caught, "Malformed JSON must cause DeserializeJson to throw.");
			Assert.IsFalse(SdcUtil.IsDeserializing.Value,
				"IsDeserializing must be false after a failed JSON deserialization (Gap-2 fix).");
		}

		// ─── Subscription-management tests ────────────────────────────────────────

		[TestMethod]
		public void ValidationOccurred_MultipleSubscribers_AllReceiveEvent()
		{
			// Rationale: ValidationOccurred is a standard C# multicast event; every subscribed
			// handler must be invoked — adding a second subscriber must not replace the first.
			var secondCapture = new List<SdcValidationEventArgs>();
			EventHandler<SdcValidationEventArgs> secondHandler = (_, e) => secondCapture.Add(e);
			SdcValidationEvents.ValidationOccurred += secondHandler;
			try
			{
				SdcValidationEvents.Raise("multicast test", severity: SdcValidationSeverity.Warning);

				Assert.AreEqual(1, _captured.Count,
					"Primary subscriber must receive the event exactly once.");
				Assert.AreEqual(1, secondCapture.Count,
					"Secondary subscriber must also receive the event exactly once.");
				Assert.AreEqual("multicast test", secondCapture[0].Message,
					"Both subscribers must receive the same message.");
			}
			finally
			{
				SdcValidationEvents.ValidationOccurred -= secondHandler;
			}
		}

		[TestMethod]
		public void ValidationOccurred_AfterUnsubscribe_HandlerNotInvoked()
		{
			// Rationale: -= removes a delegate from a multicast event; the removed handler
			// must not be invoked on subsequent raises.
			SdcValidationEvents.ValidationOccurred -= _handler; // unsubscribe before raising
			_captured.Clear();

			SdcValidationEvents.Raise("should not be received");

			Assert.AreEqual(0, _captured.Count,
				"A handler removed with -= must not be invoked on subsequent Raise() calls.");

			// Re-subscribe so TestCleanup's -= doesn't attempt a redundant removal
			// (harmless in .NET, but cleaner to keep symmetry).
			SdcValidationEvents.ValidationOccurred += _handler;
		}

		// ─── AddDataTypesDE StoreError integration test ────────────────────────────

		[TestMethod]
		public void AddDataTypesDE_InvalidByteValue_FiresValidationEventViaStoreError()
		{
			// Rationale: IDataHelpers.AddDataTypesDE.StoreError routes parse failures through
			// SdcValidationEvents.Raise(), so callers that subscribe to the hub see type-parse
			// errors without needing to pass the optional errors out-parameter.
			var de = new DataElementType(null);
			var q  = new QuestionItemType(de, "q_storeerror");
			q.AddQuestionResponseField(out DataTypes_DEType deType, ItemChoiceType.@string);
			var rf = q.ResponseField_Item;
			_captured.Clear(); // Discard construction events

			// "not_a_byte" cannot be parsed as sbyte → StoreError fires → SdcValidationEvents.Raise
			IDataHelpers.AddDataTypesDE(rf, ItemChoiceType.@byte, value: "not_a_byte");

			Assert.IsTrue(_captured.Count > 0,
				"At least one SdcValidationEvent must fire when AddDataTypesDE cannot parse the value.");
			Assert.AreEqual(SdcValidationSeverity.Error, _captured[0].Severity,
				"Parse failures reported via StoreError must have Error severity.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(_captured[0].Message),
				"The event message must describe the parse failure.");
		}
	}
}
