using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.QA.Rules;
using SDC.Schema.QA.Rules.Serialization;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class NoInternalStateInJsonRuleTests
{
    [TestMethod]
    public void CurrentSerializer_LeaksTreeRwLock_KnownGap()
    {
        // Rationale: this test intentionally documents a CURRENT, known upstream gap
        // (see guide/03-serialization-roundtrip.md and guide/07-known-gaps-and-future-work.md
        // item 4): GetJson() currently includes the internal TreeRwLock
        // (ReaderWriterLockSlim) object in its output. This test is expected to PASS
        // (assert the finding IS present) today; if a future SDC.Schema fix adds
        // [JsonIgnore] to TreeRwLock, this test should be updated to assert the finding is
        // gone, and the guide's known-gaps entry should be updated to "resolved."
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.Serialization.Tests");
        fd.AddBody();
        fd.Body.AddChildDisplayedItem("DI.1", "Simple item");
        fd.AssignElementNamesByReflection();

        var engine = new QaEngine([new NoInternalStateInJsonRule()]);
        var report = engine.Run(fd, "TreeRwLock leak check");

        Assert.HasCount(1, report.Findings);
        Assert.AreEqual("BP-SER-001", report.Findings[0].RuleId);
        Assert.IsFalse(report.Findings[0].IsAutoFixable, "No safe external auto-fix exists for this upstream serialization gap.");
    }
}
