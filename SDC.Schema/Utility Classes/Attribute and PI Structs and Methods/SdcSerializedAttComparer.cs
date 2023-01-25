using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
	/// <summary>
	/// Compare 2 AttributeInfo structs based on sGuid and the XML attribute Name.
	/// </summary>
	public class SdcSerializedAttComparer : EqualityComparer<SdcSerializedAtt>
	{
		/// <inheritdoc/>
		public override bool Equals(SdcSerializedAtt v1, SdcSerializedAtt v2)
		{
			if (v1.sGuid == v2.sGuid)
				if (v1.ai.Name == v2.ai.Name) return true;
			return false;
		}

		/// <inheritdoc/>
		public override int GetHashCode([DisallowNull] SdcSerializedAtt ssa)
		{
			return (ssa.sGuid + ";" + ssa.ai.Name).GetHashCode();
		}
	}
	public readonly record struct SdcSerializedAtt(string sGuid, AttributeInfo ai);
}
