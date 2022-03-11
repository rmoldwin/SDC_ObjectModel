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
/// Start here. This is the top level of the SDCFormDesign object model.
/// It represents the definition for the information content of a single data-entry
/// form.
/// </summary>
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[XmlRootAttribute("FormDesign", Namespace="urn:ihe:qrph:sdc:2016", IsNullable=false)]
[JsonObject("FormDesignType")]
public partial class FormDesignType : IdentifiedExtensionType
{
    private bool _shouldSerializecompletionStatus;
    private bool _shouldSerializeapprovalStatus;
    private bool _shouldSerializenewData;
    private bool _shouldSerializechangedData;
    private bool _shouldSerializeinstanceVersionPrev;
    private bool _shouldSerializeinstanceVersion;
    private EventType _beforeLoadForm;
    private EventType _beforeLoadData;
    private EventType _beforeShowForm;
    private EventType _beforeDataSubmit;
    private EventType _beforeCloseForm;
    private List<OnEventType> _onEvent;
    private SectionItemType _header;
    private SectionItemType _body;
    private SectionItemType _footer;
    private RulesType _rules;
    private string _lineage;
    private string _version;
    private string _versionPrev;
    private string _fullURI;
    private string _filename;
    private string _formTitle;
    private string _basedOnURI;
    private string _instanceID;
    private DateTime? _instanceVersion;
    private string _instanceVersionURI;
    private DateTime? _instanceVersionPrev;
    private System.Nullable<FormDesignTypeApprovalStatus> _approvalStatus;
    private System.Nullable<FormDesignTypeCompletionStatus> _completionStatus;
    private bool? _changedData;
    private bool? _newData;
    private bool _beforeLoadFormSpecified;
    private bool _beforeLoadDataSpecified;
    private bool _beforeShowFormSpecified;
    private bool _beforeDataSubmitSpecified;
    private bool _beforeCloseFormSpecified;
    private bool _onEventSpecified;
    private bool _headerSpecified;
    private bool _bodySpecified;
    private bool _footerSpecified;
    private bool _rulesSpecified;
    private bool _lineageSpecified;
    private bool _versionSpecified;
    private bool _versionPrevSpecified;
    private bool _fullURISpecified;
    private bool _filenameSpecified;
    private bool _formTitleSpecified;
    private bool _basedOnURISpecified;
    private bool _instanceIDSpecified;
    private bool _instanceVersionURISpecified;
    /// <summary>
    /// NEW: This event is fired before the page is loaded
    /// into memory, and before stored form data is loaded. It may be used,
    /// e.g., for authentication, to retrieve/prepare stored data, and/or to
    /// control form rendering according to user
    /// preferences.
    /// </summary>
    [XmlElement(Order=0)]
    [JsonProperty(Order=0, NullValueHandling=NullValueHandling.Ignore)]
    public virtual EventType BeforeLoadForm
    {
        get
        {
            return _beforeLoadForm;
        }
        set
        {
            if ((_beforeLoadForm == value))
            {
                return;
            }
            if (((_beforeLoadForm == null) 
                        || (_beforeLoadForm.Equals(value) != true)))
            {
                _beforeLoadForm = value;
                OnPropertyChanged("BeforeLoadForm", value);
            }
        }
    }
    
    /// <summary>
    /// NEW: This event is fired after the page is loaded into
    /// memory, before stored form data is loaded, and before the form is
    /// visible. For example, It may be used to determine the data to be
    /// loaded and to perform the data loading.
    /// </summary>
    [XmlElement(Order=1)]
    [JsonProperty(Order=1, NullValueHandling=NullValueHandling.Ignore)]
    public virtual EventType BeforeLoadData
    {
        get
        {
            return _beforeLoadData;
        }
        set
        {
            if ((_beforeLoadData == value))
            {
                return;
            }
            if (((_beforeLoadData == null) 
                        || (_beforeLoadData.Equals(value) != true)))
            {
                _beforeLoadData = value;
                OnPropertyChanged("BeforeLoadData", value);
            }
        }
    }
    
    /// <summary>
    /// NEW: This event is fired after the page is loaded is
    /// memory, after the data is loaded into the form, but before the form
    /// is displayed. It may be used to perform form activities that are
    /// controlled by the loaded data.
    /// </summary>
    [XmlElement(Order=2)]
    [JsonProperty(Order=2, NullValueHandling=NullValueHandling.Ignore)]
    public virtual EventType BeforeShowForm
    {
        get
        {
            return _beforeShowForm;
        }
        set
        {
            if ((_beforeShowForm == value))
            {
                return;
            }
            if (((_beforeShowForm == null) 
                        || (_beforeShowForm.Equals(value) != true)))
            {
                _beforeShowForm = value;
                OnPropertyChanged("BeforeShowForm", value);
            }
        }
    }
    
    [XmlElement(Order=3)]
    [JsonProperty(Order=3, NullValueHandling=NullValueHandling.Ignore)]
    public virtual EventType BeforeDataSubmit
    {
        get
        {
            return _beforeDataSubmit;
        }
        set
        {
            if ((_beforeDataSubmit == value))
            {
                return;
            }
            if (((_beforeDataSubmit == null) 
                        || (_beforeDataSubmit.Equals(value) != true)))
            {
                _beforeDataSubmit = value;
                OnPropertyChanged("BeforeDataSubmit", value);
            }
        }
    }
    
    [XmlElement(Order=4)]
    [JsonProperty(Order=4, NullValueHandling=NullValueHandling.Ignore)]
    public virtual EventType BeforeCloseForm
    {
        get
        {
            return _beforeCloseForm;
        }
        set
        {
            if ((_beforeCloseForm == value))
            {
                return;
            }
            if (((_beforeCloseForm == null) 
                        || (_beforeCloseForm.Equals(value) != true)))
            {
                _beforeCloseForm = value;
                OnPropertyChanged("BeforeCloseForm", value);
            }
        }
    }
    
    /// <summary>
    /// Generic event handler - eventName must be
    /// specified.
    /// </summary>
    [XmlElement("OnEvent", Order=5)]
    [JsonProperty(Order=5, NullValueHandling=NullValueHandling.Ignore)]
    public virtual List<OnEventType> OnEvent
    {
        get
        {
            return _onEvent;
        }
        set
        {
            if ((_onEvent == value))
            {
                return;
            }
            if (((_onEvent == null) 
                        || (_onEvent.Equals(value) != true)))
            {
                _onEvent = value;
                OnPropertyChanged("OnEvent", value);
            }
        }
    }
    
    /// <summary>
    /// Optional Section that stays at the top of a
    /// form.
    /// </summary>
    [XmlElement(Order=6)]
    [JsonProperty(Order=6, NullValueHandling=NullValueHandling.Ignore)]
    public virtual SectionItemType Header
    {
        get
        {
            return _header;
        }
        set
        {
            if ((_header == value))
            {
                return;
            }
            if (((_header == null) 
                        || (_header.Equals(value) != true)))
            {
                _header = value;
                OnPropertyChanged("Header", value);
            }
        }
    }
    
    /// <summary>
    /// Main Section of form
    /// </summary>
    [XmlElement(Order=7)]
    [JsonProperty(Order=7, NullValueHandling=NullValueHandling.Ignore)]
    public virtual SectionItemType Body
    {
        get
        {
            return _body;
        }
        set
        {
            if ((_body == value))
            {
                return;
            }
            if (((_body == null) 
                        || (_body.Equals(value) != true)))
            {
                _body = value;
                OnPropertyChanged("Body", value);
            }
        }
    }
    
    /// <summary>
    /// Optional Section that stays at the bottom of a
    /// form.
    /// </summary>
    [XmlElement(Order=8)]
    [JsonProperty(Order=8, NullValueHandling=NullValueHandling.Ignore)]
    public virtual SectionItemType Footer
    {
        get
        {
            return _footer;
        }
        set
        {
            if ((_footer == value))
            {
                return;
            }
            if (((_footer == null) 
                        || (_footer.Equals(value) != true)))
            {
                _footer = value;
                OnPropertyChanged("Footer", value);
            }
        }
    }
    
    [XmlElement(Order=9)]
    [JsonProperty(Order=9, NullValueHandling=NullValueHandling.Ignore)]
    public virtual RulesType Rules
    {
        get
        {
            return _rules;
        }
        set
        {
            if ((_rules == value))
            {
                return;
            }
            if (((_rules == null) 
                        || (_rules.Equals(value) != true)))
            {
                _rules = value;
                OnPropertyChanged("Rules", value);
            }
        }
    }
    
    /// <summary>
    /// A string identifier that is used to group multiple
    /// versions of a single form. The lineage is constant for all versions of a
    /// single kind of form. When appended to baseURI, it can be used to
    /// retrieve all versions of one particular form. Example:
    /// @lineage="Lung.Bmk.227"
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string lineage
    {
        get
        {
            return _lineage;
        }
        set
        {
            if ((_lineage == value))
            {
                return;
            }
            if (((_lineage == null) 
                        || (_lineage.Equals(value) != true)))
            {
                _lineage = value;
                OnPropertyChanged("lineage", value);
            }
        }
    }
    
    /// <summary>
    /// @version contains the version text for the current form.
    /// It is designed to be used in conjunction with @baseURI and @lineage.
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string version
    {
        get
        {
            return _version;
        }
        set
        {
            if ((_version == value))
            {
                return;
            }
            if (((_version == null) 
                        || (_version.Equals(value) != true)))
            {
                _version = value;
                OnPropertyChanged("version", value);
            }
        }
    }
    
    /// <summary>
    /// @versionPrev identifies the immediate previous version of
    /// the current FDF. The format is the same as version. The primary role of
    /// this optional attribute is to allow automated comparisons between a
    /// current FDF and the immediate previous FDF version. This is often
    /// helpful when deciding whether to adopt a newer version of an FDF.
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string versionPrev
    {
        get
        {
            return _versionPrev;
        }
        set
        {
            if ((_versionPrev == value))
            {
                return;
            }
            if (((_versionPrev == null) 
                        || (_versionPrev.Equals(value) != true)))
            {
                _versionPrev = value;
                OnPropertyChanged("versionPrev", value);
            }
        }
    }
    
    /// <summary>
    /// The full URI that uniquely identifies the current form. It
    /// is created by concatenating @baseURI + lineage + version. Each of the
    /// components is separated by a single forward slash.
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string fullURI
    {
        get
        {
            return _fullURI;
        }
        set
        {
            if ((_fullURI == value))
            {
                return;
            }
            if (((_fullURI == null) 
                        || (_fullURI.Equals(value) != true)))
            {
                _fullURI = value;
                OnPropertyChanged("fullURI", value);
            }
        }
    }
    
    /// <summary>
    /// @filename is the filename of the FDF when is saved to a
    /// file storage device (e.g., a disk or USB drive). The filename appears
    /// inside the FDF XML to help ensure the identity of the FDF content in
    /// case the saved filename (on a disk drive, etc.) has been changed for any
    /// reason.
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string filename
    {
        get
        {
            return _filename;
        }
        set
        {
            if ((_filename == value))
            {
                return;
            }
            if (((_filename == null) 
                        || (_filename.Equals(value) != true)))
            {
                _filename = value;
                OnPropertyChanged("filename", value);
            }
        }
    }
    
    /// <summary>
    /// @formTitle is a human readable title for display when
    /// choosing forms. Added 4/27/16
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string formTitle
    {
        get
        {
            return _formTitle;
        }
        set
        {
            if ((_formTitle == value))
            {
                return;
            }
            if (((_formTitle == null) 
                        || (_formTitle.Equals(value) != true)))
            {
                _formTitle = value;
                OnPropertyChanged("formTitle", value);
            }
        }
    }
    
    /// <summary>
    /// @basedOnURI is a URI that identifies the SDC form that
    /// that the current FDF is based upon. In most cases, this should be a
    /// standard SDC form that is modified and/or extended by the current FDF.
    /// It’s best to avoid using prefixes like "http://" or "https://" because
    /// these can occasionally cause XML validation errors when used in a
    /// URI-typed field. The URI format should be the same format used in
    /// fullURI, which is patterned after the SDC web service API.
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string basedOnURI
    {
        get
        {
            return _basedOnURI;
        }
        set
        {
            if ((_basedOnURI == value))
            {
                return;
            }
            if (((_basedOnURI == null) 
                        || (_basedOnURI.Equals(value) != true)))
            {
                _basedOnURI = value;
                OnPropertyChanged("basedOnURI", value);
            }
        }
    }
    
    /// <summary>
    /// @instanceID is unique string (e.g., a GUID) used to
    /// identify a unique instance of a form, such as a form used during a
    /// single patient encounter. The @instanceID is used to track saved form
    /// responses across time and across multiple episodes of editing by
    /// end-users. This string does not change for each edit session of a form
    /// or package instance. The @instanceID is required in an FDF-R; It is not
    /// allowed in an FDF.
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string instanceID
    {
        get
        {
            return _instanceID;
        }
        set
        {
            if ((_instanceID == value))
            {
                return;
            }
            if (((_instanceID == null) 
                        || (_instanceID.Equals(value) != true)))
            {
                _instanceID = value;
                OnPropertyChanged("instanceID", value);
            }
        }
    }
    
    /// <summary>
    /// @instanceVersion Timestamp is used to identify a unique instance of a form.
    /// Used for tracking form responses across time and across multiple
    /// episodes of editing by end-users. This field must change for each edit
    /// session of a form instance. The instanceVersion is required in an FDF-R;
    /// It is not allowed in an FDF.
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual System.DateTime instanceVersion
    {
        get
        {
            if (_instanceVersion.HasValue)
            {
                return _instanceVersion.Value;
            }
            else
            {
                return default(System.DateTime);
            }
        }
        set
        {
            if ((_instanceVersion.Equals(value) != true))
            {
                _instanceVersion = value;
                OnPropertyChanged("instanceVersion", value);
            }
            _shouldSerializeinstanceVersion = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool instanceVersionSpecified
    {
        get
        {
            return _instanceVersion.HasValue;
        }
        set
        {
            if (value==false)
            {
                _instanceVersion = null;
            }
        }
    }
    
    /// <summary>
    /// NEW: Globally-unique URI used to identify a unique
    /// instance of a form's saved responses. It is used for tracking form
    /// responses across time and across multiple episodes of editing by
    /// end-users. The instanceVersionURI must change for each edit/save session
    /// of a form instance (defined by instanceVersion). The instanceVersionURI
    /// should be formatted similarly to the fullURI but must include values for
    /// instanceID and instanceVersion. The instanceVersion value is the release
    /// date/time for the new version, in W3C datetime format. An example
    /// instanceVersionURI is:
    /// instanceVersionURI="_baseURI=cap.org&_lineage=Lung.Bmk.227&_version=1.001.011.RC1
    /// &_instanceID=Abc1dee2fg987&_instanceVersion=2019-07-16T19:20:30+01:00&_docType=sdcFDFR
    /// " It is possible to create a shorter URI without the _baseURI, _lineage
    /// and _version parameters, as long as the URI is able to globally and
    /// uniquely identify and retrieve the instance and version of the FDF-R
    /// that was transmitted:
    /// instanceVersionURI="_instanceID=Abc1dee2fg987&_instanceVersion=2019-07-16T19:20:30+01:00&_docType=sdcFDFR"
    /// Note that the FR webservice endpoint URI is not provided in the
    /// instanceVersionURI. The FR endpoint and its security settings may be
    /// found in the SDC Package that contains the FDF-R, at
    /// SDCPackage/SubmissionRule. An FR may also be provided in a custom FDF
    /// Property if desired. The docType for instanceVersionURI is sdcFDFR. The
    /// specific order of components shown in the URI examples is not required,
    /// but the component order shown above is suggested for consistency and
    /// readability. The instanceVersionURI is not required, and is not allowed
    /// in an FDF.
    /// </summary>
    [XmlAttribute(DataType="anyURI")]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string instanceVersionURI
    {
        get
        {
            return _instanceVersionURI;
        }
        set
        {
            if ((_instanceVersionURI == value))
            {
                return;
            }
            if (((_instanceVersionURI == null) 
                        || (_instanceVersionURI.Equals(value) != true)))
            {
                _instanceVersionURI = value;
                OnPropertyChanged("instanceVersionURI", value);
            }
        }
    }
    
    /// <summary>
    /// NEW: Unique dateTime used to identify the immediate
    /// previous instance of an form instance. Used for tracking form responses
    /// across time and across multiple episodes of editing by end-users. This
    /// field must change for each edit session of a form instance.
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual System.DateTime instanceVersionPrev
    {
        get
        {
            if (_instanceVersionPrev.HasValue)
            {
                return _instanceVersionPrev.Value;
            }
            else
            {
                return default(System.DateTime);
            }
        }
        set
        {
            if ((_instanceVersionPrev.Equals(value) != true))
            {
                _instanceVersionPrev = value;
                OnPropertyChanged("instanceVersionPrev", value);
            }
            _shouldSerializeinstanceVersionPrev = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool instanceVersionPrevSpecified
    {
        get
        {
            return _instanceVersionPrev.HasValue;
        }
        set
        {
            if (value==false)
            {
                _instanceVersionPrev = null;
            }
        }
    }
    
    /// <summary>
    /// Describes report fitness for clinical or other action
    /// inProcess: currently being edited, users should not rely on results
    /// preliminary: report is awaiting final review and approval approved:
    /// report is fit for clinical or other action; often synonymous with final
    /// cancelled: report/procedure has been aborted before issued retracted:
    /// report has been deemed unfit for clinical or other action
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual FormDesignTypeApprovalStatus approvalStatus
    {
        get
        {
            if (_approvalStatus.HasValue)
            {
                return _approvalStatus.Value;
            }
            else
            {
                return default(FormDesignTypeApprovalStatus);
            }
        }
        set
        {
            if ((_approvalStatus.Equals(value) != true))
            {
                _approvalStatus = value;
                OnPropertyChanged("approvalStatus", value);
            }
            _shouldSerializeapprovalStatus = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool approvalStatusSpecified
    {
        get
        {
            return _approvalStatus.HasValue;
        }
        set
        {
            if (value==false)
            {
                _approvalStatus = null;
            }
        }
    }
    
    /// <summary>
    /// The extent to which a report contains all of the requested
    /// information pending: no information is yet available incomplete: some
    /// requested information is not yet available complete: all information is
    /// available in the requested report
    /// </summary>
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public virtual FormDesignTypeCompletionStatus completionStatus
    {
        get
        {
            if (_completionStatus.HasValue)
            {
                return _completionStatus.Value;
            }
            else
            {
                return default(FormDesignTypeCompletionStatus);
            }
        }
        set
        {
            if ((_completionStatus.Equals(value) != true))
            {
                _completionStatus = value;
                OnPropertyChanged("completionStatus", value);
            }
            _shouldSerializecompletionStatus = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool completionStatusSpecified
    {
        get
        {
            return _completionStatus.HasValue;
        }
        set
        {
            if (value==false)
            {
                _completionStatus = null;
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool changedData
    {
        get
        {
            if (_changedData.HasValue)
            {
                return _changedData.Value;
            }
            else
            {
                return default(bool);
            }
        }
        set
        {
            if ((_changedData.Equals(value) != true))
            {
                _changedData = value;
                OnPropertyChanged("changedData", value);
            }
            _shouldSerializechangedData = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool changedDataSpecified
    {
        get
        {
            return _changedData.HasValue;
        }
        set
        {
            if (value==false)
            {
                _changedData = null;
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool newData
    {
        get
        {
            if (_newData.HasValue)
            {
                return _newData.Value;
            }
            else
            {
                return default(bool);
            }
        }
        set
        {
            if ((_newData.Equals(value) != true))
            {
                _newData = value;
                OnPropertyChanged("newData", value);
            }
            _shouldSerializenewData = true;
        }
    }
    
    [XmlIgnore]
    public virtual bool newDataSpecified
    {
        get
        {
            return _newData.HasValue;
        }
        set
        {
            if (value==false)
            {
                _newData = null;
            }
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool BeforeLoadFormSpecified
    {
        get
        {
            return _beforeLoadFormSpecified;
        }
        set
        {
            _beforeLoadFormSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool BeforeLoadDataSpecified
    {
        get
        {
            return _beforeLoadDataSpecified;
        }
        set
        {
            _beforeLoadDataSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool BeforeShowFormSpecified
    {
        get
        {
            return _beforeShowFormSpecified;
        }
        set
        {
            _beforeShowFormSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool BeforeDataSubmitSpecified
    {
        get
        {
            return _beforeDataSubmitSpecified;
        }
        set
        {
            _beforeDataSubmitSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool BeforeCloseFormSpecified
    {
        get
        {
            return _beforeCloseFormSpecified;
        }
        set
        {
            _beforeCloseFormSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool OnEventSpecified
    {
        get
        {
            return _onEventSpecified;
        }
        set
        {
            _onEventSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool HeaderSpecified
    {
        get
        {
            return _headerSpecified;
        }
        set
        {
            _headerSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool BodySpecified
    {
        get
        {
            return _bodySpecified;
        }
        set
        {
            _bodySpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool FooterSpecified
    {
        get
        {
            return _footerSpecified;
        }
        set
        {
            _footerSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool RulesSpecified
    {
        get
        {
            return _rulesSpecified;
        }
        set
        {
            _rulesSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool lineageSpecified
    {
        get
        {
            return _lineageSpecified;
        }
        set
        {
            _lineageSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool versionSpecified
    {
        get
        {
            return _versionSpecified;
        }
        set
        {
            _versionSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool versionPrevSpecified
    {
        get
        {
            return _versionPrevSpecified;
        }
        set
        {
            _versionPrevSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool fullURISpecified
    {
        get
        {
            return _fullURISpecified;
        }
        set
        {
            _fullURISpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool filenameSpecified
    {
        get
        {
            return _filenameSpecified;
        }
        set
        {
            _filenameSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool formTitleSpecified
    {
        get
        {
            return _formTitleSpecified;
        }
        set
        {
            _formTitleSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool basedOnURISpecified
    {
        get
        {
            return _basedOnURISpecified;
        }
        set
        {
            _basedOnURISpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool instanceIDSpecified
    {
        get
        {
            return _instanceIDSpecified;
        }
        set
        {
            _instanceIDSpecified = value;
        }
    }
    
    [JsonIgnore]
    [XmlIgnore()]
    public bool instanceVersionURISpecified
    {
        get
        {
            return _instanceVersionURISpecified;
        }
        set
        {
            _instanceVersionURISpecified = value;
        }
    }
    
    /// <summary>
    /// Test whether OnEvent should be serialized
    /// </summary>
    public virtual bool ShouldSerializeOnEvent()
    {
        return OnEvent != null && OnEvent.Count > 0;
    }
    
    /// <summary>
    /// Test whether instanceVersion should be serialized
    /// </summary>
    public virtual bool ShouldSerializeinstanceVersion()
    {
        if (_shouldSerializeinstanceVersion)
        {
            return true;
        }
        return (instanceVersion != default(System.DateTime));
    }
    
    /// <summary>
    /// Test whether instanceVersionPrev should be serialized
    /// </summary>
    public virtual bool ShouldSerializeinstanceVersionPrev()
    {
        if (_shouldSerializeinstanceVersionPrev)
        {
            return true;
        }
        return (instanceVersionPrev != default(System.DateTime));
    }
    
    /// <summary>
    /// Test whether changedData should be serialized
    /// </summary>
    public virtual bool ShouldSerializechangedData()
    {
        if (_shouldSerializechangedData)
        {
            return true;
        }
        return (changedData != default(bool));
    }
    
    /// <summary>
    /// Test whether newData should be serialized
    /// </summary>
    public virtual bool ShouldSerializenewData()
    {
        if (_shouldSerializenewData)
        {
            return true;
        }
        return (newData != default(bool));
    }
    
    /// <summary>
    /// Test whether approvalStatus should be serialized
    /// </summary>
    public virtual bool ShouldSerializeapprovalStatus()
    {
        if (_shouldSerializeapprovalStatus)
        {
            return true;
        }
        return (approvalStatus != default(FormDesignTypeApprovalStatus));
    }
    
    /// <summary>
    /// Test whether completionStatus should be serialized
    /// </summary>
    public virtual bool ShouldSerializecompletionStatus()
    {
        if (_shouldSerializecompletionStatus)
        {
            return true;
        }
        return (completionStatus != default(FormDesignTypeCompletionStatus));
    }
    
    /// <summary>
    /// Test whether BeforeLoadForm should be serialized
    /// </summary>
    public virtual bool ShouldSerializeBeforeLoadForm()
    {
        return (BeforeLoadForm != null);
    }
    
    /// <summary>
    /// Test whether BeforeLoadData should be serialized
    /// </summary>
    public virtual bool ShouldSerializeBeforeLoadData()
    {
        return (BeforeLoadData != null);
    }
    
    /// <summary>
    /// Test whether BeforeShowForm should be serialized
    /// </summary>
    public virtual bool ShouldSerializeBeforeShowForm()
    {
        return (BeforeShowForm != null);
    }
    
    /// <summary>
    /// Test whether BeforeDataSubmit should be serialized
    /// </summary>
    public virtual bool ShouldSerializeBeforeDataSubmit()
    {
        return (BeforeDataSubmit != null);
    }
    
    /// <summary>
    /// Test whether BeforeCloseForm should be serialized
    /// </summary>
    public virtual bool ShouldSerializeBeforeCloseForm()
    {
        return (BeforeCloseForm != null);
    }
    
    /// <summary>
    /// Test whether Header should be serialized
    /// </summary>
    public virtual bool ShouldSerializeHeader()
    {
        return (Header != null);
    }
    
    /// <summary>
    /// Test whether Body should be serialized
    /// </summary>
    public virtual bool ShouldSerializeBody()
    {
        return (Body != null);
    }
    
    /// <summary>
    /// Test whether Footer should be serialized
    /// </summary>
    public virtual bool ShouldSerializeFooter()
    {
        return (Footer != null);
    }
    
    /// <summary>
    /// Test whether Rules should be serialized
    /// </summary>
    public virtual bool ShouldSerializeRules()
    {
        return (Rules != null);
    }
    
    /// <summary>
    /// Test whether lineage should be serialized
    /// </summary>
    public virtual bool ShouldSerializelineage()
    {
        return !string.IsNullOrEmpty(lineage);
    }
    
    /// <summary>
    /// Test whether version should be serialized
    /// </summary>
    public virtual bool ShouldSerializeversion()
    {
        return !string.IsNullOrEmpty(version);
    }
    
    /// <summary>
    /// Test whether versionPrev should be serialized
    /// </summary>
    public virtual bool ShouldSerializeversionPrev()
    {
        return !string.IsNullOrEmpty(versionPrev);
    }
    
    /// <summary>
    /// Test whether fullURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializefullURI()
    {
        return !string.IsNullOrEmpty(fullURI);
    }
    
    /// <summary>
    /// Test whether filename should be serialized
    /// </summary>
    public virtual bool ShouldSerializefilename()
    {
        return !string.IsNullOrEmpty(filename);
    }
    
    /// <summary>
    /// Test whether formTitle should be serialized
    /// </summary>
    public virtual bool ShouldSerializeformTitle()
    {
        return !string.IsNullOrEmpty(formTitle);
    }
    
    /// <summary>
    /// Test whether basedOnURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializebasedOnURI()
    {
        return !string.IsNullOrEmpty(basedOnURI);
    }
    
    /// <summary>
    /// Test whether instanceID should be serialized
    /// </summary>
    public virtual bool ShouldSerializeinstanceID()
    {
        return !string.IsNullOrEmpty(instanceID);
    }
    
    /// <summary>
    /// Test whether instanceVersionURI should be serialized
    /// </summary>
    public virtual bool ShouldSerializeinstanceVersionURI()
    {
        return !string.IsNullOrEmpty(instanceVersionURI);
    }
}
}
#pragma warning restore
