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
	/// Immutable record struct that captures serialized XML attribute data for an SDC <see cref="BaseType"/> node.
	/// </summary>
	public readonly record struct AttributeInfo
	{
		/// <summary>
		/// Creates an <see cref="AttributeInfo"/> record for one serialized XML attribute on an SDC <see cref="BaseType"/> node.
		/// </summary>
		/// <param name="node">The SDC node that owns the attribute.</param>
		/// <param name="attributeValue">The current value of the attribute.</param>
		/// <param name="attributePropInfo">The <see cref="PropertyInfo"/> that describes the attribute on the owning node.</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element.</param>
		/// <param name="name">The XML attribute name; when omitted, <paramref name="attributePropInfo"/> supplies the name.</param>
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
		/// XML attribute name.
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
		/// The <see cref="ShortGuid"/> for the owning SDC node.
		/// </summary>
		public ShortGuid? sGuid { get; }

		/// <summary>
		/// The <see cref="ShortGuid"/> of the immediate parent <see cref="BaseType"/> node.
		/// </summary>
		public ShortGuid? ParentNodesGuid { get; }
		/// <summary>
		/// The <see cref="BaseType.ObjectID"/> of the immediate parent <see cref="BaseType"/> node.
		/// </summary>
		public int? ParentNodeObjectID { get; }


		/// <summary>
		/// The <see cref="ShortGuid"/> of the parent <see cref="IdentifiedExtensionType"/> node.
		/// </summary>
		public ShortGuid? ParentIETNodesGuid { get; }
		/// <summary>
		/// The <see cref="BaseType.ObjectID"/> of the parent <see cref="IdentifiedExtensionType"/> node.
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
		/// Indicates whether the record is the default, uninitialized value.
		/// </summary>
		//public bool? IsEmpty { get; }

	}
	/// <summary>
	/// Immutable record struct that captures attribute metadata for a single property value on an SDC <see cref="BaseType"/> node.
	/// </summary>
	/// <remarks>
	/// This type does not include the owning SDC object or parent-node metadata.
	/// </remarks>
	public readonly record struct AttributeInfoLite
	{
		/// <summary>
		/// Creates an <see cref="AttributeInfoLite"/> value for one XML attribute.
		/// </summary>
		/// <param name="attributeValue">The current value of the attribute.</param>
		/// <param name="attributePropInfo">The <see cref="PropertyInfo"/> that describes the attribute.</param>
		/// <param name="order">The serialized ordinal position of the attribute in the current element.</param>
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
		/// XML attribute name.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The serialized ordinal position of the attribute in the current element
		/// </summary>
		public int Order { get; }

		/// <summary>
		/// Indicates whether the record is the default, uninitialized value.
		/// </summary>
		public bool IsEmpty { get; }

	}


	/// <summary>
	/// Captures comparison results for an IET node.
	/// </summary>
	/// <remarks>
	/// Use the nested <see cref="DifNodeIET"/> type for the full change summary that includes added, removed, and changed attributes, plus subnode changes.
	/// </remarks>
	/// <param name="sGuidIET">The sGuid of the IET node being compared.</param>
	/// <param name="isParChanged">True when the IET node's parent node sGuid differs between versions.</param>
	/// <param name="isMoved">True when the IET node's previous sibling sGuid differs between versions.</param>
	/// <param name="isNew">True when the IET node exists only in the newer version.</param>
	/// <param name="isRemoved">True when the IET node exists only in the previous version.</param>
	/// <param name="isAttListChanged">True when one or more serialized attributes differ on this IET node or on a non-IET descendant node.</param>
	/// <param name="dlaiDif">Mutable attribute-difference data keyed by node sGuid.</param>
	public readonly record struct DifNodeIET2
	{
		public DifNodeIET2(
			string sGuidIET,
			bool isParChanged, //parent node has changed
			bool isMoved, //True when the IET node's previous sibling sGuid differs between versions.
			bool isNew, //True when the IET node exists only in the newer version.
			bool isRemoved, //True when the IET node exists only in the previous version.
			bool isAttListChanged = true,
			Dictionary<string, List<AttInfoDif>>? dlaiDif = null //Mutable attribute-difference data keyed by node sGuid.
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
		/// True when the IET node's parent node sGuid differs between versions.
		/// </summary>
		public readonly bool isParChanged; //parent node has changed		
		/// <summary>
		/// True when the IET node's previous sibling sGuid differs between versions.
		/// </summary>
		public readonly bool isMoved; //prev sibling node has changed		
		/// <summary>
		/// True when the IET node exists only in the newer version.
		/// </summary>
		public readonly bool isNew; //Node present in V2 only		
		/// <summary>
		/// True when the IET node exists only in the previous version.
		/// </summary>
		public readonly bool isRemoved; //Node present in V1 only		
		/// <summary>
		/// True when one or more serialized attributes differ on this IET node or on a non-IET descendant node.
		/// </summary>
		public readonly bool isAttListChanged;
		/// <summary>
		/// Mutable attribute-difference data keyed by node sGuid.
		/// </summary>
		public readonly Dictionary<string, List<AttInfoDif>> dlaiDif; //in case we need to look up attribute Diffs by subnode sGuid

	};
	/// <summary>
	/// Holds serialized attribute comparison data for one logical node across two versions of the same SDC tree.
	/// </summary>
	/// <remarks>
	/// The compared nodes must be the same SDC type and represent the same logical node, identified by the same sGuid.
	/// </remarks>
	/// <param name="sGuidSubnode">The sGuid shared by the compared node in the previous and newer versions.</param>
	/// <param name="aiPrev">Attribute information from the previous version, or null when the attribute did not exist there.</param>
	/// <param name="aiNew">Attribute information from the newer version, or null when the attribute does not exist there.</param>
	public readonly record struct AttInfoDif
	{

		public AttInfoDif(string sGuidSubnode, 
			AttributeInfo? aiPrev, AttributeInfo? aiNew
			, string elementName, string propertyName, string displayName)
		{
			this.sGuidSubnode = sGuidSubnode;			
			this.aiPrev = aiPrev;
			this.aiNew = aiNew;
			this.elementName = elementName;
			this.propertyName = propertyName;
			this.displayName = displayName;
			 
		}
		/// <summary>
		/// The <see cref="ShortGuid"/> identifying the compared node in both versions.
		/// </summary>
		public readonly string sGuidSubnode;
		/// <summary>
		/// <see cref="AttributeInfo"/> data for the compared node in the previous version.
		/// </summary>
		public readonly AttributeInfo? aiPrev;
		/// <summary>
		/// <see cref="AttributeInfo"/> data for the compared node in the newer version.
		/// </summary>
		public readonly AttributeInfo? aiNew;


		/// <summary>
		/// The XML element name for the compared node.
		/// </summary>
		public readonly string? elementName;
		/// <summary>
		/// The property name for the compared node, if the node is represented by a PropertyType.
		/// </summary>
		public readonly string? propertyName;
		/// <summary>
		/// The display name used when reporting the compared node.
		/// </summary>
		public readonly string? displayName;
	}

	/// <summary>
	/// Records the differences, if any, between the current (new) IET node (V2), and the
	/// previous version (V1) of that same node (which shares the same sGuid).
	/// </summary>
	/// <param name="sGuidIET">sGuid of the IET node.</param>
	/// <param name="isParChanged">True if the IET node's parent node sGuid differs between versions.</param>
	/// <param name="isMoved">True if the IET node's previous sibling sGuid differs between versions.</param>
	/// <param name="isNew">True if the IET node exists in V2 only.</param>
	/// <param name="isRemoved">True if the IET node exists in V1 only.</param>
	/// <param name="isAttListChanged">True if one or more serialized attributes changed on the IET node itself or on any non-IET descendant node.</param>
	/// <param name="hasAddedSubNodes">True if one or more non-IET descendant nodes exist in V2 but not in V1, or were moved under a different direct or IET parent.</param>
	/// <param name="hasRemovedSubNodes">True if one or more non-IET descendant nodes exist in V1 but not in V2, or were moved away from this IET node.</param>
	/// <param name="AddedAttributes">Read-only lookup of nodes that gained serialized attributes. The key is the node sGuid, and the value is one representative <see cref="AttributeInfo"/> for that node. This includes the IET node itself and non-IET descendant nodes. A change from a default value to a non-default serialized value is treated as an added attribute.</param>
	/// <param name="RemovedAttributes">Read-only lookup of nodes that lost serialized attributes. The key is the node sGuid, and the value is one representative <see cref="AttributeInfo"/> for that node. This includes the IET node itself and non-IET descendant nodes. A change from a non-default serialized value to a default value is treated as a removed attribute.</param>
	/// <param name="ChangedAttributes">Read-only lookup of nodes whose serialized attribute value changed between versions. The key is the node sGuid, and the value is one representative <see cref="AttributeInfo"/> for that node. This includes the IET node itself and non-IET descendant nodes. Default-value transitions are included here when the serialized form changed.</param>
	/// <param name="isChanged">True if an existing IET node has changed because its parent changed, its previous sibling changed, it gained or lost non-IET subnodes, or one or more serialized attributes changed on the IET node or on any non-IET descendant node. This does not include <paramref name="isNew"/> or <paramref name="isRemoved"/>.</param>
	/// <param name="addedSubNodes">Read-only list of non-IET descendant nodes that were added under this IET node in V2.</param>
	/// <param name="removedSubNodes">Read-only list of non-IET descendant nodes that were removed from this IET node in V2.</param>
	/// <param name="dlaiDif">Read-only lookup of attribute differences by node sGuid. Each value is a read-only list of <see cref="AttInfoDif"/> entries for that node.</param>
	public readonly record struct DifNodeIET(
			string sGuidIET,
			bool isParChanged,
			bool isMoved,
			bool isNew,
			bool isRemoved,
			bool isAttListChanged,
			bool hasAddedSubNodes,
			bool hasRemovedSubNodes,
			IReadOnlyDictionary<string, AttributeInfo>? AddedAttributes,
			IReadOnlyDictionary<string, AttributeInfo>? RemovedAttributes,
			IReadOnlyDictionary<string, AttributeInfo>? ChangedAttributes,
			bool isChanged,
			IReadOnlyList<BaseType>? addedSubNodes,
			IReadOnlyList<BaseType>? removedSubNodes,
			IReadOnlyDictionary<string, IReadOnlyList<AttInfoDif>> dlaiDif
			)
	{
		public DifNodeIET(
			string sGuidIET,
			bool isParChanged,
			bool isMoved,
			bool isNew,
			bool isRemoved,
			bool isAttListChanged,
			bool hasAddedSubNodes,
			bool hasRemovedSubNodes,
			Dictionary<string, AttributeInfo>? AddedAttributes,
			Dictionary<string, AttributeInfo>? RemovedAttributes,
			Dictionary<string, AttributeInfo>? ChangedAttributes,
			bool isChanged,
			List<BaseType>? addedSubNodes,
			List<BaseType>? removedSubNodes,
			Dictionary<string, List<AttInfoDif>>? dlaiDif)
			: this(
				sGuidIET,
				isParChanged,
				isMoved,
				isNew,
				isRemoved,
				isAttListChanged,
				hasAddedSubNodes,
				hasRemovedSubNodes,
				AddedAttributes,
				RemovedAttributes,
				ChangedAttributes,
				isChanged,
				addedSubNodes,
				removedSubNodes,
				dlaiDif is null ? null : dlaiDif.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<AttInfoDif>)kvp.Value))
		{
		}
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

