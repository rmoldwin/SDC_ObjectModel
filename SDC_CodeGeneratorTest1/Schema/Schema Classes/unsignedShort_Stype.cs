// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 6.0.0.0. www.xsd2code.com
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
using System.Xml;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MsgPack.Serialization;
using System.IO;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

[XmlInclude(typeof(unsignedShort_DEtype))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("unsignedShort_Stype")]
public partial class unsignedShort_Stype : BaseType
{
    private bool _shouldSerializequantEnum;
    private bool _shouldSerializeval;
    private System.Nullable<ushort> _val;
    private dtQuantEnum _quantEnum;
    private bool _quantEnumSpecified;
    /// <summary>
    /// unsignedShort_Stype class constructor
    /// </summary>
    public unsignedShort_Stype()
    {
        _quantEnum = dtQuantEnum.EQ;
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual ushort val
    {
        get
        {
            if (_val.HasValue)
            {
                return _val.Value;
            }
            else
            {
                return default(ushort);
            }
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
    
    [XmlIgnore]
    public virtual bool valSpecified
    {
        get
        {
            return _val.HasValue;
        }
        set
        {
            if (value==false)
            {
                _val = null;
            }
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
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool quantEnumSpecified
    {
        get
        {
            return _quantEnumSpecified;
        }
        set
        {
            _quantEnumSpecified = value;
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
        return (val != default(ushort));
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
        return (quantEnum != default(dtQuantEnum));
    }
}
}
#pragma warning restore
