using System.Diagnostics;
using System.Reflection;
using System.Xml.Serialization;



//using SDC;
namespace SDC.Schema
{
	public class TreeComparer : Comparer<BaseType>
	{
		public override int Compare(BaseType nodeA, BaseType nodeB)
		{
			//Debug.Print($" {ord},   A:{nodeA.ObjectID},   B:{nodeB.ObjectID}");

			if (nodeA == nodeB)
			{ Result(0); return 0; }
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
			if (parA is null && nodeB.ParentNode != null) return -1; //nodeA is the top node
			if (parB is null && nodeA.ParentNode != null) return 1;  //nodeB is the top node

			//If both nodes live inside the same parent object:
			if (parA == parB) return SibComparer(parA, nodeA, nodeB, out _);

			//create ascending ancestor ("anc") set ("ancSet") for nodeA branch, with nodeA as the first element in the ancester set, i.e., ancSetA[0] == nodeA
			BaseType? prevPar = null;
			int count = nodeA?.TopNode.Nodes.Count ?? 1000;
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

			//Find the first intersection of the 2 arrays (lowest common ancestor) - the common node furthest from the tree's top node
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
				{//we found the lowest common parent node at ancSetA[indexA] 
				 //the common ancestor of nodeB and nodeA is not added to ancSetB!!
				 //indexB is the highest non-null entry in ancSetB, and thus ancSetB[indexB] contains
					failed = false;
					break;
				}
				prevPar = ancSetB[indexB]?.ParentNode ?? null; //increment the nodeB ancestor node - move one parent level higher
			}
			if (failed || prevPar is null)
				throw new Exception("the supplied nodes cannot be compared because they do not have a common ancester node");

			//We have found the lowest common ancester ("ANC") located at index indexA in ancSetA and at IndexB in ancSetB
			//We now move one parent node further from the root on each tree branch (ancSetA and ancSetA), closer to nodeA and nodeB
			//and determine which of these ancesters has an XML Element sequence that is closer to the root node.
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

			//_______________________________________________________________________________________________________________
			//


			////Use ReflectNextElement on ancSetA[IndexA] or ancSetB[IndexB] to walk down nodes from the common ANC node.
			////Whichever node it encountners first wins.  This is very slow
			//var node = ancSetA[indexA];
			//while (node is not null)
			//{
			//	if (node == nodeA) return -1;
			//	if (node == nodeB) return 1;
			//	node = SdcUtil.ReflectNextElement(node);
			//}
			//throw new InvalidOperationException("Could not determine node order");
			////___________________________________________________________________________________________________________
			////Methods below here will return incorrect results.



			//BaseType? ancNodeA = null;
			//BaseType? ancNodeB = null;

			//if (indexA > 0 && indexB > 0)
			//{
			//	ancNodeA = ancSetA[indexA - 1]; //first child of common ancester node on nodeA branch; this is still an ancester of NodeA, or NodeA itself
			//	ancNodeB = ancSetB[indexB - 1]; //first child of common ancester node on nodeB branch; this is still an ancester of NodeB, or NodeB itself
			//}
			//else
			//{
			//	ancNodeA = ancSetA[indexA]; //common ancester node on nodeA branch; this is still an ancester of NodeA, or NodeA itself
			//	ancNodeB = ancSetB[indexB]; //common ancester node on nodeB branch; this is still an ancester of NodeB, or NodeB itself

			//}

			////Retrieve customized Property Metadata for the class properties that hold our nodes.
			//var piAncNodeA = SdcUtil.GetPropertyInfoMeta(ancNodeA, false);
			//var piAncNodeB = SdcUtil.GetPropertyInfoMeta(ancNodeB, false);

			////Let's see if both items come from the same IEnumerable (ieItems) in ANC, and then see which one has the lower itemIndex
			//if (piAncNodeA.IeItems is not null && piAncNodeB.IeItems is not null &&
			//	piAncNodeA.IeItems == piAncNodeB.IeItems &&
			//	piAncNodeA.ItemIndex > -1 && piAncNodeB.ItemIndex > -1)
			//{
			//	if (piAncNodeA.ItemIndex == piAncNodeB.ItemIndex)
			//	{ Result(0); return 0; }
			//	//throw new Exception("Unknown error - the compared nodes share a common ParentNode and appear to be identical");
			//	if (piAncNodeA.ItemIndex < piAncNodeB.ItemIndex)
			//	{ Result(-1); return -1; }
			//	if (piAncNodeB.ItemIndex < piAncNodeA.ItemIndex)
			//	{ Result(1); return 1; }
			//}

			//if (piAncNodeA.PropertyInfo.DeclaringType is not null && piAncNodeB.PropertyInfo.DeclaringType is not null)
			//{
			//	//In XML Schemas, it appears that base class (Schema base type) xml elements always come before subclass elements, regardless of the XmlElementAttribute Order value.
			//	if (piAncNodeA.PropertyInfo.DeclaringType.IsSubclassOf(piAncNodeB.PropertyInfo.DeclaringType))
			//	{ Result(1); return 1; } //base class xml orders come before subclasses; ancNodeA is the base type here
			//	if (piAncNodeB.PropertyInfo.DeclaringType.IsSubclassOf(piAncNodeA.PropertyInfo.DeclaringType))
			//	{ Result(-1); return -1; } //base class xml orders come before subclasses; ancNodeB is the base type here
			//}

			////Determine the comparison based on the xmlOrder in the XmlElementAttributes
			////if (piAncNodeA.XmlOrder < piAncNodeB.XmlOrder)
			////{ Result(-1); return -1; }
			////if (piAncNodeB.XmlOrder < piAncNodeA.XmlOrder)
			////{ Result(1); return 1; }





			//Debugger.Break();
			//Debug.Print($"A:{nodeA.ObjectID}, name: {nodeA.name};   B:{nodeB.ObjectID}, name: {nodeB.name}");
			//throw new Exception("the compare nodes algorithm could not determine the node order");

			void Result(int i)
			{	//For debugging only:
				//Debug.Print($" {i}:ord:{ord},   A:{nodeA.ObjectID},   B:{nodeB.ObjectID}");
				//if (i != ord) Debugger.Break();
			}
		}
		/// <summary>
		/// Given a parent node, retrieve the list of child nodes, if present.
		/// Uses reflection only, and does not use any node dictionaries.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <returns>List<BaseType>? containing the child nodes</returns>
		public static int SibComparer(BaseType parentNode, BaseType nodeA, BaseType nodeB, out int nodeIndex)
		{
			nodeIndex = -1;
			if (nodeA == nodeB) return 0;
			IEnumerable<PropertyInfo>? piIE = null;			

			//Create a LIFO stack of the targetNode inheritance hierarchy.  The stack's top level type will always be BaseType
			//For most non-datatype SDC objects, it could be a bit more efficient to use ExtensionBaseType - we can test this another time
			Type t = parentNode.GetType();
			var s = new Stack<Type>();
			s.Push(t);

			do
			{//build the stack of inherited types from parentNode
				t = t.BaseType!;
				if (t.IsSubclassOf(typeof(BaseType))) s.Push(t);
				else break; //quit when we hit a non-BaseType type
			} while (true);

			//starting with the least-derived inherited type (BaseType), look for any non-null properties of targetNode
			while (s.Count > 0)
			{
				piIE = s.Pop()
					.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
					.Where(p => p.GetCustomAttributes<XmlElementAttribute>().Any());
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
						if (o is IEnumerable<BaseType> ie && ie.Any())
							foreach (var n in ie)
							{
								if (n == nodeA) return -1;
								if (n == nodeB) return 1;
							}
					}
				}
			}
			throw new InvalidOperationException("The supplied nodes do not share a common parent node");
		}
	}










}
