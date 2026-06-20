using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace SDC.Schema.Tests.Functional.Serialization
{
	[TestClass]
	public class SdcSerializationTests
	{
		[TestMethod]
		public void DeserializeDEFromPath()
		{
			BaseType.ResetLastTopNode();
			//string path = @".\Test files\DE sample.xml";
			string path = Path.Combine("..", "..", "..", "Test files", "DE sample.xml");
			//string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
			DataElementType DE = TopNodeSerializer<DataElementType>.DeserializeFromXmlPath(path);
			var myXML = DE.GetXml();
			var myJson = DE.GetJson();
			Debug.Print(myXML);
			Debug.Print(myJson);

			// Rationale: deserialization must produce a non-null DE and both serializers must produce non-empty output.
			Assert.IsNotNull(DE, "DeserializeFromXmlPath must return a non-null DataElementType.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myXML), "GetXml() must return non-empty XML.");
			Assert.IsTrue(myXML.Contains("DataElement"), "Serialized XML must contain the DataElement root element name.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myJson), "GetJson() must return non-empty JSON.");
		}
		[TestMethod]
		public void DeserializeDEFromXml()
		{
			BaseType.ResetLastTopNode();
			//string path = @".\Test files\DE sample.xml";
			string path = Path.Combine("..", "..", "..", "Test files", "DE sample.xml");
			string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
			DataElementType DE = TopNodeSerializer<DataElementType>.DeserializeFromXml(sdcFile);
			var myXML = DE.GetXml();
			Debug.Print(myXML);

			// Rationale: deserializing from raw XML string must produce the same non-null DE and valid re-serialized XML.
			Assert.IsNotNull(DE, "DeserializeFromXml must return a non-null DataElementType.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myXML), "GetXml() must return non-empty XML.");
			Assert.IsTrue(myXML.Contains("DataElement"), "Re-serialized XML must contain the DataElement root element name.");
		}
		[TestMethod]
		public void DeserializeDemogFormDesignFromPath()
		{
			Setup.TimerStart("==>[] Started");


			BaseType.ResetLastTopNode();
			//string path = @".\Test files\Demog CCO Lung Surgery.xml";

			string path = Path.Combine("..", "..", "..", "Test files", "Demog CCO Lung Surgery.xml");
			//if (!File.Exists(path)) path = @"/Test files/Demog CCO Lung Surgery.xml";
			//string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
			DemogFormDesignType FD = TopNodeSerializer<DemogFormDesignType>.DeserializeFromXmlPath(path);
			var myXML = TopNodeSerializer<DemogFormDesignType>.GetXml(FD);
			Debug.Print(myXML);
			//Debug.Print(FD.GetJson());
			var doc = new XmlDocument();
			doc.LoadXml(myXML);
			var json = JsonConvert.SerializeXmlNode(doc);
			Debug.Print(json);
			doc = JsonConvert.DeserializeXmlNode(json);
			Debug.Print(doc?.OuterXml);
			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");

			// Rationale: deserialization and Newtonsoft JSON round-trip must all produce non-null, non-empty output.
			Assert.IsNotNull(FD, "DeserializeFromXmlPath must return a non-null DemogFormDesignType.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myXML), "GetXml() must return non-empty XML.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(json), "JsonConvert.SerializeXmlNode must return non-empty JSON.");
			Assert.IsNotNull(doc, "JsonConvert.DeserializeXmlNode must return a non-null XmlDocument.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(doc?.OuterXml), "Round-tripped XmlDocument.OuterXml must be non-empty.");
		}
		[TestMethod]
		public void DeserializeFormDesignFromPathSimple()
		{
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
			var FD = FormDesignType.DeserializeFromXmlPath(path);
			var myXml = SdcSerializer<FormDesignType>.Serialize(FD);
			Debug.Print(myXml);
			var myJson = SdcSerializerJson<FormDesignType>.SerializeJson(FD);
			Debug.Print(myJson);

			// Rationale: both XML and JSON serializers must produce non-empty output for a deserialized FormDesign.
			Assert.IsNotNull(FD, "DeserializeFromXmlPath must return a non-null FormDesignType.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myXml), "SdcSerializer<FormDesignType>.Serialize must return non-empty XML.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myJson), "SdcSerializerJson<FormDesignType>.SerializeJson must return non-empty JSON.");
		}
		[TestMethod]
		public void DeserializeFormDesignFromPath()
		{
			BaseType.ResetLastTopNode();
			//string path = @".\Test files\CCO Lung Surgery.xml";
			//string path = @".\Test files\Breast.Invasive.Staging.359_.CTP9_sdcFDF.xml";
			string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			//string path = @".\Test files\Adrenal.Bx.Res.129_3.004.001.REL_sdcFDF_test.xml";
			string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);

			var FD = FormDesignType.DeserializeFromXmlPath(path);
			//SDC.Schema.FormDesignType FD = SDC.Schema.FormDesignType.DeserializeSdcFromFile(sdcFile);
			string myXML;
			//myXML =  SdcSerializer<FormDesignType>.Serialize(FD);

			//Test adding and reading FD object model
			QuestionItemType Q = (QuestionItemType)FD.Nodes.Values.Where(
				t => t.GetType() == typeof(QuestionItemType)).Where(
				q => ((QuestionItemType)q).ID == "58218.100004300").FirstOrDefault()!;

			Assert.IsTrue(Q.ListField_Item.maxSelections == 1, $"maxSelections must be '1', but returned '{Q.ListField_Item.maxSelections}'");  //check that correct default value (1) is set 
			var DI = Q.AddChildDisplayedItem("DDDDD");//should add to end of the <List>
			DI.name = DI.ID;
			DI.title = DI.ID;

			var P = Q.AddProperty(); P.name = "PPPPP"; P.propName = "PPPPP";
			var p = Q.Property.Where(n => n.propName == "reportText").FirstOrDefault();
			var pn = Q.AddProperty();
			var li = Q.ListField_Item.List.Items[0] as ListItemType;
			var qr = Q.AddChildQuestion(QuestionEnum.QuestionFill, id: "123", title: "myTitle");
			var qr2 = Q.AddChildQuestionResponse(
				id: "123qr2",
				out DataTypes_DEType response,
				defTitle: "myTitle",
				insertPosition: -1,
				ItemChoiceType.integer,
				textAfterResponse: "cm",
				units: "cm",
				dtQuant: dtQuantEnum.EQ,
				valDefault: 0);
			var qrInteger = response.DataTypeDE_Item as integer_DEtype;
			var qrResponseField = qr2.ResponseField_Item;
			QuestionEnum qType = qr2.GetQuestionSubtype();
			qrResponseField.TextAfterResponse.val = string.Empty;

			//decimal_DEtype d = qrResponseField.AddDataType(ItemChoiceType.@decimal, dtQuantEnum.EQ, 1.1102).DataTypeDE_Item as decimal_DEtype;

			//We need to remove items from dictionaries whenever reassigning (Adding) objects to a different reference, as indicated below with li.AddListItemResponseField()

			Q.ResponseField_Item?.AddDataType(ItemChoiceType.@string, dtQuantEnum.EQ, "myVal");
			//li.ListItemResponseField.responseRequired = true;
			if (li.ListItemResponseField != null)
			{
				li.ListItemResponseField.responseRequired = true;
				li.ListItemResponseField.AddTextAfterResponse("cm");
				li.ListItemResponseField.TextAfterResponse.val = "cm";
			}
			//li.ListItemResponseField.TextAfterResponse.val = "myText";
			//li.ListItemResponseField.ResponseUnits.val = "myResponseUnits";
			//var r = li.ListItemResponseField.Response;
			var listItemResponseField = li.ListItemResponseField ?? li.AddListItemResponseField();
			DataTypes_DEType r1 = listItemResponseField.AddDataType(ItemChoiceType.@string);

			var dtItem = r1.DataTypeDE_Item;
			var elName = r1.ElementName;
			var dtEnum = Enum.Parse<ItemChoiceType>("string", true);

			DataTypes_DEType response1 = listItemResponseField.AddDataType(ItemChoiceType.@string);
			var myString = (string_DEtype)response1.Item;
			myString.maxLength = 4000;

			DataTypes_DEType response2 = listItemResponseField.AddDataType(ItemChoiceType.integer);
			var myInteger = (integer_DEtype)response2.Item;
			myInteger.minInclusive = 0;
			myInteger.maxInclusive = 100;

			DataTypes_DEType response3 = listItemResponseField.AddDataType(ItemChoiceType.@decimal);
			var myDecimal = (decimal_DEtype)response3.DataTypeDE_Item;
			myDecimal.minInclusive = 0;
			myDecimal.maxInclusive = 100;
			myDecimal.fractionDigits = 2;

			myDecimal.SetShouldSerialize(myDecimal.quantEnum);



			//Retrieve specific Properties under the FormDesign node
			//var prop = FD.GetChildList()
			//    .Where(n => n.GetType() == typeof(PropertyType)).Cast<PropertyType>()
			//    .Where(p => p.propName == "TemplateID").FirstOrDefault();

			var prop1 = FD.GetChildNodes().OfType<PropertyType>()
				.Where(p => p.propName == "TemplateID").FirstOrDefault();



			//retrieving FormDesign direct attributes
			var lineage = FD.lineage;
			//var p = FD.GetChildList().Where(n => n.GetType() == typeof(PropertyType)).Where(p=);
			//Console.WriteLine(props[0].name);

			var S = Q.AddChildSection("SSSSS", "SSSSS", 0);
			//Q.Move(new SectionItemType(), -1); Q.AddComment(); Q.Remove();
			//var li = new ListItemType(Q.ListField_Item.List,"abc" ); var b = li.SelectIf.returnVal; var rv = li.OnSelect[0].returnVal;

			DisplayedType DI1 = (DisplayedType)FD.Nodes.Values.Where(n => n.name == DI.ID)?.First();
				DisplayedType DI2 = (DisplayedType)Q.ChildItemsNode.Items[0];
				QuestionItemType Q1 = (QuestionItemType)DI2.ParentNode.ParentNode;
				myXML = SdcUtil.ReorderXml(FD.GetXml());
				myXML = SdcUtil.FormatXml(myXML);

				// Rationale: the object model built above must be self-consistent and serializable.
				// Assertions must be placed before ResetRootNode() because that call rebuilds FD.Nodes
				// and object-identity checks on the rebuilt tree are not meaningful.
				Assert.IsNotNull(FD, "FormDesign must not be null after deserialization and modification.");

				// The added DisplayedItem must be locatable by name (DI.name was set to DI.ID) in FD.Nodes.
				// DI.name == DI.ID is the convention set at line 122 (DI.name = DI.ID).
				Assert.IsTrue(FD.Nodes.Values.Any(n => n.name == DI.name),
					$"Added DisplayedItem with name '{DI.name}' must be reachable in FD.Nodes.");

				// DI1 (found by name) and DI2 (found by position) must refer to the same node.
				Assert.AreSame(DI1, DI2,
					"DI found by name in FD.Nodes and DI found at ChildItemsNode.Items[0] must be the same object.");

				// The parent chain from DI2 must lead back to Q.
				Assert.AreSame(Q, Q1,
					"The grandparent of ChildItemsNode.Items[0] must be the question Q that owns the child list.");

				// The added section S must be a child of Q in the Nodes dictionary.
				Assert.IsNotNull(S, "AddChildSection must return a non-null SectionItemType.");
				Assert.IsTrue(FD.Nodes.Values.Contains(S),
					"Added section S must be registered in FD.Nodes.");

				// The child question qr must be registered in FD.Nodes.
				Assert.IsNotNull(qr, "AddChildQuestion must return a non-null QuestionItemType.");
				Assert.IsTrue(FD.Nodes.Values.Contains(qr),
					"Added child question qr must be registered in FD.Nodes.");

				// The child question response qr2 must be registered in FD.Nodes.
				Assert.IsNotNull(qr2, "AddChildQuestionResponse must return a non-null QuestionItemType.");
				Assert.IsTrue(FD.Nodes.Values.Contains(qr2),
					"Added child question response qr2 must be registered in FD.Nodes.");

				// The decimal data type added to the response field must carry the constraint values we set.
				Assert.AreEqual(0m, myDecimal.minInclusive,
					"decimal_DEtype.minInclusive must be 0 as set.");
				Assert.AreEqual(100m, myDecimal.maxInclusive,
					"decimal_DEtype.maxInclusive must be 100 as set.");
				Assert.AreEqual(2, myDecimal.fractionDigits,
					"decimal_DEtype.fractionDigits must be 2 as set.");

				// XML output after all modifications must be non-empty and well-formed.
				Assert.IsFalse(string.IsNullOrWhiteSpace(myXML),
					"SdcUtil.FormatXml(FD.GetXml()) must return non-empty XML after modifications.");
				Assert.IsTrue(myXML.Contains("FormDesign"),
					"Serialized XML must contain the FormDesign root element name.");

				//var S1 = Q.AddOnEnter().Actions.AddActInject().Item = new SectionItemType(   //Need to add AddActionsNode to numerous classes via IHasActionsNode
				//    parentNode: Q,
				//    id: "myid",
				//    elementName: "",
				//    elementPrefix: "s");

				Debug.Print(myXML);
				FD.ResetRootNode();
				//var myMP = FD.GetMsgPack();
				//FD.SaveMsgPackToFile("C:\\MPfile");  //also support REST transactions, like sending packages to SDC endpoints; consider FHIR support
				var myJson = FD.GetJson();
				Debug.Print(myJson);

				// JSON output after ResetRootNode must be non-empty.
				Assert.IsFalse(string.IsNullOrWhiteSpace(myJson),
					"FD.GetJson() must return non-empty JSON after ResetRootNode.");
			}

		[TestMethod]
		public void AddListItemResponseField_WhenAlreadyExists_ThrowsInvalidOperationException()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null);
			var question = new QuestionItemType(de, "q");
			de.Items.Add(question);
			var listItem = question.AddListItem("li");
			var existing = listItem.AddListItemResponseField();
			Assert.IsNotNull(existing);

			// Bug fix: isolate the intentional failing branch; adding a second ListItemResponseField must throw.
			Assert.Throws<InvalidOperationException>(() => listItem.AddListItemResponseField());
		}
		[TestMethod]
		public void DeserializePkgFromPath()
		{
			BaseType.ResetLastTopNode();
			//string path = @".\Test files\..Sample SDCPackage.xml";
			string path = Path.Combine("..", "..", "..", "Test files", "..Sample SDCPackage.xml");
			//string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
			var Pkg = RetrieveFormPackageType.DeserializeFromXmlPath(path);
			//XMLPackageType XPT = (XMLPackageType)Pkg.Nodes.Values.Where(n => n is XMLPackageType).FirstOrDefault();

			DemogFormDesignType DFD = (DemogFormDesignType)Pkg.Nodes.Values.Where(n => n is DemogFormDesignType).FirstOrDefault()!;
			FormDesignType FD = (FormDesignType)Pkg.Nodes.Values.Where(n => n is FormDesignType).Skip(1).First();


			var Q = (QuestionItemType?)DFD.Nodes.Values.Where(
				t => t.GetType() == typeof(QuestionItemType)).Where(
				q => ((QuestionItemType)q).ID == "37387.100004300").FirstOrDefault();


			var DI = Q.AddChildDisplayedItem("DDDDD");//should add to end of the <List>
			DI.name = "myAddedDI";

			DisplayedType? DI1 = FD.Nodes.Values.Where(n => n.name == "myAddedDI").FirstOrDefault() as DisplayedType;
			DisplayedType? DI2 = Q?.ChildItemsNode?.Items?[0] as DisplayedType;
			QuestionItemType? Q1 = DI2?.ParentNode?.ParentNode as QuestionItemType;
			string diName = Q?.Item1.Items[0].name??"";
			string diName2 = Q?.ChildItemsNode?.ChildItemsList[0].ID??"";
			int i = Q?.ChildItemsNode?.ChildItemsList?.Count()??default;
			bool b1 = Q?.ChildItemsNode?.ShouldSerializeItems()??default;

			var myXML = Pkg.GetXml();

			Debug.Print(myXML);

			// Rationale: deserialization of the package must produce non-null top-node types and a non-empty re-serialized XML.
			Assert.IsNotNull(Pkg, "DeserializeFromXmlPath must return a non-null RetrieveFormPackageType.");
			Assert.IsNotNull(DFD, "Package must contain a DemogFormDesignType node.");
			Assert.IsNotNull(FD, "Package must contain a second FormDesignType node.");
			Assert.IsNotNull(Q, "DFD must contain question ID '37387.100004300'.");
			Assert.IsNotNull(DI2, "The added DisplayedItem must appear at ChildItemsNode.Items[0].");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myXML), "GetXml() must return non-empty XML for the package.");
		}
		[TestMethod]
		public void DeserializePkgFromPath_AddName()
		{
			string path = Path.Combine("..", "..", "..", "Test files", "..Sample SDCPackage.xml");
			SdcUtil.CreateName nameMethod = SdcUtil.CreateSimpleName;
			//var nameMethod = SdcUtil.CreateCAPname;
			var Pkg = RetrieveFormPackageType.DeserializeFromXmlPath(path, true, nameMethod, 0, 1);
			var myXML = Pkg.GetXml();
			Debug.Print(myXML);

			// Rationale: name assignment must propagate to at least one node and the resulting XML must be non-empty.
			Assert.IsNotNull(Pkg, "DeserializeFromXmlPath must return a non-null RetrieveFormPackageType.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(myXML), "GetXml() must return non-empty XML after name assignment.");
			Assert.IsTrue(Pkg.Nodes.Values.Any(n => !string.IsNullOrWhiteSpace(n.name)),
				"After CreateSimpleName name assignment, at least one node must have a non-empty name.");
		}
		[TestMethod]
		public void JsonToXML()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			// Rationale: SDC JSON produced by GetJson() must survive a Newtonsoft JSON→XmlDocument round-trip
			// and the resulting XmlDocument must be loadable and non-empty.
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			var fd = FormDesignType.DeserializeFromXmlPath(path);
			Assert.IsNotNull(fd, "Deserialized FormDesign must not be null.");

			var json = fd.GetJson();
			Assert.IsFalse(string.IsNullOrWhiteSpace(json), "GetJson() must return non-empty JSON.");

			var xmlDoc = new XmlDocument();
			var xmlFromSdc = fd.GetXml();
			xmlDoc.LoadXml(xmlFromSdc);
			var jsonFromXml = JsonConvert.SerializeXmlNode(xmlDoc);
			Assert.IsFalse(string.IsNullOrWhiteSpace(jsonFromXml), "JsonConvert.SerializeXmlNode must return non-empty JSON.");

			var roundTrippedDoc = JsonConvert.DeserializeXmlNode(jsonFromXml);
			Assert.IsNotNull(roundTrippedDoc, "JsonConvert.DeserializeXmlNode must return a non-null XmlDocument.");
			Assert.IsFalse(string.IsNullOrWhiteSpace(roundTrippedDoc!.OuterXml),
				"The round-tripped XmlDocument.OuterXml must be non-empty.");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void SdcToJson()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			// Rationale: GetJson() must produce valid, non-empty JSON that contains the SDC form ID,
			// proving both serialization and field-name mapping are working correctly.
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			var fd = FormDesignType.DeserializeFromXmlPath(path);
			Assert.IsNotNull(fd, "Deserialized FormDesign must not be null.");

			var json = fd.GetJson();
			Debug.Print(json);

			Assert.IsFalse(string.IsNullOrWhiteSpace(json), "GetJson() must return non-empty JSON.");
			// Rationale: the form ID must appear in the JSON output so field-name serialization is verified.
			Assert.IsTrue(json.Contains(fd.ID), $"JSON output must contain the form ID '{fd.ID}'.");
			// Rationale: 'ID' is a standard SDC attribute; its presence in the JSON confirms attribute serialization.
			Assert.IsTrue(json.Contains("\"ID\"") || json.Contains("ID"),
				"JSON output must reference at least one 'ID' field.");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		// ---------------------------------------------------------------------------
		// Round-trip fidelity helper
		// ---------------------------------------------------------------------------
		/// <summary>
		/// Asserts that <paramref name="roundTripped"/> is structurally identical to
		/// <paramref name="original"/> using <see cref="CompareTrees{T}"/> as the fidelity oracle.
		/// A perfect round-trip yields:
		///   – zero IET nodes added or removed,
		///   – zero non-IET nodes added or removed,
		///   – zero attribute-list changes, moves, parent changes, or sub-node additions/removals
		///     on any IET node.
		/// Any deviation causes the calling test to fail with a descriptive message.
		/// </summary>
		private static void AssertRoundTripFidelity<T>(T original, T roundTripped, string format)
			where T : class, ITopNode
		{
			var ct = new CompareTrees<T>(original, roundTripped);

			var added   = ct.GetIETnodesAddedInNew;
			var removed = ct.GetIETnodesRemovedInNew;
			var nodesAdded   = ct.GetNodesAddedInNew;
			var nodesRemoved = ct.GetNodesRemovedInNew;

			Assert.AreEqual(0, added.Count,
				$"{format} round-trip: {added.Count} IET node(s) were added that should not exist. " +
				$"First: {added.FirstOrDefault()?.ID}");
			Assert.AreEqual(0, removed.Count,
				$"{format} round-trip: {removed.Count} IET node(s) were removed that should survive. " +
				$"First: {removed.FirstOrDefault()?.ID}");
			Assert.AreEqual(0, nodesAdded.Count,
				$"{format} round-trip: {nodesAdded.Count} non-IET node(s) were added.");
			Assert.AreEqual(0, nodesRemoved.Count,
				$"{format} round-trip: {nodesRemoved.Count} non-IET node(s) were removed.");

			var diffs = ct.GetIETattDiffs;
			Assert.IsNotNull(diffs, $"{format} round-trip: GetIETattDiffs must not be null.");

			var changedNodes = diffs.Values.Where(d =>
				d.isAttListChanged || d.isMoved || d.isNew || d.isParChanged ||
				d.hasAddedSubNodes || d.hasRemovedSubNodes).ToList();

			if (changedNodes.Count != 0)
			{
				// Dump diagnostics to TestArtifacts for offline inspection
				try
				{
					var artifacts = System.IO.Path.Combine(System.Environment.CurrentDirectory ?? ".", "..", "..", "..", "TestArtifacts");
					System.IO.Directory.CreateDirectory(artifacts);
					string outPath = System.IO.Path.Combine(artifacts, $"{format}_RoundTrip_Diffs.json");
					var dump = new
					{
						format = format,
						changedCount = changedNodes.Count,
						first = changedNodes.FirstOrDefault()?.sGuidIET,
						diffs = changedNodes.Select(d => new { d.sGuidIET, d.isAttListChanged, d.isMoved, d.isNew, d.isParChanged, d.hasAddedSubNodes, d.hasRemovedSubNodes }).ToList()
					};
					System.IO.File.WriteAllText(outPath, Newtonsoft.Json.JsonConvert.SerializeObject(dump, Newtonsoft.Json.Formatting.Indented));
				}
				catch { }
				Assert.AreEqual(0, changedNodes.Count,
					$"{format} round-trip: {changedNodes.Count} IET node(s) carry unexpected change flags. " +
					$"First changed sGuid: {changedNodes.FirstOrDefault().sGuidIET}");
			}
		}

		// ---------------------------------------------------------------------------
		// XML round-trip fidelity
		// ---------------------------------------------------------------------------
		[TestMethod]
		public void XmlRoundTripFidelityTest()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Rationale: XML is the canonical SDC serialization format. A round-trip through
			// SdcSerializer<T> must reproduce a byte-for-byte equivalent object model.
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var original = FormDesignType.DeserializeFromXmlPath(path);

			var xml = TopNodeSerializer<FormDesignType>.GetXml(original, refreshSdc: false);
			Assert.IsFalse(string.IsNullOrWhiteSpace(xml), "GetXml must return non-empty XML.");

			BaseType.ResetLastTopNode();
			var roundTripped = TopNodeSerializer<FormDesignType>.DeserializeFromXml(xml, refreshSdc: true);
			Assert.IsNotNull(roundTripped, "XML deserialization must return a non-null FormDesignType.");

			AssertRoundTripFidelity(original, roundTripped, "XML");
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		// ---------------------------------------------------------------------------
		// JSON round-trip fidelity
		// ---------------------------------------------------------------------------
		[TestMethod]
		public void JsonRoundTripFidelityTest()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Rationale: SdcSerializerJson<T> now uses TypeNameHandling.All so all polymorphic
			// child-collection elements carry "$type" discriminators. This test verifies that the
			// full SDC object model survives a JSON round-trip with perfect structural fidelity,
			// as measured by CompareTrees. If TypeNameHandling or ConstructorHandling settings are
			// missing or wrong this test will fail — no workarounds are permitted.
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var original = FormDesignType.DeserializeFromXmlPath(path);

			var json = TopNodeSerializer<FormDesignType>.GetJson(original, refreshSdc: false);
			Assert.IsFalse(string.IsNullOrWhiteSpace(json), "GetJson must return non-empty JSON.");

			BaseType.ResetLastTopNode();
			var roundTripped = TopNodeSerializer<FormDesignType>.DeserializeFromJson(json, refreshSdc: true);
			Assert.IsNotNull(roundTripped, "JSON deserialization must return a non-null FormDesignType.");

			AssertRoundTripFidelity(original, roundTripped, "JSON");
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		// ---------------------------------------------------------------------------
		// BSON round-trip fidelity
		// ---------------------------------------------------------------------------
		[TestMethod]
		public void BsonRoundTripFidelityTest()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Rationale: SdcSerializerBson<T> now uses TypeNameHandling.All and
			// ConstructorHandling.AllowNonPublicDefaultConstructor. This test verifies that the
			// full SDC object model survives a BSON round-trip with perfect structural fidelity,
			// as measured by CompareTrees. If either setting is missing or wrong this test will
			// fail — no workarounds are permitted.
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var original = FormDesignType.DeserializeFromXmlPath(path);

			var bson = TopNodeSerializer<FormDesignType>.GetBson(original, refreshSdc: false);
			Assert.IsFalse(string.IsNullOrWhiteSpace(bson), "GetBson must return a non-empty base-64 string.");

			BaseType.ResetLastTopNode();
			var roundTripped = TopNodeSerializer<FormDesignType>.DeserializeFromBson(bson, refreshSdc: true);
			Assert.IsNotNull(roundTripped, "BSON deserialization must return a non-null FormDesignType.");

			AssertRoundTripFidelity(original, roundTripped, "BSON");
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		// ---------------------------------------------------------------------------
		// MsgPack round-trip fidelity
		// ---------------------------------------------------------------------------
		[TestMethod]
		public void MsgPackRoundTripFidelityTest()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Rationale: SdcSerializerMsgPack<T> uses the true MsgPack.Cli Pack/Unpack API.
			// A prior version silently fell back to XML-as-UTF8-bytes so the test passed even
			// when MsgPack.Cli could not handle XmlElement/XmlAttribute fields on SDC nodes.
			// That workaround has been reverted. If MsgPack.Cli cannot serialize the SDC object
			// model natively, this test will fail — no workarounds are permitted.
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml");
			var original = FormDesignType.DeserializeFromXmlPath(path);

			byte[] msgPack = TopNodeSerializer<FormDesignType>.GetMsgPack(original, refreshSdc: false);
			Assert.IsNotNull(msgPack, "GetMsgPack must return a non-null byte array.");
			Assert.IsTrue(msgPack.Length > 0, "GetMsgPack must return a non-empty byte array.");

			BaseType.ResetLastTopNode();
			var roundTripped = TopNodeSerializer<FormDesignType>.DeserializeFromMsgPack(msgPack, refreshSdc: true);
			Assert.IsNotNull(roundTripped, "MsgPack deserialization must return a non-null FormDesignType.");

			AssertRoundTripFidelity(original, roundTripped, "MsgPack");
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
	}
}