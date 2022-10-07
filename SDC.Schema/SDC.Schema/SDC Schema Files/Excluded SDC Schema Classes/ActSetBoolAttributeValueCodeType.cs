// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 6.0.64.0. www.xsd2code.com
//  </auto-generated>
// ------------------------------------------------------------------------------
#pragma warning disable
namespace SDC.Schema
{
using System;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Collections;
using System.Xml.Schema;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Globalization;
using System.Xml;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MsgPack.Serialization;
using System.IO;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

/// <summary>
/// Sets the value of any named attribute. However, it is primarily
/// designed for Response element(s) on a Question or ListItem. The value may be
/// determined by a literal value with a defined data type, the value at another named
/// Response item, an expression written in a specified scripting or programming
/// language, or the value of a named code listed in the same
/// template.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("ActSetBoolAttributeValueCodeType")]
public partial class ActSetBoolAttributeValueCodeType : ScriptCodeBoolType
{
    #region Private fields
    private string _elementName;
    private string _attributeName;
    #endregion
    
    /// <summary>
    /// ActSetBoolAttributeValueCodeType class constructor
    /// </summary>
    public ActSetBoolAttributeValueCodeType()
    {
        _attributeName = "val";
    }
    
    /// <summary>
    /// The @name attribute of the referenced
    /// element.
    /// </summary>
    [XmlAttribute(DataType="NCName")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string elementName
    {
        get
        {
            return _elementName;
        }
        set
        {
            if ((_elementName == value))
            {
                return;
            }
            if (((_elementName == null) 
                        || (_elementName.Equals(value) != true)))
            {
                _elementName = value;
                OnPropertyChanged("elementName", value);
            }
        }
    }
    
    /// <summary>
    /// The name of any attribute on a named
    /// element.
    /// </summary>
    [XmlAttribute(DataType="NCName")]
    [DefaultValue("val")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string attributeName
    {
        get
        {
            return _attributeName;
        }
        set
        {
            if ((_attributeName == value))
            {
                return;
            }
            if (((_attributeName == null) 
                        || (_attributeName.Equals(value) != true)))
            {
                _attributeName = value;
                OnPropertyChanged("attributeName", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether elementName should be serialized
    /// </summary>
    public virtual bool ShouldSerializeelementName()
    {
        return !string.IsNullOrEmpty(elementName);
    }
    
    /// <summary>
    /// Test whether attributeName should be serialized
    /// </summary>
    public virtual bool ShouldSerializeattributeName()
    {
        return !string.IsNullOrEmpty(attributeName);
    }
}
}
#pragma warning restore
