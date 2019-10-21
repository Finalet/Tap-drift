﻿using System;
using UnityEngine;
using System.Collections;
using ES3Internal;

namespace ES3Types
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	[UnityEngine.Scripting.Preserve]
	public abstract class ES3Type
	{
		public const string typeFieldName = "__type";

		public ES3Member[] members;
		public Type type;
		public bool isPrimitive = false;
		public bool isValueType = false;
		public bool isCollection = false;
		public bool isDictionary = false;
		public bool isES3TypeUnityObject = false;
		public bool isReflectedType = false;
		public bool isUnsupported = false;

		protected ES3Type(Type type)
		{
			ES3TypeMgr.Add(type, this);
			this.type = type;
			this.isValueType = ES3Reflection.IsValueType(type);
		}

		public abstract void Write(object obj, ES3Writer writer);
		public abstract object Read<T>(ES3Reader reader);

		public virtual void ReadInto<T>(ES3Reader reader, object obj)
		{
			throw new NotImplementedException("Self-assigning Read is not implemented or supported on this type.");
		}

		protected bool WriteUsingDerivedType(object obj, ES3Writer writer)
		{
			var objType = obj.GetType();
				
			if(objType != this.type)
			{
				writer.WriteType(objType);
				ES3TypeMgr.GetOrCreateES3Type(objType).Write(obj, writer);
				return true;
			}
			return false;
		}

		protected void ReadUsingDerivedType<T>(ES3Reader reader, object obj)
		{
			ES3TypeMgr.GetOrCreateES3Type(reader.ReadType()).ReadInto<T>(reader, obj);
		}

		internal string ReadPropertyName(ES3Reader reader)
		{
			if(reader.overridePropertiesName != null)
			{
				string propertyName = reader.overridePropertiesName;
				reader.overridePropertiesName = null;
				return propertyName;
			}
			return reader.ReadPropertyName();
		}
	
		#region Reflection Methods

		protected void WriteProperties(object obj, ES3Writer writer)
		{
			if(members == null)
				GetMembers(writer.settings.safeReflection);
			for(int i=0; i<members.Length; i++)
			{
				var property = members[i];
				writer.WriteProperty(property.name, property.reflectedMember.GetValue(obj), ES3TypeMgr.GetOrCreateES3Type(property.type), writer.settings.memberReferenceMode);
			}
		}

		protected object ReadProperties(ES3Reader reader, object obj)
		{
			// Iterate through each property in the file and try to load it using the appropriate
			// ES3Member in the members array.
			foreach(string propertyName in reader.Properties)
			{
				// Find the property.
				ES3Member property = null;
				for(int i=0; i<members.Length; i++)
				{
					if(members[i].name == propertyName)
					{
						property = members[i];
						break;
					}
				}

				if(property == null)
					reader.Skip();
				else
				{
					var type = ES3TypeMgr.GetOrCreateES3Type(property.type);

					if(ES3Reflection.IsAssignableFrom(typeof(IDictionary), property.type))
						property.reflectedMember.SetValue(obj, ((ES3DictionaryType)type).Read(reader));
					else if(ES3Reflection.IsAssignableFrom(typeof(ICollection), property.type))
						property.reflectedMember.SetValue(obj, ((ES3CollectionType)type).Read(reader));
					else
					{
						object readObj = reader.Read<object>(type);
						property.reflectedMember.SetValue(obj, readObj);
					}
				}
			}
			return obj;
		}

		protected void GetMembers(bool safe)
		{
			GetMembers(safe, null);
		}

		protected void GetMembers(bool safe, string[] memberNames)
		{
			var serializedMembers = ES3Reflection.GetSerializableMembers(type, safe, memberNames);
			members = new ES3Member[serializedMembers.Length];
			for(int i=0; i<serializedMembers.Length; i++)
				members[i] = new ES3Member(serializedMembers[i]);
		}

		#endregion

	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ES3PropertiesAttribute : System.Attribute 
	{
		public readonly string[] members;

		public ES3PropertiesAttribute(params string[] members)
		{
			this.members = members;
		}
	}
}
