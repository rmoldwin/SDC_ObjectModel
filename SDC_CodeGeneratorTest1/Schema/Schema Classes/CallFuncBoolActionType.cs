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

/// <summary>
/// Function or web service that returns a string
/// value.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("CallFuncBoolActionType")]
public partial class CallFuncBoolActionType : CallFuncBoolType
{
    private ExtensionBaseType[] _items1;
    private Items1ChoiceType[] _items1ElementName;
    private bool _items1Specified;
    private bool _items1ElementNameSpecified;
    [XmlElement("Actions", typeof(ActionsType), Order=0)]
    [XmlElement("ConditionalActions", typeof(PredActionType), Order=0)]
    [XmlElement("Else", typeof(PredActionType), Order=0)]
    [XmlChoiceIdentifierAttribute("Items1ElementName")]
    public virtual ExtensionBaseType[] Items1
    {
        get
        {
            return _items1;
        }
        set
        {
            if ((_items1 == value))
            {
                return;
            }
            if (((_items1 == null) 
                        || (_items1.Equals(value) != true)))
            {
                _items1 = value;
                OnPropertyChanged("Items1", value);
            }
        }
    }
    
    [XmlElement("Items1ElementName", Order=1)]
    [XmlIgnore]
    public virtual Items1ChoiceType[] Items1ElementName
    {
        get
        {
            return _items1ElementName;
        }
        set
        {
            if ((_items1ElementName == value))
            {
                return;
            }
            if (((_items1ElementName == null) 
                        || (_items1ElementName.Equals(value) != true)))
            {
                _items1ElementName = value;
                OnPropertyChanged("Items1ElementName", value);
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool Items1Specified
    {
        get
        {
            return _items1Specified;
        }
        set
        {
            _items1Specified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool Items1ElementNameSpecified
    {
        get
        {
            return _items1ElementNameSpecified;
        }
        set
        {
            _items1ElementNameSpecified = value;
        }
    }
}
}
#pragma warning restore
