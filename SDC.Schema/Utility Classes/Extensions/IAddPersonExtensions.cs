

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class IAddPersonExtensions
	{
		internal static PersonType AddPerson(this IAddPerson ap, ContactType contactParent, int insertPosition = -1)
		{
			var newPerson = new PersonType(contactParent, insertPosition, "Person");
			ap.AddPersonItems(newPerson);  //AddFillPersonItems?

			return newPerson;
		}
		internal static PersonType AddPerson(this IAddPerson ap, DisplayedType dtParent, int insertPosition)
		{
			var contacts = new ContactsType(dtParent, "Contacts");
			var contact = new ContactType(contacts, insertPosition, "Contact");
			var person = new PersonType(contact, -1, "Person");

			return person;
		}
		internal static PersonType AddContactPerson(this IAddPerson ap, OrganizationType otParent, int insertPosition)
		{
			return new PersonType(otParent, insertPosition, "ContactPerson");
		}
		internal static PersonType AddPersonItems(this IAddPerson ap, PersonType pt)  //AddFillPersonItems, make this abstract and move to subclass?
		{
			pt.PersonName = new NameType(pt, -1, "PersonName");//TODO: Need separate method(s) for this
											 //pt.Alias = new NameType();
											 //pt.PersonName.FirstName.val = (string)drFormDesign["FirstName"];  //TODO: replace with real data
											 //pt.PersonName.LastName.val = (string)drFormDesign["LastName"];  //TODO: replace with real data

			pt.Email = new List<EmailType>();//TODO: Need separate method(s) for this
			var email = new EmailType(pt, -1);//TODO: Need separate method(s) for this
			pt.Email.Add(email);

			pt.Phone = new List<PhoneType>();//TODO: Need separate method(s) for this
			pt.Job = new List<JobType>();//TODO: Need separate method(s) for this

			pt.Role = new string_Stype(pt, -1, "Role");

			pt.StreetAddress = new List<AddressType>();//TODO: Need separate method(s) for this
			pt.Identifier = new List<IdentifierType>();

			pt.Usage = new string_Stype(pt, -1, "Usage");
			pt.WebURL = new List<anyURI_Stype>();//TODO: Need separate method(s) for this

			return pt;
		}
	}
}
