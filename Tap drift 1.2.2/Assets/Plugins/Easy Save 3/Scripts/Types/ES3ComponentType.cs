using System;
using UnityEngine;
using System.Collections;
using ES3Internal;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public abstract class ES3ComponentType : ES3UnityObjectType
	{
		public ES3ComponentType(Type type) : base(type) {}

		protected abstract void WriteComponent(object obj, ES3Writer writer);
		protected abstract void ReadComponent<T>(ES3Reader reader, object obj);

		protected const string gameObjectPropertyName = "goID";

		protected override void WriteUnityObject(object obj, ES3Writer writer)
		{
			var instance = obj as Component;
			if(obj != null && instance == null)
				throw new ArgumentException("Only types of UnityEngine.Component can be written with this method, but argument given is type of "+obj.GetType());

			var refMgr = ES3ReferenceMgrBase.Current;

			if(refMgr != null)
			{
				// If this object is in the instance manager, store it's instance ID with it.
				writer.WriteRef(instance);
				// Write the reference of the GameObject so we know what one to attach it to.
				writer.WriteProperty(gameObjectPropertyName, refMgr.Add(instance.gameObject), ES3Type_long.Instance); 
			}
			WriteComponent(instance, writer);
		}

		protected override void ReadUnityObject<T>(ES3Reader reader, object obj)
		{
			ReadComponent<T>(reader, obj);
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
				else if(propertyName == gameObjectPropertyName && refMgr != null)
				{
					long goID = reader.Read<long>(ES3Type_long.Instance);
					var go = (GameObject)refMgr.Get(goID);

					if(go == null)
					{
						go = new GameObject("Easy Save 3 Loaded GameObject");
						#if UNITY_EDITOR
						go.AddComponent<ES3InspectorInfo>().SetMessage("This GameObject was created because Easy Save could not find a GameObject in the scene with the same instance ID as the GameObject the Component we are loading is attached to.\nTo prevent this from being created, use the LoadInto methods to tell Easy Save what Component the data should be loaded in to.");
						#endif
						refMgr.Add(go, goID);
					}
					instance = GetOrAddComponent(go, type);
					refMgr.Add(instance, id);
					break;
				}
				else
				{
					reader.overridePropertiesName = propertyName;
					if(instance == null)
					{
						var go = new GameObject("Easy Save 3 Loaded GameObject");
						#if UNITY_EDITOR
						go.AddComponent<ES3InspectorInfo>().SetMessage("This GameObject was created because Easy Save could not find a GameObject in the scene with the same instance ID as the GameObject the Component we are loading is attached to.\nTo prevent this from being created, use the LoadInto methods to tell Easy Save what Component the data should be loaded in to.");
						#endif
						instance = GetOrAddComponent(go, type);
						if(refMgr != null)
						{
							refMgr.Add(go);
							refMgr.Add(instance, id);
						}
					}
					break;
				}
			}
			ReadComponent<T>(reader, instance);
			return instance;
		}

		private static Component GetOrAddComponent(GameObject go, Type type)
		{
			if(type == typeof(Transform))
				return go.GetComponent(type);
			// Manage types which can only have a single Component attached.

			else if (type == typeof(MeshFilter) || type == typeof(MeshRenderer))
				return GetOrCreateComponentIfNotExists (go, type);
			return go.AddComponent(type);
		}

		public static Component CreateComponent(Type type)
		{
			GameObject go = new GameObject("Easy Save 3 Loaded Component");
			#if UNITY_EDITOR
			// If we're running in the Editor, add a description explaining why this object was created.
			go.AddComponent<ES3InspectorInfo>().SetMessage("This GameObject was created because Easy Save tried to load a Component with an instance ID which does not exist in this scene.\nTo prevent this from being created, use the LoadInto methods to tell Easy Save what Component the data should be loaded in to.\nThis can also happen if you load a class which references another object, but that object has not yet been loaded. In this case, you should load the object the class references before loading the class.");
			#endif
			if (type == typeof(Transform))
				return go.GetComponent (type);
			// Manage types which can only have a single Component attached.
			else if (type == typeof(MeshFilter) || type == typeof(MeshRenderer))
				return GetOrCreateComponentIfNotExists (go, type);
			return go.AddComponent(type);
		}

		// Creates a Component if one doesn't exist, or returns the existing instance.
		public static Component GetOrCreateComponentIfNotExists(GameObject go, Type type)
		{
			Component mf;
			if ((mf = go.GetComponent (type)) != null)
				return mf;
			return go.AddComponent(type);
		}	}
}