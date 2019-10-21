using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class ES2Editor_Material : ES2EditorType
{
	public ES2Editor_Material() : base(typeof(Material))
	{
		key = (byte)27;
	}
	
	public override object DisplayGUI(object data)
	{
		Material mat = (Material)data;
		//mat = (Material)EditorGUILayout.ObjectField((Material)data, typeof(Material),true);
		Editor matEditor = Editor.CreateEditor(mat);
		matEditor.DrawHeader(); 
		matEditor.OnInspectorGUI();
		return mat;
	}
	
	public override object CreateInstance()
	{
		return new Material(Shader.Find("Diffuse"));
	}
}