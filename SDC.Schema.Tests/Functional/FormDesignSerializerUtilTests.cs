using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SDC.Type.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDC.Schema;
using SDC.Schema.Extensions;
//using N = SDC.Type.NewUtils;
//using U = SDC.Type.NewUtils.SdcUtil;

namespace SDC.Schema.Tests.Functional
{
    [TestClass]
    public class FormDesignSerializerUtilTests
    {
        [TestMethod]
        public void DeserializeTest()
        {

            string contents = FileUtils.ReadXMLFile("Adrenal_partial.xml");
            Console.WriteLine(contents.Substring(0, 500));
            FormDesignType FD;
            //BaseType.ResetSdcImport();
            //FD.TreeLoadReset(); //runs BaseType.ResetSdcImport();
            FD = FormDesignType.DeserializeFromXml(contents);
            string myXML = FD.GetXml();
            //string myXML =  SdcSerializer<FormDesignType>.Serialize(FD); //Serialize
            
            Console.WriteLine(FD.Nodes.Count());

            foreach(BaseType n in FD.Nodes.Values)
            { Console.WriteLine(n.GetType().ToString()); } //Only IET nodes are included in Nodes.Values.  This is a serious problem with deep ramifications.
           


            foreach (BaseType n in FD.Nodes.Values) System.Diagnostics.Debug.WriteLine(n.GetType().Name +", name:"+ n.name + " , Order: " + n.order 
                + " , ObjectID: " + n.ObjectID);

            xxstop:;



        }
        //[TestMethod]
        //public void ReflectNextElementToNodes()
        //{
        //    string contents = FileUtils.ReadXMLFile("Adrenal_partial.xml");
        //    Console.WriteLine(contents.Substring(0, 500));
        //    FormDesignType FD;
        //    BaseType.ResetSdcImport();
        //    //FD.TreeLoadReset(); //runs BaseType.ResetSdcImport();
        //    FD = SDC.Type.Utils.FormDesignSerializerUtil.Deserialize(contents);
        //    BaseType n = FD;
        //    while (n is not null)
        //    {
        //        n = SDC.Schema.SdcUtil.ReflectNextElement(FD);
        //        if (n is not null)
        //        {

        //            SDC.Schema.IMoveRemoveExtensions.RegisterNode(n, n.ParentNode);
        //            //Create Name
        //            //Create ID
        //        }
        //    }
        //    

        //}

    }
    public static class FileUtils
    {
        public static string ReadXMLFile(string fileName)
        {
            return File.ReadAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory
                .Split("SDC.Schema.Tests")[0], "SDC.Schema.Tests","XML", fileName)
                ,Encoding.UTF8
            );
        }
    }
}