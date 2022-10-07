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

[XmlInclude(typeof(short_DEtype))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("short_Stype")]
public partial class short_Stype : BaseType
{
    #region Private fields
    private bool _shouldSerializequantEnum;
    private bool _shouldSerializeval;
    private short _val;
    private dtQuantEnum _quantEnum;
    #endregion
    
    /// <summary>
    /// short_Stype class constructor
    /// </summary>
    public short_Stype()
    {
        _quantEnum = dtQuantEnum.EQ;
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual short val
    {
        get
        {
            return _val;
        }
        set
        {
            if ((_val.Equals(value) != true))
            {
                _val = value;
                OnPropertyChanged("val", value);
            }
            _shouldSerializeval = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(dtQuantEnum.EQ)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual dtQuantEnum quantEnum
    {
        get
        {
            return _quantEnum;
        }
        set
        {
            if ((_quantEnum.Equals(value) != true))
            {
                _quantEnum = value;
                OnPropertyChanged("quantEnum", value);
            }
            _shouldSerializequantEnum = true;
        }
    }
    
    /// <summary>
    /// Test whether val should be serialized
    /// </summary>
    public virtual bool ShouldSerializeval()
    {
        if (_shouldSerializeval)
        {
            return true;
        }
        return (_val != default(short));
    }
    
    /// <summary>
    /// Test whether quantEnum should be serialized
    /// </summary>
    public virtual bool ShouldSerializequantEnum()
    {
        if (_shouldSerializequantEnum)
        {
            return true;
        }
        return (_quantEnum != default(dtQuantEnum));
    }
}
}
#pragma warning restore
