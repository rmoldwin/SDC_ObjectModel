// SDC-CUSTOM: do not overwrite
using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

namespace SDC.Schema
{
    /// <summary>
    /// Internal factory that parses, validates, and constructs <see cref="DataTypes_DEType"/>
    /// nodes for every XSD datatype supported by the SDC schema.
    /// </summary>
    /// <remarks>
    /// <b>Intended use:</b><br/>
    /// This class is <c>internal</c>. The public entry point is the extension method
    /// <c>rf.AddDataType(...)</c> in <c>IResponseFieldExtensions</c>. Internal code may
    /// call <see cref="AddDataTypesDE"/> directly.<br/>
    /// <br/>
    /// <b>Supported <see cref="ItemChoiceType"/> values:</b><br/>
    /// • <i>Numeric</i> — byte, short, int, long, float, double, decimal, integer,
    ///   negativeInteger, nonNegativeInteger, nonPositiveInteger, positiveInteger,
    ///   unsignedByte, unsignedShort, unsignedInt, unsignedLong<br/>
    /// • <i>Date / Time</i> — date, dateTime, dateTimeStamp, time<br/>
    /// • <i>Duration</i> — duration, dayTimeDuration, yearMonthDuration<br/>
    /// • <i>Gregorian date-parts</i> — gDay, gMonth, gMonthDay, gYear, gYearMonth<br/>
    /// • <i>Binary</i> — base64Binary, hexBinary<br/>
    /// • <i>String / URI</i> — string, anyURI<br/>
    /// • <i>Markup / open content</i> — HTML, XML, anyType<br/>
    /// • <i>Other</i> — boolean<br/>
    /// <br/>
    /// <b>Error handling contract:</b><br/>
    /// Parse or validation failures are <i>soft-rejected</i>: the factory never throws.
    /// Instead it calls <see cref="SdcUtil.RecordRejectedValue"/> (unconditional) and
    /// fires <see cref="SdcValidationEvents.ValidationOccurred"/> (gated by
    /// <see cref="SdcUtil.SuppressValidation"/>). When the caller passes a non-null
    /// <c>errors</c> list, a short-lived event-bridge subscription
    /// (<see cref="SubscribeErrorCollector"/>) translates every Error event into an
    /// <see cref="InvalidOperationException"/> appended to that list.<br/>
    /// <br/>
    /// <b>Date / duration types and <c>SetLexicalValue</c>:</b><br/>
    /// Date-family values are validated against XSD lexical patterns before assignment.
    /// Subsequent updates to date properties can be made via <c>SetLexicalValue</c>
    /// helper methods on each concrete date node type.<br/>
    /// <br/>
    /// <b>Markup / open-content types (HTML, XML, anyType):</b><br/>
    /// These store raw <see cref="System.Xml.XmlElement"/> lists. Well-formedness is
    /// checked via <see cref="System.Xml.XmlDocument.LoadXml"/>, but no schema or
    /// HTML5 validation is performed.
    /// </remarks>
    internal static class SdcDataTypeBuilder
    {
        /// <summary>
        /// Creates and attaches a <see cref="DataTypes_DEType"/> node to
        /// <paramref name="rfParent"/>, with the concrete datatype child selected by
        /// <paramref name="dataTypeEnum"/> and an optional initial value parsed from
        /// <paramref name="value"/>.
        /// </summary>
        /// <param name="rfParent">
        /// The <see cref="ResponseFieldType"/> that will own the new
        /// <see cref="DataTypes_DEType"/>. A new <c>Response</c> node is created on
        /// <paramref name="rfParent"/> if one does not already exist.
        /// </param>
        /// <param name="dataTypeEnum">
        /// The XSD datatype to construct. Defaults to <c>string</c>.
        /// See <see cref="ItemChoiceType"/> for all supported values.
        /// </param>
        /// <param name="quantifierEnum">
        /// The comparison quantifier (e.g., EQ, GT, LTE) to set on numeric and date
        /// types that carry a <c>quantEnum</c> property. Defaults to <c>EQ</c>.
        /// </param>
        /// <param name="value">
        /// An optional initial value. May be the native .NET type (e.g., <c>int</c>,
        /// <c>DateTime</c>, <c>byte[]</c>) or a lexical string representation.
        /// When <see langword="null"/>, the datatype node is created with its default
        /// (zero/empty) value. Malformed values are soft-rejected via
        /// <see cref="SdcUtil.RecordRejectedValue"/> and
        /// <see cref="SdcValidationEvents.ValidationOccurred"/>.
        /// </param>
        /// <param name="errors">
        /// Optional list to collect parse/validation errors as
        /// <see cref="InvalidOperationException"/> instances. When non-null, a
        /// temporary event-bridge subscription forwards every Error-severity
        /// <see cref="SdcValidationEvents.ValidationOccurred"/> event into this list
        /// for the duration of the call.
        /// </param>
        /// <returns>
        /// The <see cref="DataTypes_DEType"/> attached to <paramref name="rfParent"/>,
        /// whose <c>DataTypeDE_Item</c> holds the constructed concrete datatype node
        /// and whose <c>ItemElementName</c> is set to match <paramref name="dataTypeEnum"/>.
        /// </returns>
        /// <remarks>
        /// <b>Deserialization / SuppressValidation interaction:</b><br/>
        /// Event firing and <see cref="SdcUtil.ValidationCollector"/> entries are
        /// suppressed when <see cref="SdcUtil.SuppressValidation"/> is
        /// <see langword="true"/> (e.g., during non-validating round-trip
        /// deserialization). Rejected-value recording via
        /// <see cref="SdcUtil.RecordRejectedValue"/> is <i>unconditional</i> and is
        /// never suppressed.
        /// </remarks>
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
                    #region Markup types (HTML, XML, anyType)
                    case ItemChoiceType.HTML:
                        {
                            // HTML_DEtype stores raw XmlElement nodes. Full HTML validation requires an
                            // external HTML parser (not added as a dependency here). Instead: if value is
                            // List<XmlElement>, assign directly; if value is a string, attempt to parse as
                            // XML fragments (XHTML/HTML5 must be well-formed XML in this pipeline). Anything
                            // else is soft-rejected.
                            var dt = new HTML_DEtype(rfParent.Response);
                            if (value is List<XmlElement> elems)
                                dt.Any = elems;
                            else if (value is string s)
                            {
                                try
                                {
                                    var xdoc = new XmlDocument();
                                    xdoc.LoadXml($"<_root>{s}</_root>"); // wrap to allow multiple sibling elements
                                    var children = new List<XmlElement>();
                                    foreach (XmlNode child in xdoc.DocumentElement!.ChildNodes)
                                        if (child is XmlElement el) children.Add(el);
                                    dt.Any = children;
                                }
                                catch (XmlException)
                                {
                                    RecordAndRaise($"Supplied string could not be parsed as XML fragments for HTML_DEtype: {s}", dt, value, rfParent);
                                    dt.Any = new List<XmlElement>();
                                }
                            }
                            else if (value != null)
                            {
                                RecordAndRaise($"Unsupported type {value.GetType().Name} for HTML_DEtype; supply List<XmlElement> or a valid XML string.", dt, value, rfParent);
                                dt.Any = new List<XmlElement>();
                            }
                            else dt.Any = new List<XmlElement>();
                            dt.AnyAttr = new List<XmlAttribute>();
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    // Note: XML_DEtype and anyType_DEtype serve overlapping purposes; whether XML needs
                    // to remain as a distinct type alongside anyType is an open design question.
                    case ItemChoiceType.XML:
                        {
                            var dt = new XML_DEtype(rfParent.Response);
                            if (value is List<XmlElement> xmlElems)
                                dt.Any = xmlElems;
                            else if (value is string xs)
                            {
                                try
                                {
                                    var xdoc = new XmlDocument();
                                    xdoc.LoadXml(xs);
                                    dt.Any = new List<XmlElement> { xdoc.DocumentElement! };
                                }
                                catch (XmlException)
                                {
                                    RecordAndRaise($"Supplied string could not be parsed as well-formed XML for XML_DEtype: {xs}", dt, value, rfParent);
                                    dt.Any = new List<XmlElement>();
                                }
                            }
                            else if (value != null)
                            {
                                RecordAndRaise($"Unsupported type {value.GetType().Name} for XML_DEtype; supply List<XmlElement> or a valid XML string.", dt, value, rfParent);
                                dt.Any = new List<XmlElement>();
                            }
                            else dt.Any = new List<XmlElement>();
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    case ItemChoiceType.anyType:
                        {
                            var dt = new anyType_DEtype(rfParent.Response);
                            if (value is List<XmlElement> anyElems)
                                dt.Any = anyElems;
                            else if (value is string anys)
                            {
                                try
                                {
                                    var xdoc = new XmlDocument();
                                    xdoc.LoadXml(anys);
                                    dt.Any = new List<XmlElement> { xdoc.DocumentElement! };
                                }
                                catch (XmlException)
                                {
                                    RecordAndRaise($"Supplied string could not be parsed as well-formed XML for anyType_DEtype: {anys}", dt, value, rfParent);
                                    dt.Any = new List<XmlElement>();
                                }
                            }
                            else if (value != null)
                            {
                                RecordAndRaise($"Unsupported type {value.GetType().Name} for anyType_DEtype; supply List<XmlElement> or a valid XML string.", dt, value, rfParent);
                                dt.Any = new List<XmlElement>();
                            }
                            else dt.Any = new List<XmlElement>();
                            dt.AnyAttr = new List<XmlAttribute>();
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    #endregion // Markup types

                    // ── String and URI ──────────────────────────────────────────────────────
                    case ItemChoiceType.anyURI:
                        {
                            var dt = new anyURI_DEtype(rfParent.Response);
                            string? tmp = null;
                            if (value != null)
                            {
                                if (value is string s)
                                    if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out _)) tmp = s;
                                    else RecordAndRaise("Supplied value parameter was not in anyURI string format", dt, value, rfParent);
                                else RecordAndRaise("Supplied value parameter was not in anyURI string format", dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    // ── Binary ─────────────────────────────────────────────────────────────
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
                    // ── Boolean ────────────────────────────────────────────────────────────
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
                    // ── Numeric: byte ──────────────────────────────────────────────────────
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
                    #region Date and time types (date, dateTime, dateTimeStamp)
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
                    #endregion // Date and time types (date, dateTime, dateTimeStamp)

                    // ── Duration: dayTimeDuration ──────────────────────────────────────────
                    case ItemChoiceType.dayTimeDuration:
                        {
                            var dt = new dayTimeDuration_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    // ^-?P(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$ — dayTimeDuration allows
                                    // only day + time parts; Y (year) and month M are not legal here.
                                    if (Regex.IsMatch(s, @"^-?P(\d+D)?(T(\d+H)?(\d+M)?(\d+(\.\d+)?S)?)?$")) //dayTimeDuration
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.DayTimeDuration), dt, value, rfParent);
                                }
                                else if (value is TimeSpan ts)
                                    tmp = XmlConvert.ToString(ts); // TimeSpan has no year/month concept; XmlConvert produces valid dayTimeDuration, e.g., P13DT10H57M18S
                                else RecordAndRaise(DescribeDateError(value, XsdDateKind.DayTimeDuration), dt, value, rfParent);
                            }
                            if (!string.IsNullOrWhiteSpace(tmp)) dt.val = tmp;
                            dt.quantEnum = quantifierEnum;
                            rfParent.Response.DataTypeDE_Item = dt;
                        }
                        break;
                    // ── Numeric: decimal, double ──────────────────────────────────────────
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
                    // ── Duration: duration ────────────────────────────────────────────────
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
                    // ── Numeric: float ────────────────────────────────────────────────────
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
                    #region Gregorian date-part types (gDay, gMonth, gMonthDay, gYear, gYearMonth)
                    case ItemChoiceType.gDay: //# of day of month, +/- timezone
                        {
                            var dt = new gDay_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    // Normalize: UI fields may supply just the day digits without leading dashes.
                                    // gDay canonical form is '---DD'; if the string lacks the three-dash prefix,
                                    // strip any existing leading dashes and prepend '---' automatically.
                                    var normalized = s.StartsWith("---") ? s : "---" + s.TrimStart('-');
                                    if (Regex.IsMatch(normalized, @"^---(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?$")) //gDay
                                        tmp = normalized;
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
                                    // Normalize: UI fields may supply just the month digits without leading dashes.
                                    // gMonth canonical form is '--MM'; if the string lacks the two-dash prefix,
                                    // strip any existing leading dashes and prepend '--' automatically.
                                    var normalized = s.StartsWith("--") ? s : "--" + s.TrimStart('-');
                                    if (Regex.IsMatch(normalized, @"^--(0[1-9]|1[0-2])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?$")) //gMonth
                                        tmp = normalized;
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
                                    // Normalize: UI fields may supply just 'MM-DD' without the leading dashes.
                                    // gMonthDay canonical form is '--MM-DD'; if the string lacks the two-dash
                                    // prefix, strip any existing leading dashes and prepend '--' automatically.
                                    var normalized = s.StartsWith("--") ? s : "--" + s.TrimStart('-');
                                    if (Regex.IsMatch(normalized, @"^--(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])(Z|(\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?$")) //gMonthDay
                                        tmp = normalized;
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
                    #endregion // Gregorian date-part types

                    // ── Binary: hexBinary ─────────────────────────────────────────────────
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
                    // ── Numeric: int, integer, long ───────────────────────────────────────
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
                    #region Numeric types — integer family (negativeInteger, nonNegativeInteger, nonPositiveInteger, positiveInteger)
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
                    #endregion // Numeric types — integer family

                    // ── Numeric: short ────────────────────────────────────────────────────
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
                    // ── String and URI: string ────────────────────────────────────────────
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
                    // ── Date and time: time ───────────────────────────────────────────────
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
                    #region Numeric types — unsigned family (unsignedByte, unsignedInt, unsignedLong, unsignedShort)
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
                    #endregion // Numeric types — unsigned family

                    // ── Duration: yearMonthDuration ───────────────────────────────────────
                    case ItemChoiceType.yearMonthDuration:
                        {
                            var dt = new yearMonthDuration_DEtype(rfParent.Response);
                            string? tmp = null; //start with a default value that is not zero
                            if (value != null)
                            {
                                if (value is string s)
                                {
                                    // ^-?P(\d+Y)?(\d+M)?$ — yearMonthDuration allows only year + month
                                    // parts; D (day), T, H, S are not legal here.
                                    if (Regex.IsMatch(s, @"^-?P(\d+Y)?(\d+M)?$")) //yearMonthDuration
                                        tmp = s;
                                    else RecordAndRaise(DescribeDateError(value, XsdDateKind.YearMonthDuration), dt, value, rfParent);
                                }
                                else if (value is TimeSpan)
                                    RecordAndRaise("TimeSpan cannot represent a yearMonthDuration (years/months have no fixed day count); supply a lexical string like \"P1Y6M\" instead.", dt, value, rfParent);
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

        /// <summary>
        /// Creates a <see cref="DataTypes_DEType"/> with an <see cref="HTML_DEtype"/> child
        /// attached to <paramref name="rfParent"/>. Accepts pre-parsed
        /// <see cref="System.Xml.XmlElement"/> lists directly; no HTML parsing is performed.
        /// </summary>
        /// <param name="rfParent">The response field to attach the datatype to.</param>
        /// <param name="valEl">Optional list of XML elements representing XHTML content. Defaults to an empty list.</param>
        /// <param name="valAtt">Optional list of XML attributes for the HTML node. Defaults to an empty list.</param>
        /// <returns>The constructed <see cref="DataTypes_DEType"/> attached to <paramref name="rfParent"/>.</returns>
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

        /// <summary>
        /// Creates a <see cref="DataTypes_DEType"/> with an <see cref="XML_DEtype"/> child
        /// attached to <paramref name="rfParent"/>. The supplied elements must be well-formed XML.
        /// </summary>
        /// <param name="rfParent">The response field to attach the datatype to.</param>
        /// <param name="valEl">Optional list of XML elements. Defaults to an empty list.</param>
        /// <returns>The constructed <see cref="DataTypes_DEType"/> attached to <paramref name="rfParent"/>.</returns>
        internal static DataTypes_DEType AddXML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!)
        {
            rfParent.Response = new DataTypes_DEType(rfParent);

            var dt = new XML_DEtype(rfParent.Response);
            dt.Any = valEl ?? new List<XmlElement>();
            rfParent.Response.DataTypeDE_Item = dt;

            rfParent.Response.ItemElementName = ItemChoiceType2.XML;
            return rfParent.Response;
        }

        /// <summary>
        /// Creates a <see cref="DataTypes_DEType"/> with an <see cref="anyType_DEtype"/> child
        /// attached to <paramref name="rfParent"/>. Supports an optional namespace URI and
        /// schema location alongside the raw element and attribute lists.
        /// </summary>
        /// <param name="rfParent">The response field to attach the datatype to.</param>
        /// <param name="valEl">Optional list of XML elements. Defaults to an empty list.</param>
        /// <param name="valAtt">Optional list of XML attributes. Defaults to an empty list.</param>
        /// <param name="nameSpace">Optional XML namespace URI for the <c>anyType</c> node.</param>
        /// <param name="schema">Optional schema location URI for the <c>anyType</c> node.</param>
        /// <returns>The constructed <see cref="DataTypes_DEType"/> attached to <paramref name="rfParent"/>.</returns>
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

        /// <summary>
        /// Parses a string or symbol representation of an SDC quantifier and returns the
        /// corresponding <see cref="dtQuantEnum"/> value.
        /// </summary>
        /// <param name="quantifier">
        /// Accepted inputs (case-sensitive):<br/>
        /// <c>"EQ"</c>, <c>"="</c>, <c>"=="</c> → <see cref="dtQuantEnum.EQ"/><br/>
        /// <c>"GT"</c>, <c>"&gt;"</c> → <see cref="dtQuantEnum.GT"/><br/>
        /// <c>"GTE"</c>, <c>"&gt;="</c> → <see cref="dtQuantEnum.GTE"/><br/>
        /// <c>"LT"</c>, <c>"&lt;"</c> → <see cref="dtQuantEnum.LT"/><br/>
        /// <c>"LTE"</c>, <c>"&lt;="</c> → <see cref="dtQuantEnum.LTE"/><br/>
        /// <c>"APPROX"</c>, <c>"~"</c>, <c>"@"</c> → <see cref="dtQuantEnum.APPROX"/><br/>
        /// <c>null</c>, <c>""</c>, or any unrecognized string → <see cref="dtQuantEnum.EQ"/> (default)
        /// </param>
        /// <returns>The <see cref="dtQuantEnum"/> that corresponds to <paramref name="quantifier"/>.</returns>
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

        /// <summary>
        /// Subscribes a temporary handler to <see cref="SdcValidationEvents.ValidationOccurred"/>
        /// that forwards every Error-severity event into <paramref name="errors"/> as an
        /// <see cref="InvalidOperationException"/>. Returns the handler for cleanup in a
        /// <c>finally</c> block via <see cref="UnsubscribeErrorCollector"/>.
        /// </summary>
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

        /// <summary>
        /// Removes the bridge handler subscribed by <see cref="SubscribeErrorCollector"/>.
        /// </summary>
        private static void UnsubscribeErrorCollector(EventHandler<SdcValidationEventArgs> handler, IList<Exception>? errors)
        {
            if (errors != null)
                SdcValidationEvents.ValidationOccurred -= handler;
        }

        /// <summary>
        /// Centralized parse-failure path: records the rejected value on <paramref name="dtNode"/>
        /// (retrievable via <see cref="SdcUtil.GetRejectedValues"/>) and fires
        /// <see cref="SdcValidationEvents.ValidationOccurred"/> so all subscribers
        /// (UI, logger, error-bridge) are notified. Gated by <see cref="SdcUtil.SuppressValidation"/>.
        /// </summary>
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

        /// <summary>
        /// Builds an XSD-accurate error message for a malformed date/date-part value.
        /// Quotes the value, names the <c>xs:</c> type, gives the canonical form and an
        /// example, and pinpoints the violation. Falls back to a representability note
        /// for non-string values.
        /// </summary>
        private static string DescribeDateError(object? badValue, XsdDateKind kind)
        {
            if (badValue is string s)
                return XsdDateTimePatterns.BuildLexicalErrorMessage(kind, s);
            return $"A {XsdDateTimePatterns.XsdName(kind)} value could not be obtained from '{badValue ?? "null"}'"
                 + $" (type {badValue?.GetType().Name ?? "null"}).";
        }
    }
}
