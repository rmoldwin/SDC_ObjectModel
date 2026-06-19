using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.Utils.Extensions
{

	[TestClass()]
	public class BaseTypeExtensionsTests
	{

		[TestMethod()]
		public void GetChildrenTest()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var children = fd.GetChildNodes();
			Assert.IsNotNull(children);
			Assert.IsTrue(children!.Count > 0);
		}
		[TestMethod]
		public void GetXmlAttributesAllOneNode()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var lst = fd.TopNode?.GetNodeByName("S_57219")?.GetXmlAttributesAll();
			Assert.IsNotNull(lst, "GetXmlAttributesAll should return a list for a valid named node.");
			Assert.IsTrue(lst!.Count > 0, "GetXmlAttributesAll should return at least one attribute for S_57219.");
		}
		[TestMethod]
		public void GetXmlAttributesFilledOneNode()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var lst = fd.TopNode?.GetNodeByName("S_57219")?.GetXmlAttributesSerialized();
			Assert.IsNotNull(lst, "GetXmlAttributesSerialized should return a list for a valid named node.");
			Assert.IsTrue(lst!.Count > 0, "GetXmlAttributesSerialized should return at least one serialized attribute for S_57219.");
		}


		[TestMethod()]
		public void CompareVersions()
		{
			// This test is a thin wrapper that delegates to the manual-review implementation.
			// See BaseTypeExtensions_ForManualReview.CompareVersions for the full output-heavy logic.
			var reviewer = new BaseTypeExtensions_ForManualReview();
			reviewer.CompareVersions();
			// No assertion: the test fails automatically if CompareVersions throws.
		}

		[TestMethod()]
		public void GetPropertyInfoListTest()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var child = fd.GetChildNodes()?.FirstOrDefault();
			Assert.IsNotNull(child);
			var info = child!.GetPropertyInfoMetaData(fd);
			Assert.IsNotNull(info.PropertyInfo);
		}
		[TestMethod()]
		public void GetDotLevelIET()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			int count = 0;
			int nullCount = 0;
			foreach (var n in fd.IETnodes)
			{
				count++;
				if (n.DotLevelIET is null) nullCount++;
				if (count == 100) break;
			}
			Assert.IsTrue(count > 0, "There should be at least one IET node in the deserialized tree.");
			Assert.IsTrue(nullCount < count, "At least some IET nodes should have a non-null DotLevelIET.");
		}

		[TestMethod()]
		public void GetPropertyInfoMetaDataTest()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var first = fd.GetChildNodes()?.FirstOrDefault();
			Assert.IsNotNull(first);
			var md = first!.GetPropertyInfoMetaData(fd);
			Assert.IsNotNull(md.XmlElementName);
		}

		[TestMethod()]
		public void GetSubtreeTest()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var subtree = fd.GetSubtreeList();
			Assert.IsNotNull(subtree);
			Assert.IsTrue(subtree.Count > 0);
		}

		[TestMethod()]
		public void GetSibsTest()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Sibs");
			fd.AddBody();
			var q1 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q1", "One");
			var q2 = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q2", "Two");
			var sibs = q1.GetSibNodes();
			Assert.IsNotNull(sibs);
			Assert.IsTrue(sibs!.Count >= 2);
			Assert.IsTrue(sibs.Contains(q2));
		}

		[TestMethod()]
		public void IsItemChangeAllowedTest()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var body = fd.Body;
			var result = body.Move(fd.Body, 0);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void GetEditableAdHocAttributes_UnsupportedNode_ReturnsNull()
		{
			// Rationale:
			// Nodes without XmlAnyAttribute support must fail safely by returning null
			// so callers can branch without exceptions for unsupported types.
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			FormDesignType fd = FormDesignType.DeserializeFromXml(Setup.GetXml());

			var editable = fd.GetEditableAdHocAttributes();

			Assert.IsNull(editable,
				"Unsupported nodes should return null editable ad-hoc attribute collections.");
		}

		[TestMethod]
		public void GetEditableAdHocAttributes_SupportedEmptyNode_ReturnsEditableEmptyCollection()
		{
			// Rationale:
			// Supported nodes should expose a live editable collection even when no ad-hoc attributes exist yet,
			// enabling callers to add values without direct property reflection.
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			FormDesignType fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			var ext = fd.Body.AddExtension();

			var editable = ext.GetEditableAdHocAttributes();

			Assert.IsNotNull(editable,
				"Supported nodes should expose an editable ad-hoc collection.");
			Assert.AreEqual(0, editable!.Count,
				"Supported nodes with no existing AnyAttr values should expose an empty editable collection.");
		}

			}
		}

		public readonly record struct RemovedNode(BaseType node, BaseType newNode, string sGuidOld, string sGuidNew);