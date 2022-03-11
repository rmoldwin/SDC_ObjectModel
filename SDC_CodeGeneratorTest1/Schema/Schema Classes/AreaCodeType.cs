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

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("AreaCodeType")]
public partial class AreaCodeType : BaseType
{
    private bool _shouldSerializeval;
    private System.Nullable<ushort> _val;
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
}
}
#pragma warning restore
