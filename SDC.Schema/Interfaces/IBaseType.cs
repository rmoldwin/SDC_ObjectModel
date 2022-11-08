using Newtonsoft.Json;
using System.Xml.Serialization;
using System;
namespace SDC.Schema

{
    public interface IBaseType : IMoveRemove, INavigate
    {
        //TODO: implement AutoNameFlag
		/// <summary>
		/// Boolean flag to determine if @name values should be automatically generated for each SDC element node<br/>
        /// Not currently implemented.
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
        public bool AutoNameFlag { get; set; }


        ///// <summary>
        ///// Field to hold the ordinal position of an object (XML element) under an IdentifiedExtensionType (IET)-derived object.
        ///// This number is used for creating the name attribute suffix.
        ///// </summary>
        //[System.Xml.Serialization.XmlIgnore]
        //[JsonIgnore]
        //public int SubIETcounter { get; }


        /// <summary>
        /// The name of XML element that is output from this class instance.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public string ElementName { get; }

        /// <summary>
        /// The prefix used to autogenerate the value of the @name attribute, if AutoNameFlag = true.  <br/>
        /// A default prefix is assigned for each SDC element, based on its SDC type.<br/>
        /// THat prefix may be cusomized here.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public string ElementPrefix { get; set; }
        /// <summary>
        /// A monotonically increasing integer value, added to each SDC node as it is constructed in an SDC tree.<br/>
        /// The numeric value does not mecessarily specify the order of nodes in the SDC tree, <br/>
        /// since nodes may be added between other nodes or moved after the initial tree is created.<br/>
        /// The ObjectID is created anew each time an SDC tree is deserialized, and therefore should not <br/>
        /// be treated as a stable identifier that maintains its value after a serialization/deserialization cycle.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public int ObjectID { get; }
		/// <summary>
		/// A unique identifier added to each SDC node as it is constructed in teh object tree. <br/>
        /// If sGuid (a short GUID) is used, it is derived automatically from the ObjectGUID
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
        public Guid ObjectGUID { get; }
		//TODO: NodeType not currently implemented	
		/// <summary>
		/// The SDC Type of the current node.
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
        public ItemTypeEnum NodeType { get; }
        //[XmlIgnore]
        //[JsonIgnore]
        //public Boolean IsLeafNode { get; }


        /// <summary>
        /// Returns the ID of the parent object (representing the parent XML element)
        /// This is the ObjectID, which is a sequentially assigned integer value.
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

        ///// <summary>
        ///// Returns the ID property of the closest ancestor of type DisplayedType.  
        ///// For eCC, this is the Parent node's ID, which is derived from  the parent node's CTI_Ckey, a.k.a. ParentItemCkey.
        ///// </summary>
        //[System.Xml.Serialization.XmlIgnore]
        //[JsonIgnore]
        //public IdentifiedExtensionType ParentIETypeNode { get; }

        /// <summary>
        /// Retrieve the BaseType object that is the immediate parent of the current object in the object tree
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public BaseType? ParentNode { get; }

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
        public IdentifiedExtensionType? ParentIETypeNode { get; }
        /// <summary>
        /// Returns the ID property of the closest ancestor of type IdentifiedExtensionType.  
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public string? ParentIETypeID { get; }

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
		public ITopNode? TopNode { get; }

		/// <summary>
		/// 
		/// </summary>
		[XmlIgnore]
        [JsonIgnore]
        public RetrieveFormPackageType PackageNode { get; }

		//public abstract void SetNames(string elementName = "", string elementPrefix = "", string baseName = "");
		
        /// <summary>
		/// Used as an optional component for generating the @name SDC BaseType attribute. <br/>
		/// By default, this name is generated from the sGuid in the constructor, but may be changed as needed.
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public string BaseName { get; set; }


		//!+Added to support TE Blazor module.
		/// <summary>
		/// Used to indicate if an item is new, has been moved or updated.<br/>
		/// Loaded => 1 (default); <br/>
		/// New => 2; <br/>
		/// MovedUp => 3; <br/>
		/// MovedDown => 4; <br/>
		/// Updated => 5;
		/// </summary>
		[XmlIgnore]
		[JsonIgnore]
		public int ItemViewState { get; set; }
		/// <summary>
		/// Reset TopNodeTemp to null, so that nodes newly added to a top node<br/>
		/// use the correct node for the top of the object tree
		/// </summary>
		//public abstract static void ResetTempTopNode();

	}
}
