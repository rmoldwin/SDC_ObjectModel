using System;
using System.Collections.Generic;
using System.Xml;

namespace SDC.Schema
{
    /// <summary>
    /// Obsolete facade for <see cref="SdcDataTypeBuilder"/>. All methods delegate to
    /// <see cref="SdcDataTypeBuilder"/> and will be removed in a future release.
    /// </summary>
    /// <remarks>
    /// Use the extension method <c>rf.AddDataType(...)</c> from
    /// <see cref="Extensions.IResponseFieldExtensions"/> as the public API for constructing
    /// SDC response datatypes. Internal code should call <see cref="SdcDataTypeBuilder"/>
    /// directly.
    /// </remarks>
    [Obsolete("Use rf.AddDataType() extension method. Direct internal use: SdcDataTypeBuilder.")]
    public interface IDataHelpers
    {
        #region Data Helpers

        /// <inheritdoc cref="SdcDataTypeBuilder.AddDataTypesDE"/>
        static DataTypes_DEType AddDataTypesDE(
            ResponseFieldType rfParent,
            ItemChoiceType dataTypeEnum = ItemChoiceType.@string,
            dtQuantEnum quantifierEnum = dtQuantEnum.EQ,
            object? value = null,
            IList<Exception>? errors = null)
            => SdcDataTypeBuilder.AddDataTypesDE(rfParent, dataTypeEnum, quantifierEnum, value, errors);

        /// <inheritdoc cref="SdcDataTypeBuilder.AddHTML_DE"/>
        static DataTypes_DEType AddHTML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!, List<XmlAttribute> valAtt = null!)
            => SdcDataTypeBuilder.AddHTML_DE(rfParent, valEl, valAtt);

        /// <inheritdoc cref="SdcDataTypeBuilder.AddXML_DE"/>
        static DataTypes_DEType AddXML_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!)
            => SdcDataTypeBuilder.AddXML_DE(rfParent, valEl);

        /// <inheritdoc cref="SdcDataTypeBuilder.AddAny_DE"/>
        static DataTypes_DEType AddAny_DE(ResponseFieldType rfParent, List<XmlElement> valEl = null!, List<XmlAttribute> valAtt = null!, string nameSpace = null!, string schema = null!)
            => SdcDataTypeBuilder.AddAny_DE(rfParent, valEl, valAtt, nameSpace, schema);

        /// <inheritdoc cref="SdcDataTypeBuilder.AddBase64_DE"/>
        static DataTypes_DEType AddBase64_DE(ResponseFieldType rfParent, byte[] value = null!, string mediaType = null!)
            => SdcDataTypeBuilder.AddBase64_DE(rfParent, value, mediaType);

        /// <inheritdoc cref="SdcDataTypeBuilder.AssignQuantifier"/>
        dtQuantEnum AssignQuantifier(string quantifier)
            => SdcDataTypeBuilder.AssignQuantifier(quantifier);

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