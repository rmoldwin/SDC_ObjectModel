

//using SDC;
namespace SDC.Schema.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	public static class IResponseFieldExtensions
	{
		public static DataTypes_DEType AddDataType(this ResponseFieldType rf,
			ItemChoiceType dataType = ItemChoiceType.@string,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object? valDefault = null)
			=> IDataHelpers.AddDataTypesDE(rf, dataType, dtQuant, valDefault);  //Convert to generic type for valDefault

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
				rf.ResponseUnits ??= new(rf);
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
				rf.TextAfterResponse ??= new(rf);
				var rt = rf.TextAfterResponse;
				//rf.TextAfterResponse = rt;
				rt.val = asciiText;
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

			//use refelcction to retrieve @val here, instead of dynamic
			//var a = obj.GetType().Get;
			return false; // (val is not null && val != obj.GetType());
		}
	}




}
