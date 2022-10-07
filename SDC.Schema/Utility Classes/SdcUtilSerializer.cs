using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema.Interfaces
{
    /// <summary>
    /// Contains SDC (de)serialization static methods designed to be accessed for ITopNode classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class SdcUtilSerializer<T> where T : ITopNode
    {
		//!+XML

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sdcXmlPath"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param> 
		/// <returns></returns>
		public static T DeserializeFromXmlPath(string sdcXmlPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            string sdcXml = File.ReadAllText(sdcXmlPath);  // System.Text.Encoding.UTF8);
            return DeserializeFromXml(sdcXml, refreshSdc, createNameDelegate);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sdcXml"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param> 
		/// <returns></returns>
		public static T DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            T obj = SdcSerializer<T>.Deserialize(sdcXml);
            //return InitParentNodesFromXml<T>(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate);
            return obj;
        }
		/// <summary>
		/// Returns SDC XML from the SDC object tree.
		/// </summary>
		/// <param name="tn"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param> 
		/// <returns></returns>
		public static string GetXml(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            return SdcSerializer<T>.Serialize(tn);
        }
        //!+JSON

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdcJsonPath"></param>
        /// <param name="refreshSdc"></param>
        /// <param name="createNameDelegate"></param>
        /// <returns></returns>
        public static T DeserializeFromJsonPath(string sdcJsonPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            string sdcJson = File.ReadAllText(sdcJsonPath);
            return DeserializeFromJson(sdcJson, refreshSdc, createNameDelegate);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sdcJson"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param>
		/// <returns></returns>
		public static T DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            T obj = SdcSerializerJson<T>.DeserializeJson<T>(sdcJson);
            //return InitParentNodesFromXml<T>(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate);
            return obj;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tn"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param> 
		/// <returns></returns>
		public static string GetJson(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            return SdcSerializerJson<T>.SerializeJson(tn);
        }
		//!+BSON

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sdcBsonPath"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param> 
		/// <returns></returns>
		public static T DeserializeFromBsonPath(string sdcBsonPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            string sdcBson = File.ReadAllText(sdcBsonPath);
            return DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sdcBson"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param> 
		/// <returns></returns>
		public static T DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            T obj = SdcSerializerBson<T>.DeserializeBson(sdcBson);
            //return InitParentNodesFromXml(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate);
            return obj;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tn"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param>
		/// <returns></returns>
		public static string GetBson(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            return SdcSerializerBson<T>.SerializeBson(tn);
        }
		//!+MsgPack

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sdcMsgPackPath"></param>
		/// <param name="refreshSdc"></param>
		/// /// <param name="createNameDelegate"></param>
		/// <returns></returns>
		public static T DeserializeFromMsgPackPath(string sdcMsgPackPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            byte[] sdcMsgPack = File.ReadAllBytes(sdcMsgPackPath);
            return DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sdcMsgPack"></param>
        /// <param name="refreshSdc"></param>
        /// <param name="createNameDelegate"></param>
        /// <returns></returns>
        public static T DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            T obj = SdcSerializerMsgPack<T>.DeserializeMsgPack(sdcMsgPack);
            //return InitParentNodesFromXml<T>(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate);
            return obj;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tn"></param>
		/// <param name="path"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param>
		/// <returns></returns>
		public static byte[] GetMsgPack(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            return SdcSerializerMsgPack<T>.SerializeMsgPack(tn);
        }


		//!+Save to File

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tn"></param>
		/// <param name="path"></param>
		/// <param name="refreshSdc"></param>
		/// <param name="createNameDelegate"></param>
		public static void SaveXmlToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            SdcSerializer<T>.SaveToFile(path, tn);
        }
        /// <summary>
        /// Save the current SDC object tree to an SDC Json file at a known location (path)
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="path"></param>
        /// <param name="refreshSdc"></param>
        /// <param name="createNameDelegate"></param>
        public static void SaveJsonToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            SdcSerializerJson<T>.SaveToFileJson(path, tn);
        }
        /// <summary>
        /// Save the current SDC object tree to an SDC Bson file at a known location (path)
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="path"></param>
        /// <param name="refreshSdc"></param>
        /// <param name="createNameDelegate"></param>
        public static void SaveBsonToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            SdcSerializerBson<T>.SaveToFileBson(path, tn);
        }
        /// <summary>
        /// Save the current SDC object tree to an SDC MessagePack file at a known location (path)
        /// </summary>
        /// <param name="tn"></param>
        /// <param name="path"></param>
        /// <param name="refreshSdc"></param>
        /// <param name="createNameDelegate"></param>
        public static void SaveMsgPackToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate);
            SdcSerializerMsgPack<T>.SaveToFileMsgPack(path, tn);
        }
    }
}
