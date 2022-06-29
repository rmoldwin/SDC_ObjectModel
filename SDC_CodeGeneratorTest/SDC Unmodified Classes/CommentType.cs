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
/// CommentType allows ad-hoc comments to be added to SDC type that inherits from ExtensionType.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("CommentType")]
public partial class CommentType : BaseType
{
    #region Private fields
    private string _val;
    #endregion
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string val
    {
        get
        {
            return _val;
        }
        set
        {
            if ((_val == value))
            {
                return;
            }
            if (((_val == null) 
                        || (_val.Equals(value) != true)))
            {
                _val = value;
                OnPropertyChanged("val", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether val should be serialized
    /// </summary>
    public virtual bool ShouldSerializeval()
    {
        return !string.IsNullOrEmpty(val);
    }
}
}
#pragma warning restore