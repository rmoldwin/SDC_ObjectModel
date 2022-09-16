using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
	/// <summary>
	/// Use the BaseType.order property to sort SDC BaseType subclass objects
	/// </summary>
	public class OrderComparer : Comparer<BaseType>
	{
		/// <inheritdoc/>
		public override int Compare(BaseType? nodeA, BaseType? nodeB)
		{
			if (nodeA == null)
				throw new ArgumentNullException(nameof(nodeA), "The parameter cannot be null");
			if (nodeB == null)
				throw new ArgumentNullException(nameof(nodeB), "The parameter cannot be null");


			if (nodeA.order == nodeB.order) return 0;
			if (nodeA.order < nodeB.order) return -1;
			return 1;			
		}

	}
}
