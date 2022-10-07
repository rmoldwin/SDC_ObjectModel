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
/// Test the data in the form for proper data types, rule integrity, and
/// completeness of required questions.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("ActValidateFormType")]
public partial class ActValidateFormType : ExtensionBaseType
{
    #region Private fields
    private bool _shouldSerializevalidateCompleteness;
    private bool _shouldSerializevalidateRules;
    private bool _shouldSerializevalidateDataTypes;
    private CallFuncType _validationWebService;
    private bool _validateDataTypes;
    private bool _validateRules;
    private bool _validateCompleteness;
    private string _validationType;
    #endregion
    
    /// <summary>
    /// ActValidateFormType class constructor
    /// </summary>
    public ActValidateFormType()
    {
        _validateDataTypes = false;
        _validateRules = false;
        _validateCompleteness = false;
    }
    
    [XmlElement(Order=0)]
    [JsonProperty(Order=0, NullValueHandling=NullValueHandling.Ignore)]
    public virtual CallFuncType ValidationWebService
    {
        get
        {
            return _validationWebService;
        }
        set
        {
            if ((_validationWebService == value))
            {
                return;
            }
            if (((_validationWebService == null) 
                        || (_validationWebService.Equals(value) != true)))
            {
                _validationWebService = value;
                OnPropertyChanged("ValidationWebService", value);
            }
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool validateDataTypes
    {
        get
        {
            return _validateDataTypes;
        }
        set
        {
            if ((_validateDataTypes.Equals(value) != true))
            {
                _validateDataTypes = value;
                OnPropertyChanged("validateDataTypes", value);
            }
            _shouldSerializevalidateDataTypes = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool validateRules
    {
        get
        {
            return _validateRules;
        }
        set
        {
            if ((_validateRules.Equals(value) != true))
            {
                _validateRules = value;
                OnPropertyChanged("validateRules", value);
            }
            _shouldSerializevalidateRules = true;
        }
    }
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool validateCompleteness
    {
        get
        {
            return _validateCompleteness;
        }
        set
        {
            if ((_validateCompleteness.Equals(value) != true))
            {
                _validateCompleteness = value;
                OnPropertyChanged("validateCompleteness", value);
            }
            _shouldSerializevalidateCompleteness = true;
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string validationType
    {
        get
        {
            return _validationType;
        }
        set
        {
            if ((_validationType == value))
            {
                return;
            }
            if (((_validationType == null) 
                        || (_validationType.Equals(value) != true)))
            {
                _validationType = value;
                OnPropertyChanged("validationType", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether validateDataTypes should be serialized
    /// </summary>
    public virtual bool ShouldSerializevalidateDataTypes()
    {
        if (_shouldSerializevalidateDataTypes)
        {
            return true;
        }
        return (_validateDataTypes != default(bool));
    }
    
    /// <summary>
    /// Test whether validateRules should be serialized
    /// </summary>
    public virtual bool ShouldSerializevalidateRules()
    {
        if (_shouldSerializevalidateRules)
        {
            return true;
        }
        return (_validateRules != default(bool));
    }
    
    /// <summary>
    /// Test whether validateCompleteness should be serialized
    /// </summary>
    public virtual bool ShouldSerializevalidateCompleteness()
    {
        if (_shouldSerializevalidateCompleteness)
        {
            return true;
        }
        return (_validateCompleteness != default(bool));
    }
    
    /// <summary>
    /// Test whether ValidationWebService should be serialized
    /// </summary>
    public virtual bool ShouldSerializeValidationWebService()
    {
        return (_validationWebService != null);
    }
    
    /// <summary>
    /// Test whether validationType should be serialized
    /// </summary>
    public virtual bool ShouldSerializevalidationType()
    {
        return !string.IsNullOrEmpty(validationType);
    }
}
}
#pragma warning restore
