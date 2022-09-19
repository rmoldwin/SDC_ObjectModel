

//using SDC;
namespace SDC.Schema
{
	public static class IAddContactExtensions
	{
		public static ContactType AddContact(this IAddContact ac, FileType ftParent, int insertPosition)
		{
			ContactsType c;
			if (ftParent.Contacts == null)
				c = ac.AddContactsListToFileType(ftParent);
			else
				c = ftParent.Contacts;
			var ct = new ContactType(c);
			var count = c.Contact.Count;
			if (insertPosition < 0 || insertPosition > count) insertPosition = count;
			c.Contact.Insert(insertPosition, ct);
			//TODO: Need to be able to add multiple people/orgs by reading the data source or ORM
			var p = (ac as IAddPerson)?.AddPerson(ct);
			var org = (ac as IAddOrganization)?.AddOrganization(ct);

			return ct;
		}
		private static ContactsType AddContactsListToFileType(this IAddContact ac, FileType ftParent)
		{
			if (ftParent.Contacts == null)
				ftParent.Contacts = new ContactsType(ftParent);

			return ftParent.Contacts; //returns a .NET List<ContactType>

		}
	}
}
