
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
using static SDC.Schema.SdcUtil;


//!Handling Item and Items generic types derived from the xsd2Code++ code generator
namespace SDC.Schema
{

	#region   ITopNode SDC Elements
	public partial class FormDesignType : _ITopNode, ITopNodeDeserialize<FormDesignType>, _IUniqueIDs
    {
		#region ctor

		protected FormDesignType() : base()
		{ 
			Init(); 
		}
		public FormDesignType(BaseType? parentNode, string id, int position = -1, string elementName = "FormDesign") : base(parentNode, id, position, elementName)
		{
			if (this is DemogFormDesignType)
			{
				if (parentNode is null || parentNode is XMLPackageType parXP)
					Init();
				ElementName = "DemogFormDesign";
			}
			else if (this is FormDesignType)
			{
				if (parentNode is null || parentNode is XMLPackageType parRFP || parentNode is InjectFormType parIF)
					Init();
				ElementName = "FormDesign";
			}
			else throw new InvalidOperationException("parentNode must be either null or RetrieveFormPackageType or InjectFormType");
		}

		private void Init()
		{
			ElementName = "FormDesign";
			ElementPrefix = "FD";
		}

		/// <summary>
		/// Reset and clean up some items (e.g., collections, SDC objects and extensions) that might interfere with garbage collection.
		/// May move to <see cref="IDisposable"/>
		/// </summary>
		~FormDesignType()
		{ }
        #endregion

        /// <summary>
        /// When cloning and repeating an IET-rooted subtree in a <see cref="FormDesignType"/> tree, <br/>
        /// a repeat suffix must be appended to each ID and name property in the cloned subtree.<br/><br/>
        /// The suffix consists or 2 underscores ("__") followed by the string version of this <see cref="RepeatCounter"/>.<br/>
        /// For example, the first repeated subtree clone inside the <see cref="FormDesignType"/> tree will have the suffix "__1" <br/>
        /// appended to every ID and name property in the cloned subtree.<br/><br/>
        /// The calling application must update <see cref="RepeatCounter"/> by one, each time a new subtree <br/>
        /// is cloned and copied into the parent <see cref="FormDesignType"/> tree.<br/><br/>
        /// If cloning and repeating subtrees is handled through <see cref="IMoveRemoveExtensions.Move"/>, <br/>
		/// <see cref="RepeatCounter"/> will be automatically incremented by one for each cloned repeat. 
        /// </summary>
        [XmlIgnore]
		[JsonIgnore]
        public uint RepeatCounter { get; set; } = 0;
        HashSet<string> _IUniqueIDs._UniqueIDs { get; } = new();
        #region ITopNode 

        /// <summary>
        /// ReadOnlyObservableCollection of all SDC nodes.
        /// </summary>
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
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes
		{
			get
			{				
				if (_IETnodesRO is null)
				{
					if (TopNode is null) throw new NullReferenceException("TopNode cannot be null");
					_IETnodesRO = new(((_ITopNode)TopNode)!._IETnodes);
				}
				return _IETnodesRO;
			}
		}

		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;
		/// <summary>
		/// Allows re-importing an SDC XML file into an existing top node object.
		/// Clears all dictionaries, sets topNodeTemp (which is a static property) to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetLastTopNode();
			((_ITopNode)this)._ClearDictionaries();
			((_ITopNode)this)._MaxObjectID = 0;
			Property = null;
			Extension = null;
			Comment = null;
			Rules = null;
			OnEvent = null;

			Body = null;
			Header = null;
			Footer = null;
		}


		#region _ITopNode

		private ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETnodesRO;
		int _ITopNode._MaxObjectID { get; set; } = 0;

		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
	
		Dictionary<Guid, BaseType> _ITopNode._ParentNodes { get; } = new ();

		Dictionary<Guid, List<BaseType>> _ITopNode._ChildNodes { get; } = new ();

		HashSet<int> _ITopNode._TreeSort_NodeIds { get; } = new ();

		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>

		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		HashSet<string> _ITopNode._UniqueBaseNames { get; } = new();
        
        HashSet<string> _ITopNode._UniqueNames { get; } = new();

        /// <summary> Clears all internal dictionaries
        /// </summary>
        void _ITopNode._ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			topNode._IETnodes.Clear();
			_IETnodesRO = null;
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}

		#endregion


		#endregion
		#region Deserialization
		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static FormDesignType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);

		#endregion
	}
	public partial class DemogFormDesignType : FormDesignType
	{
		protected DemogFormDesignType() : base()
		{ Init(); }
		//public DemogFormDesignType(ITreeBuilder treeBuilder, BaseType parentNode = null, string id = "")
		//    : base(treeBuilder, parentNode, id)
		//{ }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"/>
		/// <param name="id"></param>
		public DemogFormDesignType(XMLPackageType? parentNode, string id): base(parentNode, id, -1, "DemogFormDesign")
		{ Init(); }//use the FormDesignType constructor (base(parentNode, id))

		private void Init()
		{
			ElementName = "DemogFormDesign";
			ElementPrefix = "DFD";
		}

		#region Deserialization

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static DemogFormDesignType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DemogFormDesignType>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);

		#endregion
	}

	public partial class DataElementType : _ITopNode, ITopNodeDeserialize<DataElementType>, _IUniqueIDs
    {
		protected DataElementType() : base()
		{ Init(); }
		public DataElementType(XMLPackageType? parentNode, string id = "", int position = -1) : base(parentNode, id, position, "DataElement")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "DataElement";
			ElementPrefix = "DE";
			Items = new();
		}
        /// <summary>
        /// Gets or sets the data element Items.
        /// </summary>
        /// <value>
        /// DataElement.Items (List&lt;IdentifiedExtensionType>).<br/>
		/// May contains Section, Question, DisplayedItem, Button and InjectForm nodes.
        /// </value>
        [XmlIgnore]
		[JsonIgnore]
		public List<IdentifiedExtensionType> DataElement_Items
		{
			get
			{ return Items; }
			set
			{
				Items = ItemsMutator(Items, value);
			}
		}
        HashSet<string> _IUniqueIDs._UniqueIDs { get; } = new();

        #region ITopNode

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
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes
		{
			get
			{
				if (_IETnodesRO is null)
				{
					if (TopNode is null) throw new NullReferenceException("TopNode cannot be null");
					_IETnodesRO = new(((_ITopNode)TopNode)!._IETnodes);
				}
				return _IETnodesRO;
			}
		}

		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to new(). <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetLastTopNode();
			((_ITopNode)this)._ClearDictionaries();
			//((_ITopNode)this)._MaxObjectIDint = 0;
			Property = new();
			Extension = new();
			Comment = new();

			Items = new();
		}
		#region _ITopNode
		private ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETnodesRO;

		int _ITopNode._MaxObjectID { get; set; } = 0;
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		Dictionary<Guid, BaseType> _ITopNode._ParentNodes { get; } = new Dictionary<Guid, BaseType>();
		Dictionary<Guid, List<BaseType>> _ITopNode._ChildNodes { get; } = new Dictionary<Guid, List<BaseType>>();
		HashSet<int> _ITopNode._TreeSort_NodeIds { get; } = new();

		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		HashSet<string> _ITopNode._UniqueBaseNames { get; } = new();
        HashSet<string> _ITopNode._UniqueNames { get; } = new();
        /// <summary>
        /// 
        /// </summary>
        void _ITopNode._ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();

		}
		#endregion

		#endregion
		#region Deserialization

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static DataElementType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<DataElementType>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);

		#endregion

	}
	public partial class RetrieveFormPackageType : _ITopNode, ITopNodeDeserialize<RetrieveFormPackageType>, _IUniqueIDs
    {
		protected RetrieveFormPackageType() : base()
		{ Init(); }
		public RetrieveFormPackageType(RetrieveFormPackageType? parentNode, string packageID, int position = -1) : base(parentNode, position, "SDCPackage")
		{
			this.packageID = packageID;
			Init();
		}
		private void Init()
		{
			ElementName = "SDCPackage";
			ElementPrefix = "PKG";
			this.Items = new();
			this.SubmissionRule = new();
			this.ComplianceRule = new();
			this.SDCPackage = new();
		}
        HashSet<string> _IUniqueIDs._UniqueIDs { get; } = new();

        #region ITopNode
        [XmlIgnore]
		[JsonIgnore]
		public int MaxObjectID { get; internal set; } = 0;
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
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes
		{
			get
			{
				if (_IETnodesRO is null)
				{
					if (TopNode is null) throw new NullReferenceException("TopNode cannot be null");
					_IETnodesRO = new(((_ITopNode)TopNode)!._IETnodes);
				}
				return _IETnodesRO;
			}
		}

		public List<BaseType> GetSortedNodesList() => ((ITopNode)this).GetSortedNodes();
		public ObservableCollection<BaseType> GetSortedNodesObsCol() => ((ITopNode)this).GetSortedNodesObsCol();
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;
		/// <summary>
		/// Clears all dictionaries, sets top level objects to new(). <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetLastTopNode();
			((_ITopNode)this)._ClearDictionaries();
			((_ITopNode)this)._MaxObjectID = 0;
			Property = new();
			Extension = new();
			Comment = new();

			Items = new();
			SubmissionRule = new();
			ComplianceRule = new();
			SDCPackage = new();
		}

		#region _ITopNode
		private ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETnodesRO;

		int _ITopNode._MaxObjectID { get; set; } = 0;
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		Dictionary<Guid, BaseType> _ITopNode._ParentNodes { get; } = new Dictionary<Guid, BaseType>();
		Dictionary<Guid, List<BaseType>> _ITopNode._ChildNodes { get; } = new Dictionary<Guid, List<BaseType>>();
		HashSet<int> _ITopNode._TreeSort_NodeIds { get; } = new();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		HashSet<string> _ITopNode._UniqueBaseNames { get; } = new();
        HashSet<string> _ITopNode._UniqueNames { get; } = new();
        void _ITopNode._ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}


		#endregion
		#endregion



		#region Deserialization

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static RetrieveFormPackageType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<RetrieveFormPackageType>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);
		#endregion		


	}
	public partial class BasePackageType : ExtensionBaseType
	{
		protected BasePackageType()
		{ }
		public BasePackageType(RetrieveFormPackageType? parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{ }
	}
	public partial class HTMLPackageType : ExtensionBaseType
	{
		protected HTMLPackageType()
		{ }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "HTML"<br/>
		/// "HTMLPackage"<br/>
		/// </param>
		public HTMLPackageType(ExtensionBaseType? parentNode, string elementName) : base(parentNode, -1, elementName)
		{ 
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "htmlPkg";
		}
	}
	public partial class XMLPackageType : ExtensionBaseType, _IUniqueIDs //This might deserve to be ITopNode, but for now, it's not
    {
		protected XMLPackageType()
		{ }
		public XMLPackageType(ExtensionBaseType? parentNode, int position = -1) : base(parentNode, position, "XMLPackage")
		{ }
		private void Init()
		{
			ElementName = "XMLPackage";
			ElementPrefix = "xmlPkg";
		}
		HashSet<string> _IUniqueIDs._UniqueIDs { get; } = new();
    }
	public partial class PackageItemType : ExtensionBaseType
	{
		protected PackageItemType()
		{ }
		public PackageItemType(ExtensionBaseType? parentNode, int position = -1) : base(parentNode, position, "PackageItem")
		{ }
		private void Init()
		{
			ElementName = "PackageItem";
			ElementPrefix = "pkgItem";
		}

	}

	public partial class PackageListType : _ITopNode, ITopNodeDeserialize<PackageListType>, _IUniqueIDs
    {
		protected PackageListType() : base()
		{ Init(); }
		public PackageListType(PackageListType? parentNode, int position = -1) : base(parentNode, position, "SDCPackageList")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SDCPackageList";
			ElementPrefix = "PL";
		}
        HashSet<string> _IUniqueIDs._UniqueIDs { get; } = new();

        #region ITopNode
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
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes
		{
			get
			{
				if (_IETnodesRO is null)
				{
					if (TopNode is null) throw new NullReferenceException("TopNode cannot be null");
					_IETnodesRO = new(((_ITopNode)TopNode)!._IETnodes);
				}
				return _IETnodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetLastTopNode();
			((_ITopNode)this)._ClearDictionaries();
			((_ITopNode)this)._MaxObjectID = 0;
			Property = null;
			Extension = null;
			Comment = null;
			this.SDCPackageList = null;
			this.PackageItem = null;
			this.HTML = null;
		}
		#region _ITopNode

		private ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETnodesRO;

		int _ITopNode._MaxObjectID { get; set; } = 0;
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();	
		Dictionary<Guid, BaseType> _ITopNode._ParentNodes { get; } = new Dictionary<Guid, BaseType>();
		Dictionary<Guid, List<BaseType>> _ITopNode._ChildNodes { get; } = new Dictionary<Guid, List<BaseType>>();
		HashSet<int> _ITopNode._TreeSort_NodeIds { get; } = new();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		HashSet<string> _ITopNode._UniqueBaseNames { get; } = new();
        HashSet<string> _ITopNode._UniqueNames { get; } = new();
        /// <summary>
        /// 
        /// </summary>
        void _ITopNode._ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}
		#endregion



		#endregion


		#region Deserialization

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static PackageListType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<PackageListType>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);

		#endregion


	}
	public partial class MappingType : _ITopNode, ITopNodeDeserialize<MappingType>
	{
		protected MappingType() : base()
		{ Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="templateID"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "Map"<br/>
		/// "MapTemplate" (uses position)<br/>
		/// </param>
		public MappingType(XMLPackageType? parentNode, string templateID, int position, string elementName) : base(parentNode, position, elementName)
		{
			this.templateID = templateID;
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "MAP";
		}
		#region ITopNode
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
		public ReadOnlyObservableCollection<IdentifiedExtensionType> IETnodes
		{
			get
			{
				if (_IETnodesRO is null)
				{
					if (TopNode is null) throw new NullReferenceException("TopNode cannot be null");
					_IETnodesRO = new(((_ITopNode)TopNode)!._IETnodes);
				}
				return _IETnodesRO;
			}
		}
		[XmlIgnore]
		[JsonIgnore]
		public bool GlobalAutoNameFlag { get; set; } = true;
		/// <summary>
		/// Clears all dictionaries, sets topNodeTemp to null, sets top level objects to null. <br/>
		/// Does <b>not</b> reset <b>TopNode</b> - this must be done by the calling code for nested top nodes, if needed .
		/// </summary>
		public void ResetRootNode()
		{
			BaseType.ResetLastTopNode();
			((_ITopNode)this)._ClearDictionaries();
			((_ITopNode)this)._MaxObjectID = 0;
			Property = null;
			Extension = null;
			Comment = null;
			this.ItemMap = null;
			this.DefaultCodeSystem = null;
		}
		#region _ITopNode
		private ReadOnlyDictionary<Guid, BaseType>? _nodesRO;
		private ReadOnlyObservableCollection<IdentifiedExtensionType>? _IETnodesRO;

		int _ITopNode._MaxObjectID { get; set; } = 0;
		Dictionary<Guid, BaseType> _ITopNode._Nodes { get; } = new Dictionary<Guid, BaseType>();
		Dictionary<Guid, BaseType> _ITopNode._ParentNodes { get; } = new ();
		Dictionary<Guid, List<BaseType>> _ITopNode._ChildNodes { get;} = new ();
		HashSet<int> _ITopNode._TreeSort_NodeIds { get; } = new();
		/// <summary>Base object for the ReadOnlyObservableCollection IETnodes.</summary>
		ObservableCollection<IdentifiedExtensionType> _ITopNode._IETnodes { get; } = new();
		HashSet<string> _ITopNode._UniqueBaseNames { get; } = new();
        HashSet<string> _ITopNode._UniqueNames { get; } = new();
        /// <summary>
        /// 
        /// </summary>
        void _ITopNode._ClearDictionaries()
		{
			var topNode = (_ITopNode)this;
			topNode._Nodes.Clear();
			topNode._ParentNodes.Clear();
			topNode._ChildNodes.Clear();
			_nodesRO = null;
			topNode._IETnodes.Clear();
		}
		#endregion

		#endregion


		#region Deserialization

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXmlPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromXmlPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromXmlPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromXml(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromXml(string sdcXml, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromXml(sdcXml, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromJsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromJsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromJson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromJson(string sdcJson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromJson(sdcJson, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBsonPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromBsonPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromBsonPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromBson(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromBson(string sdcBson, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromBson(sdcBson, refreshSdc, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPackPath(string, bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromMsgPackPath(string sdcPath, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromMsgPackPath(sdcPath, refreshSdc: true, createNameDelegate, orderStart, orderGap);

		///<inheritdoc cref="TopNodeSerializer{T}.DeserializeFromMsgPack(byte[], bool, SdcUtil.CreateName?, int, int)"/>
		public static MappingType DeserializeFromMsgPack(byte[] sdcMsgPack, bool refreshSdc = true, SdcUtil.CreateName? createNameDelegate = null, int orderStart = 0, int orderGap = 10)
			=> TopNodeSerializer<MappingType>.DeserializeFromMsgPack(sdcMsgPack, refreshSdc, createNameDelegate, orderStart, orderGap);

		#endregion		#endregion
		


	}

	#endregion

	#region Main Types
	public partial class ButtonItemType
		: IChildItemsMember<ButtonItemType>
	{
		protected ButtonItemType() { Init(); }
		public ButtonItemType(BaseType parentNode, string id = "", int position = -1) : base(parentNode, id, position, "ButtonAction")
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
		public InjectFormType(BaseType parentNode, string id = "", int position = -1) : base(parentNode, id, position, "InjectForm")
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

        internal SectionBaseType(BaseType parentNode, string id, int position, string elementName) : base(parentNode, id, position, elementName)
		{ Init(); }
		private void Init()
		{	this._ordered = true;	}
	}

	public partial class SectionItemType : IChildItemsParent, IChildItemsMember<SectionItemType>
	{
		protected SectionItemType() { Init(); } //change back to protected
        /// <summary>
        /// Note: When adding a new Header, Body, or Footer node, use -1 for <paramref name="position"/>, and specify "Header", "Body", or "Footer" for <paramref name="elementName"/>.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="elementName"></param>
        public SectionItemType(BaseType parentNode, string id, int position = -1, string elementName = "Section") : base(parentNode, id, position, elementName)
		{ Init(); }
		private void Init()
		{			
			ElementName = "Section";
			ElementPrefix = "S";
		}

		#region IChildItemsParent Implementation
		private IChildItemsParent ci => this as IChildItemsParent;
		[XmlIgnore]
		[JsonIgnore]
		public ChildItemsType ChildItemsNode
		{
			get { return this.Item; }
			set
			{
				Item = ItemMutator(Item, value);
			}
		}
		#endregion
	}
	#region QAS

	#region Question

	public partial class QuestionItemType : IChildItemsParent, IChildItemsMember<QuestionItemType>, IQuestionItem, IQuestionList
	{
		protected QuestionItemType() { Init(); }  //need public parameterless constructor to support generics
		public QuestionItemType(BaseType parentNode, string id = "", int position = -1) : base(parentNode, id, position)
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
		public ChildItemsType? ChildItemsNode
		{
			get 
			{ return Item1;	}
			set 
			{ 

				this.Item1 = value; 
			
			}
		}
		#endregion

	}

	public partial class QuestionItemBaseType : IQuestionBase
	{
		protected QuestionItemBaseType() { Init(); }
		public QuestionItemBaseType(BaseType parentNode, string id, int position) : base(parentNode, id, position, "Question")
		{
			Init();
			//this._readOnly = false;  // tag:#IsThisCorrect
		}
		private void Init()
		{ }

		[XmlIgnore]
		[JsonIgnore]
		public ListFieldType? ListField_Item
		{
			get
			{
				if (Item?.GetType() == typeof(ListFieldType))
					return (ListFieldType)this.Item;
				else return null;
			}
			set
			{
				Item = ItemMutator(Item, value);
			}
		}


	[XmlIgnore]
		[JsonIgnore]
		public ResponseFieldType? ResponseField_Item
		{
			get
			{
				if (Item?.GetType() == typeof(ResponseFieldType))
					return (ResponseFieldType)this.Item;
				else return null;
			}
			set
			{
				Item = ItemMutator(Item, value);
			}
		}
	}
	#endregion

	#region QAS ListItems and Lookups


	public partial class ListType : IQuestionList
	{
		protected ListType() { Init(); }
		public ListType(BaseType parentNode) : base(parentNode, -1, "")
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "lst";  // tag:#IsThisCorrect
			ElementName = "List";
			Items = new();
		}

		/// <summary>
		/// Replaces Items; ListItem or DisplayedItem
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		internal List<DisplayedType> QuestionListMembers
		{
			get 
			{return this.Items; }
			set
			{
				Items = ItemsMutator(Items, value);
			}
		}
	}

	public partial class ListFieldType : IListField

	{// #NeedsTest
		protected ListFieldType() { Init(); }
		public ListFieldType(BaseType parentNode) : base(parentNode, -1, "ListField")
		{
			Init();
		}

		private void Init()
		{
			ElementPrefix = "lf";
			ElementName = "ListField";
			this._colTextDelimiter = "|";
			this._numCols = ((byte)(1));
			this._storedCol = ((byte)(1));
			this._minSelections = ((ushort)(1));
			this._maxSelections = ((ushort)(1));
			this._ordered = true;
		}

		[XmlIgnore]
		[JsonIgnore]
		public ListType? List //this is SDC.Schema.List, not a .NET List<> type
		{
			get
			{
				if (Item is ListType)
					return (ListType)this.Item;
				else return null;
			}
			set
			{
				Item = ItemMutator(Item, value);
			}
		}
		/// <summary>
		/// Replaces Item
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public LookupEndPointType? LookupEndpoint
		{
			get
			{
				if (Item is not null && Item.GetType() == typeof(LookupEndPointType))
					return (LookupEndPointType)this.Item;
				else return null;
			}
			set
			{
				Item = ItemMutator(Item, value);
			} //TODO: should this setter be internal scope to prevent changing/removing/nulling the node?
		}

	}

	public partial class ListItemType : IChildItemsParent //, IListItem //, IQuestionListMember
	{
		protected ListItemType() { Init(); }
		public ListItemType(ListType parentNode, string id = "", int position = -1) : base(parentNode, id, position)
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
			set
			{
				Item = ItemMutator(Item, value);
			}
		}

		#endregion


	}

	public partial class ListItemBaseType
	{
		protected ListItemBaseType() { Init(); }
		public ListItemBaseType(ListType parentNode, string id, int position) : base(parentNode, id, position)
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
		public LookupEndPointType(ListFieldType parentNode) : base(parentNode, -1, "LookupEndPoint")
		{
			Init();

		}
		private void Init()
		{
			this._includesHeaderRow = false;
			ElementPrefix = "lep";
			ElementName = "LookupEndPoint";
		}
	}

	#endregion

	#region Responses

	public partial class ListItemResponseFieldType
	{
		protected ListItemResponseFieldType() { Init(); }
		public ListItemResponseFieldType(ListItemBaseType parentNode) : base(parentNode, "ListItemResponseField")
		{
			Init();

		}
		private void Init()
		{
			this._responseRequired = false;
			ElementPrefix = "lirf";
			ElementName = "ListItemResponseField";
		}
	}

	public partial class ResponseFieldType
	{
		protected ResponseFieldType() { Init(); }
		public ResponseFieldType(IdentifiedExtensionType parentNode, string elementName = "ResponseField") : base(parentNode, -1, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementName = "ResponseField";
			ElementPrefix = "rf";
			//this.Item = null; // #NeedsTest
		}
	}

	public partial class UnitsType
	{
		protected UnitsType() { Init(); }
		/// <summary>
		/// Options: <br/>
		///Units
		///ResponseUnits 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="elementName">Options: <br/>
		///"Units"<br/>
		///"ResponseUnits"</param>
		public UnitsType(BaseType parentNode, string elementName) : base(parentNode, -1, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			_unitSystem = "UCUM";
			ElementPrefix = "un";
			//Units
			//ResponseUnits
		}
	}

	#endregion

	#endregion


	#endregion

	#region Base Types

	public partial class BaseType : IBaseType //IBaseType inherits IMoveRemove and INavigate
	{
		/// <summary>
		/// This constructor is used only to deserialize SDC classes with the SDC.Schema serializers.<br/>
		///		Parent Nodes cannot be assigned through this constructor. <br/>
		///		Node dictionaries cannot be populated here either.<br/>
		///		After the SDC object tree is created, parent nodes and other metadata can be assigned using:<br/>
		///		<see cref="InitBaseType"/> to refresh individual nodes, or<br/>
		///		<see cref="SdcUtil.ReflectRefreshTree"/> to reflect node metadata for the entire tree.
		/// </summary>
		protected BaseType()
		{			

			if (this is ITopNode tn)
			{
				this.ObjectID = 0;
				((_ITopNode)this)._MaxObjectID = 1;

				if (LastTopNode is null)
				{
					LastTopNode = tn; //Point to myself as the TopNode
					TopNode = tn;
					//ObjectID = ((_ITopNode)this)._MaxObjectID++;
				}
				else
				{
					TopNode = LastTopNode; //Point to LastTopNode as the TopNode
					LastTopNode = tn;
					//ObjectID = ((_ITopNode)this)._MaxObjectID++;
				}
			}//not ITopNode below here
			else if (LastTopNode is not null)
			{
				TopNode = LastTopNode;
				ObjectID = ((_ITopNode)TopNode)._MaxObjectID++;
			}
			else if (LastTopNode is null) 
			{//the caller is instantiating a new node that is not descended from an ITopNode node.
				//ObjectID will need to be incremented if & when the node is grafted onto another node that is ITopNode, or has an ITopNode ancestor
				ObjectID = -1;
			}
		}
		
		/// <summary>
		/// This parameterized constructor is NOT used to Deserialize SDC classes.
		/// TopNode is retrieved from the parent node, if it exists, and used to set the current TopNode.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position">Location of this new node in its parent node's list (if applicable)</param>
		/// <param name="elementName">For new SDC nodes that can take on more than one element name, the caller must supply the intended element name.<br/>
		/// These types include: <see cref="CallFuncType"/>, <see cref="FileType"/>, <see cref="EventType"/>, <see cref="gMonthDay_DEtype"/>, <see cref="anyURI_Stype"/>.<br/>
		/// The <paramref name="parentNode"/> types that require elementName are: 
		/// <see cref="DataTypesDateTime_DEType"/>, <see cref="DataTypesDateTime_SType"/>, <see cref="DataTypes_DEType"/>, 
		/// <see cref="DataTypes_SType"/>, <see cref="CallFuncBaseType"/>, <see cref="RegistrySummaryType"/>, <see cref="ActionsType"/></param>
		protected BaseType(BaseType? parentNode, int position = -1, string elementName = "")
		{
			if (parentNode is null && this is not ITopNode)
				throw new NullReferenceException($"{nameof(parentNode)} can only be null if this object implements ITopNode.");
			InitBaseType(parentNode);
			if (parentNode is not null)
			{
				bool result = SdcUtil.TryAttachNewNode(this, elementName
					, parentNode, out var piTarget, out var targetPropertyObject
					, out PropertyInfo? piChoiceEnum, out var choiceEnum, out string errorMsg
					, position, true, false);

				if(!result )
					throw new InvalidOperationException(
					$"This object type ({this.GetType().Name}) cannot be attached to the provided {nameof(parentNode)} type " +
					$"({parentNode?.GetType().Name}).\r\n" + errorMsg);

				if (targetPropertyObject is null)
					throw new InvalidOperationException(
					$"This object type ({this.GetType().Name}) cannot be attached to the provided {nameof(parentNode)} type ({parentNode?.GetType().Name}).");
			}
			InitAfterTreeAdd(parentNode); //Register this node, assign this.order, assign this.name
			return;
		}


		#region     Init 
		internal void InitBaseType (BaseType? parentNode)
		{
			//TopNode is retrieved from parentNode, if it exists, and used to set the current TopNode.
			//+Assign this.TopNode
			if (this is ITopNode tn)
			{
				this.ObjectID = 0;
				((_ITopNode)this)._MaxObjectID = 1;

				if (parentNode is null)this.TopNode = tn;
				else //if (parentNode is not null)
				{
					if (parentNode.TopNode is not null)
					{
						//parentNode's TopNode holds dictionaries we need to populate
						_ITopNode par_ITopNode;
						if (parentNode is _ITopNode ptn)
							par_ITopNode = ptn;//only occurs in RetrieveFormPackage under RetrieveFormPackage
						else par_ITopNode = (_ITopNode)parentNode.TopNode; //par_ITopNode could still be null here

					this.TopNode = par_ITopNode;
					}
					else { } //{ObjectID = -1; } //this node descends form a non-ITopNode root node; it cannot be added to ITopNode dictionaries without a TopNode
											//throw new InvalidOperationException("ParentNode is not null, but ParentNode.TopNode is null");
				}
			}//not ITopNode here
			else if (parentNode is not null)
			{
				if (parentNode is ITopNode ptn)
				{
					this.TopNode = (_ITopNode)parentNode;
					this.ObjectID = ((_ITopNode)TopNode)._MaxObjectID++;
				}
				else if (parentNode.TopNode is not null)
				{
					this.TopNode = (_ITopNode)parentNode.TopNode;
					this.ObjectID = ((_ITopNode)TopNode)._MaxObjectID++;
				}
				else
				{ //this node descends form an "illegal" non-ITopNode root node; it cannot be added to ITopNode dictionaries without a TopNode,
				  //but we can still process it, as long as we check for null TopNode everywhere we need it (mainly in Dictionaries).
				  //later, this "illegal" tree will need to be grafted onto a legal tree and TopNodes will need to be assigned during
				  //the grafting/moving process, based on the target tree's TopNodes.
					this.ObjectID = -1;
				}
			}
			else if (parentNode is null)
			{ this.ObjectID = -1; }//the caller is trying to instantiate a standalone root node that is not a proper ITopNode.
							  //TopNode is thus null here
							  //Object ID is unassigned

			//!________Assign default sGuid, BaseName, ObjectGUID, ObjectID, @order, @name & populate dictionaries___________________

			SdcUtil.AssignGuid_sGuid_BaseName(this);

			//if (this.TopNode is not null)
			//{	//a node with a null TopNode will not be registered in any TopNode dictionaries.
			//	this.RegisterNodeAndParent(parentNode); 

			//	//The following code requires that the current node is first added
			//	//to the ParentNodes dictionary.  Thus, these statements must come
			//	//*after* the dictionaries are populated (in RegisterNodeAndParent)

			//	this.AssignOrder(orderGap: 10);
			//	//SdcUtil.CreateCAPname(this,"",SdcUtil.NameChangeEnum.Normal); //This won't work until the node is fully initialized, after adding it to the SDC tree.
			//	this.AssignSimpleName(); //add options to keep original imported name, or to only create a new name when the original name is null.
			//}
		}

        /// <summary>
        /// //This code is intended to run in the BaseType parameterized constructor.
        /// </summary>
        /// <param name="parentNode"></param>
        internal void InitAfterTreeAdd(BaseType? parentNode)
		{
			if (this.TopNode is not null)
			{   //a node with a null TopNode will not be registered in any TopNode dictionaries.
				this.RegisterAll(parentNode);

				//The following code requires that the current node is first added
				//to the ParentNodes dictionary.  Thus, these statements must come
				//*after* the dictionaries are populated (in RegisterNodeAndParent)

				this.AssignOrder(orderGap: 10);
                //SdcUtil.CreateCAPname(this,"",SdcUtil.NameChangeEnum.Normal); //This won't work until the node is fully initialized (including BaseType.ID for IET nodes), after adding it to the SDC tree.
                
				
				//ElementPrefix is assigned later in the top-level constructor. It will be empty here, unless we make it a constant
                //Thus the simple name below will start with "_" instead of the ElementPrefix.
                this.AssignSimpleName(); //add options to keep original imported name, or to only create a new name when the original name is null.
			}
		}
		

		#endregion Init

		#region  Local Members
		//internal static int LastObjectID { get => lastObjectID; private set => lastObjectID = value; }
		internal void StoreError(string errorMsg) //TODO: Replace with event that logs each error
		{
			var exData = new Exception();
			exData.Data.Add("QuestionID: ", ParentIETnode?.ID.ToString() ?? "null");
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
					throw new Exception("Could not find SubIETcounter because TopNode?.Nodes is null");
				if (ParentNode is null)
					return 0;
					//throw new Exception("Could not find SubIETcounter because ParentNode is null");

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
					//this call may return null, unless this is ITopNode. Or an exception may be thrown in sdcUtil
					var meta = SdcUtil.GetElementPropertyInfoMeta(this, ParentNode);
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
			get => _elementPrefix;
			set => _elementPrefix = value;
		}

		/// <summary>
		/// Returns a calculated prefix for the first part of the <see cref="BaseType.name"/> attribute.<br/>
		/// This value may not match <see cref="BaseType.ElementPrefix"/>.
		/// </summary>
		/// <returns></returns>
		public string GetNamePrefix()
		{
			return SdcUtil.GetNamePrefix(this);
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
		public IdentifiedExtensionType? ParentIETnode
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
		public string ParentIETnodeID
		{ get => ParentIETnode?.ID; }

		[XmlIgnore]
		[JsonIgnore]
		/// <summary>
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
		public static void ResetLastTopNode()  //Rename to ResetStatic
		{
			LastTopNode = null;
			//LastObjectID = 0;
		}
		/// <summary>
		/// A method to update ITopNode dictionaries, if needed, when setting values the value of an SDC property.<br/>
		/// If an existing property (<b><paramref name="item"/></b>) is going to be replaced with a new object (<b><paramref name="valueNew"/></b>),<br/> 
		/// then <b><paramref name="item"/></b> will be removed from ITopNode dictionaries. <br/>
		/// In addition, if <b><paramref name="item"/></b> has descendant nodes, they will also be removed from the dictionaries.<br/>
		/// The method then simply returns the replacement <b><paramref name="valueNew"/></b> object, so that it may be used to assign to <b><paramref name="item"/></b> as its new object value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item">The source object to be repaced by <b><paramref name="valueNew"/></b>></param>
		/// <param name="valueNew">The incoming object to replace <b><paramref name="item"/></b></param>
		/// <param name="elementName"></param>
		protected internal T? ItemMutator<T>(T? item, T? valueNew, string elementName = "") where T : BaseType?
		{
			if (item == valueNew) 
				return valueNew;  //this will prevent running item.RemoveRecursive when we are reassigning the same object.

			if (item is not null)
				item.RemoveRecursive(false);

			if (valueNew is not null && valueNew.TopNode != this.TopNode)
			{
				//we have a node or subtree that is being grafted from a different SDC tree.
				//in most cases like this, we will want new sGuid, ObjectID, ObjectGUID, name, ID
				//Later, we can also reorder the entire tree.
				valueNew.Move(this, -1, true, SdcUtil.RefreshMode.UpdateNodeIdentity); //checks if  ParentNode is allowed
			}
			return valueNew;
		}
		/// <summary>
		/// A method to update ITopNode dictionaries, if needed, when setting values in SDC property lists.
		/// 
		/// </summary>
		/// <typeparam name="L">A generic List type for <b><paramref name="itemsListOld"/></b> and <b><paramref name="valueListNew"/></b></typeparam>
		/// <typeparam name="T">The type held by <paramref name="itemsListOld"/> and <paramref name="valueListNew"/></typeparam>
		/// <param name="itemsListOld">The current source List to be repaced by <paramref name="valueListNew"/>.  This List is often named "Items"</param>
		/// <param name="valueListNew">The incoming List to replace <paramref name="itemsListOld"/></param>
		protected List<T>? ItemsMutator<T> (List<T>? itemsListOld, List<T>? valueListNew)
			//where L : List<T>?  //the List is often null
			where T : BaseType  //we do not allow nulls in the list
		{
			if(itemsListOld == valueListNew)
				return valueListNew;  //this will prevent running RemoveRecursive when we are reassigning the same object.

			if (itemsListOld is not null  && itemsListOld.Count > 0)
				foreach (T n in itemsListOld) n.RemoveRecursive(false);

			if (valueListNew is not null)
			{
				if (valueListNew.Count > 0)
				{
					foreach (T n in valueListNew) n.Move(this); //We may wish to use RemoveRecursive instead
					return valueListNew;
				}
				throw new InvalidOperationException($"The supplied {nameof(valueListNew)} could not be used to set {nameof(itemsListOld)}.");
			}
			return null; //value will be allowed to have a null value until compiler null-checking is enabled globally, and we can reliably exclude all nulls from this method at compile time and runtime
		}


        string baseName = "";				


		/// <inheritdoc/>
		[XmlIgnore]
        [JsonIgnore]
        public string BaseName
        {
            get => baseName;
            set
            {
                if (baseName == value)
                    return;
                if (TopNode is not null) 
                {
                    if (!SdcUtil.IsValidVariableName(value))
                        throw new InvalidOperationException($"The name \"{value}\" is not a legal variable name.");

                    _ITopNode tn = (_ITopNode)this.TopNode;
                    if (!tn._UniqueBaseNames.Add(value))
                        throw new InvalidOperationException($"The name \"{value}\" already exists within the TopNode's tree.  A unique value is required.");
					tn._UniqueBaseNames.Remove(baseName); //remove the old name
                }
                baseName = value;
            }

        }
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
		private RetrieveFormPackageType? _PackageNode;
		private static ITopNode? LastTopNode;
		//private static BaseType? LastAddedNode;
		//private static int lastObjectID = 0;

		private List<Exception> ExceptionList = new();

		#endregion

	}

	public partial class ExtensionBaseType : IExtensionBase
	{
		protected ExtensionBaseType() { }
		public ExtensionBaseType(BaseType? parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{ }
	}

	#region IExtensionBaseTypeMember
	public partial class ExtensionType : IExtensionBaseTypeMember
	{
		private IExtensionBaseTypeMember Iebtm { get => (IExtensionBaseTypeMember)this; }
		protected ExtensionType() { Init(); }
		public ExtensionType(BaseType parentNode, int position = -1) : base(parentNode, position) { Init(); }
		private void Init()
		{	}

		#region IExtensionBaseTypeMember
		//public bool Remove() => Iebtm.Remove();
		//public bool Move(ExtensionBaseType ebtTarget, int newListIndex = -1) => Iebtm.MoveI(this, ebtTarget, newListIndex);
		#endregion

	}
	public partial class PropertyType : IExtensionBaseTypeMember, IHtmlHelpers
	{
		private IExtensionBaseTypeMember Iebtm { get => (IExtensionBaseTypeMember)this; }
		protected PropertyType() { Init(); }
		public PropertyType(ExtensionBaseType parentNode, int position = -1) : base(parentNode, position , "Property")
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
			this.TypedValue = new DataTypes_SType(this, "TypedValue");
			var rtf = new RichTextType(TypedValue, -1, "HTML");
			var h = (this as IHtmlHelpers).AddHTML(rtf);
			return h;
		}

	}
	public partial class CommentType : IExtensionBaseTypeMember
	{
		private IExtensionBaseTypeMember Iebtm { get => (IExtensionBaseTypeMember)this; }
		protected CommentType() { Init(); }
		public CommentType(BaseType parentNode, int position = -1) : base(parentNode, position)
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "cmt";
		}

	}
	#endregion

	public partial class IdentifiedExtensionType : IIdentifiedExtensionType
	{
		protected IdentifiedExtensionType() { Init(); }
		protected IdentifiedExtensionType(BaseType? parentNode, string id, int position, string elementName) : base(parentNode, position, elementName)
		{
			if (id.IsNullOrWhitespace())
				ID = $"___{BaseName}"; //BaseName was based on the sGuid and assigned in the BaseType ctor
			else ID = id;
			Init();
		}
		private void Init()
		{   //The ID may be assigned later by a deserializer after this runs, but that should be OK
			//if (string.IsNullOrWhiteSpace(ID))
				//this.ID = this.ObjectGUID.ToString();// #IsThisCorrect 
		}

		/// <summary>
		///  Hierarchical level using nested dot notation.<br/>
		///  Only includes <see cref="IdentifiedExtensionType"/> nodes in the hierarchy.
		/// </summary>
		/// <exception cref="NullReferenceException"/>
		[XmlIgnore]
		[JsonIgnore]
		public string DotLevelIET
		{
			get
			{
				if (TopNode is null) throw new NullReferenceException("To determine DotLevel, TopNode must not be null");
				//Walk up parent node tree and place each parent in a stack.
				//pop each node off the stack and determine its position (seq) in its parent object

				Stack<IdentifiedExtensionType>? s = new();
				IdentifiedExtensionType? parIET = ParentIETnode;

				if (this is IdentifiedExtensionType iet) s.Push(iet);
				while (parIET != null)
				{
					s.Push(parIET);
					parIET = parIET.ParentIETnode;
				}
				int level = 0;
				var sb = new StringBuilder("0");
				var topNode = (_ITopNode)TopNode;
				int seq;

				s.Pop();  //pop off the top node, which has no parent.
				while (s.Count > 0)
				{
					seq = -1;
					var n = s.Pop();
					parIET = n.ParentIETnode;
					if (parIET is not null) // parIET should never be null, since we popped the top IET node off of the stack
					{
						List<IdentifiedExtensionType>? lst;

						if (parIET is QuestionItemType q && q.ListField_Item is not null)
						{
							lst = q.GetListAndChildItemsList();
						}
						else//Otherwise, use n.ChildItemsNode.GetChildNodes() to find the index of n
						{
							if (parIET is IChildItemsParent cip)
								lst = cip.ChildItemsNode?.ChildItemsList;
							else //if(parIET is FormDesignType)
								lst = parIET.GetChildNodes()?.Where(n => n is IdentifiedExtensionType).Cast<IdentifiedExtensionType>().ToList();
							//else if (parIET is InjectFormType)
							//	lst = parIET.GetChildNodes()?.Where(n => n is IdentifiedExtensionType).Cast<IdentifiedExtensionType>().ToList();
							//else throw new InvalidOperationException
							//		("The IdentifiedExtensionType parent node contained neither a List object nor a ChildItems object");
						}
						if (lst is null) throw new InvalidOperationException
								("The IdentifiedExtensionType parent node did not contain a child node matching the current node");

						seq = lst.IndexOf(n);
						if (seq == -1) throw new InvalidOperationException
								("The IdentifiedExtensionType parent node did not contain a child node matching the current node");
						seq++;

						sb.Append('.').Append(seq); ;
						level++;
					}
				}
				return sb.ToString();
			}
		}
	}
	
	public partial class RepeatingType //this is an SDC abstract class
	{
		protected RepeatingType()
		{
			Init();
		}
		protected RepeatingType(BaseType parentNode, string id, int position, string elementName) : base(parentNode, id, position, elementName)
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
		public ChildItemsType(IChildItemsParent parentNode) : base((BaseType)parentNode, -1, "ChildItems")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ChildItems";
			ElementPrefix = "ch";
			Items = new();
		}

		[XmlIgnore]
		[JsonIgnore]
		public List<IdentifiedExtensionType> ChildItemsList
		{
			get { return this.Items; } 
			set
			{
				Items = ItemsMutator(Items, value);
			}
		}
	}

	#endregion

	#region DisplayedType and Members

	public partial class DisplayedType : IChildItemsMember<DisplayedType> //, IQuestionListMember
	{
		protected DisplayedType() { Init(); }
		public DisplayedType(BaseType parentNode, string id, int position = -1, string elementName = "DisplayedItem") : base(parentNode, id, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		public BlobType(DisplayedType parentNode, int position) : base(parentNode, position, "BlobContent")
		{
			Init();			
		}
		private void Init()
		{
			ElementPrefix = "blob";
			ElementName = "BlobContent";
		}
	}

	public partial class LinkType : IDisplayedTypeMember
	{
		protected LinkType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// ""<br/>
		/// ""<br/>
		/// </param>
		public LinkType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{			
			ElementPrefix = "link";
			//Link
			//DemogFormPkgLink
			//FormDesignPkgLink
			//MainFormPkgLink
			//InjectedFormPkgLink
			//PackageLink

		}
	}

	#region Coding
	public partial class CodingType : IDisplayedTypeMember
	{
		protected CodingType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "ResponseValue" (uses position)<br/>
		/// "CodedValue" (uses position)<br/>
		/// "Code"<br/>
		/// "MappedCode"<br/>
		/// "Included" (uses position)<br/>
		/// "Excluded" (uses position)<br/>
		/// </param>
		public CodingType(ExtensionBaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{			
			ElementPrefix = "code";
		}
	}

	public partial class CodeMatchType
	{
		protected CodeMatchType() { Init(); }
		public CodeMatchType(CodingType parentNode) : base(parentNode, -1, "CodeMatch")
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="elementName">Choices:<br/>
		/// "CodeSystem"<br/>
		/// "DefaultCodeSystem"<br/>
		/// </param>
		public CodeSystemType(ExtensionBaseType parentNode, string elementName) : base(parentNode, -1, elementName)
		{
			Init();
			if(!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{			
			ElementPrefix = "cdsys";
		}
	}

	#endregion

	#endregion


	#endregion

	#region DataTypes
	public partial class DataTypes_DEType //This is the Response element
	{
		protected DataTypes_DEType() { Init(); }
		public DataTypes_DEType(ResponseFieldType parentNode) : base(parentNode, -1, "Response")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Response";
			ElementPrefix = "rsp";          
		}

		/// <summary>
		/// any *_DEType data type
		/// </summary>
		[XmlIgnore]
		public BaseType DataTypeDE_Item
		{
			get { return this.Item; }
			set
			{
				Item = ItemMutator(Item, value);
			}
		}
	}

	public partial class anyType_DEtype: IDataType_DEType
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="elementName">Choices:<br/>
		/// "TypedValue"<br/>
		/// "ChangedTo"<br/>
		/// "ChangedFrom"<br/>
		/// "NewValue"<br/>
		/// "extended by: ParameterValueType"<br/>
		/// ""<br/>
		/// </param>
		public DataTypes_SType(BaseType parentNode, string elementName) : base(parentNode, -1, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "dts";
		}

		/// <summary>
		/// any *_SType data type
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public BaseType DataTypeS_Item
		{
			get { return this.Item; }
			set
			{
				Item = ItemMutator(Item, value);
			}
		}
	}

	public partial class anyURI_DEtype : IDataType_DEType
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
		public anyURI_Stype(BaseType parentNode, int position = -1, string elementName = "") : base(parentNode, position, elementName)

		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "uri";
			//FunctionURI
			//LocalFunctionName
			//TargetItemID
			//BlobURI
			//CodeURI
			//CodeSystmURI
			//WebURL
			//FileURI
			//LinkURI
			//Endpoijt
			//ReplacedID
			//anyURI
			//extended by: TargetItemIDType, anyURI_DEtype

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

	public partial class base64Binary_DEtype : IDataType_DEType
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

	public partial class boolean_DEtype : IDataType_DEType
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

	public partial class byte_DEtype : IDataType_DEType
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

	public partial class byte_Stype : IVal, IValNumeric, IInteger
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

	public partial class date_DEtype : IDataType_DEType
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

	public partial class dateTime_DEtype : IDataType_DEType
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

	public partial class dateTimeStamp_DEtype : IDataType_DEType
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

	public partial class dayTimeDuration_DEtype : IDataType_DEType
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

	public partial class decimal_DEtype : IDataType_DEType
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

	public partial class decimal_Stype : IVal, IValNumeric, IFraction
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

	public partial class double_DEtype : IDataType_DEType
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

	public partial class double_Stype : IVal, IValNumeric, IFraction
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

	public partial class duration_DEtype : IDataType_DEType
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

	public partial class float_DEtype : IDataType_DEType
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

	public partial class float_Stype : IVal, IValNumeric, IFraction
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

	public partial class gDay_DEtype : IDataType_DEType
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

	public partial class gMonth_DEtype : IDataType_DEType
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

	public partial class gMonthDay_DEtype : IDataType_DEType
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

	public partial class gYear_DEtype : IDataType_DEType
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

	public partial class gYearMonth_DEtype : IDataType_DEType
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

	public partial class hexBinary_DEtype : IDataType_DEType
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

	public partial class HTML_DEtype : IDataType_DEType
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


	public partial class int_DEtype : IDataType_DEType
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

	public partial class int_Stype : IVal, IValNumeric, IInteger
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

	public partial class integer_DEtype : IDataType_DEType
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

	public partial class integer_Stype : IVal, IValNumeric, IInteger
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

	public partial class long_DEtype : IDataType_DEType
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

	public partial class long_Stype : IVal, IValNumeric, IInteger
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

	public partial class negativeInteger_DEtype : IDataType_DEType
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

	public partial class negativeInteger_Stype : IVal, IValNumeric, IInteger
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

	public partial class nonNegativeInteger_DEtype : IDataType_DEType
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

	public partial class nonNegativeInteger_Stype : IVal, IValNumeric, IInteger
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

	public partial class nonPositiveInteger_DEtype : IDataType_DEType
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

	public partial class nonPositiveInteger_Stype : IVal, IValNumeric, IInteger
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

	public partial class positiveInteger_DEtype : IDataType_DEType
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

	public partial class positiveInteger_Stype : IVal, IValNumeric, IInteger
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

	public partial class short_DEtype : IDataType_DEType
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

	public partial class short_Stype : IVal, IValNumeric, IFraction, IInteger
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

	public partial class string_DEtype : IDataType_DEType
	{
		protected string_DEtype() { Init(); }
		public string_DEtype(BaseType parentNode) : base(parentNode)
		{
			Init();
		} 
		private void Init()
		{ }
	}

	public partial class string_Stype
	{
		protected string_Stype() { Init(); }
		public string_Stype(BaseType parentNode, int position = -1, string elementName = "") : base(parentNode, position, elementName)

		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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

	public partial class time_DEtype : IDataType_DEType
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

	public partial class unsignedByte_DEtype : IDataType_DEType
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

	public partial class unsignedByte_Stype : IVal, IValNumeric, IInteger
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

	public partial class unsignedInt_DEtype : IDataType_DEType
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

	public partial class unsignedInt_Stype : IVal, IValNumeric, IInteger
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

	public partial class unsignedLong_DEtype : IDataType_DEType
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

	public partial class unsignedLong_Stype : IVal, IValNumeric, IInteger
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

	public partial class unsignedShort_DEtype : IDataType_DEType
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

	public partial class unsignedShort_Stype : IVal, IValNumeric, IInteger
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

	public partial class XML_DEtype : IDataType_DEType
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

	public partial class yearMonthDuration_DEtype : IDataType_DEType
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
		public ItemNameType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "itnm";
		}
	}
	public partial class ItemNameAttributeType
	{
		protected ItemNameAttributeType() { Init(); }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position">not used (enter -1)</param>
		/// <param name="elementName">Choices:<br/>
		/// "MatchSource"<br/>
		/// "Target"<br/>
		/// </param>
		public ItemNameAttributeType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
		}
		private void Init()
		{
			this._attributeName = "val";
		}
	}
	public partial class NameType //OK
	{
		protected NameType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "AliasName"  (uses position)<br/>
		/// "PersonName"<br/>
		/// </param>
		public NameType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "nm";
		}
	}
	public partial class TargetItemIDType //OK
	{
		protected TargetItemIDType() { Init(); }
		public TargetItemIDType(BaseType parentNode) : base(parentNode, -1, "TargetItemID")
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "tiid";
			ElementName = "TargetItemID";
		}
	}
	public partial class TargetItemNameType //OK
	{
		protected TargetItemNameType() { Init(); }
		public TargetItemNameType(BaseType parentNode) : base(parentNode, -1, "TargetItemName")
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "tinm";
			ElementName = "TargetItemName";
		}
	}
	public partial class TargetItemXPathType //OK
	{
		protected TargetItemXPathType() { Init(); }
		public TargetItemXPathType(BaseType parentNode) : base(parentNode, -1, "TargetItemXPath") //OK
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "tixp";
			ElementName = "TargetItemXPath";
		}
	}
	public partial class ListItemParameterType //OK
	{
		protected ListItemParameterType() { Init(); }
		public ListItemParameterType(BaseType parentNode, int position = -1) : base(parentNode, position, "ListItemParameterRef") //OK
		{
			Init();
			this._listItemAttribute = "associatedValue";
		}
		private void Init()
		{
			this._dataType = "string";
			ElementPrefix = "liParam";
			ElementName = "ListItemParameterRef";
		}
	}
	public partial class ParameterItemType //OK
	{
		protected ParameterItemType() { Init(); }
		public ParameterItemType(BaseType parentNode, int position = -1) : base(parentNode, position, "ParameterRef") //OK
		{
			Init();			
		}
		private void Init()
		{
			this._dataType = "string";
			this._sourceItemAttribute = "val";
			ElementPrefix = "paramItem";
			ElementName = "ParameterRef";
		}
	}
	public partial class ParameterValueType //OK
	{
		protected ParameterValueType() { Init(); }
		public ParameterValueType(BaseType parentNode) : base(parentNode, "ParameterValue") //OK
		{
			Init();			
		}
		private void Init()
		{
			ElementPrefix = "paramVal";
			ElementName = "ParameterValue";
		}
	}
	public partial class PredAlternativesType //OK
	{
		public PredAlternativesType() { Init(); }
		public PredAlternativesType(BaseType parentNode, int position = -1) : base(parentNode, position, "ItemAlternatives")
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			this._minAnswered = 1;
			this._maxAnswered = 0;
			ElementPrefix = "predAlt";
			ElementName = "ItemAlternatives";
		}
	}
	public partial class PredEvalAttribValuesType //OK
	{
		protected PredEvalAttribValuesType() { Init(); }
		public PredEvalAttribValuesType(PredGuardType parentNode, int position = -1) : base(parentNode, position, "AttributeEval")

		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			this._boolOp = PredEvalAttribValuesTypeBoolOp.AND;
			ElementPrefix = "predEvAttVal";
			ElementName = "AttributeEval";
		}
	}
	public partial class PredGuardTypeSelectionSets //OK
	{
		protected PredGuardTypeSelectionSets() { Init(); }
		public PredGuardTypeSelectionSets(PredGuardType parentNode, int position = -1) : base(parentNode, position, "SelectionSets")
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementName = "SelectionSets";
			ElementPrefix = "selset";
		}
	}
	public partial class PredSingleSelectionSetsType
	{
		protected PredSingleSelectionSetsType() { Init(); }
		public PredSingleSelectionSetsType(BaseType parentNode, int position = -1, string elementName = "IllegalCoSelectedListItems") : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			this._maxSelections = ((short)(1));
			ElementPrefix = "predSngSelSet";
			//IllegalCoSelectedListItems
			//Extended/inherited by SelectionSets
		}
	}
	public partial class RuleAutoActivateType //OK
	{
		protected RuleAutoActivateType() { Init(); }
		public RuleAutoActivateType(BaseType parentNode, int position = -1) : base(parentNode, position, "AutoActivation")
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
			ElementName = "AutoActivation";
			//this._x_removeResponsesWhenDeactivated = false;
		}
	}
	public partial class RuleAutoSelectType //OK
	{
		protected RuleAutoSelectType() { Init(); }
		public RuleAutoSelectType(BaseType parentNode, int position = -1) : base(parentNode, position, "AutoSelection")
		{
			Init();
		}
		private void Init()
		{
			this._onlyIf = false;
			ElementPrefix = "ras";
			ElementName = "AutoSelection";
		}
	}
	public partial class RuleListItemMatchTargetsType //OK
	{
		protected RuleListItemMatchTargetsType() { Init(); }
		public RuleListItemMatchTargetsType(BaseType parentNode) : base(parentNode, -1, "ListItemMatchTargets")
		{
			Init();
		}
		private void Init()
		{
			this._attributeToMatch = RuleListItemMatchTargetsTypeAttributeToMatch.associatedValue;
			ElementPrefix = "rlimt";
			ElementName = "ListItemMatchTargets";
		}
	}	
	public partial class ValidationTypeSelectionSets //OK
	{
		protected ValidationTypeSelectionSets() { Init(); }
		public ValidationTypeSelectionSets(BaseType parentNode, int position = -1) : base(parentNode, position, "SelectionSets")

		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementPrefix = "vtss";
			ElementName = "SelectionSets";
		}
	}
	public partial class ValidationType //OK
	{
		protected ValidationType() { Init(); }
		public ValidationType(RulesType parentNode, int position = -1) : base(parentNode, position, "Validation")

		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "valTyp";
			ElementName = "Validation";
		}
	}
	public partial class ValidationTypeSelectionTest //OK
	{
		protected ValidationTypeSelectionTest() { Init(); }
		public ValidationTypeSelectionTest(BaseType parentNode, int position = -1) : base(parentNode, position, "SelectionTest")
		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementPrefix = "valTypSel";
			ElementName = "SelectionTest";
		}
	}
	public partial class PredSelectionTestType
	{
		protected PredSelectionTestType() { Init(); }
		public PredSelectionTestType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{ 
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private  void Init()
		{
			ElementPrefix = "predSelTst";
			//SelectionTest
			//IllegalListItemPairings
		}
	}
	public partial class CallFuncType
	{
		protected CallFuncType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "CallSetValue"<br/>
		/// "WebService" (uses position)<br/>
		/// "ValidationWebService"<br/>
		/// "ShowURL" (uses position)<br/>
		/// "CallFunction (uses position)"<br/>
		/// "ExternalRule (uses position)" (uses position)<br/>
		/// </param>
		public CallFuncType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		/// <summary>
		/// This is an abstract Schema type
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "CallFuncType"<br/>
		/// "CallFuncBoolType"<br/>	
		/// </param>
		public CallFuncBaseType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			this._returnList = false;
			this._listDelimiter = "|";
			this._allowNull = true;
			//ElementPrefix = "callFuncBase";
		}
	}
	partial class CallFuncBoolType
	{
		protected CallFuncBoolType() { Init(); }
		public CallFuncBoolType(BaseType parentNode, int position = -1) : base(parentNode, position, "CallBoolFunc")

		{
			Init();
		}
		private void Init()
		{
			this._not = false;
			ElementPrefix = "callBoolF";
			ElementName = "CallBoolFunc";
		}
	}


	#endregion
	#region PredActions

	public partial class PredMultiSelectionSetBoolType
	{
		protected PredMultiSelectionSetBoolType() { Init(); }
		public PredMultiSelectionSetBoolType(BaseType parentNode, int position) : base(parentNode, position, "MultiSelections")
		{ 
			Init();
		}
		private void Init()
		{
			ElementPrefix = "pmssb";
			ElementName = "MultiSelections";
		}
	}


	#endregion
	#region  Actions

	public partial class ActionsType : IActions
	{
		protected ActionsType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		public ActionsType(ExtensionBaseType parentNode) : base(parentNode, -1, "Actions")
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
		public ActActionType(ActionsType parentNode, int position = -1) : base(parentNode, position, "Action")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Action";
			ElementPrefix = "actAct";
			Items = new();
		}
		[XmlIgnore]
		public List<ExtensionBaseType> ActAction_Items
		{
			get
			{ return Items; } 
			set
			{
				Items = ItemsMutator(Items, value);
			}
		}
	}
	public partial class RuleSelectMatchingListItemsType
	{
		protected RuleSelectMatchingListItemsType() { Init(); }
		public RuleSelectMatchingListItemsType(ActionsType parentNode, int position = -1) : base(parentNode, position, "SelectMatchingListItems")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SelectMatchingListItems";
			ElementPrefix = "selMLI";
		}
	}
	public partial class ActAddCodeType
	{
		protected ActAddCodeType() { Init(); }
		public ActAddCodeType(ActionsType parentNode, int position = -1) : base(parentNode, position, "AddCode")
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
		public ActInjectType(ActionsType parentNode, string id = "", int position = -1) : base(parentNode, id, position)
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
		public ActSaveResponsesType(ActionsType parentNode, int position = -1) : base(parentNode, position, "Save")
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
		public ActSendReportType(ActionsType parentNode, int position = -1) : base(parentNode, position, "SendReport")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "SendReport";
			ElementPrefix = "actSndRep";
			Items = new();
		}

		internal List<ExtensionBaseType> Email_Phone_WebSvc_List
	{
		get { return this.Items; }
		set
		{
			Items = ItemsMutator(Items, value);
		}
	}
	}
	public partial class ActSendMessageType
	{
		protected ActSendMessageType() { Init(); }
		/// <summary>
		/// "SendMessage111" in Schema
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		public ActSendMessageType(ActionsType parentNode, int position = -1) : base(parentNode, position, "SendMessage")
		{
			Init();
		} 
		private void Init()
		{
			ElementName = "SendMessage";  //"SendMessage111"  this type may be removed
			ElementPrefix = "actSndMsg";
			Items = new();
		}

		/// <summary>
		/// List&lt;BaseType> accepts: EmailAddressType, PhoneNumberType, WebServiceType
		/// </summary>
		internal List<ExtensionBaseType> Email_Phone_WebSvc_List
		{
			get { return this.Items; }

			set
			{
				Items = ItemsMutator(Items, value);
			}
		}
	}
	public partial class ActSetAttributeType
	{
		protected ActSetAttributeType() { Init(); }
		public ActSetAttributeType(ActionsType parentNode, int position = -1) : base(parentNode, position, "SetAttributeValue")
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
		public ActSetAttrValueScriptType(ActionsType parentNode, int position = -1) : base(parentNode, position, "SetAttributeValueScript")
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
		public ActSetBoolAttributeValueCodeType(ActionsType parentNode, int position = -1) : base(parentNode, position)
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
		/// <summary>
		/// Note: subclassed by ActSetBoolAttributeValueCodeType (SetBoolAttributeValueCode).  That subclass can overwrite the ctor's hard-coded ElementName.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName"></param>
		public ScriptCodeBoolType(ActionsType parentNode, int position = -1, string elementName = "ScriptBoolFunc") : base(parentNode, position, elementName)
		{
			Init();
		}
		private void Init()
		{
			ElementName = "ScriptBoolFunc";
			ElementPrefix = "scbt";
			this._not = false;
		}
	}
	public partial class ActShowFormType
	{
		protected ActShowFormType() { Init(); }
		public ActShowFormType(ActionsType parentNode, int position = -1) : base(parentNode, position, "ShowForm")
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
		public ActShowMessageType(ActionsType parentNode, int position = -1) : base(parentNode, position, "ShowMessage")
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

		public ActShowReportType(ActionsType parentNode, int position = -1) : base(parentNode, position, "ShowReport")
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
		public ActPreviewReportType(ActionsType parentNode, int position = -1) : base(parentNode, position, "PreviewReport")
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
		public ActValidateFormType(ActionsType parentNode, int position = -1) : base(parentNode, position, "ValidateForm")
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

	public partial class ScriptCodeAnyType
	{
		protected ScriptCodeAnyType()
		{ Init(); }
		/// <summary>
		/// 
		/// Note: ScriptCodeAnyType is a base type for ActSetAttrValueScriptType.
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "SetValue"<br/>
		/// "RunCode" (uses position)<br/>
		/// "ScriptedRule" (uses position)<br/>
		/// </param>
		public ScriptCodeAnyType(ActionsType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{ 
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "sca";
			this._dataType = "string";
		}
	}
	public partial class ScriptCodeBaseType
	{
		protected ScriptCodeBaseType() { Init(); }
		/// <summary>
		/// Abtract class underlying ScriptCodeAnyType and ScriptCodeBoolType
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName"></param>
		public ScriptCodeBaseType(ActionsType parentNode, int position, string elementName) : base(parentNode, position, elementName)
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

	#endregion
	#region Events
	public partial class OnEventType : IDisplayedTypeMember
	{
		protected OnEventType() { Init(); }
		public OnEventType(ExtensionBaseType parentNode, int position = -1, string elementName = "") : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "onEv";
		}
	}

	public partial class RulesType
	{
		protected RulesType() { Init(); }
		public RulesType(BaseType parentNode) : base(parentNode, -1, "Rules")
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "rules";
			ElementName = "Rules";
		}
	}

	public partial class EventType : IDisplayedTypeMember
	{
		protected EventType() { Init(); }
		public EventType(ExtensionBaseType parentNode, int position = -1, string elementName = "") : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "evnt";
		}
	}

	public partial class PredGuardType : IDisplayedTypeMember
	{

		protected PredGuardType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "SelectIf"<br/>
		/// "DeselectIf"<br/>
		/// "ActivateIf"<br/>
		/// "DeActivateIf"<br/>
		/// "Group (uses position)"<br/>
		/// </param>
		public PredGuardType(BaseType parentNode, int position = -1, string elementName = "") : base(parentNode, position, elementName)

		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "ConditionalGroupAction"<br/>
		/// "ConditionalActions"<br/>
		/// "Else"<br/>
		/// "subclassed by EventType"<br/>
		/// </param>
		public PredActionType(ExtensionBaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		public FuncBoolBaseType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "Contact" (uses position)<br/>
		/// "ApprovalContact" (uses position)<br/>
		/// "Signer"<br/>
		/// </param>
		public ContactType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "cntct";
		}
	}

	public partial class OrganizationType
	{
		protected OrganizationType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "Organization"<br/>
		/// "ComplianceOrganization" (uses position)<br/>
		/// </param>
		public OrganizationType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			this.ElementPrefix = "org";
		}
	}

	public partial class PersonType
	{
		protected PersonType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "Person"<br/>
		/// "ContactPerson" (uses position)<br/>
		/// </param>
		public PersonType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			this.ElementPrefix = "pers";
		}
	}

	public partial class AddressType
	{
		protected AddressType() { Init(); }
		public AddressType(BaseType parentNode, int position = -1) : base(parentNode, position, "StreetAddress")
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "adrs";
			ElementName = "StreetAddress";
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "Security"<br/>
		/// "ListHeaderText"<br/>
		/// "TextAfterResponse"<br/>
		/// "Message" (uses position)<br/>
		/// "MessageText"<br/>
		/// "TargetDisplayText"<br/>
		/// "MapComment" (uses position)<br/>
		/// "Description" (uses position)<br/>
		/// "CodeText"<br/>
		/// "VersionComments"<br/>
		/// "LinkText"<br/>
		/// "PackageDescription" (uses position)<br/>
		/// "RegisteredItemDescription" (uses position)<br/>
		/// </param>
		public RichTextType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "ComplianceRule (uses position)"<br/>
		/// "DefaultComplianceRule (uses position)"<br/>
		/// </param>
		public ComplianceRuleType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
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
		public SubmissionRuleType(BaseType parentNode, int position = -1) : base(parentNode, position, "SubmissionRule")
		{
			Init();
		}
		private void Init()
		{
			this.ElementPrefix = "sr";
			ElementName = "SubmissionRule";
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
		public IdentifierType(BaseType parentNode, int position = -1) : base(parentNode, position, "Identifier")
		{
			Init();
		}
		private void Init()
		{
			this.ElementPrefix = "id";
			ElementName = "Identifier";
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
		public LanguageType(BaseType parentNode, int position = -1) : base(parentNode, -1, "Language")
		{
			Init();
		}
		private void Init()
		{
			this.ElementPrefix = "lng";
			ElementName = "Language";
		}
	}

	public partial class ProvenanceType
	{
		protected ProvenanceType() { Init(); }
		public ProvenanceType(BaseType parentNode) : base(parentNode, -1, "Provenance")
		{
			Init();
		}
		private void Init()
		{
			this.ElementPrefix = "prv";
			ElementName = "Provenance";
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
		public VersionType(BaseType parentNode) : base(parentNode, -1, "Version")
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "ver";
			ElementName = "Version";
		}
	}

	public partial class VersionTypeChanges
	{
		protected VersionTypeChanges() { Init(); }
		public VersionTypeChanges(BaseType parentNode) : base(parentNode, -1, "Changes")
		{
			Init();

		}
		private void Init()
		{
			this.ElementPrefix = "vch";
			ElementName = "Changes";
		}
	}


	#endregion

	#region Contacts classes

	public partial class ContactsType
	{
		protected ContactsType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="elementName">Options: <br/>
		/// "Contacts"<br/>
		/// "Editors"
		/// </param>
		public ContactsType(BaseType parentNode, string elementName) : base(parentNode, -1, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
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
		public DestinationType(BaseType parentNode, int position = -1) : base(parentNode, position, "Destination")
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "dest";
			ElementName = "Destination";
		}
	}


	public partial class PhoneNumberType
	{
		protected PhoneNumberType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Choices:<br/>
		/// "Fax"<br/>
		/// "PhoneNumber"<br/>
		/// </param>
		public PhoneNumberType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
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
		public PhoneType(BaseType parentNode, int position = -1) : base(parentNode, position, "Phone")
		{
			Init();
		}
		private void Init()
		{
			ElementName = "Phone";
			ElementPrefix = "pht";
		}
	}

	public partial class JobType
	{
		protected JobType() { Init(); }
		public JobType(BaseType parentNode) : base(parentNode, -1, "Job")
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "job";
			ElementName = "Job";
		}
	}
	#endregion

	#region  Email
	public partial class EmailAddressType
	{
		protected EmailAddressType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Options: <br/>
		/// "Email"<br/>
		/// "EmailAddress"<br/>
		/// </param>
		public EmailAddressType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "emd";
		}
	}

	public partial class EmailType
	{
		protected EmailType() { Init(); }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		public EmailType(BaseType parentNode, int position = -1) : base(parentNode, position, "Email")
		{
			Init();

			//this.Usage = new string_Stype();
			//this.EmailClass = new string_Stype();
			//this.EmailAddress = new EmailAddressType();
		}
		private void Init()
		{
			ElementPrefix = "em";
			ElementName = "Email";
		}
	}

	#endregion

	#region Files


	public partial class ApprovalType
	{
		protected ApprovalType() { Init(); }
		public ApprovalType(BaseType parentNode, int position = -1) : base(parentNode, position, "Approval")
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "app";
			ElementName = "Approval";
		}
	}

	public partial class AssociatedFilesType
	{
		protected AssociatedFilesType() { Init(); }
		public AssociatedFilesType(BaseType parentNode) : base(parentNode, -1, "AssociatedFiles")
		{
			Init();
		}
		private void Init()
		{
			ElementPrefix = "asf";
			ElementName = "AssociatedFiles";
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
		public FileDatesType(BaseType parentNode) : base(parentNode, -1, "Dates")
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "fld";
			ElementName = "Dates";
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="position"></param>
		/// <param name="elementName">Options: <br/>
		/// "VersioningReference"<br/>
		/// "File" (uses position)<br/>
		/// "ReplacedFile" (uses position)<br/>
		/// "TemplateFile"<br/>
		/// "ServiceLevelAgreement"<br/>
		/// "RegistryPurpose"<br/>
		/// "ReferenceDocument (uses position)"<br/> 
		/// </param>
		public FileType(BaseType parentNode, int position, string elementName) : base(parentNode, position, elementName)
		{
			Init();
			if (!elementName.IsNullOrWhitespace()) ElementName = elementName;
		}
		private void Init()
		{
			ElementPrefix = "fil";
		}
	}
	public partial class InterfaceType
	{
		protected InterfaceType() { Init(); }
		public InterfaceType(BaseType parentNode) : base(parentNode, -1, "RegistryInterface")

		{
			Init();
		}
		private void Init()
		{
			ElementName = "RegistryInterface";
			ElementPrefix = "intrf";
		}
	}

	public partial class FileUsageType
	{
		protected FileUsageType() { Init(); }
		public FileUsageType(BaseType parentNode, int position = -1) : base(parentNode, position, "Usage")
		{
			Init();

		}
		private void Init()
		{
			ElementPrefix = "flu";
			ElementName = "Usage";
		}
	}

	#endregion

	#endregion

	#region Registry Summary Types

	#endregion

}

