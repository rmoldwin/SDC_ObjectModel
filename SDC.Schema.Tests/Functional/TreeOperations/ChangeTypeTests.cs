using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDC.Schema.Tests.Functional.TreeOperations
{
	[TestClass]
	public class ChangeTypeTests
	{
		[TestMethod]
		public void ChangeTypeTest()
		{
			var sut = new ChangeType();
			Assert.IsNotNull(sut);
		}

		[TestMethod]
		public void TargetItemIDPropertyTest()
		{
			var sut = new ChangeType();
			sut.TargetItemID = (TargetItemIDType)System.Activator.CreateInstance(typeof(TargetItemIDType), true)!;
			Assert.IsNotNull(sut.TargetItemID);
		}

		[TestMethod]
		public void TargetItemNamePropertyTest()
		{
			var sut = new ChangeType();
			sut.TargetItemName = (TargetItemNameType)System.Activator.CreateInstance(typeof(TargetItemNameType), true)!;
			Assert.IsNotNull(sut.TargetItemName);
		}

		[TestMethod]
		public void TargetItemXPathPropertyTest()
		{
			var sut = new ChangeType();
			sut.TargetItemXPath = (TargetItemXPathType)System.Activator.CreateInstance(typeof(TargetItemXPathType), true)!;
			Assert.IsNotNull(sut.TargetItemXPath);
		}

		[TestMethod]
		public void NewValuePropertyTest()
		{
			var sut = new ChangeType();
			sut.NewValue = (DataTypes_SType)System.Activator.CreateInstance(typeof(DataTypes_SType), true)!;
			Assert.IsNotNull(sut.NewValue);
		}

		[TestMethod]
		public void ShouldSerializeTargetItemIDTest()
		{
			var sut = new ChangeType();
			Assert.IsFalse(sut.ShouldSerializeTargetItemID());
			sut.TargetItemID = (TargetItemIDType)System.Activator.CreateInstance(typeof(TargetItemIDType), true)!;
			Assert.IsTrue(sut.ShouldSerializeTargetItemID());
		}

		[TestMethod]
		public void ShouldSerializeTargetItemNameTest()
		{
			var sut = new ChangeType();
			Assert.IsFalse(sut.ShouldSerializeTargetItemName());
			sut.TargetItemName = (TargetItemNameType)System.Activator.CreateInstance(typeof(TargetItemNameType), true)!;
			Assert.IsTrue(sut.ShouldSerializeTargetItemName());
		}

		[TestMethod]
		public void ShouldSerializeTargetItemXPathTest()
		{
			var sut = new ChangeType();
			Assert.IsFalse(sut.ShouldSerializeTargetItemXPath());
			sut.TargetItemXPath = (TargetItemXPathType)System.Activator.CreateInstance(typeof(TargetItemXPathType), true)!;
			Assert.IsTrue(sut.ShouldSerializeTargetItemXPath());
		}

		[TestMethod]
		public void ShouldSerializeNewValueTest()
		{
			var sut = new ChangeType();
			Assert.IsFalse(sut.ShouldSerializeNewValue());
			sut.NewValue = (DataTypes_SType)System.Activator.CreateInstance(typeof(DataTypes_SType), true)!;
			Assert.IsTrue(sut.ShouldSerializeNewValue());
		}
	}
}
