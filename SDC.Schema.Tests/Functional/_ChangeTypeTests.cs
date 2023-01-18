using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class _ChangeTypeTests
	{

		[TestMethod]
		public void LItoDI()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void DItoLI()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void DItoQ()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
		[TestMethod]
		public void DItoS()
		{
			Setup.TimerStart("==>[] Started");

			Setup.TimerPrintSeconds("  seconds: ", "\r\n<==[] Complete");
		}
	}
}