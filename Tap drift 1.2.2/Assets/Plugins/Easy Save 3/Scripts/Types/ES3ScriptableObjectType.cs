using System;
using UnityEngine;
using System.Collections;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public abstract class ES3ScriptableObjectType : ES3UnityObjectType
	{
		public ES3ScriptableObjectType(Type type) : base(type) {}

		protected abstract void WriteScriptableObject(object obj, ES3Writer writer);
		protected abstract void ReadScriptableObject<T>(ES3Reader reader, object obj);

		protected override void WriteUnityObject(object obj, ES3Writer writer)
		{
			var instance = obj as ScriptableObject;
			if(obj != null && instance == null)
				throw new ArgumentException("Only types of UnityEngine.ScriptableObject can be written with this method, but argument given is type of "+obj.GetType());

			// If this object is in the instance manager, store it's instance ID with it.
			var refMgr = ES3ReferenceMgrBase.Current;
			if(refMgr != null)
				writer.WriteRef(instance);
			WriteScriptableObject(instance, writer);
		}

		protected override void ReadUnityObject<T>(ES3Reader reader, object obj)
		{
			ReadScriptableObject<T>(reader, obj);
		}

		protected override object ReadUnityObject<T>(ES3Reader reader)
		{
			var refMgr = ES3ReferenceMgrBase.Current;
			long id = -1;
			UnityEngine.Object instance = null;

			foreach(string propertyName in reader.Properties)
			{
				if(propertyName == ES3ReferenceMgrBase.referencePropertyName && refMgr != null)
				{
					id = reader.Read<long>(ES3Type_long.Instance);
					instance = refMgr.Get(id);

					if(instance != null)
						break;
				}
				else
				{
					reader.overridePropertiesName = propertyName;
					if(instance == null)
					{
						instance = ScriptableObject.CreateInstance(type);
						refMgr.Add(instance, id);
					}
					break;
				}
			}

			ReadScriptableObject<T>(reader, instance);
			return instance;
		}
	}
}