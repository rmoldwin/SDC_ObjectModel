using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;
using System.Xml;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace SDC.Schema
{
    public interface IDataHelpers
    {
        #region Data Helpers
        static DataTypes_DEType AddDataTypesDE(
          ResponseFieldType rfParent,
          ItemChoiceType dataTypeEnum = ItemChoiceType.@string,
          dtQuantEnum quantifierEnum = dtQuantEnum.EQ,
          object? value = null)
        {
            //Exception ex;
            List<Exception> exList = new();

            //if (rfParent.Response != null) throw new InvalidOperationException("A DataTypes_DEType object already exists on the rfParent parameter (ResponseFieldType)");
            rfParent.Response = new DataTypes_DEType(rfParent);

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
                                else StoreError("Supplied value parameter was not in anyURI string format");
                            else StoreError("Supplied value parameter was not in anyURI string format");
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
                                    $"Bytes written = {bytesWritten}");
                            }
                            else if (value is byte[] bVal) tmp = bVal;
                            else StoreError("Supplied value parameter could not be parsed as base64Binary (byte[])");
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
                                else StoreError("Supplied value parameter could not be parsed as bool");
                            }
                            else if (value is bool v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as bool");
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
                                else StoreError("Supplied value parameter could not be parsed as sbyte");
                            }
                            else if (value.TryAs(out sbyte v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as sbyte");
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
                                    else StoreError("Supplied value parameter could not be parsed as date");
                                }
                                else StoreError("Supplied value parameter could not be parsed as date");
                            }
                            else if (value is DateTime v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as date");
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
                                else StoreError("Supplied value parameter could not be parsed as dateTime");
                            }
                            else if (value is DateTime v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as dateTime");
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
                                else StoreError("Supplied value parameter could not be parsed as dateTimeStamp");
                            }
                            else if (value is DateTime v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as dateTimeStamp");
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
                                else StoreError("Supplied value parameter could not be parsed as dayTimeDuration");
                            }
                            else if (value is TimeSpan ts) 
                                tmp = XmlConvert.ToString(ts); //ToDo: Need to modify the Year part (convert Years to hours [ts.totalHours] and add to hours part); e.g., P13DT10H57M18S
                            else StoreError("Supplied value parameter could not be parsed as dayTimeDuration");
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
                                else StoreError("Supplied value parameter could not be parsed as decimal");
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as decimal");
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
                                else StoreError("Supplied value parameter could not be parsed as double");
                            }
                            else if (value.TryAs(out double v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as double");
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
                                else StoreError("Supplied value parameter could not be parsed as duration");
                            }
                            else if (value is TimeSpan ts && ts != default)
                                tmp = XmlConvert.ToString(ts);
                            else StoreError("Supplied value parameter could not be parsed as duration");
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
                                else StoreError("Supplied value parameter could not be parsed as float");
                            }
                            else if (value.TryAs(out float v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as float");
                        }
                        var dt = new double_DEtype(rfParent.Response);
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
                                else StoreError("Supplied value parameter could not be parsed as gDay");
                            }
                            else StoreError("Supplied value parameter could not be parsed as gDay");
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
                                else StoreError("Supplied value parameter could not be parsed as gMonth");
                            }
                            else StoreError("Supplied value parameter could not be parsed as gMonth");
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
                                else StoreError("Supplied value parameter could not be parsed as gMonthDay");
                            }
                            else StoreError("Supplied value parameter could not be parsed as gMonthDay");
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
                                else StoreError("Supplied value parameter could not be parsed as gYear");
                            }
                            else StoreError("Supplied value parameter could not be parsed as gYear");
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
                                else StoreError("Supplied value parameter could not be parsed as gYearMonth");
                            }
                            else StoreError("Supplied value parameter could not be parsed as gYearMonth");
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
                                else StoreError("Supplied value parameter could not be parsed as byte[]");
                            }
                            else if (value is byte[] v) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as byte[]");
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
                                else StoreError("Supplied value parameter could not be parsed as int");
                            }
                            else if (value.TryAs(out int v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as int");
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
                                else StoreError("Supplied value parameter could not be parsed as integer");
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as integer");
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
                                else StoreError("Supplied value parameter could not be parsed as long");
                            }
                            else if (value.TryAs(out long v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as long");
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
                                else StoreError("Supplied value parameter could not be parsed as negativeInteger");
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as negativeInteger");

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp2 < 0) dt.val = tmp2;
                            }
                            else StoreError("Supplied value parameter could not be parsed as negativeInteger");
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
                                else StoreError("Supplied value parameter could not be parsed as nonNegativeInteger");
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as nonNegativeInteger");


                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp2 != default && tmp > 0) dt.val = tmp2;  //not using >=0, since 0 is the decimal default value
                            }
                            else StoreError("Supplied value parameter could not be parsed as nonnegativeInteger");
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
                                else StoreError("Supplied value parameter could not be parsed as nonPositiveInteger");
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as nonPositiveInteger");

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp < 0) dt.val = tmp2;  //not using <=0, since 0 is the decimal default value
                            }
                            else StoreError("Supplied value parameter could not be parsed as nonPositiveInteger");
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
                                else StoreError("Supplied value parameter could not be parsed as positiveInteger");
                            }
                            else if (value.TryAs(out decimal v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as positiveInteger");

                            if (tmp != null)
                            {
                                decimal tmp2 = decimal.Truncate((decimal)tmp);
                                if (tmp > 0) dt.val = tmp2;
                            }
                            else StoreError("Supplied value parameter could not be parsed as PositiveInteger");
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
                                else StoreError("Supplied value parameter could not be parsed as short");
                            }
                            else if (value.TryAs(out short v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as short");
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
                        if (value != null)
                        {
                            if (value is string)
                            {
                                if (string.IsNullOrWhiteSpace((string)value))
                                    dt.val = (string)value;
                            }
                            else StoreError("Supplied value parameter was not a string datatype");
                        }
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
                                    else StoreError("Supplied value parameter could not be parsed as time");
                                }
                                else StoreError("Supplied value parameter could not be parsed as time");
                            }
                            else if (value is DateTime v) tmp = v.ToLocalTime();
                            else StoreError("Supplied value parameter could not be parsed as time");
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
                                else StoreError("Supplied value parameter could not be parsed as unsigned byte");
                            }
                            else if (value.TryAs(out byte v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as unsigned byte");
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
                                else StoreError("Supplied value parameter could not be parsed as uint");
                            }
                            else if (value.TryAs(out uint v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as uint");
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
                                else StoreError("Supplied value parameter could not be parsed as ulong");
                            }
                            else if (value.TryAs(out ulong v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as ulong");
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
                                else StoreError("Supplied value parameter could not be parsed as ushort");
                            }
                            else if (value.TryAs(out ushort v, out _)) tmp = v;
                            else StoreError("Supplied value parameter could not be parsed as ushort");
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
                                else StoreError("Supplied value parameter could not be parsed as yearMonthDuration");
                            }
                            else if (value is TimeSpan ts && ts != default)
                                tmp = XmlConvert.ToString(ts); //ToDo: Need to truncate after hh, mm, ss via regex, e.g., P13DT10H57M18S
                            else StoreError("Supplied value parameter could not be parsed as yearMonthDuration");
                        }
                        var dt = new yearMonthDuration_DEtype(rfParent.Response);
                        if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                        dt.quantEnum = quantifierEnum;
                        rfParent.Response.DataTypeDE_Item = dt;
                    }
                    break;
            }

            rfParent.Response.ItemElementName = (ItemChoiceType2)dataTypeEnum;
            return rfParent.Response;

            void StoreError(string errorMsg)
            {
                var exData = new Exception();
                exData.Data.Add("QuestionID: ", rfParent.ParentIETypeNode.ID.ToString() ?? "null");
                exData.Data.Add("Error: ", errorMsg);
                exList.Add(exData);
            }

        }

        DataTypes_SType AddDataTypesS();
        DataTypesDateTime_DEType DataTypesDateTimeDE();
        DataTypesDateTime_SType DataTypesDateTimeS();
        DataTypesNumeric_DEType AddDataTypesNumericDE();
        DataTypesNumeric_SType AddDataTypesNumericS();



        dtQuantEnum AssignQuantifier(string quantifier)
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


        static DataTypes_DEType AddHTML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!, List<XmlAttribute> valAtt = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new HTML_DEtype(rfParent.Response);
            dt.Any = valEl ?? new List<XmlElement>();
            dt.AnyAttr = valAtt ?? new List<XmlAttribute>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.HTML;
            return rfParent.Response;
        }
        static DataTypes_DEType AddXML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new XML_DEtype(rfParent.Response);
            dt.Any = valEl ?? new List<XmlElement>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.XML;
            return rfParent.Response;
        }
        static DataTypes_DEType AddAny_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!, List<XmlAttribute> valAtt = null!, string nameSpace = null!, string schema = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new anyType_DEtype(rfParent.Response);
            dt.@namespace = nameSpace ?? null;
            dt.schema = schema ?? null;
            dt.Any = valEl ?? new List<XmlElement>();
            dt.AnyAttr = valAtt ?? new List<XmlAttribute>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.HTML;
            return rfParent.Response;
        }
        static DataTypes_DEType AddBase64_DE(ResponseFieldType rfParent, byte[] value = null!, string mediaType = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new base64Binary_DEtype(rfParent.Response);
            dt.val = value;
            dt.mediaType = mediaType;
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.base64Binary;
            return rfParent.Response;
        }


        #endregion

    }






    public interface IHtmlHelpers
    {
        HTML_Stype AddHTML(RichTextType rt)
        {
            var html = new HTML_Stype(rt);
            rt.RichText = html;
            html.Any = new List<XmlElement>();

            return html;

            //TODO: Check XHTML builder here:
            //https://gist.github.com/rarous/3150395,
            //http://www.authorcode.com/code-snippet-converting-xmlelement-to-xelement-and-xelement-to-xmlelement-in-vb-net/
            //https://msdn.microsoft.com/en-us/library/system.xml.linq.loadoptions%28v=vs.110%29.aspx

        }

    }


}
