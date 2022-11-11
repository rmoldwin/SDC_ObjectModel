using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SDC.Schema.Extensions
{
	/// <summary>
	/// Extension methods for ITopNode SDC classes (for instance objects only, not for static class methods).<br/> 
	/// Used by FormDesignType, DemogFormDesignType, DataElementType, RetrieveFormPackageType, PackageListType and MappingType.<br/>
	/// </summary>
	public static class ITopNodeSerializeExtensions
	{
		///<inheritdoc cref="TopNodeSerializer{T}.GetXml(T, bool, SdcUtil.CreateName?, int, int)"/>
		public static string GetXml<T>(this T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T: ITopNode
		=> TopNodeSerializer<T>.GetXml(tn, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.GetJson(T, bool, SdcUtil.CreateName?, int, int)"/>
		public static string GetJson<T>(this T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		=> TopNodeSerializer<T>.GetJson(tn, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.GetBson(T, bool, SdcUtil.CreateName?, int, int)"/>
		public static string GetBson<T>(this T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		=> TopNodeSerializer<T>.GetBson(tn, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.GetMsgPack(T, bool, SdcUtil.CreateName?, int, int)"/>
		public static byte[] GetMsgPack<T>(this T tn, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		=> TopNodeSerializer<T>.GetMsgPack(tn, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.SaveXmlToFile(T, string, bool, SdcUtil.CreateName?, int, int)"/>
		public static void SaveXmlToFile<T>(this T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveXmlToFile(tn, path, refreshSdc, createNameDelegate, orderStart, orderGap);
		}

		///<inheritdoc cref="TopNodeSerializer{T}.SaveJsonToFile(T, string, bool, SdcUtil.CreateName?, int, int)"/>
		public static void SaveJsonToFile<T>(this T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveJsonToFile(tn, path, refreshSdc, createNameDelegate, orderStart, orderGap);
		}

		///<inheritdoc cref="TopNodeSerializer{T}.SaveBsonToFile(T, string, bool, SdcUtil.CreateName?, int, int)"/>
		public static void SaveBsonToFile<T>(this T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveBsonToFile(tn, path, refreshSdc, createNameDelegate, orderStart, orderGap);
		}

		///<inheritdoc cref="TopNodeSerializer{T}.SaveMsgPackToFile(T, string, bool, SdcUtil.CreateName?, int, int)"/>
		public static void SaveMsgPackToFile<T>(this T tn, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveMsgPackToFile(tn, path, refreshSdc, createNameDelegate, orderStart, orderGap);
		}
	}












}
