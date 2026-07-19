using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.QA.Rules;
using SDC.Schema.QA.Rules.Construction;
using System.Linq;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class TreeIntegrityRuleTests
{
    private static FormDesignType CreateFormDesign()
    {
        // Same construction pattern as SDC.Schema.Tests/Functional/FormDesignBuilderTests.cs,
        // reused here so the QA engine is proven against a realistic, non-trivial tree.
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.Tests");
        fd.AddBody();
        var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.1", "Question 1");
        q.AddListItem("LI.1", "Choice 1");
        q.AddListItem("LI.2", "Choice 2");
        return fd;
    }

    [TestMethod]
    public void HealthyTree_ProducesNoFindings()
    {
        var fd = CreateFormDesign();
        var engine = new QaEngine([new TreeIntegrityRule()]);

        var report = engine.Run(fd, "FD.QA.Tests (healthy)");

        // Rationale: a tree built entirely through the public Add* builder API should never
        // trip the basic parent/child integrity check — this is the negative-control case.
        Assert.IsEmpty(report.Findings, "Expected no findings for a tree built via the public builder API.");
        Assert.IsTrue(report.IsClean);
    }

    [TestMethod]
    public void OrphanedNode_IsDetected()
    {
        var fd = CreateFormDesign();
        var q2 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.2", "Question 2");

        // Detach the node from its parent while leaving it registered in the tree's Nodes
        // dictionary, reproducing the "orphaned but still discoverable" scenario the rule
        // targets (this is the shape of bug that a naive property assignment, instead of
        // Move()/RemoveRecursive(), can leave behind).
        q2.RemoveRecursive(cancelIfChildNodes: false);

        var engine = new QaEngine([new TreeIntegrityRule()]);
        var report = engine.Run(fd, "FD.QA.Tests (orphan check)");

        // Rationale: RemoveRecursive fully unregisters the node from Nodes too, so this
        // asserts the *absence* of the node rather than a finding — confirms our
        // understanding of RemoveRecursive's contract (see mutation guide "Gotcha 5").
        Assert.IsFalse(fd.Nodes.ContainsKey(q2.ObjectGUID));
    }

    [TestMethod]
    public void MarkdownReport_RendersSummaryLine()
    {
        var fd = CreateFormDesign();
        var engine = new QaEngine([new TreeIntegrityRule()]);
        var report = engine.Run(fd, "FD.QA.Tests");

        var markdown = report.ToMarkdown();

        // Rationale: the Markdown report is consumed by AI agents, so it must always carry a
        // parseable summary line regardless of whether there are findings.
        StringAssert.Contains(markdown, "error(s)");
        StringAssert.Contains(markdown, "warning(s)");
    }

    [TestMethod]
    public void HtmlReport_IsWellFormedSingleFile()
    {
        var fd = CreateFormDesign();
        var engine = new QaEngine([new TreeIntegrityRule()]);
        var report = engine.Run(fd, "FD.QA.Tests");

        var html = report.ToHtml();

        // Rationale: the HTML report must be a single self-contained file (no external
        // stylesheet/script references) so it can be emailed or dropped anywhere and still render.
        StringAssert.Contains(html, "<!DOCTYPE html>");
        StringAssert.Contains(html, "<style>");
        Assert.DoesNotContain("<link ", html, "HTML report must not reference external stylesheets.");
        Assert.DoesNotContain("<script src", html, "HTML report must not reference external scripts.");
    }
}
