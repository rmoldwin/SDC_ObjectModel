using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.Tests.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace SDC.Schema.Tests.UtilityClasses.AnyAttr
{
    [TestClass]
    public class AnyAttrScenarioTests
    {
        private static string AnyAttrFixtureFolder => Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Test Files", "AnyAttr Scenarios"));

        private static string ReadFixture(string fileName)
            => File.ReadAllText(Path.Combine(AnyAttrFixtureFolder, fileName));

        [TestMethod]
        public void AnyAttr_Fixtures_Deserialize_AsSchemaValidTopNodes()
        {
            // Rationale:
            // The AnyAttr test suite depends on fixture XML files being structurally valid for the SDC object model.
            // This test is the first guardrail: if any fixture cannot deserialize, downstream AnyAttr behavior tests are not meaningful.
            string[] fixtureNames =
            {
                "AnyAttr_Base_NoCustom.xml",
                "AnyAttr_Add_Custom.xml",
                "AnyAttr_Change_Custom.xml",
                "AnyAttr_Remove_Custom.xml"
            };

            foreach (string fixture in fixtureNames)
            {
                string xml = ReadFixture(fixture);
                FormDesignType fd = FormDesignType.DeserializeFromXml(xml);
                Assert.IsNotNull(fd, $"Fixture '{fixture}' must deserialize into FormDesignType.");
            }
        }

        [TestMethod]
        public void AnyAttr_CanBeAddedEditedRemoved_AndRoundTripped()
        {
            // Rationale:
            // FormDesignType does not itself expose XmlAnyAttribute storage, so AnyAttr behavior is tested on ExtensionType,
            // which is the schema-defined ad-hoc extension carrier. This verifies add/edit/remove and round-trip persistence.
            string baseXml = ReadFixture("AnyAttr_Base_NoCustom.xml");
            FormDesignType fd = FormDesignType.DeserializeFromXml(baseXml);

            var ext = fd.Body.AddExtension();

            XmlDocument doc = new XmlDocument();
            XmlElement markerElement = doc.CreateElement("qa", "Marker", "urn:sdc:test:anyattr");
            markerElement.InnerText = "extension-node";
            ext.Any ??= new List<XmlElement>();
            ext.Any.Add(markerElement);

            XmlAttribute qaAttr = doc.CreateAttribute("qa", "fdFlag", "urn:sdc:test:anyattr");
            qaAttr.Value = "added";
            ext.AnyAttr ??= new List<XmlAttribute>();
            ext.AnyAttr.Add(qaAttr);

            string xmlAdded = fd.GetXml(refreshSdc: false);
            FormDesignType fdAfterAdd = FormDesignType.DeserializeFromXml(xmlAdded);
            ExtensionType extAfterAdd = fdAfterAdd.Body.Extension!.First();
            Assert.IsTrue(extAfterAdd.AnyAttr?.Any(a => a.LocalName == "fdFlag" && a.Value == "added") == true,
                "Round-trip serialization must preserve newly added XmlAnyAttribute values on ExtensionType nodes.");

            XmlAttribute attrToEdit = extAfterAdd.AnyAttr!.First(a => a.LocalName == "fdFlag");
            attrToEdit.Value = "changed";
            string xmlChanged = fdAfterAdd.GetXml(refreshSdc: false);
            FormDesignType fdAfterEdit = FormDesignType.DeserializeFromXml(xmlChanged);
            ExtensionType extAfterEdit = fdAfterEdit.Body.Extension!.First();
            Assert.IsTrue(extAfterEdit.AnyAttr?.Any(a => a.LocalName == "fdFlag" && a.Value == "changed") == true,
                "Round-trip serialization must preserve edited XmlAnyAttribute values on ExtensionType nodes.");

            XmlAttribute attrToRemove = extAfterEdit.AnyAttr!.First(a => a.LocalName == "fdFlag");
            extAfterEdit.AnyAttr.Remove(attrToRemove);
            string xmlRemoved = fdAfterEdit.GetXml(refreshSdc: false);
            FormDesignType fdAfterRemove = FormDesignType.DeserializeFromXml(xmlRemoved);
            ExtensionType extAfterRemove = fdAfterRemove.Body.Extension!.First();
            Assert.IsFalse(extAfterRemove.AnyAttr?.Any(a => a.LocalName == "fdFlag") == true,
                "Round-trip serialization must remove XmlAnyAttribute entries that were deleted in memory.");
        }

        [TestMethod]
        public void AnyAttr_Retrieval_IsIncludedInSerializedAttributeLists()
        {
            // Rationale:
            // This test encodes the target behavior: ad-hoc XmlAnyAttribute values should be discoverable
            // through the same serialized-attribute retrieval APIs used by comparison/reporting logic.
            string xml = ReadFixture("AnyAttr_Add_Custom.xml");
            FormDesignType fd = FormDesignType.DeserializeFromXml(xml);

            // Add a real ExtensionType AnyAttr payload in-memory so this test remains aligned with the schema model.
            var ext = fd.Body.AddExtension();
            XmlDocument doc = new XmlDocument();
            XmlElement markerElement = doc.CreateElement("qa", "Marker", "urn:sdc:test:anyattr");
            markerElement.InnerText = "extension-node";
            ext.Any = new List<XmlElement> { markerElement };
            XmlAttribute qaAttr = doc.CreateAttribute("qa", "fdFlag", "urn:sdc:test:anyattr");
            qaAttr.Value = "added";
            ext.AnyAttr = new List<XmlAttribute> { qaAttr };

            var extAttributes = ext.GetXmlAttributesSerialized();

            // Rationale:
            // XmlAnyAttribute lives on ExtensionType, so serialized-attribute retrieval must expose
            // each ad-hoc attribute as an AttributeInfo entry on that node.
            bool foundAnyAttr = extAttributes
                .Any(ai => ai.Name?.Contains("fdFlag") == true && ai.Value?.ToString() == "added");

            Assert.IsTrue(foundAnyAttr,
                "Serialized attribute retrieval should include AnyAttr names/values once feature work is complete.");
        }

        [TestMethod]
        public void AnyAttr_NodeLevel_CanHostAndHasFilled_AreDetectedCorrectly()
        {
            // Rationale:
            // Node-level helpers should distinguish between a node that can host XmlAnyAttribute entries
            // and a node that currently has one or more populated ad-hoc attributes.
            FormDesignType fd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Base_NoCustom.xml"));
            var ext = fd.Body.AddExtension();

            Assert.IsFalse(fd.CanHostAdHocAttributes(),
                "FormDesignType should not report ad-hoc attribute host capability when it has no XmlAnyAttribute slot.");
            Assert.IsFalse(fd.HasFilledAdHocAttributes(),
                "FormDesignType should not report filled ad-hoc attributes when it cannot host them.");

            Assert.IsTrue(ext.CanHostAdHocAttributes(),
                "ExtensionType should report ad-hoc attribute host capability because it defines XmlAnyAttribute storage.");
            Assert.IsFalse(ext.HasFilledAdHocAttributes(),
                "ExtensionType should report no filled ad-hoc attributes before AnyAttr values are assigned.");

            XmlDocument doc = new XmlDocument();
            XmlAttribute qaAttr = doc.CreateAttribute("qa", "fdFlag", "urn:sdc:test:anyattr");
            qaAttr.Value = "added";
            ext.AnyAttr = new List<XmlAttribute> { qaAttr };

            Assert.IsTrue(ext.HasFilledAdHocAttributes(),
                "ExtensionType should report filled ad-hoc attributes once AnyAttr contains at least one value.");
        }

        [TestMethod]
        public void AnyAttr_CompareTrees_Detects_AddChangeRemove_Differences()
        {
            // Rationale:
            // CompareTrees behavior must eventually treat AnyAttr additions/changes/removals
            // as serialized-attribute diffs, so metadata consumers get consistent change reporting.
            FormDesignType baseFd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Base_NoCustom.xml"));
            FormDesignType addFd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Add_Custom.xml"));
            FormDesignType changeFd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Change_Custom.xml"));
            FormDesignType removeFd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Remove_Custom.xml"));

            // Attach AnyAttr-bearing ExtensionType nodes to each tree so CompareTrees assertions target real schema extension points.
            AddExtensionAnyAttr(baseFd, null);
            AddExtensionAnyAttr(addFd, "added");
            AddExtensionAnyAttr(changeFd, "changed");
            AddExtensionAnyAttr(removeFd, null);

            var compareAdd = new CompareTrees<FormDesignType>(baseFd, addFd);
            var compareChange = new CompareTrees<FormDesignType>(addFd, changeFd);
            var compareRemove = new CompareTrees<FormDesignType>(addFd, removeFd);

            DifNodeIET? addDif = compareAdd.GetIETattributes(addFd.sGuid);
            DifNodeIET? changeDif = compareChange.GetIETattributes(changeFd.sGuid);
            DifNodeIET? removeDif = compareRemove.GetIETattributes(removeFd.sGuid);

            Assert.IsTrue(addDif?.isAttListChanged == true, "AnyAttr additions should be reported as attribute-list changes.");
            Assert.IsTrue(changeDif?.isAttListChanged == true, "AnyAttr edits should be reported as attribute-list changes.");
            Assert.IsTrue(removeDif?.isAttListChanged == true, "AnyAttr removals should be reported as attribute-list changes.");
        }

        [TestMethod]
        public void AnyAttr_TreeLevel_Detection_FindsCustomAttributesAcrossTopNode()
        {
            // Rationale:
            // Callers need a top-node convenience API that answers whether any custom ad-hoc attributes
            // exist anywhere in the tree without manually traversing nodes.
            FormDesignType fd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Base_NoCustom.xml"));

            Assert.IsFalse(fd.TreeHasAnyFilledAdHocAttributes(),
                "A tree with no AnyAttr assignments should report no filled ad-hoc attributes.");

            AddExtensionAnyAttr(fd, "tree-flag");

            Assert.IsTrue(fd.TreeHasAnyFilledAdHocAttributes(),
                "A tree should report filled ad-hoc attributes when any descendant node contains AnyAttr values.");
        }

        [TestMethod]
        public void AnyAttr_AttributeInfo_ExposesIsAdHocAttributeCorrectly()
        {
            // Rationale:
            // AttributeInfo should provide a reliable ad-hoc origin indicator so callers can distinguish
            // XmlAnyAttribute-derived entries from schema-defined attribute entries.
            FormDesignType fd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Base_NoCustom.xml"));
            var ext = fd.Body.AddExtension();

            XmlDocument doc = new XmlDocument();
            XmlAttribute qaAttr = doc.CreateAttribute("qa", "fdFlag", "urn:sdc:test:anyattr");
            qaAttr.Value = "added";
            ext.AnyAttr = new List<XmlAttribute> { qaAttr };

            var extAttributes = ext.GetXmlAttributesSerialized();
            AttributeInfo adhocAttribute = extAttributes.First(ai => ai.Name == "fdFlag");

            // Rationale:
            // An XmlAnyAttribute-derived entry should report ad-hoc origin.
            Assert.IsTrue(adhocAttribute.IsAdHocAttribute,
                "AttributeInfo should identify XmlAnyAttribute-derived entries as ad-hoc attributes.");

            AttributeInfo schemaAttribute = fd.GetXmlAttributesSerialized().First();
            Assert.IsFalse(schemaAttribute.IsAdHocAttribute,
                "Schema-based attributes should not be flagged as ad-hoc in AttributeInfo.");
        }

        [TestMethod]
        public void AnyAttr_AttributeInfo_UsesExplicitPerAttributeFlag_NotNodeLevelState()
        {
            // Rationale:
            // This regression test covers the corrected issue: when a node has filled AnyAttr values,
            // schema-based attributes on the same node must still remain non-ad-hoc.
            FormDesignType fd = FormDesignType.DeserializeFromXml(ReadFixture("AnyAttr_Base_NoCustom.xml"));
            var ext = fd.Body.AddExtension();

            ext.type = "ExtensionWithMixedAttributes";

            XmlDocument doc = new XmlDocument();
            XmlAttribute qaAttr = doc.CreateAttribute("qa", "fdFlag", "urn:sdc:test:anyattr");
            qaAttr.Value = "added";
            ext.AnyAttr = new List<XmlAttribute> { qaAttr };

            var extAttributes = ext.GetXmlAttributesSerialized();

            AttributeInfo schemaAttribute = extAttributes.First(ai => ai.Name == "type");
            AttributeInfo adHocAttribute = extAttributes.First(ai => ai.Name == "fdFlag");

            Assert.IsFalse(schemaAttribute.IsAdHocAttribute,
                "Schema-defined attributes on a node with AnyAttr entries must remain non-ad-hoc.");
            Assert.IsTrue(adHocAttribute.IsAdHocAttribute,
                "XmlAnyAttribute-derived entries on the same node must be flagged as ad-hoc.");
        }

        private static void AddExtensionAnyAttr(FormDesignType fd, string? flagValue)
        {
            var ext = fd.Body.AddExtension();
            XmlDocument doc = new XmlDocument();
            XmlElement markerElement = doc.CreateElement("qa", "Marker", "urn:sdc:test:anyattr");
            markerElement.InnerText = "extension-node";
            ext.Any = new List<XmlElement> { markerElement };

            if (flagValue is not null)
            {
                XmlAttribute qaAttr = doc.CreateAttribute("qa", "fdFlag", "urn:sdc:test:anyattr");
                qaAttr.Value = flagValue;
                ext.AnyAttr = new List<XmlAttribute> { qaAttr };
            }
        }
    }
}
