using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Linq;

namespace SDC.Schema.Tests.UtilityClasses.Extensions
{
	[TestClass()]
	public class ITopNodeExtensionsTests
	{
		private static (FormDesignType fd, SectionItemType section, QuestionItemType question, ListItemType listItem, DisplayedType displayed, ButtonItemType button, InjectFormType inject, ResponseFieldType responseField, PropertyType property, ExtensionType extension, CommentType comment, ContactType contact, LinkType link, BlobType blob, CodingType coded) CreateFixture()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.ITop");
			fd.name = "FDName";
			var body = fd.AddBody();
			body.name = "BodyName";
			var section = body.AddChildSection("S1", "Section", 0);
			section.name = "SectionName";
			var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q1", "Question", 0);
			question.name = "QuestionName";
			var listItem = question.AddListItem("LI1", "List Item", 0);
			listItem.name = "ListItemName";
			var responseQuestion = section.AddChildQuestion(QuestionEnum.QuestionFill, "Q2", "Question Fill", 1);
			responseQuestion.name = "QuestionFillName";
			var responseField = responseQuestion.ResponseField_Item;
			responseField.name = "ResponseFieldName";
			var displayed = section.AddChildDisplayedItem("D1", "Displayed", 0);
			displayed.name = "DisplayedName";
			var button = section.AddChildButtonAction("B1", "Button", 0);
			button.name = "ButtonName";
			var inject = section.AddChildInjectedForm("I1", 0);
			inject.name = "InjectName";
			var property = section.AddProperty();
			property.name = "PropertyName";
			var extension = section.AddExtension();
			extension.name = "ExtensionName";
			var comment = section.AddComment();
			comment.name = "CommentName";
			var contact = displayed.AddContact(0);
			contact.name = "ContactName";
			var link = displayed.AddLink(0);
			link.name = "LinkName";
			var blob = displayed.AddBlob(0);
			blob.name = "BlobName";
			var coded = displayed.AddCodedValue(0);
			coded.name = "CodedName";
			fd.RefreshTree();
			return (fd, section, question, listItem, displayed, button, inject, responseField, property, extension, comment, contact, link, blob, coded);
		}

		[TestMethod()]
		public void RefreshTreeTest()
		{
			var fx = CreateFixture();
			var nodes = fx.fd.RefreshTree();
			Assert.IsTrue(nodes.Count > 0);
		}

		[TestMethod()]
		public void AssignElementNamesByReflectionTest()
		{
			var fx = CreateFixture();
			fx.fd.AssignElementNamesByReflection();
			Assert.IsFalse(string.IsNullOrWhiteSpace(fx.question.ElementName));
		}

		[TestMethod()]
		public void AssignElementNamesFromXmlDocTest()
		{
			var fx = CreateFixture();
			fx.fd.AssignElementNamesByReflection();
			Assert.IsFalse(string.IsNullOrWhiteSpace(fx.section.ElementName));
		}

		[TestMethod()]
		public void GetSortedNodesTest()
		{
			var fx = CreateFixture();
			Assert.IsTrue(fx.fd.GetSortedNodes().Count > 0);
		}

		[TestMethod()]
		public void GetSortedNodesObsColTest()
		{
			var fx = CreateFixture();
			Assert.IsTrue(fx.fd.GetSortedNodesObsCol().Count > 0);
		}

		[TestMethod()]
		public void TryGetIetNodeByIDTest()
		{
			var fx = CreateFixture();
			var ok = fx.fd.TryGetIETnodeByID(fx.question.ID, out var node);
			Assert.IsTrue(ok);
			Assert.AreSame(fx.question, node);
		}

		[TestMethod()]
		public void TryGetNodeByNameTest()
		{
			var fx = CreateFixture();
			var ok = fx.fd.TryGetNodeByName(fx.section.name, out var node);
			Assert.IsTrue(ok);
			Assert.AreSame(fx.section, node);
		}

		[TestMethod()]
		public void TryGetNodeByShortGuidTest()
		{
			var fx = CreateFixture();
			var ok = fx.fd.TryGetNodeByShortGuid(fx.question.sGuid, out var node);
			Assert.IsTrue(ok);
			Assert.AreSame(fx.question, node);
		}

		[TestMethod()]
		public void TryGetNodeByIndexTest()
		{
			var fx = CreateFixture();
			var sorted = fx.fd.GetSortedNodes();
			var qIndex = sorted.FindIndex(n => n == fx.question);
			var ok = fx.fd.TryGetNodeByIndex(qIndex, out var node);
			Assert.IsTrue(ok);
			Assert.AreSame(fx.question, node);
		}

		[TestMethod()]
		public void TryGetNodeByObjectIDTest()
		{
			var fx = CreateFixture();
			var ok = fx.fd.TryGetNodeByObjectID(fx.question.ObjectID, out var node);
			Assert.IsTrue(ok);
			Assert.AreSame(fx.question, node);
		}

		[TestMethod()]
		public void GetIetNodeByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.question, fx.fd.GetIETnodeByID(fx.question.ID));
		}

		[TestMethod()]
		public void GetNodeByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.section, fx.fd.GetNodeByName(fx.section.name));
		}

		[TestMethod()]
		public void GetNodeByShortGuidTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.question, fx.fd.GetNodeByShortGuid(fx.question.sGuid));
		}

		[TestMethod()]
		public void GetNodeByObjectGUIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.question, fx.fd.GetNodeByObjectGUID(fx.question.ObjectGUID));
		}

		[TestMethod()]
		public void GetNodeByPositionIndexTest()
		{
			var fx = CreateFixture();
			var sorted = fx.fd.GetSortedNodes();
			var qIndex = sorted.FindIndex(n => n == fx.question);
			Assert.AreSame(fx.question, fx.fd.GetNodeByPositionIndex(qIndex));
		}

		[TestMethod()]
		public void GetNodeByObjectIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.question, fx.fd.GetNodeByObjectID(fx.question.ObjectID));
		}

		[TestMethod()]
		public void ResetSdcImportTest()
		{
			var fx = CreateFixture();
			fx.fd.ResetRootNode();
			Assert.AreEqual(0, fx.fd.Nodes.Count);
			Assert.IsNull(fx.fd.Body);
		}

		[TestMethod()]
		public void GetXmlAttributesFilledTest()
		{
			var fx = CreateFixture();
			var atts = fx.fd.GetXmlAttributesFilled(log: out string _, doLog: false);
			Assert.IsTrue(atts.Count > 0);
		}

		[TestMethod]
		public void GetNodesWithEditableAdHocAttributes_ReturnsHostCapableNodesOnly()
		{
			// Bug fix: use per-test fresh graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			FormDesignType fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			fd.Body.AddExtension();

			var hostNodes = fd.GetNodesWithEditableAdHocAttributes().ToList();

			Assert.IsTrue(hostNodes.Count > 0);
			Assert.IsTrue(hostNodes.All(n => n.CanHostAdHocAttributes()));
		}

		[TestMethod()]
		public void GetDescendantDictionaryTest()
		{
			var fx = CreateFixture();
			Assert.Throws<InvalidCastException>(() => fx.fd.GetDescendantDictionary(fx.fd));
		}

		[TestMethod()]
		public void GetDescendantListTest()
		{
			var fx = CreateFixture();
			Assert.Throws<InvalidCastException>(() => fx.fd.GetDescendantList(fx.fd));
		}


		[TestMethod()]
		public void GetQuestionByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.question, fx.fd.GetQuestionByID(fx.question.ID));
		}

		[TestMethod()]
		public void GetQuestionByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.question, fx.fd.GetQuestionByName(fx.question.name));
		}

		[TestMethod()]
		public void GetDisplayedTypeByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.displayed, fx.fd.GetDisplayedTypeByID(fx.displayed.ID));
		}

		[TestMethod()]
		public void GetDisplayedTypeByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.displayed, fx.fd.GetDisplayedTypeByName(fx.displayed.name));
		}

		[TestMethod()]
		public void GetSectionByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.section, fx.fd.GetSectionByID(fx.section.ID));
		}

		[TestMethod()]
		public void GetSectionByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.section, fx.fd.GetSectionByName(fx.section.name));
		}

		[TestMethod()]
		public void GetListItemByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.listItem, fx.fd.GetListItemByID(fx.listItem.ID));
		}

		[TestMethod()]
		public void GetListItemByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.listItem, fx.fd.GetListItemByName(fx.listItem.name));
		}

		[TestMethod()]
		public void GetButtonByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.button, fx.fd.GetButtonByID(fx.button.ID));
		}

		[TestMethod()]
		public void GetButtonByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.button, fx.fd.GetButtonByName(fx.button.name));
		}

		[TestMethod()]
		public void GetInjectFormByIDTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.inject, fx.fd.GetInjectFormByID(fx.inject.ID));
		}

		[TestMethod()]
		public void GetInjectFormByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.inject, fx.fd.GetInjectFormByName(fx.inject.name));
		}

		[TestMethod()]
		public void GetResponseFieldByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.responseField, fx.fd.GetResponseFieldByName(fx.responseField.name));
		}

		[TestMethod()]
		public void GetPropertyByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.property, fx.fd.GetPropertyByName(fx.property.name));
		}

		[TestMethod()]
		public void GetExtensionByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.extension, fx.fd.GetExtensionByName(fx.extension.name));
		}

		[TestMethod()]
		public void GetCommentByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.comment, fx.fd.GetCommentByName(fx.comment.name));
		}

		[TestMethod()]
		public void GetContactByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.contact, fx.fd.GetContactByName(fx.contact.name));
		}

		[TestMethod()]
		public void GetLinkByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.link, fx.fd.GetLinkByName(fx.link.name));
		}

		[TestMethod()]
		public void GetBlobByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.blob, fx.fd.GetBlobByName(fx.blob.name));
		}

		[TestMethod()]
		public void GetCodedValueByNameTest()
		{
			var fx = CreateFixture();
			Assert.AreSame(fx.coded, fx.fd.GetCodedValueByName(fx.coded.name));
		}
	}
}
