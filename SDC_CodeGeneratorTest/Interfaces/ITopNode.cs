using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

//using SDC;
namespace SDC.Schema
{
	/// <summary>
	/// A public/internal interface inherited by all types that sit at the top of the SDC class hierarchy
	/// Used by FormDesignType, DemogFormDesignType, DataElementType, RetrieveFormPackageType, and PackageListType
	/// The interface provides a common way to fill the above object trees using a single set of shared code.
	/// It also provdes a set of consistent, type-specific, public utilities for working with SDC objects.
	/// </summary>

	public interface ITopNodePublic : IBaseType
	{
		/// <summary>
		/// Dictionary.  Given an Node ObjectGUID, returns the node's object reference.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> Nodes { get; }

		/// <summary>
		/// ReadOnlyObservableCollection of all SDC nodes.
		/// </summary>
		ReadOnlyDictionary<Guid, BaseType> NodesRO { get; }

		/// <summary>
		/// ReadOnlyObservableCollection of IET nodes.
		/// </summary>
		ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodesRO { get; }

		[XmlIgnore]
		[JsonIgnore]
		int MaxObjectID { get; }

		//[XmlIgnore]
		//[JsonIgnore]
		//      internal int MaxObjectID { get; set; }
		/// <summary>
		/// Automatically create and assign element names to all SDC elements
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		bool GlobalAutoNameFlag { get; set; }

		#region Serialization
		/// <summary>
		/// Returns SDC XML from the SDC object tree.  The XML top node is determined by the top-level object tree node:
		/// FormDesignType, DemogFormDesignType, DataElementType, RetrieveFormPackageType, or PackageListType
		/// </summary>
		/// <returns></returns>
		public string GetXml();
		/// <summary>
		/// Not yet supported
		/// </summary>
		/// <returns></returns>
		public string GetJson();
		//{
		//    var doc = new XmlDocument();
		//    doc.LoadXml(GetXml()); ;
		//    return JsonConvert.SerializeXmlNode(doc);
		//}

		/// <summary>
		/// Not yet supported
		/// </summary>
		/// <returns></returns>
		public byte[] GetMsgPack();

		public void SaveXmlToFile(string path);
		public void SaveJsonToFile(string path);
		public void SaveBsonToFile(string path);
		public void SaveMsgPackToFile(string path);
		#endregion

	}

	/// <summary>
	/// This interface (ITopNode) hides its Internal members, and also imports public members from ITopNodePublic.
	/// <br/><br/>
	/// See here for a description of an internal interface inheriting a public interface:
	/// https://www.csharp411.com/c-internal-interface/ <br/><br/>
	/// Note that all interface members use the access level of their defining interface (e.g., internal, in this case), 
	/// regardless of the access modifier on each member.  
	/// Inheritance of a less restrictive interface (e.g., ITopNodePublic) leaves those inherited members with their less restrictive access, 
	/// even though the top-level interface is more restictive.
	/// </summary>
	internal interface ITopNode : ITopNodePublic
	{
		/// <summary>
		/// Internal base object for initializing IETnodesRO.
		/// </summary>
		internal ObservableCollection<IdentifiedExtensionType> IETnodes { get; }

		/// <summary>
		/// Internal version of MaxObjectID, which has a setter; MaxObjectID only has a getter
		/// </summary> 
		internal int MaxObjectIDint { get; set; }
		/// <summary>
		/// Dictionary.  Given a Node ObjectGUID, return the *parent* node's object reference
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> ParentNodes { get; }
		/// <summary>
		/// Dictionary.  Given a NodeID ObjectGUID, return a list of the child nodes object reference
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, List<BaseType>> ChildNodes { get; }
	}
}

