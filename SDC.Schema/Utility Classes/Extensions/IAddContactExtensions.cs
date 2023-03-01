

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class IAddContactExtensions
	{
		public static ContactType AddContact(this IAddContact ac, FileType ftParent, int insertPosition)
		{
			ContactsType c = ac.GetContactsListToFileType(ftParent);
			var ct = new ContactType(c, insertPosition, "Contact");

			//TODO: Need to be able to add multiple people/orgs by reading the data source or ORM
			var p = (ac as IAddPerson)?.AddPerson(ct, -1); //add to last position
			var org = (ac as IAddOrganization)?.AddOrganization(ct, -1); //add to last position

			return ct;
		}
		private static ContactsType GetContactsListToFileType(this IAddContact ac, FileType ftParent)
		{
			if (ftParent.Contacts == null)
				ftParent.Contacts = new ContactsType(ftParent, "Contacts");

			return ftParent.Contacts; //returns a .NET List<ContactType>

		}
	}
}
