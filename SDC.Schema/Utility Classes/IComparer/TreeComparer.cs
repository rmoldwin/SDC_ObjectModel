using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;


//using SDC;
namespace SDC.Schema
{
	public class TreeComparer : Comparer<BaseType>
	{

		public override int Compare(BaseType? nodeA, BaseType? nodeB)
		{
			if(nodeA is null || nodeB is null)
				throw new ArgumentNullException("neither nodeA nor nodeB can be null");
			return CompareNodes(nodeA, nodeB);
		}
		public static int CompareNodes(BaseType nodeA, BaseType nodeB)
		{
			//Debug.Print($" {ord},   A:{nodeA.ObjectID},   B:{nodeB.ObjectID}");

			if (nodeA == nodeB)
			{ Result(0); return 0; }
			//These next line works only for trees with a single common TopNode.  We need to generalize it later to support packages with subtrees.
			if (nodeA.TopNode != nodeB.TopNode) throw new Exception("nodeA and nodeB are derived from different SDC templates");
			if (nodeA == nodeA.TopNode)
			{ Result(-1); return -1; }
			if (nodeB == nodeB.TopNode)
			{ Result(1); return 1; }
			if (nodeB.ParentNode == nodeA)
			{ Result(-1); return -1; }
			if (nodeA.ParentNode == nodeB)
			{ Result(1); return 1; }
			var parA = nodeA.ParentNode;
			var parB = nodeB.ParentNode;
			if (parA is null && parB is null) throw new Exception("nodeA and nodeB both have null parent _Nodes, and thus cannot be sorted");
			if (parA is null && nodeB.ParentNode != null) return -1; //nodeA is the top node
			if (parB is null && nodeA.ParentNode != null) return 1;  //nodeB is the top node

			//If both nodes live inside the same parent object:
			if (parA == parB) return SibComparer(parA!, nodeA, nodeB, out _);

			//create ascending ancestor ("anc") set ("ancSet") for nodeA branch, with nodeA as the first element in the ancester set, i.e., ancSetA[0] == nodeA
			BaseType? prevPar = null;
			int count = nodeA?.TopNode?.Nodes.Count ?? 1000;
			if (count < 1000) count = 1000;  //in case Nodes is not yet fully populated
			BaseType[]? ancSetA = new BaseType[count];
			BaseType[]? ancSetB = new BaseType[count];
			int indexA = 0;
			ancSetA[indexA] = nodeA!;
			prevPar = ancSetA[indexA]?.ParentNode ?? null;
			while (prevPar != null)
			{
				ancSetA[indexA + 1] = prevPar;
				indexA++;
				prevPar = ancSetA[indexA]?.ParentNode ?? null;
			}

			//Find the first intersection of the 2 arrays (closest common ancestor) - the common node furthest from the tree's top node
			//If they share an ancestor, get the common ancestor object, to find which ancester is first.
			//If they have different ancesters, move down one node in both ancSetA and ancSetB.
			//The set with the highest level (first) parent indicates that the that all nodes in that set come before all nodes in the other set.

			//we reuse indexA to refer to the common ancester in AncSetA:
			//find index of nodeB in the ancestor tree of nodeA
			indexA = SdcUtil.IndexOf(ancSetA, nodeB);

			if (indexA > -1)  //nodeB is an ancester of NodeA,and thus comes first in order
			{ Result(1); return 1; }

			int indexB = 0;
			ancSetB[indexB] = nodeB!;
			bool failed = true;//failed indicates that the nodeA and nodeB trees branches never converge on a common ancestor.              
							   //This will generate an exception unless it is set to false below.
							   //!Now we look for nodeB *ancestors* in the ancestor tree of nodeA.
							   //Loop through nodeB ancesters (we build ancSetB as we loop here) until we find a common ancester in nodeA's ancesters (already assembled in ancSetB)
			prevPar = ancSetB[indexB].ParentNode;



			while (prevPar != null)
			{   //add the current nodeB ancestor node to ancSetB
				ancSetB[++indexB] = prevPar;  //note that we create the ancSetB only as needed.  No need to walk all the way up to the root node if we don't have to.  Thus it's slightly more efficient to place the deeper-on-tree node in nodeB.
				indexA = SdcUtil.IndexOf(ancSetA, prevPar); //Find the lowest common parent node; later we'll see we can determine which parent node comes first in the tree
															//indexA is the location of the current nodeB ancestor in the list of nodeA ancestors (ancSetA).
															//We are looking for the first time a nodeB ancestor appears in ancSetA.
				if (indexA > -1)
				{//we found the closest common parent node at ancSetA[indexA] 
				 //the common ancestor of nodeB and nodeA is not added to ancSetB!!
				 //indexB is the highest non-null entry in ancSetB, and thus ancSetB[indexB] contains
					failed = false;
					break;
				}
				prevPar = ancSetB[indexB]?.ParentNode ?? null; //increment the nodeB ancestor node - move one parent level higher
			}
			if (failed || prevPar is null)
				throw new Exception("the supplied nodes cannot be compared because they do not have a common ancester node");

			//We have found the closest common ancester ("ANC") located at indexA in ancSetA and at IndexB in ancSetB
			//We now move one parent node further from the root on each tree branch (ancSetA and ancSetB), closer to nodeA and nodeB
			//and determine which of these ancesters has an XML Element (node) position that is closer to the root node.
			//Both of these ancester nodes have ANC as a common SDC ParentNode.
			if (indexA == 0 && indexB > 0)
			{ Result(-1); return -1; } //nodeA (located at index 0) is a direct ancestor of nodeB, so it must come first
			if (indexB == 0 && indexA > 0)
			{ Result(1); return 1; }  //nodeB (located at index 0) is a direct ancester of nodeA, so it must come first

			//Now we reflect inside PrevPar to see which tree comes first - the one with child node at ancSetA[indexA-1] or ancSetB[indexB]
			//If both nodes live inside the same parent object:
			//Check if both common ancestor nodes match:
			if (!(ancSetA[indexA] == ancSetB[indexB])) throw new DataMisalignedException("Error in finding common node parent");
			//Subtract one from each index above to arrive at one node distal (lower) to the common node in each subtree
			return SibComparer(prevPar, ancSetA[indexA-1], ancSetB[indexB-1], out _);
			
			//____________________________________________________________________________________________________________
			void Result(int i)
			{	//For debugging only:
				//Debug.Print($" {i}:ord:{ord},   A:{nodeA.ObjectID},   B:{nodeB.ObjectID}");
				//if (i != ord) Debugger.Break();
			}
		}



		/// <summary>
		/// Given a parent node, determine whether nodeA or nodeB comes first in the child list.<br/>
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns><b>-1</b>: nodeA comes first; <b>1</b>: nodeB comes first; <b>0</b>: nodeA and nodeB reference the same node.</returns>
		public static int SibComparer(BaseType parentNode, BaseType nodeA, BaseType nodeB, out int nodeIndex)
		{
			nodeIndex = -1;
			if (nodeA == nodeB) return 0;
			IEnumerable<PropertyInfo>? piIE = null;
			Type? sType = null;

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = parentNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);

			object locker = new();

			do
			{//build the stack of inherited types from parentNode
				t = t.BaseType!;
				if (t.IsSubclassOf(typeof(BaseType))) s.Push(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);
			
			//starting with the least-derived inherited type (BaseType), look for any non-null properties of targetNode
			while (s.Count > 0)
			{
				sType = s.Pop();
					piIE = sType
					.GetProperties() //(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Any()); //GetCustomAttributes is static, and thus not thread-safe

					foreach (var p in piIE)
					{
						nodeIndex++;
						object? o = p.GetValue(parentNode);
						if (o is not null)
						{
							if (o is BaseType bt)
							{
								if (bt == nodeA) return -1;
								if (bt == nodeB) return 1;
							}
							else if (o is IEnumerable<BaseType> ie && ie.Any())
								foreach (var n in ie)
								{
									if (n == nodeA) return -1;
									if (n == nodeB) return 1;
								}
							else { }
						}
						else { }
					}
				}
			//If we get down here, there is a problem with the TopNode dictionaries.  This data will help with debugging.

			InvalidOperationException ex = new("The supplied nodes cannot be sorted");
			ex.Data.Add("nodeA.name", nodeA?.name);
			ex.Data.Add("nodeA.type", nodeA?.GetType().Name);
			ex.Data.Add("nodeB.name", nodeB?.name);			
			ex.Data.Add("nodeB.type", nodeB?.GetType().Name);
			ex.Data.Add("parentNode.name", parentNode?.name);
			ex.Data.Add("ParentNode.type", parentNode?.GetType().Name);
			ex.Data.Add("nodeIndex", nodeIndex.ToString());
			ex.Data.Add("sType", sType?.Name??"null");


			throw ex;
		}
		
	}

	public class TreeOrderComparer : Comparer<BaseType>
	{
		public override int Compare(BaseType? nodeA, BaseType? nodeB)
		{
			if (nodeA is null && nodeB is null) throw new InvalidOperationException("The supplied nodes cannot be sorted: Both nodes are null");
			if (nodeA == nodeB) return 0; //node reference comparison rather than using order
			if (nodeA is not null && nodeB is null) throw new InvalidOperationException("The supplied nodes cannot be sorted: NodeB is null");
			if (nodeA is null && nodeB is not null) throw new InvalidOperationException("The supplied nodes cannot be sorted: NodeA is null");

			if (nodeA!.order < nodeB!.order) return -1;
			return 1;
		}


	}

		public class TreeGuidComparer : Comparer<Guid>
	{
		private ITopNode topNode { get; set; }
		static TreeComparer treeComparer = new TreeComparer();
		TreeGuidComparer(ITopNode TopNode) //this only works if both nodes share the same TopNode
		{
			topNode = TopNode;
		}
		public override int Compare(Guid guidA, Guid guidB)
		{		

			BaseType? nodeA, nodeB;
			_ITopNode _topNode = (_ITopNode)topNode; //this only works if both nodes share the same TopNode

			_topNode._Nodes.TryGetValue(guidA, out nodeA);
			_topNode._Nodes.TryGetValue(guidB, out nodeB);

			//If a node cannot be located, find the RootNode, and then find all TopNodes, and from there, search each tree
			//alternatively, create a global dictionary,
			//or maintain @order with each add/move
			//or use the dot notation to determine order
			
			if (nodeA is null || nodeB is null)
				throw new ArgumentNullException("neither nodeA nor nodeB can be null");

			
			return treeComparer.Compare(nodeA, nodeB);
		}
	}





	}
