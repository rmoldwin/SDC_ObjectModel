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
[JsonObject("TemplateTargetType")]
public partial class TemplateTargetType : ExtensionBaseType
{
    private anyURI_Stype _targetItemID;
    private RichTextType _targetDisplayText;
    private bool _targetItemIDSpecified;
    private bool _targetDisplayTextSpecified;
    [XmlElement(Order=0)]
    [JsonProperty(Order=0, NullValueHandling=NullValueHandling.Ignore)]
    public virtual anyURI_Stype TargetItemID
    {
        get
        {
            return _targetItemID;
        }
        set
        {
            if ((_targetItemID == value))
            {
                return;
            }
            if (((_targetItemID == null) 
                        || (_targetItemID.Equals(value) != true)))
            {
                _targetItemID = value;
                OnPropertyChanged("TargetItemID", value);
            }
        }
    }
    
    [XmlElement(Order=1)]
    [JsonProperty(Order=1, NullValueHandling=NullValueHandling.Ignore)]
    public virtual RichTextType TargetDisplayText
    {
        get
        {
            return _targetDisplayText;
        }
        set
        {
            if ((_targetDisplayText == value))
            {
                return;
            }
            if (((_targetDisplayText == null) 
                        || (_targetDisplayText.Equals(value) != true)))
            {
                _targetDisplayText = value;
                OnPropertyChanged("TargetDisplayText", value);
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool TargetItemIDSpecified
    {
        get
        {
            return _targetItemIDSpecified;
        }
        set
        {
            _targetItemIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool TargetDisplayTextSpecified
    {
        get
        {
            return _targetDisplayTextSpecified;
        }
        set
        {
            _targetDisplayTextSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether TargetItemID should be serialized
    /// </summary>
    public virtual bool ShouldSerializeTargetItemID()
    {
        return (TargetItemID != null);
    }
    
    /// <summary>
    /// Test whether TargetDisplayText should be serialized
    /// </summary>
    public virtual bool ShouldSerializeTargetDisplayText()
    {
        return (TargetDisplayText != null);
    }
}
}
#pragma warning restore
