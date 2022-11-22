using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CSharpVitamins;

namespace SDC.Schema.UtilityClasses.Extensions
{
	public static class ShortGuidExtensions
	{
		/// <summary>
		/// Converts a Guid to an equivalent CSharpVitamins.ShortGuid string.
		/// </summary>
		/// <param name="g"></param>
		/// <returns></returns>
		public static string ToShortGuidString(this Guid g)
		{
			return CSharpVitamins.ShortGuid.Encode(g);
		}

		/// <summary>
		/// Convert ShortGuid struct to a Guid
		/// </summary>
		/// <param name="sg"></param>
		/// <returns>Guid</returns>
		public static Guid ToGuid(this CSharpVitamins.ShortGuid sg)
		{
			return CSharpVitamins.ShortGuid.Decode(sg);
		}
	}
}
