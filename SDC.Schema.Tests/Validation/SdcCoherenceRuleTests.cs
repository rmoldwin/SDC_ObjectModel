// SDC-CUSTOM: do not overwrite
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.Validation
{
	[TestClass]
	public class SdcCoherenceRuleTests
	{
		[TestCleanup]
		public void Cleanup()
		{
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
	}
}
