using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class _ChangeTypeTests
	{

		[TestMethod]
		public void ListItemToDisplayedItem()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void DisplayedItemToListItem()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void DisplayedItemToQuestion()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void DisplayedItemToSection()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
	}
}