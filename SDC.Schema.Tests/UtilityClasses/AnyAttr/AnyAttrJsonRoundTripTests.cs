using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;
using System.Xml;

namespace SDC.Schema.Tests.UtilityClasses.AnyAttr
{
    /// <summary>
    /// Closes a gap identified while building the SDC OM best-practices guide
    /// (see copilot-context/SDC-Best-Practices/v0.1/guide/04-adhoc-attributes-namespaces.md
    /// and guide/07-known-gaps-and-future-work.md item 2): whether ad-hoc ("any")
    /// attributes hosted on an ExtensionType node survive a JSON round-trip was previously
    /// unverified. XmlAnyAttribute-based storage is a classically JSON-unfriendly pattern
    /// (no native "arbitrary namespaced attribute bag" concept in JSON), so this deserved
    /// explicit verification rather than an assumption either way.
    /// </summary>
    [TestClass]
    public class AnyAttrJsonRoundTripTests
    {
        [TestMethod]
        public void AnyAttr_MixedNamespaces_JsonRoundTrip_CurrentlyThrows_KnownBug()
        {
            // Build a tree the same way as the best-practices guide's Example 03
            // (SDC.Schema.QA.ExampleGenerator/Program.cs): an ExtensionType hosting two
            // ad-hoc attributes in two different namespaces, added via the public
            // AddExtension()/AddOrUpdateAdHocAttribute() API.
            BaseType.ResetLastTopNode();
            var fd = new FormDesignType(null, "FD.AnyAttrJsonRoundTrip");
            fd.AddBody();
            var di = fd.Body.AddChildDisplayedItem("DI.WithAdHocAttrs", "Display item carrying custom ad-hoc attributes");
            var ext = di.AddExtension();

            var doc = new XmlDocument();
            ext.AddOrUpdateAdHocAttribute(doc, "qa", "reviewStatus", "urn:example:qa", "approved");
            ext.AddOrUpdateAdHocAttribute(doc, "cap", "protocolRef", "urn:example:cap", "A & B <legal \"escaped\" content>");

            string json = fd.GetJson();

            // Rationale: this is a CONFIRMED, REAL bug (not merely a theoretical risk),
            // found while writing this test. GetJson() succeeds (ad-hoc attributes ARE
            // present in the serialized JSON text), but DeserializeFromJson() throws
            // Newtonsoft.Json.JsonSerializationException: "XmlNodeConverter only supports
            // deserializing XmlDocument, XmlElement or XmlNode" when it hits the
            // ExtensionType.AnyAttr (List<XmlAttribute>) collection. This test intentionally
            // asserts today's (broken) behavior so the suite stays green while the gap is
            // tracked; see guide/07-known-gaps-and-future-work.md item 2, now upgraded from
            // "unverified" to "confirmed bug." If/when SdcSerializerJson's XmlNodeConverter
            // usage is fixed to handle XmlAttribute (e.g. via a small custom
            // JsonConverter), THIS TEST SHOULD BE REWRITTEN to assert successful round-trip
            // instead (see the sibling AnyAttrJsonRoundTripTests-style assertions removed
            // from this method for the expected-once-fixed shape).
            var ex = Assert.ThrowsExactly<Newtonsoft.Json.JsonSerializationException>(
                () => FormDesignType.DeserializeFromJson(json));
            StringAssert.Contains(ex.InnerException?.Message ?? ex.Message, "XmlNodeConverter");
        }

        [TestMethod]
        public void AnyAttr_MixedNamespaces_BsonRoundTrip_CurrentlyThrows_KnownBug()
        {
            // Same underlying bug as the JSON test above (SdcSerializerBson shares the same
            // Newtonsoft.Json XmlNodeConverter-based approach for List<XmlAttribute>/
            // List<XmlElement>), reached through the BSON serializer instead.
            BaseType.ResetLastTopNode();
            var fd = new FormDesignType(null, "FD.AnyAttrBsonRoundTrip");
            fd.AddBody();
            var di = fd.Body.AddChildDisplayedItem("DI.WithAdHocAttrs", "Display item carrying custom ad-hoc attributes");
            var ext = di.AddExtension();

            var doc = new XmlDocument();
            ext.AddOrUpdateAdHocAttribute(doc, "qa", "reviewStatus", "urn:example:qa", "approved");

            string bson = fd.GetBson();

            var ex = Assert.ThrowsExactly<Newtonsoft.Json.JsonSerializationException>(
                () => FormDesignType.DeserializeFromBson(bson));
            StringAssert.Contains(ex.Message, "XmlNodeConverter");
        }
    }
}
