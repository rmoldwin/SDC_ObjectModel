using System.Diagnostics;



//using SDC;
namespace SDC.Schema
{
	/// <summary>
	/// Use in Sort methods to determine the correct ordered sequence of SDC child nodes.  Inherits from Comparer&lt;T>
	/// </summary>
	public class TreeSibComparer : Comparer<BaseType>
	{
		/// <inheritdoc/>
		public override int Compare(BaseType? nodeA, BaseType? nodeB)
		{
			if (nodeA is null || nodeB is null) 
				throw new ArgumentException("Neither nodeA nor nodeB can be null");
			var par = nodeA.ParentNode;
			if (par is null) 
				throw new ArgumentException("The nodeA parent node was null"); 
			if (nodeB.ParentNode != par) 
				throw new ArgumentException("The nodeA parent node must be the same as the nodeB parent node");
			if (nodeA == nodeB)
				return 0; 

			return TreeComparer.SibComparer(par, nodeA, nodeB, out _);

		}
	}
}
