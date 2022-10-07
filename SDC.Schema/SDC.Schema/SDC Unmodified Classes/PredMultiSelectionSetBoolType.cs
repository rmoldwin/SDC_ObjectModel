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
/// This Rule component evaluates the @selected status of any set of
/// ListItems at runtime, and returns a true or false value based on the @selected
/// status of each ListItem.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("PredMultiSelectionSetBoolType")]
public partial class PredMultiSelectionSetBoolType : FuncBoolBaseType
{
    #region Private fields
    private string _selectedItemSet;
    #endregion
    
    [XmlAttribute(DataType="NMTOKENS")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string selectedItemSet
    {
        get
        {
            return _selectedItemSet;
        }
        set
        {
            if ((_selectedItemSet == value))
            {
                return;
            }
            if (((_selectedItemSet == null) 
                        || (_selectedItemSet.Equals(value) != true)))
            {
                _selectedItemSet = value;
                OnPropertyChanged("selectedItemSet", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether selectedItemSet should be serialized
    /// </summary>
    public virtual bool ShouldSerializeselectedItemSet()
    {
        return !string.IsNullOrEmpty(selectedItemSet);
    }
}
}
#pragma warning restore
