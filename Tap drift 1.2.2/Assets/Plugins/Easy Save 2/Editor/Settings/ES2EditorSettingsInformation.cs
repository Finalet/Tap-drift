#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;


public class ES2EditorSettingsInformation : ES2EditorWindowContent
{
	public ES2EditorSettingsInformation()
	{
	}

	public void Draw()
	{
		ES2EditorWindowStyle style = ES2EditorWindow.instance.style;

		EditorGUILayout.BeginVertical(style.windowContentStyle);

		ES2EditorUtility.TextFieldReadOnly("Application.persistentDataPath", Application.persistentDataPath);

		EditorGUILayout.EndVertical();
	}

	public void OnHierarchyChange(){}

	public void OnProjectChange(){}
}
#endif