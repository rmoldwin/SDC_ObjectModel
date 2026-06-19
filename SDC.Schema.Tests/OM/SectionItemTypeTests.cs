using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OMTests
{
	[TestClass()]
	public class SectionItemTypeTests
	{
		private static SectionItemType CreateSection()
		{
			var de = new DataElementType(null);
			return new SectionItemType(de, "S.Root");
		}

		[TestMethod()]
		public void SectionItemTypeTest()
		{
			var s = CreateSection();
			Assert.IsNotNull(s);
		}

		[TestMethod()]
		public void AddChildSectionTest()
		{
			var s = CreateSection();
			var child = s.AddChildSection("S.Child", "Child", 0);
			Assert.IsNotNull(child);
			Assert.AreEqual("S.Child", child.ID);
		}

		[TestMethod()]
		public void AddChildQuestionTest()
		{
			var s = CreateSection();
			var q = s.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Child", "Question", 0);
			Assert.IsNotNull(q);
			Assert.AreEqual("Q.Child", q.ID);
		}

		[TestMethod()]
		public void AddChildInjectedFormTest()
		{
			var s = CreateSection();
			var injected = s.AddChildInjectedForm("I.Child", 0);
			Assert.IsNotNull(injected);
			Assert.AreEqual("I.Child", injected.ID);
		}

		[TestMethod()]
		public void AddChildButtonActionTest()
		{
			var s = CreateSection();
			var btn = s.AddChildButtonAction("B.Child", "Button", 0);
			Assert.IsNotNull(btn);
			Assert.AreEqual("B.Child", btn.ID);
		}

		[TestMethod()]
		public void AddChildDisplayedItemTest()
		{
			var s = CreateSection();
			var di = s.AddChildDisplayedItem("D.Child", "Display", 0);
			Assert.IsNotNull(di);
			Assert.AreEqual("D.Child", di.ID);
		}

		[TestMethod()]
		public void HasChildItemsTest()
		{
			var s = CreateSection();
			Assert.IsNull(s.GetChildItemsList());
			s.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Child", "Question", 0);
			Assert.IsTrue((s.GetChildItemsList()?.Count ?? 0) > 0);
		}

		/// <summary>
		/// Verifies AddChildQuestionResponse works on a section that was attached to a
		/// <see cref="DataElementType"/> via <c>DataElement_Items.Add()</c>.
		/// Rationale: exercises the ChildItems attach path via explicit parent-list add,
		/// distinct from the default-helper path used in other tests.
		/// </summary>
		[TestMethod()]
		public void AddChildQuestionResponseTest_ViaDataElementAdd()
		{
			var de = new DataElementType(null);
			var si = new SectionItemType(de, "sitID");
			de.DataElement_Items.Add(si);
			var qr = si.AddChildQuestionResponse("QR_id1", out _, "QR_id1_Title", 2,
				ItemChoiceType.@string, "textAfterResp", "units", dtQuantEnum.EQ, "test");
			// Rationale: confirms the returned node is non-null and carries the expected ID.
			Assert.IsNotNull(qr);
			Assert.AreEqual("QR_id1", qr.ID);
		}
	}
}
