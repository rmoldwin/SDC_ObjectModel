

//using SDC;
namespace SDC.Schema
{
	public static class IAddPersonExtensions
	{
		internal static PersonType AddPerson(this IAddPerson ap, ContactType contactParent)
		{

			var newPerson = new PersonType(contactParent);
			contactParent.Person = newPerson;

			ap.AddPersonItems(newPerson);  //AddFillPersonItems?

			return newPerson;
		}
		internal static PersonType AddPerson(this IAddPerson ap, DisplayedType dtParent, int insertPosition)
		{
			List<ContactType> contactList;
			if (dtParent.Contact == null)
			{
				contactList = new List<ContactType>();
				dtParent.Contact = contactList;
			}
			else
				contactList = dtParent.Contact;
			var newContact = new ContactType(dtParent); //newContact will contain a person child
			var count = contactList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			contactList.Insert(insertPosition, newContact);

			var newPerson = ap.AddPerson(newContact);

			return newPerson;
		}
		internal static PersonType AddContactPerson(this IAddPerson ap, OrganizationType otParent, int insertPosition)
		{
			List<PersonType> contactPersonList;
			if (otParent.ContactPerson == null)
			{
				contactPersonList = new List<PersonType>();
				otParent.ContactPerson = contactPersonList;
			}
			else
				contactPersonList = otParent.ContactPerson;

			var newPerson = new PersonType(otParent);
			ap.AddPersonItems(newPerson);

			var count = contactPersonList.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			contactPersonList.Insert(insertPosition, newPerson);

			return newPerson;
		}
		internal static PersonType AddPersonItems(this IAddPerson ap, PersonType pt)  //AddFillPersonItems, make this abstract and move to subclass?
		{
			pt.PersonName = new NameType(pt);//TODO: Need separate method(s) for this
											 //pt.Alias = new NameType();
											 //pt.PersonName.FirstName.val = (string)drFormDesign["FirstName"];  //TODO: replace with real data
											 //pt.PersonName.LastName.val = (string)drFormDesign["LastName"];  //TODO: replace with real data

			pt.Email = new List<EmailType>();//TODO: Need separate method(s) for this
			var email = new EmailType(pt);//TODO: Need separate method(s) for this
			pt.Email.Add(email);

			pt.Phone = new List<PhoneType>();//TODO: Need separate method(s) for this
			pt.Job = new List<JobType>();//TODO: Need separate method(s) for this

			pt.Role = new string_Stype(pt);
			pt.Role.ElementName = "Role";

			pt.StreetAddress = new List<AddressType>();//TODO: Need separate method(s) for this
			pt.Identifier = new List<IdentifierType>();

			pt.Usage = new string_Stype(pt);
			pt.Usage.ElementName = "Usage";

			pt.WebURL = new List<anyURI_Stype>();//TODO: Need separate method(s) for this

			return pt;
		}
	}
}
