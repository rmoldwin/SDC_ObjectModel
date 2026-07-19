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
    ///
    /// GitHub issue #27 tracked a confirmed bug here: GetJson()/GetBson() succeeded, but
    /// DeserializeFromJson()/DeserializeFromBson() always threw
    /// Newtonsoft.Json.JsonSerializationException ("XmlNodeConverter only supports
    /// deserializing XmlDocument, XmlElement or XmlNode") whenever ExtensionType.AnyAttr
    /// (List&lt;XmlAttribute&gt;) was populated. Root cause: Newtonsoft's built-in
    /// XmlNodeConverter activates automatically for any XmlNode-derived type; its WriteJson
    /// supports XmlAttribute but its ReadJson explicitly does not. Fixed by registering
    /// XmlAttributeListJsonConverter (SDC.Schema/SDC Customized Classes/SDC Serializers/) in
    /// SdcSerializerJson/SdcSerializerBson, which fully replaces XmlNodeConverter's handling
    /// of List&lt;XmlAttribute&gt; for both read and write. These tests now assert the fixed,
    /// successful round-trip.
    /// </summary>
    [TestClass]
    public class AnyAttrJsonRoundTripTests
    {
        [TestMethod]
        public void AnyAttr_MixedNamespaces_JsonRoundTrip_Succeeds()
        {
            // Build a tree the same way as the best-practices guide's Example 03
            // (SDC.Schema.QA.ExampleGenerator/Program.cs): an ExtensionType hosting two
            // ad-hoc attributes in two different namespaces, added via the public
            // AddExtension()/AddOrUpdateAdHocAttribute() API.
            BaseType.ResetLastTopNode();
            var fd = new FormDesignType(null, "FD.AnyAttrJsonRoundTrip");
            fd.AddBody();
            var ext = fd.Body.AddExtension();

            var doc = new XmlDocument();
            ext.AddOrUpdateAdHocAttribute(doc, "qa", "reviewStatus", "urn:example:qa", "approved");
            ext.AddOrUpdateAdHocAttribute(doc, "cap", "protocolRef", "urn:example:cap", "A & B <legal \"escaped\" content>");

            string json = fd.GetJson();

            // Rationale: prior to the issue #27 fix, this threw JsonSerializationException.
            // With XmlAttributeListJsonConverter registered, deserialization now succeeds and
            // both ad-hoc attributes (including mixed namespaces and escaped/illegal XML
            // content) must survive the round-trip intact.
            var rehydrated = FormDesignType.DeserializeFromJson(json);
            var rehydratedExt = rehydrated.Body!.Extension!.First();

            Assert.IsNotNull(rehydratedExt.AnyAttr);
            Assert.AreEqual(2, rehydratedExt.AnyAttr.Count);

            var reviewStatus = rehydratedExt.AnyAttr.First(a => a.LocalName == "reviewStatus");
            Assert.AreEqual("qa", reviewStatus.Prefix);
            Assert.AreEqual("urn:example:qa", reviewStatus.NamespaceURI);
            Assert.AreEqual("approved", reviewStatus.Value);

            // Rationale: this attribute's value contains "&", "<", and escaped quotes — exactly
            // the kind of content that would reveal any escaping bug introduced by a hand-rolled
            // JSON converter. Asserting the value round-trips byte-for-byte confirms the
            // converter handles illegal/special XML content correctly, not just the happy path.
            var protocolRef = rehydratedExt.AnyAttr.First(a => a.LocalName == "protocolRef");
            Assert.AreEqual("cap", protocolRef.Prefix);
            Assert.AreEqual("urn:example:cap", protocolRef.NamespaceURI);
            Assert.AreEqual("A & B <legal \"escaped\" content>", protocolRef.Value);
        }

        [TestMethod]
        public void AnyAttr_MixedNamespaces_BsonRoundTrip_Succeeds()
        {
            // Same underlying fix as the JSON test above (SdcSerializerBson registers the same
            // XmlAttributeListJsonConverter for List<XmlAttribute>), reached through the BSON
            // serializer instead.
            BaseType.ResetLastTopNode();
            var fd = new FormDesignType(null, "FD.AnyAttrBsonRoundTrip");
            fd.AddBody();
            var ext = fd.Body.AddExtension();

            var doc = new XmlDocument();
            ext.AddOrUpdateAdHocAttribute(doc, "qa", "reviewStatus", "urn:example:qa", "approved");

            string bson = fd.GetBson();

            var rehydrated = FormDesignType.DeserializeFromBson(bson);
            var rehydratedExt = rehydrated.Body!.Extension!.First();

            Assert.IsNotNull(rehydratedExt.AnyAttr);
            Assert.AreEqual(1, rehydratedExt.AnyAttr.Count);

            var reviewStatus = rehydratedExt.AnyAttr.First(a => a.LocalName == "reviewStatus");
            Assert.AreEqual("qa", reviewStatus.Prefix);
            Assert.AreEqual("urn:example:qa", reviewStatus.NamespaceURI);
            Assert.AreEqual("approved", reviewStatus.Value);
        }
    }
}
