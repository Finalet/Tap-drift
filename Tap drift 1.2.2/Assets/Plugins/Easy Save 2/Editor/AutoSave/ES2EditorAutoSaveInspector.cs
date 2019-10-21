#if !UNITY_4 && !UNITY_2017_1_OR_NEWER
using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ES2AutoSave))]
public class ES2EditorAutoSaveInspector : Editor
{
	private bool showAdvancedSettings = false;

	public override void OnInspectorGUI()
	{
		ES2AutoSave autoSave = (ES2AutoSave)target;
		EditorGUI.indentLevel++;
		showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Show Advanced Settings");
		if(showAdvancedSettings)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("Warning! Please do not modify anything below.", EditorStyles.boldLabel);
			DrawDefaultInspector();
			EditorGUI.indentLevel--;
		}
		EditorGUI.indentLevel--;
	}
}
#endif