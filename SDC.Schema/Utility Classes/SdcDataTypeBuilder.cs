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
    /// All construction paths converge here. Public API: use extension methods in
    /// <c>IResponseFieldExtensions</c> (e.g. <c>rf.AddDataType(...)</c>) rather than
    /// calling this class directly.
    /// </summary>
    internal static class SdcDataTypeBuilder
    {
        /// <summary>
        /// Creates and attaches a <see cref="DataTypes_DEType"/> to the supplied
        /// <paramref name="rfParent"/>, with the appropriate concrete data-type child
        /// node selected by <paramref name="dataTypeEnum"/>.
        /// Any parse failures are recorded in <paramref name="errors"/> (if supplied)
        /// and fired through <see cref="SdcValidationEvents"/>.
        /// </summary>
        internal static DataTypes_DEType AddDataTypesDE(
            ResponseFieldType rfParent,
            ItemChoiceType dataTypeEnum = ItemChoiceType.@string,
            dtQuantEnum quantifierEnum = dtQuantEnum.EQ,
            object? value = null,
            IList<Exception>? errors = null)
        {
            List<Exception> exList = new();

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
                        string? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                                if (Regex.Match(s, @"([#x1-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF])+").Success) tmp = (string)value;
                                else StoreError("Supplied value parameter was not in anyURI string format", rfParent, exList);
                            else StoreError("Supplied value parameter was not in anyURI string format", rfParent, exList);
                        }
                        var dt = new anyURI_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.base64Binary:
                    {
                        byte[]? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                var s64 = new Span<byte>();
                                if (Convert.TryFromBase64String(s, s64, out int bytesWritten)) tmp = s64.ToArray();
                                else StoreError("Supplied value parameter could not be parsed as base64Binary (byte[]).  " +
                                    $"Bytes written = {bytesWritten}", rfParent, exList);
                            }
                            else if (value is byte[] bVal) tmp = bVal;
                            else StoreError("Supplied value parameter could not be parsed as base64Binary (byte[])", rfParent, exList);
                        }
                        var dt = new base64Binary_DEtype(rfParent.Response);
                        if (tmp != null) dt.val = (byte[])tmp;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.boolean:
                    {
                        bool? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (bool.TryParse(s, out bool sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as bool", rfParent, exList);
                            }
                            else if (value is bool v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as bool", rfParent, exList);
                        }
                        var dt = new boolean_DEtype(rfParent.Response);
                        if (tmp != null && tmp != false) dt.val = (bool)tmp;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@byte: //XML signed "byte" is "sbyte" in .NET
                    {
                        sbyte? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (sbyte.TryParse(s, out sbyte sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as sbyte", rfParent, exList);
                            }
                            else if (value.TryAs(out sbyte v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as sbyte", rfParent, exList);
                        }
                        var dt = new byte_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (sbyte)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.date:
                    {
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
                                    else StoreError(DescribeDateError(value, XsdDateKind.Date), rfParent, exList);
                                }
                                else StoreError(DescribeDateError(value, XsdDateKind.Date), rfParent, exList);
                            }
                            else if (value is DateTime v) tmp = v;
                            else StoreError(DescribeDateError(value, XsdDateKind.Date), rfParent, exList);
                        }
                        var dt = new date_DEtype(rfParent.Response);
                        if (tmp != null && tmp != default(DateTime)) dt.val = ((DateTime)tmp).Date;

                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@dateTime:
                    {
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
                                else StoreError(DescribeDateError(value, XsdDateKind.DateTime), rfParent, exList);
                            }
                            else if (value is DateTime v) tmp = v;
                            else StoreError(DescribeDateError(value, XsdDateKind.DateTime), rfParent, exList);
                        }
                        var dt = new dateTime_DEtype(rfParent.Response);
                        if (tmp != null && tmp != default(DateTime)) dt.val = (DateTime)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@dateTimeStamp:  //Must be UTC.  All values will be converted to UTC.  If no timezone is supplied, local time will be converted to UTC.
                    {
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
                                else StoreError(DescribeDateError(value, XsdDateKind.DateTimeStamp), rfParent, exList);
                            }
                            else if (value is DateTime v) tmp = v;
                            else StoreError(DescribeDateError(value, XsdDateKind.DateTimeStamp), rfParent, exList);
                        }
                        var dt = new dateTimeStamp_DEtype(rfParent.Response);
                        if (tmp != null && tmp != default(DateTime)) dt.val = (DateTime)tmp;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.dayTimeDuration:
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"-?([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+(\.[0-9]+)?S)?)?").Success) //dayTimeDuration
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.DayTimeDuration), rfParent, exList);
                            }
                            else if (value is TimeSpan ts)
                                tmp = XmlConvert.ToString(ts); //ToDo: Need to modify the Year part (convert Years to hours [ts.totalHours] and add to hours part); e.g., P13DT10H57M18S
                            else StoreError(DescribeDateError(value, XsdDateKind.DayTimeDuration), rfParent, exList);
                        }
                        var dt = new dayTimeDuration_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@decimal:
                    {
                        decimal? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as decimal", rfParent, exList);
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as decimal", rfParent, exList);
                        }
                        var dt = new decimal_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (decimal)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@double:
                    {
                        double? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (double.TryParse(s, out double sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as double", rfParent, exList);
                            }
                            else if (value.TryAs(out double v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as double", rfParent, exList);
                        }
                        var dt = new double_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (double)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.duration:
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"-?P[0-9]+Y?([0-9]+M)?([0-9]+D)?(T([0-9]+H)?([0-9]+M)?([0-9]+(\.[0-9]+)?S)?)?").Success) //duration
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.Duration), rfParent, exList);
                            }
                            else if (value is TimeSpan ts && ts != default)
                                tmp = XmlConvert.ToString(ts);
                            else StoreError(DescribeDateError(value, XsdDateKind.Duration), rfParent, exList);
                        }
                        var dt = new duration_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@float:
                    {
                        float? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (float.TryParse(s, out float sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as float", rfParent, exList);
                            }
                            else if (value.TryAs(out float v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as float", rfParent, exList);
                        }
                        var dt = new float_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (float)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.gDay: //# of day of month, +/- timezone
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"---(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gDay;
                                    //ToDo: We'll probably want to trim the initial 3 dashes before using the regex on UI form fields, and restore them when storing in the SDC OM
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.GDay), rfParent, exList);
                            }
                            else StoreError(DescribeDateError(value, XsdDateKind.GDay), rfParent, exList);
                        }
                        var dt = new gDay_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.gMonth: //# of month, +/- timezone
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"--(0[1-9]|1[0-2])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gMonth
                                    //ToDo: We'll probably want to trim the initial 2 dashes before using the regex on UI form fields, and restore them when storing in the SDC OM
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.GMonth), rfParent, exList);
                            }
                            else StoreError(DescribeDateError(value, XsdDateKind.GMonth), rfParent, exList);
                        }
                        var dt = new gMonth_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.gMonthDay: //# of day - # of month, +/- timezone
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"--(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gMonthDay
                                    //ToDo: We'll probably want to trim the initial 2 dashes before using the regex on UI form fields, and restore them when storing in the SDC OM
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.GMonthDay), rfParent, exList);
                            }
                            else StoreError(DescribeDateError(value, XsdDateKind.GMonthDay), rfParent, exList);
                        }
                        var dt = new gMonthDay_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.gYear: //# of year +/- timezone
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"-?([1-9][0-9]{3,}|0[0-9]{3})(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gYear
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.GYear), rfParent, exList);
                            }
                            else StoreError(DescribeDateError(value, XsdDateKind.GYear), rfParent, exList);
                        }
                        var dt = new gYear_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.gYearMonth:
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"-?([1-9][0-9]{3,}|0[0-9]{3})-(0[1-9]|1[0-2])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?").Success) //gYearMonth
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.GYearMonth), rfParent, exList);
                            }
                            else StoreError(DescribeDateError(value, XsdDateKind.GYearMonth), rfParent, exList);
                        }
                        var dt = new gYearMonth_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.hexBinary:
                    {
                        byte[]? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"([0-9a-fA-F]{2})*").Success)
                                    tmp = HexConversions.HexStringToByteArrayFastest(s);
                                else StoreError("Supplied value parameter could not be parsed as byte[]", rfParent, exList);
                            }
                            else if (value is byte[] v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as byte[]", rfParent, exList);
                        }
                        var dt = new hexBinary_DEtype(rfParent.Response);
                        if (tmp != null) dt.val = tmp;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@int:
                    {
                        int? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (int.TryParse(s, out int sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as int", rfParent, exList);
                            }
                            else if (value.TryAs(out int v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as int", rfParent, exList);
                        }
                        var dt = new int_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (int)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.integer: //XML integer is based on decimal datatype.  Decimal values after the "." will be truncated
                    {
                        decimal? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (decimal.TryParse(s, out decimal sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as integer", rfParent, exList);
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as integer", rfParent, exList);
                        }
                        var dt = new integer_DEtype(rfParent.Response);
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
                        long? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (long.TryParse(s, out long sVal) && sVal != default) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as long", rfParent, exList);
                            }
                            else if (value.TryAs(out long v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as long", rfParent, exList);
                        }
                        var dt = new long_DEtype(rfParent.Response);
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
                                else StoreError("Supplied value parameter could not be parsed as negativeInteger", rfParent, exList);
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as negativeInteger", rfParent, exList);

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp2 < 0) dt.val = tmp2;
                            }
                            else StoreError("Supplied value parameter could not be parsed as negativeInteger", rfParent, exList);
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
                                else StoreError("Supplied value parameter could not be parsed as nonNegativeInteger", rfParent, exList);
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as nonNegativeInteger", rfParent, exList);

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp2 != default && tmp > 0) dt.val = tmp2;  //not using >=0, since 0 is the decimal default value
                            }
                            else StoreError("Supplied value parameter could not be parsed as nonnegativeInteger", rfParent, exList);
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
                                else StoreError("Supplied value parameter could not be parsed as nonPositiveInteger", rfParent, exList);
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as nonPositiveInteger", rfParent, exList);

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp < 0) dt.val = tmp2;  //not using <=0, since 0 is the decimal default value
                            }
                            else StoreError("Supplied value parameter could not be parsed as nonPositiveInteger", rfParent, exList);
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
                                else StoreError("Supplied value parameter could not be parsed as positiveInteger", rfParent, exList);
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as positiveInteger", rfParent, exList);

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp > 0) dt.val = tmp2;
                            }
                            else StoreError("Supplied value parameter could not be parsed as PositiveInteger", rfParent, exList);
                        }
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.@short:
                    {
                        short? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (short.TryParse(s, out short sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as short", rfParent, exList);
                            }
                            else if (value.TryAs(out short v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as short", rfParent, exList);
                        }
                        var dt = new short_DEtype(rfParent.Response);
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
                        else StoreError("Supplied value parameter was not a string datatype", rfParent, exList);

                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.time:
                    {
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
                                    else StoreError(DescribeDateError(value, XsdDateKind.Time), rfParent, exList);
                                }
                                else StoreError(DescribeDateError(value, XsdDateKind.Time), rfParent, exList);
                            }
                            else if (value is DateTime v) tmp = v.ToLocalTime();
                            else StoreError(DescribeDateError(value, XsdDateKind.Time), rfParent, exList);
                        }
                        var dt = new time_DEtype(rfParent.Response);
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
                        byte? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (byte.TryParse(s, out byte sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as unsigned byte", rfParent, exList);
                            }
                            else if (value.TryAs(out byte v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as unsigned byte", rfParent, exList);
                        }
                        var dt = new unsignedByte_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (byte)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.unsignedInt:
                    {
                        uint? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (uint.TryParse(s, out uint sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as uint", rfParent, exList);
                            }
                            else if (value.TryAs(out uint v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as uint", rfParent, exList);
                        }
                        var dt = new unsignedInt_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (uint)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.unsignedLong:
                    {
                        ulong? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (ulong.TryParse(s, out ulong sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as ulong", rfParent, exList);
                            }
                            else if (value.TryAs(out ulong v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as ulong", rfParent, exList);
                        }
                        var dt = new unsignedLong_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (ulong)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.unsignedShort:
                    {
                        ushort? tmp = null;
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (ushort.TryParse(s, out ushort sVal)) tmp = sVal;
                                else StoreError("Supplied value parameter could not be parsed as ushort", rfParent, exList);
                            }
                            else if (value.TryAs(out ushort v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as ushort", rfParent, exList);
                        }
                        var dt = new unsignedShort_DEtype(rfParent.Response);
                        if (tmp != null && tmp != 0) dt.val = (ushort)tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
                case ItemChoiceType.yearMonthDuration:
                    {
                        string? tmp = null; //start with a default value that is not zero
                        if (value != null)
                        {
                            if (value is string s)
                            {
                                if (Regex.Match(s, @"^-?P[0-9]+Y?([0-9]+M)?$").Success) //yearMonthDuration
                                    tmp = s;
                                else StoreError(DescribeDateError(value, XsdDateKind.YearMonthDuration), rfParent, exList);
                            }
                            else if (value is TimeSpan ts && ts != default)
                                tmp = XmlConvert.ToString(ts); //ToDo: Need to truncate after hh, mm, ss via regex, e.g., P13DT10H57M18S
                            else StoreError(DescribeDateError(value, XsdDateKind.YearMonthDuration), rfParent, exList);
                        }
                        var dt = new yearMonthDuration_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
            }

            rfParent.Response.ItemElementName = (ItemChoiceType2)dataTypeEnum;
            if (errors != null)
                foreach (var ex in exList) errors.Add(ex);
            return rfParent.Response;
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

        // Records a parse/format error: adds to the caller's error list and fires the central
        // validation event so subscribers (UI, logger) are notified without throwing.
        private static void StoreError(string errorMsg, ResponseFieldType rfParent, List<Exception> exList)
        {
            var exData = new Exception();
            exData.Data.Add("QuestionID: ", rfParent?.ParentIETnode?.ID.ToString() ?? "null");
            exData.Data.Add("Error: ", errorMsg);
            exList.Add(exData);

            if (!SdcUtil.IsDeserializing.Value)
                SdcValidationEvents.Raise(
                    message:      errorMsg,
                    nodeID:       rfParent?.ParentIETnode?.ID.ToString(),
                    propertyName: nameof(DataTypes_DEType.Item));
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
