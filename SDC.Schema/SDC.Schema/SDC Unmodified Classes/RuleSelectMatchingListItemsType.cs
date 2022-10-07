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

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("RuleSelectMatchingListItemsType")]
public partial class RuleSelectMatchingListItemsType : ExtensionBaseType
{
    #region Private fields
    private ItemNameAttributeType _matchSource;
    private RuleListItemMatchTargetsType _listItemMatchTargets;
    #endregion
    
    [XmlElement(Order=0)]
    [JsonProperty(Order=0, NullValueHandling=NullValueHandling.Ignore)]
    public virtual ItemNameAttributeType MatchSource
    {
        get
        {
            return _matchSource;
        }
        set
        {
            if ((_matchSource == value))
            {
                return;
            }
            if (((_matchSource == null) 
                        || (_matchSource.Equals(value) != true)))
            {
                _matchSource = value;
                OnPropertyChanged("MatchSource", value);
            }
        }
    }
    
    [XmlElement(Order=1)]
    [JsonProperty(Order=1, NullValueHandling=NullValueHandling.Ignore)]
    public virtual RuleListItemMatchTargetsType ListItemMatchTargets
    {
        get
        {
            return _listItemMatchTargets;
        }
        set
        {
            if ((_listItemMatchTargets == value))
            {
                return;
            }
            if (((_listItemMatchTargets == null) 
                        || (_listItemMatchTargets.Equals(value) != true)))
            {
                _listItemMatchTargets = value;
                OnPropertyChanged("ListItemMatchTargets", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether MatchSource should be serialized
    /// </summary>
    public virtual bool ShouldSerializeMatchSource()
    {
        return (_matchSource != null);
    }
    
    /// <summary>
    /// Test whether ListItemMatchTargets should be serialized
    /// </summary>
    public virtual bool ShouldSerializeListItemMatchTargets()
    {
        return (_listItemMatchTargets != null);
    }
}
}
#pragma warning restore
