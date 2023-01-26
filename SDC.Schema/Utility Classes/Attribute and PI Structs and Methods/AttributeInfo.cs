using CSharpVitamins;
using SDC.Schema;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
	/// <summary>
	/// Immutable record struct to hold information about SDC object properties that will become XML attributes when serialized to XML.
	/// </summary>
	public readonly record struct AttributeInfo
	{
		/// <summary>
		/// Holds information for one XML attribute of the supplied SDC node parameter/>
		/// </summary>
		/// <param name="node">The SDC node that is the subject of this record.</param>
		/// <param name="attributeValue">The value of this attribute instance.</param>
		/// <param name="attributePropInfo">The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element.</param>
		/// <param name="name">The name of the property, and the text of the attribute as it will appear in XML.</param>
		public AttributeInfo(BaseType node,
			object? attributeValue,
			PropertyInfo? attributePropInfo,
			int order,
			string? name = null)
		{
			this.sGuid = node.sGuid;
			BaseType? par = node.ParentNode;			
			this.ParentNodesGuid = par?.sGuid;
			this.ParentNodeObjectID = par?.ObjectID;

			var parentIETNode = node.ParentIETnode;
			this.ParentIETNodesGuid = parentIETNode?.sGuid;			
			this.ParentIETNodeObjectID = parentIETNode?.ObjectID;

			this.Value = attributeValue;
			this.ValueString = attributeValue?.ToString();

			if (attributePropInfo is not null)
			{
				this.DefaultValue = SdcUtil.GetAttributeDefaultValue(attributePropInfo);
				this.DefaultValueString = DefaultValue?.ToString();
				this.AttributePropInfo = attributePropInfo;
				this.Name = name ?? AttributePropInfo.Name;
			} else
			{
				this.DefaultValue = null;
				this.DefaultValueString = null;
				this.AttributePropInfo = null;
				this.Name = (name is not null) ? name : "null";
			}
			this.Order = order;
		}


		/// <summary>
		/// Name of the attribute, as it will appear in XML.
		/// </summary>
		public string? Name { get; }

		/// <summary>
		/// The value of this attribute instance.
		/// </summary>
		public object? Value { get; }

		/// <summary>
		/// The value of the DefaultValueAttribute which is present on some XML attribute properties.
		/// The Value property of DefaultValueAttribute contains the property default value. 
		/// </summary>
		public object? DefaultValue { get; }

		/// <summary>
		/// String version of Value.
		/// </summary>
		public string? ValueString { get; }

		/// <summary>
		/// String version of DefaultValue.
		/// </summary>
		public string? DefaultValueString { get; }
		/// <summary>
		/// ShortGuid (sGuid) for the SDC node.
		/// </summary>
		public ShortGuid? sGuid { get; }

		/// <summary>
		/// The ShortGuid property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.
		/// The sGuid can be used to retrieve an SDC object, while not holding onto an object reference inside this struct.
		/// </summary>
		public ShortGuid? ParentNodesGuid { get; }
		/// <summary>
		/// The ObjectID property of the SDC node (serialized to an XML element) that holds the attribute repesented by this struct.
		/// The ObjectID can be used to retrieve an SDC object, while not holding an object reference inside this struct.
		/// </summary>
		public int? ParentNodeObjectID { get; }


		/// <summary>
		/// The ShortGuid property of the ParentIEType
		/// The sGuid can be used to retrieve an SDC object, while not holding onto an object reference inside this struct.
		/// The ParentIETNode may be null, generating a null value for ParentIETNodesGuid.
		/// </summary>
		public ShortGuid? ParentIETNodesGuid { get; }
		/// <summary>
		/// The ObjectID property of the ParentIEType
		/// The ObjectID can be used to retrieve an SDC object, while not holding an object reference inside this struct.
		/// The ParentIETNode may be null, generating a null value for ParentIETNodeObjectID.
		/// </summary>
		public int? ParentIETNodeObjectID { get; }
		/// <summary>
		/// The serialized ordinal position of the attribute in the current element
		/// </summary>
		public int Order { get; }


		/// <summary>
		/// The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).
		/// </summary>
		public PropertyInfo? AttributePropInfo { get; }

		/// <summary>
		/// If true, if the source node's sGuid is populated, then the attribute is not serialized, and all other fields are uninitialized. <br/>
		/// if sGuid is null or has a default value ("AAAAAAAAAAAAAAAAAAAAAA"), then the node is also null.  This is a default value for a <see cref="ShortGuid"/>. 
		/// </summary>
		//public bool? IsEmpty { get; }

	}
	/// <summary>
	/// Immutable record struct to hold information about SDC object properties that will become XML attributes when serialized to XML.<br/>
	/// Does not include information about the SDC object itself, or any parent node properties
	/// </summary>
	public readonly record struct AttributeInfoLite
	{
		/// <summary>
		/// Constructor for <see cref="AttributeInfo"/>
		/// </summary>
		/// <param name="attributeValue">The value of this attribute instance.</param>
		/// <param name="attributePropInfo">The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element</param>
		public AttributeInfoLite(
			object? attributeValue,
			PropertyInfo attributePropInfo,
			int order)
		{
			if (attributePropInfo is null)
			{ this.IsEmpty = true; }
			else this.IsEmpty = false;
			this.AttributeValue = attributeValue;
			this.DefaultValue = SdcUtil.GetAttributeDefaultValue(attributePropInfo);
			this.AttributePropInfo = attributePropInfo;
			this.Name = AttributePropInfo.Name;
			this.Order = order;
		}


		/// <summary>
		/// The PropertyInfo object that describes this attribute on its parent object node (which is represented by SdcElementNodeSguid).
		/// </summary>
		public PropertyInfo AttributePropInfo { get; }

		/// <summary>
		/// The value of this attribute instance.
		/// </summary>
		public object? AttributeValue { get; }

		/// <summary>
		/// The value of the DefaultValueAttribute which is present on some XML attribute properties.
		/// The Value property of DefaultValueAttribute contains the property default value. 
		/// </summary>
		public object? DefaultValue { get; }


		/// <summary>
		/// Name of the attribute, as it will appear in XML
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The serialized ordinal position of the attribute in the current element
		/// </summary>
		public int Order { get; }

		/// <summary>
		/// If true, then if the source node's sGuid is populated, then the attribute is not serialized, and all other fields are uninitialized. <br/>
		/// if sGuid has a default value ("AAAAAAAAAAAAAAAAAAAAAA"), then the node is also null.  This is a default value for this struct. 
		/// </summary>
		public bool IsEmpty { get; }

	}


	/// <summary>
	/// public readonly record struct DifNodeIET 
	/// </summary>
	/// <param name="sGuidIET"></param>
	/// <param name="isParChanged"></param>
	/// <param name="isMoved"></param>
	/// <param name="isNew"></param>
	/// <param name="isRemoved"></param>
	/// <param name="isAttListChanged"></param>
	/// <param name="dlaiDif"></param>
	public readonly record struct DifNodeIET2
	{
		public DifNodeIET2(
			string sGuidIET,
			bool isParChanged, //parent node has changed
			bool isMoved, //prev sibling node has changed
			bool isNew, //Node present in V2 only
			bool isRemoved, //Node present in V1 only
			bool isAttListChanged = true,
			Dictionary<string, List<AttInfoDif>>? dlaiDif = null //in case we need to look up attribute Diffs by subnode sGuid
			)
		{
			this.sGuidIET = sGuidIET;
			this.isParChanged = isParChanged;
			this.isMoved = isMoved;
			this.isNew = isNew;
			this.isRemoved = isRemoved;
			this.isAttListChanged = isAttListChanged;
			this.dlaiDif = dlaiDif;
			DifNodeIET2 test = new("", true, true, true, true, true, new()); 
		}
		/// <summary>
		/// sGuid of the IET node
		/// </summary>
		public readonly string sGuidIET;
		/// <summary>
		/// True if oarent node has changed
		/// </summary>
		public readonly bool isParChanged; //parent node has changed		
		/// <summary>
		/// True if the prev sibling node has changed	
		/// </summary>
		public readonly bool isMoved; //prev sibling node has changed		
		/// <summary>
		/// True if the node is present in V2 (new version) only
		/// </summary>
		public readonly bool isNew; //Node present in V2 only		
		/// <summary>
		/// True if the node is present in V1 (previous version) only	
		/// </summary>
		public readonly bool isRemoved; //Node present in V1 only		
		/// <summary>
		/// True if the node's list of attributes has changed across versions
		/// </summary>
		public readonly bool isAttListChanged;
		/// <summary>
		/// This dictionary is used to look up attribute changes (as a <see cref="AttInfoDif"/> struct) <br/> 
		/// across new and previous node versions, using the subnode sGuid as the dictionary key.
		/// </summary>
		public readonly Dictionary<string, List<AttInfoDif>> dlaiDif; //in case we need to look up attribute Diffs by subnode sGuid

	};
	/// <summary>
	/// Holds the non-default attribute values for 2 SDC nodes (aiPrev and aiNew) whose attribtues are being compared.<br/>
	/// The nodes must be of the same SDC type, and represent the same node (i.e., share the same sGuid) <br/>
	/// from two different versions of the same SDC tree.
	/// </summary>
	/// <param name="sGuidSubnode">The sGuid shared by both SDC nodes (the older [aiPrev] node, and the newer [aiNew] node)</param>
	/// <param name="aiPrev">An <see cref="AttributeInfo"/> record containing attribute information for the older SDC tree's node.</param>
	/// <param name="aiNew">An <see cref="AttributeInfo"/> record containing attribute information for the newer SDC tree's node.</param>
	public readonly record struct AttInfoDif
	{

		public AttInfoDif(string sGuidSubnode, AttributeInfo? aiPrev, AttributeInfo? aiNew)
		{
			this.sGuidSubnode = sGuidSubnode;
			this.aiPrev = aiPrev;
			this.aiNew = aiNew;

		}
		/// <summary>
		/// The ShortGuid (sGuid) identifying the same compared node in the new and previous versions of 2 compared SDC trees.
		/// </summary>
		public readonly string sGuidSubnode;
		/// <summary>
		/// <see cref="AttributeInfo"/> record struct holding attribute information for the compared node in the previous version
		/// </summary>
		public readonly AttributeInfo? aiPrev;
		/// <summary>
		/// <see cref="AttributeInfo"/> record struct holding attribute information for the compared node in the new version
		/// </summary>
		public readonly AttributeInfo? aiNew;

	}

	/// <summary>
	/// Records the differences, if any, between the current (new) IET node (V2), and a <br/>
	/// previous version (V1) of that same node (which shares the same sGuid).
	/// </summary>
	/// <param name="sGuidIET">sGuid of the IET node</param>
	/// <param name="isParChanged">True if the parent node has changed</param>
	/// <param name="isMoved">True if the prev sibling node has changed</param>
	/// <param name="isNew">True if the node is present in V2 (new version) only</param>
	/// <param name="isRemoved">True if the node is present in V1 (previous version) only</param>
	/// <param name="isAttListChanged">True if the node's list of attributes has changed across versions</param>
	/// <param name="hasAddedSubNodes">True if the node has gained new sub-nodes</param>
	/// <param name="hasRemovedSubNodes"></param>
	/// <param name="addedSubNodes">List&lt;;BaseType> of sub-nodes that were added since the previous version of the IET.<br/>
	/// If no previous version exists for the IET node, then this list will be empty (Count = 0)</param>
	/// <param name="removedSubNodes"></param>
	/// <param name="dlaiDif">This dictionary is used to look up attribute changes (as a <see cref="AttInfoDif"/> struct) <br/> 
	/// across new and previous node versions, using the subnode sGuid as the dictionary key.</param>
	public readonly record struct DifNodeIET( //using simplified init-only, readonly, auto-implemented property syntax
			string sGuidIET,
			bool isParChanged, //parent node has changed
			bool isMoved, //prev sibling node has changed		
			bool isNew, //Node present in V2 only			
			bool isRemoved, //Node present in V1 only
			bool isAttListChanged, //one or more of the node's XML attributes have changed
			bool hasAddedSubNodes, //Node has gained new sub-nodes
			bool hasRemovedSubNodes, //Node has removed sub-nodes
			List<BaseType>? addedSubNodes,
			List<BaseType>? removedSubNodes,
			Dictionary<string, List<AttInfoDif>> dlaiDif //in case we need to look up attribute Diffs by subnode sGuid
			)
	{
		//testing of init-only, readonly, auto-implemented props:
		//readonly string s = sGuidIET;
		//void testMethod ()
		//{ 
		//	var a = hasAddedSubNodes;
		//	//hasAddedSubNodes = false;
		//}

		//readonly DifNodeIET2 test = new("", true, true, true, true, true, new());

	};


	#region SDC eCP Attribute Rollup Helpers

	public readonly record struct ResponseAttributes<T>
	( //init auto-implemented syntax

		//ResponseField
		bool responseRequired,

		//RF.TextAfterResponse
		string textAfterResponse_val,
		//RF.ResponseUnits
		string unitSystem,
		string responseUnits_val,
		//RF.Response

		//RF.Response.DataType
		T response_val,
		dtQuantEnum quantEnum,
		//numeric attributes
		T minInclusive,
		T maxInclusive,
		T minExclusive,
		T maxExclusive,
		byte fractionDigits,
		byte totalDigits


	) where T : struct, INullable
	{ };


	public readonly record struct QuestionAttributes<T>
		( //init auto-implemented syntax
			string sGuid,
			QuestionEnum QuestionType,
			//ListField attributes
			char colTextDelimiter,
			byte numCols,
			byte storedCol,
			uint minSelections,
			uint maxSelections,
			bool ordered,
			ItemChoiceType defaultListItemDataType,
			//List
			//RF
			ResponseAttributes<T> Response

		) where T : struct, INullable

	{ };
	public readonly record struct ListItemAttributes<T>
	( //init auto-implemented syntax
		string sGuid,

		bool selected,
		bool selectionDisableChildren,
		bool selectionDeselectsSiblings,
		bool omitWhenSelected,

		string associatedValue,
		ItemChoiceType associatedValueType,
		string[] SelectionActivatesItems,
		string[] SelectionSelectsListItems,

		//Response Reporting Attributes
		uint repeat,
		Guid instanceGuid,
		Guid parentGuid,

		//List
		//LIRF
		ResponseAttributes<T> Response


	) where T : struct, INullable

	{ };

	public readonly record struct QuestionAttributesHistory<Tnew, Tprev> 
		(
		QuestionAttributes<Tnew> QnewAtt,
		QuestionAttributes<Tprev> QprevAtt


		)where Tnew : struct, INullable
		where Tprev: struct, INullable
	{ };

	public readonly record struct ListItemAttributesHistory<Tnew, Tprev>
	(
	QuestionAttributes<Tnew> LInewAtt,
	QuestionAttributes<Tprev> LIprevAtt


	) where Tnew : struct, INullable
	where Tprev : struct, INullable
	{ };
	#endregion 
}

