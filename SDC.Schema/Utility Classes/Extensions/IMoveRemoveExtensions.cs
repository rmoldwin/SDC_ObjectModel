﻿using CSharpVitamins;
using System.Collections;
using System.Data;
using System.Diagnostics;
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
		private static bool X_ChildNodesSort = false;
		#region IMoveRemove //not tested
		private static void MoveInDictionaries(this BaseType btSource, BaseType targetParent = null!)
		{
			//Remove from ParentNodes and ChildNodes as needed
			btSource.UnRegisterParent();

			//Re-register item node under new parent
			btSource.RegisterParent(targetParent, true);
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
			var par = btSource.ParentNode;
			if (par is null) throw new InvalidOperationException("btSource.ParentNode cannot be null.");
			if (cancelIfChildNodes && btSource.TryGetChildNodes(out _)) return false;
			var topNode = (_ITopNode)(par.TopNode);


			if (topNode._ChildNodes.TryGetValue(btSource.ObjectGUID, out List<BaseType>? kids))
				while (kids.Count > 0)
				{
					var kid = kids.First();
					Debug.Print($"kids.Remove recursive: {kid.name}");
					kid.Remove(cancelIfChildNodes); //note - this is recursive
				}

			//reflect the parent property that represents the "this" object, then set the property to null
			var prop = btSource.GetPropertyInfoMetaData().PropertyInfo;
			//var prop = par.GetType().GetProperties().Where(p => p.GetValue(par) == btSource).FirstOrDefault();
			if (prop is null) throw new InvalidOperationException("Cannot obtain parent Property holding btSource.");

			if (prop != null)
			{
				var propObj = prop.GetValue(par);
				if (propObj is IList propIL && propIL[0] != null)
				{
					(propObj as IList)?.Remove(btSource);
					Debug.Print($"IList.Remove: {btSource.name}");
				}
				else
				{
					prop.SetValue(par, null);
					Debug.Print($"SetValue null: {btSource.name}");
				}

				btSource.UnRegisterThis();
				Debug.Print($"Unregister: {btSource.name}");

				return true;
			}
			return false;
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
		/// <exception cref="Exception">Exception("Could not reflect parent property object to remove node");</exception>
		/// <exception cref="Exception">Exception("Invalid targetProperty");</exception>
		public static bool Move(this BaseType btSource, BaseType newParent, int newListIndex = -1)
		{
			if (btSource is null) throw new NullReferenceException("btSource must not be null.");
			if (newParent is null) throw new NullReferenceException("newParent must not be null.");
			if (btSource.ParentNode is null) throw new NullReferenceException("btSource.ParentNode must not be null.  A top-level (root) node cannot be moved");

			if (btSource.IsParentNodeAllowed(newParent, out object? targetObj))
			{

				if (targetObj is BaseType) //btSource can be attached directly to the target
				{
					targetObj = btSource;
					btSource.MoveInDictionaries(targetParent: newParent);
					return true;
				}
				else if (targetObj is IList propList) //btSource can be attached to a member of a List
				{
					//Remove this from current parent object
					btSource.IsParentNodeAllowed(btSource.ParentNode, out object? currentParentObj);
					if (currentParentObj is BaseType)
						currentParentObj = null;  //remove the btSource reference form this parent object
					else if (currentParentObj is not null && currentParentObj is IList)
					{
						//remove the btSource reference form this parent IList
						var objList = (IList)currentParentObj;
						if (objList?.IndexOf(btSource) > -1) //this extra test may not be necessary
							objList.Remove(btSource);
						else throw new Exception("Could not find node in parent object list");
					}
					else throw new Exception("Could not reflect parent property object to remove node");

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
		//TODO: ChildNodes are not maintained in sorted order here; consider adding a sort flag to sort nodes after adding.
		//Alternatively, add a required index to specify where the incoming child node should be inserted among the sib nodes.
		//There is always a risk of getting out of sync with the nodes in the SDC object model classes.
		internal static void RegisterParent(this BaseType btSource, BaseType inParentNode, bool childNodesSort = false)
		{
			if (inParentNode != null)
			{
				var topNode = (_ITopNode)btSource.TopNode;
				RegisterParentNode(btSource, inParentNode, topNode, childNodesSort);
				if (btSource is _ITopNode)
				{//also register this ITopNode object in its own dictionaries.
					topNode = (_ITopNode)btSource;
					RegisterParentNode(btSource, inParentNode, topNode, childNodesSort);
				}
			}
			static void RegisterParentNode(BaseType btSource, BaseType inParentNode, _ITopNode topNode, bool childNodesSort)
			{
				try
				{   //Register parent node

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
						if (kids.Count > 1 & childNodesSort)
							kids.Sort(treeSibComparer); //sort by reflecting the object tree
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message + "/n  ObjectID:" + btSource.ObjectID.ToString());
					throw;
				}
			}
		}

		internal static void UnRegisterParent(this BaseType btSource)
		{
			var par = btSource.ParentNode;
			try
			{
				bool success = false;
				var topNode = (_ITopNode)btSource.TopNode;

				if (topNode._ParentNodes.ContainsKey(btSource.ObjectGUID))
					success = topNode._ParentNodes.Remove(btSource.ObjectGUID);
				// if (!success) throw new Exception($"Could not remove object from ParentNodes dictionary: name: {this.name ?? "(none)"} , ObjectID: {this.ObjectID}");

				if (par != null)
				{
					if (topNode._ChildNodes.ContainsKey(par.ObjectGUID))
					{
						var childList = topNode._ChildNodes[par.ObjectGUID];
						success = childList.Remove(btSource); //Returns a List<BaseType> and removes "item" from that list
						if (!success) throw new Exception($"Could not remove list node from GetChildNodes dictionary: name: {btSource.name ?? "(none)"}, ObjectID: {btSource.ObjectID}");
						if (childList.Count == 0) success = topNode._ChildNodes.Remove(par.ObjectGUID); //remove the entire entry from _ChildNodes
						if (!success) throw new Exception($"Could not remove parent entry from GetChildNodes dictionary: name: {par.name ?? "(none)"}, ObjectID: {par.ObjectID}");
					}
				}
			}
			catch (Exception ex)
			{ 
				Debug.WriteLine(ex.Message + "/n  ObjectID:" + btSource.ObjectID.ToString());
				throw;
			}

			//btSource.ParentNode = null;

		} //!not tested
		private static void UnRegisterThis(this BaseType btSource)
		{
			var topNode = (_ITopNode)btSource.TopNode;
			bool success = topNode._Nodes.Remove(btSource.ObjectGUID);
			if (!success) throw new Exception($"Could not remove object from GetNodes dictionary: name: {btSource.name ?? "(none)"}, ObjectID: {btSource.ObjectID}");
			btSource.UnRegisterParent();

			if (btSource is IdentifiedExtensionType iet)
			{
				var inb = topNode._IETnodes;
				success = inb.Remove(iet);
				if (!success) throw new Exception($"Could not remove object from IETnodesBase collection: name: {btSource.name ?? "(none)"}, sGuid: {btSource.sGuid}");
			}

		} //!not tested
		#endregion



	}
}