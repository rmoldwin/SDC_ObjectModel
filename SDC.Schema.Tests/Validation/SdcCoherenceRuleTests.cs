// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace SDC.Schema.Tests.Validation
{
	[TestClass]
	public class SdcCoherenceRuleTests
	{
		[TestCleanup]
		public void Cleanup()
		{
			SdcCoherenceRuleRegistry.Unregister(TestOnlyAlwaysFiresRule.TestRuleId);
			SdcUtil.SuppressValidation.Value = false;
			SdcUtil.ValidationCollector.Value = null;
			SdcUtil.IsDeserializing.Value = false;
			BaseType.ResetLastTopNode();
		}

		[TestMethod]
		public void ValidateTree_ReportsSdacWarning_WhenSelectedSdacListItemHasDescendantCapturedValue()
		{
			// Rationale: the built-in SDAC rule must run through ValidateTree end-to-end and emit
			// exactly one Warning tagged with RuleCode=SDAC when a selected SDAC list item would
			// deactivate a descendant response that already contains user-entered data.
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Coherence.SDAC");
			fd.AddBody();

			var parentQuestion = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent question");
			var sdacListItem = parentQuestion.AddListItem("LI.SDAC", "Disable descendants");
			sdacListItem.selectionDisablesChildren = true;

			sdacListItem.AddChildQuestionResponse("Q.Child", out var descendantResponseDataType, "Child question", dt: ItemChoiceType.@string);
			var descendantValue = (string_DEtype)descendantResponseDataType.Item;
			descendantValue.val = "captured";
			sdacListItem.selected = true;

			var report = fd.ValidateTree();
			var sdacIssues = report.Issues.Where(i => i.RuleCode == "SDAC").ToList();
			var warningIssues = report.Issues.Where(i => i.Severity == SdcValidationSeverity.Warning).ToList();

			Assert.AreEqual(1, warningIssues.Count,
				"The smoke tree should produce exactly one warning so the SDAC signal is unambiguous even if unrelated validation errors are also present.");
			Assert.AreEqual(1, report.Issues.Count(i => i.RuleCode == "SDAC" && i.Severity == SdcValidationSeverity.Warning),
				"ValidateTree must emit exactly one Warning tagged with RuleCode=SDAC for the selected SDAC list item.");
			Assert.AreEqual(SdcValidationSeverity.Warning, sdacIssues[0].Severity,
				"The built-in SDAC rule must report Warning severity when descendant data is at risk.");
		}

		[TestMethod]
		public void ValidateTree_DoesNotReportSdacWarning_WhenSelectedSdacListItemHasNoDescendantUserData()
		{
			// Rationale: SDAC is a data-at-risk warning, not a generic notice that an SDAC list item
			// was selected. An empty descendant subtree must therefore stay silent so callers can trust
			// that every SDAC finding represents real captured data that might be orphaned.
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Coherence.SDAC.Empty");
			fd.AddBody();

			var parentQuestion = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent question");
			var sdacListItem = parentQuestion.AddListItem("LI.SDAC.Empty", "Disable descendants");
			sdacListItem.selectionDisablesChildren = true;
			sdacListItem.AddChildQuestionResponse("Q.Child.Empty", out _, "Child question", dt: ItemChoiceType.@string);
			sdacListItem.selected = true;

			var report = fd.ValidateTree();

			Assert.AreEqual(0, report.Issues.Count(i => i.RuleCode == "SDAC"),
				"Selecting an SDAC list item with no descendant user data must not produce a false-positive SDAC finding.");
		}

		[TestMethod]
		public void ValidateTree_ReportsSdsWarning_WhenSelectedSdsListItemWouldDeselectSelectedSiblingWithCapturedData()
		{
			// Rationale: SDS is only hazardous when the triggering selection would immediately clear a
			// sibling that is itself currently selected and already holds user-entered descendant data.
			// This test locks in the intended positive case and confirms the message names both list items.
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Coherence.SDS.Positive");
			fd.AddBody();

			var parentQuestion = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent question");
			var triggeringListItem = parentQuestion.AddListItem("LI.A", "Primary choice");
			triggeringListItem.selectionDeselectsSiblings = true;
			triggeringListItem.selected = true;

			var selectedSibling = parentQuestion.AddListItem("LI.B", "Sibling at risk");
			selectedSibling.selected = true;
			selectedSibling.AddChildQuestionResponse("Q.B.Child", out var descendantResponseDataType, "Sibling child question", dt: ItemChoiceType.@string);
			((string_DEtype)descendantResponseDataType.Item).val = "captured";

			var report = fd.ValidateTree();
			var sdsIssues = report.Issues.Where(i => i.RuleCode == "SDS").ToList();

			Assert.AreEqual(1, report.Issues.Count(i => i.RuleCode == "SDS" && i.Severity == SdcValidationSeverity.Warning),
				"The smoke tree should yield exactly one SDS warning so the single sibling-at-risk path stays unambiguous.");
			Assert.AreEqual(1, report.Issues.Count(i => i.Severity == SdcValidationSeverity.Warning),
				"No extra warnings should appear in this focused tree; otherwise the SDS proof signal becomes ambiguous.");
			Assert.AreEqual(SdcValidationSeverity.Warning, sdsIssues[0].Severity,
				"The built-in SDS rule must report Warning severity when a selected sibling's data is at risk.");
			StringAssert.Contains(sdsIssues[0].Message, "LI.A",
				"The SDS message should identify the triggering selected list item so callers know what action caused the risk.");
			StringAssert.Contains(sdsIssues[0].Message, "LI.B",
				"The SDS message should also identify the sibling that would be auto-deselected so the user can review the at-risk branch.");
		}

		[TestMethod]
		public void ValidateTree_DoesNotReportSdsWarning_WhenAtRiskSiblingHasCapturedDataButIsNotSelected()
		{
			// Rationale: the recent false-positive fix intentionally limits SDS to siblings that are
			// currently selected, because only those siblings would actually be auto-deselected. Stored
			// data under an unselected sibling is unaffected and must not trigger an SDS warning.
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Coherence.SDS.Negative");
			fd.AddBody();

			var parentQuestion = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent question");
			var triggeringListItem = parentQuestion.AddListItem("LI.A", "Primary choice");
			triggeringListItem.selectionDeselectsSiblings = true;
			triggeringListItem.selected = true;

			var unselectedSibling = parentQuestion.AddListItem("LI.B", "Sibling not selected");
			unselectedSibling.selected = false;
			unselectedSibling.AddChildQuestionResponse("Q.B.Child", out var descendantResponseDataType, "Sibling child question", dt: ItemChoiceType.@string);
			((string_DEtype)descendantResponseDataType.Item).val = "captured";

			var report = fd.ValidateTree();

			Assert.AreEqual(0, report.Issues.Count(i => i.RuleCode == "SDS"),
				"An unselected sibling is not actually auto-deselected by SDS, so descendant data under that branch must not produce an SDS false positive.");
		}

		[TestMethod]
		public void ValidateTree_ExecutesAdHocRegisteredRule_AndStopsAfterUnregister()
		{
			// Rationale: the registry exists to let callers plug in arbitrary coherence logic that has
			// nothing to do with the built-in SDAC/SDS rules. This test proves a custom rule flows through
			// ValidateTree like a first-class built-in and that unregistering it restores isolation.
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Coherence.CustomRule");
			fd.AddBody();
			var question = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Custom", "Custom question");
			question.AddListItem("LI.Custom", "Custom choice");

			SdcCoherenceRuleRegistry.Register(new TestOnlyAlwaysFiresRule());
			try
			{
				var report = fd.ValidateTree();
				var customIssue = report.Issues.Single(i => i.RuleCode == TestOnlyAlwaysFiresRule.TestRuleId);

				Assert.AreEqual(SdcValidationSeverity.Info, customIssue.Severity,
					"The throwaway test rule should preserve its own severity when ValidateTree surfaces it.");
				Assert.AreEqual(1, report.Issues.Count(i => i.RuleCode == TestOnlyAlwaysFiresRule.TestRuleId),
					"A custom registered rule should emit exactly one issue in this proof tree so the registry behavior is unambiguous.");
				StringAssert.Contains(customIssue.Message, "Custom test-only coherence rule fired.",
					"The emitted issue should preserve the custom rule's message rather than rewriting it as a built-in warning.");
			}
			finally
			{
				SdcCoherenceRuleRegistry.Unregister(TestOnlyAlwaysFiresRule.TestRuleId);
			}

			var postUnregisterReport = fd.ValidateTree();
			Assert.AreEqual(0, postUnregisterReport.Issues.Count(i => i.RuleCode == TestOnlyAlwaysFiresRule.TestRuleId),
				"Unregister must remove the test-only rule so later tests do not inherit leaked registry state.");
		}

		private sealed class TestOnlyAlwaysFiresRule : ISdcCoherenceRule
		{
			public const string TestRuleId = "TEST-ADHOC-001";

			public string RuleId => TestRuleId;

			public IEnumerable<SdcNodeValidationIssue> Evaluate(ITopNode topNode)
			{
				var sourceNode = topNode as BaseType ?? topNode.Nodes.Values.OfType<BaseType>().First();
				yield return new SdcNodeValidationIssue
				{
					RuleCode = RuleId,
					NodeID = sourceNode.sGuid,
					NodeType = sourceNode.GetType().Name,
					PropertyName = "CustomRule",
					Message = "Custom test-only coherence rule fired.",
					Severity = SdcValidationSeverity.Info
				};
			}
		}
	}
}
