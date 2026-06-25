// SDC-CUSTOM: do not overwrite
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema
{
    /// <summary>
    /// Internal factory for constructing, parsing, and validating SDC datatype nodes.
    /// Parse failures are recorded via <see cref="SdcUtil.RecordRejectedValue"/> and
    /// fired through <see cref="SdcValidationEvents"/>. Caller-supplied
    /// <see cref="IList{T}"/> errors are populated through an event-bridge subscription
    /// (see <see cref="SubscribeErrorCollector"/>).
    /// Public API: use extension methods in <c>IResponseFieldExtensions</c>
    /// (e.g. <c>rf.AddDataType(...)</c>) rather than calling this class directly.
    /// </summary>
    internal static class SdcDataTypeBuilder
    {
        /// <summary>
        /// Creates and attaches a <see cref="DataTypes_DEType"/> to the supplied
        /// <paramref name="rfParent"/>, with the appropriate concrete data-type child
        /// node selected by <paramref name="dataTypeEnum"/>.
        /// Parse failures are recorded on the node via <see cref="SdcUtil.RecordRejectedValue"/>
        /// and reported through <see cref="SdcValidationEvents"/>. When
        /// <paramref name="errors"/> is non-null a local subscription bridges those events
        /// into the list so no separate polling is required.
        /// </summary>
        internal static DataTypes_DEType AddDataTypesDE(
            ResponseFieldType rfParent,
            ItemChoiceType dataTypeEnum = ItemChoiceType.@string,
            dtQuantEnum quantifierEnum = dtQuantEnum.EQ,
            object? value = null,
            IList<Exception>? errors = null)
        {
            // Bridge: route every ValidationOccurred Error event into the caller's list.
            var bridge = SubscribeErrorCollector(errors);
            try
            {
                rfParent.Response ??= new DataTypes_DEType(rfParent);

                switch (dataTypeEnum)
                {
                    case ItemChoiceType.HTML:
                        {//TODO:
                            var dt = new HTML_DEtype(rfParent.Response);
                            if (value != null)
                            {   //check is value is valid HTML and assign value to dt
                                dt.Any = value as List<XmlElement> ?? new List<XmlElement>();
                            }
                            dt.AnyAttr = new List<XmlAttribute>();
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.XML: //TODO: Need to be able to add custom attributes to first wrapper element - see anyType; in fact, do we even need XML as a separate type?
                        {//TODO:
                            var dt = new XML_DEtype(rfParent.Response);
                            dt.Any = new List<XmlElement>();
                            //check is value is valid XML and assign value to dt
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.anyType:
                        {//TODO:
                            var dt = new anyType_DEtype(rfParent.Response);
                            dt.Any = new List<XmlElement>();
                            dt.AnyAttr = new List<XmlAttribute>();
                            //check is value is valid XML and assign value to dt
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.anyURI:
                        {
                            var dt = new anyURI_DEtype(rfParent.Response);
                            string? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                    if (Regex.Match(s, @"([#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF])+").Success) tmp = (string)value;
                                    else RecordAndRaise("Supplied value parameter was not in anyURI string format", dt, value, rfParent);
                                else RecordAndRaise("Supplied value parameter was not in anyURI string format", dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.base64Binary:
                        {
                            var dt = new base64Binary_DEtype(rfParent.Response);
                            byte[]? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    var s64 = new Span<byte>();
                                    if (Convert.TryFromBase64String(s, s64, out int bytesWritten)) tmp = s64.ToArray();
                                    else RecordAndRaise("Supplied value parameter could not be parsed as base64Binary (byte[]).  " +
                                        $"Bytes written = {bytesWritten}", dt, value, rfParent);
                                }
                                else if (value is byte[] bVal) tmp = bVal;
                                else RecordAndRaise("Supplied value parameter could not be parsed as base64Binary (byte[])", dt, value, rfParent);
                            }
                            if (tmp != null) dt.val = (byte[])tmp;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.boolean:
                        {
                            var dt = new boolean_DEtype(rfParent.Response);
                            bool? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (bool.TryParse(s, out bool sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as bool", dt, value, rfParent);
                                }
                                else if (value is bool v) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as bool", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != false) dt.val = (bool)tmp;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@byte: //XML signed "byte" is "sbyte" in .NET
                        {
                            var dt = new byte_DEtype(rfParent.Response);
                            sbyte? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (sbyte.TryParse(s, out sbyte sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as sbyte", dt, value, rfParent);
                                }
                                else if (value.TryAs(out sbyte v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as sbyte", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (sbyte)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.date:
                        {
                            var dt = new date_DEtype(rfParent.Response);
                            DateTime? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)   //Can decide here to support timezone or not in the DateTime field.
                                                         //Consider switch to DateTimeOffset.
                                {
                                    if (DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime sVal)
                                        && sVal != default)
                                    {
                                        if (Regex.Match(s, @"-?([1-9][0-9]{3,}|0[0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?") //date
                                            .Success) tmp = sVal;
                                        else RecordAndRaise(DescribeDateError(value, XsdDateKind.Date), dt, value, rfParent);
                                    }
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.Date), dt, value, rfParent);
                                }
                                else if (value is DateTime v) tmp = v;
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.Date), dt, value, rfParent);
                            }
                            if (tmp != null && tmp != default(DateTime)) dt.val = ((DateTime)tmp).Date;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@dateTime:
                        {
                            var dt = new dateTime_DEtype(rfParent.Response);
                            DateTime? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)   //Can decide here to support timezone or not in the DateTime field.
                                                         //Consider switch to DateTimeOffset.
                                {
                                    if (DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime sVal)
                                        && sVal != default
                                        && Regex.Match(s,
                                            @"-?([1-9][0-9]{3,}|0[0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])T(([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\.[0-9]+)?|(24:00:00(\.0+)?))(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?")
                                            .Success)
                                        tmp = sVal;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.DateTime), dt, value, rfParent);
                                }
                                else if (value is DateTime v) tmp = v;
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.DateTime), dt, value, rfParent);
                            }
                            if (tmp != null && tmp != default(DateTime)) dt.val = (DateTime)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@dateTimeStamp:  //Must be UTC.  All values will be converted to UTC.  If no timezone is supplied, local time will be converted to UTC.
                        {
                            var dt = new dateTimeStamp_DEtype(rfParent.Response);
                            DateTime? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                                         //Consider switch to DateTimeOffset.
                                {
                                    if (DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime sVal)
                                        && sVal != default
                                        && Regex.Match(s, //!timezone is required in this regex
                                            @"-?([1-9][0-9]{3,}|0[0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])T(([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\.[0-9]+)?|(24:00:00(\.0+)?))(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))")
                                            .Success)
                                        tmp = sVal;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.DateTimeStamp), dt, value, rfParent);
                                }
                                else if (value is DateTime v) tmp = v;
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.DateTimeStamp), dt, value, rfParent);
                            }
                            if (tmp != null && tmp != default(DateTime)) dt.val = (DateTime)tmp;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.dayTimeDuration:
                        {
                            var dt = new dayTimeDuration_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"-?([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+(\.[0-9]+)?S)?)?").Success) //dayTimeDuration
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.DayTimeDuration), dt, value, rfParent);
                                }
                                else if (value is TimeSpan ts)
                                    tmp = XmlConvert.ToString(ts); //ToDo: Need to modify the Year part (convert Years to hours [ts.totalHours] and add to hours part); e.g., P13DT10H57M18S
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.DayTimeDuration), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@decimal:
                        {
                            var dt = new decimal_DEtype(rfParent.Response);
                            decimal? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as decimal", dt, value, rfParent);
                                }
                                else if (value.TryAs(out decimal v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as decimal", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (decimal)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@double:
                        {
                            var dt = new double_DEtype(rfParent.Response);
                            double? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (double.TryParse(s, out double sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as double", dt, value, rfParent);
                                }
                                else if (value.TryAs(out double v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as double", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (double)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.duration:
                        {
                            var dt = new duration_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"-?P[0-9]+Y?([0-9]+M)?([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+(\.[0-9]+)?S)?)?").Success) //duration
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.Duration), dt, value, rfParent);
                                }
                                else if (value is TimeSpan ts && ts != default)
                                    tmp = XmlConvert.ToString(ts);
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.Duration), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@float:
                        {
                            var dt = new float_DEtype(rfParent.Response);
                            float? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (float.TryParse(s, out float sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as float", dt, value, rfParent);
                                }
                                else if (value.TryAs(out float v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as float", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (float)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.gDay: //# of day of month, +/- timezone
                        {
                            var dt = new gDay_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"---(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gDay;
                                        //ToDo: We'll probably want to trim the initial 3 dashes before using the regex on UI form fields, and restore them when storing in the SDC OM
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.GDay), dt, value, rfParent);
                                }
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.GDay), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.gMonth: //# of month, +/- timezone
                        {
                            var dt = new gMonth_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"--(0[1-9]|1[0-2])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gMonth
                                        //ToDo: We'll probably want to trim the initial 2 dashes before using the regex on UI form fields, and restore them when storing in the SDC OM
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.GMonth), dt, value, rfParent);
                                }
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.GMonth), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.gMonthDay: //# of day - # of month, +/- timezone
                        {
                            var dt = new gMonthDay_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"--(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gMonthDay
                                        //ToDo: We'll probably want to trim the initial 2 dashes before using the regex on UI form fields, and restore them when storing in the SDC OM
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.GMonthDay), dt, value, rfParent);
                                }
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.GMonthDay), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.gYear: //# of year +/- timezone
                        {
                            var dt = new gYear_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"-?([1-9][0-9]{3,}|0[0-9]{3})(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gYear
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.GYear), dt, value, rfParent);
                                }
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.GYear), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.gYearMonth:
                        {
                            var dt = new gYearMonth_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"-?([1-9][0-9]{3,}|0[0-9]{3})-(0[1-9]|1[0-2])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gYearMonth
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.GYearMonth), dt, value, rfParent);
                                }
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.GYearMonth), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.hexBinary:
                        {
                            var dt = new hexBinary_DEtype(rfParent.Response);
                            byte[]? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"([0-9a-fA-F]{2})*").Success)
                                        tmp = HexConversions.HexStringToByteArrayFastest(s);
                                    else RecordAndRaise("Supplied value parameter could not be parsed as byte[]", dt, value, rfParent);
                                }
                                else if (value is byte[] v) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as byte[]", dt, value, rfParent);
                            }
                            if (tmp != null) dt.val = tmp;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@int:
                        {
                            var dt = new int_DEtype(rfParent.Response);
                            int? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (int.TryParse(s, out int sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as int", dt, value, rfParent);
                                }
                                else if (value.TryAs(out int v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as int", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (int)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.integer: //XML integer is based on decimal datatype.  Decimal values after the "." will be truncated
                        {
                            var dt = new integer_DEtype(rfParent.Response);
                            decimal? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as integer", dt, value, rfParent);
                                }
                                else if (value.TryAs(out decimal v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as integer", dt, value, rfParent);
                            }
                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp2 != 0) dt.val = tmp2;
                            }
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@long:
                        {
                            var dt = new long_DEtype(rfParent.Response);
                            long? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (long.TryParse(s, out long sVal) && sVal != default) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as long", dt, value, rfParent);
                                }
                                else if (value.TryAs(out long v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as long", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (long)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.negativeInteger:
                        {
                            decimal? tmp = null; //start with a default value that is not zero
                            var dt = new negativeInteger_DEtype(rfParent.Response);
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as negativeInteger", dt, value, rfParent);
                                }
                                else if (value.TryAs(out decimal v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as negativeInteger", dt, value, rfParent);

                                if (tmp != null)
                                {
                                    decimal tmp2 = decimal.Truncate((decimal)tmp);
                                    if (tmp2 < 0) dt.val = tmp2;
                                }
                                else RecordAndRaise("Supplied value parameter could not be parsed as negativeInteger", dt, value, rfParent);
                            }
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.nonNegativeInteger:
                        {
                            decimal? tmp = null; //start with a default value that is not zero
                            var dt = new nonNegativeInteger_DEtype(rfParent.Response);
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as nonNegativeInteger", dt, value, rfParent);
                                }
                                else if (value.TryAs(out decimal v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as nonNegativeInteger", dt, value, rfParent);

                                if (tmp != null)
                                {
                                    decimal tmp2 = decimal.Truncate((decimal)tmp);
                                    if (tmp2 != default && tmp > 0) dt.val = tmp2;  //not using >=0, since 0 is the decimal default value
                                }
                                else RecordAndRaise("Supplied value parameter could not be parsed as nonnegativeInteger", dt, value, rfParent);
                            }
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.nonPositiveInteger:
                        {
                            decimal? tmp = null; //start with a default value that is not zero
                            var dt = new nonPositiveInteger_DEtype(rfParent.Response);
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as nonPositiveInteger", dt, value, rfParent);
                                }
                                else if (value.TryAs(out decimal v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as nonPositiveInteger", dt, value, rfParent);

                                if (tmp != null)
                                {
                                    decimal tmp2 = decimal.Truncate((decimal)tmp);
                                    if (tmp < 0) dt.val = tmp2;  //not using <=0, since 0 is the decimal default value
                                }
                                else RecordAndRaise("Supplied value parameter could not be parsed as nonPositiveInteger", dt, value, rfParent);
                            }
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.positiveInteger:
                        {
                            decimal? tmp = null; //start with a default value that is not zero
                            var dt = new positiveInteger_DEtype(rfParent.Response);
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as positiveInteger", dt, value, rfParent);
                                }
                                else if (value.TryAs(out decimal v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as positiveInteger", dt, value, rfParent);

                                if (tmp != null)
                                {
                                    decimal tmp2 = decimal.Truncate((decimal)tmp);
                                    if (tmp > 0) dt.val = tmp2;
                                }
                                else RecordAndRaise("Supplied value parameter could not be parsed as PositiveInteger", dt, value, rfParent);
                            }
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@short:
                        {
                            var dt = new short_DEtype(rfParent.Response);
                            short? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (short.TryParse(s, out short sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as short", dt, value, rfParent);
                                }
                                else if (value.TryAs(out short v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as short", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (short)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.@string:
                        {
                            var dt = new @string_DEtype(rfParent.Response);
                            if (value is not null && (value.ToString() is string s))
                            {
                                //Escape special characters (", <, &) in attributes: " &quot; < &lt; & &amp;
                                //https://stackoverflow.com/questions/1091945/what-characters-do-i-need-to-escape-in-xml-documents
                                //This might get slow if we're assigning/replacing a lot of strings this way
                                //ToDo: Convert Replace methods to Span<string> ? or look for occurrance before replacing; this is not easy to do...
                                //https://stackoverflow.com/questions/67387766/fastest-way-to-replace-occurences-in-small-string-using-span-in-c-sharp

                                s = s.Replace("\"", "&quot;")
                                    .Replace("<", "&lt;")
                                    .Replace("&", "&amp;");

                                dt.val = s;
                            }
                            else RecordAndRaise("Supplied value parameter was not a string datatype", dt, value, rfParent);

                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.time:
                        {
                            var dt = new time_DEtype(rfParent.Response);
                            DateTime? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)   //Can decide here to support timezone or not in the DateTime field.
                                                         //Consider switch to DateTimeOffset.
                                {
                                    if (DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime sVal)
                                        && sVal != default)
                                    {
                                        if (Regex.Match(s, @"(([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\.[0-9]+)?|(24:00:00(\.0+)?))(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?")
                                            .Success) tmp = sVal;
                                        else RecordAndRaise(DescribeDateError(value, XsdDateKind.Time), dt, value, rfParent);
                                    }
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.Time), dt, value, rfParent);
                                }
                                else if (value is DateTime v) tmp = v.ToLocalTime();
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.Time), dt, value, rfParent);
                            }
                            if (tmp != null && tmp != default(DateTime))
                            {
                                DateTime tmp2 = (DateTime)tmp;
                                if (tmp2.Kind == DateTimeKind.Local)
                                {
                                    dt.timeZone = TimeZoneInfo.Local.BaseUtcOffset.ToString();  //ToDo: I may want to convert timeZone to a TimeSpan datatype in the SDC Schema
                                    dt.val = tmp2.ToLocalTime();
                                }
                                else if (tmp2.Kind == DateTimeKind.Utc)
                                {
                                    dt.timeZone = TimeZoneInfo.Utc.BaseUtcOffset.ToString(); //00:00:00 or Z
                                    dt.val = tmp2.ToUniversalTime();
                                }
                                else //if (tmp2.Kind == DateTimeKind.Unspecified)
                                {
                                    dt.timeZone = null;
                                    dt.val = tmp2;
                                }
                            }
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.unsignedByte:
                        {
                            var dt = new unsignedByte_DEtype(rfParent.Response);
                            byte? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (byte.TryParse(s, out byte sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as unsigned byte", dt, value, rfParent);
                                }
                                else if (value.TryAs(out byte v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as unsigned byte", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (byte)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.unsignedInt:
                        {
                            var dt = new unsignedInt_DEtype(rfParent.Response);
                            uint? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (uint.TryParse(s, out uint sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as uint", dt, value, rfParent);
                                }
                                else if (value.TryAs(out uint v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as uint", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (uint)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.unsignedLong:
                        {
                            var dt = new unsignedLong_DEtype(rfParent.Response);
                            ulong? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (ulong.TryParse(s, out ulong sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as ulong", dt, value, rfParent);
                                }
                                else if (value.TryAs(out ulong v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as ulong", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (ulong)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.unsignedShort:
                        {
                            var dt = new unsignedShort_DEtype(rfParent.Response);
                            ushort? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (ushort.TryParse(s, out ushort sVal)) tmp = sVal;
                                    else RecordAndRaise("Supplied value parameter could not be parsed as ushort", dt, value, rfParent);
                                }
                                else if (value.TryAs(out ushort v, out _)) tmp = v;
                                else RecordAndRaise("Supplied value parameter could not be parsed as ushort", dt, value, rfParent);
                            }
                            if (tmp != null && tmp != 0) dt.val = (ushort)tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.yearMonthDuration:
                        {
                            var dt = new yearMonthDuration_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    if (Regex.Match(s, @"^-?P[0-9]+Y?([0-9]+M)?$").Success) //yearMonthDuration
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.YearMonthDuration), dt, value, rfParent);
                                }
                                else if (value is TimeSpan ts && ts != default)
                                    tmp = XmlConvert.ToString(ts); //ToDo: Need to truncate after hh, mm, ss via regex, e.g., P13DT10H57M18S
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.YearMonthDuration), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                }

                rfParent.Response.ItemElementName = (ItemChoiceType2)dataTypeEnum;
                return rfParent.Response;
            }
            finally
            {
                UnsubscribeErrorCollector(bridge, errors);
            }
        }

        internal static DataTypes_DEType AddHTML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!, List<XmlAttribute> valAtt = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new HTML_DEtype(rfParent.Response);
            dt.Any = valEl ?? new List<XmlElement>();
            dt.AnyAttr = valAtt ?? new List<XmlAttribute>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.HTML;
            return rfParent.Response;
        }

        internal static DataTypes_DEType AddXML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new XML_DEtype(rfParent.Response);
            dt.Any = valEl ?? new List<XmlElement>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.XML;
            return rfParent.Response;
        }

        internal static DataTypes_DEType AddAny_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!, List<XmlAttribute> valAtt = null!, string nameSpace = null!, string schema = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new anyType_DEtype(rfParent.Response);
            dt.@namespace = nameSpace ?? null;
            dt.schema = schema ?? null;
            dt.Any = valEl ?? new List<XmlElement>();
            dt.AnyAttr = valAtt ?? new List<XmlAttribute>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.anyType;
            return rfParent.Response;
        }

        internal static DataTypes_DEType AddBase64_DE(ResponseFieldType rfParent, byte[] value = null!, string mediaType = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new base64Binary_DEtype(rfParent.Response);
            dt.val = value;
            dt.mediaType = mediaType;
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.base64Binary;
            return rfParent.Response;
        }

        internal static dtQuantEnum AssignQuantifier(string quantifier)
        {
            var dtQE = new dtQuantEnum();

            switch (quantifier)
            {
                case "EQ":
                case "=":
                case "==":
                    dtQE = dtQuantEnum.EQ;
                    break;
                case "GT":
                case ">":
                    dtQE = dtQuantEnum.GT;
                    break;
                case "GTE":
                case ">=":
                    dtQE = dtQuantEnum.GTE;
                    break;
                case "LT":
                case "<":
                    dtQE = dtQuantEnum.LT;
                    break;
                case "LTE":
                case "<=":
                    dtQE = dtQuantEnum.LTE;
                    break;
                case "APPROX":
                case "~":
                case "@":
                    dtQE = dtQuantEnum.APPROX;
                    break;
                case "":
                    dtQE = dtQuantEnum.EQ;
                    break;
                case null:
                    dtQE = dtQuantEnum.EQ;
                    break;
                default:
                    dtQE = dtQuantEnum.EQ;
                    break;
            }
            return dtQE;
        }

        // Subscribes a temporary handler to ValidationOccurred so that every Error event
        // fired during AddDataTypesDE is also appended to the caller's IList<Exception>.
        // Returns the handler so the caller can unsubscribe it in a finally block.
        private static EventHandler<SdcValidationEventArgs> SubscribeErrorCollector(IList<Exception>? errors)
        {
            EventHandler<SdcValidationEventArgs> handler = (_, e) =>
            {
                if (e.Severity == SdcValidationSeverity.Error)
                    errors?.Add(new InvalidOperationException(e.Message));
            };
            if (errors != null)
                SdcValidationEvents.ValidationOccurred += handler;
            return handler;
        }

        // Removes the bridge handler subscribed by SubscribeErrorCollector.
        private static void UnsubscribeErrorCollector(EventHandler<SdcValidationEventArgs> handler, IList<Exception>? errors)
        {
            if (errors != null)
                SdcValidationEvents.ValidationOccurred -= handler;
        }

        // Centralized parse-failure path: records the rejected value on the dt node
        // (so it can be retrieved via SdcUtil.GetRejectedValues) and fires the central
        // validation event so subscribers (UI, logger, SubscribeErrorCollector bridge) see it.
        private static void RecordAndRaise(string message, BaseType dtNode, object? attemptedValue, ResponseFieldType rfParent)
        {
            SdcUtil.RecordRejectedValue(dtNode, new SdcRejectedValue
            {
                PropertyName   = "val",
                AttemptedValue = attemptedValue,
                Message        = message,
                RejectedAt     = DateTimeOffset.Now
            });
            if (!SdcUtil.IsDeserializing.Value)
                SdcValidationEvents.Raise(
                    message:      message,
                    nodeID:       rfParent?.ParentIETnode?.ID.ToString(),
                    propertyName: "val");
        }

        // Builds an XSD-accurate error message for a malformed date/date-part value.
        // Quotes the value, names the xs: type, gives the canonical form + example, and
        // pinpoints the violation. Falls back to a representability note for non-string values.
        private static string DescribeDateError(object? badValue, XsdDateKind kind)
        {
            if (badValue is string s)
                return XsdDateTimePatterns.BuildLexicalErrorMessage(kind, s);
            return $"A {XsdDateTimePatterns.XsdName(kind)} value could not be obtained from '{badValue ?? "null"}'"
                 + $" (type {badValue?.GetType().Name ?? "null"}).";
        }
    }
}
