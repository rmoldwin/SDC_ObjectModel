

//using SDC;
namespace SDC.Schema
{
	public static class IResponseFieldExtensions
	{
		public static DataTypes_DEType AddDataType(this ResponseFieldType rf,
			ItemChoiceType dataType = ItemChoiceType.@string,
			dtQuantEnum dtQuant = dtQuantEnum.EQ,
			object valDefault = null)
			=> IDataHelpers.AddDataTypesDE(rf, dataType, dtQuant, valDefault);  //Convert to generic type for valDefault

		public static UnitsType? AddResponseUnits(this ResponseFieldType rf, string units)
		{
			if (rf != null && units != null)
			{
				var u = new UnitsType(rf);
				rf.ResponseUnits = u;
				u.val = units;
				return u;
			}
			return null;
		}

		public static RichTextType? AddTextAfterResponse(this ResponseFieldType rf, string taf)
		{
			if (rf != null && taf != null)
			{
				var rt = new RichTextType(rf);
				rf.TextAfterResponse = rt;
				return rt;
			}
			return null;
		}

		//public static CallFuncActionType AddCallSetValue_(this ResponseFieldType rf)
		//{ throw new NotImplementedException(); }
		public static ScriptCodeAnyType AddSetValue_(this ResponseFieldType rf)
		{ throw new NotImplementedException(); }
	}
}
