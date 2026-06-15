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
			BaseType.ResetLastTopNode();
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
			var json = fd.GetJson();
			var result = SdcValidate.ValidateSdcJson(json);
			Assert.IsNotNull(result);
		}
		[TestMethod]
		public void ValidateXmlFormDesign()
		{
			var xml = fd.GetXml();
			var result = SdcValidate.ValidateSdcXml(xml);
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ValidateXmlDemogFormDesign()
		{
			var dfd = new DemogFormDesignType(null, "Demog1");
			var result = dfd.ValidateSdcObjectTree();
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ValidateXmlPackage()
		{
			var pkg = new RetrieveFormPackageType(null, "Pkg1");
			var result = pkg.ValidateSdcObjectTree();
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ValidateJsonPackage()
		{
			var pkg = new RetrieveFormPackageType(null, "Pkg2");
			var json = pkg.GetJson();
			var result = SdcValidate.ValidateSdcJson(json);
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ValidateXmlDataElement()
		{
			var de = new DataElementType(null);
			de.ID = "De1";
			var result = de.ValidateSdcObjectTree();
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void ValidateXmlMap()
		{
			var map = new MappingType(null, "Map1", -1, "Map");
			var result = map.ValidateSdcObjectTree();
			Assert.IsNotNull(result);
		}

	}
}