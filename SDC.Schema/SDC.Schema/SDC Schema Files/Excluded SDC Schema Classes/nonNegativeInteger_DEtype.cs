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

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("nonNegativeInteger_DEtype")]
public partial class nonNegativeInteger_DEtype : nonNegativeInteger_Stype
{
    #region Private fields
    private bool _shouldSerializeallowAPPROX;
    private bool _shouldSerializeallowLTE;
    private bool _shouldSerializeallowLT;
    private bool _shouldSerializeallowGTE;
    private bool _shouldSerializeallowGT;
    private bool _shouldSerializemaxExclusive;
    private bool _shouldSerializeminExclusive;
    private bool _shouldSerializemaxInclusive;
    private bool _shouldSerializeminInclusive;
    private decimal _minInclusive;
    private decimal _maxInclusive;
    private decimal _minExclusive;
    private decimal _maxExclusive;
    private byte _totalDigits;
    private string _mask;
    private bool _allowGT;
    private bool _allowGTE;
    private bool _allowLT;
    private bool _allowLTE;
    private bool _allowAPPROX;
    #endregion
    
    /// <summary>
    /// nonNegativeInteger_DEtype class constructor
    /// </summary>
    public nonNegativeInteger_DEtype()
    {
        _allowGT = false;
        _allowGTE = false;
        _allowLT = false;
        _allowLTE = false;
        _allowAPPROX = false;
    }
    
    [XmlAttribute]
    [FractionDigitsAttribute(0)]
    [MaxDigitsAttribute(29)]
    [RangeAttribute(0D, 7.9228162514264338E+28D)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual decimal minInclusive
    {
        get
        {
            return _minInclusive;
        }
        set
        {
            if ((_minInclusive.Equals(value) != true))
            {
                ValidationContext validatorPropContext = new ValidationContext(this, null, null);
                validatorPropContext.MemberName = "minInclusive";
                Validator.ValidateProperty(value, validatorPropContext);
                _minInclusive = value;
                OnPropertyChanged("minInclusive", value);
            }
            _shouldSerializeminInclusive = true;
        }
    }
    
    [XmlAttribute]
    [FractionDigitsAttribute(0)]
    [MaxDigitsAttribute(29)]
    [RangeAttribute(0D, 7.9228162514264338E+28D)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual decimal maxInclusive
    {
        get
        {
            return _maxInclusive;
        }
        set
        {
            if ((_maxInclusive.Equals(value) != true))
            {
                ValidationContext validatorPropContext = new ValidationContext(this, null, null);
                validatorPropContext.MemberName = "maxInclusive";
                Validator.ValidateProperty(value, validatorPropContext);
                _maxInclusive = value;
                OnPropertyChanged("maxInclusive", value);
            }
            _shouldSerializemaxInclusive = true;
        }
    }
    
    [XmlAttribute]
    [FractionDigitsAttribute(0)]
    [MaxDigitsAttribute(29)]
    [RangeAttribute(-1D, 7.9228162514264338E+28D)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual decimal minExclusive
    {
        get
        {
            return _minExclusive;
        }
        set
        {
            if ((_minExclusive.Equals(value) != true))
            {
                ValidationContext validatorPropContext = new ValidationContext(this, null, null);
                validatorPropContext.MemberName = "minExclusive";
                Validator.ValidateProperty(value, validatorPropContext);
                _minExclusive = value;
                OnPropertyChanged("minExclusive", value);
            }
            _shouldSerializeminExclusive = true;
        }
    }
    
    [XmlAttribute]
    [MaxDigitsAttribute(29)]
    [FractionDigitsAttribute(0)]
    [RangeAttribute(1D, 7.9228162514264338E+28D)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual decimal maxExclusive
    {
        get
        {
            return _maxExclusive;
        }
        set
        {
            if ((_maxExclusive.Equals(value) != true))
            {
                ValidationContext validatorPropContext = new ValidationContext(this, null, null);
                validatorPropContext.MemberName = "maxExclusive";
                Validator.ValidateProperty(value, validatorPropContext);
                _maxExclusive = value;
                OnPropertyChanged("maxExclusive", value);
            }
            _shouldSerializemaxExclusive = true;
        }
    }
    
    [XmlAttribute]
    [FractionDigitsAttribute(0)]
    [MaxDigitsAttribute(2)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual byte totalDigits
    {
        get
        {
            return _totalDigits;
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
    
    /// <summary>
    /// Test whether minInclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializeminInclusive()
    {
        if (_shouldSerializeminInclusive)
        {
            return true;
        }
        return (_minInclusive != default(decimal));
    }
    
    /// <summary>
    /// Test whether maxInclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializemaxInclusive()
    {
        if (_shouldSerializemaxInclusive)
        {
            return true;
        }
        return (_maxInclusive != default(decimal));
    }
    
    /// <summary>
    /// Test whether minExclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializeminExclusive()
    {
        if (_shouldSerializeminExclusive)
        {
            return true;
        }
        return (_minExclusive != default(decimal));
    }
    
    /// <summary>
    /// Test whether maxExclusive should be serialized
    /// </summary>
    public virtual bool ShouldSerializemaxExclusive()
    {
        if (_shouldSerializemaxExclusive)
        {
            return true;
        }
        return (_maxExclusive != default(decimal));
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
        return (_allowGT != default(bool));
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
        return (_allowGTE != default(bool));
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
        return (_allowLT != default(bool));
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
        return (_allowLTE != default(bool));
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
        return (_allowAPPROX != default(bool));
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
