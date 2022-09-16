using Newtonsoft.Json;
using System.Xml;
using System.Xml.Schema;



//using SDC;
namespace SDC.Schema
{
	public static class Validate
	{

		public static string ValidateSdcObjectTree(this ITopNodePublic itn)
		{
			//custom statements to enforce some things that the object model and/or XML Schema can't enforce by themselves.
			//complex nestings of choice and sequence
			//datatype metadata encoded in XML (i.e., no in the Schema per se)
			//references to element names inside of rules
			//uniqueness of BaseURI/ID pairs in FormDesign, DemogFormDesign, DataElement etc.
			//content consistency inside of SDCPackages

			throw new NotImplementedException();
		}
		public static string ValidateSdcXml(string xml, string sdcSchemaUri = null)
		{
			//https://docs.microsoft.com/en-us/dotnet/standard/data/xml/xmlschemaset-for-schema-compilation
			try
			{
				var sdcSchemas = new XmlSchemaSet();

				if (sdcSchemaUri is null)
				{
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCRetrieveForm.xsd"));

					//unclear if the following Schemas will be automatically discovered by the validator
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCFormDesign.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCMapping.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCBase.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCDataTypes.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCExpressions.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCResources.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCTemplateAdmin.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "xhtml.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "xml.xsd"));
					//Extras, not currently used.
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC_IDR.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCRetrieveFormComplex.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDCOverrides.xsd"));
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
				//TODO: Should create error list to deliver all messages to ValidationLastMessage
			}
			return ValidationLastMessage;

		}
		public static string ValidationLastMessage { get; private set; }
		public static string ValidateSdcJson(string json)
		{
			return ValidateSdcXml(GetXmlFromJson(json));
		}

		public static void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			switch (e.Severity)
			{
				case XmlSeverityType.Error:
					Console.WriteLine("Error: {0}", e.Message);
					break;
				case XmlSeverityType.Warning:
					Console.WriteLine("Warning {0}", e.Message);
					break;
			}
			ValidationLastMessage = e.Message;
			//Should create error list to deliver all messages to ValidationLastMessage
		}

		public static string GetXmlFromJson(string json)
		{
			var doc = JsonConvert.DeserializeXmlNode(json);
			return doc.OuterXml;
		}

	}
}
