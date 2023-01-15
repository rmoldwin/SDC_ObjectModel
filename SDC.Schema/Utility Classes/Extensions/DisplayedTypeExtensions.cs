

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class DisplayedTypeExtensions
	{//LinkType, BlobType, ContactType, CodingType, EventType, OnEventType, PredGuardType
		public static BlobType AddBlob(this DisplayedType dtParent, int insertPosition = -1)
		{
			var blob = new BlobType(dtParent);
			if (dtParent.BlobContent == null) dtParent.BlobContent = new List<BlobType>();
			var count = dtParent.BlobContent.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			dtParent.BlobContent.Insert(insertPosition, blob);
			return blob;
		}
		public static LinkType AddLink(this DisplayedType dtParent, int insertPosition = -1)
		{
			var link = new LinkType(dtParent);

			if (dtParent.Link == null) dtParent.Link = new List<LinkType>();
			var count = dtParent.Link.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			dtParent.Link.Insert(insertPosition, link);
			//link.order = link.ObjectID;

			var rtf = new RichTextType(link);
			link.LinkText = rtf;
			return link;
		}
		public static ContactType AddContact(this DisplayedType dtParent, int insertPosition = -1)
		{
			if (dtParent.Contact == null) dtParent.Contact = new List<ContactType>();
			var ct = new ContactType(dtParent);
			var count = dtParent.Contact.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			dtParent.Contact.Insert(insertPosition, ct);
			return ct;
		}
		public static CodingType AddCodedValue(this DisplayedType dtParent, int insertPosition = -1)
		{
			if (dtParent.CodedValue == null) dtParent.CodedValue = new List<CodingType>();
			var ct = new CodingType(dtParent);
			var count = dtParent.CodedValue.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			dtParent.CodedValue.Insert(insertPosition, ct);
			return ct;
		}


		public static PredGuardType AddActivateIf(this DisplayedType dt)
		{
			var pg = new PredGuardType(dt);
			pg.ElementPrefix = "ActivateIf";
			pg.ElementPrefix = "acif";
			dt.ActivateIf = pg;
			return pg;
		}
		public static PredGuardType AddDeActivateIf(this DisplayedType dt)
		{
			var pg = new PredGuardType(dt);
			pg.ElementPrefix = "DeActivateIf";
			pg.ElementPrefix = "deif";
			dt.DeActivateIf = pg;
			return pg;
		}
		public static EventType AddOnEnter(this DisplayedType dt)
		{
			var ev = new EventType(dt);
			ev.ElementName = "OnEnter";
			ev.ElementPrefix = "onen";
			dt.OnEnter ??= new ();
			dt.OnEnter.Add(ev);
			return ev;
		}
		public static OnEventType AddOnEvent(this DisplayedType dt)
		{
			var oe = new OnEventType(dt);
			oe.ElementName = "OnEvent";
			oe.ElementPrefix = "onev";
			dt.OnEvent ??= new();
			dt.OnEvent.Add(oe);
			return oe;
		}
		public static EventType AddOnExit(this DisplayedType dt)
		{
			var oe = new EventType(dt);
			oe.ElementName = "OnExit";
			oe.ElementPrefix = "onex";
			dt.OnEvent ??= new();
			dt.OnEnter.Add(oe);
			return oe;
		}
		public static bool MoveEvent_(this DisplayedType dt, EventType ev, List<EventType> targetList = null, int index = -1)
		{
			throw new NotImplementedException();
		}
	}
}
