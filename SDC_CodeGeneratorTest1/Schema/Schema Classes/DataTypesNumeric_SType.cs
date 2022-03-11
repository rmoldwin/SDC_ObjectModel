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
/// NEW: SDC datatypes in Simple (S) format, based mostly on W3C specifications. Uses
/// baseAttributes and Extension capability to enhance the list of Data Types. **CHECK for ERRORS and
/// completeness**
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("DataTypesNumeric_SType")]
public partial class DataTypesNumeric_SType : ExtensionBaseType
{
    private BaseType _item;
    private bool _itemSpecified;
    [XmlElement("byte", typeof(byte_Stype), IsNullable=true, Order=0)]
    [XmlElement("decimal", typeof(decimal_Stype), IsNullable=true, Order=0)]
    [XmlElement("double", typeof(double_Stype), IsNullable=true, Order=0)]
    [XmlElement("float", typeof(float_Stype), IsNullable=true, Order=0)]
    [XmlElement("int", typeof(int_Stype), IsNullable=true, Order=0)]
    [XmlElement("integer", typeof(integer_Stype), IsNullable=true, Order=0)]
    [XmlElement("long", typeof(long_Stype), IsNullable=true, Order=0)]
    [XmlElement("negativeInteger", typeof(negativeInteger_Stype), IsNullable=true, Order=0)]
    [XmlElement("nonNegativeInteger", typeof(nonNegativeInteger_Stype), IsNullable=true, Order=0)]
    [XmlElement("nonPositiveInteger", typeof(nonPositiveInteger_Stype), IsNullable=true, Order=0)]
    [XmlElement("positiveInteger", typeof(positiveInteger_Stype), IsNullable=true, Order=0)]
    [XmlElement("short", typeof(short_Stype), IsNullable=true, Order=0)]
    [XmlElement("unsignedByte", typeof(unsignedByte_Stype), IsNullable=true, Order=0)]
    [XmlElement("unsignedInt", typeof(unsignedInt_Stype), IsNullable=true, Order=0)]
    [XmlElement("unsignedLong", typeof(unsignedLong_Stype), IsNullable=true, Order=0)]
    [XmlElement("unsignedShort", typeof(unsignedShort_Stype), IsNullable=true, Order=0)]
    [XmlElement("yearMonthDuration", typeof(yearMonthDuration_Stype), IsNullable=true, Order=0)]
    public virtual BaseType Item
    {
        get
        {
            return _item;
        }
        set
        {
            if ((_item == value))
            {
                return;
            }
            if (((_item == null) 
                        || (_item.Equals(value) != true)))
            {
                _item = value;
                OnPropertyChanged("Item", value);
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool ItemSpecified
    {
        get
        {
            return _itemSpecified;
        }
        set
        {
            _itemSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether Item should be serialized
    /// </summary>
    public virtual bool ShouldSerializeItem()
    {
        return (Item != null);
    }
}
}
#pragma warning restore
