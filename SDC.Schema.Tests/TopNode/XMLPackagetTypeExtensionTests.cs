using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDC.Schema.Tests.TopNode
{
	[TestClass]
	public class XMLPackagetTypeExtensionTests
	{
		[TestMethod]
		public void XMLPackageType_DefaultCollections_AreNull()
		{
			var sut = (XMLPackageType)System.Activator.CreateInstance(typeof(XMLPackageType), true)!;
			Assert.IsNull(sut.FormDesign);
			Assert.IsNull(sut.MapTemplate);
			Assert.IsNull(sut.HelperFile);
		}

		[TestMethod]
		public void XMLPackageType_MapTemplate_AssignsAndRetainsItems()
		{
			var sut = (XMLPackageType)System.Activator.CreateInstance(typeof(XMLPackageType), true)!;
			sut.MapTemplate = new System.Collections.Generic.List<MappingType>
			{
				(MappingType)System.Activator.CreateInstance(typeof(MappingType), true)!
			};
			Assert.AreEqual(1, sut.MapTemplate.Count);
		}
	}
}
