using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SDC.Schema;
using SDC.Schema.Interfaces;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;


//using SDC.Schema;

namespace SDCObjectModelTests.TestClasses
{
    [TestClass]
    public class SdcSerializationTests
    {
        [TestMethod]
        public void DeserializeDEFromPath()
        {
            BaseType.ResetSdcImport();
            //string path = @".\Test files\DE sample.xml";
            string path = Path.Combine("..", "..", "..", "Test files", "DE sample.xml");
            //string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
            DataElementType DE = SdcUtilSerializer<DataElementType>.DeserializeFromXmlPath(path);
            var myXML = DE.GetXml();
            Debug.Print(myXML);
            Debug.Print(DE.GetJson());


        }
        [TestMethod]
        public void DeserializeDEFromXml()
        {
            BaseType.ResetSdcImport();
            //string path = @".\Test files\DE sample.xml";
            string path = Path.Combine("..", "..", "..", "Test files", "DE sample.xml");
            string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
            DataElementType DE = SdcUtilSerializer<DataElementType>.DeserializeFromXml(sdcFile);
            var myXML = DE.GetXml();
            Debug.Print(myXML);

        }
        [TestMethod]
        public void DeserializeDemogFormDesignFromPath()
        {
            Setup.TimerStart("==>[] Started");


            BaseType.ResetSdcImport();
            //string path = @".\Test files\Demog CCO Lung Surgery.xml";

            string path = Path.Combine("..", "..", "..", "Test files", "Demog CCO Lung Surgery.xml");
            //if (!File.Exists(path)) path = @"/Test files/Demog CCO Lung Surgery.xml";
            //string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
            DemogFormDesignType FD = SdcUtilSerializer<DemogFormDesignType>.DeserializeFromXmlPath(path);
            var myXML = SdcUtilSerializer<DemogFormDesignType>.GetXml(FD);
            Debug.Print(myXML);
            //Debug.Print(FD.GetJson());
            var doc = new XmlDocument();
            doc.LoadXml(myXML);
            var json = JsonConvert.SerializeXmlNode(doc);
            Debug.Print(json);
            doc = JsonConvert.DeserializeXmlNode(json);
            Debug.Print(doc?.OuterXml);
            Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
        }
        [TestMethod]
        public void DeserializeFormDesignFromPathSimple()
        {
            BaseType.ResetSdcImport();
            string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
            string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var FD = FormDesignType.DeserializeFromXmlPath(path);
            var myXml = SdcSerializer<FormDesignType>.Serialize(FD);
            Debug.Print(myXml);
            var myJson = SdcSerializerJson<FormDesignType>.SerializeJson(FD);
            Debug.Print(myJson);
        }
        [TestMethod]
        public void DeserializeFormDesignFromPath()
        {
            BaseType.ResetSdcImport();
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
            var Q = (QuestionItemType)FD.Nodes.Values.Where(
                t => t.GetType() == typeof(QuestionItemType)).Where(
                q => ((QuestionItemType)q).ID == "58218.100004300").FirstOrDefault();

            Assert.IsTrue(Q.ListField_Item.maxSelections == 1, $"maxSelections must be '1', but returned '{Q.ListField_Item.maxSelections}'");  //check that correct default value (1) is set 
            var DI = Q.AddChildDisplayedItem("DDDDD");//should add to end of the <List>
            DI.name = DI.ID;
            DI.title = DI.ID;

            var P = Q.AddProperty(); P.name = "PPPPP"; P.propName = "PPPPP";
            var p = Q.Property.Where(n => n.propName == "reportText").FirstOrDefault();
            var pn = Q.AddProperty();
            var li = Q.ListField_Item.List.Items[0] as ListItemType;
            var qr = Q.AddChildQuestion(QuestionEnum.QuestionFill, id: "123", title:"myTitle");
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
            qrResponseField.TextAfterResponse.val = "";     
            
            //decimal_DEtype d = qrResponseField.AddDataType(ItemChoiceType.@decimal, dtQuantEnum.EQ, 1.1102).DataTypeDE_Item as decimal_DEtype;

           
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
            DataTypes_DEType r1 = li.AddListItemResponseField().AddDataType(ItemChoiceType.@string);
           
            var dtItem = r1.DataTypeDE_Item;
            var elName = r1.ElementName;
            var dtEnum = Enum.Parse<ItemChoiceType>("string", true);

            DataTypes_DEType response1 = li.AddListItemResponseField().AddDataType(ItemChoiceType.@string);
            var myString = (string_DEtype)response1.Item;
            myString.maxLength = 4000;

            DataTypes_DEType response2 = li.AddListItemResponseField().AddDataType(ItemChoiceType.integer);
            var myInteger = (integer_DEtype)response2.Item;
            myInteger.minInclusive = 0;
            myInteger.maxInclusive = 100;
           
            DataTypes_DEType response3 = li.AddListItemResponseField().AddDataType(ItemChoiceType.@decimal);
            var myDecimal = (decimal_DEtype)response3.DataTypeDE_Item;
            myDecimal.minInclusive = 0;
            myDecimal.maxInclusive = 100;
            myDecimal.fractionDigits = 2;

            myDecimal.SetShouldSerialize(myDecimal.quantEnum);



            //Retrieve specific Properties under the FormDesign node
            //var prop = FD.GetChildList()
            //    .Where(n => n.GetType() == typeof(PropertyType)).Cast<PropertyType>()
            //    .Where(p => p.propName == "TemplateID").FirstOrDefault();

            var prop1 = FD.GetChildList().OfType<PropertyType>()
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
            myXML = SdcUtil.XmlReorder(FD.GetXml());
            myXML = SdcUtil.XmlFormat(myXML);

            //var S1 = Q.AddOnEnter().Actions.AddActInject().Item = new SectionItemType(   //Need to add AddActionsNode to numerous classes via IHasActionsNode
            //    parentNode: Q,
            //    id: "myid",
            //    elementName: "",
            //    elementPrefix: "s");

            Debug.Print(myXML);
            FD.Clear();
            //var myMP = FD.GetMsgPack();
            //FD.SaveMsgPackToFile("C:\\MPfile");  //also support REST transactions, like sending packages to SDC endpoints; consider FHIR support
            var myJson = FD.GetJson();
            Debug.Print(myJson);
        }
        [TestMethod]
        public void DeserializePkgFromPath()
        {
            BaseType.ResetSdcImport();
            //string path = @".\Test files\..Sample SDCPackage.xml";
            string path = Path.Combine("..", "..", "..", "Test files", "..Sample SDCPackage.xml");
            //string sdcFile = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var Pkg = RetrieveFormPackageType.DeserializeFromXmlPath(path);
            FormDesignType FD = (FormDesignType)Pkg.Nodes.Values.Where(n => n.GetType() == typeof(FormDesignType)).FirstOrDefault();


            var Q = (QuestionItemType?)Pkg.Nodes.Values.Where(
                t => t.GetType() == typeof(QuestionItemType)).Where(
                q => ((QuestionItemType)q).ID == "37387.100004300").FirstOrDefault();
            var DI = Q.AddChildDisplayedItem("DDDDD");//should add to end of the <List>
            DI.name = "my added DI";

            DisplayedType DI1 = (DisplayedType)Pkg.Nodes.Values.Where(n => n.name == "my added DI").First();
            DisplayedType DI2 = (DisplayedType)Q.ChildItemsNode.Items[0];
            QuestionItemType Q1 = (QuestionItemType)DI2.ParentNode.ParentNode;
            string diName = Q.Item1.Items[0].name;
            string diName2 = Q.ChildItemsNode.ChildItemsList[0].ID;
            int i = Q.ChildItemsNode.ChildItemsList.Count();
            bool b1 = Q.ChildItemsNode.ShouldSerializeItems();

            var myXML = Pkg.GetXml();


            Debug.Print(myXML);

        }
        [TestMethod]
        public void JsonToXML()
        {
            Setup.TimerStart($"==>{Setup.CallerName()} Started");

            Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
        }

        [TestMethod]
        public void SdcToJson()
        {
            Setup.TimerStart($"==>{Setup.CallerName()} Started");

            Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
        }


    }
}