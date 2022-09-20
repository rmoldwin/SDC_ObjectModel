using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
public static class StringExtensions
{
	/// <summary>
	/// Count the number of "words" in the input string by splitting the string at space, comma, period and question mark.
	/// </summary>
	/// <param name="str"></param>
	/// <returns>integer containing the number of words in the input string</returns>
	public static int WordCount(this string str)
	{
		return str.Split(new char[] { ' ', '.', '?' },
						 StringSplitOptions.RemoveEmptyEntries).Length;
	}
	/// <summary>
	/// Determines if input string is null or an empty string("").
	/// </summary>
	/// <param name="str">Input string</param>
	/// <returns>true or false</returns>
	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}
	/// <summary>
	/// Indicates whether a string is null, empty or consists only of whitespace characters
	/// </summary>	
	/// <param name="str">Input string</param>
	public static bool IsNullOrWhitespace(this string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	/// <summary>
	/// Trim leading and trailing whitespace, and remove redundant internal spaces.
	/// </summary>
	/// <param name="str">Input string</param>
	/// <returns>Trimmed string with redundant internal spaces removed</returns>
	public static string TrimAndReduce(this string str)
	{
		return str.ReduceWhitespace().Trim();
	}

	/// <summary>
	/// Remove redundant internal spaces.
	/// </summary>
	/// <param name="str">Input string</param>
	/// <returns>String with duplicate spaces removed</returns>
	public static string ReduceWhitespace(this string str)
	{
		return Regex.Replace(str, @"\s+", " ");
	}
	/// <summary>
	/// Removes all spaces, line feeds, carriage returns and tabs
	/// </summary>
	/// <param name="str">Input string</param>
	/// <returns>string with whitespace removed</returns>
	public static string RemoveWhitespace(this string str)
	{
		return Regex.Replace(str, "[ \n\r\t]", "");
	}
	/// <summary>
	/// Removes all line feeds, carriage returns and tabs.  Preserves spaces.
	/// </summary>
	/// <param name="str">Input string</param>
	/// <returns>String with line feeds, carriage returns and tabs removed</returns>
	public static string ReduceWhitespaceRegex(this string str)
	{
		return Regex.Replace(str, "[\n\r\t]", " ");
	}
	/// <summary>
	/// Return an XmlElement from input rawXML, which must be properly formatted XML.
	/// </summary>
	/// <param name="rawXML"></param>
	/// <returns>XmlElement or null</returns>
	public static XmlElement? ToXmlElement(this string rawXML)
	{
		var xe = XElement.Parse(rawXML, LoadOptions.PreserveWhitespace);
		var doc = new XmlDocument();

		var xmlReader = xe.CreateReader();
		doc.Load(xmlReader);
		xmlReader.Dispose();

		return doc.DocumentElement;
	}

	/// <summary>
	/// Converts the string expression of an enum value to the desired type. Example: var qType= reeBuilder.ConvertStringToEnum&lt;ItemType&gt;("answer");
	/// </summary>
	/// <typeparam name="Tenum">The enum type that the inputString will be converted into.</typeparam>
	/// <param name="inputString">The string that must represent one of the Tenum enumerated values; not case sensitive</param>
	/// <returns></returns>
	public static Tenum ToEnum<Tenum>(this string inputString) where Tenum : struct
	{
		//T newEnum = (T)Enum.Parse(typeof(T), inputString, true);

		Tenum newEnum;
		if (Enum.TryParse(inputString, true, out newEnum))
		{
			return newEnum;
		}
		else
		{ //throw new Exception("Failure to create enum");

		}
		return newEnum;
	}

}
