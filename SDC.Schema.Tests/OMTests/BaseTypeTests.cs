using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.OMTests
{
	[TestClass()]
	public class BaseTypeTests
	{
		private static (FormDesignType fd, BaseType first, BaseType middle, BaseType last) CreateSiblingFixture()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.BaseType.Tests");
			fd.AddBody();
			var q = fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.BaseType", "BaseType");
			var first = q.AddListItem("LI.1", "One");
			var middle = q.AddListItem("LI.2", "Two");
			var last = q.AddListItem("LI.3", "Three");
			return (fd, first, middle, last);
		}

		[TestMethod()]
		public void IsMoveAllowedTest()
		{
			var (fd, first, middle, _) = CreateSiblingFixture();
			var moved = middle.Move(fd.Body, 0);
			Assert.IsFalse(moved);
		}

		[TestMethod()]
		public void RemoveTest()
		{
			var (_, _, middle, _) = CreateSiblingFixture();
			var removed = middle.RemoveRecursive(cancelIfChildNodes: true);
			Assert.IsTrue(removed);
		}

		[TestMethod()]
		public void MoveTest()
		{
			var (fd, _, middle, _) = CreateSiblingFixture();
			var moved = middle.Move(fd.Body, 0);
			Assert.IsFalse(moved);
		}

		[TestMethod()]
		public void GetNodeFirstSibTest()
		{
			var (_, first, middle, _) = CreateSiblingFixture();
			Assert.AreSame(first, middle.GetNodeFirstSib());
		}

		[TestMethod()]
		public void GetNodeLastSibTest()
		{
			var (_, _, middle, last) = CreateSiblingFixture();
			Assert.AreSame(last, middle.GetNodeLastSib());
		}

		[TestMethod()]
		public void GetNodePreviousSibTest()
		{
			var (_, first, middle, _) = CreateSiblingFixture();
			Assert.AreSame(first, middle.GetNodePreviousSib());
		}

		[TestMethod()]
		public void GetNodeNextSibTest()
		{
			var (_, _, middle, last) = CreateSiblingFixture();
			Assert.AreSame(last, middle.GetNodeNextSib());
		}

		[TestMethod()]
		public void GetNodePreviousTest()
		{
			var (_, _, middle, _) = CreateSiblingFixture();
			Assert.IsNotNull(middle.GetNodePrevious());
		}

		[TestMethod()]
		public void GetNodeNextTest()
		{
			var (_, _, middle, _) = CreateSiblingFixture();
			Assert.IsNotNull(middle.GetNodeNext());
		}

		[TestMethod()]
		public void GetNodeFirstChildTest()
		{
			var (fd, _, _, _) = CreateSiblingFixture();
			Assert.IsNotNull(fd.GetNodeFirstChild());
		}

		[TestMethod()]
		public void GetNodeLastChildTest()
		{
			var (fd, _, _, _) = CreateSiblingFixture();
			Assert.IsNotNull(fd.GetNodeLastChild());
		}

		[TestMethod()]
		public void GetNodeLastDescendantTest()
		{
			var (fd, _, _, _) = CreateSiblingFixture();
			Assert.IsNotNull(fd.GetNodeLastDescendant());
		}

		[TestMethod()]
		public void GetPropertyInfoTest()
		{
			var (_, _, middle, _) = CreateSiblingFixture();
			Assert.IsNotNull(middle.GetPropertyInfoMetaData(middle.ParentNode));
		}

		[TestMethod()]
		public void SetNamesTest()
		{
			var (_, first, _, _) = CreateSiblingFixture();
			var name = first.AssignSimpleName();
			Assert.IsFalse(string.IsNullOrWhiteSpace(name));
		}

		[TestMethod()]
		public void ResetSdcImportTest()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.ResetRoot");
			fd.AddBody();
			fd.ResetRootNode();
			Assert.AreEqual(0, fd.Nodes.Count);
		}
	}
}
