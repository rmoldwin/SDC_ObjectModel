using System;
using System.Linq;

namespace SDC.Schema.Extensions
{
	public static class IAddOrganizationExtension
	{
		public static OrganizationType AddOganization_(this IAddOrganization ao)
		{ throw new NotImplementedException(); }

		public static OrganizationType AddOrganization(this IAddOrganization ao, ContactType contactParent, int insertPosition = -1)
		{
			var ot = new OrganizationType(contactParent, insertPosition, "Organization");
			return ot;
		}
		public static OrganizationType AddOrganization(this IAddOrganization ao, JobType jobParent, int insertPosition = -1)
		{
			var ot = new OrganizationType(jobParent, insertPosition, "Organization");
			return ot;
		}
		public static OrganizationType AddOrganizationItems_(this IAddOrganization ao, OrganizationType ot)
		{ throw new NotImplementedException(); }
	}  //Not Implemented
}
