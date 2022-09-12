using System.Collections;
using System.Data;
using System.Diagnostics;



//using SDC;
namespace SDC.Schema
{
	public static class IMoveRemoveExtensions
	{
		#region IMoveRemove //not tested
		private static void MoveInDictionaries(this BaseType btSource, BaseType targetParent = null!)
		{
			//Remove from ParentNodes and ChildNodes as needed
			(btSource as BaseType).UnRegisterParent();

			//Re-register item node under new parent
			btSource.RegisterParent(targetParent);
		}


		/// <summary>
		/// Reflect the object tree to determine if <paramref name="item"/> can be attached to <paramref name="newParent"/>.   
		/// We must find an <em>exact</em> match for <paramref name="item"/>'s element name and the data type in <paramref name="newParent"/> to allow the move.
		/// </summary>
		/// <param name="item">The SDC node to test for its ability to be attached to the <paramref name="newParent"/> node.</param>
		/// <param name="newParent">The node to which the <paramref name="item"/> node should be moved.</param>
		/// <param name="pObj">The property object on <paramref name="newParent"/> that would attach to <paramref name="item"/> (hold its object reference).
		/// pObj may be a List&lt;> or a non-List object.</param>
		/// <returns>True for allowed parent nodes, false for disallowed not allowed<</returns>
		public static bool IsParentNodeAllowed(this BaseType btSource, BaseType newParent, out object? pObj)
			=> SdcUtil.IsParentNodeAllowed(btSource, newParent, out pObj);

		/// <summary>
		/// Reflect the object tree to determine if <paramref name="item"/> can be attached to <paramref name="newParent"/>.   
		/// We must find an <em>exact</em> match for <paramref name="item"/>'s element name and the data type in <paramref name="newParent"/> to allow the move.
		/// </summary>
		/// <param name="item">The SDC node to test for its ability to be attached to the <paramref name="newParent"/> node.</param>
		/// <param name="newParent">The node to which the <paramref name="item"/> node should be moved.</param>
		/// <returns>True for allowed parent nodes, false for disallowed not allowed<</returns>
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
			if (cancelIfChildNodes && btSource.HasChildren()) return false;

			foreach (var childNode in par.TopNode.ChildNodes[btSource.ObjectGUID])
				childNode.Remove();

			//reflect the parent property that represents the "this" object, then set the property to null
			var prop = par.GetType().GetProperties().Where(p => p.GetValue(par) == btSource).FirstOrDefault();
			if (prop is null) throw new InvalidOperationException("Cannot obtain parent Property holding btSource.");

			if (prop != null)
			{
				var propObj = prop.GetValue(par);
				if (propObj is IList propIL && propIL[0] != null)
				{
					(propObj as IList<BaseType>)?.Remove(btSource);
				}
				else prop.SetValue(par, null);

				btSource.UnRegisterThis();

				return true;
			}
			return false;
		}
		/// <summary>
		/// Move an SDC node from one parent node to another. 
		/// A check is performed for illegal moves using IsParentNodeAllowed(newParent, out object targetObj, newListIndex).
		/// Illegal moves are not performed, cauising this method to return false
		/// </summary>
		/// <param name="btSource">THe node to move.</param>
		/// <param name="newParent">The parent node destination to which btSource should be attached</param>
		/// <param name="newListIndex">If newParent supports IList, newListIndex holds the intended destination index in the list.
		/// All negative values will place btSource at the first IList position (index 0).
		/// All values greater than the current last IList index will be added to the end of the list.
		/// The defaut value is -1, which will place btSource at the start of the list</param>
		/// <returns>true if the move was successfull false if the move was not allowed</returns>
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


			if (btSource.IsParentNodeAllowed(newParent, out object targetObj))
			{
				
				if (targetObj is BaseType)
				{
					btSource.MoveInDictionaries(targetParent: newParent);
					targetObj = btSource;
					return true;
				}
				else if (targetObj is IList propList)
				{
					//Remove this from current parent object
					btSource.IsParentNodeAllowed(btSource.ParentNode, out object? currentParentObj);
					if (currentParentObj is BaseType) currentParentObj = null;
					else if (currentParentObj is not null && currentParentObj is IList)
					{
						var objList = (IList)currentParentObj;
						if (objList?.IndexOf(btSource) > -1) objList.Remove(btSource);
						else throw new Exception("Could not find node in parent object list");
					}
					else throw new Exception("Could not reflect parent property object to remove node");

					//IList propList = (IList)targetObj;
					btSource.MoveInDictionaries(targetParent: newParent);

					if (newListIndex < 0 || newListIndex >= propList.Count) propList.Add(btSource);
					else propList.Insert(newListIndex, btSource);

					return true;
				}
				throw new Exception("Invalid targetProperty");

			}
			else return false; //invalid Move
		}


		#endregion
		#region Register-UnRegister
		//TODO: ChildNodes are not maintained in sorted order here; consider adding a sort flag to sort nodes after adding.
		//Alternatively, add a required index to specify where the incoming child node node should be inserted among the sib nodes.
		//Ther is always a risk of getting out of sync with the nodes in the SDC object model classes.
		internal static void RegisterParent<T>(this BaseType btSource, T inParentNode) where T : BaseType
		{
			//btSource.ParentNode = inParentNode;

			try
			{
				if (inParentNode != null)
				{   //Register parent node
					btSource.TopNode.ParentNodes.Add(btSource.ObjectGUID, inParentNode);

					List<BaseType>? kids;
					btSource.TopNode.ChildNodes.TryGetValue(inParentNode.ObjectGUID, out kids);
					if (kids is null)
					{
						kids = new List<BaseType>();
						btSource.TopNode.ChildNodes.Add(inParentNode.ObjectGUID, kids);
					}
					kids.Add(btSource);
					//inParentNode.IsLeafNode = false; //the parent node has a child node, so it can't be a leaf node
				}
			}
			catch (Exception ex)
			{ Debug.WriteLine(ex.Message + "/n  ObjectID:" + btSource.ObjectID.ToString()); }
		}

		internal static void UnRegisterParent(this BaseType btSource)
		{
			var par = btSource.ParentNode;
			try
			{
				bool success = false;
				if (par != null)
				{
					if (btSource.TopNode.ParentNodes.ContainsKey(btSource.ObjectGUID))
						success = btSource.TopNode.ParentNodes.Remove(btSource.ObjectGUID);
					// if (!success) throw new Exception($"Could not remove object from ParentNodes dictionary: name: {this.name ?? "(none)"} , ObjectID: {this.ObjectID}");

					if (btSource.TopNode.ChildNodes.ContainsKey(par.ObjectGUID))
						success = btSource.TopNode.ChildNodes[par.ObjectGUID].Remove(btSource); //Returns a List<BaseType> and removes "item" from that list
																								//if (!success) throw new Exception($"Could not remove object from ChildNodes dictionary: name: {this.name ?? "(none)"}, ObjectID: {this.ObjectID}");

					//if (TopNode.ChildNodes.ContainsKey(this.ObjectGUID))
					//    success = TopNode.ChildNodes[this.ObjectGUID].Remove(this);
					//if (!success) throw new Exception($"Could not remove object from ChildNodes dictionary: name: {this.name ?? "(none)"}, ObjectID: {this.ObjectID}");

					//if(TopNode.ChildNodes[par.ObjectGUID] is null || par.TopNode.ChildNodes[par.ObjectGUID].Count() == 0)
					//par.IsLeafNode = true; //the parent node has no child nodes, so it is a leaf node
				}
			}
			catch (Exception ex)
			{ Debug.WriteLine(ex.Message + "/n  ObjectID:" + btSource.ObjectID.ToString()); }

			//btSource.ParentNode = null;

		} //!not tested
		private static void UnRegisterThis(this BaseType btSource)
		{
			bool success = btSource.TopNode.Nodes.Remove(btSource.ObjectGUID);
			if (!success) throw new Exception($"Could not remove object from Nodes dictionary: name: {btSource.name ?? "(none)"}, ObjectID: {btSource.ObjectID}");
			btSource.UnRegisterParent();
		} //!not tested
		#endregion



	}
}
