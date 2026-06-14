using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class SdcUtilTests
	{
		private sealed class DefaultValueProbe
		{
			[DefaultValue(42)]
			public int Number { get; set; }
		}

		[TestMethod]
		public void IsGenericList_ReturnsExpectedValues()
		{
			Assert.IsTrue(SdcUtil.IsGenericList(typeof(List<int>)));
			Assert.IsFalse(SdcUtil.IsGenericList(typeof(int[])));
			Assert.IsFalse(SdcUtil.IsGenericList(typeof(string)));
		}

		[TestMethod]
		public void ArrayHelpers_ReturnExpectedPositionsAndObjects()
		{
			var arr = new string?[] { "a", null, "c" };
			Assert.AreEqual(1, SdcUtil.GetFirstNullArrayIndex(arr));

			IEnumerable seq = new List<string> { "x", "y", "z" };
			Assert.AreEqual(1, SdcUtil.GetIndexFromIEnumerableObject(seq, "y"));
			Assert.AreEqual("z", SdcUtil.GetObjectFromIEnumerableIndex(seq, 2));
			Assert.AreEqual(0, SdcUtil.IndexOf(seq, "x"));
			Assert.AreEqual(-1, SdcUtil.IndexOf(seq, "missing"));
			Assert.AreEqual("y", SdcUtil.ObjectAtIndex(seq, 1));
		}

		[TestMethod]
		public void ArrayAddHelpers_AddItemsAsExpected()
		{
			var arr = new string?[] { "a", null };
			var outArr = SdcUtil.ArrayAddItemReturnArray(arr, "b");
			Assert.AreSame(arr, outArr);
			CollectionAssert.AreEqual(new string?[] { "a", "b" }, outArr);

			var arr2 = new string?[] { "x", null };
			var returned = SdcUtil.ArrayAddReturnItem(arr2, "y");
			Assert.AreEqual("y", returned);
			CollectionAssert.AreEqual(new string?[] { "x", "y" }, arr2);
		}

		[TestMethod]
		public void RemoveArrayNullsNew_RemovesNullEntries()
		{
			var arr = new string?[] { "a", null, "c" };
			var cleaned = SdcUtil.RemoveArrayNullsNew(arr!);
			CollectionAssert.AreEqual(new[] { "a", "c" }, cleaned);
		}

		[TestMethod]
		public void XmlHelpers_FormatAndReorderXml()
		{
			var xml = "<root><child/><child2/></root>";
			var formatted = SdcUtil.FormatXml(xml);
			Assert.IsTrue(formatted.Contains("\n") || formatted.Contains("\r"));

			var reordered = SdcUtil.ReorderXml(xml, 10);
			Assert.IsTrue(reordered.Contains("order=\"0\""));
			Assert.IsTrue(reordered.Contains("order=\"10\""));
			Assert.IsTrue(reordered.Contains("order=\"20\""));
		}

		[TestMethod]
		public void TypeAndPropertyHelpers_ReturnDefaultsAndValues()
		{
			Assert.AreEqual(0, SdcUtil.GetTypeDefaultValue(typeof(int)));
			Assert.IsNull(SdcUtil.GetTypeDefaultValue(typeof(string)));

			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Property.Helpers");
			var header = fd.AddHeader();
			var headerObj = SdcUtil.GetPropertyObject(fd, "Header");
			Assert.AreSame(header, headerObj);
		}

		[TestMethod]
		public void GetAttributeDefaultValue_ReadsDefaultValueAttribute()
		{
			var pi = typeof(DefaultValueProbe).GetProperty(nameof(DefaultValueProbe.Number), BindingFlags.Public | BindingFlags.Instance);
			Assert.IsNotNull(pi);
			Assert.AreEqual(42, SdcUtil.GetAttributeDefaultValue(pi!));
		}

		[TestMethod]
		public void CreateBaseNameFromsGuid_ReturnsAtLeastRequestedLength()
		{
			for (var i = 0; i < 20; i++)
			{
				var de = new DataElementType(null);
				var q = new QuestionItemType(de);
				de.Items.Add(q);
				var baseName = SdcUtil.CreateBaseNameFromsGuid(q, 6);
				Assert.IsTrue(baseName.Length >= 6);
				Debug.WriteLine($"BaseName: {baseName}, sGuid: {q.sGuid}, Guid: {q.ObjectGUID}");
			}
		}

		[TestMethod]
		public void NavigationHelpers_ReturnExpectedSiblingAndChildNodes()
		{
			var (_, _, q1, q2, q3) = BuildNavigationFixture();

			Assert.AreSame(q1, SdcUtil.GetFirstSibElement(q2));
			Assert.AreSame(q3, SdcUtil.GetLastSibElement(q2));
			Assert.AreSame(q3, SdcUtil.GetNextSibElement(q2));
			Assert.AreSame(q1, SdcUtil.GetPrevSibElement(q2));

			Assert.AreSame(q1, SdcUtil.ReflectFirstSibElement(q2));
			Assert.AreSame(q3, SdcUtil.ReflectLastSibElement(q2));
			Assert.AreSame(q3, SdcUtil.ReflectNextSibElement(q2));
			Assert.AreSame(q1, SdcUtil.ReflectPrevSibElement(q2));
		}

		[TestMethod]
		public void ChildAndDescendantHelpers_ReturnExpectedNodes()
		{
			var (_, body, q1, q2, q3) = BuildNavigationFixture();
			q2.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.2.1", "Child");
			var bodyItems = body.ChildItemsNode;

			Assert.AreSame(q1, SdcUtil.GetFirstChildElement(bodyItems));
			Assert.AreSame(q3, SdcUtil.GetLastChildElement(bodyItems));
			Assert.AreSame(q1, SdcUtil.ReflectFirstChild(bodyItems));
			Assert.AreSame(q3, SdcUtil.ReflectLastChildElement(bodyItems));

			var lastDesc = SdcUtil.GetLastDescendantElement(bodyItems);
			Assert.IsNotNull(lastDesc);

			var refLastDesc = SdcUtil.ReflectLastDescendantElement(bodyItems);
			Assert.IsNotNull(refLastDesc);
		}

		[TestMethod]
		public void NextPrevElementHelpers_WalkTreeInOrder()
		{
			var (_, body, q1, q2, q3) = BuildNavigationFixture();

			Assert.IsNotNull(SdcUtil.GetNextElement(q1));
			Assert.IsNotNull(SdcUtil.ReflectNextElement(q1));
			Assert.IsNotNull(SdcUtil.GetPrevElement(q2));
			Assert.IsNotNull(SdcUtil.ReflectPrevElement(q2));

			Assert.IsNotNull(SdcUtil.ReflectNextElement2(body));
			Assert.IsNotNull(SdcUtil.GetNextElement(q3));
		}

		[TestMethod]
		public void ChildCollectionAndReflectionHelpers_ReturnExpectedMetadata()
		{
			var (fd, body, q1, _, _) = BuildNavigationFixture();
			var bodyItems = body.ChildItemsNode;

			var bodyKids = SdcUtil.GetChildElements(body);
			Assert.IsNotNull(bodyKids);
			Assert.AreEqual(1, bodyKids!.Count);

			var kids = SdcUtil.GetChildElements(bodyItems);
			Assert.IsNotNull(kids);
			Assert.AreEqual(3, kids!.Count);

			var hasKids = SdcUtil.TryGetChildElements(bodyItems, out var roKids);
			Assert.IsTrue(hasKids);
			Assert.IsNotNull(roKids);
			Assert.AreEqual(3, roKids!.Count);

			var reflectedKids = SdcUtil.ReflectChildElements(bodyItems);
			Assert.IsNotNull(reflectedKids);
			Assert.AreEqual(3, reflectedKids!.Count);

			var subtreeList = SdcUtil.GetSubtreeReOrderNodesList(bodyItems);
			Assert.IsTrue(subtreeList.Count >= 3);

			var subtreeDict = SdcUtil.GetSubtreeDictionary(bodyItems);
			Assert.IsTrue(subtreeDict.ContainsKey(q1.ObjectGUID));

			var xmlAtts = SdcUtil.ReflectNodeXmlAttributes(fd, getAllXmlAttributes: true, omitDefaultValues: false, attributesToExclude: Array.Empty<string>());
			Assert.IsNotNull(xmlAtts);
			Assert.IsTrue(xmlAtts.Count > 0);
		}

		private static (FormDesignType fd, SectionItemType body, QuestionItemType q1, QuestionItemType q2, QuestionItemType q3) BuildNavigationFixture()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Nav");
			var body = fd.AddBody();
			var q1 = body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.1", "Q1");
			var q2 = body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.2", "Q2");
			var q3 = body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.3", "Q3");
			return (fd, body, q1, q2, q3);
		}
	}
}
