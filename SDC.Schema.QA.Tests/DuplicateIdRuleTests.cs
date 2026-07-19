using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.QA.Rules;
using SDC.Schema.QA.Rules.Mutation;
using System;
using System.Linq;

namespace SDC.Schema.QA.Tests;

[TestClass]
public class DuplicateIdRuleTests
{
    private static FormDesignType CreateFormDesign()
    {
        BaseType.ResetLastTopNode();
        var fd = new FormDesignType(null, "FD.QA.DuplicateId.Tests");
        fd.AddBody();
        return fd;
    }

    [TestMethod]
    public void UniqueIds_ProducesNoFindings()
    {
        var fd = CreateFormDesign();
        fd.Body.AddChildDisplayedItem("DI.1", "First item");
        fd.Body.AddChildDisplayedItem("DI.2", "Second item");

        var engine = new QaEngine([new DuplicateIdRule()]);
        var report = engine.Run(fd, "unique ids");

        // Rationale: two distinct ids on two distinct nodes is exactly the normal, correct
        // construction pattern - this must never be flagged.
        Assert.IsEmpty(report.Findings, "Nodes with distinct IDs must not be flagged.");
    }

    [TestMethod]
    public void DuplicateId_ViaBuilderApi_ThrowsInsteadOfSilentlyDuplicating()
    {
        var fd = CreateFormDesign();
        fd.Body.AddChildDisplayedItem("DI.DUP", "First item");

        // Rationale: this is a "positive finding" worth proving directly (see
        // guide/07-known-gaps-and-future-work.md item 18's precedent for BP-GEN-001) -
        // IdentifiedExtensionType.ID's setter maintains a tree-wide _UniqueIDs set and throws
        // InvalidOperationException the moment a second node's ID setter would collide with an
        // existing one, as long as TopNode is already wired up (true for the public Add*
        // builder path). So duplicate IDs can never silently arise through ordinary
        // construction/mutation - this rule's real value is catching duplicates that survive
        // an unreconciled deserialization (see DeserializedTree_WithDuplicateIds_IsDetected
        // below), not builder-time mistakes, which are already rejected loudly by the library
        // itself.
        Assert.Throws<InvalidOperationException>(() =>
            fd.Body.AddChildDisplayedItem("DI.DUP", "Second item reusing the same id"));
    }

    [TestMethod]
    public void DuplicateId_ViaDeserialization_AlsoThrows()
    {
        // Rationale: a second "positive finding" alongside the builder-API test above. One
        // might expect XML deserialization to be a backdoor around the ID setter's uniqueness
        // check (nodes are constructed via the parameterless constructor, and an internal code
        // comment on the setter itself speculates TopNode may still be null at that point).
        // Empirically it is not a backdoor: XmlSerializer wires each node's ParentNode/TopNode
        // up as it walks the document, so by the time a later sibling's ID is assigned, TopNode
        // is already non-null and the same uniqueness check fires - deserializing a
        // hand-corrupted document with two elements sharing an ID throws
        // InvalidOperationException (wrapped in an XmlSerializer InvalidOperationException)
        // instead of silently producing a tree with a duplicate ID. Combined with the test
        // above and Graft()'s documented ID-reassignment behavior (guide/02-mutation.md), this
        // means BP-MUT-001's trigger condition appears unreachable through any legitimate
        // public API path in this library - the same "defensive, not routinely-firing" role
        // BP-GEN-001 plays for orphaned nodes (see TreeIntegrityRuleTests.OrphanedNode_IsDetected
        // and guide/07-known-gaps-and-future-work.md item 18).
        var fd = CreateFormDesign();
        fd.Body.AddChildDisplayedItem("DI.1", "First item");
        fd.Body.AddChildDisplayedItem("DI.2", "Second item");
        string xml = fd.GetXml();

        string corruptedXml = xml.Replace("ID=\"DI.2\"", "ID=\"DI.1\"");
        StringAssert.Contains(corruptedXml, "ID=\"DI.1\"");
        Assert.DoesNotContain("ID=\"DI.2\"", corruptedXml);

        Assert.Throws<InvalidOperationException>(() =>
            FormDesignType.DeserializeFromXml(corruptedXml, refreshSdc: false));
    }

    [TestMethod]
    public void RepeatedSection_ViaCopy_DoesNotCollide()
    {
        var fd = CreateFormDesign();
        var section = fd.Body.AddChildSection("SEC.1", "Repeatable section");

        bool copied = section.Copy();

        var engine = new QaEngine([new DuplicateIdRule()]);
        var report = engine.Run(fd, "legitimate repeat via Copy()");

        // Rationale: Copy() (guide/02-mutation.md, "Repeating a section or question") is the
        // sanctioned way to duplicate a subtree, and it auto-suffixes the clone's ID with
        // "__N" specifically so this rule's check never fires for legitimate repeats - only
        // for accidental hand-duplicated IDs. This distinguishes the good pattern from the bad
        // one the rule targets.
        Assert.IsTrue(copied, "Precondition: Copy() must succeed on a freshly-built section.");
        Assert.IsEmpty(report.Findings, "A legitimate Copy()-produced repeat must not be flagged as a duplicate ID.");
    }
}
