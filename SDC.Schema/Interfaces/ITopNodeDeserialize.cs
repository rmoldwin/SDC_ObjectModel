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
		public static T DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromBson(sdcBson);
		public static T DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static T DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<T>.DeserializeFromMsgPack(sdcMsgPack);
	}
}
