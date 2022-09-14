using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
public static class StringExtensions
{
	public static int WordCount(this string str)
	{
		return str.Split(new char[] { ' ', '.', '?' },
						 StringSplitOptions.RemoveEmptyEntries).Length;
	}
	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}
	public static bool IsNullOrWhitespace(this string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}
	public static bool IsEmpty(this string str)
	{
		return str.IsNullOrEmpty() || str.IsNullOrWhitespace();
	}
	public static string TrimAndReduce(this string str)
	{
		return str.ReduceWhitespace().Trim();
	}

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
	public static string ReduceWhitespaceRegex(this string str)
	{
		return Regex.Replace(str, "[\n\r\t]", " ");
	}
	public static XmlElement ToXmlElement(this string rawXML)
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
