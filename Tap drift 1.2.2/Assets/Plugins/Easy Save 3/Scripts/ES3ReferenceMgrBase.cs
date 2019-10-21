using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ES3Internal
{
	[System.Serializable]
	[DisallowMultipleComponent]
	public abstract class ES3ReferenceMgrBase : MonoBehaviour
	{
		public const string referencePropertyName = "_ES3Ref";
		private static ES3ReferenceMgrBase _current = null;

		private static System.Random rng;

		[HideInInspector]
		public bool openReferences = false; // Whether the reference Dictionary should be open in the Editor.
		[HideInInspector]
		public bool openPrefabs = false; // Whether the prefab list should be open in the Editor.

		public static ES3ReferenceMgrBase Current
		{
			get
			{
				if(_current == null)
				{
					var mgrs = UnityEngine.Object.FindObjectsOfType<ES3ReferenceMgrBase>();
					if(mgrs.Length == 1)
						_current = mgrs[0];
					else if(mgrs.Length > 1)
						throw new InvalidOperationException("There is more than one ES3ReferenceMgr in this scene, but there must only be one.");
				}
				return _current; 
			}
		}

		public bool IsInitialised{ get{ return idRef.Count > 0; } }

		[SerializeField]
		public ES3IdRefDictionary idRef = new ES3IdRefDictionary();
		private ES3RefIdDictionary _refId = null;

		public ES3RefIdDictionary refId
		{
			get
			{
				if(_refId == null)
				{
					_refId = new ES3RefIdDictionary();
					// Populate the reverse dictionary with the items from the normal dictionary.
					foreach (var kvp in idRef)
						if(kvp.Value != null)
							_refId [kvp.Value] = kvp.Key;
				}
				return _refId;
			}
			set
			{
				_refId = value;
			}
		}

		public List<ES3Prefab> prefabs = new List<ES3Prefab>();

		public void Awake()
		{
			if(_current != null && _current != this)
			{
				_current.Merge(this);
				if(gameObject.name.Contains("Easy Save 3 Manager"))
					Destroy(this.gameObject);
				else
					Destroy(this);
			}
			else
				_current = this;
		}

		// Merges two managers, not allowing any clashes of IDs
		public void Merge(ES3ReferenceMgrBase otherMgr)
		{
			foreach(var kvp in otherMgr.idRef)
			{
				// Check for duplicate keys with different values.
				UnityEngine.Object value;
				if(idRef.TryGetValue(kvp.Key, out value))
				{
					if(value != kvp.Value)
						throw new ArgumentException ("Attempting to merge two ES3 Reference Managers, but they contain duplicate IDs. If you've made a copy of a scene and you're trying to load it additively into another scene, generate new reference IDs by going to Assets > Easy Save 3 > Generate New Reference IDs for Scene. Alternatively, remove the Easy Save 3 Manager from the scene if you do not intend on saving any data from it.");
				}
				else
					Add(kvp.Value, kvp.Key);
			}
		}

		public long Get(UnityEngine.Object obj)
		{
			long id;
			if(!refId.TryGetValue(obj, out id))
				return -1;
			return id;
		}

		public UnityEngine.Object Get(long id)
		{
			if(id == -1)
				return null;
			UnityEngine.Object obj;
			if(!idRef.TryGetValue(id, out obj))
				return null;
			return obj;
		}

		public ES3Prefab GetPrefab(long id)
		{
			for(int i=0; i<prefabs.Count; i++)
				if(prefabs[i] != null && prefabs[i].prefabId == id)
					return prefabs[i];
			return null;
		}

		public long GetPrefab(ES3Prefab prefab)
		{
			for(int i=0; i<prefabs.Count; i++)
				if(prefabs[i] == prefab)
					return prefabs[i].prefabId;
			return -1;
		}

		public long Add(UnityEngine.Object obj)
		{
			long id; 
			// If it already exists in the list, do nothing.
			if(refId.TryGetValue(obj, out id))
				return id;
			// Add the reference to the Dictionary.
			id = GetNewRefID();
			Add(obj, id);
			return id;
		}

		public void Add(UnityEngine.Object obj, long id)
		{
			// If the ID is -1, auto-generate an ID.
			if(id == -1)
				id = GetNewRefID();
			// Add the reference to the Dictionary.
			idRef[id] = obj;
			refId [obj] = id;
		}

		public void AddPrefab(ES3Prefab prefab)
		{
			if(!prefabs.Contains(prefab))
				prefabs.Add(prefab);
		}

		public void Remove(UnityEngine.Object obj)
		{
			long referenceID;

			// Get the reference ID, or do nothing if it doesn't exist.
			if(refId.TryGetValue(obj, out referenceID))
				return;
			
			refId.Remove(obj);
			idRef.Remove(referenceID);
		}

		public void Remove(long referenceID)
		{
			UnityEngine.Object obj;
			// Get the reference ID, or do nothing if it doesn't exist.
			if(!idRef.TryGetValue(referenceID, out obj))
				return;

			refId.Remove(obj);
			idRef.Remove(referenceID);
		}

		public void RemoveNullValues()
		{
			var nullKeys = idRef.Where(pair => pair.Value == null)
								.Select(pair => pair.Key).ToList();
			foreach (var key in nullKeys)
				idRef.Remove(key);
		}

		public void Clear()
		{
			refId.Clear();
			idRef.Clear();
		}

		public bool Contains(UnityEngine.Object obj)
		{
			return refId.ContainsKey(obj);
		}

		public bool Contains(long referenceID)
		{
			return idRef.ContainsKey(referenceID);
		}

		public void ChangeId(long oldId, long newId)
		{
			idRef.ChangeKey(oldId, newId);
			// Empty the refId so it has to be refreshed.
			refId = null;
		}

		internal static long GetNewRefID()
		{
			if(rng == null)
				rng = new System.Random();

			byte[] buf = new byte[8];
			rng.NextBytes(buf);
			long longRand = BitConverter.ToInt64(buf, 0);

			return (System.Math.Abs(longRand % (long.MaxValue - 0)) + 0);
		}
	}

	[System.Serializable]
	public class ES3IdRefDictionary : ES3SerializableDictionary<long, UnityEngine.Object>
	{
		protected override bool KeysAreEqual(long a, long b)
		{
			return a == b;
		}

		protected override bool ValuesAreEqual(UnityEngine.Object a, UnityEngine.Object b)
		{
			return a == b;
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	[System.Serializable]
	public class ES3RefIdDictionary : ES3SerializableDictionary<UnityEngine.Object, long>
	{
		protected override bool KeysAreEqual(UnityEngine.Object a, UnityEngine.Object b)
		{
			return a == b;
		}

		protected override bool ValuesAreEqual(long a, long b)
		{
			return a == b;
		}
	}
}