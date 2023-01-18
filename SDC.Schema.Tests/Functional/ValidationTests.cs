using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.IO;
using System.Linq;
//using SDC.Schema;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class ValidationTests
	{
		FormDesignType fd;
		private TestContext testContextInstance;

		public FormDesignType FD
		{
			get => fd;
			set => fd = value;
		}

		public ValidationTests()
		{
			BaseType.ResetRootNode();
			string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(path);

		}


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
		[TestMethod]
		public void ValidateJsonFormDesign()
		{

		}
		[TestMethod]
		public void ValidateXmlFormDesign()
		{
			var xml = fd.GetXml();
			var result = SdcValidate.ValidateSdcXml(xml);

			Console.WriteLine("***" + result ?? string.Empty);

		}

		[TestMethod]
		public void ValidateXmlDemogFormDesign()
		{

		}

		public void ValidateXmlPackage()
		{

		}
		public void ValidateJsonPackage()
		{

		}

		[TestMethod]
		public void ValidateXmlDataElement()
		{

		}

		[TestMethod]
		public void ValidateXmlMap()
		{

		}

	}
}