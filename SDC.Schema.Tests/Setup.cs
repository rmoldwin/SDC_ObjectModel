using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SDC.Schema.Tests
{

	public class Setup
	{
		private static string _fileFolderPath;
		private static FormDesignType? _BreastInvasive;
		private static FormDesignType? _Adrenal;
		private static RetrieveFormPackageType? _SampleSDCPackage;
		private static DataElementType? _DE_Sample;
		private static DemogFormDesignType? _DemogCCO_LungSurg;
		private static FormDesignType? _BreastStagingTestV1;
		private static FormDesignType? _BreastStagingTestV2;
		private static FormDesignType? _DefaultValsV1;
		private static FormDesignType? _DefaultValsV2;

		private static float TimerStartTime;
		private static string _Xml;
		private static FormDesignType? _fD;
		private static string _XmlPath =>
			//Path.Combine("..", "..", "..", "Test files", "DefaultValsTest2.xml")
			Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");



		#region Ctor
		public Setup(string testFileFolderPath = "")
		{
			if (Path.Exists(testFileFolderPath))
			{
				_fileFolderPath = Path.Combine(testFileFolderPath.Split(new char[] { '/', '\\' }));
				_fileFolderPath = Path.GetFullPath(_fileFolderPath);
			}
			else
			{
				_fileFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Test Files"); // the 3 ".." up-levels from the current folder (net7.0) are: \SDC.Schema.Tests\bin\Debug\net7.0
				_fileFolderPath = Path.GetFullPath(_fileFolderPath);
			}
			Setup.BreastInvasive_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "Breast.Invasive.Res.189_4.001.001.CTP4_sdcFDF.xml"));
			Setup.Adrenal_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "Adrenal.Bx.Res.129_3.004.001.REL_sdcFDF.xml"));
			Setup.SampleSDCPackage_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "..Sample SDCPackage.xml"));
			Setup.DE_Sample_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "DE sample.xml"));
			Setup.DemogCCO_LungSurg_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "Demog CCO Lung Surgery.xml"));
			//Files for CompareTrees
			Setup.BreastStagingTestV1_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "BreastStagingTest2v1.xml"));
			Setup.BreastStagingTestV2_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "BreastStagingTest2v2.xml"));
			Setup.BreastStagingTestV3_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "BreastStagingTest2v3.xml"));
			//Setup.BreastStagingTestV4_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "BreastStagingTest2v4.xml"));
			//Setup.BreastStagingTestV5_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "BreastStagingTest2v5.xml"));

			Setup.DefaultValsV1_XML = File.ReadAllText(Path.Combine(_fileFolderPath, "DefaultValsTest2v1.xml"));

		}
		#endregion

		public static void Reset()
		{
			FD = null; //no backing field for FD
			_BreastInvasive = null;
			_Adrenal = null;
			_SampleSDCPackage = null;
			_DE_Sample = null;
			_DemogCCO_LungSurg = null;
			_BreastStagingTestV1 = null;
			_BreastStagingTestV2 = null;
			_DefaultValsV1 = null;
			_DefaultValsV2 = null;


			Setup.TimerStart("==>Setup starting----------");
			BaseType.ResetRootNode();
			_Xml = System.IO.File.ReadAllText(_XmlPath);
			FD = FormDesignType.DeserializeFromXml(_Xml);
			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==Setup finished----------\r\n");
		}
		public bool SaveFile(string fileText, string fileName, bool overWriteFile = false)
		{
			FileInfo fi = new FileInfo(Path.Combine(_fileFolderPath, fileName));
			if (!overWriteFile && fi.Exists) return false;

			//File.WriteAllText(_fileFolderPath, fileName);
			using (TextWriter w = fi.CreateText())
				w.Write(fileText);
			
			fi.Refresh();
			return true;
		}
		public static string? FD_XML { get; private set; }
		public static string? BreastInvasive_XML { get; private set; }
		public static string? Adrenal_XML { get; private set; }
		public static string? SampleSDCPackage_XML { get; private set; }
		public static string? DE_Sample_XML { get; private set; }
		public static string? DemogCCO_LungSurg_XML { get; private set; }
		//-----------------------------------------------
		public static string? BreastStagingTestV1_XML { get; private set; }
		public static string? BreastStagingTestV2_XML { get; private set; }
		public static string? BreastStagingTestV3_XML { get; private set; }
		public static string? BreastStagingTestV4_XML { get; private set; }
		public static string? BreastStagingTestV5_XML { get; private set; }
		public static string? DefaultValsV1_XML { get; private set; }
		public static string? DefaultValsV2_XML { get; private set; }
		//------------------------------------------------
		public static FormDesignType BreastInvasive
		{
			get
			{
				if (_BreastInvasive is null)
					_BreastInvasive = FormDesignType.DeserializeFromXml(BreastInvasive_XML ?? string.Empty);
				return _BreastInvasive;
			}
			private set { _BreastInvasive = value; }
		}
		public static FormDesignType Adrenal
		{
			get
			{
				if (_Adrenal is null)
					_Adrenal = FormDesignType.DeserializeFromXml(Adrenal_XML ?? string.Empty);
				return _Adrenal;
			}
			private set { _Adrenal = value; }
		}
		public static RetrieveFormPackageType SampleSDCPackage
		{
			get
			{
				if (_SampleSDCPackage is null)
					_SampleSDCPackage = RetrieveFormPackageType.DeserializeFromXml(SampleSDCPackage_XML ?? string.Empty);
				return _SampleSDCPackage;
			}
			private set { _SampleSDCPackage = value; }
		}
		public static DataElementType DE_Sample
		{
			get
			{
				if (_DE_Sample is null)
					_DE_Sample = DataElementType.DeserializeFromXml(DE_Sample_XML ?? string.Empty);
				return _DE_Sample;
			}
			private set { _DE_Sample = value; }
		}
		public static DemogFormDesignType DemogCCO_LungSurg
		{
			get
			{
				if (_DemogCCO_LungSurg is null)
					_DemogCCO_LungSurg = DemogFormDesignType.DeserializeFromXml(DemogCCO_LungSurg_XML ?? string.Empty);
				return _DemogCCO_LungSurg;
			}
			private set { _DemogCCO_LungSurg = value; }
		}
		public static FormDesignType BreastStagingTestV1
		{
			get
			{
				if (_BreastStagingTestV1 is null)
					_BreastStagingTestV1 = FormDesignType.DeserializeFromXml(BreastStagingTestV1_XML ?? string.Empty);
				return _BreastStagingTestV1;
			}
			private set { BreastStagingTestV1 = value; }
		}
		public static FormDesignType BreastStagingTestV2
		{
			get
			{
				if (_BreastStagingTestV2 is null)
					_BreastStagingTestV2 = FormDesignType.DeserializeFromXml(BreastStagingTestV2_XML ?? string.Empty);
				return _BreastStagingTestV2;
			}
			private set { _BreastStagingTestV2 = value; }
		}
		public static FormDesignType DefaultValsV1
		{
			get
			{
				if (_DefaultValsV1 is null)
					_DefaultValsV1 = FormDesignType.DeserializeFromXml(DefaultValsV1_XML ?? string.Empty);
				return _DefaultValsV1;
			}
			private set { _DefaultValsV1 = value; }
		}
		public static FormDesignType DefaultValsV2
		{
			get
			{
				if (_DefaultValsV2 is null)
					_DefaultValsV2 = FormDesignType.DeserializeFromXml(DefaultValsV2_XML ?? string.Empty);
				return _DefaultValsV2;
			}
			private set { _DefaultValsV2 = value; }
		}

		public static string MappingXml { get; set; }
		public static string FormDesignWithHtmlXml { get; set; }
		public static string FormDesignXml { get; set; }
		public static string? FileFolderPath { get => _fileFolderPath; private set => _fileFolderPath = value; }
		public static FormDesignType? FD { get => _fD; private set => _fD = value; }

		public static void TimerStart(string message = "")
		{
			var sw = Stopwatch.StartNew();
			TimerStartTime = (float)Stopwatch.GetTimestamp();
			if (!message.IsNullOrWhitespace()) Debug.Print(message);
		}
		public static string TimerGetSeconds()
		{
			return (
				((Stopwatch.GetTimestamp() - TimerStartTime) / Stopwatch.Frequency)
				.ToString());
		}
		public static void TimerPrintSeconds(string messageBefore = "", string messageAfter = "")
		{
			Console.WriteLine(messageBefore +
				((Stopwatch.GetTimestamp() - TimerStartTime) / Stopwatch.Frequency)
				.ToString()
				+ messageAfter);
		}


		[TestMethod]
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



	}

	[TestClass]
	public class Tests
	{
		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
		[TestMethod()]
		public void Test()
		{
			var s = new Setup();
			var bstV1 = Setup.BreastStagingTestV1;
			var bstV2 = Setup.BreastStagingTestV2;
		}
	}
}
