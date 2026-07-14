using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.QA.Rules;
using SDC.Schema.QA.Rules.Validation;
using System.Linq;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class UnresolvedRejectedValuesRuleTests
{
    private static FormDesignType CreateFormDesign()
    {
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.RejectedValues.Tests");
        fd.AddBody();
        return fd;
    }

    [TestMethod]
    public void NoRejectedValues_ProducesNoFindings()
    {
        var fd = CreateFormDesign();
        fd.Body.AddChildQuestionResponse("Q.CLEAN", out var deType, "Clean question", dt: ItemChoiceType.integer);
        var intDt = (integer_DEtype)deType.DataTypeDE_Item!;

        // A whole-number value satisfies integer_DEtype.minInclusive's [FractionDigitsAttribute(0)]
        // constraint, so nothing should be rejected here.
        intDt.minInclusive = 5m;

        var engine = new QaEngine([new UnresolvedRejectedValuesRule()]);
        var report = engine.Run(fd, "no rejected values");

        // Rationale: a tree where every set value passed validation is exactly the healthy
        // case - it must never be flagged.
        Assert.IsEmpty(report.Findings, "A tree with no rejected values must not be flagged.");
    }

    [TestMethod]
    public void UnresolvedRejectedValue_IsDetected()
    {
        var fd = CreateFormDesign();
        fd.Body.AddChildQuestionResponse("Q.BAD", out var deType, "Question with a bad set attempt", dt: ItemChoiceType.integer);
        var intDt = (integer_DEtype)deType.DataTypeDE_Item!;

        // integer_DEtype.minInclusive carries [FractionDigitsAttribute(0)]; 1.5 has a
        // fractional part, so the soft-reject contract must silently drop it and record it
        // in RejectedValues instead of storing it or throwing (see
        // SdcValidationEventsTests.IntegerDEtype_SetFractionalMinInclusive_ValueIsSoftRejected,
        // which proves this same setter behavior directly against the underlying node).
        intDt.minInclusive = 1.5m;

        var engine = new QaEngine([new UnresolvedRejectedValuesRule()]);
        var report = engine.Run(fd, "unresolved rejected value");

        // Rationale: this reproduces exactly the "silently dropped and never looked at again"
        // scenario the rule exists to catch - the soft-reject contract by design never throws
        // or corrupts the tree, so nothing else would surface this to an agent/human unless
        // they specifically inspect RejectedValues (which is precisely what this rule
        // automates across the whole tree).
        Assert.HasCount(1, report.Findings);
        Assert.AreEqual("BP-VAL-001", report.Findings[0].RuleId);
        Assert.AreEqual(intDt.ObjectGUID, report.Findings[0].NodeObjectGuid);
        StringAssert.Contains(report.Findings[0].Message, "minInclusive");
        Assert.IsFalse(report.Findings[0].IsAutoFixable,
            "Clearing a rejected value without a real fix would just hide the problem - no safe auto-fix.");
    }

    [TestMethod]
    public void ClearingRejectedValue_RemovesTheFinding()
    {
        var fd = CreateFormDesign();
        fd.Body.AddChildQuestionResponse("Q.CLEARED", out var deType, "Question that gets corrected", dt: ItemChoiceType.integer);
        var intDt = (integer_DEtype)deType.DataTypeDE_Item!;
        intDt.minInclusive = 1.5m;

        // Simulate the recommended remediation from the guide: the caller notices the
        // rejection and explicitly clears it (e.g. after supplying a corrected value, or
        // after deliberately deciding to accept the prior value as final).
        intDt.ClearRejectedValues();

        var engine = new QaEngine([new UnresolvedRejectedValuesRule()]);
        var report = engine.Run(fd, "cleared rejected value");

        // Rationale: once RejectedValues is explicitly cleared via the public
        // ClearRejectedValue(s) API, the rule must stop firing - proving the rule tracks live
        // node state rather than some cached snapshot, and that the guide's recommended
        // remediation path actually resolves the finding.
        Assert.IsEmpty(report.Findings, "Clearing the rejected value must remove the finding.");
    }
}
