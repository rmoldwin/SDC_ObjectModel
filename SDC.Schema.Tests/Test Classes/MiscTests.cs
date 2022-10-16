using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using System.Diagnostics;
//using SDC.Schema;

namespace SDCObjectModelTests.TestClasses
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

    }
}