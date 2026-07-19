using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema;
using SDC.Schema.Extensions;
using System;
using System.Diagnostics;
using System.IO;

namespace SDC.Schema.Tests.Functional.Serialization
{
	/// <summary>
	/// Output-heavy diagnostic tests for <see cref="SdcSerializerJsonXml{T}"/> designed for
	/// manual side-by-side review of XML vs XML-isomorphic JSON output.  These tests print
	/// extensive structured output to the debug stream; they do fail on unexpected exceptions.
	/// </summary>
	[TestClass]
	public class SdcSerializerJsonXml_ForManualReview
	{
		private static readonly string AdrenalPartialPath =
			Path.Combine("..", "..", "..", "Test Files", "Adrenal_partial.xml");

		/// <summary>
		/// Deserializes <c>Adrenal_partial.xml</c> and prints both the normalized XML and
		/// XML-isomorphic JSON to <see cref="Debug"/> output so they can be read side-by-side.
		/// Run this test with "Show output from Tests" enabled in Test Explorer.
		/// </summary>
		[TestMethod]
		public void SideBySide_XmlAndJsonXml_AdrenalPartial()
		{
			try
			{
				BaseType.ResetLastTopNode();
				FormDesignType fd = TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(AdrenalPartialPath);

				string xml     = TopNodeSerializer<FormDesignType>.GetXml(fd, refreshSdc: true);
				string jsonXml = SdcSerializerJsonXml<FormDesignType>.SerializeJsonXml(fd);

				Debug.WriteLine("════════════════════════════════════════════════════════════════");
				Debug.WriteLine("  SDC XML — Adrenal_partial.xml");
				Debug.WriteLine("════════════════════════════════════════════════════════════════");
				Debug.WriteLine(xml);

				Debug.WriteLine("");
				Debug.WriteLine("════════════════════════════════════════════════════════════════");
				Debug.WriteLine("  XML-Isomorphic JSON (SdcSerializerJsonXml) — same document");
				Debug.WriteLine("════════════════════════════════════════════════════════════════");
				Debug.WriteLine(jsonXml);

				// Minimal sanity check — must not be empty
				Assert.IsFalse(string.IsNullOrWhiteSpace(xml),     "XML output must not be empty.");
				Assert.IsFalse(string.IsNullOrWhiteSpace(jsonXml), "JsonXml output must not be empty.");
			}
			catch (Exception ex)
			{
				Assert.Fail($"Unexpected exception in SideBySide test: {ex}");
			}
		}
	}
}
