using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Collections.Generic;
using CSharpVitamins;
using SDC.Schema.Extensions;
using SDC.Schema.UtilityClasses.Extensions;
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
			BaseType.ResetRootNode();
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
        }

        [TestMethod]
        public void GetIetNodesTest()
        {
            Setup.TimerStart($"==>{Setup.CallerName()} Started");
            var FD = Setup.FD;
            Debug.Print((FD.Nodes.Equals(FD.TopNode.Nodes)).ToString());
            foreach (BaseType n in FD.Nodes.Values)
            {
                Debug.Print("Node name: " +n?.name + "Node type: " + n?.GetType().Name + ", ParentIET: " + n?.ParentIETypeNode?.ID);
            }

            Setup.TimerPrintSeconds("  seconds: ", $"\r\n<=={Setup.CallerName()} Complete");

        }
        [TestMethod]
        public void GetHtmlItems()
        {
			var fd = Setup.FD;
            foreach(var iet in fd.IETnodes)
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

                        if(qType == QuestionEnum.QuestionSingle)
                        {
                            itemType = "Q";
                            title = q.title;
                            pubOption = q.Property?.Find(p => p.propName == "pubOption")?.val;
                            mustImp = q.mustImplement;
                            parIet = q.ParentIETypeNode;
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
	}
}