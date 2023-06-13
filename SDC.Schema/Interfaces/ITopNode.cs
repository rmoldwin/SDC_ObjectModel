using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

//using SDC;
namespace SDC.Schema
{
	/// <summary>
	/// A public/internal interface inherited by all types that sit at the top of the SDC class hierarchy.<br/>
	/// Used by FormDesignType, DemogFormDesignType, DataElementType, RetrieveFormPackageType, PackageListType and MappingType.<br/>
	/// The interface provides a common way to fill the above object trees using a single set of shared code.<br/>
	/// It also provides a set of consistent, type-specific, public utilities for working with SDC objects.
	/// </summary>

	public interface ITopNode : IBaseType
	//Note about inheriting from IBaseType:
	//All ITopNode annotated classes must descend from BaseType, and BaseType implements IBaseType.
	//An interface can't inherit a class (BaseType), but we can inherit from a common interface (IBaseType), which is also inherited by the BaseType class .
	//This allows us to use ITopNode objects to close generic functions whenever the generic T is restricted to IBaseType (T: IBaseType).
	//In particular, SdcSerializer<T> uses "where T:IBaseType" as its generic restricion, enabling the SdcSerializer methods to use ITopNode as a parameter type.
	//If SdcSerializer<T> used T: BaseType, its methods would not accept ITopNode, since an interface cannot inherit a class (BaseType)
	{

        /// <summary>
        /// ReadOnlyDictionary of all SDC nodes.
        /// </summary>
        [XmlIgnore]
		[JsonIgnore]
		ReadOnlyDictionary<Guid, BaseType> Nodes { get; }

		/// <summary>
		/// ReadOnlyObservableCollection of IET nodes.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes { get; }

		/// <summary>
		/// Automatically create and assign element names to all SDC elements
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		bool GlobalAutoNameFlag { get; set; }
		/// <summary>
		/// Runs method BaseType.ResetSdcImport(), which resets TopNodeTemp, allowing the addition of a new TopNode for newly added BaseType objects.
		/// </summary>
		/// <param name="itn">The itn.</param>
		public void ResetRootNode(); //=> BaseType.ResetSdcImport();

		#region Serialization
		/// <summary>
		/// Returns SDC XML from the SDC object tree.  The XML top node is determined by the top-level object tree node:
		/// FormDesignType, DemogFormDesignType, DataElementType, RetrieveFormPackageType, PackageListType or MappingType
		/// </summary>
		/// <returns></returns>
		//public string GetXml();
		///// <summary>
		///// Not yet supported
		///// </summary>
		///// <returns></returns>
		//public string GetJson();
		////{
		////    var doc = new XmlDocument();
		////    doc.LoadXml(GetXml()); ;
		////    return JsonConvert.SerializeXmlNode(doc);
		////}

		///// <summary>
		///// Not yet supported
		///// </summary>
		///// <returns></returns>
		//public byte[] GetMsgPack();

		//public void SaveXmlToFile(string path);
		//public void SaveJsonToFile(string path);
		//public void SaveBsonToFile(string path);
		//public void SaveMsgPackToFile(string path);
		#endregion

	}

    /// <summary>
    /// This interface (ITopNode) hides its Internal members, and also imports public members from ITopNodePublic.<br/>
    /// Since _ITopNode is internal, it can only be used by classes in the SDC.Schema assembly.<br/>
    /// The interface and all its members use the "_" prefix to indicate that they are internal.<br/>
    /// <br/><br/>
    /// See here for a description of an internal interface inheriting a public interface:
    /// <see href="https://www.csharp411.com/c-internal-interface/"/> <br/><br/>
    /// Note that all <see cref="_ITopNode"/> interface members use the access level of their defining interface (e.g., internal, in this case), 
    /// regardless of the access modifier on each member. <br/>
    /// Inheritance of a less restrictive interface (<see cref="ITopNode"/>) leaves those inherited <see cref="ITopNode"/>members with their less restrictive (public) access, 
    /// even though the top-level interface is more restrictive.<br/><br/>
    /// Note that all internal <see cref="ITopNode"/> interface member names are prefixed with "_" and start with a capital letter. 
    /// </summary>
    internal interface _ITopNode : ITopNode
	{
        /// <summary>
        /// Gets or sets the maximum object identifier.
        /// </summary>
        int _MaxObjectID { get; set; }

		///<summary>
		/// Internal base object for initializing IETnodes.<br/>
		/// The contents of this list are copied (as a read-only collection) to the public IETnodes when the IETnodes property is accessed.
		/// </summary>
		ObservableCollection<IdentifiedExtensionType> _IETnodes { get; }

        /// <summary>
        /// Internal Dictionary.  Given an Node ObjectGUID, returns the node's object reference.
        /// </summary>
        Dictionary<Guid, BaseType> _Nodes { get; }

        /// <summary>
        /// Internal Dictionary.  Given a Node ObjectGUID, return the *parent* node's object reference
        /// </summary>
        Dictionary<Guid, BaseType> _ParentNodes { get;}
        /// <summary>
        /// Internal Dictionary.  Given a NodeID ObjectGUID, return a list of the child nodes object reference
        /// </summary>
        Dictionary<Guid, List<BaseType>> _ChildNodes { get;}

        /// <summary>
        /// This internal HashSet contains the ObjectID of each parent node that has had its child nodes sorted by ITreeSibComparer. <br/>
        /// The presence of an ObjectID entry in this HashSet indicates that parent node's child nodes have already been sorted. <br/>
        /// Checking for a parent node in this HashSet is used to skip the resorting of child nodes during a tree sorting operation. <br/>
        /// The HashSet may be cleared using TreeSort_ClearNodeIds().  This action clears all entries from the HashSet and will thus <br/>
		/// cause all parent nodes to resort their child nodes when a request is made for the parent node's _ChildItems entries.
        /// </summary>
        HashSet<int> _TreeSort_NodeIds { get; }

		/// <summary>
		/// Internal HashSet that contains every BaseName generated inside a TopNode's tree. It ensures that all BaseNames are unique. 
		/// </summary>
		HashSet<string> _UniqueBaseNames { get; }
        /// <summary>
		/// Internal HashSet that contains every BaseType.name property generated inside a TopNode's tree. It ensures that all name values are unique. 
		/// </summary>
		HashSet<string> _UniqueNames { get; }
        /// <summary>
		/// Remove all entries from internal <see cref="_ITopNode"/> dictionaries
		/// </summary>
		internal void _ClearDictionaries();
	}
}

