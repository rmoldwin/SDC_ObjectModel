using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.IO;
using System.Linq;

namespace SDC.Schema.Tests.TopNode
{
	[TestClass()]
	public class FormDesignTypeTests
	{
		private static FormDesignType CreateFormDesign()
		{
			BaseType.ResetLastTopNode();
			return new FormDesignType(null, "FD.Test");
		}

		private static FormDesignType CreateFormDesignFromSampleXml()
		{
			BaseType.ResetLastTopNode();
			return FormDesignType.DeserializeFromXml(Setup.GetXml());
		}

		private static string GetTempFilePath(string extension)
			=> Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");

		[TestMethod()]
		public void FormDesignTypeTest()
		{
			var fd = CreateFormDesign();
			Assert.IsNotNull(fd);
			Assert.AreEqual("FD.Test", fd.ID);
		}

		[TestMethod()]
		public void ClearTest()
		{
			var fd = CreateFormDesign();
			fd.AddHeader();
			fd.AddBody();
			fd.AddFooter();
			fd.ResetRootNode();
			Assert.IsNull(fd.Header);
			Assert.IsNull(fd.Body);
			Assert.IsNull(fd.Footer);
		}

		[TestMethod()]
		public void AddBodyTest()
		{
			var fd = CreateFormDesign();
			var body = fd.AddBody();
			Assert.IsNotNull(body);
			Assert.AreSame(body, fd.Body);
		}

		[TestMethod()]
		public void AddFooterTest()
		{
			var fd = CreateFormDesign();
			var footer = fd.AddFooter();
			Assert.IsNotNull(footer);
			Assert.AreSame(footer, fd.Footer);
		}

		[TestMethod()]
		public void AddHeaderTest()
		{
			var fd = CreateFormDesign();
			var header = fd.AddHeader();
			Assert.IsNotNull(header);
			Assert.AreSame(header, fd.Header);
		}

		[TestMethod()]
		public void RemoveFooterTest()
		{
			var fd = CreateFormDesign();
			fd.AddFooter();
			fd.Footer = null;
			Assert.IsNull(fd.Footer);
		}

		[TestMethod()]
		public void RemoveHeaderTest()
		{
			var fd = CreateFormDesign();
			fd.AddHeader();
			fd.Header = null;
			Assert.IsNull(fd.Header);
		}

		[TestMethod()]
		public void RemoveBodyTest()
		{
			var fd = CreateFormDesign();
			fd.AddBody();
			fd.Body = null;
			Assert.IsNull(fd.Body);
		}

		[TestMethod()]
		public void AddRulesTest()
		{
			var fd = CreateFormDesign();
			var rules = fd.AddRule_();
			Assert.IsNotNull(rules);
			Assert.AreSame(rules, fd.Rules);
		}

		[TestMethod()]
		public void GetSortedNodesListTest()
		{
			var fd = CreateFormDesign();
			fd.AddBody().AddChildQuestion(QuestionEnum.QuestionSingle, "Q1", "Question 1", 0);
			var sorted = fd.GetSortedNodes();
			Assert.IsTrue(sorted.Count > 0);
		}

		[TestMethod()]
		public void GetSortedNodesObsColTest()
		{
			var fd = CreateFormDesign();
			fd.AddBody();
			var sorted = fd.GetSortedNodesObsCol();
			Assert.IsNotNull(sorted);
			Assert.IsTrue(sorted.Count > 0);
		}

		[TestMethod()]
		public void TreeLoadResetTest()
		{
			var fd = CreateFormDesign();
			fd.AddBody();
			fd.ResetRootNode();
			Assert.AreEqual(0, fd.Nodes.Count);
		}

		[TestMethod()]
		public void NodeFromIDTest()
		{
			var fd = CreateFormDesign();
			var q = fd.AddBody().AddChildQuestion(QuestionEnum.QuestionSingle, "Q.ID", "Question", 0);
			var hit = fd.GetIETnodeByID(q.ID);
			Assert.AreSame(q, hit);
		}

		[TestMethod()]
		public void NodeFromNameTest()
		{
			// Bug fix: use a per-test fresh object graph to avoid shared static fixture warm-state/order dependencies.
			var fd = CreateFormDesignFromSampleXml();
			var nodeWithName = fd.Nodes.Values.First(n => !string.IsNullOrWhiteSpace(n.name));
			var hit = fd.GetNodeByName(nodeWithName.name);
			Assert.AreSame(nodeWithName, hit);
		}

		[TestMethod()]
		public void NodeFromObjectGUIDTest()
		{
			var fd = CreateFormDesign();
			var q = fd.AddBody().AddChildQuestion(QuestionEnum.QuestionSingle, "Q.Guid", "Question", 0);
			var hit = fd.GetNodeByObjectGUID(q.ObjectGUID);
			Assert.AreSame(q, hit);
		}

		[TestMethod()]
		public void DeserializeFromXmlPathTest()
		{
			var path = Setup.GetXmlPath();
			var fd = FormDesignType.DeserializeFromXmlPath(path);
			Assert.IsNotNull(fd);
		}

		[TestMethod()]
		public void DeserializeFromXmlTest()
		{
			var xml = Setup.GetXml();
			var fd = FormDesignType.DeserializeFromXml(xml);
			Assert.IsNotNull(fd);
		}

		[TestMethod()]
		public void GetXmlTest()
		{
			// Bug fix: use a per-test fresh object graph to avoid shared static fixture warm-state/order dependencies.
			var fd = CreateFormDesignFromSampleXml();
			var xml = fd.GetXml();
			Assert.IsFalse(string.IsNullOrWhiteSpace(xml));
		}

		[TestMethod()]
		public void DeserializeFromJsonPathTest()
		{
			// Bug fix: validate direct JSON deserialization path after enabling non-public default constructor handling.
			var fd = CreateFormDesign();
			fd.AddBody();
			var path = GetTempFilePath(".json");
			try
			{
				fd.SaveJsonToFile(path);
				var loaded = FormDesignType.DeserializeFromJsonPath(path);
				Assert.IsNotNull(loaded);
			}
			finally
			{
				if (File.Exists(path)) File.Delete(path);
			}
		}

		[TestMethod()]
		public void DeserializeFromJsonTest()
		{
			// Bug fix: validate direct JSON deserialization path after enabling non-public default constructor handling.
			var source = CreateFormDesign();
			source.AddBody();
			var marker = source.AddProperty();
			marker.propName = "JsonCtorMarker";
			marker.val = "MarkerValue";
			var json = source.GetJson();
			var fd = FormDesignType.DeserializeFromJson(json);
			Assert.IsNotNull(fd);
			Assert.IsTrue(fd.Property?.Any(p => p.propName == "JsonCtorMarker" && p.val == "MarkerValue") == true);
		}

		[TestMethod()]
		public void GetJsonTest()
		{
			// Bug fix: use a per-test fresh object graph to avoid shared static fixture warm-state/order dependencies.
			var fd = CreateFormDesignFromSampleXml();
			var json = fd.GetJson();
			Assert.IsFalse(string.IsNullOrWhiteSpace(json));
		}

		[TestMethod()]
		public void DeserializeFromMsgPackPathTest()
		{
			// Bug fix: validate MsgPack API path after serializer fallback changed to XML-backed UTF8 bytes for XmlElement-safe round-trips.
			var fd = CreateFormDesign();
			fd.AddBody();
			var path = GetTempFilePath(".mpk");
			try
			{
				fd.SaveMsgPackToFile(path);
				var loaded = FormDesignType.DeserializeFromMsgPackPath(path);
				Assert.IsNotNull(loaded);
			}
			finally
			{
				if (File.Exists(path)) File.Delete(path);
			}
		}

		[TestMethod()]
		public void DeserializeFromMsgPackTest()
		{
			// Bug fix: validate MsgPack API path after serializer fallback changed to XML-backed UTF8 bytes for XmlElement-safe round-trips.
			var source = CreateFormDesign();
			source.AddBody();
			var marker = source.AddProperty();
			marker.propName = "MsgPackMarker";
			marker.val = "MarkerValue";
			var bytes = source.GetMsgPack();
			var fd = FormDesignType.DeserializeFromMsgPack(bytes);
			Assert.IsNotNull(fd);
			Assert.IsTrue(fd.Property?.Any(p => p.propName == "MsgPackMarker" && p.val == "MarkerValue") == true);
		}

		[TestMethod()]
		public void GetMsgPackTest()
		{
			// Bug fix: validate MsgPack byte generation after serializer fallback changed to XML-backed UTF8 bytes.
			var fd = CreateFormDesign();
			fd.AddBody();
			var bytes = fd.GetMsgPack();
			Assert.IsTrue(bytes.Length > 0);
		}

		[TestMethod()]
		public void SaveXmlToFileTest()
		{
			// Bug fix: use a per-test fresh object graph to avoid shared static fixture warm-state/order dependencies.
			var fd = CreateFormDesignFromSampleXml();
			var path = GetTempFilePath(".xml");
			try
			{
				fd.SaveXmlToFile(path);
				Assert.IsTrue(File.Exists(path));
			}
			finally
			{
				if (File.Exists(path)) File.Delete(path);
			}
		}

		[TestMethod()]
		public void SaveJsonToFileTest()
		{
			// Bug fix: use a per-test fresh object graph to avoid shared static fixture warm-state/order dependencies.
			var fd = CreateFormDesignFromSampleXml();
			var path = GetTempFilePath(".json");
			try
			{
				fd.SaveJsonToFile(path);
				Assert.IsTrue(File.Exists(path));
			}
			finally
			{
				if (File.Exists(path)) File.Delete(path);
			}
		}

		[TestMethod()]
		public void SaveMsgPackToFileTest()
		{
			// Bug fix: validate MsgPack file output after serializer fallback changed to XML-backed UTF8 bytes.
			var fd = CreateFormDesign();
			fd.AddBody();
			var path = GetTempFilePath(".mpk");
			try
			{
				fd.SaveMsgPackToFile(path);
				Assert.IsTrue(File.Exists(path));
			}
			finally
			{
				if (File.Exists(path)) File.Delete(path);
			}
		}
	}
}
