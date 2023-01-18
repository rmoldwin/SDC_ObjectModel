using CSharpVitamins;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
	/// <summary>
	/// Use the BaseType.order property to sort SDC BaseType subclass objects
	/// </summary>
	public class SDCsGuidEqualityComparer<T> : EqualityComparer<T> where T: BaseType
	{
		/// <inheritdoc/>
		public override bool Equals(T? x, T y)
		{
			if(x?.sGuid == y?.sGuid) return true;
			return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode([DisallowNull] T iet)
		{
			return iet.sGuid.GetHashCode();
		}
	}
}