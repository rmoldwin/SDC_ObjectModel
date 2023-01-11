using Newtonsoft.Json.Linq;
using System.Numerics;
//using static System.Runtime.InteropServices.JavaScript.JSType;  //C# 11 only

public static class ObjectExtensions
{
	public static bool IsGenericList(this object o)
	{
		try
		{
			return SDC.Schema.SdcUtil.IsGenericList(o);
		}
		catch { return false; }

	}
	/// <summary>
	/// Direct Cast as T, if possible
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="o"></param>
	/// <returns></returns>
	public static T As<T>(this object o) //where T : class
	{
		try
		{ return (T)o; }
		catch
		{ return default; }
	}

	/// <summary> Extension method that tries to convert the value of the source object 'oIn' to the type of 'oOut'.</summary>
	/// <typeparam name="Tout">The type of the returned object, after conversion.</typeparam>
	/// <param name="oIn">The inpput object whose type we want to convert to Tout.</param>
	/// <param name="oOut">If successful (i.e., the method returns true), then oOut is an object of type Tout, containing the value of oIn.  
	/// If not successful, the method returns false, and oOut contins the default value of Tout.</param>
	/// <param name="exOut">An Exception object from a failed type conversion, or null if no exception was generated</param>
	/// <param name="id">An optional identifier or text to help identify the context of any generated exception</param>
	/// <returns>Boolean true if the type conversion was successful. Otherwise false.</returns>
	public static bool TryAs<Tout>(this object oIn, out Tout? oOut, out Exception? exOut, string id = "") //where T : struct
	{
		try
		{
			exOut = null;
			if (oIn is null) throw new ArgumentNullException("oIn", "The input object 'oIn' was null");

			oOut = (Tout)Convert.ChangeType(oIn, typeof(Tout));
			//oOut = (T?)oIn;
			return true;
		}
		catch (Exception ex)
		{
			ex.Data.Add("Exception:", ex.Message);
			ex.Data.Add("id:", id);
			ex.Data.Add("Input:", oIn);
			ex.Data.Add("Input Type", oIn?.GetType().Name);
			ex.Data.Add("Output Type", typeof(Tout)?.Name);
			exOut = ex;
			oOut = default;
			return false;
		}
	}



	/// <summary>
	/// 
	/// 
	/// </summary>
	/// <typeparam name="Tout">The type of the returned object, after conversion.</typeparam>
	/// <param name="oIn">The input object whose type we want to convert to Tout</param>
	/// <param name="exOut">An Exception object from a failed type conversion, or null if no exception was generated</param>
	/// <param name="exList">An optional supplied IList object used to log exceptions, without interupting the code flow</param>
	/// <param name="id">An optional identifier or text to help identify the context of any generated exception</param>
	/// <returns>An object of type Tout.  
	/// If the conversion failed, exOut will not be null, 
	/// exList will contain the exception as its last item, 
	/// and the return value will be the default value of Tout</returns>
	public static Tout? TryAs2<Tout>(this object oIn, out Exception? exOut, IList<Exception>? exList = null, string id = "")
	{
		try
		{
			exOut = null;
			if (oIn is null) return default;

			var oOut = (Tout)Convert.ChangeType(oIn, typeof(Tout));
			//var oOut = (Tout)oIn;

			return oOut;
		}
		catch (Exception ex)
		{
			exOut = ex;
			exList?.Add(ex);
			ex.Data.Add("Exception:", ex.Message);
			ex.Data.Add("id:", id);
			ex.Data.Add("Input:", oIn);
			ex.Data.Add("Input Type", oIn?.GetType().Name);
			ex.Data.Add("Output Type", typeof(Tout)?.Name);
			return default;
		}
	}

	public static bool TryAssign<Tout>(this object oIn,
		ref Tout? objToAssign,
		out Exception exOut,
		IList<Exception>? exList = null,
		string id = "")
	{
		try
		{
			exOut = null!;

			if (oIn == null && default(Tout) != null)
			{
				Console.WriteLine("TryAssign: No assignment made. Cannot set the non-nullable objToAssign to null");
				return false;
			}  //Cannot set a non-nullable object to null, so we do nothing and return

			if (oIn == null && objToAssign != null)
			{
				Console.WriteLine("TryAssign: Assigned null to objToAssign");
				objToAssign = default!; return true;  //TryAssign can return null
			} //This will assign null to objToAssign, if objToAssign is nullable


			var oOut = (Tout?)Convert.ChangeType(oIn, typeof(Tout?));
			//display("base type name: " + typeof(Tout).BaseType.Name);
			if ( //Do not assign null to a ValueType
					oOut is null && default(Tout?) != null
					||
					typeof(Tout).BaseType?.Name == "ValueType"
					&& objToAssign != null
					&& oOut != null
					&& objToAssign.Equals(oOut)//don't make assignment if the value will not change
											   //Should we handle ref types with equal values? (string, xml, html, byte[] (e.g., base64) etc)
				)
			{
				Console.WriteLine("TryAssign: objToAssign value was not changed");
				return false;
			} //Do not change the assigned value
			  //display(oOut.ToString());

			objToAssign = oOut;
			return true;
		}
		catch (Exception ex)
		{
			exOut = ex;
			exList?.Add(ex);
			ex.Data.Add("Exception:", ex.Message);
			ex.Data.Add("id:", id);
			ex.Data.Add("Input:", oIn);
			ex.Data.Add("Input Type", oIn?.GetType().Name);
			ex.Data.Add("Output Type", typeof(Tout)?.Name);
			return false;
		}
	}


	//public static bool IsNumeric(this object obj)
	//{
	//	switch (obj)
	//	{
	//		case Byte:
	//		case SByte:
	//		case UInt16:
	//		case UInt32:
	//		case UInt64:
	//		case Int16:
	//		case Int32:
	//		case Int64:
	//		case Single:
	//		case Decimal:
	//		case Double:

	//			return true;
	//		default:
	//			return false;
	//	}
	//}


	public static bool IsNumeric(this ValueType value)
	{
		return (value is Byte ||
				value is Int16 ||
				value is Int32 ||
				value is Int64 ||
				value is SByte ||
				value is UInt16 ||
				value is UInt32 ||
				value is UInt64 ||
				value is BigInteger ||
				value is Decimal ||
				value is Double ||
				value is Single);
	}


	public static bool IsNumeric(this object value)
	{
		return (value is Byte ||
				value is Int16 ||
				value is Int32 ||
				value is Int64 ||
				value is SByte ||
				value is UInt16 ||
				value is UInt32 ||
				value is UInt64 ||
				value is BigInteger ||
				value is Decimal ||
				value is Double ||
				value is Single);
	}

	public static bool IsInteger(this ValueType value)
	{
		return (value is SByte || value is Int16 || value is Int32
				|| value is Int64 || value is Byte || value is UInt16
				|| value is UInt32 || value is UInt64
				|| value is BigInteger);
	}
	public static bool IsInteger(object value)
	{
		return (value is SByte || value is Int16 || value is Int32
				|| value is Int64 || value is Byte || value is UInt16
				|| value is UInt32 || value is UInt64
				|| value is BigInteger);
	}
	public static bool IsFloat(this ValueType value)
	{
		return (value is float | value is double | value is decimal);
	}
	public static bool IsFloat(object value)
	{
		return (value is float | value is double | value is decimal);
	}

	public enum NumericRelationship
	{
		GreaterThan = 1,
		EqualTo = 0,
		LessThan = -1
	};
	public static NumericRelationship Compare(ValueType value1, ValueType value2)
	{
		if (!IsNumeric(value1))
			throw new ArgumentException($"{nameof(value1)} is not a number.");
		if (!IsNumeric(value2))
			throw new ArgumentException($"{nameof(value2)} is not a number.");

		// Use long as common integral type
		if (IsInteger(value1) && IsInteger(value2))
		{
			long long1 = (long)value1;
			long long2 = (long)value2;
			return (NumericRelationship)BigInteger.Compare(long1, long2);
		}
		// At least one value is floating point; use Double.
		else
		{
			Double dbl1 = 0;
			Double dbl2 = 0;
			try
			{
				dbl1 = Convert.ToDouble(value1);
			}
			catch (OverflowException)
			{
				Console.WriteLine($"{nameof(value1)} is outside the range of a Double.");
			}
			try
			{
				dbl2 = Convert.ToDouble(value2);
			}
			catch (OverflowException)
			{
				Console.WriteLine($"{nameof(value2)} is outside the range of a Double.");
			}
			return (NumericRelationship)dbl1.CompareTo(dbl2);
		}
	}
	public static NumericRelationship Compare(BigInteger bigint1, ValueType value2)
	{
		if (!IsNumeric(value2)) throw new ArgumentException($"{nameof(value2)} is not a number.");
		// Use BigInteger as common integral type
		var bigint2 = (BigInteger)value2;
		return (NumericRelationship)BigInteger.Compare(bigint1, bigint2);
	}
	public static NumericRelationship Compare(ValueType value1, BigInteger bigint2)
	{
		if (!IsNumeric(value1)) throw new ArgumentException($"{nameof(value1)} is not a number.");
		return (NumericRelationship) (-1 * (int)Compare(bigint2, value1));
	}

	//https://learn.microsoft.com/en-us/dotnet/api/system.valuetype?view=net-7.0

}
