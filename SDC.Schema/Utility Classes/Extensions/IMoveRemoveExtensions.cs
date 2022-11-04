using CSharpVitamins;
using System.CodeDom;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

		/// <summary>Remove <b><paramref name="btSource"/></b> and all its descendants from the SDC tree. 
		/// Checks recursively for descendants and removes them from all dictionaries.
		/// </summary>
		/// <param name="btSource">The node to remove</param>
		/// <param name="cancelIfChildNodes">If true (default), abort node removal if child nodes (descendants) are present.
		/// If false, btSource and all descendants will be permanently removed.</param>
		/// <returns>True if node removal was successful; false if unsuccessful</returns>
		public static bool RemoveRecursive(this BaseType btSource, bool cancelIfChildNodes = true)
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


			bool result = RemoveNodesRecursively(btSource); //remove node from TopNode dictionaries
			if (result is true)
				return result;
			else
				throw new InvalidOperationException($"Method {nameof(RemoveRecursive)} removed btSource from the object tree dictionaries.\r\n" +
					$"However, an error occured while trying to remove the node object (and descendants, if applicable).\r\n" +
					$"The object tree and its dictionares are now in an inconsistent state.\r\n" +
					$"Try running {nameof(SdcUtil.ReflectRefreshTree)} to refresh the object tree and its dictionaries");


			bool RemoveNodesRecursively(BaseType nodeToRemove)
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
						RemoveNodesRecursively(kids.Last()); //recurse depth first 
						var lastKidPar = kids.Last().ParentNode; //retrieve lastKidPar now, before we remove nodes from dictionaries

						RemoveNodeObject(kids.Last()); //Remove from object tree
						kids.Last().UnRegisterNode(); //Remove from dictionaries
					}
					return true;
				}
				return false;
			}
		}
		/// <summary>
		/// Reflect the parent property in the object tree that represents nodeToRemove, 
		/// then use reflection to set the property to null.  This is non-recursive.<br/>
		/// Child nodes are not checked and are not individually removed, so the caller must ensure that no child nodes are present.<br/>
		/// Call UnRegisterNode after this method to remove nodes from the node dictionaries.
		/// </summary>
		/// <param name="nodeToRemove"></param>
		/// <returns>true if the node is successfuly removed</returns>
		/// <exception cref="InvalidOperationException"></exception>
		private static bool RemoveNodeObject(BaseType nodeToRemove)
		{
			var par = nodeToRemove.ParentNode;
			if (par is null)
				throw new InvalidOperationException($"{nameof(nodeToRemove.ParentNode)} cannot be null.");

			var prop = nodeToRemove.GetPropertyInfoMetaData().PropertyInfo;
			if (prop is null)
				throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: Cannot obtain parent PropertyInfo holding: {nameof(nodeToRemove)}.");

			var propObj = prop.GetValue(par);
			if (propObj is null)
				throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: Cannot reflect the parent object holding: {nameof(nodeToRemove)}.");

			if (propObj is IList propIL)
			{
				if (propIL.Contains(nodeToRemove))
				{
					Debug.Print($"Before Remove: nodeToRemove is null? {nodeToRemove is null}");
					Console.WriteLine($"Before Remove: nodeToRemove is null? {nodeToRemove is null}");
					propIL.Remove(nodeToRemove);
					Debug.Print($"After Remove: nodeToRemove is null? {nodeToRemove is null}");
					Console.WriteLine($"After Remove: nodeToRemove is null? {nodeToRemove is null}");
					//nodeToRemove may still hold a reference to our prop object, until nodeToRemove goes out of scope.
					if (nodeToRemove is not null) Debugger.Break();
					return true;
				}
				else
					throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: unable to locate {nameof(nodeToRemove)} in IList.");
			}
			else
			{
				Debug.Print($"Before SetValue: propObj is null? {propObj is null}");
				Console.WriteLine($"Before SetValue: propObj is null? {propObj is null}");
				prop.SetValue(par, null);
				Debug.Print($"After SetValue: propObj is null? {propObj is null}");
				Console.WriteLine($"After SetValue: propObj is null? {propObj is null}");
				//propObj will still hold a reference to our prop object, until propObj goes out of scope.
				//if (propObj is not null) Debugger.Break();
				return true;
			}
			throw new InvalidOperationException($"{nameof(RemoveNodeObject)}: unable to remove node: {nameof(nodeToRemove)}.");
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
				else //!+Process btSource tree with different root node
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
					//Re-create dictionaries, name, sGuid, etc for all btSource nodes.
					//Order is not reset here, but maybe we'll add it later.
					var sourceNodeList = SdcUtil.ReflectRefreshSubtreeList(btSource, false, true, 0, 1, true, true, true, true, SdcUtil.CreateElementNameCAP); 

				}//TODO: process donor node/branch: ObjectID, sGuid, ObjectID, @name, ID, baseURI?, Link?, events?, rule targets?

				bool isSourceParentChildless = false;

				if (targetObj is BaseType) //btSource can be attached directly to targetObj
				{
					targetObj = btSource;
					btSource.MoveInDictionaries(targetParent: newParent);
					return true;
				}
				else if (targetObj is IList propList) //btSource can be attached to a member of a List
				{
					var sourceParent = btSource.ParentNode;
					//par can be null if we are grafting from the btSource tree to another SDC object tree, and btSource is the root node of its tree
					if (sourceParent is not null) //Remove the reference from par to btSource
					{
						//Remove this from current parent object
						//The IsParentNodeAllowed call is done only to obtain the currentParentObj, which hold the reference to btSource
						btSource.IsParentNodeAllowed(sourceParent, out object? currentParentObj);
						if (currentParentObj is not null)
						{
							if (currentParentObj is BaseType)
							{
								RemoveNodeObject(sourceParent);
							}
							else if (currentParentObj is not null && currentParentObj is IList objList)
							{
								//remove the btSource reference from this parent IList
								//var objList = (IList)currentParentObj;
								if (objList?.IndexOf(btSource) > -1) //this extra test may not be necessary
								{
									objList.Remove(btSource);

									if (objList.Count == 0)
									{
										RemoveNodeObject(sourceParent); //requires _ParentNodes entry to work

										//sourceParent was not previously removed in dictionaries, we only removed its last child node
										//Since it's now "childless," we should remove this orphan node from both the dictionaries and the SDC OM
										isSourceParentChildless = true;								
									}
								}
							}
							else
								throw new InvalidOperationException($"Could not reflect parent SDC property object ({nameof(currentParentObj)}) to remove node");
						}
						else
							throw new InvalidOperationException($"Could not obtain SDC property object ({nameof(currentParentObj)})");
					}
					else { }//sourceParent is null
						//btSource.RegisterNodeAndParent();

					if (newListIndex < 0 || newListIndex >= propList.Count) 
						propList.Add(btSource);
					else 
						propList.Insert(newListIndex, btSource);

					btSource.MoveInDictionaries(targetParent: newParent);
					if (isSourceParentChildless)
					{
						//UnRegisterParent(sourceParent!);
						UnRegisterNode(sourceParent!); //this calls UnRegisterParent also
					}
					return true;
				}
				else 
					throw new InvalidOperationException("Invalid targetObj: targetObj must be BaseType or IList");
				
			}
			else return false; //invalid Move
		}


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
		/// If <b><paramref name="node"/></b> is ITopNode, it is also registered in it's own class's TopNode dictionaries
		/// </summary>
		/// <param name="node"/>
		/// <param name="node"></param>
		/// <param name="parentNode">If adding nodes manually, in the BaseType constructor, a parent Node should be provided</param>
		/// <param name="childNodesSort"></param>
		internal static void RegisterNodeAndParent(this BaseType node, BaseType? parentNode = null, bool childNodesSort = true)
		{
			if (node.TopNode is not null)
			{
				parentNode??= node.ParentNode;
				_ITopNode? _topNode = (_ITopNode)node.TopNode;

				if (_topNode is null)
					throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

				RegisterNode(_topNode);
				//Populate the _ChildNodes and _ParentNodes dictionaries:
				if (parentNode is not null) node.RegisterParent(parentNode, childNodesSort);


				if (node is _ITopNode _topTopNode && _topTopNode != _topNode) //if we did not already do this... 
				{   //also register this ITopNode object in its own dictionaries.
					_topTopNode = (_ITopNode)node;
					RegisterNode(_topTopNode);
				}

				void RegisterNode(_ITopNode tn)
				{
					tn._Nodes.Add(node.ObjectGUID, node);

					//This is a convenient place to update regTopNode.MaxObjectID; 
					tn._MaxObjectIDint = BaseType.LastObjectID;

					if (node is IdentifiedExtensionType iet)
					{
						tn._IETnodes.Add(iet);
						//tn._IETnodes.OrderBy(n => n.order);
					}
					return;
				}
			}
			else { } //There is no TopNode to hold our dictionaries, so we can't register the node
		}

		/// <summary>
		/// Register <b><paramref name="node"/></b> in _ParentNodes and _ChildNodes dictionaries.  
		/// If <b><paramref name="node"/></b> is ITopNode, it is also registered in <b><paramref name="node"/></b>'s own TopNode dictionaries
		/// </summary>
		/// <param name="node"></param>
		/// <param name="inParentNode"></param>
		/// <param name="childNodesSort"></param>
		/// <exception cref="NullReferenceException"></exception>
		internal static void RegisterParent(this BaseType node, BaseType inParentNode, bool childNodesSort = true)
		{
			if (inParentNode != null)
			{
				var _topNode = (_ITopNode?)node.TopNode;
				if (_topNode is null)
					throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

				RegisterParentNode(node, inParentNode, _topNode, childNodesSort);

				if (node is _ITopNode _topTopNode && _topTopNode != _topNode) //if we did not already do this... 
				{   //also register this ITopNode object in its own dictionaries.
					RegisterParentNode(node, inParentNode, _topTopNode, childNodesSort);
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
					kids.Add(btSource);
					if (kids.Count > 1 && childNodesSort)
					{
							kids.Sort(treeSibComparer); //sort by reflecting the object tree							
					}
				}
			}
		}

		/// <summary>
		/// UnRegister <b><paramref name="node"/></b> in _ParentNodes and _ChildNodes dictionaries.  
		/// If <b><paramref name="node"/></b> is ITopNode, it is also unregistered in <b><paramref name="node"/></b>'s own TopNode dictionaries
		/// </summary>
		/// <param name="node"></param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		internal static void UnRegisterParent(this BaseType node)
		{
			var par = node.ParentNode;
			bool success;
			var _topNode = (_ITopNode?)node.TopNode;
			if (_topNode is null)
				throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");

			UnRegisterParentNode(_topNode);

			if (node is _ITopNode _topTopNode && _topTopNode != _topNode) //if we did not already do this... 
			{   //also register this ITopNode object in its own dictionaries.
				UnRegisterParentNode(_topTopNode);
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
						throw new InvalidOperationException($"Could not remove list node from _ChildNodes dictionary: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");
					if (childList.Count == 0) success = tn._ChildNodes.Remove(par.ObjectGUID); //remove the entire entry from _ChildNodes
					if (!success)
						throw new InvalidOperationException($"Could not remove parent entry from _ChildNodes dictionary: name: {par.name ?? "(none)"}, ObjectGUID: {par.ObjectGUID}");
				}
				else {} //no _ChildNodes entries are present for this node
			}
		} //!not tested

		/// <summary>
		/// Remove <b><paramref name="node"/></b> from _Nodes (and, if applicable, _IETnodes) dictionaries.<br/>
		/// It will also call <b><see cref="UnRegisterParent(BaseType)"/></b> to remove entries from _ChildNodes and _ParentNodes  
		/// </summary>
		/// <param name="node"></param>
		/// <exception cref="Exception"></exception>
		private static void UnRegisterNode(this BaseType node)
		{
			var _topNode = (_ITopNode?)node.TopNode;
			if (_topNode is null)
				throw new NullReferenceException($"{nameof(node.TopNode)} cannot be null.");
			UnRegister(_topNode);

			if (node is _ITopNode _topTopNode && _topTopNode != _topNode) //if we did not already do this... 
			{   //also register this ITopNode object in its own dictionaries.
				UnRegister(_topTopNode);
			}

			void UnRegister(_ITopNode tn)
			{
				bool success = tn?._Nodes.Remove(node.ObjectGUID) ?? false;
				if (!success)
					throw new Exception($"Could not remove object from _Nodes dictionary: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");
				node.UnRegisterParent();

				if (node is IdentifiedExtensionType iet)
				{
					var inb = tn?._IETnodes;
					success = inb?.Remove(iet) ?? false;
					if (!success)
						throw new Exception($"Could not remove object from IETnodesBase collection: name: {node.name ?? "(none)"}, ObjectGUID: {node.ObjectGUID}");
				}
			}
		} //!not tested
		#endregion



	}
}
