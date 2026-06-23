using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDC.Schema.Extensions;
using System;
using System.Linq;

namespace SDC.Schema.Tests.OM
{
	/// <summary>
	/// Shared construction / navigation / round-trip helpers for the numeric ResponseType
	/// boundary tests. Used by both the unit-test file (<c>NumericResponseTypeBoundaryTests</c>)
	/// and the functional round-trip file (<c>NumericResponseTypeRoundTripTests</c>), so it lives
	/// in its own <c>*Helpers.cs</c> file per the project test-organization rules.
	/// </summary>
	internal static class NumericResponseTypeTestHelpers
	{
		/// <summary>Stable question ID used to locate the response datatype node after a round-trip.</summary>
		internal const string QId = "Q.Num";

		/// <summary>
		/// Builds a minimal <see cref="FormDesignType"/> tree containing a single QuestionResponse
		/// whose response field carries a datatype of <paramref name="ict"/>. Resets the static
		/// top-node state first so every test gets an isolated tree.
		/// </summary>
		internal static FormDesignType NewForm(ItemChoiceType ict, out QuestionItemType q, out DataTypes_DEType deType, string formId = "FD.Numeric")
		{
			BaseType.ResetLastTopNode();
			var fd = new FormDesignType(null, formId);
			fd.AddBody();
			q = fd.Body.AddChildQuestionResponse(QId, out deType, "Numeric question", dt: ict);
			return fd;
		}

		/// <summary>
		/// Builds a fresh form and returns the concrete <c>*_DEtype</c> response datatype node,
		/// already cast to <typeparamref name="T"/>, plus the owning form for serialization.
		/// </summary>
		internal static T DE<T>(ItemChoiceType ict, out FormDesignType fd) where T : BaseType
		{
			fd = NewForm(ict, out _, out var deType);
			return (T)deType.DataTypeDE_Item!;
		}

		/// <summary>
		/// Locates the response datatype node by question ID after a round-trip and casts it to
		/// <typeparamref name="T"/>. Uses <see cref="ITopNode.Nodes"/>, which is rebuilt by
		/// <c>ReflectRefreshTree</c> during deserialization.
		/// </summary>
		internal static T FindResponseDE<T>(FormDesignType fd, string qId = QId) where T : BaseType
		{
			var q = fd.Nodes.Values.OfType<QuestionItemType>().First(qi => qi.ID == qId);
			return (T)q.GetResponseDataTypeNode()!;
		}

		internal static string GetXml(FormDesignType fd) => TopNodeSerializer<FormDesignType>.GetXml(fd);
		internal static string GetJson(FormDesignType fd) => TopNodeSerializer<FormDesignType>.GetJson(fd);

		internal static FormDesignType XmlRoundTrip(FormDesignType fd)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromXml(TopNodeSerializer<FormDesignType>.GetXml(fd));

		internal static FormDesignType JsonRoundTrip(FormDesignType fd)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromJson(TopNodeSerializer<FormDesignType>.GetJson(fd));

		internal static FormDesignType BsonRoundTrip(FormDesignType fd)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromBson(TopNodeSerializer<FormDesignType>.GetBson(fd));

		internal static FormDesignType MsgPackRoundTrip(FormDesignType fd)
			=> TopNodeSerializer<FormDesignType>.DeserializeFromMsgPack(TopNodeSerializer<FormDesignType>.GetMsgPack(fd));

		/// <summary>
		/// Round-trips a <c>val</c> assignment through one serializer and asserts the deserialized
		/// value still satisfies <paramref name="matches"/>. <paramref name="setVal"/> mutates the
		/// freshly created node; <paramref name="roundTrip"/> selects XML/JSON/BSON/MsgPack.
		/// </summary>
		internal static void AssertValRoundTrip<T>(
			ItemChoiceType ict,
			Action<T> setVal,
			Func<T, bool> matches,
			Func<FormDesignType, FormDesignType> roundTrip,
			string because) where T : BaseType
		{
			var node = DE<T>(ict, out var fd);
			setVal(node);
			var fd2 = roundTrip(fd);
			var node2 = FindResponseDE<T>(fd2);
			Assert.IsTrue(matches(node2), because);
		}
	}
}
