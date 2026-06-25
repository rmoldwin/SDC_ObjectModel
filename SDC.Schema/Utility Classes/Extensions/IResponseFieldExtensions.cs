

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
		/// Adds a typed response datatype node to the response field and optionally applies an initial value.
		/// </summary>
		/// <param name="rf">The response field that will own the datatype container.</param>
		/// <param name="dataType">The XSD-backed datatype to create. Defaults to <c>string</c>.</param>
		/// <param name="dtQuant">The comparison quantifier to store on datatypes that support one.</param>
		/// <param name="valDefault">Optional initial value to parse and validate for the created datatype.</param>
		/// <returns>The datatype container attached to <paramref name="rf"/>.</returns>
		/// <remarks>
		/// This is the public entry point for response-datatype construction. It delegates to
		/// <see cref="SdcDataTypeBuilder.AddDataTypesDE"/> which creates the concrete datatype node and
		/// routes invalid initial values through the validation soft-reject pipeline instead of throwing.
		/// </remarks>
		/// <seealso cref="SdcDataTypeBuilder.AddDataTypesDE"/>
		public static DataTypes_DEType AddDataType(this ResponseFieldType rf,
			ItemChoiceType dataType = ItemChoiceType.@string,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null)
			=> SdcDataTypeBuilder.AddDataTypesDE(rf, dataType, dtQuant, valDefault);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rf"></param>
		/// <param name="units"></param>
		/// <returns></returns>
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
		/// 
		/// </summary>
		/// <param name="rf"></param>
		/// <param name="asciiText"></param>
		/// <returns></returns>
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
