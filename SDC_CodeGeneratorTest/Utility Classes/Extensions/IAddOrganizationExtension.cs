using System;
using System.Linq;

namespace SDC.Schema
{
	public static class IAddOrganizationExtension
	{
		public static OrganizationType AddOganization_(this IAddOrganization ao)
		{ throw new NotImplementedException(); }

		public static OrganizationType AddOrganization(this IAddOrganization ao, ContactType contactParent)
		{
			var ot = new OrganizationType(contactParent);
			contactParent.Organization = ot;

			return ot;
		}
		public static OrganizationType AddOrganization(this IAddOrganization ao, JobType jobParent)
		{
			var ot = new OrganizationType(jobParent);
			jobParent.Organization = ot;

			return ot;
		}
		public static OrganizationType AddOrganizationItems_(this IAddOrganization ao, OrganizationType ot)
		{ throw new NotImplementedException(); }
	}  //Not Implemented
}
