using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDC.Schema;

namespace SDC.Schema.Tests.Utils.Tests
{
	[TestClass()]
	public class _CompareTreesTests
	{
		[TestMethod()]
		public void _CompareTreesTest()
		{
			string xNew = "";
			string xOld = "";

			var tNew = FormDesignType.DeserializeFromXml(xNew);
			var tOld = FormDesignType.DeserializeFromXml(xOld);
			var comp = new CompareTrees<FormDesignType>(tOld, tNew);

			var sGuid_q1 = "";
			DifNodeIET? q1 = comp.GetIETattributes(sGuid_q1);
			//var x = q1.IsMoved;


		}

		[TestMethod()]
		public void _CompareTreesTest1()
		{

		}

		[TestMethod()]
		public void _ChangePrevVersionTest()
		{

		}

		[TestMethod()]
		public void _ChangeNewVersionTest()
		{

		}

		[TestMethod()]
		public void _GetSerializedXmlAttributesFromTreeTest()
		{

		}

		[TestMethod()]
		public void _GetIETattributesTest()
		{

		}

		[TestMethod()]
		public void _GetIETattributesTest1()
		{

		}
	}
}