using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ES3Internal;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
using System;
#endif

public class ES3ReferenceMgr : ES3ReferenceMgrBase, ISerializationCallbackReceiver 
{
	public void OnBeforeSerialize()
	{
		#if UNITY_EDITOR
		// This is called before building or when things are being serialised before pressing play.
		if(BuildPipeline.isBuildingPlayer || (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying))
		{
			AddPrefabsToManager();
			RemoveNullValues();
		}
		#endif
	}

	public void OnAfterDeserialize(){}

	#if UNITY_EDITOR

	public void RefreshDependencies()
	{
		var gos = EditorSceneManager.GetActiveScene().GetRootGameObjects();
		// Remove Easy Save 3 Manager from dependency list
		AddDependencies(gos);
	}

	public void AddDependencies(UnityEngine.Object[] objs, float timeoutSecs=2)
	{
		var startTime = Time.realtimeSinceStartup;
        
        foreach(var obj in objs)
        {
        	if(Time.realtimeSinceStartup - startTime > timeoutSecs)
        		break;
        	
        	if(obj.name == "Easy Save 3 Manager")
        		 continue;
        	
    	    var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[]{obj});
    
    		foreach(var dependency in dependencies)
    		{
    			if(dependency == null || !CanBeSaved(dependency))
    				continue;
    
    			Add(dependency);
    		}
        }
        Undo.RecordObject(this, "Update Easy Save 3 Reference List");
	}

	public void GeneratePrefabReferences()
	{
		AddPrefabsToManager();
		foreach(var es3Prefab in prefabs)
			es3Prefab.GeneratePrefabReferences();
    }
    
    public void AddPrefabsToManager()
	{
		if(this.prefabs.RemoveAll(item => item == null) > 0)
			Undo.RecordObject(this, "Update Easy Save 3 Reference List");
			
		foreach(var es3Prefab in Resources.FindObjectsOfTypeAll<ES3Prefab>())
		{
			if(GetPrefab(es3Prefab) == -1)
			{
				AddPrefab(es3Prefab);
				Undo.RecordObject(this, "Update Easy Save 3 Reference List");
			}
		}
	}

    public static bool CanBeSaved(UnityEngine.Object obj)
	{
		// Check if any of the hide flags determine that it should not be saved.
		if(	(((obj.hideFlags & HideFlags.DontSave) == HideFlags.DontSave) || 
		     ((obj.hideFlags & HideFlags.DontSaveInBuild) == HideFlags.DontSaveInBuild) ||
		     ((obj.hideFlags & HideFlags.DontSaveInEditor) == HideFlags.DontSaveInEditor) ||
		     ((obj.hideFlags & HideFlags.HideAndDontSave) == HideFlags.HideAndDontSave)))
		{
			var type = obj.GetType();
			// Meshes are marked with HideAndDontSave, but shouldn't be ignored.
			if(type != typeof(Mesh) && type != typeof(Material))
				return false;
		}
		return true;
	}

	#endif
}
