using SDC.Schema.Extensions;
using SDC.Schema.QA.Reporting;
using SDC.Schema.QA.Rules;
using System.Collections.Generic;
using System.Linq;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class CoherenceValidationBridgeRuleTests
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
    public void DefaultRuleSet_BridgesSdacWarningsIntoQaReport()
    {
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.Coherence.Bridge");
        fd.AddBody();

        var parentQuestion = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Parent", "Parent question");
        var sdacListItem = parentQuestion.AddListItem("LI.SDAC", "Disable descendants");
        sdacListItem.selectionDisablesChildren = true;

        sdacListItem.AddChildQuestionResponse("Q.Child", out var responseField, "Child question", dt: ItemChoiceType.@string);
        var descendantValue = (string_DEtype)responseField.Item;
        descendantValue.val = "captured";
        sdacListItem.selected = true;

        var engine = new QaEngine();
        var report = engine.Run(fd, "default QA rule set coherence bridge");
        var finding = report.Findings.Single(f => f.RuleId == "BP-VAL-002");

        // Rationale: BP-VAL-002 exists solely to prove the core-library coherence rule
        // pipeline and the QA pipeline stay in sync without re-implementing SDAC logic in the
        // QA project. A default QaEngine run over an SDAC-at-risk tree must therefore surface
        // exactly one bridged warning with the original node identity/message preserved.
        Assert.IsTrue(engine.Rules.Any(r => r.RuleId == "BP-VAL-002"),
            "The default QaEngine rule set must include BP-VAL-002 so coherence findings surface without custom wiring.");
        Assert.AreEqual(1, report.Findings.Count(f => f.RuleId == "BP-VAL-002"),
            "This smoke tree should trigger exactly one bridged QA finding from the single SDAC-at-risk selection.");
        Assert.AreEqual(QaSeverity.Warning, finding.Severity,
            "SDAC/SDS coherence findings are warning-level by design and must stay warnings after bridging into QA.");
        Assert.AreEqual(sdacListItem.sGuid, finding.NodeId,
            "The bridge should preserve the original SdcNodeValidationIssue node identifier rather than invent a new one.");
        Assert.AreEqual(sdacListItem.ObjectGUID, finding.NodeObjectGuid,
            "The bridge should also resolve the live node's ObjectGUID so callers can navigate back to the offending node.");
        StringAssert.Contains(finding.Message, "LI.SDAC");
        StringAssert.Contains(finding.Message, "selectionDisablesChildren=true");
        StringAssert.Contains(finding.Recommendation!, "SDAC");
        StringAssert.Contains(finding.Recommendation!, sdacListItem.GetType().Name);
        Assert.IsFalse(finding.IsAutoFixable,
            "The bridge cannot safely auto-decide whether the user intended to keep or undo an SDAC/SDS selection.");
    }

    [TestMethod]
    public void DefaultRuleSet_BridgesAdHocRegisteredCoherenceRuleIntoQaReport()
    {
        // Rationale: BP-VAL-002 should bridge every ValidateTree coherence finding, including
        // custom rules registered entirely outside the built-in SDAC/SDS set, so clients only
        // author one rule implementation and still see it in both validation surfaces.
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.Coherence.CustomBridge");
        fd.AddBody();
        var question = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Custom", "Custom question");
        question.AddListItem("LI.Custom", "Custom choice");

        SdcCoherenceRuleRegistry.Register(new TestOnlyAlwaysFiresRule());
        try
        {
            var engine = new QaEngine();
            var report = engine.Run(fd, "default QA rule set custom coherence bridge");
            var finding = report.Findings.Single(f => f.RuleId == "BP-VAL-002" && f.Recommendation!.Contains(TestOnlyAlwaysFiresRule.TestRuleId));

            Assert.AreEqual(QaSeverity.Info, finding.Severity,
                "The QA bridge must preserve the custom rule's original severity mapping instead of flattening everything to Warning.");
            Assert.AreEqual(fd.sGuid, finding.NodeId,
                "The bridged QA finding should preserve the original custom rule node id so report consumers can navigate to the source node.");
            Assert.AreEqual(fd.ObjectGUID, finding.NodeObjectGuid,
                "The bridge should resolve the custom rule's short Guid back to the live node ObjectGUID just like built-in SDAC/SDS findings.");
            StringAssert.Contains(finding.Message, "Custom test-only coherence rule fired.");
            StringAssert.Contains(finding.Recommendation!, TestOnlyAlwaysFiresRule.TestRuleId);
        }
        finally
        {
            SdcCoherenceRuleRegistry.Unregister(TestOnlyAlwaysFiresRule.TestRuleId);
        }
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
