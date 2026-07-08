using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.Linq;

namespace SDC.Schema.Tests.Functional.TreeOperations
{
	/// <summary>
	/// Gap-closing tests for the "repeating section" construction pattern, using ONLY the public
	/// SDC.Schema.dll API surface, starting from a FRESHLY-CONSTRUCTED (not XML-loaded) SDC tree.
	/// <br/><br/>
	/// Existing coverage (<see cref="SdcUtilTests.ReflectRefreshSubtreeList_CloneAndRepeatSubtree_CoverageGap"/>)
	/// only proves the <see cref="SdcUtil.RefreshMode.CloneAndRepeatSubtree"/> branch doesn't throw when a
	/// single already-constructed node is passed directly to <see cref="SdcUtil.ReflectRefreshSubtreeList"/> —
	/// it never actually clones or attaches a new repeated node.
	/// <br/><br/>
	/// Real, working "clone and repeat" coverage previously existed only in
	/// <see cref="MoveTests.CloneRepeatSdcSubtreeXmlTest"/>, which loads a real clinical XML fixture from disk.
	/// That left an open question (guide/07-known-gaps-and-future-work.md, item: repeating-section construction)
	/// about whether the SAME pattern works for a tree built purely via public constructors/extension methods
	/// (the scenario this guide targets: AI-prompt-driven OM construction from scratch). These tests confirm it does.
	/// </summary>
	[TestClass]
	public class RepeatingSectionConstructionTests
	{
		/// <summary>
		/// The simplest, most idiomatic public API for repeating an existing section/question is
		/// <see cref="IMoveRemoveExtensions.Copy(IdentifiedExtensionType)"/> — a one-line convenience wrapper
		/// around <c>Move(ParentNode, -1, false, RefreshMode.CloneAndRepeatSubtree)</c>. This test proves it
		/// works end-to-end on a tree built entirely from scratch via public constructors/extension methods
		/// (FormDesignType -> AddBody -> AddChildSection -> AddChildQuestion), not just on trees deserialized
		/// from an existing XML fixture.
		/// </summary>
		[TestMethod]
		public void Copy_OnFreshlyConstructedSection_CreatesCorrectlySuffixedRepeat()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Repeat.Construction");
			var body = fd.AddBody();
			var section = body.AddChildSection("S.Specimen", "Specimen");
			var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Site", "Site");

			// Capture original identity values before Copy() so we can assert the original is untouched
			// and the new repeat's values are correctly derived from them.
			string origSectionId = section.ID;
			string origSectionName = section.name;
			string origSectionSGuid = section.sGuid;
			string origQuestionName = question.name;
			string origQuestionId = question.ID;

			bool copied = section.Copy();
			Assert.IsTrue(copied, "Copy() must succeed for a fully-attached section with a valid ChildItemsType parent");

			// The original section must be completely unmodified by Copy() -- Copy() only affects the new clone.
			Assert.AreEqual(origSectionId, section.ID, "Original section ID must not change after Copy()");
			Assert.AreEqual(origSectionName, section.name, "Original section name must not change after Copy()");
			Assert.AreEqual(origSectionSGuid, section.sGuid, "Original section sGuid must not change after Copy()");

			// The repeat is inserted immediately after the original in the same ChildItemsType list.
			var siblingSections = body.GetChildItemsList()!.OfType<SectionItemType>().ToList();
			// Comment: rationale for count == 2 -- body started with exactly one child section (the original);
			// after one Copy() call there must be exactly the original plus one repeat, no more, no fewer.
			Assert.HasCount(2, siblingSections);
			var sectionCopy = siblingSections[1];

			// Comment: rationale for suffix assertions -- CloneAndRepeatSubtree assigns a "__N" suffix to both
			// ID and name, where N is FormDesignType.RepeatCounter after being incremented by Move(); the first
			// repeat of any subtree in a fresh form must be suffixed "__1".
			Assert.AreEqual(origSectionId + "__1", sectionCopy.ID);
			Assert.AreEqual(origSectionName + "__1", sectionCopy.name);
			// Comment: rationale -- sGuid/ObjectGUID must be freshly generated for the clone so it's a distinct,
			// independently addressable node in the TopNode dictionaries, not an alias of the original.
			Assert.AreNotEqual(origSectionSGuid, sectionCopy.sGuid);

			// The clone's own descendants (the question) must ALSO receive the repeat suffix -- proving the
			// whole subtree was deep-cloned and refreshed, not just the root node.
			var questionCopy = sectionCopy.GetChildItemsList()!.OfType<QuestionItemType>().Single();
			Assert.AreEqual(origQuestionName + "__1", questionCopy.name);
			Assert.AreEqual(origQuestionId + "__1", questionCopy.ID);

			// Original question must remain untouched too.
			Assert.AreEqual(origQuestionName, question.name);
			Assert.AreEqual(origQuestionId, question.ID);
		}

		/// <summary>
		/// Repeating the SAME source section twice in succession must produce sequential "__1"/"__2" suffixes
		/// (driven by <c>FormDesignType.RepeatCounter</c>), confirming multi-repeat scenarios (e.g. "specimen 1",
		/// "specimen 2", "specimen 3"...) work correctly starting from a freshly-constructed tree.
		/// </summary>
		[TestMethod]
		public void Copy_CalledTwice_ProducesSequentialRepeatSuffixes()
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, "FD.Repeat.Construction.Multi");
			var body = fd.AddBody();
			var section = body.AddChildSection("S.Specimen", "Specimen");
			section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Site", "Site");

			Assert.IsTrue(section.Copy(), "First Copy() call must succeed");
			Assert.IsTrue(section.Copy(), "Second Copy() call (repeating the ORIGINAL again) must also succeed");

			var siblingSections = body.GetChildItemsList()!.OfType<SectionItemType>().ToList();
			// Comment: rationale -- original + 2 repeats == 3 total sections in the body's ChildItemsType list.
			Assert.HasCount(3, siblingSections);

			// Comment: rationale -- repeats are always inserted immediately after the original (per
			// IMoveRemoveExtensions.Move's CloneAndRepeatSubtree branch, which forces newListIndex to
			// immediately follow the source node), so index 1 is the "__1" repeat and index 2 is "__2".
			Assert.AreEqual(section.ID + "__1", siblingSections[1].ID);
			Assert.AreEqual(section.ID + "__2", siblingSections[2].ID);
			Assert.AreNotEqual(siblingSections[1].sGuid, siblingSections[2].sGuid);
		}
	}
}
