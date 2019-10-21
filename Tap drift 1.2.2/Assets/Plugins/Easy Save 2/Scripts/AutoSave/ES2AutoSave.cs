#if !UNITY_4
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
[ExecuteInEditMode] 
public class ES2AutoSave : MonoBehaviour, IES2Selectable 
{
	public UnityEngine.Object undoRecordObject { get{ return this; } }

	[SerializeField]
	public List<ES2AutoSaveVariableInfo> variableCache = new List<ES2AutoSaveVariableInfo>();

	[SerializeField]
	public List<ES2AutoSaveComponentInfo> components = new List<ES2AutoSaveComponentInfo>();

	// GameObject Instance Variables
	[SerializeField]
	public ES2AutoSaveVariableInfo activeSelfVariable = null;
	[SerializeField]
	public ES2AutoSaveVariableInfo nameVariable = null;
	[SerializeField]
	public ES2AutoSaveVariableInfo tagVariable = null;
	[SerializeField]
	public ES2AutoSaveVariableInfo layerVariable = null;
	[SerializeField]
	public ES2AutoSaveVariableInfo parentVariable = null;

	[SerializeField]
	public string id = "";
	[SerializeField]
	public string prefabID = "";

	[SerializeField]
	public bool isInitialised = false;

	// Whether this GameObject is selected in the Auto Save Editor Window.
	[SerializeField]
	public bool _selected = false;
	public bool selected { get{ return _selected; } set{_selected = value; selectionChanged = true; } }
	public bool selectionChanged { get; set; }

	[SerializeField]
	public bool saveButtonSelected = false;
	public bool buttonSelected { get{ return saveButtonSelected; } set{ saveButtonSelected = value; buttonSelectionChanged = true; } }
	public bool buttonSelectionChanged { get; set; }

	[SerializeField]
	// The color associated with this Auto Save in the Editor.
	public Color color = Color.clear;

	[SerializeField]
	public new Transform transform;

	public bool hasButtonSelectedComponents
	{
		get
		{
			if(activeSelfVariable.buttonSelected || parentVariable.buttonSelected || nameVariable.buttonSelected || tagVariable.buttonSelected || layerVariable.buttonSelected)
				return true;
			for(int i=0; i<components.Count; i++)
				if(components[i].buttonSelected)
					return true;
			return false;
		}
	}

	public bool isPrefab{ get{ return !string.IsNullOrEmpty(prefabID); } }

	public ES2AutoSaveVariableInfo GetCachedVariableInfo(string id)
	{
		for(int i=0; i<variableCache.Count; i++)
			if(variableCache[i] != null && variableCache[i].id == id)
				return variableCache[i];
		return null;
	}

	public ES2AutoSaveVariableInfo CacheVariableInfo(ES2AutoSaveVariableInfo variable)
	{
		variableCache.Add(variable);
		return variable;
	}

	public void RemoveCachedVariableInfo(string id)
	{
		ES2AutoSaveVariableInfo info = GetCachedVariableInfo(id);
		if(info == null)
			return;

		// Recursively delete all child variables.
		foreach(string childId in info.variableIDs)
			RemoveCachedVariableInfo(childId);

		// Remove this variable.
		variableCache.Remove(info);
	}

	public void Awake()
	{
		if(string.IsNullOrEmpty(id))
		{
			// If this is not an Auto Save created at runtime, do nothing.
			if(!isPrefab || ES2AutoSaveManager.Instance == null)
				return;
			
			id = GenerateID();

			ES2AutoSaveManager.Instance.AddAutoSave(this, id);

			// If this is being called because we've added this prefab to the scene
			// outside of runtime, turn it into a scene object.
			if(!Application.isPlaying && ES2AutoSaveManager.Instance.convertPrefabsToSceneObjectsOnImport)
				prefabID = "";
		}
		else if(isPrefab && Application.isPlaying)
		{
			// If this has already been initialised, it means it's a duplicated prefab,
			// so we need to give it a new ID.
			if(isInitialised)
				id = GenerateID();
			if(transform.parent == null)
				ES2AutoSaveManager.Instance.AddAutoSave(this, id);
		}

		if(Application.isPlaying)
			isInitialised = true;
	}

	public ES2AutoSaveComponentInfo GetComponentInfo(Component c)
	{
		for(int i=0; i<components.Count; i++)
		{
			ES2AutoSaveComponentInfo info = components[i];
			if(info != null && info.component != null && info.component == c)
				return info;
		}
		return null;
	}

	public ES2AutoSaveComponentInfo GetComponentInfo(string id)
	{
		for(int i=0; i<components.Count; i++)
		{
			ES2AutoSaveComponentInfo info = components[i];
			if(info.component != null && info != null && info.id == id)
				return info;
		}
		return null;
	}

	public ES2AutoSaveComponentInfo AddComponentInfo(ES2AutoSaveComponentInfo info)
	{
		components.Add(info);
		return info;
	}

	public static ES2AutoSave AddAutoSave(GameObject go, Color color, bool hide, bool generateID, string id)
	{
		ES2AutoSave autoSave = go.AddComponent<ES2AutoSave>();
		autoSave.color = color;
		autoSave.transform = go.transform;

		if(hide)
			autoSave.hideFlags = HideFlags.HideInInspector;

		// Add instance variables of GameObject.
		// parent.
		autoSave.parentVariable = new ES2AutoSaveVariableInfo("parent", typeof(string), true, autoSave, null);
		// activeSelf.
		autoSave.activeSelfVariable = new ES2AutoSaveVariableInfo("activeSelf", typeof(bool), true, autoSave, null);
		// name.
		autoSave.nameVariable = new ES2AutoSaveVariableInfo("name", typeof(string), true, autoSave, null);
		// tag.
		autoSave.tagVariable = new ES2AutoSaveVariableInfo("tag", typeof(string), true, autoSave, null);
		// layer.
		autoSave.layerVariable = new ES2AutoSaveVariableInfo("layer", typeof(int), true, autoSave, null);

		// Generate a GUID (without dashes) if necessary.
		if(generateID)
			autoSave.id = GenerateID();
		if(id != "")
			autoSave.id = id;
		return autoSave;
	}

	public static ES2AutoSave GetAutoSave(GameObject go)
	{
		return go.GetComponent<ES2AutoSave>();
	}

	public static string GenerateID()
	{
		string enc = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
		enc = enc.Replace("/", "0");
		enc = enc.Replace("+", "1");
		enc = enc.Replace("-", "2");
		return enc.Substring(0, 22);
	}
}

/*
 * 	Stores information about each Component we want to save.
 */
[System.Serializable]
public class ES2AutoSaveComponentInfo : ES2AutoSaveVariableInfo
{
	// The Component we're storing the information of.
	[SerializeField]
	public Component component;

	public ES2AutoSaveComponentInfo(){}

	public ES2AutoSaveComponentInfo(Component c, ES2AutoSave autoSave)
	{
		this.component = c;
		this.type = c.GetType();
		this.name = this.type.Name;
		this.id = ES2AutoSave.GenerateID();
		this.autoSave = autoSave;
		this.isComponent = true;
	}
	
	/*
	 * 	Constructor for displaying instance variables of GameObject.
	 */
	public ES2AutoSaveComponentInfo(string name, Type type, ES2AutoSave autoSave)
	{
		this.component = autoSave;
		this.type = type;
		this.name = name;
		this.id = ES2AutoSave.GenerateID();
		this.autoSave = autoSave;
		this.isComponent = false;
		this.isProperty = true;
	}
}

/*
 * 	Stores information about each variable we want to save.
 */
[System.Serializable]
public class ES2AutoSaveVariableInfo : IES2Selectable
{
	[SerializeField]
	public string name;
	[SerializeField]
	public string id;
	[SerializeField]
	public string previousID = "";
	[SerializeField]
	public bool isComponent = false;
	[SerializeField]
	private bool _selected = false;
	public bool selected { get{ return _selected; } set{_selected = value; selectionChanged = true; } }
	[SerializeField]
	public bool selectionChanged {get; set;}
	[SerializeField]
	private bool saveButtonSelected = false;
	[SerializeField]
	public bool isProperty = false;
	public bool buttonSelected { get{ return saveButtonSelected; } set{ saveButtonSelected = value; buttonSelectionChanged = true; } }
	public bool buttonSelectionChanged { get; set; }
	public string assemblyQualifiedTypeName;

	public List<string> variableIDs = new List<string>();

	public GameObject gameObject = null;
	public ES2AutoSave _autoSave = null;
	public ES2AutoSave autoSave
	{
		get
		{
			// Auto Save might be null after recompilation as the original script might be swapped with custom one.
			// For this reason, we also store the GameObject so we can retrieve the Auto Save.
			if(_autoSave == null)
				_autoSave = gameObject.GetComponent<ES2AutoSave>();
			return _autoSave;
		}
		set
		{
			_autoSave = value;
			gameObject = value.gameObject;
		}
	}

	public UnityEngine.Object undoRecordObject { get{ return autoSave; } }

	public System.Type type
	{
		get{ return System.Type.GetType(assemblyQualifiedTypeName); }
		set{ assemblyQualifiedTypeName = value.AssemblyQualifiedName; }
	}

	public bool HasButtonSelectedVariables
	{
		get
		{
			foreach(string id in variableIDs)
			{
				ES2AutoSaveVariableInfo variable = GetVariableInfo(id);
				if(variable != null && variable.buttonSelected)
					return true;
			}
			return false;
		}
	}

	public ES2AutoSaveVariableInfo(){}

	public ES2AutoSaveVariableInfo(string name, Type type, bool isProperty, ES2AutoSave autoSave, ES2AutoSaveVariableInfo previous)
	{
		this.name = name;
		this.id = ES2AutoSave.GenerateID();
		this.type = type;
		this.autoSave = autoSave;
		this.isProperty = isProperty;
		if(previous != null)
			this.previousID = previous.id;
	}

	public ES2AutoSaveVariableInfo GetVariableInfo(string name)
	{
		for(int i=0; i<variableIDs.Count; i++)
		{
			ES2AutoSaveVariableInfo v = autoSave.GetCachedVariableInfo(variableIDs[i]);
			if(v != null && v.name == name)
				return v;
		}
		return null;
	}

	public ES2AutoSaveVariableInfo AddVariableInfo(string name, Type type, bool isProperty, ES2AutoSave autoSave = null, ES2AutoSaveVariableInfo previous = null)
	{
		if(previous == null)
			previous = this;
		if(autoSave == null)
			autoSave = previous.autoSave;

		ES2AutoSaveVariableInfo info = new ES2AutoSaveVariableInfo(name, type, isProperty, autoSave, previous);
		autoSave.CacheVariableInfo(info);
		variableIDs.Add(info.id);
		return info;
	}

	/* Deletes the variable info with the given ID from this variable info and it's Auto Save */
	public void DeleteVariableInfo(string id)
	{
		variableIDs.Remove(id);
		autoSave.RemoveCachedVariableInfo(id);
	}
}

/*
 * 	Represents an object which can have a 'selected' state.
 */
public interface IES2Selectable
{
	bool selected {get; set;}
	bool selectionChanged {get; set;}
	bool buttonSelected {get; set;}
	bool buttonSelectionChanged {get; set;}
	// The Component we will call Undo.RecordObject on to record the change.
	UnityEngine.Object undoRecordObject {get;}
}
#endif