using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using ES3Internal;

[InitializeOnLoad]
public class ES3Postprocessor : UnityEditor.AssetModificationProcessor
{
	public static ES3ReferenceMgr _refMgr;
	public static ES3ReferenceMgr refMgr
	{
		get
		{
			if(defaultSettings.addMgrToSceneAutomatically && _refMgr == null)
				AddManagerToScene();
			return _refMgr;
		}
	}
	
	public static ES3AutoSaveMgr _autoSaveMgr;
	public static ES3AutoSaveMgr autoSaveMgr
	{
		get{ if(_autoSaveMgr != null) return _autoSaveMgr; if(refMgr == null) return null; return refMgr.gameObject.GetComponent<ES3AutoSaveMgr>(); }
	}
	
	public static ES3DefaultSettings _defaultSettings;
	public static ES3DefaultSettings defaultSettings
	{
		get
		{ 
			if(_defaultSettings == null) 
				_defaultSettings = ES3Settings.GetDefaultSettings(); 
				return _defaultSettings;
		}
	}
	
	public static bool didGenerateReferences = false;
	public static ES3DefaultSettings settings;

	public static GameObject lastSelected = null;
	
	public static Queue<GameObject> referenceQueue = new Queue<GameObject>();


	// This constructor is also called once when playmode is activated.
	static ES3Postprocessor()
	{
		// Open the Easy Save 3 window the first time ES3 is installed.
		ES3Editor.ES3Window.OpenEditorWindowOnStart();

		EditorApplication.update += Update;
		Selection.selectionChanged += SelectionChanged;
	}
	
	static void SelectionChanged()
	{
		if(EditorApplication.isPlaying)
			return;
		
		var selected = Selection.activeGameObject;
		
		// If the previously selected object isn't same GameObject, process previously selected GO.
		if(lastSelected != selected && lastSelected != null)
			ProcessGameObject(lastSelected);
		
		lastSelected = selected;
	}
	
	static void Update()
	{
		if(EditorApplication.isPlaying)
			return;
		
		// If the last selected GameObject hasn't been deselected, process it as if it had been deselected.
		if(EditorApplication.isPlayingOrWillChangePlaymode)
		{
			ProcessGameObject(lastSelected);
			lastSelected = null;
			return;
		}
		
		var timeStarted = Time.realtimeSinceStartup;
			
		/* Ensure that the following code is always last in the Update() routine */
		
		if(defaultSettings.autoUpdateReferences && refMgr != null)
		{
			while(referenceQueue.Count > 0)
			{
				if(Time.realtimeSinceStartup - timeStarted > 0.02f)
					return;
				refMgr.AddDependencies(new UnityEngine.Object[]{referenceQueue.Dequeue()});
			}
		}
	}
	
	private static void ProcessGameObject(GameObject go)
	{
		if(go == null) return;
		
		if(ES3EditorUtility.IsPrefabInAssets(go))
		{
			var es3Prefab = go.GetComponent<ES3Prefab>();
			if(es3Prefab != null)
				es3Prefab.GeneratePrefabReferences();
		}
		else if(refMgr != null)
			refMgr.AddDependencies(new UnityEngine.Object[]{go});
	}

	public static GameObject AddManagerToScene()
	{
		if(_refMgr != null)
			return _refMgr.gameObject;
		
		var mgr = GameObject.Find("Easy Save 3 Manager");

		if(mgr == null)
		{
			mgr = new GameObject("Easy Save 3 Manager");
			var inspectorInfo = mgr.AddComponent<ES3InspectorInfo>();
			inspectorInfo.message = "The Easy Save 3 Manager is required in any scenes which use Easy Save, and is automatically added to your scene when you enter Play mode.\n\nTo stop this from automatically being added to your scene, go to 'Window > Easy Save 3 > Settings' and deselect the 'Auto Add Manager to Scene' checkbox.";

			_refMgr = mgr.AddComponent<ES3ReferenceMgr>();
			_autoSaveMgr = mgr.AddComponent<ES3AutoSaveMgr>();
			
			referenceQueue = new Queue<GameObject>(EditorSceneManager.GetActiveScene().GetRootGameObjects());
			
			_refMgr.GeneratePrefabReferences();

			Undo.RegisterCreatedObjectUndo(mgr, "Enabled Easy Save for Scene");

		}
		else
		{
			_refMgr = mgr.GetComponent<ES3ReferenceMgr>();
			if(_refMgr == null)
			{
				_refMgr = mgr.AddComponent<ES3ReferenceMgr>();
				Undo.RegisterCreatedObjectUndo(_refMgr, "Enabled Easy Save for Scene");
			}

			_autoSaveMgr = mgr.GetComponent<ES3AutoSaveMgr>();
			if(_autoSaveMgr == null)
			{
				_autoSaveMgr = mgr.AddComponent<ES3AutoSaveMgr>();
				Undo.RegisterCreatedObjectUndo(_autoSaveMgr, "Enabled Easy Save for Scene");
			}
		}
		return mgr;
	}
}

// Used to initialise the reference manager for the first time.
// Displays a loading bar.
/*public class ES3ReferenceMgrInitialiser : EditorWindow
{
	ES3ReferenceMgr mgr = null;
	public Queue<GameObject> gos = new Queue<GameObject>();

	void Awake()
	{
		var go = ES3Postprocessor.AddManagerToScene();
		if(go == null)
			return;
		
		mgr = go.GetComponent<ES3ReferenceMgr>();
		if(mgr == null)
			return;

		if(mgr.IsInitialised)
			return;

		var list = new List<GameObject> ();
		EditorSceneManager.GetActiveScene().GetRootGameObjects(list);
		// Remove Easy Save 3 Manager from dependency list
		list.Remove(go);

		gos = new Queue<GameObject>(list);

		EditorApplication.update += OnUpdate;
	}

	public static void Init()
	{
		UnityEditor.EditorWindow window = GetWindow(typeof(ES3ReferenceMgrInitialiser));
		window.position = new Rect (256, 256, 256, 96);
		window.ShowUtility();
	}

	void OnUpdate()
	{
		if(gos.Count > 0)
		{
			mgr.AddDependencies(new Object[] { gos.Dequeue() });
			Repaint();
		}

		if(gos.Count == 0)
		{
			EditorApplication.update -= OnUpdate;
			mgr.GeneratePrefabReferences();
			this.Close();
		}
	}
		
	void OnGUI()
	{
		if (gos.Count > 0)
		{
			EditorGUILayout.LabelField("Adding references to Easy Save 3 Manager", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(gos.Count+" GameObjects remaining");
			EditorGUILayout.Space();
		}

		if(GUILayout.Button("Cancel"))
		{
			this.Close();
			mgr.Clear();
		}
	}
}*/
