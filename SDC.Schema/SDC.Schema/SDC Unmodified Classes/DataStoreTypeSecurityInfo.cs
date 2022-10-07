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
[XmlTypeAttribute(AnonymousType=true, Namespace="urn:ihe:qrph:sdc:2016")]
[JsonObject("DataStoreTypeSecurityInfo")]
public partial class DataStoreTypeSecurityInfo : string_Stype
{
    #region Private fields
    private string _password;
    private string _userName;
    #endregion
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string password
    {
        get
        {
            return _password;
        }
        set
        {
            if ((_password == value))
            {
                return;
            }
            if (((_password == null) 
                        || (_password.Equals(value) != true)))
            {
                _password = value;
                OnPropertyChanged("password", value);
            }
        }
    }
    
    [XmlAttribute]
    [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
    public virtual string userName
    {
        get
        {
            return _userName;
        }
        set
        {
            if ((_userName == value))
            {
                return;
            }
            if (((_userName == null) 
                        || (_userName.Equals(value) != true)))
            {
                _userName = value;
                OnPropertyChanged("userName", value);
            }
        }
    }
    
    /// <summary>
    /// Test whether password should be serialized
    /// </summary>
    public virtual bool ShouldSerializepassword()
    {
        return !string.IsNullOrEmpty(password);
    }
    
    /// <summary>
    /// Test whether userName should be serialized
    /// </summary>
    public virtual bool ShouldSerializeuserName()
    {
        return !string.IsNullOrEmpty(userName);
    }
}
}
#pragma warning restore
