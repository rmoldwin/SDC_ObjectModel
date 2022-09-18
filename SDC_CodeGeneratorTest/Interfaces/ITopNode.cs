using CSharpVitamins;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml;
using System.Xml.Schema;
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

	public interface ITopNodePublic: IBaseType
    {
		/// <summary>
		/// Dictionary.  Given an Node ObjectGUID, returns the node's object reference.
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
        Dictionary<Guid, BaseType> Nodes { get; }

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
	internal interface ITopNode:ITopNodePublic
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










	/// <summary>
	/// ITopNodeCollectionsPublic defines public read-only SDC node collections for ITopNode SDC objects.
	/// For use in ITopNode classes, this public interface must be inherited by ITopNodeCollections.
	/// </summary>
	public interface ITopNodeCollectionsPublic
	{
		/// <summary>
		/// ReadOnlyObservableCollection of IET nodes.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodesRO { get; }

		/// <summary>
		/// ReadOnlyDictionary.  Given an ObjectGUID, returns the node's object reference.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		ReadOnlyDictionary<Guid, BaseType> NodesRO { get; }
		///// <summary>
		///// ReadOnlyDictionary.  Given an ObjectGUID, return the *parent* node's object reference.
		///// </summary>
		//[XmlIgnore]
		//[JsonIgnore]
		//ReadOnlyDictionary<Guid, BaseType> ParentNodesRO { get; }

		///// <summary>
		///// ReadOnlyDictionary.  Given an ObjectGUID, return a list of the child nodes object reference.
		///// </summary>
		//[XmlIgnore]
		//[JsonIgnore]
		//ReadOnlyDictionary<Guid, IReadOnlyList<BaseType>> ChildNodesRO { get; }
	}


	/// <summary>
	/// Defines internal SDC node collections for ITopNode SDC objects.
	/// When implemented in class instances, these collections will be 
	/// wrapped by the read-only collections in ITopNodeCollectionsPublic.
	/// The internal members of this interface must be declared explicitly
	/// , (e.g., ITopNodeCollections.IETnodes),
	/// and will not be visible outside this assembly.
	/// The public interface is available for public access to the read-only collections.
	/// 
	/// </summary>
	internal interface ITopNodeCollections: ITopNodeCollectionsPublic
	{
		/// <summary>
		/// Internal base object for initializing IETnodesRO.
		/// </summary>
		internal ObservableCollection<IdentifiedExtensionType> IETnodes { get; }
		/// <summary>
		/// Dictionary.  Given an ObjectGUID, returns the node's object reference.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal Dictionary<Guid, BaseType> Nodes { get; }
		/// <summary>
		/// Dictionary.  Given a ObjectGUID, return the *parent* node's object reference
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal Dictionary<Guid, BaseType> ParentNodes { get; }

		/// <summary>
		/// Dictionary.  Given a NodeID ObjectGUID, return a list of the child nodes object reference
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal Dictionary<Guid, List<BaseType>> ChildNodes { get; }
	}
}
