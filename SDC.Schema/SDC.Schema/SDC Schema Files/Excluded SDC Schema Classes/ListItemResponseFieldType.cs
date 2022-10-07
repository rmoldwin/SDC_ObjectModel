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
/// This type represents a place to store a fill-in response associated
/// directly with a selected ListItem. The response may be optional or
/// required.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("ListItemResponseFieldType")]
public partial class ListItemResponseFieldType : ResponseFieldType
{
    #region Private fields
    private bool _shouldSerializeresponseRequired;
    private bool _responseRequired;
    #endregion
    
    /// <summary>
    /// ListItemResponseFieldType class constructor
    /// </summary>
    public ListItemResponseFieldType()
    {
        _responseRequired = false;
    }
    
    /// <summary>
    /// If @responseRequired is set to true, then the appropriate
    /// text or Blob must be entered in the data-entry field associated with
    /// this list item.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool responseRequired
    {
        get
        {
            return _responseRequired;
        }
        set
        {
            if ((_responseRequired.Equals(value) != true))
            {
                _responseRequired = value;
                OnPropertyChanged("responseRequired", value);
            }
            _shouldSerializeresponseRequired = true;
        }
    }
    
    /// <summary>
    /// Test whether responseRequired should be serialized
    /// </summary>
    public virtual bool ShouldSerializeresponseRequired()
    {
        if (_shouldSerializeresponseRequired)
        {
            return true;
        }
        return (_responseRequired != default(bool));
    }
}
}
#pragma warning restore
