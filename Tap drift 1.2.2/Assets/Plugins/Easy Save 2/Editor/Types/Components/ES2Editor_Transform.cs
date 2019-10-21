using UnityEngine;
using UnityEditor;
using System.Collections;

public class ES2Editor_Transform : ES2EditorType
{
	public ES2Editor_Transform() : base(typeof(Transform)){}
	
	public override object DisplayGUI(object data)
	{
		Transform c = (Transform)data;
		c.position = EditorGUILayout.Vector3Field("Position", c.position);
		c.eulerAngles = EditorGUILayout.Vector3Field("Rotation", c.rotation.eulerAngles);
		c.localScale = EditorGUILayout.Vector3Field("Local Scale", c.localScale);
		EditorGUILayout.LabelField("Tag");
		EditorGUI.indentLevel++;
		c.tag = EditorGUILayout.TextField(c.tag);
		EditorGUI.indentLevel--;
		return c;
	}
}