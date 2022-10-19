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
/// A generic structure for recording file version metadata.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(AnonymousType=true, Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("VersionTypeChanges")]
public partial class VersionTypeChanges : ExtensionBaseType
{
    #region Private fields
    private List<ChangeLogType> _change;
    #endregion
    
    [XmlElement("Change", Order=0)]
    [JsonProperty(Order=0, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<ChangeLogType> Change
    {
        get
        {
            return _change;
        }
        set
        {
            if ((_change == value))
            {
                return;
            }
            if (((_change == null) 
                        || (_change.Equals(value) != true)))
            {
                _change = value;
                OnPropertyChanged("Change", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether Change should be serialized
    /// </summary>
    public virtual bool ShouldSerializeChange()
    {
        return Change != null && Change.Count > 0;
    }
}
}
#pragma warning restore