using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using UnityEngine;

namespace ES3Internal
{
	internal class ES3XMLWriter : ES3Writer
	{
		internal StreamWriter baseWriter;

		public ES3XMLWriter(Stream stream, ES3Settings settings) : this(stream, settings, true, true){}

		internal ES3XMLWriter(Stream stream, ES3Settings settings, bool writeHeaderAndFooter, bool overwriteKeys) : base(settings, writeHeaderAndFooter, overwriteKeys)
		{
			baseWriter = new StreamWriter(stream);
			StartWriteFile();
		}

		internal override void WritePrimitive(int value)		{ baseWriter.Write(value); }
		internal override void WritePrimitive(float value)	{ baseWriter.Write(value); }
		internal override void WritePrimitive(bool value)		{ baseWriter.Write(value); }
		internal override void WritePrimitive(char value)		{ baseWriter.Write(value); }
		internal override void WritePrimitive(decimal value)	{ baseWriter.Write(value); }
		internal override void WritePrimitive(double value)	{ baseWriter.Write(value); }
		internal override void WritePrimitive(long value)		{ baseWriter.Write(value); }
		internal override void WritePrimitive(ulong value)	{ baseWriter.Write(value); }
		internal override void WritePrimitive(uint value)		{ baseWriter.Write(value); }
		internal override void WritePrimitive(string value)	{ baseWriter.Write(value); }
		internal override void WritePrimitive(byte value)		{ baseWriter.Write(System.Convert.ToChar(value)); }
		internal override void WritePrimitive(sbyte value)	{ baseWriter.Write(System.Convert.ToChar(value)); }
		internal override void WritePrimitive(short value)	{ baseWriter.Write(System.Convert.ToString(value)); }
		internal override void WritePrimitive(ushort value)	{ baseWriter.Write(System.Convert.ToString(value)); }
		internal override void WritePrimitive(byte[] value)		{ WritePrimitive( System.Convert.ToBase64String(value) ); }

		internal override void WriteRawProperty(string name, byte[] bytes)
		{  
			StartWriteProperty(name); 
			baseWriter.Write(settings.encoding.GetString(bytes, 0, bytes.Length));
			EndWriteProperty(name);
		}

		internal override void StartWriteFile()
		{
		}

		internal override void EndWriteFile()
		{
		}

		internal override void StartWriteObject(string name)
		{
			StartWriteProperty(name);
		}

		internal override void EndWriteObject(string name)
		{
			EndWriteProperty(name);
		}

		internal override void StartWriteProperty(string name)
		{
			baseWriter.Write('<');
			baseWriter.Write(name);
			baseWriter.Write('>');
		}

		protected void StartWriteProperty(string name, ICollection<KeyValuePair<string,object>> attributes = null)
		{
			baseWriter.Write('<');
			baseWriter.Write(name);

			// Write attributes if there are any.
			if(attributes != null)
			{
				foreach(var kvp in attributes)
				{
					baseWriter.Write(' '); // Separate property name and attributes using space.
					baseWriter.Write(kvp.Key);
					baseWriter.Write('=');
					baseWriter.Write('\"');
					baseWriter.Write(kvp.Value);
					baseWriter.Write('\"');
				}
			}

			baseWriter.Write('>');
		}

		internal override void EndWriteProperty(string name)
		{
			baseWriter.Write("</");
			baseWriter.Write(name);
			baseWriter.Write('>');
		}

		internal override void StartWriteCollection(int length)
		{
			
		}

		internal override void EndWriteCollection()
		{
			
		}

		internal override void StartWriteCollectionItem(int index)
		{

		}

		internal override void EndWriteCollectionItem(int index)
		{

		}

		internal override void StartWriteDictionary(int length)
		{

		}

		internal override void EndWriteDictionary()
		{

		}

		internal override void StartWriteDictionaryKey(int index)
		{

		}

		internal override void EndWriteDictionaryKey(int index)
		{

		}

		internal override void StartWriteDictionaryValue(int index)
		{

		}

		internal override void EndWriteDictionaryValue(int index)
		{

		}

		internal override void WriteNull()
		{
			baseWriter.Write("null");
		}

		public override void Dispose()
		{
			baseWriter.Dispose();
		}
	}
}