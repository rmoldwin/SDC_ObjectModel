using SDC.Schema;
using SDC.Schema.Extensions;
using System.Xml;

// SDC OM Best Practices Guide — Example Generator
//
// This console program is the canonical source of every XML/JSON snippet that appears in
// the SDC-Best-Practices/v0.1 guide. Snippets are NEVER hand-authored: they are produced by
// actually constructing and serializing real SDC.Schema OM trees through the public API, so
// the guide can never drift from what the compiled DLL actually does. Re-run this program
// (see HOW-TO-REBUILD.md) any time SDC.Schema changes and the guide needs refreshing.
//
// Output root defaults to the sibling copilot-context examples folder; override with the
// first command-line argument for local/dev runs.

string outputRoot = args.Length > 0
    ? args[0]
    : @"C:\Users\RMold\OneDrive\One Drive Documents\Source\caporg\copilot-context\SDC-Best-Practices\v0.1\examples";

string xmlDir = Path.Combine(outputRoot, "xml");
string jsonDir = Path.Combine(outputRoot, "json");
Directory.CreateDirectory(xmlDir);
Directory.CreateDirectory(jsonDir);

void WriteXml(string name, string xml) => File.WriteAllText(Path.Combine(xmlDir, name + ".xml"), xml);
void WriteJson(string name, string json) => File.WriteAllText(Path.Combine(jsonDir, name + ".json"), json);

Console.WriteLine($"Writing generated examples to: {outputRoot}");

// ─────────────────────────────────────────────────────────────────────────
// Example 01 — Basic construction/hydration: Header/Body/Footer, one
// single-select question with two list-item choices, one display-only item.
// Demonstrates the canonical Add* fluent builder pattern.
// ─────────────────────────────────────────────────────────────────────────
{
    BaseType.ResetLastTopNode();
    var fd = new FormDesignType(null, "FD.Example01.BasicForm");
    fd.AddHeader();
    fd.AddBody();
    fd.AddFooter();

    var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.FavoriteColor", "What is your favorite color?");
    q.AddListItem("LI.Red", "Red");
    q.AddListItem("LI.Blue", "Blue");
    fd.Body.AddChildDisplayedItem("DI.Instructions", "Please select one option.");

    fd.AssignElementNamesByReflection();

    WriteXml("01-construction-basic-form", fd.GetXml());
    WriteJson("01-construction-basic-form", fd.GetJson());
    Console.WriteLine("  [ok] 01-construction-basic-form (xml + json)");
}

// ─────────────────────────────────────────────────────────────────────────
// Example 02 — Mutation: move a ListItem from one Question's list to
// another Question's list using the public Move() API (never direct
// property assignment — see the mutation guide for why).
// ─────────────────────────────────────────────────────────────────────────
{
    BaseType.ResetLastTopNode();
    var fd = new FormDesignType(null, "FD.Example02.MoveListItem");
    fd.AddBody();

    var q1 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Source", "Source question");
    var moving = q1.AddListItem("LI.Movable", "This choice will move");
    q1.AddListItem("LI.Stays", "This choice stays put");

    var q2 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Target", "Target question");
    q2.AddListItem("LI.AlreadyThere", "Existing choice in target");

    fd.AssignElementNamesByReflection();
    WriteXml("02-mutation-before-move", fd.GetXml());

    // Move the "movable" list item into q2's list, appending it at the end (-1).
    var targetList = q2.ListField_Item!.List!;
    bool moved = moving.Move(targetList, -1);

    fd.AssignElementNamesByReflection();
    WriteXml("02-mutation-after-move", fd.GetXml());
    Console.WriteLine($"  [ok] 02-mutation-before-move / after-move (moved={moved})");
}

// ─────────────────────────────────────────────────────────────────────────
// Example 03 — Ad-hoc ("any") attributes with mixed namespaces.
//
// IMPORTANT DISCOVERY (kept here deliberately, not papered over): ad-hoc
// XmlAnyAttribute storage lives ONLY on ExtensionType — the object behind
// the <Extension> sub-element — never directly on DisplayedItem/Section/
// Question/etc. CanHostAdHocAttributes() returns false for ordinary nodes.
// You must first call ebt.AddExtension() (any ExtensionBaseType-derived
// node can host one or more <Extension> children), then add ad-hoc
// attributes to THAT ExtensionType object. This mirrors the real usage in
// SDC.Schema.Tests/UtilityClasses/AnyAttr/AnyAttrScenarioTests.cs.
// ─────────────────────────────────────────────────────────────────────────
{
    BaseType.ResetLastTopNode();
    var fd = new FormDesignType(null, "FD.Example03.AdHocAttributes");
    fd.AddBody();
    var di = fd.Body.AddChildDisplayedItem("DI.WithAdHocAttrs", "Display item carrying custom ad-hoc attributes");

    Console.WriteLine($"  di.CanHostAdHocAttributes() = {di.CanHostAdHocAttributes()}  (expected: False — DisplayedType has no AnyAttr slot itself)");

    var ext = di.AddExtension();
    Console.WriteLine($"  ext.CanHostAdHocAttributes() = {ext.CanHostAdHocAttributes()}  (expected: True — ExtensionType defines the AnyAttr slot)");

    var doc = new XmlDocument();
    ext.AddOrUpdateAdHocAttribute(doc, prefix: "qa", localName: "reviewStatus", namespaceUri: "urn:example:qa", value: "approved");
    ext.AddOrUpdateAdHocAttribute(doc, prefix: "cap", localName: "protocolRef", namespaceUri: "urn:example:cap", value: "A & B <legal \"escaped\" content>");

    fd.AssignElementNamesByReflection();
    WriteXml("03-adhoc-attributes-mixed-namespaces", fd.GetXml());
    Console.WriteLine("  [ok] 03-adhoc-attributes-mixed-namespaces (xml only — see guide for JSON/BSON gap)");
}

Console.WriteLine("Done.");

// ─────────────────────────────────────────────────────────────────────────
// Sample QA/QC report generation — runs every currently-implemented
// SDC.Schema.QA rule against two trees (one "healthy," one deliberately
// containing the specific violations each rule targets) and writes the
// resulting Markdown + single-file HTML reports into reports/, so the guide
// always ships a live, regenerable example of what QA/QC output looks like.
// ─────────────────────────────────────────────────────────────────────────
{
    string reportsDir = Path.Combine(outputRoot, "..", "reports");
    Directory.CreateDirectory(reportsDir);

    var rules = new List<SDC.Schema.QA.Rules.IQaRule>
    {
        new SDC.Schema.QA.Rules.Construction.TreeIntegrityRule(),
        new SDC.Schema.QA.Rules.AdHocAttributes.EmptyExtensionRule(),
        new SDC.Schema.QA.Rules.Serialization.NoInternalStateInJsonRule(),
    };

    // Healthy tree: built purely through the public Add* builder API, no
    // deliberate defects.
    BaseType.ResetLastTopNode();
    var healthy = new FormDesignType(null, "FD.QaReportSample.Healthy");
    healthy.AddBody();
    var hq = healthy.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Healthy", "A healthy question");
    hq.AddListItem("LI.A", "Choice A");
    hq.AddListItem("LI.B", "Choice B");
    var hExt = healthy.Body.AddExtension();
    var hDoc = new XmlDocument();
    hExt.AddOrUpdateAdHocAttribute(hDoc, "qa", "reviewStatus", "urn:example:qa", "approved");

    var engine = new SDC.Schema.QA.Rules.QaEngine(rules);
    var healthyReport = engine.Run(healthy, "FD.QaReportSample.Healthy (expected: BP-SER-001 only — see guide/03)");

    // Defective tree: deliberately includes an empty/inert <Extension> element
    // (BP-ADH-001), in addition to the unavoidable BP-SER-001 TreeRwLock finding every
    // tree currently produces. Note: BP-GEN-001 (orphaned-but-registered node) is NOT
    // reproduced here deliberately — it turns out to be difficult/impossible to trigger
    // through the public API alone, since Move()/RemoveRecursive() both maintain tree
    // integrity by design. That is itself a useful confirmation that the public mutation
    // API is hard to misuse into an orphaned-node state; BP-GEN-001 remains a defensive
    // check for hypothetical future API additions or edge cases, not a commonly-hit rule.
    BaseType.ResetLastTopNode();
    var defective = new FormDesignType(null, "FD.QaReportSample.Defective");
    defective.AddBody();
    var dq = defective.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Defective", "A question with defects nearby");
    dq.AddListItem("LI.X", "Choice X");
    var di = defective.Body.AddChildDisplayedItem("DI.EmptyExt", "Item with a forgotten extension");
    di.AddExtension(); // created but never populated -> BP-ADH-001

    var defectiveReport = engine.Run(defective, "FD.QaReportSample.Defective (deliberately includes BP-ADH-001-triggering content; see code comment on why BP-GEN-001 is not reproduced here)");

    File.WriteAllText(Path.Combine(reportsDir, "sample-report-healthy.md"), healthyReport.ToMarkdown());
    File.WriteAllText(Path.Combine(reportsDir, "sample-report-healthy.html"), healthyReport.ToHtml());
    File.WriteAllText(Path.Combine(reportsDir, "sample-report-defective.md"), defectiveReport.ToMarkdown());
    File.WriteAllText(Path.Combine(reportsDir, "sample-report-defective.html"), defectiveReport.ToHtml());

    Console.WriteLine($"  [ok] sample QA reports written to {Path.GetFullPath(reportsDir)}");
}
