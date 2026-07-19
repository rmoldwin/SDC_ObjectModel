using System.Text.RegularExpressions;

namespace SDC.Schema.Extensions
{
	/// <summary>
	/// Extension methods that let an FDF-R (Form Design Form - Response) instance locate and load its source FDF
	/// (Form Design Form template) as a fully independent OM (Object Model) instance, so that repeat/injection
	/// operations (see <see cref="IMoveRemoveExtensions.InjectSubtree(IdentifiedExtensionType, ChildItemsType, int)"/>
	/// and <see cref="IMoveRemoveExtensions.Copy(IdentifiedExtensionType)"/>) can clone pristine template content
	/// -- with no user-entered response data, but with genuine template-default <c>@selected</c>/<c>@val</c> content
	/// intact -- rather than cloning a live, possibly response-laden instance node.
	/// See <c>CopyPasteInject_ResponseStripping_Design.md</c> for the full design rationale.
	/// </summary>
	public static class SourceFormDesignExtensions
	{
		/// <summary>
		/// Matches a trailing repeat/injection suffix of the form <c>"__123"</c> at the end of an SDC <c>@ID</c>
		/// or <c>@name</c> value (see <see cref="SdcUtil.RefreshMode.CloneAndRepeatSubtree"/>).
		/// </summary>
		private static readonly Regex RepeatSuffixRegex = new(@"^(.*)__\d+$", RegexOptions.Compiled);

		/// <summary>
		/// Strips a single trailing repeat/injection suffix (e.g. <c>"__3"</c>) from an SDC <c>ID</c> value, if
		/// present, returning the "base" (un-suffixed) ID. IDs with no suffix are returned unchanged.
		/// </summary>
		/// <param name="id">The (possibly repeat-suffixed) <c>@ID</c> value.</param>
		/// <returns>The base ID, with any single trailing <c>"__N"</c> suffix removed.</returns>
		public static string StripRepeatSuffix(this string id)
		{
			if (string.IsNullOrEmpty(id)) return id;
			Match m = RepeatSuffixRegex.Match(id);
			return m.Success ? m.Groups[1].Value : id;
		}

		/// <summary>
		/// Locates and loads the source FDF (Form Design Form template) referenced by an FDF-R instance's
		/// <see cref="FormDesignType"/> TopNode, as a fully independent OM (Object Model) instance -- an entirely
		/// separate <see cref="ITopNode"/> tree from <paramref name="instanceNode"/>'s own tree.
		/// <br/><br/>
		/// The source FDF is located via <see cref="FormDesignType.filename"/> on the FDF-R's TopNode element,
		/// which is expected to be a resolvable file path to the source FDF (per its XML doc comment: "the
		/// filename of the FDF when it is saved to a file storage device"). <see cref="FormDesignType.fullURI"/>
		/// is a logical identity string (baseURI + lineage + version), not generally a resolvable file path, so
		/// it is not used to load the file -- but it is included in error messages to help identify which
		/// template was expected.
		/// </summary>
		/// <param name="instanceNode">Any node within the live FDF-R instance tree whose source FDF should be loaded.</param>
		/// <returns>The root <see cref="FormDesignType"/> of the independently-loaded source FDF OM instance.</returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if <paramref name="instanceNode"/>'s TopNode is not a <see cref="FormDesignType"/>, if no
		/// <see cref="FormDesignType.filename"/> is present to locate the source FDF, or if the referenced file
		/// cannot be loaded as a valid <see cref="FormDesignType"/>.
		/// </exception>
		public static FormDesignType LoadSourceFormDesign(this BaseType instanceNode)
		{
			if (instanceNode.TopNode is not FormDesignType instanceFd)
				throw new InvalidOperationException(
					$"{nameof(LoadSourceFormDesign)} requires the TopNode of {nameof(instanceNode)} to be a {nameof(FormDesignType)}.");

			if (string.IsNullOrWhiteSpace(instanceFd.filename))
				throw new InvalidOperationException(
					$"The FDF-R instance's TopNode (ID='{instanceFd.ID}', fullURI='{instanceFd.fullURI}') " +
					$"does not carry a {nameof(FormDesignType.filename)} reference to its source FDF, so the source " +
					$"FDF cannot be located and loaded. Consider the fallback (clean-copy-of-live-instance) path " +
					$"instead when no source FDF is available -- see CopyPasteInject_ResponseStripping_Design.md.");

			FormDesignType sourceFd;
			try
			{
				// TopNodeSerializer<T>.DeserializeFromXmlPath (unlike the lower-level
				// SdcSerializer<T>.LoadFromFile) also runs SdcUtil.ReflectRefreshTree over the
				// resulting tree, which registers every node into the new, independent OM instance's
				// own ITopNode dictionaries. Without that registration pass, tree-walking helpers such
				// as FindNodeByTemplateID (which relies on GetSubtreeList()) would not see any of the
				// loaded template's descendant nodes.
				sourceFd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(instanceFd.filename);
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(
					$"Failed to load the source FDF referenced by filename='{instanceFd.filename}' " +
					$"(fullURI='{instanceFd.fullURI}').", ex);
			}

			if (sourceFd is null)
				throw new InvalidOperationException(
					$"Loading the source FDF from filename='{instanceFd.filename}' returned a null {nameof(FormDesignType)}.");

			return sourceFd;
		}

		/// <summary>
		/// Finds the node within a separately-loaded source FDF template tree (see
		/// <see cref="LoadSourceFormDesign(BaseType)"/>) that corresponds to a given node in a live FDF-R
		/// instance tree, by matching <c>@ID</c>.
		/// <br/><br/>
		/// Per SDC's ID/name uniqueness rule, IDs are stable between an FDF template and any FDF-R instance
		/// derived from it, <b>except</b> that the client may have added repeating subtrees to the live
		/// instance, whose IDs carry a <c>"__N"</c> repeat suffix not present on the corresponding template
		/// node. This lookup strips any such suffix from <paramref name="liveID"/> before searching, so that a
		/// repeated instance node still resolves to its correct (un-suffixed) template counterpart.
		/// </summary>
		/// <param name="templateRoot">The root of a separately-loaded source FDF template OM instance.</param>
		/// <param name="liveID">The <c>@ID</c> of the live FDF-R instance node to find the template counterpart of.
		/// May itself carry a repeat suffix (e.g. <c>"Section1__2"</c>); the suffix is stripped before matching.</param>
		/// <returns>The matching <see cref="IdentifiedExtensionType"/> node in the template tree, or <see langword="null"/>
		/// if no node with that (suffix-stripped) ID exists there.</returns>
		public static IdentifiedExtensionType? FindNodeByTemplateID(this FormDesignType templateRoot, string liveID)
		{
			string templateID = liveID.StripRepeatSuffix();

			if (templateRoot.ID == templateID) return templateRoot;

			return templateRoot.GetSubtreeList()?
				.OfType<IdentifiedExtensionType>()
				.FirstOrDefault(n => n.ID == templateID);
		}
	}
}
