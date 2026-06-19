using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.Utils.Extensions
{
	[TestClass()]
	public class INavigateExtensionsTests
	{
		private static FormDesignType _fd = null!;
		private static BaseType _first = null!;
		private static BaseType _middle = null!;
		private static BaseType _last = null!;

		[TestInitialize()]
		public void Setup()
		{
			BaseType.ResetLastTopNode();
			_fd = new FormDesignType(null, "FD.Nav.Tests");
			_fd.AddBody();
			var q = _fd.Body.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Nav", "Navigation");
			_first = q.AddListItem("LI.1", "One");
			_middle = q.AddListItem("LI.2", "Two");
			_last = q.AddListItem("LI.3", "Three");
		}

		[TestMethod()]
		public void GetNodeFirstSibTest()
		{
			Assert.AreSame(_first, _middle.GetNodeFirstSib());
		}

		[TestMethod()]
		public void GetNodeLastSibTest()
		{
			Assert.AreSame(_last, _middle.GetNodeLastSib());
		}

		[TestMethod()]
		public void GetNodePreviousSibTest()
		{
			Assert.AreSame(_first, _middle.GetNodePreviousSib());
		}

		[TestMethod()]
		public void GetNodeNextSibTest()
		{
			Assert.AreSame(_last, _middle.GetNodeNextSib());
		}

		[TestMethod()]
		public void GetNodePreviousTest()
		{
			Assert.IsNotNull(_middle.GetNodePrevious());
		}

		[TestMethod()]
		public void GetNodePreviousIETTest()
		{
			Assert.IsNotNull(_middle.GetNodePreviousIET());
		}

		[TestMethod()]
		public void GetNodeReflectNextTest()
		{
			Assert.IsNotNull(_middle.GetNodeReflectNext());
		}

		[TestMethod()]
		public void GetNodeNextTest()
		{
			Assert.IsNotNull(_middle.GetNodeNext());
		}

		[TestMethod()]
		public void GetNodeFirstChildTest()
		{
			Assert.IsNotNull(_fd.GetNodeFirstChild());
		}

		[TestMethod()]
		public void GetNodeLastChildTest()
		{
			Assert.IsNotNull(_fd.GetNodeLastChild());
		}

		[TestMethod()]
		public void GetNodeLastDescendantTest()
		{
			Assert.IsNotNull(_fd.GetNodeLastDescendant());
		}

		[TestMethod()]
		public void TryGetChildNodesTest()
		{
			var ok = _fd.TryGetChildNodes(out var kids);
			Assert.IsTrue(ok);
			Assert.IsNotNull(kids);
		}

		[TestMethod()]
		public void GetChildNodesTest()
		{
			Assert.IsNotNull(_fd.GetChildNodes());
		}

		[TestMethod()]
		public void GetSubtreeListTest()
		{
			Assert.IsTrue((_fd.GetSubtreeList()?.Count ?? 0) > 0);
		}

		[TestMethod()]
		public void GetSubtreeDictionaryTest()
		{
			Assert.IsTrue((_fd.GetSubtreeDictionary()?.Count ?? 0) > 0);
		}

		[TestMethod()]
		public void GetPropertyInfoTest()
		{
			Assert.IsNotNull(_middle.GetPropertyInfo());
		}

		[TestMethod()]
		public void GetFullTreeTest()
		{
			Assert.IsTrue((_middle.GetFullTree()?.Count ?? 0) > 0);
		}
	}
}
