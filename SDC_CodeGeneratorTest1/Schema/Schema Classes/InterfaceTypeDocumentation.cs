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
/// This type provides information about an Applications Programming Interface (API)
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(AnonymousType=true, Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("InterfaceTypeDocumentation")]
public partial class InterfaceTypeDocumentation : BaseType
{
    private List<FileType> _file;
    private bool _fileSpecified;
    [XmlElement("File", Order=0)]
    [JsonProperty(Order=0, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<FileType> File
    {
        get
        {
            return _file;
        }
        set
        {
            if ((_file == value))
            {
                return;
            }
            if (((_file == null) 
                        || (_file.Equals(value) != true)))
            {
                _file = value;
                OnPropertyChanged("File", value);
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool FileSpecified
    {
        get
        {
            return _fileSpecified;
        }
        set
        {
            _fileSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether File should be serialized
    /// </summary>
    public virtual bool ShouldSerializeFile()
    {
        return File != null && File.Count > 0;
    }
}
}
#pragma warning restore
