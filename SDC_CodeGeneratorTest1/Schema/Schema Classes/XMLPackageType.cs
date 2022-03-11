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
[JsonObject("XMLPackageType")]
public partial class XMLPackageType : ExtensionBaseType
{
    private ExtensionBaseType _item;
    private List<FormDesignType> _formDesign;
    private List<LinkType> _formDesignPkgLink;
    private List<DataElementType> _dataElement;
    private List<MappingType> _mapTemplate;
    private List<XMLPackageTypeReportDesignTemplate> _reportDesignTemplate;
    private List<XMLPackageTypeHelperFile> _helperFile;
    private bool _itemSpecified;
    private bool _formDesignSpecified;
    private bool _formDesignPkgLinkSpecified;
    private bool _dataElementSpecified;
    private bool _mapTemplateSpecified;
    private bool _reportDesignTemplateSpecified;
    private bool _helperFileSpecified;
    [XmlElement("DemogFormDesign", typeof(FormDesignType), Order=0)]
    [XmlElement("DemogFormPkgLink", typeof(LinkType), Order=0)]
    public virtual ExtensionBaseType Item
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
    
    [XmlElement("FormDesign", Order=1)]
    [JsonProperty(Order=1, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<FormDesignType> FormDesign
    {
        get
        {
            return _formDesign;
        }
        set
        {
            if ((_formDesign == value))
            {
                return;
            }
            if (((_formDesign == null) 
                        || (_formDesign.Equals(value) != true)))
            {
                _formDesign = value;
                OnPropertyChanged("FormDesign", value);
            }
        }
    }
    
    /// <summary>
    /// Retrieve raw FormDesign XML from link
    /// (Previously, retrieve a Pkg that wraps FormDesign XML)
    /// </summary>
    [XmlElement("FormDesignPkgLink", Order=2)]
    [JsonProperty(Order=2, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<LinkType> FormDesignPkgLink
    {
        get
        {
            return _formDesignPkgLink;
        }
        set
        {
            if ((_formDesignPkgLink == value))
            {
                return;
            }
            if (((_formDesignPkgLink == null) 
                        || (_formDesignPkgLink.Equals(value) != true)))
            {
                _formDesignPkgLink = value;
                OnPropertyChanged("FormDesignPkgLink", value);
            }
        }
    }
    
    [XmlElement("DataElement", Order=3)]
    [JsonProperty(Order=3, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<DataElementType> DataElement
    {
        get
        {
            return _dataElement;
        }
        set
        {
            if ((_dataElement == value))
            {
                return;
            }
            if (((_dataElement == null) 
                        || (_dataElement.Equals(value) != true)))
            {
                _dataElement = value;
                OnPropertyChanged("DataElement", value);
            }
        }
    }
    
    /// <summary>
    /// Describes mappings between FormDesignTemplate items and data elements, terminologies, databases, XML files, local values, etc.
    /// </summary>
    [XmlElement("MapTemplate", Order=4)]
    [JsonProperty(Order=4, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<MappingType> MapTemplate
    {
        get
        {
            return _mapTemplate;
        }
        set
        {
            if ((_mapTemplate == value))
            {
                return;
            }
            if (((_mapTemplate == null) 
                        || (_mapTemplate.Equals(value) != true)))
            {
                _mapTemplate = value;
                OnPropertyChanged("MapTemplate", value);
            }
        }
    }
    
    /// <summary>
    /// ReportDesignTemplate describes the information content of a report (e.g., sections, questions etc).  This enables control of the presentation view of the user responses derived from a designated FormDesignTemplate.  It allows the report presentation to look substantially different from the data-entry form view defined by the FormDesignTemplate.
    /// </summary>
    [XmlElement("ReportDesignTemplate", Order=5)]
    [JsonProperty(Order=5, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<XMLPackageTypeReportDesignTemplate> ReportDesignTemplate
    {
        get
        {
            return _reportDesignTemplate;
        }
        set
        {
            if ((_reportDesignTemplate == value))
            {
                return;
            }
            if (((_reportDesignTemplate == null) 
                        || (_reportDesignTemplate.Equals(value) != true)))
            {
                _reportDesignTemplate = value;
                OnPropertyChanged("ReportDesignTemplate", value);
            }
        }
    }
    
    [XmlElement("HelperFile", Order=6)]
    [JsonProperty(Order=6, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<XMLPackageTypeHelperFile> HelperFile
    {
        get
        {
            return _helperFile;
        }
        set
        {
            if ((_helperFile == value))
            {
                return;
            }
            if (((_helperFile == null) 
                        || (_helperFile.Equals(value) != true)))
            {
                _helperFile = value;
                OnPropertyChanged("HelperFile", value);
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
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool FormDesignSpecified
    {
        get
        {
            return _formDesignSpecified;
        }
        set
        {
            _formDesignSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool FormDesignPkgLinkSpecified
    {
        get
        {
            return _formDesignPkgLinkSpecified;
        }
        set
        {
            _formDesignPkgLinkSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool DataElementSpecified
    {
        get
        {
            return _dataElementSpecified;
        }
        set
        {
            _dataElementSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool MapTemplateSpecified
    {
        get
        {
            return _mapTemplateSpecified;
        }
        set
        {
            _mapTemplateSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool ReportDesignTemplateSpecified
    {
        get
        {
            return _reportDesignTemplateSpecified;
        }
        set
        {
            _reportDesignTemplateSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool HelperFileSpecified
    {
        get
        {
            return _helperFileSpecified;
        }
        set
        {
            _helperFileSpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether FormDesign should be serialized
    /// </summary>
    public virtual bool ShouldSerializeFormDesign()
    {
        return FormDesign != null && FormDesign.Count > 0;
    }
    
    /// <summary>
    /// Test whether FormDesignPkgLink should be serialized
    /// </summary>
    public virtual bool ShouldSerializeFormDesignPkgLink()
    {
        return FormDesignPkgLink != null && FormDesignPkgLink.Count > 0;
    }
    
    /// <summary>
    /// Test whether DataElement should be serialized
    /// </summary>
    public virtual bool ShouldSerializeDataElement()
    {
        return DataElement != null && DataElement.Count > 0;
    }
    
    /// <summary>
    /// Test whether MapTemplate should be serialized
    /// </summary>
    public virtual bool ShouldSerializeMapTemplate()
    {
        return MapTemplate != null && MapTemplate.Count > 0;
    }
    
    /// <summary>
    /// Test whether ReportDesignTemplate should be serialized
    /// </summary>
    public virtual bool ShouldSerializeReportDesignTemplate()
    {
        return ReportDesignTemplate != null && ReportDesignTemplate.Count > 0;
    }
    
    /// <summary>
    /// Test whether HelperFile should be serialized
    /// </summary>
    public virtual bool ShouldSerializeHelperFile()
    {
        return HelperFile != null && HelperFile.Count > 0;
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
