// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 5.1.1.0. www.xsd2code.com
//  </auto-generated>
//  Extensively modified rlm 2020_05_11
// ------------------------------------------------------------------------------
#pragma warning disable
namespace SDC.Schema
{
	using System;
	using System.Diagnostics;
	using System.Xml.Serialization;
	using System.Collections;
	using System.Xml.Schema;
	using System.ComponentModel;
	using Newtonsoft.Json;
	using J = System.Text.Json;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	public static partial class SdcSerializerJson<T> where T : ITopNode

	{

		#region Serialize/Deserialize
		/// <summary>
		/// Serializes current EntityBase object into an json string
		/// </summary>
		public static string SerializeJson<T>(T obj)
		{
			//return J.JsonSerializer.Serialize<T>(obj);
			return JsonConvert.SerializeObject(obj);
		}

		/// <summary>
		/// Deserializes workflow markup into an EntityBase object
		/// </summary>
		/// <param name="input">string workflow markup to deserialize</param>
		/// <param name="obj">Output EntityBase object</param>
		/// <param name="exception">output Exception value if deserialize failed</param>
		/// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
		public static bool DeserializeJson<T>(string input, out T obj, out System.Exception exception)
		{
			exception = null;
			obj = default(T);
			try
			{
				obj = DeserializeJson<T>(input);
				//obj = J.JsonSerializer.Deserialize<T>(input);  //System.Text.Json
				return true;
			}
			catch (System.Exception ex)
			{
				exception = ex;
				return false;
			}
		}

		public static bool DeserializeJson<T>(string input, out T obj)
		{
			System.Exception exception = null;
			return DeserializeJson<T>(input, out obj, out exception);
		}

		public static T DeserializeJson<T>(string input)
		{
			//return J.JsonSerializer.Deserialize<T>(input); //System.Text.Json
			return JsonConvert.DeserializeObject<T>(input);
		}
		#endregion

		public static void SaveToFileJson<T>(string fileName, T obj)
		{
			System.IO.StreamWriter streamWriter = null;
			try
			{
				string xmlString = SerializeJson<T>(obj);
				System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
				streamWriter = xmlFile.CreateText();
				streamWriter.WriteLine(xmlString);
				streamWriter.Close();
			}
			finally
			{
				if ((streamWriter != null))
				{
					streamWriter.Dispose();
				}
			}
		}

		public static T LoadFromFileJson(string fileName)
		{
			System.IO.FileStream file = null;
			System.IO.StreamReader sr = null;
			try
			{
				file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
				sr = new System.IO.StreamReader(file);
				string xmlString = sr.ReadToEnd();
				sr.Close();
				file.Close();

				BaseType.ResetLastTopNode();
				T output = DeserializeJson<T>(xmlString);
				BaseType.ResetLastTopNode();
				return output;
			}
			finally
			{
				if ((file != null))
				{
					file.Dispose();
				}
				if ((sr != null))
				{
					sr.Dispose();
				}
			}
		}
	}

}
#pragma warning restore
