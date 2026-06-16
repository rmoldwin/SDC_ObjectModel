using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Collections.Generic;
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

		[TestMethod()]
		public void ItemMutator_ReassignsSameTreeSingleValueNodeToNewParent()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null);
			var sourceQuestion = new QuestionItemType(de, "Q.Source");
			var sourceResponse = new ResponseFieldType(sourceQuestion);
			sourceResponse.Response = new DataTypes_DEType(sourceResponse);
			var reassignedDataType = new string_DEtype(sourceResponse.Response);
			sourceResponse.Response.DataTypeDE_Item = reassignedDataType;

			var targetQuestion = new QuestionItemType(de, "Q.Target");
			var targetResponse = new ResponseFieldType(targetQuestion);
			targetResponse.Response = new DataTypes_DEType(targetResponse);

			targetResponse.Response.DataTypeDE_Item = reassignedDataType;

			// Rationale: verifies the same-tree reassignment path in ItemMutator keeps the moved node attached to the new parent.
			Assert.AreSame(reassignedDataType, targetResponse.Response.DataTypeDE_Item);
			// Rationale: verifies parent dictionary updates for same-tree reassignment without requiring Move() reflection attach.
			Assert.AreSame(targetResponse.Response, reassignedDataType.ParentNode);
			// Rationale: verifies old single-value slot is cleared when replaced, proving RemoveRecursive(false) detached old target node.
			Assert.IsNull(sourceResponse.Response.DataTypeDE_Item);
		}

		[TestMethod()]
		public void ItemMutator_ReassigningSameReferenceDoesNotDetachNode()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null);
			var question = new QuestionItemType(de, "Q.SameRef");
			var response = new ResponseFieldType(question);
			response.Response = new DataTypes_DEType(response);
			var dataType = new string_DEtype(response.Response);
			response.Response.DataTypeDE_Item = dataType;
			var currentNode = response.Response.DataTypeDE_Item;

			response.Response.DataTypeDE_Item = currentNode;

			// Rationale: verifies ItemMutator short-circuit preserves node identity for no-op assignments.
			Assert.AreSame(currentNode, response.Response.DataTypeDE_Item);
			// Rationale: ensures no-op assignment does not disturb parent registration.
			Assert.AreSame(response.Response, currentNode.ParentNode);
		}

		[TestMethod()]
		public void ItemsMutator_ReplacesListAndReparentsIncomingNodes()
		{
			BaseType.ResetLastTopNode();
			var de = new DataElementType(null);
			var originalSection = new SectionItemType(de, "S.Original");
			var originalChildren = originalSection.GetChildItemsNode();
			var oldNode = new DisplayedType(originalChildren, "DI.Old");

			var newNodeA = new DisplayedType(de, "DI.NewA");
			var newNodeB = new DisplayedType(de, "DI.NewB");
			var replacementList = new List<IdentifiedExtensionType> { newNodeA, newNodeB };

			originalChildren.ChildItemsList = replacementList;

				// Rationale: validates list-level replacement returns/keeps the exact incoming list instance.
				Assert.AreSame(replacementList, originalChildren.ChildItemsList);
				// Rationale: validates each incoming item is reparented to the target ChildItems container via ItemsMutator Move(this).
				Assert.AreSame(originalChildren, newNodeA.ParentNode);
				Assert.AreSame(originalChildren, newNodeB.ParentNode);
				// Rationale: validates old list entry is detached during replacement (its ParentNode becomes null).
				Assert.IsNull(oldNode.ParentNode);
			}
	}
}
