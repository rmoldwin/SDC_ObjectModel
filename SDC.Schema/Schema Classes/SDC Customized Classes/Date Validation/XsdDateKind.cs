// SDC-CUSTOM: do not overwrite. Hand-authored (not xsd2code++ generated).
namespace SDC.Schema
{
	/// <summary>
	/// Identifies which XML Schema date / date-part lexical space a value must conform to.
	/// Used by <see cref="XsdDateLexicalAttribute"/> and the date-validation rule registry to
	/// select the correct canonical pattern and to build an exceptionally helpful error message.
	/// </summary>
	public enum XsdDateKind
	{
		/// <summary><c>xs:date</c> — <c>CCYY-MM-DD</c> with an optional timezone.</summary>
		Date,
		/// <summary><c>xs:dateTime</c> — <c>CCYY-MM-DDThh:mm:ss</c> with an optional timezone.</summary>
		DateTime,
		/// <summary><c>xs:time</c> — <c>hh:mm:ss</c> with an optional timezone.</summary>
		Time,
		/// <summary><c>xs:dateTimeStamp</c> — an <c>xs:dateTime</c> that <b>requires</b> a timezone.</summary>
		DateTimeStamp,
		/// <summary><c>xs:gYear</c> — <c>CCYY</c> (4+ digits) with an optional timezone.</summary>
		GYear,
		/// <summary><c>xs:gYearMonth</c> — <c>CCYY-MM</c> with an optional timezone.</summary>
		GYearMonth,
		/// <summary><c>xs:gMonth</c> — <c>--MM</c> with an optional timezone.</summary>
		GMonth,
		/// <summary><c>xs:gMonthDay</c> — <c>--MM-DD</c> with an optional timezone.</summary>
		GMonthDay,
		/// <summary><c>xs:gDay</c> — <c>---DD</c> with an optional timezone.</summary>
		GDay,
		/// <summary><c>xs:duration</c> — <c>PnYnMnDTnHnMnS</c>, at least one component.</summary>
		Duration,
		/// <summary><c>xs:dayTimeDuration</c> — duration restricted to day + time parts.</summary>
		DayTimeDuration,
		/// <summary><c>xs:yearMonthDuration</c> — duration restricted to year + month parts.</summary>
		YearMonthDuration
	}
}
