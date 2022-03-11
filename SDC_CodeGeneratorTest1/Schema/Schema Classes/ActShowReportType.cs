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
/// Show a stored report. The following parameters may be used: reportID:
/// indicator of a report definition in SDC format. packageID: retrieve report as a
/// package with ancillary information. reportInstanceGUID: retreive 1 or more report
/// versions by using a report instance GUID. This may be used in conjunctions with a
/// packageID. reportInstanceVersionGUID: retrieve a single version of a report
/// representing the state of a report when it was saved. This may be used in
/// conjunctions with a packageID.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("ActShowReportType")]
public partial class ActShowReportType : ExtensionBaseType
{
    private string _reportID;
    private string _packageID;
    private string _reportInstanceGuid;
    private string _reportInstanceVersonGuid;
    private string _displayState;
    private bool _reportIDSpecified;
    private bool _packageIDSpecified;
    private bool _reportInstanceGuidSpecified;
    private bool _reportInstanceVersonGuidSpecified;
    private bool _displayStateSpecified;
    /// <summary>
    /// This ID represents the report to be
    /// displayed.
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string reportID
    {
        get
        {
            return _reportID;
        }
        set
        {
            if ((_reportID == value))
            {
                return;
            }
            if (((_reportID == null) 
                        || (_reportID.Equals(value) != true)))
            {
                _reportID = value;
                OnPropertyChanged("reportID", value);
            }
        }
    }
    
    /// <summary>
    /// This ID represents the Package that contains the report to
    /// be displayed.
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string packageID
    {
        get
        {
            return _packageID;
        }
        set
        {
            if ((_packageID == value))
            {
                return;
            }
            if (((_packageID == null) 
                        || (_packageID.Equals(value) != true)))
            {
                _packageID = value;
                OnPropertyChanged("packageID", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string reportInstanceGuid
    {
        get
        {
            return _reportInstanceGuid;
        }
        set
        {
            if ((_reportInstanceGuid == value))
            {
                return;
            }
            if (((_reportInstanceGuid == null) 
                        || (_reportInstanceGuid.Equals(value) != true)))
            {
                _reportInstanceGuid = value;
                OnPropertyChanged("reportInstanceGuid", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string reportInstanceVersonGuid
    {
        get
        {
            return _reportInstanceVersonGuid;
        }
        set
        {
            if ((_reportInstanceVersonGuid == value))
            {
                return;
            }
            if (((_reportInstanceVersonGuid == null) 
                        || (_reportInstanceVersonGuid.Equals(value) != true)))
            {
                _reportInstanceVersonGuid = value;
                OnPropertyChanged("reportInstanceVersonGuid", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string displayState
    {
        get
        {
            return _displayState;
        }
        set
        {
            if ((_displayState == value))
            {
                return;
            }
            if (((_displayState == null) 
                        || (_displayState.Equals(value) != true)))
            {
                _displayState = value;
                OnPropertyChanged("displayState", value);
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool reportIDSpecified
    {
        get
        {
            return _reportIDSpecified;
        }
        set
        {
            _reportIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool packageIDSpecified
    {
        get
        {
            return _packageIDSpecified;
        }
        set
        {
            _packageIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool reportInstanceGuidSpecified
    {
        get
        {
            return _reportInstanceGuidSpecified;
        }
        set
        {
            _reportInstanceGuidSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool reportInstanceVersonGuidSpecified
    {
        get
        {
            return _reportInstanceVersonGuidSpecified;
        }
        set
        {
            _reportInstanceVersonGuidSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool displayStateSpecified
    {
        get
        {
            return _displayStateSpecified;
        }
        set
        {
            _displayStateSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether reportID should be serialized
    /// </summary>
    public virtual bool ShouldSerializereportID()
    {
        return !string.IsNullOrEmpty(reportID);
    }
    
    /// <summary>
    /// Test whether packageID should be serialized
    /// </summary>
    public virtual bool ShouldSerializepackageID()
    {
        return !string.IsNullOrEmpty(packageID);
    }
    
    /// <summary>
    /// Test whether reportInstanceGuid should be serialized
    /// </summary>
    public virtual bool ShouldSerializereportInstanceGuid()
    {
        return !string.IsNullOrEmpty(reportInstanceGuid);
    }
    
    /// <summary>
    /// Test whether reportInstanceVersonGuid should be serialized
    /// </summary>
    public virtual bool ShouldSerializereportInstanceVersonGuid()
    {
        return !string.IsNullOrEmpty(reportInstanceVersonGuid);
    }
    
    /// <summary>
    /// Test whether displayState should be serialized
    /// </summary>
    public virtual bool ShouldSerializedisplayState()
    {
        return !string.IsNullOrEmpty(displayState);
    }
}
}
#pragma warning restore
