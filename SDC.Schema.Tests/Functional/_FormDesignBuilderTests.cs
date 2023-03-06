using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.Tests.Utils.Extensions;
using System;
using System.Linq;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class _FormDesignBuilderTests
	{
		[TestMethod]
		public void AssignXmlNames()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			if(Setup.FD is null ) Setup.Reset();
			Setup.FD.AssignElementNamesByReflection();
			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AssignNamesFromXmlDoc()
		{
			return; //no longer needed
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			//This method is now private:
			//Setup.FD.X_AssignElementNamesFromXmlDoc(Setup.GetXml());

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddRemoveHeader()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddRemoveFooter()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddQuestions()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddListItemToQuestionList()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddListItemOnListItem()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AdListItemOnDisplayedItem()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddDisplayedItemToQuestionList()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddDisplayedItemOnListItem()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AdDisplayedItemOnDisplayedItem()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddDisplayedItemAsChild()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddQuestionAsChild()
		{ //to LI, DI in List, DI, S, Q
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}
		[TestMethod]
		public void AddSectionAsChild()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void AddProperties()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");
		}

		[TestMethod]
		public void Misc()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

		}
	}
}