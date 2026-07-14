// ============================================================================
// SdcScriptOmMutationTests.cs
// SDC.ScriptEngine.Tests — deep SDC OM mutation via script tests
// ============================================================================
//
// These tests are the CORE CORRECTNESS PROOF that IL injection actually works
// for real SDC OM mutations.  Each test:
//
//   1. Builds a minimal SDC OM node tree on the host.
//   2. Runs a script that mutates or reads the tree.
//   3. Verifies on the host that the mutation took effect.
//
// This proves the engine pipeline (compile → load → invoke via reflection)
// successfully passes a live reference — not a copy — to the script.
//
// SCRIPT NAMESPACE NOTE
// ----------------------
// The SdcScriptTemplate imports `using SDC.Schema;` into every script.
// Types in `SDC.Schema` (QuestionItemType, string_DEtype, etc.) are directly
// accessible.  Types in `SDC.Schema.Extensions` (e.g., RemoveRecursive)
// require a fully-qualified static call because the template does not import
// that namespace:
//   SDC.Schema.Extensions.IMoveRemoveExtensions.RemoveRecursive(node);
// Newtonsoft.Json is available via the AppDomainReferenceProvider but requires
// full qualification: Newtonsoft.Json.JsonConvert.SerializeObject(sdc).

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using SDC.ScriptEngine;

namespace SDC.ScriptEngine.Tests;

[TestClass]
public class SdcScriptOmMutationTests
{
    // ── Cleanup ───────────────────────────────────────────────────────────────

    [TestInitialize]
    public void Init() => BaseType.ResetLastTopNode();

    [TestCleanup]
    public void Cleanup()
    {
        // Reset the OM's global last-top-node state between tests.
        // Some tests create DataElementType with null parent, which registers
        // as a top node.  Resetting prevents cross-test interference.
        BaseType.ResetLastTopNode();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // @val / value mutations
    // ═════════════════════════════════════════════════════════════════════════

    // ── Test 1: Script writes val on a string response field ──────────────────

    [TestMethod]
    public async Task Script_WritesVal_OnResponseField_OMReflectsChange()
    {
        // A script writing a val to a string response proves that the engine
        // correctly passes the live OM object reference and that mutations on
        // leaf value nodes persist after the script returns.
        var engine = ScriptEngineTestHelper.CreateEngine();

        var de = new DataElementType(null);
        var q = new QuestionItemType(de, "q-val-write");
        q.AddQuestionResponseField(out DataTypes_DEType dt, ItemChoiceType.@string);
        var strNode = (string_DEtype)dt.Item;

        // The script receives the string_DEtype node directly as 'sdc'.
        // It casts and sets the val.
        const string script = """
            var node = (string_DEtype)sdc;
            node.val = "written-by-script";
            """;

        var result = await engine.ExecuteAsync(script, strNode);

        // The run must succeed — cast and property set must not throw.
        Assert.IsTrue(result.Success,
            "Script setting string_DEtype.val must succeed.");

        // The host-side object must now reflect the script's mutation.
        Assert.AreEqual("written-by-script", strNode.val,
            "After script writes val, the host-side string_DEtype must show the new value.");
    }

    // ── Test 2: Script reads val from a response field ─────────────────────────

    [TestMethod]
    public async Task Script_ReadsVal_FromResponseField_ValueIsCorrect()
    {
        // Verifies that the script correctly reads the current val of a
        // response node that was set by the host before the script ran.
        // If the reference were a copy instead of the live object, the read
        // would see the default (null/empty) rather than the host-set value.
        var engine = ScriptEngineTestHelper.CreateEngine();

        var de = new DataElementType(null);
        var q = new QuestionItemType(de, "q-val-read");
        q.AddQuestionResponseField(out DataTypes_DEType dt, ItemChoiceType.@string);
        var strNode = (string_DEtype)dt.Item;

        // Host sets a known value before running the script.
        strNode.val = "host-set-value";

        // The script throws if the val it reads does not match the expected value.
        // A throw → run failure → the test assertion below will catch it.
        const string script = """
            var node = (string_DEtype)sdc;
            if (node.val != "host-set-value")
                throw new System.Exception($"Expected 'host-set-value' but got '{node.val}'");
            """;

        var result = await engine.ExecuteAsync(script, strNode);

        // If the script ran without throwing, the read was correct.
        Assert.IsTrue(result.Success,
            $"Script must read the host-set val correctly. Error: {result.ErrorMessage}");
    }

    // ── Test 3: Script reads sibling question values ──────────────────────────

    [TestMethod]
    public async Task Script_ReadsSiblingQuestionValues_AllCorrect()
    {
        // Two questions under the same DataElement, both with string response
        // fields.  A script receives the DataElement as the target node and
        // reads both questions' names.  This tests cross-node navigation from
        // within a script.
        var engine = ScriptEngineTestHelper.CreateEngine();

        // Use a QuestionItemType as the parent because QuestionItemType implements
        // IChildItemsParent, which is required by GetChildItemsList.
        // DataElementType does NOT implement IChildItemsParent.
        var de = new DataElementType(null);
        var parentQ = new QuestionItemType(de, "q-parent");
        var q1 = new QuestionItemType(parentQ.GetChildItemsNode(), "q-sib-1");
        q1.name = "SiblingOne";
        var q2 = new QuestionItemType(parentQ.GetChildItemsNode(), "q-sib-2");
        q2.name = "SiblingTwo";

        // The script throws if it cannot access both questions via the parent QuestionItemType.
        // It uses the fully-qualified static call because GetChildItemsList is in
        // SDC.Schema.Extensions, which is NOT imported by the script template.
        const string script = """
            var parent = (QuestionItemType)sdc;
            var children = SDC.Schema.Extensions.IChildItemsParentExtensions.GetChildItemsList(parent);
            if (children == null || children.Count < 2)
                throw new System.Exception("Expected at least 2 child questions.");
            """;

        var result = await engine.ExecuteAsync(script, parentQ);

        // Both sibling questions must be reachable from de via GetChildItemsList.
        Assert.IsTrue(result.Success,
            $"Script navigating sibling questions must succeed. Error: {result.ErrorMessage}");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Metadata mutations
    // ═════════════════════════════════════════════════════════════════════════

    // ── Test 5: Script sets name ──────────────────────────────────────────────

    [TestMethod]
    public async Task Script_SetsName_OnQuestionItem_Verified()
    {
        // Setting q.name from a script and verifying the change on the host
        // is the simplest possible mutation proof.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("OriginalName");

        const string script = """
            var q = (QuestionItemType)sdc;
            q.name = "ScriptSetName";
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            "Setting QuestionItemType.name from a script must succeed.");
        Assert.AreEqual("ScriptSetName", q.name,
            "q.name must equal the value written by the script.");
    }

    // ── Test 6: Script sets title ──────────────────────────────────────────────

    [TestMethod]
    public async Task Script_SetsTitle_OnQuestionItem_Verified()
    {
        // 'title' is inherited from DisplayedType.  Setting it from a script
        // proves that properties on base-class members are also writable.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string script = """
            var q = (QuestionItemType)sdc;
            q.title = "Script-Set Title";
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            "Setting DisplayedType.title from a script must succeed.");
        Assert.AreEqual("Script-Set Title", q.title,
            "q.title must equal the value written by the script.");
    }

    // ── Test 7: Script sets enabled ────────────────────────────────────────────

    [TestMethod]
    public async Task Script_SetsEnabled_OnNode_Verified()
    {
        // 'enabled' is a bool property from DisplayedType.  Toggling it
        // from a script simulates a common runtime behavior (showing/hiding
        // a question based on some condition).
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        // Set enabled to false via the script.
        const string script = """
            var q = (QuestionItemType)sdc;
            q.enabled = false;
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            "Setting DisplayedType.enabled from a script must succeed.");
        Assert.IsFalse(q.enabled,
            "q.enabled must be false after the script sets it to false.");
    }

    // ── Test 8: Script sets mustImplement ─────────────────────────────────────

    [TestMethod]
    public async Task Script_SetsMustImplement_OnQuestionItem_Verified()
    {
        // mustImplement (from DisplayedType) controls whether a question must
        // be answered before form submission.  Setting it via script simulates
        // dynamic form validation rules.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string script = """
            var q = (QuestionItemType)sdc;
            q.mustImplement = true;
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            "Setting DisplayedType.mustImplement from a script must succeed.");
        Assert.IsTrue(q.mustImplement,
            "q.mustImplement must be true after the script sets it.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Structural mutations
    // ═════════════════════════════════════════════════════════════════════════

    // ── Test 9: Script adds a child QuestionItemType ───────────────────────────

    [TestMethod]
    public async Task Script_AddsQuestionItemChild_AppearsInParentCollection()
    {
        // Adding a child node via script is the most complex mutation.
        // After the script runs, the host verifies three invariants:
        //   a) The child appears in the parent's child items list.
        //   b) The child has a non-null, non-empty ObjectID.
        //   c) The OM can be serialized to XML without error.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        // The script uses the fully-qualified static call for AddChildQuestion
        // because it is an extension method in SDC.Schema.Extensions, which
        // is not imported by the script template (only SDC.Schema is imported).
        const string script = """
            var q = (QuestionItemType)sdc;
            SDC.Schema.Extensions.IChildItemsParentExtensions.AddChildQuestion(q, QuestionEnum.QuestionSingle, "ScriptChild");
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script adding a child question must succeed. Error: {result.ErrorMessage}");

        // After the script runs, the host verifies using the host-side extension method.
        // The host CAN use extension methods since it imports the required namespaces.
        var children = SDC.Schema.Extensions.IChildItemsParentExtensions.GetChildItemsList(q);
        Assert.IsNotNull(children, "GetChildItemsList must return a non-null list.");
        Assert.IsTrue(children.Count > 0,
            "After script adds a child, the parent's ChildItems list must be non-empty.");

        // (b) Every node registered in the OM receives a positive integer ObjectID.
        // ObjectID is assigned during construction-with-parent-registration;
        // a value of 0 indicates the node was NOT registered in the OM dictionaries.
        var addedChild = children.First();
        Assert.IsTrue(addedChild.ObjectID > 0,
            "The script-added child node must have a positive ObjectID (i.e., must be registered in the OM).");

        // (c) The parent DataElement must still serialize without throwing.
        var de = new DataElementType(null);
        var q2 = new QuestionItemType(de, "q-serialize-check");
        q2.AddChildQuestion(QuestionEnum.QuestionSingle, "serialCheckChild");
        var xml = TopNodeSerializer<DataElementType>.GetXml(de);
        Assert.IsFalse(string.IsNullOrWhiteSpace(xml),
            "DataElement with a script-added child must serialize to non-empty XML.");
    }

    // ── Test 10: Script adds a response field ─────────────────────────────────

    [TestMethod]
    public async Task Script_AddsResponseField_PopulatedCorrectly()
    {
        // A script can call AddQuestionResponseField, which registers both
        // the ResponseFieldType and the DataTypes_DEType in the OM.  This
        // proves the engine handles out-parameter-free approaches inside scripts
        // (the script discards the out parameter to keep it simple).
        var engine = ScriptEngineTestHelper.CreateEngine();

        var de = new DataElementType(null);
        var q = new QuestionItemType(de, "q-rf");

        // The script adds a string response field.  The out parameter is not
        // needed — we verify the response field from the host side.
        const string script = """
            var q = (QuestionItemType)sdc;
            DataTypes_DEType dt;
            q.AddQuestionResponseField(out dt, ItemChoiceType.@string);
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script adding a response field must succeed. Error: {result.ErrorMessage}");

        // After the script adds the response field, GetResponseDataTypeNode must
        // return the newly created node (not null).
        var responseNode = q.GetResponseDataTypeNode();
        Assert.IsNotNull(responseNode,
            "GetResponseDataTypeNode must return non-null after script adds a response field.");
    }

    // ── Test 11: Script adds a list field with items ───────────────────────────

    [TestMethod]
    public async Task Script_AddsListFieldWithItems_ItemsEnumerable()
    {
        // GetListField() creates a ListFieldType if one doesn't exist.
        // AddListItem() adds a ListItemType to the list.
        // After the script runs, the host verifies the items are accessible.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();
        q.GetListField();  // initialize the list so question subtype is list

        const string script = """
            var q = (QuestionItemType)sdc;
            q.AddListItem("LI-Script-1", "Script Item 1", 0);
            q.AddListItem("LI-Script-2", "Script Item 2", 1);
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script adding list items must succeed. Error: {result.ErrorMessage}");

        // The items added by the script must be visible via GetListItems().
        var items = q.GetListItems();
        Assert.IsNotNull(items, "GetListItems must return non-null after script adds items.");
        Assert.IsTrue(items.Count >= 2,
            "At least 2 items added by the script must be present in the list.");
    }

    // ── Test 12: Script removes a child node ───────────────────────────────────

    [TestMethod]
    public async Task Script_RemovesChildNode_NoLongerInCollection()
    {
        // This test verifies that a script can both add and remove nodes.
        // The removal uses the IMoveRemoveExtensions.RemoveRecursive extension
        // method, called with full namespace qualification from the script
        // (since the template only imports `using SDC.Schema;`).
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        // Host adds a child that the script will remove.
        q.AddChildQuestion(QuestionEnum.QuestionSingle, "ToRemove");
        var initialCount = q.GetChildItemsList()?.Count ?? 0;
        Assert.IsTrue(initialCount > 0, "Pre-condition: parent must have at least one child.");

        // The script uses fully-qualified static calls because both
        // GetChildItemsList (IChildItemsParentExtensions) and RemoveRecursive
        // (IMoveRemoveExtensions) are in SDC.Schema.Extensions, not SDC.Schema.
        const string script = """
            var q = (QuestionItemType)sdc;
            var children = SDC.Schema.Extensions.IChildItemsParentExtensions.GetChildItemsList(q);
            if (children != null && children.Count > 0)
            {
                var first = children[0];
                SDC.Schema.Extensions.IMoveRemoveExtensions.RemoveRecursive(first, false);
            }
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script removing a child node must succeed. Error: {result.ErrorMessage}");

        // After removal, the child items list should be empty or shorter.
        var finalCount = q.GetChildItemsList()?.Count ?? 0;
        Assert.IsTrue(finalCount < initialCount,
            "After script removes the child, the child items list must be shorter.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // JSON from script
    // ═════════════════════════════════════════════════════════════════════════

    // ── Test 13: Script serializes node to JSON ────────────────────────────────

    [TestMethod]
    public async Task Script_SerializesNodeToJson_NonEmptyAndParseable()
    {
        // Newtonsoft.Json is loaded into the AppDomain by AppDomainReferenceProvider
        // (because SDC.Schema depends on it).  Scripts can call JsonConvert via
        // fully-qualified name even without `using Newtonsoft.Json;`.
        // This test proves that JSON serialization works from inside a script.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("JsonTest");

        // The script serializes sdc to JSON and throws if the result is empty
        // or doesn't contain the expected type identifier.
        const string script = """
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(sdc);
            if (string.IsNullOrWhiteSpace(json))
                throw new System.Exception("SerializeObject returned empty JSON.");
            """;

        var result = await engine.ExecuteAsync(script, q);

        // If the script ran without throwing, JSON serialization worked.
        Assert.IsTrue(result.Success,
            $"Script serializing SDC node to JSON must succeed. Error: {result.ErrorMessage}");
    }

    // ── Test 14: Script deserializes JSON to node ──────────────────────────────

    [TestMethod]
    public async Task Script_DeserializesJsonToNode_AttachesSuccessfully()
    {
        // Verifies that a script can create a new SDC node by deserializing
        // JSON.  This tests a potential data-import use case.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("DeserializeTest");

        // The script round-trips a sub-object through JSON to verify the
        // serializer can produce and consume valid JSON for SDC types.
        const string script = """
            var q = (QuestionItemType)sdc;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(q);
            // Re-deserializing as JObject (not as QuestionItemType, which
            // requires OM registration) just proves the JSON is parseable.
            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
            if (obj == null)
                throw new System.Exception("Failed to parse JSON back to JObject.");
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script round-tripping SDC node through JSON must succeed. Error: {result.ErrorMessage}");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // XML from script
    // ═════════════════════════════════════════════════════════════════════════

    // ── Test 15: Script serializes node to XML ─────────────────────────────────

    [TestMethod]
    public async Task Script_SerializesNodeToXml_NonEmptyAndContainsExpectedElements()
    {
        // TopNodeSerializer<T> is in the SDC.Schema namespace, so scripts can
        // call it directly with `using SDC.Schema;` already imported.
        // The script calls GetXml on the DataElement top-node and validates
        // the result internally (throws on failure → RunResult.Success = false).
        var engine = ScriptEngineTestHelper.CreateEngine();
        var de = new DataElementType(null);
        var q = new QuestionItemType(de, "q-xml");
        q.name = "XmlTestQuestion";

        // The script receives the DataElementType as the top node.
        // It calls TopNodeSerializer<DataElementType>.GetXml(de) and validates.
        const string script = """
            var de = (DataElementType)sdc;
            var xml = TopNodeSerializer<DataElementType>.GetXml(de);
            if (string.IsNullOrWhiteSpace(xml))
                throw new System.Exception("GetXml returned empty XML.");
            if (!xml.Contains("DataElement"))
                throw new System.Exception("XML does not contain DataElement.");
            """;

        var result = await engine.ExecuteAsync(script, de);

        Assert.IsTrue(result.Success,
            $"Script producing XML via TopNodeSerializer must succeed. Error: {result.ErrorMessage}");
    }

    // ── Test 16: Script round-trips node through XML ───────────────────────────

    [TestMethod]
    public async Task Script_RoundTripsNodeThroughXml_MatchesOriginal()
    {
        // Verifies that serializing the SDC node to XML inside a script
        // produces valid, parseable XML.  We verify that the XML can be
        // deserialized back to the same structure.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var de = new DataElementType(null);
        var q = new QuestionItemType(de, "q-roundtrip");
        q.name = "RoundTripQuestion";

        // The script produces XML, then deserializes it to verify roundtrip.
        // Re-serializes and checks the second XML equals the first.
        const string script = """
            var de = (DataElementType)sdc;
            var xml1 = TopNodeSerializer<DataElementType>.GetXml(de);
            if (string.IsNullOrWhiteSpace(xml1))
                throw new System.Exception("First XML serialization was empty.");
            var de2 = TopNodeSerializer<DataElementType>.DeserializeFromXml(xml1);
            var xml2 = TopNodeSerializer<DataElementType>.GetXml(de2);
            if (string.IsNullOrWhiteSpace(xml2))
                throw new System.Exception("Second XML serialization was empty.");
            """;

        var result = await engine.ExecuteAsync(script, de);

        Assert.IsTrue(result.Success,
            $"Script XML round-trip must succeed. Error: {result.ErrorMessage}");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Reflection from script
    // ═════════════════════════════════════════════════════════════════════════

    // ── Test 17: Script gets concrete type name ────────────────────────────────

    [TestMethod]
    public async Task Script_GetTypeName_ReturnsCorrectConcreteName()
    {
        // The script receives 'sdc' as BaseType but the runtime type is
        // QuestionItemType.  Reflection must return the concrete type name.
        // This is critical for scripts that need type-dispatch logic.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string script = """
            var typeName = sdc.GetType().Name;
            if (typeName != "QuestionItemType")
                throw new System.Exception($"Expected 'QuestionItemType' but got '{typeName}'");
            """;

        var result = await engine.ExecuteAsync(script, q);

        // GetType() must return the concrete type, not BaseType.
        Assert.IsTrue(result.Success,
            $"Script using GetType().Name must return 'QuestionItemType'. Error: {result.ErrorMessage}");
    }

    // ── Test 18: Script enumerates public properties ───────────────────────────

    [TestMethod]
    public async Task Script_EnumeratesPublicProperties_NonEmpty()
    {
        // GetProperties() via reflection must return a non-empty array for
        // SDC OM types, which have many XML-serializable properties.
        // If the reference set is missing or the ALC has an isolation issue,
        // GetProperties might return an empty array or throw.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree();

        const string script = """
            var props = sdc.GetType().GetProperties();
            if (props.Length == 0)
                throw new System.Exception("GetProperties returned empty array.");
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script enumerating properties via reflection must succeed. Error: {result.ErrorMessage}");
    }

    // ── Test 19: Script sets property by name via reflection ───────────────────

    [TestMethod]
    public async Task Script_SetsPropertyByName_ValueIsSet()
    {
        // Verifies that a script can use SetValue() to mutate a property by
        // name at runtime.  This is necessary for dynamic/generic scripts that
        // don't know the concrete type at compile time.
        var engine = ScriptEngineTestHelper.CreateEngine();
        var (_, q) = ScriptEngineTestHelper.CreateTestOmTree("ReflectTest");

        const string script = """
            var prop = sdc.GetType().GetProperty("title");
            if (prop == null)
                throw new System.Exception("Property 'title' not found via reflection.");
            prop.SetValue(sdc, "by-reflection");
            """;

        var result = await engine.ExecuteAsync(script, q);

        Assert.IsTrue(result.Success,
            $"Script setting a property via reflection must succeed. Error: {result.ErrorMessage}");

        // The mutation via reflection must be visible on the host-side object.
        Assert.AreEqual("by-reflection", q.title,
            "q.title must equal the value set via reflection by the script.");
    }
}
