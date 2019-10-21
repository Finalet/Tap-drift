#if !UNITY_4
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ES2AutoSaveManager : MonoBehaviour
{
	public enum LoadEvent{ Start, OnApplicationUnPause, /*OnLevelWasLoaded,*/ None };
	public enum SaveEvent{ OnApplicationQuit, OnApplicationPause, OnDisable, None };
	public LoadEvent loadEvent = LoadEvent.Start; 
	public SaveEvent saveEvent = SaveEvent.OnApplicationQuit;

	// Settings
	public string filePath = "AutoSave.es2";
	public new string tag = "default";
	public bool encrypt = ES2GlobalSettings.defaultEncrypt;
	public string encryptionPassword = ES2GlobalSettings.defaultEncryptionPassword;
	public ES2Settings.EncryptionType encryptionType = ES2GlobalSettings.defaultEncryptionType;
	public bool deletePrefabsOnLoad = false;
	public bool convertPrefabsToSceneObjectsOnImport = true;


	public ES2AutoSave[] sceneObjects = new ES2AutoSave[0];
	private Dictionary<string, ES2AutoSave> autoSaves = new Dictionary<string, ES2AutoSave>();
	private Dictionary<Transform, ES2AutoSave> transforms = new Dictionary<Transform, ES2AutoSave>();
	

	[SerializeField]
	public ES2AutoSaveGlobalManager globalManager = null;

    // A list of IDs used. Useful if we want to check for duplicated objects.
    // Don't serialize as we want this to refresh every time scrips recompile.
    [NonSerialized]
    public HashSet<string> ids = new HashSet<string>();

	private static ES2AutoSaveManager _instance = null;
	public static ES2AutoSaveManager Instance 
	{ 
		get
		{ 
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<ES2AutoSaveManager>();
			return _instance;
		} 
	}

	public void SetFilePath(string filePath)
	{
		this.filePath = filePath;
	}

	public void Awake()
	{
		ES2AutoSaveManager._instance = this;

		for(int i=0; i<sceneObjects.Length; i++)
		{
			ES2AutoSave autoSave = sceneObjects[i];
			if(autoSave == null)
				continue;
			autoSaves[autoSave.id] = autoSave;
			transforms[autoSave.transform] = autoSave;
		}
	}
	
	public void AddAutoSave(ES2AutoSave autoSave, string id)
	{
		autoSaves[id] = autoSave;
		transforms[autoSave.transform] = autoSave;
	}

	public ES2AutoSave GetAutoSave(string id)
	{
		ES2AutoSave autoSave;
		if(autoSaves.TryGetValue(id, out autoSave))
			return autoSave;
		return null;
	}

	/* Adds a child Auto Save, which should not be added to the main Auto Save dictionary,
	 * but should be added to the transforms Dictionary */
	public void AddChildAutoSave(ES2AutoSave autoSave, string id)
	{
		transforms[autoSave.transform] = autoSave;
	}
	
	public void Save()
	{
		ES2Settings settings = new ES2Settings();
		settings.encrypt = encrypt;
		settings.encryptionPassword = encryptionPassword;

		using(ES2Writer writer = ES2Writer.Create(filePath, settings))
		{
			writer.Write(this, tag);
			writer.Save(true);
		}
	}

	public void Load()
	{
		ES2Settings settings = new ES2Settings();
		settings.encrypt = encrypt;
		settings.encryptionPassword = encryptionPassword;

		// If we want to delete instantiated prefabs before loading, do so.
		if(deletePrefabsOnLoad)
		{
			List<string> keysToRemove = new List<string>();
			foreach(KeyValuePair<string, ES2AutoSave> kvp in autoSaves)
			{
				ES2AutoSave autoSave = kvp.Value;
				if(autoSave != null && autoSave.isPrefab)
				{
					keysToRemove.Add(kvp.Key);
					transforms.Remove(autoSave.transform);
					Destroy(autoSave.gameObject);
				}
			}
			for(int i=0; i<keysToRemove.Count; i++)
				autoSaves.Remove(keysToRemove[i]);
		}

		if(!ES2.Exists(filePath+"?tag="+tag, settings))
			return;

		using(ES2Reader reader = ES2Reader.Create(filePath, settings))
		{
			reader.Read<ES2AutoSaveManager>(tag, this);
		}
	}

	public void WriteAutoSaves(ES2Writer writer)
	{
		// Save scene objects.
		foreach(KeyValuePair<Transform, ES2AutoSave> kvp in transforms)
		{
			ES2AutoSave autoSave = kvp.Value;
			Transform t = kvp.Key;

			if(autoSave == null || t == null || t.parent != null)
				continue;

			WriteAutoSaveRecursive(t, autoSave, writer);
		}

		// Signify that we have no more Auto Saves to load.
		writer.writer.Write("null");
	}

	private void WriteAutoSaveRecursive(Transform transform, ES2AutoSave autoSave, ES2Writer writer)
	{
		// Get child Auto Saves to see if any are selected, as this may mean we need to save the parent even if it's unselected.
		bool hasSelectedChildren = false;
		var children = new List<ES2AutoSave>();
		foreach(Transform t in transform)
		{
			if(t == null)
				continue;
			ES2AutoSave child;
			if(transforms.TryGetValue(t, out child))
			{
				children.Add(child);
				if(child.buttonSelected)
					hasSelectedChildren = true;
			}
		}

		// Only write the Auto Save if it has it's button selected, or we need to save the parent.
		if(autoSave.buttonSelected || hasSelectedChildren)
		{
			WriteAutoSave(autoSave, transform, writer);
		}

		// Don't recursively write children for prefabs as it has it's own routine for writing children.
		if(!autoSave.isPrefab)
			foreach(var childAutoSave in children)
				WriteAutoSaveRecursive(childAutoSave.transform, childAutoSave, writer);
	}

	private void WriteAutoSave(ES2AutoSave autoSave, Transform transform, ES2Writer writer)
	{
		writer.writer.Write(autoSave.id);
		int startPosition = writer.WritePropertyPrefix();

		writer.writer.Write(autoSave.isPrefab);
		// If we're saving a prefab, write it's prefab ID.
		if(autoSave.isPrefab)
			writer.writer.Write(autoSave.prefabID);

		bool hasSelectedComponents = autoSave.hasButtonSelectedComponents;

		// Write Auto Save instance variables.

		// Parent: Write the ID of the parent, or "null" if no parent, or blank string if not saving.
		if(autoSave.parentVariable.buttonSelected || !hasSelectedComponents)
		{
			if(transform.parent != null)
			{
				// Get the Auto Save of the parent.
				ES2AutoSave parentAutoSave;
				if(!transforms.TryGetValue(transform.parent, out parentAutoSave))
					writer.writer.Write("");
				else
					writer.writer.Write(parentAutoSave.id);
			}
			else
				writer.writer.Write("null");
		}
		else
			writer.writer.Write("");

		// activeSelf.
		if(autoSave.activeSelfVariable.buttonSelected || !hasSelectedComponents)
			writer.writer.Write(autoSave.gameObject.activeSelf.ToString());
		else
			writer.writer.Write("");

		// name
		if(autoSave.nameVariable.buttonSelected || !hasSelectedComponents)
			writer.writer.Write(autoSave.gameObject.name);
		else
			writer.writer.Write("");

		// tag
		if(autoSave.tagVariable.buttonSelected || !hasSelectedComponents)
			writer.writer.Write(autoSave.gameObject.tag);
		else
			writer.writer.Write("");

		// layer
		if(autoSave.layerVariable.buttonSelected || !hasSelectedComponents)
			writer.writer.Write(autoSave.gameObject.layer);
		else
			writer.writer.Write(-1);


		// If this is a prefab, we must write the prefabID and ID of each child prefab so that we can assign these
		// values to the correct child object before loading the child.
		List<string> childPrefabIDs = new List<string>();
		List<ES2AutoSave> childAutoSaves = new List<ES2AutoSave>();

		if(autoSave.isPrefab)
		{
			foreach(Transform childTransform in autoSave.transform)
			{
				ES2AutoSave childAutoSave = childTransform.GetComponent<ES2AutoSave>();
				if(childAutoSave != null && childAutoSave.transform != null)
				{
					childAutoSaves.Add(childAutoSave);
					childPrefabIDs.Add(childAutoSave.prefabID);
				}
			}

			writer.Write(childPrefabIDs);

			// Manually save each child.
			for(int i=0; i<childAutoSaves.Count; i++)
				WriteAutoSave(childAutoSaves[i], childAutoSaves[i].transform, writer);
		}

		foreach(ES2AutoSaveComponentInfo componentInfo in autoSave.components)
		{
			if(componentInfo.buttonSelected)
				WriteVariableRecursive(autoSave, componentInfo, writer, componentInfo.component);
		}

		// If this Auto Save is selected but not any of it's Components, save all Components.
		if(!hasSelectedComponents && autoSave.buttonSelected)
			WriteAllComponents(autoSave, writer);

		writer.WritePropertyLength(startPosition);
	}

	private void WriteVariableRecursive(ES2AutoSave autoSave, ES2AutoSaveVariableInfo variable, ES2Writer writer, object obj)
	{
		// Get the Type data for this variable, or return if a type is not supported.
		ES2Type es2Type = null;
		ES2Type dictValueType = null;
		ES2Keys.Key collectionType = ES2Keys.GetCollectionType(variable.type);

		// If it's not a collection type, just get the basic ES2Type.
		if(collectionType == ES2Keys.Key._Null)
			es2Type = ES2TypeManager.GetES2Type(variable.type);
		// Otherwise, get the collection element types.
		else
		{
			if(collectionType == ES2Keys.Key._NativeArray) // Get Array Element Type.
				es2Type = ES2TypeManager.GetES2Type(variable.type.GetElementType());
			else
			{
				// Get ES2Types for generic type arguments.
				Type[] genericArgs = ES2Reflection.GetGenericArguments(variable.type);
				if(genericArgs.Length > 0)
				{
					es2Type = ES2TypeManager.GetES2Type(genericArgs[0]);
					if(genericArgs.Length > 1)
					{
						dictValueType = ES2TypeManager.GetES2Type(genericArgs[1]);
						if(dictValueType == null)
							return;
					}
				}
			}
		}

		// Skip if unsupported. Enums are not supported unless they have an explicit ES2Type.
		if(es2Type == null || es2Type.GetType() == typeof(ES2_Enum))
			return;

		writer.writer.Write(variable.id);
		int startPosition = writer.WritePropertyPrefix();

		// Write the Type data for this variable.
		writer.writer.Write((byte)collectionType);
		writer.writer.Write(es2Type.hash);
		// If Dictionary, also write the Dictionary value type.
		if(dictValueType != null)
			writer.writer.Write(dictValueType.hash);

		int variableCount = 0;

		foreach(string variableID in variable.variableIDs)
		{
			ES2AutoSaveVariableInfo variableInfo = autoSave.GetCachedVariableInfo(variableID);
			if(variableInfo.buttonSelected)
			{
				variableCount++;
				if(variableCount == 1)
					writer.writer.Write("vars");

				object variableObj = ES2Reflection.GetValue(obj, variableInfo.name, variableInfo.isProperty);

				WriteVariableRecursive(autoSave, variableInfo, writer, variableObj);
			}
		}

		// If no variables were written, we want to save this variable.
		if(variableCount == 0)
		{
			writer.writer.Write("data");
			if(collectionType == ES2Keys.Key._Null)
				writer.Write(obj, es2Type);
			else if(collectionType == ES2Keys.Key._NativeArray)
				writer.WriteSystemArray((Array)obj, es2Type);
			else if(collectionType == ES2Keys.Key._List)
				writer.WriteICollection((ICollection)obj, es2Type);
			else if(collectionType == ES2Keys.Key._Queue)
				writer.WriteICollection((ICollection)obj, es2Type);
			else if(collectionType == ES2Keys.Key._Stack)
				writer.WriteICollection((ICollection)obj, es2Type);
			else if(collectionType == ES2Keys.Key._HashSet)
				writer.WriteICollection((ICollection)obj, es2Type);
			else if(collectionType == ES2Keys.Key._Dictionary)
				writer.WriteIDictionary((IDictionary)obj, es2Type, dictValueType);
		}

		writer.WritePropertyLength(startPosition);
	}

	/*
	 * 	This is called from the ES2Type class 'ES2_ES2AutoSaveManager'.
	 */
	public void ReadAutoSaves(ES2Reader reader)
	{
		while(ReadAutoSave(reader))
		{
		}
	}

	/*
	 * 	Reads an Auto Save from a reader.
	 * 	Returns false if there are no more Auto Saves to load.
	 * 	Optionally allows you to specify an Auto Save to load it into.
	 */
	private bool ReadAutoSave(ES2Reader reader, ES2AutoSave autoSave = null)
	{
		// If we've not already read the Auto Save's ID, read it.
		string autoSaveID = reader.reader.ReadString();

		if(autoSaveID == "null")
			return false;

		int endPosition = (int)reader.stream.Position;
		int length = reader.reader.ReadInt32();
		endPosition += length;

		bool isPrefab = reader.reader.ReadBoolean();
		string prefabID = isPrefab ? reader.reader.ReadString() : "";

		if(autoSave != null)
		{
			// If an Auto Save has already been specified, do nothing.
			// i.e. this happens when loading a child Auto Save.
		}
		// Manage loading of scene objects.
		else if(!isPrefab)
		{
			// If no Auto Save exists, create a GameObject and Auto Save for it.
			if((autoSave = GetAutoSave(autoSaveID)) == null)
				autoSave = ES2AutoSave.AddAutoSave(new GameObject(), Color.red, true, false, autoSaveID);
		}
		// Manage loading of prefabs.
		else
		{
			ES2AutoSave prefabAutoSave = null;
			ES2AutoSave[] prefabs = globalManager.prefabArray;
			// TODO: Use Dictionary for performance?
			for(int i=0; i<prefabs.Length; i++)
			{
				if(prefabs[i] != null && prefabs[i].prefabID == prefabID)
				{
					prefabAutoSave = prefabs[i];
					break;
				}
			}

			GameObject instance;

			if(prefabAutoSave == null)
			{
				// If Auto Save with ID doesn't exist, create a blank object with a new Auto Save instead.
				instance = new GameObject();
				autoSave = ES2AutoSave.AddAutoSave(instance, Color.clear, true, false, autoSaveID);
				autoSave.prefabID = prefabID;
			}
			else
			{
				// If an object with this ID doesn't already exist, instantiate the prefab.
				if(autoSaves.TryGetValue(autoSaveID, out autoSave))
					instance = autoSave.gameObject;
				else
				{
					instance = Instantiate(prefabAutoSave.gameObject);
					autoSave = instance.GetComponent<ES2AutoSave>();
					autoSave.id = autoSaveID;
					AddAutoSave(autoSave, autoSaveID);
				}
			}
		}

		/* Read Instance Variables. */

		// Parent
		string parentID = reader.reader.ReadString();
		if(parentID != "")
		{
			if(parentID == "null")
			{
				autoSave.transform.SetParent(null, false);
			}
			else
			{
				ES2AutoSave parentAutoSave;
				if(!autoSaves.TryGetValue(parentID, out parentAutoSave))
				{
					// Only set parent if the parent actually exists.
				}
				else
					autoSave.transform.SetParent(parentAutoSave.transform, false);
			}
		}

		// activeSelf
		string activeSelf = reader.reader.ReadString();
		if(activeSelf == "True")			
			autoSave.gameObject.SetActive(true);
		if(activeSelf == "False")
			autoSave.gameObject.SetActive(false);

		// name
		string name = reader.reader.ReadString();
		if(name != "")			
			autoSave.name = name;

		// tag
		string tag = reader.reader.ReadString();
		if(tag != "")			
			autoSave.tag = tag;

		// layer
		int layer = reader.reader.ReadInt32();
		if(layer > -1)
			autoSave.gameObject.layer = layer;

		// If this is a prefab, we'll need to load it's children too.
		if(autoSave.isPrefab)
		{
			List<string> childPrefabIDs = reader.ReadList<string>();
			for(int i=0; i<childPrefabIDs.Count; i++)
			{
				bool foundAutoSave = false;
				// Get the child with the given prefab ID, and load the data into it.
				foreach(Transform t in autoSave.transform)
				{
					ES2AutoSave childAutoSave = t.GetComponent<ES2AutoSave>();
					if(childAutoSave != null)
					{
						if(childAutoSave.prefabID == childPrefabIDs[i])
						{
							foundAutoSave = true;
							ReadAutoSave(reader, childAutoSave);
							break;
						}
					}
				}
				// If we didn't find an appropriate Auto Save, it's likely that this has been made a child of
				// the prefab at runtime, so load it as a normal auto save.
				if(!foundAutoSave)
				{
					ReadAutoSave(reader, null);

					/* 
					// Skip the Auto Save.
					reader.reader.ReadString();
					int endPos = (int)reader.stream.Position;
					int len = reader.reader.ReadInt32();
					endPos += len;
					reader.stream.Position = endPos;*/
				}
			}
		}

		while(reader.stream.Position != endPosition)
			ReadComponent(autoSave, reader);

		return true;
	}

	private void ReadComponent(ES2AutoSave autoSave, ES2Reader reader)
	{
		string componentID = reader.reader.ReadString();
		int endPosition = (int)reader.stream.Position;
		int length = reader.reader.ReadInt32();
		endPosition += length;

		// Read collection type byte which is not relevant to Components.
		reader.reader.ReadByte();

		int typeHash = reader.reader.ReadInt32();
		ES2Type type = ES2TypeManager.GetES2Type(typeHash);

		// If there's no ES2Type for this type of Component, skip it.
		if(type == null)
		{
			reader.stream.Position = endPosition;
			return;
		}

		// Get or create the Component.
		Component c;
		ES2AutoSaveComponentInfo componentInfo = autoSave.GetComponentInfo(componentID);
		if(componentInfo == null || componentInfo.component == null)
		{
			// If no Component info, look for the Component, or otherwise, add one.
			if(!(c = autoSave.gameObject.GetComponent(type.type)))
				c = autoSave.gameObject.AddComponent(type.type);
		}
		else
			c = componentInfo.component;

		string dataType = reader.reader.ReadString(); 

		if(dataType == "data")
		{
			reader.Read<System.Object>(type, c);
			return;
		}
		else if(dataType == "vars") // Else we're reading a series of variables denoted by "vars".
		{
			while(reader.stream.Position != endPosition)
				ReadVariableRecursive(autoSave, componentInfo, reader, c);
		}
		else
		{
			reader.stream.Position = endPosition;
			return;
		}
	}

	private void ReadVariableRecursive(ES2AutoSave autoSave, ES2AutoSaveVariableInfo variable, ES2Reader reader, object obj)
	{
		string variableID = reader.reader.ReadString();
		int endPosition = (int)reader.stream.Position;
		int length = reader.reader.ReadInt32();
		endPosition += length;

		// Read Type data
		ES2Keys.Key collectionType = (ES2Keys.Key)reader.reader.ReadByte();
		int typeHash = reader.reader.ReadInt32();
		int dictValueHash = 0;
		if(collectionType == ES2Keys.Key._Dictionary)
			dictValueHash = reader.reader.ReadInt32();

		ES2AutoSaveVariableInfo info = autoSave.GetCachedVariableInfo(variableID);

		if(info == null)
		{
			reader.stream.Position = endPosition;
			return;
		}
			
		string dataType = reader.reader.ReadString();

		if(dataType == "data")
		{
			// Get ES2Types.
			ES2Type type = ES2TypeManager.GetES2Type(typeHash);
			ES2Type dictValueType = null;
			if(collectionType == ES2Keys.Key._Dictionary)
				dictValueType = ES2TypeManager.GetES2Type(dictValueHash);

			// If the type of data we're loading isn't supported, skip it.
			if(type == null || (collectionType == ES2Keys.Key._Dictionary && dictValueType == null))
			{
				reader.stream.Position = endPosition;
				return;
			}
				
			bool valueWasSet = false;

			if(collectionType == ES2Keys.Key._Null)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.Read<System.Object>(type), info.isProperty);
			else if(collectionType == ES2Keys.Key._NativeArray)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.ReadSystemArray(type), info.isProperty);
			else if(collectionType == ES2Keys.Key._List)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.ReadICollection(typeof(List<>), type), info.isProperty);
			else if(collectionType == ES2Keys.Key._Queue)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.ReadICollection(typeof(Queue<>), type), info.isProperty);
			else if(collectionType == ES2Keys.Key._Stack)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.ReadICollection(typeof(Stack<>), type), info.isProperty);
			else if(collectionType == ES2Keys.Key._HashSet)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.ReadICollection(typeof(HashSet<>), type), info.isProperty);
			else if(collectionType == ES2Keys.Key._Dictionary)
				valueWasSet = ES2Reflection.SetValue(obj, info.name, reader.ReadIDictionary(typeof(Dictionary<,>), type, dictValueType), info.isProperty);

			if(!valueWasSet)
			{
				reader.stream.Position = endPosition;
				return;
			}
		}
		// Else, we have variables to load.
		else if(dataType == "vars")
		{
			object thisObj = ES2Reflection.GetValue(obj, info.name, info.isProperty);
			if(thisObj == null)
				reader.stream.Position = endPosition;
			ReadVariableRecursive(autoSave, info, reader, thisObj);
		}
		else
		{
			reader.stream.Position = endPosition;
			return;
		}
	}

	protected void WriteAllComponents(ES2AutoSave autoSave, ES2Writer writer)
	{
		Component[] components = autoSave.GetComponents<Component>();

		foreach(Component c in components)
		{
			if(c == null)
				continue;
			
			ES2Type type;
			ES2AutoSaveComponentInfo info = autoSave.GetComponentInfo(c);

			type = ES2TypeManager.GetES2Type(c.GetType());
			if(type == null)
				continue;

			if(info == null)
				info = new ES2AutoSaveComponentInfo(c, autoSave);
			WriteVariableRecursive(autoSave, info, writer, c);
		}
	}
	
	/* ----- EVENTS ----- */

	public void Start()
	{
		if(loadEvent == LoadEvent.Start)
			Load();
	}

	public void OnApplicationPause(bool pause)
	{
		if(loadEvent == LoadEvent.OnApplicationUnPause && !pause)
			Load();

		if(saveEvent == SaveEvent.OnApplicationPause && pause)
			Save();
	}

	public void OnApplicationQuit()
	{
		if(saveEvent == SaveEvent.OnApplicationQuit)
			Save();
	}

	/*public void LevelWasLoaded(int level)
	{
		if(loadEvent == LoadEvent.OnLevelWasLoaded)
			Load();
	}*/

	public void OnDisable()
	{
		if(saveEvent == SaveEvent.OnDisable)
			Save();
	}
}
#endif
