#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;


public class ES2EditorSettingsChangeLog : ES2EditorWindowContent
{
	public ES2EditorSettingsChangeLog()
	{
	}

	public void Draw()
	{
		ES2EditorWindowStyle style = ES2EditorWindow.instance.style;

		EditorGUILayout.BeginVertical(style.windowContentStyle);

		if(System.IO.File.Exists(Application.dataPath+"/Easy Save 2/changelog.txt"))
			ES2EditorUtility.TextArea("", System.IO.File.ReadAllText(Application.dataPath+"/Easy Save 2/changelog.txt"));
		else
			ES2EditorUtility.TextArea("", "Change Log could not be found");

		EditorGUILayout.EndVertical();
	}

	public void OnHierarchyChange(){}

	public void OnProjectChange(){}
}
#endif