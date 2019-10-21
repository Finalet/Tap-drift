#if !UNITY_4 && !UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;

public class ES2EditorAutoSaveUtility
{
	public const string enableAutoSaveForPrefabMenuName = "Assets/Easy Save 2/Enable Auto Save for Prefab(s)";
	public const string managerPrefabPath = "Assets/Easy Save 2/Auto Save/ES2 Auto Save Manager.prefab";
	public const string globalManagerPrefabPath = "Assets/Easy Save 2/Auto Save/ES2 Auto Save Global Manager.prefab";

	public static ES2AutoSaveGlobalManager globalMgr
	{
		get { return ((GameObject)AssetDatabase.LoadAssetAtPath(globalManagerPrefabPath, typeof(GameObject))).GetComponent<ES2AutoSaveGlobalManager>(); }
	}

	public static ES2AutoSaveManager _mgr = null;
	public static ES2AutoSaveManager mgr
	{
		get
		{  
			if(_mgr == null)
			{
				ES2AutoSaveManager[] mgrs = GameObject.FindObjectsOfType<ES2AutoSaveManager>();
				if(mgrs.Length > 0)
					_mgr = mgrs[0];
			}
			return _mgr;
		}
		set{ _mgr = value; }
	}

	/*
	 * 	Adds an ES2 Auto Save Manager object to the current scene.
	 */
	public static void AddManagerToScene()
	{
		GameObject managerPrefab = GetManagerPrefab();
		GameObject instance = PrefabUtility.InstantiatePrefab(managerPrefab) as GameObject;
		PrefabUtility.DisconnectPrefabInstance(instance);
		ES2EditorAutoSaveUtility.mgr = instance.GetComponent<ES2AutoSaveManager>();
		if(AutoSaveComponentsAreHidden())
			instance.hideFlags = HideFlags.HideInHierarchy;
	}

	/*
	 * 	Gets the prefab for the ES2 Auto Save Manager from Assets.
	 */
	public static GameObject GetManagerPrefab()
	{
		return (GameObject)AssetDatabase.LoadAssetAtPath(managerPrefabPath, typeof(GameObject));
	}

	/*
	 * 	Enables Auto Save for the prefabs which are currently selected.	
	 */
	[MenuItem(enableAutoSaveForPrefabMenuName, false, 1000)]
	public static void EnableAutoSaveForSelectedPrefabs()
	{
		if(Selection.gameObjects == null)
			return;
		
		for(int i=0; i<Selection.gameObjects.Length; i++)
		{
			GameObject prefab = Selection.gameObjects[i];

			// If this has a parent, support must be added via it's parent.
			if(prefab.transform.parent != null)
				continue;

			EnableAutoSaveForSelectedPrefabRecursive(prefab);
		}

		// Clean-up any deleted prefabs from the array.
		for(int j=0; j<globalMgr.prefabArray.Length; j++)
			if(globalMgr.prefabArray[j] == null)
				ArrayUtility.RemoveAt(ref globalMgr.prefabArray, j);

		RefreshPrefabAutoSaves();
	}

	// Enable Auto Save for a prefab and it's children.
	public static void EnableAutoSaveForSelectedPrefabRecursive(GameObject prefab)
	{
		ES2AutoSave autoSave = prefab.GetComponent<ES2AutoSave>();

		// Only add an Auto Save if this prefab doesn't already have one.
		if(autoSave == null)
		{
			// Add an ES2AutoSave to the prefab and add it to the Auto Save manager prefab.
			autoSave = ES2AutoSave.AddAutoSave(prefab, RandomColor(), AutoSaveComponentsAreHidden(), false, "");

			ES2AutoSaveGlobalManager globalManager = globalMgr;

			// Don't add prefab to prefab array if it's a child prefab.
			if(autoSave.transform.parent == null)
				ArrayUtility.Add(ref globalManager.prefabArray, autoSave);
		}

		foreach(Transform childTransform in prefab.transform)
			EnableAutoSaveForSelectedPrefabRecursive(childTransform.gameObject);
	}
	
	/* 	Makes the MenuItem for EnableAutoSaveForPrefab only appear if it's a prefab. */
	[MenuItem(enableAutoSaveForPrefabMenuName, true, 1000)]
	public static bool SelectionIsPrefab()
	{
		if(Application.unityVersion[0] == '4')
			return false;

		if(Selection.gameObjects == null || Selection.gameObjects.Length < 1)
			return false;

		for(int i=0; i<Selection.gameObjects.Length; i++)
		{
#if UNITY_2018_3_OR_ABOVE
			if(PrefabUtility.GetCorrespondingObjectFromSource(Selection.gameObjects[i]) != null || PrefabUtility.GetPrefabObject(Selection.gameObjects[i]) == null)
				return false;
#else
			if(PrefabUtility.GetPrefabParent(Selection.gameObjects[i]) != null || PrefabUtility.GetPrefabObject(Selection.gameObjects[i]) == null)
				return false;
#endif
		}

		return true;
	}

	/*
	 * 	Refreshes the variables and Components for all Auto Saves in this scene.
	 * 	Should be called after a change is made to the scene, or scripts are recompiled.
	 */
	public static void RefreshSceneAutoSaves()
	{
		// Only refresh if Easy Save is enabled for this scene.
		if(mgr == null || EditorApplication.isPlayingOrWillChangePlaymode)
			return;

		mgr.sceneObjects = new ES2AutoSave[0];
		mgr.ids = new HashSet<string>();

		Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
		
		// Recurse over top-level scene objects and display their children hierarchically.
		foreach(Transform t in transforms)
		{
			if(t.parent == null && t.hideFlags == HideFlags.None)
			{
				ES2AutoSave autoSave = ES2AutoSave.GetAutoSave(t.gameObject);
				if(autoSave == null)
				{
					autoSave = ES2AutoSave.AddAutoSave(t.gameObject, RandomColor(), AutoSaveComponentsAreHidden(), true, "");

					if(AutoSaveComponentsAreHidden())
						autoSave.hideFlags = HideFlags.HideInInspector;
				}

				// If a prefab has been added to the scene, give it an ID and treat it as a scene object.
				if(string.IsNullOrEmpty(autoSave.id))
				{
					autoSave.id = ES2AutoSave.GenerateID();
					autoSave.prefabID = "";
				}

				// Check for duplicate IDs.
				if(mgr.ids.Contains(autoSave.id))
					autoSave.id = ES2AutoSave.GenerateID();
				mgr.ids.Add(autoSave.id);

				UpdateAutoSave(autoSave);
				AddAutoSaveToManager(autoSave);

				GetOrAddChildrenForAutoSave(autoSave);
			}
		}
	}
		
	/*
	 * 	Refreshes the variables and Components for all Auto Saves attached to prefabs.
	 * 	Should be called after scripts are recompiled.
	 */
	public static void RefreshPrefabAutoSaves()
	{
		ES2AutoSaveGlobalManager globalMgr = ES2EditorAutoSaveUtility.globalMgr;

		if(globalMgr == null)
			return;
		// Clear all prefabs and start from fresh.
		globalMgr.prefabArray = new ES2AutoSave[0];
		globalMgr.ids = new HashSet<string>();

		List<GameObject> prefabs = GetPrefabs();

		for(int i=0; i<prefabs.Count; i++)
		{
			if(prefabs[i] == null)
				continue;
			ES2AutoSave autoSave = prefabs[i].GetComponent<ES2AutoSave>();
			if(autoSave != null)
				ArrayUtility.Add(ref globalMgr.prefabArray, autoSave);
		}


		// Recurse over top-level scene objects, and recursively update them and their children.
		foreach(ES2AutoSave autoSave in globalMgr.prefabArray)
			UpdatePrefabAutoSaveRecursive(autoSave);
	}

	private static void UpdatePrefabAutoSaveRecursive(ES2AutoSave autoSave)
	{
		// If this prefab has been created from a scene object, or the prefab has been duplicated,
		// generate a new ID for it.
		if(String.IsNullOrEmpty(autoSave.prefabID) || globalMgr.ids.Contains(autoSave.prefabID))
			autoSave.prefabID = ES2AutoSave.GenerateID();

		// If this prefab has been created from a scene object, we'll also need to remove it's id.
		if(!String.IsNullOrEmpty(autoSave.id))
			autoSave.id = "";

		globalMgr.ids.Add(autoSave.prefabID);

		UpdateAutoSave(autoSave);

		foreach(Transform t in autoSave.transform)
		{
			ES2AutoSave childAutoSave = t.GetComponent<ES2AutoSave>();
			if(childAutoSave != null)
				UpdatePrefabAutoSaveRecursive(childAutoSave);
		}
	}

	private static void GetChildrenForAutoSave(ES2AutoSave autoSave)
	{
		foreach(Transform t in autoSave.transform)
		{
			ES2AutoSave child = ES2AutoSave.GetAutoSave(t.gameObject);
			if(child == null)
				child = ES2AutoSave.AddAutoSave(t.gameObject, RandomColor(), AutoSaveComponentsAreHidden(), true, "");
			GetChildrenForAutoSave(child);
		}
	}

	private static void GetOrAddChildrenForAutoSave(ES2AutoSave autoSave)
	{
		if(autoSave == null)
			return;

		foreach(Transform t in autoSave.transform)
		{
			ES2AutoSave child = ES2AutoSave.GetAutoSave(t.gameObject);
			if(child == null)
				child = ES2AutoSave.AddAutoSave(t.gameObject, RandomColor(), AutoSaveComponentsAreHidden(), true, "");
			UpdateAutoSave(child);
			AddAutoSaveToManager(child);
			GetOrAddChildrenForAutoSave(child);
		}
	}

	public static void UpdateAutoSave(ES2AutoSave autoSave)
	{
		// Delete any null values from Components array.
		for(int i=0; i<autoSave.components.Count; i++)
		{
			if(autoSave.components[i] == null || autoSave.components[i].component == null)
			{
				autoSave.components.RemoveAt(i);
				i--;
			}
		}

		Component[] components = autoSave.gameObject.GetComponents(typeof(Component));
		
		for(int i=0; i<components.Length; i++)
		{
			Component c = components[i];

			if(c == null)
				continue;
			
			// Exclude Easy Save components.
			Type type = c.GetType();
			if(typeof(ES2AutoSave).IsAssignableFrom(type) || 
			   typeof(ES2AutoSaveManager) == type)
				continue;
			
			ES2AutoSaveComponentInfo componentInfo = autoSave.GetComponentInfo(c);
			// If the Component Info for this Component hasn't been added to the Auto Save, add it.
			if(componentInfo == null)
				componentInfo = autoSave.AddComponentInfo(new ES2AutoSaveComponentInfo(c, autoSave));
			
			// Get variables column if this Component is selected.
			if(componentInfo.selected)
				UpdateVariablesForVariable(componentInfo);
		}
	}

	public static void UpdateVariablesForVariable(ES2AutoSaveVariableInfo variable)
	{
		// Don't display variables of strings, enums, collections or value types.
		if(variable.type == typeof(string) || variable.type.IsEnum || variable.type.IsValueType || ES2EditorTypeUtility.IsCollectionType(variable.type))
			return;

		Type type = variable.type;
		FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

		// Add all of the variables to a List so we can see if any need deleting.
		List<ES2AutoSaveVariableInfo> variables = new List<ES2AutoSaveVariableInfo>();

		foreach(FieldInfo field in fields)
		{
			if(!ES2EditorTypeUtility.FieldIsSupportable(field) || typeof(Component).IsAssignableFrom(field.FieldType))
				continue;
			
			ES2AutoSaveVariableInfo newInfo = variable.GetVariableInfo(field.Name);
			if(newInfo == null)
				newInfo = variable.AddVariableInfo(field.Name, field.FieldType, false);
			variables.Add(newInfo);

			// Update type incase type has changed.
			newInfo.type = field.FieldType;
			
			if(newInfo.selected)
				UpdateVariablesForVariable(newInfo);
		}

		PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		
		foreach(PropertyInfo property in properties)
		{
			if(!ES2EditorTypeUtility.PropertyIsSupportable(property) || typeof(Component).IsAssignableFrom(property.PropertyType))
				continue;

			ES2AutoSaveVariableInfo newInfo = variable.GetVariableInfo(property.Name);
			if(newInfo == null)
				newInfo = variable.AddVariableInfo(property.Name, property.PropertyType, true);
			variables.Add(newInfo);

			// Update type incase type has changed.
			newInfo.type = property.PropertyType;

			if(newInfo.selected)
				UpdateVariablesForVariable(newInfo);
		}

		// Check if any variables need deleting.
		if(variable.variableIDs.Count != variables.Count)
		{
			List<string> variableIDsToDelete = new List<string>();
			for(int i=0; i<variable.variableIDs.Count; i++)
			{
				string variableID = variable.variableIDs[i];
				bool deleteVariable = true;
				for(int j=0; j<variables.Count; j++)
				{
					if(variables[j].id == variableID)
					{
						deleteVariable = false;
						break;
					}
				}
				if(deleteVariable)
					variableIDsToDelete.Add(variableID);
			}

			// Now remove the variable IDs we want to delete.
			foreach(string variableIDToDelete in variableIDsToDelete)
				variable.DeleteVariableInfo(variableIDToDelete);
		}
	}

	private static void AddAutoSaveToManager(ES2AutoSave autoSave)
	{
		ArrayUtility.Add(ref mgr.sceneObjects, autoSave);
	}
		
	/*
	 * 	Gets any variables which are at the end of the variable chain.
	 *  i.e. it is button selected, but it has no child variables which are button selected.
	 * 	Excludes Components.
	 */
	private static List<ES2AutoSaveVariableInfo> GetVariablesAtEndOfChains(ES2AutoSave autoSave)
	{
		List<ES2AutoSaveVariableInfo> variables = new List<ES2AutoSaveVariableInfo>();

		// Check if any variables are at the end of a variable chain.
		foreach(ES2AutoSaveVariableInfo variable in autoSave.variableCache)
			if(variable.buttonSelected && !variable.HasButtonSelectedVariables)
				variables.Add(variable);

		return variables;
	}

	/*
	 * 	Gets the chain of variables leading up to a given variable, in reverse order.
	 * 	i.e. end of chain ('variable' parameter) is first item in List.
	 */
	private static List<ES2AutoSaveVariableInfo> GetVariableChain(ES2AutoSaveVariableInfo variable)
	{
		List<ES2AutoSaveVariableInfo> chain = new List<ES2AutoSaveVariableInfo>();
		chain.Add(variable);

		if(string.IsNullOrEmpty(variable.previousID))
			return chain;

		ES2AutoSaveVariableInfo previous = variable.autoSave.GetCachedVariableInfo(variable.previousID);
		if(previous == null)
			previous = variable.autoSave.GetComponentInfo(variable.previousID);

		chain.AddRange( GetVariableChain(previous) );

		return chain;
	}

	private void SelectParentButtons(ES2AutoSaveVariableInfo info)
	{
		if(!string.IsNullOrEmpty(info.previousID))
		{
			ES2AutoSaveVariableInfo previous = info.autoSave.GetCachedVariableInfo(info.previousID);
			if(previous == null)
				previous = info.autoSave.GetComponentInfo(info.previousID);
			previous.buttonSelected = true;
			SelectParentButtons(previous);
		}
		else if(info.isComponent)
			info.autoSave.buttonSelected = true;
	}

	public static void ToggleHideAutoSaveComponents()
	{
		bool hide = !AutoSaveComponentsAreHidden();
		EditorPrefs.SetBool("HideAutoSaveComponents", hide);
		HideFlags goHideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;
		HideFlags componentHideFlags = hide ? HideFlags.HideInInspector : HideFlags.None;

		// Hide scene components.
		if(mgr != null)
		{
			mgr.gameObject.hideFlags = goHideFlags;
            EditorSceneManager.MarkAllScenesDirty();

			foreach(ES2AutoSave autoSave in mgr.sceneObjects)
				autoSave.hideFlags = componentHideFlags;
		}

		// Hide Prefab components.
		List<GameObject> prefabs = GetPrefabs();

		for(int i=0; i<prefabs.Count; i++)
		{
			ES2AutoSave autoSave = prefabs[i].GetComponent<ES2AutoSave>();
			if(autoSave != null)
				autoSave.hideFlags = componentHideFlags;
		}
	}

	public static bool AutomaticallyRefreshSceneAutoSaves
	{
		set{ EditorPrefs.SetBool("AutomaticallyUpdateSceneAutoSaves", value); }
		get{ return EditorPrefs.GetBool("AutomaticallyUpdateSceneAutoSaves", true); }
	}

	public static void RemoveAutoSaveFromScene()
	{
		object[] objs = Resources.FindObjectsOfTypeAll(typeof(GameObject));

		for(int i=0; i<objs.Length; i++)
		{
			GameObject go = (GameObject)objs[i];
			ES2AutoSave autoSave = go.GetComponent<ES2AutoSave>();
			if(autoSave != null)
			{
				Undo.DestroyObjectImmediate(autoSave);
			}
		}

		GameObject mgr = GameObject.Find("ES2 Auto Save Manager");
		if(mgr != null)
			Undo.DestroyObjectImmediate(mgr);
	}

	public static void RemoveAutoSaveFromAllPrefabs()
	{
		// Hide Prefab components.
		List<GameObject> prefabs = GetPrefabs();

		for(int i=0; i<prefabs.Count; i++)
			RemoveAutoSaveFromPrefabRecursive(prefabs[i]);
	}

	public static void RemoveAutoSaveFromPrefabRecursive(GameObject go)
	{
		ES2AutoSave autoSave = go.GetComponent<ES2AutoSave>();
		if(autoSave != null)
			Undo.DestroyObjectImmediate(autoSave);
		foreach(Transform childTransform in go.transform)
			RemoveAutoSaveFromPrefabRecursive(childTransform.gameObject);
	}

	public static bool AutoSaveComponentsAreHidden()
	{
		return EditorPrefs.GetBool("HideAutoSaveComponents", true);
	}

	private static Color RandomColor()
	{
		Color[] colors = {	Color.red, Color.green, Color.blue,
							Color.yellow, Color.cyan, Color.magenta,
			(Color.red+Color.yellow)/2, (Color.cyan+Color.blue)/2, (Color.blue+Color.magenta)/2, (Color.green+Color.cyan)/2, (Color.magenta+Color.red)/2};

		int previousRandomColorIndex = EditorPrefs.GetInt("es2PreviousRandomColorIndex", UnityEngine.Random.Range(0, colors.Length-1));
		previousRandomColorIndex = (previousRandomColorIndex == colors.Length-1) ? 0 : previousRandomColorIndex+1;
		EditorPrefs.SetInt("es2PreviousRandomColorIndex", previousRandomColorIndex);

		Color color = colors[previousRandomColorIndex];
		float brightness = UnityEngine.Random.Range(0.5f, 0.95f);
		return new Color((color.r+brightness)/2,(color.g+brightness)/2,(color.b+brightness)/2);
	}

	private static int GetHash(string value)
	{
		unchecked
		{
			// check for degenerate input
			if (string.IsNullOrEmpty(value))
				return 0;
			
			int length = value.Length;
			uint hash = (uint) length;
			
			int remainder = length & 1;
			length >>= 1;
			
			// main loop
			int index = 0;
			for (; length > 0; length--)
			{
				hash += value[index];
				uint temp = (uint) (value[index + 1] << 11) ^ hash;
				hash = (hash << 16) ^ temp;
				index += 2;
				hash += hash >> 11;
			}
			
			// handle odd string length
			if (remainder == 1)
			{
				hash += value[index];
				hash ^= hash << 11;
				hash += hash >> 17;
			}
			
			// force "avalanching" of final 127 bits
			hash ^= hash << 3;
			hash += hash >> 5;
			hash ^= hash << 4;
			hash += hash >> 17;
			hash ^= hash << 25;
			hash += hash >> 6;
			
			return (int) hash;
		}
	}

	private static List<GameObject> GetPrefabs()
	{
		if(Application.unityVersion[0] != '4')
			return GetPrefabsUnity5();
		else
			return GetPrefabsUnity4();
	}

	private static List<GameObject> GetPrefabsUnity5()
	{
		List<GameObject> tempObjects = new List<GameObject>();
		string[] goGuids = AssetDatabase.FindAssets("t:GameObject");

		for(int i=0; i<goGuids.Length; i++)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(goGuids[i]);
			tempObjects.Add((GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)));
		}
		return tempObjects;
	}

	private static List<GameObject> GetPrefabsUnity4()
	{
		List<GameObject> tempObjects = new List<GameObject>();

		DirectoryInfo directory = new DirectoryInfo(Application.dataPath);
		FileInfo[] goFileInfo = directory.GetFiles("*.prefab", SearchOption.AllDirectories);

		int i = 0; int goFileInfoLength = goFileInfo.Length;
		FileInfo tempGoFileInfo; string tempFilePath;
		GameObject tempGO;
		for (; i < goFileInfoLength; i++)
		{
			tempGoFileInfo = goFileInfo[i];
			if (tempGoFileInfo == null)
				continue;

			tempFilePath = tempGoFileInfo.FullName;
			tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");

			tempGO = AssetDatabase.LoadAssetAtPath(tempFilePath, typeof(GameObject)) as GameObject;
			if (tempGO == null)
				continue;
			else if (tempGO.GetType() != typeof(GameObject))
				continue;

			tempObjects.Add(tempGO);
		}

		return tempObjects;
	}
}
#endif