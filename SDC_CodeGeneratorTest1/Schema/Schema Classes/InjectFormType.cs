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
/// CHANGED: This type represents a form or portion of a form that is
/// imported into the current form at a specific location. It allows the composition of
/// forms from other forms or parts of other forms. In practice, using an injected
/// section will requiresome or all of the injected FormDesignXML to be injected under
/// this the InjectForm element. For that reason, the schema supports those elements to
/// appear inline. However, in a "raw" form (not yet filled out), the FormDesign element
/// would generally be empty; only the top-level InjectFormType attributes would be used
/// to point to the parts to be later injected. Form parts to be injected are specified
/// by packageID, not FormID. This allows an injected form to be assocaited with helper
/// files, or to return previosuly completed form parts containing responses.
/// </summary>
[XmlInclude(typeof(ActInjectType))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("InjectFormType")]
public partial class InjectFormType : IdentifiedExtensionType
{
    private IdentifiedExtensionType _item;
    private string _injectionSourceURI;
    private string _rootItemID;
    private string _serverURI;
    private string _repeat;
    private string _instanceGUID;
    private string _parentGUID;
    private string _x_fullURI;
    private string _x_pkgFullURI;
    private string _x_pkgBaseURI;
    private bool _itemSpecified;
    private bool _injectionSourceURISpecified;
    private bool _rootItemIDSpecified;
    private bool _serverURISpecified;
    private bool _repeatSpecified;
    private bool _instanceGUIDSpecified;
    private bool _parentGUIDSpecified;
    private bool _x_fullURISpecified;
    private bool _x_pkgFullURISpecified;
    private bool _x_pkgBaseURISpecified;
    /// <summary>
    /// InjectFormType class constructor
    /// </summary>
    public InjectFormType()
    {
        _repeat = "0";
    }
    
    [XmlElement("FormDesign", typeof(FormDesignType), Order=0)]
    [XmlElement("Question", typeof(QuestionItemType), Order=0)]
    [XmlElement("Section", typeof(SectionItemType), Order=0)]
    public virtual IdentifiedExtensionType Item
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
    
    /// <summary>
    /// NEW (2/24/2022): The source of the SDC FormDesign, Section or Question to inject
    /// The URI is formed from :
    /// 
    /// serverURI + \packageID (as described in the SDC Technical Reference Guide [TRG])
    /// Retrieves a package containing no FDF responses (contains FDF(s) only)
    /// -OR-
    /// serverURI + \fullURI (as described in the TRG)
    /// Retrieves the latest package version with FDF responses (contains the latest FDF-R content)
    /// -OR-
    /// serverURI +\instanceVersionURI (as described in the TRG)
    /// Retrieves a specific package version with FDF responses  (contains the FDF-R content from a specific point in time)
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string InjectionSourceURI
    {
        get
        {
            return _injectionSourceURI;
        }
        set
        {
            if ((_injectionSourceURI == value))
            {
                return;
            }
            if (((_injectionSourceURI == null) 
                        || (_injectionSourceURI.Equals(value) != true)))
            {
                _injectionSourceURI = value;
                OnPropertyChanged("InjectionSourceURI", value);
            }
        }
    }
    
    /// <summary>
    /// The rootItemID is the ID of the form or form part that
    /// will be injected. It must point to a valid FormDesign, Section or
    /// Question element.
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string rootItemID
    {
        get
        {
            return _rootItemID;
        }
        set
        {
            if ((_rootItemID == value))
            {
                return;
            }
            if (((_rootItemID == null) 
                        || (_rootItemID.Equals(value) != true)))
            {
                _rootItemID = value;
                OnPropertyChanged("rootItemID", value);
            }
        }
    }
    
    /// <summary>
    /// The server from which the injected package will be
    /// retrieved. Former name "pkgManagerURI"
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string serverURI
    {
        get
        {
            return _serverURI;
        }
        set
        {
            if ((_serverURI == value))
            {
                return;
            }
            if (((_serverURI == null) 
                        || (_serverURI.Equals(value) != true)))
            {
                _serverURI = value;
                OnPropertyChanged("serverURI", value);
            }
        }
    }
    
    [XmlAttribute(DataType="nonNegativeInteger")]
    [DefaultValue("0")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string repeat
    {
        get
        {
            return _repeat;
        }
        set
        {
            if ((_repeat == value))
            {
                return;
            }
            if (((_repeat == null) 
                        || (_repeat.Equals(value) != true)))
            {
                _repeat = value;
                OnPropertyChanged("repeat", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string instanceGUID
    {
        get
        {
            return _instanceGUID;
        }
        set
        {
            if ((_instanceGUID == value))
            {
                return;
            }
            if (((_instanceGUID == null) 
                        || (_instanceGUID.Equals(value) != true)))
            {
                _instanceGUID = value;
                OnPropertyChanged("instanceGUID", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string parentGUID
    {
        get
        {
            return _parentGUID;
        }
        set
        {
            if ((_parentGUID == value))
            {
                return;
            }
            if (((_parentGUID == null) 
                        || (_parentGUID.Equals(value) != true)))
            {
                _parentGUID = value;
                OnPropertyChanged("parentGUID", value);
            }
        }
    }
    
    /// <summary>
    /// NEW: The full URI that uniquely identifies the current package instance.
    /// This URI does not vary with updated versions of the package instance.
    /// This URI does not include the server address, from which the package is retrieved (the Form Manager).
    /// (The Form Manager server address is found in pkgManagerURI).
    /// Removed 2/24/2022
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string X_fullURI
    {
        get
        {
            return _x_fullURI;
        }
        set
        {
            if ((_x_fullURI == value))
            {
                return;
            }
            if (((_x_fullURI == null) 
                        || (_x_fullURI.Equals(value) != true)))
            {
                _x_fullURI = value;
                OnPropertyChanged("X_fullURI", value);
            }
        }
    }
    
    /// <summary>
    /// The injected package is retrieved form pkgManagerURI + "/" + pkgFullURI.
    /// If pkgFullURI is null, then then current FormDesign is used as the source for injection.
    /// Removed 2/24/2022
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string X_pkgFullURI
    {
        get
        {
            return _x_pkgFullURI;
        }
        set
        {
            if ((_x_pkgFullURI == value))
            {
                return;
            }
            if (((_x_pkgFullURI == null) 
                        || (_x_pkgFullURI.Equals(value) != true)))
            {
                _x_pkgFullURI = value;
                OnPropertyChanged("X_pkgFullURI", value);
            }
        }
    }
    
    /// <summary>
    /// DRAFT: The baseURI of the package, which indicates the
    /// home source of the package.
    /// 
    /// pkgManagerURI + /pkgBaseURI + /pkgID
    /// are concatenated to retrieve an empty form, wrapped in SDCPackage.
    /// 
    /// pkgManagerURI + /pkgBaseURI + /pkgInstanceVersionURI
    /// are concatenated to retrieve a specific version of
    /// a populated form, wrapped in SDCPackage.
    /// 
    /// pkgManagerURI + /pkgBaseURI + /pkgInstanceURI
    /// are concatenated to retrieve the latest instance of a
    /// populated form, wrapped in SDCPackage.
    /// Removed 2/24/2022
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string X_pkgBaseURI
    {
        get
        {
            return _x_pkgBaseURI;
        }
        set
        {
            if ((_x_pkgBaseURI == value))
            {
                return;
            }
            if (((_x_pkgBaseURI == null) 
                        || (_x_pkgBaseURI.Equals(value) != true)))
            {
                _x_pkgBaseURI = value;
                OnPropertyChanged("X_pkgBaseURI", value);
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
    public bool InjectionSourceURISpecified
    {
        get
        {
            return _injectionSourceURISpecified;
        }
        set
        {
            _injectionSourceURISpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool rootItemIDSpecified
    {
        get
        {
            return _rootItemIDSpecified;
        }
        set
        {
            _rootItemIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool serverURISpecified
    {
        get
        {
            return _serverURISpecified;
        }
        set
        {
            _serverURISpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool repeatSpecified
    {
        get
        {
            return _repeatSpecified;
        }
        set
        {
            _repeatSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool instanceGUIDSpecified
    {
        get
        {
            return _instanceGUIDSpecified;
        }
        set
        {
            _instanceGUIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool parentGUIDSpecified
    {
        get
        {
            return _parentGUIDSpecified;
        }
        set
        {
            _parentGUIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool X_fullURISpecified
    {
        get
        {
            return _x_fullURISpecified;
        }
        set
        {
            _x_fullURISpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool X_pkgFullURISpecified
    {
        get
        {
            return _x_pkgFullURISpecified;
        }
        set
        {
            _x_pkgFullURISpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool X_pkgBaseURISpecified
    {
        get
        {
            return _x_pkgBaseURISpecified;
        }
        set
        {
            _x_pkgBaseURISpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether Item should be serialized
    /// </summary>
    public virtual bool ShouldSerializeItem()
    {
        return (Item != null);
    }
    
    /// <summary>
    /// Test whether InjectionSourceURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializeInjectionSourceURI()
    {
        return !string.IsNullOrEmpty(InjectionSourceURI);
    }
    
    /// <summary>
    /// Test whether rootItemID should be serialized
    /// </summary>
    public virtual bool ShouldSerializerootItemID()
    {
        return !string.IsNullOrEmpty(rootItemID);
    }
    
    /// <summary>
    /// Test whether serverURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializeserverURI()
    {
        return !string.IsNullOrEmpty(serverURI);
    }
    
    /// <summary>
    /// Test whether repeat should be serialized
    /// </summary>
    public virtual bool ShouldSerializerepeat()
    {
        return !string.IsNullOrEmpty(repeat);
    }
    
    /// <summary>
    /// Test whether instanceGUID should be serialized
    /// </summary>
    public virtual bool ShouldSerializeinstanceGUID()
    {
        return !string.IsNullOrEmpty(instanceGUID);
    }
    
    /// <summary>
    /// Test whether parentGUID should be serialized
    /// </summary>
    public virtual bool ShouldSerializeparentGUID()
    {
        return !string.IsNullOrEmpty(parentGUID);
    }
    
    /// <summary>
    /// Test whether X_fullURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializeX_fullURI()
    {
        return !string.IsNullOrEmpty(X_fullURI);
    }
    
    /// <summary>
    /// Test whether X_pkgFullURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializeX_pkgFullURI()
    {
        return !string.IsNullOrEmpty(X_pkgFullURI);
    }
    
    /// <summary>
    /// Test whether X_pkgBaseURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializeX_pkgBaseURI()
    {
        return !string.IsNullOrEmpty(X_pkgBaseURI);
    }
}
}
#pragma warning restore
