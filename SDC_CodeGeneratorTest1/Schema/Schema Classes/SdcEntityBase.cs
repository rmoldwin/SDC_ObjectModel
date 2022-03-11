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

#region Base entity class
public partial class SdcEntityBase<T> : INotifyPropertyChanged

{
    private ObjectChangeTracker changeTrackerField;
    private static XmlSerializer _serializerXml;
    private static JsonSerializer _serializerBson;
    private static MessagePackSerializer _serializerMsPack;
    [XmlIgnore()]
    public ObjectChangeTracker ChangeTracker
    {
        get
        {
            if ((changeTrackerField == null))
            {
                changeTrackerField = new ObjectChangeTracker(this);
            }
            return changeTrackerField;
        }
    }
    
    private static XmlSerializer SerializerXml
    {
        get
        {
            if ((_serializerXml == null))
            {
                _serializerXml = new XmlSerializerFactory().CreateSerializer(typeof(T));
                SerializerXml.UnknownNode += delegate(object sender, XmlNodeEventArgs e) {Debug.WriteLine("[Unknown Node] Ln {0} Col {1} Object: {2} LocalName {3}, NodeName: {4}", e.LineNumber, e.LinePosition, e.ObjectBeingDeserialized.GetType().FullName, e.LocalName, e.Name);};
                SerializerXml.UnknownElement +=  delegate(object sender, XmlElementEventArgs e){Debug.WriteLine("[Unknown Element  ] Ln {0} Col {1} Object : {2} ExpectedElements {3}, Element : {4}", e.LineNumber, e.LinePosition, e.ObjectBeingDeserialized.GetType().FullName, e.ExpectedElements, e.Element.InnerXml);};
                SerializerXml.UnknownAttribute +=  delegate(object sender, XmlAttributeEventArgs e) {Debug.WriteLine("[Unknown Attribute] Ln {0} Col {1} Object : {2} LocalName {3}, Text : {4}", e.LineNumber, e.LinePosition, e.ObjectBeingDeserialized.GetType().FullName, e.ExpectedAttributes, e.Attr.Name);};
            }
            return _serializerXml;
        }
    }
    
    private static JsonSerializer SerializerBson
    {
        get
        {
            if ((_serializerBson == null))
            {
                _serializerBson = new JsonSerializer();
            }
            return _serializerBson;
        }
    }
    
    private static MessagePackSerializer SerializerMsPack
    {
        get
        {
            if ((_serializerMsPack == null))
            {
                _serializerMsPack = MsgPack.Serialization.MessagePackSerializer.Get<T>( new SerializationContext().SerializationMethod = SerializationMethod.Map);
            }
            return _serializerMsPack;
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    public virtual void OnPropertyChanged(string propertyName, object value)
    {
        ChangeTracker.RecordCurrentValue(propertyName, value);
        PropertyChangedEventHandler handler = PropertyChanged;
        if ((handler != null))
        {
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    #region Serialize/Deserialize
    /// <summary>
    /// Serialize SdcEntityBase object
    /// </summary>
    /// <returns>XML value</returns>
    public virtual string Serialize(System.Text.Encoding encoding)
    {
        StreamReader streamReader = null;
        MemoryStream memoryStream = null;
        try
        {
            memoryStream = new MemoryStream();
            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Encoding = encoding;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "  ";
            System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
            SerializerXml.Serialize(xmlWriter, this);
            memoryStream.Seek(0, SeekOrigin.Begin);
            streamReader = new StreamReader(memoryStream, encoding);
            return streamReader.ReadToEnd();
        }
        finally
        {
            if ((streamReader != null))
            {
                streamReader.Dispose();
            }
            if ((memoryStream != null))
            {
                memoryStream.Dispose();
            }
        }
    }
    
    public virtual string Serialize()
    {
        return Serialize(System.Text.Encoding.UTF8);
    }
    
    /// <summary>
    /// Deserializes SdcEntityBase object
    /// </summary>
    /// <param name="input">string to deserialize</param>
    /// <param name="obj">Output SdcEntityBase object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
    public static bool Deserialize(string input, out T obj, out Exception exception)
    {
        exception = null;
        obj = default(T);
        try
        {
            obj = Deserialize(input);
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }
    
    public static bool Deserialize(string input, out T obj)
    {
        Exception exception = null;
        return Deserialize(input, out obj, out exception);
    }
    
    public new static T Deserialize(string input)
    {
        StringReader stringReader = null;
        try
        {
            stringReader = new StringReader(input);
            return ((T)(SerializerXml.Deserialize(XmlReader.Create(stringReader))));
        }
        finally
        {
            if ((stringReader != null))
            {
                stringReader.Dispose();
            }
        }
    }
    
    public static T Deserialize(Stream s)
    {
        return ((T)(SerializerXml.Deserialize(s)));
    }
    #endregion
    
    /// <summary>
    /// Serializes current SdcEntityBase object into file
    /// </summary>
    /// <param name="fileName">full path of outupt xml file</param>
    /// <param name="exception">output Exception value if failed</param>
    /// <returns>true if can serialize and save into file; otherwise, false</returns>
    public virtual bool SaveToFile(string fileName, System.Text.Encoding encoding, out Exception exception)
    {
        exception = null;
        try
        {
            SaveToFile(fileName, encoding);
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            return false;
        }
    }
    
    public virtual bool SaveToFile(string fileName, out Exception exception)
    {
        return SaveToFile(fileName, System.Text.Encoding.UTF8, out exception);
    }
    
    public virtual void SaveToFile(string fileName)
    {
        SaveToFile(fileName, System.Text.Encoding.UTF8);
    }
    
    public virtual void SaveToFile(string fileName, System.Text.Encoding encoding)
    {
        StreamWriter streamWriter = null;
        try
        {
            string dataString = Serialize(encoding);
            streamWriter = new StreamWriter(fileName, false, encoding);
            streamWriter.WriteLine(dataString);
            streamWriter.Close();
        }
        finally
        {
            if ((streamWriter != null))
            {
                streamWriter.Dispose();
            }
        }
    }
    
    /// <summary>
    /// Deserializes xml markup from file into an SdcEntityBase object
    /// </summary>
    /// <param name="fileName">File to load and deserialize</param>
    /// <param name="obj">Output SdcEntityBase object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
    public static bool LoadFromFile(string fileName, System.Text.Encoding encoding, out T obj, out Exception exception)
    {
        exception = null;
        obj = default(T);
        try
        {
            obj = LoadFromFile(fileName, encoding);
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }
    
    public static bool LoadFromFile(string fileName, out T obj, out Exception exception)
    {
        return LoadFromFile(fileName, System.Text.Encoding.UTF8, out obj, out exception);
    }
    
    public static bool LoadFromFile(string fileName, out T obj)
    {
        Exception exception = null;
        return LoadFromFile(fileName, out obj, out exception);
    }
    
    public static T LoadFromFile(string fileName)
    {
        return LoadFromFile(fileName, System.Text.Encoding.UTF8);
    }
    
    public new static T LoadFromFile(string fileName, System.Text.Encoding encoding)
    {
        FileStream file = null;
        StreamReader sr = null;
        try
        {
            file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            sr = new StreamReader(file, encoding);
            string dataString = sr.ReadToEnd();
            sr.Close();
            file.Close();
            return Deserialize(dataString);
        }
        finally
        {
            if ((file != null))
            {
                file.Dispose();
            }
            if ((sr != null))
            {
                sr.Dispose();
            }
        }
    }
    
    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current SdcEntityBase object into an XML string
    /// </summary>
    /// <returns>string XML value</returns>
    public virtual string SerializeBson()
    {
        MemoryStream memoryStream = null;
        try
        {
            memoryStream = new MemoryStream();
            BsonDataWriter bsonDataWriter = new BsonDataWriter(memoryStream);
            SerializerBson.Serialize(bsonDataWriter, this);
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        finally
        {
            if ((memoryStream != null))
            {
                memoryStream.Dispose();
            }
        }
    }
    
    /// <summary>
    /// Deserializes SdcEntityBase object
    /// </summary>
    /// <param name="input">string to deserialize</param>
    /// <param name="obj">Output SdcEntityBase object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
    public static bool DeserializeBson(string input, out T obj, out Exception exception)
    {
        exception = null;
        obj = default(T);
        try
        {
            obj = DeserializeBson(input);
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }
    
    public static bool DeserializeBson(string input, out T obj)
    {
        Exception exception = null;
        return DeserializeBson(input, out obj, out exception);
    }
    
    /// <returns>string XML value</returns>
    public new static T DeserializeBson(string input)
    {
        MemoryStream memoryStream = null;
        try
        {
            byte[] data;
            data = Convert.FromBase64String(input);
            memoryStream = new MemoryStream(data);
            BsonDataReader bsonDataReader = new BsonDataReader(memoryStream);
            return SerializerBson.Deserialize<T>(bsonDataReader);
        }
        finally
        {
            if ((memoryStream != null))
            {
                memoryStream.Dispose();
            }
        }
    }
    #endregion
    
    public virtual void SaveToFileBson(string fileName)
    {
        StreamWriter streamWriter = null;
        try
        {
            string dataString = SerializeBson();
            FileInfo outputFile = new FileInfo(fileName);
            streamWriter = outputFile.CreateText();
            streamWriter.WriteLine(dataString);
            streamWriter.Close();
        }
        finally
        {
            if ((streamWriter != null))
            {
                streamWriter.Dispose();
            }
        }
    }
    
    public new static T LoadFromFileBson(string fileName)
    {
        FileStream file = null;
        StreamReader sr = null;
        try
        {
            file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            sr = new StreamReader(file);
            string dataString = sr.ReadToEnd();
            sr.Close();
            file.Close();
            return DeserializeBson(dataString);
        }
        finally
        {
            if ((file != null))
            {
                file.Dispose();
            }
            if ((sr != null))
            {
                sr.Dispose();
            }
        }
    }
    
    #region Serialize/Deserialize
    /// <summary>
    /// Serializes current SdcEntityBase object into an json string
    /// </summary>
    public virtual string SerializeJson()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.DefaultValueHandling = DefaultValueHandling.Include;
        return JsonConvert.SerializeObject(this, settings);
    }
    
    /// <summary>
    /// Deserializes SdcEntityBase object
    /// </summary>
    /// <param name="input">string to deserialize</param>
    /// <param name="obj">Output SdcEntityBase object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
    public static bool DeserializeJson(string input, out T obj, out Exception exception)
    {
        exception = null;
        obj = default(T);
        try
        {
            obj = DeserializeJson(input);
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }
    
    public static bool DeserializeJson(string input, out T obj)
    {
        Exception exception = null;
        return DeserializeJson(input, out obj, out exception);
    }
    
    public new static T DeserializeJson(string input)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.DefaultValueHandling = DefaultValueHandling.Include;
        return JsonConvert.DeserializeObject<T>(input, settings);
    }
    #endregion
    
    public virtual void SaveToFileJson(string fileName)
    {
        StreamWriter streamWriter = null;
        try
        {
            string dataString = SerializeJson();
            FileInfo outputFile = new FileInfo(fileName);
            streamWriter = outputFile.CreateText();
            streamWriter.WriteLine(dataString);
            streamWriter.Close();
        }
        finally
        {
            if ((streamWriter != null))
            {
                streamWriter.Dispose();
            }
        }
    }
    
    public new static T LoadFromFileJson(string fileName)
    {
        FileStream file = null;
        StreamReader sr = null;
        try
        {
            file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            sr = new StreamReader(file);
            string dataString = sr.ReadToEnd();
            sr.Close();
            file.Close();
            return DeserializeJson(dataString);
        }
        finally
        {
            if ((file != null))
            {
                file.Dispose();
            }
            if ((sr != null))
            {
                sr.Dispose();
            }
        }
    }
    
    #region Serialize/Deserialize
    /// <summary>
    /// Serialize current SdcEntityBase object to msgpack
    /// </summary>
    /// <returns>string binary value</returns>
    public virtual byte[] SerializeMsgPack()
    {
        MemoryStream byteStream = null;
        try
        {
            byteStream = new MemoryStream();
            SerializerMsPack.Pack(byteStream, this);
            return byteStream.ToArray();
        }
        finally
        {
            if ((byteStream != null))
            {
                byteStream.Dispose();
            }
        }
    }
    
    /// <summary>
    /// Deserializes SdcEntityBase object
    /// </summary>
    /// <param name="input">string to deserialize</param>
    /// <param name="obj">Output SdcEntityBase object</param>
    /// <param name="exception">output Exception value if deserialize failed</param>
    /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
    public static bool DeserializeMsgPack(byte[] input, out T obj, out Exception exception)
    {
        exception = null;
        obj = default(T);
        try
        {
            obj = DeserializeMsgPack(input);
            return true;
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }
    
    public static bool DeserializeMsgPack(byte[] input, out T obj)
    {
        Exception exception = null;
        return DeserializeMsgPack(input, out obj, out exception);
    }
    
    /// <summary>
    /// Deserializes msgpack to current T object
    /// </summary>
    public new static T DeserializeMsgPack(byte[] input)
    {
        MemoryStream byteStream = null;
        try
        {
            byteStream = new MemoryStream(input);
            return ((T)(SerializerMsPack.Unpack(byteStream)));
        }
        finally
        {
            if ((byteStream != null))
            {
                byteStream.Dispose();
            }
        }
    }
    #endregion
    
    public virtual void SaveToFileMsgPack(string fileName)
    {
        FileStream fileStream = null;
        try
        {
            byte[] msgPackBytes = SerializeMsgPack();
            fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            fileStream.Write(msgPackBytes, 0, msgPackBytes.Length);
            fileStream.Close();
        }
        finally
        {
            if ((fileStream != null))
            {
                fileStream.Dispose();
            }
        }
    }
    
    public new static T LoadFromFileMsgPack(string fileName)
    {
        FileStream file = null;
        byte[] buffer = null;
        try
        {
            file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            buffer = new byte[file.Length];
            file.Read(buffer, 0, ((int)(file.Length)));
            return DeserializeMsgPack(buffer);
        }
        finally
        {
            if ((file != null))
            {
                file.Dispose();
            }
        }
    }
    
    #region Clone method
    /// <summary>
    /// Create a clone of this T object
    /// </summary>
    public virtual T Clone()
    {
        return ((T)(MemberwiseClone()));
    }
    #endregion
}
#endregion
}
#pragma warning restore
