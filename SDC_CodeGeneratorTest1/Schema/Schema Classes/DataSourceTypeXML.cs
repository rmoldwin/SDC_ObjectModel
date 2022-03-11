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
[XmlTypeAttribute(AnonymousType=true, Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("DataSourceTypeXML")]
public partial class DataSourceTypeXML : DataStoreType
{
    private string_Stype _item;
    private bool _itemSpecified;
    [XmlElement("ItemID", typeof(string_Stype), Order=0)]
    [XmlElement("SQL", typeof(SQL_Type), Order=0)]
    [XmlElement("XPath", typeof(XPathType), Order=0)]
    [XmlElement("XQuery", typeof(XQueryType), Order=0)]
    public virtual string_Stype Item
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
