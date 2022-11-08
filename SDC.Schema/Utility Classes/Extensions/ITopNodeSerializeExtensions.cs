using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SDC.Schema.Extensions
{
	public static class ITopNodeSerializeExtensions
	{
		/// <summary>
		/// Retrieve SDC XML from the current node
		/// </summary>
		/// <typeparam name="T">The node type</typeparam>
		/// <param name="node">The node to serialize</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formattted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		/// <returns></returns>
		public static string GetXml<T>(this T node, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T: ITopNode
		=> TopNodeSerializer<T>.GetXml(node, refreshSdc, createNameDelegate);
		/// <summary>
		/// Retrieve SDC JSON from the current node
		/// </summary>
		/// <typeparam name="T">The node type</typeparam>
		/// <param name="node">The node to serialize</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formattted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		/// <returns></returns>
		public static string GetJson<T>(this T node, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		=> TopNodeSerializer<T>.GetJson(node, refreshSdc, createNameDelegate);
		/// <summary>
		/// Retrieve SDC BSON from the current node
		/// </summary>
		/// <typeparam name="T">The node type</typeparam>
		/// <param name="node">The node to serialize</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formattted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		/// <returns></returns>
		public static string GetBson<T>(this T node, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		=> TopNodeSerializer<T>.GetBson(node, refreshSdc, createNameDelegate);
		/// <summary>
		/// Retrieve SDC MsgPack from the current node
		/// </summary>
		/// <typeparam name="T">The node type</typeparam>
		/// <param name="node">The node to serialize</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formattted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		/// <returns></returns>
		public static byte[] GetMsgPack<T>(this T node, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		=> TopNodeSerializer<T>.GetMsgPack(node, refreshSdc, createNameDelegate);
		/// <summary>
		/// Save the current SDC object tree to an SDC XML file at a known location (path)
		/// </summary>
		/// <param name="node">The node to serialize</param>
		/// <param name="path">Path to save the file</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formatted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		public static void SaveXmlToFile<T>(this T node, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveXmlToFile(node, path, refreshSdc, createNameDelegate);
		}
		/// <summary>
		/// Save the current SDC object tree to an SDC Json file at a known location (path)
		/// </summary>
		/// <param name="node">The node to serialize</param>
		/// <param name="path">Path to save the file</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formatted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		public static void SaveJsonToFile<T>(this T node, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveJsonToFile(node, path, refreshSdc, createNameDelegate);
		}
		/// <summary>
		/// Save the current SDC object tree to an SDC Bson file at a known location (path)
		/// </summary>
		/// <param name="node">The node to serialize</param>
		/// <param name="path">Path to save the file</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formatted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		public static void SaveBsonToFile<T>(this T node, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveBsonToFile(node, path, refreshSdc, createNameDelegate);
		}
		/// <summary>
		/// Save the current SDC object tree to an SDC MessagePack file at a known location (path)
		/// </summary>
		/// <param name="node">The node to serialize</param>
		/// <param name="path">Path to save the file</param>
		/// <param name="refreshSdc">Refresh basic metadata like order, name, sGuid, ElementName, and refill ITopNode dictionaries.<br/>
		/// see: <seealso cref="SDC.Schema.SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?, int)"/> </param>
		/// <param name="createNameDelegate">A delegate that returns a string value for the creating a node's @name value.<br/>
		/// The default delegate can process Ckey-formatted (decimal) IDs on <see cref="IdentifiedExtensionType"/> nodes.</param>
		public static void SaveMsgPackToFile<T>(this T node, string path, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null) where T : ITopNode
		{
			if (createNameDelegate is null) createNameDelegate = SdcUtil.CreateCAPname;
			TopNodeSerializer<T>.SaveMsgPackToFile(node, path, refreshSdc, createNameDelegate);
		}
	}
}
