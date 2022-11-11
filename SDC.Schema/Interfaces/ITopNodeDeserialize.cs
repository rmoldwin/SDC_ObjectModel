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
		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromXmlPath(sdcPath, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromXml(sdcXml, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromJsonPath(sdcPath, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromJson(sdcJson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromBsonPath(sdcPath, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromMsgPackPath(sdcPath, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static T DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<T>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);
	}
}
