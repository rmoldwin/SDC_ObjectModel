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
			// Rationale: concrete type identity must match the expected class, not a subtype.
			Assert.AreEqual(typeof(ChangeType), sut.GetType(), "Instance must be exactly ChangeType.");
			// Rationale: all ShouldSerialize guards must return false on a freshly constructed instance
			// with no properties set, proving the guards are wired to the backing fields.
			Assert.IsFalse(sut.ShouldSerializeTargetItemID(), "ShouldSerializeTargetItemID must be false on a new ChangeType.");
			Assert.IsFalse(sut.ShouldSerializeTargetItemName(), "ShouldSerializeTargetItemName must be false on a new ChangeType.");
			Assert.IsFalse(sut.ShouldSerializeTargetItemXPath(), "ShouldSerializeTargetItemXPath must be false on a new ChangeType.");
			Assert.IsFalse(sut.ShouldSerializeNewValue(), "ShouldSerializeNewValue must be false on a new ChangeType.");
		}

		[TestMethod]
		public void TargetItemIDPropertyTest()
		{
			var sut = new ChangeType();
			var value = (TargetItemIDType)System.Activator.CreateInstance(typeof(TargetItemIDType), true)!;
			sut.TargetItemID = value;
			Assert.IsNotNull(sut.TargetItemID);
			// Rationale: the property must retain the exact assigned instance (reference equality).
			Assert.AreSame(value, sut.TargetItemID, "TargetItemID must return the exact assigned instance.");
			// Rationale: ShouldSerialize must flip to true when the property is set, exercising the guard wiring.
			Assert.IsTrue(sut.ShouldSerializeTargetItemID(),
				"ShouldSerializeTargetItemID must return true after TargetItemID is assigned.");
		}

		[TestMethod]
		public void TargetItemNamePropertyTest()
		{
			var sut = new ChangeType();
			var value = (TargetItemNameType)System.Activator.CreateInstance(typeof(TargetItemNameType), true)!;
			sut.TargetItemName = value;
			Assert.IsNotNull(sut.TargetItemName);
			// Rationale: the property must retain the exact assigned instance (reference equality).
			Assert.AreSame(value, sut.TargetItemName, "TargetItemName must return the exact assigned instance.");
			// Rationale: ShouldSerialize must flip to true when the property is set.
			Assert.IsTrue(sut.ShouldSerializeTargetItemName(),
				"ShouldSerializeTargetItemName must return true after TargetItemName is assigned.");
		}

		[TestMethod]
		public void TargetItemXPathPropertyTest()
		{
			var sut = new ChangeType();
			var value = (TargetItemXPathType)System.Activator.CreateInstance(typeof(TargetItemXPathType), true)!;
			sut.TargetItemXPath = value;
			Assert.IsNotNull(sut.TargetItemXPath);
			// Rationale: the property must retain the exact assigned instance (reference equality).
			Assert.AreSame(value, sut.TargetItemXPath, "TargetItemXPath must return the exact assigned instance.");
			// Rationale: ShouldSerialize must flip to true when the property is set.
			Assert.IsTrue(sut.ShouldSerializeTargetItemXPath(),
				"ShouldSerializeTargetItemXPath must return true after TargetItemXPath is assigned.");
		}

		[TestMethod]
		public void NewValuePropertyTest()
		{
			var sut = new ChangeType();
			var value = (DataTypes_SType)System.Activator.CreateInstance(typeof(DataTypes_SType), true)!;
			sut.NewValue = value;
			Assert.IsNotNull(sut.NewValue);
			// Rationale: the property must retain the exact assigned instance (reference equality).
			Assert.AreSame(value, sut.NewValue, "NewValue must return the exact assigned instance.");
			// Rationale: ShouldSerialize must flip to true when the property is set.
			Assert.IsTrue(sut.ShouldSerializeNewValue(),
				"ShouldSerializeNewValue must return true after NewValue is assigned.");
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
