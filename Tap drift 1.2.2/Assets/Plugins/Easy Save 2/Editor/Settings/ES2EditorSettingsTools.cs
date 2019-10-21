#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;


public class ES2EditorSettingsTools : ES2EditorWindowContent
{
	public ES2EditorSettingsTools()
	{
	}

	public void Draw()
	{
		ES2EditorWindowStyle style = ES2EditorWindow.instance.style;

		EditorGUILayout.BeginVertical(style.windowContentStyle);

		EditorGUILayout.BeginHorizontal(style.sectionStyle);
		if(ES2EditorUtility.Button("Clear Default Save Folder"))
			ES2EditorTools.ClearDefaultSaveFolder();
		if(ES2EditorUtility.Button("Clear PlayerPrefs"))
			ES2EditorTools.ClearPlayerPrefs();
		if(ES2EditorUtility.Button("Open Default Save Folder"))
			ES2EditorTools.ShowInFileBrowser(Application.persistentDataPath);

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(style.sectionStyle);
		if(ES2EditorUtility.Button("Add Default Settings Object to Scene"))
		{
			GameObject g = new GameObject();
			g.name = "Easy Save Default Settings";
			g.AddComponent<ES2GlobalSettings>();
		}

		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(style.sectionStyle);
		if(ES2EditorUtility.Button("Enable or Update Playmaker Action"))
		{
			AssetDatabase.ImportPackage(Application.dataPath+"/Easy Save 2/Disabled/ES2Playmaker.unitypackage", false);
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Easy Save 2 PlayMaker Action Enabled",
				"Easy Save 2 PlayMaker Action has been Enabled and Updated.", "Ok");
		}

		EditorGUILayout.EndHorizontal();


		EditorGUILayout.EndVertical();
	}

	public void OnHierarchyChange(){}

	public void OnProjectChange(){}
}
#endif