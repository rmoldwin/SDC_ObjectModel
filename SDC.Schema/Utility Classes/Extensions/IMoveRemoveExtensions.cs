using CSharpVitamins;
using System.CodeDom;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SDC.Schema.SdcUtil;
using static System.Collections.Specialized.BitVector32;



//using SDC;
namespace SDC.Schema.Extensions
{
	public static class IMoveRemoveExtensions
	{
		private static TreeSibComparer treeSibComparer = new();

		/// <summary>
		/// Holds write locks on one or two <see cref="ReaderWriterLockSlim"/> tree locks acquired
		/// in a deterministic order (smaller <see cref="ITopNode"/> <see cref="BaseType.ObjectGUID"/>
		/// acquired first) to prevent AB/BA deadlock during cross-tree <see cref="Move"/> calls.
		/// </summary>
		/// <remarks>
		/// TS-6 fix: <see cref="Move"/> previously performed <c>IList.Remove</c> and <c>IList.Add</c>
		/// with no outer lock and read non-thread-safe <c>Dictionary&lt;&gt;</c> collections
		/// (<c>_ParentNodes</c>, <c>_ChildNodes</c>) without holding any lock. This class serialises
		/// the entire <see cref="Move"/> body — including all dictionary reads, IList mutations, and
		/// nested <c>RegisterAll</c>/<c>UnRegisterAll</c> dictionary updates — atomically under one
		/// or two write locks, preventing concurrent corruption when multiple threads call
		/// <see cref="Move"/> simultaneously. The lock is acquired at the very top of
		/// <see cref="Move"/> (after null guards) so every code path is protected.
		/// </remarks>
		private sealed class DualWriteLock : IDisposable
		{
			private readonly ReaderWriterLockSlim? _lock1;
			private readonly ReaderWriterLockSlim? _lock2;
			private bool _disposed;

			/// <param name="lock1">First lock acquired (may be null if source has no TopNode yet).</param>
			/// <param name="lock2">Second lock acquired, or null for same-tree moves.</param>
			internal DualWriteLock(ReaderWriterLockSlim? lock1, ReaderWriterLockSlim? lock2)
			{
				_lock1 = lock1;
				_lock2 = lock2;
				_lock1?.EnterWriteLock();
				_lock2?.EnterWriteLock();
			}

			public void Dispose()
			{
				if (_disposed) return;
				_disposed = true;
				// Release in reverse acquisition order.
				_lock2?.ExitWriteLock();
				_lock1?.ExitWriteLock();
			}
		}

		/// <summary>
		/// Acquires write locks on the trees that own <paramref name="source"/> and
		/// <paramref name="target"/> in a deterministic GUID order to prevent AB/BA deadlock.
		/// For same-tree moves only one lock is acquired.
		/// </summary>
		/// <remarks>
		/// TS-6 fix: called at the entry point of every <see cref="Move"/> code path that mutates
		/// an <c>IList</c> (both the same-tree <c>MoveSingleNode</c> IList branch and the
		/// <c>UpdateNodeIdentity</c> cross-tree branch). The returned <see cref="DualWriteLock"/>
		/// must be disposed in a <c>using</c> statement so that the locks are released even when
		/// an exception is thrown.
		/// </remarks>
		private static DualWriteLock AcquireMoveLocks(BaseType source, BaseType target)
		{
			var sourceLock = source.TopNode is _ITopNode stn ? stn.TreeRwLock : null;
			var targetLock = target.TopNode is _ITopNode ttn ? ttn.TreeRwLock : null;

			if (sourceLock is null || ReferenceEquals(sourceLock, targetLock))
				return new DualWriteLock(targetLock, null);  // same-tree or source has no TopNode

			// Cross-tree: acquire in TopNode ObjectGUID order to prevent AB/BA deadlock.
			// We intentionally use TopNode.ObjectGUID (a plain field) rather than FindRootNode()
			// (which reads _ParentNodes) so that this method is safe to call before holding any lock.
			// SupportsRecursion means re-entrant write-in-write from RegisterAll/UnRegisterAll is safe.
			Guid sourceId = (source.TopNode is BaseType sb) ? sb.ObjectGUID : Guid.Empty;
			Guid targetId = (target.TopNode is BaseType tb) ? tb.ObjectGUID : Guid.Empty;
			bool sourceFirst = sourceId.CompareTo(targetId) <= 0;
			return sourceFirst
				? new DualWriteLock(sourceLock, targetLock)
				: new DualWriteLock(targetLock, sourceLock);
		}

		/// <summary>
		/// Set to true to keep the ChildNodes List&lt;BaseType> sorted in the same order as the the SDC object tree
		/// </summary>
		#region IMoveRemove //not tested
		///<summary>
		///Un-register from _Nodes and _ChildNodes dictionaries.  Does not unregister subnodes or IETnodes.
		///</summary>
		/// 
		private static void MoveInDictionaries(this BaseType btSource, BaseType targetParent = null!)
		{
			// BUG FIX: For cross-tree moves with RefreshMode.UpdateNodeIdentity, ReflectRefreshSubtreeList
			// has already handled all dictionary registration/unregistration. Check if work is already done.
			// For same-tree moves, we MUST update _ParentNodes even though the node is already in _Nodes.
			var currentTopNode = btSource.TopNode;
			var targetTopNode = targetParent?.TopNode;

			if (currentTopNode != null && targetTopNode != null && 
				ReferenceEquals(currentTopNode, targetTopNode))
			{
				// Same tree OR cross-tree work already completed by ReflectRefreshSubtreeList
				// Always call UnRegisterAll + RegisterAll to update _ParentNodes dictionary
				// (The node remains in _Nodes, but its parent reference must be updated)
				btSource.UnRegisterAll(true);
				btSource.RegisterAll(targetParent, childNodesSort: true, addIETnodesRecursively: true);
			}
			else
			{
				// Should not happen, but handle gracefully
				// TopNode mismatch suggests incomplete cross-tree processing
				throw new InvalidOperationException(
					$"TopNode mismatch in MoveInDictionaries: node.TopNode and targetParent.TopNode are different. " +
					$"This suggests the cross-tree move was not properly handled by ReflectRefreshSubtreeList.");
			}

			// Resort IET nodes collection
			// We should resort the topNode.IETnodes collection for every add and move operation
		}

        /// <summary>
		/// This method removes <b><paramref name="btSource"/></b> and all its descendants from the SDC tree.<br/><br/>
        /// It also checks recursively for descendants and removes them from all dictionaries.<br/><br/>
        /// This method requires that <see cref="BaseType.ParentNode"/> is not null for the supplied node and all descendant nodes,<br/>
        /// which requires that the <see cref="_ITopNode._ChildNodes"/> dictionary has been correctly populated for all subtree nodes.<br/><br/>
        /// Removes Item(s)ChoiceType <see cref="Enum"/> entries when present.<br/>
        /// Will also remove a <see cref="ChildItemsType"/> node, when that node is the direct parent of <paramref name="btSource"/>, and 
        /// all the <see cref="ChildItemsType"/> node's Items members have been removed.
        /// </summary>
        /// <param name="btSource">The node to remove</param>
        /// <param name="cancelIfChildNodes">If true (default), abort node removal if child nodes (descendants) are present.
        /// If false, btSource and all descendants will be permanently removed.</param>
        /// <returns>True if node removal was successful; false if unsuccessful</returns>
        public static bool RemoveRecursive(this BaseType btSource, bool cancelIfChildNodes = true)
		{
			if (cancelIfChildNodes && btSource.TryGetChildNodes(out var roc) && roc is not null && roc.Count > 0)
				return false;
			if (btSource is null)
				throw new InvalidOperationException($"{nameof(btSource)} cannot be null.");
			if (btSource.TopNode is null)
				throw new InvalidOperationException($"{nameof(btSource.TopNode)} cannot be null.");
			var par = btSource.ParentNode;
			if (par is null)
				throw new InvalidOperationException($"{nameof(btSource.ParentNode)} cannot be null.");

            //Remove btSource subtree modes from TopNode dictionaries
            //__name and BaseName will be removed from _UniqueNames and _UniqueBaseNames for all nodes in the subtree.
            //__If btSource is an IET node, ID will be removed from _UniqueIDs, for all nodes in the btSource subtree
            bool result = RemoveNodesRecursively(btSource); 
			if (result is true)  //remove the btSource node
			{
				//Check for and Item(s)ChoiceType enum for btSource, and if present, remove it.
				//It's not necessary to check Item(s)ChoiceType enums on subnodes, since they will all be garbage collected anyway.  
				bool choiceTypeRemovalSucceeded = SdcUtil.TryRemoveItemChoiceEnumValue(btSource, out string errorMsg);

				if (!choiceTypeRemovalSucceeded)
					throw new InvalidOperationException($"Error removing value from the Item(s)ChoiceType enum servicing {nameof(btSource)}.");

                //Disconnect btSource from object tree
                result = RemoveNodeObject(btSource);
                //unless the btSource subtree is rejoined to the same or another tree, the entire disconnected subtree can now be garbage-collected


                //Remove btSource  from _ITopNode dictionaries.
                //If btSource is an IET node, btSource will also be removed from _IETnodes
                if (result) btSource.UnRegisterAll(false);
				return result;
			}
			else
				throw new InvalidOperationException($"Method {nameof(RemoveRecursive)} removed btSource from the object tree dictionaries.\r\n" +
					$"However, an error occured while trying to remove the node object (and descendants, if applicable).\r\n" +
					$"The object tree and its dictionares are now in an inconsistent state.\r\n" +
					$"Try running {nameof(SdcUtil.ReflectRefreshTree)} to refresh the object tree and its dictionaries");

			static bool RemoveNodesRecursively(BaseType nodeToRemove)
			{
				_ITopNode? topNode;
				if (nodeToRemove is ITopNode)
					topNode = (_ITopNode?)nodeToRemove;
				else topNode = (_ITopNode?)nodeToRemove.TopNode;

				if (topNode is null)
					throw new NullReferenceException($"{nameof(topNode)} cannot be null.");
				bool remResult = false;
				if (topNode._ChildNodes.TryGetValue(nodeToRemove.ObjectGUID, out List<BaseType>? kids) && kids is not null)
				{
					while (kids?.Count > 0)
					{
						var lastKid = kids.Last();
						RemoveNodesRecursively(lastKid); //recurse depth first 
						remResult = lastKid.RemoveNodeObject(); //Remove from object tree
						if (remResult) lastKid.UnRegisterAll(); //Remove from dictionaries
                        else { Debugger.Break(); break; } //exit early if failure to remove a node
                        topNode._UniqueBaseNames.Remove(nodeToRemove.BaseName);
                        topNode._UniqueNames.Remove(nodeToRemove.name);
						if(nodeToRemove is IdentifiedExtensionType iet) ((_IUniqueIDs)topNode)._UniqueIDs.Remove(iet.ID);
                    }
					return remResult;
				}
				return true;  //no kids were found or removed
			}
		}
        /// <summary>
        /// Remove <paramref name="nodeToRemove"/> from the SDC object tree. <br/><br/>
        /// Reflect the parent property in the object tree that represents nodeToRemove, 
        /// then use reflection to set the property to null.  This is non-recursive.<br/><br/>
        /// Child nodes are not checked and are not individually removed, so the caller must ensure that no child nodes are present.<br/>
        /// If <see cref="BaseType.ParentNode"/> is null for <paramref name="nodeToRemove"/>, an exception will be thrown.
        /// Call UnRegisterNode after this method to remove nodes from the node dictionaries.
        /// </summary>
        /// <param name="nodeToRemove"></param>
        /// <returns>true if the node is successfuly removed</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static bool RemoveNodeObject(this BaseType nodeToRemove)
		{
			var parentNode = nodeToRemove.ParentNode;
			if (parentNode is null)
				if (nodeToRemove is not ITopNode) throw new InvalidOperationException($"{nameof(parentNode)} cannot be null.");
				else return true; //ITopNode nodes do not have to have a parent node.

			var piProp = nodeToRemove.GetPropertyInfoMetaData(parentNode).PropertyInfo;
			if (piProp is null)
				throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: Cannot obtain parent PropertyInfo object for: {nameof(nodeToRemove)}.");

			var propObj = piProp.GetValue(parentNode);
			if (propObj is null)
				throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: Cannot reflect the parent object holding: {nameof(nodeToRemove)}.");

			if (propObj is IList lstPropObj)
			{
				if (lstPropObj.Contains(nodeToRemove))
				{
					Debug.Print($"Before Remove: nodeToRemove is null? {nodeToRemove is null}");
					Console.WriteLine($"Before Remove: nodeToRemove is null? {nodeToRemove is null}");
					lstPropObj.Remove(nodeToRemove);
					Debug.Print($"After Remove: nodeToRemove is null? {nodeToRemove is null}");
					Console.WriteLine($"After Remove: nodeToRemove is null? {nodeToRemove is null}");
					//nodeToRemove may still hold a reference to our prop object, until nodeToRemove goes out of scope.
					//if (nodeToRemove is not null) Debugger.Break();
					return true;
				}
				else
					throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: unable to locate {nameof(nodeToRemove)} in IList.");
			}
			else
			{
				piProp.SetValue(parentNode, null);
				//propObj will still hold a reference to our prop object, until propObj goes out of scope.
				return true;
			}
			throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: unable to remove node: {nameof(nodeToRemove)}.");
		}


        /// <summary>--------------------------------------------------------------<br/>
        /// Move an SDC node from one parent node to another. <br/>  
        /// The source/donor tree (home of node <paramref name="btSource"/>) may be different from the target tree 
		/// (home of node <paramref name="newParent"/>).<br/><br/>
        /// A check is performed for illegal moves using <see cref="SdcUtil.IsAttachNodeAllowed"/>. <br/>
        /// Illegal moves are not performed, causing this method to return false.<br/><br/>
        /// <see cref="BaseType.TopNode"/> is updated for all nodes in the moved subtree, as needed.<br/>
        /// All dictionaries are updated in the source <paramref name="btSource"/>'s and target <paramref name="newParent"/>'s <see cref="ITopNode"/> objects, as needed.<br/><br/>
        /// 
		/// If the top-level source subtree node (<paramref name="btSource"/>) derives from a different root node than the target (<paramref name="newParent"/>) node, <br/>
        /// all <see cref="BaseType.sGuid"/>, <see cref="BaseType.ObjectID"/>, <see cref="BaseType.name"/>, <see cref="IdentifiedExtensionType.ID"/> and dictionary entries will be updated automatically.<br/>  
        /// This behavior cannot be suppressed.<br/><br/>
		/// 
        /// The caller may force updating of <see cref="BaseType.sGuid"/>, <see cref="IdentifiedExtensionType.ID"/>, <see cref="BaseType.ObjectID"/>, 
		/// and <see cref="BaseType.name"/> metadata by setting <paramref name="refreshMode"/> to <see cref="RefreshMode.UpdateNodeIdentity"/>.
        /// </summary>
        /// <param name="btSource">The node to move.</param>
        /// <param name="newParent">The parent node destination to which <paramref name="btSource"/> should be attached</param>
        /// <param name="newListIndex">If <paramref name="newParent"/> supports IList, newListIndex holds the intended destination index in the list.
        /// All negative values will place <paramref name="btSource"/> at the first IList position (index 0).
        /// All values greater than the current last IList index will be added to the end of the list.
        /// The default value is -1, which will place <paramref name="btSource"/> at the start of the list</param>
        /// <param name="deleteEmptyParentNode">If <b><paramref name="deleteEmptyParentNode"/></b> is true (default is false) 
        /// then the method will check to see if the parent node of <b><paramref name="btSource"/></b> (the moved node) has no child nodes after
        /// <b><paramref name="btSource"/></b> is moved.  If <b><paramref name="deleteEmptyParentNode"/></b> is true 
        /// and also the parent node is childless, the parent node will be removed.<br/>
        /// This is useful for removing a childless (empty) <see cref="ChildItemsType"/> node, after moving its 
		/// last child node to a different parent node.</param>
        /// <param name="refreshMode">See <see href="RefreshMode"/> for enum values.<br/>
		/// Determine how metadata will be refreshed or modified in an SDC subtree that is moving.<br/>
		/// The subtree may be moving to another location in the same parent SDC tree or to a location in another SDC tree.		/// </param>
        /// <returns>True if the move was successful; false if the move was not allowed.</returns>
        /// <exception cref="NullReferenceException"/>
        ///<exception cref="InvalidOperationException"/>
		public static bool Move(this BaseType btSource, BaseType newParent, int newListIndex = -1
			, bool deleteEmptyParentNode = false
			, RefreshMode refreshMode = RefreshMode.NoChange
			)
		{		
			if (btSource is null)
				throw new NullReferenceException($"{nameof(btSource)} must not be null.");
			if (newParent is null)
				throw new NullReferenceException($"{nameof(newParent)} must not be null.");
			//if (btSource.ParentNode is null) throw new NullReferenceException("btSource.ParentNode must not be null.  A top-level (root) node cannot be moved");
			if (newParent.TopNode is null) throw new NullReferenceException($"{nameof(newParent.TopNode)} must not be null.");

			// TS-6 fix: acquire write locks on both the source and target trees (in TopNode ObjectGUID order)
			// BEFORE any dictionary read or IList mutation. FindRootNode(), IsAttachNodeAllowed(), and all
			// IList Remove/Insert operations all read or write _ParentNodes/_ChildNodes, which are plain
			// non-thread-safe Dictionary<> instances. Without this outer lock, concurrent Move() calls race
			// on those collections and throw ArgumentException/InvalidOperationException (corrupted dictionary
			// enumeration). DualWriteLock uses SupportsRecursion so the nested WriteLockScopes inside
			// RegisterAll/UnRegisterAll are safe.
			using var _moveLock = AcquireMoveLocks(btSource, newParent);

			//!

			//Do btSource and newParent share the same root node? (Are they from the same SDC tree?)
			var sourceRoot = btSource.FindRootNode();
			if (sourceRoot is null) throw new NullReferenceException($"The root node of {nameof(btSource)} could not be determined.");
			var newParentRoot = newParent.FindRootNode();
			if (newParentRoot is null) throw new NullReferenceException($"The root node of {nameof(newParent)} could not be determined.");
			bool sameRoot = (sourceRoot == newParentRoot);

			bool isAllowed = SdcUtil.IsAttachNodeAllowed(btSource, btSource.ElementName
				, newParent, out PropertyInfo? piTargetProperty, out object? targetPropertyObject
				, out PropertyInfo? piChoiceEnum, out object? choiceEnum, out string errorMsg);
			if (!isAllowed) return false;
			bool moveResult;


            if (refreshMode == RefreshMode.NoChange && !sameRoot)
            {
                //updateMetadata = true;  //Source and target subtrees do not share the same root node
                refreshMode = RefreshMode.UpdateNodeIdentity; //Since we are moving btSource subtree to a new tree, we must update ID, sGuid, ObjectGUID...
            }

            List<BaseType>? sourceNodeList;
            if (refreshMode == RefreshMode.CloneAndRepeatSubtree)
            {  //TODO: do we need to process donor node/branch: baseURI?, Link?, Codes, events?, rule targets (name)?	

                if (btSource is not IdentifiedExtensionType iet)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
                     $"the cloned subtree root ({nameof(btSource)}) must be of type {nameof(IdentifiedExtensionType)}");
                
				if (newParent is not ChildItemsType citTarget)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
                     $"the {nameof(newParent)} hosting the cloned subtree must be of type {nameof(ChildItemsType)}");

                if (newParent.TopNode is not FormDesignType fd)
					throw new InvalidOperationException(
					 $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
					 $"the TopNode of the cloned subtree must be of type {nameof(FormDesignType)}");

                // The donor (iet/btSource) may live in the same tree as newParent (a same-tree repeat,
                // e.g. Copy()), a different live instance tree, or a separately-loaded FDF template tree
                // (e.g. InjectSubtree()). Regardless of donor tree, the insertion point and repeat-suffix
                // numbering are always driven by the TARGET tree's own existing content and its own
                // FormDesignType.RepeatCounter -- never by the donor's position/siblings in its own
                // (possibly foreign) tree. This also allows injections anywhere in the target tree, not
                // just appended at the end.
                const string repeatSuffixPattern = @"^(.*)__\d+$";

                // The donor's "base" ID: its own ID with any pre-existing repeat/injection suffix
                // stripped. Donor IDs (especially from an FDF template) normally carry no suffix at all;
                // stripping defensively guards against a donor ID that already happens to carry one
                // (e.g. an FDF template that was incorrectly authored with a pre-suffixed ID) so that
                // repeat-detection still matches correctly instead of silently creating an orphan series.
                Match donorBaseIdMatch = Regex.Match(iet.ID, repeatSuffixPattern);
                string donorBaseID = donorBaseIdMatch.Success ? donorBaseIdMatch.Groups[1].Value : iet.ID;

                // Scan the target's existing children for the last node whose own base ID (similarly
                // stripped) matches the donor's base ID. The new repeat/injected copy is always inserted
                // immediately after the last such match (i.e., after the last existing repeat), or at the
                // caller-supplied newListIndex (or the end of the list) if this is the first occurrence of
                // this subtree at this target location.
                List<IdentifiedExtensionType> targetItems = citTarget.Items;
                int insertIndex = (newListIndex < 0 || newListIndex > targetItems.Count) ? targetItems.Count : newListIndex;
                for (int i = 0; i < targetItems.Count; i++)
                {
                    string childID = targetItems[i].ID;
                    Match childBaseIdMatch = Regex.Match(childID, repeatSuffixPattern);
                    string childBaseID = childBaseIdMatch.Success ? childBaseIdMatch.Groups[1].Value : childID;
                    if (childBaseID == donorBaseID)
                        insertIndex = i + 1;
                }
                newListIndex = insertIndex;

                //Clone the donor subtree. This works whether the donor is in the same tree as newParent or
                //a different (instance or template) tree, since Clone() always produces a fully independent
                //copy via XML serialize/deserialize round-trip (see BaseTypeExtensions.Clone<T>()) with no
                //reference back to its originating tree.
                IdentifiedExtensionType clone = (IdentifiedExtensionType)btSource.Clone(); //the clone retains the original sGuid, name, ID... at this point

				fd.RepeatCounter++;

				btSource = clone;  //btSource must be reset to the clone so that MoveSingleNode processes the new subtree correctly

                //This will overwrite the clone's identifiers with new values, and add its nodes to dictionaries.
                //Modify name and ID identifiers with repeat suffixes, based on FormDesignType.RepeatCounter.
                sourceNodeList =
				SdcUtil.ReflectRefreshSubtreeList(clone, false, true, false,
				(int)newParent.order + 1, 1, refreshMode, citTarget, SdcUtil.CreateCAPname);
            }
			else if(refreshMode == RefreshMode.UpdateNodeIdentity) 
						{   //Re-create dictionary and hashtable entries for: ID, BaseName, @name, sGuid/ObjectGUID, ObjectID etc for all btSource subtree nodes.
							//TODO: do we need to process donor node/branch: baseURI?, Link?, Codes, events?, rule targets (name)?

							// Lock already held: Move() acquired DualWriteLock at method entry (TS-6 fix).
							// ReflectRefreshSubtreeList and its nested RegisterAll/UnRegisterAll calls run safely
							// inside the lock because TreeRwLock uses SupportsRecursion.

							// For cross-tree moves, clear the source property reference before doing the move
							var sourceParent = btSource.ParentNode;
						if (sourceParent != null)
						{
							isAllowed = SdcUtil.IsAttachNodeAllowed(btSource, btSource.ElementName,
								sourceParent, out PropertyInfo? piSourceProperty, out object? sourcePropertyObject,
								out _, out _, out errorMsg);

							if (piSourceProperty != null && sourcePropertyObject is BaseType)
							{
								piSourceProperty.SetValue(sourceParent, null);
							}
							else if (piSourceProperty != null && sourcePropertyObject is IList sourceList)
							{
								sourceList.Remove(btSource);
							}
						}

						sourceNodeList =
							SdcUtil.ReflectRefreshSubtreeList(btSource, false, true, true,
							(int)newParent.order + 1, 1, refreshMode, newParent, SdcUtil.CreateCAPname);

						// IMPORTANT: After ReflectRefreshSubtreeList, the node is already registered in the target tree's
						// dictionaries with new GUIDs. Now we must attach it to the parent's property.
						if (piTargetProperty != null)
						{
							// A single-node slot may currently be empty (null) rather than occupied by an
							// existing BaseType instance (e.g., CopyPaste()-ing into a previously-unpopulated
							// single-node property). Detect that case via the declared property type, mirroring
							// the equivalent fix in MoveSingleNode(), so attachment isn't silently skipped.
							bool targetIsSingleNodeSlot = targetPropertyObject is BaseType
								|| (targetPropertyObject is null
									&& typeof(BaseType).IsAssignableFrom(piTargetProperty.PropertyType));

							if (targetIsSingleNodeSlot)
							{
								// Single-property attachment
								piTargetProperty.SetValue(newParent, btSource);
							}
							else if (targetPropertyObject is IList propList)
							{
								// List attachment
								if (newListIndex < 0 || newListIndex >= propList.Count)
									propList.Add(btSource);
								else
									propList.Insert(newListIndex, btSource);
							}
						}

						btSource.AssignOrder();
						return true;  // Skip MoveSingleNode() since we've already done the attachment
					}
			else if (refreshMode == RefreshMode.RestoreSubtreeFromOlderVersion)
            {
                if (btSource is not IdentifiedExtensionType ietSource)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.RestoreSubtreeFromOlderVersion)}, " +
                     $"the donor subtree ({nameof(btSource)}) must be of type {nameof(IdentifiedExtensionType)}");

                BaseType clone;
                if (!sameRoot)
                    //clone the subtree from the old version; we will not overwrite its name and ID identifiers
                    clone = btSource.Clone(); //the clone retains the original sGuid, name, ID... at this point
                else
                    throw new InvalidOperationException(
                         $"When using {nameof(RefreshMode.RestoreSubtreeFromOlderVersion)}, " +
                         $"the root of the subtree to clone ({nameof(btSource)}) must not be a member of the same SDC tree as {nameof(newParent)}");
                
				btSource = clone;  //btSource must be reset to the clone so that MoveSingleNode processes the new subtree correctly

				//This will assign new sGuid, ObjectGUID and ObjectID identifiers to all subtree nodes.  Preserve name and ID identifiers.
                sourceNodeList =
                    SdcUtil.ReflectRefreshSubtreeList(clone, false, true, true,
                    (int)newParent.order + 1, 1, refreshMode, newParent, SdcUtil.CreateCAPname);
            }
			else if  (refreshMode == RefreshMode.NoChange)
			{
				//we are doing a simple move within the same SDC tree.  No updating of ID, sGUID, ObjectGuid or name is needed.
				//However, ObjectID could be updated (maybe in the future) to reflect the sequence of node addition/moving.
			}


            moveResult = MoveSingleNode();
            return moveResult;

            bool MoveSingleNode()
			{
				// A single-node slot may currently be empty (null) rather than occupied by an existing
				// BaseType instance. Detect that case via the declared property type so we can attach
				// into a previously-unpopulated slot instead of falling through to the "invalid targetObj"
				// exception below (bug fix ported from Features/CompareTrees).
				bool targetIsSingleNodeSlot = targetPropertyObject is BaseType
					|| (targetPropertyObject is null && piTargetProperty is not null
						&& typeof(BaseType).IsAssignableFrom(piTargetProperty.PropertyType));

				if (targetIsSingleNodeSlot)
				{   //btSource can be attached directly to targetObj
					if (refreshMode == RefreshMode.NoChange)
					{
						var sourceParent = btSource.ParentNode;
						if (sourceParent is not null)
						{
							isAllowed = SdcUtil.IsAttachNodeAllowed(btSource, btSource.ElementName
								, sourceParent, out _, out object? sourceAttachmentObject
								, out _, out _, out errorMsg);

							if (sourceAttachmentObject is BaseType par)
								par.RemoveNodeObject();
							else if (sourceAttachmentObject is IList objList)
								objList.Remove(btSource);
						}
					}

					piTargetProperty!.SetValue(newParent, btSource);
					btSource.MoveInDictionaries(targetParent: newParent);
					btSource.AssignOrder(); //Requires that dictionaries are first populated for the entire btSource subtree

					return true;
				}
				//else if: btSource can be attached to a member of a List
					else if (targetPropertyObject is IList propList)
					//TODO: refactor block: bool AttachSourceNodetoList()
					{   //if ParentNode is null, and is not ITopNode, this may cause an exception or other errors below

						// Lock already held: Move() acquired DualWriteLock at method entry (TS-6 fix).
						var sourceParent = btSource.ParentNode;

						//par can be null if we are grafting from the btSource tree to another SDC object tree, and btSource is the root node of its tree
						if (sourceParent is not null) //Remove the reference from par to btSource
						{
							//Remove btSource from current parent object
							//This call is done only to obtain the sourceAttachmentObject, which hold the reference to btSource

							isAllowed = SdcUtil.IsAttachNodeAllowed(btSource, btSource.ElementName
							, sourceParent, out _, out object? sourceAttachmentObject
							, out _, out _, out errorMsg);

							//!Remove entries from the enum ItemChoiceType# (ItemElementName) object or ItemChoiceType#[] (ItemsElementName)
							if (piChoiceEnum is not null && choiceEnum is not null)
								SdcUtil.TryRemoveItemChoiceEnumValue(btSource, targetPropertyObject, piChoiceEnum, choiceEnum, out errorMsg);

							if (sourceAttachmentObject is BaseType par)
								par.RemoveNodeObject();
							else if (sourceAttachmentObject is IList objList)
							{
								//remove the btSource reference from this parent IList
								if (objList?.IndexOf(btSource) > -1) //this extra test may not be necessary
								{
									objList.Remove(btSource);

									if (deleteEmptyParentNode && objList.Count == 0)
									{
										sourceParent.RemoveNodeObject(); //requires sourceParent.ParentNodes entry to work; will throw if null

										//sourceParent was not previously removed in dictionaries, we only removed its last child node
										//Since it's now "childless," we can remove this orphan node from both the dictionaries and the SDC OM
										//isSourceParentChildless = true;

										UnRegisterAll(sourceParent, false); //this calls UnRegisterParent also
									}
								}
							}
							else
								throw new InvalidOperationException($"Could not reflect parent SDC property object ({nameof(targetPropertyObject)}) to remove node");
						}
						else { }//sourceParent is null
								//btSource.RegisterNodeAndParent();

						if (newListIndex < 0 || newListIndex >= propList.Count)
							propList.Add(btSource);
						else
							propList.Insert(newListIndex, btSource);

						//!Remove deleted nodes from _ITopNode dictionaries
						btSource.MoveInDictionaries(targetParent: newParent);
						btSource.AssignOrder(); //Requires that dictionaries are first populated for the entire btSource subtree

						return true;
					}//end of (targetPropertyObject is IList propList)
				else //not IList<BaseType> or BaseType
					throw new InvalidOperationException("Invalid targetObj: targetObj must be BaseType or IList");
			}
		}

        /// <summary>
        /// For FDF-R instance versions, copy an SDC data element subtree with user responses and insert it after the <br/>
        /// last existing repeat of the source block (or immediately after the source block itself, if no repeats <br/>
        /// exist yet), but without user responses, so that the user may add new responses in the repeated block.<br/>
        /// The name and ID properties receive a repeat suffix, and sGuid, ObjectGUID and ObjectID all receive new values.<br/>
        /// This is a same-tree convenience wrapper over <see cref="InjectSubtree(IdentifiedExtensionType, ChildItemsType, int)"/>;
        /// see that method for the cross-tree (instance-to-instance or template-to-instance) equivalent.
        /// </summary>
        /// <param name="btSource">The root node of the SDC subtree to copy</param>
        /// <returns></returns>
        public static bool Copy(this IdentifiedExtensionType btSource)
		{
			var par = btSource.ParentNode;
			if(par is null) throw new NullReferenceException($"The ParentNode of {nameof(btSource)} was null");
			if (par is not ChildItemsType citPar) throw new InvalidOperationException($"The ParentNode of {nameof(btSource)} must be of type {nameof(ChildItemsType)}");

			return btSource.InjectSubtree(citPar, -1);
        }

        /// <summary>
        /// Clones <paramref name="donorNode"/> and inserts the clone into <paramref name="targetParent"/>, always
        /// assigning a fresh repeat suffix (<c>"__N"</c>, driven by the target tree's own
        /// <see cref="FormDesignType.RepeatCounter"/>) to the clone's root ID and name, exactly as a repeating-node
        /// clone would receive. <paramref name="donorNode"/> is never modified or moved.<br/>
        /// <br/>
        /// Unlike <see cref="Copy(IdentifiedExtensionType)"/> (a same-tree-only repeat convenience method),
        /// <b><paramref name="donorNode"/> may be a member of the same tree as <paramref name="targetParent"/>,
        /// a different live instance OM tree, or a separately-loaded FDF template OM tree</b> (e.g. loaded via
        /// <see cref="LoadSourceFormDesign(BaseType)"/>). The insertion point and repeat-suffix numbering are
        /// always determined by <paramref name="targetParent"/>'s own existing children and its own tree's
        /// <see cref="FormDesignType.RepeatCounter"/> -- never by the donor's position in its own (possibly
        /// foreign) tree. If nodes matching the donor's base ID already exist among <paramref name="targetParent"/>'s
        /// children, the clone is inserted immediately after the last such match (mirroring repeat semantics);
        /// otherwise it is inserted at <paramref name="newListIndex"/> (or appended at the end if unspecified).
        /// </summary>
        /// <param name="donorNode">The node/subtree root to clone. May live in any SDC tree: the same tree as
        /// <paramref name="targetParent"/>, a different instance tree, or a separately-loaded FDF template tree.</param>
        /// <param name="targetParent">The <see cref="ChildItemsType"/> node in the recipient tree that will contain
        /// the injected clone.</param>
        /// <param name="newListIndex">Desired insertion index, used only when no existing repeat of this subtree
        /// is already present among <paramref name="targetParent"/>'s children; ignored (and computed automatically)
        /// otherwise.</param>
        /// <returns>True for success, false for failure.</returns>
        public static bool InjectSubtree(this IdentifiedExtensionType donorNode, ChildItemsType targetParent, int newListIndex = -1)
        {
            return donorNode.Move(targetParent, newListIndex, false, SdcUtil.RefreshMode.CloneAndRepeatSubtree);
        }

        /// <summary>
        /// The recommended entry point for repeating or injecting a subtree <b>from a live FDF-R (Form Design
        /// Form - Response) instance</b>. By default, clones from the node's <b>source FDF template</b> (see
        /// <see cref="LoadSourceFormDesign(BaseType)"/>/<see cref="FindNodeByTemplateID(FormDesignType, string)"/>)
        /// rather than from the live instance node itself, so the injected/repeated copy is automatically free of
        /// user-entered response data -- a template has none by definition -- while any <c>@readOnly</c>-locked
        /// default <c>@selected</c>/<c>@val</c> content is automatically preserved, because it is literally present
        /// in the template.<br/><br/>
        /// <b>Important, by-design limitation:</b> because the default-mode clone is sourced from the
        /// <i>template's</i> version of the subtree, any content that exists <i>only</i> in the live instance --
        /// most notably nested repeats/injections added under <paramref name="instanceDonorNode"/> after the
        /// instance was created from the template -- is <b>not</b> reproduced in the copy. This is a deliberate
			/// structural reset to the template's shape, not a bug: the alternative (diff-based cloning of a cleaned
			/// live-instance copy) is an explicitly out-of-scope fallback for now — see
			/// <c>docs/architecture/tree-operations.md</c>.<br/><br/>
        /// Set <paramref name="preserveInstanceData"/> to <see langword="true"/> to instead clone
        /// <paramref name="instanceDonorNode"/> directly (same as calling
        /// <see cref="InjectSubtree(IdentifiedExtensionType, ChildItemsType, int)"/>), keeping whatever response
        /// data (including nested repeats) currently exists on the live node, without any cross-check against the
        /// template -- this mode trusts the instance's current state as-is, for every node including
        /// <c>readOnly</c> ones.
        /// </summary>
        /// <param name="instanceDonorNode">The live FDF-R instance node/subtree root to repeat or inject.</param>
        /// <param name="targetParent">The <see cref="ChildItemsType"/> node in the recipient tree that will
        /// contain the injected/repeated clone.</param>
        /// <param name="newListIndex">Desired insertion index, used only when no existing repeat of this subtree
        /// is already present among <paramref name="targetParent"/>'s children.</param>
        /// <param name="preserveInstanceData">If <see langword="true"/>, clone the live instance node directly
        /// (preserving its current response data) instead of resolving and cloning from the source FDF template.
        /// Defaults to <see langword="false"/> (clone from source FDF template).</param>
        /// <returns>True for success, false for failure.</returns>
        /// <exception cref="InvalidOperationException">Thrown (via <see cref="LoadSourceFormDesign(BaseType)"/>)
        /// if the source FDF cannot be located/loaded, or if no node with a matching (suffix-stripped) <c>ID</c>
        /// exists in the loaded template -- e.g. because <paramref name="instanceDonorNode"/> was renamed, or
        /// added to the instance after the template was last saved. In that case, consider
        /// <paramref name="preserveInstanceData"/>: <see langword="true"/> instead.</exception>
        public static bool InjectSubtreeFromTemplate(
            this IdentifiedExtensionType instanceDonorNode,
            ChildItemsType targetParent,
            int newListIndex = -1,
            bool preserveInstanceData = false)
        {
            if (preserveInstanceData)
                return instanceDonorNode.InjectSubtree(targetParent, newListIndex);

            FormDesignType sourceFd;
            IdentifiedExtensionType? templateNode;
            try
            {
                sourceFd = instanceDonorNode.LoadSourceFormDesign();
                templateNode = sourceFd.FindNodeByTemplateID(instanceDonorNode.ID);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"{nameof(InjectSubtreeFromTemplate)} failed to resolve the source FDF template counterpart " +
                    $"of instance node ID='{instanceDonorNode.ID}'. Consider preserveInstanceData: true instead.", ex);
            }

            if (templateNode is null)
                throw new InvalidOperationException(
                    $"No node with ID '{instanceDonorNode.ID.StripRepeatSuffix()}' was found in the source FDF " +
                    $"template referenced by instance node ID='{instanceDonorNode.ID}'. The instance node may have " +
                    $"been renamed, or added after the template was last saved. Consider preserveInstanceData: " +
                    $"true instead.");

            return templateNode.InjectSubtree(targetParent, newListIndex);
        }
        /// <summary>
		/// Clone and then copy (graft) a subtree from one SDC template to another.  All identifiers will be replaced with new values,<br/>
		/// including sGuid, ObjectGUID, name, ID, and ObjectID)
		/// </summary>
		/// <param name="btSource">The donor/source node or subtree root to graft.</param>
		/// <param name="newParent">The target parent node that will subsume the grafted subtree.</param>
		/// <param name="newListIndex">If <paramref name="newParent"/> contains a List of subsumed SDC nodes, then <paramref name="newListIndex"/> <br/>
		/// contains the index of the grafted subtree in that List.</param>
		/// <returns>True for success, false for failure.</returns>
		public static bool Graft(this BaseType btSource, BaseType newParent, int newListIndex = -1)
        {
            return btSource.Move(newParent, newListIndex, false, SdcUtil.RefreshMode.UpdateNodeIdentity);
        }

        /// <summary>
        /// Clone a subtree and move the clone (not the original) to a new parent, leaving <paramref name="btSource"/> in place.<br/>
        /// All identifiers on the clone will be replaced with new values, including sGuid, ObjectGUID, name, ID, and ObjectID.<br/>
        /// Use this for copy/paste style operations where the original node must remain untouched.
        /// </summary>
        /// <param name="btSource">The source node or subtree root to clone; this node itself is not moved or modified.</param>
        /// <param name="newParent">The target parent node that will subsume the cloned subtree.</param>
        /// <param name="newListIndex">If <paramref name="newParent"/> contains a List of subsumed SDC nodes, then <paramref name="newListIndex"/> <br/>
        /// contains the index of the pasted (cloned) subtree in that List.</param>
        /// <returns>True for success, false for failure.</returns>
        public static bool CopyPaste(this BaseType btSource, BaseType newParent, int newListIndex = -1)
        {
            BaseType copy = btSource.Clone();
            return copy.Move(newParent, newListIndex, false, SdcUtil.RefreshMode.UpdateNodeIdentity);
        }
        
        /// <summary>
        /// Restore a subtree that existed in a previous version of the same SDC tree lineage.<br/>
        /// </summary>
        /// <param name="btSource">The old SDC subtree containing donor/source node or subtree root to restore.</param>
        /// <param name="newParent">The target parent node that will subsume the restored subtree.</param>
        /// <param name="newListIndex">If <paramref name="newParent"/> contains a List of subsumed SDC nodes, then <paramref name="newListIndex"/> <br/>
        /// contains the index of the restored subtree in that List.</param>
        /// <returns>True for success, false for failure.</returns>
        public static bool Restore(this BaseType btSource, BaseType newParent, int newListIndex = -1)
        {
            return btSource.Move(newParent, newListIndex, false, SdcUtil.RefreshMode.RestoreSubtreeFromOlderVersion);

        }

        /// <summary>
		/// In a user interface with only <see cref="IdentifiedExtensionType"/> nodes, a node (<b><paramref name="sourceNode"/></b>) may be moved <br/>
		/// (dragged and dropped) before, after or over (on top of) another node (<b><paramref name="targetNode"/></b>).  This method<br/>
		///  alters the SDC tree based on the type of move, defined in the <b><paramref name="position"/></b> enum parameter.
		/// </summary>
		/// <param name="sourceNode">The node that was moved (dragged and dropped).</param>
		/// <param name="targetNode">The node receiving the moved node, and firing the drop event.</param>
		/// <param name="position">The type of move, defined in the enum <b><see cref="DropPosition"/></b>, 
		/// and contained in the drop event from the <b><paramref name="targetNode"/></b>.</param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static bool DropMove(IdentifiedExtensionType sourceNode, IdentifiedExtensionType targetNode, DropPosition position)
        {
            try
            {
                //Console.WriteLine("-------------------------------------------");
                //Console.WriteLine($"{position.ToString().ToUpper()}: Source: {sourceNode.ElementName}: {sourceNode.As<DisplayedType>().title ?? "null"}, Target: {targetNode.ElementName}: {targetNode.As<DisplayedType>().title ?? "null"} ");

                //!Handle some common illegal Move cases
                if (sourceNode is null) return false;
                if (sourceNode.ParentNode is null) return false;
                if (targetNode is null) return false;

                if (sourceNode is ListItemType && (targetNode is SectionItemType)) //can't drop LI on, before or after S
                    return false;
                if (targetNode.IsDescendantOf(sourceNode))
                    return false;
                if (targetNode.GetType() == typeof(DisplayedType) && position == DropPosition.Over)
                    return false;

                //!Begin SETUP_______________________________________________________            

                IChildItemsParent? targetAsCIP = null; //This will not be null if the targetNode can subsume a ChildItems node
                BaseType? targetAttachementSite;  //The object where sourceNode should be attached.  It will be either a List.Items or ChildItems.Items object, which contain a "List<T> Items" attachment property

                //!Test if target is QS or QM
                QuestionItemType? qsqmTarget = null; //qsqmTarget will not be null if targetNode is QS or QM
                if (targetNode is QuestionItemType q &&
                        (q.GetQuestionSubtype() & QuestionEnum.QuestionSingleOrMultiple) > 0) //"Bitwise And" test for QS or QM
                    qsqmTarget = q;

                //!Test if source is LI or DI
                DisplayedType? listItemNodeSource = null; //source is LI or DI
                if (sourceNode is ListItemType || sourceNode.GetType() == typeof(DisplayedType))
                    listItemNodeSource = (DisplayedType)sourceNode;

                int targetIndex = 0; //drop in first position of target (List or ChildItems node)
                int sourceIndex = 0; //drop in first position of source (List or ChildItems node)
                PropertyInfoMetadata pimTarget;
                PropertyInfoMetadata pimSource;

                ChildItemsType? sourceChildItemsNode = null;
                ListType? sourceListNode = null;
                if (sourceNode.ParentNode is ChildItemsType cit)
                    sourceChildItemsNode = cit;
                if (sourceNode.ParentNode is ListType lt)
                    sourceListNode = lt;

				//!End SETUP_______________________________________________________      

				//Determine Drop Type:
				if (position == DropPosition.Over) //Add as the first child item, at the top of the list (itemIndex = 0)
				{
					targetAsCIP = targetNode as IChildItemsParent; //childItemsParent is null only if targetNode is DI

					if (targetNode is ListItemType li && sourceNode is ListItemType)
						return false;

					else if (qsqmTarget is not null && listItemNodeSource is not null) //If we drop LI/DI on QS/QM, add to Q LIST node
					{
						//Console.WriteLine("(qsqmTarget is not null &&  listItemNodeSource is not null");
						if (sourceNode is QuestionItemType || sourceNode is SectionItemType)
							Debugger.Break(); //We should never get here

						ListType? targetTest = qsqmTarget.ListField_Item?.List;
						if (targetTest is null)
						{
							Debugger.Break(); //We should never get here
							qsqmTarget.GetListField().GetList();
						}
						targetAttachementSite = qsqmTarget.ListField_Item?.List;
						if (targetAttachementSite is null) throw new NullReferenceException("targetAttachementSite (qsqmTarget.ListField_Item) cannot be null");
					}
					else if (targetAsCIP is not null) //i.e., targetNode != DI //includes all other IChildItemsParent drop targets,with any source type
					{
						//Console.WriteLine("Over: childItemsParent is not null");
						if (sourceNode is ListItemType) return false;

						targetAttachementSite = targetAsCIP.GetChildItemsNode(); //Create ChildItemsNode only when needed 

					}
					else //any other non-IChildItemsParent target nodes (only DI target nodes are left)
					{
						//Console.WriteLine("nop");

						if (targetNode.GetType() != typeof(DisplayedType))
							Debugger.Break(); //we should never get here
						return false;

					} // if we get here, user tried to drop on a DisplayedType node (not IChildItemsParent)
				}
				else if (position == DropPosition.After)
				{
					//Console.WriteLine("if (position == DropPosition.After)");
					pimTarget = targetNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains targetNode
					targetIndex = pimTarget.ItemIndex + 1;
					pimSource = sourceNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains targetNode
					sourceIndex = pimSource.ItemIndex;
					if (sourceNode.ParentNode == targetNode.ParentNode
						&& targetIndex > sourceIndex) targetIndex--;

					if (targetNode.ParentNode is ListType) //targetNode is LI or DI
					{
						if (listItemNodeSource is not null)
						{
							//Console.WriteLine("A0");
							targetAttachementSite = targetNode.ParentNode;  //(the List node)
						}
						else
						{
							//Console.WriteLine("A1");
							return false; //The source node is not LI or DI
						}
					}
					else if (sourceNode is ListItemType)
					{
						//Console.WriteLine("A2");
						return false; //Can't drop LI before or after a non-LI target
					}
					else
					{
						//Console.WriteLine("A3");
						targetAttachementSite = targetNode.ParentNode; //(ChildItemsNode)
					}
				}
				else if (position == DropPosition.Before)
				{
					//Console.WriteLine("if (position == DropPosition.Before)");
					pimTarget = targetNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains targetNode
					targetIndex = pimTarget.ItemIndex;
					pimSource = sourceNode.GetPropertyInfoMetaData(sourceNode.ParentNode); //retrieve the IEnumerable object that contains sourceNode
					sourceIndex = pimSource.ItemIndex;
					if (sourceNode.ParentNode == targetNode.ParentNode
						&& targetIndex > sourceIndex) targetIndex--;  //we removed the sourceNode before re-adding in the new position, so we decrement index by one

					if (targetNode is ListItemType)
					{
						if (listItemNodeSource is not null)
						{
							//Console.WriteLine("B0");
							targetAttachementSite = targetNode.ParentNode;  //(the List node)
						}
						else
						{
							//Console.WriteLine("B1");
							return false; //The source node is not LI or DI
						}
					}
					else if (sourceNode is ListItemType)
					{
						//Console.WriteLine("B2");
						return false; //Can't drop LI before or after a non-LI target
					}
					else
					{
						//Console.WriteLine("B3");
						targetAttachementSite = targetNode.ParentNode; //(ChildItemsNode)		
					}
				}
				else { return false; }//Console.WriteLine("No position");  }

                //Console.WriteLine($"targetAsCIP: {targetAsCIP?.As<DisplayedType>()?.title ?? "null"}");
                //Console.WriteLine($"targetNode: {targetNode?.As<DisplayedType>()?.title ?? "null"}");
                //Console.WriteLine($"qsqmTarget: {(qsqmTarget?.As<DisplayedType>()?.title ?? "null")}; NewTarget: {targetNode?.ElementName ?? "null"}: {targetNode?.As<DisplayedType>()?.title ?? "null"}");
                //Console.WriteLine($"targetAttachementSite: {targetAttachementSite?.ElementName ?? targetAttachementSite?.GetType().Name ?? "null"}");

                if (targetAttachementSite is null) throw new InvalidOperationException("Could not determine targetAttachementSite");

                bool result = false;
                bool deleteEmptyParentNode = false;
                if (targetAttachementSite is ChildItemsType) deleteEmptyParentNode = true; //delete the ChildItems node is it is "childless"

                //Console.WriteLine("Before Move");

                result = sourceNode.Move(targetAttachementSite, targetIndex, deleteEmptyParentNode);

                //Console.WriteLine("After Move");
                //Console.WriteLine("result: " + result);

                //var subTree = targetAttachementSite.GetSubtreeIETList();
				//if (subTree is not null && subTree.Count > 0)
				//{
				//	Console.WriteLine(subTree.Count);
				//	foreach (IdentifiedExtensionType n in subTree)
				//		Console.WriteLine(n.ElementPrefix + ": " + n.As<DisplayedType>().title ?? "(null)" + "; ");
				//}
				//else { Console.WriteLine("subTree.Count == 0"); }

                //Console.WriteLine("END:--------------------------------------");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex: {ex.Message}\r\n Inner Ex:{ex.InnerException?.Message}\r\nStack:\r\n{ex.StackTrace}");
                if (ex.InnerException?.Data is not null)
                {
                    foreach (DictionaryEntry kv in ex.InnerException.Data)
                        Console.WriteLine($"Key: {kv.Key}, Value: {kv.Value}");
                }
                return false;
            }
        }


        /// <summary>
		/// Describes where a source <see cref="IdentifiedExtensionType"/> node is dropped, relative to a target <see cref="IdentifiedExtensionType"/> node.
		/// </summary>
		public enum DropPosition
        {
            /// <summary>
            /// The source node is dropped before the target node.
            /// </summary>
            Before,
            /// <summary>
			/// The source node is dropped over the target node.
            /// </summary>
            Over,
            /// <summary>
            /// The source node is dropped after the target node.
            /// </summary>
            After
        }

        //This is a nodeWorkerFirst lambda designed for us in ReflectRefreshSubtreeList
        //Not used
        //TODO: We may need to modify this nodeWorker to detect and update duplicate sGuids and IDs (on IET nodes) in the target tree.
        private static Func<BaseType, bool> X_UpdateObjectID = (BaseType node) =>
			{
				if (node is ITopNode itn)
				{
					((_ITopNode)node)._MaxObjectID = 0;
					node.ObjectID = 0;
					return true;
				}
				if (node.TopNode is null)
					throw new NullReferenceException($"Node {node.name} has a null TopNode");
				node.ObjectID = ((_ITopNode)node.TopNode).AtomicNextObjectID();
				return true;
			};

        #endregion
        #region Dictionary Register-UnRegister
        //TODO: ChildNodes is maintained in sorted order by default;
        //Setting childNodesSort to false may speed loading when recreating the dictionaries in the correct order by reflection of the SDC object tree.
        //When adding nodes without reflection-ordering, always set childNodesSort to true.
        //childNodesSort uses reflection to determine the proper order that matches the object tree order
        //There is always a risk of getting out of sync with the nodes in the SDC object model classes.
        //If that happens, we can run SdcUtil.RefreshReflectTree to rebuild the dictionaries.



        /// <summary>
        /// Register <b><paramref name="node"/></b> (this) in all TopNode dictionaries.  
        /// If <b><paramref name="node"/></b> (this) is ITopNode, it is also registered in its own class's TopNode dictionaries
        /// </summary>
        /// <param name="node"/>
        /// <param name="parentNode">If adding nodes manually, i.e., using the BaseType constructor, a parent node should be provided.<br/>
		/// ITopNode nodes may have a null parent node.</param>
        /// <param name="childNodesSort">Setting to true (the default) ensures that the _ChildNodes dictionary<br/>
		/// is correctly sorted (by reflection) after this node is registered.</param>
        /// <param name="addIETnodesRecursively">Only used for <see cref="IdentifiedExtensionType"/> (IET) nodes.<br/> 
		/// False if a single <see cref="IdentifiedExtensionType"/> node should be added to the _IETnodes collection. <br/><br/>
        /// True if an <see cref="IdentifiedExtensionType"/> node (and its subnodes, if present) are being moved, <br/>
		/// and thus all of the IET subnodes need to be added as well.<br/> 
		/// The IET subnodes are inserted into the correct _IETnodes ordered location, as determined by reflection. <br/><br/>
		/// </param>
        internal static BaseType RegisterAll(this BaseType node, BaseType? parentNode = null, 
			bool childNodesSort = true, bool addIETnodesRecursively = false)
		{

            //if node was initially created with a null parent node, even if the parent node was assigned later, 
            //the node may still have a null ITopNode.  We can fix it here, so that we can register the node.

            if (node is ITopNode tn && parentNode is null)
				node.TopNode = tn;
			else if (node.TopNode is null && parentNode is not null)
				node.TopNode = parentNode.TopNode;

			if (node.TopNode is not null)
			{
				//if ObjectID was not set previously (usually because the node was created without a TopNode or parent node), we can set it here
				if (node.ObjectID == -1) node.ObjectID = ((_ITopNode)node.TopNode).AtomicNextObjectID();

				parentNode ??= node.ParentNode;
				_ITopNode? _topNode = (_ITopNode)node.TopNode;

				if (_topNode is null)
					throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

				// TS-2: WriteLockScope replaces lock(_SyncRoot). RegisterAll is a top-level write entry point;
				// internal helpers (RegisterIn_Nodes, RegisterIn_ParentNodes_ChildNodes, RegisterSubtreeIn_IETnodes)
				// run naked under this scope. SupportsRecursion allows nested RegisterAll calls from InitAfterTreeAdd.
				using var _writeLock = new WriteLockScope(_topNode.TreeRwLock);
				{
					//Add to _Nodes
					node.RegisterIn_Nodes();
					//Add to _ChildNodes
					if (parentNode is not null)
						node.RegisterIn_ParentNodes_ChildNodes(parentNode, childNodesSort);
					//Add to _IETnodes
					if (node is IdentifiedExtensionType iet)
						iet.RegisterSubtreeIn_IETnodes(addIETnodesRecursively);

					//AddUniqueIDsToHashTables(node, out string nonUniqueErrors);
				}

			}
			else {
				throw new InvalidOperationException($"TopNode could not be set for node {node.name}");
			} //There is no TopNode to hold our dictionaries, so we can't register the node

			return node;
		}
        private static void RegisterIn_Nodes(this BaseType node)
		{
			_ITopNode _topNode = ((_ITopNode)node.TopNode!);
			_topNode._Nodes.TryAdd(node.ObjectGUID, node);

			if (node is _ITopNode _meTopNode && _meTopNode != _topNode) //if we did not already do this... 
			{   //also register this ITopNode object in its own dictionaries.
				_meTopNode._Nodes.TryAdd(node.ObjectGUID, node);
			}
		}

		/// <summary>
		/// Register <b><paramref name="node"/></b> in _ParentNodes and _ChildNodes dictionaries.  
		/// If <b><paramref name="node"/></b> is ITopNode, it is also registered in <b><paramref name="node"/></b>'s own TopNode dictionaries
		/// </summary>
		/// <param name="node"></param>
		/// <param name="inParentNode"></param>
		/// <param name="childNodesSort"></param>
		/// <exception cref="NullReferenceException"></exception>
		private static void RegisterIn_ParentNodes_ChildNodes(this BaseType node, BaseType inParentNode, bool childNodesSort = true)
		{
			if (inParentNode != null)
			{
				var _topNode = (_ITopNode?)node.TopNode;
				if (_topNode is null)
					throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

				RegisterParentNode(node, inParentNode, _topNode, childNodesSort);

				if (node is _ITopNode _meTopNode && _meTopNode != _topNode) //if we did not already do this... 
				{   //also register this ITopNode object in its own dictionaries.
					RegisterParentNode(node, inParentNode, _meTopNode, childNodesSort);
				}
			}

			static void RegisterParentNode(BaseType btSource, BaseType inParentNode, _ITopNode tn, bool childNodesSort)
			{
				tn._ParentNodes.TryAdd(btSource.ObjectGUID, inParentNode);

					// GetOrAdd atomically returns the canonical list for inParentNode, creating it only if
					// the key is absent. This prevents the orphan-list race that would occur with a separate
					// TryGetValue + TryAdd pattern under concurrent node registration.
					List<BaseType> kids = tn._ChildNodes.GetOrAdd(inParentNode.ObjectGUID, _ => new List<BaseType>());
					//if btSource should live inside an inParentNode List<> object, but has not yet been added to that list,
					//then we can't sort the _ChildNodes dictionary List<> yet, because we can't reflect the order of the new node,
					//and also, we don't have its intended List position available here
					//These are attached to the SDC tree with a List<>: ListItem, DisplayedTypes, and all IChildItemsMember nodes.
					//Also need to check rules, Events, Actions, and Admin objects, as well as things in other ITopNode trees
					//We may need to add a new Interface: IHasListParent for all nodes that are attached to an Items object,
					//so that these nodes can be easily identified.
					// Sprint F (replaces Sprint E Fix A): smart, document-order insert.
						// _ChildNodes MUST stay in document (sibling) order: FindPrevIETInDictionaries walks
						// these lists assuming document order to find the IET predecessor during construction,
						// so a node inserted at an explicit position (e.g. AddQuestion/AddListItem at index 0)
						// lands in the correct _IETnodes slot. A plain append would corrupt that order and
						// break IET indexing (regression seen in QuestionItemTypeTests.AddListItemToPosition0).
						//
						// But the Sprint D full per-insert reflection binary search was O(log k) reflection
						// compares on every insert → O(k log k) for k siblings → .NET 10 multi-threaded WASM
						// watchdog timeouts when building many siblings under one parent (Cat2 Test5/Test6).
						//
						// SmartInsertInDocumentOrder splits the difference: the childNodesSort:false callers
						// (BaseType ctor, deserialization, ReflectRefreshTree bulk rebuild) almost always
						// register children IN document order, so the new node belongs at the END — an O(1)
						// append after a SINGLE reflection compare against the current last sibling. Only a
						// genuinely out-of-document-order construction takes the (rare, bounded) reflection
						// binary-search insert path. Net: ~1 reflection compare per node instead of O(log k).
						//
						// We deliberately do NOT use @order here (Sprint E Fix A's TreeOrderComparer):
						// ReflectRefreshTree.DoTree calls RegisterAll(childNodesSort:false) BEFORE it reassigns
						// @order, so on the refresh/deserialize path @order is stale/uniform (0), and
						// TreeOrderComparer is non-antisymmetric for equal @order — it could SCRAMBLE the list.
						//
						// childNodesSort:true (MoveNode / AddChild edits) keeps the robust full reflection sort.
						//
						// All list + sort-flag mutations stay inside lock(tn._ChildNodesMutationLock): the
						// _ChildNodes list is non-thread-safe and _TreeSort_NodeIds is a plain HashSet<int>;
						// SortElementKids takes the same per-tree lock. Monitor is reentrant; the comparers
						// take no locks, so there is no AB-BA risk.
						lock (tn._ChildNodesMutationLock)
						{
							if (!childNodesSort)
							{
								SmartInsertInDocumentOrder(kids, btSource, inParentNode);
							}
							else
							{
								// childNodesSort:true — reflect the object tree to place the node correctly.
								kids.Add(btSource);
								if (kids.Count > 1)
								{
									try { kids.Sort(treeSibComparer); } //sort by reflecting the object tree
									catch (InvalidOperationException)
									{
										// Node not yet fully wired into parent properties (e.g., during concurrent
										// construction). Skip sort here; AssignOrder/ReflectRefreshTree corrects later.
									}
								}
							}
						}
			}

			// Sprint F: insert btSource into kids preserving document (sibling) order, using at most one
			// reflection comparison for the common in-order-append case and a reflection binary search only
			// for the rare out-of-document-order insert. Must be called under lock(tn._ChildNodesMutationLock).
			static void SmartInsertInDocumentOrder(List<BaseType> kids, BaseType btSource, BaseType inParentNode)
			{
				if (kids.Count == 0)
				{
					kids.Add(btSource);
					return;
				}
				try
				{
					// Fast path: if btSource sorts at/after the current last sibling, append O(1).
					if (treeSibComparer.Compare(btSource, kids[kids.Count - 1]) >= 0)
					{
						kids.Add(btSource);
						return;
					}
					// Out-of-order insert: reflection binary search for the correct document-order slot.
					int lo = 0, hi = kids.Count;
					while (lo < hi)
					{
						int mid = (lo + hi) >> 1;
						if (treeSibComparer.Compare(btSource, kids[mid]) < 0) hi = mid;
						else lo = mid + 1;
					}
					kids.Insert(lo, btSource);
				}
				catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
				{
					// The reflection compare cannot determine sibling order — either btSource (or a sibling)
					// is not yet fully wired into parent properties during concurrent construction
					// (InvalidOperationException), or its ParentNode is transiently null during a
					// ReflectRefreshTree rebuild (ArgumentException). In BOTH cases the caller is walking
					// children in document order, so a plain append already yields the correct order; we
					// invalidate the sort flag so any later lazy SortElementKids consumer can re-verify it.
					kids.Add(btSource);
					SdcUtil.TreeSort_Invalidate(inParentNode);
				}
			}
		}
		private static void RegisterSubtreeIn_IETnodes(this IdentifiedExtensionType iet, bool addIETnodesRecursively = false)
		{
			_ITopNode? itn = iet.TopNode as _ITopNode;
			int insertPosition = -1;  //add to the beginning of the list, by default.

			if (itn is not null)
			{
				var inb = itn._IETnodes;
				// TS-7 / Sprint F: find the IET insertion position using _ChildNodes (kept in document
				// order by RegisterParentNode's SmartInsertInDocumentOrder on the childNodesSort:false
				// path, or treeSibComparer sort on the childNodesSort:true path) rather than
				// calling GetNodePreviousIET() → GetPrevSibElement() → SortElementKids (reflection sort).
				//
				// Walk backwards through the document-order siblings of iet (from _ChildNodes) to
				// find the nearest IET predecessor. If found, look it up in _IETnodes (linear scan).
				// If not found among siblings, walk up to parent and repeat.
				// This avoids any reflection sort during construction.
				var ietPrev = FindPrevIETInDictionaries(iet, itn);
				if (ietPrev is not null)
					insertPosition = inb.IndexOf(ietPrev);

				if (addIETnodesRecursively)
					foreach (IdentifiedExtensionType n in iet.GetSubtreeIETList())
						inb.Insert(++insertPosition, n);
				else //we are just adding a node here, not moving
					inb.Insert(++insertPosition, iet);
			}
			else 
				throw new InvalidOperationException($"{nameof(iet.TopNode)} was null.");

			if (iet is _ITopNode myTopNode && myTopNode._IETnodes.Count == 0) //we seem to be starting a new SDC tree here
				myTopNode._IETnodes.Insert(0, iet);
		}

		/// <summary>
		/// TS-7 helper: find the nearest IET predecessor of <paramref name="iet"/> using only
		/// the _ChildNodes and _ParentNodes dictionaries (no reflection sort).
		/// The sibling lists in _ChildNodes are kept in document order by RegisterParentNode's
		/// append (childNodesSort:false) or <see cref="treeSibComparer"/> sort (childNodesSort:true),
		/// so walking backwards from <paramref name="iet"/>'s position gives the correct in-tree predecessor.
		/// </summary>
		private static IdentifiedExtensionType? FindPrevIETInDictionaries(IdentifiedExtensionType iet, _ITopNode itn)
		{
			BaseType? cur = iet;
			while (cur is not null)
			{
				var par = itn._ParentNodes.TryGetValue(cur.ObjectGUID, out var p) ? p : null;
				if (par is null) return null; // reached the top without finding an IET predecessor

				if (itn._ChildNodes.TryGetValue(par.ObjectGUID, out var sibs) && sibs is not null)
				{
					int idx = sibs.IndexOf(cur);
					// walk backwards through sorted siblings looking for an IET node
					for (int i = idx - 1; i >= 0; i--)
					{
						if (sibs[i] is IdentifiedExtensionType prevIET)
						{
							// Return the last descendant IET of prevIET (if any), otherwise prevIET itself
							// — this mirrors what GetPrevElement → GetLastDescendant does.
							var lastDesc = GetLastDescendantIET(prevIET, itn);
							return lastDesc ?? prevIET;
						}
					}
				}
				// no IET sibling before cur — move up to parent
				cur = par;
				if (cur is IdentifiedExtensionType ietParent)
					return ietParent; // parent itself is the predecessor
			}
			return null;
		}

		/// <summary>
		/// TS-7 helper: return the last IET descendant of <paramref name="root"/> using
		/// _ChildNodes only (no reflection), or null if <paramref name="root"/> has no IET descendants.
		/// </summary>
		private static IdentifiedExtensionType? GetLastDescendantIET(BaseType root, _ITopNode itn)
		{
			// DFS: push kids in reverse order so the stack pops them in document order.
			// Walk to the deepest last node and return the last IET encountered.
			IdentifiedExtensionType? lastIET = null;
			var stack = new Stack<BaseType>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				var node = stack.Pop();
				if (node != root && node is IdentifiedExtensionType iet)
					lastIET = iet;
				if (itn._ChildNodes.TryGetValue(node.ObjectGUID, out var kids) && kids is not null)
					for (int i = kids.Count - 1; i >= 0; i--) // push in reverse → pop in forward document order
						stack.Push(kids[i]);
			}
			return lastIET;
		}

		/// <summary>
		/// Create a new @order value for the current node (this), and all its distal nodes in the same tree
		/// </summary>
		/// <param name="node"></param>
		/// <param name="orderGap"></param>
		/// <param name="reorderToEnd">If false, reorders nodes until the method encounters an order value larger than the previous node's order value<br/>
		/// If true, reorders all nodes until the end of the object tree.</param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		internal static decimal AssignOrder(this BaseType node, int orderGap = 1, bool reorderToEnd = false)

		{
			BaseType? curNode = node;

			var prevNode = curNode.GetNodePrevious();
			if (prevNode is null)
			{
				curNode.order = 0;
				return 0;
			}
			else
				curNode.order = prevNode.order + orderGap;

			//make sure that lower nodes have sequentially-numbered IDs
			int i = 0;
			do
			{
				prevNode = curNode;
				curNode = curNode.GetNodeNext();
				if (curNode is null) 
					return prevNode.order;
				else if(!reorderToEnd && prevNode.order < curNode.order)
					//all nodes are sequential now, so we can return now
					return prevNode.order;
				else
					curNode.order = prevNode.order + orderGap;

				i++; 
				if (i > 100000) 
					throw new IndexOutOfRangeException($"Too many cycles (100000) to walk down SDC object tree.  Run {nameof(SdcUtil.ReflectRefreshTree)} to fix.");
			} while (true);
			throw new InvalidOperationException("Could not assign @order to node.");
		}

		/// <summary>
		/// UnRegister <b><paramref name="node"/></b> (this) in _ParentNodes and _ChildNodes dictionaries.  
		/// If <b><paramref name="node"/></b> is ITopNode, it is also unregistered in <b><paramref name="node"/></b>'s own TopNode dictionaries
		/// </summary>
		/// <param name="node"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		private static void UnRegisterIn_ParentNodes_ChildNodes(this BaseType node)
		{
			var par = node.ParentNode;
			bool success;
			var _topNode = (_ITopNode?)node.TopNode;
			if (_topNode is null)
				throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

			UnRegisterParentNode(_topNode);

			if (node is _ITopNode _meTopNode && _meTopNode != _topNode) //if we did not already do this... 
			{   //also register this ITopNode object in its own dictionaries.
				UnRegisterParentNode(_meTopNode);
			}

			void UnRegisterParentNode(_ITopNode tn)
			{
				if (tn?._ParentNodes.ContainsKey(node.ObjectGUID) ?? false)
					success = tn._ParentNodes.TryRemove(node.ObjectGUID, out _);
				// if (!success) throw new Exception($"Could not remove object from ParentNodes dictionary: name: {this.name ?? "(none)"} , ObjectGUID: {this.ObjectGUID}");

				if (par is not null && (tn?._ChildNodes.ContainsKey(par.ObjectGUID) ?? false))
				{
					var childList = tn._ChildNodes[par.ObjectGUID];
					// Sprint E Fix B (thread-safety): serialise this list mutation against concurrent
					// SortElementKids (which calls kids.Sort under the same per-tree lock). Without this,
					// a PLINQ CompareTrees worker sorting childList while UnRegisterParentNode removes from
					// it raced and threw Arg_LongerThanDestArray (Cat4 Test3). If _mutLock is null the
					// TopNode is not yet established (no concurrency possible), so proceed without locking.
					object? _mutLock = tn?._ChildNodesMutationLock;
					if (_mutLock is not null)
					{
						lock (_mutLock)
						{
							success = childList.Remove(node); //Returns a List<BaseType> and removes "item" from that list
							if (!success)
								throw new InvalidOperationException($"Could not remove list node from {nameof(tn._ChildNodes)} dictionary: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");
							if (childList.Count == 0) success = tn._ChildNodes.TryRemove(par.ObjectGUID, out _); //remove the entire entry from _ChildNodes
							if (!success)
								throw new InvalidOperationException($"Could not remove parent entry from {nameof(tn._ChildNodes)} dictionary: name: {par.name ?? "(none)"}, ObjectGUID: {par.ObjectGUID}");
						}
					}
					else
					{
						success = childList.Remove(node); //Returns a List<BaseType> and removes "item" from that list
						if (!success)
							throw new InvalidOperationException($"Could not remove list node from {nameof(tn._ChildNodes)} dictionary: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");
						if (childList.Count == 0) success = tn._ChildNodes.TryRemove(par.ObjectGUID, out _); //remove the entire entry from _ChildNodes
						if (!success)
							throw new InvalidOperationException($"Could not remove parent entry from {nameof(tn._ChildNodes)} dictionary: name: {par.name ?? "(none)"}, ObjectGUID: {par.ObjectGUID}");
					}
				}
				else {} //no _ChildNodes entries are present for this node
			}
		} //!not tested

        /// <summary>
        /// Removes <paramref name="node"/> (this) from the <see href="_ITopNode._Nodes"/> dictionary.  Also calls <br/>
		/// <see href="UnRegisterIn_ParentNodes_ChildNodes(BaseType)"/> to remove <paramref name="node"/> 
		/// from <see href="_ITopNode._ChildNodes"/>, and <see href="_ITopNode._IETnodes"/>.  <br/>
        /// By default, the method does not remove nodes recursively from <see href="_ITopNode._Nodes"/> or <see href="_ITopNode._ChildNodes"/><br/>
		/// Optionally moves nodes recursively from <see href="_ITopNode._IETnodes"/>.  See <paramref name="removeIETnodesRecursively"/>.<br/>
        /// 
		/// </summary>
        /// <param name="node"></param>
        /// <param name="removeIETnodesRecursively">If <paramref name="node"/> is moving to another location, then we need <br/>
		/// to remove its entire subtree, and re-add it at the new target site.  Setting <paramref name="removeIETnodesRecursively"/><br/>
		/// to true will remove the entire IET subtree from <see href="_ITopNode._IETnodes"/>
		/// </param>
        /// <exception href="Exception"></exception>
        internal static void UnRegisterAll(this BaseType node, bool removeIETnodesRecursively = false)
		{
			var _topNode = (_ITopNode?)node.TopNode;
			var par = node.ParentNode;  //save par now,because we won;t be able to use node.ParentNode after it's removed from dictionaries.
			if (_topNode is null)
				return;  //with no TopNode, there is nothing to unregister
				//throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");
			UnRegister(_topNode, node);

			if (node is _ITopNode _myTopNode && _myTopNode != _topNode) //if we did not already do this... 
			{   //also register this ITopNode object in its own dictionaries.
				UnRegister(_myTopNode, node);
			}

			void UnRegister(_ITopNode tn, BaseType node)
			{
				// TS-2: WriteLockScope replaces lock(_SyncRoot). UnRegisterAll is a top-level write entry point;
				// internal helpers run naked under this scope. SupportsRecursion allows nesting from RemoveRecursive.
				using var _writeLock = new WriteLockScope(tn.TreeRwLock);
				{

					//Unregister _Nodes
					bool success = tn._Nodes.TryRemove(node.ObjectGUID, out _);
					if (!success)
						throw new Exception($"Could not remove object from {nameof(tn._Nodes)} dictionary: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");

					//Unregister _ChildNodes
					node.UnRegisterIn_ParentNodes_ChildNodes();

					//Unregister _IETnodes
					if (node is IdentifiedExtensionType iet)
					{
						var inb = tn._IETnodes;
						if(inb is null) 
							throw new InvalidOperationException($"{nameof(tn._IETnodes)} was null; Node name: {iet.name ?? "(none)"}, Short Guid: {node.sGuid}");
						if (!removeIETnodesRecursively)
						{
							success = inb.Remove(iet);
							if (!success)
								throw new Exception($"Could not remove object from {nameof(tn._IETnodes)} collection. Node name: {node.name ?? "(none)"}, Short Guid: {node.sGuid}");
						}
						else
						{
							foreach (IdentifiedExtensionType n in iet.GetSubtreeIETList())
							{
								success = inb.Remove(n);

								if (!success)
									throw new Exception($"Could not remove object from {nameof(tn._IETnodes)} collection. Node name: {node.name ?? "(none)"}, Short Guid: {node.sGuid}");
							}
						}
					}

					// Remove other TopNode registries
					tn._UniqueBaseNames.Remove(node.BaseName);
					tn._TreeSort_NodeIds.Remove(node.ObjectID);
					tn._UniqueNames.Remove(node.name);

					//Remove the various types of unique identifiers from _UniqueIDs
					//Only TopNode types that implement _IUniqueID contain the hashtable _UniqueIDs
					//_IUniqueIDs includes FormDesignType, DataElementType, RetrieveFormPackageType, PackageListType, XMLPackageType
					if (tn is _IUniqueIDs u)
					{
						if (node is IdentifiedExtensionType ietNode) //FormDesign, DemogFormDesign, DataElement, Section, DisplayedItem, Question, ListItem, Button, InjectForm
						{
							u._UniqueIDs.Remove(ietNode.ID);
							if (node is FormDesignType fd) //Includes DemogFormDesignType
								u._UniqueIDs.Remove(fd.instanceVersionURI);
							else if (node is DataElementType de)
								u._UniqueIDs.Remove(de.fullURI);
							else if (node is RetrieveFormPackageType rf)
							{
								u._UniqueIDs.Remove(rf.packageID);
								u._UniqueIDs.Remove(rf.instanceVersionURI);
								u._UniqueIDs.Remove(rf.fullURI);
							}
						}
						else if (node is PackageItemType pi)
						{
							u._UniqueIDs.Remove(pi.fullURI);
							u._UniqueIDs.Remove(pi.packageID);
							u._UniqueIDs.Remove(pi.formInstanceVersionURI);
						}
						else if (par is XMLPackageType)
						{
							if (node is MappingType m) //exists only under XMLPackageType parent
								u._UniqueIDs.Remove(m.templateID);
							else if (node is XMLPackageTypeHelperFile h) // exists only under XMLPackageType parent
								u._UniqueIDs.Remove(h.templateID);
							else if (node is LinkType lt)  // (named FormURL) uniqueness only important when under XMLPackageType parent, 
								u._UniqueIDs.Remove(lt.LinkURI.val);
						}
					}
				}
			}
		} //!not tested
        #endregion
		private static void X_AddUniqueIDsToHashTables(BaseType node, out string errors)
		{
			//TODO: Handle hashTable collisions (_UniqueIDs.Add returns false) and add to error log; do not throw exceptions here.

			_ITopNode tn = node.TopNode as _ITopNode ?? throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null");
			BaseType? par = node.ParentNode;
			errors = "";
            //List<string> errorList = new();
            StringBuilder sb = new();
            //if (!string.IsNullOrWhiteSpace(node.name))
            //{
            //    if (!tn._UniqueNames.Add(node.name))
            //        AddError(nameof(node.name), node.name);
            //}
            //if (!string.IsNullOrWhiteSpace(node.BaseName))
            //{
            //	if (!tn._UniqueBaseNames.Add(node.BaseName))
            //		AddError(nameof(node.BaseName), node.BaseName);
            //}


            //Only SDC types that implement _IUniqueID contain the hashtable _UniqueIDs
            //_IUniqueIDs includes FormDesignType, DataElementType, RetrieveFormPackageType, PackageListType, XMLPackageType (XMLPackageType does implement ITopNode)
            if (tn is _IUniqueIDs u)
            {
                //if (node is IdentifiedExtensionType ietNode) //FormDesign, DemogFormDesign, DataElement, Section, DisplayedItem, Question, ListItem, Button, InjectForm
                //{
                //    if (!string.IsNullOrWhiteSpace(ietNode.ID))
                //        if (!u._UniqueIDs.Add(ietNode.ID)) AddError(nameof(ietNode.ID), ietNode.ID);
                //}
                if (node is FormDesignType fd) //Includes DemogFormDesignType
                {
                    if (!string.IsNullOrWhiteSpace(fd.instanceVersionURI))
                        if (!u._UniqueIDs.Add(fd.instanceVersionURI)) AddError(nameof(fd.instanceVersionURI), fd.instanceVersionURI);
                }
                else if (node is DataElementType de)
                {
                    if (!string.IsNullOrWhiteSpace(de.fullURI))
                        if (!u._UniqueIDs.Add(de.fullURI)) AddError(nameof(de.fullURI), de.fullURI);
                }
                else if (node is RetrieveFormPackageType rf)
                {
                    if (!string.IsNullOrWhiteSpace(rf.packageID))
                        if (!u._UniqueIDs.Add(rf.packageID)) AddError(nameof(rf.packageID), rf.packageID);
                    if (!string.IsNullOrWhiteSpace(rf.instanceVersionURI))
                        if (!u._UniqueIDs.Add(rf.instanceVersionURI)) AddError(nameof(rf.instanceVersionURI), rf.instanceVersionURI);
                    if (!string.IsNullOrWhiteSpace(rf.fullURI))
                        if (!u._UniqueIDs.Add(rf.fullURI)) AddError(nameof(rf.fullURI), rf.fullURI);
                }

                else if (node is PackageItemType pi)
                {
                    if (!string.IsNullOrWhiteSpace(pi.fullURI))
                        if (!u._UniqueIDs.Add(pi.fullURI)) AddError(nameof(pi.fullURI), pi.fullURI);
                    if (!string.IsNullOrWhiteSpace(pi.packageID))
                        if (!u._UniqueIDs.Add(pi.packageID)) AddError(nameof(pi.packageID), pi.packageID);
                    if (!string.IsNullOrWhiteSpace(pi.formInstanceVersionURI))
                        if (!u._UniqueIDs.Add(pi.formInstanceVersionURI)) AddError(nameof(pi.formInstanceVersionURI), pi.formInstanceVersionURI);
                }
                else if (par is not null && par is XMLPackageType)
                {
                    if (node is MappingType m) //exists only under XMLPackageType parent
                    {
                        if (!string.IsNullOrWhiteSpace(m.templateID))
                            if (!u._UniqueIDs.Add(m.templateID)) AddError(nameof(m.templateID), m.templateID);
                    }
                    else if (node is XMLPackageTypeHelperFile h) // exists only under XMLPackageType parent
                    {
                        if (!string.IsNullOrWhiteSpace(h.templateID))
                            if (!u._UniqueIDs.Add(h.templateID)) AddError(nameof(h.templateID), h.templateID);
                    }
                    else if (node is LinkType lt)  // (named FormURL) uniqueness only important when under XMLPackageType parent, 
                    {
                        if (!string.IsNullOrWhiteSpace(lt.LinkURI.val))
                            if (!u._UniqueIDs.Add(lt.LinkURI.val)) AddError(nameof(lt.LinkURI.val), lt.LinkURI.val);
                    }
                }
            }
            errors = sb.ToString();
			if(!string.IsNullOrWhiteSpace(errors))
				Console.WriteLine(errors);

            void AddError(string property, string value)
                => sb.Append($"Duplicate {property}: {value}, sGuid{node.sGuid}, TopNode: {((BaseType)tn).name}\r\n");
        }



    }
}
