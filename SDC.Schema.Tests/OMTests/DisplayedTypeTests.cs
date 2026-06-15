using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;

namespace SDC.Schema.Tests.OMTests
{
	[TestClass()]
	public class DisplayedTypeTests
	{
		private static DisplayedType CreateDisplayedType()
		{
			var de = new DataElementType(null);
			return new DisplayedType(de, "di");
		}

		[TestMethod()]
		public void DisplayedTypeTest()
		{
			var sut = CreateDisplayedType();
			Assert.IsNotNull(sut);
		}

		[TestMethod()]
		public void AddLinkTest()
		{
			var sut = CreateDisplayedType();
			var link = sut.AddLink(1);
			Assert.IsNotNull(link);
			Assert.AreEqual(1, sut.Link.Count);
		}

		[TestMethod()]
		public void AddBlobTest()
		{
			var sut = CreateDisplayedType();
			var blob = sut.AddBlob(1);
			Assert.IsNotNull(blob);
			Assert.AreEqual(1, sut.BlobContent.Count);
		}

		[TestMethod()]
		public void AddContactTest()
		{
			var sut = CreateDisplayedType();
			var contact = sut.AddContact(1);
			Assert.IsNotNull(contact);
			Assert.AreEqual(1, sut.Contact.Count);
		}

		[TestMethod()]
		public void AddCodedValueTest()
		{
			var sut = CreateDisplayedType();
			var codedValue = sut.AddCodedValue(1);
			Assert.IsNotNull(codedValue);
			Assert.AreEqual(1, sut.CodedValue.Count);
		}

		[TestMethod()]
		public void AddOnEventTest()
		{
			var sut = CreateDisplayedType();
			var onEvent = sut.AddOnEvent();
			Assert.IsNotNull(onEvent);
		}

		[TestMethod()]
		public void AddOnEnterTest()
		{
			var sut = CreateDisplayedType();
			var onEnter = sut.AddOnEnter();
			Assert.IsNotNull(onEnter);
			Assert.AreEqual(1, sut.OnEnter.Count);
		}

		[TestMethod()]
		public void AddOnExitTest()
		{
			var sut = CreateDisplayedType();
			var onExit = sut.AddOnExit();
			Assert.IsNotNull(onExit);
			Assert.AreEqual(1, sut.OnExit.Count);
		}

		[TestMethod()]
		public void AddActivateIfTest()
		{
			var sut = CreateDisplayedType();
			var guard = sut.AddActivateIf();
			Assert.IsNotNull(guard);
		}

		[TestMethod()]
		public void AddDeActivateIfTest()
		{
			var sut = CreateDisplayedType();
			var guard = sut.AddDeActivateIf();
			Assert.IsNotNull(guard);
		}

		[TestMethod()]
		public void MoveEventTest()
		{
			var sut = CreateDisplayedType();
			var ev = sut.AddOnEnter();
			Assert.ThrowsException<System.NotImplementedException>(() => sut.MoveEvent_(ev));
		}
	}
}
