using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDC.Schema.Tests.TopNode
{
	[TestClass]
	public class MappingTypeExtensionTests
	{
		[TestMethod]
		public void MappingType_DefaultSerializationFlags_AreFalseWhenUnset()
		{
			var map = (MappingType)System.Activator.CreateInstance(typeof(MappingType), true)!;
			Assert.IsFalse(map.ShouldSerializeItemMap());
			Assert.IsFalse(map.ShouldSerializeDefaultCodeSystem());
			Assert.IsFalse(map.ShouldSerializetemplateID());
			Assert.IsFalse(map.ShouldSerializetargetTemplateID());
		}

		[TestMethod]
		public void MappingType_ShouldSerializeItemMap_TrueWhenListHasItem()
		{
			var map = (MappingType)System.Activator.CreateInstance(typeof(MappingType), true)!;
			map.ItemMap = new System.Collections.Generic.List<ItemMapType>
			{
				(ItemMapType)System.Activator.CreateInstance(typeof(ItemMapType), true)!
			};
			Assert.IsTrue(map.ShouldSerializeItemMap());
		}
	}
}
