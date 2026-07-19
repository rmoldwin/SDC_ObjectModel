using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDC.Schema.Tests.TopNode
{
	[TestClass]
	public class PackageListTypeExtensionTests
	{
		[TestMethod]
		public void PackageListType_DefaultSerializationFlags_AreFalseWhenUnset()
		{
			var sut = (PackageListType)System.Activator.CreateInstance(typeof(PackageListType), true)!;
			Assert.IsFalse(sut.ShouldSerializePackageItem());
			Assert.IsFalse(sut.ShouldSerializeSDCPackageList());
			Assert.IsFalse(sut.ShouldSerializeHTML());
		}

		[TestMethod]
		public void PackageListType_ShouldSerializePackageItem_TrueWhenListHasItem()
		{
			var sut = (PackageListType)System.Activator.CreateInstance(typeof(PackageListType), true)!;
			sut.PackageItem = new System.Collections.Generic.List<PackageItemType>
			{
				(PackageItemType)System.Activator.CreateInstance(typeof(PackageItemType), true)!
			};
			Assert.IsTrue(sut.ShouldSerializePackageItem());
		}
	}
}
