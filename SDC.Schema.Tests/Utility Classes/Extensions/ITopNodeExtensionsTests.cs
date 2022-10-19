using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using SDCObjectModelTests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema.Tests
{
	[TestClass()]
	public class ITopNodeExtensionsTests
	{
		[TestMethod()]
		public void RefreshTreeTest()
		{

		}

		[TestMethod()]
		public void AssignElementNamesByReflectionTest()
		{

		}

		[TestMethod()]
		public void U_AssignElementNamesFromXmlDocTest()
		{

		}

		[TestMethod()]
		public void GetSortedNodesTest()
		{

		}

		[TestMethod()]
		public void GetSortedNodesObsColTest()
		{

		}

		[TestMethod()]
		public void TryGetIetNodeByIDTest()
		{

		}

		[TestMethod()]
		public void TryGetNodeByNameTest()
		{

		}

		[TestMethod()]
		public void TryGetNodeByShortGuidTest()
		{

		}

		[TestMethod()]
		public void TryGetNodeByIndexTest()
		{

		}

		[TestMethod()]
		public void TryGetNodeByObjectIDTest()
		{

		}

		[TestMethod()]
		public void GetIetNodeByIDTest()
		{

		}

		[TestMethod()]
		public void GetNodeByNameTest()
		{

		}

		[TestMethod()]
		public void GetNodeByShortGuidTest()
		{

		}

		[TestMethod()]
		public void GetNodeByObjectGUIDTest()
		{

		}

		[TestMethod()]
		public void GetNodeByPositionIndexTest()
		{

		}

		[TestMethod()]
		public void GetNodeByObjectIDTest()
		{

		}

		[TestMethod()]
		public void ResetSdcImportTest()
		{

		}

		[TestMethod()]
		public void GetXmlAttributesFilledTest()
		{		
			var fd = Setup.FD;
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			var atts = fd.GetXmlAttributesFilled(log: out string log, doLog: false);

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
			Debug.Print(log);
		}

		[TestMethod()]
		public void GetDescendantDictionaryTest()
		{

		}

		[TestMethod()]
		public void GetDescendantListTest()
		{

		}

		[TestMethod()]
		public void GetItemByIDTest()
		{

		}

		[TestMethod()]
		public void GetItemByNameTest()
		{

		}

		[TestMethod()]
		public void GetQuestionByIDTest()
		{

		}

		[TestMethod()]
		public void GetQuestionByNameTest()
		{

		}

		[TestMethod()]
		public void GetDisplayedTypeByIDTest()
		{

		}

		[TestMethod()]
		public void GetDisplayedTypeByNameTest()
		{

		}

		[TestMethod()]
		public void GetSectionByIDTest()
		{

		}

		[TestMethod()]
		public void GetSectionByNameTest()
		{

		}

		[TestMethod()]
		public void GetListItemByIDTest()
		{

		}

		[TestMethod()]
		public void GetListItemByNameTest()
		{

		}

		[TestMethod()]
		public void GetButtonByIDTest()
		{

		}

		[TestMethod()]
		public void GetButtonByNameTest()
		{

		}

		[TestMethod()]
		public void GetInjectFormByIDTest()
		{

		}

		[TestMethod()]
		public void GetInjectFormByNameTest()
		{

		}

		[TestMethod()]
		public void GetResponseFieldByNameTest()
		{

		}

		[TestMethod()]
		public void GetPropertyByNameTest()
		{

		}

		[TestMethod()]
		public void GetExtensionByNameTest()
		{

		}

		[TestMethod()]
		public void GetCommentByNameTest()
		{

		}

		[TestMethod()]
		public void GetContactByNameTest()
		{

		}

		[TestMethod()]
		public void GetLinkByNameTest()
		{

		}

		[TestMethod()]
		public void GetBlobByNameTest()
		{

		}

		[TestMethod()]
		public void GetCodedValueByNameTest()
		{

		}
	}
}