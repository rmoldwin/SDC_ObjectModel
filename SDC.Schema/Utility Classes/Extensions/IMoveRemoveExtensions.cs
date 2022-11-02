using CSharpVitamins;
using System.CodeDom;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Xml.Linq;



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
		private static void MoveInDictionaries(this BaseType btSource, BaseType targetParent = null!)
		{
			//Remove from ParentNodes and ChildNodes as needed
			btSource.UnRegisterParent();

			//Re-register item node under new parent
			btSource.RegisterParent(targetParent, childNodesSort: true);

			//We really should resort the topNode.IET nodes collection for every add and move operation.  It's probably best to use the TreeComparer
			//for this, but it would be better to only resort part of the collection, if possible.
			//We could also use the ChildNodes dictionary to create a new faster TreeComparer, as long as we keep ChildNodes sorted
		}


		/// <summary>
		/// Reflect the object tree to determine if <paramref name="btSource"/> can be attached to <paramref name="newParent"/>.   
		/// We must find an <em>exact</em> match for <paramref name="btSource"/>'s element name and the data type in <paramref name="newParent"/> to allow the move.
		/// </summary>
		/// <param name="btSource">The SDC node to test for its ability to be attached to the <paramref name="newParent"/> node.</param>
		/// <param name="newParent">The node to which the <paramref name="btSource"/> node should be moved.</param>
		/// <param name="pObj">The property object on <paramref name="newParent"/> that would attach to <paramref name="btSource"/> (hold its object reference).
		/// pObj may be a List&lt;> or a non-List object.</param>
		/// <returns>True for allowed parent nodes, false for disallowed not allowed</returns>
		internal static bool IsParentNodeAllowed(this BaseType btSource, BaseType newParent, out object? pObj)
			=> SdcUtil.IsParentNodeAllowed(btSource, newParent, out pObj);

		/// <summary>
		/// Reflect the object tree to determine if <paramref name="btSource"/> can be attached to <paramref name="newParent"/>.   
		/// We must find an <em>exact</em> match for <paramref name="btSource"/>'s element name and the data type in <paramref name="newParent"/> to allow the move.
		/// </summary>
		/// <param name="btSource">The SDC node to test for its ability to be attached to the <paramref name="newParent"/> node.</param>
		/// <param name="newParent">The node to which the <paramref name="btSource"/> node should be moved.</param>
		/// <returns>True for allowed parent nodes, false for disallowed not allowed</returns>
		public static bool IsParentNodeAllowed(this BaseType btSource, BaseType newParent)
			=> SdcUtil.IsParentNodeAllowed(btSource, newParent, out _);

		/// <summary>Remove <paramref name="btSource"/> and all its descendants from the SDC tree. 
		/// Checks recursively for descendants and removes them from all dictionaries.
		/// </summary>
		/// <param name="btSource">The node to remove</param>
		/// <param name="cancelIfChildNodes">If true (default), abort node removal if child nodes (descendants) are present.
		/// If false, btSource and all descendants will be permanently removed.</param>
		/// <returns>True if node removal was successful, false if unsuccessful</returns>
		public static bool Remove(this BaseType btSource, bool cancelIfChildNodes = true)
		{
			if (cancelIfChildNodes && !btSource.TryGetChildNodes(out _))
				return false;
			if (btSource is null)
				throw new InvalidOperationException($"{nameof(btSource)} cannot be null.");
			if (btSource.TopNode is null)
				throw new InvalidOperationException($"{nameof(btSource.TopNode)} cannot be null.");
			var par = btSource.ParentNode;
			if (par is null)
				throw new InvalidOperationException($"{nameof(btSource.ParentNode)} cannot be null.");


			bool result = RemoveNodeFromDictionaries(btSource); //remove node from TopNode dictionaries
			if (result is true)
				return result;
			else
				throw new InvalidOperationException($"Method {nameof(Remove)} removed btSource from the object tree dictionaries.\r\n" +
					$"However, an error occured while trying to remove the node object (and descendants, if applicable).\r\n" +
					$"The object tree and its dictionares are now in an inconsistent state.\r\n" +
					$"Try running {nameof(SdcUtil.ReflectRefreshTree)} to refresh the object tree and its dictionaries");


			bool RemoveNodeFromDictionaries(BaseType nodeToRemove)
			{
				_ITopNode? topNode;
				if (nodeToRemove is ITopNode)
					topNode = (_ITopNode?)nodeToRemove;
				else topNode = (_ITopNode?)nodeToRemove.TopNode;

				if (topNode is null)
					throw new NullReferenceException($"{nameof(topNode)} cannot be null.");

				if (topNode._ChildNodes.TryGetValue(nodeToRemove.ObjectGUID, out List<BaseType>? kids))
				{
					while(kids.Count > 0)
					{
						RemoveNodeFromDictionaries(kids.Last()); //recurse depth first 
						RemoveNodeObject(kids.Last()); //Remove from object tree
						kids.Last().UnRegister(); //Remove from dictionaries
					}
					return true;
				}
				return false;
			}


			bool RemoveNodeObject(BaseType nodeToRemove)
			{
				//reflect the parent property that represents nodeToRemove, then set the property to null
				var par = nodeToRemove.ParentNode;
				if (par is null)
					throw new InvalidOperationException($"{nameof(nodeToRemove.ParentNode)} cannot be null.");

				var prop = nodeToRemove.GetPropertyInfoMetaData().PropertyInfo;
				if (prop is null)
					throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: Cannot obtain parent PropertyInfo holding node: {nameof(nodeToRemove)}.");

				var propObj = prop.GetValue(par);
				if (propObj is null)
					throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: Cannot reflect the parent object holding node: {nameof(nodeToRemove)}.");

				if (propObj is IList propIL && propIL[0] != null)
				{
					(propObj as IList)?.Remove(nodeToRemove); //note - this is recursive
					Debug.Print($"Remove SUCCESS IList.Remove: {nodeToRemove.name ?? nodeToRemove.GetType().Name}");
					return true;
				}
				else
				{
					prop.SetValue(par, null);
					Debug.Print($"Remove SUCCESS SetValue null: {nodeToRemove.name ?? nodeToRemove.GetType().Name}");
					return true;
				}
				throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: unable to remove node: {nameof(nodeToRemove)}.");
			}

		}
		/// <summary>
		/// Move an SDC node from one parent node to another. 
		/// A check is performed for illegal moves using IsParentNodeAllowed.
		/// Illegal moves are not performed, causing this method to return false
		/// </summary>
		/// <param name="btSource">THe node to move.</param>
		/// <param name="newParent">The parent node destination to which btSource should be attached</param>
		/// <param name="newListIndex">If newParent supports IList, newListIndex holds the intended destination index in the list.
		/// All negative values will place btSource at the first IList position (index 0).
		/// All values greater than the current last IList index will be added to the end of the list.
		/// The default value is -1, which will place btSource at the start of the list</param>
		/// <returns>true if the move was successful; false if the move was not allowed</returns>
		/// <exception cref="NullReferenceException">NullReferenceException("btSource must not be null.");</exception>
		/// <exception cref="NullReferenceException">NullReferenceException("newParent must not be null.");</exception>
		/// <exception cref="NullReferenceException">NullReferenceException("btSource.ParentNode must not be null.  A top-level (root) node cannot be moved")</exception>
		//		/// <exception cref="Exception">Exception("Could not reflect parent property object to remove node");</exception>
		/// <exception cref="Exception">Exception("Invalid targetProperty");</exception>
		public static bool Move(this BaseType btSource, BaseType newParent, int newListIndex = -1)
		{
			if (btSource is null) 
				throw new NullReferenceException("btSource must not be null.");
			if (newParent is null) 
				
				throw new NullReferenceException("newParent must not be null.");
			//if (btSource.ParentNode is null) throw new NullReferenceException("btSource.ParentNode must not be null.  A top-level (root) node cannot be moved");
			if (newParent.TopNode is null) throw new NullReferenceException("newParent.TopNode must not be null.");

			if (btSource.IsParentNodeAllowed(newParent, out object? targetObj))
			{
				//+Set SameRoot
				//Do btSource and newParent share the same root node? (Are they from the same SDC tree?)
				bool sameRoot = false;  //Do source and target share the same root node?
				var sourceRoot = btSource.FindRootNode();
				if (sourceRoot is null) 
					throw new NullReferenceException("The root node of btSource could not be determined.");
				var newParentRoot = newParent.FindRootNode();
				if (sourceRoot is null) 
					throw new NullReferenceException("The root node of newParent could not be determined.");

				if (sourceRoot.Equals(newParentRoot)) sameRoot = true;
				else //+Process btSource tree with different root node
				{
					BaseType? n;
					BaseType? nextNode;
					int i = 0;
					n = btSource;
					_ITopNode currentTopNode = (_ITopNode)newParent.TopNode;

					for (; ; ) //Set TopNodes in btSource tree until we hit a new ITopNode
					{
						i++;
						if (i > 1000000) 
							throw new InvalidOperationException("Could not assign TopNode to nodes in btSource tree, due to inability to walk its SDC tree");

						n.TopNode = currentTopNode; //currentTopNode can't be null
						nextNode = btSource.GetNodeReflectNext();
						if (nextNode is null) break;

						if (n is _ITopNode itn)
						{
							if (nextNode.TopNode is not null) break; //we assume the rest of the tree has correct TopNode assignments
							currentTopNode = itn;
						}
						n = nextNode;
					}
					var sourceNodeList = SdcUtil.ReflectRefreshSubtreeList(btSource, false, true, 0, 1, true, true, true, true, SdcUtil.CreateElementNameCAP); //re-create dictionaries for all btSource nodes

				}//TODO: process donor node/branch: ObjectID, sGuid, ObjectID, @name, ID, baseURI?, Link?, events?, rule targets?


				if (targetObj is BaseType) //btSource can be attached directly to targetObj
				{
					targetObj = btSource;
					btSource.MoveInDictionaries(targetParent: newParent);
					return true;
				}
				else if (targetObj is IList propList) //btSource can be attached to a member of a List
				{
					//Remove this from current parent object
					btSource.IsParentNodeAllowed(btSource.ParentNode!, out object? currentParentObj);
					if (currentParentObj is BaseType)
						currentParentObj = null;  //remove the btSource reference form this parent object
					else if (currentParentObj is not null && currentParentObj is IList)
					{
						//remove the btSource reference from this parent IList
						var objList = (IList)currentParentObj;
						if (objList?.IndexOf(btSource) > -1) //this extra test may not be necessary
						{
							objList.Remove(btSource);
							if (objList.Count == 0)
							{
								var result = btSource.ParentNode?.Remove(cancelIfChildNodes: true) ?? false;
								if (!result) 
									throw new Exception("btSource.ParentNode was not removed");
							}
						}
					}
					else 
						throw new Exception("Could not reflect parent property object to remove node");

					if (newListIndex < 0 || newListIndex >= propList.Count) propList.Add(btSource);
					else propList.Insert(newListIndex, btSource);

					btSource.MoveInDictionaries(targetParent: newParent);

					return true;
				}
				throw new Exception("Invalid targetProperty");
			}
			else return false; //invalid Move
		}


		#endregion
		#region Register-UnRegister
		//TODO: ChildNodes is maintained in sorted order by default;
		//Setting childNodesSort to false may speed loading when recreating the dictionaries in the correct order by reflection of the SDC object tree.
		//When adding nodes without reflection-ordering, always set childNodesSort to true.
		//childNodesSort uses reflection to determine the proper order that matches the object tree order
		//There is always a risk of getting out of sync with the nodes in the SDC object model classes.
		//If that happens, we can run SdcUtil.RefreshReflectTree to rebuild the dictionaries.
		internal static void RegisterParent(this BaseType btSource, BaseType inParentNode, bool childNodesSort = true)
		{
			if (inParentNode != null)
			{
				var topNode = (_ITopNode?)btSource.TopNode;
				if (topNode is null)
					throw new NullReferenceException($"{nameof(btSource.TopNode)} cannot be null.");

				RegisterParentNode(btSource, inParentNode, topNode, childNodesSort);

				if (btSource is _ITopNode && topNode != (_ITopNode)btSource) //if we did not already do this... 
				{   //also register this ITopNode object in its own dictionaries.
					topNode = (_ITopNode)btSource;
					RegisterParentNode(btSource, inParentNode, topNode, childNodesSort);
				}
			}
			static void RegisterParentNode(BaseType btSource, BaseType inParentNode, _ITopNode topNode, bool childNodesSort)
			{
				topNode._ParentNodes.Add(btSource.ObjectGUID, inParentNode);

				List<BaseType>? kids;
				topNode._ChildNodes.TryGetValue(inParentNode.ObjectGUID, out kids);
				if (kids is null)
				{
					kids = new List<BaseType>();
					topNode._ChildNodes.Add(inParentNode.ObjectGUID, kids);
					kids.Add(btSource); //no need to sort with only one item in the list
				}
				else
				{
					kids.Add(btSource);
					if (kids.Count > 1 && childNodesSort)
					{
						kids.Sort(treeSibComparer); //sort by reflecting the object tree
					}
				}
			}
		}

		internal static void UnRegisterParent(this BaseType btSource)
		{
			var par = btSource.ParentNode;
			bool success = false;
			var topNode = (_ITopNode?)btSource.TopNode;

			if (topNode?._ParentNodes.ContainsKey(btSource.ObjectGUID) ?? false)
				success = topNode._ParentNodes.Remove(btSource.ObjectGUID);
			// if (!success) throw new Exception($"Could not remove object from ParentNodes dictionary: name: {this.name ?? "(none)"} , ObjectGUID: {this.ObjectGUID}");

			if (par is not null)
			{
				if (topNode?._ChildNodes.ContainsKey(par.ObjectGUID) ?? false)
				{
					var childList = topNode._ChildNodes[par.ObjectGUID];
					success = childList.Remove(btSource); //Returns a List<BaseType> and removes "item" from that list
					if (!success)
						throw new Exception($"Could not remove list node from _ChildNodes dictionary: name: {btSource.name ?? "(none)"}, ObjectGUID: {btSource.ObjectGUID}");
					if (childList.Count == 0) success = topNode._ChildNodes.Remove(par.ObjectGUID); //remove the entire entry from _ChildNodes
					if (!success)
						throw new Exception($"Could not remove parent entry from _ChildNodes dictionary: name: {par.name ?? "(none)"}, ObjectGUID: {par.ObjectGUID}");
				}
			}
		} //!not tested
		private static void UnRegister(this BaseType btSource)
		{
			var topNode = (_ITopNode?)btSource.TopNode;
			bool success = topNode?._Nodes.Remove(btSource.ObjectGUID) ?? false;
			if (!success)
				throw new Exception($"Could not remove object from _Nodes dictionary: name: {btSource.name ?? "(none)"}, ObjectGUID: {btSource.ObjectGUID}");
			btSource.UnRegisterParent();

			if (btSource is IdentifiedExtensionType iet)
			{
				var inb = topNode?._IETnodes;
				success = inb?.Remove(iet) ?? false;
				if (!success)
					throw new Exception($"Could not remove object from IETnodesBase collection: name: {btSource.name ?? "(none)"}, ObjectGUID: {btSource.ObjectGUID}");
			}

			if (btSource is ITopNode && topNode != (_ITopNode)btSource)
			{//also UnRegister this ITopNode object in its own dictionaries.

				topNode = (_ITopNode?)btSource;
				success = topNode?._Nodes.Remove(btSource.ObjectGUID) ?? false;
				if (!success)
					throw new Exception($"Could not remove ITopNode object from _Nodes dictionary: name: {btSource.name ?? "(none)"}, ObjectGUID: {btSource.ObjectGUID}");
				btSource.UnRegisterParent();

				if (btSource is IdentifiedExtensionType ietTop)
				{
					var inb = topNode?._IETnodes;
					success = inb?.Remove(ietTop) ?? false;
					if (!success)
						throw new Exception($"Could not remove ITopNode object from IETnodesBase collection: name: {btSource.name ?? "(none)"}, ObjectGUID: {btSource.ObjectGUID}");
				}
			}
		} //!not tested
		#endregion



	}
}
