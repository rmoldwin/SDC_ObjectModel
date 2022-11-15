using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
    /// <summary>
    /// SDC (de)serialization static methods designed to be accessed from ITopNode classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TopNodeSerializer<T> where T : ITopNode
    {
		//!+XML

		/// <summary>
		/// Read an SDC XML file and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		/// </summary>
		/// <param name="path">File path and name of the SDC XML file to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromXmlPath(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            string sdcXml = File.ReadAllText(path);  // System.Text.Encoding.UTF8);
            return DeserializeFromXml(sdcXml, refreshSdc, createNameDelegate, orderStart, orderGap);
        }

		/// <summary>
		/// Read an SDC XML string and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="sdcXml">SDC XML string to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {

			T obj = SdcSerializer<T>.Deserialize(sdcXml);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return obj;
        }
		/// <summary>
		/// Serialize the current SDC object tree to an SDC XML string.
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC XML string</returns>
		public static string GetXml(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return SdcSerializer<T>.Serialize(tn);
        }
		//!+JSON

		/// <summary>
		/// Read an SDC Json file and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="path">File path and name of the SDC Json file to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromJsonPath(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            string sdcJson = File.ReadAllText(path);
            return DeserializeFromJson(sdcJson, refreshSdc, createNameDelegate, orderStart, orderGap);
        }
		/// <summary>
		/// Read an SDC Json file and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="sdcJson">SDC Json string to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            T obj = SdcSerializerJson<T>.DeserializeJson<T>(sdcJson);
            //return InitParentNodesFromXml<T>(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return obj;
        }
		/// <summary>
		/// Serialize the current SDC object tree to an SDC Json string.
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC Json string</returns>
		public static string GetJson(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return SdcSerializerJson<T>.SerializeJson(tn);
        }
		//!+BSON

		/// <summary>
		/// Read an SDC Bson file and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="path">File path and name of the SDC Bson file to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromBsonPath(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            string sdcBson = File.ReadAllText(path);
            return DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);
        }
		/// <summary>
		/// Read an SDC Bson file and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="sdcBson">sdcBson string to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            T obj = SdcSerializerBson<T>.DeserializeBson(sdcBson);
            //return InitParentNodesFromXml(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return obj;
        }
		/// <summary>
		/// Serialize the current SDC object tree to an SDC Bson string.
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC Bson string</returns>
		public static string GetBson(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return SdcSerializerBson<T>.SerializeBson(tn);
        }
		//!+MsgPack

		/// <summary>
		/// Read an SDC MsgPack file and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="path">File path and name of the SDC MsgPack file to deserialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromMsgPackPath(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            byte[] sdcMsgPack = File.ReadAllBytes(path);
            return DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);
        }
		/// <summary>
		/// Read an SDC MsgPack byte array and return an SDC object model tree.<br/><br/>
		/// <inheritdoc cref="SdcUtil.ReflectRefreshTree"/>
		/// </summary>
		/// <param name="sdcMsgPack"></param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>SDC object tree</returns>
		public static T DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            T obj = SdcSerializerMsgPack<T>.DeserializeMsgPack(sdcMsgPack);
            //return InitParentNodesFromXml<T>(sdcXml, obj);
            if (refreshSdc) SdcUtil.ReflectRefreshTree(obj, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return obj;
        }
		/// <summary>
		/// Serialize the current SDC object tree to an SDC MessagePack byte array
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		/// <returns>MsgPack byte array</returns>
		public static byte[] GetMsgPack(T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            return SdcSerializerMsgPack<T>.SerializeMsgPack(tn);
        }


		//!+Save to File

		/// <summary>
		/// Save the current SDC object tree to an SDC XML file at a known location (path)
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="path">File path and name of the SDC XML file to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		public static void SaveXmlToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            SdcSerializer<T>.SaveToFile(path, tn);
        }
		/// <summary>
		/// Save the current SDC object tree to an SDC Json file at a known location (path)
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="path">File path and name of the SDC Json file to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		public static void SaveJsonToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            SdcSerializerJson<T>.SaveToFileJson(path, tn);
        }
		/// <summary>
		/// Save the current SDC object tree to an SDC Bson file at a known location (path)
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="path">File path and name of the Bson file to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		public static void SaveBsonToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            SdcSerializerBson<T>.SaveToFileBson(path, tn);
        }
		/// <summary>
		/// Save the current SDC object tree to an SDC MessagePack file at a known location (path)
		/// </summary>
		/// <param name="tn">An SDC ITopNode object - the top node (root) of the SDC tree to serialize.</param>
		/// <param name="path">File path and name of the MsgPack file to serialize.</param>
		/// <param name="refreshSdc">If true, the method calls <see cref="SdcUtil.ReflectRefreshTree"/></param>
		/// <param name="createNameDelegate">A method used to create names for each node in the SDC tree.  See <see cref="SdcUtil.CreateName"/></param>
		/// <param name="orderStart">The starting number for the @order attribute.</param>
		/// <param name="orderGap">A multiplier for the @order atttribute. <br/>
		/// <b><paramref name="orderGap"/></b> controls the distance between sequential @order values.<br/>
		/// The default value is 10.<br/></param>
		public static void SaveMsgPackToFile(T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
        {
            if (refreshSdc) SdcUtil.ReflectRefreshTree(tn, out _, false, refreshSdc, createNameDelegate, orderStart, orderGap);
            SdcSerializerMsgPack<T>.SaveToFileMsgPack(path, tn);
        }
    }
}
