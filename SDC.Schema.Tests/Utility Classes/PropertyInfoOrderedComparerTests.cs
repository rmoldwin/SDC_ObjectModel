using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SDC.Schema.Tests.Utils
{
	[TestClass()]
	public class PropertyInfoOrderedComparerTests
	{
		[TestMethod()]
		public void CompareTest()
		{
			var comparer = new TreeComparer();
			var de = new DataElementType(null);
			var q1 = new QuestionItemType(de, "Q1");
			var q2 = new QuestionItemType(de, "Q2");
			var result = comparer.Compare(q1, q2);
			Assert.IsTrue(result < 0 || result > 0);
		}
	}
}
