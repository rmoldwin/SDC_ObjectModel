

//using SDC;
namespace SDC.Schema.Extensions
{
	/// <summary>
	/// Extension methods providing the public API for adding typed SDC response datatypes,
	/// units, and trailing text to a <see cref="ResponseFieldType"/> node.
	/// </summary>
	public static class IResponseFieldExtensions
	{
		/// <summary>
	/// Adds a typed SDC response datatype to this <see cref="ResponseFieldType"/>.
	/// Delegates to <see cref="SdcDataTypeBuilder.AddDataTypesDE"/> which handles
	/// parsing, validation, and construction in one call.
	/// </summary>
	/// <param name="rf">The response field to attach the datatype to.</param>
	/// <param name="dataType">The XSD datatype to create (default: <c>string</c>).</param>
	/// <param name="dtQuant">The quantifier (default: EQ).</param>
	/// <param name="valDefault">Optional initial value; parsed and validated against the target type.</param>
	/// <returns>The constructed <see cref="DataTypes_DEType"/> node.</returns>
	public static DataTypes_DEType AddDataType(this ResponseFieldType rf,
			ItemChoiceType dataType = ItemChoiceType.@string,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null)
			=> SdcDataTypeBuilder.AddDataTypesDE(rf, dataType, dtQuant, valDefault);  //Convert to generic type for valDefault

	/// <summary>
	/// Adds or updates the <see cref="UnitsType"/> on this <see cref="ResponseFieldType"/>
	/// with the specified units string.
	/// </summary>
	/// <param name="rf">The response field to attach units to.</param>
	/// <param name="units">The units label (e.g., <c>"mg/dL"</c>, <c>"years"</c>).</param>
	/// <returns>
	/// The <see cref="UnitsType"/> node set on <paramref name="rf"/>, or
	/// <see langword="null"/> if either argument is null.
	/// </returns>
	public static UnitsType? AddResponseUnits(this ResponseFieldType rf, string units)
		{
			if (rf is not null && units is not null)
			{
				rf.ResponseUnits ??= new(rf, "ResponseUnits" );
				//var u = new UnitsType(rf);
				//rf.ResponseUnits = u;
				rf.ResponseUnits.val = units;
				return rf.ResponseUnits;
			}
			return null;
		}

	/// <summary>
	/// Adds or updates the <c>TextAfterResponse</c> rich-text node on this
	/// <see cref="ResponseFieldType"/> with the supplied plain-text string.
	/// </summary>
	/// <param name="rf">The response field to attach text to.</param>
	/// <param name="asciiText">The plain-text label to display after the response field.</param>
	/// <returns>
	/// The <see cref="RichTextType"/> node set on <paramref name="rf"/>, or
	/// <see langword="null"/> if either argument is null.
	/// </returns>
	public static RichTextType? AddTextAfterResponse(this ResponseFieldType rf, string? asciiText)
		{
			if (rf is not null && asciiText is not null)
			{
				rf.TextAfterResponse ??= new(rf, -1, "TextAfterResponse");
				var rt = rf.TextAfterResponse;
				//rf.TextAfterResponse = rt;
				if(!string.IsNullOrWhiteSpace(asciiText)) rt.val = asciiText;
				return rt;
			}
			return null;
		}

		//public static CallFuncActionType AddCallSetValue_(this ResponseFieldType rf)
		//{ throw new NotImplementedException(); }
		private static ScriptCodeAnyType AddSetValue_(this ResponseFieldType rf)
		{ throw new NotImplementedException(); }

		private static T? GetVal<T>(this ResponseFieldType rf) where T : BaseType, IVal
		{//TODO: needs work!

			T? val = default(T);
			val = rf.Response?.Item as T;			

			return val;
		}

		private static bool HasVal<T>(this ResponseFieldType rf, out IVal? val)
		{//TODO: needs work!

			var item = rf.Response?.Item;
			val = null;

			//use reflection to retrieve @val here, instead of dynamic
			//var a = obj.GetType().Get;
			return false; // (val is not null && val != obj.GetType());
		}
	}




}
