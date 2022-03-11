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
[JsonObject("integer_DEtype")]
public partial class integer_DEtype : integer_Stype
{
    private bool _shouldSerializeallowAPPROX;
    private bool _shouldSerializeallowLTE;
    private bool _shouldSerializeallowLT;
    private bool _shouldSerializeallowGTE;
    private bool _shouldSerializeallowGT;
    private string _maxExclusive;
    private string _minExclusive;
    private string _maxInclusive;
    private string _minInclusive;
    private System.Nullable<byte> _totalDigits;
    private string _mask;
    private bool _allowGT;
    private bool _allowGTE;
    private bool _allowLT;
    private bool _allowLTE;
    private bool _allowAPPROX;
    private bool _maxExclusiveSpecified;
    private bool _minExclusiveSpecified;
    private bool _maxInclusiveSpecified;
    private bool _minInclusiveSpecified;
    private bool _maskSpecified;
    private bool _allowGTSpecified;
    private bool _allowGTESpecified;
    private bool _allowLTSpecified;
    private bool _allowLTESpecified;
    private bool _allowAPPROXSpecified;
    /// <summary>
    /// integer_DEtype class constructor
    /// </summary>
    public integer_DEtype()
    {
        _allowGT = false;
        _allowGTE = false;
        _allowLT = false;
        _allowLTE = false;
        _allowAPPROX = false;
    }
    
    [XmlAttribute(DataType="integer")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string maxExclusive
    {
        get
        {
            return _maxExclusive;
        }
        set
        {
            if ((_maxExclusive == value))
            {
                return;
            }
            if (((_maxExclusive == null) 
                        || (_maxExclusive.Equals(value) != true)))
            {
                _maxExclusive = value;
                OnPropertyChanged("maxExclusive", value);
            }
        }
    }
    
    [XmlAttribute(DataType="integer")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string minExclusive
    {
        get
        {
            return _minExclusive;
        }
        set
        {
            if ((_minExclusive == value))
            {
                return;
            }
            if (((_minExclusive == null) 
                        || (_minExclusive.Equals(value) != true)))
            {
                _minExclusive = value;
                OnPropertyChanged("minExclusive", value);
            }
        }
    }
    
    [XmlAttribute(DataType="integer")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string maxInclusive
    {
        get
        {
            return _maxInclusive;
        }
        set
        {
            if ((_maxInclusive == value))
            {
                return;
            }
            if (((_maxInclusive == null) 
                        || (_maxInclusive.Equals(value) != true)))
            {
                _maxInclusive = value;
                OnPropertyChanged("maxInclusive", value);
            }
        }
    }
    
    [XmlAttribute(DataType="integer")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string minInclusive
    {
        get
        {
            return _minInclusive;
        }
        set
        {
            if ((_minInclusive == value))
            {
                return;
            }
            if (((_minInclusive == null) 
                        || (_minInclusive.Equals(value) != true)))
            {
                _minInclusive = value;
                OnPropertyChanged("minInclusive", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual byte totalDigits
    {
        get
        {
            if (_totalDigits.HasValue)
            {
                return _totalDigits.Value;
            }
            else
            {
                return default(byte);
            }
        }
        set
        {
            if ((_totalDigits.Equals(value) != true))
            {
                _totalDigits = value;
                OnPropertyChanged("totalDigits", value);
            }
        }
    }
    
    [XmlIgnore]
    public virtual bool totalDigitsSpecified
    {
        get
        {
            return _totalDigits.HasValue;
        }
        set
        {
            if (value==false)
            {
                _totalDigits = null;
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string mask
    {
        get
        {
            return _mask;
        }
        set
        {
            if ((_mask == value))
            {
                return;
            }
            if (((_mask == null) 
                        || (_mask.Equals(value) != true)))
            {
                _mask = value;
                OnPropertyChanged("mask", value);
            }
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool allowGT
    {
        get
        {
            return _allowGT;
        }
        set
        {
            if ((_allowGT.Equals(value) != true))
            {
                _allowGT = value;
                OnPropertyChanged("allowGT", value);
            }
            _shouldSerializeallowGT = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool allowGTE
    {
        get
        {
            return _allowGTE;
        }
        set
        {
            if ((_allowGTE.Equals(value) != true))
            {
                _allowGTE = value;
                OnPropertyChanged("allowGTE", value);
            }
            _shouldSerializeallowGTE = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool allowLT
    {
        get
        {
            return _allowLT;
        }
        set
        {
            if ((_allowLT.Equals(value) != true))
            {
                _allowLT = value;
                OnPropertyChanged("allowLT", value);
            }
            _shouldSerializeallowLT = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool allowLTE
    {
        get
        {
            return _allowLTE;
        }
        set
        {
            if ((_allowLTE.Equals(value) != true))
            {
                _allowLTE = value;
                OnPropertyChanged("allowLTE", value);
            }
            _shouldSerializeallowLTE = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool allowAPPROX
    {
        get
        {
            return _allowAPPROX;
        }
        set
        {
            if ((_allowAPPROX.Equals(value) != true))
            {
                _allowAPPROX = value;
                OnPropertyChanged("allowAPPROX", value);
            }
            _shouldSerializeallowAPPROX = true;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool maxExclusiveSpecified
    {
        get
        {
            return _maxExclusiveSpecified;
        }
        set
        {
            _maxExclusiveSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool minExclusiveSpecified
    {
        get
        {
            return _minExclusiveSpecified;
        }
        set
        {
            _minExclusiveSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool maxInclusiveSpecified
    {
        get
        {
            return _maxInclusiveSpecified;
        }
        set
        {
            _maxInclusiveSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool minInclusiveSpecified
    {
        get
        {
            return _minInclusiveSpecified;
        }
        set
        {
            _minInclusiveSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool maskSpecified
    {
        get
        {
            return _maskSpecified;
        }
        set
        {
            _maskSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool allowGTSpecified
    {
        get
        {
            return _allowGTSpecified;
        }
        set
        {
            _allowGTSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool allowGTESpecified
    {
        get
        {
            return _allowGTESpecified;
        }
        set
        {
            _allowGTESpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool allowLTSpecified
    {
        get
        {
            return _allowLTSpecified;
        }
        set
        {
            _allowLTSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool allowLTESpecified
    {
        get
        {
            return _allowLTESpecified;
        }
        set
        {
            _allowLTESpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool allowAPPROXSpecified
    {
        get
        {
            return _allowAPPROXSpecified;
        }
        set
        {
            _allowAPPROXSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether allowGT should be serialized
    /// </summary>
    public virtual bool ShouldSerializeallowGT()
    {
        if (_shouldSerializeallowGT)
        {
            return true;
        }
        return (allowGT != default(bool));
    }
    
    /// <summary>
    /// Test whether allowGTE should be serialized
    /// </summary>
    public virtual bool ShouldSerializeallowGTE()
    {
        if (_shouldSerializeallowGTE)
        {
            return true;
        }
        return (allowGTE != default(bool));
    }
    
    /// <summary>
    /// Test whether allowLT should be serialized
    /// </summary>
    public virtual bool ShouldSerializeallowLT()
    {
        if (_shouldSerializeallowLT)
        {
            return true;
        }
        return (allowLT != default(bool));
    }
    
    /// <summary>
    /// Test whether allowLTE should be serialized
    /// </summary>
    public virtual bool ShouldSerializeallowLTE()
    {
        if (_shouldSerializeallowLTE)
        {
            return true;
        }
        return (allowLTE != default(bool));
    }
    
    /// <summary>
    /// Test whether allowAPPROX should be serialized
    /// </summary>
    public virtual bool ShouldSerializeallowAPPROX()
    {
        if (_shouldSerializeallowAPPROX)
        {
            return true;
        }
        return (allowAPPROX != default(bool));
    }
    
    /// <summary>
    /// Test whether maxExclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializemaxExclusive()
    {
        return !string.IsNullOrEmpty(maxExclusive);
    }
    
    /// <summary>
    /// Test whether minExclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializeminExclusive()
    {
        return !string.IsNullOrEmpty(minExclusive);
    }
    
    /// <summary>
    /// Test whether maxInclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializemaxInclusive()
    {
        return !string.IsNullOrEmpty(maxInclusive);
    }
    
    /// <summary>
    /// Test whether minInclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializeminInclusive()
    {
        return !string.IsNullOrEmpty(minInclusive);
    }
    
    /// <summary>
    /// Test whether mask should be serialized
    /// </summary>
    public virtual bool ShouldSerializemask()
    {
        return !string.IsNullOrEmpty(mask);
    }
}
}
#pragma warning restore
