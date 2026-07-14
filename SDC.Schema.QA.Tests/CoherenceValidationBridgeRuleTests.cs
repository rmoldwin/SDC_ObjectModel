using SDC.Schema.Extensions;
using SDC.Schema.QA.Reporting;
using SDC.Schema.QA.Rules;
using System.Linq;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class CoherenceValidationBridgeRuleTests
{
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
}
