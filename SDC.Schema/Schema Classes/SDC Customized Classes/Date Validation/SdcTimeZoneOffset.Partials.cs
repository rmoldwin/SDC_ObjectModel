// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SDC.Schema
{
	public partial class date_Stype
	{
		[XmlIgnore]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(SdcTimeZoneOffsetConverter))]
		public virtual TimeSpan? TimeZoneOffset
		{
			get => XsdDateTimePatterns.TryParseOffset(timeZone, out var offset) ? offset : null;
			set
			{
				if (!value.HasValue)
				{
					timeZone = null;
					return;
				}

				timeZone = XsdDateTimePatterns.FormatOffset(value.Value);
			}
		}
	}

	public partial class dateTime_Stype
	{
		[XmlIgnore]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(SdcTimeZoneOffsetConverter))]
		public virtual TimeSpan? TimeZoneOffset
		{
			get => XsdDateTimePatterns.TryParseOffset(timeZone, out var offset) ? offset : null;
			set
			{
				if (!value.HasValue)
				{
					timeZone = null;
					return;
				}

				timeZone = XsdDateTimePatterns.FormatOffset(value.Value);
			}
		}
	}

	public partial class time_Stype
	{
		[XmlIgnore]
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(SdcTimeZoneOffsetConverter))]
		public virtual TimeSpan? TimeZoneOffset
		{
			get => XsdDateTimePatterns.TryParseOffset(timeZone, out var offset) ? offset : null;
			set
			{
				if (!value.HasValue)
				{
					timeZone = null;
					return;
				}

				timeZone = XsdDateTimePatterns.FormatOffset(value.Value);
			}
		}
	}
}
