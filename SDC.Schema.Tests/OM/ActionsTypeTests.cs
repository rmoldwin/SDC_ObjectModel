using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OM
{
	[TestClass()]
	public class ActionsTypeTests
	{
		private static ActionsType CreateActionsNode()
		{
			var de = new DataElementType(null);
			var q = new QuestionItemType(de, "Q.Actions");
			var onEnter = q.AddOnEnter();
			return new ActionsType(onEnter);
		}

		[TestMethod()]
		public void ActionsTypeTest()
		{
			var sut = CreateActionsNode();
			Assert.IsNotNull(sut);
			Assert.AreEqual("Actions", sut.ElementName);
		}

		[TestMethod()]
		public void AddActActionTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActAction();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActSelectMatchingListItemsTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSelectMatchingListItems();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActAddCodeTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActAddCode();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActInjectTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActInject("INJ.1");
			Assert.IsNotNull(action);
			Assert.AreEqual("INJ.1", action.ID);
		}

		[TestMethod()]
		public void AddActSaveResponsesTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSaveResponses();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActSendReportTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSendReport();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActSendMessageTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSendMessage();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActSetAttributeValueTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSetAttributeValue();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActSetAttributeValueScriptTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSetAttributeValueScript();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActSetBoolAttributeValueCodeTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActSetBoolAttributeValueCode();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActShowFormTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActShowForm();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActShowMessageTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActShowMessage();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActShowReportTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActShowReport();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActPreviewReportTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActPreviewReport();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActValidateFormTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActValidateForm();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActRunCodeTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActRunCode();
			Assert.IsNotNull(action);
		}

		[TestMethod()]
		public void AddActConditionalGroupTest()
		{
			var sut = CreateActionsNode();
			var action = sut.AddActConditionalGroup();
			Assert.IsNotNull(action);
		}
	}
}
