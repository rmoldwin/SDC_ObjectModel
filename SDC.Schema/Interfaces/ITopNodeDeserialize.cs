using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDC.Schema.Extensions;

namespace SDC.Schema
{
    internal interface ITopNodeDeserialize<T> where T:ITopNode
    {
		public static T DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		//public string GetXml<T>(bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
		//	//=> SdcUtilSerializer<T>.GetXml(this, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		//public string GetJson(bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
		//	//=> SdcUtilSerializer<T>.GetJson(this, refreshSdc, createNameDelegate);
		public static T DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromBson(sdcBson);
		//public string GetBson(bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
		//	//=> SdcUtilSerializer<T>.GetBson(this, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromMsgPack(sdcMsgPack);
	//	public byte[] GetMsgPack(bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
	//	//=> SdcUtilSerializer<T>.GetMsgPack(this, refreshSdc: true, createNameDelegate);
	//	/// <summary>
	//	/// Save the current SDC object tree to an SDC XML file at a known location (path)
	//	/// </summary>
	//	/// <param name="path"></param>
	//	/// <param name="refreshSdc"></param>
	//	/// <param name="createNameDelegate"></param>
	//	public void SaveXmlToFile(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
	//	//{
	//	//	if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateElementNameCAP;
	//	//	SdcUtilSerializer<T>.SaveXmlToFile(this, path, refreshSdc: true, createNameDelegate);
	//	//}
	//	/// <summary>
	//	/// Save the current SDC object tree to an SDC Json file at a known location (path)
	//	/// </summary>
	//	/// <param name="path"></param>
	//	/// <param name="refreshSdc"></param>
	//	/// <param name="createNameDelegate"></param>
	//	public void SaveJsonToFile(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
	//	//{
	//	//	if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateElementNameCAP;
	//	//	SdcUtilSerializer<T>.SaveJsonToFile(this, path, refreshSdc: true, createNameDelegate);
	//	//}
	//	/// <summary>
	//	/// Save the current SDC object tree to an SDC Bson file at a known location (path)
	//	/// </summary>
	//	/// <param name="path"></param>
	//	/// 		/// <param name="refreshSdc"></param>
	//	/// <param name="createNameDelegate"></param>
	//	public void SaveBsonToFile(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
	//	//{
	//	//	if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateElementNameCAP;
	//	//	SdcUtilSerializer<T>.SaveBsonToFile(this, path, refreshSdc: true, createNameDelegate);
	//	//}
	//	/// <summary>
	//	/// Save the current SDC object tree to an SDC MessagePack file at a known location (path)
	//	/// </summary>
	//	/// <param name="path"></param>
	//	/// <param name="refreshSdc"></param>
	//	/// <param name="createNameDelegate"></param>
	//	public void SaveMsgPackToFile(string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null);
	//	//{
	//	//	if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateElementNameCAP;
	//	//	SdcUtilSerializer<T>.SaveMsgPackToFile(this, path, refreshSdc: true, createNameDelegate);
	//	//}
	}
}
