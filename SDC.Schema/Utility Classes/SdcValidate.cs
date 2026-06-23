using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDC.Schema.Extensions;
using System.Xml;
using System.Xml.Schema;



//using SDC;
namespace SDC.Schema
{
	/// <summary>
	/// Hand-written validation helpers that enforce rules the object model and XML Schema cannot
	/// express on their own.
	/// </summary>
	/// <remarks>
	/// <b>Numeric ResponseType range divergences (XSD vs .NET).</b> Numeric <c>val</c> and constraint
	/// facets are validated by the generated <c>[Range]</c>/<c>[MaxDigits]</c> attributes, not here, but
	/// the following known divergences affect what these validators can rely on. Full detail and the
	/// characterization tests live in <c>SDC.Schema.Tests/Documentation/NumericRange_XSD_vs_NET.md</c>:
	/// <list type="bullet">
	/// <item><description><b>A.</b> Integer-family <c>MaxDigitsAttribute(29)</c> counts the sign, so
	/// negatives are capped at 28 significant digits and positives at 29 (<c>decimal.MinValue</c>
	/// throws; <c>decimal.MaxValue</c> is accepted).</description></item>
	/// <item><description><b>B.</b> <c>long_DEtype</c> exclusive facets use the
	/// <c>RangeAttribute(double,double)</c> overload and cannot be enforced at <c>long.MaxValue</c>
	/// (double precision collapse).</description></item>
	/// <item><description><b>C.</b> Sign of zero (<c>−0</c>) is not preserved when assigned to a fresh
	/// float/double node (the setter's <c>Equals</c> change-guard treats <c>+0</c>/<c>−0</c> as
	/// equal).</description></item>
	/// <item><description><b>D.</b> JSON cannot round-trip large whole-number decimal/integer-family
	/// values beyond ulong range (deserialized as BigInteger → InvalidCastException). XML preserves
	/// them.</description></item>
	/// <item><description><b>E.</b> BSON cannot serialize <c>ulong</c> values above
	/// <c>long.MaxValue</c> (no unsigned support).</description></item>
	/// <item><description><b>F.</b> BSON loses precision on high-precision decimals (encoded as IEEE
	/// double).</description></item>
	/// </list>
	/// Integer-family and decimal types additionally narrow the unbounded XSD value spaces to the .NET
	/// <c>decimal</c> range (≈ ±7.92e28), which is the binding constraint.
	/// </remarks>
	public static class SdcValidate
	{

		/// <summary>
		/// List of all errors and warnings encounterd during XML validation.
		/// Callers are responsible for clearing the list after validation
		/// </summary>
		private static List<ValidationEventArgs> valEventList = new();

		/// <summary>
		/// Convert the SDC object tree to xml and validate the xml;
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int, int)"/>
		/// </summary>
		/// <param name="itn"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param>
		/// <param name="orderStart"></param>
		/// <param name="orderGap"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public static List<ValidationEventArgs> ValidateSdcObjectTree(this ITopNode itn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
		{
			//TODO:custom statements to enforce some things that the object model and/or XML Schema can't enforce by themselves.
			//complex nestings of choice and sequence
			//datatype metadata encoded in XML (i.e., no in the Schema per se)
			//references to element names inside of rules
			//uniqueness of BaseURI/ID pairs in FormDesign, DemogFormDesign, DataElement etc.
			//content consistency inside of SDCPackages
			//return ValidateSdcXml(itn.GetXml(true, SdcUtil.CreateCAPname));

			string xml;
			switch (itn)
			{
				case DemogFormDesignType df:
					xml = df.GetXml();
					break;
				case FormDesignType fd:
					xml = fd.GetXml();
					break;
				case RetrieveFormPackageType rfp:
					xml = rfp.GetXml();
					break;
				case DataElementType de:
					xml = de.GetXml();
					break;
				case PackageListType pl:
					xml = pl.GetXml();
					break;
				case MappingType mp:
					xml = mp.GetXml();
					break;
				default:
					throw new InvalidOperationException("Unknown object type supplied for ITopNode itn");
			}

			return ValidateSdcXml(xml);

		}
		/// <summary>
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="sdcSchemaUri"></param>
		/// <returns></returns>
		public static List<ValidationEventArgs> ValidateSdcXml(string xml, string sdcSchemaUri = null)
		{
			//https://docs.microsoft.com/en-us/dotnet/standard/data/xml/xmlschemaset-for-schema-compilation
			try
			{
				var sdcSchemas = new XmlSchemaSet();

				if (sdcSchemaUri is null)
				{
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCRetrieveForm.xsd"));

					//the following sub-Schemas are NOT automatically discovered by the validator; they are required here:
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCFormDesign.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCMapping.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCBase.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCDataTypes.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCExpressions.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCResources.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "SDCTemplateAdmin.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "xhtml.xsd"));
					sdcSchemas.Add(null, Path.Combine(Directory.GetCurrentDirectory(), "SDC.Schema", "SDC Schema Files", "xml.xsd"));
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
		private static string ValidationLastMessage { get; set; }

		private static void XmlValidater(string xml)
		{






		}
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
			if (string.IsNullOrWhiteSpace(json)) return string.Empty;

			try
			{
				var doc = JsonConvert.DeserializeXmlNode(json);
				if (doc is not null) return doc.OuterXml;
			}
			catch (JsonSerializationException)
			{
				// Fall back for JSON payloads with multiple root properties by wrapping in a synthetic root.
			}

			var token = JToken.Parse(json);
			var wrapped = token is JObject
				? JsonConvert.SerializeObject(new JObject { ["Root"] = token })
				: JsonConvert.SerializeObject(new JObject { ["Root"] = new JArray(token) });

			var wrappedDoc = JsonConvert.DeserializeXmlNode(wrapped);
			return wrappedDoc?.OuterXml ?? string.Empty;
		}

	}
}
