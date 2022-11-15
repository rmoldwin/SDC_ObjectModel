﻿using Newtonsoft.Json;
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
		/// ReadOnlyObservableCollection of all SDC nodes.
		/// </summary>
		ReadOnlyDictionary<Guid, BaseType> Nodes { get; }

		/// <summary>
		/// ReadOnlyObservableCollection of IET nodes.
		/// </summary>
		ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes { get; }

		//[XmlIgnore]
		//[JsonIgnore]
		/////Holds the largest ObjectID that was most-recently assigned to a new node.  <br/>
		/////The MaxObjectID is incremented by 1 each time a new node is added to an SDC object tree. <br/>
		/////MaxObjectID can be reset to 0 by calling ITopNode.ResetRootNode()
		//int MaxObjectID { get; }

		//[XmlIgnore]
		//[JsonIgnore]
		//      internal int MaxObjectID { get; set; }
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
	/// This interface (ITopNode) hides its Internal members, and also imports public members from ITopNodePublic.
	/// <br/><br/>
	/// See here for a description of an internal interface inheriting a public interface:
	/// https://www.csharp411.com/c-internal-interface/ <br/><br/>
	/// Note that all interface members use the access level of their defining interface (e.g., internal, in this case), 
	/// regardless of the access modifier on each member.  
	/// Inheritance of a less restrictive interface (e.g., ITopNodePublic) leaves those inherited members with their less restrictive access, 
	/// even though the top-level interface is more restrictive.<br/><br/>
	/// Note that all internal interface member names are prefixed with "_" and start with a capital letter. 
	/// </summary>
	internal interface _ITopNode : ITopNode
	{
		/// <summary>
		/// Internal base object for initializing IETnodesRO.
		/// </summary>
		internal ObservableCollection<IdentifiedExtensionType> _IETnodes { get; }


		///// <summary>
		///// Internal version of MaxObjectID, which has a setter; MaxObjectID only has a getter.
		///// </summary> 
		//internal int _MaxObjectIDint { get; set; }

		/// <summary>
		/// Dictionary.  Given an Node ObjectGUID, returns the node's object reference.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal Dictionary<Guid, BaseType> _Nodes { get; }

		/// <summary>
		/// Dictionary.  Given a Node ObjectGUID, return the *parent* node's object reference
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal Dictionary<Guid, BaseType> _ParentNodes { get; }
		/// <summary>
		/// Dictionary.  Given a NodeID ObjectGUID, return a list of the child nodes object reference
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal Dictionary<Guid, List<BaseType>> _ChildNodes { get; }
		protected internal void ClearDictionaries();
	}
}

