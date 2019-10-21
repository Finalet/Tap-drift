#if !UNITY_4
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

public static class ES2EditorUtility
{
	public static ES2EditorWindowStyle _style = null;
	public static ES2EditorWindowStyle style
	{
		get
		{
			if(_style != null)
				return _style;
			return _style = ES2EditorWindow.instance.style;
		}
	}

	public static void Subheading(string text)
	{
		EditorGUILayout.LabelField(text, style.subHeadingStyle);
	}

	public static string TextField(string label, string value="")
	{
		EditorGUILayout.BeginVertical();
		if(!string.IsNullOrEmpty(label))
			EditorGUILayout.LabelField(label, style.inputLabelStyle, GUILayout.Height(style.inputLabelStyle.fixedHeight));
		string returnValue = EditorGUILayout.TextField(value, style.textInputStyle, GUILayout.Height(style.textInputStyle.fixedHeight));
		EditorGUILayout.EndVertical();
		return returnValue;
	}

	public static string TextArea(string label, string value="")
	{
		EditorGUILayout.BeginVertical();
		if(!string.IsNullOrEmpty(label))
			EditorGUILayout.LabelField(label, style.inputLabelStyle, GUILayout.Height(style.inputLabelStyle.fixedHeight));
		string returnValue = EditorGUILayout.TextArea(value, style.textAreaStyle);
		EditorGUILayout.EndVertical();
		return returnValue;
	}

	public static void TextFieldReadOnly(string label, string value="")
	{
		EditorGUILayout.BeginVertical();
		if(!string.IsNullOrEmpty(label))
			EditorGUILayout.LabelField(label, style.inputLabelStyle, GUILayout.Height(style.inputLabelStyle.fixedHeight));
		EditorGUILayout.SelectableLabel(value, style.textInputStyle, GUILayout.Height(style.textInputStyle.fixedHeight));
		EditorGUILayout.EndVertical();
	}

	public static bool Toggle(string label, bool value)
	{
		bool newValue = value;
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(value ? style.toggleSelectedBackground : null, style.toggleInputStyle,  GUILayout.Height(style.inputLabelStyle.fixedHeight)))
			newValue = !value;
		if(!string.IsNullOrEmpty(label))
			EditorGUILayout.LabelField(label, style.inputLabelStyle, GUILayout.Height(style.inputLabelStyle.fixedHeight));
		EditorGUILayout.EndHorizontal();
		return newValue;
	}

	public static bool Button(GUIContent content)
	{
		return GUILayout.Button(content, style.contentButtonStyle);
	}

	public static bool Button(string label){ return Button(new GUIContent(label)); }
	public static bool Button(Texture2D image){ return Button(new GUIContent(image)); }
	public static bool Button(string label, Texture2D image){ return Button(new GUIContent(label, image)); }

	public static System.Enum EnumField(string label, System.Enum value)
	{
		EditorGUILayout.BeginVertical();
		EditorGUILayout.LabelField(label, style.inputLabelStyle,  GUILayout.Height(style.inputLabelStyle.fixedHeight));
		System.Enum returnValue = EditorGUILayout.EnumPopup(value, style.textInputStyle,  GUILayout.Height(style.textInputStyle.fixedHeight));
		EditorGUILayout.EndVertical();
		return returnValue;
	}
}
#endif