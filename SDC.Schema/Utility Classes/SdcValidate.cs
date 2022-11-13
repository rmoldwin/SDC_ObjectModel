using Newtonsoft.Json;
using SDC.Schema.Extensions;
using System.Xml;
using System.Xml.Schema;



//using SDC;
namespace SDC.Schema
{
	public static class SdcValidate
	{

		/// <summary>
		/// List of all errors and warnings encounterd during XML validation.
		/// Callers are responsible for clearing the list after validation
		/// </summary>
		public static List<ValidationEventArgs> valEventList = new();

		/// <summary>
		/// Not yet implemented;
		/// </summary>
		/// <param name="itn"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public static string ValidateSdcObjectTree(this ITopNode itn)
		{
			//custom statements to enforce some things that the object model and/or XML Schema can't enforce by themselves.
			//complex nestings of choice and sequence
			//datatype metadata encoded in XML (i.e., no in the Schema per se)
			//references to element names inside of rules
			//uniqueness of BaseURI/ID pairs in FormDesign, DemogFormDesign, DataElement etc.
			//content consistency inside of SDCPackages
			//return ValidateSdcXml(itn.GetXml(true, SdcUtil.CreateCAPname));

			throw new NotImplementedException();
		}
		public static List<ValidationEventArgs> ValidateSdcXml(string xml, string sdcSchemaUri = null)
		{
			//https://docs.microsoft.com/en-us/dotnet/standard/data/xml/xmlschemaset-for-schema-compilation
			try
			{
				var sdcSchemas = new XmlSchemaSet();

				if (sdcSchemaUri is null)
				{
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCRetrieveForm.xsd"));

					//the following sub-Schemas are NOT automatically discovered by the validator; they are required here:
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCFormDesign.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCMapping.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCBase.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCDataTypes.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCExpressions.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCResources.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCTemplateAdmin.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "xhtml.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "xml.xsd"));
					//Extras, not currently used.  Adding them may duplicate some type definitions (defined above) and thus cause errors
					//sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC_IDR.xsd"));
					//sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCRetrieveFormComplex.xsd"));
					//sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCOverrides.xsd"));
					sdcSchemas.Compile();
				}
				ValidationLastMessage = "no error";
				var doc = new XmlDocument();
				doc.Schemas = sdcSchemas;
				doc.LoadXml(xml);
				doc.Validate(ValidationEventHandler);
			}
			catch (Exception ex)

			{
				Console.WriteLine(ex.Message);
				ValidationLastMessage = ex.Message;
				Console.WriteLine("Exception: " + ValidationLastMessage);
				//Validation will terminate after the exception
			}
			var copy = valEventList.ToList();
			valEventList.Clear();
			return copy;

		}
		internal static string ValidationLastMessage { get; private set; }


		public static List<ValidationEventArgs> ValidateSdcJson(this string json)
		{
			return ValidateSdcXml(GetXmlFromJson(json));
		}

		private static void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
				case XmlSeverityType.Error:
					Console.WriteLine($"Error: {e.Message}");
					Console.WriteLine(e.Exception.Data.ToString()+"\r\n");
					break;
				case XmlSeverityType.Warning:
					Console.WriteLine($"Warning {e.Message}");
					Console.WriteLine(e.Exception.Data.ToString() + "\r\n");
					break;
			}
			valEventList.Add(e);
			ValidationLastMessage = e.Message;
		}

		public static string GetXmlFromJson(string json)
		{
			var doc = JsonConvert.DeserializeXmlNode(json);
			return doc?.OuterXml??"";
		}

	}
}
