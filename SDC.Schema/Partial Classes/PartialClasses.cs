
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using CSharpVitamins;
using System.Reflection.Emit;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using SDC.Schema.Extensions;


//!Handling Item and Items generic types derived from the xsd2Code++ code generator
namespace SDC.Schema
{

	#region   ..ITopNode SDC Elements
	public partial class FormDesignType : _ITopNode, ITopNodeDeserialize<FormDesignType>
	{
		#region ctor

		protected FormDesignType() : base()
		{ Init(); }
		public FormDesignType(BaseType? parentNode, string id) : base(parentNode, id)
		{
			if (this is DemogFormDesignType)
			{
				if (parentNode is null || parentNode is XMLPackageType parXP)
					Init();
			}
			else if (this is FormDesignType)
			{
				if (parentNode is null || parentNode is XMLPackageType parRFP || parentNode is InjectFormType parIF)
					Init();
			}
			else throw new InvalidOperationException("parentNode must be either null or RetrieveFormPackageType or InjectFormType");
		}

		private void Init()
		{
			ElementName = "FormDesign";
			ElementPrefix = "fd";
		}

		/// <summary>
		/// Reset and clean up some items (e.g., collections, SDC objects and extensions) that might interfere with garbage collection.
		/// May move to <see cref="IDisposable"/>
		/// </summary>
		~FormDesignType()
		{ }
		#endregion

		#region ITopNode 
		#region ITopNode Main
		//[JsonIgnore]
		//int _ITopNode._MaxObjectIDint { get; set; } = 0; //internal
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		//[XmlIgnore]
		//[JsonIgnore]
		//public int MaxObjectID { get => ((_ITopNode)this)._MaxObjectIDint; }  //save the highest object counter value for the current FormDesign tree
		[XmlIgnore]
		internal ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyDictionary<Guid, BaseType> Nodes
		{
			get
			{
				if (_nodesRO is null)
					_nodesRO = new(((_ITopNode)this)._Nodes);
				return _nodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> _ITopNode._ParentNodes { get; } = new Dictionary<Guid, BaseType>();
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, List<BaseType>> _ITopNode._ChildNodes { get; } = new Dictionary<Guid, List<BaseType>>();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		[XmlIgnore]
		[JsonIgnore]
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _ietNodesRO;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETNodes
		{
			get
			{
				if (_ietNodesRO is null)
					_ietNodesRO = new(((_ITopNode)TopNode)._IETnodes);
				return _ietNodesRO;
			}
		}

		public List<BaseType> GetSortedNodesList() => ((ITopNode)this).GetSortedNodes();
		public ObservableCollection<BaseType> GetSortedNodesObsCol() => ((ITopNode)this).GetSortedNodesObsCol();
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;

		/// <summary> Clears all internal dictionaries
		/// </summary>
		void _ITopNode.ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			topNode._IETnodes.Clear();
			_ietNodesRO = null;
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}
		/// <summary>
		/// Allows re-importing an SDC XML file into an existing top node object.
		/// Clears all dictionaries, sets topNodeTemp (which is a static property) to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetRootNode();
			((_ITopNode)this).ClearDictionaries();
			//((_ITopNode)this)._MaxObjectIDint = 0;
			Property = null;
			Extension = null;
			Comment = null;
			Rules = null;
			OnEvent = null;

			Body = null;
			Header = null;
			Footer = null;
		}
		#endregion

		#region Deserialization

		public static FormDesignType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static FormDesignType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		public static FormDesignType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static FormDesignType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static FormDesignType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static FormDesignType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromBson(sdcBson);
		public static FormDesignType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static FormDesignType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromMsgPack(sdcMsgPack);

		#endregion

		#endregion
	}
	public partial class DemogFormDesignType : FormDesignType
	{
		protected DemogFormDesignType() : base()
		{ }
		//public DemogFormDesignType(ITreeBuilder treeBuilder, BaseType parentNode = null, string id = "")
		//    : base(treeBuilder, parentNode, id)
		//{ }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"/>
		/// <param name="id"></param>
		public DemogFormDesignType(XMLPackageType? parentNode = null!, string id = "")
			: base(parentNode, id)
		{ }//use the FormDesignType constructor (base(parentNode, id))

		#region ITopNode
		#region Deserialization

		public static DemogFormDesignType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
		=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DemogFormDesignType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		public static DemogFormDesignType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DemogFormDesignType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static DemogFormDesignType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DemogFormDesignType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromBson(sdcBson);
		public static DemogFormDesignType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DemogFormDesignType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromMsgPack(sdcMsgPack);

		#endregion

		#endregion
	}
	public partial class DataElementType : _ITopNode, ITopNodeDeserialize<DataElementType>
	{
		protected DataElementType() : base()
		{ Init(); }
		public DataElementType(XMLPackageType? parentNode, string id = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "DE";
		}

		#region ITopNode
		#region ITopNodeMain
		//[JsonIgnore]
		//int _ITopNode._MaxObjectIDint { get; set; } //internal
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		//[XmlIgnore]
		//[JsonIgnore]
		//public int MaxObjectID { get => ((_ITopNode)TopNode)._MaxObjectIDint; }  //save the highest object counter value for the current FormDesign tree
		[XmlIgnore]
		internal ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyDictionary<Guid, BaseType> Nodes
		{
			get
			{
				if (_nodesRO is null)
					_nodesRO = new(((_ITopNode)this)._Nodes);
				return _nodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, BaseType> _ParentNodes { get; private set; } = new Dictionary<Guid, BaseType>();
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, List<BaseType>> _ChildNodes { get; private set; } = new Dictionary<Guid, List<BaseType>>();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		[XmlIgnore]
		[JsonIgnore]
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETNodes;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETNodes
		{
			get
			{
				if (_IETNodes is null)
					_IETNodes = new(((_ITopNode)TopNode)._IETnodes);
				return _IETNodes;
			}
		}

		public List<BaseType> GetSortedNodesList() => ((ITopNode)this).GetSortedNodes();
		public ObservableCollection<BaseType> GetSortedNodesObsCol() => ((ITopNode)this).GetSortedNodesObsCol();
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;

		/// <summary>
		/// 
		/// </summary>
		void _ITopNode.ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();

		}
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetRootNode();
			((_ITopNode)this).ClearDictionaries();
			//((_ITopNode)this)._MaxObjectIDint = 0;
			Property = null;
			Extension = null;
			Comment = null;

			Items = null;
		}

		#endregion

		#region Deserialization

		public static DataElementType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DataElementType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		public static DataElementType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DataElementType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static DataElementType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DataElementType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromBson(sdcBson);
		public static DataElementType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static DataElementType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<DataElementType>.DeserializeFromMsgPack(sdcMsgPack);

		#endregion

		#endregion

	}
	public partial class RetrieveFormPackageType : _ITopNode, ITopNodeDeserialize<RetrieveFormPackageType>
	{
		protected RetrieveFormPackageType() : base()
		{ Init(); }
		public RetrieveFormPackageType(RetrieveFormPackageType? parentNode, string id = "") : base(parentNode)
		{
			this.packageID = id;
			Init();
		}
		private void Init()
		{
			ElementPrefix = "RFP";
		}

		#region ITopNode
		#region ITopNodeMain
		//[JsonIgnore]
		//int _ITopNode._MaxObjectIDint { get; set; } //internal
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		//[XmlIgnore]
		//[JsonIgnore]
		//public int MaxObjectID { get => ((_ITopNode)TopNode)._MaxObjectIDint; }  //save the highest object counter value for the current FormDesign tree
		[XmlIgnore]
		internal ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyDictionary<Guid, BaseType> Nodes
		{
			get
			{
				if (_nodesRO is null)
					_nodesRO = new(((_ITopNode)this)._Nodes);
				return _nodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, BaseType> _ParentNodes { get; private set; } = new Dictionary<Guid, BaseType>();
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, List<BaseType>> _ChildNodes { get; private set; } = new Dictionary<Guid, List<BaseType>>();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		[XmlIgnore]
		[JsonIgnore]
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETNodes;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETNodes
		{
			get
			{
				if (_IETNodes is null)
					_IETNodes = new(((_ITopNode)TopNode)._IETnodes);
				return _IETNodes;
			}
		}

		public List<BaseType> GetSortedNodesList() => ((ITopNode)this).GetSortedNodes();
		public ObservableCollection<BaseType> GetSortedNodesObsCol() => ((ITopNode)this).GetSortedNodesObsCol();
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;

		void _ITopNode.ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetRootNode();
			((_ITopNode)this).ClearDictionaries();
			//((_ITopNode)this)._MaxObjectIDint = 0;
			Property = null;
			Extension = null;
			Comment = null;

			Items = null;
			SDCPackage = null;
		}


		#endregion


		#region Deserialization

		public static RetrieveFormPackageType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static RetrieveFormPackageType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		public static RetrieveFormPackageType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static RetrieveFormPackageType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static RetrieveFormPackageType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static RetrieveFormPackageType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromBson(sdcBson);
		public static RetrieveFormPackageType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static RetrieveFormPackageType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromMsgPack(sdcMsgPack);
		#endregion

		#endregion


	}
	public partial class BasePackageType : ExtensionBaseType
	{
		protected BasePackageType()
		{ }
		public BasePackageType(RetrieveFormPackageType? parentNode) : base(parentNode)
		{ }
	}
	public partial class PackageListType : _ITopNode, ITopNodeDeserialize<PackageListType>
	{
		protected PackageListType() : base()
		{ Init(); }
		public PackageListType(PackageListType? parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "PL";
		}
		#region ITopNode
		#region ITopNodeMain
		//[JsonIgnore]
		//int _ITopNode._MaxObjectIDint { get; set; } //internal
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		//[XmlIgnore]
		//[JsonIgnore]
		//public int MaxObjectID { get => ((_ITopNode)TopNode)._MaxObjectIDint; }  //save the highest object counter value for the current FormDesign tree
		[XmlIgnore]
		internal ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyDictionary<Guid, BaseType> Nodes
		{
			get
			{
				if (_nodesRO is null)
					_nodesRO = new(((_ITopNode)this)._Nodes);
				return _nodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, BaseType> _ParentNodes { get; private set; } = new Dictionary<Guid, BaseType>();
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, List<BaseType>> _ChildNodes { get; private set; } = new Dictionary<Guid, List<BaseType>>();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		[XmlIgnore]
		[JsonIgnore]
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETNodes;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETNodes
		{
			get
			{
				if (_IETNodes is null)
					_IETNodes = new(((_ITopNode)TopNode)._IETnodes);
				return _IETNodes;
			}
		}

		public List<BaseType> GetSortedNodesList() => ((ITopNode)this).GetSortedNodes();
		public ObservableCollection<BaseType> GetSortedNodesObsCol() => ((ITopNode)this).GetSortedNodesObsCol();
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;


		/// <summary>
		/// 
		/// </summary>
		void _ITopNode.ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetRootNode();
			((_ITopNode)this).ClearDictionaries();
			//((_ITopNode)this)._MaxObjectIDint = 0;
			Property = null;
			Extension = null;
			Comment = null;
			this.SDCPackageList = null;
			this.PackageItem = null;
			this.HTML = null;
		}


		#endregion


		#region Deserialization

		public static PackageListType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static PackageListType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		public static PackageListType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static PackageListType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static PackageListType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static PackageListType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromBson(sdcBson);
		public static PackageListType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static PackageListType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<PackageListType>.DeserializeFromMsgPack(sdcMsgPack);
		#endregion		#endregion
		#endregion

	}
	public partial class MappingType : _ITopNode, ITopNodeDeserialize<MappingType>
	{
		protected MappingType() : base()
		{ Init(); }
		public MappingType(XMLPackageType? parentNode, string templateID = "") : base(parentNode)
		{
			this.templateID = templateID;
			Init();
		}
		private void Init()
		{
			ElementPrefix = "MAP";
		}
		#region ITopNode
		#region ITopNodeMain
		//[JsonIgnore]
		//int _ITopNode._MaxObjectIDint { get; set; } //internal
		[XmlIgnore]
		[JsonIgnore]
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		//[XmlIgnore]
		//[JsonIgnore]
		//public int MaxObjectID { get => ((_ITopNode)TopNode)._MaxObjectIDint; }  //save the highest object counter value for the current FormDesign tree
		[XmlIgnore]
		internal ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyDictionary<Guid, BaseType> Nodes
		{
			get
			{
				if (_nodesRO is null)
					_nodesRO = new(((_ITopNode)this)._Nodes);
				return _nodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, BaseType> _ParentNodes { get; private set; } = new Dictionary<Guid, BaseType>();
		[XmlIgnore]
		[JsonIgnore]
		public Dictionary<Guid, List<BaseType>> _ChildNodes { get; private set; } = new Dictionary<Guid, List<BaseType>>();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		[XmlIgnore]
		[JsonIgnore]
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETNodes;
		[XmlIgnore]
		[JsonIgnore]
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETNodes
		{
			get
			{
				if (_IETNodes is null)
					_IETNodes = new(((_ITopNode)TopNode)._IETnodes);
				return _IETNodes;
			}
		}

		public List<BaseType> GetSortedNodesList() => ((ITopNode)this).GetSortedNodes();
		public ObservableCollection<BaseType> GetSortedNodesObsCol() => ((ITopNode)this).GetSortedNodesObsCol();
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;

		/// <summary>
		/// 
		/// </summary>
		void _ITopNode.ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetRootNode();
			((_ITopNode)this).ClearDictionaries();
			//((_ITopNode)this)._MaxObjectIDint = 0;
			Property = null;
			Extension = null;
			Comment = null;
			this.ItemMap = null;
			this.DefaultCodeSystem = null;
		}


		#endregion

		#region Deserialization

		public static MappingType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static MappingType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate);
		public static MappingType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static MappingType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate);
		public static MappingType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static MappingType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromBson(sdcBson);
		public static MappingType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate);
		public static MappingType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null)
			=> TopNodeSerializer<MappingType>.DeserializeFromMsgPack(sdcMsgPack);

		#endregion		#endregion
		#endregion


	}

	#endregion

	#region ..Main Types
	public partial class ButtonItemType
		: IChildItemsMember<ButtonItemType>
	{
		protected ButtonItemType() { Init(); }
		public ButtonItemType(BaseType parentNode, string id = "") : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ButtonAction";
			ElementPrefix = "B";
		}

	}

	public partial class InjectFormType : IChildItemsMember<InjectFormType>
	{
		protected InjectFormType() { Init(); }
		public InjectFormType(BaseType parentNode, string id = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			this._repeat = 0;
			ElementName = "InjectForm";
			ElementPrefix = "Inj";
		}
	}

	public partial class SectionBaseType
	{
		public SectionBaseType() { Init(); }
		internal SectionBaseType(BaseType parentNode, string id = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			this._ordered = true;
			ElementName = "Section";
			ElementPrefix = "S";
		}

		//public void FillSectionBaseType()
		//{ sdcTreeBuilder.FillSectionBase(this); }
	}

	public partial class SectionItemType : IChildItemsParent, IChildItemsMember<SectionItemType>
	{
		protected SectionItemType() { Init(); } //change back to protected
		public SectionItemType(BaseType parentNode, string id = "") : base(parentNode, id)
		{ Init(); }
		private void Init()
		{

		}

		#region IChildItemsParent Implementation
		private IChildItemsParent ci => this as IChildItemsParent;
		[XmlIgnore]
		[JsonIgnore]
		public ChildItemsType ChildItemsNode
		{
			get { return this.Item; }
			set { this.Item = value; }
		}
		#endregion
	}
	#region QAS

	#region Question

	public partial class QuestionItemType : IChildItemsParent, IChildItemsMember<QuestionItemType>, IQuestionItem, IQuestionList
	{
		protected QuestionItemType() { Init(); }  //need public parameterless constructor to support generics
		public QuestionItemType(BaseType parentNode, string id = "", string elementName = "", string elementPrefix = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			//this._readOnly = false;
			ElementName = "Question";
			ElementPrefix = "Q";

		}

		#region IChildItemsParent
		IChildItemsParent ci { get => (IChildItemsParent)this; }
		[XmlIgnore]
		[JsonIgnore]
		public ChildItemsType ChildItemsNode
		{
			get { return this.Item1; }
			set { this.Item1 = value; }
		}
		#endregion

	}

	public partial class QuestionItemBaseType : IQuestionBase
	{
		protected QuestionItemBaseType() { Init(); }
		public QuestionItemBaseType(BaseType parentNode, string id = "", string elementName = "", string elementPrefix = "") : base(parentNode, id)
		{
			Init();
			//this._readOnly = false;  // tag:#IsThisCorrect
		}
		private void Init()
		{ }

		[XmlIgnore]
		[JsonIgnore]
		public ListFieldType ListField_Item
		{
			get
			{
				if (Item?.GetType() == typeof(ListFieldType))
					return (ListFieldType)this.Item;
				else return null;
			}
			set { this.Item = value; }
		}


		[XmlIgnore]
		[JsonIgnore]
		public ResponseFieldType ResponseField_Item
		{
			get
			{
				if (Item?.GetType() == typeof(ResponseFieldType))
					return (ResponseFieldType)this.Item;
				else return null;
			}
			set { this.Item = value; }
		}
	}
	#endregion

	#region QAS ListItems and Lookups


	public partial class ListType : IQuestionList
	{
		protected ListType() { Init(); }
		public ListType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "lst";  // tag:#IsThisCorrect
		}

		/// <summary>
		/// Replaces Items; ListItem or DisplayedItem
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public List<DisplayedType> QuestionListMembers
		{
			get { return this.Items; }
			set { this.Items = value; }
		}
	}

	public partial class ListFieldType : IListField

	{// #NeedsTest
		protected ListFieldType() { Init(); }
		public ListFieldType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}

		private void Init()
		{
			ElementPrefix = "lf";
			this._colTextDelimiter = "|";
			this._numCols = ((byte)(1));
			this._storedCol = ((byte)(1));
			this._minSelections = ((ushort)(1));
			this._maxSelections = ((ushort)(1));
			this._ordered = true;
		}

		[XmlIgnore]
		[JsonIgnore]
		public ListType? List
		{
			get
			{
				if (Item is ListType)
					return (ListType)this.Item;
				else return null;
			}
			set { this.Item = value; }
		}
		/// <summary>
		/// Replaces Item
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public LookupEndPointType LookupEndpoint
		{
			get
			{
				if (Item.GetType() == typeof(LookupEndPointType))
					return (LookupEndPointType)this.Item;
				else return null!;
			}
			set { this.Item = value; }
		}

	}

	public partial class ListItemType : IChildItemsParent //, IListItem //, IQuestionListMember
	{
		protected ListItemType() { Init(); }
		public ListItemType(ListType parentNode, string id = "", string elementName = "", string elementPrefix = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "LI";
			ElementName = "ListItem";
		}

		#region IChildItemsParent

		/// <summary>
		/// The ChildItems node replaces "Item" (MainNodesType), and may contain:
		///"ButtonAction", typeof(ButtonItemType),
		///"DisplayedItem", typeof(DisplayedType),
		///"InjectForm", typeof(InjectFormType),
		///"Question", typeof(QuestionItemType),
		///"Section", typeof(SectionItemType),
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public ChildItemsType ChildItemsNode
		{
			get { return this.Item; }
			set { this.Item = value; }
		}

		#endregion


	}

	public partial class ListItemBaseType
	{
		protected ListItemBaseType() { Init(); }
		public ListItemBaseType(ListType parentNode, string id = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			this._selected = false;
			this._selectionDisablesChildren = false;
			this._selectionDeselectsSiblings = false;
			this._omitWhenSelected = false;
			this._repeat = 0;
		}
	}

	public partial class LookupEndPointType  //TODO: fix base class in Schema update
	{
		protected LookupEndPointType() { Init(); }
		public LookupEndPointType(ListFieldType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._includesHeaderRow = false;
			ElementPrefix = "LEP";
		}
	}

	#endregion

	#region Responses

	public partial class ListItemResponseFieldType
	{
		protected ListItemResponseFieldType() { Init(); }
		public ListItemResponseFieldType(ListItemBaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._responseRequired = false;
			ElementPrefix = "lirf";
		}
	}

	public partial class ResponseFieldType
	{
		protected ResponseFieldType() { Init(); }
		public ResponseFieldType(IdentifiedExtensionType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "ResponseField";
			ElementPrefix = "rf";
			this.Item = null; // #NeedsTest
		}
	}

	public partial class UnitsType
	{
		protected UnitsType() { Init(); }
		public UnitsType(BaseType parentNode) : base(parentNode)
		{
			Init();


		}
		private void Init()
		{
			_unitSystem = "UCUM";
			ElementPrefix = "un";
		}
	}

	#endregion

	#endregion


	#endregion

	#region Base Types

	public partial class BaseType : IBaseType //IBaseType inherits IMoveRemove and INavigate
	{
		/// <summary>
		/// This constructor is used only to deserialize SDC classes with the SDC.Schema serializers.
		///		Parent Nodes cannot be assigned through this constructor.  
		///		Node dictionaries cannot be populated here either.
		///		After the SDC object tree is created, parent nodes and other metadata can be assigned using <see cref="SdcUtil.ReflectRefreshTree(ITopNode, out string?, bool, bool, SdcUtil.CreateName?)"/>
		/// </summary>
		protected BaseType()
		{
			ObjectID = BaseType.LastObjectID++;

			if (this is ITopNode tn)
			{
				if (LastTopNode is null)
				{
					LastTopNode = tn; //Point to myself as the TopNode
					TopNode = tn;
				}
				else
				{
					TopNode = LastTopNode; //Point to LastTopNode as the TopNode
					LastTopNode = tn;
				}
			}//not ITopNode below here
			else if (LastTopNode is not null)
			{
				TopNode = LastTopNode;
			}
			else if (LastTopNode is null) 
			{ }//the caller is instantiating a new node that is not descended from an ITopNode node.

		}

		/// <summary>
		/// This parameterized constructor is NOT used to Deserialize SDC classes.
		/// LastTopNode is not needed here to find the previous TopNode.
		/// Instead, TopNode should be retrieved from the parent node, if it exists, and used to set the current TopNode.
		/// </summary>
		/// <param name="parentNode"></param>
		protected BaseType(BaseType? parentNode)
		{ 
			//+Assign this.TopNode
			if (this is ITopNode tn)
			{
				if (parentNode is null) TopNode = tn;
				else //if (parentNode is not null)
				{
					if (parentNode.TopNode is not null)
					{
						//parentNode's TopNode holds dictionaries we need to populate
						_ITopNode par_ITopNode;
						if (parentNode is _ITopNode ptn)
							par_ITopNode = ptn;//only occurs in RetrieveFormPackage under RetrieveFormPackage
						else par_ITopNode = (_ITopNode)parentNode.TopNode; //par_ITopNode could still be null here

						TopNode = par_ITopNode;
					}
					else { } //this node descends form a non-ITopNode root node; it cannot be added to ITopNode dictionaries without a TopNode
							 //throw new InvalidOperationException("ParentNode is not null, but ParentNode.TopNode is null");
				}
			}//not ITopNode here
			else if (parentNode is not null)
			{
				if (parentNode is ITopNode ptn)
					TopNode = (_ITopNode)parentNode;
				else if (parentNode.TopNode is not null)
					TopNode = (_ITopNode)parentNode.TopNode;
				else { } //this node descends form an "illegal" non-ITopNode root node; it cannot be added to ITopNode dictionaries without a TopNode,
						 //but we can still process it, as long as we check for null TopNode everywhere we need it (mainly in Dictionaries).
						 //later, this "illegal" tree will need to be grafted onto a legal tree and TopNodes will need to be assigned during
						 //the grafting/moving process, based on the target tree's TopNodes.
			}
			else if (parentNode is null) { }//the caller is trying to instantiate a standalone root node that is not a proper ITopNode.  TopNode is thus null here

			//!________Assign default sGuid, BaseName, ObjectGUID, ObjectID, @order, @name & populate dictionaries___________________

			SdcUtil.AssignGuid_sGuid_BaseName(this);			

			this.ObjectID = BaseType.LastObjectID++; //Can we retire ObjectID?

			this.RegisterNodeAndParent(parentNode);

			//The following code requires that the current node is first added
			//to the ParentNodes dictionary.  Thus, these statements must come
			//*after* the dictionaries are populated (in RegisterNodeAndParent)
			
			this.AssignOrder(orderGap: 10);
			this.AssignSimpleName(); //add options to keep original imported name, or to only create a new name when the original name is null.
		}

		#region     Init Methods

		#endregion Local methods

		#region  Local Members
		internal static int LastObjectID { get => lastObjectID; private set => lastObjectID = value; }
		internal void StoreError(string errorMsg) //TODO: Replace with event that logs each error
		{
			var exData = new Exception();
			exData.Data.Add("QuestionID: ", ParentIETypeNode?.ID.ToString() ?? "null");
			exData.Data.Add("Error: ", errorMsg);
			ExceptionList.Add(exData);
		}

		/// <summary>
		/// Find the ordinal position of an object (XML element) under an IdentifiedExtensionType (IET)-derived object.
		/// This number is used for creating the name attribute suffix. <br/>
		/// Returns -1 if an error occurs, e.g., if TopNode or its dictionaroes are not populated in the SDC tree.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal int SubIETcounter
		{
			get
			{
				if (this is IdentifiedExtensionType || this is ITopNode) return 0;
				if (TopNode?.Nodes is null)
					throw new Exception("Could not find SubIETcounter because T.Get_Nodes is null");
				if (ParentNode is null)
					throw new Exception("Could not find SubIETcounter because ParentNode is null");

				BaseType? node = this;
				int nodeCount = TopNode.Nodes.Values.Count;

				var i = 0;
				do
				{
					i++;
					node = node.GetNodePrevious();
					if (node is IdentifiedExtensionType || node is ITopNode) return i;
				} while (node != null && i < nodeCount);

				throw new Exception("Could not determine SubIETcounter because an ancestor node of type IdentifiedExtensionType could not be found");
			}
		}

		#endregion


		#region Public Members (IBaseType)

		[XmlIgnore]
		[JsonIgnore]
		public ITopNode? TopNode
		{
			get; protected internal set;
		}


		/// <summary>
		///  Hierarchical level using nested dot notation.
		/// </summary>
		/// <exception cref="NullReferenceException"/>
		[XmlIgnore]
		[JsonIgnore]
		public string DotLevel
		{
			get
			{
				if (TopNode is null) throw new NullReferenceException("To determine DotLevel, TopNode must not be null");
				//Walk up parent node tree and place each parent in a stack.
				//pop each node off the stack and determine its position (seq) in its parent object
				BaseType? par = ParentNode;
				var s = new Stack<BaseType>();
				s.Push(this);
				while (par != null)
				{
					s.Push(par);
					par = par.ParentNode;
				}
				int level = 0;
				var sb = new StringBuilder("0");
				var topNode = (_ITopNode)TopNode;
				int seq;
				s.Pop();  //pop off the top node, which has no parent.
				while (s.Count > 0)
				{
					var n = s.Pop();

					if (topNode._ChildNodes.TryGetValue(n.ParentNode!.ObjectGUID, out List<BaseType>? lst))
					{ seq = lst.IndexOf(n) + 1; }
					else { seq = 0; }
					sb.Append('.').Append(seq); ;
					level++;
				}
				return sb.ToString();
			}
		}
		//TODO: Use or remove AutoNameFlag
		[XmlIgnore]
		[JsonIgnore]
		public bool AutoNameFlag { get; set; } = false;

		/// <summary>
		/// The name of XML element that is output from this class instance.
		/// Some SDC types are used in conjunction with multiple element names.  
		/// The auto-generated classes do not provide a way to determine the element name form the class instance.
		/// However, it is possible to achieve this effect by reflection of 
		/// attributes at the time of creating each node, and also after hydrating the SDC object tree from XML.
		/// ElementName will be most useful for auto-generating @name attributes for some elements.
		/// In many cases, ElementName will be assigned through class constructors, but it can also be assigned 
		/// through this property after the object is instantiated
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public string ElementName
		{
			get
			{
				if (!_elementName.IsNullOrWhitespace())
					return _elementName;
				try
				{
					//if the object has not yet been added to the SDC tree - 
					//(i.e., added to its parent object) when this is called,
					//an exception will be thrown in sdcUtil:
					var meta = SdcUtil.GetElementPropertyInfoMeta(this);
					return meta.XmlElementName ?? meta.PropName ?? "";
				}
				catch
				{
					return "";
				}


			}
			protected internal set
			{
				_elementName = value;
			}
		}


		/// <summary>
		/// NEW
		/// For the SDC property's XML element, returns the Order value from the property's XMLElementAttribute
		/// Assigned by reflection at the time of object creation.
		/// TODO: Add to IBaseType
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public int ElementOrder
		{
			get => elementOrder;
			internal set
			{
				if (elementOrder == value)
					return;
				elementOrder = value;
			}
		}
		// TODO: Add ElementIndex to IBaseType
		/// <summary>
		/// For the SDC property's XML element, if the property is found inside a List object.
		/// Return -1 if this object is not found inside a List object, or if TopNode is null.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public int ElementIndex
		{
			get
			{
				var par = this.ParentNode;
				if (par is null) return -1;
				if (TopNode is null) return -1;
				var topNode = (_ITopNode)TopNode;
				topNode._ChildNodes.TryGetValue(par.ObjectGUID, out List<BaseType>? kids);
				if (kids is null || kids.Count == 0) return -1;

				return kids.IndexOf(this);
			}

		}

		/// <summary>
		/// The prefix used 
		/// in the @name attribute that is output from this class instance
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public string ElementPrefix
		{
			get
			{ //assign default prefix from the ElementName
				if (_elementPrefix.IsNullOrWhitespace())
				{
					_elementPrefix = _elementName;
					if (_elementName.IsNullOrWhitespace()) return "";
					//make sure first letter is lower case for non-IET types:
					//if (!(this.GetType().IsSubclassOf(typeof(IdentifiedExtensionType)))) _elementPrefix = _elementPrefix.Substring(0, 1).ToLower() + _elementPrefix.Substring(1);
					if (!this.GetType().IsSubclassOf(typeof(IdentifiedExtensionType))) _elementPrefix = string.Concat(_elementPrefix.Substring(0, 1).ToLower(), _elementPrefix.AsSpan(1));
				}
				//if (this is QuestionItemType && _elementPrefix != "Q") Debugger.Break();
				return _elementPrefix;
			}
			set
			{
				//if (this is QuestionItemType && _elementPrefix != "Q") Debugger.Break();
				_elementPrefix = value;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public int ObjectID { get; internal set; }
		/// <inheritdoc/>
		[XmlIgnore]
		[JsonIgnore]
		public Guid ObjectGUID { get; internal set; }
		[XmlIgnore]
		[JsonIgnore]
		//TODO: not currently implemented		
		///<inheritdoc cref="IBaseType.NodeType"/>
		public ItemTypeEnum NodeType { get; private set; }
		//[XmlIgnore]
		//[JsonIgnore]
		//public Boolean IsLeafNode { get=> !this.HasChildren();  } //TODO: can use INavigate reflection methods for this, since it changes during tree editing

		/// <summary>
		/// Returns the ObjectID of the parent object (representing the parent XML element)
		/// The ObjectID, which is a sequentially assigned integer value, assigned at the time a node is added to the tree.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public int ParentID
		{
			get
			{
				if (ParentNode != null)
				{ return ParentNode.ObjectID; }
				else return -1;
			}
		}

		/// <summary>
		/// Retrieve the BaseType object that is the immediate parent of the current object in the object tree
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public BaseType? ParentNode
		{
			get
			{
				_ITopNode topNode;
				if (this is _ITopNode itn) topNode = itn;
				else if (TopNode is not null) topNode = (_ITopNode)TopNode;
				else return null;
				topNode._ParentNodes.TryGetValue(this.ObjectGUID, out BaseType? outParentNode);
				return outParentNode;

			}
		}
		/// <summary>
		/// Retrieve the BaseType object that is the SDC Package containing the current object in the object tree
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public RetrieveFormPackageType? PackageNode
		{
			get => _PackageNode;  //this works for objects that were created with the parentNode constructor
			internal set => _PackageNode = value;
		}

		[XmlIgnore]
		[JsonIgnore]
		public IdentifiedExtensionType? ParentIETypeNode
		{
			get
			{
				BaseType? par = this.ParentNode;

				while (par is not null)
				{
					if (par is IdentifiedExtensionType iet) return iet;
					par = par.ParentNode;
				}
				return null;
			}
		}
		/// <summary>
		/// Returns the ID property of the closest ancestor of type IdentifiedExtensionType.  
		/// For eCC, this is the Parent node's ID, which is derived from  the parent node's CTI_Ckey, a.k.a. ParentItemCkey.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public string ParentIETypeID
		{ get => ParentIETypeNode?.ID; }

		[XmlIgnore]
		[JsonIgnore]
		/// <summary
		/// Loaded => 1; <br/>
		/// New => 2; <br/>
		/// MovedUp => 3; <br/>
		/// MovedDown => 4; <br/>
		/// Updated => 5;
		/// </summary>
		public int ItemViewState { get; set; } = 1;
		/// <summary>
		/// Reset TopNodeTemp to null, so that nodes newly added to a top node<br/>
		/// use the correct node for the top (root) of the object tree
		/// </summary>
		public static void ResetRootNode()  //Rename to ResetStatic
		{
			LastTopNode = null;
			LastObjectID = 0;
		}

		[XmlIgnore]
		[JsonIgnore]
		/// <inheritdoc/>
		public string BaseName { get; set; }
		#endregion

		#region ChangeTracking
		//Properties to mark changed nodes for serialization to database etc.
		[XmlIgnore]
		[JsonIgnore]
		public Boolean Added { get; private set; }
		[XmlIgnore]
		[JsonIgnore]
		public Boolean Changed { get; private set; }
		[XmlIgnore]
		[JsonIgnore]
		public Boolean Deleted { get; private set; }
		[XmlIgnore]
		[JsonIgnore]
		public DateTime UpdatedDateTime { get; private set; }
		#endregion

		#region Private Members
		private int elementOrder;
		private string _elementName = "";
		private string _elementPrefix = "";
		private RetrieveFormPackageType _PackageNode;
		private static ITopNode? LastTopNode;
		//private static BaseType? LastAddedNode;
		private static int lastObjectID = 0;

		private List<Exception> ExceptionList = new();

		#endregion

	}

	public partial class ExtensionBaseType : IExtensionBase
	{
		protected ExtensionBaseType() { }
		public ExtensionBaseType(BaseType? parentNode) : base(parentNode)
		{ }
	}

	#region IExtensionBaseTypeMember
	public partial class ExtensionType : IExtensionBaseTypeMember
	{
		private IExtensionBaseTypeMember Iebtm { get => (IExtensionBaseTypeMember)this; }
		protected ExtensionType() { Init(); }
		public ExtensionType(BaseType parentNode) : base(parentNode) { Init(); }
		private void Init()
		{

		}

		#region IExtensionBaseTypeMember
		//public bool Remove() => Iebtm.Remove();
		//public bool Move(ExtensionBaseType ebtTarget, int newListIndex = -1) => Iebtm.MoveI(this, ebtTarget, newListIndex);
		#endregion

	}
	public partial class PropertyType : IExtensionBaseTypeMember, IHtmlHelpers
	{
		private IExtensionBaseTypeMember Iebtm { get => (IExtensionBaseTypeMember)this; }
		protected PropertyType() { Init(); }
		public PropertyType(ExtensionBaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "Property";
			ElementPrefix = "p";
		}

		public HTML_Stype AddHTML()
		{
			this.TypedValue = new DataTypes_SType(this);
			var rtf = new RichTextType(TypedValue);
			var h = (this as IHtmlHelpers).AddHTML(rtf);
			return h;
		}

		#region IExtensionBaseTypeMember
		//public bool Remove() => Iebtm.Remove();
		//public bool Move(ExtensionBaseType ebtTarget, int newListIndex = -1) => Iebtm.MoveI(this, ebtTarget, newListIndex);
		#endregion

	}
	public partial class CommentType : IExtensionBaseTypeMember
	{
		private IExtensionBaseTypeMember Iebtm { get => (IExtensionBaseTypeMember)this; }
		protected CommentType() { Init(); }
		public CommentType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "cmt";
		}

		#region IExtensionBaseTypeMember
		//public bool Remove() => Iebtm.Remove();
		//public bool Move(ExtensionBaseType ebtTarget, int newListIndex = -1) => Iebtm.MoveI(this, ebtTarget, newListIndex);
		#endregion

	}
	#endregion

	public partial class IdentifiedExtensionType : IIdentifiedExtensionType
	{
		protected IdentifiedExtensionType() { Init(); }
		protected IdentifiedExtensionType(BaseType? parentNode, string id = "") : base(parentNode)
		{
			if (id.IsNullOrWhitespace())
				ID = $"->{BaseName}"; //BaseName was based on the sGuid and assigned in the BaseType ctor
			else ID = id;
			Init();
		}
		private void Init()
		{   //The ID may be assigned later by a deserializer after this runs, but that should be OK
			if (string.IsNullOrWhiteSpace(ID))
				this.ID = this.ObjectGUID.ToString();// #IsThisCorrect 
		}
	}

	public partial class RepeatingType //this is an SDC abstract class
	{
		protected RepeatingType()
		{
			Init();
		}
		protected RepeatingType(BaseType parentNode, string id = "") : base(parentNode, id)
		{
			Init();
		}
		private void Init()
		{
			this._minCard = ((ushort)(1));
			this._maxCard = ((ushort)(1));
			this._repeat = 0;
		}

	}

	public partial class ChildItemsType
	{
		protected ChildItemsType() { Init(); }
		public ChildItemsType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "ChildItems";
			ElementPrefix = "ch";
		}

		[XmlIgnore]
		[JsonIgnore]
		public List<IdentifiedExtensionType> ChildItemsList
		{
			get { return this.Items; }
			set { this.Items = value; }
		}

		bool Remove(int NodeIndex)
		{
			var node = ChildItemsList[NodeIndex];
			if (node != null) return node.RemoveRecursive();
			return false;

		}
	}

	#endregion

	#region DisplayedType and Members

	public partial class DisplayedType : IChildItemsMember<DisplayedType> //, IQuestionListMember
	{
		protected DisplayedType() { Init(); }
		public DisplayedType(BaseType parentNode, string id = "", string elementName = "", string elementPrefix = "") : base(parentNode, id)
		{
			Init();

		}
		private void Init()
		{
			this._enabled = true;
			this._visible = true;
			this._mustImplement = true;
			this._showInReport = DisplayedTypeShowInReport.True;
			ElementName = "DisplayedItem";
			ElementPrefix = "DI";
		}
	}

	#region DisplayedType Members

	public partial class BlobType : IDisplayedTypeMember
	{
		protected BlobType() { Init(); }
		public BlobType(DisplayedType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "Blob";
			ElementPrefix = "blob";
		}
	}

	public partial class LinkType : IDisplayedTypeMember
	{
		protected LinkType() { Init(); }
		public LinkType(DisplayedType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "Link";
			ElementPrefix = "link";
		}
	}

	#region Coding
	public partial class CodingType : IDisplayedTypeMember
	{
		protected CodingType() { Init(); }
		public CodingType(ExtensionBaseType parentNode) : base(parentNode)
		{
			Init();


		}
		private void Init()
		{
			ElementName = "Coding";
			ElementPrefix = "cod";
		}
	}

	public partial class CodeMatchType
	{
		protected CodeMatchType() { Init(); }
		public CodeMatchType(CodingType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._codeMatchEnum = CodeMatchTypeCodeMatchEnum.ExactCodeMatch;
			ElementName = "CodeMatch";
			ElementPrefix = "cmat";
		}
	}

	public partial class CodeSystemType
	{
		protected CodeSystemType() { Init(); }
		public CodeSystemType(ExtensionBaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "CodeSystem";
			ElementPrefix = "csys";
		}
	}

	#endregion

	#endregion


	#endregion

	#region DataTypes
	public partial class DataTypes_DEType
	{
		protected DataTypes_DEType() { Init(); }
		public DataTypes_DEType(ResponseFieldType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Response"; //response element
			ElementPrefix = "rsp";  //response element            
		}

		/// <summary>
		/// any *_DEType data type
		/// </summary>
		[XmlIgnore]
		public BaseType DataTypeDE_Item
		{
			get { return this.Item; }
			set { this.Item = value; }
		}
	}

	public partial class anyType_DEtype
	{
		protected anyType_DEtype() { Init(); }
		public anyType_DEtype(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "any";
		}
	}


	public partial class DataTypes_SType
	{
		protected DataTypes_SType() { Init(); }
		public DataTypes_SType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "DataTypes";
		}

		/// <summary>
		/// any *_SType data type
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public BaseType DataTypeS_Item
		{
			get { return this.Item; }
			set { this.Item = value; }
		}
	}

	public partial class anyURI_DEtype
	{
		protected anyURI_DEtype() { Init(); }
		public anyURI_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "uri";
		}
	}

	public partial class anyURI_Stype : IVal
	{


		protected anyURI_Stype() { Init(); }
		public anyURI_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();


		}
		private void Init()
		{
			ElementPrefix = "uri";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => val;
			set
			{//patterns like '#..#' (more than one '#') and '%ZZ' (illegal ASCII escape codes) are not allowed in a URI.  '%20' (space code) and a single '#' are allowed.
			 //null, "", and empty spaces are allowed, but these values will not be serialized to XML, JSON, etc
				if (value is null || !(Regex.Match(value, @"%(?![0-9A-F]{2})|#.*#").Success))
					val = value;
				else StoreError("Supplied value parameter was not in anyURI string format");
			}
		}

	}

	public partial class base64Binary_DEtype
	{
		protected base64Binary_DEtype() { Init(); }
		public base64Binary_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "b64";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			ElementPrefix = "b64DE";
		}

	}

	public partial class base64Binary_Stype : IVal
	{
		protected base64Binary_Stype() { Init(); }
		public base64Binary_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "b64S";
		}

		[XmlIgnore]
		[JsonIgnore]

		public string ValXmlString
		{
			get => val.ToString();
			set
			{
				var s64 = new Span<byte>();
				if (Convert.TryFromBase64String(value, s64, out int bytesWritten)) val = s64.ToArray();
				else StoreError("Supplied value parameter was not in base64Binary string format");
			}
		}
	}

	public partial class boolean_DEtype
	{
		protected boolean_DEtype() { Init(); }
		public boolean_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "bool";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{

		}

	}

	public partial class boolean_Stype : IVal
	{
		protected boolean_Stype() { Init(); }
		public boolean_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "bool";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => val.ToString();
			set
			{
				if (Boolean.TryParse(value, out bool result)) val = result;
				else StoreError("Supplied value parameter was not in boolean string format");
			}
		}
	}

	public partial class byte_DEtype
	{
		protected byte_DEtype() { Init(); }
		public byte_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "byte";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{   //it's not necessary to set bool default values;
			//this._allowGT = false;
			//this._allowGTE = false;
			//this._allowLT = false;
			//this._allowLTE = false;
			//this._allowAPPROX = false;
		}
	}

	public partial class byte_Stype : IVal
	{
		protected byte_Stype() { Init(); }
		public byte_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "sbyte";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class date_DEtype
	{
		protected date_DEtype() { Init(); }
		public date_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "date";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class date_Stype : IVal
	{
		protected date_Stype() { Init(); }
		public date_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "date";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class dateTime_DEtype
	{
		protected dateTime_DEtype() { Init(); }
		public dateTime_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "dt";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class dateTime_Stype : IVal
	{
		protected dateTime_Stype() { Init(); }
		public dateTime_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "dt";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class dateTimeStamp_DEtype
	{
		protected dateTimeStamp_DEtype() { Init(); }
		public dateTimeStamp_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "dts";
			//SetNames(elementName, elementPrefix);
		}
		private  void Init()
		{
			ElementPrefix = "dtsDE";
		}
	}

	public partial class dateTimeStamp_Stype : IVal
	{
		protected dateTimeStamp_Stype() { Init(); }
		public dateTimeStamp_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "dtsS";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class dayTimeDuration_DEtype
	{
		protected dayTimeDuration_DEtype() { Init(); }
		public dayTimeDuration_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "dtdur";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class dayTimeDuration_Stype : IVal
	{
		protected dayTimeDuration_Stype() { Init(); }
		public dayTimeDuration_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "dtdur";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class decimal_DEtype
	{
		protected decimal_DEtype() { Init(); }
		public decimal_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "dec";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class decimal_Stype : IVal
	{
		protected decimal_Stype() { Init(); }
		public decimal_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "dec";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class double_DEtype
	{
		protected double_DEtype() { Init(); }
		public double_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "dbl";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class double_Stype : IVal
	{
		protected double_Stype() { Init(); }
		public double_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "dbl";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class duration_DEtype
	{
		protected duration_DEtype() { Init(); }
		public duration_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "dur";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class duration_Stype : IVal
	{
		protected duration_Stype() { Init(); }
		public duration_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class float_DEtype
	{
		protected float_DEtype() { Init(); }
		public float_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "flt";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class float_Stype : IVal
	{
		protected float_Stype() { Init(); }
		public float_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "flt";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class gDay_DEtype
	{
		protected gDay_DEtype() { Init(); }
		public gDay_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "day";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class gDay_Stype : IVal
	{
		protected gDay_Stype() { Init(); }
		public gDay_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "day";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class gMonth_DEtype
	{
		protected gMonth_DEtype() { Init(); }
		public gMonth_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "mon";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class gMonth_Stype : IVal
	{
		protected gMonth_Stype() { Init(); }
		public gMonth_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "mon";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class gMonthDay_DEtype
	{
		protected gMonthDay_DEtype() { Init(); }
		public gMonthDay_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "mday";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}

	}

	public partial class gMonthDay_Stype : IVal
	{
		protected gMonthDay_Stype() { Init(); }
		public gMonthDay_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "mday";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class gYear_DEtype
	{
		protected gYear_DEtype() { Init(); }
		public gYear_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "y";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class gYear_Stype : IVal
	{
		protected gYear_Stype() { Init(); }
		public gYear_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "y";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class gYearMonth_DEtype
	{
		protected gYearMonth_DEtype() { Init(); }
		public gYearMonth_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "ym";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}
	public partial class gYearMonth_Stype : IVal
	{
		protected gYearMonth_Stype() { Init(); }
		public gYearMonth_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "ym";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class hexBinary_DEtype
	{
		protected hexBinary_DEtype() { Init(); }
		public hexBinary_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "hexb";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			ElementPrefix = "hxBinDE";
		}
	}

	public partial class hexBinary_Stype : IVal
	{
		string _hexBinaryStringVal;

		protected hexBinary_Stype() { Init(); }
		public hexBinary_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "hexb";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}

		[System.Xml.Serialization.XmlAttributeAttribute(DataType = "string")] //changed to string
		public string valHex
		{
			get { return _hexBinaryStringVal; }
			set { _hexBinaryStringVal = value; }
		}
	}

	public partial class HTML_DEtype
	{
		protected HTML_DEtype() { Init(); }
		public HTML_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "html";
			//SetNames(elementName, elementPrefix);
			//this.Any = new List<System.Xml.XmlElement>();
		}
		private void Init()
		{
			ElementPrefix = "htmlDE";
		}
	}

	public partial class HTML_Stype : IVal
	{
		protected HTML_Stype()
		{
			Init();
		}
		public HTML_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
		private void Init()
		{
			this.Any = new List<System.Xml.XmlElement>();  // #NeedsTest
			this.AnyAttr = new List<System.Xml.XmlAttribute>();
			ElementPrefix = "html";
		}
	}


	public partial class int_DEtype
	{
		protected int_DEtype() { Init(); }
		public int_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "int";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class int_Stype : IVal
	{
		protected int_Stype() { Init(); }
		public int_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "int";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class integer_DEtype
	{
		protected integer_DEtype() { Init(); }
		public integer_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;            //ElementPrefix = "intr";
		}
	}

	public partial class integer_Stype : IVal
	{
		protected integer_Stype() { Init(); }
		public integer_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "intr";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}


		///// <summary>
		///// Added to support proper decimal set/get; 
		///// Decimal is the best data type to match W3C integer.
		///// This will not work with XML serializer, bc decimal types always serialize trailing zeros,
		///// and trailing zeros are not allowed with integer types
		///// TODO: Need to truncate (or possibly round) any digits after the decimal point in the setter/getter
		///// For positive/negative etc integers, need to check the sign and throw error if incorrect.
		///// May want to throw errors for for min/max allowed values also - not sure about this)
		///// May need to import System.Numerics.dll to use BigInteger
		///// </summary>
		///// 
		//[XmlIgnore]
		//[JsonIgnore]
		//public virtual decimal? valDec
		////rlm 2/11/18 probably don't want to use this
		//{
		//    get
		//    {
		//        if (val != null && val.Length > 0)
		//        { return Convert.ToDecimal(this.val); }
		//        return null;
		//    }
		//    set
		//    {
		//        if (value != null)
		//        { this.val = Math.Truncate(value.Value).ToString(); }
		//        else this.val = null;

		//    }
		//}

	}

	public partial class long_DEtype
	{
		protected long_DEtype() { Init(); }
		public long_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "lng";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class long_Stype : IVal
	{
		protected long_Stype() { Init(); }
		public long_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "lng";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class negativeInteger_DEtype
	{
		protected negativeInteger_DEtype() { Init(); }
		public negativeInteger_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "nint";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class negativeInteger_Stype : IVal
	{
		protected negativeInteger_Stype() { Init(); }
		public negativeInteger_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "nint";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class nonNegativeInteger_DEtype
	{
		protected nonNegativeInteger_DEtype() { Init(); }
		public nonNegativeInteger_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "nnint";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class nonNegativeInteger_Stype : IVal
	{
		protected nonNegativeInteger_Stype() { Init(); }
		public nonNegativeInteger_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "nnint";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class nonPositiveInteger_DEtype
	{
		protected nonPositiveInteger_DEtype() { Init(); }
		public nonPositiveInteger_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "npint";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class nonPositiveInteger_Stype : IVal
	{
		protected nonPositiveInteger_Stype() { Init(); }
		public nonPositiveInteger_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "npint";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class positiveInteger_DEtype
	{
		protected positiveInteger_DEtype() { Init(); }
		public positiveInteger_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "pint";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class positiveInteger_Stype : IVal
	{
		protected positiveInteger_Stype() { Init(); }
		public positiveInteger_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "pint";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class short_DEtype
	{
		protected short_DEtype() { Init(); }
		public short_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "sh";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class short_Stype : IVal
	{
		protected short_Stype() { Init(); }
		public short_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "sh";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class string_DEtype
	{
		protected string_DEtype() { Init(); }
		public string_DEtype(BaseType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "str";
			//SetNames(elementName, elementPrefix);
		} //{if (elementName.Length > 0) ElementName = elementName; }
		private void Init()
		{
			ElementPrefix = "strDE";
		}
	}

	public partial class string_Stype
	{
		protected string_Stype() { Init(); }
		public string_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "str";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class time_DEtype
	{
		protected time_DEtype() { Init(); }
		public time_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "tim";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class time_Stype : IVal
	{
		protected time_Stype() { Init(); }
		public time_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "tim";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class unsignedByte_DEtype
	{
		protected unsignedByte_DEtype() { Init(); }
		public unsignedByte_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "ubyte";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class unsignedByte_Stype : IVal
	{
		protected unsignedByte_Stype() { Init(); }
		public unsignedByte_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "ubyte";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class unsignedInt_DEtype
	{
		protected unsignedInt_DEtype() { Init(); }
		public unsignedInt_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "unint";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}

	}

	public partial class unsignedInt_Stype : IVal
	{
		protected unsignedInt_Stype() { Init(); }
		public unsignedInt_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "uint";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class unsignedLong_DEtype
	{
		protected unsignedLong_DEtype() { Init(); }
		public unsignedLong_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "ulng";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class unsignedLong_Stype : IVal
	{
		protected unsignedLong_Stype() { Init(); }
		public unsignedLong_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "ulng";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class unsignedShort_DEtype
	{
		protected unsignedShort_DEtype() { Init(); }
		public unsignedShort_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "ush";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class unsignedShort_Stype : IVal
	{
		protected unsignedShort_Stype() { Init(); }
		public unsignedShort_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "ush";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class XML_DEtype
	{
		protected XML_DEtype() { Init(); }//this.Any = new List<XmlElement>(); }
		public XML_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//this.Any = new List<XmlElement>();
		}
		private void Init()
		{
			ElementPrefix = "xmlDE";
		}
	}

	public partial class XML_Stype : IVal
	{
		protected XML_Stype()
		{
			Init();
		}
		public XML_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this.Any = new List<System.Xml.XmlElement>();  // #NeedsTest
														   //this.AnyAttr = new List<System.Xml.XmlAttribute>(); // Add AnyAttr to Schema? #NeedsFix ?
			ElementPrefix = "xmlS";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}

	public partial class yearMonthDuration_DEtype
	{
		protected yearMonthDuration_DEtype() { Init(); }
		public yearMonthDuration_DEtype(DataTypes_DEType parentNode) : base(parentNode)
		{
			Init();
			//ElementPrefix = "ymd";
			//SetNames(elementName, elementPrefix);
		}
		private void Init()
		{
			this._allowGT = false;
			this._allowGTE = false;
			this._allowLT = false;
			this._allowLTE = false;
			this._allowAPPROX = false;
		}
	}

	public partial class yearMonthDuration_Stype : IVal
	{
		protected yearMonthDuration_Stype() { Init(); }
		public yearMonthDuration_Stype(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._quantEnum = dtQuantEnum.EQ;
			ElementPrefix = "ymd";
		}
		[XmlIgnore]
		[JsonIgnore]
		public string ValXmlString
		{
			get => throw new NotImplementedException();
			set
			{
				throw new NotImplementedException();
			}
		}
	}
	#endregion

	#region Rules

	public partial class ItemNameType
	{
		protected ItemNameType() { Init(); }
		public ItemNameType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "itnm";
		}
	}
	public partial class ItemNameAttributeType
	{
		protected ItemNameAttributeType() { Init(); }

		public ItemNameAttributeType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._attributeName = "val";
		}
	}
	public partial class NameType
	{
		protected NameType() { Init(); }
		public NameType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "nm";
		}
	}
	public partial class TargetItemIDType
	{
		protected TargetItemIDType() { Init(); }
		public TargetItemIDType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "tiid";
		}
	}
	public partial class TargetItemNameType
	{
		protected TargetItemNameType() { Init(); }
		public TargetItemNameType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "tinm";
		}
	}
	public partial class TargetItemXPathType
	{
		protected TargetItemXPathType() { Init(); }
		public TargetItemXPathType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "tixp";
		}
		//{if (elementName.Length > 0) ElementName = elementName; }
	}
	public partial class ListItemParameterType
	{
		protected ListItemParameterType() { Init(); }
		public ListItemParameterType(BaseType parentNode) : base(parentNode)
		{
			Init();
			this._listItemAttribute = "associatedValue";
		}
		private void Init()
		{
			this._dataType = "string";
			ElementPrefix = "liParam";
		}
	}
	public partial class ParameterItemType
	{
		protected ParameterItemType() { Init(); }
		public ParameterItemType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._dataType = "string";
			this._sourceItemAttribute = "val";
			ElementPrefix = "paramItem";
		}
	}
	public partial class PredAlternativesType
	{
		public PredAlternativesType() { Init(); }
		public PredAlternativesType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			this._minAnswered = 1;
			this._maxAnswered = 0;
			ElementPrefix = "predAlt";
		}
	}
	public partial class PredEvalAttribValuesType
	{
		protected PredEvalAttribValuesType() { Init(); }
		public PredEvalAttribValuesType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			this._boolOp = PredEvalAttribValuesTypeBoolOp.AND;
			ElementPrefix = "predEvAttVal";
		}
	}
	public partial class PredGuardTypeSelectionSets
	{
		protected PredGuardTypeSelectionSets() { Init(); }
		public PredGuardTypeSelectionSets(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
		}
	}
	public partial class PredSingleSelectionSetsType
	{
		protected PredSingleSelectionSetsType() { Init(); }
		public PredSingleSelectionSetsType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._maxSelections = ((short)(1));
			ElementPrefix = "predSngSelSet";
		}
	}
	public partial class RuleAutoActivateType
	{
		protected RuleAutoActivateType() { Init(); }
		public RuleAutoActivateType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._onlyIf = false;
			this._setVisibility = toggleType.@true;
			this._setEnabled = toggleType.@true;
			this._setExpanded = toggleType.@true;
			ElementPrefix = "raa";
			//this._x_removeResponsesWhenDeactivated = false;
		}
	}
	public partial class RuleAutoSelectType
	{
		protected RuleAutoSelectType() { Init(); }
		public RuleAutoSelectType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._onlyIf = false;
			ElementPrefix = "ras";
		}
	}
	public partial class RuleListItemMatchTargetsType
	{
		protected RuleListItemMatchTargetsType() { Init(); }
		public RuleListItemMatchTargetsType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._attributeToMatch = RuleListItemMatchTargetsTypeAttributeToMatch.associatedValue;
			ElementPrefix = "rlimt";
		}
	}
	//public partial class SelectionSetsActionType
	//{
	//    protected SelectionSetsActionType() { Init(); }
	//    public SelectionSetsActionType(BaseType parentNode, string elementName = "", string elementPrefix = "") : base(parentNode)
	//    {
	//        Init();
	//    }
	//    private void Init()
	//    {
	//        this._not = false;
	//    }
	//}
	public partial class ValidationTypeSelectionSets
	{
		protected ValidationTypeSelectionSets() { Init(); }
		public ValidationTypeSelectionSets(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementPrefix = "vtss";
		}
	}
	public partial class ValidationTypeSelectionTest
	{
		protected ValidationTypeSelectionTest() { Init(); }
		public ValidationTypeSelectionTest(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementPrefix = "valTypSel";
		}
	}
	public partial class PredSelectionTestType
	{
		protected PredSelectionTestType() { Init(); }
		public PredSelectionTestType(BaseType parentNode) : base(parentNode)
		{ Init(); }
		private  void Init()
		{
			ElementPrefix = "predSelTst";
		}
	}
	public partial class CallFuncType
	{
		protected CallFuncType() { Init(); }
		public CallFuncType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._dataType = "string";
			ElementPrefix = "callFunc";
		}
	}
	partial class CallFuncBaseType
	{
		protected CallFuncBaseType() { Init(); }
		public CallFuncBaseType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._returnList = false;
			this._listDelimiter = "|";
			this._allowNull = true;
			ElementPrefix = "callFuncBase";
		}
	}
	partial class CallFuncBoolType
	{
		protected CallFuncBoolType() { Init(); }
		public CallFuncBoolType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementPrefix = "callFuncBool";
		}
	}


	#endregion
	#region PredActions
	//AttributeEval -       AttributeEvalActionType (actions)
	//ScriptBoolFunc -      ScriptBoolFuncActionType (actions)
	//CallBoolFunction -    CallFuncBoolActionType (actions)
	//MultiSelections -     MultiSelectionsActionType
	//SelectionSets -       SelectionSetsActionType (rule)
	//SelectionTest -       SelectionTestActionType
	//Group -               PredActionType (events)
	//SelectMatchingListItems - RuleSelectMatchingListItemsType (actions)
	//public partial class MultiSelectionsActionType
	//{
	//    protected MultiSelectionsActionType()
	//    { Init(); }
	//    public MultiSelectionsActionType(BaseType parentNode) : base(parentNode)
	//    { Init(); }
	//    private void Init()
	//    {

	//    }
	//}
	//public partial class SelectionTestActionType
	//{
	//    protected SelectionTestActionType()
	//    { Init(); }
	//    public SelectionTestActionType(BaseType parentNode) : base(parentNode)
	//    { Init(); }
	//    private void Init()
	//    {

	//    }
	//}

	public partial class PredMultiSelectionSetBoolType
	{
		protected PredMultiSelectionSetBoolType() { Init(); }
		public PredMultiSelectionSetBoolType(BaseType parentNode) : base(parentNode)
		{ Init(); }
		private void Init()
		{
			ElementPrefix = "pmssb";
		}
	}


	#endregion
	#region  Actions

	public partial class ActionsType : IActions
	{
		protected ActionsType() { Init(); }
		public ActionsType(ExtensionBaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Actions";
			ElementPrefix = "act";
		}
	}
	public partial class ActActionType
	{
		protected ActActionType() { Init(); }
		public ActActionType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Action";
			ElementPrefix = "actAct";
		}
		[XmlIgnore]
		public List<ExtensionBaseType> ActAction_Items
		{
			get { return Items; }
			set
			{
				if (Items == value)
					return;
				Items = value;
				OnPropertyChanged(nameof(ActAction_Items), this);
			}
		}
	}
	public partial class RuleSelectMatchingListItemsType
	{
		protected RuleSelectMatchingListItemsType() { Init(); }
		public RuleSelectMatchingListItemsType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SelectMatchingListItems";
			ElementPrefix = "selMli";
		}
	}
	public partial class ActAddCodeType
	{
		protected ActAddCodeType() { Init(); }
		public ActAddCodeType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "AddCode";
			ElementPrefix = "actCode";
		}
	}
	public partial class ActInjectType : InjectFormType
	{
		protected ActInjectType() { Init(); }
		public ActInjectType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Inject";
			ElementPrefix = "actInj";
		}
	}
	public partial class ActSaveResponsesType
	{
		protected ActSaveResponsesType() { Init(); }
		public ActSaveResponsesType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Save";
			ElementPrefix = "actSvRsp";
		}
	}
	public partial class ActSendReportType
	{
		protected ActSendReportType() { Init(); }
		public ActSendReportType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SendReport";
			ElementPrefix = "actSndRep";
		}

		internal List<ExtensionBaseType> Email_Phone_WebSvc_List
		{
			get { return this.Items; }
			set { this.Items = value; }
		}
	}
	public partial class ActSendMessageType
	{
		protected ActSendMessageType() { Init(); }
		public ActSendMessageType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		} //"SendMessage111" in Schema
		private void Init()
		{
			ElementName = "SendMessage";
			ElementPrefix = "actSndMsg";
		}

		/// <summary>
		/// List&lt;BaseType> accepts: EmailAddressType, PhoneNumberType, WebServiceType
		/// </summary>
		internal List<ExtensionBaseType> Email_Phone_WebSvc_List
		{
			get { return this.Items; }
			set { this.Items = value; }
		}
	}
	public partial class ActSetAttributeType
	{
		protected ActSetAttributeType() { Init(); }
		public ActSetAttributeType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SetAttributeValue";
			ElementPrefix = "actSetAttVal";
		}
	}
	public partial class ActSetAttrValueScriptType
	{
		protected ActSetAttrValueScriptType() { Init(); }
		public ActSetAttrValueScriptType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SetAttributeValueScript";
			ElementPrefix = "setAttValScr";
		}
	}
	public partial class ActSetBoolAttributeValueCodeType
	{
		protected ActSetBoolAttributeValueCodeType() { Init(); }
		public ActSetBoolAttributeValueCodeType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SetBoolAttributeValueCode";
			this._attributeName = "actSetBlAttValCode";
		}
	}
	public partial class ScriptCodeBoolType
	{
		protected ScriptCodeBoolType() { Init(); }
		public ScriptCodeBoolType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "";
			ElementPrefix = "scbt";
			this._not = false;
		}
	}
	public partial class ActShowFormType
	{
		protected ActShowFormType() { Init(); }
		public ActShowFormType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ShowForm";
			ElementPrefix = "actShowFrm";
		}
	}
	public partial class ActShowMessageType
	{
		protected ActShowMessageType() { Init(); }
		public ActShowMessageType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ShowMessage";
			ElementPrefix = "actShowMsg";
		}
	}
	public partial class ActShowReportType
	{
		protected ActShowReportType() { Init(); }

		public ActShowReportType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ShowReport";
			ElementPrefix = "actShowRpt";
		}
	}
	public partial class ActPreviewReportType
	{
		protected ActPreviewReportType() { Init(); }
		public ActPreviewReportType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "PreviewReport";
			ElementPrefix = "actPrevRpt";
		}
	}
	public partial class ActValidateFormType
	{
		protected ActValidateFormType() { Init(); }
		public ActValidateFormType(ActionsType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ValidateForm";
			ElementPrefix = "actValFrm";
			this._validateDataTypes = false;
			this._validateRules = false;
			this._validateCompleteness = false;
		}
		public ActValidateFormType Fill_ActValidateFormType()
		{ return null; }
	}

	//public partial class ScriptBoolFuncActionType
	//{
	//    protected ScriptBoolFuncActionType() { Init(); }
	//    public ScriptBoolFuncActionType(ActionsType parentNode) : base(parentNode)
	//    {
	//        Init();
	//    }
	//    private void Init()
	//    {
	//        ElementName = "ScriptBoolFunc";
	//    }
	//}

	public partial class ScriptCodeAnyType
	{
		protected ScriptCodeAnyType()
		{ Init(); }
		public ScriptCodeAnyType(ActionsType parentNode) : base(parentNode)
		{ Init(); }
		private void Init()
		{
			ElementName = "RunCode";
			ElementPrefix = "sca";
			this._dataType = "string";
		}
	}
	public partial class ScriptCodeBaseType
	{
		protected ScriptCodeBaseType() { Init(); }
		public ScriptCodeBaseType(ActionsType parentNode) : base(parentNode)
		{ Init(); }
		private void Init()
		{
			ElementName = "";
			ElementPrefix = "scb";
			this._returnList = false;
			this._listDelimiter = "|";
			this._allowNull = true;
		}
	}
	//public partial class CallFuncActionType
	//{
	//    protected CallFuncActionType() { Init(); }
	//    public CallFuncActionType(ActionsType parentNode) : base(parentNode) 
	//    { Init(); }
	//    private void Init()
	//    {
	//        ElementName = "CallFunction";
	//    }
	//}

	//public partial class CallFuncBoolActionType
	//{
	//    protected CallFuncBoolActionType() { Init(); }
	//    public CallFuncBoolActionType(ActionsType parentNode) : base(parentNode)
	//    { Init(); }
	//    private void Init()
	//    {
	//        ElementName = "CallBoolFunction";
	//    }
	//}

	#endregion
	#region Events
	public partial class OnEventType : IDisplayedTypeMember
	{
		protected OnEventType() { Init(); }
		public OnEventType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "onEv";
		}
	}

	public partial class RulesType
	{
		protected RulesType() { Init(); }
		public RulesType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "rules";
		}
	}

	public partial class EventType : IDisplayedTypeMember
	{
		protected EventType() { Init(); }
		public EventType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "evnt";
		}
	}

	public partial class PredGuardType : IDisplayedTypeMember
	{

		protected PredGuardType() { Init(); }
		public PredGuardType(BaseType parentNode) : base(parentNode)
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			this._boolOp = PredEvalAttribValuesTypeBoolOp.AND;
			ElementPrefix = "predGrd";
		}
	}

	public partial class PredActionType
	{
		protected PredActionType() { Init(); }
		public PredActionType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._not = false;
			this._boolOp = PredEvalAttribValuesTypeBoolOp.AND;
			ElementPrefix = "predAct";
		}
	}

	public partial class FuncBoolBaseType
	{
		protected FuncBoolBaseType() { Init(); }
		public FuncBoolBaseType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this._allowNull = true;
			ElementPrefix = "funcBoolBase";
		}
	}


	#endregion

	#region Contacts

	public partial class ContactType : IDisplayedTypeMember, IAddPerson, IAddOrganization
	{
		protected ContactType() { Init(); }
		public ContactType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "cntct";
		}
	}

	public partial class OrganizationType
	{
		protected OrganizationType() { Init(); }
		public OrganizationType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "org";
		}
	}

	public partial class PersonType
	{
		protected PersonType() { Init(); }
		public PersonType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "pers";
		}
	}

	public partial class AddressType
	{
		protected AddressType() { Init(); }
		public AddressType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "adrs";
		}
	}

	public partial class AreaCodeType
	{
		protected AreaCodeType() { Init(); }
		public AreaCodeType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "arcd";
		}
	}
	#endregion

	#region Resources
	public partial class RichTextType : IHtmlHelpers
	{
		protected RichTextType() { Init(); }
		public RichTextType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "rtt";
		}
		IHtmlHelpers ihh { get => this; }
		public HTML_Stype AddHTML()
		{
			var h = ihh.AddHTML(this);
			return h;
		}
	}


	#endregion

	#region Classes that need ctor parameters

	#region RequestForm (Package)
	public partial class ComplianceRuleType
	{
		protected ComplianceRuleType() { Init(); }
		public ComplianceRuleType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "cr";
		}
	}

	public partial class SubmissionRuleType
	{
		protected SubmissionRuleType() { Init(); }
		public SubmissionRuleType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "sr";
		}

	}


	public partial class HashType
	{
		protected HashType() { Init(); }
		public HashType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "hsh";
		}
	}


	public partial class IdentifierType
	{
		protected IdentifierType() { Init(); }
		public IdentifierType(BaseType parentNode) : base(parentNode)
		{
			Init();


		}
		private void Init()
		{
			this.ElementPrefix = "id";
		}
	}

	public partial class LanguageCodeISO6393_Type
	{
		protected LanguageCodeISO6393_Type() { Init(); }
		public LanguageCodeISO6393_Type(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "lc";
		}
	}

	public partial class LanguageType
	{
		protected LanguageType() { Init(); }
		public LanguageType(BaseType parentNode) : base(parentNode)
		{
			Init();


		}
		private void Init()
		{
			this.ElementPrefix = "lng";
		}
	}

	public partial class ProvenanceType
	{
		protected ProvenanceType() { Init(); }
		public ProvenanceType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "prv";
		}
	}

	public partial class ReplacedIDsType
	{
		protected ReplacedIDsType() { Init(); }
		public ReplacedIDsType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "rid";
		}
	}

	public partial class VersionType
	{
		protected VersionType() { Init(); }
		public VersionType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "ver";
		}
	}

	public partial class VersionTypeChanges
	{
		protected VersionTypeChanges() { Init(); }
		public VersionTypeChanges(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "vch";
		}
	}


	#endregion

	#region Contacts classes

	public partial class ContactsType
	{
		protected ContactsType() { Init(); }
		public ContactsType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "contacts";
		}
	}

	public partial class CountryCodeType
	{
		protected CountryCodeType() { Init(); }
		public CountryCodeType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "ctc";
		}
	}

	public partial class DestinationType
	{
		protected DestinationType() { Init(); }
		public DestinationType(BaseType parentNode) : base(parentNode)
		{
			Init();


		}
		private void Init()
		{
			ElementPrefix = "dest";
		}
	}


	public partial class PhoneNumberType
	{
		protected PhoneNumberType() { Init(); }
		public PhoneNumberType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "phn";
		}
	}

	public partial class PhoneType
	{
		protected PhoneType() { Init(); }
		public PhoneType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementName = "PhoneType";
			ElementPrefix = "pht";
		}
	}

	public partial class JobType
	{
		protected JobType() { Init(); }
		public JobType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "job";
		}
	}
	#endregion

	#region  Email
	public partial class EmailAddressType
	{
		protected EmailAddressType() { Init(); }
		public EmailAddressType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "emd";
		}
	}

	public partial class EmailType
	{
		protected EmailType() { Init(); }
		public EmailType(BaseType parentNode) : base(parentNode)
		{
			Init();

			//this.Usage = new string_Stype();
			//this.EmailClass = new string_Stype();
			//this.EmailAddress = new EmailAddressType();
		}
		private void Init()
		{
			ElementPrefix = "em";
		}
	}

	#endregion

	#region Files


	public partial class ApprovalType
	{
		protected ApprovalType() { Init(); }
		public ApprovalType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "app";
		}
	}

	public partial class AssociatedFilesType
	{
		protected AssociatedFilesType() { Init(); }
		public AssociatedFilesType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "asf";
		}
	}

	public partial class AcceptabilityType
	{
		protected AcceptabilityType() { Init(); }
		public AcceptabilityType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "acc";
		}
	}


	public partial class FileDatesType
	{
		protected FileDatesType() { Init(); }
		public FileDatesType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "fld";
		}
	}
	public partial class FileHashType
	{
		protected FileHashType() { Init(); }
		public FileHashType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "flh";
		}
	}

	public partial class FileType
	{
		protected FileType() { Init(); }
		public FileType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "fil";
		}
	}

	public partial class FileUsageType
	{
		protected FileUsageType() { Init(); }
		public FileUsageType(BaseType parentNode) : base(parentNode)
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "flu";
		}
	}

	#endregion

	#endregion

	#region Registry Summary Types

	#endregion

}

