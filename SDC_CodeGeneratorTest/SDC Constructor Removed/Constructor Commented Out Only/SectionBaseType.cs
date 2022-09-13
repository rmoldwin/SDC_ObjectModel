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
/// This base item is the same as the SectionItemType, except it lacks the
/// recursion created by the inclusion of MainItems sub-group.
/// </summary>
[XmlInclude(typeof(SectionItemType))]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
[Serializable]
[DesignerCategoryAttribute("code")]
[XmlTypeAttribute(Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("SectionBaseType")]
public abstract partial class SectionBaseType : RepeatingType
{
    #region Private fields
    protected internal bool _shouldSerializenewData;
    protected internal bool _shouldSerializechangedData;
    protected internal bool _shouldSerializeordered;
    private bool _ordered;
    private bool _changedData;
    private bool _newData;
    #endregion
    
    ///// <summary>
    ///// SectionBaseType class constructor
    ///// </summary>
    //public SectionBaseType()
    //{
    //    _ordered = true;
    //}
    
    /// <summary>
    /// If false, then the form implementation may change the
    /// order of items in the section.
    /// </summary>
    [XmlAttribute]
    [DefaultValue(true)]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool ordered
    {
        get
        {
            return _ordered;
        }
        set
        {
            if ((_ordered.Equals(value) != true))
            {
                _ordered = value;
                OnPropertyChanged("ordered", value);
            }
            _shouldSerializeordered = true;
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool changedData
    {
        get
        {
            return _changedData;
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
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual bool newData
    {
        get
        {
            return _newData;
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

		/// <summary>
		/// Test whether ordered should be serialized
		/// </summary>
		public virtual bool ShouldSerializeordered()
		{
			return _shouldSerializeordered;

			//if (_shouldSerializeordered)
			//	{
			//		return true;
			//	}
			//	return (_ordered != default(bool));
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
        return (_changedData != default(bool));
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
        return (_newData != default(bool));
    }
}
}
#pragma warning restore
