using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.QA.Rules;
using SDC.Schema.QA.Rules.AdHocAttributes;
using System.Xml;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class EmptyExtensionRuleTests
{
    private static FormDesignType CreateFormDesign()
    {
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.EmptyExtension.Tests");
        fd.AddBody();
        return fd;
    }

    [TestMethod]
    public void PopulatedExtension_ProducesNoFindings()
    {
        var fd = CreateFormDesign();
        var di = fd.Body.AddChildDisplayedItem("DI.1", "Item with real ad-hoc data");
        var ext = di.AddExtension();
        var doc = new XmlDocument();
        ext.AddOrUpdateAdHocAttribute(doc, "qa", "reviewStatus", "urn:example:qa", "approved");

        var engine = new QaEngine([new EmptyExtensionRule()]);
        var report = engine.Run(fd, "populated extension");

        // Rationale: an Extension that actually carries ad-hoc data is exactly the
        // recommended pattern from guide/04-adhoc-attributes-namespaces.md and must never
        // be flagged as inert.
        Assert.IsEmpty(report.Findings, "A populated <Extension> element must not be flagged as inert.");
    }

    [TestMethod]
    public void EmptyExtension_IsDetected()
    {
        var fd = CreateFormDesign();
        var di = fd.Body.AddChildDisplayedItem("DI.2", "Item with a forgotten extension");
        var ext = di.AddExtension(); // created, but never populated - the mistake this rule targets

        var engine = new QaEngine([new EmptyExtensionRule()]);
        var report = engine.Run(fd, "empty extension");

        // Rationale: this reproduces exactly the mistake found in this guide's own first
        // draft of Example 03 (an Extension created and then never populated) - the rule
        // exists specifically to catch this class of forgotten follow-up call.
        Assert.HasCount(1, report.Findings);
        Assert.AreEqual("BP-ADH-001", report.Findings[0].RuleId);
        Assert.AreEqual(ext.ObjectGUID, report.Findings[0].NodeObjectGuid);
        Assert.IsTrue(report.Findings[0].IsAutoFixable);
    }

    [TestMethod]
    public void TryFix_RemovesInertExtension()
    {
        var fd = CreateFormDesign();
        var di = fd.Body.AddChildDisplayedItem("DI.3", "Item with a forgotten extension");
        var ext = di.AddExtension();

        var rule = new EmptyExtensionRule();
        var engine = new QaEngine([rule]);
        var report = engine.RunAndAutoFix(fd, "auto-fix empty extension");

        // Rationale: the empty extension carries zero information, so it is always safe to
        // remove automatically - after auto-fix, the tree should report clean and the
        // ExtensionType node should be fully unregistered (same RemoveRecursive contract
        // proven in TreeIntegrityRuleTests.OrphanedNode_IsDetected).
        Assert.IsTrue(report.IsClean);
        Assert.IsFalse(fd.Nodes.ContainsKey(ext.ObjectGUID));
    }
}
