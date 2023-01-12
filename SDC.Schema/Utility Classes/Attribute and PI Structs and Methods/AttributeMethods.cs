using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SDC.Schema
{
	public class AttributeMethods
	{
		private IList<AttributeInfo> _attributes { get; set; }
		protected delegate IList<AttributeInfo>? dlgMethod(Type t, BaseType n, string[]? attributesToExclude = null);
		private static readonly Dictionary<Type, dlgMethod> _actions = new();

		public AttributeMethods()
		{
			if (_actions.Keys.Count == 0) //no need to refill the static _actions dictionary  if was was filled earlier.
			{
				AddMethod(typeof(BaseType), MapBaseType);
				AddMethod(typeof(ExtensionBaseType), MapExtensionBaseType);
				AddMethod(typeof(PropertyType), MapPropertyType);
				AddMethod(typeof(ExtensionType), MapExtensionType);
				AddMethod(typeof(CommentType), MapCommentType);
				AddMethod(typeof(DisplayedType), MapDisplayedType);
				AddMethod(typeof(RepeatingType), MapRepeatingType);
				AddMethod(typeof(IdentifiedExtensionType), MapIdentifiedExtensionType);
				AddMethod(typeof(FormDesignType), MapFormDesignType);
				AddMethod(typeof(DemogFormDesignType), MapDemogFormDesignType);
				AddMethod(typeof(QuestionItemBaseType), MapQuestionItemBaseType);
				AddMethod(typeof(ResponseFieldType), MapResponseFieldType);
				AddMethod(typeof(ListItemResponseFieldType), MapListItemResponseFieldType);
				AddMethod(typeof(DataTypes_DEType), MapDataTypes_DEType);
				AddMethod(typeof(ListFieldType), MapListFieldType);
				AddMethod(typeof(ListType), MapListType);
				AddMethod(typeof(SectionItemType), MapSectionItemType);
				AddMethod(typeof(SectionBaseType), MapSectionBaseType);
				AddMethod(typeof(ListItemType), MapListItemType);
				AddMethod(typeof(ListItemBaseType), MapListItemBaseType);
				AddMethod(typeof(ButtonItemType), MapButtonItemType);
				AddMethod(typeof(InjectFormType), MapInjectFormType);
			}

			_attributes = new List<AttributeInfo>(0);
		}

		internal IList<AttributeInfo>? GetTypeFilledAttributes(Type t, BaseType bt, string[] attributesToExclude = null)
		{
			if (_actions.TryGetValue(t, out var method))
				return method.Invoke(t, bt, attributesToExclude);
			return null;
		}
		//private readonly Dictionary<Type, Func<Type, BaseType, string[], IList<AttributeInfo>?>> _actions2 = new();

		protected void AddMethod(Type t, dlgMethod method)
		{
			_actions.Add(t, method);
		}
		private IList<AttributeInfo>? MapBaseType(Type t, BaseType n, string[]? attributesToExclude = null)
		{
			if (t == typeof(BaseType))
			{
				_attributes = new List<AttributeInfo>(8);
				int order = 0;

				if (n.ShouldSerializename() && !(attributesToExclude?.Contains("name") ?? false))
					_attributes.Add(new(n, n.sGuid, n.name, null, order++, "name"));
				if (n.ShouldSerializetype() && !(attributesToExclude?.Contains("type") ?? false))
					_attributes.Add(new(n, n.sGuid, n.type, null, order++, "type"));
				if (n.ShouldSerializestyleClass() && !(attributesToExclude?.Contains("styleClass") ?? false))
					_attributes.Add(new(n, n.sGuid, n.styleClass, null, order++, "styleClass"));
				if (n.ShouldSerializesGuid() && !(attributesToExclude?.Contains("sGuid") ?? false))
					_attributes.Add(new(n, n.sGuid, n.order, null, order++, "sGuid"));
				if (n.ShouldSerializeorder() && !(attributesToExclude?.Contains("order") ?? false))
					_attributes.Add(new(n, n.sGuid, n.order, null, order++, "order"));

				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapExtensionBaseType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(ExtensionBaseType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}
		private IList<AttributeInfo>? MapPropertyType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{

			if (t == typeof(PropertyType))
			{
				var n = (PropertyType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializepropName() && !(attributesToExclude?.Contains("propName") ?? false))
					_attributes.Add(new(n, n.sGuid, n.propName, null, order++, "propName"));
				if (n.ShouldSerializepropClass() && !(attributesToExclude?.Contains("propClass") ?? false))
					_attributes.Add(new(n, n.sGuid, n.propClass, null, order++, "propClass"));
				if (n.ShouldSerializeval() && !(attributesToExclude?.Contains("val") ?? false))
					_attributes.Add(new(n, n.sGuid, n.val, null, order++, "val"));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapExtensionType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(ExtensionType))
			{
				var n = (ExtensionType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeAnyAttr() && !(attributesToExclude?.Contains("AnyAttr") ?? false))
					_attributes.Add(new(n, n.sGuid, n.AnyAttr, null, order++, "AnyAttr"));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapCommentType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(CommentType))
			{
				var n = (CommentType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeval() && !(attributesToExclude?.Contains("val") ?? false))
					_attributes.Add(new(n, n.sGuid, n.val, null, order++, "val"));
				return _attributes;
			}
			return null;
		}

		private IList<AttributeInfo>? MapDisplayedType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(DisplayedType))
			{
				var n = (DisplayedType)bt;
				_attributes = new List<AttributeInfo>(8);
				int order = 0;

				if (n.ShouldSerializetitle() && !(attributesToExclude?.Contains("title") ?? false))
					_attributes.Add(new(n, n.sGuid, n.title, null, order++, "title"));
				if (n.ShouldSerializeenabled() && !(attributesToExclude?.Contains("enabled") ?? false))
					_attributes.Add(new(n, n.sGuid, n.enabled, null, order++, "enabled"));
				if (n.ShouldSerializevisible() && !(attributesToExclude?.Contains("visible") ?? false))
					_attributes.Add(new(n, n.sGuid, n.visible, null, order++, "visible"));
				if (n.ShouldSerializemustImplement() && !(attributesToExclude?.Contains("mustImplement") ?? false))
					_attributes.Add(new(n, n.sGuid, n.mustImplement, null, order++, "mustImplement"));
				if (n.ShouldSerializeshowInReport() && !(attributesToExclude?.Contains("showInReport") ?? false))
					_attributes.Add(new(n, n.sGuid, n.showInReport, null, order++, "showInReport"));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapRepeatingType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(RepeatingType))
			{
				var n = (RepeatingType)bt;
				_attributes = new List<AttributeInfo>(8);
				int order = 0;

				if (n.ShouldSerializeminCard() && !(attributesToExclude?.Contains("minCard") ?? false))
					_attributes.Add(new(n, n.sGuid, n.minCard, null, order++, "minCard"));
				if (n.ShouldSerializemaxCard() && !(attributesToExclude?.Contains("maxCard") ?? false))
					_attributes.Add(new(n, n.sGuid, n.maxCard, null, order++, "maxCard"));

				//ResponseReportingAttributes
				if (n.ShouldSerializerepeat() && !(attributesToExclude?.Contains("repeat") ?? false))
					_attributes.Add(new(n, n.sGuid, n.repeat, null, order++));
				if (n.ShouldSerializeinstanceGUID() && !(attributesToExclude?.Contains("instanceGUID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceGUID, null, order++));
				if (n.ShouldSerializeparentGUID() && !(attributesToExclude?.Contains("parentGUID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.parentGUID, null, order++));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapIdentifiedExtensionType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(IdentifiedExtensionType))
			{
				var n = (IdentifiedExtensionType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeID() && !(attributesToExclude?.Contains("ID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.ID, null, order++, "ID"));
				if (n.ShouldSerializebaseURI() && !(attributesToExclude?.Contains("baseURI") ?? false))
					_attributes.Add(new(n, n.sGuid, n.baseURI, null, order++, "baseURI"));
				return _attributes;
			}
			return null;
		}

		private IList<AttributeInfo>? MapFormDesignType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(FormDesignType))
			{
				var n = (FormDesignType)bt;
				_attributes = new List<AttributeInfo>(16);
				int order = 0;

				if (n.ShouldSerializelineage() && !(attributesToExclude?.Contains("lineage") ?? false))
					_attributes.Add(new(n, n.sGuid, n.lineage, null, order++, "lineage"));
				if (n.ShouldSerializeversion() && !(attributesToExclude?.Contains("version") ?? false))
					_attributes.Add(new(n, n.sGuid, n.version, null, order++, "version"));
				if (n.ShouldSerializeversionPrev() && !(attributesToExclude?.Contains("versionPrev") ?? false))
					_attributes.Add(new(n, n.sGuid, n.versionPrev, null, order++, "versionPrev"));
				if (n.ShouldSerializefullURI() && !(attributesToExclude?.Contains("fullURI") ?? false))
					_attributes.Add(new(n, n.sGuid, n.fullURI, null, order++, "fullURI"));
				if (n.ShouldSerializefilename() && !(attributesToExclude?.Contains("filename") ?? false))
					_attributes.Add(new(n, n.sGuid, n.filename, null, order++, "filename"));
				if (n.ShouldSerializeformTitle() && !(attributesToExclude?.Contains("formTitle") ?? false))
					_attributes.Add(new(n, n.sGuid, n.formTitle, null, order++, "formTitle"));
				if (n.ShouldSerializebasedOnURI() && !(attributesToExclude?.Contains("basedOnURI") ?? false))
					_attributes.Add(new(n, n.sGuid, n.basedOnURI, null, order++, "basedOnURI"));
				if (n.ShouldSerializeinstanceID() && !(attributesToExclude?.Contains("instanceID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceID, null, order++, "instanceID"));
				if (n.ShouldSerializeinstanceVersion() && !(attributesToExclude?.Contains("instanceVersion") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceVersion, null, order++, "instanceVersion"));
				if (n.ShouldSerializeinstanceVersionURI() && !(attributesToExclude?.Contains("instanceVersionURI") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceVersionURI, null, order++, "instanceVersionURI"));
				if (n.ShouldSerializeinstanceVersionPrev() && !(attributesToExclude?.Contains("instanceVersionPrev") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceVersionPrev, null, order++, "instanceVersionPrev"));
				if (n.ShouldSerializeapprovalStatus() && !(attributesToExclude?.Contains("approvalStatus") ?? false))
					_attributes.Add(new(n, n.sGuid, n.approvalStatus, null, order++, "approvalStatus"));
				if (n.ShouldSerializecompletionStatus() && !(attributesToExclude?.Contains("completionStatus") ?? false))
					_attributes.Add(new(n, n.sGuid, n.completionStatus, null, order++, "completionStatus"));

				//Response Attributes
				if (n.ShouldSerializechangedData() && !(attributesToExclude?.Contains("changedData") ?? false))
					_attributes.Add(new(n, n.sGuid, n.changedData, null, order++, "changedData"));
				if (n.ShouldSerializenewData() && !(attributesToExclude?.Contains("newData") ?? false))
					_attributes.Add(new(n, n.sGuid, n.newData, null, order++, "newData"));

				return _attributes;
			}
			return null;
		}

		private IList<AttributeInfo>? MapDemogFormDesignType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(DemogFormDesignType))
			{
				var n = (DemogFormDesignType)bt;
				return MapFormDesignType(typeof(FormDesignType), n, attributesToExclude);
			}
			return null;
		}

		private IList<AttributeInfo>? MapQuestionItemBaseType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(QuestionItemBaseType))
			{
				var n = (QuestionItemBaseType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializereadOnly() && !(attributesToExclude?.Contains("readOnly") ?? false))
					_attributes.Add(new(n, n.sGuid, n.readOnly, null, order++, "readOnly"));
				//Response Attributes
				if (n.ShouldSerializechangedData() && !(attributesToExclude?.Contains("changedData") ?? false))
					_attributes.Add(new(n, n.sGuid, n.changedData, null, order++, "changedData"));
				if (n.ShouldSerializenewData() && !(attributesToExclude?.Contains("newData") ?? false))
					_attributes.Add(new(n, n.sGuid, n.newData, null, order++, "newData"));
				return _attributes;
			}
			return null;
		}


		private IList<AttributeInfo>? MapResponseFieldType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(ResponseFieldType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}
		private IList<AttributeInfo>? MapListItemResponseFieldType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(ListItemResponseFieldType))
			{
				var n = (ListItemResponseFieldType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeresponseRequired() && !(attributesToExclude?.Contains("responseRequired") ?? false))
					_attributes.Add(new(n, n.sGuid, n.responseRequired, null, order++, "responseRequired"));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapDataTypes_DEType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(DataTypes_DEType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}

		private IList<AttributeInfo>? MapListFieldType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(ListFieldType))
			{
				var n = (ListFieldType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializecolTextDelimiter() && !(attributesToExclude?.Contains("colTextDelimiter") ?? false))
					_attributes.Add(new(n, n.sGuid, n.colTextDelimiter, null, order++, "colTextDelimiter"));
				if (n.ShouldSerializenumCols() && !(attributesToExclude?.Contains("numCols") ?? false))
					_attributes.Add(new(n, n.sGuid, n.numCols, null, order++, "numCols"));
				if (n.ShouldSerializestoredCol() && !(attributesToExclude?.Contains("storedCol") ?? false))
					_attributes.Add(new(n, n.sGuid, n.storedCol, null, order++, "storedCol"));
				if (n.ShouldSerializeminSelections() && !(attributesToExclude?.Contains("minSelections") ?? false))
					_attributes.Add(new(n, n.sGuid, n.minSelections, null, order++, "minSelections"));
				if (n.ShouldSerializemaxSelections() && !(attributesToExclude?.Contains("maxSelections") ?? false))
					_attributes.Add(new(n, n.sGuid, n.maxSelections, null, order++, "maxSelections"));
				if (n.ShouldSerializeordered() && !(attributesToExclude?.Contains("ordered") ?? false))
					_attributes.Add(new(n, n.sGuid, n.ordered, null, order++, "ordered"));
				if (n.ShouldSerializedefaultListItemDataType() && !(attributesToExclude?.Contains("defaultListItemDataType") ?? false))
					_attributes.Add(new(n, n.sGuid, n.defaultListItemDataType, null, order++, "defaultListItemDataType"));
				return _attributes;
			}
			return null;
		}

		private IList<AttributeInfo>? MapListType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(ListType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}


		private IList<AttributeInfo>? MapSectionItemType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(SectionItemType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}

		private IList<AttributeInfo>? MapSectionBaseType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(SectionBaseType))
			{
				var n = (SectionBaseType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeordered() && !(attributesToExclude?.Contains("ordered") ?? false))
					_attributes.Add(new(n, n.sGuid, n.ordered, null, order++, "ordered"));

				//ResponseAttributes
				if (n.ShouldSerializechangedData() && !(attributesToExclude?.Contains("changedData") ?? false))
					_attributes.Add(new(n, n.sGuid, n.changedData, null, order++, "changedData"));
				if (n.ShouldSerializenewData() && !(attributesToExclude?.Contains("newData") ?? false))
					_attributes.Add(new(n, n.sGuid, n.newData, null, order++, "newData"));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapListItemType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(ListItemType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}
		private IList<AttributeInfo>? MapListItemBaseType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(ListItemType))
			{
				var n = (ListItemType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeselected() && !(attributesToExclude?.Contains("selected") ?? false))
					_attributes.Add(new(n, n.sGuid, n.selected, null, order++, "selected"));
				if (n.ShouldSerializeselectionDisablesChildren() && !(attributesToExclude?.Contains("selectionDisablesChildren") ?? false))
					_attributes.Add(new(n, n.sGuid, n.selectionDisablesChildren, null, order++, "selectionDisablesChildren"));
				if (n.ShouldSerializeselectionActivatesItems() && !(attributesToExclude?.Contains("selectionActivatesItems") ?? false))
					_attributes.Add(new(n, n.sGuid, n.selectionActivatesItems, null, order++, "selectionActivatesItems"));
				if (n.ShouldSerializeselectionSelectsListItems() && !(attributesToExclude?.Contains("selectionSelectsListItems") ?? false))
					_attributes.Add(new(n, n.sGuid, n.selectionSelectsListItems, null, order++, "selectionSelectsListItems"));
				if (n.ShouldSerializeselectionDeselectsSiblings() && !(attributesToExclude?.Contains("selectionDeselectsSiblings") ?? false))
					_attributes.Add(new(n, n.sGuid, n.selectionDeselectsSiblings, null, order++, "selectionDeselectsSiblings"));
				if (n.ShouldSerializeomitWhenSelected() && !(attributesToExclude?.Contains("omitWhenSelected") ?? false))
					_attributes.Add(new(n, n.sGuid, n.omitWhenSelected, null, order++, "omitWhenSelected"));

				//ResponseReportingAttributes
				if (n.ShouldSerializerepeat() && n.repeat > 0 && !(attributesToExclude?.Contains("repeat") ?? false))
					_attributes.Add(new(n, n.sGuid, n.repeat, null, order++, "repeat"));
				if (n.ShouldSerializeinstanceGUID() && !(attributesToExclude?.Contains("instanceGUID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceGUID, null, order++, "instanceGUID"));
				if (n.ShouldSerializeparentGUID() && !(attributesToExclude?.Contains("parentGUID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.parentGUID, null, order++, "parentGUID"));

				if (n.ShouldSerializeassociatedValue() && !(attributesToExclude?.Contains("associatedValue") ?? false))
					_attributes.Add(new(n, n.sGuid, n.associatedValue, null, order++, "associatedValue"));
				if (n.ShouldSerializeassociatedValueType() && !(attributesToExclude?.Contains("associatedValueType") ?? false))
					_attributes.Add(new(n, n.sGuid, n.associatedValueType, null, order++, "associatedValueType"));
				return _attributes;
			}
			return null;
		}
		private IList<AttributeInfo>? MapButtonItemType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			//no attributes present
			if (t == typeof(ButtonItemType))
			{
				return new List<AttributeInfo>(1);
			}
			return null;
		}

		private IList<AttributeInfo>? MapInjectFormType(Type t, BaseType bt, string[]? attributesToExclude = null)
		{
			if (t == typeof(InjectFormType))
			{
				var n = (InjectFormType)bt;
				_attributes = new List<AttributeInfo>(4);
				int order = 0;

				if (n.ShouldSerializeInjectionSourceURI() && !(attributesToExclude?.Contains("InjectionSourceURI") ?? false))
					_attributes.Add(new(n, n.sGuid, n.InjectionSourceURI, null, order++, "InjectionSourceURI"));
				if (n.ShouldSerializerootItemID() && !(attributesToExclude?.Contains("rootItemID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.rootItemID, null, order++, "rootItemID"));
				if (n.ShouldSerializeserverURI() && !(attributesToExclude?.Contains("serverURI") ?? false))
					_attributes.Add(new(n, n.sGuid, n.serverURI, null, order++, "serverURI"));

				//ResponseReportingAttributes
				if (n.ShouldSerializerepeat() && n.repeat > 0 && !(attributesToExclude?.Contains("repeat") ?? false))
					_attributes.Add(new(n, n.sGuid, n.repeat, null, order++, "repeat"));
				if (n.ShouldSerializeinstanceGUID() && !(attributesToExclude?.Contains("instanceGUID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.instanceGUID, null, order++, "instanceGUID"));
				if (n.ShouldSerializeparentGUID() && !(attributesToExclude?.Contains("parentGUID") ?? false))
					_attributes.Add(new(n, n.sGuid, n.parentGUID, null, order++, "parentGUID"));
				return _attributes;
			}
			return null;
		}



	}
}
