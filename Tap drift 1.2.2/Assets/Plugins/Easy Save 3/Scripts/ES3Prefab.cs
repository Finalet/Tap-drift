using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ES3Internal;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ES3Internal
{
	public class ES3Prefab : MonoBehaviour 
	{
		public long prefabId = GetNewRefID();
		public ES3RefIdDictionary localRefs = new ES3RefIdDictionary();

		public void Awake()
		{
			// Add the references to the reference list when this prefab is instantiated.
			var mgr = ES3ReferenceMgrBase.Current;

			if(mgr == null)
				return;
			
			foreach(var kvp in localRefs)
				if(kvp.Key != null)
					mgr.Add(kvp.Key);
		}

		public long Get(UnityEngine.Object obj)
		{
			long id;
			if(localRefs.TryGetValue(obj, out id))
				return id;
			return -1;
		}

		public long Add(UnityEngine.Object obj)
		{
			long id;
			if(localRefs.TryGetValue(obj, out id))
				return id;
			id = GetNewRefID();
			localRefs.Add(obj, id);
			return id;
		}

		public Dictionary<long, long> GetReferences()
		{
			var localToGlobal = new Dictionary<long,long>();

			var refMgr = ES3ReferenceMgrBase.Current;

			if(refMgr == null)
				return localToGlobal;

			foreach(var kvp in localRefs)
			{
				long id = refMgr.Add(kvp.Key);
				localToGlobal.Add(kvp.Value, id);
			}
			return localToGlobal;
		}

		public void ApplyReferences(Dictionary<long, long> localToGlobal)
		{
			if(ES3ReferenceMgrBase.Current == null)
				return;

			foreach(var localRef in localRefs)
			{
				long globalId;
				if(localToGlobal.TryGetValue(localRef.Value, out globalId))
					ES3ReferenceMgrBase.Current.Add(localRef.Key, globalId);
			}
		}

		public static long GetNewRefID()
		{
			return ES3ReferenceMgrBase.GetNewRefID();
		}
#if UNITY_EDITOR
		public void GeneratePrefabReferences()
		{
			#if UNITY_2018_3_OR_NEWER
			var prefabType = PrefabUtility.GetPrefabInstanceStatus(this.gameObject);
			if(prefabType != PrefabInstanceStatus.NotAPrefab && prefabType != PrefabInstanceStatus.MissingAsset)
				return;
			#else
				var prefabType = PrefabUtility.GetPrefabType(this.gameObject);
				if(prefabType != PrefabType.Prefab && prefabType != PrefabType.MissingPrefabInstance)
					return;
			#endif

	
			// Get GameObject and it's children and add them to the reference list.
			foreach(var obj in EditorUtility.CollectDependencies(new UnityEngine.Object[]{this}))
			{
				if(obj == null || !ES3ReferenceMgr.CanBeSaved(obj))
					continue;
	
				if(this.Get(obj) == -1)
				{
					Undo.RecordObject(this, "Update Easy Save 3 Prefab");
					EditorUtility.SetDirty(this);
					Add(obj);
				}
			}
	    }
#endif
	}
}

/*
 * 	Create a blank ES3Type for ES3Prefab as it does not require serialising/deserialising when stored as a Component.
 */
namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public class ES3Type_ES3Prefab : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3Type_ES3Prefab() : base(typeof(ES3Prefab)){ Instance = this; }

		public override void Write(object obj, ES3Writer writer)
		{
		}

		public override object Read<T>(ES3Reader reader)
		{
			return null;
		}
	}

/*
 * 	Use this ES3Type to serialise the .
 */
	public class ES3Type_ES3PrefabInternal : ES3Type
	{
		public static ES3Type Instance = null;

		public ES3Type_ES3PrefabInternal() : base(typeof(ES3Type_ES3PrefabInternal)){ Instance = this; }

		public override void Write(object obj, ES3Writer writer)
		{
			ES3Prefab es3Prefab = (ES3Prefab)obj;

			writer.WriteProperty("prefabId", es3Prefab.prefabId, ES3Type_long.Instance);
			writer.WriteProperty("refs", es3Prefab.GetReferences());
		}

		public override object Read<T>(ES3Reader reader)
		{
			var prefabId = reader.ReadProperty<long>(ES3Type_long.Instance);
			var localToGlobal = reader.ReadProperty<Dictionary<long,long>>();

			if(ES3ReferenceMgrBase.Current == null)
				return null;

			var es3Prefab = ES3ReferenceMgrBase.Current.GetPrefab(prefabId);
			if(es3Prefab == null)
				throw new MissingReferenceException("Prefab with ID "+prefabId+" could not be found.");
			var instance = GameObject.Instantiate(es3Prefab.gameObject);
			var instanceES3Prefab = ((GameObject)instance).GetComponent<ES3Prefab>();
			if(instanceES3Prefab == null)
				throw new MissingReferenceException("Prefab with ID "+prefabId+" was found, but it does not have an ES3Prefab component attached.");

			instanceES3Prefab.ApplyReferences(localToGlobal);

			return instanceES3Prefab.gameObject;
		}
	}
}