#pragma warning disable
//using System.Threading;

namespace SDC.Schema
{

	using System;
	using System.Diagnostics;
	using System.Xml.Serialization;
	//using System.Collections;
	//using System.Xml.Schema;
	//using System.ComponentModel;
	//using System.Collections.Specialized;
	//using System.Runtime.Serialization;
	//using System.Collections.ObjectModel;
	//using System.Reflection;
	using System.IO;
	//using System.Text;
	//using System.ComponentModel.DataAnnotations;
	using System.Xml;
	//using System.Collections.Generic;

	#region Base entity class
	public static class SdcSerializer<T> where T : IBaseType
	{
		private static XmlSerializer serializer;

		private static XmlSerializer Serializer
		{
			get
			{
				if ((serializer == null))
				{
					serializer = new XmlSerializerFactory().CreateSerializer(typeof(T));
					serializer.UnknownAttribute += Serializer_UnknownAttribute;
					serializer.UnknownElement += Serializer_UnknownElement;
					serializer.UnreferencedObject += Serializer_UnreferencedObject;
					serializer.UnknownNode += Serializer_UnknownNode;
				}
				return serializer;
			}
		}
		#region Serialize/Deserialize
		/// <summary>
		/// Serializes current T object into an XML string
		/// </summary>
		/// <returns>string XML value</returns>
		public static string Serialize(T obj, System.Text.Encoding encoding)
		{
			System.IO.StreamReader streamReader = null;
			System.IO.MemoryStream memoryStream = null;
			try
			{
				memoryStream = new System.IO.MemoryStream();
				System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
				xmlWriterSettings.Encoding = encoding;
				xmlWriterSettings.Indent = true;
				xmlWriterSettings.IndentChars = "  ";
				System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
				Serializer.Serialize(xmlWriter, obj);
				memoryStream.Seek(0, SeekOrigin.Begin);
				streamReader = new System.IO.StreamReader(memoryStream, encoding);
				return streamReader.ReadToEnd();
			}
			finally
			{
				if ((streamReader != null))
				{
					streamReader.Dispose();
				}
				if ((memoryStream != null))
				{
					memoryStream.Dispose();
				}
			}
		}

		public static string Serialize(T obj)
		{
			return Serialize(obj, System.Text.Encoding.UTF8);
		}

		/// <summary>
		/// Deserializes workflow markup into an EntityBase object
		/// </summary>
		/// <param name="input">string workflow markup to deserialize</param>
		/// <param name="obj">Output T object</param>
		/// <param name="exception">output Exception value if deserialize failed</param>
		/// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
		public static bool Deserialize(string input, out T obj, out System.Exception exception)
		{
			exception = null;
			obj = default(T);
			try
			{
				BaseType.ResetLastTopNode();
				obj = Deserialize(input);
				BaseType.ResetLastTopNode();
				return true;
			}
			catch (System.Exception ex)
			{
				exception = ex;
				return false;
			}
		}

		public static bool Deserialize(string input, out T obj)
		{
			System.Exception exception = null;
			BaseType.ResetLastTopNode();
			bool result =  Deserialize(input, out obj, out exception);
			BaseType.ResetLastTopNode();
			return result;
		}

		public new static T Deserialize(string input)
		{
			System.IO.StringReader stringReader = null;

			try
			{
				stringReader = new System.IO.StringReader(input);
				BaseType.ResetLastTopNode();
				T obj = ((T)(Serializer.Deserialize(XmlReader.Create(stringReader))));
				BaseType.ResetLastTopNode();
				return obj;
			}
			finally
			{
				if ((stringReader != null))
				{
					stringReader.Dispose();
				}
			}
		}

		public static T Deserialize(System.IO.Stream s)
		{
			BaseType.ResetLastTopNode();
			var obj = ((T)(Serializer.Deserialize(s)));
			BaseType.ResetLastTopNode();
			return obj;
		}
		#endregion

		///// <summary>
		///// Serializes current T object into file
		///// </summary>
		///// <param name="fileName">full path of outupt xml file</param>
		///// <param name="exception">output Exception value if failed</param>
		///// <returns>true if can serialize and save into file; otherwise, false</returns>
		//public static bool SaveToFile(string fileName, T obj, System.Text.Encoding encoding)
		//{
		//	//exception = null;
		//	try
		//	{
		//		SaveToFile(fileName, obj, encoding);
		//		return true;
		//	}
		//	catch (System.Exception e)
		//	{
		//		//exception = e;
		//		return false;
		//	}
		//}

		//public static bool SaveToFile(string fileName, T obj)
		//{
		//	return SaveToFile(fileName, obj, System.Text.Encoding.UTF8);
		//}


		public static bool SaveToFile(T obj, string fileName, System.Text.Encoding encoding, out System.Exception exception)
		{
			exception = null;
			try
			{
				SaveToFile(obj, fileName, encoding);
				return true;
			}
			catch (System.Exception e)
			{
				exception = e;
				return false;
			}
		}

		public static bool SaveToFile(T obj, string fileName, out System.Exception exception)
		{
			return SaveToFile(obj, fileName, System.Text.Encoding.UTF8, out exception);
		}

		public static void SaveToFile(T obj, string fileName)
		{
			SaveToFile(obj, fileName, System.Text.Encoding.UTF8);
		}

		public static void SaveToFile(T obj, string fileName, System.Text.Encoding encoding)
		{
			System.IO.StreamWriter streamWriter = null;
			try
			{
				string xmlString = Serialize(obj, encoding);
				streamWriter = new System.IO.StreamWriter(fileName, false, encoding);
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

		/// <summary>
		/// Deserializes xml markup from file into an EntityBase object
		/// </summary>
		/// <param name="fileName">string xml file to load and deserialize</param>
		/// <param name="obj">Output T object</param>
		/// <param name="exception">output Exception value if deserialize failed</param>
		/// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
		public static bool LoadFromFile(string fileName, System.Text.Encoding encoding, out T obj, out System.Exception exception)
		{
			exception = null;
			obj = default(T);
			try
			{
				obj = LoadFromFile(fileName, encoding);
				return true;
			}
			catch (System.Exception ex)
			{
				exception = ex;
				return false;
			}
		}

		public static bool LoadFromFile(string fileName, out T obj, out System.Exception exception)
		{
			return LoadFromFile(fileName, System.Text.Encoding.UTF8, out obj, out exception);
		}

		public static bool LoadFromFile(string fileName, out T obj)
		{
			System.Exception exception = null;
			return LoadFromFile(fileName, out obj, out exception);
		}

		public static T LoadFromFile(string fileName)
		{
			return LoadFromFile(fileName, System.Text.Encoding.UTF8);
		}

		public new static T LoadFromFile(string fileName, System.Text.Encoding encoding)
		{
			System.IO.FileStream file = null;
			System.IO.StreamReader sr = null;
			try
			{
				file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
				sr = new System.IO.StreamReader(file, encoding);
				string xmlString = sr.ReadToEnd();
				sr.Close();
				file.Close();
				return Deserialize(xmlString);
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
		#region Event Handlers
		private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
		{
			Console.WriteLine("UnknownNode: {0} as name {1} as localname, column {2} as lineposition", e.Name, e.LocalName, e.LinePosition);
		}

		private static void Serializer_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		{
			Console.WriteLine("Unreferenced Object: " + sender.ToString() + " : " + e.UnreferencedId);
		}

		private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
		{
			Console.WriteLine("Unexpected element: {0} as line {1}, column {2}", e.Element.Name, e.LineNumber, e.LinePosition);
		}

		private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			Console.WriteLine("Unexpected attribute: {0} as line {1}, column {2}", e.Attr.Name, e.LineNumber, e.LinePosition);
		}

		#endregion


	}
	#endregion
}
#pragma warning restore
