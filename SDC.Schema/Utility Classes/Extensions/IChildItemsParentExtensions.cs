

//using SDC;
using System.Collections.ObjectModel;

namespace SDC.Schema.Extensions
{
    public static class IChildItemsParentExtensions
    {
        public static SectionItemType AddChildSection(this IChildItemsParent parent, string id, string? defTitle = null, int insertPosition = -1)
        {
            var childItems = parent.GetChildItemsNode();
            var sNew = new SectionItemType(childItems, id);
            sNew.title = defTitle;

            return sNew;
        }
        /// <summary>
        /// Note: Any QuestionResponse (QF/QR) will be created with default values and a string data type.<br/>
        /// Use AddChildQuestionResponse for more control over QR creation.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="qType"></param>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="insertPosition"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static QuestionItemType AddChildQuestion(this IChildItemsParent parent, QuestionEnum qType, string id, string title = null, int insertPosition = -1)
        //where T : BaseType, IChildItemsParent<T>
        {
            var childItems = parent.GetChildItemsNode();
            var qNew = new QuestionItemType(childItems, id);

            switch (qType)
            {
                case QuestionEnum.QuestionSingle:
                    qNew.GetListField().GetList();
                    break;
                case QuestionEnum.QuestionMultiple:
                    qNew.GetListField().GetList();
                    qNew.ListField_Item!.maxSelections = 0;
                    break;
                case QuestionEnum.QuestionFill:
                    qNew.AddQuestionResponseField(out DataTypes_DEType dtDE);
                    break;
                case QuestionEnum.QuestionLookupSingle:
                    qNew.GetListField().GetLookupEndpoint();
                    break;
                case QuestionEnum.QuestionLookupMultiple:
                    qNew.GetListField().GetLookupEndpoint();

                    break;
                default:
                    throw new NotSupportedException($"{qType} is not supported");
            }
            qNew.title = title;
            return qNew;
        }

        public static QuestionItemType AddChildQuestionResponse(this IChildItemsParent parent,
            string id,
            out DataTypes_DEType deType,
            string? defTitle = null,
            int insertPosition = -1,
            ItemChoiceType dt = ItemChoiceType.@string,
            string? textAfterResponse = null,
            string? units = null,
            dtQuantEnum dtQuant = dtQuantEnum.EQ,
            object? valDefault = null) //where T : BaseType, IChildItemsParent<T>
        {
            var childItems = parent.GetChildItemsNode();
            var qNew = new QuestionItemType(childItems, id);
            if (!string.IsNullOrWhiteSpace(defTitle)) qNew.title = defTitle;

            var rf = qNew.AddQuestionResponseField(out deType, dt, dtQuant, valDefault);
            if (!string.IsNullOrWhiteSpace(units)) rf.AddResponseUnits(units);
            if (!string.IsNullOrWhiteSpace(textAfterResponse)) rf.AddTextAfterResponse(textAfterResponse);

            return qNew;

        }
        public static DisplayedType AddChildDisplayedItem(this IChildItemsParent parent, string id, string defTitle = null, int insertPosition = -1)
        //where T : BaseType, IChildItemsParent<T>
        {
            var childItems = parent.GetChildItemsNode();
            var dNew = new DisplayedType(childItems, id);
            dNew.title = defTitle;

            return dNew;
        }
        public static ButtonItemType AddChildButtonAction(this IChildItemsParent parent, string id, string defTitle = null, int insertPosition = -1)
        //where T : BaseType, IChildItemsParent<T>
        {
            var childItems = parent.GetChildItemsNode();
            var btnNew = new ButtonItemType(childItems, id);
            btnNew.title = defTitle;

            // TODO: Add AddButtonActionTypeItems(btnNew);
            return btnNew;
        }
        public static InjectFormType AddChildInjectedForm(this IChildItemsParent parent, string id, int insertPosition = -1)
        //where T : BaseType, IChildItemsParent<T>
        {
            var childItems = parent.GetChildItemsNode();
            var injForm = new InjectFormType(childItems, id);

            //TODO: init this InjectForm object
            return injForm;
        }
        /// <summary>
        /// Return members of the ChildItems node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childNodes"></param>
        /// <returns></returns>
        public static bool TryGetChildItemsList(this IChildItemsParent parent, out ReadOnlyObservableCollection<IdentifiedExtensionType>? childNodes)
        //where T : BaseType, IChildItemsParent<T>
        {
            childNodes = null;
            if (parent is null) return false;
            var oc = new ObservableCollection<IdentifiedExtensionType>(parent.ChildItemsNode.ChildItemsList);
            if (oc is null || !oc.Any()) return false;

            childNodes = new ReadOnlyObservableCollection<IdentifiedExtensionType>(oc);

            if (childNodes.Count > 0) return true;
            return false;
        }
        /// <summary>
        /// Retrieve all IdentifiedExtensionType nodes subsumed under the ChildItems node. <br/>
        /// This does not include ListItems and DisplayedType nodes under a Question's List node
        /// </summary>
        /// <returns>ImmutableList&lt;IdentifiedExtensionType> or null if the ChildItems node is null or has no descendants </returns>
        public static ReadOnlyObservableCollection<IdentifiedExtensionType>? GetChildItemsList(this IChildItemsParent parent)
        {
            if (parent is null || parent.ChildItemsNode is null || parent.ChildItemsNode.ChildItemsList is null) return null;
            var oc = new ObservableCollection<IdentifiedExtensionType>(parent.ChildItemsNode.ChildItemsList);
            if (oc is null || !oc.Any()) return null;
            return new ReadOnlyObservableCollection<IdentifiedExtensionType>(oc);
        }
        /// <summary>
        /// Add a <see cref="ChildItemsType"/> node to the parent object.<br/>
        /// If a <see cref="ChildItemsType"/> node already exists, the existing node is returned.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ChildItemsType GetChildItemsNode(this IChildItemsParent parent)
        {
            if (parent.ChildItemsNode is not null) return parent.ChildItemsNode;
            return new ChildItemsType(parent);
        }
    }
}
