using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace SDC.Schema.Tests.Functional
{
	[TestClass()]
	public class _SdcUtilTests
	{
		[TestMethod()]
		public void _IsGenericListTest()
		{

		}

		[TestMethod()]
		public void _GetFirstNullArrayIndexTest()
		{

		}

		[TestMethod()]
		public void _GetObjectFromIEnumerableObjectTest()
		{

		}

		[TestMethod()]
		public void _GetObjectFromIEnumerableIndexTest()
		{

		}

		[TestMethod()]
		public void _GetListIndexTest()
		{

		}

		[TestMethod()]
		public void _IndexOfTest()
		{

		}

		[TestMethod()]
		public void _ObjectAtIndexTest()
		{

		}

		[TestMethod()]
		public void _IEnumerableCopyTest()
		{

		}

		[TestMethod()]
		public void _ArrayAddItemReturnArrayTest()
		{

		}

		[TestMethod()]
		public void _ArrayAddReturnItemTest()
		{

		}

		[TestMethod()]
		public void _RemoveArrayNullsNewTest()
		{

		}

		[TestMethod()]
		public void _PrevElementTest()
		{

		}

		[TestMethod()]
		public void _NextElement2Test()
		{

		}

		[TestMethod()]
		public void _NextElementTest()
		{

		}

		[TestMethod()]
		public void _GetLastSibTest()
		{

		}

		[TestMethod()]
		public void _ReflectLastSibTest()
		{

		}

		[TestMethod()]
		public void _GetFirstSibTest()
		{

		}

		[TestMethod()]
		public void _ReflectFirstSibTest()
		{

		}

		[TestMethod()]
		public void _GetNextSibTest()
		{

		}

		[TestMethod()]
		public void _ReflectNextSibTest()
		{

		}

		[TestMethod()]
		public void _GetPrevSibTest()
		{

		}

		[TestMethod()]
		public void _ReflectPrevSibTest()
		{

		}

		[TestMethod()]
		public void _GetLastChildTest()
		{

		}

		[TestMethod()]
		public void _ReflectLastChildTest()
		{

		}

		[TestMethod()]
		public void _GetFirstChildTest()
		{

		}

		[TestMethod()]
		public void _ReflectFirstChildTest()
		{

		}

		[TestMethod()]
		public void _GetLastDescendantTest()
		{

		}

		[TestMethod()]
		public void _ReflectLastDescendantTest()
		{

		}

		[TestMethod()]
		public void _GetPropertyInfoTest()
		{

		}

		[TestMethod()]
		public void _ReflectPropertyInfoListTest()
		{

		}

		[TestMethod()]
		public void _ReflectPropertyInfoElementsTest()
		{

		}

		[TestMethod()]
		public void _ReflectPropertyInfoAttributesTest()
		{

		}

		[TestMethod()]
		public void _ReflectXmlAttributesFilledTest()
		{

		}

		[TestMethod()]
		public void _ReflectXmlAttributesAllTest()
		{

		}

		[TestMethod()]
		public void _ReflectChildListTest()
		{

		}

		[TestMethod()]
		public void _ReflectSubtreeTest()
		{

		}

		[TestMethod()]
		public void _IsItemChangeAllowedTest()
		{

		}

		[TestMethod()]
		public void _XmlReorderTest()
		{

		}

		[TestMethod()]
		public void _XmlFormatTest()
		{

		}
		[TestMethod()]
		public void sGuidShortNameTest()
		{
			for (int i = 0; i < 100; i++)
			{
				var de = new DataElementType(null);
				var q = new QuestionItemType(de);
				de.Items.Add(q);
				Debug.WriteLine($"sGuid: {SdcUtil.CreateBaseNameFromsGuid(q, 6)}, sGuid: {q.sGuid}, Guid: {q.ObjectGUID}");
			}
		}
	}
}