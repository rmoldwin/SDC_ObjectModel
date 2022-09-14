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

			if (nodeA is null || nodeB is null) { return 0; Debugger.Break(); throw new ArgumentException("nodeA and nodeB cannot be null"); }
			//Debug.Print("A: " + nodeA.order + "_" + nodeA.name +"\t-- "+ "B: " + nodeB.order + "_" + nodeA.name);
			var par = nodeA.ParentNode;
			if (par is null) { throw new ArgumentException("The nodeA parent node was null"); }
			if (nodeA.ParentNode != par) { Debugger.Break(); throw new ArgumentException("The nodeA parent node must be the same as the nodeB parent node"); }
			if (nodeA == nodeB) { return 0; }//  Debugger.Break(); Debug.Print("  0"); return 0; }

			BaseType? node = nodeA;
			while (node is not null && node.ParentNode == par)
			{
				try { node = SdcUtil.ReflectNextSib(node); }
				catch { Debugger.Break(); throw; }
				//if (node is null) { return 1; Debugger.Break(); }
				if (node == nodeB)
				{
					//Debug.Print("  -1");
					//if (nodeB.order < nodeA.order) Debugger.Break();  //this is an error
					return -1;  //nodeA comes first
				}
			}
			//Debug.Print("  1");
			//if (nodeB.order > nodeA.order) Debugger.Break();  //this is an error
			return 1;
		}
	}
}
