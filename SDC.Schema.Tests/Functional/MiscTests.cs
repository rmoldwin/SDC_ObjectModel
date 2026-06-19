using CSharpVitamins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using SDC.Schema.UtilityClasses.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
//using SDC.Schema;

namespace SDC.Schema.Tests.Functional
{
	[TestClass]
	public class MiscTests
	{
		FormDesignType fd;

		public FormDesignType FD
		{
			get => fd;
			set => fd = value;
		}

		public MiscTests()
		{
			BaseType.ResetLastTopNode();
			string path = Path.Combine("..", "..", "..", "Test files", "BreastStagingTest.xml");
			fd = FormDesignType.DeserializeFromXmlPath(path);
		}

		[TestMethod]
		public void Fibonacci()
		{
			var serializer = new XmlSerializer(typeof(BaseType));

			(int curr, int prev) Fib(int i)
			{
				if (i == 0) return (1, 0);
				var (curr, prev) = Fib(i - 1);
				return (curr + prev, curr);
			}

			var a = Fib(9);
			var b = a.ToTuple();

			// Rationale: Fib(9) with this recurrence (start 1,0) produces the sequence 1,1,2,3,5,8,13,21,34,55.
			// At i=9: curr=55, prev=34.
			Assert.AreEqual(55, a.curr, "Fib(9).curr must equal 55.");
			Assert.AreEqual(34, a.prev, "Fib(9).prev must equal 34.");
			Assert.AreEqual(55, b.Item1, "ToTuple() Item1 must equal curr (55).");
			Assert.AreEqual(34, b.Item2, "ToTuple() Item2 must equal prev (34).");
		}

		[TestMethod]
		public void GetIetNodesTest()
		{
			Setup.TimerStart($"==>{Setup.CallerName()} Started");
			// Bug fix: use a per-test fresh object graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var FD = FormDesignType.DeserializeFromXml(Setup.GetXml());
			Debug.Print((FD.Nodes.Equals(FD.TopNode.Nodes)).ToString());
			foreach (BaseType n in FD.Nodes.Values)
			{
				Debug.Print("Node name: " + n?.name + "Node type: " + n?.GetType().Name + ", ParentIET: " + n?.ParentIETnode?.ID);
			}

			Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

		}
		[TestMethod]
		public void GetHtmlItems()
		{
			// Bug fix: use a per-test fresh object graph to avoid shared Setup.FD warm-state/order dependencies.
			BaseType.ResetLastTopNode();
			var fd = FormDesignType.DeserializeFromXml(Setup.GetXml());
			foreach (var iet in fd.IETnodes)
			{
				string? title;
				string itemType;
				string? pubOption = null; //0 = eCP only; 1 = ;  2 = print only; 3 = print/eCP
				string? units;
				bool mustImp;

				IdentifiedExtensionType? parIet = null;
				List<IdentifiedExtensionType>? childIetList = new();
				var sGuid = ShortGuid.NewGuid();
				sGuid = fd.GetQuestionByID("53309.100004300")?.sGuid;
				BaseType? n = fd.Nodes[sGuid.ToGuid()];


				switch (iet)
				{
					case QuestionItemType q:
						var qType = q.GetQuestionSubtype();

						if (qType == QuestionEnum.QuestionSingle)
						{
							itemType = "Q";
							title = q.title;
							pubOption = q.Property?.Find(p => p.propName == "pubOption")?.val;
							mustImp = q.mustImplement;
							parIet = q.ParentIETnode;
							childIetList = q.GetListItems()?.Cast<IdentifiedExtensionType>().ToList();
							childIetList = q.ListField_Item?.List?.Items?.Cast<IdentifiedExtensionType>().ToList(); ;
							childIetList = q.ChildItemsNode?.ChildItemsList;
							childIetList = q.GetListAndChildItemsList();
							units = q.ResponseField_Item?.ResponseUnits?.val;
							var childIetListRO = q.GetChildItemsList();



						}
						break;
					case SectionItemType s:

						break;
					case ListItemType li:

						break;
					case DisplayedType d:

						break;

				}
			}

			// Rationale: the deserialized form must have IET nodes and the known question ID must be locatable.
				Assert.IsTrue(fd.IETnodes.Count > 0, "Deserialized FormDesign must contain IET nodes.");
				// FRAGILITY NOTE: The assertion below is hard-coded to question ID "53309.100004300" which is
				// expected to exist in BreastStagingTest.xml.  If that ID is ever removed or renamed in the
				// fixture file, this test will fail.  At that point, update the ID to a stable question from
				// the current fixture rather than weakening the assertion to a conditional check.
				var knownQuestion = fd.GetQuestionByID("53309.100004300");
				Assert.IsNotNull(knownQuestion, "Question ID '53309.100004300' must exist in the BreastStagingTest form.");
		}
		public enum PreviewItemTypes
		{
			SectionHeader = 24,
			Notes = 12,
			QuestionSingleSelect = 4,
			QuestionMultiSelect = 23,
			QuestionFillin = 17,
			Answer = 6,
			AnswerFillin = 20
		}
		[TestMethod]
		public void Test()
		{
			var s = new Setup();
			var bstV1 = Setup.BreastStagingTestV1;
			var bstV2 = Setup.BreastStagingTestV2;

			// Rationale: Setup must load both XML versions without returning null.
			Assert.IsNotNull(bstV1, "Setup.BreastStagingTestV1 must be a non-null FormDesignType.");
			Assert.IsNotNull(bstV2, "Setup.BreastStagingTestV2 must be a non-null FormDesignType.");
			Assert.AreNotSame(bstV1, bstV2, "V1 and V2 must be distinct FormDesign instances.");
		}
	}
}