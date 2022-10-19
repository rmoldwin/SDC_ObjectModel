﻿

//using SDC;
namespace SDC.Schema.Extensions
{
	public static class FormDesignTypeExtensions
	{
		//Default Implementations
		public static SectionItemType AddHeader(this FormDesignType fd)
		{
			// var fd = (ifd as FormDesignType);
			if (fd.Header == null)
			{
				fd.Header = new SectionItemType(fd, fd.ID + "_Header");  //Set a default ID, in case the database template does not have a body
				fd.Header.name = "Header";
			}
			return fd.Header;
		}
		public static SectionItemType AddBody(this FormDesignType fd)
		{
			//var fd = (ifd as FormDesignType);
			if (fd.Body == null)
			{
				fd.Body = new SectionItemType(fd, fd.ID + "_Body");  //Set a default ID, in case the database template does not have a body
				fd.Body.name = "Body";
			}
			return fd.Body;
		}
		public static SectionItemType AddFooter(this FormDesignType fd)
		{
			//var fd = (ifd as FormDesignType);
			if (fd.Footer == null)
			{
				fd.Footer = new SectionItemType(fd, fd.ID + "_Footer");  //Set a default ID, in case the database template does not have a body
				fd.Footer.name = "Footer";
			}
			return fd.Footer;
		}

		/// <summary>
		/// Not yet implemented.  Throws NotImplementedException.
		/// </summary>
		/// <param name="fd"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public static RulesType AddRule_(this FormDesignType fd)
		{ throw new NotImplementedException(); }

	}
}
