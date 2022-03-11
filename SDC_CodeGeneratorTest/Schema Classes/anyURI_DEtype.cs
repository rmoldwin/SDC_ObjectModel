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
[JsonObject("anyURI_DEtype")]
public partial class anyURI_DEtype : anyURI_Stype
{
    private bool _shouldSerializemaxLength;
    private bool _shouldSerializeminLength;
    private bool _shouldSerializeX_length;
    private System.Nullable<long> _x_length;
    private string _description;
    private System.Nullable<long> _minLength;
    private System.Nullable<long> _maxLength;
    private string _pattern;
    private bool _descriptionSpecified;
    private bool _patternSpecified;
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual long X_length
    {
        get
        {
            if (_x_length.HasValue)
            {
                return _x_length.Value;
            }
            else
            {
                return default(long);
            }
        }
        set
        {
            if ((_x_length.Equals(value) != true))
            {
                _x_length = value;
                OnPropertyChanged("X_length", value);
            }
            _shouldSerializeX_length = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool X_lengthSpecified
    {
        get
        {
            return _x_length.HasValue;
        }
        set
        {
            if (value==false)
            {
                _x_length = null;
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string description
    {
        get
        {
            return _description;
        }
        set
        {
            if ((_description == value))
            {
                return;
            }
            if (((_description == null) 
                        || (_description.Equals(value) != true)))
            {
                _description = value;
                OnPropertyChanged("description", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual long minLength
    {
        get
        {
            if (_minLength.HasValue)
            {
                return _minLength.Value;
            }
            else
            {
                return default(long);
            }
        }
        set
        {
            if ((_minLength.Equals(value) != true))
            {
                _minLength = value;
                OnPropertyChanged("minLength", value);
            }
            _shouldSerializeminLength = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool minLengthSpecified
    {
        get
        {
            return _minLength.HasValue;
        }
        set
        {
            if (value==false)
            {
                _minLength = null;
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual long maxLength
    {
        get
        {
            if (_maxLength.HasValue)
            {
                return _maxLength.Value;
            }
            else
            {
                return default(long);
            }
        }
        set
        {
            if ((_maxLength.Equals(value) != true))
            {
                _maxLength = value;
                OnPropertyChanged("maxLength", value);
            }
            _shouldSerializemaxLength = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool maxLengthSpecified
    {
        get
        {
            return _maxLength.HasValue;
        }
        set
        {
            if (value==false)
            {
                _maxLength = null;
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string pattern
    {
        get
        {
            return _pattern;
        }
        set
        {
            if ((_pattern == value))
            {
                return;
            }
            if (((_pattern == null) 
                        || (_pattern.Equals(value) != true)))
            {
                _pattern = value;
                OnPropertyChanged("pattern", value);
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool descriptionSpecified
    {
        get
        {
            return _descriptionSpecified;
        }
        set
        {
            _descriptionSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool patternSpecified
    {
        get
        {
            return _patternSpecified;
        }
        set
        {
            _patternSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether X_length should be serialized
    /// </summary>
    public virtual bool ShouldSerializeX_length()
    {
        if (_shouldSerializeX_length)
        {
            return true;
        }
        return (X_length != default(long));
    }
    
    /// <summary>
    /// Test whether minLength should be serialized
    /// </summary>
    public virtual bool ShouldSerializeminLength()
    {
        if (_shouldSerializeminLength)
        {
            return true;
        }
        return (minLength != default(long));
    }
    
    /// <summary>
    /// Test whether maxLength should be serialized
    /// </summary>
    public virtual bool ShouldSerializemaxLength()
    {
        if (_shouldSerializemaxLength)
        {
            return true;
        }
        return (maxLength != default(long));
    }
    
    /// <summary>
    /// Test whether description should be serialized
    /// </summary>
    public virtual bool ShouldSerializedescription()
    {
        return !string.IsNullOrEmpty(description);
    }
    
    /// <summary>
    /// Test whether pattern should be serialized
    /// </summary>
    public virtual bool ShouldSerializepattern()
    {
        return !string.IsNullOrEmpty(pattern);
    }
}
}
#pragma warning restore
