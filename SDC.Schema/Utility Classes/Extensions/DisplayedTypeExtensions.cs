

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class DisplayedTypeExtensions
	{
		//LinkType, BlobType, ContactType, CodingType, EventType, OnEventType, PredGuardType
		public static BlobType AddBlob(this DisplayedType dtParent, int insertPosition = -1)
		{
			var blob = new BlobType(dtParent, insertPosition);
			return blob;
		}
		public static LinkType AddLink(this DisplayedType dtParent, int insertPosition = -1)
		{
			var link = new LinkType(dtParent, insertPosition, "Link");
			link.LinkText = new RichTextType(link, -1, "LinkText");
			link.LinkURI = new anyURI_Stype(link);
			return link;
		}
		public static ContactType AddContact(this DisplayedType dtParent, int insertPosition = -1)
		{
			var ct = new ContactType(dtParent, insertPosition, "Contact");
			return ct;
		}
		public static CodingType AddCodedValue(this DisplayedType dtParent, int insertPosition = -1)
		{
			var ct = new CodingType(dtParent, insertPosition, "CodedValue");
			return ct;
		}


		public static PredGuardType AddActivateIf(this DisplayedType dt)
		{
			var pg = new PredGuardType(dt, -1, "ActivateIf");
			pg.ElementPrefix = "actIf";
			return pg;
		}
		public static PredGuardType AddDeActivateIf(this DisplayedType dt)
		{
			var pg = new PredGuardType(dt, -1, "DeActivateIf");
			pg.ElementPrefix = "deActIf";
			return pg;
		}
		public static EventType AddOnEnter(this DisplayedType dt, int position = -1)
		{
			var ev = new EventType(dt, -1, "OnEnter");
			ev.ElementPrefix = "onEntr";
			return ev;
		}
		public static OnEventType AddOnEvent(this DisplayedType dt, int position = -1)
		{
			var oe = new OnEventType(dt, -1, "OnEvent");
			oe.ElementPrefix = "onEvnt";
			return oe;
		}
		public static EventType AddOnExit(this DisplayedType dt, int position = -1)
		{
			var oe = new EventType(dt, -1, "OnExit");
			oe.ElementName = "OnExit";
			oe.ElementPrefix = "onext";
			return oe;
		}
		public static bool MoveEvent_(this DisplayedType dt, EventType ev, List<EventType>? targetList = null, int index = -1)
		{
			throw new NotImplementedException();
		}
	}
}
