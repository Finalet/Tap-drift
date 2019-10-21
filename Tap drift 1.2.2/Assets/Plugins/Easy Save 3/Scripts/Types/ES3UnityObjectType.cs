using System;
using UnityEngine;
using System.Collections;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public abstract class ES3UnityObjectType : ES3ObjectType
	{
		public ES3UnityObjectType(Type type) : base(type)
		{
			this.isValueType = false;
			isES3TypeUnityObject = true;
		}

		protected abstract void WriteUnityObject(object obj, ES3Writer writer);
		protected abstract void ReadUnityObject<T>(ES3Reader reader, object obj);
		protected abstract object ReadUnityObject<T>(ES3Reader reader);

		protected override void WriteObject(object obj, ES3Writer writer)
		{
			WriteObject(obj, writer, ES3.ReferenceMode.ByRefAndValue);
		}

		public virtual void WriteObject(object obj, ES3Writer writer, ES3.ReferenceMode mode)
		{
			if(WriteUsingDerivedType(obj, writer))
				return;

			var instance = obj as UnityEngine.Object;
			if(obj != null && instance == null)
				throw new ArgumentException("Only types of UnityEngine.Object can be written with this method, but argument given is type of "+obj.GetType());

			// If this object is in the instance manager, store it's instance ID with it.
			var refMgr = ES3ReferenceMgrBase.Current;
			if(refMgr != null && mode != ES3.ReferenceMode.ByValue)
			{
				writer.WriteRef(instance);
				if(mode == ES3.ReferenceMode.ByRef)
					return;
			}
			WriteUnityObject(instance, writer);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			ReadUnityObject<T>(reader, obj);
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var refMgr = ES3ReferenceMgrBase.Current;
			if(refMgr == null)
				return ReadUnityObject<T>(reader);

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
						return ReadUnityObject<T>(reader);
					break;
				}
			}

			ReadUnityObject<T>(reader, instance);
			return instance;
		}
	}
}