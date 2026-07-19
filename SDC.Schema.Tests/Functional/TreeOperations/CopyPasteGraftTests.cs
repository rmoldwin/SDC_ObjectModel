using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.Functional.TreeOperations
{
	/// <summary>
	/// Tests for <see cref="IMoveRemoveExtensions.CopyPaste(BaseType, BaseType, int)"/> and
	/// <see cref="IMoveRemoveExtensions.Graft(BaseType, BaseType, int)"/> -- the plain (non-repeat,
	/// no "__N" suffix forced) clone/move-to-a-new-parent operations, used for cross-tree or same-tree
	/// copy/paste and drag-and-drop style grafting.
	/// <br/><br/>
	/// This file specifically covers §12.5 items 1-2 of <c>CompareTrees_Merge_Plan.md</c>: regression
	/// coverage for the "empty single-node slot" null-fix (a node property that starts out null, rather
	/// than already holding a BaseType instance) in the <c>UpdateNodeIdentity</c> inline attach path used
	/// by CopyPaste/Graft, plus deep-clone correctness (children independent, source untouched) for
	/// CopyPaste applied to a subtree with descendants.
	/// <br/><br/>
	/// <see cref="QuestionItemType.ListField_Item"/> (type <see cref="ListFieldType"/>) is used as the
	/// "empty single-node slot" target rather than <see cref="FormDesignType.Header"/>/<c>.Body</c>/
	/// <c>.Footer</c>: those three properties all share the exact same declared type (SectionItemType)
	/// with no discriminating Type/ElementName on their XmlElementAttribute, so IsAttachNodeAllowed's
	/// type-based inference is genuinely ambiguous among them (a separate, pre-existing characteristic
	/// of the schema, not something this task fixes). ListField_Item's type (ListFieldType) is unique
	/// among QuestionItemType's single-node properties (distinct from ResponseField_Item's
	/// ResponseFieldType), so it resolves unambiguously regardless of ElementName state.
	/// </summary>
	[TestClass]
	public class CopyPasteGraftTests
	{
		/// <summary>
		/// Builds a small FDF tree with one Section containing a QuestionSingle (ListField_Item populated
		/// with two ListItems) under the form's Body.
		/// </summary>
		private static (FormDesignType fd, SectionItemType body, SectionItemType section, QuestionItemType question) BuildTreeWithListField(string fdID, string sectionID, string questionID)
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, fdID);
			var body = fd.AddBody();
			var section = body.AddChildSection(sectionID, "Specimen");
			var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, questionID, "Site");
			question.AddListItem("LI.Yes", "Yes");
			question.AddListItem("LI.No", "No");
			return (fd, body, section, question);
		}

		/// <summary>
		/// Builds a small FDF tree with one Section containing a QuestionFill (ResponseField_Item
		/// populated, ListField_Item left null) under the form's Body -- the "empty single-node slot"
		/// target needed to exercise the null-slot fix.
		/// </summary>
		private static (FormDesignType fd, SectionItemType body, QuestionItemType question) BuildTreeWithEmptyListField(string fdID, string sectionID, string questionID)
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, fdID);
			var body = fd.AddBody();
			var section = body.AddChildSection(sectionID, "Other");
			var question = section.AddChildQuestion(QuestionEnum.QuestionFill, questionID, "Other");
			return (fd, body, question);
		}

		/// <summary>
		/// Regression test for the null-slot fix in the UpdateNodeIdentity inline attach path (used by
		/// CopyPaste via Move(..., RefreshMode.UpdateNodeIdentity)): CopyPaste-ing a node into a
		/// single-node property slot (QuestionItemType.ListField_Item) that is currently null (never
		/// populated) must succeed, not silently no-op. Before the fix, only an already-populated
		/// single-node slot (holding an existing BaseType instance) was detected correctly; a null slot
		/// fell through undetected.
		/// </summary>
		[TestMethod]
		public void CopyPaste_IntoEmptySingleNodeSlot_AttachesSuccessfully()
		{
			var donor = BuildTreeWithListField("FD.CopySrc", "S.Specimen", "Q.Site");
			var target = BuildTreeWithEmptyListField("FD.CopyTarget", "S.Other", "Q.Other");
			// Comment: rationale -- ListField_Item must start out null on a QuestionFill; this is the
			// exact "empty single-node slot" shape that triggered the pre-fix silent no-op bug.
			Assert.IsNull(target.question.ListField_Item);

			bool copied = donor.question.ListField_Item!.CopyPaste(target.question, -1);
			Assert.IsTrue(copied, "CopyPaste into an empty (null) single-node slot must succeed, not silently no-op");

			Assert.IsNotNull(target.question.ListField_Item);

			// Comment: rationale -- CopyPaste clones; the original donor ListField_Item must remain
			// completely untouched (not moved, not removed, still on the donor question).
			Assert.IsNotNull(donor.question.ListField_Item);
		}

		/// <summary>
		/// Regression test for the same null-slot fix, but via Graft() -- which moves the ORIGINAL node
		/// (not a clone) through the same UpdateNodeIdentity inline attach path. Once
		/// ReflectRefreshSubtreeList completes, attachment into the target's single-node slot must not be
		/// skipped just because that slot started out null.
		/// </summary>
		[TestMethod]
		public void Graft_IntoEmptySingleNodeSlot_AttachesAndRemovesFromOriginalParent()
		{
			var donor = BuildTreeWithListField("FD.GraftSrc", "S.Specimen", "Q.Site");
			var target = BuildTreeWithEmptyListField("FD.GraftTarget", "S.Other", "Q.Other");
			Assert.IsNull(target.question.ListField_Item);

			var donorListField = donor.question.ListField_Item!;
			bool grafted = donorListField.Graft(target.question, -1);
			Assert.IsTrue(grafted, "Graft into an empty (null) single-node slot must succeed, not silently no-op");

			Assert.IsNotNull(target.question.ListField_Item);

			// Comment: rationale -- unlike CopyPaste, Graft moves the actual node (not a clone), so the
			// donor question must no longer hold it afterward.
			Assert.IsNull(donor.question.ListField_Item);
		}

		/// <summary>
		/// Deep-clone correctness for CopyPaste applied to a subtree WITH descendants (Section -> Question ->
		/// two ListItems): confirms the clone is a fully independent object graph (not a shallow
		/// MemberwiseClone() sharing child references with the original), gets fresh identifiers, and that
		/// mutating the clone afterward does not affect the original donor subtree.
		/// </summary>
		[TestMethod]
		public void CopyPaste_SubtreeWithChildren_DeepClonesIndependentlyWithFreshIdentity()
		{
			var donor = BuildTreeWithListField("FD.DeepCloneDonor", "S.Specimen", "Q.Site");
			var target = BuildTreeWithListField("FD.DeepCloneTarget", "S.Other", "Q.Other");

			string donorSectionSGuid = donor.section.sGuid;
			string donorQuestionSGuid = donor.question.sGuid;

			bool copied = donor.section.CopyPaste(target.body.GetChildItemsNode(), -1);
			Assert.IsTrue(copied);

			var targetSections = target.body.GetChildItemsList()!.OfType<SectionItemType>().ToList();
			// Comment: rationale -- target Body started with one section (S.Other); after CopyPaste it must
			// have exactly two: the original plus the pasted clone.
			Assert.HasCount(2, targetSections);
			var pastedSection = targetSections[1];

			// Comment: rationale -- CopyPaste/Graft assign entirely fresh identifiers (unlike Copy()'s
			// repeat-suffix scheme), so ID/name/sGuid must all differ from the donor's originals.
			Assert.AreNotEqual("S.Specimen", pastedSection.ID);
			Assert.AreNotEqual(donorSectionSGuid, pastedSection.sGuid);

			var pastedQuestion = pastedSection.GetChildItemsList()!.OfType<QuestionItemType>().Single();
			Assert.AreNotEqual("Q.Site", pastedQuestion.ID);
			Assert.AreNotEqual(donorQuestionSGuid, pastedQuestion.sGuid);

			// Comment: rationale -- the pasted ListItems must be present (proving the whole subtree, not
			// just the root, was deep-cloned) and independent objects from the donor's own ListItems.
			// ListItems live under Question.ListField_Item.List, not the question's own ChildItemsList.
			var pastedListItems = pastedQuestion.GetListField().GetList().Items.OfType<ListItemType>().ToList();
			Assert.HasCount(2, pastedListItems);
			var donorListItems = donor.question.GetListField().GetList().Items.OfType<ListItemType>().ToList();
			for (int i = 0; i < 2; i++)
				Assert.AreNotSame(donorListItems[i], pastedListItems[i]);

			// Comment: rationale -- mutating the clone's ListItem.selected must not affect the donor's own
			// ListItem -- proving the object graphs are fully independent (a true deep clone), not sharing
			// any child references (which a shallow MemberwiseClone() of only the root would have caused).
			pastedListItems[0].selected = true;
			Assert.IsFalse(donorListItems[0].selected);

			// Comment: rationale -- the donor tree itself must be completely untouched by CopyPaste (it
			// only clones; it never mutates or moves the donor), confirming donor/clone independence from
			// the donor side too.
			Assert.AreEqual("S.Specimen", donor.section.ID);
			Assert.HasCount(1, donor.body.GetChildItemsList()!.OfType<SectionItemType>());
		}
	}
}
