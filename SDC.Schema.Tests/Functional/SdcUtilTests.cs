using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace SDC.Schema.Tests.Functional
{
	[TestClass()]
	public class _SdcUtilTests
	{
		[TestMethod()]
		public void IsGenericListTest()
		{

		}

		[TestMethod()]
		public void GetFirstNullArrayIndexTest()
		{

		}

		[TestMethod()]
		public void GetObjectFromIEnumerableObjectTest()
		{

		}

		[TestMethod()]
		public void GetObjectFromIEnumerableIndexTest()
		{

		}

		[TestMethod()]
		public void GetListIndexTest()
		{

		}

		[TestMethod()]
		public void IndexOfTest()
		{

		}

		[TestMethod()]
		public void ObjectAtIndexTest()
		{

		}

		[TestMethod()]
		public void IEnumerableCopyTest()
		{

		}

		[TestMethod()]
		public void ArrayAddItemReturnArrayTest()
		{

		}

		[TestMethod()]
		public void ArrayAddReturnItemTest()
		{

		}

		[TestMethod()]
		public void RemoveArrayNullsNewTest()
		{

		}

		[TestMethod()]
		public void PrevElementTest()
		{

		}

		[TestMethod()]
		public void NextElement2Test()
		{

		}

		[TestMethod()]
		public void NextElementTest()
		{

		}

		[TestMethod()]
		public void GetLastSibTest()
		{

		}

		[TestMethod()]
		public void ReflectLastSibTest()
		{

		}

		[TestMethod()]
		public void GetFirstSibTest()
		{

		}

		[TestMethod()]
		public void ReflectFirstSibTest()
		{

		}

		[TestMethod()]
		public void GetNextSibTest()
		{

		}

		[TestMethod()]
		public void ReflectNextSibTest()
		{

		}

		[TestMethod()]
		public void GetPrevSibTest()
		{

		}

		[TestMethod()]
		public void ReflectPrevSibTest()
		{

		}

		[TestMethod()]
		public void GetLastChildTest()
		{

		}

		[TestMethod()]
		public void ReflectLastChildTest()
		{

		}

		[TestMethod()]
		public void GetFirstChildTest()
		{

		}

		[TestMethod()]
		public void ReflectFirstChildTest()
		{

		}

		[TestMethod()]
		public void GetLastDescendantTest()
		{

		}

		[TestMethod()]
		public void ReflectLastDescendantTest()
		{

		}

		[TestMethod()]
		public void GetPropertyInfoTest()
		{

		}

		[TestMethod()]
		public void ReflectPropertyInfoListTest()
		{

		}

		[TestMethod()]
		public void ReflectPropertyInfoElementsTest()
		{

		}

		[TestMethod()]
		public void ReflectPropertyInfoAttributesTest()
		{

		}

		[TestMethod()]
		public void ReflectXmlAttributesFilledTest()
		{

		}

		[TestMethod()]
		public void ReflectXmlAttributesAllTest()
		{

		}

		[TestMethod()]
		public void ReflectChildListTest()
		{

		}

		[TestMethod()]
		public void ReflectSubtreeTest()
		{

		}

		[TestMethod()]
		public void IsItemChangeAllowedTest()
		{

		}

		[TestMethod()]
		public void XmlReorderTest()
		{

		}

		[TestMethod()]
		public void XmlFormatTest()
		{

		}
		[TestMethod()]
		public void sGuidShortNameTest()
		{
			for (int i = 0; i < 100; i++)
			{
				var q = new QuestionItemType(null);
				Debug.WriteLine($"sGuid: {SdcUtil.CreateBaseNameFromsGuid(q.sGuid, 6)}, sGuid: {q.sGuid}, Guid: {q.ObjectGUID}");
			}
		}
	}
}