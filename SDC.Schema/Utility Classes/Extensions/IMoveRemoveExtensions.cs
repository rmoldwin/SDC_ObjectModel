using CSharpVitamins;
using System.CodeDom;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
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
		/// Set to true to keep the ChildNodes List&lt;BaseType> sorted in the same order as the the SDC object tree
		/// </summary>
		#region IMoveRemove //not tested
		///<summary>
		///Un-register from _Nodes and _ChildNodes dictionaries.  Does not unregister subnodes or IETnodes.
		///</summary>
		/// 
		private static void MoveInDictionaries(this BaseType btSource, BaseType targetParent = null!)
		{
			//Remove from ParentNodes and ChildNodes as needed
			//BUG: We need to also remove and reregister entries from IETnodes,
			//BUG: and also from "meTopNode" dictionaries when the nodes are ITopNode and have entries in their own ITopNode dicts.
			//btSource.UnRegisterIn_ParentNodes_ChildNodes(); //Does not remove IETnodes
			btSource.UnRegisterAll();

			//Re-register item node under new parent
			//btSource.RegisterIn_ParentNodes_ChildNodes(targetParent, childNodesSort: true); //Does not touch IETnodes
			btSource.RegisterAll(targetParent, childNodesSort: true, true);


			//We really should resort the topNode.IET nodes collection for every add and move operation.  It's probably best to use the TreeComparer
			//for this, but it would be better to only resort part of the collection, if possible.
			//We could also use the ChildNodes dictionary to create a new faster TreeComparer, as long as we keep ChildNodes sorted
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

			//remove btSource subtree from TopNode dictionaries
			bool result = RemoveNodesRecursively(btSource); 
			if (result is true)  //remove the btSource node
			{
				//Check for and Item(s)ChoiceType enum for btSource, and if present, remove it.
				//It's not necessary to check Item(s)ChoiceType enums on subnodes, since they will all be garbage collected anyway.  
				bool choiceTypeRemovalSucceeded = SdcUtil.TryRemoveItemChoiceEnumValue(btSource, out string errorMsg);

				if (!choiceTypeRemovalSucceeded)
					throw new InvalidOperationException($"Error removing value from the Item(s)ChoiceType enum servicing {nameof(btSource)}.");

				//Remove btSource from object tree,
				//unless the btSource subtree is rejoined to the same or another tree, the entire disconnected subtree can now be garbage-collected
				result = RemoveNodeObject(btSource);

				//Remove btSource from _ITopNode dictionaries.
				//Also, if btSource is an IET node, remove entire btSource subtree from _IETnodes
				if (result) btSource.UnRegisterAll();
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
        ///<exception cref="Exception"/>
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
			
			//!------------------------------------------------------------

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

            if (refreshMode == RefreshMode.NoChange && !sameRoot)
            {
                //updateMetadata = true;  //Source and target subtrees do not share the same root node
                refreshMode = RefreshMode.UpdateNodeIdentity; //Since we are moving btSource subtree to a new tree, we must update ID, sGuid, ObjectGUID...
            }

            List<BaseType>? sourceNodeList;
            if (refreshMode == RefreshMode.CloneAndRepeatSubtree)
            {  //TODO: do we need to process donor node/branch: baseURI?, Link?, Codes, events?, rule targets (name)?	

				newListIndex = -1;  //Repeats are always added at the end of the attachment site (ChildItemsType) list, even if set by the caller to specific value

                if (btSource is not IdentifiedExtensionType)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
                     $"the cloned subtree {nameof(btSource)} must be of type {nameof(IdentifiedExtensionType)}");
                
				if (newParent is not ChildItemsType citTarget)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
                     $"the {nameof(newParent)} hosting the cloned subtree must be of type {nameof(ChildItemsType)}");

                IdentifiedExtensionType clone;
                if (sameRoot)
					clone = (IdentifiedExtensionType)btSource.Clone(); //the clone retains the original sGuid, name, ID... at this point
				else
                    throw new InvalidOperationException(
                         $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
                         $"the root of the subtree to clone {nameof(btSource)} must be a member of the same SDC tree as {nameof(newParent)}");

                if (newParent.TopNode is not FormDesignType fd)
					throw new InvalidOperationException(
					 $"When using {nameof(RefreshMode.CloneAndRepeatSubtree)}, " +
					 $"the {nameof(BaseType.TopNode)} of the cloned subtree must be of type {nameof(FormDesignType)}");
				else 
					fd.RepeatCounter++;

				sourceNodeList =
				SdcUtil.ReflectRefreshSubtreeList(clone, false, true, false,
				(int)newParent.order + 1, 1, refreshMode, citTarget, SdcUtil.CreateCAPname);
            }
			else if(refreshMode == RefreshMode.UpdateNodeIdentity) 
			{   //Re-create dictionaries, ID, BaseName, @name, sGuid/ObjectGUID, ObjectID etc for all btSource nodes.
				//TODO: do we need to process donor node/branch: baseURI?, Link?, Codes, events?, rule targets (name)?		
				sourceNodeList =
					SdcUtil.ReflectRefreshSubtreeList(btSource, false, true, true,
					(int)newParent.order + 1, 1, refreshMode, newParent, SdcUtil.CreateCAPname);
			}
			else if (refreshMode == RefreshMode.RestoreSubtreeFromOlderVersion)
            {
                if (btSource is not IdentifiedExtensionType ietSource)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.RestoreSubtreeFromOlderVersion)}, " +
                     $"the donor subtree {nameof(btSource)} must be of type {nameof(IdentifiedExtensionType)}");

                if (newParent is not ChildItemsType citTarget)
                    throw new InvalidOperationException(
                     $"When using {nameof(RefreshMode.RestoreSubtreeFromOlderVersion)}, " +
                     $"the {nameof(newParent)} node that is hosting the donor subtree ({nameof(btSource)}) must be of type {nameof(ChildItemsType)}");

                sourceNodeList =
                    SdcUtil.ReflectRefreshSubtreeList(ietSource, false, true, true,
                    (int)newParent.order + 1, 1, refreshMode, citTarget, SdcUtil.CreateCAPname);
            }
			else if  (refreshMode == RefreshMode.NoChange)
			{
				//we are doing a simple move withhin the same SDC tree.  No updating of ID, sGUID, ObjectGuid or name is needed.
				//However, ObjectID could be updated (optionally)  to reflect the sequence of node addition/moving.
			}

            if (targetPropertyObject is BaseType) 
			{	//btSource can be attached directly to targetObj
				targetPropertyObject = btSource;
				btSource.MoveInDictionaries(targetParent: newParent);
				btSource.AssignOrder(); //Required that dictionaries are first populated

				return true;
			}
			else if (targetPropertyObject is IList propList)
			//btSource can be attached to a member of a List
			//TODO: refactor block: bool AttachSourceNodetoList()
			{   //if ParentNode is null, and is not ITopNode, this may cause an exception or other errors below 
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
					if(piChoiceEnum is not null && choiceEnum is not null) 
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

								UnRegisterAll(sourceParent); //this calls UnRegisterParent also
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
				btSource.AssignOrder(); //Requires that dictionaries are first populated

				return true;
			}//end of (targetPropertyObject is IList propList)
			else //not IList<BaseType> or BaseType
				throw new InvalidOperationException("Invalid targetObj: targetObj must be BaseType or IList");
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
				node.ObjectID = ((_ITopNode)node.TopNode)._MaxObjectID++;
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
        /// Register <b><paramref name="node"/></b> in all TopNode dictionaries.  
        /// If <b><paramref name="node"/></b> is ITopNode, it is also registered in its own class's TopNode dictionaries
        /// </summary>
        /// <param name="node"/>
        /// <param name="parentNode">If adding nodes manually, i.e., using the BaseType constructor, a parent node should be provided.<br/>
		/// ITopNode nodes may have a null parent node.</param>
        /// <param name="childNodesSort">Setting to true (the default) ensures that the _ChildNodes dictionary<br/>
		/// is correctly sorted (by reflection) after this node is registered.</param>
        /// <param name="isMoving">Only used for <see cref="IdentifiedExtensionType"/> (IET) nodes.<br/> 
		/// False if a single <see cref="IdentifiedExtensionType"/> node should be added to the _IETnodes collection. <br/><br/>
        /// True if an <see cref="IdentifiedExtensionType"/> node (and its subnodes, if present) are being moved, <br/>
		/// and thus all of the IET subnodes need to be added as well.<br/> 
		/// The IET subnodes are inserted into the correct _IETnodes ordered location, as determined by reflection. <br/><br/>
		/// </param>
        internal static BaseType RegisterAll(this BaseType node, BaseType? parentNode = null, 
			bool childNodesSort = true, bool isMoving = false)
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
				if (node.ObjectID == -1) node.ObjectID = ((_ITopNode)node.TopNode)._MaxObjectID++;

				parentNode ??= node.ParentNode;
				_ITopNode? _topNode = (_ITopNode)node.TopNode;

				if (_topNode is null)
					throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

				//Add to _Nodes
				node.RegisterIn_Nodes();
				//Add to _ChildNodes
				if (parentNode is not null) 
					node.RegisterIn_ParentNodes_ChildNodes(parentNode, childNodesSort);
				//Add to _IETnodes
				if (node is IdentifiedExtensionType iet)
					iet.RegisterSubtreeIn_IETnodes(isMoving);

				AddUniqueIDsToHashTables(node, out string nonUniqueErrors);
			}
			else {
				throw new InvalidOperationException($"TopNode could not be set for node {node.name}");
			} //There is no TopNode to hold our dictionaries, so we can't register the node

			return node;
		}
		internal static void AddUniqueIDsToHashTables(BaseType node, out string errors)
		{
			//TODO: Handle hashTable collisions (_UniqueIDs.Add returns false) and add to error log; do not throw exceptions here.

			_ITopNode tn = node.TopNode as _ITopNode ?? throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null");
			BaseType? par = node.ParentNode;
			errors = "";
            //List<string> errorList = new();
            StringBuilder sb = new();
            if (!string.IsNullOrWhiteSpace(node.name))
            {
                if (!tn._UniqueNames.Add(node.name))
                    AddError(nameof(node.name), node.name);
            }
			if (!string.IsNullOrWhiteSpace(node.BaseName))
			{
				if (!tn._UniqueBaseNames.Add(node.BaseName))
					AddError(nameof(node.BaseName), node.BaseName);
			}

            //Remove the various types of unique identifiers from _UniqueIDs
            //Only TopNode types that implement _IUniqueID contain the hashtable _UniqueIDs
            //_IUniqueIDs includes FormDesignType, DataElementType, RetrieveFormPackageType, PackageListType, XMLPackageType
            if (tn is _IUniqueIDs u)
            {
                if (node is IdentifiedExtensionType ietNode) //FormDesign, DemogFormDesign, DataElement, Section, DisplayedItem, Question, ListItem, Button, InjectForm
                {
                    if (!string.IsNullOrWhiteSpace(ietNode.ID))
                        if (!u._UniqueIDs.Add(ietNode.ID)) AddError(nameof(ietNode.ID), ietNode.ID);
                }
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
			if(string.IsNullOrWhiteSpace(errors))
				Console.WriteLine(errors);

            void AddError(string property, string value)
                => sb.Append($"Duplicate {property}: {value}, sGuid{node.sGuid}, TopNode: {((BaseType)tn).name}\r\n");
        }
        private static void RegisterIn_Nodes(this BaseType node)
		{
			_ITopNode _topNode = ((_ITopNode)node.TopNode!);
			_topNode._Nodes.Add(node.ObjectGUID, node);

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
				tn._ParentNodes.Add(btSource.ObjectGUID, inParentNode);

				List<BaseType>? kids;
				tn._ChildNodes.TryGetValue(inParentNode.ObjectGUID, out kids);
				if (kids is null)
				{
					kids = new List<BaseType>();
					tn._ChildNodes.Add(inParentNode.ObjectGUID, kids);
					kids.Add(btSource); //no need to sort with only one item in the list
				}
				else
				{
					//if btSource should live inside an inParentNode List<> object, but has not yet been added to that list,
					//then we can't sort the _ChildNodes dictionary List<> yet, because we can't reflect the order of the new node,
					//and also, we don't have its intended List position available here
					//These are attached to the SDC tree with a List<>: ListItem, DisplayedTypes, and all IChildItemsMember nodes.
					//Also need to check rules, Events, Actions, and Admin objects, as well as things in other ITopNode trees
					//We may need to add a new Interface: IHasListParent for all nodes that are attached to an Items object,
					//so that these nodes can be easily identified.
					kids.Add(btSource);					
					if (kids.Count > 1 && childNodesSort)
					{
							kids.Sort(treeSibComparer); //sort by reflecting the object tree							
					}
				}
			}
		}
		private static void RegisterSubtreeIn_IETnodes(this IdentifiedExtensionType iet, bool isMoving = false)
		{
			_ITopNode? itn = iet.TopNode as _ITopNode;
			var ietPrev = iet.GetNodePreviousIET(); //find the position to insert our new/moved node	
			
			int insertPosition = -1;  //add to the beginning of the list, by default.

			if (itn is not null)
			{
				if (ietPrev is not null)
					insertPosition = itn._IETnodes.IndexOf(ietPrev);  //TODO: this collection scan may be inefficient; we may want to switch to KeyedCollection<Tkey, Titem> (C# Nutshell page 353) or ConditionalWeakTable instead (using sGuid or the object ref as Key).

				if (isMoving)
					foreach (IdentifiedExtensionType n in iet.GetSubtreeIETList())
						itn._IETnodes.Insert(++insertPosition, n);
				else //we are just adding a node here, not moving
					itn._IETnodes.Insert(++insertPosition, iet);
			}

			else 
				throw new InvalidOperationException($"{nameof(iet.TopNode)} was null.");

			if (iet is _ITopNode myTopNode && myTopNode._IETnodes.Count == 0) //we seem to be starting a new SDC tree here
				myTopNode._IETnodes.Insert(0, iet);

		}

		/// <summary>
		/// Create a new @order value for the current node, and all its distal nodes in the same tree
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
		/// UnRegister <b><paramref name="node"/></b> in _ParentNodes and _ChildNodes dictionaries.  
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
					success = tn._ParentNodes.Remove(node.ObjectGUID);
				// if (!success) throw new Exception($"Could not remove object from ParentNodes dictionary: name: {this.name ?? "(none)"} , ObjectGUID: {this.ObjectGUID}");

				if (par is not null && (tn?._ChildNodes.ContainsKey(par.ObjectGUID) ?? false))
				{
					var childList = tn._ChildNodes[par.ObjectGUID];
					success = childList.Remove(node); //Returns a List<BaseType> and removes "item" from that list
					if (!success)
						throw new InvalidOperationException($"Could not remove list node from {nameof(tn._ChildNodes)} dictionary: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");
					if (childList.Count == 0) success = tn._ChildNodes.Remove(par.ObjectGUID); //remove the entire entry from _ChildNodes
					if (!success)
						throw new InvalidOperationException($"Could not remove parent entry from {nameof(tn._ChildNodes)} dictionary: name: {par.name ?? "(none)"}, ObjectGUID: {par.ObjectGUID}");
				}
				else {} //no _ChildNodes entries are present for this node
			}
		} //!not tested

		/// <summary>
		/// Removes <b><paramref name="node"/></b> from the <see cref="_ITopNode._Nodes"/> dictionary.<br/>
		/// Also calls <see cref="UnRegisterIn_ParentNodes_ChildNodes(BaseType)"/> to remove <b><paramref name="node"/></b> from <see cref="_ITopNode._ChildNodes"/>.  <br/>
		/// Does not remove nodes recursively from <see cref="_ITopNode._Nodes"/> or <see cref="_ITopNode._ChildNodes"/><br/>
		/// If the current <b><paramref name="node"/></b> is an IET node, then <b><paramref name="node"/></b> and all of its IET subnodes
		/// will be removed from <see cref="_ITopNode._IETnodes"/>.
		/// </summary>
		/// <param name="node"></param>
		/// <exception cref="Exception"></exception>
		internal static void UnRegisterAll(this BaseType node)
		{
			var _topNode = (_ITopNode?)node.TopNode;
			var par = node.ParentNode;  //save par now,because we won;t be able to use node.ParentNode after it's removed from dictionaries.
			if (_topNode is null)
				throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");
			UnRegister(_topNode, node);

			if (node is _ITopNode _myTopNode && _myTopNode != _topNode) //if we did not already do this... 
			{   //also register this ITopNode object in its own dictionaries.
				UnRegister(_myTopNode, node);
			}

			void UnRegister(_ITopNode tn, BaseType node)
			{
				//Unregister _Nodes
				bool success = tn._Nodes.Remove(node.ObjectGUID);
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

					foreach (IdentifiedExtensionType n in iet.GetSubtreeIETList())
					{
						success = inb.Remove(n);

						if (!success)
						throw new Exception($"Could not remove object from {nameof(tn._IETnodes)} collection. Node name: {node.name ?? "(none)"}, Short Guid: {node.sGuid}");
					}
				}

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
        } //!not tested
        #endregion



    }
}
