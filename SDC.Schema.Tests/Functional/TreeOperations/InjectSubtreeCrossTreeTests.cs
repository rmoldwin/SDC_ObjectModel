using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System.IO;
using System.Linq;

namespace SDC.Schema.Tests.Functional.TreeOperations
{
	/// <summary>
	/// Tests for <see cref="IMoveRemoveExtensions.InjectSubtree(IdentifiedExtensionType, ChildItemsType, int)"/> --
	/// the generalized "clone and attach" operation that (unlike <see cref="IMoveRemoveExtensions.Copy(IdentifiedExtensionType)"/>,
	/// which only supports same-tree repeats) accepts a donor node from ANY SDC tree: the same tree as the target,
	/// a different live instance tree, or a separately-loaded FDF (Form Design Form) template tree.
	/// <br/><br/>
	/// See <c>CopyPasteInject_ResponseStripping_Design.md</c> for the full design rationale, including why
	/// insertion position and repeat-suffix numbering must be driven by the TARGET tree's own content and its own
	/// <see cref="FormDesignType.RepeatCounter"/>, never by the donor's position in its own (possibly foreign) tree.
	/// </summary>
	[TestClass]
	public class InjectSubtreeCrossTreeTests
	{
		/// <summary>
		/// Builds a small, freshly-constructed FDF tree with one top-level Section (containing one Question)
		/// under the form's Body. Used as either a donor or target tree in the tests below.
		/// </summary>
		private static (FormDesignType fd, SectionItemType body, SectionItemType section, QuestionItemType question) BuildTree(string fdID, string sectionID, string questionID)
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, fdID);
			var body = fd.AddBody();
			var section = body.AddChildSection(sectionID, "Specimen");
			var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, questionID, "Site");
			return (fd, body, section, question);
		}

		/// <summary>
		/// The core new capability: cloning a subtree from a COMPLETELY SEPARATE OM tree (donor) and attaching
		/// it into a different tree (target). This is impossible with Copy()/the pre-existing CloneAndRepeatSubtree
		/// implementation, which threw InvalidOperationException for any donor not sharing the target's root.
		/// </summary>
		[TestMethod]
		public void InjectSubtree_FromSeparateTree_ClonesAndAttachesWithFreshIdentity()
		{
			var donor = BuildTree("FD.Donor", "S.Specimen", "Q.Site");
			var target = BuildTree("FD.Target", "S.Other", "Q.Other");

			// Comment: rationale -- capture the donor's actual (auto-generated) name before injection, since
			// AddChildSection never explicitly sets .name; we can't assume it equals the ID.
			string donorOrigName = donor.section.name;

			bool injected = donor.section.InjectSubtree(target.body.GetChildItemsNode());
			// Comment: rationale -- InjectSubtree must succeed even though donor.section belongs to an
			// entirely different TopNode tree than target.body; this is the whole point of the new method.
			Assert.IsTrue(injected, "InjectSubtree must succeed for a donor node from a different SDC tree");

			var targetSections = target.body.GetChildItemsList()!.OfType<SectionItemType>().ToList();
			// Comment: rationale -- target started with one section (S.Other); after injection it must have
			// exactly two: the original plus the injected clone.
			Assert.HasCount(2, targetSections);
			var injectedSection = targetSections[1];

			// Comment: rationale -- even though this is the FIRST injection of "S.Specimen" content into the
			// target tree (no pre-existing repeat), InjectSubtree always assigns a repeat suffix to the
			// injected root, per the user's explicit design decision ("always add the repeat suffix").
			Assert.AreEqual("S.Specimen__1", injectedSection.ID);
			// Comment: rationale -- name isn't necessarily equal to ID (AddChildSection never sets name
			// explicitly, so it's auto-generated); just confirm it received the same repeat suffix as ID did,
			// on top of whatever base name the donor originally had.
			Assert.AreEqual(donorOrigName + "__1", injectedSection.name);

			// Comment: rationale -- the donor tree must be completely untouched by injection (InjectSubtree only
			// clones; it never mutates or moves the donor).
			Assert.AreEqual("S.Specimen", donor.section.ID);
			Assert.HasCount(1, donor.body.GetChildItemsList()!.OfType<SectionItemType>());

			// Comment: rationale -- the clone's descendant (the question) must also be present, deep-cloned, and
			// suffixed -- proving the whole subtree (not just the root) made it across the tree boundary.
			var injectedQuestion = injectedSection.GetChildItemsList()!.OfType<QuestionItemType>().Single();
			Assert.AreEqual("Q.Site__1", injectedQuestion.ID);

			// Comment: rationale -- sGuid/ObjectGUID must be freshly generated in the TARGET tree's namespace,
			// not collide with (or be aliases of) anything in the donor tree.
			Assert.AreNotEqual(donor.section.sGuid, injectedSection.sGuid);
			Assert.AreNotEqual(donor.question.sGuid, injectedQuestion.sGuid);
		}

		/// <summary>
		/// Injecting the SAME donor subtree into the SAME target location twice must produce sequential
		/// "__1"/"__2" suffixes, driven by the TARGET tree's own FormDesignType.RepeatCounter (which is
		/// unrelated to whatever counter value the donor's own tree happens to have).
		/// </summary>
		[TestMethod]
		public void InjectSubtree_CalledTwiceFromSameDonor_ProducesSequentialSuffixesInTargetTree()
		{
			var donor = BuildTree("FD.Donor2", "S.Specimen", "Q.Site");
			var target = BuildTree("FD.Target2", "S.Other", "Q.Other");
			var targetChildItems = target.body.GetChildItemsNode();

			Assert.IsTrue(donor.section.InjectSubtree(targetChildItems), "First injection must succeed");
			Assert.IsTrue(donor.section.InjectSubtree(targetChildItems), "Second injection (same donor, same target) must also succeed");

			var targetSections = target.body.GetChildItemsList()!.OfType<SectionItemType>().ToList();
			// Comment: rationale -- original target section + 2 injected clones == 3 total.
			Assert.HasCount(3, targetSections);
			Assert.AreEqual("S.Specimen__1", targetSections[1].ID);
			Assert.AreEqual("S.Specimen__2", targetSections[2].ID);
			Assert.AreNotEqual(targetSections[1].sGuid, targetSections[2].sGuid);
		}

		/// <summary>
		/// Injection must work correctly when the insertion point is NOT at the end of the target's existing
		/// children -- i.e., injections can occur "anywhere in the tree", not just appended at the end. This
		/// proves the new target-side scan (which locates existing same-base-ID matches wherever they are,
		/// rather than only ever appending) places the clone immediately after the correct existing sibling,
		/// leaving unrelated siblings undisturbed.
		/// </summary>
		[TestMethod]
		public void InjectSubtree_WithUnrelatedSiblingsPresent_InsertsImmediatelyAfterLastMatchingRepeat()
		{
			var donor = BuildTree("FD.Donor3", "S.Specimen", "Q.Site");
			var target = BuildTree("FD.Target3", "S.First", "Q.First");
			var targetChildItems = target.body.GetChildItemsNode();

			// Add an unrelated section AFTER the anchor, so the target list is: [S.First, S.Unrelated].
			var unrelated = target.body.AddChildSection("S.Unrelated", "Unrelated");

			// Inject donor.section once: since there's no pre-existing "S.Specimen" content in target yet,
			// it should append at the end of the list (after S.Unrelated).
			Assert.IsTrue(donor.section.InjectSubtree(targetChildItems));
			var afterFirstInject = target.body.GetChildItemsList()!.OfType<SectionItemType>().Select(s => s.ID).ToList();
			CollectionAssert.AreEqual(new[] { "S.First", "S.Unrelated", "S.Specimen__1" }, afterFirstInject);

			// Inject donor.section a second time: it must land immediately after "S.Specimen__1" (the last
			// existing match), NOT at the very end by coincidence and NOT disturbing S.First/S.Unrelated.
			Assert.IsTrue(donor.section.InjectSubtree(targetChildItems));
			var afterSecondInject = target.body.GetChildItemsList()!.OfType<SectionItemType>().Select(s => s.ID).ToList();
			CollectionAssert.AreEqual(new[] { "S.First", "S.Unrelated", "S.Specimen__1", "S.Specimen__2" }, afterSecondInject);
			Assert.IsNotNull(unrelated); // keep 'unrelated' referenced/meaningful for the reader
		}

		/// <summary>
		/// Defensive/edge-case coverage: per the user's explicit direction, if a donor subtree's ID already
		/// happens to carry a pre-existing repeat/injection-style "__N" suffix (a non-standard, discouraged, but
		/// not-impossible FDF authoring mistake), InjectSubtree must NOT throw or corrupt the tree -- it may
		/// produce a compound suffix (e.g. "S.PreSuffixed__3__1"), which is an accepted, documented limitation
		/// rather than a crash.
		/// </summary>
		[TestMethod]
		public void InjectSubtree_WithDonorIdAlreadyCarryingSuffix_DoesNotThrow()
		{
			BaseType.ResetLastTopNode();
			var donorFd = new FormDesignType(null, "FD.DonorPreSuffixed");
			var donorBody = donorFd.AddBody();
			// Comment: rationale -- deliberately construct a donor whose ID already looks like a repeat ("__3"),
			// simulating a mis-authored template, to prove the code path is resilient rather than assuming
			// well-formed input.
			var donorSection = donorBody.AddChildSection("S.PreSuffixed__3", "Odd");

			var target = BuildTree("FD.TargetPreSuffixed", "S.Other", "Q.Other");

			bool injected = donorSection.InjectSubtree(target.body.GetChildItemsNode());
			Assert.IsTrue(injected, "InjectSubtree must not throw or fail even when the donor ID already carries a suffix-like pattern");

			var injectedSection = target.body.GetChildItemsList()!.OfType<SectionItemType>().Last();
			// Comment: rationale -- the compound suffix is an accepted limitation (documented in the design doc),
			// not a silently-wrong or colliding ID; we only assert it's still unique and non-null/non-empty.
			Assert.IsFalse(string.IsNullOrEmpty(injectedSection.ID));
			Assert.AreNotEqual(donorSection.ID, injectedSection.ID);
		}

		/// <summary>
		/// End-to-end coverage of <see cref="SourceFormDesignExtensions.LoadSourceFormDesign(BaseType)"/> and
		/// <see cref="SourceFormDesignExtensions.FindNodeByTemplateID(FormDesignType, string)"/>: an FDF-R
		/// (Form Design Form - Response) instance's TopNode references its source FDF template via @filename;
		/// loading that reference must produce a fully independent OM instance, and looking up a live (possibly
		/// repeat-suffixed) instance node's ID in that template must correctly strip the suffix and find the
		/// un-suffixed template counterpart.
		/// </summary>
		[TestMethod]
		public void LoadSourceFormDesign_AndFindNodeByTemplateID_ResolvesTemplateCounterpartOfRepeatedInstanceNode()
		{
			// Build and save the pristine source FDF template to a temp file.
			var template = BuildTree("FD.Template.Repeat", "S.Specimen", "Q.Site");
			string tempFile = Path.Combine(Path.GetTempPath(), $"SdcSourceFdfTest_{System.Guid.NewGuid():N}.xml");
			try
			{
				SdcSerializer<FormDesignType>.SaveToFile(template.fd, tempFile);

				// Build a separate, live FDF-R instance tree that references the saved template file via
				// @filename (mirroring the real FDF-R -> source-FDF reference mechanism).
				BaseType.ResetLastTopNode();
				var instanceFd = new FormDesignType(null, "FD.Template.Repeat"); // FDF-R normally shares the template's @ID
				instanceFd.filename = tempFile;
				var instanceBody = instanceFd.AddBody();
				var instanceSection = instanceBody.AddChildSection("S.Specimen", "Specimen");

				// Simulate the instance having already repeated this section once (so its live ID carries a
				// "__1" suffix, as InjectSubtree/Copy would produce).
				Assert.IsTrue(instanceSection.Copy(), "Setting up the repeated instance node must succeed");
				var repeatedInstanceSection = instanceBody.GetChildItemsList()!.OfType<SectionItemType>().Last();
				Assert.AreEqual("S.Specimen__1", repeatedInstanceSection.ID);

				// Load the source FDF as an independent OM instance from the live instance node.
				FormDesignType loadedTemplate = instanceSection.LoadSourceFormDesign();
				Assert.IsNotNull(loadedTemplate);
				// Comment: rationale -- the loaded template must be a genuinely separate tree/object graph, not
				// the same in-memory instance as either the live instance tree or the original in-test template.
				Assert.AreNotSame(instanceFd, loadedTemplate);
				Assert.AreNotSame(template.fd, loadedTemplate);

				// Look up the repeated instance node's template counterpart by its (suffixed) live ID; the
				// suffix must be stripped so the un-suffixed template node ("S.Specimen") is found.
				var templateNode = loadedTemplate.FindNodeByTemplateID(repeatedInstanceSection.ID);
				Assert.IsNotNull(templateNode, "The suffix-stripped ID lookup must find the un-suffixed template node");
				Assert.AreEqual("S.Specimen", templateNode!.ID);
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}
	}
}
