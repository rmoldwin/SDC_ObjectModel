using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDC.Schema
{
	#region Empty Interface Extension Classes	 - Not yet used
	public static class INewTopLevelExtensions { } //Empty
	public static class IPackageExtensions { } //Empty
	public static class IDataElementExtensions { } //Empty
	public static class IDemogFormExtensions { } //Empty
	public static class IMapExtensions { } //Empty
	public static class RetrieveFormPackageTypeExtensions //Not Implemented
	{
		public static LinkType AddFormURL_(this RetrieveFormPackageType rfp)
		{ throw new NotImplementedException(); }
		public static HTMLPackageType AddHTMLPackage_(this RetrieveFormPackageType rfp)
		{ throw new NotImplementedException(); }
		public static XMLPackageType AddXMLPackage_(this RetrieveFormPackageType rfp)
		{ throw new NotImplementedException(); }
	}
	public static class IChildItemsMemberExtensions
	{
		//!    public static bool X_IsMoveAllowedToChild<U>(U Utarget, out string error)
		//where U : notnull, IdentifiedExtensionType
		//        //where T : notnull, IdentifiedExtensionType
		//    {
		//        Tchild Tsource = this as Tchild;
		//        var errorSource = "";
		//        var errorTarget = "";
		//        error = "";
		//        bool sourceOK = false;
		//        bool targetOK = false;

		//        if (Tsource is null) { error = "source is null"; return false; }
		//        if (Utarget is null) { error = "target is null"; return false; }
		//        if (Utarget is ButtonItemType) { error = "ButtonItemType is not allowed as a target"; return false; }
		//        if (Utarget is InjectFormType) { error = "InjectFormType is not allowed as a target"; return false; }
		//        if (Utarget is DisplayedType) { error = "DisplayedItem is not allowed as a target"; return false; }

		//        if (Tsource is ListItemType && !(Utarget is QuestionItemType) && !(Utarget is ListItemType)) { error = "A ListItem can only be moved into a Question List"; return false; };

		//        //special case to allow LI to drop on a Q and be added to the Q's List, rather than under ChildItem (which would be illegal)
		//        if (Tsource is ListItemType &&
		//            Utarget is QuestionItemType &&
		//            !((Utarget as QuestionItemType).GetQuestionSubtype() == QuestionEnum.QuestionSingle) &&
		//            !((Utarget as QuestionItemType).GetQuestionSubtype() == QuestionEnum.QuestionMultiple))
		//        { error = "A Question target must be a QuestionSingle or QuestionMultiple"; return false; }


		//        if (Tsource is DisplayedType || Tsource is InjectFormType) sourceOK = true;
		//        if (Utarget is QuestionItemType || Utarget is SectionItemType || Utarget is ListItemType) targetOK = true;

		//        if (!sourceOK || !targetOK)
		//        {
		//            if (!sourceOK) errorSource = "Illegal source object";
		//            if (!targetOK) errorTarget = "Illegal target object";
		//            if (errorTarget.Length > 0) errorTarget += " and ";
		//            error = errorSource + errorTarget;
		//        }


		//        return sourceOK & targetOK;
		//    }
		//!    public static bool X_MoveAsChild<S, T>(S source, T target, int newListIndex)
		//        where S : notnull, IdentifiedExtensionType    //, IChildItemMember
		//        where T : DisplayedType, IChildItemsParent<T>
		//    {
		//        if (source is null) return false;
		//        if (source.ParentNode is null) return false;
		//        if (source is ListItemType && !(target is QuestionItemType)) return false;  //ListItem can only be moved to a question.

		//        List<BaseType> sourceList;
		//        BaseType newParent = target;

		//        switch (source)  //get the sourceList from the parent node
		//        {
		//            case QuestionItemType _:
		//            case SectionItemType _:
		//            case InjectFormType _:
		//            case ButtonItemType _:
		//                sourceList = (source.ParentNode as ChildItemsType)?.Items.ToList<BaseType>();
		//                //sourceList = (source.ParentNode as ChildItemsType).Items.Cast<BaseType>().ToList(); //alternate method
		//                break;
		//            case ListItemType _:
		//                sourceList = (source.ParentNode as ListType)?.Items.ToList<BaseType>();
		//                break;
		//            case DisplayedType _:
		//                sourceList = (source.ParentNode as ChildItemsType)?.Items.ToList<BaseType>();
		//                if (sourceList is null)
		//                    sourceList = (source.ParentNode as ListType)?.Items.ToList<BaseType>();
		//                else return false;
		//                break;
		//            default:
		//                return false; //error in source type
		//        }

		//        if (sourceList is null) return false;

		//        List<BaseType> targetList = null;

		//        if (target != null)
		//        {
		//            switch (target)  //get the targetList from the child node
		//            {
		//                case QuestionItemType q:
		//                    //This is an exception - if we drop a source LI on a QS/QM, we will want to add it ant the end of the Q's List object
		//                    if (source is ListItemType)
		//                    {
		//                        if (q.GetQuestionSubtype() != QuestionEnum.QuestionSingle &&
		//                            q.GetQuestionSubtype() != QuestionEnum.QuestionMultiple &&
		//                            q.GetQuestionSubtype() != QuestionEnum.QuestionRaw) return false;  //QR, and QL cannot have child LI nodes
		//                        if (q.ListField_Item is null)  //create new targetList
		//                        {
		//                            targetList = IQuestionBuilder.AddListToListField(IQuestionBuilder.AddListFieldToQuestion(q)).Items.ToList<BaseType>();
		//                            if (targetList is null) return false;
		//                            break;
		//                        }
		//                        newParent = q.ListField_Item.List;
		//                        targetList = q.ListField_Item.List.Items.ToList<BaseType>();
		//                    }
		//                    else //use the ChildItems node instead as the targetList
		//                    {
		//                        (q as IChildItemsParent<QuestionItemType>).AddChildItemsNode(q);
		//                        targetList = q.ChildItemsNode.Items.ToList<BaseType>();
		//                    }
		//                    break;
		//                case SectionItemType s:
		//                    (s as IChildItemsParent<SectionItemType>).AddChildItemsNode(s);
		//                    targetList = s.ChildItemsNode.Items.ToList<BaseType>();
		//                    break;
		//                case ListItemType l:
		//                    (l as IChildItemsParent<ListItemType>).AddChildItemsNode(l);
		//                    targetList = l.ChildItemsNode.Items.ToList<BaseType>();
		//                    break;
		//                default:
		//                    return false; //error in source type
		//            }
		//        }
		//        else targetList = sourceList;
		//        if (targetList is null) return false;


		//        var count = targetList.Count;
		//        if (newListIndex < 0 || newListIndex > count) newListIndex = count; //add to end  of list

		//        var indexSource = sourceList.IndexOf(source);  //save the original source index in case we need to replace the source node back to its origin
		//        bool b = sourceList.Remove(source); if (!b) return false;
		//        targetList.Insert(newListIndex, source);
		//        if (targetList[newListIndex] == source) //check for success
		//        {
		//            source.TopNode.ParentNodes[source.ObjectGUID] = newParent;
		//            return true;
		//        }
		//        //Error - the source item is now disconnected from the list.  Lets add it back to the end of the list.
		//        sourceList.Insert(indexSource, source); //put source back where it came from; the move failed
		//        return false;
		//    }
		//!    public static bool X_MoveAfterSib<S, T>(S source, T target, int newListIndex, bool moveAbove)
		//        where S : notnull, IdentifiedExtensionType
		//        where T : notnull, IdentifiedExtensionType
		//    {
		//        //iupdate TopNode.ParentNodes
		//        throw new Exception(String.Format("Not Implemented"));
		//    }
	} //Empty
	public static class IQuestionBaseExtensions { } //Empty
													//public static class ISectionExtensions { } //Empty
	public static class ButtonItemTypeExtensions //Not Implemented
	{
		public static EventType AddOnClick_(this ButtonItemType bf)
		{ throw new NotImplementedException(); }
	}
	public static class InjectFormTypeExtensions //Not Implemented
	{  //ChildItems.InjectForm - this is mainly useful for a DEF injecting items based on the InjectForm URL
	   //Item types choice under ChildItems
		public static FormDesignType AddFormDesign_(this InjectFormType ijt)
		{ throw new NotImplementedException(); }
		public static QuestionItemType AddQuestion_(this InjectFormType ijt)
		{ throw new NotImplementedException(); }
		public static SectionItemType AddSection_(this InjectFormType ijt)
		{ throw new NotImplementedException(); }

	}
	public static class DisplayedTypeChangesExtensions
	{
		public static QuestionItemType ChangeToQuestionMultiple_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionSingle_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionResponse_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionLookup_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static SectionItemType ChangeToSection_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static ButtonItemType ChangeToButtonAction_(DisplayedType source)
		{ throw new NotImplementedException(); }
		public static InjectFormType ChangeToInjectForm_(DisplayedType source)
		{ throw new NotImplementedException(); }

		public static DisplayedType ChangeToDisplayedItem_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionMultiple_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionSingle_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionResponse_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static QuestionItemType ChangeToQuestionLookup_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static ButtonItemType ChangeToButtonAction_(SectionItemType source)
		{ throw new NotImplementedException(); }
		public static InjectFormType ChangeToInjectForm_(SectionItemType source)
		{ throw new NotImplementedException(); }


		public static DisplayedType ChangeToDisplayedItem_(ListItemType source)
		{ throw new NotImplementedException(); }

		//ListItemType ChangeToListItem
		//ListItemType ChangeToListItemResponse
		//SectionItemType ChangeToSection()
		//ChangeToButtonAction
		//ChangeToInjectForm
		//etc.


		//Question
		public static SectionItemType ChangeToSection_(QuestionItemType source)
		{ throw new NotImplementedException(); }
		public static DisplayedType ChangeToDisplayedType_(QuestionItemType source)
		{ throw new NotImplementedException(); }
	}
	//public static class IDisplayedTypeMemberExtensions { }//Empty; for LinkType, BlobType, ContactType, CodingType, EventType, OnEventType, PredGuardType
	public static class BlobExtensions //Not Implemented
	{
		//DisplayedItem.BlobType
		//Uses Items types choice
		public static bool AddBinaryMedia_(this BlobType b)
		{ throw new NotImplementedException(); } //Empty

		public static bool AddBlobURI_(this BlobType b)
		{ throw new NotImplementedException(); }
	}
	public static class IdentifiedExtensionTypeExtensions //Not Implemented
	{
		public static string GetNewCkey_(this IdentifiedExtensionType i)
		{ throw new NotImplementedException(); }
	}
	public static class IResponseExtensions //Not Implemented
	{
		//UnitsType AddUnits(ResponseFieldType rfParent);
		//public static UnitsType AddUnits(this IResponse _, ResponseFieldType rfParent)
		//{
		//    UnitsType u = new UnitsType(rfParent);
		//    rfParent.ResponseUnits = u;
		//    return u;
		//}
		public static RichTextType AddTextAfterResponse_()
		{ throw new NotImplementedException(); }
		public static BaseType GetDataTypeObject_()
		{ throw new NotImplementedException(); }
	}
	public static class IValExtensions
	{//Implemented by data types, which have a strongly-typed val attribute.  Not implemented by anyType, XML, or HTML  
	} //Empty
	public static class IValNumericExtensions
	{//Implemented by numeric data types, which have a strongly-type val attribute.

	} //Empty
	public static class IValDateTimeExtensions
	{//Implemented by DateTime data types, which have a strongly-type val attribute.

	} //Empty
	public static class IValIntegerExtensions
	{//Implemented by Integer data types, which have a strongly-type val attribute.  Includes byte, short, long, positive, no-positive, negative and non-negative types
	} //Empty
	public static class IAddCodingExtensions //Not Implemented
	{
		public static CodingType AddCodedValue_(this IAddCoding ac, DisplayedType dt, int insertPosition)
		{
			throw new NotImplementedException();
		}

		public static CodingType AddCodedValue_(this IAddCoding ac, LookupEndPointType lep, int insertPosition)
		{
			throw new NotImplementedException();
		}
		public static UnitsType AddUnits(this IAddCoding ac, CodingType ctParent)
		{
			var u = new UnitsType(ctParent);
			ctParent.Units = u;
			return u;
		}
	}
	public static class IEventExtension  //Not Implemented  //Used for events (PredActionType)
	{
		public static PredEvalAttribValuesType AddAttributeVal(this IEvent ae)
		{
			var pgt = (PredActionType)ae;
			var av = new PredEvalAttribValuesType(pgt);
			pgt.Items.Add(av);
			return av;
		}
		//public static ScriptBoolFuncActionType AddScriptBoolFunc_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		//public static CallFuncBoolActionType AddCallBoolFunction_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		//public static MultiSelectionsActionType AddMultiSelections_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		//public static SelectionSetsActionType AddSelectionSets_(this IEvent ae)
		//{ throw new NotImplementedException(); }
		public static PredSelectionTestType AddSelectionTest_(this IEvent ae)
		{ throw new NotImplementedException(); }
		//PredAlternativesType AddItemAlternatives();
		public static RuleSelectMatchingListItemsType SelectMatchingListItems_(this IEvent ae)
		{ throw new NotImplementedException(); }
		public static PredGuardType AddGroup_(this IEvent ae)
		{ throw new NotImplementedException(); }
	}
	public static class IPredGuardExtensions //Not Implemented //used by Guards on ListItem, Button, e.g., SelectIf, DeselectIf
	{
		public static PredEvalAttribValuesType AddAttributeVal(this IPredGuard ipg)
		{
			var pgt = (PredGuardType)ipg;
			var av = new PredEvalAttribValuesType(pgt);
			pgt.Items.Add(av);
			return av;
		}
		//public static ScriptBoolFuncActionType AddScriptBoolFunc_(this IPredGuard ipg)
		//{ throw new NotImplementedException(); }
		//public static CallFuncBoolActionType AddCallBoolFunction_(this IPredGuard ipg) 
		//{ throw new NotImplementedException(); }
		//public static MultiSelectionsActionType AddMultiSelections_(this IPredGuard ipg) 
		//{ throw new NotImplementedException(); }
		public static PredSelectionTestType AddSelectionTest_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }
		public static PredGuardTypeSelectionSets AddSelectionSets_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }
		public static PredAlternativesType AddItemAlternatives_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }
		public static PredGuardType AddGroup_(this IPredGuard ipg)
		{ throw new NotImplementedException(); }


	}
	public static class IRuleExtensions
	{
		public static RuleAutoActivateType AddAutoActivation_(this IRule r)
		{ throw new NotImplementedException(); }
		public static RuleAutoSelectType AddAutoSelection_(this IRule r)
		{ throw new NotImplementedException(); }
		public static PredActionType AddConditionalActions_(this IRule r)
		{ throw new NotImplementedException(); }
		//public static CallFuncActionType AddExternalRule_(this IRule r)
		//{ throw new NotImplementedException(); }
		public static ScriptCodeAnyType AddScriptedRule_(this IRule r)
		{ throw new NotImplementedException(); }
		public static RuleSelectMatchingListItemsType AddSelectMatchingListItems_(this IRule r)
		{ throw new NotImplementedException(); }
		public static ValidationType AddValidation_(this IRule r)
		{ throw new NotImplementedException(); }
	} //Not Implemented
	public static class IHasConditionalActionsNodeExtensions
	{
		public static PredActionType AddConditionalActionsNode_(this IHasConditionalActionsNode hcan)
		{ throw new NotImplementedException(); }
	} //Not Implemented
	public static class IHasParameterGroupExtensions //Not Implemented
	{
		public static ParameterItemType AddParameterRefNode_(this IHasParameterGroup hpg)
		{ throw new NotImplementedException(); }
		public static ListItemParameterType AddListItemParameterRefNode_(this IHasParameterGroup hpg)
		{ throw new NotImplementedException(); }
		public static ParameterValueType AddParameterValueNode_(IHasParameterGroup hpg)
		{ throw new NotImplementedException(); }
	}
	public static class IHasDataType_STypeExtensions //Not Implemented
	{
		public static DataTypes_SType AddDataTypes_SType_(this DataTypes_SType S)
		{ throw new NotImplementedException(); }
	}
	public static class IHasDataType_DETypeExtensions
	{
		public static DataTypes_DEType AddDataTypes_DEType_(this DataTypes_DEType DE)
		{ throw new NotImplementedException(); }
	} //Not Implemented
	public static class IHasActionElseGroupExtensions { } //Empty
	//public static class IHasElseNodeExtensions
	//{
	//    public static PredActionType AddElseNode(this IHasElseNode hen)
	//    {
	//        if (hen is null) return null;
	//        var elseNode = new PredActionType((BaseType)hen);

	//        switch (hen)
	//        {
	//            case PredActionType pe:
	//                pe.Else.Add(elseNode); return elseNode;
	//            case CallFuncBoolActionType cfb:
	//                return (PredActionType)SdcUtil.ArrayAddReturnItem(cfb.Items1, elseNode);
	//            case ScriptBoolFuncActionType sb:
	//                return (PredActionType)SdcUtil.ArrayAddReturnItem(sb.Items, elseNode);
	//            case AttributeEvalActionType ae:
	//                ae.Else.Add(elseNode); return elseNode;
	//            case MultiSelectionsActionType ms:
	//                ms.Else.Add(elseNode); return elseNode;
	//            case SelectionSetsActionType ss:
	//                ss.Else.Add(elseNode); return elseNode;
	//            case SelectionTestActionType st:
	//                st.Else.Add(elseNode); return elseNode;
	//            default:
	//                break;
	//        }
	//        throw new InvalidCastException();
	//        //return new Els
	//    }
	//}
	public static class IActionsMemberExtensions { } //Empty
	public static class ActSendMessageTypeExtensions //Not Implemented
	{
		//List<ExtensionBaseType> Items
		//Supports ActSendMessageType and ActSendReportType
		public static EmailAddressType AddEmail_(this ActSendMessageType smr)
		{ throw new NotImplementedException(); }
		public static PhoneNumberType AddFax_(this ActSendMessageType smr)
		{ throw new NotImplementedException(); }
		//public static CallFuncActionType AddWebService_(this ActSendMessageType smr)
		//{ throw new NotImplementedException(); }
	}
	public static class CallFuncBaseTypeExtensions //Not Implemented
	{
		//anyURI_Stype Item (choice)
		public static anyURI_Stype AddFunctionURI_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
		public static anyURI_Stype AddLocalFunctionName_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }

		//List<ExtensionBaseType> Items
		public static ListItemParameterType AddListItemParameterRef_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
		public static ParameterItemType AddParameterRef_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
		public static ParameterValueType AddParameterValue_(this CallFuncBaseType cfb)
		{ throw new NotImplementedException(); }
	}


	//public static class CallFuncActionTypeExtensions
	//{
	//    public static anyURI_Stype AddConnditionalActions_(this CallFuncActionType cfat)
	//    { throw new NotImplementedException(); }
	//}
	//public static class ScriptBoolFuncActionTypeExtensions
	//{
	//    //ExtensionBaseType[] Items 
	//    public static ActionsType AddActions_(this ScriptBoolFuncActionType sbfa)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddConditionalActions_(this ScriptBoolFuncActionType sbfa)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddElse_(this ScriptBoolFuncActionType sbfa)
	//    { throw new NotImplementedException(); }
	//} 
	//public static class CallFuncBoolActionTypeExtensions
	//{
	//    //ExtensionBaseType[] Items1
	//    //see IScriptBoolFuncAction, which is identical except that this interface implementation must use "Item1", not "Item"
	//    //Implementations using Item1:
	//    public static ActionsType AddActions_(this CallFuncBoolActionType cfba)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddConditionalActions_(this CallFuncBoolActionType cfba)
	//    { throw new NotImplementedException(); }
	//    public static PredActionType AddElse_(this CallFuncBoolActionType cfba)
	//    { throw new NotImplementedException(); }
	//}
	public static class IValidationTestsExtensions //Not Implemented
	{
		public static PredAlternativesType AddItemAlternatives_(this IValidationTests vt)
		{ throw new NotImplementedException(); }
		public static ValidationTypeSelectionSets AddSelectionSets_(this IValidationTests vt)
		{ throw new NotImplementedException(); }
		public static ValidationTypeSelectionTest AddSelectionTest_(this IValidationTests vt)
		{ throw new NotImplementedException(); }
	}
	public static class ICloneExtensions// Probably belongs on IBaseType 
	{
		public static BaseType CloneSubtree_(this IClone c, BaseType top)
		{ throw new NotImplementedException(); }
	}
	public static class IHtmlPackageExtensions //Not Implemented
	{
		public static base64Binary_Stype AddHTMLbase64_(this IHtmlPackage hp)
		{ throw new NotImplementedException(); }
	}
	public static class RegistrySummaryTypeExtensions //Not Implemented
	{
		//BaseType[] Items
		//Attach to Admin.RegistryData as OriginalRegistry and/or CurrentRegistry

		public static ContactType AddContact_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static FileType AddManual_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static string_Stype AddReferenceStandardIdentifier_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static InterfaceType AddRegistryInterfaceType_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static string_Stype AddRegistryName_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static FileType AddRegistryPurpose_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
		public static FileType AddServiceLevelAgreement_(this RegistrySummaryType rs)
		{ throw new NotImplementedException(); }
	}
	#endregion
}
