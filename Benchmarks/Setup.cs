
using System.Runtime.CompilerServices;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace SDC.Schema.Tests
{
    public static class Setup
    {
        public static FormDesignType FD;
        private static float TimerStartTime;
        private static string _XmlPath =>
            //Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xml")
            Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
		//private static string pathOrig = Path.Combine("C:\\Users\\rmoldwi\\OneDrive\\One Drive Documents\\SDC\\SDC Git Repo\\SDC.Schema\\SDC.Schema.Tests\\Test files\\BreastStagingTest2.xml".Split("\\"));
		//Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCFormDesign.xsd"));
		//C:\Users\rmoldwi\OneDrive\One Drive Documents\SDC\SDC Git Repo\SDC.Schema\SDC.Schema.Tests\Test files\
		private static string _Xml;

        public static string DataElementXml { get; set; }
        public static string DemogFormDesignXml { get; set; }
        public static string RetrieveFormXml { get; set; }
        public static string MappingXml { get; set; }
        public static string X_RetrieveFormComplexXml { get; set; }
        public static string X_IdrXml { get; set; }
        public static string FormDesignWithHtmlXml { get; set; }

        static Setup()
        {
            Reset();
            //Reset();
        }
        public static string FormDesignXml { get; set; }

        public static void TimerStart(string message = "")
        {
            Stopwatch.StartNew();
            TimerStartTime  = (float)Stopwatch.GetTimestamp();
            if (!message.IsNullOrWhitespace()) Debug.Print(message);
        }
        public static string TimerGetSeconds()
        {
            return(
                ((Stopwatch.GetTimestamp() - TimerStartTime) / Stopwatch.Frequency)
                .ToString());
        }
        public static void TimerPrintSeconds(string messageBefore = "", string messageAfter = "")
        {
            Console.WriteLine (messageBefore +
                ((Stopwatch.GetTimestamp() - TimerStartTime) / Stopwatch.Frequency)
                .ToString()
                + messageAfter);
        }


        public static string GetXmlPath()
        {
            return _XmlPath;
        }
        public static string GetXml()
        {
            return System.IO.File.ReadAllText(_XmlPath);
        }
        public static void TraceMessage(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        {
            Trace.WriteLine("message: " + message);
            Trace.WriteLine("member name: " + memberName);
            Trace.WriteLine("source file path: " + sourceFilePath);
            Trace.WriteLine("source line number: " + sourceLineNumber);

        }
        public static string CallerName([CallerMemberName] string memberName = "")
        => memberName;  

        public static void Reset()
        {
            Setup.TimerStart("==>Setup starting----------");
			BaseType.ResetLastTopNode();
			_Xml = System.IO.File.ReadAllText(_XmlPath);
            FD = TopNodeSerializer<FormDesignType>.DeserializeFromXml(_Xml);
            Setup.TimerPrintSeconds("  seconds: ", "\r\n<==Setup finished----------\r\n");
        }



    }
}
