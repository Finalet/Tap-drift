using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using ES3Internal;

[CustomEditor(typeof(ES3Prefab))]
[System.Serializable]
public class ES3PrefabEditor : Editor
{
	bool showAdvanced = false;

	public override void OnInspectorGUI()
	{
		var es3Prefab = (ES3Prefab)serializedObject.targetObject;
		EditorGUILayout.HelpBox("Easy Save is enabled for this prefab, and can be saved and loaded with the ES3 methods.", MessageType.None);


		showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced Settings");
		if(showAdvanced)
		{
			EditorGUI.indentLevel++;
			es3Prefab.prefabId =  EditorGUILayout.LongField("Prefab ID", es3Prefab.prefabId);
			EditorGUILayout.LabelField("Reference count", es3Prefab.localRefs.Count.ToString());
			EditorGUI.indentLevel--;
		}
	}
}