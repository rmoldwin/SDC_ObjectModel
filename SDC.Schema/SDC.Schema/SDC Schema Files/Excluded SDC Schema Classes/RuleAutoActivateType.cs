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
/// This Rule sets the activation status of Items based on the selection
/// status of other ListItems. This declarative rule determines (guards) when target
/// Items should be activated or deactivated. The rule may optionally
/// activate/deactivate multiple target items with a single rule. The target item(s) to
/// activate/deactivate are listed in targetNameList. In the simplest case, this rule
/// operates as follows: A list of ListItems is provided (selectedItemWatchList). If all
/// the items in the list are selected (or unselected - see below) as specified in the
/// selectedItemWatchList list, then the guard evaluates to true, and the targetNameList
/// items are activated/deactivated. In some cases, we may wish to watch unselected
/// items in the selectedItemWatchList. This is indicated by prefixing the name of the
/// watched item with a minus sign/dash ("-"). In some cases, we may wish to deactivate
/// items in the targetNameSelectList list when the selectedItemWatchList evaluated to
/// true. In this case, the target item is prefixed with a dash ("-").
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("RuleAutoActivateType")]
public partial class RuleAutoActivateType : ExtensionBaseType
{
    #region Private fields
    private bool _shouldSerializesetExpanded;
    private bool _shouldSerializesetEnabled;
    private bool _shouldSerializesetVisibility;
    private bool _shouldSerializeonlyIf;
    private string _selectedItemSet;
    private bool _onlyIf;
    private string _targetNameActivationList;
    private toggleType _setVisibility;
    private toggleType _setEnabled;
    private toggleType _setExpanded;
    #endregion
    
    /// <summary>
    /// RuleAutoActivateType class constructor
    /// </summary>
    public RuleAutoActivateType()
    {
        _onlyIf = false;
        _setVisibility = toggleType.@false;
        _setEnabled = toggleType.@true;
        _setExpanded = toggleType.@true;
    }
    
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
    
    [XmlAttribute]
    [DefaultValue(false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool onlyIf
    {
        get
        {
            return _onlyIf;
        }
        set
        {
            if ((_onlyIf.Equals(value) != true))
            {
                _onlyIf = value;
                OnPropertyChanged("onlyIf", value);
            }
            _shouldSerializeonlyIf = true;
        }
    }
    
    /// <summary>
    /// This list contains the @names of Identified Items that
    /// will be automatically activated or deactivated when the @selectedItemSet
    /// evaluates to true. If a @name is prefixed with a hyphen (-), then the
    /// item will be deactivated when @selectedItemSet evaluates to true. If
    /// @not = true, then the Boolean rule evaluation is negated, and thus the
    /// rule works in reverse. If @onlyIf is true, then the above rule is
    /// reversed when @selectedItemSet evaluates to false. In other words, named
    /// items will be deactivated, and hyphen-prefixed items will be activated
    /// when @selectedItemSet is false.
    /// </summary>
    [XmlAttribute(DataType="NCName")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string targetNameActivationList
    {
        get
        {
            return _targetNameActivationList;
        }
        set
        {
            if ((_targetNameActivationList == value))
            {
                return;
            }
            if (((_targetNameActivationList == null) 
                        || (_targetNameActivationList.Equals(value) != true)))
            {
                _targetNameActivationList = value;
                OnPropertyChanged("targetNameActivationList", value);
            }
        }
    }
    
    /// <summary>
    /// Make target items visible when activated and vice versa.
    /// Default = false. All descendants are affected in the same
    /// way.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(toggleType.@false)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual toggleType setVisibility
    {
        get
        {
            return _setVisibility;
        }
        set
        {
            if ((_setVisibility.Equals(value) != true))
            {
                _setVisibility = value;
                OnPropertyChanged("setVisibility", value);
            }
            _shouldSerializesetVisibility = true;
        }
    }
    
    /// <summary>
    /// Make target items enabled when activated and vice versa.
    /// Default = true. All descendants are affected in the same
    /// way.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(toggleType.@true)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual toggleType setEnabled
    {
        get
        {
            return _setEnabled;
        }
        set
        {
            if ((_setEnabled.Equals(value) != true))
            {
                _setEnabled = value;
                OnPropertyChanged("setEnabled", value);
            }
            _shouldSerializesetEnabled = true;
        }
    }
    
    /// <summary>
    /// Expand target items when activated and collapse item when
    /// deactivated. Default = false. All descendants are affected in the same
    /// way.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(toggleType.@true)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual toggleType setExpanded
    {
        get
        {
            return _setExpanded;
        }
        set
        {
            if ((_setExpanded.Equals(value) != true))
            {
                _setExpanded = value;
                OnPropertyChanged("setExpanded", value);
            }
            _shouldSerializesetExpanded = true;
        }
    }
    
    /// <summary>
    /// Test whether onlyIf should be serialized
    /// </summary>
    public virtual bool ShouldSerializeonlyIf()
    {
        if (_shouldSerializeonlyIf)
        {
            return true;
        }
        return (_onlyIf != default(bool));
    }
    
    /// <summary>
    /// Test whether setVisibility should be serialized
    /// </summary>
    public virtual bool ShouldSerializesetVisibility()
    {
        if (_shouldSerializesetVisibility)
        {
            return true;
        }
        return (_setVisibility != default(toggleType));
    }
    
    /// <summary>
    /// Test whether setEnabled should be serialized
    /// </summary>
    public virtual bool ShouldSerializesetEnabled()
    {
        if (_shouldSerializesetEnabled)
        {
            return true;
        }
        return (_setEnabled != default(toggleType));
    }
    
    /// <summary>
    /// Test whether setExpanded should be serialized
    /// </summary>
    public virtual bool ShouldSerializesetExpanded()
    {
        if (_shouldSerializesetExpanded)
        {
            return true;
        }
        return (_setExpanded != default(toggleType));
    }
    
    /// <summary>
    /// Test whether selectedItemSet should be serialized
    /// </summary>
    public virtual bool ShouldSerializeselectedItemSet()
    {
        return !string.IsNullOrEmpty(selectedItemSet);
    }
    
    /// <summary>
    /// Test whether targetNameActivationList should be serialized
    /// </summary>
    public virtual bool ShouldSerializetargetNameActivationList()
    {
        return !string.IsNullOrEmpty(targetNameActivationList);
    }
}
}
#pragma warning restore
