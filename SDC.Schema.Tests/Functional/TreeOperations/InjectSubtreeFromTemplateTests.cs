using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.IO;
using System.Linq;

namespace SDC.Schema.Tests.Functional.TreeOperations
{
	/// <summary>
	/// Tests for <see cref="IMoveRemoveExtensions.InjectSubtreeFromTemplate(IdentifiedExtensionType, ChildItemsType, int, bool)"/> --
	/// the recommended entry point for repeating/injecting a subtree FROM A LIVE FDF-R (Form Design Form -
	/// Response) INSTANCE. By default, it resolves and clones the donor's counterpart in the referenced source
	/// FDF (Form Design Form) template instead of the live instance node, so the copy is automatically free of
	/// user-entered response data (a template has none by definition) while any <c>@readOnly</c>-locked default
	/// <c>@selected</c>/<c>@val</c> content -- which can only be read reliably from the template -- is
	/// automatically preserved.
	/// <br/><br/>
	/// See <c>CopyPasteInject_ResponseStripping_Design.md</c> §8 for the full design rationale, including why
	/// this is NOT implemented as an active "strip response values" tree-walk: cloning from the template is
	/// sufficient and correct because "response" is explicitly defined (by the domain expert) as "anything found
	/// in the instance OM that is not in the original FDF template" -- so a template-sourced clone cannot, by
	/// construction, contain any response data.
	/// </summary>
	[TestClass]
	public class InjectSubtreeFromTemplateTests
	{
		/// <summary>
		/// Builds and saves a pristine source FDF template to a temp file: one Section containing one
		/// QuestionSingle with two ListItems, where "LI.Yes" is the @readOnly-locked default selection
		/// (selected=true in the template itself). Returns the saved file path and the in-memory template tree
		/// (for reference/cleanup).
		/// </summary>
		private static (FormDesignType fd, string filePath) BuildAndSaveTemplate(string fdID, string tempFile)
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, fdID);
			var body = fd.AddBody();
			var section = body.AddChildSection("S.Specimen", "Specimen");
			var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Site", "Site");
			question.readOnly = true; // locked: the template's default selection below must always be honored
			var liYes = question.AddListItem("LI.Yes", "Yes");
			liYes.selected = true; // the one and only template-authored default
			var liNo = question.AddListItem("LI.No", "No");
			liNo.selected = false;

			SdcSerializer<FormDesignType>.SaveToFile(fd, tempFile);
			return (fd, tempFile);
		}

		/// <summary>
		/// Builds a live FDF-R instance tree that references <paramref name="templateFile"/> via @filename, with
		/// a Section/Question/ListItem subtree matching the template's IDs, but whose CURRENT (user-answered)
		/// selection diverges from the template default -- simulating a real user response that overrode the
		/// (non-readOnly, in this instance) default.
		/// </summary>
		private static (FormDesignType fd, SectionItemType body, SectionItemType section, QuestionItemType question) BuildInstanceReferencingTemplate(string fdID, string templateFile)
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, fdID) { filename = templateFile };
			var body = fd.AddBody();
			var section = body.AddChildSection("S.Specimen", "Specimen");
			var question = section.AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Site", "Site");
			question.readOnly = true;
			var liYes = question.AddListItem("LI.Yes", "Yes");
			liYes.selected = false; // diverges from the template default (true) -- simulates a user response
			var liNo = question.AddListItem("LI.No", "No");
			liNo.selected = true; // the user's actual answer, absent from the template

			return (fd, body, section, question);
		}

		/// <summary>
		/// Default mode (preserveInstanceData: false): the injected clone must reflect the TEMPLATE's default
		/// selection (LI.Yes.selected == true), NOT the live instance's user-entered answer (LI.Yes.selected ==
		/// false in the instance). This is the core "response-free by construction" guarantee.
		/// </summary>
		[TestMethod]
		public void InjectSubtreeFromTemplate_DefaultMode_ClonesTemplateDefaultsNotInstanceResponses()
		{
			string tempFile = Path.Combine(Path.GetTempPath(), $"SdcInjectFromTemplateTest_{System.Guid.NewGuid():N}.xml");
			try
			{
				BuildAndSaveTemplate("FD.Template", tempFile);

				var instance = BuildInstanceReferencingTemplate("FD.Instance", tempFile);
				var target = instance.body; // inject into the SAME instance tree's body, elsewhere in the tree

				bool injected = instance.section.InjectSubtreeFromTemplate(target.GetChildItemsNode());
				Assert.IsTrue(injected, "InjectSubtreeFromTemplate must succeed when the source FDF template is resolvable");

				var sections = target.GetChildItemsList()!.OfType<SectionItemType>().ToList();
				Assert.HasCount(2, sections, "original instance section + template-sourced injected clone");
				var injectedSection = sections[1];
				Assert.AreEqual("S.Specimen__1", injectedSection.ID);

				var injectedQuestion = injectedSection.GetChildItemsList()!.OfType<QuestionItemType>().Single();
				var injectedListItems = injectedQuestion.GetListItems()!.OfType<ListItemType>().ToList();
				var injectedYes = injectedListItems.Single(li => li.ID == "LI.Yes__1");
				var injectedNo = injectedListItems.Single(li => li.ID == "LI.No__1");

				// Comment: rationale -- these MUST match the template's defaults (Yes=true/No=false), proving the
				// clone came from the template, not from the live instance's diverging user response
				// (instance.question has Yes=false/No=true).
				Assert.IsTrue(injectedYes.selected, "Injected clone must carry the TEMPLATE's default selection, not the instance's user-entered response");
				Assert.IsFalse(injectedNo.selected, "Injected clone must not carry the instance's user-entered response");

				// Comment: rationale -- confirm the live instance itself is untouched (still shows the user's
				// actual, diverging answer) -- InjectSubtreeFromTemplate must never mutate the donor.
				Assert.IsFalse(instance.question.GetListItems()!.OfType<ListItemType>().Single(li => li.ID == "LI.Yes").selected);
				Assert.IsTrue(instance.question.GetListItems()!.OfType<ListItemType>().Single(li => li.ID == "LI.No").selected);
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}

		/// <summary>
		/// Opt-in mode (preserveInstanceData: true): the injected clone must instead reflect the LIVE INSTANCE's
		/// current (user-answered) values, not the template defaults -- proving the flag genuinely switches
		/// which tree is cloned from, with no template resolution/loading required at all.
		/// </summary>
		[TestMethod]
		public void InjectSubtreeFromTemplate_PreserveInstanceDataTrue_ClonesLiveInstanceResponses()
		{
			string tempFile = Path.Combine(Path.GetTempPath(), $"SdcInjectFromTemplateTest_{System.Guid.NewGuid():N}.xml");
			try
			{
				BuildAndSaveTemplate("FD.Template2", tempFile);
				var instance = BuildInstanceReferencingTemplate("FD.Instance2", tempFile);
				var target = instance.body;

				bool injected = instance.section.InjectSubtreeFromTemplate(target.GetChildItemsNode(), preserveInstanceData: true);
				Assert.IsTrue(injected);

				var sections = target.GetChildItemsList()!.OfType<SectionItemType>().ToList();
				var injectedSection = sections[1];
				var injectedQuestion = injectedSection.GetChildItemsList()!.OfType<QuestionItemType>().Single();
				var injectedListItems = injectedQuestion.GetListItems()!.OfType<ListItemType>().ToList();

				// Comment: rationale -- opposite of the default-mode test above: with preserveInstanceData=true,
				// the clone must carry the INSTANCE's current (user-answered) values (Yes=false/No=true), not
				// the template's defaults (Yes=true/No=false).
				Assert.IsFalse(injectedListItems.Single(li => li.ID == "LI.Yes__1").selected);
				Assert.IsTrue(injectedListItems.Single(li => li.ID == "LI.No__1").selected);
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}

		/// <summary>
		/// Response data living in a QuestionFill's ResponseFieldType/DataTypes_DEType (here, a
		/// <c>string_DEtype</c>, with a strongly-typed <c>val</c> property) must also be resolved from the
		/// template by default -- proving the guarantee isn't limited to ListItem.selected.
		/// </summary>
		[TestMethod]
		public void InjectSubtreeFromTemplate_DefaultMode_ClonesTemplateResponseFieldValueNotInstanceValue()
		{
			string tempFile = Path.Combine(Path.GetTempPath(), $"SdcInjectFromTemplateTest_{System.Guid.NewGuid():N}.xml");
			try
			{
				BaseType.ResetLastTopNode();
				var templateFd = new FormDesignType(null, "FD.TemplateQF");
				var templateBody = templateFd.AddBody();
				// Template default response value: "TemplateDefault".
				templateBody.AddChildQuestionResponse("Q.Fill", out _, defTitle: "Fill", valDefault: "TemplateDefault");
				SdcSerializer<FormDesignType>.SaveToFile(templateFd, tempFile);

				BaseType.ResetLastTopNode();
				var instanceFd = new FormDesignType(null, "FD.InstanceQF") { filename = tempFile };
				var instanceBody = instanceFd.AddBody();
				// Live instance's CURRENT (user-entered) value diverges from the template default.
				var instanceQuestion = instanceBody.AddChildQuestionResponse("Q.Fill", out _, defTitle: "Fill", valDefault: "UserEnteredAnswer");

				bool injected = instanceQuestion.InjectSubtreeFromTemplate(instanceBody.GetChildItemsNode());
				Assert.IsTrue(injected);

				var injectedQuestion = instanceBody.GetChildItemsList()!.OfType<QuestionItemType>().Single(q => q.ID == "Q.Fill__1");
				var injectedDataTypeNode = injectedQuestion.GetResponseDataTypeNode();
				Assert.IsNotNull(injectedDataTypeNode);
				// Comment: rationale -- read .val directly via the concrete string_DEtype rather than through the
				// IVal.ValXmlString interface: ValXmlString is unimplemented (throws NotImplementedException) for
				// several concrete *_Stype types in this codebase today (a pre-existing gap unrelated to this
				// task), so asserting via the concrete .val property is the reliable approach here.
				var injectedVal = ((string_DEtype)injectedDataTypeNode!).val;

				// Comment: rationale -- must be the TEMPLATE's value ("TemplateDefault"), not the live instance's
				// current user-entered value ("UserEnteredAnswer"), proving the template-sourced guarantee
				// extends to ResponseFieldType/DataTypes_DEType values, not just ListItems.
				Assert.AreEqual("TemplateDefault", injectedVal);
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}

		/// <summary>
		/// If the live instance node's ID has no counterpart in the source FDF template (e.g. the node was
		/// renamed or added to the instance after the template was last saved), InjectSubtreeFromTemplate must
		/// fail loudly with a clear, actionable exception rather than silently misbehaving or throwing an opaque
		/// cast/null-reference error.
		/// </summary>
		[TestMethod]
		public void InjectSubtreeFromTemplate_DefaultMode_WithNoTemplateCounterpart_ThrowsClearException()
		{
			string tempFile = Path.Combine(Path.GetTempPath(), $"SdcInjectFromTemplateTest_{System.Guid.NewGuid():N}.xml");
			try
			{
				BuildAndSaveTemplate("FD.Template3", tempFile);

				BaseType.ResetLastTopNode();
				var instanceFd = new FormDesignType(null, "FD.Instance3") { filename = tempFile };
				var instanceBody = instanceFd.AddBody();
				// Comment: rationale -- "S.OrphanedInInstance" was never part of the saved template, simulating
				// a node added to the instance (or renamed) after the template was last saved.
				var orphanSection = instanceBody.AddChildSection("S.OrphanedInInstance", "Orphan");

				var ex = Assert.Throws<InvalidOperationException>(() =>
					orphanSection.InjectSubtreeFromTemplate(instanceBody.GetChildItemsNode()));
				// Comment: rationale -- the message must be actionable: identify the missing ID and suggest the
				// preserveInstanceData escape hatch, rather than surfacing a raw NullReferenceException/InvalidCastException.
				StringAssert.Contains(ex.Message, "S.OrphanedInInstance");
				StringAssert.Contains(ex.Message, "preserveInstanceData");
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}

		/// <summary>
		/// A repeated donor's suffixed ID (e.g. "S.Specimen__2") must still correctly resolve to the template's
		/// un-suffixed counterpart ("S.Specimen"), reusing the same suffix-stripping lookup already proven in
		/// <see cref="InjectSubtreeCrossTreeTests"/>.
		/// </summary>
		[TestMethod]
		public void InjectSubtreeFromTemplate_DefaultMode_WithRepeatSuffixedDonorId_ResolvesTemplateCounterpart()
		{
			string tempFile = Path.Combine(Path.GetTempPath(), $"SdcInjectFromTemplateTest_{System.Guid.NewGuid():N}.xml");
			try
			{
				BuildAndSaveTemplate("FD.Template4", tempFile);
				var instance = BuildInstanceReferencingTemplate("FD.Instance4", tempFile);

				// Simulate the instance already having repeated this section once, so its live ID carries "__1".
				Assert.IsTrue(instance.section.Copy());
				var repeatedInstanceSection = instance.body.GetChildItemsList()!.OfType<SectionItemType>().Last();
				Assert.AreEqual("S.Specimen__1", repeatedInstanceSection.ID);

				bool injected = repeatedInstanceSection.InjectSubtreeFromTemplate(instance.body.GetChildItemsNode());
				Assert.IsTrue(injected, "Must resolve the un-suffixed template counterpart of a repeat-suffixed donor ID");

				var sections = instance.body.GetChildItemsList()!.OfType<SectionItemType>().ToList();
				// original section, then Copy()'s plain same-tree repeat (__1), then our template-sourced
				// injection (__2) -- the suffix-stripping lookup must find the template's "S.Specimen" for the
				// "S.Specimen__1" donor, regardless of how that donor's suffix was produced.
				Assert.AreEqual("S.Specimen__2", sections.Last().ID);
			}
			finally
			{
				if (File.Exists(tempFile)) File.Delete(tempFile);
			}
		}
	}
}
